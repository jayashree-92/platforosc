using System;
using System.Collections.Generic;
using System.Linq;
using HedgeMark.Operations.Secure.DataModel;
using System.Data.Entity.Migrations;
using HMOSecureMiddleware.Models;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.SwiftMessageHandler;

namespace HMOSecureMiddleware
{
    public class WireDataManager
    {
        public enum WireStatus
        {
            Drafted = 1,
            Initiated,
            Approved,
            Cancelled,
            Failed,
        }

        public enum SwiftStatus
        {
            NotInitiated = 1,
            Processing,
            Acknowledged,
            NegativeAcknowledged,
            Completed,
            Failed,
        }

        public class WireTicketStatus
        {
            public WireStatus WireStatus { get; set; }
            public SwiftStatus SwiftStatus { get; set; }
        }

        public static WireTicket GetWireData(long wireId)
        {
            hmsWire hmWire;

            using (var context = new OperationsSecureContext())
            {
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                hmWire = context.hmsWires.Include("hmsWireMessageType")
                                         .Include("hmsWireDocuments")
                                         .Include("hmsWireWorkflowLogs")
                                         .Include("hmsWireStatusLkup")
                                         .Include("hmsWirePurposeLkup")
                                         .Include("hmsWireLogs").FirstOrDefault(s => s.hmsWireId == wireId);
            }


            if (hmWire == null)
                return null;

            hmWire.hmsWireLogs.ForEach(s =>
            {
                s.hmsWire = null;
                s.hmsWireStatusLkup = null;
                s.hmsWireMessageType = null;
            });

            hmWire.hmsWireStatusLkup.hmsWires = null;
            hmWire.hmsWireStatusLkup.hmsWireWorkflowLogs = null;
            hmWire.hmsWireStatusLkup.hmsWireLogs = null;
            hmWire.hmsWirePurposeLkup.hmsWires = null;
            hmWire.hmsWireMessageType.hmsWires = null;
            hmWire.hmsWireMessageType.hmsWireLogs = null;
            hmWire.hmsWireDocuments.ForEach(s => s.hmsWire = null);
            hmWire.hmsWireWorkflowLogs.ForEach(s =>
            {
                s.hmsWire = null;
                s.hmsWireStatusLkup = null;
            });
            if (hmWire.hmsSwiftStatusLkup != null)
            {
                hmWire.hmsSwiftStatusLkup.hmsWires = null;
                hmWire.hmsSwiftStatusLkup.hmsWireWorkflowLogs = null;
            }
            hmWire.hmsWireWorkflowLogs = hmWire.hmsWireWorkflowLogs.OrderByDescending(s => s.CreatedAt).ToList();
            dmaAgreementOnBoarding wireAgreement;
            onBoardingAccount wireSendingAccount;
            onBoardingAccount wireReceivingAccount;
            onBoardingSSITemplate wireSSITemplate;
            List<string> workflowUsers;
            List<string> attachmentUsers;

            using (var context = new AdminContext())
            {
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                wireAgreement = context.dmaAgreementOnBoardings.Include("onboardingFund")
                                                                .Include("dmaCounterPartyOnBoarding")
                                                                .FirstOrDefault(s => hmWire.OnBoardAgreementId == s.dmaAgreementOnBoardingId) ?? new dmaAgreementOnBoarding();

                var userIds = hmWire.hmsWireWorkflowLogs.Select(s => s.CreatedBy).ToList();
                userIds.AddRange(hmWire.hmsWireDocuments.Select(s => s.CreatedBy).ToList());
                var users = context.hLoginRegistrations.Where(s => userIds.Contains(s.intLoginID)).ToDictionary(s => s.intLoginID, v => v.varLoginID.HumanizeEmail());
                users.Add(-1, "System");

                workflowUsers = hmWire.hmsWireWorkflowLogs.Select(s => users[s.CreatedBy]).ToList();
                attachmentUsers = hmWire.hmsWireDocuments.Select(s => users[s.CreatedBy]).ToList();

                wireSendingAccount = context.onBoardingAccounts.FirstOrDefault(s => hmWire.OnBoardAccountId == s.onBoardingAccountId) ?? new onBoardingAccount();
                wireReceivingAccount = context.onBoardingAccounts.FirstOrDefault(s => hmWire.OnBoardSSITemplateId == s.onBoardingAccountId) ?? new onBoardingAccount();
                wireSSITemplate = context.onBoardingSSITemplates.FirstOrDefault(s => hmWire.OnBoardSSITemplateId == s.onBoardingSSITemplateId) ?? new onBoardingSSITemplate();
            }

            wireAgreement.dmaAgreementDocuments = null;
            wireAgreement.dmaAgreementSettlementInstructions = null;
            wireAgreement.dmaAgreementOnBoardingChecklists = null;
            wireAgreement.onBoardingAccounts = null;
            if (wireAgreement.onboardingFund != null)
            {
                wireAgreement.onboardingFund.dmaAgreementOnBoardings = null;
                wireAgreement.onboardingFund.onBoardingAccounts = null;
            }
            if (wireAgreement.dmaCounterPartyOnBoarding != null)
                wireAgreement.dmaCounterPartyOnBoarding.dmaAgreementOnBoardings = null;

            return new WireTicket()
            {
                HMWire = hmWire,
                Agreement = wireAgreement,
                Account = wireSendingAccount,
                ReceivingAccount = hmWire.IsBookTransfer ? wireReceivingAccount : new onBoardingAccount(),
                SSITemplate = wireSSITemplate,
                AttachmentUsers = attachmentUsers,
                WorkflowUsers = workflowUsers,
                SwiftMessages = GetFormattedSwiftMessages(hmWire.hmsWireLogs.ToList(), hmWire.WireStatusId, hmWire.SwiftStatusId)
            };
        }

        private static Dictionary<string, string> GetFormattedSwiftMessages(List<hmsWireLog> wireLogs, int wireStatusId, int swiftStatusId)
        {
            var swiftMessages = new Dictionary<string, string>();

            if (wireLogs.Count == 0)
                return swiftMessages;

            var shouldIncludeMsgType = wireLogs.Count > 1;

            //var wireTransactionLog = wireLogs.Count <= 1 ? wireLogs.FirstOrDefault() : wireLogs.LastOrDefault(s => s.WireStatusId == wireStatusId);

            foreach (var wireTransactionLog in wireLogs)
            {

                if (wireTransactionLog == null)
                    return swiftMessages;

                swiftMessages.Add("Outbound" + (shouldIncludeMsgType ? wireTransactionLog.hmsWireMessageType.MessageType : string.Empty), SwiftMessageInterpreter.GetDetailedFormatted(wireTransactionLog.OutBoundSwiftMessage, true));

                var ackLabel = string.Format("{0}Acknowledgement" + (shouldIncludeMsgType ? wireTransactionLog.hmsWireMessageType.MessageType : string.Empty), (swiftStatusId == (int)SwiftStatus.NegativeAcknowledged ? "N-" : string.Empty));
                swiftMessages.Add(ackLabel, SwiftMessageInterpreter.GetDetailedFormatted(wireTransactionLog.ServiceSwiftMessage, true));

                swiftMessages.Add("Confirmation" + (shouldIncludeMsgType ? wireTransactionLog.hmsWireMessageType.MessageType : string.Empty), SwiftMessageInterpreter.GetDetailedFormatted(wireTransactionLog.InBoundSwiftMessage, true));

            }

            return swiftMessages;
        }


        public static WireTicketStatus GetWireStatus(long wireId)
        {
            using (var context = new OperationsSecureContext())
            {
                var wireTicket = context.hmsWires.First(s => s.hmsWireId == wireId);
                return new WireTicketStatus()
                {
                    WireStatus = (WireStatus)wireTicket.WireStatusId,
                    SwiftStatus = (SwiftStatus)wireTicket.SwiftStatusId
                };
            }
        }

        public static void SetWireStatus(long wireId, SwiftStatus swiftStatus, string comment)
        {
            using (var context = new OperationsSecureContext())
            {
                //Update Wire Status Id
                var hmsWire = context.hmsWires.First(s => s.hmsWireId == wireId);
                hmsWire.SwiftStatusId = (int)swiftStatus;
                context.SaveChanges();

                //Add a Workflow Status 
                SaveWireWorflow(wireId, (WireStatus)hmsWire.WireStatusId, swiftStatus, comment, -1);
            }
        }

        public static void SetWireStatus(WireInBoundMessage inBoundMessage, WireStatus wireStatus, SwiftStatus swiftStatus, string comment)
        {
            using (var context = new OperationsSecureContext())
            {
                //Update Wire Status Id
                var hmsWire = context.hmsWires.First(s => s.hmsWireId == inBoundMessage.WireId);
                hmsWire.WireStatusId = (int)wireStatus;
                hmsWire.SwiftStatusId = (int)swiftStatus;
                context.SaveChanges();

                //Add a Workflow Status 
                SaveWireWorflow(inBoundMessage.WireId, wireStatus, swiftStatus, comment, -1);
            }
        }

        public static void SetWireStatus(long wireId, WireStatus wireStatus, SwiftStatus swiftStatus, string comment)
        {
            using (var context = new OperationsSecureContext())
            {
                //Update Wire Status Id
                var hmsWire = context.hmsWires.First(s => s.hmsWireId == wireId);
                hmsWire.WireStatusId = (int)wireStatus;
                hmsWire.SwiftStatusId = (int)swiftStatus;
                context.SaveChanges();

                //Add a Workflow Status 
                SaveWireWorflow(wireId, wireStatus, swiftStatus, comment, -1);
            }
        }

        public static WireTicket SaveWireData(WireTicket wireTicket, WireStatus wireStatus, string comment, int userId)
        {
            using (var context = new OperationsSecureContext())
            {
                var priorWireStatus = GetWireStatus(wireTicket.HMWire.hmsWireId);

                wireTicket.HMWire.WireStatusId = (int)wireStatus;
                wireTicket.HMWire.SwiftStatusId = (int)SwiftStatus.NotInitiated;
                context.hmsWires.AddOrUpdate(wireTicket.HMWire);
                context.hmsWireDocuments.AddRange(wireTicket.HMWire.hmsWireDocuments.Where(s => s.hmsWireDocumentId == 0));
                context.SaveChanges();

                SaveWireWorflow(wireTicket.HMWire.hmsWireId, wireStatus, (SwiftStatus)wireTicket.HMWire.SwiftStatusId, comment, userId);
                var currentWireStatus = GetWireStatus(wireTicket.HMWire.hmsWireId);
                wireTicket = GetWireData(wireTicket.HMWire.hmsWireId);

                if (currentWireStatus.WireStatus == WireStatus.Approved)
                    WireTransactionManager.InititateWireTransfer(wireTicket);

                if (priorWireStatus.WireStatus == WireStatus.Approved && currentWireStatus.WireStatus == WireStatus.Cancelled)
                    WireTransactionManager.CancelWireTransfer(wireTicket);

                return wireTicket;
            }
        }

        public static void SaveWireWorflow(long wireId, WireStatus wireStatus, SwiftStatus swiftStatus, string comment, int userId)
        {
            using (var context = new OperationsSecureContext())
            {
                var wireWorkFlowLog = new hmsWireWorkflowLog
                {
                    hmsWireId = wireId,
                    WireStatusId = (int)wireStatus,
                    SwiftStatusId = (int)swiftStatus,
                    Comment = comment,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                };
                context.hmsWireWorkflowLogs.AddOrUpdate(wireWorkFlowLog);
                context.SaveChanges();
            }
        }

        public static bool IsWireCreated(DateTime valueDate, string purpose, long sendingAccountId, long receivingAccountId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsWires.Any(s => s.ValueDate == valueDate && s.OnBoardAccountId == sendingAccountId && s.OnBoardSSITemplateId == receivingAccountId && s.hmsWirePurposeLkup.Purpose == purpose);
            }
        }

        public static void RemoveWireDocument(long documentId)
        {
            using (var context = new OperationsSecureContext())
            {
                var document = context.hmsWireDocuments.First(x => x.hmsWireDocumentId == documentId);
                context.hmsWireDocuments.Remove(document);
                context.SaveChanges();
            }
        }

        public static void RemoveWireDocument(long wireId, string fileName)
        {
            using (var context = new OperationsSecureContext())
            {
                var document = context.hmsWireDocuments.FirstOrDefault(x => x.hmsWireId == wireId && x.FileName == fileName);
                if (document == null)
                    return;

                context.hmsWireDocuments.Remove(document);
                context.SaveChanges();
            }
        }

        public static hmsWireJobSchedule GetJobSchedule(long wireId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsWireJobSchedules.FirstOrDefault(s => s.hmsWireId == wireId);

            }
        }

        public static void EditJobSchedule(hmsWireJobSchedule schedule)
        {
            using (var context = new OperationsSecureContext())
            {
                context.hmsWireJobSchedules.AddOrUpdate(schedule);
                context.SaveChanges();
            }
        }

        public static void RemoveJobSchedule(hmsWireJobSchedule schedule)
        {
            using (var context = new OperationsSecureContext())
            {
                var scheduleToDelete = context.hmsWireJobSchedules.FirstOrDefault(s => s.hmsWireJobSchedulerId == schedule.hmsWireJobSchedulerId);
                context.hmsWireJobSchedules.Remove(scheduleToDelete);
                context.SaveChanges();
            }
        }

        public static List<hmsWireJobSchedule> GetSchedulesWithoutJobsData()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsWireJobSchedules.Where(s => !s.IsJobCreated).ToList();
            }
        }

        public static List<hmsInBoundMQLog> GetInboundMQLogs(DateTime startDate, DateTime endDate)
        {
            using (var context = new OperationsSecureContext())
            {
                return context.hmsInBoundMQLogs.Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate).ToList();
            }
        }
    }
}
