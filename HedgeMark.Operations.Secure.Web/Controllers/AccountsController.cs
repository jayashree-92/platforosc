using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                var hFunds = AuthorizedDMAFundData;
                var hFundIds = hFunds.Select(s => s.HmFundId).ToList();

                var funds = hFunds.Select(s => new { id = s.HmFundId, text = s.PreferredFundName, LegalName = s.LegalFundName });
                var agreementData = OnBoardingDataManager.GetAgreementsForOnboardingAccountPreloadData(hFundIds, AuthorizedSessionData.IsPrivilegedUser);
                var counterpartyFamilies = OnBoardingDataManager.GetAllCounterparties().Select(x => new
                {
                    id = x.CounterpartyId,
                    text = x.CounterpartyName,
                    familyId = x.CounterpartyFamilyId,
                    familyText = x.CounterpartyFamilyName
                }).OrderBy(x => x.text).ToList();

                var agreementFundIds = agreementData.Where(s => s.HMFundId > 0).Select(s => s.HMFundId).ToList();
                var fundsWithAgreements = funds.Where(s => agreementFundIds.Contains(s.id)).ToList();
                var agreements = agreementData.Select(s => new
                {
                    id = s.AgreementOnboardingId,
                    text = s.AgreementShortName,
                    hmFundId = s.HMFundId,
                    AgreementTypeId = s.AgreementTypeId,
                    CounterpartyFamilyId = s.CounterpartyFamilyId,
                    CounterpartyId = s.CounterpartyId,
                    AgreementType = s.AgreementType
                }).ToList();
                var accountTypes = OnBoardingDataManager.GetAllAgreementTypes(new List<string> { "DDA", "Custody" });
                return Json(new
                {
                    agreementData,
                    agreements,
                    funds,
                    fundsWithAgreements,
                    counterpartyFamilies,
                    ddaAgreementTypeId = accountTypes.First(s => s.Value == "DDA").Key,
                    custodyAgreementTypeId = accountTypes.First(s => s.Value == "Custody").Key,
                });
            }

        }

        public JsonResult GetAccountAssociationPreloadData()
        {
            var hFunds = AuthorizedDMAFundData;
            var funds = hFunds.Select(x => new { id = x.HmFundId, text = x.LegalFundName }).ToList();
            return Json(funds);
        }

        public JsonResult GetAccountDescriptionsByAgreementTypeId(long agreementTypeId)
        {
            var accountDescriptionChoices = FundAccountManager.GetAccountDescriptionsByAgreementTypeId(agreementTypeId);
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
            var accountModules = FundAccountManager.GetOnBoardingModules();
            return Json(new
            {
                accountModules = accountModules.Select(choice => new
                {
                    id = choice.onBoardingModuleId,
                    text = choice.ModuleName,
                    report = GetReportName(choice.dmaReportsId),
                }).OrderBy(x => x.text).ToList()
            }, JsonContentType, JsonContentEncoding);
        }

        private string GetReportName(long dmaReportsId)
        {
            if (dmaReportsId == 4)
                return "Collateral";
            else if (dmaReportsId == 17)
                return "Repo Collateral";
            else
                return "Invoices";
        }

        public JsonResult GetAccountReports()
        {
            var accountReports = FundAccountManager.GetAccountReports();
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
            FundAccountManager.AddAccountDescription(accountDescription, agreementTypeId);
        }

        public void AddAccountModule(long reportId, string accountModule)
        {
            FundAccountManager.AddOnboardingModule(reportId, accountModule, UserName);
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
            var onBoardingAccount = FundAccountManager.GetOnBoardingAccount(accountId);

            return Json(new
            {
                OnBoardingAccount = onBoardingAccount,
                isAuthorizedUserToApprove = (User.IsWireApprover() && onBoardingAccount.onBoardingAccountStatus == "Pending Approval" && onBoardingAccount.CreatedBy != UserName && onBoardingAccount.UpdatedBy != UserName)

            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetAccountSsiTemplateMap(long accountId, long fundId, string currency, string messages)
        {
            var ssiTemplateMaps = FundAccountManager.GetAccountSsiTemplateMap(accountId);

            var counterpartyIds = OnBoardingDataManager.GetCounterpartyIdsbyFund(fundId);

            if (string.IsNullOrWhiteSpace(messages))
                messages = string.Empty;

            var ssiTemplates = FundAccountManager.GetAllApprovedSsiTemplates(counterpartyIds, messages.Split(',').ToList(), string.IsNullOrWhiteSpace(messages), currency);
            var availableSSITemplates = ssiTemplates
                .Where(s => !ssiTemplateMaps.Select(p => p.onBoardingSSITemplateId)
                .Contains(s.onBoardingSSITemplateId)).ToList();

            return Json(new
            {
                ssiTemplateMaps = ssiTemplateMaps.Select(ssi =>
                {
                    var ssiTemplate = ssiTemplates.FirstOrDefault(template => template.onBoardingSSITemplateId == ssi.onBoardingSSITemplateId);
                    return ssiTemplate != null ? new
                    {
                        ssi.onBoardingAccountSSITemplateMapId,
                        ssi.onBoardingAccountId,
                        ssi.onBoardingSSITemplateId,
                        ssiTemplate.FFCName,
                        ssiTemplate.FFCNumber,
                        ssiTemplate.Reference,
                        ssi.CreatedAt,
                        ssi.CreatedBy,
                        ssi.UpdatedAt,
                        ssi.UpdatedBy,
                        ssi.Status,
                        ssi.StatusComments,
                        ssiTemplate.SSITemplateType,
                        ssiTemplate.TemplateName,
                        AccountNumber = ssiTemplate.UltimateBeneficiaryAccountNumber ?? ""
                    } : null;
                }).Where(temp => temp != null).OrderBy(y => y.TemplateName).ToList(),
                ssiTemplates = availableSSITemplates,
            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetSsiTemplateAccountMap(long ssiTemplateId, long brokerId, string currency, string message, bool isServiceType)
        {
            var ssiTemplateMaps = FundAccountManager.GetSsiTemplateAccountMap(ssiTemplateId);
            var hmFundIds = OnBoardingDataManager.GetFundIdsbyCounterparty(brokerId);
            if (string.IsNullOrWhiteSpace(message))
                message = string.Empty;
            var fundAccounts = FundAccountManager.GetAllApprovedAccounts(hmFundIds, message, currency, isServiceType);
            var existingAccountMaps = ssiTemplateMaps.Select(p => p.onBoardingAccountId).ToList();
            var availableFundAccounts = fundAccounts.Where(s => !existingAccountMaps.Contains(s.onBoardingAccountId)).ToList();
            availableFundAccounts.ForEach(s => s.SwiftGroup = null);

            return Json(new
            {
                ssiTemplateMaps = ssiTemplateMaps.Select(ssi =>
                {
                    var account = fundAccounts.FirstOrDefault(acc => acc.onBoardingAccountId == ssi.onBoardingAccountId);
                    return account != null ? new
                    {
                        ssi.onBoardingAccountSSITemplateMapId,
                        ssi.onBoardingAccountId,
                        ssi.onBoardingSSITemplateId,
                        account.FFCName,
                        account.FFCNumber,
                        account.Reference,
                        ssi.CreatedAt,
                        ssi.CreatedBy,
                        ssi.UpdatedAt,
                        ssi.UpdatedBy,
                        ssi.Status,
                        ssi.StatusComments,
                        account.AccountType,
                        account.AccountName,
                        AccountNumber = account.UltimateBeneficiaryAccountNumber ?? ""
                    } : null;
                }).Where(temp => temp != null).OrderBy(y => y.AccountName).ToList(),
                fundAccounts = availableFundAccounts,
            }, JsonContentType, JsonContentEncoding);
        }
        public JsonResult GetAccountDocuments(long accountId)
        {
            var accountDocuments = FundAccountManager.GetAccountDocuments(accountId);
            return Json(new
            {
                accountDocuments
            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetAllOnBoardingAccount()
        {
            var hmFundIds = AuthorizedSessionData.HMFundIds.Where(s => s.Level > 0).Select(s => s.Id).ToList();
            var onBoardingAccounts = FundAccountManager.GetAllOnBoardingAccounts(hmFundIds, AuthorizedSessionData.IsPrivilegedUser);
            var fundAccounts = FundAccountManager.GetOnBoardingAccountDetails(hmFundIds, AuthorizedSessionData.IsPrivilegedUser);
            var fundAccountmap = fundAccounts.ToDictionary(s => s.onBoardingAccountId, v => v);
            var accountTypes = OnBoardingDataManager.GetAllAgreementTypes();
            var receivingAccountTypes = PreferencesManager.GetSystemPreference(PreferencesManager.SystemPreferences.ReceivingAgreementTypesForAccount).Split(',').ToList();

            return Json(new
            {
                accountTypes = accountTypes.Select(x => new { id = x.Key, text = x.Value }).OrderBy(x => x.text).ToList(),
                receivingAccountTypes,
                OnBoardingAccounts = onBoardingAccounts.Select(account => new
                {
                    Account = FundAccountManager.SetAccountDefaults(account),
                    AccountNumber = fundAccountmap.ContainsKey(account.onBoardingAccountId) ? fundAccountmap[account.onBoardingAccountId].AccountNumber : string.Empty,
                    AgreementName = fundAccountmap.ContainsKey(account.onBoardingAccountId) ? fundAccountmap[account.onBoardingAccountId].AgreementShortName : string.Empty,
                    AgreementTypeId = fundAccountmap.ContainsKey(account.onBoardingAccountId) ? fundAccountmap[account.onBoardingAccountId].dmaAgreementTypeId ?? 0 : 0,
                    CounterpartyFamilyName = fundAccountmap.ContainsKey(account.onBoardingAccountId) ? fundAccountmap[account.onBoardingAccountId].CounterpartyFamily : string.Empty,
                    CounterpartyName = fundAccountmap.ContainsKey(account.onBoardingAccountId) ? fundAccountmap[account.onBoardingAccountId].CounterpartyName : string.Empty,
                    FundName = fundAccountmap.ContainsKey(account.onBoardingAccountId) ? fundAccountmap[account.onBoardingAccountId].LegalFundName : string.Empty,
                    ApprovedMaps = account.onBoardingAccountSSITemplateMaps.Count(s => s.Status == "Approved"),
                    PendingApprovalMaps = account.onBoardingAccountSSITemplateMaps.Count(s => s.Status == "Pending Approval"),

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

                var accountId = FundAccountManager.AddAccount(account, UserName);

                if (accountId > 0)
                {
                    AuditManager.Log(auditLogList);
                }
            }

        }

        public JsonResult GetAllOnBoardingAccountContacts(long hmFundId)
        {
            var onBoardingContacts = OnBoardingDataManager.GetAllOnBoardingContacts(hmFundId);
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
            FundAccountManager.DeleteAccount(onBoardingAccountId, UserName);
        }

        public JsonResult GetAllCurrencies()
        {
            var currenciesChoices = FundAccountManager.GetAllCurrencies();
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
            FundAccountManager.AddCurrency(currency);
        }

        public JsonResult GetAllCashInstruction()
        {
            var cashInstructionChoices = FundAccountManager.GetAllCashInstruction();
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
            FundAccountManager.AddCashInstruction(cashInstruction);
        }

        public JsonResult GetAllAuthorizedParty()
        {
            var authorizedParties = FundAccountManager.GetAllAuthorizedParty();
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
            FundAccountManager.AddAuthorizedParty(authorizedParty, UserName);
        }

        public JsonResult GetAllRelatedSwiftGroup(long brokerId)
        {
            var swiftGroups = FundAccountManager.GetAllSwiftGroup(brokerId);
            swiftGroups.ForEach(s =>
            {
                s.onBoardingAccounts = null;
                s.hmsSwiftGroupStatusLkp.hmsSwiftGroups = null;
            });
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

        public JsonResult GetAccountCallbackData(long accountId)
        {
            var callbacks = FundAccountManager.GetAccountCallbacks(accountId);
            return Json(callbacks, JsonContentType, JsonContentEncoding);
        }

        public void AddOrUpdateCallback(hmsAccountCallback callback)
        {
            hmsAccountCallback existingCallback;
            if (callback.hmsAccountCallbackId > 0)
            {
                existingCallback = FundAccountManager.GetCallbackData(callback.hmsAccountCallbackId);
                callback.RecCreatedDt = existingCallback.RecCreatedDt;
                callback.RecCreatedBy = existingCallback.RecCreatedBy;
                if (callback.IsCallbackConfirmed)
                {
                    callback.ConfirmedBy = UserName;
                    callback.ConfirmedAt = DateTime.Now;
                }
            }
            else
            {
                callback.RecCreatedBy = UserName;
                callback.RecCreatedDt = DateTime.Now;
            }

            FundAccountManager.AddOrUpdateCallback(callback);
        }

        public JsonResult GetAllAccountBicorAba()
        {
            var accountBicorAba = FundAccountManager.GetAllAccountBicorAba();
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

            FundAccountManager.AddAccountBiCorAba(accountBiCorAba);
        }

        public void UpdateAccountStatus(string accountStatus, long accountId, string comments)
        {
            FundAccountManager.UpdateAccountStatus(accountStatus, accountId, comments, UserName);
        }

        public void UpdateAccountMapStatus(string status, long accountMapId, string comments)
        {
            FundAccountManager.UpdateAccountMapStatus(status, accountMapId, comments, UserName);
        }

        public JsonResult GetCutoffTime(string cashInstruction, string currency)
        {
            var cutOffTime = FundAccountManager.GetCutoffTime(cashInstruction, currency);
            return Json(new
            {
                cutOffTime
            }, JsonContentType, JsonContentEncoding);
        }

        public void AddAccountSsiTemplateMap(List<onBoardingAccountSSITemplateMap> accountSsiTemplateMap)
        {
            FundAccountManager.AddAccountSsiTemplateMap(accountSsiTemplateMap, UserName);
        }

        public void RemoveAccountDocument(string fileName, long documentId)
        {
            //var fileinfo = new FileInfo(FileSystemManager.OnboardingAccountFilesPath + fileName);

            //if (System.IO.File.Exists(fileinfo.FullName))
            //    System.IO.File.Delete(fileinfo.FullName);
            if (documentId > 0)
                FundAccountManager.RemoveAccountDocument(documentId);
        }

        public bool IsAccountDocumentExists(long accountId)
        {
            return FundAccountManager.IsAccountDocumentExists(accountId);
        }


        #region Export and Upload

        public FileResult ExportAllAccountlist()
        {

            var hmFundIds = AuthorizedSessionData.HMFundIds.Where(s => s.Level > 0).Select(s => s.Id).ToList();
            var onBoardingAccounts = FundAccountManager.GetAllOnBoardingAccounts(hmFundIds, AuthorizedSessionData.IsPrivilegedUser).OrderByDescending(x => x.UpdatedAt).ToList();
            var fundAccounts = FundAccountManager.GetOnBoardingAccountDetails(hmFundIds, AuthorizedSessionData.IsPrivilegedUser);


            var accountListRows = BuildAccountRows(onBoardingAccounts, fundAccounts);
            //File name and path

            var contentToExport = new Dictionary<string, List<Row>>() { { "List of Accounts", accountListRows } };
            var fileName = string.Format("AccountList_{0:yyyyMMdd}", DateTime.Now);
            var exportFileInfo = new FileInfo(string.Format("{0}{1}{2}", FileSystemManager.UploadTemporaryFilesPath, fileName, DefaultExportFileFormat));
            //Export the checklist file

            Exporter.CreateExcelFile(contentToExport, exportFileInfo.FullName, true);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        //Build Account Rows
        private List<Row> BuildAccountRows(List<onBoardingAccount> onBoardingAccounts, List<vw_FundAccounts> fundAccounts)
        {
            var fundAccountMap = fundAccounts.ToDictionary(s => s.onBoardingAccountId, v => v);
            var accountListRows = new List<Row>();

            foreach (var account in onBoardingAccounts)
            {
                var row = new Row();
                row["Account Id"] = account.onBoardingAccountId.ToString();
                row["Entity Type"] = account.AccountType;
                row["Fund Name"] = fundAccountMap.ContainsKey(account.onBoardingAccountId) ? fundAccountMap[account.onBoardingAccountId].LegalFundName : string.Empty;
                row["Agreement Name"] = fundAccountMap.ContainsKey(account.onBoardingAccountId) ? fundAccountMap[account.onBoardingAccountId].AgreementShortName : string.Empty;
                row["Counterparty"] = fundAccountMap.ContainsKey(account.onBoardingAccountId) ? fundAccountMap[account.onBoardingAccountId].CounterpartyName : string.Empty;
                row["Counterparty Family"] = fundAccountMap.ContainsKey(account.onBoardingAccountId) ? fundAccountMap[account.onBoardingAccountId].CounterpartyFamily : string.Empty;
                row["Account Name"] = account.AccountName;
                row["Account Number"] = fundAccountMap.ContainsKey(account.onBoardingAccountId) ? fundAccountMap[account.onBoardingAccountId].AccountNumber : string.Empty;
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
                row["Margin Account Number"] = account.MarginAccountNumber;
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
            var onboardingAccounts = FundAccountManager.GetAllOnBoardingAccounts(new List<long>(), true);
            var counterpartyFamilies = OnBoardingDataManager.GetAllCounterpartyFamilies().ToDictionary(x => x.dmaCounterpartyFamilyId, x => x.CounterpartyFamily);
            var counterparties = OnBoardingDataManager.GetAllCounterparties().ToDictionary(x => x.CounterpartyId, x => x.CounterpartyName);
            var hFunds = AuthorizedDMAFundData;
            var agreements = OnBoardingDataManager.GetAllAgreements();
            var accountBicorAba = FundAccountManager.GetAllAccountBicorAba();
            var swiftgroups = FundAccountManager.GetAllSwiftGroup();

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
                            WirePortalCutoff = new hmsWirePortalCutoff(),
                            SwiftGroup = new hmsSwiftGroup(),
                            onBoardingAccountId = string.IsNullOrWhiteSpace(account["Account Id"]) ? 0 : long.Parse(account["Account Id"]),
                            UltimateBeneficiaryAccountNumber = account["Account Number"],
                            AccountType = account["Entity Type"]
                        };


                        if (account["Entity Type"] == "Agreement")
                        {
                            accountDetail.dmaAgreementOnBoardingId = agreements.FirstOrDefault(x => x.Value == account["Agreement Name"]).Key;

                            //No agreement available for given name
                            if (accountDetail.dmaAgreementOnBoardingId == 0)
                                continue;

                            accountDetail.hmFundId = hFunds.FirstOrDefault(x => x.LegalFundName == account["Fund Name"]).HmFundId;
                            var counterPartyByAgreement = onboardingAccounts.FirstOrDefault(s => s.dmaAgreementOnBoardingId == accountDetail.dmaAgreementOnBoardingId);
                            if (counterPartyByAgreement != null)
                            {
                                accountDetail.dmaCounterpartyFamilyId = counterPartyByAgreement.dmaCounterpartyFamilyId;
                                accountDetail.dmaCounterpartyId = counterPartyByAgreement.dmaCounterpartyId;
                            }

                            if (accountDetail.onBoardingAccountId == 0)
                            {
                                var existsAccount = onboardingAccounts.FirstOrDefault(x =>
                                    x.dmaAgreementOnBoardingId == accountDetail.dmaAgreementOnBoardingId &&
                                    x.hmFundId == accountDetail.hmFundId &&
                                    x.UltimateBeneficiaryAccountNumber == accountDetail.UltimateBeneficiaryAccountNumber);
                                if (existsAccount != null) continue;
                            }
                        }
                        else
                        {
                            accountDetail.hmFundId = hFunds.FirstOrDefault(x => x.LegalFundName == account["Fund Name"]).HmFundId;
                            accountDetail.dmaCounterpartyFamilyId = counterpartyFamilies.FirstOrDefault(x => x.Value == account["Counterparty Family"]).Key;
                            accountDetail.dmaCounterpartyId = counterparties.FirstOrDefault(x => x.Value == account["Counterparty"]).Key;
                            if (accountDetail.onBoardingAccountId == 0)
                            {
                                var existsAccount = onboardingAccounts.FirstOrDefault(x => x.hmFundId == accountDetail.hmFundId &&
                                    x.dmaCounterpartyFamilyId == accountDetail.dmaCounterpartyFamilyId && x.UltimateBeneficiaryAccountNumber == accountDetail.UltimateBeneficiaryAccountNumber);
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
                            accountDetail.SwiftGroupId = swiftGroup != null ? swiftGroup.hmsSwiftGroupId : (long?)null;
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
                        accountDetail.BeneficiaryAccountNumber = account["Beneficiary Account Number"];
                        accountDetail.Beneficiary.BICorABA = account["Beneficiary BIC or ABA"];
                        var beneficiaryBiCorAba = accountBicorAba.FirstOrDefault(x => x.IsABA == (accountDetail.BeneficiaryType == "ABA") && x.BICorABA == accountDetail.Beneficiary.BICorABA);
                        if (beneficiaryBiCorAba != null)
                        {
                            accountDetail.Beneficiary.onBoardingAccountBICorABAId = beneficiaryBiCorAba.onBoardingAccountBICorABAId;
                            accountDetail.Beneficiary.BankName = beneficiaryBiCorAba.BankName;
                            accountDetail.Beneficiary.BankAddress = beneficiaryBiCorAba.BankAddress;
                            accountDetail.BeneficiaryBICorABAId = beneficiaryBiCorAba.onBoardingAccountBICorABAId;
                        }
                        else
                        {
                            accountDetail.Beneficiary.BankName = string.Empty;
                            accountDetail.Beneficiary.BankAddress = string.Empty;
                            accountDetail.Beneficiary.BICorABA = string.Empty;
                            accountDetail.BeneficiaryType = string.Empty;
                            accountDetail.BeneficiaryBICorABAId = null;
                        }

                        accountDetail.IntermediaryType = account["Intermediary Beneficiary Type"];
                        accountDetail.IntermediaryAccountNumber = account["Intermediary Account Number"];
                        accountDetail.Intermediary.BICorABA = account["Intermediary BIC or ABA"];
                        var intermediaryBiCorAba = accountBicorAba.FirstOrDefault(x => x.IsABA == (accountDetail.IntermediaryType == "ABA") && x.BICorABA == accountDetail.Intermediary.BICorABA);
                        if (intermediaryBiCorAba != null)
                        {
                            accountDetail.Intermediary.onBoardingAccountBICorABAId = intermediaryBiCorAba.onBoardingAccountBICorABAId;
                            accountDetail.Intermediary.BankName = intermediaryBiCorAba.BankName;
                            accountDetail.Intermediary.BankAddress = intermediaryBiCorAba.BankAddress;
                            accountDetail.IntermediaryBICorABAId = intermediaryBiCorAba.onBoardingAccountBICorABAId;
                        }
                        else
                        {
                            accountDetail.Intermediary.BankName = string.Empty;
                            accountDetail.Intermediary.BankAddress = string.Empty;
                            accountDetail.Intermediary.BICorABA = string.Empty;
                            accountDetail.IntermediaryType = string.Empty;
                            accountDetail.IntermediaryBICorABAId = null;
                        }
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
                            accountDetail.UltimateBeneficiaryBICorABAId = ultimateBeneficiaryBiCorAba.onBoardingAccountBICorABAId;
                        }
                        else
                        {
                            accountDetail.UltimateBeneficiary.BankName = string.Empty;
                            accountDetail.UltimateBeneficiary.BankAddress = string.Empty;
                            accountDetail.UltimateBeneficiary.BICorABA = string.Empty;
                            accountDetail.UltimateBeneficiaryBICorABAId = null;
                        }

                        accountDetail.FFCName = account["FFC Name"];
                        accountDetail.FFCNumber = account["FFC Number"];
                        accountDetail.MarginAccountNumber = account["Margin Account Number"];
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
                            FundAccountManager.AddAccount(accountDetail, UserName);
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
            row["Margin Account Number"] = String.Empty;
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
                        FundAccountManager.RemoveAccountDocument(accountId, file.FileName);
                        System.IO.File.Delete(fileinfo.FullName);
                    }

                    file.SaveAs(fileinfo.FullName);

                    //Build account document
                    var document = new onBoardingAccountDocument();
                    document.FileName = file.FileName;
                    document.RecCreatedAt = DateTime.Now;
                    document.RecCreatedBy = UserName;
                    document.onBoardingAccountId = accountId;

                    FundAccountManager.AddAccountDocument(document);

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