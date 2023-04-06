using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Com.HedgeMark.Commons.Mail;
using Cronos;
using Hangfire;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware.Models;
using HM.Operations.Secure.Middleware.Util;

namespace HM.Operations.Secure.Middleware.Jobs
{
    public class ScheduleManager
    {
        public static Dictionary<string, TimeZoneInfo> TimeZones = new Dictionary<string, TimeZoneInfo>()
        {
            {"EST", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")},
            {"IST", TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")},
            {"CET", TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")},
            {"PST", TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time")},
        };

        protected static void TryGetStartAndEndDate(DashboardScheduleRange dashboardScheduleRange, out DateTime startDate, out DateTime endDate)
        {
            var valueDate = DateTime.Today;
            endDate = valueDate;

            switch (dashboardScheduleRange)
            {
                case DashboardScheduleRange.TodayOnly:
                    startDate = valueDate;
                    break;
                case DashboardScheduleRange.TodayAndFuture:
                    startDate = valueDate;
                    endDate = valueDate.AddDays(10);
                    break;
                case DashboardScheduleRange.TodayAndYesterday:
                    startDate = valueDate.GetContextDate();
                    break;
                case DashboardScheduleRange.Last7Days:
                    startDate = valueDate.AddDays(-7);
                    break;
                case DashboardScheduleRange.Last30Days:
                    startDate = valueDate.AddDays(-30);
                    break;
                case DashboardScheduleRange.ThisMonth:
                    startDate = valueDate.GetStartOfMonth();
                    break;
                case DashboardScheduleRange.ThisYear:
                    startDate = valueDate.GetStartOfYear();
                    break;
                case DashboardScheduleRange.Last3Months:
                    startDate = valueDate.AddMonths(-3);
                    break;
                default:
                    startDate = valueDate;
                    break;
            }
        }

        public static void Initialise()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var dashboardSchedules = context.hmsDashboardSchedules.Include(s => s.hmsSchedule).Include(s => s.hmsDashboardTemplate).Include(s => s.hmsDashboardTemplate.hmsDashboardPreferences).Where(s => !s.IsDeleted && s.hmsSchedule.IsActive).Select(s => s).ToList();

                foreach (var job in dashboardSchedules)
                    DashboardScheduleHandler.ScheduleDashboard(job);
            }
        }

        public static List<string> GetExternalDomains()
        {
            using (var context = new AdminContext())
            {
                return context.vw_EmailDomailForFunds.Select(s => s.Domain).Distinct().ToList();
            }
        }

        public static void SetNextExecutionTime(List<JobSchedule> schedules, bool isDashboard)
        {
            if (schedules.Count == 0)
                return;

            foreach (var job in schedules)
            {
                job.NextRunAt = GetNextExecutionTime(job.Schedule.ScheduleExpression, job.Schedule.TimeZone, DateTime.UtcNow);
            }
        }

        public static DateTime GetNextExecutionTime(string cronExpression, string timeZone, DateTime utcNow)
        {
            var expression = CronExpression.Parse(cronExpression);
            var nextUtcOccurence = expression.GetNextOccurrence(utcNow, TimeZones[timeZone], true);
            return nextUtcOccurence ?? new DateTime();
        }


        public static void AddSchedule(long jobId, bool isDashboard)
        {
            //if (isDashboard)
            DashboardScheduleHandler.ScheduleDashboard(jobId);
        }

        public static void RemoveSchedule(long jobId, bool isDashboard)
        {
            var scheduleName = GetScheduleName(jobId, isDashboard);
            RecurringJob.RemoveIfExists(scheduleName);
        }

        protected static string GetScheduleName(long jobId, bool isDashboard)
        {
            var scheduleName = isDashboard
                ? $"Dashboard-Schedule-{jobId}"
                : $"Report-Schedule-{jobId}";
            return scheduleName;
        }

        public static void SetScheduleActiveOrInactive(long jobId, long scheduleId, bool isActive, bool isDashboard, int userId)
        {
            if (jobId == 0 || scheduleId == 0)
                return;

            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                var schedule = context.hmsSchedules.First(s => s.hmsScheduleId == scheduleId);
                schedule.IsActive = isActive;
                schedule.LastModifiedBy = userId;
                schedule.LastUpdatedAt = DateTime.Now;
                context.SaveChanges();
            }

            if (!isActive)
                RemoveSchedule(jobId, isDashboard);
            else
                AddSchedule(jobId, isDashboard);
        }

        protected static void SendOutReport(FileInfo exportFileInfo, hmsDashboardSchedule schedule, string subject, string mailBody, bool isNoFilesReceived)
        {
            var job = schedule.hmsSchedule;
            var preferences = schedule.hmsDashboardTemplate.hmsDashboardPreferences.ToDictionary(s => (DashboardReport.PreferenceCode)s.PreferenceCode, v => v.Preferences);

            var clientIds = new List<long>();
            if (preferences.ContainsKey(DashboardReport.PreferenceCode.Clients))
                clientIds = Array.ConvertAll(preferences[DashboardReport.PreferenceCode.Clients].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries), long.Parse).ToList();
            var adminIds = new List<long>();
            if (preferences.ContainsKey(DashboardReport.PreferenceCode.Admins))
                adminIds = Array.ConvertAll(preferences[DashboardReport.PreferenceCode.Admins].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries), long.Parse).ToList();
            var isExternalMailAllowed = true;
            if (clientIds.Any(s => s == -1) || clientIds.Count > 1)
                isExternalMailAllowed = false;

            var allToEmails = job.To;
            if (!string.IsNullOrWhiteSpace(job.ExternalToApproved) && isExternalMailAllowed)
                allToEmails = $"{allToEmails},{job.ExternalToApproved}";

            var newMail = new MailInfo(subject, mailBody, allToEmails, isNoFilesReceived ? null : exportFileInfo, ccAddress: job.CC, mailSignature: Utility.MailSignatureOps);
            var mailSentResult = MailIdQualifier.SendMailToQualifiedIds(newMail);

            if (!isNoFilesReceived)
            {
                //Send this to internal directory if configured
                if (!string.IsNullOrWhiteSpace(job.InternalFolder))
                    ReportDeliveryManager.SendReportToInternalDir(job.InternalFolder, exportFileInfo);

                //Send this to external directory if configured
                if (!string.IsNullOrWhiteSpace(job.SFTPFolder) && job.SFTPFolderWorkflowCode == (int)ScheduleWorkflowCode.Approved)
                    ReportDeliveryManager.SftpThisReport(job.SFTPFolder, exportFileInfo);
            }

            if (File.Exists(exportFileInfo.FullName))
                File.Delete(exportFileInfo.FullName);
        }
    }
}
