using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class FieldWithAccountAndAddress : Field
    {
        public FieldWithAccountAndAddress(string name) : base(name)
        {
        }

        public string Account { get; set; }
        public string NameAndAddressLine1 { get; set; }
        public string NameAndAddressLine2 { get; set; }
        public string NameAndAddressLine3 { get; set; }
        public string NameAndAddressLine4 { get; set; }

        protected string NameAndFullAddress
        {
            get
            {
                var builder = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(NameAndAddressLine1))
                    builder.AppendFormat("{0}{1}", Environment.NewLine, NameAndAddressLine1.TrimToLength());
                if (!string.IsNullOrWhiteSpace(NameAndAddressLine2))
                    builder.AppendFormat("{0}{1}", Environment.NewLine, NameAndAddressLine2.TrimToLength());
                if (!string.IsNullOrWhiteSpace(NameAndAddressLine3))
                    builder.AppendFormat("{0}{1}", Environment.NewLine, NameAndAddressLine3.TrimToLength());
                if (!string.IsNullOrWhiteSpace(NameAndAddressLine4))
                    builder.AppendFormat("{0}{1}", Environment.NewLine, NameAndAddressLine4.TrimToLength());
                return builder.ToString();
            }
        }

        public override string GetValue()
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Account))
                builder.AppendFormat("/{0}", Account);
            if (!string.IsNullOrWhiteSpace(NameAndFullAddress))
                builder.Append(NameAndFullAddress);

            return Value = builder.ToString();
        }

        protected T setAccount<T>(T callingClass, string account)
        {
            Account = account;
            return callingClass;
        }

        public T setNameAndAddress<T>(T callingClass, string nameAndAddress)
        {
            if (string.IsNullOrWhiteSpace(nameAndAddress))
                return callingClass;

            nameAndAddress = Regex.Replace(nameAndAddress, @"\t|\r", "");
            var addressLines = nameAndAddress.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (addressLines.Length > 0)
                setNameAndAddressLine1(callingClass, addressLines[0].Trim());
            if (addressLines.Length > 1)
                setNameAndAddressLine2(callingClass, addressLines[1].Trim());
            if (addressLines.Length > 2)
                setNameAndAddressLine3(callingClass, addressLines[2].Trim());
            if (addressLines.Length > 3)
                setNameAndAddressLine4(callingClass, addressLines[3].Trim());

            return callingClass;
        }

        public T setNameAndAddressLine1<T>(T callingClass, string nameAndAddressLine1)
        {
            if (!string.IsNullOrWhiteSpace(nameAndAddressLine1))
                nameAndAddressLine1 = RemoveInvalidXCharacterSet(nameAndAddressLine1);

            NameAndAddressLine1 = nameAndAddressLine1;
            return callingClass;
        }

        public T setNameAndAddressLine2<T>(T callingClass, string nameAndAddressLine2)
        {
            if (!string.IsNullOrWhiteSpace(nameAndAddressLine2))
                nameAndAddressLine2 = RemoveInvalidXCharacterSet(nameAndAddressLine2);

            NameAndAddressLine2 = nameAndAddressLine2;
            return callingClass;
        }

        public T setNameAndAddressLine3<T>(T callingClass, string nameAndAddressLine3)
        {
            if (!string.IsNullOrWhiteSpace(nameAndAddressLine3))
                nameAndAddressLine3 = RemoveInvalidXCharacterSet(nameAndAddressLine3);

            NameAndAddressLine3 = nameAndAddressLine3;
            return callingClass;
        }

        public T setNameAndAddressLine4<T>(T callingClass, string nameAndAddressLine4)
        {
            if (!string.IsNullOrWhiteSpace(nameAndAddressLine4))
                nameAndAddressLine4 = RemoveInvalidXCharacterSet(nameAndAddressLine4);

            NameAndAddressLine4 = nameAndAddressLine4;
            return callingClass;
        }


        public override List<string> Components =>
            new List<string>()
            {
                FieldConstants.ACCOUNT,FieldConstants.NAME_AND_ADDRESS,FieldConstants.NAME_AND_ADDRESS_LINE_2,FieldConstants.NAME_AND_ADDRESS_LINE_3,FieldConstants.NAME_AND_ADDRESS_LINE_4
            };

        public override string GetComponentValue(string component)
        {
            var nameAndAddressStartIndex = component.GetComponentValue(!string.IsNullOrWhiteSpace(Account) ? Account : Value).Length + 1;
            var addressLine1 = !string.IsNullOrWhiteSpace(NameAndFullAddress) ? NameAndFullAddress : Value.Length > nameAndAddressStartIndex ? Value.Substring(nameAndAddressStartIndex, Value.Length - nameAndAddressStartIndex) : string.Empty;

            var address = addressLine1.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            switch (component)
            {
                case FieldConstants.ACCOUNT:
                    string derivedVal;
                    if (!string.IsNullOrWhiteSpace(Account))
                        return Account;

                    derivedVal = Value;
                    return Account = component.GetComponentValue(derivedVal);
                case FieldConstants.NAME_AND_ADDRESS:
                    return address.Length > 0 ? address[0] : string.Empty;
                case FieldConstants.NAME_AND_ADDRESS_LINE_2:
                    return address.Length > 1 && !string.IsNullOrWhiteSpace(address[1]) ? address[1] : string.Empty;
                case FieldConstants.NAME_AND_ADDRESS_LINE_3:
                    return address.Length > 2 && !string.IsNullOrWhiteSpace(address[2]) ? address[2] : string.Empty;
                case FieldConstants.NAME_AND_ADDRESS_LINE_4:
                    return address.Length > 3 && !string.IsNullOrWhiteSpace(address[3]) ? address[3] : string.Empty;
            }

            return string.Empty;
        }
    }
}
