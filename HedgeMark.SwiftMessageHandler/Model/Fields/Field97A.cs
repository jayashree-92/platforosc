using System.Collections.Generic;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    //:97A::SAFE//TDBANK
    public class Field97A : FieldWithQualifierAndCode
    {
        //	:4!c//35x (Qualifier)(Account Number)
        // :97A::SAFE//720134
        public Field97A() : base(FieldDirectory.FIELD_97A)
        {

        }

        public Field97A setAccount(string account)
        {
            return setCode(this, account);
        }

        public Field97A setQualifier(string qualifier)
        {
            return setQualifier(this, qualifier);
        }

        
        public override List<string> Components =>
            new List<string>()
            {
                FieldConstants.QUALIFIER,FieldConstants.ACCOUNT
            };
    }
}
