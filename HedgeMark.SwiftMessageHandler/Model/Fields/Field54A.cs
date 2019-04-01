namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field54A : FieldWithAccountBICAndDCMark
    {
        public Field54A() : base(FieldDirectory.FIELD_54A)
        {
        }

        public Field54A setAccount(string account)
        {
            return setAccount(this, account);
        }

        public Field54A setBIC(string bic)
        {
            return setBIC(this, bic);
        }

        public Field54A setDCMark(string dcMark)
        {
            return setDCMark(this, dcMark);
        }
    }
}
