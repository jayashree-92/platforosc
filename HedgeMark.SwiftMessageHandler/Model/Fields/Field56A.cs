namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field56A : FieldWithAccountBICAndDCMark
    {
        public Field56A() : base(FieldDirectory.FIELD_56A)
        {
        }

        public Field56A setAccount(string account)
        {
            return setAccount(this, account);
        }

        public Field56A setBIC(string bic)
        {
            return setBIC(this, bic);
        }

        public Field56A setDCMark(string dcMark)
        {
            return setDCMark(this, dcMark);
        }
    }
}
