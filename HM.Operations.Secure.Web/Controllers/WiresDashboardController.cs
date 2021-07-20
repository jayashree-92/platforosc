using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using HedgeMark.Operations.FileParseEngine.Models;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware;
using HM.Operations.Secure.Middleware.Models;
using HM.Operations.Secure.Web.Utility;

namespace HM.Operations.Secure.Web.Controllers
{
    public class WiresDashboardController : WireUserBaseController
    {
        // GET: WiresDashboard
        public ActionResult Index()
        {
            return View();
        }

        private static readonly List<string> EligibleOutboundMessageTypes = new List<string>() { "MT103", "MT202", "MT202 COV", "MT210" };

        public static List<DashboardReport.Preferences> GetWirePreferences(List<HFundBasic> authorizedFunds, bool isPrivilegedUser)
        {
            //Clients, Funds, AgreementTypes, MessageTypes, Status

            var authFundIds = authorizedFunds.Select(s => s.HmFundId).ToList();
            var fundDetails = FundAccountManager.GetOnBoardingAccountDetails(authFundIds, isPrivilegedUser);

            var clientMaps = new Dictionary<long, string>();
            foreach (var fund in fundDetails.Where(fund => !string.IsNullOrWhiteSpace(fund.ClientName) && fund.dmaClientOnBoardId != null && fund.dmaClientOnBoardId != 0 && !clientMaps.ContainsKey(fund.dmaClientOnBoardId ?? 0)))
            {
                clientMaps.Add(fund.dmaClientOnBoardId ?? 0, fund.ClientName);
            }

            var adminMaps = new Dictionary<long, string>();
            foreach (var fund in fundDetails.Where(fund => !string.IsNullOrWhiteSpace(fund.AdminChoice) && fund.dmaOnBoardingAdminChoiceId != null && fund.dmaOnBoardingAdminChoiceId != 0 && !adminMaps.ContainsKey(fund.dmaOnBoardingAdminChoiceId ?? 0)))
            {
                adminMaps.Add(fund.dmaOnBoardingAdminChoiceId ?? 0, fund.AdminChoice);
            }

            var clients = clientMaps.Select(s => new Select2Type() { id = s.Key.ToString(), text = s.Value }).OrderBy(s => s.text).ToList();
            var admins = adminMaps.Select(s => new Select2Type() { id = s.Key.ToString(), text = s.Value }).OrderBy(s => s.text).ToList();
            var funds = (from fnd in fundDetails where fnd.hmFundId > 0 select new Select2Type() { id = fnd.hmFundId.ToString(), text = fnd.ShortFundName }).Distinct(new Select2HeaderComparer()).OrderBy(s => s.text).ToList();
            var accountTypes = WireDataManager.AgreementTypesEligibleForSendingWires.OrderBy(s => s).Select(s => new Select2Type() { id = s, text = s }).ToList();
            var wireMessageTypes = WireDataManager.GetWireMessageTypes().Where(s => s.IsOutbound && EligibleOutboundMessageTypes.Contains(s.MessageType)).Select(s => new Select2Type() { id = s.hmsWireMessageTypeId.ToString(), text = s.MessageType }).ToList();
            var wireStatus = Enum.GetValues(typeof(WireDataManager.WireStatus)).Cast<int>().Select(x => new Select2Type() { id = ((int)x).ToString(), text = ((WireDataManager.WireStatus)x).ToString() }).ToList();

            List<Select2Type> wireReports;
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                wireReports = context.hmsWirePurposeLkups.Select(s => s.ReportName).Distinct().Select(x => new Select2Type() { id = x, text = x }).ToList();
            }

            return new List<DashboardReport.Preferences>()
            {
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.Clients.ToString(),Options = clients},
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.Funds.ToString(),Options = funds},
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.Admins.ToString(),Options = admins},
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.AccountTypes.ToString(),Options = accountTypes},
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.MessageTypes.ToString(),Options = wireMessageTypes},
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.Modules.ToString(),Options = wireReports},
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.Status.ToString(),Options = wireStatus}

            };
        }

        public JsonResult GetFundDetails(List<long> clientIds)
        {
            var authFundIds = AuthorizedDMAFundData.Select(s => s.HmFundId).ToList();
            var fundDetails = FundAccountManager.GetOnBoardingAccountDetails(authFundIds, AuthorizedSessionData.IsPrivilegedUser).Where(s => clientIds.Contains(-1) || clientIds.Contains(s.dmaClientOnBoardId ?? 0)).ToList();
            var funds = (from fnd in fundDetails where fnd.hmFundId > 0 select new Select2Type() { id = fnd.hmFundId.ToString(), text = fnd.ShortFundName }).Distinct(new Select2HeaderComparer()).OrderBy(s => s.text).ToList();
            //            var agreementTypes = fundDetails.Where(s => !string.IsNullOrWhiteSpace(s.AgreementType)).Select(s => s.AgreementType).Distinct().Union(fundDetails.Where(s => !string.IsNullOrWhiteSpace(s.AccountType) && s.AccountType != "Agreement").Select(s => s.AccountType).Distinct()).OrderBy(s => s).Select(s => new Select2Type() { id = s, text = s }).ToList();
            var adminMaps = new Dictionary<long, string>();
            foreach (var fund in fundDetails.Where(fund => !string.IsNullOrWhiteSpace(fund.AdminChoice) && fund.dmaOnBoardingAdminChoiceId != null && fund.dmaOnBoardingAdminChoiceId != 0 && !adminMaps.ContainsKey(fund.dmaOnBoardingAdminChoiceId ?? 0)))
            {
                adminMaps.Add(fund.dmaOnBoardingAdminChoiceId ?? 0, fund.AdminChoice);
            }
            var admins = adminMaps.Select(s => new Select2Type() { id = s.Key.ToString(), text = s.Value }).OrderBy(s => s.text).ToList();

            return Json(new List<DashboardReport.Preferences>()
            {
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.Funds.ToString(),Options = funds},
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.Admins.ToString(),Options = admins}
                //new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.AccountTypes.ToString(),Options = agreementTypes},
            });
        }

        public JsonResult GetWireLogData(DateTime startDate, DateTime endDate, Dictionary<DashboardReport.PreferenceCode, string> searchPreference, string timeZone)
        {
            var wireData = WireDashboardManager.GetWireTickets(startDate, endDate, AuthorizedSessionData.IsPrivilegedUser, searchPreference, false, timeZone, AuthorizedDMAFundData);
            var rowsToBuild = WireDashboardManager.ConstructWireDataRows(wireData, false);
            SetSessionValue(OpsSecureSessionVars.WiresDashboardData.ToString(), rowsToBuild);
            var rows = JsonHelper.GetJson(rowsToBuild);
            return Json(new { rows }, JsonRequestBehavior.AllowGet);
        }

        public FileResult ExportReport(DateTime startDate, DateTime endDate, string templateName, string format = ".xlsx")
        {
            var rowData = (List<Row>)GetSessionValue(OpsSecureSessionVars.WiresDashboardData.ToString());
            var fileName = string.Format("{0}_{1}_{2:yyyyMMdd}_{3:yyyyMMdd}", "Wires_Data", templateName, startDate, endDate);
            var exportFileInfo = new FileInfo(string.Format("{0}{1}{2}", FileSystemManager.UploadTemporaryFilesPath, fileName, format));

            foreach (var row in rowData)
            {
                row["Wire Status"] = Middleware.Util.Utility.StripHtml(row["Wire Status"]);
                row["Swift Status"] = Middleware.Util.Utility.StripHtml(row["Swift Status"]);
            }

            ReportDeliveryManager.CreateExportFile(rowData, "Wires Data", exportFileInfo);
            return DownloadAndDeleteFile(exportFileInfo);
        }
    }
}