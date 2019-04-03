using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Kent.Boogaart.KBCsv;

namespace Com.HedgeMark.Commons.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<IEnumerable<T>> PartitionBy<T>(this IEnumerable<T> collection, Func<T, bool> critieria)
        {
            var reader = collection.GetEnumerator();
            while ((reader.MoveNext()))
            {
                yield return reader.PartitionBy(critieria).ToList();
            }
        }

        private static IEnumerable<T> PartitionBy<T>(this IEnumerator<T> enumerator,Func<T, bool> critieria)
        {
            do
            {
                if (critieria(enumerator.Current))
                    yield break;
                yield return enumerator.Current;
            } while (enumerator.MoveNext());
        }

        public static IEnumerable<T> DistinctBy<T,TK>(this IEnumerable<T> collection, Func<T, TK> selector)
        {
            var distinctSet = new HashSet<TK>();
            return collection.Where(element => distinctSet.Add(selector(element)));
        }

        public static string GetPropertyName<TE, TP>(this Expression<Func<TE, TP>> expression)
        {
            return ((MemberExpression)expression.Body).Member.Name;
        }

        //public static void DumpToFile<T>(this List<T> entities, string filePath)
        //{
        //    var dataTable = entities.ToDataTable();
        //    DumpToFile(dataTable, filePath);
        //}

        public static void DumpToFile(this DataTable dataTable, string filePath)
        {
            using (var csv = new CsvWriter(filePath, false))
            {
                csv.WriteAll(dataTable);
            }
        }
    }
}