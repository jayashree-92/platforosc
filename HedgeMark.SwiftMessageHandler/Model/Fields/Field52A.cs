namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field52A : FieldWithAccountBICAndDCMark
    {
        public Field52A() : base(FieldDirectory.FIELD_52A)
        {
        }

        public Field52A setAccount(string account)
        {
            return setAccount(this, account);
        }

        public Field52A setBIC(string bic)
        {
            return setBIC(this, bic);
        }

        public Field52A setDCMark(string dcMark)
        {
            return setDCMark(this, dcMark);
        }
    }
}
