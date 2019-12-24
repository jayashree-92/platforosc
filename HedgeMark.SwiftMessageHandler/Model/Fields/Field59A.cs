namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field59A : FieldWithAccountBICAndDCMark
    {
        public Field59A() : base(FieldDirectory.FIELD_59A)
        {
        }

        public Field59A setAccount(string account)
        {
            return setAccount(this, account);
        }
        public Field59A setBIC(string bic)
        {
            return setBIC(this, bic);
        }
        public Field59A setBICorABA(string bicOrAba, bool isAba)
        {
            return setBICorABA(this, bicOrAba, isAba);
        }
        public Field59A setDCMark(string dcMark)
        {
            return setDCMark(this, dcMark);
        }
    }
}
