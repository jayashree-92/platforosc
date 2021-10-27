using System;
using System.Collections.Generic;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class FieldWithQualifierAndCode : Field
    {
        public FieldWithQualifierAndCode(string name) : base(name)
        {
        }


        public string Qualifier { get; set; }
        public string Code { get; set; }

        public T setCode<T>(T callingFunction, string code)
        {
            this.Code = code;
            return callingFunction;
        }

        public T setQualifier<T>(T callingFunction, string qualifier)
        {
            this.Qualifier = qualifier;
            return callingFunction;
        }

        public override string GetValue()
        {
            return $":{Qualifier}//{Code}";
        }

        public override List<string> Components =>
            new List<string>()
            {
                FieldConstants.QUALIFIER,FieldConstants.CODE
            };

        public override string GetComponentValue(string component)
        {
            switch (component)
            {
                case FieldConstants.QUALIFIER:
                    if (!string.IsNullOrWhiteSpace(Qualifier)) return Qualifier;
                    return Value.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(":", string.Empty);

                case FieldConstants.REFERENCE:
                case FieldConstants.BIC:
                case FieldConstants.CODE:
                    if (!string.IsNullOrWhiteSpace(Code)) return Code;

                    var splitUps = Value.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitUps.Length > 1)
                        return splitUps[1];
                    return string.Empty;
            }

            return string.Empty;
        }
    }
}
