using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using log4net;

namespace Com.HedgeMark.Commons.Extensions
{
    public static class Extensions
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Extensions));
        public static readonly string DefaultDateFormat = ConfigurationManagerWrapper.StringSetting(Config.DefaultDateFormat, "yyyyMMdd");

        public static T DeepCopy<T>(this T source) where T : class
        {
            return source == null ? default(T) : Cloner.DeepCopy(source);
        }

        public static void SafeExecute(this Action action, [CallerMemberName] string memberName = "")
        {
            try
            {
                if (action != null)
                    action();
            }
            catch (Exception ex)
            {
                Console.WriteLine(memberName);
                logger.Error(string.Format("Executing action failed from {0}", memberName), ex);
            }
        }

        public static bool ToBool(this object value, bool defaultValue = false)
        {
            try
            {
                return (bool)Convert.ChangeType(value, typeof(bool));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
        public static int ToInt(this object value, int defaultValue = 0)
        {
            try
            {
                return (int)Convert.ChangeType(value, typeof(int));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static DateTime ToDateTime(this object value, string parseFormat)
        {
            try
            {
                DateTime dateTime;
                DateTime.TryParseExact(value.ToString(), new[] { DefaultDateFormat, parseFormat }, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
                return dateTime;
                //return (int)Convert.ChangeType(value, typeof(int));
            }
            catch (Exception)
            {
                return new DateTime();
            }
        }

        public static decimal ToDecimal(this object value, decimal defaultValue = 0)
        {
            try
            {
                return (decimal)Convert.ChangeType(value, typeof(decimal));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static double ToDouble(this object value, double defaultValue = 0)
        {
            try
            {
                return (double)Convert.ChangeType(value, typeof(double));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static long ToLong(this object value, long defaultValue = 0)
        {
            try
            {
                return (long)Convert.ChangeType(value, typeof(long));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static string ToCurrency(this object value, string groupSeparator = ",", string currencySymbol = "")
        {
            var nfi = new NumberFormatInfo
            {
                CurrencyGroupSeparator = groupSeparator,
                //CurrencyDecimalDigits = noOfDecimalPlaces,
                CurrencySymbol = currencySymbol
            };

            if (string.IsNullOrWhiteSpace(value.ToString())) return 0.ToString("C", nfi);

            decimal decimalNo;
            decimal.TryParse(value.ToString(), NumberStyles.Currency | NumberStyles.Any, null, out decimalNo);

            return decimalNo.ToString("C", nfi);
        }

        public static string JoinToCsv<T>(this IEnumerable<T> array, string separator = ",")
        {
            return string.Join(separator, array);
        }

        public static List<string> ToStringList(this string value)
        {
            try
            {
                return value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }


        public static string[] SplitToCsv(this string value)
        {
            return value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SplitNonNullCsv(this string value)
        {
            return string.IsNullOrEmpty(value) ? new string[0] : value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static double? Abs(this double? source)
        {
            return source.HasValue ? Math.Abs(source.Value) : (double?)null;
        }

        public static double Default(this double? source)
        {
            return source.HasValue ? source.Value : 0.0;
        }

        public static bool IsTrue(this bool? source)
        {
            return source.HasValue && source.Value;
        }

        public static T CustomTo<T>(this string source)
        {
            // Dirty patch to handle ="data" format from excel
            if (source.StartsWith("="))
                source = source.Substring(1);
            return To<T>(source);
        }

        public static T To<T>(this object source)
        {
            var value = default(T);
            if (source != null)
            {
                if (source is string && string.IsNullOrWhiteSpace(source.ToString()))
                {
                    return value;
                }
                var sourceType = typeof(T);
                var underlyingType = Nullable.GetUnderlyingType(sourceType);
                var requestedType = underlyingType ?? (sourceType == typeof(DateTime) ? sourceType : null);
                if (requestedType == typeof(DateTime))
                {
                    DateTime date;
                    if (source.ToString().Length == DefaultDateFormat.Length)
                    {
                        if (!DateTime.TryParseExact(source.ToString(), DefaultDateFormat, null, DateTimeStyles.None, out date))
                            return value;
                    }
                    else
                    {
                        if (!DateTime.TryParse(source.ToString(), out date))
                            return value;
                    }
                    return (T)Convert.ChangeType(date, requestedType);
                }
                try
                {
                    value = (T)Convert.ChangeType(source, requestedType ?? sourceType);
                }
                catch (FormatException)
                {
                    if (underlyingType != null)
                    {
                        return value;
                    }
                    throw;
                }
            }
            return value;
        }


        //TODO: Consolidate all Tos to use universal type converter
        //        public static T To<T>(this string obj)
        //        {
        //            if (string.IsNullOrWhiteSpace(obj))
        //                return default(T);
        //
        //            Object value;
        //
        //            if (UniversalTypeConverter.TryConvert(obj, typeof(T), out value))
        //                return (T)value;
        //            return default(T);
        //        }

        public static string ToReadableByteString(this long bytes)
        {
            if (bytes >= 1024)
                return Math.Round(bytes / (double)1024, digits: 2) + " KB";
            return bytes + " bytes";
        }

        public static string ToReadableByteString(this int bytes)
        {
            return ToReadableByteString((long)bytes);
        }

        public static string GetUniqueFileName(this string filePath)
        {
            if (!File.Exists(filePath))
                return filePath;

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var directoryName = Path.GetDirectoryName(filePath);
            var extension = Path.GetExtension(filePath);

            var pattern = Path.Combine(directoryName, fileName + "(Copy{0})" + extension);

            string tryFileName;
            var i = 1;

            do
            {
                tryFileName = string.Format(pattern, i++);
            } while (File.Exists(tryFileName));

            return tryFileName;
        }

        public static TV GetValue<TK, TV>(this Dictionary<TK, TV> dictionary, TK key)
        {
            TV value;
            if (!dictionary.TryGetValue(key, out value))
                throw new KeyNotFoundException(string.Format("Key '{0}' not found in dictionary", key));
            return value;
        }

        public static string ToDateString(this DateTime dateTime)
        {
            return ToDateTimeString(dateTime, ConfigurationManagerWrapper.StringSetting(Config.DefaultDateFormat, "MM/dd/yyyy"));
        }

        public static string ToDateString(this DateTime? dateTime)
        {
            return dateTime.HasValue ? ToDateString(dateTime.Value) : string.Empty;
        }

        public static string ToDateTimeString(this DateTime? dateTime, string format = "")
        {
            return dateTime.HasValue ? ToDateTimeString(dateTime.Value, format) : string.Empty;
        }

        public static string ToDateTimeString(this DateTime dateTime, string format = "")
        {
            if (string.IsNullOrEmpty(format))
                format = ConfigurationManagerWrapper.StringSetting(Config.DateTimeFormat, "MM/dd/yyyy hh:mm:ss tt");
            return dateTime.ToString(format);
        }

        public static DataTable ConvertToDataTable<T>(this IList<T> data)
        {
            var properties = TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                type = type.IsEnumerable() ? typeof(string) : type;
                table.Columns.Add(prop.Name, type);
            }

            foreach (var item in data)
            {
                var row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    object value;
                    if (prop.PropertyType.IsEnumerable())
                    {
                        var arrValue = prop.GetValue(item) ?? new string[0];
                        value = ((IEnumerable)arrValue).Cast<object>().Select(x => x.ToString()).JoinToCsv();
                    }
                    else
                    {
                        value = prop.GetValue(item) ?? DBNull.Value;
                    }
                    row[prop.Name] = value;
                }
                table.Rows.Add(row);
            }
            return table;
        }

        public static bool IsEnumerable(this Type type)
        {
            if (type == typeof(string))
                return false;

            return type.IsArray || type.GetInterfaces().Contains(typeof(IEnumerable));
        }

        public static DataTable ToTable<T>(this List<T> data)
        {
            var properties = typeof(T).GetProperties().ToList();
            var table = new DataTable();
            properties.ForEach(
                property =>
                table.Columns.Add(property.Name,
                                  Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType));
            data.ForEach(obj =>
                {
                    var newRow = table.NewRow();
                    properties.ForEach(property => newRow[property.Name] = property.GetValue(obj) ?? DBNull.Value);
                    table.Rows.Add(newRow);
                });
            return table;
        }

        public static DataTable GetDataTableFromObjects(this List<dynamic> dataList)
        {
            if (dataList != null && dataList.Count > 0)
            {
                Type type = dataList[0].GetType();
                var propertyInfos = type.GetProperties();
                var dt = new DataTable();
                foreach (PropertyInfo info in propertyInfos)
                {
                    dt.Columns.Add(new DataColumn(info.Name));
                }
                foreach (var data in dataList)
                {
                    DataRow dr = dt.NewRow();
                    foreach (PropertyInfo info in propertyInfos)
                    {
                        dr[info.Name] = info.GetValue(data);
                    }
                    dt.Rows.Add(dr);
                }
                return dt;
            }
            return null;
        }

        public static ParallelQuery<T> RunParallel<T>(this IEnumerable<T> source, int dop = 0)
        {
            dop = !ConfigurationManagerWrapper.BooleanSetting(Config.ShouldRunInParallel) ? 1 : (dop == 0 ? Environment.ProcessorCount : dop);
            return source.AsParallel().WithDegreeOfParallelism(dop);
        }

        public static double? Add(this double? value1, double? value2)
        {
            if (!value1.HasValue && !value2.HasValue)
                return null;
            return (value1.HasValue ? value1.Value : 0) + (value2.HasValue ? value2.Value : 0);
        }

        public static string StripCommas(this string untrimmedString)
        {
            return StripChar(untrimmedString, ",");
        }

        public static string StripChar(this string untrimmedString, string stripChar)
        {
            return (!string.IsNullOrWhiteSpace(untrimmedString) && untrimmedString.Contains(stripChar))
                       ? untrimmedString.Replace(stripChar, string.Empty)
                       : untrimmedString;
        }

        public static IEnumerable<TResult> SelectNonNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult?> selector) where TResult : struct
        {
            return source.Select(selector).OfType<TResult>();
        }

        public static IEnumerable<TResult> SelectNonNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return typeof(TResult) == typeof(string)
                ? source.Select(selector).Where(x => !string.IsNullOrWhiteSpace(x as string))
                : source.Select(selector).Where(x => x != null);
        }

        //// This cannot be used for converting to / validating for DateTime
        //public static T CheckAndConvertTo<T>(this object source)
        //{
        //    object result;
        //    return source.TryConvert(typeof(T), out result) ? (T)result : default(T);
        //}

        //Trims the value after checking if the value is null 
        public static string TrimEndTrial(this string value)
        {
            return value != null ? value.TrimEnd() : null;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            var index = 0;
            foreach (var t in source)
                action(t, index++);
        }
    }
}