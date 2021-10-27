using System;
using System.Collections.Generic;
using System.Text;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field53B : Field
    {
        public Field53B() : base(FieldDirectory.FIELD_53B)
        {
        }

        public string DCMark { get; set; }
        public string Account { get; set; }
        public string Location { get; set; }


        public override List<string> Components =>
            new List<string>()
            {
                FieldConstants.DC_MARK,FieldConstants.ACCOUNT,FieldConstants.LOCATION
            };


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
                        derivedVal =
                            $"/{derivedVal.Replace($"/{DCMark ?? string.Empty}/", string.Empty)}";

                    return Account = component.GetComponentValue(derivedVal);
                case FieldConstants.LOCATION:

                    if (!string.IsNullOrWhiteSpace(Location))
                        return Location;

                    var derivedValue = bicStartIndex == -1 ? Value : Value.Length >= bicStartIndex + 6 ? Value.Substring(bicStartIndex, Value.Length - bicStartIndex) : string.Empty;

                    if (DCMark != null)
                        derivedValue =
                            $"/{derivedValue.Replace($"/{DCMark ?? string.Empty}/", string.Empty)}";

                    if (Account != null)
                        derivedValue = derivedValue.Replace($"/{Account ?? string.Empty}", string.Empty);

                    return component.GetComponentValue(derivedValue);
            }

            return string.Empty;
        }


        public Field53B setAccount(string account)
        {
            Account = account;
            return this;
        }

        public Field53B setLocation(string location)
        {
            Location = location;
            return this;
        }


        public Field53B setDCMark(string dcMark)
        {
            DCMark = dcMark;
            return this;
        }

        public override string GetValue()
        {
            var builder = new StringBuilder();

            var isAccAvailable = !string.IsNullOrWhiteSpace(Account);

            if (!string.IsNullOrWhiteSpace(DCMark))
                builder.Append($"{DCMark}/");

            else
            {
                if (isAccAvailable)
                    builder.AppendFormat("/{0}", Account);

                if (!string.IsNullOrWhiteSpace(Location))
                    builder.AppendFormat("{0}{1}", isAccAvailable ? Environment.NewLine : string.Empty, Location);
            }

            return builder.ToString();
        }

    }
}
