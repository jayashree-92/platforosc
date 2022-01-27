using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using HedgeMark.Operations.FileParseEngine.Models;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware;
using HM.Operations.Secure.Middleware.Models;

namespace HM.Operations.Secure.Web.Controllers
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
            var swiftGroupData = GetAllSwiftGroupData(out var brokerLegalEntityData);
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
                        case "Testing":
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
            var swiftGroupData = GetAllSwiftGroupData(out var brokerLegalEntityData);
            var contentToExport = new Dictionary<string, List<Row>>();
            var accountListRows = BuildExportRows(swiftGroupData);
            //File name and path
            var fileName = $"SwiftGroupData_{DateTime.Now:yyyyMMdd}";
            var exportFileInfo = new FileInfo($"{FileSystemManager.UploadTemporaryFilesPath}{fileName}.xlsx");
            contentToExport.Add("Swift Group Data", accountListRows);
            //Export the file
            ReportDeliveryManager.CreateExportFile(contentToExport, exportFileInfo, true);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        private List<Row> BuildExportRows(IEnumerable<SwiftGroupData> swiftGroupData)
        {
            var swiftGroupRows = new List<Row>();

            foreach (var swift in swiftGroupData)
            {
                var row = new Row
                {
                    ["Swift Group"] = swift.SwiftGroup.SwiftGroup,
                    ["Sender's BIC"] = swift.SwiftGroup.SendersBIC,
                    ["Broker"] = swift.Broker,
                    ["Status"] = swift.SwiftGroupStatus,
                    ["MT Messages"] = swift.SwiftGroup.AcceptedMessages,
                    ["Notes"] = swift.SwiftGroup.Notes,
                    ["Requested By"] = swift.RequestedBy,
                    ["Requested At"] = swift.SwiftGroup.RequestedAt + "",
                    ["Approved By"] = swift.ApprovedBy,
                    ["Approved At"] = swift.SwiftGroup.ApprovedAt + ""
                };
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
                previousState += $"Swift Group: <i>{originalSwiftGroup.SwiftGroup}</i><br/>";
                modifiedState += $"Swift Group: <i>{swiftGroup.SwiftGroup}</i><br/>";
            }

            if (originalSwiftGroup.SendersBIC != swiftGroup.SendersBIC)
            {
                fieldsChanged.Add("Sender's BIC");
                previousState += $"Sender's BIC: <i>{originalSwiftGroup.SendersBIC}</i><br/>";
                modifiedState += $"Sender's BIC: <i>{swiftGroup.SendersBIC}</i><br/>";
            }

            if (originalSwiftGroup.BrokerLegalEntityId != swiftGroup.BrokerLegalEntityId)
            {
                var originalFamilyName = OnBoardingDataManager.GetCounterpartyFamilyName(originalSwiftGroup.BrokerLegalEntityId ?? 0);
                var modifiedFamilyName = OnBoardingDataManager.GetCounterpartyFamilyName(swiftGroup.BrokerLegalEntityId ?? 0);

                fieldsChanged.Add("Broker");
                previousState += $"Broker: <i>{originalFamilyName}</i><br/>";
                modifiedState += $"Broker: <i>{modifiedFamilyName}</i><br/>";
            }

            if (originalSwiftGroup.SwiftGroupStatusId != swiftGroup.SwiftGroupStatusId)
            {
                var statusLkup = FundAccountManager.GetSwiftGroupStatus();

                fieldsChanged.Add("Swift Group Status");
                previousState += $"Swift Group Status: <i>{(statusLkup.ContainsKey(originalSwiftGroup.SwiftGroupStatusId ?? 0) ? statusLkup[originalSwiftGroup.SwiftGroupStatusId ?? 0] : string.Empty)}</i><br/>";
                modifiedState += $"Swift Group Status: <i>{(statusLkup.ContainsKey(swiftGroup.SwiftGroupStatusId ?? 0) ? statusLkup[swiftGroup.SwiftGroupStatusId ?? 0] : string.Empty)}</i><br/>";
            }

            if (originalSwiftGroup.AcceptedMessages != swiftGroup.AcceptedMessages)
            {
                fieldsChanged.Add("Swift Messages");
                previousState += $"Swift Messages: <i>{originalSwiftGroup.AcceptedMessages ?? string.Empty}</i><br/>";
                modifiedState += $"Swift Messages: <i>{swiftGroup.AcceptedMessages ?? string.Empty}</i><br/>";
            }

            if (originalSwiftGroup.Notes != swiftGroup.Notes)
            {
                fieldsChanged.Add("Notes");
                previousState += $"Notes: <i>{originalSwiftGroup.Notes ?? string.Empty}</i><br/>";
                modifiedState += $"Notes: <i>{swiftGroup.Notes ?? string.Empty}</i><br/>";
            }

            if (!fieldsChanged.Any())
                return;

            //Log the changes in user audits
            var auditData = new hmsUserAuditLog
            {
                Action = isDeleted ? "Deleted" : originalSwiftGroup.hmsSwiftGroupId > 0 ? "Edited" : "Added",
                Module = "Swift Group",
                Log = isDeleted
                    ? $"Swift Group: <i>{originalSwiftGroup.SwiftGroup}</i><br/>Sender's BIC: <i>{originalSwiftGroup.SendersBIC}</i>"
                    : $"Swift Group: <i>{swiftGroup.SwiftGroup}</i><br/>Sender's BIC: <i>{swiftGroup.SendersBIC}</i>",
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