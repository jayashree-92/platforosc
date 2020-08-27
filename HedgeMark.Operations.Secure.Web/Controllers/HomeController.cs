using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HedgeMark.Operations.Secure.DataModel;
using HMOSecureMiddleware;
using HMOSecureMiddleware.Models;
using System.IO;
using Hangfire;
using HMOSecureWeb.Jobs;
using log4net;
using HMOSecureWeb.Utility;
using System.Data.Entity;
using Com.HedgeMark.Commons.Extensions;
using HMOSecureMiddleware.Util;
using log4net.Config;

namespace HMOSecureWeb.Controllers
{
    public class HomeController : BaseController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HomeController));
        public ActionResult Index()
        {
            return View(DateTime.Today);
        }

        public ActionResult WirePurpose()
        {
            return View();
        }

        private class WireStatusCount
        {
            public int Pending { get; set; }
            public int Cancelled { get; set; }
            public int Completed { get; set; }
            public int Failed { get; set; }
            public int Approved { get; set; }
            public int Acknowledged { get; set; }
            public int CancelledAndProcessing { get; set; }
            public int OnHold { get; set; }

            public int Total
            {
                get
                {
                    return Pending + Approved + Cancelled + Completed + Failed + CancelledAndProcessing + OnHold;
                }
            }
        }

        public JsonResult GetWireStatusCount(DateTime valueDate)
        {
            WireStatusCount wireStatusCount;
            using (var context = new OperationsSecureContext())
            {
                var wireStatusCountMap = context.hmsWires.Where(s => s.WireStatusId == 2 || s.ValueDate == valueDate || DbFunctions.TruncateTime(s.CreatedAt) == DbFunctions.TruncateTime(valueDate)).Select(s => new { s.hmFundId, s.WireStatusId, s.SwiftStatusId }).ToList();

                if (!AuthorizedSessionData.IsPrivilegedUser)
                {
                    var authorizedFundsIds = AuthorizedSessionData.HMFundIds.Where(s => s.Level > 1).Select(s => s.Id).ToList();
                    wireStatusCountMap = wireStatusCountMap.Where(s => authorizedFundsIds.Contains(s.hmFundId)).ToList();
                }

                wireStatusCount = new WireStatusCount();

                foreach (var statusCount in wireStatusCountMap)
                {
                    //Initiated and Pending Approval
                    if (statusCount.WireStatusId == 2)
                        wireStatusCount.Pending += 1;

                    //Approved and Processing
                    if (statusCount.WireStatusId == 3 && (statusCount.SwiftStatusId == 2 || statusCount.SwiftStatusId == 3))
                        wireStatusCount.Approved += 1;

                    //Cancelled and Processing
                    if (statusCount.WireStatusId == 4 && (statusCount.SwiftStatusId == 2 || statusCount.SwiftStatusId == 3))
                        wireStatusCount.CancelledAndProcessing += 1;

                    //Approved and Completed
                    if (statusCount.WireStatusId == 3 && statusCount.SwiftStatusId == 5)
                        wireStatusCount.Completed += 1;

                    //Cancelled and Completed
                    if (statusCount.WireStatusId == 4 && (statusCount.SwiftStatusId == 1 || statusCount.SwiftStatusId == 5))
                        wireStatusCount.Cancelled += 1;

                    //Failed
                    if (statusCount.WireStatusId == 5 || statusCount.SwiftStatusId == 4 || statusCount.SwiftStatusId == 6)
                        wireStatusCount.Failed += 1;

                    //Acknowledged
                    if (statusCount.SwiftStatusId == 3)
                        wireStatusCount.Acknowledged += 1;

                    if (statusCount.WireStatusId == 6)
                        wireStatusCount.OnHold += 1;
                }
            }
            return Json(wireStatusCount);
        }

        public JsonResult GetWireStatusDetails(DateTime startContextDate, DateTime endContextDate, string statusIds)
        {
            var wireData = new List<WireTicket>();
            List<hmsWire> wireStatusDetails;

            var allStatusIds = statusIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToInt()).ToList();

            using (var context = new OperationsSecureContext())
            {
                //context.Database.Log = s =>
                //{
                //    Logger.Debug(s);
                //};

                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                wireStatusDetails = context.hmsWires
                    .Include(s => s.hmsWireMessageType)
                    .Include(s => s.hmsWirePurposeLkup)
                    .Include(s => s.hmsWireStatusLkup)
                    .Include(s => s.hmsWireTransferTypeLKup)
                    .Include(s => s.SendingAccount)
                    .Include(s => s.SendingAccount.WirePortalCutoff)
                    .Include(s => s.ReceivingAccount)
                    .Include(s => s.ReceivingSSITemplate)
                    .Where(s => ((allStatusIds.Contains(0) || allStatusIds.Contains(2)) && s.WireStatusId == 2) || s.ValueDate >= startContextDate && s.ValueDate <= endContextDate && (allStatusIds.Contains(0) || allStatusIds.Contains(s.WireStatusId))
                                                    || DbFunctions.TruncateTime(s.CreatedAt) == DbFunctions.TruncateTime(endContextDate) && (allStatusIds.Contains(0) || allStatusIds.Contains(s.WireStatusId))).ToList();
            }

            if (!AuthorizedSessionData.IsPrivilegedUser)
            {
                var authorizedFundsIds = AuthorizedSessionData.HMFundIds.Where(s => s.Level > 1).Select(s => s.Id).ToList();
                wireStatusDetails = wireStatusDetails.Where(s => authorizedFundsIds.Contains(s.hmFundId)).ToList();
            }

            Dictionary<int, string> users;
            var hFundIds = wireStatusDetails.Select(s => s.hmFundId).ToList();

            using (var context = new AdminContext())
            {
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var userIds = wireStatusDetails.Select(s => s.LastUpdatedBy).Union(wireStatusDetails.Select(s => s.CreatedBy)).Union(wireStatusDetails.Select(s => s.ApprovedBy ?? 0)).Distinct().ToList();
                users = context.hLoginRegistrations.Where(s => UserDetails.Id == s.intLoginID || userIds.Contains(s.intLoginID)).ToDictionary(s => s.intLoginID, v => v.varLoginID.HumanizeEmail());
            }

            var hFunds = AdminFundManager.GetHFundsCreatedForDMA(hFundIds, PreferredFundNameInSession);
            var timeZones = FileSystemManager.GetAllTimeZones();
            foreach (var wire in wireStatusDetails)
            {
                wire.hmsWireMessageType.hmsWires = null;
                wire.hmsWirePurposeLkup.hmsWires = null;
                wire.hmsWireStatusLkup.hmsWires = null;
                wire.hmsWireTransferTypeLKup.hmsWires = null;
                wire.SendingAccount.hmsWires = null;
                if (wire.ReceivingAccount != null)
                    wire.ReceivingAccount.hmsWires1 = null;
                if (wire.ReceivingSSITemplate != null)
                    wire.ReceivingSSITemplate.hmsWires = null;

                //wire.ReceivingSSITemplate.onBoardingAccountSSITemplateMaps

                var fund = hFunds.FirstOrDefault(s => s.hmFundId == wire.hmFundId) ?? new HFund();
                var thisWire = new WireTicket
                {
                    HMWire = wire,
                    SendingAccount = wire.SendingAccount,//  wireAccounts.FirstOrDefault(s => wire.OnBoardAccountId == s.onBoardingAccountId) ?? new onBoardingAccount(),  //wire.onBoardingAccount
                    ReceivingAccount = wire.ReceivingAccount ?? new onBoardingAccount(), // wire.WireTransferTypeId == 2 ? wireAccounts.FirstOrDefault(s => wire.OnBoardSSITemplateId == s.onBoardingAccountId) ?? new onBoardingAccount() : new onBoardingAccount(),
                    SSITemplate = wire.ReceivingSSITemplate ?? new onBoardingSSITemplate(), //wire.WireTransferTypeId != 2 && wire.hmsWireTransferTypeLKup.TransferType != "Notice" ? wireSSITemplates.FirstOrDefault(s => wire.OnBoardSSITemplateId == s.onBoardingSSITemplateId) ?? new onBoardingSSITemplate() : new onBoardingSSITemplate(),
                    PreferredFundName = fund.PerferredFundName ?? string.Empty,
                    ShortFundName = fund.ShortFundName ?? string.Empty,
                    ClientLegalName = fund.ClientLegalName ?? string.Empty,
                    ClientShortName = fund.ClientShortName ?? string.Empty
                };

                thisWire.Deadline = GetDeadlineToApprove(thisWire.SendingAccount, thisWire.HMWire.ValueDate, timeZones);

                thisWire.SendingAccount.onBoardingAccountSSITemplateMaps = null;
                thisWire.ReceivingAccount.onBoardingAccountSSITemplateMaps = null;
                thisWire.SSITemplate.onBoardingAccountSSITemplateMaps = null;

                if (thisWire.SendingAccount.SwiftGroup != null)
                    thisWire.SendingAccount.SwiftGroup.onBoardingAccounts = null;

                if (thisWire.SendingAccount.WirePortalCutoff != null)
                    thisWire.SendingAccount.WirePortalCutoff.onBoardingAccounts = null;

                if (thisWire.ReceivingAccount.SwiftGroup != null)
                    thisWire.ReceivingAccount.SwiftGroup.onBoardingAccounts = null;

                if (thisWire.ReceivingAccount.WirePortalCutoff != null)
                    thisWire.ReceivingAccount.WirePortalCutoff.onBoardingAccounts = null;

                if (thisWire.SendingAccount.UltimateBeneficiary != null)
                    thisWire.SendingAccount.UltimateBeneficiary.onBoardingAccounts = thisWire.SendingAccount.UltimateBeneficiary.onBoardingAccounts1 = thisWire.SendingAccount.UltimateBeneficiary.onBoardingAccounts2 = null;
                else
                    thisWire.SendingAccount.UltimateBeneficiary = new onBoardingAccountBICorABA();
                if (thisWire.ReceivingAccount.UltimateBeneficiary != null)
                    thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingAccounts = thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingAccounts1 = thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingAccounts2 = null;
                else
                    thisWire.ReceivingAccount.UltimateBeneficiary = new onBoardingAccountBICorABA();


                if (thisWire.SSITemplate.Beneficiary != null)
                    thisWire.SSITemplate.Beneficiary.onBoardingAccounts = thisWire.SSITemplate.Beneficiary.onBoardingAccounts1 = thisWire.SSITemplate.Beneficiary.onBoardingAccounts2 = null;
                else
                    thisWire.SSITemplate.Beneficiary = new onBoardingAccountBICorABA();
                if (thisWire.SSITemplate.Intermediary != null)
                    thisWire.SSITemplate.Intermediary.onBoardingAccounts = thisWire.SSITemplate.Intermediary.onBoardingAccounts1 = thisWire.SSITemplate.Intermediary.onBoardingAccounts2 = null;
                if (thisWire.SSITemplate.UltimateBeneficiary != null)
                    thisWire.SSITemplate.UltimateBeneficiary.onBoardingAccounts = thisWire.SSITemplate.UltimateBeneficiary.onBoardingAccounts1 = thisWire.SSITemplate.UltimateBeneficiary.onBoardingAccounts2 = null;
                else
                    thisWire.SSITemplate.UltimateBeneficiary = new onBoardingAccountBICorABA();
                if (thisWire.SSITemplate.Beneficiary != null)
                    thisWire.SSITemplate.Beneficiary.onBoardingSSITemplates = thisWire.SSITemplate.Beneficiary.onBoardingSSITemplates1 = thisWire.SSITemplate.Beneficiary.onBoardingSSITemplates2 = null;
                if (thisWire.SSITemplate.Intermediary != null)
                    thisWire.SSITemplate.Intermediary.onBoardingSSITemplates = thisWire.SSITemplate.Intermediary.onBoardingSSITemplates1 = thisWire.SSITemplate.Intermediary.onBoardingSSITemplates2 = null;
                if (thisWire.SSITemplate.UltimateBeneficiary != null)
                    thisWire.SSITemplate.UltimateBeneficiary.onBoardingSSITemplates = thisWire.SSITemplate.UltimateBeneficiary.onBoardingSSITemplates1 = thisWire.SSITemplate.UltimateBeneficiary.onBoardingSSITemplates2 = null;


                if (thisWire.SendingAccount.Beneficiary != null)
                    thisWire.SendingAccount.Beneficiary.onBoardingAccounts = thisWire.SendingAccount.Beneficiary.onBoardingAccounts1 = thisWire.SendingAccount.Beneficiary.onBoardingAccounts2 = null;
                else
                    thisWire.SendingAccount.Beneficiary = new onBoardingAccountBICorABA();
                if (thisWire.SendingAccount.Intermediary != null)
                    thisWire.SendingAccount.Intermediary.onBoardingAccounts = thisWire.SendingAccount.Intermediary.onBoardingAccounts1 = thisWire.SendingAccount.Intermediary.onBoardingAccounts2 = null;
                if (thisWire.SendingAccount.UltimateBeneficiary != null)
                    thisWire.SendingAccount.UltimateBeneficiary.onBoardingAccounts = thisWire.SendingAccount.UltimateBeneficiary.onBoardingAccounts1 = thisWire.SendingAccount.UltimateBeneficiary.onBoardingAccounts2 = null;

                if (thisWire.ReceivingAccount.Beneficiary != null)
                    thisWire.ReceivingAccount.Beneficiary.onBoardingAccounts = thisWire.ReceivingAccount.Beneficiary.onBoardingAccounts1 = thisWire.ReceivingAccount.Beneficiary.onBoardingAccounts2 = null;
                else
                    thisWire.ReceivingAccount.Beneficiary = new onBoardingAccountBICorABA();
                if (thisWire.ReceivingAccount.Intermediary != null)
                    thisWire.ReceivingAccount.Intermediary.onBoardingAccounts = thisWire.ReceivingAccount.Intermediary.onBoardingAccounts1 = thisWire.ReceivingAccount.Intermediary.onBoardingAccounts2 = null;
                if (thisWire.ReceivingAccount.UltimateBeneficiary != null)
                    thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingAccounts = thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingAccounts1 = thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingAccounts2 = null;


                if (thisWire.SendingAccount.Beneficiary != null)
                    thisWire.SendingAccount.Beneficiary.onBoardingSSITemplates = thisWire.SendingAccount.Beneficiary.onBoardingSSITemplates1 = thisWire.SendingAccount.Beneficiary.onBoardingSSITemplates2 = null;
                if (thisWire.SendingAccount.Intermediary != null)
                    thisWire.SendingAccount.Intermediary.onBoardingSSITemplates = thisWire.SendingAccount.Intermediary.onBoardingSSITemplates1 = thisWire.SendingAccount.Intermediary.onBoardingSSITemplates2 = null;
                if (thisWire.SendingAccount.UltimateBeneficiary != null)
                    thisWire.SendingAccount.UltimateBeneficiary.onBoardingSSITemplates = thisWire.SendingAccount.UltimateBeneficiary.onBoardingSSITemplates1 = thisWire.SendingAccount.UltimateBeneficiary.onBoardingSSITemplates2 = null;

                if (thisWire.ReceivingAccount.Beneficiary != null)
                    thisWire.ReceivingAccount.Beneficiary.onBoardingSSITemplates = thisWire.ReceivingAccount.Beneficiary.onBoardingSSITemplates1 = thisWire.ReceivingAccount.Beneficiary.onBoardingSSITemplates2 = null;
                if (thisWire.ReceivingAccount.Intermediary != null)
                    thisWire.ReceivingAccount.Intermediary.onBoardingSSITemplates = thisWire.ReceivingAccount.Intermediary.onBoardingSSITemplates1 = thisWire.ReceivingAccount.Intermediary.onBoardingSSITemplates2 = null;
                if (thisWire.ReceivingAccount.UltimateBeneficiary != null)
                    thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingSSITemplates = thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingSSITemplates1 = thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingSSITemplates2 = null;


                //Update User Details
                thisWire.WireCreatedBy = users.First(s => s.Key == thisWire.HMWire.CreatedBy).Value.HumanizeEmail();
                thisWire.WireLastUpdatedBy = users.First(s => s.Key == thisWire.HMWire.LastUpdatedBy).Value.HumanizeEmail();
                thisWire.WireApprovedBy = thisWire.HMWire.ApprovedBy > 0 ? users.First(s => s.Key == thisWire.HMWire.ApprovedBy).Value.HumanizeEmail() : "-";

                SetUserTitles(thisWire);
                wireData.Add(thisWire);
            }

            //Custom ordering as per HMOS-56
            var customWireStatusOrder = new[] { 2, 5, 1, 4, 3 };
            var customSwiftStatusOrder = new[] { 2, 4, 6, 3, 5, 1 };
            wireData = wireData.OrderBy(s => Array.IndexOf(customWireStatusOrder, s.HMWire.WireStatusId)).ThenBy(s => Array.IndexOf(customSwiftStatusOrder, s.HMWire.SwiftStatusId)).ToList();
            return Json(new { wireData, AuthorizedSessionData.IsPrivilegedUser, isAdmin = User.IsInRole(OpsSecureUserRoles.DMAAdmin) });
        }

        private static void SetUserTitles(WireTicket thisWire)
        {
            //When wire is Drafted - hide Last Modified by
            if (thisWire.HMWire.WireStatusId == 1)
            {
                thisWire.WireLastUpdatedBy = "-";
                thisWire.HMWire.LastModifiedAt = new DateTime(1, 1, 1);
                thisWire.WireApprovedBy = "-";
                thisWire.HMWire.ApprovedAt = null;
            }

            //approved wire
            if ((thisWire.HMWire.WireStatusId == 3 || thisWire.HMWire.WireStatusId == 5) && thisWire.HMWire.ApprovedBy == null)
            {
                thisWire.WireApprovedBy = thisWire.WireLastUpdatedBy;
                thisWire.HMWire.ApprovedAt = thisWire.HMWire.LastModifiedAt;
            }

            //approved wire - MT210 -This has auto approval
            if (thisWire.HMWire.WireMessageTypeId == 5 && thisWire.HMWire.WireStatusId == 3 && thisWire.WireApprovedBy == "-")
            {
                thisWire.WireApprovedBy = "System";
            }

            if (thisWire.WireLastUpdatedBy == thisWire.WireCreatedBy)
            {
                thisWire.WireLastUpdatedBy = "-";
                thisWire.HMWire.LastModifiedAt = new DateTime(1, 1, 1);
            }
        }

        public JsonResult GetWireMessageTypeDetails(string module)
        {
            using (var context = new OperationsSecureContext())
            {
                var wireMessageTypes = context.hmsWireMessageTypes.ToList();
                var thisModuleMessageTypes = MessageTypes.ContainsKey(module) ? MessageTypes[module] : new List<string>();
                var wireMessages = wireMessageTypes.Select(s => new { id = s.hmsWireMessageTypeId, text = s.MessageType }).ToList();
                var wireTransferTypes = context.hmsWireTransferTypeLKups.Select(s => new { id = s.WireTransferTypeId, text = s.TransferType }).ToList();
                var wireSenderInformation = context.hmsWireSenderInformations.ToList();
                return Json(new { wireMessages, wireTransferTypes, wireSenderInformation = wireSenderInformation.Select(s => new { id = s.hmsWireSenderInformationId, text = string.Format("{0}-{1}", s.SenderInformation, s.Description), value = s.SenderInformation }).ToList() });
            }
        }

        public Dictionary<string, List<string>> MessageTypes = new Dictionary<string, List<string>>()
        {
            { ReportName.AdhocWireReport , new List<string>() { "MT103", "MT202", "MT202 COV", "MT210", "MT540", "MT542" } },
            { ReportName.Collateral, new List<string>() { "MT103", "MT202", "MT202 COV", "MT210" } },
            { ReportName.Invoices, new List<string>() { "MT103", "MT202", "MT202 COV" } }
        };


        public JsonResult GetWireDetails(long wireId)
        {
            var wireTicket = WireDataManager.GetWireData(wireId);
            var wireTicketStatus = new WireTicketStatus(wireTicket, UserId, User.IsWireApprover());
            var fundAccounts = new List<WireAccountBaseData>();
            long reportId = 0;

            if (wireTicketStatus.IsEditEnabled)
            {
                if (!wireTicketStatus.IsWirePurposeAdhoc)
                    reportId = FileSystemManager.GetReportId(wireTicket.HMWire.hmsWirePurposeLkup.ReportName);

                fundAccounts = wireTicketStatus.IsWirePurposeAdhoc
                    ? WireDataManager.GetApprovedFundAccounts(wireTicket.HMWire.hmFundId, wireTicket.IsFundTransfer, wireTicket.SendingAccount.Currency)
                    : WireDataManager.GetApprovedFundAccountsForModule(wireTicket.HMWire.hmFundId, wireTicket.HMWire.OnBoardSSITemplateId ?? 0, reportId);
            }

            var sendingAccountsList = fundAccounts.Where(s => s.IsAuthorizedSendingAccount).Select(s => new { id = s.OnBoardAccountId, text = s.AccountNameAndNumber }).ToList();
            var receivingAccountsList = fundAccounts.Select(s => new { id = s.OnBoardAccountId, text = s.AccountNameAndNumber }).ToList();

            //Also include who is currently viewing this wire 
            var currentlyViewedBy = GetCurrentlyViewingUsers(wireId);
            var deadlineToApprove = GetDeadlineToApprove(wireTicket.SendingAccount, wireTicket.HMWire.ValueDate);

            var wireSourceModule = GetWireSourceDetails(wireTicket);

            return Json(new { wireTicket, wireTicketStatus, deadlineToApprove, sendingAccountsList, receivingAccountsList, IsWireCreated = false, currentlyViewedBy, wireSourceModule });
        }

        private WireSourceDetails GetWireSourceDetails(WireTicket wireTicket)
        {
            var wireSourceModule = new WireSourceDetails();

            if (wireTicket.HMWire.hmsWireInvoiceAssociations.Any())
            {
                wireSourceModule.SourceModuleName = "Invoices";

                using (var context = new OperationsContext())
                {
                    var invoiceId = wireTicket.HMWire.hmsWireInvoiceAssociations.Last().InvoiceId;
                    var invoiceReport = context.vw_dmaInvoiceReport.First(s => s.dmaInvoiceReportId == invoiceId);

                    wireSourceModule.AttachmentName = invoiceReport.FileName;
                    wireSourceModule.FileSource = invoiceReport.FileSource;
                    wireSourceModule.SourceModuleId = invoiceId;

                    wireSourceModule.Details.Add("Invoice No", invoiceReport.InvoiceNo);
                    wireSourceModule.Details.Add("Invoice Date", invoiceReport.InvoiceDate.ToShortDateString());
                    wireSourceModule.Details.Add("Amount", invoiceReport.Amount.ToCurrency());
                    wireSourceModule.Details.Add("Fee Type", invoiceReport.FeeType);
                    wireSourceModule.Details.Add("Currency", invoiceReport.Currency);
                    wireSourceModule.Details.Add("Pay Date", invoiceReport.PaidDate.ToDateString());
                    wireSourceModule.Details.Add("Service Provider", invoiceReport.Vendor);
                }

            }
            else if (wireTicket.HMWire.hmsWireCollateralAssociations.Any())
            {
                wireSourceModule.SourceModuleName = "Collateral Report";

                using (var context = new OperationsContext())
                {
                    var opsCashCollateralId = wireTicket.HMWire.hmsWireCollateralAssociations.First().dmaCashCollateralId;
                    var collateralReport = context.dmaOpsCashCollaterals.Include(a => a.dmaCollateralData).First(s => s.dmaOpsCashCollateralId == opsCashCollateralId);

                    wireSourceModule.Details.Add("Counterparty", collateralReport.dmaCollateralData.BrokerName);
                    wireSourceModule.Details.Add("Collateral Pledged to / (by) Fund (System Balance)", collateralReport.dmaCollateralData.CollateralPledgedToByFundSystemBalance.ToCurrency());
                    wireSourceModule.Details.Add("Collateral Pledged to / (by) Fund (Verified Balance)", collateralReport.dmaCollateralData.CollateralPledgedToByFundVerifiedBalance.ToCurrency());
                    wireSourceModule.Details.Add("Collateral Pending to / (from) Fund", collateralReport.dmaCollateralData.CollateralPendingToFromFund.ToCurrency());
                    wireSourceModule.Details.Add("Exposure / MTM", collateralReport.dmaCollateralData.ExposureOrMtm.ToCurrency());
                    if (collateralReport.dmaCollateralData.IsCollateralReport)
                        wireSourceModule.Details.Add("Independent Amount (CounterParty)", collateralReport.dmaCollateralData.IndependentAmount.ToCurrency());
                    wireSourceModule.Details.Add("Credit Support Amount", collateralReport.dmaCollateralData.CreditSupportAmount.ToCurrency());
                    wireSourceModule.Details.Add("Agreed Movement to / (from) Fund", collateralReport.dmaCollateralData.AgreedMovementToFromFund.ToCurrency());

                    //Cash Collateral details
                    wireSourceModule.Details.Add("Eligible Currency", collateralReport.EligibleCurrency);
                    wireSourceModule.Details.Add("Settlement Date", collateralReport.SettlementDate.ToShortDateString());
                    wireSourceModule.Details.Add("Agreed Movement Type", collateralReport.AgreedMovementType);
                    wireSourceModule.Details.Add("Local Collateral Value", collateralReport.CollateralValue.ToCurrency());
                    wireSourceModule.Details.Add("Base Collateral Value", collateralReport.BaseCollateralValue.ToCurrency());
                    wireSourceModule.Details.Add("FX Rate", collateralReport.FXRate.ToCurrency());
                    wireSourceModule.Details.Add("Deliver Amount", collateralReport.PledgeAmount.ToCurrency());
                    wireSourceModule.Details.Add("Return Amount", collateralReport.ReturnAmount.ToCurrency());

                }
            }

            //else
            //{
            //    wireSourceModule.SourceModuleName = "Sample Header";
            //    wireSourceModule.AttachmentName = "SampleFile.xls";
            //    wireSourceModule.Details.Add("Service Provider", "CastorOil");
            //    wireSourceModule.Details.Add("Pay Date", "2332333"); 
            //}

            return wireSourceModule;
        }

        public JsonResult GetNewWireDetails()
        {
            var wireTicket = new WireTicket()
            {
                HMWire = new hmsWire(),
                SendingAccount = new onBoardingAccount(),
                ReceivingAccount = new onBoardingAccount(),
                SSITemplate = new onBoardingSSITemplate(),
                Counterparty = "",
                AttachmentUsers = new List<string>(),
                WorkflowUsers = new List<string>()
            };
            var wireTicketStatus = new WireTicketStatus(wireTicket, UserId, User.IsWireApprover(), true);
            return Json(new { wireTicket, wireTicketStatus, IsWireCreated = false });
        }

        private List<string> GetCurrentlyViewingUsers(long wireId)
        {
            if (wireId == 0)
                return new List<string>();

            var readableName = UserName.HumanizeEmail();
            using (var context = new OperationsSecureContext())
            {
                var allUsers = context.hmsActionInProgresses.Where(s => s.hmsWireId == wireId).Select(s => s.UserName).ToList();

                //remove all actionsInProgress for this User - as he will be able to do one wire at a time.
                var allWires = context.hmsActionInProgresses.Where(s => s.UserName == readableName).ToList();

                if (allWires.Any())
                {
                    context.hmsActionInProgresses.RemoveRange(allWires);
                    context.SaveChanges();
                }

                context.hmsActionInProgresses.Add(new hmsActionInProgress() { UserName = readableName, hmsWireId = wireId, RecCreatedDt = DateTime.Now });
                context.SaveChanges();

                if (!allUsers.Contains(readableName))
                    return allUsers;

                //same user name should not appear on his item
                allUsers.Remove(readableName);
                return allUsers;
            }
        }

        public void RemoveActionInProgress(long wireId)
        {
            var readableName = UserName.HumanizeEmail();
            using (var context = new OperationsSecureContext())
            {
                var thisAction = context.hmsActionInProgresses.FirstOrDefault(s => s.hmsWireId == wireId && s.UserName == readableName);

                if (thisAction == null)
                    return;

                context.hmsActionInProgresses.Remove(thisAction);
                context.SaveChanges();
            }
        }

        public JsonResult IsWireCreated(DateTime valueDate, string purpose, long sendingAccountId, long receivingAccountId, long receivingSSITemplateId, long wireId)
        {
            var isWireCreated = WireDataManager.IsWireCreated(valueDate, purpose, sendingAccountId, receivingAccountId, receivingSSITemplateId, wireId);
            return Json(isWireCreated);
        }

        public void SaveWire(WireTicket wireTicket, int statusId, string comment)
        {
            try
            {
                wireTicket = WireDataManager.SaveWireData(wireTicket, (WireDataManager.WireStatus)statusId, comment, UserDetails.Id);
                var deadlineToApprove = GetDeadlineToApprove(wireTicket.SendingAccount, wireTicket.HMWire.ValueDate);
                var daysToAdd = deadlineToApprove.Hours / 24;
                SaveWireScheduleInfo(wireTicket, (WireDataManager.WireStatus)statusId, UserDetails.Id, daysToAdd);
                var tempFilePath = string.Format("Temp\\{0}", UserName);

                foreach (var file in wireTicket.HMWire.hmsWireDocuments)
                {
                    var fileName = string.Format("{0}\\{1}\\{2}", FileSystemManager.OpsSecureWiresFilesPath, tempFilePath, file.FileName);
                    var fileInfo = new FileInfo(fileName);

                    if (!System.IO.File.Exists(fileInfo.FullName))
                        continue;

                    var newFileName = string.Format("{0}\\{1}\\{2}", FileSystemManager.OpsSecureWiresFilesPath, wireTicket.WireId, file.FileName);
                    var newFileInfo = new FileInfo(newFileName);

                    if (newFileInfo.Directory != null && !Directory.Exists(newFileInfo.Directory.FullName))
                        Directory.CreateDirectory(newFileInfo.Directory.FullName);

                    if (System.IO.File.Exists(newFileInfo.FullName))
                        continue;

                    System.IO.File.Copy(fileName, newFileName);
                    System.IO.File.Delete(fileInfo.FullName);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                Response.StatusCode = 500;
                Response.StatusDescription = ex.Message;
            }
        }

        private static void SaveWireScheduleInfo(WireTicket wire, WireDataManager.WireStatus workflowStatus, int userId, int daysToAdd)
        {

            var thisWireSchedule = WireDataManager.GetJobSchedule(wire.WireId);
            var scheduleName = OverdueWireCancellationScheduleManager.GetJobName(wire.WireId);

            if (workflowStatus != WireDataManager.WireStatus.Initiated)
            {
                if (thisWireSchedule != null)
                {
                    thisWireSchedule.IsJobCreated = false;
                    thisWireSchedule.IsJobInActive = true;
                    thisWireSchedule.LastModifiedAt = DateTime.Now;
                    thisWireSchedule.UpdatedBy = userId;
                }
                else
                    return;
            }
            else if (thisWireSchedule != null)
            {
                thisWireSchedule.LastModifiedAt = DateTime.Now;
                thisWireSchedule.UpdatedBy = userId;
                thisWireSchedule.IsJobCreated = true;
                thisWireSchedule.IsJobInActive = false;

            }
            else
            {
                thisWireSchedule = new hmsWireJobSchedule
                {
                    hmsWireId = wire.WireId,
                    IsJobCreated = true,
                    IsJobInActive = false,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedBy = userId,
                    LastModifiedAt = DateTime.Now
                };
            }
            thisWireSchedule.ScheduledDate = wire.HMWire.ValueDate.AddDays(daysToAdd).Date.Add(new TimeSpan(23, 59, 0));
            WireDataManager.EditJobSchedule(thisWireSchedule);
            var jobId = BackgroundJob.Schedule(() => OverdueWireCancellationScheduleManager.CancelThisWire(wire.WireId, thisWireSchedule), new DateTimeOffset(thisWireSchedule.ScheduledDate));
            if (thisWireSchedule.IsJobInActive)
                BackgroundJob.Delete(jobId);
        }

        public static void RemoveWireDocument(long documentId)
        {
            WireDataManager.RemoveWireDocument(documentId);
        }

        public JsonResult UploadWireFiles(long wireId)
        {
            var aDocments = new List<hmsWireDocument>();
            var tempFilePath = string.Format("Temp\\{0}", UserName);
            for (var i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];

                if (file == null)
                    throw new Exception("unable to retrive file information");

                var fileName = string.Format("{0}\\{1}\\{2}", FileSystemManager.OpsSecureWiresFilesPath, (wireId > 0 ? wireId.ToString() : tempFilePath), file.FileName);
                var fileInfo = new FileInfo(fileName);

                if (fileInfo.Directory != null && !Directory.Exists(fileInfo.Directory.FullName))
                    Directory.CreateDirectory(fileInfo.Directory.FullName);

                if (System.IO.File.Exists(fileInfo.FullName))
                {
                    WireDataManager.RemoveWireDocument(wireId, file.FileName);
                    System.IO.File.Delete(fileInfo.FullName);
                }

                file.SaveAs(fileInfo.FullName);

                //Build account document
                var document = new hmsWireDocument
                {
                    FileName = file.FileName,
                    CreatedAt = DateTime.Now,
                    CreatedBy = UserDetails.Id,
                    hmsWireId = wireId
                };
                aDocments.Add(document);
            }

            return Json(new
            {
                Documents = aDocments.Select(document => new
                {
                    document.hmsWireDocumentId,
                    document.hmsWireId,
                    document.FileName,
                    document.CreatedAt,
                    document.CreatedBy
                }).ToList()
            }, JsonContentType, JsonContentEncoding);
        }

        public FileResult DownloadWireFile(string fileName, long wireId)
        {
            var tempFilePath = string.Format("Temp\\{0}", UserName);
            var file = new FileInfo(string.Format("{0}{1}\\{2}", FileSystemManager.OpsSecureWiresFilesPath, (wireId > 0 ? wireId.ToString() : tempFilePath), fileName));
            return DownloadFile(file, fileName);
        }

        public FileResult DownloadInvoiceWireFile(long sourceModuleId)
        {
            using (var context = new OperationsContext())
            {
                var invoiceReport = context.vw_dmaInvoiceReport.First(s => s.dmaInvoiceReportId == sourceModuleId);
                var file = GetInvoiceFileLocation(invoiceReport);
                return DownloadFile(file, invoiceReport.FileName);
            }
        }

        private FileInfo GetInvoiceFileLocation(vw_dmaInvoiceReport invoice)
        {
            switch (invoice.FileSource)
            {
                case "Manual":
                    return new FileInfo(string.Format("{0}/{1}/{2}", FileSystemManager.InvoicesFileAttachement, invoice.dmaInvoiceReportId, invoice.FileName));
                case "Overriden":
                    return new FileInfo(string.Format("{0}/{1}/{2}", FileSystemManager.RawFilesOverridesPath, invoice.OriginalContextMonth.ToString("yyyy-MM-dd"), invoice.FileName));
                default:
                    return invoice.FileSource == "Config"
                ? new FileInfo(string.Format("{0}{1}", FileSystemManager.InternalConfigFiles, invoice.FileName))
                : new FileInfo(string.Format("{0}{1}",
                    (invoice.FileSource == "Overriden") && (!invoice.FileName.StartsWith("Overrides\\") || !invoice.FileName.StartsWith("Overrides/"))
                        ? FileSystemManager.RawFilesOverridesPath
                        : FileSystemManager.SftpRawFilesOfHM, invoice.FilePath));  // FileOriginManager.GetRawFileDirectoryIncludingSubSir(fileOrigin, contextDate)
            }
        }

        public JsonResult GetTimeToApproveTheWire(long onboardingAccountId, DateTime valueDate)
        {
            var onboardAccount = FundAccountManager.GetOnBoardingAccount(onboardingAccountId);
            var timeToApprove = GetDeadlineToApprove(onboardAccount, valueDate);
            return Json(timeToApprove);
        }

        private TimeSpan GetDeadlineToApprove(onBoardingAccount onboardAccount, DateTime valueDate, Dictionary<string, string> timeZones = null)
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

            var cashSweepTimeDeadline = GetCutOffTime(onboardAccount.CashSweepTime ?? new TimeSpan(23, 59, 0), valueDate, onboardAccount.CashSweepTimeZone, timeZones, destinationTimeZone);
            return cashSweepTimeDeadline < cutOffTimeDeadline ? cashSweepTimeDeadline - currentTime : cutOffTimeDeadline - currentTime;
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

        public JsonResult GetWirePurposes()
        {
            List<hmsWirePurposeLkup> wirePurposes;
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                wirePurposes = context.hmsWirePurposeLkups.ToList();
            }

            var userIds = wirePurposes.Select(s => s.CreatedBy).Union(wirePurposes.Where(s => s.ModifiedBy != null).Select(s => (int)s.ModifiedBy)).ToList();
            Dictionary<int, string> users;
            using (var context = new AdminContext())
            {
                users = context.hLoginRegistrations.Where(s => UserDetails.Id == s.intLoginID || userIds.Contains(s.intLoginID)).ToDictionary(s => s.intLoginID, v => v.varLoginID.HumanizeEmail());
            }

            var allPurposes = wirePurposes.Select(wirePurpose =>
                new
                {
                    hmsWirePurposeId = wirePurpose.hmsWirePurposeId,
                    ReportName = wirePurpose.ReportName,
                    Purpose = wirePurpose.Purpose,
                    CreatedBy = wirePurpose.CreatedBy > 0 ? users.ContainsKey(wirePurpose.CreatedBy) ? users[wirePurpose.CreatedBy] : "Unknown User" : "System",
                    CreatedAt = wirePurpose.CreatedAt,
                    ModifiedBy = wirePurpose.ModifiedBy == null ? "-" : wirePurpose.ModifiedBy > 0 ? users.ContainsKey((int)wirePurpose.ModifiedBy) ? users[(int)wirePurpose.ModifiedBy] : "Unknown User" : "System",
                    ModifiedAt = wirePurpose.ModifiedAt,
                    IsApproved = wirePurpose.IsApproved,
                    IsRejected = !wirePurpose.IsApproved && wirePurpose.ModifiedBy != null,
                    IsAuthorizedToApprove = wirePurpose.CreatedBy != UserDetails.Id
                });

            return Json(allPurposes);

        }

        public void AddWirePurpose(string reportName, string purpose)
        {
            using (var context = new OperationsSecureContext())
            {
                var wirePurpose = new hmsWirePurposeLkup()
                {
                    ReportName = reportName,
                    Purpose = purpose,
                    CreatedBy = UserDetails.Id,
                    CreatedAt = DateTime.Now
                };

                context.hmsWirePurposeLkups.Add(wirePurpose);
                context.SaveChanges();
            }
        }

        public void ApproveOrRejectWirePurpose(int wirePurposeId, bool isApproved)
        {
            using (var context = new OperationsSecureContext())
            {
                var wirePurpose = context.hmsWirePurposeLkups.First(s => s.hmsWirePurposeId == wirePurposeId);
                wirePurpose.ModifiedAt = DateTime.Now;
                wirePurpose.ModifiedBy = UserDetails.Id;
                wirePurpose.IsApproved = isApproved;
                context.SaveChanges();
            }
        }

        #region Adhoc Wires

        public JsonResult GetAdhocWireAssociations()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                var adhocWirePurposes = context.hmsWirePurposeLkups.Where(s => s.ReportName == ReportName.AdhocWireReport && s.IsApproved).ToList();
                var wirePurposes = adhocWirePurposes.Select(s => new { id = s.hmsWirePurposeId, text = s.Purpose }).ToList();
                var currencies = context.hmsCurrencies.AsNoTracking().Select(s => new { id = s.Currency, text = s.Currency }).ToList();
                return Json(new { wirePurposes, currencies });
            }
        }
        private class AgreementBaseDetails
        {
            public long AgreementId { get; set; }
            public string AgreementShortName { get; set; }
        }

        public JsonResult GetAuthorizedFunds()
        {
            using (var context = new OperationsContext())
            {
                var authorizedFundIds = AuthorizedSessionData.HMFundIds.Select(s => s.Id).ToList();
                var fundsWithApprovedAccounts = FundAccountManager.GetFundsOfApprovedAccounts();
                authorizedFundIds = authorizedFundIds.Count == 0 ? fundsWithApprovedAccounts : authorizedFundIds.Intersect(fundsWithApprovedAccounts).ToList();
                var hFunds = AdminFundManager.GetUniversalDMAFundListQuery(context, PreferredFundNameInSession)
                    .Where(s => authorizedFundIds.Contains(s.hmFundId)).OrderBy(s => s.PreferredFundName)
                    .Select(s => new { id = s.hmFundId, text = s.PreferredFundName }).ToList();

                return Json(hFunds, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetApprovedAgreementsForFund(long fundId)
        {
            List<AgreementBaseDetails> onBoardedAgreements;
            using (var context = new AdminContext())
            {
                onBoardedAgreements = (from oAgreement in context.vw_CounterpartyAgreements
                                       where oAgreement.FundMapId == fundId && oAgreement.HMOpsStatus == "Approved"
                                       select new AgreementBaseDetails { AgreementId = oAgreement.dmaAgreementOnBoardingId, AgreementShortName = oAgreement.AgreementShortName }).ToList();
            }

            using (var context = new OperationsSecureContext())
            {
                var qualifiedData = (from agr in onBoardedAgreements
                                     join oAcc in context.onBoardingAccounts on agr.AgreementId equals oAcc.dmaAgreementOnBoardingId
                                     select new { id = agr.AgreementId, text = agr.AgreementShortName }).ToList();

                return Json(qualifiedData);
            }
        }

        public JsonResult GetApprovedAccountsForFund(long fundId, bool isFundTransfer)
        {
            var fundAccounts = WireDataManager.GetApprovedFundAccounts(fundId, isFundTransfer);
            var sendingAccountsList = fundAccounts.Where(s => s.IsAuthorizedSendingAccount).Select(s => new { id = s.OnBoardAccountId, text = s.AccountNameAndNumber, Currency = s.Currency }).ToList();
            var receivingAccountsList = fundAccounts.Select(s => new { id = s.OnBoardAccountId, text = s.AccountNameAndNumber, Currency = s.Currency }).ToList();
            var currencies = sendingAccountsList.Select(s => new { id = s.Currency, text = s.Currency }).Distinct().OrderBy(s => s.text).ToList();
            return Json(new { sendingAccountsList, receivingAccountsList, currencies });
        }

        public JsonResult GetApprovedSSITemplatesForAccount(long accountId, bool isNormalTransfer)
        {
            List<onBoardingSSITemplate> receivingAccounts;
            using (var context = new OperationsSecureContext())
            {
                var templateType = isNormalTransfer ? "Broker" : "Fee/Expense Payment";

                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                receivingAccounts = context.onBoardingSSITemplates.Include(s => s.Beneficiary)
                    .Include(s => s.Intermediary)
                    .Include(s => s.UltimateBeneficiary)
                    .Include(x => x.onBoardingSSITemplateDocuments)
                    .Include(x => x.onBoardingAccountSSITemplateMaps)
                    .Where(s => s.onBoardingAccountSSITemplateMaps.Any(s1 => s1.Status == "Approved" && s1.onBoardingAccountId == accountId))
                    .Where(s => s.SSITemplateType == templateType)
                    .ToList();
            }

            //remove proxies to avoid circular dependency issue
            receivingAccounts.ForEach(ssiTemplate =>
            {
                if (ssiTemplate.Beneficiary == null)
                    ssiTemplate.Beneficiary = new onBoardingAccountBICorABA();
                if (ssiTemplate.Intermediary == null)
                    ssiTemplate.Intermediary = new onBoardingAccountBICorABA();
                if (ssiTemplate.UltimateBeneficiary == null)
                    ssiTemplate.UltimateBeneficiary = new onBoardingAccountBICorABA();
                if (ssiTemplate.onBoardingSSITemplateDocuments == null)
                    ssiTemplate.onBoardingSSITemplateDocuments = new List<onBoardingSSITemplateDocument>();
                if (ssiTemplate.onBoardingAccountSSITemplateMaps == null)
                    ssiTemplate.onBoardingAccountSSITemplateMaps = new List<onBoardingAccountSSITemplateMap>();

                //remove circular references
                ssiTemplate.Beneficiary.onBoardingSSITemplates = ssiTemplate.Beneficiary.onBoardingSSITemplates1 = ssiTemplate.Beneficiary.onBoardingSSITemplates2 = null;
                ssiTemplate.Intermediary.onBoardingSSITemplates = ssiTemplate.Intermediary.onBoardingSSITemplates1 = ssiTemplate.Intermediary.onBoardingSSITemplates2 = null;
                ssiTemplate.UltimateBeneficiary.onBoardingSSITemplates = ssiTemplate.UltimateBeneficiary.onBoardingSSITemplates1 = ssiTemplate.UltimateBeneficiary.onBoardingSSITemplates2 = null;
                ssiTemplate.onBoardingSSITemplateDocuments.ForEach(s => s.onBoardingSSITemplate = null);
                ssiTemplate.onBoardingAccountSSITemplateMaps.ForEach(s => { s.onBoardingSSITemplate = null; s.onBoardingAccount = null; });

            });

            using (var context = new AdminContext())
            {
                var entityIds = receivingAccounts.Select(s => s.TemplateEntityId).Distinct().ToList();
                var counterparties = context.dmaCounterPartyOnBoardings.Where(s => entityIds.Contains(s.dmaCounterPartyOnBoardId)).ToDictionary(s => s.dmaCounterPartyOnBoardId.ToString(), v => v.CounterpartyName);
                var receivingAccountList = receivingAccounts.Select(s => new { id = s.onBoardingSSITemplateId, text = string.IsNullOrWhiteSpace(s.FFCNumber) ? string.Format("{0}-{1}", s.UltimateBeneficiaryAccountNumber, s.TemplateName) : string.Format("{0}-{1}-{2}", s.FFCNumber, s.UltimateBeneficiaryAccountNumber, s.TemplateName) }).ToList();
                return Json(new { receivingAccounts, receivingAccountList, counterparties });
            }
        }


        public JsonResult GetFundAccount(long onBoardingAccountId, DateTime valueDate)
        {
            var onboardAccount = FundAccountManager.GetOnBoardingAccount(onBoardingAccountId);
            var deadlineToApprove = GetDeadlineToApprove(onboardAccount, valueDate);

            return Json(new { onboardAccount, deadlineToApprove });
        }

        public JsonResult GetCashBalances(long sendingAccountId, DateTime valueDate)
        {
            var cashBalance = FundAccountManager.GetAccountCashBalances(sendingAccountId, valueDate);
            return Json(cashBalance);
        }


        public JsonResult ValidateAccountDetails(string wireMessageType, onBoardingAccount account, onBoardingAccount receivingAccount, onBoardingSSITemplate ssiTemplate, bool isFundTransfer)
        {
            bool isMandatoryFieldsMissing = false;
            string validationMsg = string.Empty;
            switch (wireMessageType)
            {
                case "MT103":
                    if (isFundTransfer)
                        isMandatoryFieldsMissing = (string.IsNullOrWhiteSpace(account.SwiftGroup.SendersBIC) || string.IsNullOrWhiteSpace(account.Reference) || string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(receivingAccount.Currency) || string.IsNullOrWhiteSpace(receivingAccount.UltimateBeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(receivingAccount.Description) || string.IsNullOrWhiteSpace(receivingAccount.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(receivingAccount.IntermediaryAccountNumber) || string.IsNullOrWhiteSpace(receivingAccount.Intermediary.BICorABA) || string.IsNullOrWhiteSpace(receivingAccount.Intermediary.BankName) || string.IsNullOrWhiteSpace(receivingAccount.Intermediary.BankAddress) ||
                                                   string.IsNullOrWhiteSpace(receivingAccount.BeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(receivingAccount.Beneficiary.BICorABA) || string.IsNullOrWhiteSpace(receivingAccount.Beneficiary.BankName) || string.IsNullOrWhiteSpace(receivingAccount.Beneficiary.BankAddress));
                    else
                        isMandatoryFieldsMissing = (string.IsNullOrWhiteSpace(account.SwiftGroup.SendersBIC) || string.IsNullOrWhiteSpace(account.Reference) || string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(ssiTemplate.Currency) || string.IsNullOrWhiteSpace(ssiTemplate.UltimateBeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(ssiTemplate.ReasonDetail) || string.IsNullOrWhiteSpace(ssiTemplate.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(ssiTemplate.IntermediaryAccountNumber) || string.IsNullOrWhiteSpace(ssiTemplate.Intermediary.BICorABA) || string.IsNullOrWhiteSpace(ssiTemplate.Intermediary.BankName) || string.IsNullOrWhiteSpace(ssiTemplate.Intermediary.BankAddress) ||
                                                   string.IsNullOrWhiteSpace(ssiTemplate.BeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(ssiTemplate.Beneficiary.BICorABA) || string.IsNullOrWhiteSpace(ssiTemplate.Beneficiary.BankName) || string.IsNullOrWhiteSpace(ssiTemplate.Beneficiary.BankAddress));
                    validationMsg = "Sender's Account Institution, BIC & Receiver's  Account Institution, Intermediary Institution, Benificiary Institution, Currency fields and Description are required to initiate this wire";
                    break;
                case "MT202":
                case "MT202 COV":
                    if (isFundTransfer)
                        isMandatoryFieldsMissing = (string.IsNullOrWhiteSpace(account.SwiftGroup.SendersBIC) || string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(receivingAccount.Currency) || string.IsNullOrWhiteSpace(receivingAccount.UltimateBeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(receivingAccount.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(receivingAccount.IntermediaryAccountNumber) || string.IsNullOrWhiteSpace(receivingAccount.Intermediary.BICorABA) || string.IsNullOrWhiteSpace(receivingAccount.Intermediary.BankName) || string.IsNullOrWhiteSpace(receivingAccount.Intermediary.BankAddress) ||
                                                   string.IsNullOrWhiteSpace(receivingAccount.BeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(receivingAccount.Beneficiary.BICorABA) || string.IsNullOrWhiteSpace(receivingAccount.Beneficiary.BankName) || string.IsNullOrWhiteSpace(receivingAccount.Beneficiary.BankAddress));

                    else
                        isMandatoryFieldsMissing = (string.IsNullOrWhiteSpace(account.SwiftGroup.SendersBIC) || string.IsNullOrWhiteSpace(account.Reference) || string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(ssiTemplate.Currency) || string.IsNullOrWhiteSpace(ssiTemplate.UltimateBeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(ssiTemplate.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(ssiTemplate.IntermediaryAccountNumber) || string.IsNullOrWhiteSpace(ssiTemplate.Intermediary.BICorABA) || string.IsNullOrWhiteSpace(ssiTemplate.Intermediary.BankName) || string.IsNullOrWhiteSpace(ssiTemplate.Intermediary.BankAddress) ||
                                                   string.IsNullOrWhiteSpace(ssiTemplate.BeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(ssiTemplate.Beneficiary.BICorABA) || string.IsNullOrWhiteSpace(ssiTemplate.Beneficiary.BankName) || string.IsNullOrWhiteSpace(ssiTemplate.Beneficiary.BankAddress));
                    validationMsg = "Sender's Account Institution, BIC & Receiver's Account Institution, Intermediary Institution, Benificiary Institution and Currency fields are required to initiate this wire";
                    break;
                case "MT210":
                    isMandatoryFieldsMissing = (string.IsNullOrWhiteSpace(account.SwiftGroup.SendersBIC) || string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountName) || string.IsNullOrWhiteSpace(account.Currency));
                    validationMsg = "Sender's Account Institution, BIC and Currency fields are required to initiate this wire";
                    break;
                case "MT540":
                case "MT542": break;
            }
            return Json(new { isMandatoryFieldsMissing, validationMsg });
        }
        #endregion
    }
}