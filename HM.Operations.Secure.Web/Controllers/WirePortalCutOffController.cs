using System;
using System.Collections.Generic;
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
    public class WirePortalCutoffController : WireUserBaseController
    {
        // GET: WirePortalCutOff
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetWirePortalCutOffData()
        {
            var wireCutoffData = GetWirePortalCutoffData();
            return Json(wireCutoffData);
        }

        public JsonResult GetCutoffRelatedData()
        {
            var timeZones = FileSystemManager.GetAllTimeZones().Select(s => new { id = s.Key, text = s.Key }).OrderBy(s => s.text).ToList();
            var currencies = FundAccountManager.GetAllCurrencies().Select(s => new { id = s.Currency, text = s.Currency }).OrderBy(s => s.text).ToList();
            var cashInstructions = FundAccountManager.GetAllCashInstruction().Select(s => new { id = s.CashInstruction, text = s.CashInstruction }).Distinct().OrderBy(s => s.text).ToList();
            return Json(new { cashInstructions, currencies, timeZones }, JsonRequestBehavior.AllowGet);
        }

        private static List<WirePortalCutOffData> GetWirePortalCutoffData()
        {
            List<WirePortalCutOffData> cutOffData;
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                cutOffData = context.hmsWirePortalCutoffs.Select(s => new WirePortalCutOffData() { WirePortalCutoff = s }).ToList();
            }

            var loginIds = cutOffData.Select(s => s.WirePortalCutoff.RecCreatedBy).Union(cutOffData.Select(s => s.WirePortalCutoff.ApprovedBy ?? 0)).Distinct().ToList();

            Dictionary<int, string> loginIdMap;
            using (var context = new AdminContext())
            {
                loginIdMap = context.hLoginRegistrations.Where(s => loginIds.Contains(s.intLoginID)).ToDictionary(s => s.intLoginID, v => v.varLoginID);
            }

            cutOffData.ForEach(s =>
                {
                    s.RequestedBy = loginIdMap.ContainsKey(s.WirePortalCutoff.RecCreatedBy) ? loginIdMap[s.WirePortalCutoff.RecCreatedBy] : "#Retrofit";
                    s.ApprovedBy = !s.WirePortalCutoff.IsApproved ? "-" : loginIdMap.ContainsKey(s.WirePortalCutoff.ApprovedBy ?? 0) ? loginIdMap[s.WirePortalCutoff.ApprovedBy ?? 0] : "#Retrofit";
                });

            return cutOffData;
        }


        private hmsWirePortalCutoff GetWirePortalCutOffData(long wireCutoffId)
        {
            using (var context = new OperationsSecureContext())
            {
                return context.hmsWirePortalCutoffs.FirstOrDefault(s => s.hmsWirePortalCutoffId == wireCutoffId) ?? new hmsWirePortalCutoff();
            }
        }

        public void SaveWirePortalCutoff(hmsWirePortalCutoff wirePortalCutoff, bool shouldApprove)
        {
            var originalCutOff = GetWirePortalCutOffData(wirePortalCutoff.hmsWirePortalCutoffId);

            wirePortalCutoff.IsApproved = shouldApprove;
            if (!shouldApprove)
            {
                wirePortalCutoff.RecCreatedBy = UserId;
                wirePortalCutoff.RecCreatedAt = DateTime.Now;
            }
            else
            {
                wirePortalCutoff.RecCreatedBy = originalCutOff.RecCreatedBy;
                wirePortalCutoff.RecCreatedAt = originalCutOff.RecCreatedAt;

                wirePortalCutoff.ApprovedBy = UserId;
                wirePortalCutoff.ApprovedAt = DateTime.Now;
            }

            using (var context = new OperationsSecureContext())
            {
                context.hmsWirePortalCutoffs.AddOrUpdate(wirePortalCutoff);
                context.SaveChanges();
            }

            AuditWireCutoffChanges(wirePortalCutoff, originalCutOff);
        }

        private void AuditWireCutoffChanges(hmsWirePortalCutoff wirePortalCutoff, hmsWirePortalCutoff originalCutOff, bool isDeleted = false)
        {
            var fieldsChanged = new List<string>();

            if (originalCutOff.CutOffTimeZone != wirePortalCutoff.CutOffTimeZone)
                fieldsChanged.Add("Time Zone");
            if (originalCutOff.CutoffTime != wirePortalCutoff.CutoffTime)
                fieldsChanged.Add("Cutoff Time");
            if (originalCutOff.DaystoWire != wirePortalCutoff.DaystoWire)
                fieldsChanged.Add("Days To Wire");

            //Log the changes in user audits
            var auditData = new hmsUserAuditLog
            {
                Action = isDeleted ? "Deleted" : originalCutOff.hmsWirePortalCutoffId > 0 ? "Edited" : "Added",
                Module = "Wire Cutoff",
                Log = $"Cash Instruction: <i>{wirePortalCutoff.CashInstruction}</i><br/>Currency: <i>{wirePortalCutoff.Currency}</i>",
                Field = string.Join(",<br>", fieldsChanged),
                PreviousStateValue = $"TimeZone: <i>{originalCutOff.CutOffTimeZone}</i><br/>Cutoff Time: <i>{originalCutOff.CutoffTime}</i><br/>Days to Wire:<i>{originalCutOff.DaystoWire}</i><br/>IsApproved:<i>{originalCutOff.IsApproved}</i>",
                ModifiedStateValue = $"TimeZone: <i>{wirePortalCutoff.CutOffTimeZone}</i><br/>Cutoff Time: <i>{wirePortalCutoff.CutoffTime}</i><br/>Days to Wire:<i>{wirePortalCutoff.DaystoWire}</i><br/>IsApproved:<i>{originalCutOff.IsApproved}</i>",
                CreatedAt = DateTime.Now,
                UserName = User.Identity.Name
            };
            AuditManager.LogAudit(auditData);
        }

        public void DeleteWirePortalCutoff(long wireCutoffId)
        {
            var originalCutOff = GetWirePortalCutOffData(wireCutoffId);

            using (var context = new OperationsSecureContext())
            {
                var wirePortalCutoff = context.hmsWirePortalCutoffs.First(s => s.hmsWirePortalCutoffId == wireCutoffId);
                context.hmsWirePortalCutoffs.Remove(wirePortalCutoff);
                context.SaveChanges();
            }

            AuditWireCutoffChanges(new hmsWirePortalCutoff() { hmsWirePortalCutoffId = wireCutoffId }, originalCutOff, true);
        }


        public FileResult ExportData()
        {
            var wireCutoffData = GetWirePortalCutoffData().OrderBy(s => s.WirePortalCutoff.CashInstruction).ThenBy(s => s.WirePortalCutoff.Currency).ToList();
            var contentToExport = new Dictionary<string, List<Row>>();
            var accountListRows = BuildExportRows(wireCutoffData);
            //File name and path
            var fileName = $"WireCutOffData_{DateTime.Now:yyyyMMdd}";
            var exportFileInfo = new FileInfo($"{FileSystemManager.UploadTemporaryFilesPath}{fileName}{".xlsx"}");
            contentToExport.Add("Wire Portal CutOff Data", accountListRows);
            //Export the checklist file
            ReportDeliveryManager.CreateExportFile(contentToExport, exportFileInfo, true);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        private List<Row> BuildExportRows(List<WirePortalCutOffData> wireCutoffData)
        {
            var wireCutoffRows = new List<Row>();

            foreach (var cutoffData in wireCutoffData)
            {
                var cutoff = cutoffData.WirePortalCutoff;
                var row = new Row();
                row["Cash Instruction"] = cutoff.CashInstruction;
                row["Currency"] = cutoff.Currency;
                row["Time Zone"] = cutoff.CutOffTimeZone;
                var dateTime = DateTime.Today.AddHours(cutoff.CutoffTime.Hours).AddMinutes(cutoff.CutoffTime.Minutes).AddSeconds(cutoff.CutoffTime.Seconds);
                var stringTime = dateTime.ToString("hh:mm tt");
                row["Cutoff Time"] = stringTime;
                row["Days to Wire"] = cutoff.DaystoWire.ToString();
                row["Created By"] = cutoffData.RequestedBy;
                row["Created At"] = cutoff.RecCreatedAt + "";
                row["Is Approved"] = cutoff.IsApproved ? "Approved" : "Pending Approval";
                row["Approved By"] = cutoffData.ApprovedBy;
                row["Approved At"] = cutoff.ApprovedAt + "";
                wireCutoffRows.Add(row);
            }

            return wireCutoffRows;
        }

    }
}