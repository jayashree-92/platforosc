using System.Text;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field52D : FieldWithAccountAndAddress
    {
        public Field52D() : base(FieldDirectory.FIELD_52D)
        {
        }

        public Field52D setAccount(string account)
        {
            return setAccount(this, account);
        }

        public Field52D setNameAndAddress(string nameAndAddress)
        {
            return setNameAndAddress(this, nameAndAddress);
        }

        public Field52D setNameAndAddressLine1(string nameAndAddressLine1)
        {
            return setNameAndAddressLine1(this, nameAndAddressLine1);
        }

        public Field52D setNameAndAddressLine2(string nameAndAddressLine2)
        {
            return setNameAndAddressLine2(this, nameAndAddressLine2);
        }

        public Field52D setNameAndAddressLine3(string nameAndAddressLine3)
        {
            return setNameAndAddressLine3(this, nameAndAddressLine3);
        }

        public override string GetValue()
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(DCMark))
                builder.Append(string.Format("{0}/", DCMark));
            if (!string.IsNullOrWhiteSpace(Account))
                builder.AppendFormat("/{0}", Account);
            if (!string.IsNullOrWhiteSpace(NameAndFullAddress))
                builder.Append(NameAndFullAddress);

            return builder.ToString();
        }

        public string DCMark { get; set; }

        public Field52D setDCMark(string dcMark)
        {
            DCMark = dcMark;
            return this;
        }
    }
}
