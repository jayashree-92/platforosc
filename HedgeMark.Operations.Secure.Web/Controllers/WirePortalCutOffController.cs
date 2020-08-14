using ExcelUtility.Operations.ManagedAccounts;
using HedgeMark.Operations.FileParseEngine.Models;
using HedgeMark.Operations.Secure.DataModel;
using HMOSecureMiddleware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HMOSecureWeb.Controllers
{
    public class WirePortalCutoffController : BaseController
    {
        // GET: WirePortalCutOff
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetWirePortalCutOffData()
        {
            var wireCutoffData = WireDataManager.GetWirePortalCutoffData();
            return Json(wireCutoffData);
        }

        public JsonResult GetCutoffRelatedData()
        {
            var timeZones = FileSystemManager.GetAllTimeZones().Select(s => new { id = s.Key, text = s.Key }).OrderBy(s => s.text).ToList();
            var currencies = FundAccountManager.GetAllCurrencies().Select(s => new { id = s.Currency, text = s.Currency }).OrderBy(s => s.text).ToList();
            var cashInstructions = FundAccountManager.GetAllCashInstruction().Select(s => new { id = s.CashInstruction, text = s.CashInstruction }).Distinct().OrderBy(s => s.text).ToList();
            return Json(new { cashInstructions, currencies, timeZones }, JsonRequestBehavior.AllowGet);
        }

        private hmsWirePortalCutoff GetWirePortalCutOffData(long wireCutoffId)
        {
            using (var context = new OperationsSecureContext())
            {
                return context.hmsWirePortalCutoffs.FirstOrDefault(s => s.hmsWirePortalCutoffId == wireCutoffId) ?? new hmsWirePortalCutoff();
            }
        }

        public void SaveWirePortalCutoff(hmsWirePortalCutoff wirePortalCutoff)
        {
            var originalCutOff = GetWirePortalCutOffData(wirePortalCutoff.hmsWirePortalCutoffId);

            wirePortalCutoff.RecCreatedBy = UserDetails.Id;
            wirePortalCutoff.RecCreatedAt = DateTime.Now;
            WireDataManager.SaveWirePortalCutoff(wirePortalCutoff);

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
                Log = string.Format("Cash Instruction: <i>{0}</i><br/>Currency: <i>{1}</i>", wirePortalCutoff.CashInstruction,
                    wirePortalCutoff.Currency),
                Field = string.Join(",<br>", fieldsChanged),
                PreviousStateValue =
                    string.Format("TimeZone: <i>{0}</i><br/>Cutoff Time: <i>{1}</i><br/>Days to Wire:<i>{2}</i>",
                        originalCutOff.CutOffTimeZone, originalCutOff.CutoffTime, originalCutOff.DaystoWire),
                ModifiedStateValue =
                    string.Format("TimeZone: <i>{0}</i><br/>Cutoff Time: <i>{1}</i><br/>Days to Wire:<i>{2}</i>",
                        wirePortalCutoff.CutOffTimeZone, wirePortalCutoff.CutoffTime, wirePortalCutoff.DaystoWire),
                CreatedAt = DateTime.Now,
                UserName = User.Identity.Name
            };
            AuditManager.LogAudit(auditData);
        }

        public void DeleteWirePortalCutoff(long wireCutoffId)
        {
            var originalCutOff = GetWirePortalCutOffData(wireCutoffId);
            WireDataManager.DeleteWirePortalCutoff(wireCutoffId);

            AuditWireCutoffChanges(new hmsWirePortalCutoff() { hmsWirePortalCutoffId = wireCutoffId }, originalCutOff, true);
        }


        public FileResult ExportData()
        {
            var wireCutoffData = WireDataManager.GetWirePortalCutoffData().OrderBy(s => s.CashInstruction).ThenBy(s => s.Currency).ToList();
            var contentToExport = new Dictionary<string, List<Row>>();
            var accountListRows = BuildExportRows(wireCutoffData);
            //File name and path
            var fileName = string.Format("{0}_{1:yyyyMMdd}", "WireCutOffData", DateTime.Now);
            var exportFileInfo = new FileInfo(string.Format("{0}{1}{2}", FileSystemManager.UploadTemporaryFilesPath, fileName, ".xlsx"));
            contentToExport.Add("Wire Portal CutOff Data", accountListRows);
            //Export the checklist file
            Exporter.CreateExcelFile(contentToExport, exportFileInfo.FullName, true);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        private List<Row> BuildExportRows(List<hmsWirePortalCutoff> wireCutoffData)
        {
            var wireCutoffRows = new List<Row>();

            foreach (var cutoff in wireCutoffData)
            {
                var row = new Row();
                row["Cash Instruction"] = cutoff.CashInstruction.ToString();
                row["Currency"] = cutoff.Currency;
                row["Time Zone"] = cutoff.CutOffTimeZone;
                var dateTime = DateTime.Today.AddHours(cutoff.CutoffTime.Hours).AddMinutes(cutoff.CutoffTime.Minutes).AddSeconds(cutoff.CutoffTime.Seconds);
                var stringTime = dateTime.ToString("hh:mm tt");
                row["Cutoff Time"] = stringTime;
                row["Days to Wire"] = cutoff.DaystoWire.ToString();
                row["Created At"] = cutoff.RecCreatedAt + "";
                wireCutoffRows.Add(row);
            }

            return wireCutoffRows;
        }

    }
}