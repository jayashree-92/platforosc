using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class FieldWithCurrencyAndAmount : Field
    {

        public static readonly Dictionary<string, int> CurrencyToDecimalPoints;
        static FieldWithCurrencyAndAmount()
        {
            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "HedgeMark.SwiftMessageHandler.Configs.CurrencyDecimalUnits.csv";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var lines = reader.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                CurrencyToDecimalPoints = (from line in lines
                                           let lineSplits = line.Split(',')
                                           select new { currency = lineSplits[1], decimalPoint = lineSplits[3] }).ToDictionary(s => s.currency, v => v.decimalPoint.ToInt(2));
            }
        }

        public FieldWithCurrencyAndAmount(string name) : base(name)
        {
        }

        public T setCurrency<T>(T callingClass, string currency)
        {
            Currency = currency;
            return callingClass;
        }

        public T setAmount<T>(T callingClass, decimal amount)
        {
            Amount = amount;
            return callingClass;
        }

        public string Currency { get; set; }
        public decimal Amount { get; set; }

        public override string GetValue()
        {
            var amountStr = Amount.ToString(CultureInfo.InvariantCulture);
            amountStr = amountStr.Contains(".") ? amountStr.ToString(CultureInfo.InvariantCulture).Replace(".", ",") : string.Format("{0},", amountStr);

            if (string.IsNullOrWhiteSpace(amountStr))
                amountStr = "0,";

            // we need to add or remove decimal places according to Currency List - if its not in currency list the default currency decimal units = 2
            if (!CurrencyToDecimalPoints.ContainsKey(Currency))
                return string.Format("{0}{1}", Currency ?? FieldConstants.USD, amountStr);

            var amountStrSplits = amountStr.Split(',');
            var allowedDecimalPlaces = CurrencyToDecimalPoints[Currency];
            var amountDecimalUnits = allowedDecimalPlaces > 2 ? amountStrSplits[1].PadRight(allowedDecimalPlaces, '0') : amountStrSplits[1].Substring(0, allowedDecimalPlaces);
            amountStr = string.Format("{0},{1}", amountStrSplits[0], amountDecimalUnits);
            return string.Format("{0}{1}", Currency ?? FieldConstants.USD, amountStr);
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
                    derivedValue = Amount > 0 ? Amount.ToString(CultureInfo.InvariantCulture) : (Value.Length > 3 ? Value.Substring(3, Value.Length - 3) : Amount.ToString(CultureInfo.InvariantCulture));
                    derivedValue = derivedValue.Replace(",", ".");
                    break;
            }

            return component.GetComponentValue(derivedValue);
        }

    }
}
