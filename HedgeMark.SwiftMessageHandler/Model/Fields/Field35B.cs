using System;
using System.Collections.Generic;
using System.Text;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field35B : Field
    {
        //[ISIN1!e12!c] (Identification of Security)
        //[4*35x]	(Description of Security)
        // 
        public Field35B() : base(FieldDirectory.FIELD_35B)
        {
        }

        public string Qualifier { get; set; }
        public string ISIN { get; set; }

        public string Description { get; set; }
        public string DescriptionLine1 { get; set; }
        public string DescriptionLine2 { get; set; }
        public string DescriptionLine3 { get; set; }

        protected string DescriptionInFull
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Description))
                    return $"{Environment.NewLine}{Description}";

                var builder = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(DescriptionLine1))
                    builder.AppendFormat("{0}{1}", Environment.NewLine, DescriptionLine1);
                if (!string.IsNullOrWhiteSpace(DescriptionLine2))
                    builder.AppendFormat("{0}{1}", Environment.NewLine, DescriptionLine2);
                if (!string.IsNullOrWhiteSpace(DescriptionLine3))
                    builder.AppendFormat("{0}{1}", Environment.NewLine, DescriptionLine3);
                return builder.ToString();
            }
        }

        public override string GetValue()
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(ISIN))
                builder.Append("ISIN " + ISIN);
            if (!string.IsNullOrWhiteSpace(DescriptionInFull))
                builder.Append(DescriptionInFull);

            return builder.ToString();
        }

        public Field35B setISIN(string isin)
        {
            ISIN = isin;
            return this;
        }

        public Field35B setDescription(string desc)
        {
            Description = desc;
            return this;
        }

        public Field35B setDescriptionLine1(string desc)
        {
            DescriptionLine1 = desc;
            return this;
        }
        public Field35B setDescriptionLine2(string desc)
        {
            DescriptionLine2 = desc;
            return this;
        }
        public Field35B setDescriptionLine3(string desc)
        {
            DescriptionLine3 = desc;
            return this;
        }

        public override List<string> Components =>
            new List<string>()
            {
                FieldConstants.ISIN,FieldConstants.DESCRIPTION,FieldConstants.DESCRIPTION_LINE_2,FieldConstants.DESCRIPTION_LINE_3
            };

        public override string GetComponentValue(string component)
        {
            var DescriptionStartIndex = component.GetComponentValue(!string.IsNullOrWhiteSpace(ISIN) ? ISIN : Value).Length;
            var addressLine1 = !string.IsNullOrWhiteSpace(Description) ? Description : Value.Length > DescriptionStartIndex ? Value.Substring(DescriptionStartIndex, Value.Length - DescriptionStartIndex) : string.Empty;

            var address = addressLine1.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            switch (component)
            {
                case FieldConstants.ISIN:
                    string derivedVal;
                    if (!string.IsNullOrWhiteSpace(ISIN))
                        return ISIN;

                    derivedVal = Value;
                    return ISIN = component.GetComponentValue(derivedVal);
                case FieldConstants.DESCRIPTION:
                    return address.Length > 0 ? address[0] : string.Empty;
                case FieldConstants.DESCRIPTION_LINE_2:
                    return address.Length > 1 && !string.IsNullOrWhiteSpace(address[1]) ? address[1] : string.Empty;
                case FieldConstants.DESCRIPTION_LINE_3:
                    return address.Length > 2 && !string.IsNullOrWhiteSpace(address[2]) ? address[2] : string.Empty;
            }

            return string.Empty;
        }

    }
}
