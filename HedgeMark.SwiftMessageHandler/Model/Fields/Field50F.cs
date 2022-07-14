using System.Text;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field50F : FieldWithAccountAndAddress
    {
        public Field50F() : base(FieldDirectory.FIELD_50F)
        {
        }

        public Field50F setAccount(string account)
        {
            return setAccount(this, account);
        }

        public Field50F setNameAndAddress(string nameAndAddress)
        {
            return setNameAndAddress(this, nameAndAddress);
        }

        public Field50F setNameAndAddressLine1(string nameAndAddressLine1)
        {
            return setNameAndAddressLine1(this, nameAndAddressLine1);
        }

        public Field50F setNameAndAddressLine2(string nameAndAddressLine2)
        {
            return setNameAndAddressLine2(this, nameAndAddressLine2);
        }

        public Field50F setNameAndAddressLine3(string nameAndAddressLine3)
        {
            return setNameAndAddressLine3(this, nameAndAddressLine3);
        }

        public override string GetValue()
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(DCMark))
                builder.Append($"{DCMark}/");
            if (!string.IsNullOrWhiteSpace(Account))
                builder.AppendFormat("/{0}", Account);
            if (!string.IsNullOrWhiteSpace(NameAndFullAddress))
                builder.Append(NameAndFullAddress);

            return builder.ToString();
        }

        public string DCMark { get; set; }

        public Field50F setDCMark(string dcMark)
        {
            DCMark = dcMark;
            return this;
        }
    }
}
