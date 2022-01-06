using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Com.HedgeMark.Commons;

namespace HM.Operations.Secure.Middleware.Util
{
    public static class Utility
    {

        //public static List<dmaHoliday> Holidays = new List<dmaHoliday>();
        //public static List<DateTime> HolidayDates
        //{
        //    get { return Holidays.Select(s => s.HolidayContextDate.Date).ToList(); }
        //}
        public static string Environment = ConfigurationManagerWrapper.StringSetting("Environment");

        public static readonly TimeZoneInfo DefaultSystemTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        public static readonly TextInfo TextInfo = new CultureInfo("en-US", false).TextInfo;

        public static bool IsLocal => Environment.Equals("Local");

        public static bool IsLowerEnvironment => !ConfigurationManagerWrapper.StringSetting("Environment").Equals("Prod");

        public static bool IsWorkingDay(DateTime contextDate)
        {
            return contextDate.DayOfWeek != DayOfWeek.Saturday && contextDate.DayOfWeek != DayOfWeek.Sunday;
        }

        public static DateTime GetContextDate(this DateTime contextDate)
        {
            contextDate = contextDate.AddDays(-1);
            while (!IsWorkingDay(contextDate))
            {
                contextDate = contextDate.AddDays(-1);
            }
            return contextDate.Date;
        }

        public static DateTime GetStartOfMonth(this DateTime contextDate)
        {
            return new DateTime(contextDate.Year, contextDate.Month, 1);
        }

        public static DateTime GetStartOfYear(this DateTime contextDate)
        {
            return new DateTime(contextDate.Year, 1, 1);
        }

        public static string HumanizeEmail(this string emailId)
        {
            if (string.IsNullOrWhiteSpace(emailId))
                return string.Empty;

            var charIndex = emailId.IndexOf("@", StringComparison.InvariantCultureIgnoreCase);
            if (charIndex > 0)
                emailId = emailId.Substring(0, charIndex);

            return emailId.Replace(".", " ").Titleize();
        }

        public static string Titleize(this string inputStr)
        {
            inputStr = inputStr.Replace("-", " ");
            return TextInfo.ToTitleCase(inputStr);
        }

        public static string GetMailBoxName(this string emailId)
        {
            var charIndex = emailId.IndexOf("@", StringComparison.InvariantCultureIgnoreCase);
            if (charIndex > 0)
                emailId = emailId.Substring(0, charIndex);

            return emailId;
        }

        private const string HtmlRegexPatten = "<.*?>";
        public static string StripHtml(this string input)
        {
            return Regex.Replace(input, HtmlRegexPatten, string.Empty);
        }

        public static readonly string MailSignatureOps = $"<br/><br/><br/>Regards,<br/>HedgeMark Operations Team. {HedgeMarkOpsAddressOnly}";

        public static string HedgeMarkOpsAddressOnly
        {
            get
            {
                var mailSignatureLine =  OpsSecureSwitches.GetSwitchValue(Switches.SwitchKey.HedgeMarkMailOpsSignatureLines).Replace("\n", "<br />");
                return @"<br /><br /><div style = 'font-size:11px' >" + mailSignatureLine + "</div>";
            }
        }
    }

    public static class ReportName
    {
        public static readonly string PositionReconciliation = "2-way Position Rec";
        public static readonly string MultiWayRec = "Multi-way Rec";
        public static readonly string CashReconciliation = "2-way Cash Rec";
        public static readonly string AdhocReconciliation = "Ad-hoc 2-way Rec";
        public static readonly string IndpAmount = "Indp.Amount";
        public static readonly string Collateral = "Collateral";
        public static readonly string Cash = "Cash";
        public static readonly string Margin = "Margin";
        public static readonly string NAV = "NAV Report";
        public static readonly string CapitalActivity = "Capital Activity";
        public static readonly string Loader = "Loader";
        public static readonly string InterestReport = "Interest Report";
        public static readonly string Invoices = "Invoices";
        public static readonly string Expenses = "Expenses";
        public static readonly string NavReview = "NAV Review";
        public static readonly string ManagerReturns = "Manager Returns";
        public static readonly string CashAndMargin = "Cash & Margin";
        public static readonly string RepoCollateral = "Repo Collateral";

        //Wires Report
        public static readonly string AdhocWireReport = "Adhoc Report";
    }
}
