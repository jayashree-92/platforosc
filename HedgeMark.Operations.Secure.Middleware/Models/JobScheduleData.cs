using System;
using CronExpressionDescriptor;
using HedgeMark.Operations.Secure.DataModel;

namespace HedgeMark.Operations.Secure.Middleware.Models
{
    public enum DashboardScheduleRange
    {
        TodayOnly = 1, TodayAndFuture, TodayAndYesterday, Last7Days, Last30Days, ThisMonth, ThisYear, Last3Months
    }

    public enum ReportScheduleContextRun
    {
        ContextDate = 1, ContextDateMinusOne, ContextDateMinusTwo, ContextDateMinusThree, RecentMonthEndContextDate
    }

    public enum ScheduleWorkflowCode
    {
        NoAction = 0, Approved = 1, Rejected = 2
    }
    public class DueDate
    {
        public string CronExpression { get; set; }
        public string CronDescription
        {
            get
            {
                return !string.IsNullOrEmpty(CronExpression) ? ExpressionDescriptor.GetDescription(CronExpression) : "Invalid Expression";
            }
        }
        public string Frequency { get; set; }
        public TimeSpan DueTime { get; set; }
        public string DueTimeStamp { get { return string.Format("{0:00}:{1:00}:{2:00}", DueTime.Hours, DueTime.Minutes, DueTime.Seconds); } }
        public int DueDayOfWeek { get; set; }
        public int[] DueDaysOfWeek { get; set; }
        public int DueDayOfMonth { get; set; }
        public bool IsMonthlyNthDaySelected { get; set; }
        public string MonthlyNthDay { get; set; }
        public string MonthlyNthDayOfWeek { get; set; }
        public bool ShouldGetNthBusinessDayOfMonth { get; set; }
    }
    public class JobSchedule
    {
        public int Id { get; set; }
        public hmsSchedule Schedule { get; set; }
        public DueDate Due { get; set; }
        public bool ShouldSendOnlyWhenAllReceived { get; set; }
        public ReportScheduleContextRun ScheduleContextRunkupId { get; set; }
        public DashboardScheduleRange ScheduleRangeLkupId { get; set; }
        public DateTime NextRunAt { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public string ExternalToModifiedBy { get; set; }
        public string SFTPFolderModifiedBy { get; set; }
        public string DeadlineTimeString { get; set; }
        public bool IsExternalToRequestCreatedBySameUser { get; set; }
        public bool IsSFTPFolderRequestCreatedBySameUser { get; set; }
    }
}