using System.ComponentModel;
using HedgeMark.Operations.Secure.DataModel;
using HMOSecureMiddleware;
using log4net;
using Hangfire;
using System;

namespace HMOSecureWeb.Jobs
{ 
    public class NoticeWiresApprovalManager : OperationsSecureSystemSchedule
    {
        public new const string JobName = "NoticeWireApproval-Requester";

        private static readonly ILog Logger = LogManager.GetLogger(typeof(NoticeWiresApprovalManager));

        public static string GetJobName(long wireId)
        {
            return string.Format("Notice wire approval schedule for > {0}", wireId);
        }

        [DisplayName("Notice wire approval schedule for > {0}")]
        public static void ApproveNoticeWire(long wireId)
        {
            var wireStatus = WireDataManager.GetWireStatus(wireId);
            if (wireStatus.WireStatus != WireDataManager.WireStatus.Initiated)
                return;

            var wireTicket = WireDataManager.GetWireData(wireId);
            var workflowComment = "Auto approval of notice by system.";
            WireDataManager.SaveWireData(wireTicket, WireDataManager.WireStatus.Approved, workflowComment, -1);
        }

        public static void ScheduleNoticeWiresApproval()
        {
            var noticeWiresToApprove = WireDataManager.GetNoticeWiresAwaitingApproval();
            if (noticeWiresToApprove.Count == 0)
                return;

            noticeWiresToApprove.ForEach(wireId => BackgroundJob.Enqueue(() => NoticeWiresApprovalManager.ApproveNoticeWire(wireId)));
        }
    }
}