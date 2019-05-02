namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field53A : FieldWithAccountBICAndDCMark
    {
        public Field53A() : base(FieldDirectory.FIELD_53A)
        {
        }

        public Field53A setAccount(string account)
        {
            return setAccount(this, account);
        }

        public Field53A setBIC(string bic)
        {
            return setBIC(this, bic);
        }


        public Field53A setBICorABA(string bicOrAba, bool isAba)
        {
            return setBICorABA(this, bicOrAba, isAba);
        }


        public Field53A setDCMark(string dcMark)
        {
            return setDCMark(this, dcMark);
        }
    }
}
