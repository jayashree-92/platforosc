using System;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class FieldWithDate : Field
    {
        public FieldWithDate(string name) : base(name)
        {
        }

        public string DateString { get; set; }

        public T setDate<T>(T callingClass, DateTime date)
        {
            DateString = date.ToString("yyMMdd");
            return callingClass;
        }
    }
}
