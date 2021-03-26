using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Com.HedgeMark.Commons.Extensions;
using ExcelUtility.Operations.ManagedAccounts;
using HedgeMark.Operations.FileParseEngine.Models;
using HedgeMark.Operations.FileParseEngine.RuleEngine;
using HedgeMark.Operations.Secure.Middleware;
using HedgeMark.Operations.Secure.Middleware.Models;
using HedgeMark.Operations.Secure.Middleware.Util;
using HMOSecureWeb.Utility;

namespace HMOSecureWeb.Controllers
{
    public class WiresDashboardController : BaseController
    {
        // GET: WiresDashboard
        public ActionResult Index()
        {
            return View();
        }

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

            var clients = clientMaps.Select(s => new Select2Type() { id = s.Key.ToString(), text = s.Value }).OrderBy(s => s.text).ToList();
            var funds = (from fnd in fundDetails where fnd.hmFundId > 0 select new Select2Type() { id = fnd.hmFundId.ToString(), text = fnd.ShortFundName }).Distinct(new Select2HeaderComparer()).OrderBy(s => s.text).ToList();
            var agreementTypes = fundDetails.Where(s => !string.IsNullOrWhiteSpace(s.AgreementType)).Select(s => s.AgreementType).Distinct().Union(fundDetails.Where(s => !string.IsNullOrWhiteSpace(s.AccountType) && s.AccountType != "Agreement").Select(s => s.AccountType).Distinct()).OrderBy(s => s).Select(s => new Select2Type() { id = s, text = s }).ToList();
            var wireMessageTypes = WireDataManager.GetWireMessageTypes().Where(s => s.IsOutbound).Select(s => new Select2Type() { id = s.hmsWireMessageTypeId.ToString(), text = s.MessageType }).ToList();
            var wireStatus = Enum.GetValues(typeof(WireDataManager.WireStatus)).Cast<int>().Select(x => new Select2Type() { id = ((int)x).ToString(), text = ((WireDataManager.WireStatus)x).ToString() }).ToList();

            return new List<DashboardReport.Preferences>()
            {
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.Clients.ToString(),Options = clients},
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.Funds.ToString(),Options = funds},
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.AgreementTypes.ToString(),Options = agreementTypes},
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.MessageTypes.ToString(),Options = wireMessageTypes},
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.Status.ToString(),Options=wireStatus}
            };
        }


        public JsonResult GetFundDetails(List<long> clientIds)
        {
            var authFundIds = AuthorizedDMAFundData.Select(s => s.HmFundId).ToList();
            var fundDetails = FundAccountManager.GetOnBoardingAccountDetails(authFundIds, AuthorizedSessionData.IsPrivilegedUser).Where(s => clientIds.Contains(-1) || clientIds.Contains(s.dmaClientOnBoardId ?? 0)).ToList();
            var funds = (from fnd in fundDetails where fnd.hmFundId > 0 select new Select2Type() { id = fnd.hmFundId.ToString(), text = fnd.ShortFundName }).Distinct(new Select2HeaderComparer()).OrderBy(s => s.text).ToList();
            var agreementTypes = fundDetails.Where(s => !string.IsNullOrWhiteSpace(s.AgreementType)).Select(s => s.AgreementType).Distinct().Union(fundDetails.Where(s => !string.IsNullOrWhiteSpace(s.AccountType) && s.AccountType != "Agreement").Select(s => s.AccountType).Distinct()).OrderBy(s => s).Select(s => new Select2Type() { id = s, text = s }).ToList();

            return Json(new List<DashboardReport.Preferences>()
            {
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.Funds.ToString(),Options = funds},
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.AgreementTypes.ToString(),Options = agreementTypes},
            });
        }

        public JsonResult GetAgreementTypes(List<long> fundIds)
        {
            var authFundIds = AuthorizedDMAFundData.Select(s => s.HmFundId).ToList();
            var fundDetails = FundAccountManager.GetOnBoardingAccountDetails(authFundIds, AuthorizedSessionData.IsPrivilegedUser).Where(s => fundIds.Contains(-1) || fundIds.Contains(s.hmFundId)).ToList();
            var agreementTypes = fundDetails.Where(s => !string.IsNullOrWhiteSpace(s.AgreementType)).Select(s => s.AgreementType).Distinct().Union(fundDetails.Where(s => !string.IsNullOrWhiteSpace(s.AccountType) && s.AccountType != "Agreement").Select(s => s.AccountType).Distinct()).OrderBy(s => s).Select(s => new Select2Type() { id = s, text = s }).ToList();

            return Json(new List<DashboardReport.Preferences>()
            {
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.AgreementTypes.ToString(),Options = agreementTypes},
            });
        }

        public JsonResult GetWireLogData(DateTime startDate, DateTime endDate, Dictionary<DashboardReport.PreferenceCode, string> searchPreference)
        {
            var wireData = WireDashboardManager.GetWireTickets(startDate, endDate, searchPreference, false, AuthorizedDMAFundData);
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
                row["Wire Status"] = row["Wire Status"].StripHtml();
                row["Swift Status"] = row["Swift Status"].StripHtml();
            }

            var contentToExport = new ExportContent() { Rows = rowData, TabName = "Wires Data" };
            ReportDeliveryManager.CreateExportFile(contentToExport, exportFileInfo);
            return DownloadAndDeleteFile(exportFileInfo);
        }
    }
}