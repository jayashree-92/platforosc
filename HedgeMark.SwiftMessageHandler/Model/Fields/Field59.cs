namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field59 : FieldWithAccountAndAddress
    {
        public Field59() : base(FieldDirectory.FIELD_59)
        {
        }

        public Field59 setAccount(string account)
        {
            return setAccount(this, account);
        }

        public Field59 setNameAndAddress(string nameAndAddress)
        {
            return setNameAndAddress(this, nameAndAddress);
        }

        public Field59 setNameAndAddressLine1(string nameAndAddressLine1)
        {
            return setNameAndAddressLine1(this, nameAndAddressLine1);
        }

        public Field59 setNameAndAddressLine2(string nameAndAddressLine2)
        {
            return setNameAndAddressLine2(this, nameAndAddressLine2);
        }

        public Field59 setNameAndAddressLine3(string nameAndAddressLine3)
        {
            return setNameAndAddressLine3(this, nameAndAddressLine3);
        }

        public Field59 setNameAndAddressLine4(string nameAndAddressLine4)
        {
            return setNameAndAddressLine4(this, nameAndAddressLine4);
        }
    }
}
