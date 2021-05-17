using System.ComponentModel;
using log4net;
using Hangfire;
using HedgeMark.Operations.Secure.Middleware;

namespace HMOSecureWeb.Jobs
{ 
    public class NoticeWiresApprovalManager : OperationsSecureSystemSchedule
    {
        public new const string JobName = "NoticeWireApproval-Requester";

        private static readonly ILog Logger = LogManager.GetLogger(typeof(NoticeWiresApprovalManager));

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