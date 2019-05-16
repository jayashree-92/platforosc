using System;
using System.Collections.Generic;
using System.Linq;
using HedgeMark.Operations.Secure.DataModel;
using System.Data.Entity.Migrations;
using HMOSecureMiddleware.Models;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.SwiftMessageHandler;
using HedgeMark.SwiftMessageHandler.Model;
using log4net;

namespace HMOSecureMiddleware
{
    public class WireDataManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(WireDataManager));
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

        public enum TransferType
        {
            NormalTransfer = 1,
            BookTransfer,
            FeeOrExpensesPayment
        }

        public class WireTicketStatus
        {
            public WireStatus WireStatus { get; set; }
            public SwiftStatus SwiftStatus { get; set; }
        }

        public static string GetWireTransactionId(long wireId)
        {
            var wireIdStr = wireId.ToString();
            wireIdStr = wireIdStr.Length < 6 ? wireIdStr.PadLeft(6, '0') : wireIdStr;
            var environmentStr = Utility.Environment.ToUpper() == "PROD" ? string.Empty : Utility.Environment.ToUpper()[0].ToString();
            return string.Format("{0}DMO{1}", environmentStr, wireIdStr);
        }


        public static WireTicket GetWireData(long wireId)
        {
            hmsWire hmWire;

            using (var context = new OperationsSecureContext())
            {
                //context.Database.Log = s =>
                //{
                //    Logger.Debug(s);
                //};

                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                hmWire = context.hmsWires.Include("hmsWireMessageType")
                                         //.Include("hmsWireDocuments")
                                         //.Include("hmsWireWorkflowLogs")
                                         .Include("hmsWireStatusLkup")
                                         .Include("hmsWirePurposeLkup")
                                         .Include("hmsWireTransferTypeLKup")
                                         //.Include("hmsWireLogs")
                                         .First(s => s.hmsWireId == wireId);

                hmWire.hmsWireDocuments = context.hmsWireDocuments.Where(s => s.hmsWireId == wireId).ToList();
                hmWire.hmsWireWorkflowLogs = context.hmsWireWorkflowLogs.Where(s => s.hmsWireId == wireId).ToList();
                hmWire.hmsWireLogs = context.hmsWireLogs.Where(s => s.hmsWireId == wireId).ToList();
            }

            //if (hmWire == null)
            //    return null;

            hmWire.hmsWireLogs.ForEach(s =>
            {
                s.hmsWire = null;
                s.hmsWireMessageType = null;
                s.hmsWireWorkflowLog = null;
                s.hmsWireLogTypeLkup = null;
            });

            hmWire.hmsWireStatusLkup.hmsWires = null;
            hmWire.hmsWireStatusLkup.hmsWireWorkflowLogs = null;
            hmWire.hmsWirePurposeLkup.hmsWires = null;
            hmWire.hmsWireMessageType.hmsWires = null;
            hmWire.hmsWireTransferTypeLKup.hmsWires = null;
            hmWire.hmsWireMessageType.hmsWireLogs = null;

            hmWire.hmsWireDocuments.ForEach(s => s.hmsWire = null);
            hmWire.hmsWireWorkflowLogs.ForEach(s =>
            {
                s.hmsWire = null;
                s.hmsWireStatusLkup = null;
                s.hmsWireLogs = null;
            });
            if (hmWire.hmsSwiftStatusLkup != null)
            {
                hmWire.hmsSwiftStatusLkup.hmsWires = null;
                hmWire.hmsSwiftStatusLkup.hmsWireWorkflowLogs = null;
            }
            hmWire.hmsWireWorkflowLogs = hmWire.hmsWireWorkflowLogs.OrderByDescending(s => s.CreatedAt).ToList();
            //dmaAgreementOnBoarding wireAgreement;
            onBoardingAccount wireSendingAccount;
            onBoardingAccount wireReceivingAccount;
            onBoardingSSITemplate wireSSITemplate;
            dmaCounterPartyOnBoarding counterparty;
            List<string> workflowUsers;
            List<string> attachmentUsers;

            using (var context = new AdminContext())
            {
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                //wireAgreement = context.dmaAgreementOnBoardings.Include("onboardingFund")
                //                                                .Include("dmaCounterPartyOnBoarding")
                //                                                .FirstOrDefault(s => hmWire.OnBoardAgreementId == s.dmaAgreementOnBoardingId) ?? new dmaAgreementOnBoarding();

                var userIds = hmWire.hmsWireWorkflowLogs.Select(s => s.CreatedBy).ToList();
                userIds.AddRange(hmWire.hmsWireDocuments.Select(s => s.CreatedBy).ToList());
                var users = context.hLoginRegistrations.Where(s => userIds.Contains(s.intLoginID)).ToDictionary(s => s.intLoginID, v => v.varLoginID.HumanizeEmail());
                users.Add(-1, "System");

                workflowUsers = hmWire.hmsWireWorkflowLogs.Select(s => users.ContainsKey(s.CreatedBy) ? users[s.CreatedBy] : "Unknown User").ToList();
                attachmentUsers = hmWire.hmsWireDocuments.Select(s => users.ContainsKey(s.CreatedBy) ? users[s.CreatedBy] : "Unknown User").ToList();

                wireSendingAccount = context.onBoardingAccounts.FirstOrDefault(s => hmWire.OnBoardAccountId == s.onBoardingAccountId) ?? new onBoardingAccount();
                wireReceivingAccount = context.onBoardingAccounts.FirstOrDefault(s => hmWire.OnBoardSSITemplateId == s.onBoardingAccountId) ?? new onBoardingAccount();
                wireSSITemplate = context.onBoardingSSITemplates.FirstOrDefault(s => hmWire.OnBoardSSITemplateId == s.onBoardingSSITemplateId) ?? new onBoardingSSITemplate();
                counterparty = context.dmaCounterPartyOnBoardings.FirstOrDefault(s => wireSSITemplate.TemplateEntityId == s.dmaCounterPartyOnBoardId);
            }

            //wireAgreement.dmaAgreementDocuments = null;
            //wireAgreement.dmaAgreementSettlementInstructions = null;
            //wireAgreement.dmaAgreementOnBoardingChecklists = null;
            //wireAgreement.onBoardingAccounts = null;
            //if (wireAgreement.onboardingFund != null)
            //{
            //    wireAgreement.onboardingFund.dmaAgreementOnBoardings = null;
            //    wireAgreement.onboardingFund.onBoardingAccounts = null;
            //}
            //if (wireAgreement.dmaCounterPartyOnBoarding != null)
            //    wireAgreement.dmaCounterPartyOnBoarding.dmaAgreementOnBoardings = null;

            return new WireTicket()
            {
                HMWire = hmWire,
                //Agreement = wireAgreement,
                SendingAccount = wireSendingAccount,
                ReceivingAccount = hmWire.WireTransferTypeId == 2 ? wireReceivingAccount : new onBoardingAccount(),
                SSITemplate = wireSSITemplate,
                AttachmentUsers = attachmentUsers,
                WorkflowUsers = workflowUsers,
                Counterparty = (counterparty ?? new dmaCounterPartyOnBoarding()).CounterpartyName,
                SwiftMessages = GetFormattedSwiftMessages(hmWire.hmsWireId),
            };
        }

        private static List<KeyValuePair<string, string>> GetFormattedSwiftMessages(long wireId)
        {
            var swiftMessages = new List<KeyValuePair<string, string>>();

            List<hmsWireLog> wireLogs;

            using (var context = new OperationsSecureContext())
            {
                wireLogs = context.hmsWireLogs.Include("hmsWireMessageType").Include("hmsWireLogTypeLkup").Where(s => s.hmsWireId == wireId).OrderBy(s => s.hmsWireLogId).ToList();
            }

            if (wireLogs.Count == 0)
                return swiftMessages;

            var isMultiMessage = wireLogs.Select(s => s.WireMessageTypeId).Distinct().Count() > 1;

            var lastMessageStatus = 0;
            var lastKey = string.Empty;
            var lastMessageType = string.Empty;
            foreach (var log in wireLogs)
            {
                lastMessageType = log.hmsWireMessageType.MessageType;

                lastKey = !isMultiMessage
                    ? log.hmsWireLogTypeLkup.LogType
                    : string.Format("{0}-{1}", log.hmsWireLogTypeLkup.LogType.Replace("Acknowledged", "Ack"), lastMessageType);

                lastMessageStatus = log.hmsWireLogTypeId;

                swiftMessages.Add(new KeyValuePair<string, string>(lastKey, SwiftMessageInterpreter.GetDetailedFormatted(log.SwiftMessage, true)));
            }

            //Outbound
            if (lastMessageStatus == 1)
            {
                swiftMessages.Add(new KeyValuePair<string, string>(lastKey.Replace("Outbound", isMultiMessage ? "Ack" : "Acknowledged"), string.Empty));
                swiftMessages.Add(new KeyValuePair<string, string>(lastKey.Replace("Outbound", "Confirmation"), string.Empty));
            }


            //Acknowledgment
            else if (lastMessageStatus == 2)
            {
                swiftMessages.Add(new KeyValuePair<string, string>(lastKey.Replace(isMultiMessage ? "Ack" : "Acknowledged", "Confirmation"), string.Empty));
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


        public static hmsWireWorkflowLog SetWireStatusAndWorkFlow(long wireId, WireStatus wireStatus, WireDataManager.SwiftStatus swiftStatus, string comment, int userId)
        {
            var hmsWire = GetWire(wireId);
            return SetWireStatusAndWorkFlow(hmsWire, wireStatus, swiftStatus, comment, userId);
        }

        public static hmsWireWorkflowLog SetWireStatusAndWorkFlow(hmsWire wire, WireStatus wireStatus, SwiftStatus swiftStatus, string comment, int userId)
        {
            using (var context = new OperationsSecureContext())
            {
                wire.WireStatusId = (int)wireStatus;
                wire.SwiftStatusId = (int)swiftStatus;

                if (userId != -1)
                {
                    wire.LastUpdatedBy = userId;
                    wire.LastModifiedAt = DateTime.Now;
                }

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

                return wireWorkFlowLog;

            }

        }


        public static WireTicket SaveWireData(WireTicket wireTicket, WireStatus wireStatus, string comment, int userId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.hmsWireDocuments.AddRange(wireTicket.HMWire.hmsWireDocuments.Where(s => s.hmsWireDocumentId == 0));
                context.SaveChanges();
            }

            var existingWireTicket = GetWireData(wireTicket.HMWire.hmsWireId);

            if (wireStatus == WireStatus.Approved)
                WireTransactionManager.ApproveAndInitiateWireTransfer(existingWireTicket, comment, userId);

            else if (existingWireTicket.HMWire.WireStatusId == (int)WireStatus.Approved && wireStatus == WireStatus.Cancelled)
                WireTransactionManager.CancelWireTransfer(existingWireTicket, comment, userId);

            else
            {
                wireTicket.HMWire.CreatedAt = existingWireTicket.HMWire.CreatedAt;
                SetWireStatusAndWorkFlow(wireTicket.HMWire, wireStatus, SwiftStatus.NotInitiated, comment, userId);
            }

            return existingWireTicket;
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

        public static List<hmsMQLog> GetMQLogs(DateTime startDate, DateTime endDate)
        {
            endDate = endDate.Date == DateTime.Now.Date ? DateTime.Now : endDate;
            using (var context = new OperationsSecureContext())
            {
                return context.hmsMQLogs.Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate).ToList();
            }
        }
    }
}
