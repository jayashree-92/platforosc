using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Com.HedgeMark.Commons.Extensions;
using ExcelUtility.Operations.ManagedAccounts;
using HedgeMark.Operations.FileParseEngine.Models;
using HedgeMark.Operations.Secure.DataModel;
using HMOSecureMiddleware;
using HMOSecureWeb.Utility;

namespace HMOSecureWeb.Controllers
{
    public class SSITemplateController : BaseController
    {

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
            });
        }

        public JsonResult GetAllSsiTemplates(int templateTypeId, long templateEntityId)
        {
            var onBoardingSsiTemplates = AccountManager.GetAllSsiTemplates(templateTypeId, templateEntityId);
            return Json(new
            {
                OnBoardingSSITemplates = onBoardingSsiTemplates
            });
        }

        public JsonResult GetAllBrokerSsiTemplates()
        {
            var brokerSsiTemplates = AccountManager.GetAllBrokerSsiTemplates();
            var counterParties = OnBoardingDataManager.GetAllOnBoardedCounterparties();
            var agreementTypes = OnBoardingDataManager.GetAllAgreementTypes();
            var serviceProviders = AccountManager.GetAllServiceProviderList().Select(y => new { id = y.ServiceProvider, text = y.ServiceProvider }).DistinctBy(x => x.id).OrderBy(x => x.id).ToList();
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
                    BeneficiaryBICorABA = template.Beneficiary != null ? template.Beneficiary.BICorABA : string.Empty,
                    BeneficiaryBankName = template.Beneficiary != null ? template.Beneficiary.BankName : string.Empty,
                    BeneficiaryBankAddress = template.Beneficiary != null ? template.Beneficiary.BankAddress : string.Empty,
                    template.BeneficiaryAccountNumber,
                    template.IntermediaryType,
                    IntermediaryBICorABA = template.Intermediary != null ? template.Intermediary.BICorABA : string.Empty,
                    IntermediaryBankName = template.Intermediary != null ? template.Intermediary.BankName : string.Empty,
                    IntermediaryBankAddress = template.Intermediary != null ? template.Intermediary.BankAddress : string.Empty,
                    template.IntermediaryAccountNumber,
                    template.UltimateBeneficiaryType,
                    template.UltimateBeneficiaryAccountName,
                    UltimateBeneficiaryBICorABA = template.UltimateBeneficiary != null ? template.UltimateBeneficiary.BICorABA : string.Empty,
                    UltimateBeneficiaryBankName = template.UltimateBeneficiary != null ? template.UltimateBeneficiary.BankName : string.Empty,
                    UltimateBeneficiaryBankAddress = template.UltimateBeneficiary != null ? template.UltimateBeneficiary.BankAddress : string.Empty,
                    template.FFCName,
                    template.FFCNumber,
                    template.Reference,
                    AgreementType = (agreementTypes.ContainsKey(template.dmaAgreementTypeId) && string.IsNullOrEmpty(template.ServiceProvider)) ? agreementTypes[template.dmaAgreementTypeId] : string.Empty,
                    Broker = (counterParties.ContainsKey(template.TemplateEntityId) ? counterParties[template.TemplateEntityId] : string.Empty)
                }).ToList(),
                counterParties = counterParties.Select(x => new { id = x.Key, text = x.Value }).OrderBy(x => x.text).ToList(),
                serviceProviders
            });
        }

        public JsonResult GetSsiTemplate(long templateId)
        {
            var onBoardingSsiTemplate = AccountManager.GetSsiTemplate(templateId);
            var document = onBoardingSsiTemplate.onBoardingSSITemplateDocuments.ToList();
            onBoardingSsiTemplate.onBoardingSSITemplateDocuments = null;

            //associatedAccounts

            return Json(new
            {
                OnBoardingSsiTemplate = onBoardingSsiTemplate,
                isAuthorizedUserToApprove = (User.IsWireApprover() && onBoardingSsiTemplate.SSITemplateStatus == "Pending Approval" && onBoardingSsiTemplate.CreatedBy != UserName && onBoardingSsiTemplate.UpdatedBy != UserName),
                document
            });
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
            var auditLogList = AuditManager.GetAuditLogs(ssiTemplate, accountType, broker, UserName);
            var ssiTemplateId = AccountManager.AddSsiTemplate(ssiTemplate, UserName);
            if (ssiTemplateId > 0)
            {
                AuditManager.Log(auditLogList);
            }

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

        public void DeleteSsiTemplate(long ssiTemplateId)
        {
            using (var context = new OperationsSecureContext())
            {
                var ssiTemplateToBeDeleted = context.onBoardingSSITemplates.FirstOrDefault(template => template.onBoardingSSITemplateId == ssiTemplateId);
                if (ssiTemplateToBeDeleted == null) return;
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

        public void AddPaymentOrReceiptReasonDetails(string reason, string templateType, int? agreementTypeId, string serviceProviderName)
        {
            using (var context = new OperationsSecureContext())
            {
                if (templateType == "Broker")
                {
                    var onBoardingSsiTemplateAccountType = new OnBoardingSSITemplateAccountType
                    {
                        Reason = reason,
                        dmaAgreementTypeId = agreementTypeId ?? 0
                    };
                    context.OnBoardingSSITemplateAccountTypes.Add(onBoardingSsiTemplateAccountType);
                }
                else
                {
                    var onBoardingSsiTemplateServiceProvider = new OnBoardingSSITemplateServiceProvider
                    {
                        Reason = reason,
                        ServiceProvider = serviceProviderName
                    };
                    context.OnBoardingSSITemplateServiceProviders.Add(onBoardingSsiTemplateServiceProvider);
                }
                context.SaveChanges();
            }
        }

        public void AddServiceProvider(string serviceProviderName)
        {
            using (var context = new OperationsSecureContext())
            {
                var onboardingServiceProvider = new OnBoardingSSITemplateServiceProvider() { Reason = "Vendor Expenses", ServiceProvider = serviceProviderName };
                context.OnBoardingSSITemplateServiceProviders.Add(onboardingServiceProvider);
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
                var fileName = string.Format("{0}{1}\\{2}", FileSystemManager.OpsSecureSSITemplateFileUploads, document.onBoardingSSITemplateId, document.FileName);
                var fileinfo = new FileInfo(fileName);

                if (System.IO.File.Exists(fileinfo.FullName))
                    System.IO.File.Delete(fileinfo.FullName);

                context.onBoardingSSITemplateDocuments.Remove(document);
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

        public FileResult ExportAllSsiTemplatelist()
        {
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

                    var fileName = string.Format("{0}{1}\\{2}", FileSystemManager.OpsSecureSSITemplateFileUploads,
                        ssiTemplateId, file.FileName);
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