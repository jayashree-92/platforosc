using System;
using System.Collections.Generic;
using System.Globalization;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field32A : FieldWithDate
    {
        public Field32A() : base(FieldDirectory.FIELD_32A)
        {
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
                    derivedValue = !string.IsNullOrWhiteSpace(Amount) ? Amount : Value.Length > 9 ? Value.Substring(9, Value.Length - 9) : string.Empty;
                    derivedValue = derivedValue.Replace(",", ".");
                    break;
            }

            return component.GetComponentValue(derivedValue);
        }

        public Field32A setDate(DateTime dateTime)
        {
            return setDate(this, dateTime);
        }

        public Field32A setCurrency(string currency)
        {
            Currency = currency;
            return this;
        }

        public Field32A setAmount(decimal amount)
        {
            Amount = amount.ToString(CultureInfo.InvariantCulture).Replace(".", ",");
            return this;
        }

        public override string GetValue()
        {
            return Value = string.Format("{0}{1}{2}", DateString ?? string.Empty, Currency ?? FieldConstants.USD, Amount == null ? "0" : Amount);
        }

        public string Currency { get; set; }
        public string Amount { get; set; }

    }
}

