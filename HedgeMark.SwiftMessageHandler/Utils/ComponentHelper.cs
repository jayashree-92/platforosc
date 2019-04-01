using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HedgeMark.SwiftMessageHandler.Model.Fields;

namespace HedgeMark.SwiftMessageHandler.Utils
{
    internal static class ComponentHelper
    {
        public static string GetComponentValue(this string component, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            string componentVal;
            switch (component)
            {
                case FieldConstants.ACCOUNT:

                    var accountStartIndex = -1;
                    var accountEndIndex = 0;

                    if (value.IndexOf("/", StringComparison.Ordinal) >= 0)
                        accountStartIndex = value.IndexOf("/", StringComparison.Ordinal) + 1;
                    if (value.IndexOf("\n", StringComparison.Ordinal) >= 0)
                        accountEndIndex = value.IndexOf("\n", StringComparison.Ordinal);

                    //if there  is no backward slash, then no account number is present
                    if (accountStartIndex == -1)
                        return string.Empty;

                    var accountLength = (accountEndIndex - accountStartIndex);

                    if (accountLength <= 0)
                        return string.Empty;

                    componentVal = value.Substring(accountStartIndex, (accountEndIndex - accountStartIndex));
                    break;
                case FieldConstants.PRICE:
                case FieldConstants.AMOUNT:
                    componentVal = Extensions.GetFormatedCurrency(value.Replace(",", "."));
                    break;
                case FieldConstants.DATE:
                    DateTime dateTime;
                    DateTime.TryParseExact(value, new[] { Extensions.DefaultDateFormat, Extensions.DefaultDateFormatWithFullYear }, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
                    componentVal = dateTime.ToString("MMM dd, yyyy");
                    break;
                default:
                    componentVal = value;
                    break;
            }

            return componentVal.Replace("\n", string.Empty).Replace("\r", string.Empty);

        }
    }
}
