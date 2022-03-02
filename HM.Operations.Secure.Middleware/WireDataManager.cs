using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.CompilerServices;
using Com.HedgeMark.Commons;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.SwiftMessageHandler;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware.Models;
using HM.Operations.Secure.Middleware.Util;
using log4net;

namespace HM.Operations.Secure.Middleware
{
    public class WireAccountBaseData
    {
        public long OnBoardAccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string FFCNumber { get; set; }
        public bool IsAuthorizedSendingAccount { get; set; }
        public string Currency { get; set; }
        public DateTime ValueDate { get; set; }
        public int WireStatusId { get; set; }
        public decimal Amount { get; set; }
        public bool IsParentFund { get; set; }
        public bool IsSubAdvisorFund { get; set; }
        public long FundId { get; set; }

        public string AccountNameAndNumber => string.IsNullOrWhiteSpace(FFCNumber) ? $"{AccountNumber}-{AccountName}" : $"{FFCNumber}-{AccountNumber}-{AccountName}";

        public DateTimeOffset? ApprovedAt { get; set; }
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

        public bool IsSourceAvailable => !string.IsNullOrWhiteSpace(SourceModuleName);
    }

    public class CashBalances
    {
        public class WiredDetails
        {
            public DateTime ValueDate { get; set; }
            public int ApprovedCount { get; set; }
            public int PendingCount { get; set; }
            public decimal ApprovedWireAmount { get; set; }
            public decimal PendingWireAmount { get; set; }
            public decimal TotalWireEntered => ApprovedWireAmount + PendingWireAmount;
        }

        public List<WiredDetails> WireDetails { get; set; }
        public bool IsCashBalanceAvailable { get; set; }
        public DateTime ContextDate { get; set; }
        public decimal TreasuryBalance { get; set; }
        public decimal TotalWireEntered { get; set; }
        public string Currency { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal AvailableBalance => TreasuryBalance - TotalWireEntered;
        public decimal TotalPendingWireEntered { get; set; }
        public decimal TotalApprovedWireAfterDeadline { get; set; }
        public decimal AvailableHoldBackBalance => HoldBackAmount - TotalPendingWireEntered - TotalApprovedWireAfterDeadline;
        public decimal MarginBuffer { get; set; }
        public decimal HoldBackAmount { get; set; }
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

            var swiftStatusId = wireTicket.HMWire.SwiftStatusId;

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

            IsDraftEnabled = (IsWireStatusInitiated || IsWireStatusFailed || IsWireStatusCancelled) && IsSwiftStatusNotInitiated;//!IsDeadlineCrossed && 
            IsWirePurposeAdhoc = isAdHocWire || wireTicket.HMWire.hmsWirePurposeLkup.ReportName == ReportName.AdhocWireReport;

            var isUserInvolvedInInitiation = wireTicket.HMWire.hmsWireWorkflowLogs.Where(s => s.WireStatusId == (int)WireDataManager.WireStatus.Initiated || s.WireStatusId == (int)WireDataManager.WireStatus.Drafted).Any(s => s.CreatedBy == userId)
                || wireTicket.HMWire.CreatedBy == userId || wireTicket.HMWire.LastUpdatedBy == userId;

            IsAuthorizedUserToApprove = IsWireStatusInitiated && !isUserInvolvedInInitiation && !IsDeadlineCrossed && isWireApprover && !IsNoticePending;

            ShouldEnableCollateralPurpose = (wireTicket.Is3rdPartyTransfer || wireTicket.IsNotice) && wireTicket.SendingAccount.AuthorizedParty == "Hedgemark" && OpsSecureSwitches.SwiftBicToEnableField21.Contains(wireTicket.SendingAccount.SwiftGroup.SendersBIC);

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
        public bool IsCancelEnabled { get; private set; }
        public bool IsWirePurposeAdhoc { get; private set; }
        public bool IsDraftEnabled { get; private set; }
        public bool IsAuthorizedUserToApprove { get; private set; }
        public bool IsLastModifiedUser { get; private set; }
        public bool IsNoticePending { get; private set; }
        public bool ShouldEnableCollateralPurpose { get; private set; }
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
            FeeOrExpensesPayment,
            Notice,
            BankLoanOrPrivateOrIPO
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
            return $"{environmentStr}DMO{wireIdStr}";
        }

        public static string GetFundRegistedAddress(long hmFundId)
        {
            using (var context = new OperationsContext())
            {
                return context.vw_HFundOps.Where(s => s.hmFundId == hmFundId).Select(s => s.RegisterAddress).FirstOrDefault();
            }
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
                    .Include(s => s.hmsWireStatusLkup)
                                         .Include(s => s.hmsWirePurposeLkup)
                                         .Include(s => s.hmsWireTransferTypeLKup)
                                         .Include(s => s.hmsWireSenderInformation)
                                         .Include(s => s.hmsWireInvoiceAssociations)
                                         .Include(s => s.hmsWireCollateralAssociations)
                                         .Include(s => s.hmsWireInterestAssociations)
                                         .Include(s => s.hmsWireField)
                                         .Include(s => s.hmsWireField.hmsCollateralCashPurposeLkup)
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
            hmWire.hmsWireInterestAssociations.ForEach(s => s.hmsWire = null);

            if (hmWire.hmsWireField != null)
            {
                hmWire.hmsWireField.hmsWires = null;
                hmWire.hmsWireField.hmsCollateralCashPurposeLkup.hmsWireFields = null;
            }
            else
                hmWire.hmsWireField = new hmsWireField();

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
                FundRegisterAddress = hFund.RegisteredAddress,
                SendingAccount = hmWire.SendingAccount,
                ReceivingAccount = hmWire.WireTransferTypeId == 2 ? hmWire.ReceivingAccount : new onBoardingAccount(),
                SSITemplate = hmWire.ReceivingSSITemplate ?? new onBoardingSSITemplate(),
                AttachmentUsers = attachmentUsers,
                WorkflowUsers = workflowUsers,
                Counterparty = (counterparty ?? new dmaCounterPartyOnBoarding()).CounterpartyName,
                SwiftMessages = GetFormattedSwiftMessages(hmWire.hmsWireId),
                ShortFundName = hFund.PreferredFundName
            };
        }

        public static List<WireAccountBaseData> GetApprovedFundAccounts(long hmFundId, WireDataManager.TransferType wireTransferType, string currency = null)
        {
            var allEligibleAgreementIds = AllEligibleAgreementIds();

            var isFundTransfer = wireTransferType == TransferType.FundTransfer;
            var isNotice = wireTransferType == TransferType.Notice;

            Dictionary<long, IEnumerable<long>> umberllaFundMap;
            using (var context = new AdminContext())
            {
                var onbFundId = context.vw_HFund.Where(s => s.hmFundId == hmFundId).Select(s => s.dmaFundOnBoardId).FirstOrDefault() ?? 0;
                umberllaFundMap = context.onboardingSubAdvisorFundMaps.Include(s => s.parentFund).Include(s => s.umberllaFund).Where(s => !s.IsDeleted && (s.dmaFundOnBoardId == onbFundId || s.UmbrellaFundId == onbFundId))
                    .Select(s => new { parentFundId = s.parentFund.FundMapId ?? 0, umberllaFundId = s.umberllaFund.FundMapId ?? 0 }).GroupBy(s => s.parentFundId).ToDictionary(s => s.Key, v => v.Select(s => s.umberllaFundId));
            }

            List<long> parentFundIds = new List<long>(), subFundIds = new List<long>();

            foreach (var umberllaMap in umberllaFundMap)
            {
                parentFundIds.Add(umberllaMap.Key);
                subFundIds.AddRange(umberllaMap.Value);
            }

            var allFundIds = new List<long>() { hmFundId };
            allFundIds.AddRange(parentFundIds.Union(subFundIds));
            allFundIds = allFundIds.Distinct().ToList();

            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                var fundAccounts = (from oAccnt in context.onBoardingAccounts
                                    where allFundIds.Contains(oAccnt.hmFundId) && oAccnt.onBoardingAccountStatus == "Approved" && !oAccnt.IsDeleted && oAccnt.AccountStatus != "Closed"
                                    let isAuthorizedSendingAccount = (currency == null || oAccnt.Currency == currency) && oAccnt.AuthorizedParty == "Hedgemark" && (oAccnt.AccountType == "DDA" || oAccnt.AccountType == "Custody" || oAccnt.AccountType == "Agreement" && allEligibleAgreementIds.Contains(oAccnt.dmaAgreementOnBoardingId ?? 0))
                                    let isAuthorizedSendingAccountFinal = isNotice ? isAuthorizedSendingAccount && oAccnt.SwiftGroup.AcceptedMessages.Contains("MT210") : isAuthorizedSendingAccount
                                    where (isFundTransfer || isAuthorizedSendingAccountFinal)

                                    select new WireAccountBaseData
                                    {
                                        OnBoardAccountId = oAccnt.onBoardingAccountId,
                                        AccountName = oAccnt.AccountName,
                                        AccountNumber = oAccnt.UltimateBeneficiaryAccountNumber,
                                        FFCNumber = oAccnt.FFCNumber,
                                        IsAuthorizedSendingAccount = isAuthorizedSendingAccount,
                                        Currency = oAccnt.Currency,
                                        IsParentFund = parentFundIds.Contains(oAccnt.hmFundId),
                                        IsSubAdvisorFund = subFundIds.Contains(oAccnt.hmFundId),
                                        FundId = oAccnt.hmFundId
                                    }).Distinct().ToList();

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
                                    select new WireAccountBaseData
                                    {
                                        OnBoardAccountId = oAccnt.onBoardingAccountId,
                                        AccountName = oAccnt.AccountName,
                                        AccountNumber = oAccnt.UltimateBeneficiaryAccountNumber,
                                        FFCNumber = oAccnt.FFCNumber,
                                        IsAuthorizedSendingAccount = isAuthorizedSendingAccount,
                                        Currency = oAccnt.Currency
                                    }).ToList();
                return fundAccounts;
            }
        }

        public static readonly List<string> AgreementTypesEligibleForSendingWires = OpsSecureSwitches.GetSwitchValue(Switches.SwitchKey.AgreementTypesEligibleForSendingWires);

        private static List<long> AllEligibleAgreementIds()
        {
            using (var context = new AdminContext())
            {
                return context.vw_CounterpartyAgreements.Where(s => AgreementTypesEligibleForSendingWires.Contains(s.AgreementType)).Select(s => s.dmaAgreementOnBoardingId).Distinct().ToList();
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
                    : $"{log.hmsWireLogTypeLkup.LogType.Replace("Acknowledged", "Ack")}-{lastMessageType}";

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

        public static hmsWire GetWire(long wireId)
        {
            using (var context = new OperationsSecureContext())
            {
                var wireTicket = context.hmsWires.FirstOrDefault(s => s.hmsWireId == wireId);
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
            hmsWire hmsWire = GetWire(wireId);
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
                    wire.LastModifiedAt = DateTime.UtcNow;
                }

                if (wireStatus == WireStatus.Approved && userId != -1)
                {
                    wire.ApprovedAt = DateTime.UtcNow;
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
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                };
                context.hmsWireWorkflowLogs.AddOrUpdate(wireWorkFlowLog);
                context.SaveChanges();

                return wireWorkFlowLog;

            }
        }

        //We need to make sure only one transaction is performed at a given time - to avoid same wire is being approved by two different scenario
        public static object WireSaveTransactionLock = new object();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static WireTicket SaveWireData(WireTicket wireTicket, WireStatus wireStatus, string comment, int userId)
        {
            using (var context = new OperationsSecureContext())
            {
                if (wireTicket.HMWire.hmsWireField != null && wireTicket.HMWire.hmsWireField.hmsCollateralCashPurposeLkupId == 0)
                    wireTicket.HMWire.hmsWireField = null;

                if (wireTicket.HMWire.hmsWireId == 0)
                {
                    wireTicket.HMWire.WireStatusId = (int)wireStatus;
                    wireTicket.HMWire.SwiftStatusId = (int)SwiftStatus.NotInitiated;
                    wireTicket.HMWire.CreatedBy = userId;
                    var currentUtcTime = DateTime.UtcNow;
                    wireTicket.HMWire.CreatedAt = currentUtcTime;
                    wireTicket.HMWire.LastModifiedAt = currentUtcTime;
                    wireTicket.HMWire.LastUpdatedBy = userId;

                    if (wireTicket.HMWire.OnBoardSSITemplateId == 0)
                        wireTicket.HMWire.OnBoardSSITemplateId = null;

                    if (wireTicket.HMWire.ReceivingOnBoardAccountId == 0)
                        wireTicket.HMWire.ReceivingOnBoardAccountId = null;

                    context.hmsWires.AddOrUpdate(wireTicket.HMWire);
                    context.SaveChanges();
                }
                if (wireTicket.HMWire.hmsWireField != null)
                    context.hmsWireFields.AddOrUpdate(wireTicket.HMWire.hmsWireField);
                context.hmsWireDocuments.AddRange(wireTicket.HMWire.hmsWireDocuments.Where(s => s.hmsWireDocumentId == 0));
                context.SaveChanges();
            }
            lock (WireSaveTransactionLock)
            {
                var existingWireTicket = GetWireData(wireTicket.HMWire.hmsWireId);

                if (wireStatus == WireStatus.Approved)
                    WireTransactionManager.ApproveAndInitiateWireTransfer(existingWireTicket, comment, userId);

                else if (existingWireTicket.HMWire.WireStatusId == (int)WireStatus.Approved && wireStatus == WireStatus.Cancelled)
                    WireTransactionManager.CancelWireTransfer(existingWireTicket, comment, userId);

                //if the wire is already approved, user can only cancel it, and cannot perform other options such as hold
                else if (existingWireTicket.HMWire.WireStatusId == (int)WireStatus.Approved && wireStatus != WireStatus.Cancelled)
                    throw new InvalidOperationException(
                        $"Wire already Approved and '{wireStatus}' action cannot be performed at this time");

                else
                {
                    wireTicket.HMWire.CreatedAt = existingWireTicket.HMWire.CreatedAt;
                    SetWireStatusAndWorkFlow(wireTicket.HMWire, wireStatus, SwiftStatus.NotInitiated, comment, userId);
                    if (existingWireTicket.IsNotice && wireStatus == WireStatus.Initiated)
                        SaveWireData(wireTicket, WireStatus.Approved, comment, userId);
                }

                return wireTicket;
            }
        }

        public static TimeSpan GetDeadlineToApprove(onBoardingAccount onboardAccount, DateTime valueDate, Dictionary<string, string> timeZones = null)
        {
            if (timeZones == null)
                timeZones = FileSystemManager.GetAllTimeZones();

            if (onboardAccount.WirePortalCutoff == null)
                onboardAccount.WirePortalCutoff = new hmsWirePortalCutoff();

            var baseTimeZone = timeZones[FileSystemManager.DefaultTimeZone];
            var destinationTimeZone = TimeZoneInfo.FindSystemTimeZoneById(baseTimeZone);

            var currentTime = DateTime.Now;
            if (TimeZoneInfo.Local.Id != baseTimeZone)
                currentTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, baseTimeZone);

            var cutOffTimeDeadline = GetCutOffTime(onboardAccount.WirePortalCutoff.CutoffTime, valueDate, onboardAccount.WirePortalCutoff.CutOffTimeZone, timeZones, destinationTimeZone, onboardAccount.WirePortalCutoff.DaystoWire);

            //when there is no cash sweep, use only cut-off time
            if (onboardAccount.CashSweep == "No")
                return cutOffTimeDeadline - currentTime;

            var cashSweepTimeDeadline = GetCashSweepDeadline(valueDate, onboardAccount.CashSweepTime, onboardAccount.CashSweepTimeZone, destinationTimeZone, timeZones);
            return cashSweepTimeDeadline < cutOffTimeDeadline ? cashSweepTimeDeadline - currentTime : cutOffTimeDeadline - currentTime;

        }

        public static DateTime GetCashSweepDeadline(DateTime valueDate, TimeSpan? cashSweepTime, string cashSweepTimeZone, TimeZoneInfo destinationTimeZone = null, Dictionary<string, string> timeZones = null)
        {
            if (timeZones == null)
                timeZones = FileSystemManager.GetAllTimeZones();

            if (destinationTimeZone == null)
            {
                var baseTimeZone = timeZones[FileSystemManager.DefaultTimeZone];
                destinationTimeZone = TimeZoneInfo.FindSystemTimeZoneById(baseTimeZone);
            }

            return GetCutOffTime(cashSweepTime ?? new TimeSpan(23, 59, 0), valueDate, cashSweepTimeZone, timeZones, destinationTimeZone);
        }


        private static DateTime GetCutOffTime(TimeSpan cutOffTime, DateTime valueDate, string cutoffTimeZone, Dictionary<string, string> timeZones, TimeZoneInfo destinationTimeZone, int daysToAdd = 0)
        {
            if (string.IsNullOrWhiteSpace(cutoffTimeZone))
                cutoffTimeZone = destinationTimeZone.Id;

            var cutOff = valueDate.AddDays(daysToAdd).Date.Add(cutOffTime);

            if (cutoffTimeZone == destinationTimeZone.Id)
                return cutOff;

            var cutOffTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZones.ContainsKey(cutoffTimeZone) ? timeZones[cutoffTimeZone] : destinationTimeZone.Id);
            var deadline = TimeZoneInfo.ConvertTime(new DateTime(cutOff.Ticks, DateTimeKind.Unspecified), cutOffTimeZoneInfo, destinationTimeZone);

            return deadline;
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


        public static List<long> GetNoticeWiresAwaitingApproval()
        {
            using (var context = new OperationsSecureContext())
            {
                return context.hmsWires.Where(s => s.hmsWireMessageType.MessageType == "MT210" && s.WireStatusId == (int)WireStatus.Initiated).Select(s => s.hmsWireId).ToList();
            }
        }
    }
}
