using System;
using System.Collections.Generic;
using System.Linq;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.Operations.Secure.Middleware.Models;

namespace HMOSecureWeb.Jobs
{
    public class CronHelper
    {

        public CronHelper()
        {
            Interval = 1;
        }

        public int Interval { get; set; }

        public CronHelper Every(int interval = 1)
        {
            Interval = interval;
            return this;
        }

        public string Minute()
        {
            return string.Format("*/{0} * * * *", Interval);
        }

        public string Hour()
        {
            return string.Format("0 */{0} * * *", Interval);
        }

        public string Day(TimeSpan at)
        {
            return string.Format("{0} {1} */{2} * *", at.Minutes, at.Hours, Interval);
        }

        private static readonly Dictionary<char, string> DayNumberMap = new Dictionary<char, string>()
        {
            {'0',"sunday"},
            {'1',"monday"},
            {'2',"tuesday"},
            {'3',"wednesday"},
            {'4',"thursday"},
            {'5',"friday"},
            {'6',"saturday"}
        };

        private static readonly Dictionary<char, string> NumberToTextMap = new Dictionary<char, string>()
        {
            {'1',"first"},
            {'2',"second"},
            {'3',"third"},
            {'4',"fourth"},
            {'L',"last"},
            {'W',"weekday"},
        };


        public static string GetCronExpression(DueDate due, ScheduleFrequency frequency, int nthMonthInQuarter = 1)
        {

            //0 0 L* * | At 00:00 AM on the last day of the month
            //0 0 L-1 * * | At 00:00 AM the day before the last day of the month
            //0 0 3W * * | At 00:00 AM, on the 3rd weekday of every month
            //0 0 LW * * | At 00:00 AM, on the last weekday of the month
            //0 0 * * 2L | At 00:00 AM on the last tuesday of the month
            //0 0 * * 6#3 | At 00:00 AM on the third Saturday of the month
            //0 0 ? 1 MON#1 | At 00:00 AM on the first Monday of the January

            if (frequency == ScheduleFrequency.Daily)
                return string.Format("{0} {1} * * 1-5", due.DueTime.Minutes, due.DueTime.Hours);

            if (frequency == ScheduleFrequency.Weekly)
            {
                if (due.DueDaysOfWeek != null && due.DueDaysOfWeek.Length > 0)
                {
                    return string.Format("{0} {1} * * {2}", due.DueTime.Minutes, due.DueTime.Hours, string.Join(",", due.DueDaysOfWeek));
                }

                return string.Format("{0} {1} * * {2}", due.DueTime.Minutes, due.DueTime.Hours, due.DueDayOfWeek);
            }

            if (nthMonthInQuarter <= 0)
                nthMonthInQuarter = 1;
            else if (nthMonthInQuarter > 3)
                nthMonthInQuarter = 3;

            if (frequency == ScheduleFrequency.Monthly || frequency == ScheduleFrequency.Quarterly || frequency == ScheduleFrequency.HalfYearly || frequency == ScheduleFrequency.Yearly)
            {
                string monthOr3RdString;
                if (frequency == ScheduleFrequency.Quarterly)
                    monthOr3RdString = nthMonthInQuarter + "/3";
                else if (frequency == ScheduleFrequency.HalfYearly)
                    monthOr3RdString = nthMonthInQuarter + "/6";
                else if (frequency == ScheduleFrequency.Yearly)
                    monthOr3RdString = nthMonthInQuarter + "/1";
                else
                    monthOr3RdString = "*";

                var weekOr4ThString = due.ShouldGetNthBusinessDayOfMonth ? "1-5" : "*";

                return string.Format("{0} {1} {2}", due.DueTime.Minutes, due.DueTime.Hours,
                    !due.IsMonthlyNthDaySelected
                        ? string.Format("{0} {1} {2}", due.DueDayOfMonth < 0
                                ? string.Format("L-{0}{1}", (due.DueDayOfMonth * -1), due.ShouldGetNthBusinessDayOfMonth ? "W" : "")
                                : due.DueDayOfMonth + (due.ShouldGetNthBusinessDayOfMonth ? "W" : ""), monthOr3RdString, weekOr4ThString)

                        : due.MonthlyNthDayOfWeek == "weekday" ? string.Format("{0}W {1} 1-5", NumberToTextMap.First(s => s.Value == due.MonthlyNthDay).Key, monthOr3RdString)
                        : due.MonthlyNthDay == "last" ? string.Format("* {0} {1}L", monthOr3RdString, DayNumberMap.First(s => s.Value == due.MonthlyNthDayOfWeek).Key)
                        : string.Format("* {0} {1}#{2}", monthOr3RdString, DayNumberMap.First(s => s.Value == due.MonthlyNthDayOfWeek).Key, NumberToTextMap.First(s => s.Value == due.MonthlyNthDay).Key));
            }

            return string.Format("{0} {1} * {2} {3}", due.DueTime.Minutes, frequency == ScheduleFrequency.Quarterly ? nthMonthInQuarter + "/3" : "*", due.DueTime.Hours, due.ShouldGetNthBusinessDayOfMonth ? "1-5" : "*");
        }

        public static DueDate GetDue(string cronExpression, ScheduleFrequency frequency, out int nthQuarterlyMonth)
        {
            nthQuarterlyMonth = 1;
            //Set default based on frequency
            if (string.IsNullOrWhiteSpace(cronExpression))
            {
                switch (frequency)
                {
                    case ScheduleFrequency.Daily:
                        cronExpression = "0 0 * * 1-5";
                        break;
                    case ScheduleFrequency.Weekly:
                        cronExpression = "0 0 * * 1";
                        break;
                    case ScheduleFrequency.Monthly:
                        cronExpression = "0 0 1 * *";
                        break;
                    case ScheduleFrequency.Quarterly:
                        cronExpression = "0 0 * 1/3 *";
                        break;
                    case ScheduleFrequency.HalfYearly:
                        cronExpression = "0 0 * 1/6 *";
                        break;
                    case ScheduleFrequency.Yearly:
                        cronExpression = "0 0 * 1 *";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(frequency.ToString(), frequency, null);
                }
            }

            var charactes = cronExpression.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (charactes.Length < 5)
                throw new FormatException(string.Format("'{0}' is Invalid CRON Expression and system is unable to parse it", cronExpression));

            //Char[0] = minutes
            //Char[1] = hours
            //Char[2] = days
            //Char[3] = month
            //Char[4] = week
            //Char[5] = year or nth week

            var dueTime = new TimeSpan(charactes[1].ToInt(), charactes[0].ToInt(), 0);
            var due = new DueDate()
            {
                Frequency = frequency.ToString(),
                DueTime = dueTime,
                //IsMonthlyNthDaySelected = isMonthlyNthDaySelected,
                CronExpression = cronExpression
            };

            if (frequency == ScheduleFrequency.Daily)
                return due;

            if (frequency == ScheduleFrequency.Weekly)
            {
                if (!charactes[4].Contains(","))
                {
                    due.DueDayOfWeek = charactes[4].ToInt();
                    due.DueDaysOfWeek = new[] { charactes[4].ToInt() };
                }
                else
                {
                    due.DueDaysOfWeek = charactes[4].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => Extensions.ToInt(s)).ToArray();
                }

                return due;
            }
            if (frequency == ScheduleFrequency.Monthly || frequency == ScheduleFrequency.Quarterly || frequency == ScheduleFrequency.HalfYearly || frequency == ScheduleFrequency.Yearly)
            {
                var dayKey = charactes[2];
                var monthKey = charactes[3];
                var nthMonthKey = charactes[4];

                if (frequency == ScheduleFrequency.Quarterly)
                    nthQuarterlyMonth = monthKey.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries)[0].ToInt();

                //if (!due.IsMonthlyNthDaySelected)
                //{
                //    if (nthMonthKey == "1-5")
                //        due.ShouldGetNthBusinessDayOfMonth = true;

                //    due.DueDayOfMonth = monthKey.Replace("W", string.Empty).Replace("L", string.Empty).ToInt();
                //    return due;
                //}
                if (dayKey.Length == 2 && dayKey[1] == 'W')
                {
                    //Weekday Scenario - calendar day is not supported here - hence making ShouldGetNthBusinessDayOfMonth equals to true
                    if (NumberToTextMap.ContainsKey(dayKey[0]) && nthMonthKey == "1-5" || dayKey == "LW")
                    {
                        due.IsMonthlyNthDaySelected = true;
                        due.MonthlyNthDayOfWeek = "weekday";
                        due.MonthlyNthDay = NumberToTextMap[dayKey[0]];
                        due.ShouldGetNthBusinessDayOfMonth = true;
                    }
                    else
                    {
                        //due.DueDayOfMonth = charactes[2].ToInt();
                        due.DueDayOfMonth = dayKey.Replace("W", string.Empty).ToInt();

                        if (nthMonthKey == "1-5")
                            due.ShouldGetNthBusinessDayOfMonth = true;
                    }
                }

                //Weekday Scenario with nth Business Day 
                //Scenaio: 0 0 10W * *
                else if (dayKey.Length == 3 && dayKey[2] == 'W')
                {
                    due.DueDayOfMonth = dayKey.Replace("W", string.Empty).ToInt();

                    if (nthMonthKey == "1-5")
                        due.ShouldGetNthBusinessDayOfMonth = true;
                }

                //Scenario: 0 0 * * 2L | At 00:00 AM on the last tuesday of the month
                else if (nthMonthKey.Length == 2 && nthMonthKey[1] == 'L')
                {
                    due.IsMonthlyNthDaySelected = true;
                    due.MonthlyNthDayOfWeek = DayNumberMap[nthMonthKey[0]];
                    due.MonthlyNthDay = NumberToTextMap[nthMonthKey[1]];
                }
                //Scenario: 0 0 L-1 * * | At 00:00 AM the day before the last day of the month
                else if (dayKey.Length >= 3 && dayKey.Contains("L-"))
                {
                    if (nthMonthKey == "1-5")
                        due.ShouldGetNthBusinessDayOfMonth = true;

                    due.DueDayOfMonth = dayKey.Replace("W", string.Empty).Replace("L", string.Empty).ToInt();
                }
                else
                    due.DueDayOfMonth = charactes[2].ToInt();

                if (nthMonthKey.Length == 3 && nthMonthKey.Contains("#"))
                {
                    var nthMChars = nthMonthKey.Split(new[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
                    due.IsMonthlyNthDaySelected = true;
                    due.MonthlyNthDayOfWeek = DayNumberMap[nthMChars[0][0]];
                    due.MonthlyNthDay = NumberToTextMap[nthMChars[1][0]];
                }
                else if (nthMonthKey.Length == 3 && nthMonthKey == "1-5")
                {
                    due.ShouldGetNthBusinessDayOfMonth = true;
                }
                return due;
            }

            return due;
        }



        public enum ScheduleFrequency
        {
            Minutely, MinutelyOnWeekDays, Hourly, Daily, Weekly, Monthly, Quarterly, HalfYearly, Yearly,
        }

        public static string GetCronExpression(TimeSpan scheduleTime, int retryInterval = 0, int retryCount = 0, ScheduleFrequency frequency = ScheduleFrequency.Daily)
        {
            switch (frequency)
            {
                case ScheduleFrequency.Minutely:
                    if (scheduleTime.Minutes == 0)
                        throw new Exception("Minutes is zero");
                    return string.Format("*/{0} * * * *", scheduleTime.Minutes);
                case ScheduleFrequency.MinutelyOnWeekDays:
                    if (scheduleTime.Minutes == 0)
                        throw new Exception("Minutes is zero");
                    return string.Format("*/{0} * * * 1-5", scheduleTime.Minutes);
                case ScheduleFrequency.Hourly:
                    if (scheduleTime.Hours == 0)
                        throw new Exception("Hours is zero");
                    return string.Format("* */{0} * * *", scheduleTime.Hours);
                case ScheduleFrequency.Daily:
                default:
                    {
                        //retry count and retry interval are applicable only for daily jobs as of now
                        if (retryInterval == 0 || retryCount == 0)
                            return string.Format("{1} {0} * * 1-5", scheduleTime.Hours, scheduleTime.Minutes);

                        var retryTimespan = TimeSpan.FromHours((double)retryInterval * retryCount / 60);
                        if (retryTimespan.Hours < 1)
                        {
                            var minuteRange = scheduleTime.Minutes + retryTimespan.Minutes;
                            return string.Format("{2}-{4}/{3} {1} * * {0}", "1-5", scheduleTime.Hours, scheduleTime.Minutes, retryInterval, minuteRange > 59 ? 59 : minuteRange);
                        }

                        var hourRange = scheduleTime.Hours + retryTimespan.Hours;
                        return string.Format("*/{2} {1}-{3} * * {0}", "1-5", scheduleTime.Hours, retryInterval > 59 ? 59 : retryInterval, hourRange > 23 ? 23 : hourRange);
                    }
                case ScheduleFrequency.Weekly:
                    //Will be executed every friday
                    return string.Format("{1} {0} * * 5", scheduleTime.Hours, scheduleTime.Minutes);
                case ScheduleFrequency.Monthly:
                    //Will be executed every last working day of the month
                    return string.Format("{1} {0} LW * *", scheduleTime.Hours, scheduleTime.Minutes);
                case ScheduleFrequency.Quarterly:
                    return string.Format("{1} {0} LW */3 *", scheduleTime.Hours, scheduleTime.Minutes);
                case ScheduleFrequency.HalfYearly:
                    return string.Format("{1} {0} LW */6 *", scheduleTime.Hours, scheduleTime.Minutes);
                case ScheduleFrequency.Yearly:
                    return string.Format("{1} {0} LW 12 *", scheduleTime.Hours, scheduleTime.Minutes);
            }
        }
    }
}