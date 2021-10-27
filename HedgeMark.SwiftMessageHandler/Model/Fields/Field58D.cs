using System.Text;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field58D : FieldWithAccountAndAddress
    {
        public Field58D() : base(FieldDirectory.FIELD_58D)
        {
        }

        public Field58D setAccount(string account)
        {
            return setAccount(this, account);
        }

        public Field58D setNameAndAddress(string nameAndAddress)
        {
            return setNameAndAddress(this, nameAndAddress);
        }

        public Field58D setNameAndAddressLine1(string nameAndAddressLine1)
        {
            return setNameAndAddressLine1(this, nameAndAddressLine1);
        }

        public Field58D setNameAndAddressLine2(string nameAndAddressLine2)
        {
            return setNameAndAddressLine2(this, nameAndAddressLine2);
        }

        public Field58D setNameAndAddressLine3(string nameAndAddressLine3)
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

        public Field58D setDCMark(string dcMark)
        {
            DCMark = dcMark;
            return this;
        }
    }
}
