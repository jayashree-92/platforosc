﻿using System;
using System.Collections.Generic;
using System.Linq;
using HedgeMark.Operations.Secure.DataModel;
using System.Data.Entity.Migrations;
using HMOSecureMiddleware.Models;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.SwiftMessageHandler;
using HMOSecureMiddleware.Util;
using log4net;
using System.Data.Entity;

namespace HMOSecureMiddleware
{

    public class WireAccountBaseData
    {
        public long OnBoardAccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string FFCNumber { get; set; }
        public bool IsAuthorizedSendingAccount { get; set; }
        public string Currency { get; set; }
        public string AccountNameAndNumber
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FFCNumber))
                    return string.Format("{0}-{1}", AccountNumber, AccountName);

                return string.Format("{0}-{1}-{2}", FFCNumber, AccountNumber, AccountName);
            }
        }
    }

    public class FormattedSwiftMessage
    {
        public string Key { get; set; }
        public string OriginalFinMsg { get; set; }
        public string FormatedMsg { get; set; }
    }

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
            onBoardingAccount wireSendingAccount;
            onBoardingAccount wireReceivingAccount;
            onBoardingSSITemplate wireSSITemplate;


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
                                         .Include("hmsWireSenderInformation")
                                         .Include("hmsWireInvoiceAssociations")
                                         .Include("hmsWireCollateralAssociations")
                                         //.Include("hmsWireLogs")
                                         .First(s => s.hmsWireId == wireId);

                hmWire.hmsWireDocuments = context.hmsWireDocuments.Where(s => s.hmsWireId == wireId).ToList();
                hmWire.hmsWireWorkflowLogs = context.hmsWireWorkflowLogs.Where(s => s.hmsWireId == wireId).ToList();
                hmWire.hmsWireLogs = context.hmsWireLogs.Where(s => s.hmsWireId == wireId).ToList();
            }

            wireSendingAccount = AccountManager.GetOnBoardingAccount(hmWire.OnBoardAccountId);
            wireReceivingAccount = hmWire.hmsWireTransferTypeLKup.TransferType == "Book Transfer" ? AccountManager.GetOnBoardingAccount(hmWire.OnBoardSSITemplateId) : new onBoardingAccount();
            wireSSITemplate = hmWire.hmsWireTransferTypeLKup.TransferType == "Normal Transfer" || hmWire.hmsWireTransferTypeLKup.TransferType == "Fee/Expense Payment" ? AccountManager.GetSsiTemplate(hmWire.OnBoardSSITemplateId) : new onBoardingSSITemplate();
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

            hmWire.hmsWireCollateralAssociations.ForEach(s => s.hmsWire = null);
            hmWire.hmsWireInvoiceAssociations.ForEach(s => s.hmsWire = null);

            if (hmWire.hmsSwiftStatusLkup != null)
            {
                hmWire.hmsSwiftStatusLkup.hmsWires = null;
                hmWire.hmsSwiftStatusLkup.hmsWireWorkflowLogs = null;
            }
            if (hmWire.hmsWireSenderInformation != null)
                hmWire.hmsWireSenderInformation.hmsWires = null;
            hmWire.hmsWireWorkflowLogs = hmWire.hmsWireWorkflowLogs.OrderByDescending(s => s.CreatedAt).ToList();

            dmaCounterPartyOnBoarding counterparty;
            List<string> workflowUsers;
            List<string> attachmentUsers;

            var hFund = AdminFundManager.GetHFundCreatedForDMA(hmWire.hmFundId, PreferencesManager.FundNameInDropDown.OpsShortName);

            using (var context = new AdminContext())
            {
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var userIds = hmWire.hmsWireWorkflowLogs.Select(s => s.CreatedBy).ToList();
                userIds.AddRange(hmWire.hmsWireDocuments.Select(s => s.CreatedBy).ToList());
                var users = context.hLoginRegistrations.Where(s => userIds.Contains(s.intLoginID)).ToDictionary(s => s.intLoginID, v => v.varLoginID.HumanizeEmail());
                users.Add(-1, "System");

                workflowUsers = hmWire.hmsWireWorkflowLogs.Select(s => users.ContainsKey(s.CreatedBy) ? users[s.CreatedBy] : "Unknown User").ToList();
                attachmentUsers = hmWire.hmsWireDocuments.Select(s => users.ContainsKey(s.CreatedBy) ? users[s.CreatedBy] : "Unknown User").ToList();
                counterparty = context.dmaCounterPartyOnBoardings.FirstOrDefault(s => wireSSITemplate.TemplateEntityId == s.dmaCounterPartyOnBoardId);
            }

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
                ShortFundName = hFund != null ? hFund.ShortFundName : string.Empty
            };
        }

        public static List<WireAccountBaseData> GetApprovedFundAccounts(long hmFundId, bool isBookTransfer, string currency = null)
        {
            var allEligibleAgreementIds = AllEligibleAgreementIds();

            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                var fundAccounts = (from oAccnt in context.onBoardingAccounts
                                    where oAccnt.hmFundId == hmFundId && oAccnt.onBoardingAccountStatus == "Approved" && !oAccnt.IsDeleted && oAccnt.AccountStatus != "Closed"
                                    let isAuthorizedSendingAccount = (currency == null || oAccnt.Currency == currency) && oAccnt.AuthorizedParty == "Hedgemark" && (oAccnt.AccountType == "DDA" || oAccnt.AccountType == "Custody" || oAccnt.AccountType == "Agreement" && allEligibleAgreementIds.Contains(oAccnt.dmaAgreementOnBoardingId ?? 0))
                                    where (isBookTransfer || isAuthorizedSendingAccount)
                                    select new WireAccountBaseData { OnBoardAccountId = oAccnt.onBoardingAccountId, AccountName = oAccnt.AccountName, AccountNumber = oAccnt.AccountNumber, FFCNumber = oAccnt.FFCNumber, IsAuthorizedSendingAccount = isAuthorizedSendingAccount, Currency = oAccnt.Currency }).Distinct().ToList();
                return fundAccounts;
            }
        }

        public static List<WireAccountBaseData> GetApprovedFundAccountsForModule(long hmFundId, long onBoardSSITemplateId, long reportId)
        {
            var allEligibleAgreementIds = AllEligibleAgreementIds();

            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                var fundAccounts = (from oAccnt in context.onBoardingAccounts
                                    where oAccnt.hmFundId == hmFundId && oAccnt.onBoardingAccountStatus == "Approved" && !oAccnt.IsDeleted && oAccnt.AccountStatus != "Closed"
                                    join oMap in context.onBoardingAccountSSITemplateMaps on oAccnt.onBoardingAccountId equals oMap.onBoardingAccountId
                                    let dmaReports = oAccnt.onBoardingAccountModuleAssociations.Select(s => s.onBoardingModule).Select(s => s.dmaReportsId)
                                    let isAuthorizedSendingAccount = (oAccnt.AuthorizedParty == "Hedgemark" && (oAccnt.AccountType == "DDA" || oAccnt.AccountType == "Custody" || oAccnt.AccountType == "Agreement" && allEligibleAgreementIds.Contains(oAccnt.dmaAgreementOnBoardingId ?? 0)))
                                    where oMap.onBoardingSSITemplateId == onBoardSSITemplateId && oMap.Status == "Approved" && isAuthorizedSendingAccount && dmaReports.Contains(reportId)
                                    select new WireAccountBaseData { OnBoardAccountId = oAccnt.onBoardingAccountId, AccountName = oAccnt.AccountName, AccountNumber = oAccnt.AccountNumber, FFCNumber = oAccnt.FFCNumber, IsAuthorizedSendingAccount = isAuthorizedSendingAccount, Currency = oAccnt.Currency }).ToList();
                return fundAccounts;
            }
        }

        private static List<long> AllEligibleAgreementIds()
        {
            using (var context = new AdminContext())
            {
                return context.vw_OnboardedAgreements.Where(s => s.AgreementType == "PB" || s.AgreementType == "Custody").Select(s => s.dmaAgreementOnBoardingId).ToList();
            }
        }

        private static List<FormattedSwiftMessage> GetFormattedSwiftMessages(long wireId)
        {
            var swiftMessages = new List<FormattedSwiftMessage>();

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
            foreach (var log in wireLogs)
            {
                var lastMessageType = log.hmsWireMessageType.MessageType;

                lastKey = !isMultiMessage
                    ? log.hmsWireLogTypeLkup.LogType
                    : string.Format("{0}-{1}", log.hmsWireLogTypeLkup.LogType.Replace("Acknowledged", "Ack"), lastMessageType);

                lastMessageStatus = log.hmsWireLogTypeId;

                swiftMessages.Add(new FormattedSwiftMessage() { Key = lastKey, FormatedMsg = SwiftMessageInterpreter.GetDetailedFormatted(log.SwiftMessage, false), OriginalFinMsg = log.SwiftMessage });
            }

            //Outbound
            if (lastMessageStatus == 1)
            {
                swiftMessages.Add(new FormattedSwiftMessage() { Key = lastKey.Replace("Outbound", isMultiMessage ? "Ack" : "Acknowledged"), FormatedMsg = string.Empty, OriginalFinMsg = string.Empty });
                swiftMessages.Add(new FormattedSwiftMessage() { Key = lastKey.Replace("Outbound", "Confirmation"), FormatedMsg = string.Empty, OriginalFinMsg = string.Empty });
            }

            //Acknowledgment
            else if (lastMessageStatus == 2)
            {
                swiftMessages.Add(new FormattedSwiftMessage() { Key = lastKey.Replace(isMultiMessage ? "Ack" : "Acknowledged", "Confirmation"), FormatedMsg = string.Empty, OriginalFinMsg = string.Empty });
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

                if (userId != -1 && wireStatus != WireStatus.Approved)
                {
                    wire.LastUpdatedBy = userId;
                    wire.LastModifiedAt = DateTime.Now;
                }

                if (wireStatus == WireStatus.Approved && userId != -1)
                {
                    wire.ApprovedAt = DateTime.Now;
                    wire.ApprovedBy = userId;
                }
                else if (swiftStatus == SwiftStatus.NotInitiated)
                {
                    wire.ApprovedAt = null;
                    wire.ApprovedBy = null;
                }

                //if (wireStatus == WireStatus.Initiated)
                //    wire.CreatedBy = userId;

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
                if (wireTicket.HMWire.hmsWireId == 0)
                {
                    wireTicket.HMWire.WireStatusId = (int)wireStatus;
                    wireTicket.HMWire.SwiftStatusId = (int)SwiftStatus.NotInitiated;
                    wireTicket.HMWire.CreatedBy = userId;
                    wireTicket.HMWire.CreatedAt = DateTime.Now;
                    wireTicket.HMWire.LastModifiedAt = DateTime.Now;
                    wireTicket.HMWire.LastUpdatedBy = userId;
                    context.hmsWires.AddOrUpdate(wireTicket.HMWire);
                    context.SaveChanges();
                }
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
                if (existingWireTicket.IsNotice && wireStatus == WireStatus.Initiated)
                    SaveWireData(wireTicket, WireStatus.Approved, comment, userId);
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
        public static List<hmsWireMessageType> GetWireMessageTypes()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsWireMessageTypes.AsNoTracking().ToList();
            }
        }


        public static bool IsNoticeWirePendingAcknowledgement(hmsWire wire)
        {
            using (var context = new OperationsSecureContext())
            {
                var duplicateNotice = context.hmsWires.FirstOrDefault(s => DbFunctions.TruncateTime(s.ValueDate) == DbFunctions.TruncateTime(wire.ValueDate) && s.Currency == wire.Currency && s.hmsWireTransferTypeLKup.TransferType == "Notice" && s.hmsWireId != wire.hmsWireId && ((SwiftStatus)s.SwiftStatusId == SwiftStatus.Processing || (SwiftStatus)s.SwiftStatusId == SwiftStatus.Acknowledged)) ?? new hmsWire();
                return duplicateNotice.Amount == wire.Amount;
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
                var document = context.hmsWireDocuments.Where(x => x.hmsWireId == wireId).FirstOrDefault(s => s.FileName == fileName);
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

        public static List<hmsWireJobSchedule> GetSchedulesWithoutJobsData()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsWireJobSchedules.Where(s => !s.IsJobCreated).ToList();
            }
        }

        public static List<long> GetNoticeWiresAwaitingApproval()
        {
            using (var context = new OperationsSecureContext())
            {
                return context.hmsWires.Where(s => s.hmsWireMessageType.MessageType == "MT210" && s.WireStatusId == (int)WireStatus.Initiated).Select(s => s.hmsWireId).ToList();
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

        #region Wire Portal Cutoffs

        public static List<onBoardingWirePortalCutoff> GetWirePortalCutoffData()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.onBoardingWirePortalCutoffs.ToList();
            }
        }

        public static void SaveWirePortalCutoff(onBoardingWirePortalCutoff wirePortalCutoff)
        {
            using (var context = new OperationsSecureContext())
            {
                context.onBoardingWirePortalCutoffs.AddOrUpdate(wirePortalCutoff);
                context.SaveChanges();
            }
        }

        public static void DeleteWirePortalCutoff(long wireCutoffId)
        {
            using (var context = new OperationsSecureContext())
            {
                var wirePortalCutoff = context.onBoardingWirePortalCutoffs.FirstOrDefault(s => s.onBoardingWirePortalCutoffId == wireCutoffId);
                context.onBoardingWirePortalCutoffs.Remove(wirePortalCutoff);
                context.SaveChanges();
            }
        }

        #endregion
    }
}
