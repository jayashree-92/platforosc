using System.Collections.Generic;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field20C : FieldWithQualifierAndCode
    {
        //:4!c//16x	(Qualifier)(Reference)
        //:20C::SEME//DM1306332B
        public Field20C() : base(FieldDirectory.FIELD_20C)
        {
        }
        public Field20C setReference(string reference)
        {
            return setCode(this, reference);
        }

        public Field20C setQualifier(string qualifier)
        {
            return setQualifier(this, qualifier);
        }

        public override List<string> Components
        {
            get
            {
                return new List<string>()
                {
                    FieldConstants.QUALIFIER,FieldConstants.REFERENCE
                };
            }
        }
    }
}
