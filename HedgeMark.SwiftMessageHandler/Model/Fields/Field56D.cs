using System.Text;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field56D : FieldWithAccountAndAddress
    {
        public Field56D() : base(FieldDirectory.FIELD_56D)
        {
        }

        public Field56D setAccount(string account)
        {
            return setAccount(this, account);
        }

        public Field56D setNameAndAddress(string nameAndAddress)
        {
            return setNameAndAddress(this, nameAndAddress);
        }

        public Field56D setNameAndAddressLine1(string nameAndAddressLine1)
        {
            return setNameAndAddressLine1(this, nameAndAddressLine1);
        }

        public Field56D setNameAndAddressLine2(string nameAndAddressLine2)
        {
            return setNameAndAddressLine2(this, nameAndAddressLine2);
        }

        public Field56D setNameAndAddressLine3(string nameAndAddressLine3)
        {
            return setNameAndAddressLine3(this, nameAndAddressLine3);
        }

        public override string GetValue()
        {
            var builder = new StringBuilder("/");

            if (!string.IsNullOrWhiteSpace(DCMark))
                builder.Append(string.Format("{0}/", DCMark));
            if (!string.IsNullOrWhiteSpace(Account))
                builder.Append(Account);
            if (!string.IsNullOrWhiteSpace(NameAndFullAddress))
                builder.Append(NameAndFullAddress);

            return builder.ToString();
        }

        public string DCMark { get; set; }

        public Field56D setDCMark(string dcMark)
        {
            DCMark = dcMark;
            return this;
        }
    }
}
