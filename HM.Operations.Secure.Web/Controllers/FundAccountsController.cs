using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.Operations.FileParseEngine.Models;
using ExcelUtility.Operations.ManagedAccounts;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware;
using HM.Operations.Secure.Middleware.Models;
using HM.Operations.Secure.Middleware.Util;
using HM.Operations.Secure.Web.Utility;

namespace HM.Operations.Secure.Web.Controllers
{
    public class FundAccountsController : WireUserBaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult BankAddress()
        {
            return View();
        }

        public JsonResult GetAccountPreloadData()
        {

            var hFunds = AuthorizedDMAFundData;

            var funds = hFunds.Select(s => new { id = s.HmFundId, text = s.PreferredFundName, LegalName = s.LegalFundName }).ToList();
            var hFundIds = funds.Select(s => s.id).ToList();


            var counterpartyFamilies = OnBoardingDataManager.GetAllCounterparties().Select(x => new
            {
                id = x.CounterpartyId,
                text = x.CounterpartyName,
                familyId = x.CounterpartyFamilyId,
                familyText = x.CounterpartyFamilyName
            }).OrderBy(x => x.text).ToList();

            var agreementData = OnBoardingDataManager.GetAgreementsForOnboardingAccountPreloadData(hFundIds, AuthorizedSessionData.IsPrivilegedUser);
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
            var agreementTypes = OnBoardingDataManager.GetAllAgreementTypes(new List<string> { "DDA", "Custody" });
            return Json(new
            {
                agreements,
                funds,
                fundsWithAgreements,
                counterpartyFamilies,
                ddaAgreementTypeId = agreementTypes.First(s => s.Value == "DDA").Key,
                custodyAgreementTypeId = agreementTypes.First(s => s.Value == "Custody").Key,
            });

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
                    report = FileSystemManager.GetReportName(choice.dmaReportsId),
                }).OrderBy(x => x.text).ToList()
            }, JsonContentType, JsonContentEncoding);
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
                contactName = $"{lastname} {firstname}";

            return contactName;

        }

        public JsonResult GetOnBoardingAccount(long accountId)
        {
            var onBoardingAccount = FundAccountManager.GetOnBoardingAccount(accountId);
            var registedAddress = WireDataManager.GetFundRegistedAddress(onBoardingAccount.hmFundId);
            return Json(new
            {
                OnBoardingAccount = onBoardingAccount,
                isAuthorizedUserToApprove = (User.IsWireApprover() && onBoardingAccount.onBoardingAccountStatus == "Pending Approval" && onBoardingAccount.CreatedBy != UserName && onBoardingAccount.UpdatedBy != UserName),
                registedAddress
            }, JsonContentType, JsonContentEncoding);
        }



        public JsonResult GetAccountSsiTemplateMap(long accountId, long fundId, string currency, string messages)
        {
            var ssiTemplateMaps = FundAccountManager.GetAccountSsiTemplateMap(accountId);
            var counterpartyIds = OnBoardingDataManager.GetCounterpartyIdsbyFund(fundId);
            var fundData = AuthorizedDMAFundData.FirstOrDefault(s => s.HmFundId == fundId) ?? new HFundBasic();

            if (string.IsNullOrWhiteSpace(messages))
                messages = string.Empty;

            var messageTypes = messages.Split(',').ToList();
            var shouldBringAllMessageTypes = string.IsNullOrWhiteSpace(messages);
            List<onBoardingSSITemplate> ssiTemplates;

            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                ssiTemplates = context.onBoardingSSITemplates.Where(template => !template.IsDeleted && template.SSITemplateStatus == "Approved"
                   && ((template.SSITemplateType == "Bank Loan/Private/IPO" && fundData.IsFundAllowedForBankLoanAndIpOs) || counterpartyIds.Contains(template.TemplateEntityId) || template.SSITemplateType == "Fee/Expense Payment")
                   && (currency == null || template.Currency == currency) && (shouldBringAllMessageTypes || messageTypes.Contains(template.MessageType))).ToList();
            }

            var alreadyAssociatedSsIs = ssiTemplateMaps.Select(s => s.onBoardingSSITemplateId).Distinct().ToList();
            var availableSsiTemplates = ssiTemplates.Where(s => !alreadyAssociatedSsIs.Contains(s.onBoardingSSITemplateId)).ToList();

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
                ssiTemplates = availableSsiTemplates,
            }, JsonContentType, JsonContentEncoding);
        }


        public JsonResult GetSsiTemplateAccountMap(long ssiTemplateId, long brokerId, string currency, string message, string ssiTemplateType)
        {
            var isServiceType = ssiTemplateType == "Fee/Expense Payment";
            var isBankLoanOrIpo = ssiTemplateType == "Bank Loan/Private/IPO";
            var isBrokerType = ssiTemplateType == "Broker";

            var hmFundIds = isBrokerType ? OnBoardingDataManager.GetFundIdsbyCounterparty(brokerId)
                : isBankLoanOrIpo ? AuthorizedDMAFundData.Where(s => s.IsFundAllowedForBankLoanAndIpOs).Select(s => s.HmFundId).ToList()
                : new List<long>();

            if (string.IsNullOrWhiteSpace(message))
                message = string.Empty;

            List<onBoardingAccount> fundAccounts;
            List<onBoardingAccountSSITemplateMap> ssiTemplateMaps;
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                fundAccounts = (from account in context.onBoardingAccounts
                                join swift in context.hmsSwiftGroups on account.SwiftGroupId equals swift.hmsSwiftGroupId
                                where !account.IsDeleted && account.onBoardingAccountStatus == "Approved" && account.AccountStatus != "Closed"
                                      && (isServiceType || hmFundIds.Contains(account.hmFundId))
                                      && (currency == null || account.Currency == currency)
                                      && swift.AcceptedMessages.Contains(message)
                                select account).ToList();

                ssiTemplateMaps = context.onBoardingAccountSSITemplateMaps.Where(x => x.onBoardingSSITemplateId == ssiTemplateId).ToList();
            }

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
            var fundAccounts = FundAccountManager.GetFundAccountDetails(hmFundIds, AuthorizedSessionData.IsPrivilegedUser);
            var fundAccountmap = fundAccounts.ToDictionary(s => s.onBoardingAccountId, v => v);
            var agreementTypes = OnBoardingDataManager.GetAllAgreementTypes();
            var clearingBrokers = FundAccountManager.GetAllClearingBrokers();

            var allFundAccounts = onBoardingAccounts.AsParallel().Select(account =>
            {
                account.CreatedBy = account.CreatedBy.HumanizeEmail();
                account.UpdatedBy = account.UpdatedBy.HumanizeEmail();
                account.ApprovedBy = account.ApprovedBy.HumanizeEmail();
                var fundAccMap = fundAccountmap.ContainsKey(account.onBoardingAccountId) ? fundAccountmap[account.onBoardingAccountId] : new FundAccountData();
                return new
                {
                    Account = FundAccountManager.SetAccountDefaults(account),
                    AccountNumber = fundAccMap.AccountNumber,
                    AgreementName = fundAccMap.AgreementShortName,
                    AgreementTypeId = fundAccMap.AccountType == "DDA" || fundAccMap.AccountType == "Custody"
                            ? agreementTypes.ContainsValue(fundAccMap.AccountType) ? agreementTypes.First(s => s.Value == fundAccMap.AccountType).Key
                            : fundAccMap.dmaAgreementTypeId ?? 0 : fundAccMap.dmaAgreementTypeId ?? 0,

                    CounterpartyFamilyName = fundAccMap.CounterpartyFamily,
                    CounterpartyName = fundAccMap.CounterpartyName,
                    FundName = fundAccMap.LegalFundName,
                    ClientName = fundAccMap.ClientName,
                    FundStatus = fundAccMap.LaunchStatus,
                    ApprovedMaps = account.onBoardingAccountSSITemplateMaps.Count(s => s.Status == "Approved"),
                    PendingApprovalMaps = account.onBoardingAccountSSITemplateMaps.Count(s => s.Status == "Pending Approval"),
                };
            }).ToList();

            foreach (var clearingBroker in clearingBrokers)
            {
                var fundAccount = allFundAccounts.FirstOrDefault(s => s.Account.onBoardingAccountId == clearingBroker.ClearingBroker.onBoardingAccountId);
                clearingBroker.AgreementName = fundAccount?.AgreementName;
                clearingBroker.AccountNumber = fundAccount?.AccountNumber;
                clearingBroker.CounterpartyName = fundAccount?.CounterpartyName;
                clearingBroker.ClientName = fundAccount?.ClientName;
                clearingBroker.AccountName = fundAccount?.AccountNumber;
                clearingBroker.FundName = fundAccount?.FundName;
                clearingBroker.Currency = fundAccount?.Account?.Currency;
            }
            clearingBrokers = clearingBrokers.Where(s => s.AccountName != null).ToList();
            SetSessionValue(OpsSecureSessionVars.ClearingBrokersData.ToString(), clearingBrokers);
            return Json(new
            {
                agreementTypes = agreementTypes.Select(x => new { id = x.Key, text = x.Value }).OrderBy(x => x.text).ToList(),
                receivingAccountTypes = OpsSecureSwitches.AllowedAgreementTypesForReceivingFundAccounts,
                OnBoardingAccounts = allFundAccounts,
                FundAccountClearingBrokers= clearingBrokers
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
            var currenciesChoices = FundAccountManager.GetAllCurrencies().Select(currency => new { id = currency, text = currency }).ToList();
            return Json(new
            {
                currencies = currenciesChoices
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

        public JsonResult GetAccountClearingBrokers(long accountId)
        {
            List<hmsFundAccountClearingBroker> clearingBrokers;
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                clearingBrokers = context.hmsFundAccountClearingBrokers.Where(s => s.onBoardingAccountId == accountId).AsNoTracking().ToList();
            }

            return Json(clearingBrokers, JsonContentType, JsonContentEncoding);
        }

        public void AddOrUpdateMarginExposureType(long accountId, int exposureTypeId)
        {
            using (var context = new OperationsSecureContext())
            {
                var account = context.onBoardingAccounts.FirstOrDefault(s => s.onBoardingAccountId == accountId);

                if (account == null)
                    return;

                account.MarginExposureTypeID = exposureTypeId;
                context.onBoardingAccounts.AddOrUpdate(account);
            }
        }


        public string AddClearingBrokers(long accountId, string clearingBrokerName)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                var clearingBroker = context.hmsFundAccountClearingBrokers.FirstOrDefault(s => s.ClearingBrokerName == clearingBrokerName);
                if (clearingBroker != null)
                    return context.onBoardingAccounts.FirstOrDefault(s => s.onBoardingAccountId == clearingBroker.onBoardingAccountId).AccountName;
                
                context.hmsFundAccountClearingBrokers.Add(new hmsFundAccountClearingBroker()
                {
                    ClearingBrokerName = clearingBrokerName,
                    onBoardingAccountId = accountId,
                    RecCreatedAt = DateTime.Now,
                    RecCreatedById = UserId,
                });
                context.SaveChanges();
                return string.Empty;
            }
        }


        public void DeleteClearingBrokers(long clearingBrokerId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                var clearingBroker = context.hmsFundAccountClearingBrokers.FirstOrDefault(s => s.hmsFundAccountClearingBrokerId == clearingBrokerId);
                if (clearingBroker != null) context.hmsFundAccountClearingBrokers.Remove(clearingBroker);
                context.SaveChanges();
            }
        }

        public void AddOrUpdateCallback(hmsAccountCallback callback)
        {
            if (callback.hmsAccountCallbackId > 0)
            {
                var existingCallback = FundAccountManager.GetCallbackData(callback.hmsAccountCallbackId);
                callback.RecCreatedDt = existingCallback.RecCreatedDt;
                callback.RecCreatedBy = existingCallback.RecCreatedBy;
            }
            else
            {
                callback.RecCreatedBy = UserName;
                callback.RecCreatedDt = DateTime.Now;
                FundAccountManager.UpdateIsKeyFieldsChanged(callback.onBoardingAccountId);
            }

            FundAccountManager.AddOrUpdateCallback(callback);
        }

        public JsonResult GetAllAccountBicorAba()
        {
            var accountBicorAba = FundAccountManager.GetAllAccountBicorAba().Select(s => new
            {
                s.onBoardingAccountBICorABAId,
                s.BICorABA,
                s.BankAddress,
                s.BankName,
                s.IsABA,
                s.CreatedAt,
                s.CreatedBy,
                s.UpdatedAt,
                s.UpdatedBy
            });
            return Json(new
            {
                accountBicorAba
            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult GetAllBankAccountAddress()
        {
            var addressList = FundAccountManager.GetAllBankAccountAddress();
            return Json(new
            {
                addressList
            }, JsonContentType, JsonContentEncoding);
        }

        public void AddorEditBankAccountAddress(hmsBankAccountAddress accountAddress)
        {
            if (accountAddress.hmsBankAccountAddressId == 0)
            {
                accountAddress.IsDeleted = false;
                accountAddress.CreatedAt = DateTime.Now;
                accountAddress.CreatedBy = UserName;
            }
            accountAddress.UpdatedAt = DateTime.Now;
            accountAddress.UpdatedBy = UserName;

            FundAccountManager.AddorUpdateAccountAddress(accountAddress);
        }

        public void AddorEditAccountBiCorAba(onBoardingAccountBICorABA accountBiCorAba)
        {
            if (accountBiCorAba.onBoardingAccountBICorABAId == 0)
            {
                accountBiCorAba.IsDeleted = false;
                accountBiCorAba.CreatedAt = DateTime.Now;
                accountBiCorAba.CreatedBy = UserName;
            }
            accountBiCorAba.UpdatedAt = DateTime.Now;
            accountBiCorAba.UpdatedBy = UserName;

            FundAccountManager.AddorUpdateAccountBiCorAba(accountBiCorAba);
        }

        public JsonResult DeleteAccountBiCorAba(long onBoardingAccountBICorABAId)
        {
            var isDeleted = FundAccountManager.DeleteAccountBiCorAba(onBoardingAccountBICorABAId);
            return Json(new
            {
                isDeleted
            }, JsonContentType, JsonContentEncoding);
        }

        public JsonResult DeleteAccountAddress(long hmsBankAccountAddressId)
        {
            var isDeleted = FundAccountManager.DeleteAccountAddress(hmsBankAccountAddressId);
            return Json(new
            {
                isDeleted
            }, JsonContentType, JsonContentEncoding);
        }

        public void UpdateContacts(long accountId,string contactType, string contactName)
        {
            FundAccountManager.UpdateContacts(accountId, contactType,contactName, UserName);
        }

        public void UpdateTreasuryMarginCheck(long accountId, bool isExcludedFromTreasuryMarginCheck)
        {
            FundAccountManager.UpdateTreasuryMarginCheck(accountId, isExcludedFromTreasuryMarginCheck, UserName);
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
            if (documentId > 0)
                FundAccountManager.RemoveAccountDocument(documentId);
        }

        public bool IsAccountDocumentExists(long accountId)
        {
            return FundAccountManager.IsAccountDocumentExists(accountId);
        }


        #region ExportExport and Upload

        public FileResult ExportAllAccountlist()
        {

            var hmFundIds = AuthorizedSessionData.HMFundIds.Where(s => s.Level > 0).Select(s => s.Id).ToList();
            var onBoardingAccounts = FundAccountManager.GetAllOnBoardingAccounts(hmFundIds, AuthorizedSessionData.IsPrivilegedUser).OrderByDescending(x => x.UpdatedAt).ToList();
            var fundAccounts = FundAccountManager.GetFundAccountDetails(hmFundIds, AuthorizedSessionData.IsPrivilegedUser);


            var accountListRows = BuildAccountRows(onBoardingAccounts, fundAccounts);
            //File name and path

            var contentToExport = new Dictionary<string, List<Row>>() { { "List of Accounts", accountListRows } };
            var fileName = $"AccountList_{DateTime.Now:yyyyMMdd}";
            var exportFileInfo = new FileInfo(
                $"{FileSystemManager.UploadTemporaryFilesPath}{fileName}{DefaultExportFileFormat}");
            //Export the checklist file

            ReportDeliveryManager.CreateExportFile(contentToExport, exportFileInfo);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        public FileResult ExportAccountBICorABAlist()
        {
            var accountBicorAbaList = FundAccountManager.GetAllAccountBicorAba();
            var accountListRows = BuildAccountBICorABAExportRows(accountBicorAbaList);

            var contentToExport = new Dictionary<string, List<Row>>() { { "List of AccountBICorABA", accountListRows } };
            var fileName = $"AccountBICorABAList_{DateTime.Now:yyyyMMdd}";
            var exportFileInfo = new FileInfo(
                $"{FileSystemManager.UploadTemporaryFilesPath}{fileName}{DefaultExportFileFormat}");

            ReportDeliveryManager.CreateExportFile(contentToExport, exportFileInfo, true);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        private List<Row> BuildAccountBICorABAExportRows(List<onBoardingAccountBICorABA> accountBicOrAbaList)
        {
            return accountBicOrAbaList.Select(account => new Row
            {
                ["Type"] = account.IsABA ? "ÄBA" : "BIC",
                ["BICorABA"] = account.BICorABA,
                ["BankName"] = account.BankName,
                ["BankAddress"] = account.BankAddress,
                ["CreatedBy"] = account.CreatedBy,
                ["CreatedAt"] = account.CreatedAt + "",
                ["UpdatedBy"] = account.UpdatedBy,
                ["UpdatedAt"] = account.UpdatedAt + ""
            }).ToList();
        }

        public FileResult ExportBankAccountlist()
        {
            var accountList = FundAccountManager.GetAllBankAccountAddress();
            var accountListRows = BuildBankAccountExportRows(accountList);

            var contentToExport = new Dictionary<string, List<Row>>() { { "List of Account Address", accountListRows } };
            var fileName = $"Bank Account Address List_{DateTime.Now:yyyyMMdd}";
            var exportFileInfo = new FileInfo(
                $"{FileSystemManager.UploadTemporaryFilesPath}{fileName}{DefaultExportFileFormat}");

            ReportDeliveryManager.CreateExportFile(contentToExport, exportFileInfo, true);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        public FileResult ExportClearingBrokerslist()
        {
            var clearingBrokers = (List<FundAccountClearingBrokerData>)GetSessionValue(OpsSecureSessionVars.ClearingBrokersData.ToString());
            var exportContent = CreateClearingBrokersExportContent(clearingBrokers);
            var contentToExport = new List<ExportContent>();
            contentToExport.Add(exportContent);
            var fileName = $"Associated Clearing Brokers_{DateTime.Now:yyyyMMdd}";
            var exportFileInfo = new FileInfo(
                $"{FileSystemManager.UploadTemporaryFilesPath}{fileName}{DefaultExportFileFormat}");
            Exporter.CreateExcelFile(contentToExport, exportFileInfo.FullName);

            return DownloadAndDeleteFile(exportFileInfo);
        }

        private List<Row> BuildBankAccountExportRows(List<hmsBankAccountAddress> accountList)
        {
            var accountRows = new List<Row>();

            foreach (var account in accountList)
            {
                var row = new Row
                {
                    ["AccountName"] = account.AccountName,
                    ["AccountAddress"] = account.AccountAddress,
                    ["CreatedBy"] = account.CreatedBy,
                    ["CreatedAt"] = account.CreatedAt + "",
                    ["UpdatedBy"] = account.UpdatedBy,
                    ["UpdatedAt"] = account.UpdatedAt + ""
                };

                accountRows.Add(row);
            }

            return accountRows;
        }


        private List<Row> BuildClearingBrokersExportRows(List<FundAccountClearingBrokerData> clearingBrokers)
        {
            var clearingBrokerRows = new List<Row>();

            foreach (var clearingBroker in clearingBrokers)
            {
                var row = new Row
                {
                    ["Clearing Broker Name"] = clearingBroker.ClearingBroker.ClearingBrokerName,
                    ["Counterparty"] = clearingBroker.CounterpartyName,
                    ["Agreement Name"] = clearingBroker.AgreementName,
                    ["Account Type"] = clearingBroker.AccountType,
                    ["Account Number"] = clearingBroker.AccountNumber,
                    ["FFC Name"] = clearingBroker.FFCName,
                    ["FFC Number"] = clearingBroker.FFCNumber,
                    ["CreatedBy"] = clearingBroker.RecCreatedBy,
                    ["CreatedAt"] = clearingBroker.ClearingBroker.RecCreatedAt + ""
                };
                clearingBrokerRows.Add(row);
            }
            return clearingBrokerRows;

        }
        private Row BuildClearingBrokerGroupRow(FundAccountClearingBrokerData data)
        {
            return new Row
            {
                ["Clearing Broker Name"] = $"{data.AccountName} - {data.FundName} - {data.CounterpartyName} - {data.Currency}",
                ["Counterparty"] = string.Empty,
                ["Agreement Name"] = string.Empty,
                ["Account Type"] = string.Empty,
                ["Account Number"] = string.Empty,
                ["FFC Name"] = string.Empty,
                ["FFC Number"] = string.Empty,
                ["CreatedBy"] = string.Empty,
                ["CreatedAt"] = string.Empty,
            };
        }
        private ExportContent CreateClearingBrokersExportContent(List<FundAccountClearingBrokerData> clearingBrokers)
        {       
            var groupRows = new List<GroupRow>();
            var dataRows=new List<Row>();
            var groupedData = clearingBrokers.GroupBy(s => s.AccountName).ToDictionary(s => s.Key, v => v.ToList());
            var startIndex =  1;
            var endIndex = 0;
            foreach (var fund in groupedData.Keys)
            {
                startIndex = dataRows.Count+2;
                endIndex = startIndex+ groupedData[fund].Count-1;
                var row = BuildClearingBrokerGroupRow(groupedData[fund][0]);
                row.RowHighlight = Row.Highlight.SubHeader;
                dataRows.Add(row);
                dataRows.AddRange(BuildClearingBrokersExportRows(groupedData[fund]));
                groupRows.Add(new GroupRow() { StartRow = startIndex, EndRow = endIndex, IsHidden = false });
            }
            return new ExportContent()
            {
                Rows = dataRows,
                TabName = "Clearing Brokers",
                GroupRows = groupRows,
            };
        }

        //Build Account Rows
        private List<Row> BuildAccountRows(List<onBoardingAccount> onBoardingAccounts, List<FundAccountData> fundAccounts)
        {
            var fundAccountMap = fundAccounts.ToDictionary(s => s.onBoardingAccountId, v => v);
            var accountListRows = new List<Row>();

            foreach (var account in onBoardingAccounts)
            {
                var row = new Row
                {
                    ["Account Id"] = account.onBoardingAccountId.ToString(),
                    ["Entity Type"] = account.AccountType,
                    ["Client Name"] = fundAccountMap.ContainsKey(account.onBoardingAccountId) ? fundAccountMap[account.onBoardingAccountId].ClientName : string.Empty,
                    ["Fund Name"] = fundAccountMap.ContainsKey(account.onBoardingAccountId) ? fundAccountMap[account.onBoardingAccountId].LegalFundName : string.Empty,
                    ["Fund Status"] = fundAccountMap.ContainsKey(account.onBoardingAccountId) ? fundAccountMap[account.onBoardingAccountId].LaunchStatus : string.Empty,
                    ["Agreement Name"] = fundAccountMap.ContainsKey(account.onBoardingAccountId) ? fundAccountMap[account.onBoardingAccountId].AgreementShortName : string.Empty,
                    ["Counterparty"] = fundAccountMap.ContainsKey(account.onBoardingAccountId) ? fundAccountMap[account.onBoardingAccountId].CounterpartyName : string.Empty,
                    ["Counterparty Family"] = fundAccountMap.ContainsKey(account.onBoardingAccountId) ? fundAccountMap[account.onBoardingAccountId].CounterpartyFamily : string.Empty,
                    ["Account Name"] = account.AccountName,
                    ["Account Number"] = fundAccountMap.ContainsKey(account.onBoardingAccountId) ? fundAccountMap[account.onBoardingAccountId].AccountNumber : string.Empty,
                    ["Account Type"] = account.AccountPurpose,
                    ["Account Status"] = account.AccountStatus,
                    ["Currency"] = account.Currency,
                    ["Description"] = account.Description,
                    ["Notes"] = account.Notes,
                    ["Authorized Party"] = account.AuthorizedParty,
                    ["Cash Instruction Mechanism"] = account.CashInstruction,
                    ["Swift Group"] = account.SwiftGroup != null ? account.SwiftGroup.SwiftGroup : string.Empty,
                    ["Senders BIC"] = account.SwiftGroup != null ? account.SwiftGroup.SendersBIC : string.Empty,
                    ["Cash Sweep"] = account.CashSweep
                };

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
                row["Top Level Manager Account"] = account.TopLevelManagerAccountNumber;
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

                var fileInfo = new FileInfo(
                    $"{FileSystemManager.OpsSecureBulkFileUploads}\\FundAccount\\{DateTime.Now:yyyy-MM-dd}\\{file.FileName}");

                if (fileInfo.Directory != null && !Directory.Exists(fileInfo.Directory.FullName))
                    Directory.CreateDirectory(fileInfo.Directory.FullName);

                var newFileName = file.FileName;
                var splitFileNames = file.FileName.Split('.');
                var ind = 1;
                while (System.IO.File.Exists(fileInfo.FullName))
                {
                    newFileName = $"{splitFileNames[0]}_{ind++}.{splitFileNames[1]}";
                    fileInfo = new FileInfo(
                        $"{FileSystemManager.OpsSecureBulkFileUploads}\\FundAccount\\{DateTime.Now:yyyy-MM-dd}\\{newFileName}");
                }

                file.SaveAs(fileInfo.FullName);

                var accountRows = ReportDeliveryManager.ParseAsRows(fileInfo, "List of Accounts", string.Empty, true);

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

                        var fund = hFunds.FirstOrDefault(x => x.LegalFundName == account["Fund Name"] || x.PreferredFundName == account["Fund Name"]);

                        if (fund == null)
                            continue;


                        if (account["Entity Type"] == "Agreement")
                        {
                            accountDetail.dmaAgreementOnBoardingId = agreements.FirstOrDefault(x => x.Value == account["Agreement Name"]).Key;

                            //No agreement available for given name
                            if (accountDetail.dmaAgreementOnBoardingId == 0)
                                continue;

                            accountDetail.hmFundId = fund.HmFundId;
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
                            accountDetail.hmFundId = fund.HmFundId;
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
                            accountDetail.SwiftGroupId = swiftGroup?.hmsSwiftGroupId;
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
                        accountDetail.TopLevelManagerAccountNumber = account["Top Level Manager Account"];
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

            var row = new Row
            {
                ["Entity Type"] = string.Empty,
                ["Fund Name"] = string.Empty,
                ["Agreement Name"] = string.Empty,
                ["Broker"] = string.Empty,
                ["Account Name"] = string.Empty,
                ["Account Number"] = string.Empty,
                ["Account Type"] = string.Empty,
                ["Account Status"] = string.Empty,
                ["Currency"] = string.Empty,
                ["Description"] = string.Empty,
                ["Notes"] = string.Empty,
                ["Authorized Party"] = string.Empty,
                ["Cash Instruction Mechanism"] = string.Empty,
                ["Swift Group"] = string.Empty,
                ["Senders BIC"] = string.Empty,
                ["Cash Sweep"] = string.Empty,
                ["Cash Sweep Time"] = string.Empty,
                ["Cash Sweep Time Zone"] = string.Empty,
                ["Cutoff Time Zone"] = string.Empty,
                ["Cutoff Time"] = string.Empty,
                ["Days to wire per V.D"] = string.Empty,
                ["Holdback Amount"] = string.Empty,
                ["Sweep Comments"] = string.Empty,
                ["Associated Custody Acct"] = string.Empty,
                ["Associated Custody Acct Number"] = string.Empty,
                ["Portfolio Selection"] = string.Empty,
                ["Ticker/ISIN"] = string.Empty,
                ["Sweep Currency"] = string.Empty,
                ["Contact Type"] = string.Empty,
                ["Beneficiary Type"] = "ABA/BIC",
                ["Beneficiary BIC or ABA"] = string.Empty,
                ["Beneficiary Account Number"] = string.Empty,
                ["Intermediary Beneficiary Type"] = "ABA/BIC",
                ["Intermediary BIC or ABA"] = string.Empty,
                ["Intermediary Account Number"] = string.Empty,
                ["Ultimate Beneficiary Account Name"] = string.Empty,
                ["Ultimate Beneficiary Type"] = "ABA/BIC",
                ["Ultimate Beneficiary BIC or ABA"] = string.Empty,
                ["FFC Name"] = string.Empty,
                ["FFC Number"] = string.Empty,
                ["Margin Account Number"] = string.Empty,
                ["Top Level Manager Account"] = string.Empty,
                ["Reference"] = string.Empty,
                ["Status"] = string.Empty,
                ["Comments"] = string.Empty,
                ["CreatedBy"] = string.Empty,
                ["CreatedDate"] = string.Empty,
                ["UpdatedBy"] = string.Empty,
                ["ModifiedDate"] = string.Empty,
                ["ApprovedBy"] = string.Empty
            };

            var accountListRows = new List<Row> { row };

            //File name and path
            var fileName = $"AccountList_{DateTime.Now:yyyyMMdd}";
            var exportFileInfo = new FileInfo($"{FileSystemManager.UploadTemporaryFilesPath}{fileName}{DefaultExportFileFormat}");
            contentToExport.Add("List of Accounts", accountListRows);

            //Export the account file
            ReportDeliveryManager.CreateExportFile(contentToExport, exportFileInfo, true);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        public FileResult DownloadAccountFile(string fileName, long accountId)
        {
            var file = new FileInfo($"{FileSystemManager.OpsSecureAccountsFileUploads}{accountId}\\{fileName}");
            return DownloadFile(file, file.Name);
        }

        public JsonResult UploadAccountFiles(long accountId)
        {
            var aDocuments = new List<onBoardingAccountDocument>();
            if (aDocuments == null) throw new ArgumentNullException(nameof(aDocuments));
            if (accountId <= 0)
            {
                return Json(new
                {
                    Documents = aDocuments.Select(document => new
                    {
                        document.onBoardingAccountDocumentId,
                        document.onBoardingAccountId,
                        document.FileName,
                        document.RecCreatedAt,
                        document.RecCreatedBy
                    }).ToList()
                }, JsonContentType, JsonContentEncoding);
            }

            for (var i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];

                if (file == null)
                    throw new Exception("unable to retrieve file information");
                var fileName = $"{FileSystemManager.OpsSecureAccountsFileUploads}{accountId}\\{file.FileName}";
                var fileInfo = new FileInfo(fileName);

                if (fileInfo.Directory != null && !Directory.Exists(fileInfo.Directory.FullName))
                    Directory.CreateDirectory(fileInfo.Directory.FullName);

                if (System.IO.File.Exists(fileInfo.FullName))
                {
                    FundAccountManager.RemoveAccountDocument(accountId, file.FileName);
                    System.IO.File.Delete(fileInfo.FullName);
                }

                file.SaveAs(fileInfo.FullName);

                //Build account document
                var document = new onBoardingAccountDocument
                {
                    FileName = file.FileName,
                    RecCreatedAt = DateTime.Now,
                    RecCreatedBy = UserName,
                    onBoardingAccountId = accountId
                };

                FundAccountManager.AddAccountDocument(document);

                aDocuments.Add(document);
            }

            return Json(new
            {
                Documents = aDocuments.Select(document => new
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