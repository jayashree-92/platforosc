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
                var wireStatusCountMap = context.hmsWires.Where(s => s.WireStatusId == 2 || s.ValueDate == valueDate).Select(s => new { s.WireStatusId, s.SwiftStatusId }).ToList();

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
                    if (statusCount.WireStatusId == 4 && statusCount.SwiftStatusId == 5)
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

        public JsonResult GetWireStatusDetails(DateTime startContextDate, DateTime endContextDate, int statusId)
        {
            var wireData = new List<WireTicket>();
            List<hmsWire> wireStatusDetails;
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
                    .Where(s => (statusId == 0 && s.WireStatusId == 2) || s.ValueDate >= startContextDate && s.ValueDate <= endContextDate && (statusId == 0 || s.WireStatusId == statusId)
                                                    || DbFunctions.TruncateTime(s.CreatedAt) == DbFunctions.TruncateTime(endContextDate) && (statusId == 0 || s.WireStatusId == statusId)).ToList();
            }



            var authorizedFundsIds = AuthorizedSessionData.HMFundIds.Where(s => s.Level > 1).Select(s => s.Id).ToList();

            if (!AuthorizedSessionData.IsPrivilegedUser)
                wireStatusDetails = wireStatusDetails.Where(s => authorizedFundsIds.Contains(s.hmFundId)).ToList();

            // List<dmaAgreementOnBoarding> wireAgreements;
            List<onBoardingAccount> wireAccounts;
            List<onBoardingSSITemplate> wireSSITemplates;
            List<dmaCounterPartyOnBoarding> counterParties;
            Dictionary<int, string> users;
            var hFundIds = wireStatusDetails.Select(s => s.hmFundId).ToList();


            using (var context = new AdminContext())
            {
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;


                var accountIds = wireStatusDetails.Select(s => s.OnBoardAccountId).Union(wireStatusDetails.Where(s => s.WireTransferTypeId == 2).Select(s => s.OnBoardSSITemplateId)).Distinct().ToList();

                var ssiTemplateIds = wireStatusDetails.Select(s => s.OnBoardSSITemplateId).Distinct().ToList();
                var userIds = wireStatusDetails.Select(s => s.LastUpdatedBy).Union(wireStatusDetails.Select(s => s.CreatedBy)).Distinct().ToList();

                //var agreementIds = wireStatusDetails.Select(s => s.OnBoardAgreementId).Distinct().ToList();
                //wireAgreements = context.dmaAgreementOnBoardings.Include("onboardingFund")
                //                                                .Include("dmaCounterPartyOnBoarding")
                //                                                .Where(s => agreementIds.Contains(s.dmaAgreementOnBoardingId)).ToList();

                wireAccounts = context.onBoardingAccounts.Where(s => accountIds.Contains(s.onBoardingAccountId)).ToList();
                wireSSITemplates = context.onBoardingSSITemplates.Where(s => ssiTemplateIds.Contains(s.onBoardingSSITemplateId)).ToList();
                users = context.hLoginRegistrations.Where(s => UserDetails.Id == s.intLoginID || userIds.Contains(s.intLoginID)).ToDictionary(s => s.intLoginID, v => v.varLoginID.HumanizeEmail());
                var counterpartyIds = wireSSITemplates.Select(s => s.TemplateEntityId).ToList();
                counterParties = context.dmaCounterPartyOnBoardings.Where(s => counterpartyIds.Contains(s.dmaCounterPartyOnBoardId)).ToList();
            }

            var hFunds = AdminFundManager.GetHFundsCreatedForDMA(hFundIds, UserName);

            foreach (var wire in wireStatusDetails)
            {
                wire.hmsWireMessageType.hmsWires = null;
                wire.hmsWirePurposeLkup.hmsWires = null;
                wire.hmsWireStatusLkup.hmsWires = null;
                wire.hmsWireTransferTypeLKup.hmsWires = null;
                var counterpartyId = (wireSSITemplates.FirstOrDefault(s => wire.OnBoardSSITemplateId == s.onBoardingSSITemplateId) ?? new onBoardingSSITemplate()).TemplateEntityId;
                var thisWire = new WireTicket
                {
                    HMWire = wire,
                    //Agreement = wireAgreements.FirstOrDefault(s => wire.OnBoardAgreementId == s.dmaAgreementOnBoardingId) ?? new dmaAgreementOnBoarding()
                    //{
                    //    onboardingFund = new onboardingFund() { FundShortName = string.Format("unknown agrId {0}", wire.OnBoardAgreementId) },
                    //    dmaCounterPartyOnBoarding = new dmaCounterPartyOnBoarding() { CounterpartyName = string.Format("unknown agrId {0}", wire.OnBoardAgreementId) },
                    //},
                    SendingAccount = wireAccounts.FirstOrDefault(s => wire.OnBoardAccountId == s.onBoardingAccountId) ?? new onBoardingAccount(),
                    ReceivingAccount = wire.WireTransferTypeId == 2 ? wireAccounts.FirstOrDefault(s => wire.OnBoardSSITemplateId == s.onBoardingAccountId) ?? new onBoardingAccount() : new onBoardingAccount(),
                    SSITemplate = wire.WireTransferTypeId != 2 ? wireSSITemplates.FirstOrDefault(s => wire.OnBoardSSITemplateId == s.onBoardingSSITemplateId) ?? new onBoardingSSITemplate() : new onBoardingSSITemplate(),
                    FundName = hFunds.FirstOrDefault(s => s.HFundId == wire.hmFundId) == null ? "" : hFunds.First(s => s.HFundId == wire.hmFundId).PerferredFundName,
                    Counterparty = (counterParties.FirstOrDefault(s => counterpartyId == s.dmaCounterPartyOnBoardId) ?? new dmaCounterPartyOnBoarding()).CounterpartyName
                };

                //thisWire.Agreement.dmaAgreementDocuments = null;
                //thisWire.Agreement.dmaAgreementSettlementInstructions = null;
                //thisWire.Agreement.dmaAgreementOnBoardingChecklists = null;
                //thisWire.Agreement.onBoardingAccounts = null;

                //thisWire.Agreement.onboardingFund.dmaAgreementOnBoardings = null;
                //thisWire.Agreement.onboardingFund.onBoardingAccounts = null;
                //thisWire.Agreement.dmaCounterPartyOnBoarding.dmaAgreementOnBoardings = null;

                //Update User Details
                thisWire.WireCreatedBy = users.First(s => s.Key == thisWire.HMWire.CreatedBy).Value.HumanizeEmail();
                thisWire.WireLastUpdatedBy = users.First(s => s.Key == thisWire.HMWire.LastUpdatedBy).Value.HumanizeEmail();

                if (thisWire.HMWire.WireStatusId == 2 && thisWire.WireLastUpdatedBy == thisWire.WireCreatedBy)
                    thisWire.WireLastUpdatedBy = "-";

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

            var isAuthorizedUserToApprove = (WireDataManager.WireStatus.Initiated == (WireDataManager.WireStatus)(wireTicket.HMWire.WireStatusId) && wireTicket.HMWire.LastUpdatedBy != UserDetails.Id) && !isDeadlineCrossed && User.IsAuthorizedWireApprover();
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
            var cashSweep = wireTicket.HMWire.ValueDate.Date.Add(wireTicket.SendingAccount.CashSweepTime ?? new TimeSpan(23, 59, 0));
            var cutOff = wireTicket.HMWire.ValueDate.Date.Add(wireTicket.SendingAccount.CutoffTime ?? new TimeSpan(23, 59, 0));
            var deadlineToApprove = GetTimeToApprove(cashSweep, cutOff, wireTicket.SendingAccount.CashSweepTimeZone);
            var isLastModifiedUser = wireTicket.HMWire.LastUpdatedBy == UserDetails.Id;
            return Json(new { wireTicket, isEditEnabled, isAuthorizedUserToApprove, isCancelEnabled, isApprovedOrFailed, isInitiationEnabled, isDraftEnabled, deadlineToApprove, isLastModifiedUser });
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

                var cashSweep = wireTicket.HMWire.ValueDate.Date.Add(wireTicket.SendingAccount.CashSweepTime ?? new TimeSpan());
                var cutOff = wireTicket.HMWire.ValueDate.Date.Add(wireTicket.SendingAccount.CutoffTime ?? new TimeSpan());
                var deadlineToApprove = GetDeadlineToApprove(cashSweep, cutOff, wireTicket.SendingAccount.CashSweepTimeZone);
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
        public JsonResult GetTimeToApproveTheWire(DateTime? cashSweepOfAccount, DateTime? cutOffTimeOfAccount, DateTime valueDate, string cashSweepTimeZone)
        {
            var cashSweepAccount = valueDate.Date.Add(cashSweepOfAccount.HasValue ? cashSweepOfAccount.Value.TimeOfDay : new TimeSpan(23, 59, 00));
            var cutOffTimeAccount = valueDate.Date.Add(cutOffTimeOfAccount.HasValue ? cutOffTimeOfAccount.Value.TimeOfDay : new TimeSpan(23, 59, 00));
            var timeToApprove = GetTimeToApprove(cashSweepAccount, cutOffTimeAccount, cashSweepTimeZone);
            return Json(timeToApprove);
        }

        private TimeSpan GetTimeToApprove(DateTime cashSweep, DateTime cutOff, string cashSweepTimeZone)
        {
            try
            {
                cashSweepTimeZone = cashSweepTimeZone ?? "";
                TimeZoneInfo customTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZones.ContainsKey(cashSweepTimeZone) ? TimeZones[cashSweepTimeZone] : TimeZones[FileSystemManager.DefaultTimeZone]);
                var cashSweepTime = new DateTime();
                if (cashSweepTimeZone != "EST")
                {
                    var actualTime = TimeZoneInfo.ConvertTime(cashSweep, customTimeZone);
                    cashSweepTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(actualTime, "Eastern Standard Time");
                }
                else
                    cashSweepTime = cashSweep;
                var cutOffTime = cutOff;
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
                Logger.Error(e.Message, e);
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
                Logger.Error(e.Message, e);
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
    }
}