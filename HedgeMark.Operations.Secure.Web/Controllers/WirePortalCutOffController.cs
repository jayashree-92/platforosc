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
            var timeZones = FileSystemManager.GetAllTimeZones().Select(s => new { id = s.Key, text = s.Key }).ToList();
            var currencies = AccountManager.GetAllCurrencies().Select(s => new { id = s.Currency, text = s.Currency }).ToList();
            var cashInstructions = AccountManager.GetAllCashInstruction().Select(s => new { id = s.CashInstruction, text = s.CashInstruction }).Distinct().ToList();
            return Json(new { cashInstructions, currencies, timeZones }, JsonRequestBehavior.AllowGet);
        }

        public void SaveWirePortalCutoff(onBoardingWirePortalCutoff wirePortalCutoff)
        {
            wirePortalCutoff.RecCreatedBy = UserDetails.Id;
            wirePortalCutoff.RecCreatedAt = DateTime.Now;
            WireDataManager.SaveWirePortalCutoff(wirePortalCutoff);
        }

        public void DeleteWirePortalCutoff(long wireCutoffId)
        {
            WireDataManager.DeleteWirePortalCutoff(wireCutoffId);
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

        private List<Row> BuildExportRows(List<onBoardingWirePortalCutoff> wireCutoffData)
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