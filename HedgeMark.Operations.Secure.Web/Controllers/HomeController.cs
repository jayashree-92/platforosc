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

            public int Total
            {
                get
                {
                    return Pending + Approved + Cancelled + Completed + Failed + CancelledAndProcessing;
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
                }
            }
            return Json(wireStatusCount);
        }

        public JsonResult GetWireStatusDetails(DateTime startContextDate, DateTime endContextDate, string statusIds)
        {
            var wireData = new List<WireTicket>();
            List<hmsWire> wireStatusDetails;
            List<onBoardingAccount> wireAccounts;
            List<onBoardingSSITemplate> wireSSITemplates;
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
                    .Include("hmsWireMessageType")
                    .Include("hmsWirePurposeLkup")
                    .Include("hmsWireStatusLkup")
                    .Include("hmsWireTransferTypeLKup")
                    //.Include("onBoardingAccount")
                    //.Include("onBoardingSSITemplate")
                    .Where(s => ((allStatusIds.Contains(0) || allStatusIds.Contains(2)) && s.WireStatusId == 2) || s.ValueDate >= startContextDate && s.ValueDate <= endContextDate && (allStatusIds.Contains(0) || allStatusIds.Contains(s.WireStatusId))
                                                    || DbFunctions.TruncateTime(s.CreatedAt) == DbFunctions.TruncateTime(endContextDate) && (allStatusIds.Contains(0) || allStatusIds.Contains(s.WireStatusId))).ToList();


                var accountIds = wireStatusDetails.Select(s => s.OnBoardAccountId).Union(wireStatusDetails.Where(s => s.WireTransferTypeId == 2).Select(s => s.OnBoardSSITemplateId)).Distinct().ToList();
                var ssiTemplateIds = wireStatusDetails.Select(s => s.OnBoardSSITemplateId).Distinct().ToList();

                wireAccounts = context.onBoardingAccounts.Include(s => s.UltimateBeneficiary).Where(s => accountIds.Contains(s.onBoardingAccountId)).ToList();
                wireSSITemplates = context.onBoardingSSITemplates.Where(s => ssiTemplateIds.Contains(s.onBoardingSSITemplateId)).ToList();
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

            foreach (var wire in wireStatusDetails)
            {
                wire.hmsWireMessageType.hmsWires = null;
                wire.hmsWirePurposeLkup.hmsWires = null;
                wire.hmsWireStatusLkup.hmsWires = null;
                wire.hmsWireTransferTypeLKup.hmsWires = null;

                var fund = hFunds.FirstOrDefault(s => s.HFundId == wire.hmFundId) ?? new HFund();
                var thisWire = new WireTicket
                {
                    HMWire = wire,
                    SendingAccount = wireAccounts.FirstOrDefault(s => wire.OnBoardAccountId == s.onBoardingAccountId) ?? new onBoardingAccount(),  //wire.onBoardingAccount
                    ReceivingAccount = wire.WireTransferTypeId == 2 ? wireAccounts.FirstOrDefault(s => wire.OnBoardSSITemplateId == s.onBoardingAccountId) ?? new onBoardingAccount() : new onBoardingAccount(),
                    SSITemplate = wire.WireTransferTypeId != 2 && wire.hmsWireTransferTypeLKup.TransferType != "Notice" ? wireSSITemplates.FirstOrDefault(s => wire.OnBoardSSITemplateId == s.onBoardingSSITemplateId) ?? new onBoardingSSITemplate() : new onBoardingSSITemplate(),
                    PreferredFundName = fund.PerferredFundName ?? string.Empty,
                    ShortFundName = fund.ShortFundName ?? string.Empty,
                    ClientLegalName = fund.ClientLegalName ?? string.Empty,
                    ClientShortName = fund.ClientShortName ?? string.Empty
                };

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
                if (thisWire.SSITemplate.Intermediary != null)
                    thisWire.SSITemplate.Intermediary.onBoardingAccounts = thisWire.SSITemplate.Intermediary.onBoardingAccounts1 = thisWire.SSITemplate.Intermediary.onBoardingAccounts2 = null;
                if (thisWire.SSITemplate.UltimateBeneficiary != null)
                    thisWire.SSITemplate.UltimateBeneficiary.onBoardingAccounts = thisWire.SSITemplate.UltimateBeneficiary.onBoardingAccounts1 = thisWire.SSITemplate.UltimateBeneficiary.onBoardingAccounts2 = null;

                if (thisWire.SendingAccount.Beneficiary != null)
                    thisWire.SendingAccount.Beneficiary.onBoardingAccounts = thisWire.SendingAccount.Beneficiary.onBoardingAccounts1 = thisWire.SendingAccount.Beneficiary.onBoardingAccounts2 = null;
                if (thisWire.SendingAccount.Intermediary != null)
                    thisWire.SendingAccount.Intermediary.onBoardingAccounts = thisWire.SendingAccount.Intermediary.onBoardingAccounts1 = thisWire.SendingAccount.Intermediary.onBoardingAccounts2 = null;
                if (thisWire.SendingAccount.UltimateBeneficiary != null)
                    thisWire.SendingAccount.UltimateBeneficiary.onBoardingAccounts = thisWire.SendingAccount.UltimateBeneficiary.onBoardingAccounts1 = thisWire.SendingAccount.UltimateBeneficiary.onBoardingAccounts2 = null;

                if (thisWire.ReceivingAccount.Beneficiary != null)
                    thisWire.ReceivingAccount.Beneficiary.onBoardingAccounts = thisWire.ReceivingAccount.Beneficiary.onBoardingAccounts1 = thisWire.ReceivingAccount.Beneficiary.onBoardingAccounts2 = null;
                if (thisWire.ReceivingAccount.Intermediary != null)
                    thisWire.ReceivingAccount.Intermediary.onBoardingAccounts = thisWire.ReceivingAccount.Intermediary.onBoardingAccounts1 = thisWire.ReceivingAccount.Intermediary.onBoardingAccounts2 = null;
                if (thisWire.ReceivingAccount.UltimateBeneficiary != null)
                    thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingAccounts = thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingAccounts1 = thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingAccounts2 = null;

                //Update User Details
                thisWire.WireCreatedBy = users.First(s => s.Key == thisWire.HMWire.CreatedBy).Value.HumanizeEmail();
                thisWire.WireLastUpdatedBy = users.First(s => s.Key == thisWire.HMWire.LastUpdatedBy).Value.HumanizeEmail();
                thisWire.WireApprovedBy = thisWire.HMWire.ApprovedBy > 0 ? users.First(s => s.Key == thisWire.HMWire.ApprovedBy).Value.HumanizeEmail() : "-";

                SetUserTitles(thisWire);

                wireData.Add(thisWire);
            }

            //Custom ordering as per HMOS-56
            var customStatusOrder = new[] { 2, 5, 1, 4, 3 };
            wireData = wireData.OrderBy(s => Array.IndexOf(customStatusOrder, s.HMWire.WireStatusId)).ToList();
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
            if (thisWire.HMWire.WireStatusId == 3 && thisWire.HMWire.ApprovedBy == null)
            {
                thisWire.WireApprovedBy = thisWire.WireLastUpdatedBy;
                thisWire.HMWire.ApprovedAt = thisWire.HMWire.LastModifiedAt;
            }

            //approved wire - MT210 -This has auto approval
            if (thisWire.HMWire.WireMessageTypeId == 5 && thisWire.HMWire.WireStatusId == 3 && thisWire.WireApprovedBy == "-")
            {
                thisWire.WireApprovedBy = "System";
            }



            if (thisWire.HMWire.WireStatusId == 2 && thisWire.WireLastUpdatedBy == thisWire.WireCreatedBy)
                thisWire.WireLastUpdatedBy = "-";
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
            { ReportName.AdhocReport , new List<string>() { "MT103", "MT202", "MT202 COV", "MT210", "MT540", "MT542" } },
            { ReportName.Collateral, new List<string>() { "MT103", "MT202", "MT202 COV", "MT210" } },
            { ReportName.Invoices, new List<string>() { "MT103", "MT202", "MT202 COV" } }
        };

        public JsonResult GetWireDetails(long wireId)
        {
            var wireTicket = WireDataManager.GetWireData(wireId);
            var isDeadlineCrossed = DateTime.Now.Date > wireTicket.HMWire.ValueDate.Date;
            var isNoticePending = false;
            var validationMsg = "";
            if (wireTicket.IsNotice)
            {
                isNoticePending = WireDataManager.IsNoticeWirePendingAcknowledgement(wireTicket.HMWire);
                if (isNoticePending)
                    validationMsg = "The notice with same amount, value date and currency is already Processing with SWIFT.You cannot notice the same untill it gets a Confirmation";
            }

            var isEditEnabled = WireDataManager.WireStatus.Drafted == (WireDataManager.WireStatus)(wireTicket.HMWire.WireStatusId) && !isDeadlineCrossed;
            var isApprovedOrFailed = (int)WireDataManager.WireStatus.Cancelled == wireTicket.HMWire.WireStatusId
                                     || (int)WireDataManager.WireStatus.Approved == wireTicket.HMWire.WireStatusId
                                     || (int)WireDataManager.WireStatus.Failed == wireTicket.HMWire.WireStatusId;

            var isSwiftCancelDisabled = (int)WireDataManager.SwiftStatus.Processing == wireTicket.HMWire.SwiftStatusId
                                      || (int)WireDataManager.SwiftStatus.Completed == wireTicket.HMWire.SwiftStatusId
                                      || (int)WireDataManager.SwiftStatus.NegativeAcknowledged == wireTicket.HMWire.SwiftStatusId
                                      || (int)WireDataManager.SwiftStatus.Failed == wireTicket.HMWire.SwiftStatusId;

            var isCancelled = (int)WireDataManager.WireStatus.Cancelled == wireTicket.HMWire.WireStatusId;
            var isApprovalMet = (int)WireDataManager.WireStatus.Approved == wireTicket.HMWire.WireStatusId || wireTicket.HMWire.SwiftStatusId > 1;
            var isCancelEnabled = (!isApprovalMet && !isDeadlineCrossed || !isSwiftCancelDisabled) && !isCancelled;
            var isInitiationEnabled = !isDeadlineCrossed && (WireDataManager.WireStatus.Drafted == (WireDataManager.WireStatus)wireTicket.HMWire.WireStatusId);
            var isDraftEnabled = !isDeadlineCrossed && (WireDataManager.WireStatus.Initiated == (WireDataManager.WireStatus)wireTicket.HMWire.WireStatusId || WireDataManager.WireStatus.Failed == (WireDataManager.WireStatus)wireTicket.HMWire.WireStatusId
                                                        || (WireDataManager.WireStatus.Cancelled == (WireDataManager.WireStatus)wireTicket.HMWire.WireStatusId && WireDataManager.SwiftStatus.NotInitiated == (WireDataManager.SwiftStatus)wireTicket.HMWire.SwiftStatusId));

            var deadlineToApprove = GetDeadlineToApprove(wireTicket.SendingAccount, wireTicket.HMWire.ValueDate);
            var isLastModifiedUser = wireTicket.HMWire.LastUpdatedBy == UserDetails.Id;
            var isWirePurposeAdhoc = wireTicket.HMWire.hmsWirePurposeLkup.ReportName == ReportName.AdhocReport;
            var fundAccounts = new List<WireAccountBaseData>();
            long reportId = 0;
            if (isEditEnabled)
            {
                if (!isWirePurposeAdhoc)
                    reportId = FileSystemManager.GetReportId(wireTicket.HMWire.hmsWirePurposeLkup.ReportName);

                fundAccounts = isWirePurposeAdhoc
                    ? WireDataManager.GetApprovedFundAccounts(wireTicket.HMWire.hmFundId, wireTicket.IsBookTransfer, wireTicket.SendingAccount.Currency)
                    : WireDataManager.GetApprovedFundAccountsForModule(wireTicket.HMWire.hmFundId, wireTicket.HMWire.OnBoardSSITemplateId, reportId);
            }
            var sendingAccountsList = fundAccounts.Where(s => s.IsAuthorizedSendingAccount).Select(s => new { id = s.OnBoardAccountId, text = s.AccountNameAndNumber }).ToList();
            var receivingAccountsList = fundAccounts.Select(s => new { id = s.OnBoardAccountId, text = s.AccountNameAndNumber }).ToList();

            //Also include who is currently viewing this wire 
            var currentlyViewedBy = GetCurrentlyViewingUsers(wireId);

            var usersInvolvedInWire = wireTicket.HMWire.hmsWireWorkflowLogs.Where(s => s.WireStatusId == (int)WireDataManager.WireStatus.Initiated || s.WireStatusId == (int)WireDataManager.WireStatus.Drafted).Select(s => s.CreatedBy).Distinct().ToList();
            usersInvolvedInWire.AddRange(new List<int>() { wireTicket.HMWire.CreatedBy, wireTicket.HMWire.LastUpdatedBy });
            var isAuthorizedUserToApprove = WireDataManager.WireStatus.Initiated == (WireDataManager.WireStatus)(wireTicket.HMWire.WireStatusId) && !usersInvolvedInWire.Contains(UserDetails.Id) && !isDeadlineCrossed && User.IsWireApprover() && !isNoticePending;

            return Json(new { wireTicket, isEditEnabled, isAuthorizedUserToApprove, isCancelEnabled, isApprovedOrFailed, isInitiationEnabled, isDraftEnabled, deadlineToApprove, isLastModifiedUser, isWirePurposeAdhoc, validationMsg, sendingAccountsList, receivingAccountsList, IsWireCreated = false, currentlyViewedBy });
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

        public JsonResult IsWireCreated(DateTime valueDate, string purpose, long sendingAccountId, long receivingAccountId)
        {
            var isWireCreated = WireDataManager.IsWireCreated(valueDate, purpose, sendingAccountId, receivingAccountId);
            return Json(isWireCreated);
        }

        public void SaveWire(WireTicket wireTicket, int statusId, string comment)
        {
            try
            {
                wireTicket = WireDataManager.SaveWireData(wireTicket, (WireDataManager.WireStatus)statusId, comment, UserDetails.Id);
                var deadlineToApprove = GetDeadlineToApprove(wireTicket.SendingAccount, wireTicket.HMWire.ValueDate);
                SaveWireScheduleInfo(wireTicket, (WireDataManager.WireStatus)statusId, UserDetails.Id, deadlineToApprove);
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
                //Response.Status = ex.Message; ;
                Response.StatusCode = 500;
                Response.StatusDescription = ex.Message;
            }
        }

        private static void SaveWireScheduleInfo(WireTicket wire, WireDataManager.WireStatus workflowStatus, int userId, TimeSpan deadlineToApprove)
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
                thisWireSchedule.ScheduledDate = wire.HMWire.ValueDate.AddDays(1).Date.Add(deadlineToApprove);
            }
            else
            {
                thisWireSchedule = new hmsWireJobSchedule
                {
                    hmsWireId = wire.WireId,
                    ScheduledDate = wire.HMWire.ValueDate.AddDays(1).Date.Add(deadlineToApprove),
                    IsJobCreated = true,
                    IsJobInActive = false,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedBy = userId,
                    LastModifiedAt = DateTime.Now
                };
            }
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

        public JsonResult GetTimeToApproveTheWire(long onboardingAccountId, DateTime valueDate)
        {
            var onboardAccount = AccountManager.GetOnBoardingAccount(onboardingAccountId);
            var timeToApprove = GetDeadlineToApprove(onboardAccount, valueDate);
            return Json(timeToApprove);
        }

        private TimeSpan GetDeadlineToApprove(onBoardingAccount onboardAccount, DateTime valueDate)
        {
            try
            {
                var timeZones = FileSystemManager.GetAllTimeZones();
                var cashSweep = valueDate.Date.Add(onboardAccount.CashSweepTime ?? new TimeSpan(23, 59, 0));
                var cutOff = valueDate.AddDays(onboardAccount.WirePortalCutoff.DaystoWire).Date.Add(onboardAccount.WirePortalCutoff.CutoffTime);
                var baseTimeZone = timeZones[FileSystemManager.DefaultTimeZone];
                var cashSweepTimeZone = onboardAccount.CashSweepTimeZone ?? "";
                var customTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZones.ContainsKey(cashSweepTimeZone) ? timeZones[cashSweepTimeZone] : baseTimeZone);
                var destinationTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZones[FileSystemManager.DefaultTimeZone]);

                var cashSweepTime = customTimeZone.Id != baseTimeZone ? TimeZoneInfo.ConvertTime(new DateTime(cashSweep.Ticks, DateTimeKind.Unspecified), customTimeZone, destinationTimeZone) : cashSweep;
                var cutoffTimeZone = onboardAccount.WirePortalCutoff.CutOffTimeZone ?? "";

                customTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZones.ContainsKey(cutoffTimeZone) ? timeZones[cutoffTimeZone] : baseTimeZone);
                var cutOffTime = customTimeZone.Id != baseTimeZone ? TimeZoneInfo.ConvertTime(new DateTime(cutOff.Ticks, DateTimeKind.Unspecified), customTimeZone, destinationTimeZone) : cutOff;

                var currentTime = DateTime.Now;
                if (TimeZoneInfo.Local.Id != baseTimeZone)
                    currentTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, baseTimeZone);

                TimeSpan offSetTime;
                if (cashSweepTime < cutOffTime)
                    offSetTime = cashSweepTime - currentTime;
                else
                    offSetTime = cutOffTime - currentTime;

                return offSetTime;
            }
            catch (TimeZoneNotFoundException)
            {
                return new TimeSpan();
            }
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
                var adhocWirePurposes = context.hmsWirePurposeLkups.Where(s => s.ReportName == ReportName.AdhocReport && s.IsApproved).ToList();
                var wirePurposes = adhocWirePurposes.Select(s => new { id = s.hmsWirePurposeId, text = s.Purpose }).ToList();
                var currencies = context.onBoardingCurrencies.AsNoTracking().Select(s => new { id = s.Currency, text = s.Currency }).ToList();
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
                var fundsWithApprovedAccounts = AccountManager.GetFundsOfApprovedAccounts();
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
                onBoardedAgreements = (from oAgreement in context.vw_OnboardedAgreements
                                       where oAgreement.hmFundId == fundId && oAgreement.HMOpsStatus == "Approved"
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

        public JsonResult GetApprovedAccountsForFund(long fundId, bool isBookTransfer)
        {
            var fundAccounts = WireDataManager.GetApprovedFundAccounts(fundId, isBookTransfer);
            var sendingAccountsList = fundAccounts.Where(s => s.IsAuthorizedSendingAccount).Select(s => new { id = s.OnBoardAccountId, text = s.AccountNameAndNumber, Currency = s.Currency }).ToList();
            var receivingAccountsList = fundAccounts.Select(s => new { id = s.OnBoardAccountId, text = s.AccountNameAndNumber }).ToList();
            var currencies = sendingAccountsList.Select(s => new { id = s.Currency, text = s.Currency }).Distinct().ToList();
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
                receivingAccounts = (from oTemplate in context.onBoardingSSITemplates
                                     join oMap in context.onBoardingAccountSSITemplateMaps on oTemplate.onBoardingSSITemplateId equals oMap.onBoardingSSITemplateId
                                     where oMap.onBoardingAccountId == accountId && oMap.Status == "Approved" && oTemplate.SSITemplateType == templateType
                                     select oTemplate).ToList();
            }

            using (var context = new AdminContext())
            {
                var entityIds = receivingAccounts.Select(s => s.TemplateEntityId).Distinct().ToList();
                var counterparties = context.dmaCounterPartyOnBoardings.Where(s => entityIds.Contains(s.dmaCounterPartyOnBoardId)).ToDictionary(s => s.dmaCounterPartyOnBoardId.ToString(), v => v.CounterpartyName);
                var receivingAccountList = receivingAccounts.Select(s => new { id = s.onBoardingSSITemplateId, text = string.Format("{0}-{1}", s.AccountNumber, s.TemplateName) }).ToList();
                return Json(new { receivingAccounts, receivingAccountList, counterparties });
            }
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

            return Json(new
            {
                wireTicket,
                isEditEnabled = true,
                isInitiationEnabled = true,
                isDraftEnabled = false,
                isCancelEnabled = false,
                isAuthorizedUserToApprove = false,
                isApprovedOrFailed = false,
                isWireCreated = false,
                isLastModifiedUser = false,
                isWirePurposeAdhoc = true
            });
        }

        public JsonResult GetBoardingAccount(long onBoardingAccountId, DateTime valueDate)
        {
            var onboardAccount = AccountManager.GetOnBoardingAccount(onBoardingAccountId);
            var deadlineToApprove = GetDeadlineToApprove(onboardAccount, valueDate);
            return Json(new { onboardAccount, deadlineToApprove });
        }

        public JsonResult ValidateAccountDetails(string wireMessageType, onBoardingAccount account, onBoardingAccount receivingAccount, onBoardingSSITemplate ssiTemplate, bool isBookTransfer)
        {
            bool isMandatoryFieldsMissing = false;
            string validationMsg = string.Empty;
            switch (wireMessageType)
            {
                case "MT103":
                    if (isBookTransfer)
                        isMandatoryFieldsMissing = (string.IsNullOrWhiteSpace(account.SwiftGroup.SendersBIC) || string.IsNullOrWhiteSpace(account.Reference) || string.IsNullOrWhiteSpace(account.AccountNumber) || string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(receivingAccount.Currency) || string.IsNullOrWhiteSpace(receivingAccount.AccountNumber) || string.IsNullOrWhiteSpace(receivingAccount.Description) || string.IsNullOrWhiteSpace(receivingAccount.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(receivingAccount.IntermediaryAccountNumber) || string.IsNullOrWhiteSpace(receivingAccount.Intermediary.BICorABA) || string.IsNullOrWhiteSpace(receivingAccount.Intermediary.BankName) || string.IsNullOrWhiteSpace(receivingAccount.Intermediary.BankAddress) ||
                                                   string.IsNullOrWhiteSpace(receivingAccount.BeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(receivingAccount.Beneficiary.BICorABA) || string.IsNullOrWhiteSpace(receivingAccount.Beneficiary.BankName) || string.IsNullOrWhiteSpace(receivingAccount.Beneficiary.BankAddress));
                    else
                        isMandatoryFieldsMissing = (string.IsNullOrWhiteSpace(account.SwiftGroup.SendersBIC) || string.IsNullOrWhiteSpace(account.Reference) || string.IsNullOrWhiteSpace(account.AccountNumber) || string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(ssiTemplate.Currency) || string.IsNullOrWhiteSpace(ssiTemplate.AccountNumber) || string.IsNullOrWhiteSpace(ssiTemplate.ReasonDetail) || string.IsNullOrWhiteSpace(ssiTemplate.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(ssiTemplate.IntermediaryAccountNumber) || string.IsNullOrWhiteSpace(ssiTemplate.Intermediary.BICorABA) || string.IsNullOrWhiteSpace(ssiTemplate.Intermediary.BankName) || string.IsNullOrWhiteSpace(ssiTemplate.Intermediary.BankAddress) ||
                                                   string.IsNullOrWhiteSpace(ssiTemplate.BeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(ssiTemplate.Beneficiary.BICorABA) || string.IsNullOrWhiteSpace(ssiTemplate.Beneficiary.BankName) || string.IsNullOrWhiteSpace(ssiTemplate.Beneficiary.BankAddress));
                    validationMsg = "Sender's Account Institution, BIC & Receiver's  Account Institution, Intermediary Institution, Benificiary Institution, Currency fields and Description are required to initiate this wire";
                    break;
                case "MT202":
                case "MT202 COV":
                    if (isBookTransfer)
                        isMandatoryFieldsMissing = (string.IsNullOrWhiteSpace(account.SwiftGroup.SendersBIC) || string.IsNullOrWhiteSpace(account.AccountNumber) || string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(receivingAccount.Currency) || string.IsNullOrWhiteSpace(receivingAccount.AccountNumber) || string.IsNullOrWhiteSpace(receivingAccount.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(receivingAccount.IntermediaryAccountNumber) || string.IsNullOrWhiteSpace(receivingAccount.Intermediary.BICorABA) || string.IsNullOrWhiteSpace(receivingAccount.Intermediary.BankName) || string.IsNullOrWhiteSpace(receivingAccount.Intermediary.BankAddress) ||
                                                   string.IsNullOrWhiteSpace(receivingAccount.BeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(receivingAccount.Beneficiary.BICorABA) || string.IsNullOrWhiteSpace(receivingAccount.Beneficiary.BankName) || string.IsNullOrWhiteSpace(receivingAccount.Beneficiary.BankAddress));

                    else
                        isMandatoryFieldsMissing = (string.IsNullOrWhiteSpace(account.SwiftGroup.SendersBIC) || string.IsNullOrWhiteSpace(account.Reference) || string.IsNullOrWhiteSpace(account.AccountNumber) || string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(ssiTemplate.Currency) || string.IsNullOrWhiteSpace(ssiTemplate.AccountNumber) || string.IsNullOrWhiteSpace(ssiTemplate.UltimateBeneficiaryAccountName) ||
                                                   string.IsNullOrWhiteSpace(ssiTemplate.IntermediaryAccountNumber) || string.IsNullOrWhiteSpace(ssiTemplate.Intermediary.BICorABA) || string.IsNullOrWhiteSpace(ssiTemplate.Intermediary.BankName) || string.IsNullOrWhiteSpace(ssiTemplate.Intermediary.BankAddress) ||
                                                   string.IsNullOrWhiteSpace(ssiTemplate.BeneficiaryAccountNumber) || string.IsNullOrWhiteSpace(ssiTemplate.Beneficiary.BICorABA) || string.IsNullOrWhiteSpace(ssiTemplate.Beneficiary.BankName) || string.IsNullOrWhiteSpace(ssiTemplate.Beneficiary.BankAddress));
                    validationMsg = "Sender's Account Institution, BIC & Receiver's Account Institution, Intermediary Institution, Benificiary Institution and Currency fields are required to initiate this wire";
                    break;
                case "MT210":
                    isMandatoryFieldsMissing = (string.IsNullOrWhiteSpace(account.SwiftGroup.SendersBIC) || string.IsNullOrWhiteSpace(account.AccountNumber) || string.IsNullOrWhiteSpace(account.UltimateBeneficiaryAccountName) || string.IsNullOrWhiteSpace(account.Currency));
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