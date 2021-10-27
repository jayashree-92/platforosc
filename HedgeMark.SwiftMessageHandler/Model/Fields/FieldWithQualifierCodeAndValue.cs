using System;
using System.Collections.Generic;
using System.Globalization;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class FieldWithQualifierCodeAndValue : Field
    {
        //	:4!c//4!c/[N]15d
        //  :36B::SETT//FAMT/1937000,
        public FieldWithQualifierCodeAndValue(string name) : base(name)
        {
        }

        public string Qualifier { get; set; }
        public string Code { get; set; }
        public string QualifierValue { get; set; }
        public string Sign { get; set; }


        public T setQualifier<T>(T callingMethod, string qualifier)
        {
            this.Qualifier = qualifier;
            return callingMethod;
        }

        public T setCode<T>(T callingMethod, string code)
        {
            this.Code = code;
            return callingMethod;
        }
        public T setQualifierValue<T>(T callingMethod, decimal price)
        {
            this.QualifierValue = price.ToString(CultureInfo.InvariantCulture).Replace(".", ",").Replace("-", string.Empty);
            Sign = price < 0 ? "N" : string.Empty;

            return callingMethod;
        }

        public override string GetValue()
        {
            return Value = $":{Qualifier}//{Code}/{Sign}{QualifierValue}";
        }

        public override List<string> Components =>
            new List<string>()
            {
                FieldConstants.QUALIFIER,FieldConstants.CODE,FieldConstants.SIGN,FieldConstants.PRICE
            };

        public override string GetComponentValue(string component)
        {
            var splitUps = Value.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
            string[] furthurSplits = { };
            string signAndprice = string.Empty;
            if (splitUps.Length > 1)
                furthurSplits = splitUps[1].Split('/');
            if (furthurSplits.Length > 1)
                signAndprice = furthurSplits[1];

            switch (component)
            {
                case FieldConstants.QUALIFIER:
                    if (!string.IsNullOrWhiteSpace(Qualifier)) return Qualifier;
                    return splitUps[0].Replace(":", string.Empty);

                case FieldConstants.CODE:
                    if (!string.IsNullOrWhiteSpace(Code)) return Code;

                    if (splitUps.Length > 1)
                        return splitUps[1];
                    return string.Empty;

                case FieldConstants.SIGN:
                    if (!string.IsNullOrWhiteSpace(Sign)) return Sign == string.Empty ? "+" : "-";

                    if (signAndprice.Length <= 0)
                        return string.Empty;

                    return signAndprice[0] == 'N' ? "-" : "+";
                case FieldConstants.QUANTITY:
                case FieldConstants.PRICE:
                    if (!string.IsNullOrWhiteSpace(QualifierValue)) return QualifierValue.GetComponentValue(component);
                    return signAndprice.Replace("N", "-").GetComponentValue(component);
            }

            return string.Empty;
        }
    }
}
