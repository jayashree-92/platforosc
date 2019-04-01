using System.Collections.Generic;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field36B : FieldWithQualifierCodeAndValue
    {
        //	:4!c//4!c/[N]15d
        //  :36B::SETT//FAMT/1937000,
        public Field36B() : base(FieldDirectory.FIELD_36B)
        {

        }

        public Field36B setQualifier(string qualifier)
        {
            return setQualifier(this, qualifier);
        }

        public Field36B setCode(string code)
        {
            return setCode(this, code);
        }

        public Field36B setQuantity(decimal price)
        {
            return setQualifierValue(this, price);
        }
        public override List<string> Components
        {
            get
            {
                return new List<string>()
                {
                    FieldConstants.QUALIFIER,FieldConstants.CODE,FieldConstants.QUANTITY
                };
            }
        }
    }
}
