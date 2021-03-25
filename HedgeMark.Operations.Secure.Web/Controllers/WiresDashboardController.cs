﻿using System;
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
            var fundDetails = FundAccountManager.GetOnBoardingAccountDetails(authFundIds, AuthorizedSessionData.IsPrivilegedUser).Where(s => clientIds.Contains(s.dmaClientOnBoardId ?? 0)).ToList();
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
            var fundDetails = FundAccountManager.GetOnBoardingAccountDetails(authFundIds, AuthorizedSessionData.IsPrivilegedUser).Where(s => fundIds.Contains(s.hmFundId)).ToList();
            var agreementTypes = fundDetails.Where(s => !string.IsNullOrWhiteSpace(s.AgreementType)).Select(s => s.AgreementType).Distinct().Union(fundDetails.Where(s => !string.IsNullOrWhiteSpace(s.AccountType) && s.AccountType != "Agreement").Select(s => s.AccountType).Distinct()).OrderBy(s => s).Select(s => new Select2Type() { id = s, text = s }).ToList();

            return Json(new List<DashboardReport.Preferences>()
            {
                new DashboardReport.Preferences(){Preference = DashboardReport.PreferenceCode.AgreementTypes.ToString(),Options = agreementTypes},
            });
        }

        public JsonResult GetWireLogData(DateTime startDate, DateTime endDate, Dictionary<DashboardReport.PreferenceCode, string> searchPreference)
        {
            var wireData = WireDashboardManager.GetWireTickets(startDate, endDate, searchPreference, false, AuthorizedDMAFundData);
            var rowsToBuild = ConstructWireDataRows(wireData);
            SetSessionValue(OpsSecureSessionVars.WiresDashboardData.ToString(), rowsToBuild);
            var rows = JsonHelper.GetJson(rowsToBuild);
            return Json(new { rows }, JsonRequestBehavior.AllowGet);
        }

        private static readonly string StatusPattern = "<label.*?>(.*?)<\\/label>";

        private string GetStatusLabel(string htmlStr)
        {
            var matches = Regex.Matches(htmlStr, StatusPattern);
            return matches[0].Groups[1].Value;
        }

        private string GetWireStatusLabel(WireDataManager.WireStatus wireStatus, WireDataManager.SwiftStatus swiftStatus)
        {
            switch (wireStatus)
            {
                case WireDataManager.WireStatus.Drafted: return "<label class='label label-default'>Drafted</label>";
                case WireDataManager.WireStatus.Initiated: return "<label class='label label-warning'>Pending</label>";
                case WireDataManager.WireStatus.Approved: return "<label class='label label-success'>Approved</label>";
                case WireDataManager.WireStatus.Cancelled: return (int)swiftStatus == 1 ? "<label class='label label-danger'>Rejected</label>" : "<label class='label label-default'>Cancelled</label>";
                case WireDataManager.WireStatus.Failed: return "<label class='label label-danger'>Failed</label>";
                case WireDataManager.WireStatus.OnHold: return "<label class='label label-info'>On-Hold</label>";
                default:
                    return "Status Unknown";
            }
        }

        private string GetSwiftStatusLabel(WireDataManager.SwiftStatus swiftStatus)
        {
            switch (swiftStatus)
            {
                case WireDataManager.SwiftStatus.NotInitiated: return "<label class='label label-default'>Not Started</label>";
                case WireDataManager.SwiftStatus.Processing: return "<label class='label label-warning'>Pending Ack</label>";
                case WireDataManager.SwiftStatus.Acknowledged: return "<label class='label label-success'>Acknowledged</label>";
                case WireDataManager.SwiftStatus.NegativeAcknowledged: return "<label class='label label-danger'>N-Acknowledged</label>";
                case WireDataManager.SwiftStatus.Completed: return "<label class='label label-info'>Completed</label>";
                case WireDataManager.SwiftStatus.Failed: return "<label class='label label-danger'>Failed</label>";
                default:
                    return "Status Unknown";
            }
        }

        private List<Row> ConstructWireDataRows(List<WireTicket> wireData)
        {

            var rows = new List<Row>();

            foreach (var ticket in wireData)
            {
                var thisRow = new Row();
                thisRow["WireId"] = ticket.WireId.ToString();
                thisRow["Wire Status"] = GetWireStatusLabel((WireDataManager.WireStatus)ticket.HMWire.WireStatusId, (WireDataManager.SwiftStatus)ticket.HMWire.SwiftStatusId);
                thisRow["Swift Status"] = GetSwiftStatusLabel((WireDataManager.SwiftStatus)ticket.HMWire.SwiftStatusId);
                thisRow["Client"] = ticket.ClientLegalName;
                thisRow["Fund"] = ticket.PreferredFundName;
                thisRow["Sending Account Name"] = ticket.SendingAccount.AccountName;
                thisRow["Sending Account Number"] = ticket.SendingAccountNumber;
                thisRow["Transfer Type"] = ticket.TransferType;
                thisRow["Source Report"] = ticket.HMWire.hmsWirePurposeLkup.ReportName;
                thisRow["Wire Purpose"] = ticket.HMWire.hmsWirePurposeLkup.Purpose;
                thisRow["Value Date", RuleHelper.DefaultDateFormat] = ticket.HMWire.ValueDate.ToString("yyyy-MM-dd");
                thisRow["Currency"] = ticket.HMWire.Currency;
                thisRow["Amount", RuleHelper.DefaultCurrencyFormat] = ticket.HMWire.ToCurrency();
                thisRow["Template Name"] = ticket.ReceivingAccountName;
                thisRow["Beneficiary Bank"] = ticket.BeneficiaryBank;
                thisRow["Beneficiary"] = ticket.Beneficiary;
                thisRow["Beneficiary A/C Number"] = ticket.BeneficiaryAccountNumber;
                thisRow["Wire Message Type"] = ticket.HMWire.hmsWireMessageType.MessageType;

                switch (ticket.HMWire.hmsWireStatusLkup.Status)
                {
                    case "Drafted":
                        // $(row).addClass("info");
                        break;
                    case "Initiated":
                        thisRow.RowHighlight = Row.Highlight.Warning;
                        break;
                    case "Approved":
                    case "Processing":
                        thisRow.RowHighlight = Row.Highlight.Success;
                        break;
                    case "Cancelled":
                        thisRow.RowHighlight = Row.Highlight.SubHeader;
                        break;
                    case "Completed":
                        thisRow.RowHighlight = Row.Highlight.Info;
                        break;
                    case "Failed":
                        thisRow.RowHighlight = Row.Highlight.Error;
                        break;
                    case "On Hold":
                        thisRow.RowHighlight = Row.Highlight.Header;
                        break;
                }



                rows.Add(thisRow);
            }

            return rows;

        }


        public FileResult ExportReport(DateTime startDate, DateTime endDate, string templateName, string format = ".xlsx")
        {
            var rowData = (List<Row>)GetSessionValue(OpsSecureSessionVars.WiresDashboardData.ToString());
            var fileName = string.Format("{0}_{1}_{2:yyyyMMdd}_{3:yyyyMMdd}", "Wires_Data", templateName, startDate, endDate);
            var exportFileInfo = new FileInfo(string.Format("{0}{1}{2}", FileSystemManager.UploadTemporaryFilesPath, fileName, format));

            foreach (var row in rowData)
            {
                row["Wire Status"] = GetStatusLabel(row["Wire Status"]);
                row["Swift Status"] = GetStatusLabel(row["Swift Status"]);
            }

            var contentToExport = new ExportContent() { Rows = rowData, TabName = "Wires Data" };
            ReportDeliveryManager.CreateExportFile(contentToExport, exportFileInfo);
            return DownloadAndDeleteFile(exportFileInfo);
        }
    }
}