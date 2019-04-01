namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field50A : FieldWithAccountAndBIC
    {
        public Field50A() : base(FieldDirectory.FIELD_50A)
        {
        }

        public Field50A setAccount(string account)
        {
            return setAccount(this, account);
        }

        public Field50A setBIC(string bic)
        {
            return setBIC(this, bic);
        }
    }
}
