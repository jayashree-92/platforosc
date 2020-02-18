using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Com.HedgeMark.Commons;
using Hangfire;
using Hangfire.Annotations;
using HMOSecureMiddleware.Queues;
using log4net;

namespace HMOSecureWeb.Jobs
{
    public abstract class OperationsSecureSystemSchedule
    {
        [NotNull]
        public static readonly string JobName;
    }

    public class CronHelper
    {

        public CronHelper()
        {
            Interval = 1;
        }

        public int Interval { get; set; }

        public CronHelper Every(int interval = 1)
        {
            Interval = interval;
            return this;
        }

        public string Minute()
        {
            return string.Format("*/{0} * * * *", Interval);
        }

        public string Hour()
        {
            return string.Format("0 */{0} * * *", Interval);
        }

        public string Day(TimeSpan at)
        {
            return string.Format("{0} {1} */{2} * *", at.Minutes, at.Hours, Interval);
        }

    }

    public class BackGroundJobScheduler : IRegisteredObject
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BackGroundJobScheduler));
        private static readonly BackGroundJobScheduler BackgroundJobScheduler = new BackGroundJobScheduler();

        public static bool KeepBackGroundAlive
        {
            get { return ConfigurationManagerWrapper.BooleanSetting("ShouldKeepBackGroundJobsAlive", true); }
        }

        private static void KeepAlive()
        {
            HostingEnvironment.QueueBackgroundWorkItem(clt =>
            {
                Thread.Sleep(1000 * 60 * 60 * 24);
                KeepAlive();
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void StartAll()
        {
            log.Debug("Starting all background jobs schedulers...");
            OperationsSecureRecurringJob.Initialise();
            BackgroundJobScheduler.Start();
        }

        public void Stop(bool immediate)
        {
            log.DebugFormat("Unregistering from host and stoping all background job schedulers with immediate flag: {0}", immediate);
            HostingEnvironment.UnregisterObject(this);
        }

        private void Start()
        {
            HostingEnvironment.RegisterObject(this);

            if (KeepBackGroundAlive)
            {
                Task.Factory.StartNew(KeepAlive);
            }
        }
    }

    public class OperationsSecureRecurringJob
    {

        public static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        private static readonly string InboundMessagesReceiptJobName = "InboundMessagesReceipt-Requester";
        private static readonly string InboundAckMessagesReceiptJobName = "InboundAckMessagesReceipt-Requester";
        public static void Initialise()
        {
            ScheduleOverdueWireCancellation(false);
            ScheduleReceiptOfInboundMessages(false);
            ScheduleReceiptOfInboundAckMessages(false);
            ScheduleAutoApprovalOfNoticeWires(false);
        }

        public static void ScheduleOverdueWireCancellation(bool isDisabled)
        {
            if (isDisabled)
            {
                RecurringJob.RemoveIfExists(OverdueWireCancellationScheduleManager.JobName);
                return;
            }

            //This can be minutly for now 
            RecurringJob.AddOrUpdate(OverdueWireCancellationScheduleManager.JobName, () => OverdueWireCancellationScheduleManager.ScheduleOverdueWiresCancellation(), Cron.Minutely);
        }

        public static void ScheduleAutoApprovalofNoticeWires(bool isDisabled)
        {
            if (isDisabled)
            {
                RecurringJob.RemoveIfExists(NoticeWiresApprovalManager.JobName);
                return;
            }

            //This can be minutly for now 
            RecurringJob.AddOrUpdate(NoticeWiresApprovalManager.JobName, () => NoticeWiresApprovalManager.ScheduleNoticeWiresApproval(), new CronHelper().Every().Minute);
        }

        public static void ScheduleReceiptOfInboundMessages(bool isDisabled)
        {
            if (isDisabled)
            {
                RecurringJob.RemoveIfExists(InboundMessagesReceiptJobName);
                return;
            }

            RecurringJob.AddOrUpdate(InboundMessagesReceiptJobName, () => QueueSystemManager.GetAndProcessMessage(), new CronHelper().Every().Minute);

            ////We need to find a way to add Listener through XMS in a way where scheduling is not required 
            //var timer = new Timer { Interval = 30 * 1000 };
            //timer.Elapsed += (sender, args) =>
            //{
            //    BackgroundJob.Enqueue(() => QueueSystemManager.GetAndProcessMessage());
            //};
            //timer.Start();

        }

        public static void ScheduleReceiptOfInboundAckMessages(bool isDisabled)
        {
            if (isDisabled)
            {
                RecurringJob.RemoveIfExists(InboundAckMessagesReceiptJobName);
                return;
            }
            RecurringJob.AddOrUpdate(InboundAckMessagesReceiptJobName, () => QueueSystemManager.GetAndProcessAcknowledgement(-1), new CronHelper().Every().Minute);

            ////We need to find a way to add Listener through XMS in a way where scheduling is not required 
            //var timer = new Timer { Interval = 30 * 1000 };
            //timer.Elapsed += (sender, args) =>
            //{
            //    BackgroundJob.Enqueue(() => QueueSystemManager.GetAndProcessAcknowledgement(-1));
            //};
            //timer.Start();
        }
    }
}
