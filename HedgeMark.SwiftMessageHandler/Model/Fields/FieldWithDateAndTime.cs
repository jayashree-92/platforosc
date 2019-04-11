using System;
using System.Collections.Generic;
using System.Globalization;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class FieldWithDateAndTime : Field
    {
        public FieldWithDateAndTime(string name) : base(name)
        {
        }

        public string DateString { get; set; }
        public string TimeString { get; set; }

        public override string GetValue()
        {
            return string.Format("{0}{1}", DateString, TimeString);
        }

        public T setDateAndTime<T>(T callingClass, DateTime date)
        {
            DateString = date.ToString("yyMMdd");
            TimeString = date.ToString("hhmm");
            return callingClass;
        }

        public override string GetComponentValue(string component)
        {
            DateTime dateTime;
            DateTime.TryParseExact(Value, new[] { Extensions.DefaultDateAndTimeFormat, Extensions.DefaultDateFormatWithFullYear }, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
            return dateTime.ToString("MMM dd, yyyy hh:mm tt");
        }
    }
}
