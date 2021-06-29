using System;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using ExcelUtility.Operations.ManagedAccounts;
using Hangfire;
using HedgeMark.Operations.Secure.DataModel;
using HedgeMark.Operations.Secure.Middleware.Models;
using Humanizer;

namespace HedgeMark.Operations.Secure.Middleware.Jobs
{
    public class DashboardScheduleHandler : ScheduleManager
    {
        internal static void ScheduleDashboard(long jobId)
        {
            //get schedule details
            var job = GetDashboardSchedule(jobId);
            if (job == null || job.IsDeleted || !job.hmsSchedule.IsActive)
                return;

            ScheduleDashboard(job);
        }

        internal static void ScheduleDashboard(hmsDashboardSchedule job)
        {
            var scheduleName = GetScheduleName(job.hmsDashboardScheduleId, true);

            //Schedule the given job
            RecurringJob.AddOrUpdate(scheduleName,
                () => ExecuteDashboardSchedule(job.hmsDashboardScheduleId, job.hmsDashboardTemplate.TemplateName),
                job.hmsSchedule.ScheduleExpression, TimeZones[job.hmsSchedule.TimeZone]);
        }

        public static hmsDashboardSchedule GetDashboardSchedule(long dashboarScheduleId)
        {
            using (var context = new OperationsSecureContext())
            {
                return context.hmsDashboardSchedules.Include(s => s.hmsSchedule)
                    .Include(s => s.hmsDashboardTemplate).Include(s => s.hmsDashboardTemplate.hmsDashboardPreferences)
                    .FirstOrDefault(s => s.hmsDashboardScheduleId == dashboarScheduleId && s.hmsSchedule.IsActive);
            }
        }

        [DisplayName("Dashboard Schedule of {0} > {1}")]
        public static void ExecuteDashboardSchedule(long dashboarScheduleId, string templateName)
        {
            //get schedule details
            var job = GetDashboardSchedule(dashboarScheduleId) ?? new hmsDashboardSchedule() { IsDeleted = true }; ;

            if (job.hmsDashboardTemplate.IsDeleted || !job.hmsSchedule.IsActive)
            {
                var schedulerProcessId = SystemSwitches.SchedulerProcessId;
                ScheduleManager.SetScheduleActiveOrInactive(job.hmsDashboardScheduleId, job.hmsScheduleId, false, true, schedulerProcessId);
                return;
            }

            DateTime startDate, endDate;
            TryGetStartAndEndDate((DashboardScheduleRange)job.DashboardScheduleRangeLkupId, out startDate, out endDate);

            var reportFileName = string.IsNullOrWhiteSpace(job.hmsSchedule.ReportFileName) ? templateName : job.hmsSchedule.ReportFileName;
            var fileName = startDate == endDate ? string.Format("{0}-{1:yyyyMMdd}", reportFileName, startDate) : string.Format("{0}-{1:yyyyMMdd}-{2:yyyyMMdd}", reportFileName, startDate, endDate);
            var exportFileInfo = new FileInfo(string.Format("{0}{1}{2}", FileSystemManager.UploadTemporaryFilesPath, fileName, job.hmsSchedule.FileFormat));

            //Mail this report
            var subject = string.Format("Wires Dashboard Report of '{0}' for {1}", templateName, ((DashboardScheduleRange)job.DashboardScheduleRangeLkupId).ToString());

            var mailBody = string.Format("Hi, <br/><br/> Please find the attached the wires dasboard report of '{0}' for {1}.<br/><br/> Thanks, <br/> HM-Operations Team.",
                templateName, ((DashboardScheduleRange)job.DashboardScheduleRangeLkupId).Humanize());

            var contentToExport = GetDashboardFileToSend(job, startDate, endDate);
            ReportDeliveryManager.CreateExportFile(contentToExport, exportFileInfo);

            SendOutReport(exportFileInfo, job.hmsSchedule, subject, mailBody, false);
        }

        private static ExportContent GetDashboardFileToSend(hmsDashboardSchedule schedule, DateTime startDate, DateTime endDate)
        {
            var preferences = schedule.hmsDashboardTemplate.hmsDashboardPreferences.ToDictionary(s => (DashboardReport.PreferenceCode)s.PreferenceCode, v => v.Preferences);
            var wireData = WireDashboardManager.GetWireTickets(startDate, endDate, true, preferences, false, schedule.hmsSchedule.TimeZone);
            var rows = WireDashboardManager.ConstructWireDataRows(wireData, true);
            return new ExportContent() { Rows = rows, TabName = "Wire Logs" };
        }
    }
}
