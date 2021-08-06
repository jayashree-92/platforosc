﻿using System;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using ExcelUtility.Operations.ManagedAccounts;
using Hangfire;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware.Models;
using Humanizer;

namespace HM.Operations.Secure.Middleware.Jobs
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
            RecurringJob.AddOrUpdate(scheduleName, () => ExecuteDashboardSchedule(job.hmsDashboardScheduleId, job.hmsDashboardTemplate.TemplateName),
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

            TryGetStartAndEndDate((DashboardScheduleRange)job.DashboardScheduleRangeLkupId, out var startDate, out var endDate);

            var reportFileName = string.IsNullOrWhiteSpace(job.hmsSchedule.ReportFileName) ? templateName : job.hmsSchedule.ReportFileName;
            var fileName = startDate == endDate ? $"{reportFileName}-{startDate:yyyyMMdd}" : $"{reportFileName}-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}";
            var exportFileInfo = new FileInfo($"{FileSystemManager.UploadTemporaryFilesPath}{fileName}{job.hmsSchedule.FileFormat}");

            //Mail this report
            var subject = $"Wires Dashboard Report of '{templateName}' for {((DashboardScheduleRange)job.DashboardScheduleRangeLkupId).ToString()}";

            var mailBody = $"Hi, <br/><br/> Please find the attached the wires dasboard report of '{templateName}' for {((DashboardScheduleRange)job.DashboardScheduleRangeLkupId).Humanize()}.<br/><br/> Thanks, <br/> HM-Operations Team.";

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
