using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HedgeMark.SwiftMessageHandler.Utils
{
    public static class Extensions
    {
        public static readonly string DefaultDateFormat = "yyMMdd";
        public static readonly string DefaultTimeFormat = "HHmm";
        public static readonly string DefaultDateAndTimeFormat = "yyMMddHHmm";
        public static readonly string DefaultDateFormatWithFullYear = "yyyyMMdd";
        public static T DeepCopy<T>(this T source) where T : class
        {
            return source == null ? default : Cloner.DeepCopy(source);
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

        public static TV GetValue<TK, TV>(this Dictionary<TK, TV> dictionary, TK key)
        {
            TV value;
            if (!dictionary.TryGetValue(key, out value))
                throw new KeyNotFoundException(string.Format("Key '{0}' not found in dictionary", key));
            return value;
        }

        public static string ToDateString(this DateTime dateTime)
        {
            return ToDateTimeString(dateTime, "MM/dd/yyyy");
        }

        public static string ToDateString(this DateTime? dateTime)
        {
            return dateTime.HasValue ? ToDateString(dateTime.Value) : string.Empty;
        }

        public static string ToDateTimeString(this DateTime? dateTime, string format = "")
        {
            return dateTime.HasValue ? ToDateTimeString(dateTime.Value, format) : string.Empty;
        }

        public static string ToDateTimeString(this DateTime dateTime, string format = "MM/dd/yyyy hh:mm:ss tt")
        {
            if (string.IsNullOrEmpty(format))
                format = DefaultDateFormat;
            return dateTime.ToString(format);
        }

        public static bool IsEnumerable(this Type type)
        {
            if (type == typeof(string))
                return false;

            return type.IsArray || type.GetInterfaces().Contains(typeof(IEnumerable));
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

        public static string GetFormatedCurrency(string value, string groupSeparator = ",", string currencySymbol = "")
        {
            var nfi = new NumberFormatInfo
            {
                CurrencyGroupSeparator = groupSeparator,
                //CurrencyDecimalDigits = noOfDecimalPlaces,
                CurrencySymbol = currencySymbol
            };

            if (string.IsNullOrWhiteSpace(value)) return 0.ToString("C", nfi);

            decimal decimalNo;
            decimal.TryParse(value, NumberStyles.Currency | NumberStyles.Any, null, out decimalNo);

            return decimalNo.ToString("C", nfi);
        }

        public static string TrimToLength(this string addressLine, int length = 35)
        {
            if (string.IsNullOrWhiteSpace(addressLine))
                return addressLine;

            if (addressLine.Length <= 35)
                return addressLine;

            return addressLine.Substring(0, length);
        }


    }
}