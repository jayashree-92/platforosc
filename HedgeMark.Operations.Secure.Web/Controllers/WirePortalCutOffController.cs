using ExcelUtility.Operations.ManagedAccounts;
using HedgeMark.Operations.FileParseEngine.Models;
using HedgeMark.Operations.Secure.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HedgeMark.Operations.Secure.Middleware;
using HedgeMark.Operations.Secure.Middleware.Models;

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
                Log = string.Format("Cash Instruction: <i>{0}</i><br/>Currency: <i>{1}</i>", wirePortalCutoff.CashInstruction, wirePortalCutoff.Currency),
                Field = string.Join(",<br>", fieldsChanged),
                PreviousStateValue = string.Format("TimeZone: <i>{0}</i><br/>Cutoff Time: <i>{1}</i><br/>Days to Wire:<i>{2}</i><br/>IsApproved:<i>{3}</i>", originalCutOff.CutOffTimeZone, originalCutOff.CutoffTime, originalCutOff.DaystoWire, originalCutOff.IsApproved),
                ModifiedStateValue = string.Format("TimeZone: <i>{0}</i><br/>Cutoff Time: <i>{1}</i><br/>Days to Wire:<i>{2}</i><br/>IsApproved:<i>{3}</i>", wirePortalCutoff.CutOffTimeZone, wirePortalCutoff.CutoffTime, wirePortalCutoff.DaystoWire, originalCutOff.IsApproved),
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
            var fileName = string.Format("{0}_{1:yyyyMMdd}", "WireCutOffData", DateTime.Now);
            var exportFileInfo = new FileInfo(string.Format("{0}{1}{2}", FileSystemManager.UploadTemporaryFilesPath, fileName, ".xlsx"));
            contentToExport.Add("Wire Portal CutOff Data", accountListRows);
            //Export the checklist file
            Exporter.CreateExcelFile(contentToExport, exportFileInfo.FullName, true);
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