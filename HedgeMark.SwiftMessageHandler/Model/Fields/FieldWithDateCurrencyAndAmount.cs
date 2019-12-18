using System;
using System.Collections.Generic;
using System.Globalization;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class FieldWithDateCurrencyAndAmount : FieldWithCurrencyAndAmount
    {
        public FieldWithDateCurrencyAndAmount(string name) : base(name)
        {
        }

        public string DateString { get; set; }

        public T setDate<T>(T callingClass, DateTime date)
        {
            DateString = date.ToString("yyMMdd");
            return callingClass;
        }

        public override List<string> Components
        {
            get
            {
                return new List<string> { FieldConstants.DATE, FieldConstants.CURRENCY, FieldConstants.AMOUNT };
            }
        }

        public override string GetComponentValue(string component)
        {
            var derivedValue = string.Empty;
            switch (component)
            {
                case FieldConstants.DATE:
                    derivedValue = !string.IsNullOrWhiteSpace(DateString) ? DateString : Value.Substring(0, 6);
                    break;
                case FieldConstants.CURRENCY:
                    derivedValue = !string.IsNullOrWhiteSpace(Currency) ? Currency : Value.Length >= 9 ? Value.Substring(6, 3) : string.Empty;
                    break;
                case FieldConstants.AMOUNT:
                    derivedValue = Amount > 0 ? Amount.ToString(CultureInfo.InvariantCulture) : Value.Length > 9 ? Value.Substring(9, Value.Length - 9) : string.Empty;
                    derivedValue = derivedValue.Replace(",", ".");
                    break;
            }

            return component.GetComponentValue(derivedValue);
        }
        public override string GetValue()
        {
            return string.Format("{0}{1}", DateString ?? string.Empty, base.GetValue());
        }
    }
}
