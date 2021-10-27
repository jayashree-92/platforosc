using System.Collections.Generic;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field22F : Field
    {

        //	:4!c/[8c]/4!c	(Qualifier)(Data Source Scheme)(Indicator)
        //  :22F::COLA//OTCD
        public Field22F() : base(FieldDirectory.FIELD_22F)
        {
        }


        //private static readonly List<string> ValidQualifiers = new List<string>()
        //{
        //    "CFRE", //Variable Rate Change Frequency Indicator-Specifies the frequency of change to the variable rate of an interest bearing instrument.
        //    "FORM", //Form of Securities Indicator-Specifies the form of the financial instrument.
        //    "MICO", //Method of Interest Computation Indicator-Specifies the computation method of (accrued) interest of the financial instrument.
        //    "PAYS", //Payment Status Indicator-Specifies the status of the payment of a financial instrument at a particular time, as agreed with the issuer.
        //    "PFRE", //Payment Frequency Indicator-Specifies the frequency of a payment
        //};

        public string Qualifier { get; set; }
        public string DataSource { get; set; }
        public string Indicator { get; set; }


        public Field22F setQualifier(string qualifier)
        {
            this.Qualifier = qualifier;
            FieldValidator.Validate(qualifier, "4!c");
            return this;
        }

        public Field22F setDataSource(string dataSource)
        {
            this.DataSource = dataSource;
            return this;
        }

        public Field22F setIndicator(string indicator)
        {
            this.Indicator = indicator;
            FieldValidator.Validate(indicator, "4!c");
            return this;
        }

        public override string GetValue()
        {
            return $":{Qualifier}/{DataSource}/{Indicator}";
        }


        public override List<string> Components =>
            new List<string>()
            {
                FieldConstants.QUALIFIER,FieldConstants.DATA_SOURCE,FieldConstants.INDICATOR
            };

        public override string GetComponentValue(string component)
        {
            switch (component)
            {
                case FieldConstants.QUALIFIER:
                    var derivedVal = !string.IsNullOrWhiteSpace(Qualifier) ? Qualifier : Value.Replace(":", string.Empty);
                    return derivedVal.Length >= 4 ? derivedVal.Substring(0, 4) : string.Empty;

                case FieldConstants.DATA_SOURCE:
                    if (!string.IsNullOrWhiteSpace(DataSource)) return DataSource;
                    if (Value.Contains("//")) return string.Empty;
                    return Value.Split('/').Length >= 3 ? Value.Split('/')[1] : string.Empty;

                case FieldConstants.INDICATOR:
                    if (!string.IsNullOrWhiteSpace(Indicator)) return Indicator;
                    return Value.Split('/').Length >= 3 ? Value.Split('/')[2] : string.Empty;

            }

            return string.Empty;
        }

    }
}
