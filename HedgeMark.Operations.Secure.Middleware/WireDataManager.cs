using System;
using System.Collections.Generic;
using System.Linq;
using HedgeMark.Operations.Secure.DataModel;
using System.Data.Entity.Migrations;
using HMOSecureMiddleware.Models;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.SwiftMessageHandler;
using HedgeMark.SwiftMessageHandler.Model;

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

                workflowUsers = hmWire.hmsWireWorkflowLogs.Select(s => users.ContainsKey(s.CreatedBy) ? users[s.CreatedBy] : "Unknown User").ToList();
                attachmentUsers = hmWire.hmsWireDocuments.Select(s => users.ContainsKey(s.CreatedBy) ? users[s.CreatedBy] : "Unknown User").ToList();

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
                SwiftMessages = GetFormattedSwiftMessages(hmWire.hmsWireId)
            };
        }

        private static Dictionary<string, string> GetFormattedSwiftMessages(long wireId)
        {
            var swiftMessages = new Dictionary<string, string>();

            List<hmsWireLog> wireLogs;

            using (var context = new OperationsSecureContext())
            {
                wireLogs = context.hmsWireLogs.Include("hmsWireMessageType").Where(s => s.hmsWireId == wireId).OrderBy(s => s.hmsWireLogId).ToList();
            }

            if (wireLogs.Count == 0)
                return swiftMessages;

            var isMultiMessage = wireLogs.Select(s => s.WireMessageTypeId).Distinct().ToList().Count > 1;

            //var wireTransactionLog = wireLogs.Count <= 1 ? wireLogs.FirstOrDefault() : wireLogs.LastOrDefault(s => s.WireStatusId == wireStatusId);
            var lastMessageTypeId = wireLogs.Select(s => s.WireMessageTypeId).Last();
            foreach (var wireTransactionLog in wireLogs.OrderBy(s => s.hmsWireLogId).ToList())
            {
                var messageType = isMultiMessage ? string.Format("-{0}", wireTransactionLog.hmsWireMessageType.MessageType) : string.Empty;

                //Outbound
                var outbountStr = "Outbound" + messageType;
                if (!string.IsNullOrWhiteSpace(wireTransactionLog.OutBoundSwiftMessage) && !swiftMessages.ContainsKey(outbountStr))
                    swiftMessages.Add(outbountStr, SwiftMessageInterpreter.GetDetailedFormatted(wireTransactionLog.OutBoundSwiftMessage, true));

                //Ack or Nack
                if (!string.IsNullOrWhiteSpace(wireTransactionLog.ServiceSwiftMessage))
                {
                    var swiftMsg = SwiftMessage.Parse(wireTransactionLog.ServiceSwiftMessage);
                    var ackLabel = string.Format("{0}{2}{1}", swiftMsg.IsNack() ? "N-" : string.Empty, messageType, isMultiMessage ? "Ack" : "Acknowledgement");
                    if (!string.IsNullOrWhiteSpace(wireTransactionLog.ServiceSwiftMessage) && !swiftMessages.ContainsKey(ackLabel))
                        swiftMessages.Add(ackLabel, SwiftMessageInterpreter.GetDetailedFormatted(wireTransactionLog.ServiceSwiftMessage, true));
                }

                //InBound
                var confirmationLabel = "Confirmation" + messageType;
                if (!swiftMessages.ContainsKey(confirmationLabel) && ((lastMessageTypeId == wireTransactionLog.WireMessageTypeId) || !string.IsNullOrWhiteSpace(wireTransactionLog.InBoundSwiftMessage)))
                    swiftMessages.Add(confirmationLabel, SwiftMessageInterpreter.GetDetailedFormatted(wireTransactionLog.InBoundSwiftMessage, true));
            }

            return swiftMessages;
        }

        private static hmsWire GetWire(long wireId)
        {
            using (var context = new OperationsSecureContext())
            {
                var wireTicket = context.hmsWires.First(s => s.hmsWireId == wireId);
                return wireTicket;
            }
        }

        public static WireTicketStatus GetWireStatus(hmsWire wireTicket)
        {
            return new WireTicketStatus()
            {
                WireStatus = (WireStatus)wireTicket.WireStatusId,
                SwiftStatus = (SwiftStatus)wireTicket.SwiftStatusId
            };
        }

        public static WireTicketStatus GetWireStatus(long wireId)
        {
            var wireTicket = GetWire(wireId);
            return GetWireStatus(wireTicket);
        }

        public static void SetWireStatusAndWorkFlow(long wireId, SwiftStatus swiftStatus, string comment)
        {
            var hmsWire = GetWire(wireId);
            SetWireStatusAndWorkFlow(hmsWire, (WireStatus)hmsWire.WireStatusId, swiftStatus, comment, -1);
        }

        public static void SetWireStatusAndWorkFlow(long wireId, WireStatus wireStatus, WireDataManager.SwiftStatus swiftStatus, string comment, int userId)
        {
            var hmsWire = GetWire(wireId);
            SetWireStatusAndWorkFlow(hmsWire, wireStatus, swiftStatus, comment, userId);
        }

        public static void SetWireStatusAndWorkFlow(hmsWire wire, WireDataManager.WireStatus wireStatus, WireDataManager.SwiftStatus swiftStatus, string comment, int userId)
        {
            using (var context = new OperationsSecureContext())
            {
                wire.WireStatusId = (int)wireStatus;
                wire.SwiftStatusId = (int)swiftStatus;
                context.hmsWires.AddOrUpdate(wire);
                context.SaveChanges();

                var wireWorkFlowLog = new hmsWireWorkflowLog
                {
                    hmsWireId = wire.hmsWireId,
                    WireStatusId = (int)wireStatus,
                    SwiftStatusId = (int)swiftStatus,
                    Comment = comment ?? string.Empty,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                };
                context.hmsWireWorkflowLogs.AddOrUpdate(wireWorkFlowLog);
                context.SaveChanges();
            }
        }


        public static WireTicket SaveWireData(WireTicket wireTicket, WireStatus wireStatus, string comment, int userId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.hmsWireDocuments.AddRange(wireTicket.HMWire.hmsWireDocuments.Where(s => s.hmsWireDocumentId == 0));
                context.SaveChanges();
            }

            wireTicket = GetWireData(wireTicket.HMWire.hmsWireId);

            if (wireStatus == WireStatus.Approved)
                WireTransactionManager.ApproveAndInititateWireTransfer(wireTicket, comment, userId);

            else if (wireTicket.HMWire.WireStatusId == (int)WireStatus.Approved && wireStatus == WireStatus.Cancelled)
                WireTransactionManager.CancelWireTransfer(wireTicket, comment, userId);

            else
                SetWireStatusAndWorkFlow(wireTicket.HMWire, wireStatus, SwiftStatus.NotInitiated, comment, userId);

            return wireTicket;
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
            endDate = endDate.Date == DateTime.Now.Date ? DateTime.Now : endDate;
            using (var context = new OperationsSecureContext())
            {
                return context.hmsInBoundMQLogs.Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate).ToList();
            }
        }
    }
}
