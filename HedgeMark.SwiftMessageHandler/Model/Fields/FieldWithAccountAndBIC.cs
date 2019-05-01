using System;
using System.Collections.Generic;
using System.Text;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class FieldWithAccountAndBIC : Field
    {
        public FieldWithAccountAndBIC(string name) : base(name)
        {
        }

        public string Account { get; set; }
        public string BIC { get; set; }


        public override List<string> Components
        {
            get
            {
                return new List<string>()
                {
                    FieldConstants.ACCOUNT,FieldConstants.BIC
                };
            }
        }

        public override string GetComponentValue(string component)
        {
            switch (component)
            {
                case FieldConstants.ACCOUNT:
                    var derivedVal = !string.IsNullOrWhiteSpace(Account) ? Account : Value;
                    return component.GetComponentValue(derivedVal);

                case FieldConstants.BIC:
                    //var lengthOfAccount = GetComponentValue(FieldConstants.ACCOUNT).Length + 1;
                    var bicStartIndex = Value.IndexOf("\n", StringComparison.Ordinal);
                    var bicCode= !string.IsNullOrWhiteSpace(BIC) ? BIC : Value.Length > bicStartIndex ? Value.Substring(bicStartIndex, Value.Length - bicStartIndex) : string.Empty;
                    return component.GetComponentValue(bicCode);
            }

            return string.Empty;
        }

        public T setAccount<T>(T callingClass, string account)
        {
            if (string.IsNullOrWhiteSpace(account))
                return callingClass;

            Account = account.PadRight(12, 'X');
            return callingClass;
        }

        public T setBIC<T>(T callingClass, string bic)
        {
            if (string.IsNullOrWhiteSpace(bic))
                return callingClass;

            BIC = bic.PadRight(8, 'X');
            return callingClass;
        }

        public override string GetValue()
        {
            var builder = new StringBuilder("/");

            if (!string.IsNullOrWhiteSpace(Account))
                builder.Append(Account);
            if (!string.IsNullOrWhiteSpace(BIC))
                builder.AppendFormat("{0}{1}", Environment.NewLine, BIC);

            return builder.ToString();
        }
    }
}
