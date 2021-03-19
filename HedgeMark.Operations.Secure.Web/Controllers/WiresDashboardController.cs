using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HedgeMark.Operations.FileParseEngine.Models;
using HedgeMark.Operations.Secure.Middleware;
using HedgeMark.Operations.Secure.Middleware.Models;
using HedgeMark.Operations.Secure.Middleware.Util;

namespace HMOSecureWeb.Controllers
{
    public class WiresDashboardController : BaseController
    {
        // GET: WiresDashboard
        public ActionResult Index()
        {
            return View();
        }


        //public static List<DashboardReport.Preferences> GetWirePreferences(List<HFundBasic> authorizedFunds)
        //{
        //    var agreementData = TreasuryReportManager.GetQualifiedTreasuryAgreements();

        //    var funds = (from agg in agreementData
        //                 where agg.FundMapId != null && agg.FundMapId > 0
        //                 join fnd in authorizedFunds on agg.FundMapId ?? 0 equals fnd.HmFundId
        //                 select new Select2Type() { id = fnd.HmFundId.ToString(), text = fnd.PreferredFundName })
        //        .Distinct(new Select2HeaderComparer()).OrderBy(s => s.text).ToList();

        //    var dmaCounterParties = agreementData.Where(s => s.dmaCounterPartyOnBoardId != null && s.dmaCounterPartyOnBoardId > 0)
        //        .Select(s => new Select2Type { id = s.dmaCounterPartyOnBoardId.ToString(), text = s.CounterpartyName }).Distinct(new Select2HeaderComparer()).OrderBy(s => s.text).ToList();

        //    var stats = ReportDumpToDbManager.TreasuryDBFieldsMap.Where(s => !s.Key.EndsWith("Id", StringComparison.InvariantCultureIgnoreCase)).Select(s => new Select2Type() { id = s.Value, text = s.Key }).ToList();

        //    var currencies = TreasuryReportManager.GetQualifiedFundAccountCurrencies().Select(s => new Select2Type() { id = s, text = s }).ToList();

        //    return new List<DashboardReport.Preferences>()
        //    {
        //        new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.Funds.ToString(),Options = funds},
        //        new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.Counterparties.ToString(),Options = dmaCounterParties},
        //        new DashboardReport.Preferences()
        //        {
        //            Preference = DashboardReport.PreferenceCode.AgreementTypes.ToString(),
        //            Options = TreasuryReportManager.QualifiedAgreementForTreasury.Select(s => new Select2Type() { id = s, text = s}).OrderBy(s=>s.text).ToList()
        //        },
        //        new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.Currencies.ToString(),Options = currencies},
        //        new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.Stats.ToString(),Options=stats}
        //    };
        //}


        ////public JsonResult GetCounterpartyDetails(List<long> hmfundIds)
        ////{
        ////    var agreements = TreasuryReportManager.GetQualifiedTreasuryAgreements();

        ////    var filteredAgreements = agreements.Where(s => hmfundIds.Contains(s.FundMapId ?? 0)).ToList();        
        ////    var dmaCounterParties = filteredAgreements.Select(s => s.dmaCounterPartyOnBoardId).Distinct().ToList();
        ////    var dmaAgreementTypes = filteredAgreements.Select(s => s.AgreementType).Distinct().ToList();

        ////    return Json(new { dmaCounterParties, dmaAgreementTypes }, JsonRequestBehavior.AllowGet);
        ////}


        //public JsonResult GetWireLogData(DateTime startDate, DateTime endDate, Dictionary<DashboardReport.PreferenceCode, string> searchPreference)
        //{
        //    var rowsToBuild = TreasuryReportManager.GetTreasuryReport(startDate, endDate, searchPreference, AuthorizedDMAFundData, AuthorizedReportMapData.First(s => s.ReportName == ReportName.Treasury).ReportMapId);
        //    SetSessionValue(OpsSecureSessionVars.WiresDashboardData.ToString(), rowsToBuild);
        //    var rows = JsonHelper.GetJson(rowsToBuild);
        //    return Json(new { rows }, JsonRequestBehavior.AllowGet);
        //}

        //public FileResult ExportReport(DateTime startDate, DateTime endDate, string templateName, string format = ".xlsx")
        //{
        //    var rowData = (List<Row>)GetSessionValue(OpsSecureSessionVars.WiresDashboardData.ToString());
        //    var fileName = string.Format("{0}_{1}_{2:yyyyMMdd}_{3:yyyyMMdd}", "Treasury_Data", templateName, startDate, endDate);
        //    var exportFileInfo = new FileInfo(string.Format("{0}{1}{2}", FileSystemManager.UploadTemporaryFilesPath, fileName, format));

        //    var contentToExport = new Dictionary<string, List<Row>>() { { "Treasury Data", rowData } };
        //    ReportDeliveryManager.CreateExportFile(contentToExport, exportFileInfo);

        //    return DownloadAndDeleteFile(exportFileInfo);
        //}
    }
}