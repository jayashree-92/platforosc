namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field90A : FieldWithQualifierCodeAndValue
    {

        //	:4!c//4!c/[N]15d
        public Field90A() : base(FieldDirectory.FIELD_90A)
        {
        }

        public Field90A setQualifier(string qualifier)
        {
            return setQualifier(this, qualifier);
        }

        public Field90A setCode(string code)
        {
            return setCode(this, code);
        }

        public Field90A setPercentage(decimal price)
        {
            return setQualifierValue(this, price);
        }
    }
}
