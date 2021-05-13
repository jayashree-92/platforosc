using ExcelUtility.Operations.ManagedAccounts;
using HedgeMark.Operations.FileParseEngine.Models;
using HedgeMark.Operations.Secure.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HedgeMark.Operations.Secure.Middleware;
using HedgeMark.Operations.Secure.Middleware.Models;


namespace HMOSecureWeb.Controllers
{
    public class SwiftGroupController : WireUserBaseController
    {
        // GET: SwiftGroup
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetSwiftGroupData()
        {
            Dictionary<long, string> brokerLegalEntityData;
            var swiftGroupData = GetAllSwiftGroupData(out brokerLegalEntityData);
            var wireMessageTypes = WireDataManager.GetWireMessageTypes();
            return Json(new
            {
                brokerLegalEntityData = brokerLegalEntityData.Select(x => new { id = x.Key, text = x.Value }).OrderBy(x => x.text).ToList(),
                swiftGroupStatusData = FundAccountManager.GetSwiftGroupStatus().Select(s => new { id = s.Key, text = s.Value }).ToList(),
                wireMessageTypes = wireMessageTypes.Select(s => new { id = s.MessageType, text = s.MessageType, isOutBound = s.IsOutbound }).ToList(),
                swiftGroupData = swiftGroupData.OrderByDescending(s => s.SwiftGroup.RecCreatedAt).ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        private static List<SwiftGroupData> GetAllSwiftGroupData(out Dictionary<long, string> brokerLegalEntityData)
        {
            var swiftGroupInfo = FundAccountManager.GetAllSwiftGroup();
            brokerLegalEntityData = OnBoardingDataManager.GetAllCounterpartyFamilies().ToDictionary(s => s.dmaCounterpartyFamilyId, v => v.CounterpartyFamily);

            var loginIds = swiftGroupInfo.Select(s => s.RequestedBy ?? 0).Union(swiftGroupInfo.Select(s => s.ApprovedBy ?? 0)).Distinct().ToList();

            Dictionary<int, string> loginIdMap;
            using (var context = new AdminContext())
            {
                loginIdMap = context.hLoginRegistrations.Where(s => loginIds.Contains(s.intLoginID)).ToDictionary(s => s.intLoginID, v => v.varLoginID);
            }

            var swiftGroupData = new List<SwiftGroupData>();
            foreach (var s in swiftGroupInfo)
            {
                if (s.hmsSwiftGroupStatusLkp != null)
                    s.hmsSwiftGroupStatusLkp.hmsSwiftGroups = null;
                var swiftGrpData = new SwiftGroupData
                {
                    SwiftGroup = s,
                    Broker = s.BrokerLegalEntityId != null && brokerLegalEntityData.ContainsKey(s.BrokerLegalEntityId ?? 0)
                        ? brokerLegalEntityData[s.BrokerLegalEntityId ?? 0]
                        : string.Empty,
                    SwiftGroupStatus = s.hmsSwiftGroupStatusLkp != null ? s.hmsSwiftGroupStatusLkp.Status : "Requested",
                    IsAssociatedToAccount = s.onBoardingAccounts.Any(s1 => !s1.IsDeleted),
                    RequestedBy = loginIdMap.ContainsKey(s.RequestedBy ?? 0) ? loginIdMap[s.RequestedBy ?? 0] : "#Retrofit",
                    ApprovedBy = s.hmsSwiftGroupStatusLkp != null && s.hmsSwiftGroupStatusLkp.Status != "Live" ? "-" : loginIdMap.ContainsKey(s.ApprovedBy ?? 0) ? loginIdMap[s.ApprovedBy ?? 0] : "#Retrofit",
                };
                swiftGrpData.SwiftGroup.onBoardingAccounts = null;
                swiftGroupData.Add(swiftGrpData);
            }

            return swiftGroupData;
        }

        private hmsSwiftGroup GetSwiftGroupData(long swiftGroupId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                //context.Configuration.ProxyCreationEnabled = false;
                var swiftGroup = context.hmsSwiftGroups.Include(s => s.hmsSwiftGroupStatusLkp)
                        .FirstOrDefault(s => s.hmsSwiftGroupId == swiftGroupId) ?? new hmsSwiftGroup();
                return swiftGroup;
            }
        }


        public void AddOrUpdateSwiftGroup(hmsSwiftGroup hmsSwiftGroup)
        {
            var originalSwiftGroup = GetSwiftGroupData(hmsSwiftGroup.hmsSwiftGroupId);

            if (hmsSwiftGroup.hmsSwiftGroupId == 0)
            {
                hmsSwiftGroup.RecCreatedBy = UserName;
                hmsSwiftGroup.RecCreatedAt = DateTime.Now;
            }

            hmsSwiftGroup.SendersBIC = hmsSwiftGroup.SendersBIC.ToUpper();
            using (var context = new OperationsSecureContext())
            {
                var swiftStatusLkup = context.hmsSwiftGroupStatusLkps.ToDictionary(s => s.hmsSwiftGroupStatusLkpId, v => v.Status);
                if (swiftStatusLkup.ContainsKey(hmsSwiftGroup.SwiftGroupStatusId ?? 0))
                {
                    switch (swiftStatusLkup[hmsSwiftGroup.SwiftGroupStatusId ?? 0])
                    {
                        case "Requested":
                            hmsSwiftGroup.RequestedBy = UserId;
                            hmsSwiftGroup.RequestedAt = DateTime.Now;
                            break;
                        case "Live":
                            hmsSwiftGroup.ApprovedBy = UserId;
                            hmsSwiftGroup.ApprovedAt = DateTime.Now;

                            hmsSwiftGroup.RequestedBy = originalSwiftGroup.RequestedBy;
                            hmsSwiftGroup.RequestedAt = originalSwiftGroup.RequestedAt;
                            break;
                    }
                }

                context.hmsSwiftGroups.AddOrUpdate(hmsSwiftGroup);
                context.SaveChanges();
            }

            AuditSwiftGroupChanges(hmsSwiftGroup, originalSwiftGroup);
        }

        public void DeleteSwiftGroup(long swiftGroupId)
        {
            hmsSwiftGroup swiftGroup;

            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                swiftGroup = context.hmsSwiftGroups.Include(s => s.hmsSwiftGroupStatusLkp).FirstOrDefault(s => s.hmsSwiftGroupId == swiftGroupId) ?? new hmsSwiftGroup();
                swiftGroup.IsDeleted = true;
                swiftGroup.RecCreatedAt = DateTime.Now;
                context.SaveChanges();
            }

            AuditSwiftGroupChanges(new hmsSwiftGroup(), swiftGroup, true);
        }

        public FileResult ExportData()
        {
            Dictionary<long, string> brokerLegalEntityData;
            var swiftGroupData = GetAllSwiftGroupData(out brokerLegalEntityData);
            var contentToExport = new Dictionary<string, List<Row>>();
            var accountListRows = BuildExportRows(swiftGroupData);
            //File name and path
            var fileName = string.Format("{0}_{1:yyyyMMdd}", "SwiftGroupData", DateTime.Now);
            var exportFileInfo = new FileInfo(string.Format("{0}{1}{2}", FileSystemManager.UploadTemporaryFilesPath,
                fileName, ".xlsx"));
            contentToExport.Add("Swift Group Data", accountListRows);
            //Export the file
            Exporter.CreateExcelFile(contentToExport, exportFileInfo.FullName, true);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        private List<Row> BuildExportRows(IEnumerable<SwiftGroupData> swiftGroupData)
        {
            var swiftGroupRows = new List<Row>();

            foreach (var swift in swiftGroupData)
            {
                var row = new Row();
                row["Swift Group"] = swift.SwiftGroup.SwiftGroup;
                row["Sender's BIC"] = swift.SwiftGroup.SendersBIC;
                row["Broker"] = swift.Broker;
                row["Status"] = swift.SwiftGroupStatus;
                row["MT Messages"] = swift.SwiftGroup.AcceptedMessages;
                row["Notes"] = swift.SwiftGroup.Notes;
                row["Requested By"] = swift.RequestedBy;
                row["Requested At"] = swift.SwiftGroup.RequestedAt + "";
                row["Approved By"] = swift.ApprovedBy;
                row["Approved At"] = swift.SwiftGroup.ApprovedAt + "";
                swiftGroupRows.Add(row);
            }

            return swiftGroupRows;
        }

        private void AuditSwiftGroupChanges(hmsSwiftGroup swiftGroup, hmsSwiftGroup originalSwiftGroup, bool isDeleted = false)
        {
            var fieldsChanged = new List<string>();

            string previousState = string.Empty, modifiedState = string.Empty;

            if (originalSwiftGroup.SwiftGroup != swiftGroup.SwiftGroup)
            {
                fieldsChanged.Add("Swift Group");
                previousState += string.Format("Swift Group: <i>{0}</i><br/>", originalSwiftGroup.SwiftGroup);
                modifiedState += string.Format("Swift Group: <i>{0}</i><br/>", swiftGroup.SwiftGroup);
            }

            if (originalSwiftGroup.SendersBIC != swiftGroup.SendersBIC)
            {
                fieldsChanged.Add("Sender's BIC");
                previousState += string.Format("Sender's BIC: <i>{0}</i><br/>", originalSwiftGroup.SendersBIC);
                modifiedState += string.Format("Sender's BIC: <i>{0}</i><br/>", swiftGroup.SendersBIC);
            }

            if (originalSwiftGroup.BrokerLegalEntityId != swiftGroup.BrokerLegalEntityId)
            {
                var originalFamilyName = OnBoardingDataManager.GetCounterpartyFamilyName(originalSwiftGroup.BrokerLegalEntityId ?? 0);
                var modifiedFamilyName = OnBoardingDataManager.GetCounterpartyFamilyName(swiftGroup.BrokerLegalEntityId ?? 0);

                fieldsChanged.Add("Broker");
                previousState += string.Format("Broker: <i>{0}</i><br/>", originalFamilyName);
                modifiedState += string.Format("Broker: <i>{0}</i><br/>", modifiedFamilyName);
            }

            if (originalSwiftGroup.SwiftGroupStatusId != swiftGroup.SwiftGroupStatusId)
            {
                var statusLkup = FundAccountManager.GetSwiftGroupStatus();

                fieldsChanged.Add("Swift Group Status");
                previousState += string.Format("Swift Group Status: <i>{0}</i><br/>", statusLkup.ContainsKey(originalSwiftGroup.SwiftGroupStatusId ?? 0) ? statusLkup[originalSwiftGroup.SwiftGroupStatusId ?? 0] : string.Empty);
                modifiedState += string.Format("Swift Group Status: <i>{0}</i><br/>", statusLkup.ContainsKey(swiftGroup.SwiftGroupStatusId ?? 0) ? statusLkup[swiftGroup.SwiftGroupStatusId ?? 0] : string.Empty);
            }

            if (originalSwiftGroup.AcceptedMessages != swiftGroup.AcceptedMessages)
            {
                fieldsChanged.Add("Swift Messages");
                previousState += string.Format("Swift Messages: <i>{0}</i><br/>", originalSwiftGroup.AcceptedMessages ?? string.Empty);
                modifiedState += string.Format("Swift Messages: <i>{0}</i><br/>", swiftGroup.AcceptedMessages ?? string.Empty);
            }

            if (originalSwiftGroup.Notes != swiftGroup.Notes)
            {
                fieldsChanged.Add("Notes");
                previousState += string.Format("Notes: <i>{0}</i><br/>", originalSwiftGroup.Notes ?? string.Empty);
                modifiedState += string.Format("Notes: <i>{0}</i><br/>", swiftGroup.Notes ?? string.Empty);
            }

            if (!fieldsChanged.Any())
                return;

            //Log the changes in user audits
            var auditData = new hmsUserAuditLog
            {
                Action = isDeleted ? "Deleted" : originalSwiftGroup.hmsSwiftGroupId > 0 ? "Edited" : "Added",
                Module = "Swift Group",
                Log = isDeleted
                    ? string.Format("Swift Group: <i>{0}</i><br/>Sender's BIC: <i>{1}</i>",
                        originalSwiftGroup.SwiftGroup, originalSwiftGroup.SendersBIC)
                    : string.Format("Swift Group: <i>{0}</i><br/>Sender's BIC: <i>{1}</i>", swiftGroup.SwiftGroup,
                        swiftGroup.SendersBIC),
                Field = string.Join(",<br>", fieldsChanged),
                PreviousStateValue = previousState,
                ModifiedStateValue = modifiedState,
                CreatedAt = DateTime.Now,
                UserName = UserName
            };
            AuditManager.LogAudit(auditData);
        }
    }
}