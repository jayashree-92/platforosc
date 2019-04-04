using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HedgeMark.Operations.Secure.DataModel;
using HMOSecureMiddleware;
using HMOSecureMiddleware.Models;
using System.IO;
using HMOSecureMiddleware.Queues;
using HMOSecureWeb.Utility;
using Hangfire;
using HMOSecureWeb.Jobs;

namespace HMOSecureWeb.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View(DateTime.Now.GetContextDate());
        }

        public ActionResult StaticTest()
        {
            if (Util.IsLowerEnvironment)
                return View();

            return null;
        }

        private class WireStatusCount
        {
            public int TotalPending { get; set; }
            public int TotalCancelled { get; set; }
            public int TotalCompleted { get; set; }
            public int TotalFailed { get; set; }
            public int TotalApproved { get; set; }
            public int TotalCancelledAndProcessing { get; set; }
        }

        public JsonResult GetWireStatusCount(DateTime contextDate)
        {
            WireStatusCount wireStatusCount;
            using (var context = new OperationsSecureContext())
            {
                var wireStatusCountMap = context.hmsWires.Where(s => s.ContextDate == contextDate).Select(s => new { s.WireStatusId, s.SwiftStatusId }).ToList();
                wireStatusCount = new WireStatusCount();

                foreach (var statusCount in wireStatusCountMap)
                {
                    //Initiated and Pending Approval
                    if (statusCount.WireStatusId == 2)
                        wireStatusCount.TotalPending += 1;

                    //Approved and Processing
                    if (statusCount.WireStatusId == 3 && statusCount.SwiftStatusId != 4)
                        wireStatusCount.TotalApproved += 1;

                    //Approved and Completed
                    if (statusCount.WireStatusId == 3 && statusCount.SwiftStatusId == 4)
                        wireStatusCount.TotalCompleted += 1;

                    //Cancelled and Processing
                    if (statusCount.WireStatusId == 4 && (statusCount.SwiftStatusId != 1 && statusCount.SwiftStatusId != 4))
                        wireStatusCount.TotalCancelledAndProcessing += 1;

                    //Cancelled and Completed
                    if (statusCount.WireStatusId == 4 && (statusCount.SwiftStatusId == 1 || statusCount.SwiftStatusId == 4))
                        wireStatusCount.TotalCancelled += 1;

                    //Failed
                    if (statusCount.WireStatusId == 5 || statusCount.SwiftStatusId == 5)
                        wireStatusCount.TotalFailed += 1;
                }
            }
            return Json(wireStatusCount);
        }

        public JsonResult GetWireStatusDetails(DateTime startContextDate, DateTime endContextDate, int statusId)
        {
            var wireData = new List<WireTicket>();
            List<hmsWire> wireStatusDetails;
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                wireStatusDetails = context.hmsWires.Include("hmsWireMessageType")
                                                    .Include("hmsWirePurposeLkup")
                                                    .Include("hmsWireStatusLkup")
                                                    .Where(s => s.ContextDate >= startContextDate && s.ContextDate <= endContextDate && (statusId == 0 || s.WireStatusId == statusId)).ToList();
            }

            List<dmaAgreementOnBoarding> wireAgreements;
            List<onBoardingAccount> wireAccounts;
            List<onBoardingSSITemplate> wireSSITemplates;
            Dictionary<int, string> users;
            List<long> hFundIds = wireStatusDetails.Select(s => s.hmFundId).ToList();

            using (var context = new AdminContext())
            {
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;


                var accountIds = wireStatusDetails.Select(s => s.OnBoardAccountId).Union(wireStatusDetails.Where(s => s.IsBookTransfer).Select(s => s.OnBoardSSITemplateId)).Distinct().ToList();

                var ssiTemplateIds = wireStatusDetails.Select(s => s.OnBoardSSITemplateId).Distinct().ToList();
                var userIds = wireStatusDetails.Select(s => s.LastUpdatedBy).Union(wireStatusDetails.Select(s => s.CreatedBy)).Distinct().ToList();

                var agreementIds = wireStatusDetails.Select(s => s.OnBoardAgreementId).Distinct().ToList();
                wireAgreements = context.dmaAgreementOnBoardings.Include("onboardingFund")
                                                                .Include("dmaCounterPartyOnBoarding")
                                                                .Where(s => agreementIds.Contains(s.dmaAgreementOnBoardingId)).ToList();

                wireAccounts = context.onBoardingAccounts.Where(s => accountIds.Contains(s.onBoardingAccountId)).ToList();
                wireSSITemplates = context.onBoardingSSITemplates.Where(s => ssiTemplateIds.Contains(s.onBoardingSSITemplateId)).ToList();
                users = context.hLoginRegistrations.Where(s => UserDetails.Id == s.intLoginID || userIds.Contains(s.intLoginID)).ToDictionary(s => s.intLoginID, v => v.varLoginID.HumanizeEmail());
            }

            var hFunds = AdminFundManager.GetHFundsCreatedForDMA(hFundIds, UserName);

            foreach (var wire in wireStatusDetails)
            {
                wire.hmsWireMessageType.hmsWires = null;
                wire.hmsWirePurposeLkup.hmsWires = null;
                wire.hmsWireStatusLkup.hmsWires = null;

                var thisWire = new WireTicket
                {
                    HMWire = wire,
                    Agreement = wireAgreements.FirstOrDefault(s => wire.OnBoardAgreementId == s.dmaAgreementOnBoardingId) ?? new dmaAgreementOnBoarding()
                    {
                        onboardingFund = new onboardingFund() { FundShortName = string.Format("unknown agrId {0}", wire.OnBoardAgreementId) },
                        dmaCounterPartyOnBoarding = new dmaCounterPartyOnBoarding() { CounterpartyName = string.Format("unknown agrId {0}", wire.OnBoardAgreementId) },
                    },
                    Account = wireAccounts.FirstOrDefault(s => wire.OnBoardAccountId == s.onBoardingAccountId) ?? new onBoardingAccount(),
                    ReceivingAccount = wire.IsBookTransfer ? wireAccounts.FirstOrDefault(s => wire.OnBoardSSITemplateId == s.onBoardingAccountId) ?? new onBoardingAccount() : new onBoardingAccount(),
                    SSITemplate = !wire.IsBookTransfer ? wireSSITemplates.FirstOrDefault(s => wire.OnBoardSSITemplateId == s.onBoardingSSITemplateId) ?? new onBoardingSSITemplate() : new onBoardingSSITemplate(),
                    FundName = hFunds.FirstOrDefault(s => s.HFundId == wire.hmFundId) == null ? "" : hFunds.First(s => s.HFundId == wire.hmFundId).PerferredFundName
                };

                thisWire.Agreement.dmaAgreementDocuments = null;
                thisWire.Agreement.dmaAgreementSettlementInstructions = null;
                thisWire.Agreement.dmaAgreementOnBoardingChecklists = null;
                thisWire.Agreement.onBoardingAccounts = null;

                thisWire.Agreement.onboardingFund.dmaAgreementOnBoardings = null;
                thisWire.Agreement.onboardingFund.onBoardingAccounts = null;
                thisWire.Agreement.dmaCounterPartyOnBoarding.dmaAgreementOnBoardings = null;

                //Update User Details
                thisWire.WireCreatedBy = users.First(s => s.Key == thisWire.HMWire.CreatedBy).Value.HumanizeEmail();
                thisWire.WireLastUpdatedBy = users.First(s => s.Key == thisWire.HMWire.LastUpdatedBy).Value.HumanizeEmail();

                wireData.Add(thisWire);

            }
            return Json(wireData);
        }

        public JsonResult GetWireMessageTypeDetails(string module)
        {
            using (var context = new OperationsSecureContext())
            {
                var wireMessageTypes = context.hmsWireMessageTypes.ToList();
                var thisModuleMessageTypes = MessageTypes.ContainsKey(module) ? MessageTypes[module] : new List<string>();
                wireMessageTypes = MessageTypes.ContainsKey(module) ? wireMessageTypes.Where(s => thisModuleMessageTypes.Contains(s.MessageType)).ToList() : wireMessageTypes.Where(s => s.IsOutbound).ToList();

                return Json(wireMessageTypes.Select(s => new { id = s.hmsWireMessageTypeId, text = s.MessageType }).ToList());
            }
        }

        public Dictionary<string, List<string>> MessageTypes = new Dictionary<string, List<string>>()
        {
            { "Adhoc Report" , new List<string>() { "MT103", "MT202", "MT202 COV", "MT210", "MT540", "MT542" } },
            { "Collateral", new List<string>() { "MT103", "MT202", "MT202 COV", "MT210" } }
        };

        public JsonResult GetWireDetails(long wireId)
        {
            var wireTicket = WireDataManager.GetWireData(wireId);
            var isDeadlineCrossed = DateTime.Now.Date > wireTicket.HMWire.ValueDate.Date;
            var isAuthorizedUserToApprove = (WireDataManager.WorkflowStatus.Initiated == (WireDataManager.WorkflowStatus)(wireTicket.HMWire.WireStatusId) && wireTicket.HMWire.LastUpdatedBy != UserDetails.Id) && !isDeadlineCrossed;
            var isEditEnabled = WireDataManager.WorkflowStatus.Drafted == (WireDataManager.WorkflowStatus)(wireTicket.HMWire.WireStatusId) && !isDeadlineCrossed;
            var isApprovedOrFailed = WireDataManager.WorkflowStatus.Cancelled == (WireDataManager.WorkflowStatus)wireTicket.HMWire.WireStatusId || WireDataManager.WorkflowStatus.Approved == (WireDataManager.WorkflowStatus)wireTicket.HMWire.WireStatusId || WireDataManager.SwiftStatus.Processing == (WireDataManager.SwiftStatus)wireTicket.HMWire.SwiftStatusId || WireDataManager.SwiftStatus.Completed == (WireDataManager.SwiftStatus)wireTicket.HMWire.SwiftStatusId || WireDataManager.WorkflowStatus.Failed == (WireDataManager.WorkflowStatus)wireTicket.HMWire.WireStatusId;
            var isCompletedOrFailed = WireDataManager.WorkflowStatus.Cancelled == (WireDataManager.WorkflowStatus)wireTicket.HMWire.WireStatusId || WireDataManager.SwiftStatus.Completed == (WireDataManager.SwiftStatus)wireTicket.HMWire.SwiftStatusId || WireDataManager.WorkflowStatus.Failed == (WireDataManager.WorkflowStatus)wireTicket.HMWire.WireStatusId;
            var isCancelEnabled = !isCompletedOrFailed && !isDeadlineCrossed;
            var cashSweep = wireTicket.HMWire.ValueDate.Date.Add(wireTicket.Account.CashSweepTime ?? new TimeSpan());
            var cutOff = wireTicket.HMWire.ValueDate.Date.Add(wireTicket.Account.CutoffTime ?? new TimeSpan());
            var deadlineToApprove = GetTimeToApprove(cashSweep, cutOff, wireTicket.Account.CashSweepTimeZone);
            return Json(new { wireTicket, isEditEnabled, isAuthorizedUserToApprove, isCancelEnabled, isApprovedOrFailed, deadlineToApprove });
        }

        public JsonResult IsWireCreated(DateTime valueDate, string purpose, long sendingAccountId, long receivingAccountId)
        {
            var isWireCreated = WireDataManager.IsWireCreated(valueDate, purpose, sendingAccountId, receivingAccountId);
            return Json(isWireCreated);
        }

        public void SaveWire(WireTicket wireTicket, int statusId, string comment)
        {
            wireTicket.HMWire.LastModifiedAt = DateTime.Now;
            wireTicket.HMWire.LastUpdatedBy = UserDetails.Id;
            wireTicket = WireDataManager.SaveWireData(wireTicket, (WireDataManager.WorkflowStatus)statusId, comment, UserDetails.Id);
            var cashSweep = wireTicket.HMWire.ValueDate.Date.Add(wireTicket.Account.CashSweepTime ?? new TimeSpan());
            var cutOff = wireTicket.HMWire.ValueDate.Date.Add(wireTicket.Account.CutoffTime ?? new TimeSpan());
            var deadlineToApprove = GetDeadlineToApprove(cashSweep, cutOff, wireTicket.Account.CashSweepTimeZone);
            SaveWireScheduleInfo(wireTicket, (WireDataManager.WorkflowStatus)statusId, UserDetails.Id, deadlineToApprove);
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


        private static void SaveWireScheduleInfo(WireTicket wire, WireDataManager.WorkflowStatus workflowStatus, int userId, TimeSpan deadlineToApprove)
        {

            var thisWireSchedule = WireDataManager.GetJobSchedule(wire.WireId);
            var scheduleName = OverdueWireCancellationScheduleManager.GetJobName(wire.WireId);

            if (workflowStatus != WireDataManager.WorkflowStatus.Initiated)
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

        public Dictionary<string, string> TimeZones = new Dictionary<string, string>()
        {
                { "EST", "Eastern Standard Time" },
                { "EDT", "Eastern Daylight Time" },
                { "CST", "Central Standard Time" },
                { "CDT", "Central Daylight Time" },
                { "MST", "Mountain Standard Time" },
                { "MDT", "Mountain Daylight Time" },
                { "PST", "Pacific Standard Time" },
                { "PDT", "Pacific Daylight Timee" },
                { "GMT", "Greenwich Mean Time" }
        };
        public JsonResult GetTimeToApproveTheWire(DateTime cashSweepOfAccount, DateTime cutOffTimeOfAccount, DateTime valueDate, string cashSweepTimeZone)
        {
            cashSweepOfAccount = valueDate.Date.Add(cashSweepOfAccount.TimeOfDay);
            cutOffTimeOfAccount = valueDate.Date.Add(cutOffTimeOfAccount.TimeOfDay);
            var timeToApprove = GetTimeToApprove(cashSweepOfAccount, cutOffTimeOfAccount, cashSweepTimeZone);
            return Json(timeToApprove);
        }

        private TimeSpan GetTimeToApprove(DateTime cashSweep, DateTime cutOff, string cashSweepTimeZone)
        {
            try
            {
                cashSweepTimeZone = cashSweepTimeZone ?? "";
                TimeZoneInfo customTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZones.ContainsKey(cashSweepTimeZone) ? TimeZones[cashSweepTimeZone] : TimeZones[FileSystemManager.DefaultTimeZone]);
                var actualTime = TimeZoneInfo.ConvertTime(cashSweep, customTimeZone);
                var cashSweepTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(actualTime, "Eastern Standard Time");
                var cutOffTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(cutOff, "Eastern Standard Time");
                var currentTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Eastern Standard Time");

                TimeSpan offSetTime;
                if (cashSweepTime < cutOffTime)
                    offSetTime = cashSweepTime - currentTime;
                else
                    offSetTime = cutOffTime - currentTime;

                return offSetTime;
            }
            catch (TimeZoneNotFoundException e)
            {
                return new TimeSpan();
            }
        }

        private TimeSpan GetDeadlineToApprove(DateTime cashSweep, DateTime cutOff, string cashSweepTimeZone)
        {
            try
            {
                cashSweepTimeZone = cashSweepTimeZone ?? "";
                TimeZoneInfo customTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZones.ContainsKey(cashSweepTimeZone) ? TimeZones[cashSweepTimeZone] : TimeZones[FileSystemManager.DefaultTimeZone]);
                var actualTime = TimeZoneInfo.ConvertTime(cashSweep, customTimeZone);
                var cashSweepTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(actualTime, "Eastern Standard Time");
                var cutOffTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(cutOff, "Eastern Standard Time");
                var currentTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Eastern Standard Time");

                return cashSweepTime < cutOffTime ? cashSweepTime.TimeOfDay : cutOffTime.TimeOfDay;

            }
            catch (TimeZoneNotFoundException e)
            {
                return new TimeSpan();
            }
        }

        public void SendSwiftMessage(string swiftMessage)
        {
            QueueSystemManager.SendMessage(swiftMessage);
        }

        public void ReceiveSwiftMessage()
        {
            QueueSystemManager.GetAndProcessAcknowledgement();
            QueueSystemManager.GetAndProcessMessage();
        }
    }

}