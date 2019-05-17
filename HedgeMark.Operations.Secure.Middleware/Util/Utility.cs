using System;
using System.Globalization;
using Com.HedgeMark.Commons;

namespace HMOSecureMiddleware.Util
{
    public static class Utility
    {

        //public static List<dmaHoliday> Holidays = new List<dmaHoliday>();
        //public static List<DateTime> HolidayDates
        //{
        //    get { return Holidays.Select(s => s.HolidayContextDate.Date).ToList(); }
        //}
        public static string Environment = ConfigurationManagerWrapper.StringSetting("Environment");
        public static readonly TextInfo TextInfo = new CultureInfo("en-US", false).TextInfo;

        public static bool IsLocal()
        {
            return Environment.Equals("Local");
        }

        public static bool IsWorkingDay(DateTime contextDate)
        {
            return contextDate.DayOfWeek != DayOfWeek.Saturday && contextDate.DayOfWeek != DayOfWeek.Sunday;
        }

        public static bool IsWeekEndDay(DateTime contextDate)
        {
            return contextDate.DayOfWeek == DayOfWeek.Saturday || contextDate.DayOfWeek == DayOfWeek.Sunday;
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

        public static DateTime GetCalenderDate(this DateTime contextDate)
        {
            contextDate = contextDate.AddDays(1);
            while (!IsWorkingDay(contextDate))
            {
                contextDate = contextDate.AddDays(1);
            }
            return contextDate.Date;
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
    }
}
