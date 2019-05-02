namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field58A : FieldWithAccountBICAndDCMark
    {
        public Field58A() : base(FieldDirectory.FIELD_58A)
        {
        }

        public Field58A setAccount(string account)
        {
            return setAccount(this, account);
        }
        public Field58A setBIC(string bic)
        {
            return setBIC(this, bic);
        }
        public Field58A setBICorABA(string bicOrAba, bool isAba)
        {
            return setBICorABA(this, bicOrAba, isAba);
        }
        public Field58A setDCMark(string dcMark)
        {
            return setDCMark(this, dcMark);
        }
    }
}
