﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HedgeMark.Operations.Secure.DataModel;
using log4net;

namespace HMOSecureMiddleware
{
    public class AccountManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AccountManager));

        #region Account

        public const int FundTemplateTypeId = 1;
        public const int BrokerTemplateTypeId = 2;
        public const string AgreementAccountType = "Agreement";

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
                    .Include(x => x.onBoardingAccountSSITemplateMaps).Where(x => !x.IsDeleted).Where(s => isPreviledgedUser || hmFundIds.Contains(s.hmFundId)).ToList();
            }

        }

        public static List<onBoardingAccount> GetAllOnBoardingAccounts(long agreementId = 0, long fundId = 0, long brokerId = 0)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                if (agreementId > 0)
                    return context.onBoardingAccounts
                        .Include(s => s.Beneficiary)
                        .Include(s => s.Intermediary)
                        .Include(s => s.UltimateBeneficiary)
                        .Include(s => s.WirePortalCutoff)
                        .Include(s => s.SwiftGroup)
                        .Include(x => x.onBoardingAccountSSITemplateMaps).Include(x => x.onBoardingAccountDocuments)
                        .Where(account => account.dmaAgreementOnBoardingId == agreementId && !account.IsDeleted).ToList();

                return context.onBoardingAccounts
                    .Include(s => s.Beneficiary)
                    .Include(s => s.Intermediary)
                    .Include(s => s.UltimateBeneficiary)
                    .Include(s => s.WirePortalCutoff)
                    .Include(s => s.SwiftGroup).Include(x => x.onBoardingAccountSSITemplateMaps).Include(x => x.onBoardingAccountDocuments)
                    .Where(account => account.BrokerId == brokerId && account.hmFundId == fundId && account.AccountType != AgreementAccountType && !account.IsDeleted).ToList();
            }
        }

        public static onBoardingAccount GetOnBoardingAccount(long accountId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                var account = context.onBoardingAccounts.Include(s => s.Beneficiary)
                    .Include(s => s.Intermediary)
                    .Include(s => s.UltimateBeneficiary)
                    .Include(s => s.WirePortalCutoff)
                    .Include(s => s.SwiftGroup).FirstOrDefault(acnt => acnt.onBoardingAccountId == accountId);

                if (account.WirePortalCutoff == null)
                    account.WirePortalCutoff = new onBoardingWirePortalCutoff() { CutOffTimeZone = "EST" };
                if (account.Beneficiary == null)
                    account.Beneficiary = new onBoardingAccountBICorABA();
                if (account.Intermediary == null)
                    account.Intermediary = new onBoardingAccountBICorABA();
                if (account.UltimateBeneficiary == null && account.UltimateBeneficiaryType != "Account Name")
                    account.UltimateBeneficiary = new onBoardingAccountBICorABA();
                if (account.SwiftGroup == null)
                    account.SwiftGroup = new hmsSwiftGroup();
                //remove circular references
                account.WirePortalCutoff.onBoardingAccounts = null;
                account.Beneficiary.onBoardingAccounts = account.Beneficiary.onBoardingAccounts1 = account.Beneficiary.onBoardingAccounts2 = null;
                account.Intermediary.onBoardingAccounts = account.Intermediary.onBoardingAccounts1 = account.Intermediary.onBoardingAccounts2 = null;
                if (account.UltimateBeneficiary != null)
                    account.UltimateBeneficiary.onBoardingAccounts = account.UltimateBeneficiary.onBoardingAccounts1 = account.UltimateBeneficiary.onBoardingAccounts2 = null;
                account.SwiftGroup.onBoardingAccounts = null;
                if (account.SwiftGroup.hmsSwiftGroupStatusLkp != null)
                    account.SwiftGroup.hmsSwiftGroupStatusLkp.hmsSwiftGroups = null;
                return account;
            }

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

            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to create the Onboarding Account Description", methodName);
            try
            {
                using (var context = new OperationsSecureContext())
                {
                    var onBoardingAccountDescription = new OnBoardingAccountDescription();
                    onBoardingAccountDescription.AccountDescription = accountDescription;
                    onBoardingAccountDescription.dmaAgreementTypeId = agreementTypeId;
                    context.OnBoardingAccountDescriptions.Add(onBoardingAccountDescription);
                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
            }
        }

        public static void AddOnboardingModule(long reportId, string accountModule, string userName)
        {
            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to create the Onboarding Account Module", methodName);
            try
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
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
            }
        }

        public static void UpdateAccountStatus(string accountStatus, long accountId, string comments, string userName)
        {
            using (var context = new OperationsSecureContext())
            {
                var account = context.onBoardingAccounts.FirstOrDefault(a => a.onBoardingAccountId == accountId);
                if (account != null)
                {
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
        }

        public static void UpdateAccountMapStatus(string status, long accountMapId, string comments, string userName)
        {
            using (var context = new OperationsSecureContext())
            {
                var accountSsiTemplateMap = context.onBoardingAccountSSITemplateMaps.FirstOrDefault(a => a.onBoardingAccountSSITemplateMapId == accountMapId);
                if (accountSsiTemplateMap != null)
                {
                    accountSsiTemplateMap.Status = status;
                    accountSsiTemplateMap.StatusComments = comments;
                    accountSsiTemplateMap.UpdatedAt = DateTime.Now;
                    accountSsiTemplateMap.UpdatedBy = userName;
                    context.SaveChanges();
                }
            }
        }

        public static long AddAccount(onBoardingAccount account, string userName)
        {
            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to create the onboarding account", methodName);
            try
            {
                using (var context = new OperationsSecureContext())
                {
                    if (account.onBoardingAccountId == 0)
                    {
                        account.CreatedAt = DateTime.Now;
                        account.CreatedBy = userName;
                        account.UpdatedAt = DateTime.Now;
                        account.UpdatedBy = userName;
                        account.onBoardingAccountStatus = "Created";
                        context.onBoardingAccounts.Add(account);
                    }
                    else
                    {
                        account.UpdatedAt = DateTime.Now;
                        account.UpdatedBy = userName;
                        context.onBoardingAccounts.AddOrUpdate(s => s.onBoardingAccountId, account);
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
                    context.SaveChanges();
                }

            }
            catch (DbEntityValidationException dbex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1} - {2}", methodName, dbex.Message, string.Join(",", dbex.EntityValidationErrors.SelectMany(s => s.ValidationErrors.Select(p => p.PropertyName)).ToList())), dbex);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1} - {2}", methodName, ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""), ex);
            }
            return account.onBoardingAccountId;
        }

        public static void AddAccountSsiTemplateMap(onBoardingAccountSSITemplateMap accountSsiTemplateMap, string userName)
        {

            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to create the onboarding account ssi map", methodName);
            try
            {
                using (var context = new OperationsSecureContext())
                {
                    if (accountSsiTemplateMap.onBoardingAccountSSITemplateMapId == 0)
                    {
                        accountSsiTemplateMap.CreatedAt = DateTime.Now;
                        accountSsiTemplateMap.CreatedBy = userName;
                        accountSsiTemplateMap.UpdatedAt = DateTime.Now;
                        accountSsiTemplateMap.UpdatedBy = userName;
                        accountSsiTemplateMap.Status = "Pending Approval";
                        context.onBoardingAccountSSITemplateMaps.Add(accountSsiTemplateMap);
                    }
                    else
                    {
                        accountSsiTemplateMap.UpdatedAt = DateTime.Now;
                        accountSsiTemplateMap.UpdatedBy = userName;
                        context.onBoardingAccountSSITemplateMaps.AddOrUpdate(accountSsiTemplateMap);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
            }
        }

        public static void DeleteAccount(long onBoardingAccountId, string username)
        {

            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to delete the onboarding account", methodName);
            try
            {
                using (var context = new OperationsSecureContext())
                {
                    var accountToBeDeleted = context.onBoardingAccounts.FirstOrDefault(account => account.onBoardingAccountId == onBoardingAccountId);
                    if (accountToBeDeleted != null)
                    {
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
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
            }
        }

        public static void RemoveAccountDocument(long documentId)
        {

            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to delete the onboarding account document", methodName);
            try
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
                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
            }
        }

        public static void RemoveAccountDocument(long accountId, string fileName)
        {

            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to delete the onboarding account document", methodName);
            try
            {
                using (var context = new OperationsSecureContext())
                {
                    var document = context.onBoardingAccountDocuments.FirstOrDefault(x => x.onBoardingAccountId == accountId && x.FileName == fileName);
                    if (document != null)
                    {
                        context.onBoardingAccountDocuments.Remove(document);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
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

            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to add the onboarding account document", methodName);
            try
            {
                using (var context = new OperationsSecureContext())
                {
                    context.onBoardingAccountDocuments.Add(document);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
            }
        }

        public static Dictionary<long, string> GetAllSsiTemplates()
        {
            using (var context = new OperationsSecureContext())
            {
                return context.onBoardingSSITemplates.Where(f => !f.IsDeleted).ToDictionary(s => s.onBoardingSSITemplateId, s => s.TemplateName);
            }
        }

        public static List<onBoardingSSITemplate> GetAllSsiTemplates(int templateTypeId, long templateEntityId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.onBoardingSSITemplates.Include(x => x.onBoardingSSITemplateDocuments).Where(template => template.TemplateTypeId == templateTypeId && template.TemplateEntityId == templateEntityId && !template.IsDeleted).ToList();
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

        public static List<onBoardingSSITemplate> GetAllApprovedSsiTemplates(List<long> counterpartyIds, List<string> messageTypes = null,bool isAll = true, string currency = null)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.onBoardingSSITemplates.Where(template => !template.IsDeleted && template.SSITemplateStatus == "Approved" && (counterpartyIds.Contains(template.TemplateEntityId) || template.SSITemplateType == "Fee/Expense Payment") && (currency == null || template.Currency == currency) && (isAll || messageTypes.Contains(template.MessageType))).ToList();
            }
        }

        public static List<onBoardingSSITemplate> GetAllBrokerSsiTemplates()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.onBoardingSSITemplates.Where(template => template.TemplateTypeId == BrokerTemplateTypeId && !template.IsDeleted).ToList();
            }
        }

        public static onBoardingSSITemplate GetSsiTemplate(long templateId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.onBoardingSSITemplates.Include(x => x.onBoardingSSITemplateDocuments).FirstOrDefault(template => template.onBoardingSSITemplateId == templateId);
            }
        }

        public static List<OnBoardingSSITemplateServiceProvider> GetAllServiceProviderList()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.OnBoardingSSITemplateServiceProviders.ToList();
            }
        }

        public static void RemoveSsiTemplateMap(long ssiTemplateMapId)
        {
            using (var context = new OperationsSecureContext())
            {
                var ssiTemplateMap = context.onBoardingAccountSSITemplateMaps.FirstOrDefault(x => x.onBoardingAccountSSITemplateMapId == ssiTemplateMapId);
                context.onBoardingAccountSSITemplateMaps.Remove(ssiTemplateMap);
                context.SaveChanges();
            }
        }

        public static List<OnBoardingSSITemplateAccountType> GetAllSsiTemplateAccountTypes(int? agreementTypeId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.OnBoardingSSITemplateAccountTypes.Where(x => x.dmaAgreementTypeId == agreementTypeId).ToList();
            }
        }

        public static List<OnBoardingSSITemplateServiceProvider> GetAllSsiTemplateServiceProviders(string serviceProviderName)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.OnBoardingSSITemplateServiceProviders.Where(x => x.ServiceProvider == serviceProviderName).ToList();
            }
        }

        public static void AddServiceProvider(string serviceProviderName)
        {
            using (var context = new OperationsSecureContext())
            {
                var onboardingServiceProvider = new OnBoardingSSITemplateServiceProvider() { Reason = "Vendor Expenses", ServiceProvider = serviceProviderName };
                context.OnBoardingSSITemplateServiceProviders.Add(onboardingServiceProvider);
                context.SaveChanges();
            }
        }

        public static long AddSsiTemplate(onBoardingSSITemplate ssiTemplate, string userName)
        {

            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to create the onboarding SSI Template", methodName);
            try
            {
                using (var context = new OperationsSecureContext())
                {
                    ssiTemplate.dmaAgreementTypeId = !string.IsNullOrEmpty(ssiTemplate.ServiceProvider) ? 1 : ssiTemplate.dmaAgreementTypeId;

                    if (ssiTemplate.onBoardingSSITemplateId == 0)
                    {
                        ssiTemplate.CreatedAt = DateTime.Now;
                        ssiTemplate.CreatedBy = userName;
                        ssiTemplate.UpdatedAt = DateTime.Now;
                        ssiTemplate.UpdatedBy = userName;
                        ssiTemplate.SSITemplateStatus = "Saved As Draft";
                        context.onBoardingSSITemplates.Add(ssiTemplate);
                    }
                    else
                    {
                        ssiTemplate.UpdatedAt = DateTime.Now;
                        ssiTemplate.UpdatedBy = userName;
                        context.onBoardingSSITemplates.AddOrUpdate(ssiTemplate);

                        if (ssiTemplate.onBoardingSSITemplateDocuments != null && ssiTemplate.onBoardingSSITemplateDocuments.Count > 0)
                        {
                            context.onBoardingSSITemplateDocuments.AddRange(ssiTemplate.onBoardingSSITemplateDocuments.Where(s => s.onBoardingSSITemplateDocumentId == 0));
                            //new Repository<onBoardingSSITemplateDocument>().BulkInsert(, dbSchemaName: "HMADMIN.");
                        }
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
            }
            return ssiTemplate.onBoardingSSITemplateId;
        }

        public static void AddPaymentOrReceiptReasonDetails(string reason, string templateType, int? agreementTypeId, string serviceProviderName)
        {

            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to create the Onboarding SSI Template Reason", methodName);
            try
            {
                using (var context = new OperationsSecureContext())
                {
                    if (templateType == "Broker")
                    {
                        var onBoardingSsiTemplateAccountType = new OnBoardingSSITemplateAccountType();
                        onBoardingSsiTemplateAccountType.Reason = reason;
                        onBoardingSsiTemplateAccountType.dmaAgreementTypeId = agreementTypeId ?? 0;
                        context.OnBoardingSSITemplateAccountTypes.Add(onBoardingSsiTemplateAccountType);
                    }
                    else
                    {
                        var onBoardingSsiTemplateServiceProvider = new OnBoardingSSITemplateServiceProvider();
                        onBoardingSsiTemplateServiceProvider.Reason = reason;
                        onBoardingSsiTemplateServiceProvider.ServiceProvider = serviceProviderName;
                        context.OnBoardingSSITemplateServiceProviders.Add(onBoardingSsiTemplateServiceProvider);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
            }
        }

        public static void DeleteSsiTemplate(long ssiTemplateId, string username)
        {

            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to delete the onboarding SSI Template", methodName);
            try
            {
                using (var context = new OperationsSecureContext())
                {
                    var ssiTemplateToBeDeleted = context.onBoardingSSITemplates.FirstOrDefault(template => template.onBoardingSSITemplateId == ssiTemplateId);
                    if (ssiTemplateToBeDeleted != null)
                    {
                        ssiTemplateToBeDeleted.IsDeleted = true;
                        context.SaveChanges();
                        //var auditLog = new onBoardingUserAuditLog
                        //{
                        //    UpdatedAt = DateTime.Now,
                        //    CreatedAt = DateTime.Now,
                        //    CreatedBy = username,
                        //    UpdatedBy = username,
                        //    Module = "SSITemplate",
                        //    PreviousStateValue = "",
                        //    ModifiedStateValue = "",
                        //    Action = "Deleted",
                        //    Field = "",
                        //    Association = String.Format("Onboarding Name: <i>{0}</i><br/>SSI Template Name: <i>{1}</i>", "SSITemplate", ssiTemplateToBeDeleted.TemplateName)

                        //};
                        //AuditManager.AddAuditLog(auditLog);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
            }
        }

        public static void UpdateSsiTemplateStatus(string ssiTemplateStatus, long ssiTemplateId, string comments, string userName)
        {
            using (var context = new OperationsSecureContext())
            {
                var ssiTemplate = context.onBoardingSSITemplates.AsNoTracking().FirstOrDefault(template => template.onBoardingSSITemplateId == ssiTemplateId);
                if (ssiTemplate != null)
                {
                    var existingStatus = ssiTemplate.SSITemplateStatus;
                    ssiTemplate.SSITemplateStatus = ssiTemplateStatus;
                    ssiTemplate.StatusComments = comments;
                    ssiTemplate.UpdatedAt = DateTime.Now;
                    ssiTemplate.UpdatedBy = ssiTemplateStatus == "Approved" ? ssiTemplate.UpdatedBy : userName;
                    ssiTemplate.ApprovedBy = ssiTemplateStatus == "Approved" ? userName : null;
                    context.onBoardingSSITemplates.AddOrUpdate(ssiTemplate);
                    context.SaveChanges();

                    //var auditLog = new onBoardingUserAuditLog
                    //{
                    //    UpdatedAt = DateTime.Now,
                    //    CreatedAt = DateTime.Now,
                    //    CreatedBy = userName,
                    //    UpdatedBy = userName,
                    //    Module = "SSITemplate",
                    //    PreviousStateValue = existingStatus,
                    //    ModifiedStateValue = ssiTemplateStatus,
                    //    Action = "Updated",
                    //    Field = "Status",
                    //    Association = String.Format("Onboarding Name: <i>{0}</i><br/>Template Name: <i>{1}</i>", "SSITemplate", ssiTemplate.TemplateName)

                    //};
                    //AuditManager.AddAuditLog(auditLog);
                }
            }
        }

        public static void RemoveSsiTemplateDocument(long documentId)
        {
            using (var context = new OperationsSecureContext())
            {
                var document = context.onBoardingSSITemplateDocuments.FirstOrDefault(x => x.onBoardingSSITemplateDocumentId == documentId);
                if (document != null)
                {
                    var fileName = string.Format("{0}{1}\\{2}", FileSystemManager.OpsSecureSSITemplateFileUploads, document.onBoardingSSITemplateId, document.FileName);
                    var fileinfo = new FileInfo(fileName);

                    if (File.Exists(fileinfo.FullName))
                        File.Delete(fileinfo.FullName);

                    context.onBoardingSSITemplateDocuments.Remove(document);
                    context.SaveChanges();
                }
            }
        }

        public static List<onBoardingCurrency> GetAllCurrencies()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.onBoardingCurrencies.AsNoTracking().ToList();
            }
        }

        public static void AddCurrency(string currency)
        {

            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to create the Onboarding Currency", methodName);
            try
            {
                using (var context = new OperationsSecureContext())
                {
                    var onBoardingCurrency = new onBoardingCurrency();
                    onBoardingCurrency.Currency = currency;
                    context.onBoardingCurrencies.Add(onBoardingCurrency);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
            }
        }

        public static List<onBoardingCashInstruction> GetAllCashInstruction()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.onBoardingCashInstructions.AsNoTracking().ToList();
            }
        }

        public static void AddCashInstruction(string cashInstruction)
        {

            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to create the Onboarding Cash Instruction", methodName);
            try
            {
                using (var context = new OperationsSecureContext())
                {
                    var onboardingCashInstruction = new onBoardingCashInstruction();
                    onboardingCashInstruction.CashInstruction = cashInstruction;
                    context.onBoardingCashInstructions.Add(onboardingCashInstruction);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : { 1}", methodName, ex.Message), ex);
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

            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to create the Onboarding Account BIC or ABA", methodName);
            try
            {
                using (var context = new OperationsSecureContext())
                {
                    context.onBoardingAccountBICorABAs.Add(accountBiCorAba);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
            }
        }

        public static onBoardingWirePortalCutoff GetCutoffTime(string cashInstruction, string currency)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                var onBoardingWirePortalCutoff = context.onBoardingWirePortalCutoffs.AsNoTracking().FirstOrDefault(x => x.CashInstruction == cashInstruction && x.Currency == currency);
                return onBoardingWirePortalCutoff ?? new onBoardingWirePortalCutoff();
            }
        }

        public static void RemoveSsiTemplateDocument(long ssiTemplateId, string fileName)
        {
            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to delete the onboarding ssi template document", methodName);
            try
            {
                using (var context = new OperationsSecureContext())
                {
                    var document = context.onBoardingSSITemplateDocuments.FirstOrDefault(x => x.onBoardingSSITemplateId == ssiTemplateId && x.FileName == fileName);
                    if (document != null)
                    {
                        context.onBoardingSSITemplateDocuments.Remove(document);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
            }
        }

        public static void AddSsiTemplateDocument(onBoardingSSITemplateDocument document)
        {

            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to add the onboarding ssi template document", methodName);
            try
            {
                using (var context = new OperationsSecureContext())
                {
                    context.onBoardingSSITemplateDocuments.Add(document);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
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

            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to create the Onboarding Authorized Party", methodName);
            try
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
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
            }
        }

        public static List<hmsSwiftGroup> GetAllSwiftGroup(long brokerId = -1)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsSwiftGroups.Where(s => !s.IsDeleted && (brokerId == -1 || s.BrokerLegalEntityId == brokerId)).AsNoTracking().ToList();
            }
        }

        public static List<hmsSwiftGroupStatusLkp> GetSwiftGroupStatus()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsSwiftGroupStatusLkps.ToList();
            }
        }

        public static void AddOrUpdateSwiftGroup(hmsSwiftGroup hmsSwiftGroup)
        {

            var methodName = MethodBase.GetCurrentMethod();
            Logger.InfoFormat("{0} - calling to create the Onboarding Swift Group", methodName);
            try
            {
                using (var context = new OperationsSecureContext())
                {
                    hmsSwiftGroup.RecCreatedAt = DateTime.Now;
                    context.hmsSwiftGroups.AddOrUpdate(hmsSwiftGroup);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message), ex);
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
        public static bool IsSsiTemplateDocumentExists(long ssiTemplateId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                var document = context.onBoardingSSITemplateDocuments.FirstOrDefault(x => x.onBoardingSSITemplateId == ssiTemplateId);
                return (document != null);
            }
        }

        #endregion
    }
}
