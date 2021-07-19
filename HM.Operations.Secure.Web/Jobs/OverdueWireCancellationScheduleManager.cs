using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;
using Hangfire;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware;
using log4net;

namespace HM.Operations.Secure.Web.Jobs
{
    public class OverdueWireAutoCancellationJobManager : OperationsSecureSystemSchedule
    {
        public new const string JobName = "OverdueWireCancellation-Requester";

        private static readonly ILog Logger = LogManager.GetLogger(typeof(OverdueWireAutoCancellationJobManager));

        [DisplayName("Wire cancellation schedule for > {0}")]
        public static void CancelThisWire(long hmsWireJobSchedulerId)
        {
            var wireSchedule = GetWireCancellationJobByScheduleId(hmsWireJobSchedulerId);

            if (wireSchedule == null || wireSchedule.IsJobExecuted)
                return;

            var wireStatus = WireDataManager.GetWireStatus(wireSchedule.hmsWireId);
            if (wireStatus.WireStatus != WireDataManager.WireStatus.Initiated)
                return;

            var wireTicket = WireDataManager.GetWireData(wireSchedule.hmsWireId);
            var workflowComment = "Auto Rejected by system as deadline crossed.";
            WireDataManager.SaveWireData(wireTicket, WireDataManager.WireStatus.Cancelled, workflowComment, -1);
            wireSchedule.IsJobExecuted = true;
            AddOrUpdateWireCancellationJob(wireSchedule);
        }

        public static void ScheduleOverdueWiresCancellation()
        {
            var joblessWireSchedules = GetAllUnexecutedCancellationJobs();
            foreach (var schedule in joblessWireSchedules)
            {
                if (!string.IsNullOrWhiteSpace(schedule.JobId))
                    return;

                var jobId = BackgroundJob.Schedule(() => CancelThisWire(schedule.hmsWireJobSchedulerId), new DateTimeOffset(schedule.ScheduledDate));
                schedule.JobId = jobId;
                AddOrUpdateWireCancellationJob(schedule);
            }
        }

        public static hmsWireAutoCancellationJob GetWireCancellationJobByWireId(long wireId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsWireAutoCancellationJobs.FirstOrDefault(s => s.hmsWireId == wireId);
            }
        }

        public static hmsWireAutoCancellationJob GetWireCancellationJobByScheduleId(long wireCancellationJobId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsWireAutoCancellationJobs.FirstOrDefault(s => s.hmsWireJobSchedulerId == wireCancellationJobId);
            }
        }

        public static void AddOrUpdateWireCancellationJob(hmsWireAutoCancellationJob schedule)
        {
            schedule.LastModifiedAt = DateTime.Now;
            using (var context = new OperationsSecureContext())
            {
                context.hmsWireAutoCancellationJobs.AddOrUpdate(schedule);
                context.SaveChanges();
            }
        }
        public static void RemoveWireCancellationJob(long scheduleId)
        {
            var wireSchedule = GetWireCancellationJobByScheduleId(scheduleId);

            if (wireSchedule == null)
                return;

            using (var context = new OperationsSecureContext())
            {
                context.hmsWireAutoCancellationJobs.Remove(wireSchedule);
                context.SaveChanges();
            }
        }

        public static List<hmsWireAutoCancellationJob> GetAllUnexecutedCancellationJobs()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                return context.hmsWireAutoCancellationJobs.Where(s => !s.IsJobExecuted).ToList();
            }
        }
    }
}