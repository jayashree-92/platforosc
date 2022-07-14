namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field50C : FieldWithAccountAndBIC
    {
        public Field50C() : base(FieldDirectory.FIELD_50C)
        {
        }

        public Field50C setAccount(string account)
        {
            return setAccount(this, account);
        }

        public Field50C setBIC(string bic)
        {
            return setBIC(this, bic);
        }
    }
}
