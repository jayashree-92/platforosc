using System.ComponentModel;
using HedgeMark.Operations.Secure.DataModel;
using HMOSecureMiddleware;
using log4net;
using Hangfire;
using System;

namespace HMOSecureWeb.Jobs
{
    public class OverdueWireCancellationScheduleManager : OperationsSecureSystemSchedule
    {
        public new const string JobName = "OverdueWireCancellation-Requester";

        private static readonly ILog Logger = LogManager.GetLogger(typeof(OverdueWireCancellationScheduleManager));

        public static string GetJobName(long wireId)
        {
            return string.Format("Wire cancellation schedule for > {0}", wireId);
        }

        [DisplayName("Wire cancellation schedule for > {0}")]
        public static void CancelThisWire(long wireId, hmsWireJobSchedule wireSchedule)
        {
            var wireStatus = WireDataManager.GetWireStatus(wireId);
            if (wireStatus.WorkflowStatus != WireDataManager.WorkflowStatus.Initiated)
                return;

            var wireTicket = WireDataManager.GetWireData(wireId);
            var workflowComment = "Auto Cancelled by System as deadline crossed.";
            WireDataManager.SaveWireData(wireTicket, WireDataManager.WorkflowStatus.Cancelled, workflowComment, -1);
            wireSchedule.IsJobCreated = true;
            WireDataManager.EditJobSchedule(wireSchedule);
        }

        public static void ScheduleOverdueWiresCancellation()
        {

            var joblessWireSchedules = WireDataManager.GetSchedulesWithoutJobsData();
            foreach (var schedule in joblessWireSchedules)
            {
                var scheduleName = OverdueWireCancellationScheduleManager.GetJobName(schedule.hmsWireId);
                schedule.IsJobCreated = true;
                WireDataManager.EditJobSchedule(schedule);
                var jobId = BackgroundJob.Schedule(() => OverdueWireCancellationScheduleManager.CancelThisWire(schedule.hmsWireId, schedule), new DateTimeOffset(schedule.ScheduledDate));
                if(schedule.IsJobInActive)
                  BackgroundJob.Delete(jobId);
            }
        }
    }
}