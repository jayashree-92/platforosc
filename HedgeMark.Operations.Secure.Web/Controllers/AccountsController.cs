using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Com.HedgeMark.Commons.Extensions;
using ExcelUtility.Operations.ManagedAccounts;
using HedgeMark.Operations.FileParseEngine.Models;
using HedgeMark.Operations.Secure.DataModel;
using HMOSecureMiddleware;
using HMOSecureMiddleware.Util;
using HMOSecureWeb.Utility;
using Newtonsoft.Json;

namespace HMOSecureWeb.Controllers
{
    public class AccountsController : BaseController
    {
        
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetAccountPreloadData()
        {
            using (var context = new OperationsContext())
            {
                var hFundIds = AuthorizedSessionData.HMFundIds.Where(s => s.Level > 0).Select(s => s.Id).ToList();
                var intFundIds = AdminFundManager.GetUniversalDMAFundListQuery(context, PreferredFundNameInSession)
                    .Where(s => AuthorizedSessionData.IsPrivilegedUser || hFundIds.Contains(s.hmFundId)).OrderBy(s => s.PreferredFundName)
                    .Select(s => s.hmFundId).ToList();
                var hmFundIds = intFundIds.Select(s => Convert.ToInt64(s)).ToList();
                var hFunds = AdminFundManager.GetHFundsCreatedForDMA(hmFundIds, PreferredFundNameInSession);
                var funds = hFunds.Select(s => new { id = s.HFundId, text = s.PerferredFundName, LegalName = s.LegalFundName });
                var agreementData = OnBoardingDataManager.GetAgreementsForOnboardingAccountPreloadData(hFundIds, AuthorizedSessionData.IsPrivilegedUser);
                var counterpartyFamilies = OnBoardingDataManager.GetAllCounterpartyFamilies().Select(x => new { id = x.dmaCounterpartyFamilyId, text = x.CounterpartyFamily }).OrderBy(x => x.text).ToList();
                var agreementFundIds = agreementData.Where(s => s.HMFundId > 0).Select(s => s.HMFundId).ToList();
                var fundsWithAgreements = funds.Where(s => agreementFundIds.Contains(s.id)).ToList();
                var agreements = agreementData.Select(s => new { id = s.AgreementOnboardingId, text = s.AgreementShortName, AgreementTypeId = s.AgreementTypeId, hmFundId = s.HMFundId, BrokerId = s.BrokerId }).ToList();
                return Json(new
                {
                    agreementData,
                    agreements,
                    funds,
                    fundsWithAgreements,
                    counterpartyFamilies
                });
            }
            
        }

        public JsonResult GetAccountAssociationPreloadData()
        {
            var hmFundIds = AuthorizedSessionData.HMFundIds.Where(s => s.Level > 0).Select(s => s.Id).ToList();

            using (var context = new OperationsContext())
            {
                var funds = AdminFundManager
                    .GetUniversalDMAFundListQuery(context, PreferencesManager.FundNameInDropDown.LegalFundName)
                    .Where(s => AuthorizedSessionData.IsPrivilegedUser || hmFundIds.Contains(s.hmFundId))
                    .OrderBy(s => s.PreferredFundName).Select(x => new { id = x.hmFundId, text = x.PreferredFundName }).ToList();

                return Json(new
                {
                    funds
                });
            }

        }

        public JsonResult GetAccountDescriptionsByAgreementTypeId(long agreementTypeId)
        {
            var accountDescriptionChoices = AccountManager.GetAccountDescriptionsByAgreementTypeId(agreementTypeId);
            return Json(new
            {
                accountDescriptions = accountDescriptionChoices.Select(choice => new
                {
                    id = choice.AccountDescription,
                    text = choice.AccountDescription
                }).OrderBy(x => x.text).ToList()
            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetAccountModules()
        {
            var accountModules = AccountManager.GetOnBoardingModules();
            return Json(new
            {
                accountModules = accountModules.Select(choice => new
                {
                    id = choice.onBoardingModuleId,
                    text = choice.ModuleName,
                    report = choice.dmaReportsId == 4 ? "Collateral" : "Invoices"
                }).OrderBy(x => x.text).ToList()
            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetAccountReports()
        {
            var accountReports = AccountManager.GetAccountReports();
            return Json(new
            {
                accountReports = accountReports.Select(choice => new
                {
                    id = choice.dmaReportsId,
                    text = choice.ReportName
                }).OrderBy(x => x.text).ToList()
            }, JsonContentType, JsonContentEncoding);
        }

        public void AddAccountDescriptions(string accountDescription, int agreementTypeId)
        {
            AccountManager.AddAccountDescription(accountDescription, agreementTypeId);
        }

        public void AddAccountModule(long reportId, string accountModule)
        {
            AccountManager.AddOnboardingModule(reportId, accountModule, UserName);
        }

        private string GetContactName(string lastname, string firstname, string email)
        {
            var contactName = string.Empty;

            if (string.IsNullOrWhiteSpace(lastname) && string.IsNullOrWhiteSpace(firstname))
                return email.HumanizeEmail();

            if (string.IsNullOrWhiteSpace(lastname) && !string.IsNullOrWhiteSpace(firstname))
                contactName = firstname;
            else if (!string.IsNullOrWhiteSpace(lastname) && string.IsNullOrWhiteSpace(firstname))
                contactName = lastname;
            else if (!string.IsNullOrWhiteSpace(lastname) && !string.IsNullOrWhiteSpace(firstname))
                contactName = string.Format("{0} {1}", lastname, firstname);

            return contactName;

        }

        public JsonResult GetOnBoardingAccount(long accountId)
        {
            var onBoardingAccount = AccountManager.GetOnBoardingAccount(accountId);

            return Json(new
            {
                OnBoardingAccount = onBoardingAccount,
                isAuthorizedUserToApprove = (User.IsWireApprover() && onBoardingAccount.onBoardingAccountStatus == "Pending Approval" && onBoardingAccount.CreatedBy != UserName && onBoardingAccount.UpdatedBy != UserName)

            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetAccountSsiTemplateMap(long accountId, long fundId, string currency, string messages)
        {
            var ssiTemplateMaps = AccountManager.GetAccountSsiTemplateMap(accountId);

            var counterpartyIds = OnBoardingDataManager.GetCounterpartyIdsbyFund(fundId);

            if (string.IsNullOrWhiteSpace(messages))
                messages = string.Empty;

            var ssiTemplates = AccountManager.GetAllApprovedSsiTemplates(counterpartyIds, messages.Split(',').ToList(), false, currency);

            ssiTemplateMaps.ForEach(x => x.onBoardingSSITemplate = null);

            return Json(new
            {
                ssiTemplateMaps = ssiTemplateMaps.Select(ssi =>
                {
                    var onBoardingSsiTemplate = ssiTemplates.FirstOrDefault(template => template.onBoardingSSITemplateId == ssi.onBoardingSSITemplateId);
                    return onBoardingSsiTemplate != null ? new
                    {
                        ssi.onBoardingAccountSSITemplateMapId,
                        ssi.onBoardingAccountId,
                        ssi.onBoardingSSITemplateId,
                        ssi.FFCName,
                        ssi.FFCNumber,
                        ssi.Reference,
                        ssi.CreatedAt,
                        ssi.CreatedBy,
                        ssi.UpdatedAt,
                        ssi.UpdatedBy,
                        ssi.Status,
                        ssi.StatusComments,
                        onBoardingSsiTemplate.SSITemplateType,
                        onBoardingSsiTemplate.TemplateName,
                        onBoardingSsiTemplate.AccountNumber
                    } : null;
                }).Where(temp => temp != null).OrderBy(y => y.TemplateName).ToList(),
                ssiTemplates = ssiTemplates.Where(s => !ssiTemplateMaps.Select(p => p.onBoardingSSITemplateId).Contains(s.onBoardingSSITemplateId)).ToList(),
            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetAccountDocuments(long accountId)
        {
            var accountDocuments = AccountManager.GetAccountDocuments(accountId);
            return Json(new
            {
                accountDocuments
            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetAllOnBoardingAccount()
        {
            var hmFundIds = AuthorizedSessionData.HMFundIds.Where(s => s.Level > 0).Select(s => s.Id).ToList();
            var onBoardingAccounts = AccountManager.GetAllOnBoardingAccounts(hmFundIds, AuthorizedSessionData.IsPrivilegedUser);

            var accountTypes = OnBoardingDataManager.GetAllAgreementTypes();

            Dictionary<long, string> counterparties;
            Dictionary<long, AgreementBaseData> agreements;
            var allAgreementIds = onBoardingAccounts.Select(s => s.dmaAgreementOnBoardingId).Distinct().ToList();
            var allCounterpartyFamilyIds = onBoardingAccounts.Select(s => s.BrokerId).Distinct().ToList();

            using (var context = new AdminContext())
            {
                var agreementList = context.vw_OnboardedAgreements.AsNoTracking()
                    .Where(s => allAgreementIds.Contains(s.dmaAgreementOnBoardingId)).OrderBy(s => s.hmFundId ?? 0)
                    .Select(s => new AgreementBaseData { AgreementOnboardingId = s.dmaAgreementOnBoardingId, AgreementShortName = s.AgreementShortName, AgreementTypeId = (int)s.AgreementTypeId }).ToList();

                agreements = new Dictionary<long, AgreementBaseData>();
                foreach (var data in agreementList.Where(data => !agreements.ContainsKey(data.AgreementOnboardingId)))
                {
                    agreements.Add(data.AgreementOnboardingId, data);
                }

                counterparties = context.dmaCounterpartyFamilies.AsNoTracking().Where(s => allCounterpartyFamilyIds.Contains(s.dmaCounterpartyFamilyId)).ToDictionary(s => s.dmaCounterpartyFamilyId, v => v.CounterpartyFamily);
            }
            var receivingAccountTypes = PreferencesManager.GetSystemPreference(PreferencesManager.SystemPreferences.ReceivingAgreementTypesForAccount).Split(',').ToList();
            var funds = AdminFundManager.GetHFundsCreatedForDMA(hmFundIds, AuthorizedSessionData.IsPrivilegedUser, PreferencesManager.FundNameInDropDown.LegalFundName);

            return Json(new
            {
                accountTypes = accountTypes.Select(x => new { id = x.Key, text = x.Value }).OrderBy(x => x.text).ToList(),
                receivingAccountTypes,
                OnBoardingAccounts = onBoardingAccounts.Select(x => new
                {
                    AgreementName = x.dmaAgreementOnBoardingId != null && agreements.ContainsKey((long)x.dmaAgreementOnBoardingId) ? agreements[(long)x.dmaAgreementOnBoardingId].AgreementShortName : string.Empty,

                    AgreementTypeId = x.dmaAgreementOnBoardingId != null && agreements.ContainsKey((long)x.dmaAgreementOnBoardingId) ? agreements[(long)x.dmaAgreementOnBoardingId].AgreementTypeId :
                                      (accountTypes.ContainsValue(x.AccountType) ? accountTypes.FirstOrDefault(y => y.Value == x.AccountType).Key : 0),

                    Broker = x.BrokerId != null && counterparties.ContainsKey((long)x.BrokerId) ? counterparties[(long)x.BrokerId] : string.Empty,
                    FundName = funds.ContainsKey((int)x.hmFundId) ? funds[(int)x.hmFundId] : string.Empty,
                    ApprovedMaps = x.onBoardingAccountSSITemplateMaps.Count(s => s.Status == "Approved"),
                    PendingApprovalMaps = x.onBoardingAccountSSITemplateMaps.Count(s => s.Status == "Pending Approval"),
                    x.onBoardingAccountId,
                    x.dmaAgreementOnBoardingId,
                    x.AccountType,
                    x.BrokerId,
                    CutoffTime = x.WirePortalCutoff != null ? x.WirePortalCutoff.CutoffTime : new TimeSpan(),
                    DaystoWire = x.WirePortalCutoff != null ? x.WirePortalCutoff.DaystoWire : 0,
                    x.AccountName,
                    x.AccountNumber,
                    x.AuthorizedParty,
                    x.CashInstruction,
                    x.CashSweep,
                    x.CashSweepTime,
                    x.CashSweepTimeZone,
                    x.ContactEmail,
                    x.ContactName,
                    x.Currency,
                    x.ContactNumber,
                    x.ContactType,
                    x.Description,
                    x.AccountModule,
                    x.CreatedAt,
                    x.CreatedBy,
                    x.Notes,
                    x.UpdatedAt,
                    x.UpdatedBy,
                    x.ApprovedBy,
                    x.BeneficiaryType,
                    BeneficiaryBICorABA = x.Beneficiary != null ? x.Beneficiary.BICorABA : string.Empty,
                    BeneficiaryBankName = x.Beneficiary != null ? x.Beneficiary.BankName : string.Empty,
                    BeneficiaryBankAddress = x.Beneficiary != null ? x.Beneficiary.BankAddress : string.Empty,
                    x.BeneficiaryAccountNumber,
                    x.IntermediaryType,
                    IntermediaryBICorABA = x.Intermediary != null ? x.Intermediary.BICorABA : string.Empty,
                    IntermediaryBankName = x.Intermediary != null ? x.Intermediary.BankName : string.Empty,
                    IntermediaryBankAddress = x.Intermediary != null ? x.Intermediary.BankAddress : string.Empty,
                    x.IntermediaryAccountNumber,
                    x.UltimateBeneficiaryType,
                    UltimateBeneficiaryBICorABA = x.UltimateBeneficiary != null ? x.UltimateBeneficiary.BICorABA : string.Empty,
                    UltimateBeneficiaryBankName = x.UltimateBeneficiary != null ? x.UltimateBeneficiary.BankName : string.Empty,
                    UltimateBeneficiaryBankAddress = x.UltimateBeneficiary != null ? x.UltimateBeneficiary.BankAddress : string.Empty,
                    x.UltimateBeneficiaryAccountName,
                    x.FFCName,
                    x.FFCNumber,
                    x.Reference,
                    x.onBoardingAccountStatus,
                    x.hmFundId,
                    SendersBIC = x.SwiftGroup != null ? x.SwiftGroup.SendersBIC : string.Empty,
                    x.StatusComments,
                    SwiftGroup = x.SwiftGroup != null ? x.SwiftGroup.SwiftGroup : string.Empty,
                    x.AccountPurpose,
                    x.AccountStatus,
                    x.HoldbackAmount,
                    x.SweepComments,
                    x.AssociatedCustodyAcct,
                    x.PortfolioSelection,
                    x.TickerorISIN,
                    x.SweepCurrency
                }).ToList()
            });
        }

        public void AddAccounts(List<onBoardingAccount> onBoardingAccounts, string fundName = "", string agreement = "", string broker = "")
        {
            foreach (var account in onBoardingAccounts)
            {
                var ssiTemplates = account.onBoardingAccountSSITemplateMaps.ToList();

                if (ssiTemplates.Count > 0)
                {
                    ssiTemplates.RemoveAll(x => x.onBoardingSSITemplateId == 0);

                    if (account.onBoardingAccountId > 0)
                        ssiTemplates.ForEach(x =>
                        {
                            x.onBoardingAccountId = account.onBoardingAccountId;
                            x.UpdatedAt = DateTime.Now;
                            x.UpdatedBy = UserName;
                            x.CreatedAt = DateTime.Now;
                            x.CreatedBy = UserName;
                        });
                    else
                        ssiTemplates.ForEach(x =>
                        {
                            x.onBoardingAccountId = 0;
                            x.UpdatedAt = DateTime.Now;
                            x.UpdatedBy = UserName;
                            x.CreatedAt = DateTime.Now;
                            x.CreatedBy = UserName;
                        });

                    account.onBoardingAccountSSITemplateMaps = ssiTemplates;
                }
                if (!string.IsNullOrEmpty(account.AccountModule))
                {
                    var onboardModuleAssociations = account.AccountModule.Split(',').Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.ToLong()).Select(module => new onBoardingAccountModuleAssociation()
                        {
                            onBoardingAccountId = account.onBoardingAccountId,
                            onBoardingModuleId = module,
                            CreatedAt = DateTime.Now,
                            CreatedBy = UserName
                        }).ToList();
                    account.onBoardingAccountModuleAssociations = onboardModuleAssociations;
                }

                var auditLogList = AuditManager.GetAuditLogs(account, fundName, agreement, broker, UserName);

                var accountId = AccountManager.AddAccount(account, UserName);

                if (accountId > 0)
                {
                    AuditManager.Log(auditLogList);
                }
            }

        }

        public JsonResult GetAllOnBoardingAccountContacts(long entityId)
        {
            var onBoardingContacts = OnBoardingDataManager.GetAllOnBoardingContacts(ContactManager.CounterpartyTypeId, entityId);
            return Json(new
            {
                OnBoardingContacts = onBoardingContacts.Select(contact => new
                {
                    id = contact.dmaOnBoardingContactDetailId,
                    contact.Email,
                    contact.ContactType,
                    JobTitle = (!string.IsNullOrWhiteSpace(contact.JobTitle)) ? contact.JobTitle.Trim(',') : string.Empty,
                    contact.Notes,
                    wires = contact.Wires ? "Yes" : "No",
                    margin = contact.Margin ? "Yes" : "No",
                    cash = contact.Cash ? "Yes" : "No",
                    collateral = contact.Collateral ? "Yes" : "No",
                    Interest = contact.InterestRate ? "Yes" : "No",
                    contact.BusinessPhone,
                    name = GetContactName(contact.LastName, contact.FirstName, contact.Email)
                }).OrderBy(x => x.name).ToList()
            }, JsonContentType, JsonContentEncoding);
        }

        public void DeleteAccount(long onBoardingAccountId)
        {
            AccountManager.DeleteAccount(onBoardingAccountId, UserName);
        }

        public JsonResult GetAllCurrencies()
        {
            var currenciesChoices = AccountManager.GetAllCurrencies();
            return Json(new
            {
                currencies = currenciesChoices.Select(choice => new
                {
                    id = choice.Currency,
                    text = choice.Currency
                }).OrderBy(x => x.text).ToList()
            }, JsonContentType, JsonContentEncoding);
        }

        public void AddCurrency(string currency)
        {
            AccountManager.AddCurrency(currency);
        }

        public JsonResult GetAllCashInstruction()
        {
            var cashInstructionChoices = AccountManager.GetAllCashInstruction();
            return Json(new
            {
                cashInstructions = cashInstructionChoices.Select(choice => new
                {
                    id = choice.CashInstruction,
                    text = choice.CashInstruction
                }).OrderBy(x => x.text).ToList(),
                timeZones = FileSystemManager.GetAllTimeZones().Select(s => new
                {
                    id = s.Key,
                    text = s.Key
                }).ToList(),
            }, JsonContentType, JsonContentEncoding);
        }

        public void AddCashInstruction(string cashInstruction)
        {
            AccountManager.AddCashInstruction(cashInstruction);
        }

        public JsonResult GetAllAuthorizedParty()
        {
            var authorizedParties = AccountManager.GetAllAuthorizedParty();
            return Json(new
            {
                AuthorizedParties = authorizedParties.Select(choice => new
                {
                    id = choice.AuthorizedParty,
                    text = choice.AuthorizedParty
                }).OrderBy(x => x.text).ToList()
            }, JsonContentType, JsonContentEncoding);
        }

        public void AddAuthorizedParty(string authorizedParty)
        {
            AccountManager.AddAuthorizedParty(authorizedParty, UserName);
        }

        public JsonResult GetAllRelatedSwiftGroup(long brokerId)
        {
            var swiftGroups = AccountManager.GetAllSwiftGroup(brokerId);
            return Json(new
            {
                swiftGroups,
                SwiftGroupData = swiftGroups.Select(choice => new
                {
                    id = choice.hmsSwiftGroupId,
                    text = choice.SwiftGroup
                }).OrderBy(x => x.text).ToList()
            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetAllAccountBicorAba()
        {
            var accountBicorAba = AccountManager.GetAllAccountBicorAba();
            return Json(new
            {
                accountBicorAba
            }, JsonContentType, JsonContentEncoding);
        }

        public void AddAccountBiCorAba(onBoardingAccountBICorABA accountBiCorAba)
        {
            accountBiCorAba.IsDeleted = false;
            accountBiCorAba.UpdatedAt = DateTime.Now;
            accountBiCorAba.CreatedAt = DateTime.Now;
            accountBiCorAba.CreatedBy = UserName;
            accountBiCorAba.UpdatedBy = UserName;

            AccountManager.AddAccountBiCorAba(accountBiCorAba);
        }

        public void UpdateAccountStatus(string accountStatus, long accountId, string comments)
        {
            AccountManager.UpdateAccountStatus(accountStatus, accountId, comments, UserName);
        }

        public void UpdateAccountMapStatus(string status, long accountMapId, string comments)
        {
            AccountManager.UpdateAccountMapStatus(status, accountMapId, comments, UserName);
        }

        public void RemoveSsiTemplateMap(long ssiTemplateMapId)
        {
            AccountManager.RemoveSsiTemplateMap(ssiTemplateMapId);
        }

        public JsonResult GetCutoffTime(string cashInstruction, string currency)
        {
            var cutOffTime = AccountManager.GetCutoffTime(cashInstruction, currency);
            return Json(new
            {
                cutOffTime
            }, JsonContentType, JsonContentEncoding);
        }

        public void AddAccountSsiTemplateMap(onBoardingAccountSSITemplateMap accountSsiTemplateMap)
        {
            AccountManager.AddAccountSsiTemplateMap(accountSsiTemplateMap, UserName);
        }

        public void RemoveAccountDocument(string fileName, long documentId)
        {
            //var fileinfo = new FileInfo(FileSystemManager.OnboardingAccountFilesPath + fileName);

            //if (System.IO.File.Exists(fileinfo.FullName))
            //    System.IO.File.Delete(fileinfo.FullName);
            if (documentId > 0)
                AccountManager.RemoveAccountDocument(documentId);
        }

        public bool IsAccountDocumentExists(long accountId)
        {
            return AccountManager.IsAccountDocumentExists(accountId);
        }

        
        #region Export and Upload

        public FileResult ExportAllAccountlist()
        {

            var hmFundIds = AuthorizedSessionData.HMFundIds.Where(s => s.Level > 0).Select(s => s.Id).ToList();
            var onBoardingAccounts = AccountManager.GetAllOnBoardingAccounts(hmFundIds, AuthorizedSessionData.IsPrivilegedUser).OrderByDescending(x => x.UpdatedAt).ToList();
            var contentToExport = new Dictionary<string, List<Row>>();
            var accountListRows = BuildAccountRows(onBoardingAccounts);
            //File name and path
            var fileName = string.Format("AccountList_{0:yyyyMMdd}", DateTime.Now);
            var exportFileInfo = new FileInfo(string.Format("{0}{1}{2}", FileSystemManager.UploadTemporaryFilesPath, fileName, DefaultExportFileFormat));
            contentToExport.Add("List of Accounts", accountListRows);
            //Export the checklist file
            Exporter.CreateExcelFile(contentToExport, exportFileInfo.FullName, true);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        //Build Account Rows
        private List<Row> BuildAccountRows(List<onBoardingAccount> onBoardingAccounts)
        {
            var accountListRows = new List<Row>();

            Dictionary<long, string> counterparties;
            Dictionary<int, string> funds;
            Dictionary<long, AgreementBaseData> agreements;
            var allAgreementIds = onBoardingAccounts.Select(s => s.dmaAgreementOnBoardingId).Distinct().ToList();
            var allCounterpartyFamilyIds = onBoardingAccounts.Select(s => s.BrokerId).Distinct().ToList();
            var allFundIds = onBoardingAccounts.Select(s => s.hmFundId).Distinct().ToList();

            using (var context = new AdminContext())
            {
                agreements = context.vw_OnboardedAgreements.AsNoTracking()
                    .Where(s => allAgreementIds.Contains(s.dmaAgreementOnBoardingId))
                    .Select(s => new AgreementBaseData { AgreementOnboardingId = s.dmaAgreementOnBoardingId, AgreementShortName = s.AgreementShortName, AgreementTypeId = (int)s.AgreementTypeId })
                    .ToDictionary(s => s.AgreementOnboardingId, v => v);

                counterparties = context.dmaCounterpartyFamilies.AsNoTracking().Where(s => allCounterpartyFamilyIds.Contains(s.dmaCounterpartyFamilyId)).ToDictionary(s => s.dmaCounterpartyFamilyId, v => v.CounterpartyFamily);

                funds = context.vw_HFund.AsNoTracking()
                    .Where(s => AuthorizedSessionData.IsPrivilegedUser || allFundIds.Contains(s.intFundID))
                    .ToDictionary(x => x.intFundID, v => v.LegalFundName);
            }

            foreach (var account in onBoardingAccounts)
            {
                var row = new Row();
                row["Account Id"] = account.onBoardingAccountId.ToString();
                row["Entity Type"] = account.AccountType;
                row["Fund Name"] = funds.ContainsKey((int)account.hmFundId) ? funds[(int)account.hmFundId] : string.Empty;
                row["Agreement Name"] = account.dmaAgreementOnBoardingId != null && agreements.ContainsKey((long)account.dmaAgreementOnBoardingId) ? agreements[(long)account.dmaAgreementOnBoardingId].AgreementShortName : string.Empty;
                row["Broker"] = account.BrokerId != null && counterparties.ContainsKey((long)account.BrokerId) ? counterparties[(long)account.BrokerId] : string.Empty;
                row["Account Name"] = account.AccountName;
                row["Account Number"] = account.AccountNumber;
                row["Account Type"] = account.AccountPurpose;
                row["Account Status"] = account.AccountStatus;
                row["Currency"] = account.Currency;
                row["Description"] = account.Description;
                row["Notes"] = account.Notes;
                row["Authorized Party"] = account.AuthorizedParty;
                row["Cash Instruction Mechanism"] = account.CashInstruction;
                row["Swift Group"] = account.SwiftGroup != null ? account.SwiftGroup.SwiftGroup : string.Empty;
                row["Senders BIC"] = account.SwiftGroup != null ? account.SwiftGroup.SendersBIC : string.Empty;
                row["Cash Sweep"] = account.CashSweep;

                if (account.CashSweepTime != null)
                {
                    var dateTime = DateTime.Today.AddHours(account.CashSweepTime.Value.Hours).AddMinutes(account.CashSweepTime.Value.Minutes);
                    var stringTime = dateTime.ToString("hh:mm tt");

                    row["Cash Sweep Time"] = stringTime;
                }
                else
                {
                    row["Cash Sweep Time"] = string.Empty;
                }

                row["Cash Sweep Time Zone"] = account.CashSweepTimeZone;

                if (account.WirePortalCutoff != null && account.WirePortalCutoff.CutoffTime != null)
                {
                    var dateTime = DateTime.Today.AddHours(account.WirePortalCutoff.CutoffTime.Hours).AddMinutes(account.WirePortalCutoff.CutoffTime.Minutes);
                    var stringTime = dateTime.ToString("hh:mm tt");

                    row["Cutoff Time"] = stringTime;
                }
                else
                {
                    row["Cutoff Time"] = string.Empty;
                }

                row["Days to wire per V.D"] = account.WirePortalCutoff != null ? account.WirePortalCutoff.DaystoWire.ToString() + (account.WirePortalCutoff.DaystoWire > 1 ? " Days" : " Day") : string.Empty;

                row["Holdback Amount"] = account.HoldbackAmount.HasValue ? account.HoldbackAmount.ToString() : string.Empty;
                row["Sweep Comments"] = account.SweepComments;
                row["Associated Custody Acct"] = account.AssociatedCustodyAcct;
                row["Associated Custody Acct Number"] = account.AssociatedCustodyAcctNumber;
                row["Portfolio Selection"] = account.PortfolioSelection;
                row["Ticker/ISIN"] = account.TickerorISIN;
                row["Sweep Currency"] = account.SweepCurrency;

                row["Contact Type"] = account.ContactType;
                //row["Contact Name"] = account.ContactName;
                //row["Contact Email"] = account.ContactEmail;
                //row["Contact Number"] = account.ContactNumber;
                row["Beneficiary Type"] = account.BeneficiaryType;
                row["Beneficiary BIC or ABA"] = account.Beneficiary != null ? account.Beneficiary.BICorABA : string.Empty;
                row["Beneficiary Bank Name"] = account.Beneficiary != null ? account.Beneficiary.BankName : string.Empty;
                row["Beneficiary Bank Address"] = account.Beneficiary != null ? account.Beneficiary.BankAddress : string.Empty;
                row["Beneficiary Account Number"] = account.Beneficiary != null ? account.BeneficiaryAccountNumber : string.Empty;
                row["Intermediary Beneficiary Type"] = account.IntermediaryType;
                row["Intermediary BIC or ABA"] = account.Intermediary != null ? account.Intermediary.BICorABA : string.Empty;
                row["Intermediary Bank Name"] = account.Intermediary != null ? account.Intermediary.BankName : string.Empty;
                row["Intermediary Bank Address"] = account.Intermediary != null ? account.Intermediary.BankAddress : string.Empty;
                row["Intermediary Account Number"] = account.IntermediaryAccountNumber;
                row["Ultimate Beneficiary Type"] = account.UltimateBeneficiaryType;
                row["Ultimate Beneficiary BIC or ABA"] = account.UltimateBeneficiary != null ? account.UltimateBeneficiary.BICorABA : string.Empty;
                row["Ultimate Beneficiary Bank Name"] = account.UltimateBeneficiary != null ? account.UltimateBeneficiary.BankName : string.Empty;
                row["Ultimate Beneficiary Bank Address"] = account.UltimateBeneficiary != null ? account.UltimateBeneficiary.BankAddress : string.Empty;
                row["Ultimate Beneficiary Account Name"] = account.UltimateBeneficiaryAccountName;
                row["FFC Name"] = account.FFCName;
                row["FFC Number"] = account.FFCNumber;
                row["Reference"] = account.Reference;
                row["Status"] = account.onBoardingAccountStatus;
                row["Comments"] = account.StatusComments;
                row["CreatedBy"] = account.CreatedBy;
                row["CreatedDate"] = account.CreatedAt + "";
                row["UpdatedBy"] = account.UpdatedBy;
                row["ModifiedDate"] = account.UpdatedAt + "";
                row["ApprovedBy"] = account.ApprovedBy;
                switch (account.onBoardingAccountStatus)
                {
                    case "Approved":
                        row.RowHighlight = Row.Highlight.Success;
                        break;
                    case "Pending Approval":
                        row.RowHighlight = Row.Highlight.Warning;
                        break;
                    default:
                        row.RowHighlight = Row.Highlight.None;
                        break;
                }
                accountListRows.Add(row);
            }

            return accountListRows;
        }

        public string UploadAccount()
        {
            var onboardingAccounts = AccountManager.GetAllOnBoardingAccounts(new List<long>(), true);
            var counterpartyFamilies = OnBoardingDataManager.GetAllCounterpartyFamilies().ToDictionary(x => x.dmaCounterpartyFamilyId, x => x.CounterpartyFamily);
            var funds = AdminFundManager.GetHFundsCreatedForDMA(PreferencesManager.FundNameInDropDown.LegalFundName);
            var agreements = OnBoardingDataManager.GetAllAgreements();
            var accountBicorAba = AccountManager.GetAllAccountBicorAba();
            var swiftgroups = AccountManager.GetAllSwiftGroup();

            var bulkUploadLogs = new List<hmsBulkUploadLog>();
            for (var i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];

                if (file == null)
                    throw new Exception("unable to retrieve file information");

                var fileInfo = new FileInfo(string.Format("{0}\\{1}\\{2:yyyy-MM-dd}\\{3}", FileSystemManager.OpsSecureBulkFileUploads, "FundAccount", DateTime.Now, file.FileName));

                if (fileInfo.Directory != null && !Directory.Exists(fileInfo.Directory.FullName))
                    Directory.CreateDirectory(fileInfo.Directory.FullName);

                var newFileName = file.FileName;
                var splitFileNames = file.FileName.Split('.');
                var ind = 1;
                while (System.IO.File.Exists(fileInfo.FullName))
                {
                    newFileName = string.Format("{0}_{1}.{2}", splitFileNames[0], ind++, splitFileNames[1]);
                    fileInfo = new FileInfo(string.Format("{0}\\{1}\\{2:yyyy-MM-dd}\\{3}", FileSystemManager.OpsSecureBulkFileUploads, "FundAccount", DateTime.Now, newFileName));
                }

                file.SaveAs(fileInfo.FullName);

                var accountRows = new Parser().ParseAsRows(fileInfo, "List of Accounts", string.Empty, true);

                if (accountRows.Count > 0)
                {
                    foreach (var account in accountRows)
                    {
                        var accountDetail = new onBoardingAccount
                        {
                            Beneficiary = new onBoardingAccountBICorABA(),
                            Intermediary = new onBoardingAccountBICorABA(),
                            UltimateBeneficiary = new onBoardingAccountBICorABA(),
                            WirePortalCutoff = new onBoardingWirePortalCutoff(),
                            SwiftGroup = new hmsSwiftGroup(),
                            onBoardingAccountId = string.IsNullOrWhiteSpace(account["Account Id"]) ? 0 : long.Parse(account["Account Id"]),
                            AccountNumber = account["Account Number"],
                            AccountType = account["Entity Type"]
                        };


                        if (account["Entity Type"] == "Agreement")
                        {
                            accountDetail.dmaAgreementOnBoardingId = agreements.FirstOrDefault(x => x.Value == account["Agreement Name"]).Key;
                            accountDetail.hmFundId = funds.FirstOrDefault(x => x.Value == account["Fund Name"]).Key;
                            var counterPartyByAgreement = onboardingAccounts.FirstOrDefault(s => s.dmaAgreementOnBoardingId == accountDetail.dmaAgreementOnBoardingId);
                            if (counterPartyByAgreement != null)
                            {
                                accountDetail.BrokerId = counterPartyByAgreement.BrokerId;
                            }

                            if (accountDetail.onBoardingAccountId == 0)
                            {
                                var existsAccount = onboardingAccounts.FirstOrDefault(x =>
                                    x.dmaAgreementOnBoardingId == accountDetail.dmaAgreementOnBoardingId &&
                                    x.hmFundId == accountDetail.hmFundId &&
                                    x.AccountNumber == accountDetail.AccountNumber);
                                if (existsAccount != null) continue;
                            }
                        }
                        else
                        {
                            accountDetail.hmFundId = funds.FirstOrDefault(x => x.Value == account["Fund Name"]).Key;
                            accountDetail.BrokerId = counterpartyFamilies.FirstOrDefault(x => x.Value == account["Broker"]).Key;
                            if (accountDetail.onBoardingAccountId == 0)
                            {
                                var existsAccount = onboardingAccounts.FirstOrDefault(x => x.hmFundId == accountDetail.hmFundId &&
                                    x.BrokerId == accountDetail.BrokerId && x.AccountNumber == accountDetail.AccountNumber);
                                if (existsAccount != null) continue;
                            }
                        }
                        accountDetail.AccountName = account["Account Name"];
                        accountDetail.AccountPurpose = account["Account Type"];
                        accountDetail.AccountStatus = account["Account Status"];
                        accountDetail.Currency = account["Currency"];
                        accountDetail.Description = account["Description"];
                        accountDetail.Notes = account["Notes"];
                        accountDetail.AuthorizedParty = account["Authorized Party"];
                        accountDetail.CashInstruction = account["Cash Instruction Mechanism"];
                        accountDetail.SwiftGroup.SwiftGroup = account["Swift Group"];
                        if (!string.IsNullOrWhiteSpace(account["Swift Group"]))
                        {
                            var swiftGroup = swiftgroups.FirstOrDefault(x => x.SwiftGroup == account["Swift Group"]);
                            accountDetail.SwiftGroup.SendersBIC = (swiftGroup != null) ? swiftGroup.SendersBIC : string.Empty;
                        }

                        accountDetail.CashSweep = account["Cash Sweep"];

                        if (!string.IsNullOrWhiteSpace(account["Cash Sweep Time"]))
                        {
                            var cashSweepTime = account["Cash Sweep Time"];
                            var increment = (cashSweepTime.Contains("PM") ? 12 : 0);
                            cashSweepTime = cashSweepTime.Replace("PM", "").Replace("AM", "").Trim();
                            var splitCashSweep = cashSweepTime.Split(':');
                            if (splitCashSweep.Length > 1)
                            {
                                var hours = Convert.ToInt32(splitCashSweep[0]);
                                hours = hours >= 12 ? hours % 12 : hours;
                                cashSweepTime = (hours + increment).ToString() + ":" + splitCashSweep[1];
                                accountDetail.CashSweepTime = TimeSpan.Parse(cashSweepTime);
                            }
                            accountDetail.CashSweepTimeZone = account["Cash Sweep Time Zone"];
                        }

                        if (!string.IsNullOrWhiteSpace(account["Cutoff Time"]))
                        {
                            var cutoffTime = account["Cutoff Time"];
                            var increment = (cutoffTime.Contains("PM") ? 12 : 0);
                            cutoffTime = cutoffTime.Replace("PM", "").Replace("AM", "").Trim();
                            var splitCutoffTime = cutoffTime.Split(':');
                            if (splitCutoffTime.Length > 1)
                            {
                                var hours = Convert.ToInt32(splitCutoffTime[0]);
                                hours = hours >= 12 ? hours % 12 : hours;
                                cutoffTime = (hours + increment).ToString() + ":" + splitCutoffTime[1];
                                accountDetail.WirePortalCutoff.CutoffTime = TimeSpan.Parse(cutoffTime);
                            }
                        }
                        accountDetail.WirePortalCutoff.CutOffTimeZone = string.IsNullOrWhiteSpace(account["Cutoff Time Zone"]) ? "EST" : account["Cutoff Time Zone"];
                        if (!string.IsNullOrWhiteSpace(account["Days to wire per V.D"]))
                        {
                            var wirePerDays = account["Days to wire per V.D"].Replace(" Days", "").Replace(" Day", "").Trim();
                            accountDetail.WirePortalCutoff.DaystoWire = Convert.ToInt32(wirePerDays);
                        }

                        if (!string.IsNullOrWhiteSpace(account["Cash Sweep"]) && account["Cash Sweep"] == "Yes")
                        {
                            if (!string.IsNullOrWhiteSpace(account["Holdback Amount"]))
                                accountDetail.HoldbackAmount = Convert.ToDouble(account["Holdback Amount"]);

                            accountDetail.SweepComments = account["Sweep Comments"];
                            accountDetail.AssociatedCustodyAcct = account["Associated Custody Acct"];
                            accountDetail.AssociatedCustodyAcctNumber = account["Associated Custody Acct Number"];
                            accountDetail.PortfolioSelection = account["Portfolio Selection"];
                            accountDetail.TickerorISIN = account["Ticker/ISIN"];
                            accountDetail.SweepCurrency = account["Sweep Currency"];
                        }

                        accountDetail.ContactType = account["Contact Type"];

                        //if (accountDetail.BrokerId > 0 && accountDetail.BrokerId.HasValue)
                        //{
                        //    var onBoardingContacts = ContactManager.GetAllOnBoardingContacts(ContactManager.CounterpartyTypeId, accountDetail.BrokerId.ToLong());
                        //    var contactDetail = onBoardingContacts.FirstOrDefault(x => (GetContactName(x.LastName, x.FirstName) == account["Contact Name"]));
                        //    if (contactDetail != null)
                        //    {
                        //        accountDetail.ContactName = GetContactName(contactDetail.LastName, contactDetail.FirstName);
                        //        //accountDetail.ContactEmail = contactDetail.Email;
                        //       // accountDetail.ContactNumber = contactDetail.BusinessPhone;
                        //    }
                        //}
                        accountDetail.BeneficiaryType = account["Beneficiary Type"];
                        accountDetail.Beneficiary.BICorABA = account["Beneficiary BIC or ABA"];
                        var beneficiaryBiCorAba = accountBicorAba.FirstOrDefault(x => x.IsABA == (accountDetail.BeneficiaryType == "ABA") && x.BICorABA == accountDetail.Beneficiary.BICorABA);
                        if (beneficiaryBiCorAba != null)
                        {
                            accountDetail.Beneficiary.onBoardingAccountBICorABAId = beneficiaryBiCorAba.onBoardingAccountBICorABAId;
                            accountDetail.Beneficiary.BankName = beneficiaryBiCorAba.BankName;
                            accountDetail.Beneficiary.BankAddress = beneficiaryBiCorAba.BankAddress;
                        }
                        else
                        {
                            accountDetail.Beneficiary.BankName = string.Empty;
                            accountDetail.Beneficiary.BankAddress = string.Empty;
                            accountDetail.Beneficiary.BICorABA = string.Empty;
                            accountDetail.BeneficiaryType = string.Empty;
                        }

                        accountDetail.BeneficiaryAccountNumber = account["Beneficiary Account Number"];
                        accountDetail.IntermediaryType = account["Intermediary Beneficiary Type"];
                        accountDetail.Intermediary.BICorABA = account["Intermediary BIC or ABA"];
                        var intermediaryBiCorAba = accountBicorAba.FirstOrDefault(x => x.IsABA == (accountDetail.IntermediaryType == "ABA") && x.BICorABA == accountDetail.Intermediary.BICorABA);
                        if (intermediaryBiCorAba != null)
                        {
                            accountDetail.Intermediary.onBoardingAccountBICorABAId = intermediaryBiCorAba.onBoardingAccountBICorABAId;
                            accountDetail.Intermediary.BankName = intermediaryBiCorAba.BankName;
                            accountDetail.Intermediary.BankAddress = intermediaryBiCorAba.BankAddress;
                        }
                        else
                        {
                            accountDetail.Intermediary.BankName = string.Empty;
                            accountDetail.Intermediary.BankAddress = string.Empty;
                            accountDetail.Intermediary.BICorABA = string.Empty;
                            accountDetail.IntermediaryType = string.Empty;
                        }

                        accountDetail.IntermediaryAccountNumber = account["Intermediary Account Number"];
                        accountDetail.UltimateBeneficiaryAccountName = account["Ultimate Beneficiary Account Name"];
                        accountDetail.UltimateBeneficiaryType = account["Ultimate Beneficiary Type"];
                        accountDetail.UltimateBeneficiary.BICorABA = account["Ultimate Beneficiary BIC or ABA"];
                        var ultimateBeneficiaryBiCorAba = accountBicorAba.FirstOrDefault(x => x.IsABA == (accountDetail.UltimateBeneficiaryType == "ABA") && x.BICorABA == accountDetail.UltimateBeneficiary.BICorABA);
                        if (ultimateBeneficiaryBiCorAba != null && accountDetail.UltimateBeneficiaryType != "Account Name")
                        {
                            accountDetail.UltimateBeneficiary.onBoardingAccountBICorABAId = ultimateBeneficiaryBiCorAba.onBoardingAccountBICorABAId;
                            accountDetail.UltimateBeneficiary.BankName = ultimateBeneficiaryBiCorAba.BankName;
                            accountDetail.UltimateBeneficiary.BankAddress = ultimateBeneficiaryBiCorAba.BankAddress;
                            accountDetail.UltimateBeneficiaryAccountName = string.Empty;
                        }
                        else
                        {
                            accountDetail.UltimateBeneficiary.BankName = string.Empty;
                            accountDetail.UltimateBeneficiary.BankAddress = string.Empty;
                            accountDetail.UltimateBeneficiary.BICorABA = string.Empty;
                        }

                        accountDetail.FFCName = account["FFC Name"];
                        accountDetail.FFCNumber = account["FFC Number"];
                        accountDetail.Reference = account["Reference"];
                        accountDetail.onBoardingAccountStatus = account["Status"];
                        accountDetail.StatusComments = account["Comments"];
                        accountDetail.CreatedBy = account["CreatedBy"];
                        accountDetail.UpdatedBy = account["UpdatedBy"];
                        accountDetail.ApprovedBy = account["ApprovedBy"];
                        accountDetail.CreatedAt = !string.IsNullOrWhiteSpace(account["CreatedDate"])
                            ? DateTime.Parse(account["CreatedDate"])
                            : DateTime.Now;
                        accountDetail.UpdatedAt = !string.IsNullOrWhiteSpace(account["ModifiedDate"])
                            ? DateTime.Parse(account["ModifiedDate"])
                            : DateTime.Now;
                        accountDetail.IsDeleted = false;

                        if (accountDetail.hmFundId != 0)
                            AccountManager.AddAccount(accountDetail, UserName);
                    }
                }
                bulkUploadLogs.Add(new hmsBulkUploadLog() { FileName = newFileName, IsFundAccountLog = true, UserName = UserName });
            }

            AuditManager.AddBulkUploadLogs(bulkUploadLogs);
            return "";
        }

        public FileResult ExportSampleAccountlist()
        {
            var contentToExport = new Dictionary<string, List<Row>>();

            var row = new Row();
            //row["Account Id"] = String.Empty;
            row["Entity Type"] = String.Empty;
            row["Fund Name"] = String.Empty;
            row["Agreement Name"] = String.Empty;
            row["Broker"] = String.Empty;
            row["Account Name"] = String.Empty;
            row["Account Number"] = String.Empty;
            row["Account Type"] = String.Empty;
            row["Account Status"] = String.Empty;
            row["Currency"] = String.Empty;
            row["Description"] = String.Empty;
            row["Notes"] = String.Empty;
            row["Authorized Party"] = String.Empty;
            row["Cash Instruction Mechanism"] = String.Empty;
            row["Swift Group"] = String.Empty;
            row["Senders BIC"] = String.Empty;
            row["Cash Sweep"] = String.Empty;
            row["Cash Sweep Time"] = String.Empty;
            row["Cash Sweep Time Zone"] = String.Empty;
            row["Cutoff Time Zone"] = string.Empty;
            row["Cutoff Time"] = String.Empty;
            row["Days to wire per V.D"] = String.Empty;
            row["Holdback Amount"] = String.Empty;
            row["Sweep Comments"] = String.Empty;
            row["Associated Custody Acct"] = String.Empty;
            row["Associated Custody Acct Number"] = String.Empty;
            row["Portfolio Selection"] = String.Empty;
            row["Ticker/ISIN"] = String.Empty;
            row["Sweep Currency"] = String.Empty;
            row["Contact Type"] = String.Empty;
            //row["Contact Name"] = String.Empty;
            //row["Contact Email"] = String.Empty;
            // row["Contact Number"] = String.Empty;
            row["Beneficiary Type"] = "ABA/BIC";
            row["Beneficiary BIC or ABA"] = String.Empty;
            row["Beneficiary Account Number"] = String.Empty;
            row["Intermediary Beneficiary Type"] = "ABA/BIC";
            row["Intermediary BIC or ABA"] = String.Empty;
            row["Intermediary Account Number"] = String.Empty;
            row["Ultimate Beneficiary Account Name"] = String.Empty;
            row["Ultimate Beneficiary Type"] = "ABA/BIC";
            row["Ultimate Beneficiary BIC or ABA"] = String.Empty;
            row["FFC Name"] = String.Empty;
            row["FFC Number"] = String.Empty;
            row["Reference"] = String.Empty;
            row["Status"] = String.Empty;
            row["Comments"] = String.Empty;

            row["CreatedBy"] = String.Empty;
            row["CreatedDate"] = String.Empty;
            row["UpdatedBy"] = String.Empty;
            row["ModifiedDate"] = String.Empty;
            row["ApprovedBy"] = string.Empty;

            var accountListRows = new List<Row> { row };

            //File name and path
            var fileName = string.Format("{0}_{1:yyyyMMdd}", "AccountList", DateTime.Now);
            var exportFileInfo = new FileInfo(string.Format("{0}{1}{2}", FileSystemManager.UploadTemporaryFilesPath, fileName, DefaultExportFileFormat));
            contentToExport.Add("List of Accounts", accountListRows);

            //Export the account file
            Exporter.CreateExcelFile(contentToExport, exportFileInfo.FullName, true);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        public string UploadSsiTemplate()
        {
            var onboardingSsiTemplate = AccountManager.GetAllBrokerSsiTemplates();
            var counterParties = OnBoardingDataManager.GetAllOnBoardedCounterparties();
            var agreementTypes = OnBoardingDataManager.GetAllAgreementTypes();
            var accountBicorAba = AccountManager.GetAllAccountBicorAba();
            var messageTypes = new List<string> { "MT103", "MT202", "MT202 COV" };
            var bulkUploadLogs = new List<hmsBulkUploadLog>();
            for (var i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];

                if (file == null)
                    throw new Exception("unable to retrive file information");

                var fileInfo = new FileInfo(string.Format("{0}\\{1}\\{2}\\{3}", FileSystemManager.OpsSecureBulkFileUploads, "SSITemplate", DateTime.Now.ToString("yyyy-MM-dd"), file.FileName));

                if (fileInfo.Directory != null && !Directory.Exists(fileInfo.Directory.FullName))
                    Directory.CreateDirectory(fileInfo.Directory.FullName);

                var newFileName = file.FileName;
                var splitFileNames = file.FileName.Split('.');
                var ind = 1;
                while (System.IO.File.Exists(fileInfo.FullName))
                {
                    newFileName = string.Format("{0}_{1}.{2}", splitFileNames[0], ind++, splitFileNames[1]);
                    fileInfo = new FileInfo(string.Format("{0}\\{1}\\{2}\\{3}", FileSystemManager.OpsSecureBulkFileUploads, "FundAccount", DateTime.Now.ToString("yyyy-MM-dd"), newFileName));
                }

                file.SaveAs(fileInfo.FullName);
                var templateListRows = new Parser().ParseAsRows(fileInfo, "List of SSI Template", string.Empty, true);

                if (templateListRows.Count > 0)
                {
                    foreach (var template in templateListRows)
                    {
                        var templateDetail = new onBoardingSSITemplate();

                        templateDetail.Beneficiary = new onBoardingAccountBICorABA();
                        templateDetail.Intermediary = new onBoardingAccountBICorABA();
                        templateDetail.UltimateBeneficiary = new onBoardingAccountBICorABA();

                        templateDetail.onBoardingSSITemplateId = string.IsNullOrWhiteSpace(template["SSI Template Id"]) ? 0 : long.Parse(template["SSI Template Id"]);
                        templateDetail.TemplateTypeId = AccountManager.BrokerTemplateTypeId;
                        templateDetail.TemplateEntityId = counterParties.FirstOrDefault(x => x.Value == template["Legal Entity"]).Key;
                        templateDetail.dmaAgreementTypeId = agreementTypes.FirstOrDefault(x => x.Value == template["Account Type"]).Key;
                        templateDetail.ServiceProvider = template["Service Provider"];
                        templateDetail.Currency = template["Currency"];
                        templateDetail.ReasonDetail = template["Payment/Receipt Reason Detail"];
                        templateDetail.OtherReason = template["Other Reason"];
                        templateDetail.MessageType = messageTypes.Contains(template["Message Type"].Trim()) ? template["Message Type"].Trim() : string.Empty;
                        templateDetail.TemplateName = template["SSI Template Type"] == "Broker" ? template["Legal Entity"] + " - " + template["Account Type"] + " - " + templateDetail.Currency + " - " + template["Payment/Receipt Reason Detail"] : (!string.IsNullOrWhiteSpace(template["SSI Template Type"]) ? template["Service Provider"] + " - " + templateDetail.Currency + " - " + template["Payment/Receipt Reason Detail"] : template["Template Name"]);
                        if (templateDetail.onBoardingSSITemplateId == 0)
                        {
                            var existsTemplate = onboardingSsiTemplate.FirstOrDefault(x => x.TemplateName == templateDetail.TemplateName);
                            if (existsTemplate != null) continue;
                        }
                        templateDetail.BeneficiaryType = template["Beneficiary Type"];


                        templateDetail.Beneficiary.BICorABA = template["Beneficiary BIC or ABA"];
                        var beneficiaryBiCorAba = accountBicorAba.FirstOrDefault(x => x.IsABA == (templateDetail.BeneficiaryType == "ABA") && x.BICorABA == templateDetail.Beneficiary.BICorABA);
                        if (beneficiaryBiCorAba != null)
                        {
                            templateDetail.Beneficiary.onBoardingAccountBICorABAId = beneficiaryBiCorAba.onBoardingAccountBICorABAId;
                            templateDetail.Beneficiary.BankName = beneficiaryBiCorAba.BankName;
                            templateDetail.Beneficiary.BankAddress = beneficiaryBiCorAba.BankAddress;
                        }
                        else
                        {
                            templateDetail.Beneficiary.BankName = string.Empty;
                            templateDetail.Beneficiary.BankAddress = string.Empty;
                            templateDetail.Beneficiary.BICorABA = string.Empty;
                            templateDetail.BeneficiaryType = string.Empty;
                        }

                        templateDetail.BeneficiaryAccountNumber = template["Beneficiary Account Number"];
                        templateDetail.IntermediaryType = template["Intermediary Beneficiary Type"];
                        templateDetail.Intermediary.BICorABA = template["Intermediary BIC or ABA"];
                        var intermediaryBiCorAba = accountBicorAba.FirstOrDefault(x => x.IsABA == (templateDetail.IntermediaryType == "ABA") && x.BICorABA == templateDetail.Intermediary.BICorABA);
                        if (intermediaryBiCorAba != null)
                        {
                            templateDetail.Intermediary.onBoardingAccountBICorABAId = intermediaryBiCorAba.onBoardingAccountBICorABAId;
                            templateDetail.Intermediary.BankName = intermediaryBiCorAba.BankName;
                            templateDetail.Intermediary.BankAddress = intermediaryBiCorAba.BankAddress;
                        }
                        else
                        {
                            templateDetail.Intermediary.BankName = string.Empty;
                            templateDetail.Intermediary.BankAddress = string.Empty;
                            templateDetail.Intermediary.BICorABA = string.Empty;
                            templateDetail.IntermediaryType = string.Empty;
                        }

                        templateDetail.IntermediaryAccountNumber = template["Intermediary Account Number"];

                        templateDetail.UltimateBeneficiaryType = template["Ultimate Beneficiary Type"];
                        templateDetail.UltimateBeneficiary.BICorABA = template["Ultimate Beneficiary BIC or ABA"];
                        templateDetail.UltimateBeneficiaryAccountName = template["Ultimate Beneficiary Account Name"];
                        var ultimateBeneficiaryBiCorAba = accountBicorAba.FirstOrDefault(x => x.IsABA == (templateDetail.UltimateBeneficiaryType == "ABA") && x.BICorABA == templateDetail.UltimateBeneficiary.BICorABA);
                        if (ultimateBeneficiaryBiCorAba != null && templateDetail.UltimateBeneficiaryType != "Account Name")
                        {
                            templateDetail.UltimateBeneficiary.onBoardingAccountBICorABAId = ultimateBeneficiaryBiCorAba.onBoardingAccountBICorABAId;
                            templateDetail.UltimateBeneficiary.BankName = ultimateBeneficiaryBiCorAba.BankName;
                            templateDetail.UltimateBeneficiary.BankAddress = ultimateBeneficiaryBiCorAba.BankAddress;
                            templateDetail.UltimateBeneficiaryAccountName = string.Empty;
                        }
                        else
                        {
                            templateDetail.UltimateBeneficiary.BankName = string.Empty;
                            templateDetail.UltimateBeneficiary.BankAddress = string.Empty;
                            templateDetail.UltimateBeneficiary.BICorABA = string.Empty;
                        }
                        templateDetail.AccountNumber = template["Ultimate Beneficiary Account Number"];
                        templateDetail.FFCName = template["FFC Name"];
                        templateDetail.FFCNumber = template["FFC Number"];
                        templateDetail.Reference = template["Reference"];
                        templateDetail.SSITemplateType = template["SSI Template Type"];
                        if (templateDetail.onBoardingSSITemplateId == 0)
                        {
                            templateDetail.SSITemplateStatus = "Saved As Draft";
                            templateDetail.StatusComments = "Manually Uploaded";
                        }
                        else
                        {

                            templateDetail.SSITemplateStatus = template["SSI Template Status"];
                            templateDetail.StatusComments = template["Comments"];
                        }
                        templateDetail.CreatedBy = template["CreatedBy"];
                        templateDetail.UpdatedBy = template["UpdatedBy"];
                        templateDetail.ApprovedBy = template["ApprovedBy"];
                        templateDetail.CreatedAt = !string.IsNullOrWhiteSpace(template["CreatedDate"])
                            ? DateTime.Parse(template["CreatedDate"])
                            : DateTime.Now;
                        templateDetail.UpdatedAt = !string.IsNullOrWhiteSpace(template["ModifiedDate"])
                            ? DateTime.Parse(template["ModifiedDate"])
                            : DateTime.Now;
                        templateDetail.IsDeleted = false;
                        //templateDetail.TemplateName = template["SSI Template Type"] == "Broker" ? template["Broker"] + " - " + template["Account Type"] + " - " + templateDetail.Currency + " - " + template["Payment/Receipt Reason Detail"] : (!string.IsNullOrWhiteSpace(template["SSI Template Type"]) ? template["Service Provider"] + " - " + templateDetail.Currency + " - " + template["Payment/Receipt Reason Detail"] : template["Template Name"]);
                        AccountManager.AddSsiTemplate(templateDetail, UserName);
                    }
                    bulkUploadLogs.Add(new hmsBulkUploadLog() { FileName = newFileName, IsFundAccountLog = false, UserName = UserName });
                }

                AuditManager.AddBulkUploadLogs(bulkUploadLogs);
            }
            return "";
        }


        public FileResult ExportSampleSsiTemplatelist()
        {
            var contentToExport = new Dictionary<string, List<Row>>();

            var row = new Row();
            //row["SSI Template Id"] = string.Empty;
            row["Template Name"] = string.Empty;
            row["SSI Template Type"] = string.Empty;
            row["Legal Entity"] = string.Empty;
            row["Account Type"] = string.Empty;
            row["Service Provider"] = string.Empty;
            row["Currency"] = string.Empty;
            row["Payment/Receipt Reason Detail"] = string.Empty;
            row["Other Reason"] = string.Empty;
            row["Message Type"] = string.Empty;
            row["Beneficiary Type"] = "ABA/BIC";
            row["Beneficiary BIC or ABA"] = string.Empty;
            //row["Beneficiary Bank Name"] = String.Empty;
            //row["Beneficiary Bank Address"] = String.Empty;
            row["Beneficiary Account Number"] = string.Empty;
            row["Intermediary Beneficiary Type"] = "ABA/BIC";
            row["Intermediary BIC or ABA"] = string.Empty;
            //row["Intermediary Bank Name"] = String.Empty;
            //row["Intermediary Bank Address"] = String.Empty;
            row["Intermediary Account Number"] = string.Empty;
            row["Ultimate Beneficiary Type"] = "ABA/BIC";
            row["Ultimate Beneficiary BIC or ABA"] = String.Empty;
            // row["Ultimate Beneficiary Bank Name"] = String.Empty;
            //row["Ultimate Beneficiary Bank Address"] = String.Empty;
            row["Ultimate Beneficiary Account Name"] = string.Empty;
            row["Ultimate Beneficiary Account Number"] = string.Empty;
            row["FFC Name"] = string.Empty;
            row["FFC Number"] = string.Empty;
            row["Reference"] = string.Empty;
            row["CreatedBy"] = string.Empty;
            row["CreatedDate"] = string.Empty;
            row["UpdatedBy"] = string.Empty;
            row["ModifiedDate"] = string.Empty;
            row["ApprovedBy"] = string.Empty;
            var templateListRows = new List<Row> { row };

            //File name and path
            var fileName = string.Format("{0}_{1:yyyyMMdd}", "SSITemplateList", DateTime.Now);
            var exportFileInfo = new FileInfo(string.Format("{0}{1}{2}", FileSystemManager.UploadTemporaryFilesPath, fileName, DefaultExportFileFormat));
            contentToExport.Add("List of SSI Template", templateListRows);

            //Export the account file
            Exporter.CreateExcelFile(contentToExport, exportFileInfo.FullName, true);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        public FileResult DownloadAccountFile(string fileName, long accountId)
        {
            var file = new FileInfo(string.Format("{0}{1}\\{2}", FileSystemManager.OpsSecureAccountsFileUploads, accountId, fileName));
            return DownloadFile(file, file.Name);
        }

        public JsonResult UploadAccountFiles(long accountId)
        {
            var aDocments = new List<onBoardingAccountDocument>();
            if (accountId > 0)
            {
                for (var i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    if (file == null)
                        throw new Exception("unable to retrive file information");
                    var fileName = string.Format("{0}{1}\\{2}", FileSystemManager.OpsSecureAccountsFileUploads, accountId, file.FileName);
                    var fileinfo = new FileInfo(fileName);

                    if (fileinfo.Directory != null && !Directory.Exists(fileinfo.Directory.FullName))
                        Directory.CreateDirectory(fileinfo.Directory.FullName);

                    if (System.IO.File.Exists(fileinfo.FullName))
                    {
                        AccountManager.RemoveAccountDocument(accountId, file.FileName);
                        System.IO.File.Delete(fileinfo.FullName);
                    }

                    file.SaveAs(fileinfo.FullName);

                    //Build account document
                    var document = new onBoardingAccountDocument();
                    document.FileName = file.FileName;
                    document.RecCreatedAt = DateTime.Now;
                    document.RecCreatedBy = UserName;
                    document.onBoardingAccountId = accountId;

                    AccountManager.AddAccountDocument(document);

                    aDocments.Add(document);
                }
            }
            return Json(new
            {
                Documents = aDocments.Select(document => new
                {
                    document.onBoardingAccountDocumentId,
                    document.onBoardingAccountId,
                    document.FileName,
                    document.RecCreatedAt,
                    document.RecCreatedBy
                }).ToList()
            }, JsonContentType, JsonContentEncoding);
        }

        #endregion
    }
}