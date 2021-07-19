using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HedgeMark.Operations.FileParseEngine.Models;
using HedgeMark.Operations.FileParseEngine.RuleEngine;

namespace HM.Operations.Secure.Web.Utility
{

    public class JsonBuilderInput
    {
        public JsonBuilderInput()
        {
            HiddenHeaders = new List<string>();
            VisibleHeaders = new List<string>();
            IgnoreHeaders = new List<string>();
            OrderByHeaders = new List<string>();
        }

        public IReadOnlyList<Row> RowsToBuild { get; set; }
        public List<string> HiddenHeaders { get; set; }
        public List<string> IgnoreHeaders { get; set; }
        public List<string> VisibleHeaders { get; set; }
        public List<string> OrderByHeaders { get; set; }
        public bool ShouldConstructVisibleOnly { get; set; }
    }

    public class JsonHelper
    {
        public static string GetJsonInExcelFormat(List<Row> rows)
        {
            if (rows == null || rows.Count == 0)
                return "{\"aaData\":[],\"aoColumns\":[{ \"sTitle\": \"\" }]}";

            var maxColumns = rows.Max(s => s.CellValues.Count);

            var jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{\"aaData\":[");

            //header values
            var rowIndex = 1;
            var headers = rows[0].CellValues.Select(s => s.Key.Name).ToList();
            jsonBuilder = BuildJsonRow(jsonBuilder, headers, maxColumns, ref rowIndex);

            jsonBuilder = rows.Select(row => row.CellValues.Select(s => Regex.Replace(s.Value.UiValue, @"\s+", " ").Replace("\"", "\\\"")).ToList())
                .Aggregate(jsonBuilder, (current, list) => BuildJsonRow(current, list, maxColumns, ref rowIndex));

            jsonBuilder = new StringBuilder(jsonBuilder.ToString().TrimEnd(','));
            jsonBuilder.Append("],\"aoColumns\":[");

            for (var i = 0; i <= maxColumns; i++)
                jsonBuilder.AppendFormat("{{ \"sTitle\": \"{0}\" }},", i.GetColumnName());

            jsonBuilder = new StringBuilder(jsonBuilder.ToString().TrimEnd(','));
            jsonBuilder.Append("]}");

            return jsonBuilder.ToString();
        }

        private static StringBuilder BuildJsonRow(StringBuilder jsonBuilder, List<string> rowData, int maxColumns, ref int rowIndex)
        {
            jsonBuilder.Append("[");
            jsonBuilder.AppendFormat("\"{0}\",", rowIndex++);
            jsonBuilder.AppendFormat("\"{0}\"", string.Join("\",\"", rowData));

            for (var j = rowData.Count; j < maxColumns; j++)
                jsonBuilder.Append(",\"\"");

            jsonBuilder.Append("],");
            return jsonBuilder;
        }

        private const string AllSpaceRegex = "[\r\n]+$";
        public static string GetJson(OutputContent result)
        {
            var jsonBuilder = new StringBuilder();
            if (result.CompleteInfoRows == null || result.CompleteInfoRows.Count == 0)
                return "{\"aaData\":[],\"aoColumns\":[{ \"sTitle\": \"\" }],\"aaLogs\":[]}";

            var builderInput = new JsonBuilderInput() { RowsToBuild = result.Contents.SelectMany(s => s.Rows).ToList() };
            jsonBuilder.Append("{\"aaData\":[");
            jsonBuilder = BuildJson(builderInput, jsonBuilder);
            jsonBuilder.Append("],\"aaLogs\":[");

            var logList = result.Logs.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Distinct();
            foreach (var value in logList)
            {
                jsonBuilder.AppendFormat("[\"{0}\"],", Regex.Replace(value, AllSpaceRegex, string.Empty).Replace("\"", "'"));
            }

            jsonBuilder = new StringBuilder(jsonBuilder.ToString().TrimEnd(','));
            jsonBuilder.Append("]}");

            return jsonBuilder.ToString();
        }

        public static string GetJson(List<Row> rows, List<string> hiddenHeaders = null, List<string> visibleHeaders = null)
        {
            var builderInput = new JsonBuilderInput()
            {
                RowsToBuild = rows,
                HiddenHeaders = hiddenHeaders ?? new List<string>(),
                VisibleHeaders = visibleHeaders ?? new List<string>()
            };

            return GetJson(builderInput);
        }

        private const string ViolationHeader = "Violations";
        private const string WarningHeader = "Warnings";
        private const string InfoHeader = "Infos";
        public static string GetJsonIncludingTriggerResults(List<InfoRow> rows, List<string> hiddenHeaders = null, List<string> visibleHeaders = null)
        {
            var allRows = new List<Row>();
            foreach (var s in rows)
            {
                var violations = string.Join("|", s.Violations.Select(s1 => s1.Level + "~" + s1.Rule));
                var warnings = string.Join("|", s.Warnings.Select(s1 => s1.Level + "~" + s1.Rule));
                var infos = string.Join("|", s.Infos.Select(s1 => s1.Level + "~" + s1.Rule));
                s.Row[ViolationHeader] = violations;
                s.Row[WarningHeader] = warnings;
                s.Row[InfoHeader] = infos;
                allRows.Add(s.Row);
            }

            if (hiddenHeaders == null)
                hiddenHeaders = new List<string>();

            hiddenHeaders.Add(ViolationHeader);
            hiddenHeaders.Add(WarningHeader);

            var builderInput = new JsonBuilderInput()
            {
                RowsToBuild = allRows,
                HiddenHeaders = hiddenHeaders,
                VisibleHeaders = visibleHeaders ?? new List<string>()
            };

            return GetJson(builderInput);
        }

        public static string GetJson(JsonBuilderInput builderInput)
        {
            var jsonBuilder = new StringBuilder();
            if (builderInput.RowsToBuild == null || builderInput.RowsToBuild.Count == 0)
                return "{\"aaData\":[],\"aoColumns\":[{ \"sTitle\": \"\" }]}";

            jsonBuilder.Append("{\"aaData\":[");
            jsonBuilder = BuildJson(builderInput, jsonBuilder);
            jsonBuilder.Append("]}");
            return jsonBuilder.ToString();
        }

        private static readonly List<string> DefaultHiddenField = new List<string>()
        {

        };

        private static StringBuilder BuildJson(JsonBuilderInput builderInput, StringBuilder jsonBuilder)
        {
            builderInput.HiddenHeaders.AddRange(DefaultHiddenField);
            builderInput.HiddenHeaders.AddRange((builderInput.RowsToBuild.FirstOrDefault() ?? new Row()).CellValues.Where(s => s.Key != null && !s.Key.IsVisible).Select(s => s.Key.UiName));

            var uiColumnList = new List<string>();

            foreach (var list in builderInput.RowsToBuild.Select(row => row.CellValues.Where(w => w.Key != null).Select(cell => cell.Key.UiName)).ToList())
            {
                uiColumnList.AddRange(list);
            }

            if (builderInput.VisibleHeaders.Count > 0)
            {
                var nonVisibleHeaders = uiColumnList.Where(s => !builderInput.VisibleHeaders.Contains(s)).ToList();
                builderInput.HiddenHeaders.AddRange(nonVisibleHeaders);
            }

            if (builderInput.ShouldConstructVisibleOnly)
            {
                var finalUiColumnList = new List<string>();
                finalUiColumnList.AddRange(uiColumnList.Where(uiCol => !builderInput.HiddenHeaders.Contains(uiCol)));
                uiColumnList = finalUiColumnList;
            }

            uiColumnList = uiColumnList.Distinct().OrderBy(a => builderInput.OrderByHeaders.IndexOf(a)).Where(s => !builderInput.IgnoreHeaders.Contains(s)).ToList();

            var typeIndex = new List<int>();
            var hiddenIndex = new List<int>();
            var rowItem = builderInput.RowsToBuild.First();
            var colIndex = 0;
            foreach (var uiCol in uiColumnList)
            {
                typeIndex.AddRange(rowItem.CellValues.Where(pair => pair.Key != null && (pair.Key.UiName).Equals(uiCol) && pair.Key.IsNumericType).Select(pair => colIndex));
                colIndex++;
            }

            colIndex = 0;
            foreach (var uiCol in uiColumnList)
            {
                hiddenIndex.AddRange(rowItem.CellValues.Where(pair => pair.Key != null && (pair.Key.UiName).Equals(uiCol) && builderInput.HiddenHeaders.Contains(pair.Key.UiName)).Select(pair => colIndex));
                colIndex++;
            }

            foreach (var row in builderInput.RowsToBuild)
            {
                jsonBuilder.Append("[");
                Row thisRow = row;
                var cellVals = uiColumnList.Select(column => thisRow[column, true].Replace("\r", " ").Replace("\n", " ").Replace("\"", "'"));
                jsonBuilder.AppendFormat("\"{0}\"", string.Join("\",\"", cellVals));
                jsonBuilder.Append("],");
            }

            jsonBuilder = new StringBuilder(jsonBuilder.ToString().TrimEnd(','));
            jsonBuilder.Append("],\"aoColumns\":[");

            if (builderInput.RowsToBuild.Count == 0 || builderInput.RowsToBuild[0].CellValues.Count == 0)
                jsonBuilder.Append("{ \"sTitle\": \"\" }");
            else
            {
                jsonBuilder.AppendFormat("{{ \"sTitle\": \"{0}\" }}", string.Join("\"},{\"sTitle\":\"", uiColumnList));
            }

            jsonBuilder.AppendFormat("], \"columnDefs\": [{{ \"visible\": false, \"targets\": [" + string.Join(",", hiddenIndex) + "] }},{{ \"type\": \"currency\", \"targets\": [" + string.Join(",", typeIndex) + "] }}");

            return jsonBuilder;
        }

        private static Dictionary<string, List<long>> GetValue(Dictionary<string, List<long>> collection, string name, long id, List<string> allReports)
        {
            if (!collection.ContainsKey(name))
                collection.Add(name, new List<long> { id });
            else if (!collection[name].Contains(id))
                collection[name].Add(id);

            return collection.OrderBy(a => allReports.IndexOf(a.Key)).ToDictionary(s => s.Key, s => s.Value);
        }

        private static SortedDictionary<string, List<long>> AddToCollection(SortedDictionary<string, List<long>> collection, string name, long id)
        {
            if (!collection.ContainsKey(name))
                collection.Add(name, new List<long> { id });
            else if (!collection[name].Contains(id))
                collection[name].Add(id);

            return collection;
        }

        private static void BuildJson(StringBuilder jsonBuilder, IEnumerable<KeyValuePair<string, List<long>>> reports)
        {
            jsonBuilder.Append("{ ");
            foreach (var item in reports)
            {
                jsonBuilder.Append(string.Format("\"{0}\" : ", item.Key));
                jsonBuilder.Append("[");
                jsonBuilder.Append(string.Join(",", item.Value));
                jsonBuilder.Append("],");
            }

            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            jsonBuilder.Append(" }");
        }

        public static string GetDataTableRows(List<Row> rows)
        {
            var rowData = rows.Select(s => s.CellValues.Select(s1 => s1.Value.Value).ToArray()).ToList();
            var jsonBuilder = new StringBuilder();

            jsonBuilder.Append("[");
            for (var i = 0; i < rowData.Count; i++)
            {
                jsonBuilder.Append("[");
                jsonBuilder.AppendFormat("\"{0}\"", string.Join("\",\"", rowData[i]));
                jsonBuilder.Append(i == rowData.Count - 1 ? "]" : "],");
            }

            jsonBuilder.Append("]");
            return jsonBuilder.ToString();
        }

        private static List<string> GetDerivedHeaderText(string feeBaseRule)
        {
            var regex = new Regex(@"\=.*?\)]");
            var matches = regex.Matches(feeBaseRule);
            return (from object match in matches select match.ToString().Replace("=", "").Replace(")]", "")).ToList();

        }
    }
}
