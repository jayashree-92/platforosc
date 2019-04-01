using System;
using System.Collections.Generic;
using System.Text;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class FieldWithAccountBICAndDCMark : FieldWithAccountAndBIC
    {
        public FieldWithAccountBICAndDCMark(string name) : base(name)
        {
        }


        public override List<string> Components
        {
            get
            {
                return new List<string>()
                {
                    FieldConstants.DC_MARK,FieldConstants.ACCOUNT,FieldConstants.BIC
                };
            }
        }

        public override string GetComponentValue(string component)
        {
            var bicStartIndex = Value.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            switch (component)
            {
                case FieldConstants.DC_MARK:
                    if (Value.StartsWith("/") && Value.Length > 2 && Value[2].Equals('/'))
                        return DCMark = !string.IsNullOrWhiteSpace(DCMark) ? DCMark : Value[1].ToString();
                    return string.Empty;
                case FieldConstants.ACCOUNT:
                    var derivedVal = !string.IsNullOrWhiteSpace(Account) ? Account : Value;

                    if (DCMark != null)
                        derivedVal = string.Format("/{0}", derivedVal.Replace(string.Format("/{0}/", DCMark ?? string.Empty), string.Empty));

                    return Account = component.GetComponentValue(derivedVal);
                case FieldConstants.BIC:

                    if (!string.IsNullOrWhiteSpace(BIC))
                        return BIC;

                    var derivedValue = bicStartIndex == -1 ? Value : Value.Length >= bicStartIndex + 6 ? Value.Substring(bicStartIndex, Value.Length - bicStartIndex) : string.Empty;

                    if (DCMark != null)
                        derivedValue = string.Format("/{0}", derivedValue.Replace(string.Format("/{0}/", DCMark ?? string.Empty), string.Empty));

                    if (Account != null)
                        derivedValue = derivedValue.Replace(string.Format("/{0}", Account ?? string.Empty), string.Empty);

                    return component.GetComponentValue(derivedValue);

            }

            return string.Empty;
        }

        public string DCMark { get; set; }

        public override string GetValue()
        {
            var builder = new StringBuilder("/");

            if (!string.IsNullOrWhiteSpace(DCMark))
                builder.Append(string.Format("{0}/", DCMark));
            if (!string.IsNullOrWhiteSpace(Account))
                builder.Append(Account);
            if (!string.IsNullOrWhiteSpace(BIC))
                builder.AppendFormat("{0}{1}", Environment.NewLine, BIC);

            return Value = builder.ToString();
        }

        public T setDCMark<T>(T callingClass, string account)
        {
            Account = account;
            return callingClass;
        }
    }
}
