using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.Operations.Secure.DataModel;
using HedgeMark.Operations.Secure.Middleware.Util;
using log4net;

namespace HedgeMark.Operations.Secure.Middleware
{
    public class FundAccountManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FundAccountManager));

        public const int FundTemplateTypeId = 1;
        public const string AgreementAccountType = "Agreement";

        public static List<vw_FundAccounts> GetOnBoardingAccountDetails(List<long> hmFundIds, bool isPreviledgedUser)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.vw_FundAccounts.Where(s => isPreviledgedUser || hmFundIds.Contains(s.hmFundId)).ToList();
            }

        }

        public static List<onBoardingAccount> GetAllOnBoardingAccounts(List<long> hmFundIds, bool isPreviledgedUser)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                return context.onBoardingAccounts.AsNoTracking()
                    .Include(s => s.Beneficiary)
                    .Include(s => s.Intermediary)
                    .Include(s => s.UltimateBeneficiary)
                    .Include(s => s.WirePortalCutoff)
                    .Include(s => s.SwiftGroup)
                    .Include(s => s.hmsAccountCallbacks)
                    .Include(x => x.onBoardingAccountSSITemplateMaps).Where(x => !x.IsDeleted).Where(s => isPreviledgedUser || hmFundIds.Contains(s.hmFundId)).ToList();
            }

        }

        public static onBoardingAccount GetOnBoardingAccount(long accountId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                var account = context.onBoardingAccounts
                    .Include(s => s.Beneficiary)
                    .Include(s => s.Intermediary)
                    .Include(s => s.UltimateBeneficiary)
                    .Include(s => s.WirePortalCutoff)
                    .Include(s => s.SwiftGroup)
                    .Include(s => s.hmsAccountCallbacks)
                    .FirstOrDefault(acnt => acnt.onBoardingAccountId == accountId);

                return SetAccountDefaults(account);
            }
        }

        public static onBoardingAccount SetAccountDefaults(onBoardingAccount account)
        {
            if (account == null)
                return null;

            if (account.WirePortalCutoff == null)
                account.WirePortalCutoff = new hmsWirePortalCutoff() { CutOffTimeZone = "EST" };
            if (account.Beneficiary == null)
                account.Beneficiary = new onBoardingAccountBICorABA();
            if (account.Intermediary == null)
                account.Intermediary = new onBoardingAccountBICorABA();
            if (account.UltimateBeneficiary == null)
                account.UltimateBeneficiary = new onBoardingAccountBICorABA();
            if (account.SwiftGroup == null)
                account.SwiftGroup = new hmsSwiftGroup();
            if (account.hmsAccountCallbacks == null)
                account.hmsAccountCallbacks = new List<hmsAccountCallback>();
            //remove circular references
            account.WirePortalCutoff.onBoardingAccounts = null;
            account.Beneficiary.onBoardingAccounts = account.Beneficiary.onBoardingAccounts1 = account.Beneficiary.onBoardingAccounts2 = null;
            account.Intermediary.onBoardingAccounts = account.Intermediary.onBoardingAccounts1 = account.Intermediary.onBoardingAccounts2 = null;
            if (account.UltimateBeneficiary != null)
                account.UltimateBeneficiary.onBoardingAccounts = account.UltimateBeneficiary.onBoardingAccounts1 = account.UltimateBeneficiary.onBoardingAccounts2 = null;
            account.SwiftGroup.onBoardingAccounts = null;
            if (account.SwiftGroup.hmsSwiftGroupStatusLkp != null)
                account.SwiftGroup.hmsSwiftGroupStatusLkp.hmsSwiftGroups = null;
            account.hmsAccountCallbacks.ForEach(s => s.onBoardingAccount = null);

            account.onBoardingAccountDocuments.ForEach(s => s.onBoardingAccount = null);
            account.onBoardingAccountSSITemplateMaps.ForEach(s => s.onBoardingAccount = null);
            account.onBoardingAccountModuleAssociations.ForEach(s => s.onBoardingAccount = null);

            account.hmsWires = null;
            account.hmsWires1 = null;
            return account;
        }

        public static List<onBoardingAccountSSITemplateMap> GetAccountSsiTemplateMap(long accountId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.onBoardingAccountSSITemplateMaps.Include(x => x.onBoardingSSITemplate).Where(x => x.onBoardingAccountId == accountId).ToList();
            }
        }

        public static List<onBoardingAccountSSITemplateMap> GetSsiTemplateAccountMap(long ssiTemplateId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.onBoardingAccountSSITemplateMaps.Where(x => x.onBoardingSSITemplateId == ssiTemplateId).ToList();
            }
        }

        public static List<OnBoardingAccountDescription> GetAccountDescriptionsByAgreementTypeId(long agreementTypeId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.OnBoardingAccountDescriptions.Where(x => x.dmaAgreementTypeId == agreementTypeId).ToList();
            }

        }

        public static List<onBoardingModule> GetOnBoardingModules()
        {
            using (var context = new OperationsSecureContext())
            {
                return context.onBoardingModules.Where(s => s.CreatedBy != null).ToList();
            }

        }

        public static List<long> ReportList = new List<long>() { 4, 11 };
        public static List<dmaReport> GetAccountReports()
        {
            using (var context = new OperationsContext())
            {
                return context.dmaReports.Where(s => ReportList.Contains(s.dmaReportsId)).ToList();
            }
        }

        public static void AddAccountDescription(string accountDescription, int agreementTypeId)
        {
            using (var context = new OperationsSecureContext())
            {
                var onBoardingAccountDescription = new OnBoardingAccountDescription
                {
                    AccountDescription = accountDescription,
                    dmaAgreementTypeId = agreementTypeId
                };
                context.OnBoardingAccountDescriptions.Add(onBoardingAccountDescription);
                context.SaveChanges();
            }
        }

        public static void AddOnboardingModule(long reportId, string accountModule, string userName)
        {
            using (var context = new OperationsSecureContext())
            {
                var onBoardingModule = new onBoardingModule()
                {
                    dmaReportsId = reportId,
                    ModuleName = accountModule,
                    CreatedBy = userName,
                    CreatedAt = DateTime.Now
                };
                context.onBoardingModules.Add(onBoardingModule);
                context.SaveChanges();
            }
        }

        public static void UpdateAccountStatus(string accountStatus, long accountId, string comments, string userName)
        {
            using (var context = new OperationsSecureContext())
            {
                var account = context.onBoardingAccounts.FirstOrDefault(a => a.onBoardingAccountId == accountId);
                if (account == null)
                    return;

                var existingStatus = account.onBoardingAccountStatus;
                account.onBoardingAccountStatus = accountStatus;
                account.StatusComments = comments;
                account.UpdatedAt = DateTime.Now;
                account.UpdatedBy = accountStatus == "Approved" ? account.UpdatedBy : userName;
                account.ApprovedBy = accountStatus == "Approved" ? userName : null;
                context.SaveChanges();

                //var auditLog = new onBoardingUserAuditLog
                //{
                //    UpdatedAt = DateTime.Now,
                //    CreatedAt = DateTime.Now,
                //    CreatedBy = userName,
                //    UpdatedBy = userName,
                //    Module = "Account",
                //    PreviousStateValue = existingStatus,
                //    ModifiedStateValue = accountStatus,
                //    Action = "Updated",
                //    Field = "Status",
                //    Association = String.Format("Onboarding Name: <i>{0}</i><br/>Account Name: <i>{1}</i>", "Account", account.AccountName)

                //};
                //AuditManager.AddAuditLog(auditLog);
            }
        }

        public static void UpdateAccountMapStatus(string status, long accountMapId, string comments, string userName)
        {
            using (var context = new OperationsSecureContext())
            {
                var accountSsiTemplateMap = context.onBoardingAccountSSITemplateMaps.FirstOrDefault(a => a.onBoardingAccountSSITemplateMapId == accountMapId);
                if (accountSsiTemplateMap == null)
                    return;

                accountSsiTemplateMap.Status = status;
                accountSsiTemplateMap.StatusComments = comments;
                accountSsiTemplateMap.UpdatedAt = DateTime.Now;
                accountSsiTemplateMap.UpdatedBy = userName;
                context.SaveChanges();
            }
        }

        public static long AddAccount(onBoardingAccount account, string userName)
        {
            using (var context = new OperationsSecureContext())
            {
                try
                {
                    if (account.onBoardingAccountId == 0)
                    {
                        account.CreatedAt = DateTime.Now;
                        account.CreatedBy = userName;
                        account.UpdatedAt = DateTime.Now;
                        account.UpdatedBy = userName;
                        account.onBoardingAccountStatus = "Created";
                    }
                    else
                    {
                        account.UpdatedAt = DateTime.Now;
                        account.UpdatedBy = userName;

                        if (account.onBoardingAccountModuleAssociations != null && account.onBoardingAccountModuleAssociations.Count > 0)
                        {
                            var accountToBeDeleted = context.onBoardingAccountModuleAssociations.Where(x => x.onBoardingAccountId == account.onBoardingAccountId).ToList();
                            context.onBoardingAccountModuleAssociations.RemoveRange(accountToBeDeleted);
                            context.onBoardingAccountModuleAssociations.AddRange(account.onBoardingAccountModuleAssociations);

                            //new Repository<onBoardingAccountSSITemplateMap>().BulkInsert(account.onBoardingAccountSSITemplateMaps, dbSchemaName: "HMADMIN.");
                        }

                        if (account.onBoardingAccountSSITemplateMaps != null && account.onBoardingAccountSSITemplateMaps.Count > 0)
                        {
                            var ssiTemplateMapToBeDeleted = context.onBoardingAccountSSITemplateMaps.Where(x => x.onBoardingAccountId == account.onBoardingAccountId).ToList();
                            context.onBoardingAccountSSITemplateMaps.RemoveRange(ssiTemplateMapToBeDeleted);
                            context.onBoardingAccountSSITemplateMaps.AddRange(account.onBoardingAccountSSITemplateMaps);

                            //new Repository<onBoardingAccountSSITemplateMap>().BulkInsert(account.onBoardingAccountSSITemplateMaps, dbSchemaName: "HMADMIN.");
                        }
                    }

                    //if (account.WirePortalCutoffId == 0 && !string.IsNullOrWhiteSpace(account.Currency) && !string.IsNullOrWhiteSpace(account.CashInstruction))
                    //{
                    //    var wirePortalCutoff = new onBoardingWirePortalCutoff()
                    //    {
                    //        CashInstruction = account.CashInstruction,

                    //    };
                    //}

                    if (account.WirePortalCutoffId == 0)
                        account.WirePortalCutoffId = null;


                    account.WirePortalCutoff = null;
                    account.SwiftGroup = null;
                    account.hmsAccountCallbacks = null;
                    account.Beneficiary = null;
                    account.UltimateBeneficiary = null;
                    account.Intermediary = null;
                    context.onBoardingAccounts.AddOrUpdate(s => s.onBoardingAccountId, account);
                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, e);
                    throw;
                }
            }

            return account.onBoardingAccountId;
        }

        public static void AddAccountSsiTemplateMap(List<onBoardingAccountSSITemplateMap> accountSsiTemplateMap, string userName)
        {
            using (var context = new OperationsSecureContext())
            {
                accountSsiTemplateMap.ForEach(aMap =>
                {
                    if (aMap.onBoardingAccountSSITemplateMapId == 0)
                    {
                        aMap.CreatedAt = DateTime.Now;
                        aMap.CreatedBy = userName;
                        aMap.UpdatedAt = DateTime.Now;
                        aMap.UpdatedBy = userName;
                        aMap.Status = "Pending Approval";
                    }
                    else
                    {
                        aMap.UpdatedAt = DateTime.Now;
                        aMap.UpdatedBy = userName;
                    }
                    context.onBoardingAccountSSITemplateMaps.AddOrUpdate(aMap);
                });
                context.SaveChanges();
            }
        }

        public static void DeleteAccount(long onBoardingAccountId, string username)
        {
            using (var context = new OperationsSecureContext())
            {
                var accountToBeDeleted = context.onBoardingAccounts.FirstOrDefault(account => account.onBoardingAccountId == onBoardingAccountId);
                if (accountToBeDeleted == null) return;
                accountToBeDeleted.IsDeleted = true;
                context.SaveChanges();

                //var auditLog = new onBoardingUserAuditLog
                //{
                //    UpdatedAt = DateTime.Now,
                //    CreatedAt = DateTime.Now,
                //    CreatedBy = username,
                //    UpdatedBy = username,
                //    Module = "Account",
                //    PreviousStateValue = "",
                //    ModifiedStateValue = "",
                //    Action = "Deleted",
                //    Field = "",
                //    Association = String.Format("Onboarding Name: <i>{0}</i><br/>Account Name: <i>{1}</i>", "Account", accountToBeDeleted.AccountName)

                //};
                //AuditManager.AddAuditLog(auditLog);
            }

        }

        public static void RemoveAccountDocument(long documentId)
        {
            using (var context = new OperationsSecureContext())
            {
                var document = context.onBoardingAccountDocuments.FirstOrDefault(x => x.onBoardingAccountDocumentId == documentId);
                if (document == null)
                    return;

                var fileName = string.Format("{0}{1}\\{2}", FileSystemManager.OpsSecureAccountsFileUploads, document.onBoardingAccountId, document.FileName);
                var fileinfo = new FileInfo(fileName);

                if (File.Exists(fileinfo.FullName))
                    File.Delete(fileinfo.FullName);

                context.onBoardingAccountDocuments.Remove(document);
                var account = context.onBoardingAccounts.First(s => s.onBoardingAccountId == document.onBoardingAccountId);
                account.onBoardingAccountStatus = "Created";
                account.ApprovedBy = null;
                account.UpdatedAt = DateTime.Now;
                context.onBoardingAccounts.AddOrUpdate(account);
                context.SaveChanges();
            }
        }

        public static void RemoveAccountDocument(long accountId, string fileName)
        {
            using (var context = new OperationsSecureContext())
            {
                var document = context.onBoardingAccountDocuments.FirstOrDefault(x => x.onBoardingAccountId == accountId && x.FileName == fileName);
                if (document == null) return;
                context.onBoardingAccountDocuments.Remove(document);
                var account = context.onBoardingAccounts.First(s => s.onBoardingAccountId == document.onBoardingAccountId);
                account.onBoardingAccountStatus = "Created";
                account.ApprovedBy = null;
                account.UpdatedAt = DateTime.Now;
                context.onBoardingAccounts.AddOrUpdate(account);
                context.SaveChanges();
            }
        }

        public static List<onBoardingAccountDocument> GetAccountDocuments(long accountId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.onBoardingAccountDocuments.Where(x => x.onBoardingAccountId == accountId).ToList();
            }
        }

        public static void AddAccountDocument(onBoardingAccountDocument document)
        {
            using (var context = new OperationsSecureContext())
            {
                context.onBoardingAccountDocuments.Add(document);
                var account = context.onBoardingAccounts.First(s => s.onBoardingAccountId == document.onBoardingAccountId);
                account.onBoardingAccountStatus = "Created";
                account.ApprovedBy = null;
                account.UpdatedAt = DateTime.Now;
                context.onBoardingAccounts.AddOrUpdate(account);
                context.SaveChanges();
            }
        }

        public static List<long> GetFundsOfApprovedAccounts()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.onBoardingAccounts.Where(account => !account.IsDeleted && account.onBoardingAccountStatus == "Approved").Select(s => s.hmFundId).Distinct().ToList();
            }
        }

        public static List<onBoardingSSITemplate> GetAllApprovedSsiTemplates(List<long> counterpartyIds, List<string> messageTypes = null, bool isAll = true, string currency = null)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                if (messageTypes == null)
                    messageTypes = new List<string>();
                return context.onBoardingSSITemplates.Where(template => !template.IsDeleted && template.SSITemplateStatus == "Approved" && (counterpartyIds.Contains(template.TemplateEntityId) || template.SSITemplateType == "Fee/Expense Payment") && (currency == null || template.Currency == currency) && (isAll || messageTypes.Contains(template.MessageType))).ToList();
            }
        }

        public static List<onBoardingAccount> GetAllApprovedAccounts(List<long> hmFundIds, string messageType, string currency, bool isServiceType)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                return (from account in context.onBoardingAccounts
                        join swift in context.hmsSwiftGroups on account.SwiftGroupId equals swift.hmsSwiftGroupId
                        where !account.IsDeleted && account.onBoardingAccountStatus == "Approved" && (isServiceType || hmFundIds.Contains(account.hmFundId)) && (currency == null || account.Currency == currency) && swift.AcceptedMessages.Contains(messageType)
                        select account).ToList();

                //return context.onBoardingAccounts.Include(s => s.SwiftGroup).Where(account => !account.IsDeleted && account.onBoardingAccountStatus == "Approved" && (hmFundIds.Contains(account.hmFundId) || isServiceType) && (currency == null || account.Currency == currency) && ((account.SwiftGroup ?? new hmsSwiftGroup()).AcceptedMessages.Contains(messageType))).ToList();
            }
        }

        public static List<hmsCurrency> GetAllCurrencies()
        {
            using (var context = new OperationsSecureContext())
            {
                return context.hmsCurrencies.AsNoTracking().ToList();
            }
        }

        public static void AddCurrency(string currency)
        {
            using (var context = new OperationsSecureContext())
            {
                var onBoardingCurrency = new hmsCurrency() { Currency = currency };
                context.hmsCurrencies.Add(onBoardingCurrency);
                context.SaveChanges();
            }
        }

        public static List<onBoardingCashInstruction> GetAllCashInstruction()
        {
            using (var context = new OperationsSecureContext())
            {
                return context.onBoardingCashInstructions.AsNoTracking().ToList();
            }
        }

        public static void AddCashInstruction(string cashInstruction)
        {
            using (var context = new OperationsSecureContext())
            {
                var onboardingCashInstruction = new onBoardingCashInstruction { CashInstruction = cashInstruction };
                context.onBoardingCashInstructions.Add(onboardingCashInstruction);
                context.SaveChanges();
            }
        }

        public static List<onBoardingAccountBICorABA> GetAllAccountBicorAba()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.onBoardingAccountBICorABAs.AsNoTracking().Where(s => !s.IsDeleted).ToList();
            }
        }

        public static void AddAccountBiCorAba(onBoardingAccountBICorABA accountBiCorAba)
        {
            using (var context = new OperationsSecureContext())
            {
                context.onBoardingAccountBICorABAs.Add(accountBiCorAba);
                context.SaveChanges();
            }
        }

        public static hmsWirePortalCutoff GetCutoffTime(string cashInstruction, string currency)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                var onBoardingWirePortalCutoff = context.hmsWirePortalCutoffs.AsNoTracking().FirstOrDefault(x => x.CashInstruction == cashInstruction && x.Currency == currency);
                return onBoardingWirePortalCutoff ?? new hmsWirePortalCutoff();
            }
        }


        public static List<onBoardingAuthorizedParty> GetAllAuthorizedParty()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.onBoardingAuthorizedParties.AsNoTracking().ToList();
            }
        }

        public static void AddAuthorizedParty(string authorizedParty, string username)
        {

            using (var context = new OperationsSecureContext())
            {
                var onboardingauthorizedParty = new onBoardingAuthorizedParty
                {
                    AuthorizedParty = authorizedParty,
                    RecCreatedAt = DateTime.Now,
                    RecCreatedBy = username,
                    IsDeleted = false
                };
                context.onBoardingAuthorizedParties.Add(onboardingauthorizedParty);
                context.SaveChanges();
            }

        }

        public static List<hmsSwiftGroup> GetAllSwiftGroup(long brokerId = -1)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsSwiftGroups.Include(s => s.hmsSwiftGroupStatusLkp).Include(s => s.onBoardingAccounts).Where(s => !s.IsDeleted && (brokerId == -1 || s.BrokerLegalEntityId == brokerId)).AsNoTracking().ToList();
            }
        }

        public static List<hmsAccountCallback> GetAccountCallbacks(long accountId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsAccountCallbacks.Where(s => s.onBoardingAccountId == accountId).AsNoTracking().ToList();
            }
        }

        public static hmsAccountCallback GetCallbackData(long callbackId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsAccountCallbacks.First(s => s.hmsAccountCallbackId == callbackId);
            }
        }

        public static void AddOrUpdateCallback(hmsAccountCallback callback)
        {
            using (var context = new OperationsSecureContext())
            {
                context.hmsAccountCallbacks.AddOrUpdate(callback);
                context.SaveChanges();
            }
        }

        public static Dictionary<int, string> GetSwiftGroupStatus()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsSwiftGroupStatusLkps.ToDictionary(s => s.hmsSwiftGroupStatusLkpId, v => v.Status);
            }
        }

        public static bool IsAccountDocumentExists(long accountId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                var document = context.onBoardingAccountDocuments.FirstOrDefault(x => x.onBoardingAccountId == accountId);
                return (document != null);
            }
        }

        private static List<string> TreasuryAgreementTypesToUseMarginExcessOrDeficit
        {
            get { return SystemSwitches.TreasuryReportAgreementTypesToUseMarginExcessOrDeficit; }
        }

        public static CashBalances GetAccountCashBalances(long sendingFundAccountId, DateTime valueDate)
        {
            var contextDate = DateTime.Today.GetContextDate();

            vw_FundAccounts fndAccount;

            //Is PB Account 
            using (var context = new OperationsSecureContext())
            {
                fndAccount = context.vw_FundAccounts.First(s => s.onBoardingAccountId == sendingFundAccountId);
            }

            var cashbalances = fndAccount.AccountType == "Agreement" && TreasuryAgreementTypesToUseMarginExcessOrDeficit.Contains(fndAccount.AgreementType)
                ? ComputePBCashBalances(valueDate, contextDate, fndAccount)
                : ComputeNonPBCashBalances(sendingFundAccountId, valueDate, contextDate);

            if (cashbalances != null)
                cashbalances.HoldBackAmount = fndAccount.HoldbackAmount ?? 0;

            return cashbalances;
        }

        private static CashBalances ComputePBCashBalances(DateTime valueDate, DateTime contextDate, vw_FundAccounts fndAccount)
        {
            List<dmaTreasuryCashBalance> allTreasuryBals;
            using (var context = new OperationsContext())
            {
                allTreasuryBals = context.dmaTreasuryCashBalances.Where(s => TreasuryAgreementTypesToUseMarginExcessOrDeficit.Contains(s.AccountOrAgreementType) && s.ContextDate == contextDate.Date).ToList();
            }

            var allPBForContextDate = allTreasuryBals.Select(s => s.onboardAccountId).ToList();
            dmaTreasuryCashBalance treasuryBal;
            List<WireAccountBaseData> wires;
            using (var context = new OperationsSecureContext())
            {
                var treasuryBalAccId = (from acc in context.vw_FundAccounts
                                        where allPBForContextDate.Contains(acc.onBoardingAccountId)
                                        where acc.AccountNumber == fndAccount.AccountNumber && acc.AccountType == "Agreement" && TreasuryAgreementTypesToUseMarginExcessOrDeficit.Contains(acc.AgreementType)
                                        select acc.onBoardingAccountId).FirstOrDefault();
                treasuryBal = allTreasuryBals.FirstOrDefault(s => s.onboardAccountId == treasuryBalAccId);

                if (treasuryBal == null)
                    return new CashBalances() { IsCashBalanceAvailable = false };

                wires = (from wire in context.hmsWires
                         join acc in context.vw_FundAccounts on wire.OnBoardAccountId equals acc.onBoardingAccountId
                         where acc.AccountNumber == fndAccount.AccountNumber && acc.AccountType == "Agreement" && TreasuryAgreementTypesToUseMarginExcessOrDeficit.Contains(acc.AgreementType)
                         where wire.ValueDate > contextDate && wire.ValueDate <= valueDate && (wire.WireStatusId == (int)WireDataManager.WireStatus.Approved || wire.WireStatusId == (int)WireDataManager.WireStatus.Initiated)
                         where wire.hmsWireMessageType.MessageType != "MT210"
                         select new WireAccountBaseData
                         {
                             OnBoardAccountId = wire.OnBoardAccountId,
                             Amount = wire.Amount,
                             WireStatusId = wire.WireStatusId,
                             ValueDate = wire.ValueDate,
                             Currency = wire.Currency
                         }).ToList();
            }

            // Covert the amount to its Existing Sending account currency

            var allFromCurrency = wires.Select(s => s.Currency).Distinct().ToList();
            if (!allFromCurrency.Contains(treasuryBal.Currency))
                allFromCurrency.Add(treasuryBal.Currency);
            if (!allFromCurrency.Contains(fndAccount.Currency))
                allFromCurrency.Add(fndAccount.Currency);

            List<vw_ProxyCurrencyConversionData> conversionData;
            using (var context = new OperationsContext())
            {
                conversionData = context.vw_ProxyCurrencyConversionData.Where(s =>
                    s.HM_CONTEXT_DT == contextDate && s.TO_CRNCY == fndAccount.Currency &&
                    allFromCurrency.Contains(s.FROM_CRNCY)).ToList();
            }

            var totalWiredInLocalCur = (from wire in wires
                                        let fxRate = conversionData.Where(s => s.FROM_CRNCY == wire.Currency && s.TO_CRNCY == fndAccount.Currency)
                                                         .Select(s => s.FX_RATE).FirstOrDefault() ?? 0
                                        select fxRate == 0 ? wire.Amount : wire.Amount * fxRate).Sum();

            var converForTreasuryBal = conversionData.Where(s => s.FROM_CRNCY == treasuryBal.Currency && s.TO_CRNCY == fndAccount.Currency)
                                           .Select(s => s.FX_RATE).FirstOrDefault() ?? 0;

            var cashBalances = new CashBalances()
            {
                IsCashBalanceAvailable = true,
                TotalWireEntered = totalWiredInLocalCur,
                TreasuryBalance = converForTreasuryBal == 0 ? treasuryBal.CashBalance ?? 0 : (treasuryBal.CashBalance ?? 0) * converForTreasuryBal,
                MarginBuffer = converForTreasuryBal == 0 ? treasuryBal.MarginBuffer ?? 0 : (treasuryBal.MarginBuffer ?? 0) * converForTreasuryBal,
                Currency = converForTreasuryBal == 0 ? treasuryBal.Currency : fndAccount.Currency,
                ContextDate = treasuryBal.ContextDate,
                WireDetails = new List<CashBalances.WiredDetails>()
            };

            var wireDetails = wires.GroupBy(s => s.ValueDate).ToDictionary(s => s.Key, v => v.ToList());

            foreach (var detail in wireDetails)
            {
                cashBalances.WireDetails.Add(new CashBalances.WiredDetails()
                {
                    ValueDate = detail.Key,
                    ApprovedCount = detail.Value.Count(s => s.WireStatusId == (int)WireDataManager.WireStatus.Approved),
                    PendingCount = detail.Value.Count(s => s.WireStatusId == (int)WireDataManager.WireStatus.Initiated),
                    ApprovedWireAmount = (from wire in detail.Value
                                          where wire.WireStatusId == (int)WireDataManager.WireStatus.Approved
                                          let fxRate = conversionData.Where(s => s.FROM_CRNCY == wire.Currency && s.TO_CRNCY == fndAccount.Currency).Select(s => s.FX_RATE).FirstOrDefault() ?? 0
                                          select fxRate == 0 ? wire.Amount : wire.Amount * fxRate).Sum(),

                    PendingWireAmount = (from wire in detail.Value
                                         where wire.WireStatusId == (int)WireDataManager.WireStatus.Initiated
                                         let fxRate = conversionData.Where(s => s.FROM_CRNCY == wire.Currency && s.TO_CRNCY == fndAccount.Currency).Select(s => s.FX_RATE).FirstOrDefault() ?? 0
                                         select fxRate == 0 ? wire.Amount : wire.Amount * fxRate).Sum(),
                });
            }

            return cashBalances;
        }

        private static CashBalances ComputeNonPBCashBalances(long sendingFundAccountId, DateTime valueDate, DateTime contextDate)
        {
            dmaTreasuryCashBalance treasuryBal;
            using (var context = new OperationsContext())
            {
                treasuryBal = context.dmaTreasuryCashBalances.FirstOrDefault(s => s.onboardAccountId == sendingFundAccountId && s.ContextDate == contextDate.Date);
            }

            if (treasuryBal == null)
                return new CashBalances() { IsCashBalanceAvailable = false };

            List<WireAccountBaseData> wires;
            using (var context = new OperationsSecureContext())
            {
                wires = context.hmsWires.Where(s => s.OnBoardAccountId == sendingFundAccountId && s.ValueDate > contextDate && s.ValueDate <= valueDate &&
                                                    (s.WireStatusId == (int)WireDataManager.WireStatus.Approved ||
                                                    s.WireStatusId == (int)WireDataManager.WireStatus.Initiated))
                    .Where(wire => wire.hmsWireMessageType.MessageType != "MT210")
                    .Select(s => new WireAccountBaseData
                    {
                        OnBoardAccountId = s.OnBoardAccountId,
                        Amount = s.Amount,
                        WireStatusId = s.WireStatusId,
                        ValueDate = s.ValueDate,
                        Currency = s.Currency
                    }).ToList();
            }

            var cashBalances = new CashBalances()
            {
                IsCashBalanceAvailable = true,
                TotalWireEntered = wires.Sum(s => s.Amount),
                TreasuryBalance = treasuryBal.CashBalance ?? 0,
                Currency = treasuryBal.Currency,
                ContextDate = treasuryBal.ContextDate,
                WireDetails = new List<CashBalances.WiredDetails>()
            };


            var wireDetails = wires.GroupBy(s => s.ValueDate).ToDictionary(s => s.Key, v => v.ToList());

            foreach (var detail in wireDetails)
            {
                cashBalances.WireDetails.Add(new CashBalances.WiredDetails()
                {
                    ValueDate = detail.Key,
                    ApprovedCount = detail.Value.Count(s => s.WireStatusId == (int)WireDataManager.WireStatus.Approved),
                    PendingCount = detail.Value.Count(s => s.WireStatusId == (int)WireDataManager.WireStatus.Initiated),
                    ApprovedWireAmount = detail.Value.Where(s => s.WireStatusId == (int)WireDataManager.WireStatus.Approved).Sum(s => s.Amount),
                    PendingWireAmount = detail.Value.Where(s => s.WireStatusId == (int)WireDataManager.WireStatus.Initiated).Sum(s => s.Amount)
                });
            }

            return cashBalances;
        }
    }
}
