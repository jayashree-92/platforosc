using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.Operations.FileParseEngine.Models;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware;
using HM.Operations.Secure.Middleware.Util;
using HM.Operations.Secure.Web.Utility;

namespace HM.Operations.Secure.Web.Controllers
{
    public class SSITemplateController : WireUserBaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SSITemplate(long ssiTemplateId = 0)
        {
            ViewBag.ssiTemplateId = ssiTemplateId;
            ViewBag.userName = UserName;
            return View();
        }

        public JsonResult GetAllSsiTemplates()
        {
            var ssiTemplates = SSITemplateManager.GetAllSsiTemplates();
            return Json(new
            {
                SSITemplates = ssiTemplates.OrderBy(x => x.Value).Select(ac => new
                {
                    id = ac.Key,
                    text = ac.Value
                }).ToList()
            });
        }

        public JsonResult GetAllSsiTemplates(int templateTypeId, long templateEntityId)
        {
            var onBoardingSsiTemplates = SSITemplateManager.GetAllSsiTemplates(templateTypeId, templateEntityId);
            return Json(new
            {
                OnBoardingSSITemplates = onBoardingSsiTemplates
            });
        }

        public JsonResult GetAllBrokerSsiTemplates()
        {
            var ssiTemplates = SSITemplateManager.GetAllBrokerSsiTemplates();
            var counterParties = OnBoardingDataManager.GetAllOnBoardedCounterparties();
            var agreementTypes = OnBoardingDataManager.GetAllAgreementTypes();
            var serviceProviders = SSITemplateManager.GetAllServiceProviderList();

            ssiTemplates.ForEach(ssi =>
            {
                ssi.CreatedBy = ssi.CreatedBy.HumanizeEmail();
                ssi.UpdatedBy = ssi.UpdatedBy.HumanizeEmail();
            });

            var brokerSsiTemplates = ssiTemplates.AsParallel().Select(template => new
            {
                SSITemplate = SSITemplateManager.SetSSITemplateDefaults(template),
                AgreementType = agreementTypes.ContainsKey(template.dmaAgreementTypeId) && string.IsNullOrEmpty(template.ServiceProvider) ? agreementTypes[template.dmaAgreementTypeId] : string.Empty,
                Broker = counterParties.TryGetValue(template.TemplateEntityId, out var counterParty) ? counterParty : string.Empty
            }).ToList();

            return Json(new
            {
                BrokerSsiTemplates = brokerSsiTemplates,
                counterParties = counterParties.Select(x => new { id = x.Key, text = x.Value }).OrderBy(x => x.text).ToList(),
                serviceProviders = serviceProviders.Select(servProv => new { id = servProv, text = servProv }).ToList()
            });
        }

        public JsonResult GetSsiTemplate(long templateId)
        {
            var onBoardingSsiTemplate = SSITemplateManager.GetSsiTemplate(templateId);
            var document = onBoardingSsiTemplate.onBoardingSSITemplateDocuments.ToList();
            onBoardingSsiTemplate.onBoardingSSITemplateDocuments = null;

            return Json(new
            {
                OnBoardingSsiTemplate = onBoardingSsiTemplate,
                isAuthorizedUserToApprove = (User.IsWireApprover() && onBoardingSsiTemplate.SSITemplateStatus == "Pending Approval" && onBoardingSsiTemplate.CreatedBy != UserName && onBoardingSsiTemplate.UpdatedBy != UserName),
                document
            });
        }

        public JsonResult GetAllServiceProviderList()
        {
            var serviceProviders = SSITemplateManager.GetAllServiceProviderList().Select(servProv => new { id = servProv, text = servProv }).ToList();
            return Json(serviceProviders, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllDescriptions()
        {
            var ssiDescriptions = SSITemplateManager.GetAllSSITemplateDescriptions().Select(s => new { id = s.hmsSSIDescriptionId, text = s.Description }).OrderBy(s => s.text).ToList();
            return Json(ssiDescriptions, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PaymentOrReceiptReasonDetails(string templateType, int? agreementTypeId, string serviceProviderName, int descriptionId = 0)
        {
            var reasonDetail = templateType == "Broker"
                ? SSITemplateManager.GetAllSsiTemplateAccountTypes(agreementTypeId).Select(x => new { id = x.Reason, text = x.Reason }).ToList()
                : templateType == "Fee/Expense Payment"
                    ? SSITemplateManager.GetAllSsiTemplateServiceProviders(serviceProviderName).Select(x => new { id = x.FeeType, text = x.FeeType }).ToList()
                    : templateType == "Bank Loan/Private/IPO"
                        ? SSITemplateManager.GetAllSsiPaymentReasonForDescription(descriptionId).Select(x => new { id = x.PaymentReason, text = x.PaymentReason }).ToList()
                        : null;

            return Json(reasonDetail, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSsiTemplatePreloadData()
        {
            var counterParties = OnBoardingDataManager.GetAllOnBoardedCounterparties().Select(x => new { id = x.Key, text = x.Value }).OrderBy(x => x.text).ToList();
            var templates = SSITemplateManager.GetAllSsiTemplates().Select(x => new { id = x.Key, text = x.Value }).ToList();
            var permittedAgreementTypes = OpsSecureSwitches.AllowedAgreementTypesForSSITemplateCreation;
            var accountTypes = OnBoardingDataManager.GetAllAgreementTypes().Where(x => permittedAgreementTypes.Contains(x.Value)).Select(x => new { id = x.Key, text = x.Value }).OrderBy(x => x.text).ToList();
            var currencies = FundAccountManager.GetAllCurrencies().Select(currency => new { id = currency, text = currency }).ToList();
            var addressList = FundAccountManager.GetAllBankAccountAddress().Select(s => new { id = s.AccountName, text = s.AccountName }).OrderBy(s => s.text).ToList();

            return Json(new
            {
                counterParties,
                templates,
                accountTypes,
                currencies,
                addressList,
            });
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

            var ssiTemplateId = SSITemplateManager.AddSsiTemplate(ssiTemplate, UserName);
            if (ssiTemplateId <= 0)
                return ssiTemplateId;

            var auditLogList = AuditManager.GetAuditLogs(ssiTemplate, accountType, broker, UserName);
            AuditManager.Log(auditLogList);
            return ssiTemplateId;
        }

        public void UpdateSsiTemplateStatus(string ssiTemplateStatus, long ssiTemplateId, string comments)
        {
            using (var context = new OperationsSecureContext())
            {
                var ssiTemplate = context.onBoardingSSITemplates.AsNoTracking().FirstOrDefault(template => template.onBoardingSSITemplateId == ssiTemplateId);
                if (ssiTemplate == null) return;
                var existingStatus = ssiTemplate.SSITemplateStatus;
                ssiTemplate.SSITemplateStatus = ssiTemplateStatus;
                ssiTemplate.StatusComments = comments;
                ssiTemplate.UpdatedAt = DateTime.Now;
                ssiTemplate.UpdatedBy = ssiTemplateStatus == "Approved" ? ssiTemplate.UpdatedBy : UserName;
                ssiTemplate.ApprovedBy = ssiTemplateStatus == "Approved" ? UserName : null;
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

        public void RemoveSsiTemplateMap(long ssiTemplateMapId)
        {
            SSITemplateManager.RemoveSsiTemplateMap(ssiTemplateMapId);
        }

        public void DeleteSsiTemplate(long ssiTemplateId)
        {
            using (var context = new OperationsSecureContext())
            {
                var ssiTemplateToBeDeleted = context.onBoardingSSITemplates.FirstOrDefault(template => template.onBoardingSSITemplateId == ssiTemplateId);
                if (ssiTemplateToBeDeleted == null) return;
                ssiTemplateToBeDeleted.IsDeleted = true;
                context.SaveChanges();

                var allAssociationsTothisSSIId = context.onBoardingAccountSSITemplateMaps.Where(s => s.onBoardingSSITemplateId == ssiTemplateId).ToList();
                allAssociationsTothisSSIId.ForEach(s => s.Status = "Deleted");
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

        public string UploadSsiTemplate()
        {
            var onboardingSsiTemplate = SSITemplateManager.GetAllBrokerSsiTemplates();
            var counterParties = OnBoardingDataManager.GetAllOnBoardedCounterparties();
            var agreementTypes = OnBoardingDataManager.GetAllAgreementTypes();
            var accountBicorAba = FundAccountManager.GetAllAccountBicorAba();
            var messageTypes = new List<string> { "MT103", "MT202", "MT202 COV" };
            var bulkUploadLogs = new List<hmsBulkUploadLog>();
            for (var i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];

                if (file == null)
                    throw new Exception("unable to retrive file information");

                var fileInfo = new FileInfo(
                    $"{FileSystemManager.OpsSecureBulkFileUploads}\\SSITemplate\\{DateTime.Now:yyyy-MM-dd}\\{file.FileName}");

                var newFileName = file.FileName;
                var splitFileNames = file.FileName.Split('.');
                var ind = 1;
                while (System.IO.File.Exists(fileInfo.FullName))
                {
                    newFileName = $"{splitFileNames[0]}_{ind++}.{splitFileNames[1]}";
                    fileInfo = new FileInfo(
                        $"{FileSystemManager.OpsSecureBulkFileUploads}\\FundAccount\\{DateTime.Now}:yyyy-MM-dd\\{newFileName}");
                }


                if (fileInfo.Directory != null && !Directory.Exists(fileInfo.Directory.FullName))
                    Directory.CreateDirectory(fileInfo.Directory.FullName);

                file.SaveAs(fileInfo.FullName);
                var templateListRows = ReportDeliveryManager.ParseAsRows(fileInfo, "List of SSI Template", string.Empty, true);

                if (templateListRows.Count > 0)
                {
                    foreach (var template in templateListRows)
                    {
                        var templateDetail = new onBoardingSSITemplate
                        {
                            Beneficiary = new onBoardingAccountBICorABA(),
                            Intermediary = new onBoardingAccountBICorABA(),
                            UltimateBeneficiary = new onBoardingAccountBICorABA(),
                            onBoardingSSITemplateId = string.IsNullOrWhiteSpace(template["SSI Template Id"])
                                ? 0
                                : long.Parse(template["SSI Template Id"]),
                            TemplateTypeId = SSITemplateManager.BrokerTemplateTypeId,
                            TemplateEntityId = counterParties.FirstOrDefault(x => x.Value == template["Legal Entity"])
                                .Key,
                            dmaAgreementTypeId = agreementTypes.FirstOrDefault(x => x.Value == template["Account Type"])
                                .Key,
                            ServiceProvider = template["Service Provider"],
                            Currency = template["Currency"],
                            ReasonDetail = template["Payment/Receipt Reason Detail"],
                            OtherReason = template["Other Reason"],
                            MessageType = messageTypes.Contains(template["Message Type"].Trim())
                                ? template["Message Type"].Trim()
                                : string.Empty
                        };


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
                            templateDetail.BeneficiaryBICorABAId = beneficiaryBiCorAba.onBoardingAccountBICorABAId;
                        }
                        else
                        {
                            templateDetail.Beneficiary.BankName = string.Empty;
                            templateDetail.Beneficiary.BankAddress = string.Empty;
                            templateDetail.Beneficiary.BICorABA = string.Empty;
                            templateDetail.BeneficiaryType = string.Empty;
                            templateDetail.BeneficiaryBICorABAId = null;
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
                            templateDetail.IntermediaryBICorABAId = intermediaryBiCorAba.onBoardingAccountBICorABAId;
                        }
                        else
                        {
                            templateDetail.Intermediary.BankName = string.Empty;
                            templateDetail.Intermediary.BankAddress = string.Empty;
                            templateDetail.Intermediary.BICorABA = string.Empty;
                            templateDetail.IntermediaryType = string.Empty;
                            templateDetail.IntermediaryBICorABAId = null;
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
                            templateDetail.UltimateBeneficiaryBICorABAId = ultimateBeneficiaryBiCorAba.onBoardingAccountBICorABAId;
                        }
                        else
                        {
                            templateDetail.UltimateBeneficiary.BankName = string.Empty;
                            templateDetail.UltimateBeneficiary.BankAddress = string.Empty;
                            templateDetail.UltimateBeneficiary.BICorABA = string.Empty;
                            templateDetail.UltimateBeneficiaryBICorABAId = null;
                        }
                        templateDetail.UltimateBeneficiaryAccountNumber = template["Ultimate Beneficiary Account Number"];
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
                        templateDetail.LastUsedAt = DateTime.Now;
                        //templateDetail.TemplateName = template["SSI Template Type"] == "Broker" ? template["Broker"] + " - " + template["Account Type"] + " - " + templateDetail.Currency + " - " + template["Payment/Receipt Reason Detail"] : (!string.IsNullOrWhiteSpace(template["SSI Template Type"]) ? template["Service Provider"] + " - " + templateDetail.Currency + " - " + template["Payment/Receipt Reason Detail"] : template["Template Name"]);
                        SSITemplateManager.AddSsiTemplate(templateDetail, UserName);
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

            var row = new Row
            {
                ["Template Name"] = string.Empty,
                ["SSI Template Type"] = string.Empty,
                ["Legal Entity"] = string.Empty,
                ["Account Type"] = string.Empty,
                ["Service Provider"] = string.Empty,
                ["Currency"] = string.Empty,
                ["Payment/Receipt Reason Detail"] = string.Empty,
                ["Other Reason"] = string.Empty,
                ["Message Type"] = string.Empty,
                ["Beneficiary Type"] = "ABA/BIC",
                ["Beneficiary BIC or ABA"] = string.Empty,
                ["Beneficiary Account Number"] = string.Empty,
                ["Intermediary Beneficiary Type"] = "ABA/BIC",
                ["Intermediary BIC or ABA"] = string.Empty,
                ["Intermediary Account Number"] = string.Empty,
                ["Ultimate Beneficiary Type"] = "ABA/BIC",
                ["Ultimate Beneficiary BIC or ABA"] = String.Empty,
                ["Ultimate Beneficiary Account Name"] = string.Empty,
                ["Ultimate Beneficiary Account Number"] = string.Empty,
                ["FFC Name"] = string.Empty,
                ["FFC Number"] = string.Empty,
                ["Reference"] = string.Empty,
                ["CreatedBy"] = string.Empty,
                ["CreatedDate"] = string.Empty,
                ["UpdatedBy"] = string.Empty,
                ["ModifiedDate"] = string.Empty,
                ["ApprovedBy"] = string.Empty
            };
            //row["SSI Template Id"] = string.Empty;
            //row["Beneficiary Bank Name"] = String.Empty;
            //row["Beneficiary Bank Address"] = String.Empty;
            //row["Intermediary Bank Name"] = String.Empty;
            //row["Intermediary Bank Address"] = String.Empty;
            // row["Ultimate Beneficiary Bank Name"] = String.Empty;
            //row["Ultimate Beneficiary Bank Address"] = String.Empty;
            var templateListRows = new List<Row> { row };

            //File name and path
            var fileName = $"SSITemplateList_{DateTime.Now:yyyyMMdd}";
            var exportFileInfo = new FileInfo($"{FileSystemManager.UploadTemporaryFilesPath}{fileName}{DefaultExportFileFormat}");
            contentToExport.Add("List of SSI Template", templateListRows);

            //Export the account file
            ReportDeliveryManager.CreateExportFile(contentToExport, exportFileInfo, true);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        public void AddPaymentOrReceiptReasonDetails(string reason, string templateType, int? agreementTypeId, string serviceProviderName, int descriptionId = 0)
        {
            switch (templateType)
            {
                case "Broker":
                    {
                        using (var context = new OperationsSecureContext())
                        {
                            var onBoardingSsiTemplateAccountType = new OnBoardingSSITemplateAccountType { Reason = reason, dmaAgreementTypeId = agreementTypeId ?? 0 };
                            context.OnBoardingSSITemplateAccountTypes.Add(onBoardingSsiTemplateAccountType);
                            context.SaveChanges();
                        }

                        break;
                    }
                case "Fee/Expense Payment":
                    {
                        using (var context = new AdminContext())
                        {
                            var onBoardingSsiTemplateServiceProvider = new OnBoardingServiceProvider { FeeType = reason, ServiceProvider = serviceProviderName };
                            context.OnBoardingServiceProviders.Add(onBoardingSsiTemplateServiceProvider);
                            context.SaveChanges();
                        }
                        break;
                    }
                case "Bank Loan/Private/IPO":
                    {
                        using (var context = new OperationsSecureContext())
                        {
                            var hmsSSIReasonForDesc = new hmsSSIPaymentReasonForDescription { PaymentReason = reason, hmsSSIDescriptionId = descriptionId, RecCreatedBy = UserName, RecCreatedDt = DateTime.Now };
                            context.hmsSSIPaymentReasonForDescriptions.Add(hmsSSIReasonForDesc);
                            context.SaveChanges();
                        }
                        break;
                    }
            }
        }

        public void AddServiceProvider(string serviceProviderName)
        {
            using (var context = new AdminContext())
            {
                var onboardingServiceProvider = new OnBoardingServiceProvider() { FeeType = "Vendor Expenses", ServiceProvider = serviceProviderName };
                context.OnBoardingServiceProviders.Add(onboardingServiceProvider);
                context.SaveChanges();
            }
        }
        public void AddDescription(string description)
        {
            using (var context = new OperationsSecureContext())
            {
                var ssiDescription = new hmsSSIDescription() { Description = description, RecCreatedBy = UserName, RecCreatedDt = DateTime.Now };
                context.hmsSSIDescriptions.Add(ssiDescription);
                context.SaveChanges();
            }
        }

        public void RemoveSsiTemplateDocument(long documentId)
        {
            //var fileinfo = new FileInfo(FileSystemManager.OnboardingSsiTemplateFilesPath + fileName);

            //if (System.IO.File.Exists(fileinfo.FullName))
            //    System.IO.File.Delete(fileinfo.FullName);
            if (documentId <= 0)
                return;

            using (var context = new OperationsSecureContext())
            {
                var document = context.onBoardingSSITemplateDocuments.FirstOrDefault(x => x.onBoardingSSITemplateDocumentId == documentId);
                if (document == null) return;
                var fileName =
                    $"{FileSystemManager.OpsSecureSSITemplateFileUploads}{document.onBoardingSSITemplateId}\\{document.FileName}";
                var fileinfo = new FileInfo(fileName);

                if (System.IO.File.Exists(fileinfo.FullName))
                    System.IO.File.Delete(fileinfo.FullName);

                context.onBoardingSSITemplateDocuments.Remove(document);
                var ssiTemplate = context.onBoardingSSITemplates.FirstOrDefault(s => s.onBoardingSSITemplateId == document.onBoardingSSITemplateId);
                ssiTemplate.SSITemplateStatus = "Created";
                ssiTemplate.UpdatedAt = DateTime.Now;
                ssiTemplate.UpdatedBy = UserName;
                context.SaveChanges();
            }

        }

        public bool IsSsiTemplateDocumentExists(long ssiTemplateId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                var document = context.onBoardingSSITemplateDocuments.FirstOrDefault(x => x.onBoardingSSITemplateId == ssiTemplateId);
                return (document != null);
            }
        }

        public JsonResult GetSSICallbackData(long ssiTemplateId)
        {
            var callbacks = SSITemplateManager.GetSSICallbacks(ssiTemplateId);
            return Json(callbacks, JsonContentType, JsonContentEncoding);
        }

        public void AddOrUpdateCallback(hmsSSICallback callback)
        {
            if (callback.hmsSSICallbackId > 0)
            {
                var existingCallback = SSITemplateManager.GetCallbackData(callback.hmsSSICallbackId);
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
                SSITemplateManager.UpdateIsKeyFieldsChanged(callback.onBoardingSSITemplateId);
            }

            SSITemplateManager.AddOrUpdateCallback(callback);
        }

        public FileResult ExportAllSsiTemplatelist()
        {
            var ssiTemplates = SSITemplateManager.GetAllBrokerSsiTemplates().OrderByDescending(x => x.UpdatedAt).ToList();
            var contentToExport = new Dictionary<string, List<Row>>();
            var accountListRows = BuildSsiTemplateRows(ssiTemplates);
            //File name and path
            var fileName = $"{"SSITemplateList"}_{DateTime.Now:yyyyMMdd}";
            var exportFileInfo = new FileInfo(
                $"{FileSystemManager.UploadTemporaryFilesPath}{fileName}{DefaultExportFileFormat}");
            contentToExport.Add("List of SSI Template", accountListRows);
            //Export the checklist file
            ReportDeliveryManager.CreateExportFile(contentToExport, exportFileInfo);
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
                row["Beneficiary BIC or ABA"] = template.Beneficiary != null ? template.Beneficiary.BICorABA : string.Empty;
                row["Beneficiary Bank Name"] = template.Beneficiary != null ? template.Beneficiary.BankName : string.Empty;
                row["Beneficiary Bank Address"] = template.Beneficiary != null ? template.Beneficiary.BankAddress : string.Empty;
                row["Beneficiary Account Number"] = template.BeneficiaryAccountNumber;
                row["Intermediary Beneficiary Type"] = template.IntermediaryType;
                row["Intermediary BIC or ABA"] = template.Intermediary != null ? template.Intermediary.BICorABA : string.Empty;
                row["Intermediary Bank Name"] = template.Intermediary != null ? template.Intermediary.BankName : string.Empty;
                row["Intermediary Bank Address"] = template.Intermediary != null ? template.Intermediary.BankAddress : string.Empty;
                row["Intermediary Account Number"] = template.IntermediaryAccountNumber;
                row["Ultimate Beneficiary Type"] = template.UltimateBeneficiaryType;
                row["Ultimate Beneficiary BIC or ABA"] = template.UltimateBeneficiary != null ? template.UltimateBeneficiary.BICorABA : string.Empty;
                row["Ultimate Beneficiary Bank Name"] = template.UltimateBeneficiary != null ? template.UltimateBeneficiary.BankName : string.Empty;
                row["Ultimate Beneficiary Bank Address"] = template.UltimateBeneficiary != null ? template.UltimateBeneficiary.BankAddress : string.Empty;
                row["Ultimate Beneficiary Account Name"] = template.UltimateBeneficiaryAccountName;
                row["Ultimate Beneficiary Account Number"] = template.UltimateBeneficiaryAccountNumber;
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
                row["LastUsedAt"] = template.LastUsedAt + "";
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


        public FileResult DownloadSsiTemplateFile(string fileName, long ssiTemplateId)
        {
            var file = new FileInfo($"{FileSystemManager.OpsSecureSSITemplateFileUploads}{ssiTemplateId}\\{fileName}");
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

                    var fileName =
                        $"{FileSystemManager.OpsSecureSSITemplateFileUploads}{ssiTemplateId}\\{file.FileName}";
                    var fileinfo = new FileInfo(fileName);

                    if (fileinfo.Directory != null && !Directory.Exists(fileinfo.Directory.FullName))
                        Directory.CreateDirectory(fileinfo.Directory.FullName);

                    if (System.IO.File.Exists(fileinfo.FullName))
                    {
                        SSITemplateManager.RemoveSsiTemplateDocument(ssiTemplateId, file.FileName);
                        System.IO.File.Delete(fileinfo.FullName);
                    }

                    file.SaveAs(fileinfo.FullName);

                    //Build ssi template document
                    var document = new onBoardingSSITemplateDocument
                    {
                        FileName = file.FileName,
                        RecCreatedAt = DateTime.Now,
                        RecCreatedBy = User.Identity.Name,
                        onBoardingSSITemplateId = ssiTemplateId
                    };

                    using (var context = new OperationsSecureContext())
                    {
                        context.onBoardingSSITemplateDocuments.Add(document);
                        var ssiTemplate = context.onBoardingSSITemplates.FirstOrDefault(s => s.onBoardingSSITemplateId == document.onBoardingSSITemplateId);
                        ssiTemplate.SSITemplateStatus = "Created";
                        ssiTemplate.UpdatedAt = DateTime.Now;
                        ssiTemplate.UpdatedBy = UserName;
                        context.SaveChanges();
                    }

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
            });
        }
    }
}