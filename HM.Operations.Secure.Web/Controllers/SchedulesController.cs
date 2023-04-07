using Com.HedgeMark.Commons;
using Hangfire;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware;
using HM.Operations.Secure.Middleware.Jobs;
using HM.Operations.Secure.Middleware.Models;
using HM.Operations.Secure.Middleware.Util;
using HM.Operations.Secure.Web.Jobs;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Mvc;

namespace HM.Operations.Secure.Web.Controllers
{
    public class SchedulesController : WireUserBaseController
    {
        public JsonResult GetScheduleDefaults()
        {
            var counter = 1;
            var dashboardRange = Enum.GetNames(typeof(DashboardScheduleRange)).Select(s => new { id = counter++, text = s.Humanize() }).ToArray();
            counter = 1;
            var reportContextRun = Enum.GetNames(typeof(ReportScheduleContextRun)).Select(s => new { id = counter++, text = s.Humanize() }).ToArray();
            counter = 0;
            var fundPreferences = Enum.GetNames(typeof(PreferencesManager.FundNameInDropDown)).Select(s => new { id = counter++, text = s.Humanize() }).ToArray();

            var sftpFolders = ExternalSftpTransfer.ExternalClients.Select(s => new Select2Type { id = s, text = s }).ToList();
            var internalFolders = FileSystemManager.FetchFolderData(FileSystemManager.InternalOutputFilesDropPath, false);
            var timeZones = ScheduleManager.TimeZones.Keys.ToArray();
            var externalDomains = ScheduleManager.GetExternalDomains();
            return Json(new { dashboardRange, reportContextRun, fundPrefernces = fundPreferences, sftpFolders, internalFolders, timeZones, externalDomains, UserId });
        }

        public JsonResult GetSchedules(long primaryId, bool isDashboard)
        {
            var jobSchedules = GetDashboardSchedules(primaryId);
            var isExternalToApprovedHasValue = isDashboard && jobSchedules.Any(s => !string.IsNullOrEmpty(s.Schedule.ExternalToApproved));

            //Set user details
            SetUserDetails(jobSchedules);

            //Set next execution
            ScheduleManager.SetNextExecutionTime(jobSchedules, isDashboard);

            return Json(new { jobSchedules, isExternalToApprovedHasValue });
        }

        public JsonResult GetScheduleLogs(long scheduleId, string timeZone, int totalItems = 10)
        {
            List<hmsScheduleLog> logs;
            using(var context = new OperationsSecureContext())
            {
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                logs = context.hmsScheduleLogs.Where(s => s.hmsScheduleId == scheduleId && s.ContextDate <= DateTime.Today).Select(s => s).OrderByDescending(s => s.RecCreatedAt).Take(totalItems).ToList();
            }

            var timeZoneInfo = ScheduleManager.TimeZones.ContainsKey(timeZone) ? ScheduleManager.TimeZones[timeZone] : Middleware.Util.Utility.DefaultSystemTimeZone;

            foreach(var s in logs)
            {
                if(s.IsManualTrigger)
                    s.ExpectedScheduleStartAt = TimeZoneInfo.ConvertTime(s.ExpectedScheduleStartAt, timeZoneInfo);

                s.ScheduleStartTime = TimeZoneInfo.ConvertTime(s.ScheduleStartTime, timeZoneInfo);

                if(s.ScheduleEndTime != null)
                    s.ScheduleEndTime = TimeZoneInfo.ConvertTime((s.ScheduleEndTime ?? new DateTime()), timeZoneInfo);

            }

            return Json(logs.Select(s => new { s.ContextDate, s.ExpectedScheduleStartAt, s.ExpectedScheduleEndAt, s.ScheduleStartTime, s.ScheduleEndTime, s.IsManualTrigger }));
        }

        public void SaveSchedule(JobSchedule job, long primaryId, bool isDashboard)
        {
            SaveDashboardSchedule(job, primaryId, out var jobId);

            //Schedule this job
            ScheduleManager.AddSchedule(jobId, isDashboard);
        }

        public void TriggerNow(long jobId, DateTime? contextDate, bool isDashboard)
        {
            if(contextDate == null) contextDate = DateTime.Today.GetContextDate();

            var job = DashboardScheduleHandler.GetDashboardSchedule(jobId);
            BackgroundJob.Enqueue(() => DashboardScheduleHandler.ExecuteDashboardSchedule(job.hmsDashboardScheduleId, job.hmsDashboardTemplate.TemplateName, true));
        }

        public void DeleteSchedule(long jobId, bool isDashboard)
        {
            using(var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                var schedule = context.hmsDashboardSchedules.First(s => s.hmsDashboardScheduleId == jobId);
                schedule.IsDeleted = true;
                schedule.LastModifiedBy = UserId;
                schedule.LastUpdatedAt = DateTime.Now;
                context.SaveChanges();
            }

            ScheduleManager.RemoveSchedule(jobId, isDashboard);
        }

        public void SetScheduleStatus(long jobId, long scheduleId, bool isActive, bool isDashboard)
        {
            ScheduleManager.SetScheduleActiveOrInactive(jobId, scheduleId, isActive, isDashboard, UserId);
        }

        public JsonResult SetWorkflowCodeForExternalTo(long scheduleId, int workflowCode, bool isDashboard)
        {
            hmsSchedule schedule;
            using(var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                schedule = context.hmsSchedules.First(s => s.hmsScheduleId == scheduleId);

                //If Approved
                if(workflowCode == 1)
                    schedule.ExternalToApproved = schedule.ExternalTo;

                schedule.ExternalToWorkflowCode = workflowCode;
                schedule.ExternalToModifiedBy = UserId;
                schedule.ExternalToModifiedAt = DateTime.Now;
                context.SaveChanges();
            }
            var toUserId = schedule.CreatedBy;
            //if (isDashboard)
            //    NotifyDashboardSchedule(workflowCode, toUserId, scheduleId, UserDetails);

            return Json(new { Schedule = schedule, ExternalToModifiedBy = UserName.HumanizeEmail() });
        }

        //private void NotifyDashboardSchedule(int workflowCode, int toUserId, long scheduleId, UserAccountDetails userDetails)
        //{

        //    var message = "";
        //    int nthQuarterlyMonth;

        //    using (var context = new OperationsSecureContext())
        //    {

        //        var list = (from sch in context.hmsSchedules
        //                    join dSch in context.hmsDashboardSchedules on sch.hmsScheduleId equals dSch.hmsScheduleId
        //                    join tSch in context.hmsDashboardTemplates on dSch.hmsDashboardTemplateId equals tSch.hmsDashboardTemplateId
        //                    where sch.hmsScheduleId == scheduleId
        //                    select new { tSch.TemplateName, sch.TimeZone, sch.ScheduleExpression, sch.Frequency }).ToList();

        //        var cronDescription = CronHelper.GetDue(list[0].ScheduleExpression, (CronHelper.ScheduleFrequency)Enum.Parse(typeof(CronHelper.ScheduleFrequency), list[0].Frequency), out nthQuarterlyMonth);

        //        var schTime = cronDescription.CronDescription.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)[0].ToString().ToLower();

        //        message = string.Format("<b> Wires Dashboard- {0}</b> scheduled {1} {2} {3} is ", list[0].TemplateName, schTime, list[0].TimeZone, list[0].Frequency);
        //    }

        //    if (workflowCode == 1)
        //    {
        //        message += "<span style=\"color:#8aed58;\" >" + "approved " + "</span>" + "by " + "<b>" + userDetails.Name.HumanizeEmail() + "</b>";
        //        NotificationHub.Notify(toUserId, userDetails, "Approval status", message);
        //    }
        //    else if (workflowCode == 2)
        //    {
        //        message += "<span style=\"color:#fc0008;\" >" + "rejected " + "</span>" + "by " + "<b>" + userDetails.Name.HumanizeEmail() + "</b>";
        //        NotificationHub.Notify(toUserId, userDetails, "Approval status", message);
        //    }

        //}

        public JsonResult SetWorkflowCodeForSFTPFolder(long scheduleId, int workflowCode, bool isDashboard)
        {
            hmsSchedule schedule;
            using(var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                schedule = context.hmsSchedules.First(s => s.hmsScheduleId == scheduleId);
                schedule.SFTPFolderWorkflowCode = workflowCode;
                schedule.SFTPFolderModifiedBy = UserId;
                schedule.SFTPFolderModifiedAt = DateTime.Now;
                context.SaveChanges();
            }

            //  var toUserId = schedule.CreatedBy;

            //if (isDashboard)
            //    NotifyDashboardSchedule(workflowCode, toUserId, scheduleId, UserDetails);

            return Json(new { Schedule = schedule, SFTPFolderModifiedBy = UserName.HumanizeEmail() });
        }

        private static void SetUserDetails(List<JobSchedule> jobSchedules)
        {
            //Substitute UserNames 
            var allUserIds = jobSchedules.Select(s => s.Schedule.CreatedBy)
                .Union(jobSchedules.Select(s => s.Schedule.LastModifiedBy))
                .Union(jobSchedules.Select(s => s.Schedule.ExternalToModifiedBy ?? 0))
                .Union(jobSchedules.Select(s => s.Schedule.SFTPFolderModifiedBy ?? 0)).Distinct().ToList();

            var userIdMap = FileSystemManager.GetUsersList(allUserIds);

            foreach(var job in jobSchedules)
            {
                job.CreatedBy = userIdMap.ContainsKey(job.Schedule.CreatedBy) ? userIdMap[job.Schedule.CreatedBy] : "un-known user";
                job.LastModifiedBy = userIdMap.ContainsKey(job.Schedule.LastModifiedBy) ? userIdMap[job.Schedule.LastModifiedBy] : "un-known user";
                job.ExternalToModifiedBy = userIdMap.ContainsKey(job.Schedule.ExternalToModifiedBy ?? 0) ? userIdMap[job.Schedule.ExternalToModifiedBy ?? 0] : "-";
                job.SFTPFolderModifiedBy = userIdMap.ContainsKey(job.Schedule.SFTPFolderModifiedBy ?? 0) ? userIdMap[job.Schedule.SFTPFolderModifiedBy ?? 0] : "-";
            }
        }


        private JobSchedule ConstructJobData(hmsSchedule schedule)
        {
            var due = CronHelper.GetDue(schedule.ScheduleExpression, (CronHelper.ScheduleFrequency)Enum.Parse(typeof(CronHelper.ScheduleFrequency), schedule.Frequency), out var nthQuarterlyMonth);

            schedule.hmsDashboardSchedules = null;

            var job = new JobSchedule()
            {
                Schedule = schedule,
                Due = due
            };

            if(schedule.ExternalToWorkflowCode == 0 && schedule.LastModifiedBy > 0)
                job.IsExternalToRequestCreatedBySameUser = schedule.LastModifiedBy == UserId;

            if(schedule.SFTPFolderWorkflowCode == 0 && schedule.LastModifiedBy > 0)
                job.IsSFTPFolderRequestCreatedBySameUser = schedule.LastModifiedBy == UserId;

            if(job.Due.DueDayOfMonth == 0)
                job.Due.DueDayOfMonth = 1;

            return job;
        }

        private hmsSchedule ConstructSchedule(JobSchedule job)
        {
            var hmsSchedule = job.Schedule;

            if(string.IsNullOrWhiteSpace(hmsSchedule.To))
                hmsSchedule.To = string.Empty;

            if(hmsSchedule.CreatedBy == 0)
            {
                hmsSchedule.CreatedBy = UserId;
                hmsSchedule.CreatedAt = DateTime.Now;
            }

            hmsSchedule.LastModifiedBy = UserId;
            hmsSchedule.LastUpdatedAt = DateTime.Now;


            if(!string.IsNullOrWhiteSpace(hmsSchedule.ExternalTo) && hmsSchedule.ExternalToWorkflowCode == 0)
            {
                hmsSchedule.ExternalToModifiedBy = UserId;
                hmsSchedule.ExternalToModifiedAt = DateTime.Now;
            }

            if(!string.IsNullOrWhiteSpace(hmsSchedule.SFTPFolder) && hmsSchedule.SFTPFolderWorkflowCode == 0)
            {
                hmsSchedule.SFTPFolderModifiedBy = UserId;
                hmsSchedule.SFTPFolderModifiedAt = DateTime.Now;
            }

            if(job.Schedule.Frequency == "Monthly" && !job.Due.IsMonthlyNthDaySelected)
                job.Due.ShouldGetNthBusinessDayOfMonth = true;

            hmsSchedule.ScheduleExpression = CronHelper.GetCronExpression(job.Due, (CronHelper.ScheduleFrequency)Enum.Parse(typeof(CronHelper.ScheduleFrequency), job.Schedule.Frequency, true));

            return hmsSchedule;
        }

        #region Dashboard Schedules

        private List<JobSchedule> GetDashboardSchedules(long dashboardTemplateId)
        {
            List<JobSchedule> jobSchedules;
            using(var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                var schedules = context.hmsDashboardSchedules.Include(s => s.hmsSchedule).Where(s => s.hmsDashboardTemplateId == dashboardTemplateId && !s.IsDeleted).ToList();
                jobSchedules = schedules.Select(ConstructJobData).ToList();
            }
            return jobSchedules;
        }


        private hmsDashboardSchedule ConstructDashboardSchedule(JobSchedule job, long dashboardTemplateId)
        {
            var jobSchedule = new hmsDashboardSchedule
            {
                LastModifiedBy = UserId,
                LastUpdatedAt = DateTime.Now,
                hmsDashboardTemplateId = dashboardTemplateId,
                hmsDashboardScheduleId = job.Id,
                DashboardScheduleRangeLkupId = (int)job.ScheduleRangeLkupId,
                hmsSchedule = ConstructSchedule(job),
                hmsScheduleId = job.Schedule.hmsScheduleId
            };

            return jobSchedule;
        }

        private void SaveDashboardSchedule(JobSchedule job, long dashboardTemplateId, out long jobId)
        {
            // reconstruct data 
            var schedule = ConstructDashboardSchedule(job, dashboardTemplateId);

            using(var context = new OperationsSecureContext())
            {
                context.hmsDashboardSchedules.AddOrUpdate(schedule);
                context.hmsSchedules.AddOrUpdate(schedule.hmsSchedule);
                context.SaveChanges();
            }

            jobId = schedule.hmsDashboardScheduleId;
        }


        public JobSchedule ConstructJobData(hmsDashboardSchedule schedule)
        {
            var job = ConstructJobData(schedule.hmsSchedule);
            job.Id = schedule.hmsDashboardScheduleId;
            job.ScheduleRangeLkupId = (DashboardScheduleRange)schedule.DashboardScheduleRangeLkupId;

            return job;
        }


        #endregion

    }
}