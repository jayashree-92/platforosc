using System;
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


    public class WireSourceDetails
    {
        public WireSourceDetails()
        {
            Details = new Dictionary<string, string>();
        }
        public long SourceModuleId { get; set; }
        public string SourceModuleName { get; set; }
        public string AttachmentName { get; set; }
        public string FileSource { get; set; }
        public Dictionary<string, string> Details { get; set; }

        public bool IsSourceAvailable { get { return !string.IsNullOrWhiteSpace(SourceModuleName); } }
    }

    public class WireBaseDetails
    {
        public long SendingAccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime ValueDate { get; set; }
        public int WireStatusId { get; set; }
        public string Currency { get; set; }
    }

    public class CashBalances
    {
        public bool IsCashBalanceAvailable { get; set; }
        public DateTime ContextDate { get; set; }
        public decimal TreasuryBalance { get; set; }
        public int ApprovedWires { get; set; }
        public int PendingWires { get; set; }
        public decimal TotalWired { get; set; }
        public string Currency { get; set; }
        public decimal AvailableBalance
        {
            get { return TreasuryBalance - TotalWired; }
        }
    }

    public class WireTicketStatus
    {
        public WireTicketStatus(WireTicket wireTicket, int userId, bool isWireApprover, bool isAdHocWire = false)
        {
            var wireStatusId = wireTicket.HMWire.WireStatusId;

            IsWireStatusDrafted = (int)WireDataManager.WireStatus.Drafted == wireStatusId;
            IsWireStatusCancelled = (int)WireDataManager.WireStatus.Cancelled == wireStatusId;
            IsWireStatusApproved = (int)WireDataManager.WireStatus.Approved == wireStatusId;
            IsWireStatusFailed = (int)WireDataManager.WireStatus.Failed == wireStatusId;
            IsWireStatusInitiated = (int)WireDataManager.WireStatus.Initiated == wireStatusId;
            IsWireStatusOnHold = (int)WireDataManager.WireStatus.OnHold == wireStatusId;

            var swiftStatusId = wireTicket.HMWire.WireStatusId;

            IsSwiftStatusNotInitiated = (int)WireDataManager.SwiftStatus.NotInitiated == swiftStatusId;
            IsSwiftStatusProcessing = (int)WireDataManager.SwiftStatus.Processing == swiftStatusId;
            IsSwiftStatusAcknowledged = (int)WireDataManager.SwiftStatus.Acknowledged == swiftStatusId;
            IsSwiftStatusNegativeAcknowledged = (int)WireDataManager.SwiftStatus.NegativeAcknowledged == swiftStatusId;
            IsSwiftStatusCompleted = (int)WireDataManager.SwiftStatus.Completed == swiftStatusId;
            IsSwiftStatusFailed = (int)WireDataManager.SwiftStatus.Failed == swiftStatusId;

            ValidationMessage = string.Empty;

            if (wireTicket.IsNotice)
            {
                IsNoticePending = WireDataManager.IsNoticeWirePendingAcknowledgement(wireTicket.HMWire);
                if (IsNoticePending)
                    ValidationMessage = "The notice with same amount, value date and currency is already Processing with SWIFT.You cannot notice the same untill it gets a Confirmation";
            }

            IsDeadlineCrossed = DateTime.Now.Date > wireTicket.HMWire.ValueDate.Date;

            IsEditEnabled = IsWireStatusDrafted && !IsDeadlineCrossed;
            IsApprovedOrFailed = IsWireStatusCancelled || IsWireStatusApproved || IsWireStatusFailed;
            IsCancelEnabled = wireTicket.HMWire.hmsWireId > 0 && (!IsApprovedOrFailed || IsSwiftStatusAcknowledged && !IsDeadlineCrossed);

            IsDraftEnabled = !IsDeadlineCrossed && (IsWireStatusInitiated || IsWireStatusFailed || IsWireStatusCancelled && IsSwiftStatusNotInitiated);
            IsWirePurposeAdhoc = isAdHocWire || wireTicket.HMWire.hmsWirePurposeLkup.ReportName == ReportName.AdhocWireReport;

            var isUserInvolvedInInitation = wireTicket.HMWire.hmsWireWorkflowLogs.Where(s => s.WireStatusId == (int)WireDataManager.WireStatus.Initiated || s.WireStatusId == (int)WireDataManager.WireStatus.Drafted).Any(s => s.CreatedBy == userId)
                || wireTicket.HMWire.CreatedBy == userId || wireTicket.HMWire.LastUpdatedBy == userId;

            IsAuthorizedUserToApprove = IsWireStatusInitiated && !isUserInvolvedInInitation && !IsDeadlineCrossed && isWireApprover && !IsNoticePending;

            IsLastModifiedUser = wireTicket.HMWire.LastUpdatedBy == userId;
        }

        public bool IsWireStatusDrafted { get; private set; }
        public bool IsWireStatusCancelled { get; private set; }
        public bool IsWireStatusApproved { get; private set; }
        public bool IsWireStatusFailed { get; private set; }
        public bool IsWireStatusInitiated { get; private set; }
        public bool IsWireStatusOnHold { get; private set; }
        public bool IsSwiftStatusNotInitiated { get; private set; }
        public bool IsSwiftStatusProcessing { get; private set; }
        public bool IsSwiftStatusAcknowledged { get; private set; }
        public bool IsSwiftStatusNegativeAcknowledged { get; private set; }
        public bool IsSwiftStatusCompleted { get; private set; }
        public bool IsSwiftStatusFailed { get; private set; }


        public bool IsDeadlineCrossed { get; private set; }
        public bool IsEditEnabled { get; private set; }
        public bool IsApprovedOrFailed { get; private set; }
        //public bool IsSwiftCancelDisabled { get; private set; }
        public bool IsCancelEnabled { get; private set; }
        public bool IsWirePurposeAdhoc { get; private set; }
        public bool IsDraftEnabled { get; private set; }
        public bool IsAuthorizedUserToApprove { get; private set; }
        public bool IsLastModifiedUser { get; private set; }
        public bool IsNoticePending { get; private set; }
        public string ValidationMessage { get; private set; }
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
            OnHold
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
            ThirdPartyTransfer = 1,
            FundTransfer,
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

                context.Configuration.LazyLoadingEnabled = false;

                hmWire = context.hmsWires.Include(s => s.hmsWireMessageType)
                                         //.Include("hmsWireDocuments")
                                         //.Include("hmsWireWorkflowLogs")
                                         .Include(s => s.hmsWireStatusLkup)
                                         .Include(s => s.hmsWirePurposeLkup)
                                         .Include(s => s.hmsWireTransferTypeLKup)
                                         .Include(s => s.hmsWireSenderInformation)
                                         .Include(s => s.hmsWireInvoiceAssociations)
                                         .Include(s => s.hmsWireCollateralAssociations)
                                         //.Include(s => s.SendingAccount)
                                         //.Include(s => s.ReceivingAccount)
                                         //.Include(s => s.ReceivingSSITemplate)
                                         //.Include("hmsWireLogs")
                                         .First(s => s.hmsWireId == wireId);

                hmWire.hmsWireDocuments = context.hmsWireDocuments.Where(s => s.hmsWireId == wireId).ToList();
                hmWire.hmsWireWorkflowLogs = context.hmsWireWorkflowLogs.Where(s => s.hmsWireId == wireId).ToList();
                hmWire.hmsWireLogs = context.hmsWireLogs.Where(s => s.hmsWireId == wireId).ToList();
            }

            hmWire.SendingAccount = FundAccountManager.GetOnBoardingAccount(hmWire.OnBoardAccountId);

            if (hmWire.ReceivingOnBoardAccountId != null)
                hmWire.ReceivingAccount = FundAccountManager.GetOnBoardingAccount(hmWire.ReceivingOnBoardAccountId ?? 0);

            if (hmWire.OnBoardSSITemplateId != null)
                hmWire.ReceivingSSITemplate = SSITemplateManager.GetSsiTemplate(hmWire.OnBoardSSITemplateId ?? 0);

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

            dmaCounterPartyOnBoarding counterparty = null;
            List<string> workflowUsers;
            List<string> attachmentUsers;

            var hFund = AdminFundManager.GetHFundsCreatedForDMAOnly(PreferencesManager.FundNameInDropDown.OpsShortName, hmWire.hmFundId);

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
                if (hmWire.ReceivingSSITemplate != null)
                    counterparty = context.dmaCounterPartyOnBoardings.FirstOrDefault(s => hmWire.ReceivingSSITemplate.TemplateEntityId == s.dmaCounterPartyOnBoardId);
            }

            return new WireTicket()
            {
                HMWire = hmWire,
                //Agreement = wireAgreement,
                SendingAccount = hmWire.SendingAccount,
                ReceivingAccount = hmWire.WireTransferTypeId == 2 ? hmWire.ReceivingAccount : new onBoardingAccount(),
                SSITemplate = hmWire.ReceivingSSITemplate ?? new onBoardingSSITemplate(),
                AttachmentUsers = attachmentUsers,
                WorkflowUsers = workflowUsers,
                Counterparty = (counterparty ?? new dmaCounterPartyOnBoarding()).CounterpartyName,
                SwiftMessages = GetFormattedSwiftMessages(hmWire.hmsWireId),
                ShortFundName = hFund != null ? hFund.PreferredFundName : string.Empty
            };
        }

        public static List<WireAccountBaseData> GetApprovedFundAccounts(long hmFundId, bool isFundTransfer, string currency = null)
        {
            var allEligibleAgreementIds = AllEligibleAgreementIds();

            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                var fundAccounts = (from oAccnt in context.onBoardingAccounts
                                    where oAccnt.hmFundId == hmFundId && oAccnt.onBoardingAccountStatus == "Approved" && !oAccnt.IsDeleted && oAccnt.AccountStatus != "Closed"
                                    let isAuthorizedSendingAccount = (currency == null || oAccnt.Currency == currency) && oAccnt.AuthorizedParty == "Hedgemark" && (oAccnt.AccountType == "DDA" || oAccnt.AccountType == "Custody" || oAccnt.AccountType == "Agreement" && allEligibleAgreementIds.Contains(oAccnt.dmaAgreementOnBoardingId ?? 0))
                                    where (isFundTransfer || isAuthorizedSendingAccount)
                                    select new WireAccountBaseData { OnBoardAccountId = oAccnt.onBoardingAccountId, AccountName = oAccnt.AccountName, AccountNumber = oAccnt.UltimateBeneficiaryAccountNumber, FFCNumber = oAccnt.FFCNumber, IsAuthorizedSendingAccount = isAuthorizedSendingAccount, Currency = oAccnt.Currency }).Distinct().ToList();
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
                                    select new WireAccountBaseData { OnBoardAccountId = oAccnt.onBoardingAccountId, AccountName = oAccnt.AccountName, AccountNumber = oAccnt.UltimateBeneficiaryAccountNumber, FFCNumber = oAccnt.FFCNumber, IsAuthorizedSendingAccount = isAuthorizedSendingAccount, Currency = oAccnt.Currency }).ToList();
                return fundAccounts;
            }
        }

        private static List<long> AllEligibleAgreementIds()
        {
            using (var context = new AdminContext())
            {
                return context.vw_CounterpartyAgreements.Where(s => s.AgreementType == "PB" || s.AgreementType == "Custody" || s.AgreementType == "Synthetic Prime Brokerage").Select(s => s.dmaAgreementOnBoardingId).ToList();
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

                if (wire.ReceivingOnBoardAccountId == 0)
                    wire.ReceivingOnBoardAccountId = null;
                if (wire.OnBoardSSITemplateId == 0)
                    wire.OnBoardSSITemplateId = null;

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

                    if (wireTicket.HMWire.OnBoardSSITemplateId == 0)
                        wireTicket.HMWire.OnBoardSSITemplateId = null;

                    if (wireTicket.HMWire.ReceivingOnBoardAccountId == 0)
                        wireTicket.HMWire.ReceivingOnBoardAccountId = null;

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

        public static bool IsWireCreated(DateTime valueDate, string purpose, long sendingAccountId, long receivingAccountId, long receivingSSITemplateId, long wireId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsWires.Any(s =>
                    s.ValueDate == valueDate && s.hmsWirePurposeLkup.Purpose == purpose && s.hmsWireId != wireId &&
                    s.OnBoardAccountId == sendingAccountId &&
                    (receivingSSITemplateId > 0 && s.OnBoardSSITemplateId == receivingSSITemplateId
                    || receivingAccountId > 0 && s.ReceivingOnBoardAccountId == receivingAccountId));
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

        public static List<hmsWirePortalCutoff> GetWirePortalCutoffData()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsWirePortalCutoffs.ToList();
            }
        }

        public static void SaveWirePortalCutoff(hmsWirePortalCutoff wirePortalCutoff)
        {
            using (var context = new OperationsSecureContext())
            {
                context.hmsWirePortalCutoffs.AddOrUpdate(wirePortalCutoff);
                context.SaveChanges();
            }
        }

        public static void DeleteWirePortalCutoff(long wireCutoffId)
        {
            using (var context = new OperationsSecureContext())
            {
                var wirePortalCutoff = context.hmsWirePortalCutoffs.First(s => s.hmsWirePortalCutoffId == wireCutoffId);
                context.hmsWirePortalCutoffs.Remove(wirePortalCutoff);
                context.SaveChanges();
            }
        }

        #endregion
    }
}
