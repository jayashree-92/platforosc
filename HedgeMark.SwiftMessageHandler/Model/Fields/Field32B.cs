using System.Collections.Generic;
using System.Globalization;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field32B : Field
    {
        public Field32B() : base(FieldDirectory.FIELD_32B)
        {
        }

        public Field32B setCurrency(string currency)
        {
            Currency = currency;
            return this;
        }

        public Field32B setAmount(decimal amount)
        {
            var amountStr = amount.ToString(CultureInfo.InvariantCulture);
            Amount = amountStr.Contains(".") ? amountStr.ToString(CultureInfo.InvariantCulture).Replace(".", ",") : string.Format("{0},", amountStr);
            return this;
        }

        public string Currency { get; set; }
        public string Amount { get; set; }

        public override string GetValue()
        {
            return Value = string.Format("{0}{1}", Currency ?? FieldConstants.USD, Amount == null ? "0," : Amount);
        }

        public override List<string> Components
        {
            get
            {
                return new List<string>()
                {
                    FieldConstants.CURRENCY,
                    FieldConstants.AMOUNT,
                };
            }
        }

        public override string GetComponentValue(string component)
        {
            var derivedValue = string.Empty;
            switch (component)
            {
                case FieldConstants.CURRENCY:
                    derivedValue = !string.IsNullOrWhiteSpace(Currency) ? Currency : (Value.Length >= 3 ? Value.Substring(0, 3) : Currency);
                    break;
                case FieldConstants.AMOUNT:
                    derivedValue = !string.IsNullOrWhiteSpace(Amount) ? Amount : (Value.Length > 3 ? Value.Substring(3, Value.Length - 3) : Amount);
                    derivedValue = derivedValue.Replace(",", ".");
                    break;
            }

            return component.GetComponentValue(derivedValue);
        }

    }
}
