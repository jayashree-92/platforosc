using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Com.HedgeMark.Commons.Extensions;
using ExcelUtility.Operations.ManagedAccounts;
using HedgeMark.Operations.FileParseEngine.Models;
using HedgeMark.Operations.FileParseEngine.Parser;
using HedgeMark.Operations.Secure.DataModel;
using HMOSecureMiddleware;
using HMOSecureMiddleware.Util;
using HMOSecureWeb.Utility;
using Newtonsoft.Json;

namespace HMOSecureWeb.Controllers
{
    public class AccountsController : BaseController
    {
        #region Account

        public ActionResult Account(long fundId = 0, long brokerId = 0, long agreementId = 0, string accountType = "")
        {
            ViewBag.accountType = accountType;
            ViewBag.agreementId = agreementId;
            ViewBag.fundId = fundId;
            ViewBag.brokerId = brokerId;
            //ViewBag.accountTypeId = accountTypeId;
            //ViewBag.AgreementTypeId = agreementTypeId;
            return View();
        }


        public ActionResult Index()
        {
            return View();
        }


        public ActionResult SSITemplateList()
        {
            return View();
        }


        public ActionResult SSITemplate(long ssiTemplateId = 0)
        {
            ViewBag.ssiTemplateId = ssiTemplateId;
            ViewBag.userName = UserName;
            return View();
        }

        public JsonResult GetAllAccounts()
        {
            var accounts = AccountManager.GetAllAccounts();
            var custodyAccounts = accounts.Where(x => x.AccountType == "Custody").ToList();
            return Json(new
            {
                accounts = accounts.Select(choice => new
                {
                    id = choice.onBoardingAccountId,
                    text = string.Join("{0}|{1}", choice.AccountNumber, choice.FFCNumber ?? string.Empty),
                }).OrderBy(x => x.text).ToList(),
                custodyAccounts = custodyAccounts.Select(choice => new
                {
                    id = choice.AccountName,
                    text = choice.AccountName
                }).OrderBy(x => x.text).ToList()
            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetAccountPreloadData()
        {
            var fundOnBoardIds = AuthorizedSessionData.OnBoardFundIds.Where(s => s.Level > 0).Select(s => s.Id).ToList();
            var onboardedFunds = OnBoardingDataManager.GetAllOnBoardedFunds(fundOnBoardIds, AuthorizedSessionData.IsPrivilegedUser);
            var funds = onboardedFunds.Select(x => new { id = x.dmaFundOnBoardId, text = x.LegalFundName }).OrderBy(x => x.text).ToList();

            var agreements = OnBoardingDataManager.GetAgreementsForOnboardingAccountPreloadData(fundOnBoardIds, AuthorizedSessionData.IsPrivilegedUser);
            var counterpartyFamilies = OnBoardingDataManager.GetAllCounterpartyFamilies().Select(x => new { id = x.dmaCounterpartyFamilyId, text = x.CounterpartyFamily }).OrderBy(x => x.text).ToList();
            return Json(new
            {
                agreements,
                funds,
                counterpartyFamilies
            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetAccountAssociationPreloadData()
        {
            var fundOnBoardIds = AuthorizedSessionData.OnBoardFundIds.Where(s => s.Level > 0).Select(s => s.Id).ToList();
            var onboardedFunds = OnBoardingDataManager.GetAllOnBoardedFunds(fundOnBoardIds, AuthorizedSessionData.IsPrivilegedUser);
            var funds = onboardedFunds.Select(x => new { id = x.dmaFundOnBoardId, text = x.LegalFundName }).OrderBy(x => x.text).ToList();

            return Json(new
            {
                funds
            }, JsonContentType, JsonContentEncoding);
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

        public string GetAllOnBoardingAccounts(string accountType, long agreementId, long fundId, long brokerId)
        {
            var counterpartyFamilyId = brokerId;
            int agreementTypeId;
            var agreementName = string.Empty;
            var agreementType = string.Empty;

            var onBoardingAccounts = AccountManager.GetAllOnBoardingAccounts(agreementId, fundId, brokerId);

            if (agreementId > 0)
            {
                var agreement = OnBoardingDataManager.GetOnBoardedAgreement(agreementId);
                agreementName = agreement.AgreementShortName;
                counterpartyFamilyId = agreement.dmaCounterPartyFamilyId ?? 0;
                agreementTypeId = agreement.AgreementTypeId ?? 0;
                fundId = agreement.dmaFundOnBoardId;
                agreementType = agreement.AgreementType;// AgreementManager.GetAgreementType(agreementTypeId);
            }
            else
                agreementTypeId = OnBoardingDataManager.GetAgreementTypeId(accountType);


            var onBoardingContacts = OnBoardingDataManager.GetAllOnBoardingContacts(ContactManager.CounterpartyTypeId, counterpartyFamilyId);
            var accountDescriptionChoices = AccountManager.GetAccountDescriptionsByAgreementTypeId(agreementTypeId);
            var accountModules = AccountManager.GetOnBoardingModules();
            var accountReports = AccountManager.GetAccountReports();
            var counterpartyIds = OnBoardingDataManager.GetCounterpartyIdsbyFund(fundId);
            var ssiTemplates = AccountManager.GetAllApprovedSsiTemplates(counterpartyIds);

            var broker = OnBoardingDataManager.GetCounterpartyFamilyName(counterpartyFamilyId);
            var legalFundName = OnBoardingDataManager.GetOnBoardedFundName(fundId);

            var swiftGroups = AccountManager.GetAllSwiftGroup();
            var authorizedParty = AccountManager.GetAllAuthorizedParty();

            Dictionary<long, string> counterparties;
            Dictionary<long, AgreementBaseData> agreements;
            var allAgreementIds = onBoardingAccounts.Select(s => s.dmaAgreementOnBoardingId).Distinct().ToList();
            var allCounterpartyFamilyIds = onBoardingAccounts.Select(s => s.BrokerId).Distinct().ToList();

            using (var context = new AdminContext())
            {
                agreements = context.vw_OnboardedAgreements.AsNoTracking()
                    .Where(s => allAgreementIds.Contains(s.dmaAgreementOnBoardingId))
                    .Select(s => new AgreementBaseData { AgreementOnboardingId = s.dmaAgreementOnBoardingId, AgreementShortName = s.AgreementShortName, AgreementTypeId = (int)s.AgreementTypeId })
                    .ToDictionary(s => s.AgreementOnboardingId, v => v);

                counterparties = context.dmaCounterpartyFamilies.AsNoTracking().Where(s => allCounterpartyFamilyIds.Contains(s.dmaCounterpartyFamilyId)).ToDictionary(s => s.dmaCounterpartyFamilyId, v => v.CounterpartyFamily);
            }

            return JsonConvert.SerializeObject(new
            {
                legalFundName,
                fundId,
                counterpartyFamilyId,
                agreementTypeId,
                agreementName,
                broker,
                agreementType,
                OnBoardingAccounts = onBoardingAccounts.Select(x => new
                {
                    AgreementName = x.dmaAgreementOnBoardingId != null && agreements.ContainsKey((long)x.dmaAgreementOnBoardingId) ? agreements[(long)x.dmaAgreementOnBoardingId].AgreementShortName : string.Empty,
                    AgreementTypeId = x.dmaAgreementOnBoardingId != null && agreements.ContainsKey((long)x.dmaAgreementOnBoardingId) ? agreements[(long)x.dmaAgreementOnBoardingId].AgreementTypeId : 0,
                    Broker = x.BrokerId != null && counterparties.ContainsKey((long)x.BrokerId) ? counterparties[(long)x.BrokerId] : string.Empty,
                    x.onBoardingAccountId,
                    x.dmaAgreementOnBoardingId,
                    x.AccountType,
                    x.BrokerId,
                    x.CutoffTime,
                    x.DaystoWire,
                    x.dmaFundOnBoardId,
                    x.AccountName,
                    x.AccountNumber,
                    x.AuthorizedParty,
                    x.CashSweep,
                    x.CashSweepTime,
                    x.CashSweepTimeZone,
                    x.ContactEmail,
                    x.ContactName,
                    x.ContactNumber,
                    x.ContactType,
                    x.Description,
                    x.AccountModule,
                    x.CreatedAt,
                    x.Currency,
                    x.CashInstruction,
                    x.CreatedBy,
                    x.Notes,
                    x.UpdatedAt,
                    x.UpdatedBy,
                    x.ApprovedBy,
                    x.BeneficiaryType,
                    x.BeneficiaryBICorABA,
                    x.BeneficiaryBankName,
                    x.BeneficiaryBankAddress,
                    x.BeneficiaryAccountNumber,
                    x.IntermediaryType,
                    x.IntermediaryBICorABA,
                    x.IntermediaryBankName,
                    x.IntermediaryBankAddress,
                    x.IntermediaryAccountNumber,
                    x.UltimateBeneficiaryType,
                    x.UltimateBeneficiaryBICorABA,
                    x.UltimateBeneficiaryBankName,
                    x.UltimateBeneficiaryAccountName,
                    x.UltimateBeneficiaryBankAddress,
                    x.FFCName,
                    x.FFCNumber,
                    x.Reference,
                    x.onBoardingAccountStatus,
                    x.SendersBIC,
                    x.StatusComments,
                    x.SwiftGroup,
                    x.AccountPurpose,
                    x.AccountStatus,
                    x.HoldbackAmount,
                    x.SweepComments,
                    x.AssociatedCustodyAcct,
                    x.PortfolioSelection,
                    x.TickerorISIN,
                    x.SweepCurrency,
                    x.onBoardingAccountDocuments,
                    onBoardingAccountSSITemplateMaps = x.onBoardingAccountSSITemplateMaps.Select(ssi =>
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
                    }).Where(temp => temp != null).OrderBy(y => y.TemplateName).ToList()

                }).OrderBy(x => x.AccountName).ToList(),
                OnBoardingContacts = onBoardingContacts.Select(contact => new
                {
                    id = contact.dmaOnBoardingContactDetailId,
                    contact.Email,
                    JobTitle = (!string.IsNullOrWhiteSpace(contact.JobTitle)) ? contact.JobTitle.Trim(',') : string.Empty,
                    contact.ContactType,
                    contact.Notes,
                    wires = contact.Wires ? "Yes" : "No",
                    margin = contact.Margin ? "Yes" : "No",
                    cash = contact.Cash ? "Yes" : "No",
                    collateral = contact.Collateral ? "Yes" : "No",
                    Interest = contact.InterestRate ? "Yes" : "No",
                    contact.BusinessPhone,
                    name = GetContactName(contact.LastName, contact.FirstName, contact.Email)
                }).OrderBy(x => x.name).ToList(),

                swiftGroups,
                SwiftGroupData = swiftGroups.Select(choice => new
                {
                    id = choice.SwiftGroup,
                    text = choice.SwiftGroup
                }).OrderBy(x => x.text).ToList(),
                authorizedParties = authorizedParty.Select(choice => new
                {
                    id = choice.AuthorizedParty,
                    text = choice.AuthorizedParty
                }).OrderBy(x => x.text).ToList(),
                accountDescriptions = accountDescriptionChoices.Select(choice => new
                {
                    id = choice.AccountDescription,
                    text = choice.AccountDescription
                }).OrderBy(x => x.text).ToList(),
                accountModules = accountModules.Select(choice => new
                {
                    id = choice.onBoardingModuleId,
                    text = choice.ModuleName,
                    report = choice.dmaReportsId == 4 ? "Collateral" : "Invoices"
                }).OrderBy(x => x.text).ToList(),
                accountReports = accountReports.Select(choice => new
                {
                    id = choice.dmaReportsId,
                    text = choice.ReportName,
                }).OrderBy(x => x.id).ToList(),
                ssiTemplates

            }, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            });
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

        public JsonResult GetAccountSsiTemplateMap(long accountId, long fundId, string currency)
        {
            var ssiTemplateMaps = AccountManager.GetAccountSsiTemplateMap(accountId);

            var counterpartyIds = OnBoardingDataManager.GetCounterpartyIdsbyFund(fundId);

            var ssiTemplates = AccountManager.GetAllApprovedSsiTemplates(counterpartyIds, currency);

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
                ssiTemplates
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
            var fundOnBoardIds = AuthorizedSessionData.OnBoardFundIds.Where(s => s.Level > 0).Select(s => s.Id).ToList();
            var onBoardingAccounts = AccountManager.GetAllOnBoardingAccounts(fundOnBoardIds, AuthorizedSessionData.IsPrivilegedUser);

            var accountTypes = OnBoardingDataManager.GetAllAgreementTypes();
            var ssiTemplates = AccountManager.GetAllApprovedSsiTemplates().Select(s => s.onBoardingSSITemplateId).ToList();


            Dictionary<long, string> counterparties;
            Dictionary<long, string> funds;
            Dictionary<long, AgreementBaseData> agreements;
            var allAgreementIds = onBoardingAccounts.Select(s => s.dmaAgreementOnBoardingId).Distinct().ToList();
            var allCounterpartyFamilyIds = onBoardingAccounts.Select(s => s.BrokerId).Distinct().ToList();
            var allFundIds = onBoardingAccounts.Select(s => s.dmaFundOnBoardId).Distinct().ToList();

            using (var context = new AdminContext())
            {
                agreements = context.vw_OnboardedAgreements.AsNoTracking()
                    .Where(s => allAgreementIds.Contains(s.dmaAgreementOnBoardingId))
                    .Select(s => new AgreementBaseData { AgreementOnboardingId = s.dmaAgreementOnBoardingId, AgreementShortName = s.AgreementShortName, AgreementTypeId = (int)s.AgreementTypeId })
                    .ToDictionary(s => s.AgreementOnboardingId, v => v);

                counterparties = context.dmaCounterpartyFamilies.AsNoTracking().Where(s => allCounterpartyFamilyIds.Contains(s.dmaCounterpartyFamilyId)).ToDictionary(s => s.dmaCounterpartyFamilyId, v => v.CounterpartyFamily);
                funds = context.onboardingFunds.AsNoTracking().Where(s => allFundIds.Contains(s.dmaFundOnBoardId)).ToDictionary(s => s.dmaFundOnBoardId, v => v.LegalFundName);
            }


            return Json(new
            {
                accountTypes = accountTypes.Select(x => new { id = x.Key, text = x.Value }).OrderBy(x => x.text).ToList(),
                OnBoardingAccounts = onBoardingAccounts.Select(x => new
                {
                    AgreementName = x.dmaAgreementOnBoardingId != null && agreements.ContainsKey((long)x.dmaAgreementOnBoardingId) ? agreements[(long)x.dmaAgreementOnBoardingId].AgreementShortName : string.Empty,

                    AgreementTypeId = x.dmaAgreementOnBoardingId != null && agreements.ContainsKey((long)x.dmaAgreementOnBoardingId) ? agreements[(long)x.dmaAgreementOnBoardingId].AgreementTypeId :
                                      (accountTypes.ContainsValue(x.AccountType) ? accountTypes.FirstOrDefault(y => y.Value == x.AccountType).Key : 0),

                    Broker = x.BrokerId != null && counterparties.ContainsKey((long)x.BrokerId) ? counterparties[(long)x.BrokerId] : string.Empty,
                    FundName = funds.ContainsKey(x.dmaFundOnBoardId) ? funds[x.dmaFundOnBoardId] : string.Empty,
                    ApprovedMaps = x.onBoardingAccountSSITemplateMaps.Count(s => s.Status == "Approved" && ssiTemplates.Contains(s.onBoardingSSITemplateId)),
                    PendingApprovalMaps = x.onBoardingAccountSSITemplateMaps.Count(s => s.Status == "Pending Approval" && ssiTemplates.Contains(s.onBoardingSSITemplateId)),
                    x.onBoardingAccountId,
                    x.dmaAgreementOnBoardingId,
                    x.AccountType,
                    x.BrokerId,
                    x.CutoffTime,
                    x.DaystoWire,
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
                    x.BeneficiaryBICorABA,
                    x.BeneficiaryBankName,
                    x.BeneficiaryBankAddress,
                    x.BeneficiaryAccountNumber,
                    x.IntermediaryType,
                    x.IntermediaryBICorABA,
                    x.IntermediaryBankName,
                    x.IntermediaryBankAddress,
                    x.IntermediaryAccountNumber,
                    x.UltimateBeneficiaryType,
                    x.UltimateBeneficiaryBICorABA,
                    x.UltimateBeneficiaryBankName,
                    x.UltimateBeneficiaryAccountName,
                    x.UltimateBeneficiaryBankAddress,
                    x.FFCName,
                    x.FFCNumber,
                    x.Reference,
                    x.onBoardingAccountStatus,
                    x.dmaFundOnBoardId,
                    x.SendersBIC,
                    x.StatusComments,
                    x.SwiftGroup,
                    x.AccountPurpose,
                    x.AccountStatus,
                    x.HoldbackAmount,
                    x.SweepComments,
                    x.AssociatedCustodyAcct,
                    x.PortfolioSelection,
                    x.TickerorISIN,
                    x.SweepCurrency
                }).ToList()
            }, JsonContentType, JsonContentEncoding);
        }

        public void AddAccounts(List<onBoardingAccount> onBoardingAccounts, string fundName = "", string agreement = "", string broker = "")
        {
            List<hmsUserAuditLog> auditLogList;

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
                    var onboardModuleAssociations = new List<onBoardingAccountModuleAssociation>();
                    foreach (var module in account.AccountModule.Split(',').Where(s => s != "").Select(s => long.Parse(s)))
                    {
                        var moduleMap = new onBoardingAccountModuleAssociation()
                        {
                            onBoardingAccountId = account.onBoardingAccountId,
                            onBoardingModuleId = module,
                            CreatedAt = DateTime.Now,
                            CreatedBy = UserName
                        };
                        onboardModuleAssociations.Add(moduleMap);
                    }
                    account.onBoardingAccountModuleAssociations = onboardModuleAssociations;
                }

                auditLogList = account.onBoardingAccountId > 0 ? UpdateAccountAuditLog(account) : AddAccountAuditLog(account, fundName, agreement, broker);

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
                }).OrderBy(x => x.text).ToList()
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

        public JsonResult GetAllSwiftGroup()
        {
            var swiftGroups = AccountManager.GetAllSwiftGroup();
            return Json(new
            {
                swiftGroups,
                SwiftGroupData = swiftGroups.Select(choice => new
                {
                    id = choice.SwiftGroup,
                    text = choice.SwiftGroup
                }).OrderBy(x => x.text).ToList()
            }, JsonContentType, JsonContentEncoding);
        }

        public void AddSwiftGroup(string swiftGroup, string senderBic)
        {
            AccountManager.AddSwiftGroup(swiftGroup, senderBic, UserName);
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

        #endregion

        #region SsiTemplates

        public JsonResult GetAllSsiTemplates()
        {
            var ssiTemplates = AccountManager.GetAllSsiTemplates();
            return Json(new
            {
                SSITemplates = ssiTemplates.OrderBy(x => x.Value).Select(ac => new
                {
                    id = ac.Key,
                    text = ac.Value
                }).ToList()
            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetAllSsiTemplates(int templateTypeId, long templateEntityId)
        {
            var onBoardingSsiTemplates = AccountManager.GetAllSsiTemplates(templateTypeId, templateEntityId);
            return Json(new
            {
                OnBoardingSSITemplates = onBoardingSsiTemplates
            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetAllBrokerSsiTemplates()
        {
            var brokerSsiTemplates = AccountManager.GetAllBrokerSsiTemplates();
            var counterParties = OnBoardingDataManager.GetAllOnBoardedCounterparties();
            var agreementTypes = OnBoardingDataManager.GetAllAgreementTypes();

            return Json(new
            {
                BrokerSsiTemplates = brokerSsiTemplates.Select(template => new
                {
                    template.onBoardingSSITemplateId,
                    template.SSITemplateType,
                    template.TemplateName,
                    template.TemplateEntityId,
                    template.dmaAgreementTypeId,
                    template.TemplateTypeId,
                    template.Currency,
                    template.AccountNumber,
                    template.AccountName,
                    template.ReasonDetail,
                    template.ServiceProvider,
                    template.SSITemplateStatus,
                    template.OtherReason,
                    template.MessageType,
                    template.StatusComments,
                    template.CreatedAt,
                    template.CreatedBy,
                    template.UpdatedAt,
                    template.UpdatedBy,
                    template.ApprovedBy,
                    template.BeneficiaryType,
                    template.BeneficiaryBICorABA,
                    template.BeneficiaryBankName,
                    template.BeneficiaryBankAddress,
                    template.BeneficiaryAccountNumber,
                    template.IntermediaryType,
                    template.IntermediaryBICorABA,
                    template.IntermediaryBankName,
                    template.IntermediaryBankAddress,
                    template.IntermediaryAccountNumber,
                    template.UltimateBeneficiaryType,
                    template.UltimateBeneficiaryBICorABA,
                    template.UltimateBeneficiaryBankName,
                    template.UltimateBeneficiaryAccountName,
                    template.UltimateBeneficiaryBankAddress,
                    template.FFCName,
                    template.FFCNumber,
                    template.Reference,
                    AgreementType = (agreementTypes.ContainsKey(template.dmaAgreementTypeId) && string.IsNullOrEmpty(template.ServiceProvider)) ? agreementTypes[template.dmaAgreementTypeId] : string.Empty,
                    Broker = (counterParties.ContainsKey(template.TemplateEntityId) ? counterParties[template.TemplateEntityId] : string.Empty)
                }).ToList()
            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetSsiTemplate(long templateId)
        {
            var onBoardingSsiTemplate = AccountManager.GetSsiTemplate(templateId);
            var document = onBoardingSsiTemplate.onBoardingSSITemplateDocuments.ToList();
            onBoardingSsiTemplate.onBoardingSSITemplateDocuments = null;
            return Json(new
            {
                OnBoardingSsiTemplate = onBoardingSsiTemplate,
                isAuthorizedUserToApprove = (User.IsWireApprover() && onBoardingSsiTemplate.SSITemplateStatus == "Pending Approval" && onBoardingSsiTemplate.CreatedBy != UserName && onBoardingSsiTemplate.UpdatedBy != UserName),
                document
            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetAllServiceProviderList()
        {
            var serviceProviders = AccountManager.GetAllServiceProviderList().Select(y => new { id = y.ServiceProvider, text = y.ServiceProvider }).DistinctBy(x => x.id).OrderBy(x => x.id).ToList();
            return Json(serviceProviders, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PaymentOrReceiptReasonDetails(string templateType, int? agreementTypeId, string serviceProviderName)
        {
            var reasonDetail = templateType == "Broker" ? AccountManager.GetAllSsiTemplateAccountTypes(agreementTypeId).Select(x => new { id = x.Reason, text = x.Reason }).OrderBy(x => x.text).ToList() : AccountManager.GetAllSsiTemplateServiceProviders(serviceProviderName).Select(x => new { id = x.Reason, text = x.Reason }).OrderBy(x => x.text).ToList();
            return Json(reasonDetail, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSsiTemplatePreloadData()
        {
            var counterParties = OnBoardingDataManager.GetAllOnBoardedCounterparties().Select(x => new { id = x.Key, text = x.Value }).OrderBy(x => x.text).ToList();
            var templates = AccountManager.GetAllSsiTemplates().Select(x => new { id = x.Key, text = x.Value }).ToList();
            var permittedAgreementTypes = new List<string>() { "ISDA", "PB", "FCM", "CDA", "FXPB", "GMRA", "MSLA", "MRA", "MSFTA", "Listed Options", "Non-US Listed Options" };
            var accountTypes = OnBoardingDataManager.GetAllAgreementTypes().Where(x => permittedAgreementTypes.Contains(x.Value)).Select(x => new { id = x.Key, text = x.Value }).OrderBy(x => x.text).ToList();
            var currencies = AccountManager.GetAllCurrencies().Select(y => new { id = y.Currency, text = y.Currency }).OrderBy(x => x.text).ToList();

            return Json(new
            {
                counterParties,
                templates,
                accountTypes,
                currencies
            }, JsonContentType, JsonContentEncoding);
        }

        public long AddSsiTemplate(onBoardingSSITemplate ssiTemplate, string accountType, string broker)
        {
            var document = ssiTemplate.onBoardingSSITemplateDocuments.ToList();
            if (document.Count > 0)
            {
                document.RemoveAll(x => string.IsNullOrWhiteSpace(x.FileName));
                if (ssiTemplate.onBoardingSSITemplateId > 0)
                    document.ForEach(x =>
                    {
                        x.onBoardingSSITemplateId = ssiTemplate.onBoardingSSITemplateId;
                        x.RecCreatedAt = x.RecCreatedAt ?? DateTime.Now;
                    });
                else
                    document.ForEach(x =>
                    {
                        x.onBoardingSSITemplateDocumentId = 0;
                        x.onBoardingSSITemplateId = 0;
                        x.onBoardingSSITemplate = null;
                        x.RecCreatedAt = DateTime.Now;
                    });

                ssiTemplate.onBoardingSSITemplateDocuments = document;
            }
            var auditLogList = ssiTemplate.onBoardingSSITemplateId > 0 ? UpdateSsiTemplateAuditLog(ssiTemplate, accountType, broker) : AddSsiTemplateAuditLog(ssiTemplate, accountType, broker);
            var ssiTemplateId = AccountManager.AddSsiTemplate(ssiTemplate, UserName);
            if (ssiTemplateId > 0)
            {
                AuditManager.Log(auditLogList);
            }

            return ssiTemplateId;
        }

        public void UpdateSsiTemplateStatus(string ssiTemplateStatus, long ssiTemplateId, string comments)
        {
            AccountManager.UpdateSsiTemplateStatus(ssiTemplateStatus, ssiTemplateId, comments, UserName);
        }

        public void DeleteSsiTemplate(long ssiTemplateId)
        {
            AccountManager.DeleteSsiTemplate(ssiTemplateId, UserName);
        }

        public void AddPaymentOrReceiptReasonDetails(string reason, string templateType, int? agreementTypeId, string serviceProviderName)
        {
            AccountManager.AddPaymentOrReceiptReasonDetails(reason, templateType, agreementTypeId, serviceProviderName);
        }

        public void AddServiceProvider(string serviceProviderName)
        {
            AccountManager.AddServiceProvider(serviceProviderName);
        }

        public void RemoveSsiTemplateDocument(string fileName, long documentId)
        {
            //var fileinfo = new FileInfo(FileSystemManager.OnboardingSsiTemplateFilesPath + fileName);

            //if (System.IO.File.Exists(fileinfo.FullName))
            //    System.IO.File.Delete(fileinfo.FullName);
            if (documentId > 0)
                AccountManager.RemoveSsiTemplateDocument(documentId);
        }

        public bool IsSsiTemplateDocumentExists(long ssiTemplateId)
        {
            return AccountManager.IsSsiTemplateDocumentExists(ssiTemplateId);
        }
        #endregion

        #region Export and Upload

        public FileResult ExportAllAccountlist()
        {

            var fundOnBoardIds = AuthorizedSessionData.OnBoardFundIds.Where(s => s.Level > 0).Select(s => s.Id).ToList();
            var onBoardingAccounts = AccountManager.GetAllOnBoardingAccounts(fundOnBoardIds, AuthorizedSessionData.IsPrivilegedUser).OrderByDescending(x => x.UpdatedAt).ToList();
            var contentToExport = new Dictionary<string, List<Row>>();
            var accountListRows = BuildAccountRows(onBoardingAccounts);
            //File name and path
            var fileName = string.Format("{0}_{1:yyyyMMdd}", "AccountList", DateTime.Now);
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
            Dictionary<long, string> funds;
            Dictionary<long, AgreementBaseData> agreements;
            var allAgreementIds = onBoardingAccounts.Select(s => s.dmaAgreementOnBoardingId).Distinct().ToList();
            var allCounterpartyFamilyIds = onBoardingAccounts.Select(s => s.BrokerId).Distinct().ToList();
            var allFundIds = onBoardingAccounts.Select(s => s.dmaFundOnBoardId).Distinct().ToList();

            using (var context = new AdminContext())
            {
                agreements = context.vw_OnboardedAgreements.AsNoTracking()
                    .Where(s => allAgreementIds.Contains(s.dmaAgreementOnBoardingId))
                    .Select(s => new AgreementBaseData { AgreementOnboardingId = s.dmaAgreementOnBoardingId, AgreementShortName = s.AgreementShortName, AgreementTypeId = (int)s.AgreementTypeId })
                    .ToDictionary(s => s.AgreementOnboardingId, v => v);

                counterparties = context.dmaCounterpartyFamilies.AsNoTracking().Where(s => allCounterpartyFamilyIds.Contains(s.dmaCounterpartyFamilyId)).ToDictionary(s => s.dmaCounterpartyFamilyId, v => v.CounterpartyFamily);
                funds = context.onboardingFunds.AsNoTracking().Where(s => allFundIds.Contains(s.dmaFundOnBoardId)).ToDictionary(s => s.dmaFundOnBoardId, v => v.LegalFundName);
            }

            foreach (var account in onBoardingAccounts)
            {
                var row = new Row();
                row["Account Id"] = account.onBoardingAccountId.ToString();
                row["Entity Type"] = account.AccountType;
                row["Fund Name"] = funds.ContainsKey(account.dmaFundOnBoardId) ? funds[account.dmaFundOnBoardId] : string.Empty;
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
                row["Swift Group"] = account.SwiftGroup;
                row["Senders BIC"] = account.SendersBIC;
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

                if (account.CutoffTime != null)
                {
                    var dateTime = DateTime.Today.AddHours(account.CutoffTime.Value.Hours).AddMinutes(account.CutoffTime.Value.Minutes);
                    var stringTime = dateTime.ToString("hh:mm tt");

                    row["Cutoff Time"] = stringTime;
                }
                else
                {
                    row["Cutoff Time"] = string.Empty;
                }

                row["Days to wire per V.D"] = account.DaystoWire != null ? account.DaystoWire.ToString() + (account.DaystoWire.Value > 1 ? " Days" : " Day") : string.Empty;

                row["Holdback Amount"] = account.HoldbackAmount.HasValue ? account.HoldbackAmount.ToString() : string.Empty;
                row["Sweep Comments"] = account.SweepComments;
                row["Associated Custody Acct"] = account.AssociatedCustodyAcct;
                row["Portfolio Selection"] = account.PortfolioSelection;
                row["Ticker/ISIN"] = account.TickerorISIN;
                row["Sweep Currency"] = account.SweepCurrency;

                row["Contact Type"] = account.ContactType;
                //row["Contact Name"] = account.ContactName;
                //row["Contact Email"] = account.ContactEmail;
                //row["Contact Number"] = account.ContactNumber;
                row["Beneficiary Type"] = account.BeneficiaryType;
                row["Beneficiary BIC or ABA"] = account.BeneficiaryBICorABA;
                row["Beneficiary Bank Name"] = account.BeneficiaryBankName;
                row["Beneficiary Bank Address"] = account.BeneficiaryBankAddress;
                row["Beneficiary Account Number"] = account.BeneficiaryAccountNumber;
                row["Intermediary Beneficiary Type"] = account.IntermediaryType;
                row["Intermediary BIC or ABA"] = account.IntermediaryBICorABA;
                row["Intermediary Bank Name"] = account.IntermediaryBankName;
                row["Intermediary Bank Address"] = account.IntermediaryBankAddress;
                row["Intermediary Account Number"] = account.IntermediaryAccountNumber;
                row["Ultimate Beneficiary Type"] = account.UltimateBeneficiaryType;
                row["Ultimate Beneficiary BIC or ABA"] = account.UltimateBeneficiaryBICorABA;
                row["Ultimate Beneficiary Bank Name"] = account.UltimateBeneficiaryBankName;
                row["Ultimate Beneficiary Bank Address"] = account.UltimateBeneficiaryBankAddress;
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

        public FileResult ExportAllSsiTemplatelist()
        {
            //var authorizeData = OnBoardingManager.GetAuthorizedData(User.GetUserName(), User.GetRole());
            //var fundOnBoardIds = authorizeData.FundOnBoardingIds.Where(s => s.Level > 0).Select(s => s.Id).ToList();
            var ssiTemplates = AccountManager.GetAllBrokerSsiTemplates().OrderByDescending(x => x.UpdatedAt).ToList();
            var contentToExport = new Dictionary<string, List<Row>>();
            var accountListRows = BuildSsiTemplateRows(ssiTemplates);
            //File name and path
            var fileName = string.Format("{0}_{1:yyyyMMdd}", "SSITemplateList", DateTime.Now);
            var exportFileInfo = new FileInfo(string.Format("{0}{1}{2}", FileSystemManager.UploadTemporaryFilesPath, fileName, DefaultExportFileFormat));
            contentToExport.Add("List of SSI Template", accountListRows);
            //Export the checklist file
            Exporter.CreateExcelFile(contentToExport, exportFileInfo.FullName, true);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        // Build SSI Template Rows
        private List<Row> BuildSsiTemplateRows(List<onBoardingSSITemplate> ssiTemplates)
        {
            var templateListRows = new List<Row>();
            var counterParties = OnBoardingDataManager.GetAllOnBoardedCounterparties();
            var agreementTypes = OnBoardingDataManager.GetAllAgreementTypes();

            foreach (var template in ssiTemplates)
            {
                var row = new Row();
                row["SSI Template Id"] = template.onBoardingSSITemplateId.ToString();
                row["Template Name"] = template.TemplateName;
                row["SSI Template Type"] = template.SSITemplateType;
                row["Legal Entity"] = counterParties.ContainsKey(template.TemplateEntityId) ? counterParties[template.TemplateEntityId] : string.Empty;
                row["Account Type"] = (agreementTypes.ContainsKey(template.dmaAgreementTypeId) && string.IsNullOrEmpty(template.ServiceProvider)) ? agreementTypes[template.dmaAgreementTypeId] : string.Empty;
                row["Service Provider"] = template.ServiceProvider;
                row["Currency"] = template.Currency;
                row["Payment/Receipt Reason Detail"] = template.ReasonDetail;
                row["Other Reason"] = template.OtherReason;
                row["Message Type"] = template.MessageType;
                row["Beneficiary Type"] = template.BeneficiaryType;
                row["Beneficiary BIC or ABA"] = template.BeneficiaryBICorABA;
                row["Beneficiary Bank Name"] = template.BeneficiaryBankName;
                row["Beneficiary Bank Address"] = template.BeneficiaryBankAddress;
                row["Beneficiary Account Number"] = template.BeneficiaryAccountNumber;
                row["Intermediary Beneficiary Type"] = template.IntermediaryType;
                row["Intermediary BIC or ABA"] = template.IntermediaryBICorABA;
                row["Intermediary Bank Name"] = template.IntermediaryBankName;
                row["Intermediary Bank Address"] = template.IntermediaryBankAddress;
                row["Intermediary Account Number"] = template.IntermediaryAccountNumber;
                row["Ultimate Beneficiary Type"] = template.UltimateBeneficiaryType;
                row["Ultimate Beneficiary BIC or ABA"] = template.UltimateBeneficiaryBICorABA;
                row["Ultimate Beneficiary Bank Name"] = template.UltimateBeneficiaryBankName;
                row["Ultimate Beneficiary Bank Address"] = template.UltimateBeneficiaryBankAddress;
                row["Ultimate Beneficiary Account Name"] = template.UltimateBeneficiaryAccountName;
                row["Ultimate Beneficiary Account Number"] = template.AccountNumber;
                row["FFC Name"] = template.FFCName;
                row["FFC Number"] = template.FFCNumber;
                row["Reference"] = template.Reference;
                row["SSI Template Status"] = template.SSITemplateStatus;
                row["Comments"] = template.StatusComments;
                row["CreatedBy"] = template.CreatedBy;
                row["CreatedDate"] = template.CreatedAt + "";
                row["UpdatedBy"] = template.UpdatedBy;
                row["ModifiedDate"] = template.UpdatedAt + "";
                row["ApprovedBy"] = template.ApprovedBy;
                switch (template.SSITemplateStatus)
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
                templateListRows.Add(row);
            }

            return templateListRows;
        }

        public string UploadAccount()
        {
            var onboardingAccounts = AccountManager.GetAllOnBoardingAccounts(new List<long>(), true);
            var counterpartyFamilies = OnBoardingDataManager.GetAllCounterpartyFamilies().ToDictionary(x => x.dmaCounterpartyFamilyId, x => x.CounterpartyFamily);
            var funds = OnBoardingDataManager.GetAllFunds();
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
                        var accountDetail = new onBoardingAccount();
                        accountDetail.onBoardingAccountId = string.IsNullOrWhiteSpace(account["Account Id"]) ? 0 : long.Parse(account["Account Id"]);
                        accountDetail.AccountNumber = account["Account Number"];
                        accountDetail.AccountType = account["Entity Type"];

                        if (account["Entity Type"] == "Agreement")
                        {
                            accountDetail.dmaAgreementOnBoardingId = agreements.FirstOrDefault(x => x.Value == account["Agreement Name"]).Key;
                            accountDetail.dmaFundOnBoardId = funds.FirstOrDefault(x => x.Value == account["Fund Name"]).Key;
                            var counterPartyByAgreement = onboardingAccounts.FirstOrDefault(s => s.dmaAgreementOnBoardingId == accountDetail.dmaAgreementOnBoardingId);
                            if (counterPartyByAgreement != null)
                            {
                                accountDetail.BrokerId = counterPartyByAgreement.BrokerId;
                            }

                            if (accountDetail.onBoardingAccountId == 0)
                            {
                                var existsAccount = onboardingAccounts.FirstOrDefault(x =>
                                    x.dmaAgreementOnBoardingId == accountDetail.dmaAgreementOnBoardingId &&
                                    x.dmaFundOnBoardId == accountDetail.dmaFundOnBoardId &&
                                    x.AccountNumber == accountDetail.AccountNumber);
                                if (existsAccount != null) continue;
                            }
                        }
                        else
                        {
                            accountDetail.dmaFundOnBoardId = funds.FirstOrDefault(x => x.Value == account["Fund Name"]).Key;
                            accountDetail.BrokerId = counterpartyFamilies.FirstOrDefault(x => x.Value == account["Broker"]).Key;
                            if (accountDetail.onBoardingAccountId == 0)
                            {
                                var existsAccount = onboardingAccounts.FirstOrDefault(x => x.dmaFundOnBoardId == accountDetail.dmaFundOnBoardId &&
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
                        accountDetail.SwiftGroup = account["Swift Group"];
                        if (!string.IsNullOrWhiteSpace(accountDetail.SwiftGroup))
                        {
                            var swiftGroup = swiftgroups.FirstOrDefault(x => x.SwiftGroup == accountDetail.SwiftGroup);
                            accountDetail.SendersBIC = (swiftGroup != null) ? swiftGroup.SendersBIC : string.Empty;
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
                                accountDetail.CutoffTime = TimeSpan.Parse(cutoffTime);
                            }

                        }
                        if (!string.IsNullOrWhiteSpace(account["Days to wire per V.D"]))
                        {
                            var wirePerDays = account["Days to wire per V.D"].Replace(" Days", "").Replace(" Day", "").Trim();
                            accountDetail.DaystoWire = Convert.ToInt32(wirePerDays);
                        }

                        if (!string.IsNullOrWhiteSpace(account["Cash Sweep"]) && account["Cash Sweep"] == "Yes")
                        {
                            if (!string.IsNullOrWhiteSpace(account["Holdback Amount"]))
                                accountDetail.HoldbackAmount = Convert.ToDouble(account["Holdback Amount"]);

                            accountDetail.SweepComments = account["Sweep Comments"];
                            accountDetail.AssociatedCustodyAcct = account["Associated Custody Acct"];
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
                        accountDetail.BeneficiaryBICorABA = account["Beneficiary BIC or ABA"];
                        var beneficiaryBiCorAba = accountBicorAba.FirstOrDefault(x => x.IsABA == (accountDetail.BeneficiaryType == "ABA") && x.BICorABA == accountDetail.BeneficiaryBICorABA);
                        if (beneficiaryBiCorAba != null)
                        {
                            accountDetail.BeneficiaryBankName = beneficiaryBiCorAba.BankName;
                            accountDetail.BeneficiaryBankAddress = beneficiaryBiCorAba.BankAddress;
                        }
                        else
                        {
                            accountDetail.BeneficiaryBankName = string.Empty;
                            accountDetail.BeneficiaryBankAddress = string.Empty;
                        }

                        accountDetail.BeneficiaryAccountNumber = account["Beneficiary Account Number"];
                        accountDetail.IntermediaryType = account["Intermediary Beneficiary Type"];
                        accountDetail.IntermediaryBICorABA = account["Intermediary BIC or ABA"];
                        var intermediaryBiCorAba = accountBicorAba.FirstOrDefault(x => x.IsABA == (accountDetail.IntermediaryType == "ABA") && x.BICorABA == accountDetail.IntermediaryBICorABA);
                        if (intermediaryBiCorAba != null)
                        {
                            accountDetail.IntermediaryBankName = intermediaryBiCorAba.BankName;
                            accountDetail.IntermediaryBankAddress = intermediaryBiCorAba.BankAddress;
                        }
                        else
                        {
                            accountDetail.IntermediaryBankName = string.Empty;
                            accountDetail.IntermediaryBankAddress = string.Empty;
                        }

                        accountDetail.IntermediaryAccountNumber = account["Intermediary Account Number"];
                        accountDetail.UltimateBeneficiaryAccountName = account["Ultimate Beneficiary Account Name"];
                        accountDetail.UltimateBeneficiaryType = account["Ultimate Beneficiary Type"];
                        accountDetail.UltimateBeneficiaryBICorABA = account["Ultimate Beneficiary BIC or ABA"];
                        var ultimateBeneficiaryBiCorAba = accountBicorAba.FirstOrDefault(x => x.IsABA == (accountDetail.UltimateBeneficiaryType == "ABA") && x.BICorABA == accountDetail.UltimateBeneficiaryBICorABA);
                        if (ultimateBeneficiaryBiCorAba != null)
                        {
                            accountDetail.UltimateBeneficiaryBankName = ultimateBeneficiaryBiCorAba.BankName;
                            accountDetail.UltimateBeneficiaryBankAddress = ultimateBeneficiaryBiCorAba.BankAddress;
                        }
                        else
                        {
                            accountDetail.UltimateBeneficiaryBankName = string.Empty;
                            accountDetail.UltimateBeneficiaryBankAddress = string.Empty;
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

                        if (accountDetail.dmaFundOnBoardId != 0)
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
            row["Cutoff Time"] = String.Empty;
            row["Days to wire per V.D"] = String.Empty;
            row["Holdback Amount"] = String.Empty;
            row["Sweep Comments"] = String.Empty;
            row["Associated Custody Acct"] = String.Empty;
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
                        templateDetail.onBoardingSSITemplateId = string.IsNullOrWhiteSpace(template["SSI Template Id"]) ? 0 : long.Parse(template["SSI Template Id"]);
                        templateDetail.TemplateTypeId = AccountManager.BrokerTemplateTypeId;
                        templateDetail.TemplateEntityId = counterParties.FirstOrDefault(x => x.Value == template["Legal Entity"]).Key;
                        templateDetail.dmaAgreementTypeId = agreementTypes.FirstOrDefault(x => x.Value == template["Account Type"]).Key;
                        templateDetail.ServiceProvider = template["Service Provider"];
                        templateDetail.Currency = template["Currency"];
                        templateDetail.ReasonDetail = template["Payment/Receipt Reason Detail"];
                        templateDetail.OtherReason = template["Other Reason"];
                        templateDetail.MessageType = template["Message Type"];
                        templateDetail.TemplateName = template["SSI Template Type"] == "Broker" ? template["Legal Entity"] + " - " + template["Account Type"] + " - " + templateDetail.Currency + " - " + template["Payment/Receipt Reason Detail"] : (!string.IsNullOrWhiteSpace(template["SSI Template Type"]) ? template["Service Provider"] + " - " + templateDetail.Currency + " - " + template["Payment/Receipt Reason Detail"] : template["Template Name"]);
                        if (templateDetail.onBoardingSSITemplateId == 0)
                        {
                            var existsTemplate = onboardingSsiTemplate.FirstOrDefault(x => x.TemplateName == templateDetail.TemplateName);
                            if (existsTemplate != null) continue;
                        }
                        templateDetail.BeneficiaryType = template["Beneficiary Type"];
                        templateDetail.BeneficiaryBICorABA = template["Beneficiary BIC or ABA"];
                        var beneficiaryBiCorAba = accountBicorAba.FirstOrDefault(x => x.IsABA == (templateDetail.BeneficiaryType == "ABA") && x.BICorABA == templateDetail.BeneficiaryBICorABA);
                        if (beneficiaryBiCorAba != null)
                        {
                            templateDetail.BeneficiaryBankName = beneficiaryBiCorAba.BankName;
                            templateDetail.BeneficiaryBankAddress = beneficiaryBiCorAba.BankAddress;
                        }
                        else
                        {
                            templateDetail.BeneficiaryBankName = string.Empty;
                            templateDetail.BeneficiaryBankAddress = string.Empty;
                        }

                        templateDetail.BeneficiaryAccountNumber = template["Beneficiary Account Number"];
                        templateDetail.IntermediaryType = template["Intermediary Beneficiary Type"];
                        templateDetail.IntermediaryBICorABA = template["Intermediary BIC or ABA"];
                        var intermediaryBiCorAba = accountBicorAba.FirstOrDefault(x => x.IsABA == (templateDetail.IntermediaryType == "ABA") && x.BICorABA == templateDetail.IntermediaryBICorABA);
                        if (intermediaryBiCorAba != null)
                        {
                            templateDetail.IntermediaryBankName = intermediaryBiCorAba.BankName;
                            templateDetail.IntermediaryBankAddress = intermediaryBiCorAba.BankAddress;
                        }
                        else
                        {
                            templateDetail.IntermediaryBankName = string.Empty;
                            templateDetail.IntermediaryBankAddress = string.Empty;
                        }

                        templateDetail.IntermediaryAccountNumber = template["Intermediary Account Number"];

                        templateDetail.UltimateBeneficiaryType = template["Ultimate Beneficiary Type"];
                        templateDetail.UltimateBeneficiaryBICorABA = template["Ultimate Beneficiary BIC or ABA"];
                        var ultimateBeneficiaryBiCorAba = accountBicorAba.FirstOrDefault(x => x.IsABA == (templateDetail.UltimateBeneficiaryType == "ABA") && x.BICorABA == templateDetail.UltimateBeneficiaryBICorABA);
                        if (ultimateBeneficiaryBiCorAba != null)
                        {
                            templateDetail.UltimateBeneficiaryBankName = ultimateBeneficiaryBiCorAba.BankName;
                            templateDetail.UltimateBeneficiaryBankAddress = ultimateBeneficiaryBiCorAba.BankAddress;
                        }
                        else
                        {
                            templateDetail.UltimateBeneficiaryBankName = string.Empty;
                            templateDetail.UltimateBeneficiaryBankAddress = string.Empty;
                        }

                        templateDetail.UltimateBeneficiaryAccountName = template["Ultimate Beneficiary Account Name"];
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

        private const string DefaultExportFileFormat = ".xlsx";
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

        public FileResult DownloadSsiTemplateFile(string fileName, long ssiTemplateId)
        {
            var file = new FileInfo(string.Format("{0}{1}\\{2}", FileSystemManager.OpsSecureSSITemplateFileUploads, ssiTemplateId, fileName));
            return DownloadFile(file, file.Name);
        }

        /// <summary>
        /// Onboarding ssi template files load into the system
        /// </summary>
        public JsonResult UploadSsiTemplateFiles(long ssiTemplateId)
        {
            var aDocments = new List<onBoardingSSITemplateDocument>();
            if (ssiTemplateId > 0)
            {
                for (var i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    if (file == null)
                        throw new Exception("unable to retrive file information");

                    var fileName = string.Format("{0}{1}\\{2}", FileSystemManager.OpsSecureSSITemplateFileUploads, ssiTemplateId, file.FileName);
                    var fileinfo = new FileInfo(fileName);

                    if (fileinfo.Directory != null && !Directory.Exists(fileinfo.Directory.FullName))
                        Directory.CreateDirectory(fileinfo.Directory.FullName);

                    if (System.IO.File.Exists(fileinfo.FullName))
                    {
                        AccountManager.RemoveSsiTemplateDocument(ssiTemplateId, file.FileName);
                        System.IO.File.Delete(fileinfo.FullName);
                    }

                    file.SaveAs(fileinfo.FullName);

                    //Build ssi template document
                    var document = new onBoardingSSITemplateDocument();
                    document.FileName = file.FileName;
                    document.RecCreatedAt = DateTime.Now;
                    document.RecCreatedBy = User.Identity.Name;
                    document.onBoardingSSITemplateId = ssiTemplateId;

                    AccountManager.AddSsiTemplateDocument(document);

                    aDocments.Add(document);
                }
            }
            return Json(new
            {
                Documents = aDocments.Select(document => new
                {
                    document.onBoardingSSITemplateDocumentId,
                    document.onBoardingSSITemplateId,
                    document.FileName,
                    document.RecCreatedAt,
                    document.RecCreatedBy
                }).ToList()
            }, JsonContentType, JsonContentEncoding);
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

        #region Audit

        // ReSharper disable once FunctionComplexityOverflow
        private List<hmsUserAuditLog> AddAccountAuditLog(onBoardingAccount account, string fundName, string agreement, string broker)
        {
            var auditLogList = new List<hmsUserAuditLog>();

            var type = account.GetType();
            var propertyInfos = typeof(onBoardingAccount).GetProperties(BindingFlags.Public | BindingFlags.Static);


            auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Fund", "Added", "", fundName, UserName));

            if (!string.IsNullOrWhiteSpace(agreement))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Agreement", "Added", "", agreement, UserName));

            if (!string.IsNullOrWhiteSpace(broker))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Broker", "Added", "", broker, UserName));


            foreach (var propertyInfo in propertyInfos)
            {
                if (propertyInfo.GetType() == typeof(long) || propertyInfo.GetType() == typeof(long?))
                    continue;

                var propertyVal = propertyInfo.GetValue(type).ToString();

                if (string.IsNullOrWhiteSpace(propertyVal))
                    continue;


                var auditLog = new hmsUserAuditLog
                {
                    CreatedAt = DateTime.Now,
                    UserName = UserName,
                    Module = "Account",
                    PreviousStateValue = string.Empty,
                    ModifiedStateValue = propertyVal,
                    Action = "Added",
                    Field = propertyInfo.Name,
                    Log = string.Format("Onboarding Name: <i>{0}</i><br/>SSI Template Name: <i>{1}</i>", "Account", account.AccountName)
                };

                auditLogList.Add(auditLog);
            }


            //if (!string.IsNullOrWhiteSpace(account.AccountType))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Account Type", "Added", "", account.AccountType, UserName));


            //if (!string.IsNullOrWhiteSpace(account.AccountNumber))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Account Number", "Added", "", account.AccountNumber, UserName));

            //if (!string.IsNullOrWhiteSpace(account.AccountPurpose))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Account Type", "Added", "", account.AccountPurpose, UserName));

            //if (!string.IsNullOrWhiteSpace(account.AccountStatus))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Account Status", "Added", "", account.AccountStatus, UserName));

            //if (!string.IsNullOrWhiteSpace(account.Currency))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Currency", "Added", "", account.Currency, UserName));

            //if (!string.IsNullOrWhiteSpace(account.Description))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Description", "Added", "", account.Description, UserName));

            //if (!string.IsNullOrWhiteSpace(account.Notes))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Notes", "Added", "", account.Notes, UserName));

            //if (!string.IsNullOrWhiteSpace(account.AuthorizedParty))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "AuthorizedParty", "Added", "", account.AuthorizedParty, UserName));

            //if (!string.IsNullOrWhiteSpace(account.CashInstruction))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Cash Instruction", "Added", "", account.CashInstruction, UserName));

            //if (!string.IsNullOrWhiteSpace(account.CashSweep))
            //{
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Cash Sweep", "Added", "", account.CashSweep, UserName));
            //    if (account.CashSweep == "Yes")
            //    {
            //        if (account.CashSweepTime != null)
            //            auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Cash Sweep", "Added", "", account.CashSweepTime.Value.ToString(), UserName));

            //        if (account.HoldbackAmount.HasValue)
            //            auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Holdback Amount", "Added", "", account.HoldbackAmount.ToString(), UserName));
            //        if (!string.IsNullOrWhiteSpace(account.SweepComments))
            //            auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Sweep Comments", "Added", "", account.SweepComments, UserName));
            //        if (!string.IsNullOrWhiteSpace(account.AssociatedCustodyAcct))
            //            auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Associated Custody Acct", "Added", "", account.AssociatedCustodyAcct, UserName));
            //        if (!string.IsNullOrWhiteSpace(account.PortfolioSelection))
            //            auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Portfolio Selection", "Added", "", account.PortfolioSelection, UserName));
            //        if (!string.IsNullOrWhiteSpace(account.TickerorISIN))
            //            auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Ticker/ISIN", "Added", "", account.TickerorISIN, UserName));
            //        if (!string.IsNullOrWhiteSpace(account.SweepCurrency))
            //            auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Sweep Currency", "Added", "", account.SweepCurrency, UserName));
            //    }
            //}
            //if (account.CutoffTime != null)
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Cutoff Time", "Added", "", account.CutoffTime.ToString(), UserName));

            //if (!string.IsNullOrWhiteSpace(account.ContactType))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Contact Type", "Added", "", account.ContactType, UserName));

            //if (!string.IsNullOrWhiteSpace(account.ContactName))
            //{
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Contact Name", "Added", "", account.ContactName, UserName));
            //}

            //if (!string.IsNullOrWhiteSpace(account.ContactEmail))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Contact Email", "Added", "", account.ContactEmail, UserName));

            //if (!string.IsNullOrWhiteSpace(account.ContactNumber))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Contact Number", "Added", "", account.ContactNumber, UserName));

            //if (!string.IsNullOrWhiteSpace(account.BeneficiaryBICorABA))
            //{
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Beneficiary Type", "Added", "", account.BeneficiaryType, UserName));
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Beneficiary BIC or ABA", "Added", "", account.BeneficiaryBICorABA, UserName));
            //    if (!string.IsNullOrWhiteSpace(account.BeneficiaryBankName))
            //        auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Beneficiary Bank Name", "Added", "", account.BeneficiaryBankName, UserName));
            //    if (!string.IsNullOrWhiteSpace(account.BeneficiaryBankAddress))
            //        auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Beneficiary Bank Address", "Added", "", account.BeneficiaryBankAddress, UserName));
            //}

            //if (!string.IsNullOrWhiteSpace(account.BeneficiaryAccountNumber))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Beneficiary Account Number", "Added", "", account.BeneficiaryAccountNumber, UserName));

            //if (!string.IsNullOrWhiteSpace(account.IntermediaryBICorABA))
            //{
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Intermediary Type", "Added", "", account.IntermediaryType, UserName));
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Intermediary BIC or ABA", "Added", "", account.IntermediaryBICorABA, UserName));
            //    if (!string.IsNullOrWhiteSpace(account.IntermediaryBankName))
            //        auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Intermediary Bank Name", "Added", "", account.IntermediaryBankName, UserName));
            //    if (!string.IsNullOrWhiteSpace(account.IntermediaryBankAddress))
            //        auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Intermediary Bank Address", "Added", "", account.IntermediaryBankAddress, UserName));
            //}

            //if (!string.IsNullOrWhiteSpace(account.IntermediaryAccountNumber))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Intermediary Account Number", "Added", "", account.IntermediaryAccountNumber, UserName));

            //if (!string.IsNullOrWhiteSpace(account.UltimateBeneficiaryBICorABA))
            //{
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Ultimate Beneficiary Type", "Added", "", account.UltimateBeneficiaryType, UserName));
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Ultimate Beneficiary BIC or ABA", "Added", "", account.UltimateBeneficiaryBICorABA, UserName));
            //    if (!string.IsNullOrWhiteSpace(account.UltimateBeneficiaryBankName))
            //        auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Ultimate Beneficiary Bank Name", "Added", "", account.UltimateBeneficiaryBankName, UserName));
            //    if (!string.IsNullOrWhiteSpace(account.UltimateBeneficiaryBankAddress))
            //        auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Ultimate Beneficiary Bank Address", "Added", "", account.UltimateBeneficiaryBankAddress, UserName));
            //}

            //if (!string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountName))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Ultimate Beneficiary Account Name", "Added", "", account.UltimateBeneficiaryAccountName, UserName));

            //if (!string.IsNullOrWhiteSpace(account.FFCName))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "FFC Name", "Added", "", account.FFCName, UserName));
            //if (!string.IsNullOrWhiteSpace(account.FFCNumber))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "FFC Number", "Added", "", account.FFCNumber, UserName));
            //if (!string.IsNullOrWhiteSpace(account.Reference))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Reference", "Added", "", account.Reference, UserName));

            //if (!string.IsNullOrWhiteSpace(account.onBoardingAccountStatus))
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Status", "Added", "", account.onBoardingAccountStatus, UserName));

            return auditLogList;
        }

        // ReSharper disable once FunctionComplexityOverflow
        private List<hmsUserAuditLog> UpdateAccountAuditLog(onBoardingAccount account)
        {
            var auditLogList = new List<hmsUserAuditLog>();
            var nonUpdatedAccount = AccountManager.GetOnBoardingAccount(account.onBoardingAccountId);

            if (account.AccountNumber != nonUpdatedAccount.AccountNumber)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Account Number", "Edited", nonUpdatedAccount.AccountNumber, account.AccountNumber, UserName));

            if (account.Currency != nonUpdatedAccount.Currency)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Currency", "Edited", nonUpdatedAccount.Currency, account.Currency, UserName));

            if (account.Description != nonUpdatedAccount.Description)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Description", "Edited", nonUpdatedAccount.Description, account.Description, UserName));

            if (account.AccountPurpose != nonUpdatedAccount.AccountPurpose)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Account Type", "Edited", nonUpdatedAccount.AccountPurpose, account.AccountPurpose, UserName));

            if (account.AccountStatus != nonUpdatedAccount.AccountStatus)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Account Status", "Edited", nonUpdatedAccount.AccountStatus, account.AccountStatus, UserName));

            if (account.Notes != nonUpdatedAccount.Notes)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Notes", "Edited", nonUpdatedAccount.Notes, account.Notes, UserName));

            if (account.AuthorizedParty != nonUpdatedAccount.AuthorizedParty)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "AuthorizedParty", "Edited", nonUpdatedAccount.AuthorizedParty, account.AuthorizedParty, UserName));

            if (account.CashInstruction != nonUpdatedAccount.CashInstruction)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Cash Instruction", "Edited", nonUpdatedAccount.CashInstruction, account.CashInstruction, UserName));

            if (account.CashSweep != nonUpdatedAccount.CashSweep)
            {
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Cash Sweep", "Edited", nonUpdatedAccount.CashSweep, account.CashSweep, UserName));
            }
            if (account.CashSweep == "Yes")
            {
                if (account.CashSweepTime != nonUpdatedAccount.CashSweepTime)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Cash Sweep", "Edited", (nonUpdatedAccount.CashSweepTime != null ? nonUpdatedAccount.CashSweepTime.Value.ToString() : string.Empty), (account.CashSweepTime != null ? account.CashSweepTime.Value.ToString() : string.Empty), UserName));
                if (account.CashSweepTimeZone != nonUpdatedAccount.CashSweepTimeZone)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Cash Sweep Time Zone", "Edited", nonUpdatedAccount.CashSweepTimeZone, account.CashSweepTimeZone, UserName));
                if (account.HoldbackAmount != nonUpdatedAccount.HoldbackAmount)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Holdback Amount", "Edited", (nonUpdatedAccount.HoldbackAmount.HasValue ? nonUpdatedAccount.HoldbackAmount.ToString() : string.Empty), (account.HoldbackAmount.HasValue ? account.HoldbackAmount.ToString() : string.Empty), UserName));
                if (account.SweepComments != nonUpdatedAccount.SweepComments)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Sweep Comments", "Edited", nonUpdatedAccount.SweepComments, account.SweepComments, UserName));
                if (account.AssociatedCustodyAcct != nonUpdatedAccount.AssociatedCustodyAcct)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Associated Custody Acct", "Edited", nonUpdatedAccount.AssociatedCustodyAcct, account.AssociatedCustodyAcct, UserName));
                if (account.PortfolioSelection != nonUpdatedAccount.PortfolioSelection)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Portfolio Selection", "Edited", nonUpdatedAccount.PortfolioSelection, account.PortfolioSelection, UserName));
                if (account.TickerorISIN != nonUpdatedAccount.TickerorISIN)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Ticker/ISIN", "Edited", nonUpdatedAccount.TickerorISIN, account.TickerorISIN, UserName));
                if (account.SweepCurrency != nonUpdatedAccount.SweepCurrency)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Sweep Currency", "Edited", nonUpdatedAccount.SweepCurrency, account.SweepCurrency, UserName));
            }
            if (account.CutoffTime != nonUpdatedAccount.CutoffTime)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Cutoff Time", "Edited", nonUpdatedAccount.CutoffTime.ToString(), account.CutoffTime.ToString(), UserName));

            if (account.ContactType != nonUpdatedAccount.ContactType)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Contact Type", "Edited", nonUpdatedAccount.ContactType, account.ContactType, UserName));

            //if (account.ContactName != nonUpdatedAccount.ContactName)
            //{
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Contact Name", "Edited", nonUpdatedAccount.ContactName, account.ContactName, UserName));
            //}

            //if (account.ContactEmail != nonUpdatedAccount.ContactEmail)
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Contact Email", "Edited", nonUpdatedAccount.ContactEmail, account.ContactEmail, UserName));

            //if (account.ContactNumber != nonUpdatedAccount.ContactNumber)
            //    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Contact Number", "Edited", nonUpdatedAccount.ContactNumber, account.ContactNumber, UserName));

            if (account.BeneficiaryBICorABA != nonUpdatedAccount.BeneficiaryBICorABA)
            {
                if (account.BeneficiaryType != nonUpdatedAccount.BeneficiaryType)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Beneficiary Type", "Edited", nonUpdatedAccount.BeneficiaryType, account.BeneficiaryType, UserName));

                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Beneficiary BIC or ABA", "Edited", nonUpdatedAccount.BeneficiaryBICorABA, account.BeneficiaryBICorABA, UserName));

                if (account.BeneficiaryBankName != nonUpdatedAccount.BeneficiaryBankName)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Beneficiary Bank Name", "Edited", nonUpdatedAccount.BeneficiaryBankName, account.BeneficiaryBankName, UserName));
                if (account.BeneficiaryBankAddress != nonUpdatedAccount.BeneficiaryBankAddress)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Beneficiary Bank Address", "Edited", nonUpdatedAccount.BeneficiaryBankAddress, account.BeneficiaryBankAddress, UserName));
            }

            if (account.BeneficiaryAccountNumber != nonUpdatedAccount.BeneficiaryAccountNumber)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Beneficiary Account Number", "Edited", nonUpdatedAccount.BeneficiaryAccountNumber, account.BeneficiaryAccountNumber, UserName));

            if (account.IntermediaryBICorABA != nonUpdatedAccount.IntermediaryBICorABA)
            {
                if (account.IntermediaryType != nonUpdatedAccount.IntermediaryType)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Intermediary Type", "Edited", nonUpdatedAccount.IntermediaryType, account.IntermediaryType, UserName));

                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Intermediary BIC or ABA", "Edited", nonUpdatedAccount.IntermediaryBICorABA, account.IntermediaryBICorABA, UserName));
                if (account.IntermediaryBankName != nonUpdatedAccount.IntermediaryBankName)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Intermediary Bank Name", "Edited", nonUpdatedAccount.IntermediaryBankName, account.IntermediaryBankName, UserName));
                if (account.IntermediaryBankAddress != nonUpdatedAccount.IntermediaryBankAddress)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Intermediary Bank Address", "Edited", nonUpdatedAccount.IntermediaryBankAddress, account.IntermediaryBankAddress, UserName));
            }

            if (account.IntermediaryAccountNumber != nonUpdatedAccount.IntermediaryAccountNumber)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Intermediary Account Number", "Edited", nonUpdatedAccount.IntermediaryAccountNumber, account.IntermediaryAccountNumber, UserName));

            if (account.UltimateBeneficiaryBICorABA != nonUpdatedAccount.UltimateBeneficiaryBICorABA)
            {
                if (nonUpdatedAccount.UltimateBeneficiaryType != account.UltimateBeneficiaryType)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Ultimate Beneficiary Type", "Edited", nonUpdatedAccount.UltimateBeneficiaryType, account.UltimateBeneficiaryType, UserName));

                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Ultimate Beneficiary BIC or ABA", "Edited", nonUpdatedAccount.UltimateBeneficiaryBICorABA, account.UltimateBeneficiaryBICorABA, UserName));

                if (account.UltimateBeneficiaryBankName != nonUpdatedAccount.UltimateBeneficiaryBankName)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Ultimate Beneficiary Bank Name", "Edited", nonUpdatedAccount.UltimateBeneficiaryBankName, account.UltimateBeneficiaryBankName, UserName));

                if (account.UltimateBeneficiaryBankAddress != nonUpdatedAccount.UltimateBeneficiaryBankAddress)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Ultimate Beneficiary Bank Address", "Edited", nonUpdatedAccount.UltimateBeneficiaryBankAddress, account.UltimateBeneficiaryBankAddress, UserName));
            }

            if (account.UltimateBeneficiaryAccountName != nonUpdatedAccount.UltimateBeneficiaryAccountName)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Ultimate Beneficiary Account Name", "Edited", nonUpdatedAccount.UltimateBeneficiaryAccountName, account.UltimateBeneficiaryAccountName, UserName));

            if (account.FFCName != nonUpdatedAccount.FFCName)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "FFC Name", "Edited", nonUpdatedAccount.FFCName, account.FFCName, UserName));
            if (account.FFCNumber != nonUpdatedAccount.FFCNumber)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "FFC Number", "Edited", nonUpdatedAccount.FFCNumber, account.FFCNumber, UserName));
            if (account.Reference != nonUpdatedAccount.Reference)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Reference", "Edited", nonUpdatedAccount.Reference, account.Reference, UserName));

            if (account.onBoardingAccountStatus != nonUpdatedAccount.onBoardingAccountStatus)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("Account", account.AccountName, "Status", "Edited", "", "Created", UserName));
            return auditLogList;
        }

        // ReSharper disable once FunctionComplexityOverflow
        private List<hmsUserAuditLog> AddSsiTemplateAuditLog(onBoardingSSITemplate ssiTemplate, string accountType, string broker)
        {
            var auditLogList = new List<hmsUserAuditLog>();

            auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "SSITemplate Type", "Added", "", ssiTemplate.SSITemplateType, UserName));

            if (!string.IsNullOrWhiteSpace(broker))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Broker", "Added", "", broker, UserName));

            if (!string.IsNullOrWhiteSpace(accountType))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Account Type", "Added", "", accountType, UserName));

            if (!string.IsNullOrWhiteSpace(ssiTemplate.ServiceProvider))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Service Provider", "Added", "", ssiTemplate.ServiceProvider, UserName));

            if (!string.IsNullOrWhiteSpace(ssiTemplate.Currency))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Currency", "Added", "", ssiTemplate.Currency, UserName));

            if (!string.IsNullOrWhiteSpace(ssiTemplate.ReasonDetail))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Reason Detail", "Added", "", ssiTemplate.ReasonDetail, UserName));

            if (!string.IsNullOrWhiteSpace(ssiTemplate.OtherReason))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Other Reason", "Added", "", ssiTemplate.OtherReason, UserName));

            if (!string.IsNullOrWhiteSpace(ssiTemplate.AccountName))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Account Name", "Added", "", ssiTemplate.AccountName, UserName));

            if (!string.IsNullOrWhiteSpace(ssiTemplate.BeneficiaryBICorABA))
            {
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Beneficiary Type", "Added", "", ssiTemplate.BeneficiaryType, UserName));
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Beneficiary BIC or ABA", "Added", "", ssiTemplate.BeneficiaryBICorABA, UserName));
                if (!string.IsNullOrWhiteSpace(ssiTemplate.BeneficiaryBankName))
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Beneficiary Bank Name", "Added", "", ssiTemplate.BeneficiaryBankName, UserName));
                if (!string.IsNullOrWhiteSpace(ssiTemplate.BeneficiaryBankAddress))
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Beneficiary Bank Address", "Added", "", ssiTemplate.BeneficiaryBankAddress, UserName));
            }

            if (!string.IsNullOrWhiteSpace(ssiTemplate.BeneficiaryAccountNumber))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Beneficiary Account Number", "Added", "", ssiTemplate.BeneficiaryAccountNumber, UserName));

            if (!string.IsNullOrWhiteSpace(ssiTemplate.IntermediaryBICorABA))
            {
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Intermediary Type", "Added", "", ssiTemplate.IntermediaryType, UserName));
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Intermediary BIC or ABA", "Added", "", ssiTemplate.IntermediaryBICorABA, UserName));
                if (!string.IsNullOrWhiteSpace(ssiTemplate.IntermediaryBankName))
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Intermediary Bank Name", "Added", "", ssiTemplate.IntermediaryBankName, UserName));
                if (!string.IsNullOrWhiteSpace(ssiTemplate.IntermediaryBankAddress))
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Intermediary Bank Address", "Added", "", ssiTemplate.IntermediaryBankAddress, UserName));
            }



            if (!string.IsNullOrWhiteSpace(ssiTemplate.IntermediaryAccountNumber))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Intermediary Account Number", "Added", "", ssiTemplate.IntermediaryAccountNumber, UserName));

            if (!string.IsNullOrWhiteSpace(ssiTemplate.UltimateBeneficiaryBICorABA))
            {
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Ultimate Beneficiary Type", "Added", "", ssiTemplate.UltimateBeneficiaryType, UserName));
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Ultimate Beneficiary BIC or ABA", "Added", "", ssiTemplate.UltimateBeneficiaryBICorABA, UserName));
                if (!string.IsNullOrWhiteSpace(ssiTemplate.UltimateBeneficiaryBankName))
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Ultimate Beneficiary Bank Name", "Added", "", ssiTemplate.UltimateBeneficiaryBankName, UserName));
                if (!string.IsNullOrWhiteSpace(ssiTemplate.UltimateBeneficiaryBankAddress))
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Ultimate Beneficiary Bank Address", "Added", "", ssiTemplate.UltimateBeneficiaryBankAddress, UserName));
            }
            if (!string.IsNullOrWhiteSpace(ssiTemplate.AccountNumber))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Ultimate Beneficiary Account Number", "Added", "", ssiTemplate.AccountNumber, UserName));
            if (!string.IsNullOrWhiteSpace(ssiTemplate.UltimateBeneficiaryAccountName))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Ultimate Beneficiary Account Name", "Added", "", ssiTemplate.UltimateBeneficiaryAccountName, UserName));
            if (!string.IsNullOrWhiteSpace(ssiTemplate.FFCName))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "FFC Name", "Added", "", ssiTemplate.FFCName, UserName));
            if (!string.IsNullOrWhiteSpace(ssiTemplate.FFCNumber))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "FFC Number", "Added", "", ssiTemplate.FFCNumber, UserName));
            if (!string.IsNullOrWhiteSpace(ssiTemplate.Reference))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Reference", "Added", "", ssiTemplate.Reference, UserName));

            if (!string.IsNullOrWhiteSpace(ssiTemplate.SSITemplateStatus))
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Status", "Added", "", "Saved As Draft", UserName));

            return auditLogList;
        }

        // ReSharper disable once FunctionComplexityOverflow
        private List<hmsUserAuditLog> UpdateSsiTemplateAuditLog(onBoardingSSITemplate ssiTemplate, string accountType, string broker)
        {
            var auditLogList = new List<hmsUserAuditLog>();

            var nonUpdatedSsiTemplate = AccountManager.GetSsiTemplate(ssiTemplate.onBoardingSSITemplateId);

            if (!string.IsNullOrWhiteSpace(broker) && ssiTemplate.TemplateEntityId != nonUpdatedSsiTemplate.TemplateEntityId)
            {
                var nonUpdatedBroker = OnBoardingDataManager.GetCounterpartyFamilyName(nonUpdatedSsiTemplate.TemplateEntityId);
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Broker", "Edited", nonUpdatedBroker, broker, UserName));
            }

            if (!string.IsNullOrWhiteSpace(accountType) && ssiTemplate.dmaAgreementTypeId != nonUpdatedSsiTemplate.dmaAgreementTypeId)
            {
                var nonUpdatedAccountType = OnBoardingDataManager.GetAllAgreementTypes().FirstOrDefault(x => x.Key == nonUpdatedSsiTemplate.dmaAgreementTypeId);
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Account Type", "Edited", nonUpdatedAccountType.Value, accountType, UserName));
            }

            if (!string.IsNullOrWhiteSpace(ssiTemplate.ServiceProvider) && ssiTemplate.ServiceProvider != nonUpdatedSsiTemplate.ServiceProvider)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Service Provider", "Edited", nonUpdatedSsiTemplate.ServiceProvider, ssiTemplate.ServiceProvider, UserName));

            if (ssiTemplate.Currency != nonUpdatedSsiTemplate.Currency)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.AccountName, "Currency", "Edited", nonUpdatedSsiTemplate.Currency, ssiTemplate.Currency, UserName));

            if (ssiTemplate.ReasonDetail != nonUpdatedSsiTemplate.ReasonDetail)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Reason Detail", "Edited", nonUpdatedSsiTemplate.ReasonDetail, ssiTemplate.ReasonDetail, UserName));

            if (!string.IsNullOrWhiteSpace(ssiTemplate.OtherReason) && ssiTemplate.OtherReason != nonUpdatedSsiTemplate.OtherReason)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Other Reason", "Edited", nonUpdatedSsiTemplate.OtherReason, ssiTemplate.OtherReason, UserName));



            if (ssiTemplate.BeneficiaryBICorABA != nonUpdatedSsiTemplate.BeneficiaryBICorABA)
            {
                if (nonUpdatedSsiTemplate.BeneficiaryType != ssiTemplate.BeneficiaryType)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Beneficiary Type", "Edited", nonUpdatedSsiTemplate.BeneficiaryType, ssiTemplate.BeneficiaryType, UserName));

                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Beneficiary BIC or ABA", "Edited", nonUpdatedSsiTemplate.BeneficiaryBICorABA, ssiTemplate.BeneficiaryBICorABA, UserName));

                if (ssiTemplate.BeneficiaryBankName != nonUpdatedSsiTemplate.BeneficiaryBankName)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Beneficiary Bank Name", "Edited", nonUpdatedSsiTemplate.BeneficiaryBankName, ssiTemplate.BeneficiaryBankName, UserName));
                if (ssiTemplate.BeneficiaryBankAddress != nonUpdatedSsiTemplate.BeneficiaryBankAddress)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Beneficiary Bank Address", "Edited", nonUpdatedSsiTemplate.BeneficiaryBankAddress, ssiTemplate.BeneficiaryBankAddress, UserName));
            }

            if (ssiTemplate.BeneficiaryAccountNumber != nonUpdatedSsiTemplate.BeneficiaryAccountNumber)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Beneficiary Account Number", "Edited", nonUpdatedSsiTemplate.BeneficiaryAccountNumber, ssiTemplate.BeneficiaryAccountNumber, UserName));

            if (ssiTemplate.IntermediaryBICorABA != nonUpdatedSsiTemplate.IntermediaryBICorABA)
            {
                if (nonUpdatedSsiTemplate.IntermediaryType != ssiTemplate.IntermediaryType)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Intermediary Type", "Edited", nonUpdatedSsiTemplate.IntermediaryType, ssiTemplate.IntermediaryType, UserName));

                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Intermediary BIC or ABA", "Edited", nonUpdatedSsiTemplate.IntermediaryBICorABA, ssiTemplate.IntermediaryBICorABA, UserName));
                if (ssiTemplate.IntermediaryBankName != nonUpdatedSsiTemplate.IntermediaryBankName)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Intermediary Bank Name", "Edited", nonUpdatedSsiTemplate.IntermediaryBankName, ssiTemplate.IntermediaryBankName, UserName));
                if (ssiTemplate.IntermediaryBankAddress != nonUpdatedSsiTemplate.IntermediaryBankAddress)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Intermediary Bank Address", "Edited", nonUpdatedSsiTemplate.IntermediaryBankAddress, ssiTemplate.IntermediaryBankAddress, UserName));
            }

            if (ssiTemplate.IntermediaryAccountNumber != nonUpdatedSsiTemplate.IntermediaryAccountNumber)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Intermediary Account Number", "Edited", nonUpdatedSsiTemplate.IntermediaryAccountNumber, ssiTemplate.IntermediaryAccountNumber, UserName));

            if (ssiTemplate.UltimateBeneficiaryBICorABA != nonUpdatedSsiTemplate.UltimateBeneficiaryBICorABA)
            {
                if (nonUpdatedSsiTemplate.UltimateBeneficiaryType != ssiTemplate.UltimateBeneficiaryType)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Ultimate Beneficiary Type", "Edited", nonUpdatedSsiTemplate.UltimateBeneficiaryType, ssiTemplate.UltimateBeneficiaryType, UserName));

                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Ultimate Beneficiary BIC or ABA", "Edited", nonUpdatedSsiTemplate.UltimateBeneficiaryBICorABA, ssiTemplate.UltimateBeneficiaryBICorABA, UserName));

                if (ssiTemplate.UltimateBeneficiaryBankName != nonUpdatedSsiTemplate.UltimateBeneficiaryBankName)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Ultimate Beneficiary Bank Name", "Edited", nonUpdatedSsiTemplate.UltimateBeneficiaryBankName, ssiTemplate.UltimateBeneficiaryBankName, UserName));

                if (ssiTemplate.UltimateBeneficiaryBankAddress != nonUpdatedSsiTemplate.UltimateBeneficiaryBankAddress)
                    auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Ultimate Beneficiary Bank Address", "Edited", nonUpdatedSsiTemplate.UltimateBeneficiaryBankAddress, ssiTemplate.UltimateBeneficiaryBankAddress, UserName));
            }

            if (ssiTemplate.AccountNumber != nonUpdatedSsiTemplate.AccountNumber)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Ultimate Beneficiary Account Number", "Edited", nonUpdatedSsiTemplate.AccountNumber, ssiTemplate.AccountNumber, UserName));

            if (ssiTemplate.UltimateBeneficiaryAccountName != nonUpdatedSsiTemplate.UltimateBeneficiaryAccountName)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.AccountName, "Ultimate Beneficiary Account Name", "Edited", nonUpdatedSsiTemplate.UltimateBeneficiaryAccountName, ssiTemplate.UltimateBeneficiaryAccountName, UserName));

            if (ssiTemplate.FFCName != nonUpdatedSsiTemplate.FFCName)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "FFC Name", "Edited", nonUpdatedSsiTemplate.FFCName, ssiTemplate.FFCName, UserName));
            if (ssiTemplate.FFCNumber != nonUpdatedSsiTemplate.FFCNumber)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "FFC Number", "Edited", nonUpdatedSsiTemplate.FFCNumber, ssiTemplate.FFCNumber, UserName));
            if (ssiTemplate.Reference != nonUpdatedSsiTemplate.Reference)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Reference", "Edited", nonUpdatedSsiTemplate.Reference, ssiTemplate.Reference, UserName));

            if (ssiTemplate.SSITemplateStatus != nonUpdatedSsiTemplate.SSITemplateStatus)
                auditLogList.Add(AuditManager.BuildOnboardingAuditLog("SSITemplate", ssiTemplate.TemplateName, "Status", "Edited", "", "Created", UserName));
            return auditLogList;
        }

        #endregion

    }
}