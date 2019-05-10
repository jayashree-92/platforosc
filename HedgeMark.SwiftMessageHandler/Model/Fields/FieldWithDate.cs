using System;
using System.Collections.Generic;
using HedgeMark.SwiftMessageHandler.Utils;

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

        public override string GetValue()
        {
            return DateString;
        }

        public override List<string> Components
        {
            get
            {
                return new List<string>()
                {
                    FieldConstants.VALUE_DATE
                };
            }
        }

        public override string GetComponentValue(string component)
        {
            return component.GetComponentValue(DateString);
        }
    }
}
