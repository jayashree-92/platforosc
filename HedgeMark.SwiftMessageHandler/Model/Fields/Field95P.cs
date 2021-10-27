using System.Collections.Generic;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    //:95P::PSET//SICVFRPP
    public class Field95P : FieldWithQualifierAndCode
    {
        public Field95P() : base(FieldDirectory.FIELD_95P)
        {
        }

        public Field95P setBIC(string bic)
        {
            return setCode(this, bic);
        }

        public Field95P setQualifier(string qualifier)
        {
            return setQualifier(this, qualifier);
        }

        public override List<string> Components => new List<string>() { FieldConstants.QUALIFIER, FieldConstants.BIC };
    }
}
