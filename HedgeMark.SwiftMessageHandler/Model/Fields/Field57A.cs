namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field57A : FieldWithAccountBICAndDCMark
    {
        public Field57A() : base(FieldDirectory.FIELD_57A)
        {
        }

        public Field57A setAccount(string account)
        {
            return setAccount(this, account);
        }

        public Field57A setBIC(string bic)
        {
            return setBIC(this, bic);
        }

        public Field57A setBICorABA(string bicOrAba, bool isAba)
        {
            return setBICorABA(this, bicOrAba, isAba);
        }

        public Field57A setDCMark(string dcMark)
        {
            return setDCMark(this, dcMark);
        }
    }
}
