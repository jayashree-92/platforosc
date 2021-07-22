namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field50K : FieldWithAccountAndAddress
    {
        public Field50K() : base(FieldDirectory.FIELD_50K)
        {
        }

        public Field50K setAccount(string account)
        {
            return setAccount(this, account);
        }

        public Field50K setNameAndAddress(string nameAndAddress)
        {
            return setNameAndAddress(this, nameAndAddress);
        }

        public Field50K setNameAndAddressLine1(string nameAndAddressLine1)
        {
            return setNameAndAddressLine1(this, nameAndAddressLine1);
        }

        public Field50K setNameAndAddressLine2(string nameAndAddressLine2)
        {
            return setNameAndAddressLine2(this, nameAndAddressLine2);
        }

        public Field50K setNameAndAddressLine3(string nameAndAddressLine3)
        {
            return setNameAndAddressLine3(this, nameAndAddressLine3);
        }

        public Field50K setNameAndAddressLine4(string nameAndAddressLine4)
        {
            return setNameAndAddressLine4(this, nameAndAddressLine4);
        }
    }
}
