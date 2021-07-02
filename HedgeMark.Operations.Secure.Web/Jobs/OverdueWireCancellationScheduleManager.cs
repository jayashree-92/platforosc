using System.ComponentModel;
using HedgeMark.Operations.Secure.DataModel;
using log4net;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using HedgeMark.Operations.Secure.Middleware;

namespace HMOSecureWeb.Jobs
{
    public class OverdueWireAutoCancellationJobManager : OperationsSecureSystemSchedule
    {
        public new const string JobName = "OverdueWireCancellation-Requester";

        private static readonly ILog Logger = LogManager.GetLogger(typeof(OverdueWireAutoCancellationJobManager));

        [DisplayName("Wire cancellation schedule for > {0}")]
        public static void CancelThisWire(long wireId, hmsWireAutoCancellationJob wireSchedule)
        {
            var wireStatus = WireDataManager.GetWireStatus(wireId);
            if (wireStatus.WireStatus != WireDataManager.WireStatus.Initiated)
                return;

            var wireTicket = WireDataManager.GetWireData(wireId);
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
                    BackgroundJob.Delete(schedule.JobId);

                var jobId = BackgroundJob.Schedule(() => CancelThisWire(schedule.hmsWireId, schedule), new DateTimeOffset(schedule.ScheduledDate));
                schedule.JobId = jobId;
                AddOrUpdateWireCancellationJob(schedule);
            }
        }

        public static hmsWireAutoCancellationJob GetWireCancellationJob(long wireId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsWireAutoCancellationJobs.FirstOrDefault(s => s.hmsWireId == wireId);
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