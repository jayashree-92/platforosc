using ExcelUtility.Operations.ManagedAccounts;
using HedgeMark.Operations.FileParseEngine.Models;
using HedgeMark.Operations.Secure.DataModel;
using HMOSecureMiddleware;
using HMOSecureMiddleware.Util;
using HMOSecureMiddleware.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace HMOSecureWeb.Controllers
{
    public class SwiftGroupController : BaseController
    {
        // GET: SwiftGroup
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetSwiftGroupData()
        {
            var swiftGroupInfo = AccountManager.GetAllSwiftGroup();
            var brokerLegalEntityData = OnBoardingDataManager.GetAllCounterpartyFamilies().Select(x => new { id = x.dmaCounterpartyFamilyId, text = x.CounterpartyFamily }).OrderBy(x => x.text).ToList();
            var swiftGroupStatusData = AccountManager.GetSwiftGroupStatus().Select(s => new { id = s.hmsSwiftGroupStatusLkpId, text = s.Status }).ToList();
            var counterpartyData = brokerLegalEntityData.GroupBy(s => s.id).ToDictionary(p => p.Key, v => v.FirstOrDefault().text);
            var swiftStatusData = swiftGroupStatusData.GroupBy(s => s.id).ToDictionary(p => p.Key, v => v.FirstOrDefault().text);
            var wireMessageTypes = WireDataManager.GetWireMessageTypes().Select(s => new { id = s.MessageType, text = s.MessageType, isOutBound = s.IsOutbound }).ToList();
            var swiftGroupData = swiftGroupInfo.Select(s => new SwiftGroupData
            {
                hmsSwiftGroupId = s.hmsSwiftGroupId,
                SwiftGroup = s.SwiftGroup,
                SendersBIC = s.SendersBIC,
                BrokerLegalEntityId = s.BrokerLegalEntityId ?? 0,
                Broker = counterpartyData.ContainsKey(s.BrokerLegalEntityId ?? 0) ? counterpartyData[s.BrokerLegalEntityId ?? 0] : string.Empty,
                SwiftGroupStatusId = s.SwiftGroupStatusId ?? 0,
                SwiftGroupStatus = swiftStatusData.ContainsKey(s.SwiftGroupStatusId ?? 0) ? swiftStatusData[s.SwiftGroupStatusId ?? 0] : string.Empty,
                AcceptedMessages = s.AcceptedMessages,
                IsDeleted = s.IsDeleted,
                Notes = s.Notes,
                RecCreatedAt = s.RecCreatedAt.Value,
                RecCreatedBy = s.RecCreatedBy.HumanizeEmail()
            }).OrderByDescending(s => s.RecCreatedAt).ToList();
            var swiftGroupRelatedData = new SwiftGroupInformation() { SwiftGroupData = swiftGroupData, Brokers = counterpartyData, SwiftGroupStatus = swiftStatusData };
            var preferencesKey = string.Format("{0}{1}", UserId, OpsSecureSessionVars.SwiftGroupData);
            SetSessionValue(preferencesKey, swiftGroupRelatedData);
            return Json(new
            {
                brokerLegalEntityData,
                swiftGroupStatusData,
                wireMessageTypes,
                swiftGroupData
            }, JsonRequestBehavior.AllowGet);
        }

        public void AddOrUpdateSwiftGroup(hmsSwiftGroup hmsSwiftGroup)
        {
            SwiftGroupData originalSwiftGroup;
            var swiftGroupData = GetSwiftGroupData(hmsSwiftGroup.hmsSwiftGroupId);
            var preferencesKey = string.Format("{0}{1}", UserId, OpsSecureSessionVars.SwiftGroupData);
            var swiftGroupInfo = (SwiftGroupInformation)GetSessionValue(preferencesKey);
            if (hmsSwiftGroup.hmsSwiftGroupId > 0)
                originalSwiftGroup = GenerateSwiftGroupData(new List<hmsSwiftGroup>() { swiftGroupData }, swiftGroupInfo).FirstOrDefault();
            else
                originalSwiftGroup = new SwiftGroupData();
            hmsSwiftGroup.RecCreatedBy = UserName;
            AccountManager.AddOrUpdateSwiftGroup(hmsSwiftGroup);
            var swiftGroup = GenerateSwiftGroupData(new List<hmsSwiftGroup>() { hmsSwiftGroup }, swiftGroupInfo).FirstOrDefault();
            AuditSwiftGroupChanges(swiftGroup, originalSwiftGroup);
        }

        public void DeleteSwiftGroup(long swiftGroupId)
        {
            var swiftGroup = DeleteSwiftGroupData(swiftGroupId);
            var preferencesKey = string.Format("{0}{1}", UserId, OpsSecureSessionVars.SwiftGroupData);
            var swiftGroupInfo = (SwiftGroupInformation)GetSessionValue(preferencesKey);
            var originalSwiftGroup = GenerateSwiftGroupData(new List<hmsSwiftGroup>() { swiftGroup }, swiftGroupInfo).FirstOrDefault();
            AuditSwiftGroupChanges(new SwiftGroupData() { hmsSwiftGroupId = swiftGroupId }, originalSwiftGroup, true);
        }

        public FileResult ExportData()
        {
            var preferencesKey = string.Format("{0}{1}", UserId, OpsSecureSessionVars.SwiftGroupData);
            var swiftGroupInfo = (SwiftGroupInformation)GetSessionValue(preferencesKey);
            var contentToExport = new Dictionary<string, List<Row>>();
            var accountListRows = BuildExportRows(swiftGroupInfo.SwiftGroupData);
            //File name and path
            var fileName = string.Format("{0}_{1:yyyyMMdd}", "SwiftGroupData", DateTime.Now);
            var exportFileInfo = new FileInfo(string.Format("{0}{1}{2}", FileSystemManager.UploadTemporaryFilesPath, fileName, ".xlsx"));
            contentToExport.Add("Swift Group Data", accountListRows);
            //Export the file
            Exporter.CreateExcelFile(contentToExport, exportFileInfo.FullName, true);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        private hmsSwiftGroup DeleteSwiftGroupData(long swiftGroupId)
        {
            using (var context = new OperationsSecureContext())
            {
                var swiftGroup = context.hmsSwiftGroups.FirstOrDefault(s => s.hmsSwiftGroupId == swiftGroupId) ?? new hmsSwiftGroup();
                context.hmsSwiftGroups.Remove(swiftGroup);
                context.SaveChanges();
                return swiftGroup;
            }
        }

        private hmsSwiftGroup GetSwiftGroupData(long swiftGroupId)
        {
            using (var context = new OperationsSecureContext())
            {
                var swiftGroup = context.hmsSwiftGroups.FirstOrDefault(s => s.hmsSwiftGroupId == swiftGroupId) ?? new hmsSwiftGroup();
                return swiftGroup;
            }
        }

        private List<Row> BuildExportRows(List<SwiftGroupData> swiftGroupData)
        {
            var swiftGroupRows = new List<Row>();

            foreach (var swift in swiftGroupData)
            {
                var row = new Row();
                row["Swift Group"] = swift.SwiftGroup;
                row["Sender's BIC"] = swift.SendersBIC;
                row["Broker"] = swift.Broker;
                row["Status"] = swift.SwiftGroupStatus;
                row["MT Messages"] = swift.AcceptedMessages;
                row["Notes"] = swift.Notes;
                row["Created By"] = swift.RecCreatedBy;
                row["Created At"] = swift.RecCreatedAt + "";
                swiftGroupRows.Add(row);
            }

            return swiftGroupRows;
        }

        private List<SwiftGroupData> GenerateSwiftGroupData(List<hmsSwiftGroup> hmsSwiftGroups, SwiftGroupInformation swiftGroupInfo)
        {
            return hmsSwiftGroups.Select(s => new SwiftGroupData()
            {
                hmsSwiftGroupId = s.hmsSwiftGroupId,
                SwiftGroup = s.SwiftGroup,
                SendersBIC = s.SendersBIC,
                BrokerLegalEntityId = s.BrokerLegalEntityId ?? 0,
                Broker = swiftGroupInfo.Brokers.ContainsKey(s.BrokerLegalEntityId ?? 0) ? swiftGroupInfo.Brokers[s.BrokerLegalEntityId ?? 0] : string.Empty,
                SwiftGroupStatusId = s.SwiftGroupStatusId ?? 0,
                SwiftGroupStatus = swiftGroupInfo.SwiftGroupStatus.ContainsKey(s.SwiftGroupStatusId ?? 0) ? swiftGroupInfo.SwiftGroupStatus[s.SwiftGroupStatusId ?? 0] : string.Empty,
                AcceptedMessages = s.AcceptedMessages,
                IsDeleted = s.IsDeleted,
                Notes = s.Notes,
                RecCreatedAt = s.RecCreatedAt.Value,
                RecCreatedBy = (s.RecCreatedBy ?? string.Empty).HumanizeEmail()
            }).OrderByDescending(s => s.RecCreatedAt).ToList();
        }
        private void AuditSwiftGroupChanges(SwiftGroupData swiftGroup, SwiftGroupData originalSwiftGroup, bool isDeleted = false)
        {
            var fieldsChanged = new List<string>();

            if (originalSwiftGroup.SwiftGroup != swiftGroup.SwiftGroup)
                fieldsChanged.Add("Swift Group");
            if (originalSwiftGroup.SendersBIC != swiftGroup.SendersBIC)
                fieldsChanged.Add("Cutoff Time");
            if (originalSwiftGroup.Broker != swiftGroup.Broker)
                fieldsChanged.Add("Broker");
            if (originalSwiftGroup.SwiftGroupStatus != swiftGroup.SwiftGroupStatus)
                fieldsChanged.Add("Swift Group Status");
            if (originalSwiftGroup.AcceptedMessages != swiftGroup.AcceptedMessages)
                fieldsChanged.Add("Swift Messages");
            if (originalSwiftGroup.Notes != swiftGroup.Notes)
                fieldsChanged.Add("Notes");

            //Log the changes in user audits
            var auditData = new hmsUserAuditLog
            {
                Action = isDeleted ? "Deleted" : originalSwiftGroup.hmsSwiftGroupId > 0 ? "Edited" : "Added",
                Module = "Swift Group",
                Log = string.Format("Swift Group: <i>{0}</i><br/>Broker: <i>{1}</i><br/>Sender's BIC: <i>{2}</i>", swiftGroup.SwiftGroup,
                    swiftGroup.Broker, swiftGroup.SendersBIC),
                Field = string.Join(",<br>", fieldsChanged),
                PreviousStateValue =
                    string.Format("Swift Group Status: <i>{0}</i><br/>Swift Messages: <i>{1}</i><br/>Notes:<i>{2}</i>",
                        originalSwiftGroup.SwiftGroupStatus, originalSwiftGroup.AcceptedMessages, originalSwiftGroup.Notes),
                ModifiedStateValue =
                    string.Format("Swift Group Status: <i>{0}</i><br/>Swift Messages: <i>{1}</i><br/>Notes:<i>{2}</i>",
                        swiftGroup.SwiftGroupStatus, swiftGroup.AcceptedMessages, swiftGroup.Notes),
                CreatedAt = DateTime.Now,
                UserName = User.Identity.Name
            };
            AuditManager.LogAudit(auditData);
        }
    }
}