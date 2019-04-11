using System;
using System.Collections.Generic;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field22H : Field
    {
        //:4!c//4!c	(Qualifier)(Code)
        //:22H::REDE//DELI
        public Field22H() : base(FieldDirectory.FIELD_22H)
        {
        }

        public string Qualifier { get; set; }
        public string Code { get; set; }

        public Field22H setCode(string code)
        {
            this.Code = code;
            return this;
        }

        public Field22H setQualifier(string qualifier)
        {
            this.Qualifier = qualifier;
            return this;
        }
        public override string GetValue()
        {
            return string.Format(":{0}//{1}", Qualifier, Code);
        }

        public override List<string> Components
        {
            get
            {
                return new List<string>()
                {
                    FieldConstants.QUALIFIER,FieldConstants.CODE
                };
            }
        }

        public override string GetComponentValue(string component)
        {
            switch (component)
            {
                case FieldConstants.QUALIFIER:
                    if (!string.IsNullOrWhiteSpace(Qualifier)) return Qualifier;
                    return Value.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace(":", string.Empty);

                case FieldConstants.REFERENCE:
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
