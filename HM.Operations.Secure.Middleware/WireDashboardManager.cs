﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HedgeMark.Operations.FileParseEngine.Models;
using HedgeMark.Operations.FileParseEngine.RuleEngine;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware.Jobs;
using HM.Operations.Secure.Middleware.Models;
using HM.Operations.Secure.Middleware.Util;
using Com.HedgeMark.Commons.Extensions;
using HM.Operations.Secure.Middleware.SwiftMessageManager;

namespace HM.Operations.Secure.Middleware
{
    public class WireDashboardManager
    {
        public static List<WireTicket> GetWireTickets(DateTime startContextDate, DateTime endContextDate, bool isPrivilegedUser, Dictionary<DashboardReport.PreferenceCode, string> searchPreference, bool shouldBringAllPendingWires, string timeZone, List<HFundBasic> authorizedDMAFundData = null)
        {
            var wireData = new List<WireTicket>();
            List<hmsWire> wireStatusDetails;

            var clientIds = new List<long>() { -1 };
            if(searchPreference.TryGetValue(DashboardReport.PreferenceCode.Clients, out var clientIdStr))
                clientIds = Array.ConvertAll(clientIdStr.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries), long.Parse).ToList();

            var fundIds = new List<long>() { -1 };
            if(searchPreference.TryGetValue(DashboardReport.PreferenceCode.Funds, out var fundIdStr))
                fundIds = Array.ConvertAll(fundIdStr.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries), long.Parse).ToList();

            var adminIds = new List<long>() { -1 };
            if(searchPreference.TryGetValue(DashboardReport.PreferenceCode.Admins, out var adminIdStr))
                adminIds = Array.ConvertAll(adminIdStr.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries), long.Parse).ToList();

            var allStatusIds = new List<long>() { -1 };
            if(searchPreference.TryGetValue(DashboardReport.PreferenceCode.Status, out var statusIdStr))
                allStatusIds = Array.ConvertAll(statusIdStr.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries), long.Parse).ToList();

            var agrTypes = new List<string>() { "-1" };
            if(searchPreference.TryGetValue(DashboardReport.PreferenceCode.AccountTypes, out var argTypeStr))
                agrTypes = argTypeStr.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var msgTypes = new List<long>() { -1 };
            if(searchPreference.TryGetValue(DashboardReport.PreferenceCode.MessageTypes, out var msgTypeStr))
                msgTypes = Array.ConvertAll(msgTypeStr.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries), long.Parse).ToList();

            var reports = new List<string>() { "-1" };
            if(searchPreference.TryGetValue(DashboardReport.PreferenceCode.Modules, out var moduleStr))
                reports = moduleStr.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var authorizedFundMap = authorizedDMAFundData == null ? AdminFundManager.GetHFundsCreatedForDMAOnly(PreferencesManager.FundNameInDropDown.OpsShortName).ToDictionary(s => s.HmFundId, v => v) : authorizedDMAFundData.ToDictionary(s => s.HmFundId, v => v);
            using(var context = new OperationsSecureContext())
            {
                //context.Database.Log = s =>
                //{
                //    Logger.Debug(s);
                //};    

                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var authorizedFundIds = authorizedFundMap.Keys.ToList();
                var wireTicketQuery = context.hmsWires.Where(s => authorizedFundIds.Contains(s.hmFundId))
                    .Include(s => s.hmsWireMessageType)
                    .Include(s => s.hmsWirePurposeLkup)
                    .Include(s => s.hmsWireStatusLkup)
                    .Include(s => s.hmsWireTransferTypeLKup)
                    .Include(s => s.SendingAccount)
                    .Include(s => s.SendingAccount.WirePortalCutoff)
                    .Include(s => s.ReceivingAccount)
                    .Include(s => s.ReceivingAccount.Beneficiary)
                    .Include(s => s.ReceivingAccount.Intermediary)
                    .Include(s => s.ReceivingAccount.UltimateBeneficiary)
                    .Include(s => s.ReceivingSSITemplate)
                    .Include(s => s.ReceivingSSITemplate.Beneficiary)
                    .Include(s => s.ReceivingSSITemplate.Intermediary)
                    .Include(s => s.ReceivingSSITemplate.UltimateBeneficiary)
                    .Include(s => s.hmsWireSenderInformation);

                if(shouldBringAllPendingWires)
                    wireTicketQuery = wireTicketQuery.Where(s => ((allStatusIds.Contains(0) || allStatusIds.Contains(2)) && s.WireStatusId == 2)
                           || s.ValueDate >= startContextDate && s.ValueDate <= endContextDate && (allStatusIds.Contains(0) || allStatusIds.Contains(s.WireStatusId))
                           || DbFunctions.TruncateTime(s.CreatedAt) == DbFunctions.TruncateTime(endContextDate) && (allStatusIds.Contains(0) || allStatusIds.Contains(s.WireStatusId)));

                else
                {
                    wireTicketQuery = wireTicketQuery.Where(s => s.ValueDate >= startContextDate && s.ValueDate <= endContextDate)
                            .Where(s => fundIds.Contains(-1) || fundIds.Contains(s.hmFundId))
                            .Where(s => allStatusIds.Contains(-1) || allStatusIds.Contains(s.WireStatusId))
                            .Where(s => msgTypes.Contains(-1) || msgTypes.Contains(s.WireMessageTypeId))
                            .Where(s => reports.Contains("-1") || reports.Contains(s.hmsWirePurposeLkup.ReportName));
                }

                wireStatusDetails = wireTicketQuery.ToList();
            }

            if(!agrTypes.Contains("-1"))
            {
                var allWireIds = new List<long>();
                var agreementIds = wireStatusDetails.Where(s => s.SendingAccount.AccountType == "Agreement" && s.SendingAccount.dmaAgreementOnBoardingId > 0).Select(s => s.SendingAccount.dmaAgreementOnBoardingId).Distinct().ToList();
                using(var context = new AdminContext())
                {
                    var agrmtMap = context.vw_CounterpartyAgreements.Where(s => agreementIds.Contains(s.dmaAgreementOnBoardingId) && agrTypes.Contains(s.AgreementType)).Select(s => s.dmaAgreementOnBoardingId).Distinct().ToList();
                    allWireIds.AddRange(wireStatusDetails.Where(s => agrmtMap.Contains(s.SendingAccount.dmaAgreementOnBoardingId ?? 0)).Select(s => s.hmsWireId).ToList());
                }

                wireStatusDetails = wireStatusDetails.Where(s => allWireIds.Contains(s.hmsWireId)).ToList();
            }
            if(!clientIds.Contains(-1))
            {
                using var context = new AdminContext();
                var fundsOfSelectedClients = context.vw_HFund.Where(s => clientIds.Contains(s.dmaClientOnBoardId ?? 0)).Select(s => s.hmFundId).ToList();
                wireStatusDetails = wireStatusDetails.Where(s => fundsOfSelectedClients.Contains((int)s.hmFundId)).ToList();
            }
            if(!adminIds.Contains(-1))
            {
                using var context = new AdminContext();
                var fundsOfSelectedAdmins = context.vw_HFund.Where(s => adminIds.Contains(s.FundAdministrator ?? 0)).Select(s => s.hmFundId).ToList();
                wireStatusDetails = wireStatusDetails.Where(s => fundsOfSelectedAdmins.Contains((int)s.hmFundId)).ToList();
            }

            var userIds = wireStatusDetails.Select(s => s.LastUpdatedBy).Union(wireStatusDetails.Select(s => s.CreatedBy))
                .Union(wireStatusDetails.Select(s => s.ApprovedBy ?? 0)).Distinct().ToList();
            var users = FileSystemManager.GetUsersList(userIds);


            var timeZones = FileSystemManager.GetAllTimeZones();
            var authFundIds = authorizedFundMap.Select(s => s.Key).ToList();
            var fundDetails = FundAccountManager.GetFundAccountDetails(authFundIds, isPrivilegedUser).Where(s => clientIds.Contains(-1) || clientIds.Contains(s.dmaClientOnBoardId ?? 0)).ToList();
            var counterParties = OnBoardingDataManager.GetAllOnBoardedCounterparties();
            foreach(var wire in wireStatusDetails)
            {
                var fund = authorizedFundMap.TryGetValue(wire.hmFundId, out var fundVal) ? fundVal : new HFundBasic();
                var fndAcct = fundDetails.FirstOrDefault(s => s.hmFundId == wire.hmFundId);
                var thisWire = new WireTicket
                {
                    HMWire = wire,
                    SendingAccount = wire.SendingAccount,
                    ReceivingAccount = wire.ReceivingAccount ?? new onBoardingAccount(),
                    SSITemplate = wire.ReceivingSSITemplate ?? new onBoardingSSITemplate(),
                    PreferredFundName = fund.PreferredFundName ?? string.Empty,
                    ShortFundName = fund.PreferredFundName ?? string.Empty,
                    ClientLegalName = fund.ClientLegalName ?? string.Empty,
                    AdminName = fndAcct == null ? string.Empty : fndAcct.AdminChoice,
                    CustodianName = fndAcct == null ? string.Empty : fndAcct.CustodianCompanyName
                };

                thisWire.Deadline = wire.WireStatusId == 2
                    ? WireDataManager.GetDeadlineToApprove(thisWire.SendingAccount, thisWire.HMWire.ValueDate, timeZones)
                    : new TimeSpan();

                NullifyCircularReferences(thisWire);

                //Update User Details
                thisWire.WireCreatedBy = users.First(s => s.Key == thisWire.HMWire.CreatedBy).Value.HumanizeEmail();
                thisWire.WireLastUpdatedBy = users.First(s => s.Key == thisWire.HMWire.LastUpdatedBy).Value.HumanizeEmail();
                thisWire.WireApprovedBy = thisWire.HMWire.ApprovedBy > 0 ? users.First(s => s.Key == thisWire.HMWire.ApprovedBy).Value.HumanizeEmail() : "-";

                //Set CounterParty
                if(thisWire.SendingAccount != null)
                    thisWire.Counterparty = counterParties.TryGetValue(thisWire.SendingAccount.dmaCounterpartyId ?? 0, out var cptyName) ? cptyName : null;

                SetUserTitles(thisWire);
                wireData.Add(thisWire);
            }

            var timeZoneInfo = ScheduleManager.TimeZones.TryGetValue(timeZone, out var zone) ? zone : Utility.DefaultSystemTimeZone;

            foreach(var s in wireData)
            {
                s.HMWire.CreatedAt = TimeZoneInfo.ConvertTime(s.HMWire.CreatedAt, timeZoneInfo);
                s.HMWire.LastModifiedAt = TimeZoneInfo.ConvertTime(s.HMWire.LastModifiedAt, timeZoneInfo);

                if(s.HMWire.ApprovedAt != null)
                    s.HMWire.ApprovedAt = TimeZoneInfo.ConvertTime(s.HMWire.ApprovedAt ?? new DateTime(), timeZoneInfo);

                foreach(var workflowLog in s.HMWire.hmsWireWorkflowLogs)
                {
                    workflowLog.CreatedAt = TimeZoneInfo.ConvertTime(workflowLog.CreatedAt, timeZoneInfo);
                }
            }

            //Custom ordering as per HMOS-56
            var customWireStatusOrder = new[] { 2, 5, 1, 4, 3 };
            var customSwiftStatusOrder = new[] { 2, 4, 6, 3, 5, 1 };
            wireData = wireData.OrderBy(s => Array.IndexOf(customWireStatusOrder, s.HMWire.WireStatusId)).ThenBy(s => Array.IndexOf(customSwiftStatusOrder, s.HMWire.SwiftStatusId)).ToList();
            return wireData;
        }

        private static void SetUserTitles(WireTicket thisWire)
        {
            //When wire is Drafted - hide Last Modified by
            if(thisWire.HMWire.WireStatusId == 1)
            {
                thisWire.WireLastUpdatedBy = "-";
                thisWire.HMWire.LastModifiedAt = new DateTimeOffset(1, 1, 1, 1, 1, 1, new TimeSpan());
                thisWire.WireApprovedBy = "-";
                thisWire.HMWire.ApprovedAt = null;
            }

            //approved wire
            if((thisWire.HMWire.WireStatusId == 3 || thisWire.HMWire.WireStatusId == 5) && thisWire.HMWire.ApprovedBy == null)
            {
                thisWire.WireApprovedBy = thisWire.WireLastUpdatedBy;
                thisWire.HMWire.ApprovedAt = thisWire.HMWire.LastModifiedAt;
            }

            //approved wire - MT210 -This has auto approval
            if(thisWire.HMWire.WireMessageTypeId == 5 && thisWire.HMWire.WireStatusId == 3 && thisWire.WireApprovedBy == "-")
            {
                thisWire.WireApprovedBy = "System";
            }

            if(thisWire.HMWire.CreatedAt == thisWire.HMWire.LastModifiedAt)
            {
                thisWire.WireLastUpdatedBy = "-";
                thisWire.HMWire.LastModifiedAt = new DateTimeOffset(1, 1, 1, 1, 1, 1, new TimeSpan());
            }
        }


        private static void NullifyCircularReferences(WireTicket thisWire)
        {
            thisWire.HMWire.hmsWireMessageType.hmsWires = null;
            thisWire.HMWire.hmsWirePurposeLkup.hmsWires = null;
            thisWire.HMWire.hmsWireStatusLkup.hmsWires = null;
            thisWire.HMWire.hmsWireTransferTypeLKup.hmsWires = null;
            if(thisWire.HMWire.hmsWireSenderInformation != null)
                thisWire.HMWire.hmsWireSenderInformation.hmsWires = null;

            thisWire.HMWire.SendingAccount.hmsWires = null;
            if(thisWire.HMWire.ReceivingAccount != null)
                thisWire.HMWire.ReceivingAccount.hmsWires1 = null;
            if(thisWire.HMWire.ReceivingSSITemplate != null)
                thisWire.HMWire.ReceivingSSITemplate.hmsWires = null;

            thisWire.SendingAccount.onBoardingAccountSSITemplateMaps = null;
            thisWire.ReceivingAccount.onBoardingAccountSSITemplateMaps = null;
            thisWire.SSITemplate.onBoardingAccountSSITemplateMaps = null;

            if(thisWire.SendingAccount.SwiftGroup != null)
                thisWire.SendingAccount.SwiftGroup.onBoardingAccounts = null;

            if(thisWire.SendingAccount.WirePortalCutoff != null)
                thisWire.SendingAccount.WirePortalCutoff.onBoardingAccounts = null;

            if(thisWire.ReceivingAccount.SwiftGroup != null)
                thisWire.ReceivingAccount.SwiftGroup.onBoardingAccounts = null;

            if(thisWire.ReceivingAccount.WirePortalCutoff != null)
                thisWire.ReceivingAccount.WirePortalCutoff.onBoardingAccounts = null;

            if(thisWire.SendingAccount.UltimateBeneficiary != null)
                thisWire.SendingAccount.UltimateBeneficiary.onBoardingAccounts =
                    thisWire.SendingAccount.UltimateBeneficiary.onBoardingAccounts1 =
                        thisWire.SendingAccount.UltimateBeneficiary.onBoardingAccounts2 = null;
            else
                thisWire.SendingAccount.UltimateBeneficiary = new onBoardingAccountBICorABA();

            if(thisWire.ReceivingAccount.UltimateBeneficiary != null)
                thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingAccounts =
                    thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingAccounts1 =
                        thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingAccounts2 = null;
            else
                thisWire.ReceivingAccount.UltimateBeneficiary = new onBoardingAccountBICorABA();

            if(thisWire.SSITemplate.Beneficiary != null)
                thisWire.SSITemplate.Beneficiary.onBoardingAccounts =
                    thisWire.SSITemplate.Beneficiary.onBoardingAccounts1 =
                        thisWire.SSITemplate.Beneficiary.onBoardingAccounts2 = null;
            else
                thisWire.SSITemplate.Beneficiary = new onBoardingAccountBICorABA();

            if(thisWire.SSITemplate.Intermediary != null)
                thisWire.SSITemplate.Intermediary.onBoardingAccounts =
                    thisWire.SSITemplate.Intermediary.onBoardingAccounts1 =
                        thisWire.SSITemplate.Intermediary.onBoardingAccounts2 = null;

            if(thisWire.SSITemplate.UltimateBeneficiary != null)
                thisWire.SSITemplate.UltimateBeneficiary.onBoardingAccounts =
                    thisWire.SSITemplate.UltimateBeneficiary.onBoardingAccounts1 =
                        thisWire.SSITemplate.UltimateBeneficiary.onBoardingAccounts2 = null;
            else
                thisWire.SSITemplate.UltimateBeneficiary = new onBoardingAccountBICorABA();

            if(thisWire.SSITemplate.Beneficiary != null)
                thisWire.SSITemplate.Beneficiary.onBoardingSSITemplates =
                    thisWire.SSITemplate.Beneficiary.onBoardingSSITemplates1 =
                        thisWire.SSITemplate.Beneficiary.onBoardingSSITemplates2 = null;

            if(thisWire.SSITemplate.Intermediary != null)
                thisWire.SSITemplate.Intermediary.onBoardingSSITemplates =
                    thisWire.SSITemplate.Intermediary.onBoardingSSITemplates1 =
                        thisWire.SSITemplate.Intermediary.onBoardingSSITemplates2 = null;

            if(thisWire.SSITemplate.UltimateBeneficiary != null)
                thisWire.SSITemplate.UltimateBeneficiary.onBoardingSSITemplates =
                    thisWire.SSITemplate.UltimateBeneficiary.onBoardingSSITemplates1 =
                        thisWire.SSITemplate.UltimateBeneficiary.onBoardingSSITemplates2 = null;

            if(thisWire.SendingAccount.Beneficiary != null)
                thisWire.SendingAccount.Beneficiary.onBoardingAccounts =
                    thisWire.SendingAccount.Beneficiary.onBoardingAccounts1 =
                        thisWire.SendingAccount.Beneficiary.onBoardingAccounts2 = null;
            else
                thisWire.SendingAccount.Beneficiary = new onBoardingAccountBICorABA();

            if(thisWire.SendingAccount.Intermediary != null)
                thisWire.SendingAccount.Intermediary.onBoardingAccounts =
                    thisWire.SendingAccount.Intermediary.onBoardingAccounts1 =
                        thisWire.SendingAccount.Intermediary.onBoardingAccounts2 = null;

            if(thisWire.SendingAccount.UltimateBeneficiary != null)
                thisWire.SendingAccount.UltimateBeneficiary.onBoardingAccounts =
                    thisWire.SendingAccount.UltimateBeneficiary.onBoardingAccounts1 =
                        thisWire.SendingAccount.UltimateBeneficiary.onBoardingAccounts2 = null;

            if(thisWire.ReceivingAccount.Beneficiary != null)
                thisWire.ReceivingAccount.Beneficiary.onBoardingAccounts =
                    thisWire.ReceivingAccount.Beneficiary.onBoardingAccounts1 =
                        thisWire.ReceivingAccount.Beneficiary.onBoardingAccounts2 = null;
            else
                thisWire.ReceivingAccount.Beneficiary = new onBoardingAccountBICorABA();

            if(thisWire.ReceivingAccount.Intermediary != null)
                thisWire.ReceivingAccount.Intermediary.onBoardingAccounts =
                    thisWire.ReceivingAccount.Intermediary.onBoardingAccounts1 =
                        thisWire.ReceivingAccount.Intermediary.onBoardingAccounts2 = null;

            if(thisWire.ReceivingAccount.UltimateBeneficiary != null)
                thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingAccounts =
                    thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingAccounts1 =
                        thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingAccounts2 = null;

            if(thisWire.SendingAccount.Beneficiary != null)
                thisWire.SendingAccount.Beneficiary.onBoardingSSITemplates =
                    thisWire.SendingAccount.Beneficiary.onBoardingSSITemplates1 =
                        thisWire.SendingAccount.Beneficiary.onBoardingSSITemplates2 = null;

            if(thisWire.SendingAccount.Intermediary != null)
                thisWire.SendingAccount.Intermediary.onBoardingSSITemplates =
                    thisWire.SendingAccount.Intermediary.onBoardingSSITemplates1 =
                        thisWire.SendingAccount.Intermediary.onBoardingSSITemplates2 = null;

            if(thisWire.SendingAccount.UltimateBeneficiary != null)
                thisWire.SendingAccount.UltimateBeneficiary.onBoardingSSITemplates =
                    thisWire.SendingAccount.UltimateBeneficiary.onBoardingSSITemplates1 =
                        thisWire.SendingAccount.UltimateBeneficiary.onBoardingSSITemplates2 = null;

            if(thisWire.ReceivingAccount.Beneficiary != null)
                thisWire.ReceivingAccount.Beneficiary.onBoardingSSITemplates =
                    thisWire.ReceivingAccount.Beneficiary.onBoardingSSITemplates1 =
                        thisWire.ReceivingAccount.Beneficiary.onBoardingSSITemplates2 = null;

            if(thisWire.ReceivingAccount.Intermediary != null)
                thisWire.ReceivingAccount.Intermediary.onBoardingSSITemplates =
                    thisWire.ReceivingAccount.Intermediary.onBoardingSSITemplates1 =
                        thisWire.ReceivingAccount.Intermediary.onBoardingSSITemplates2 = null;

            if(thisWire.ReceivingAccount.UltimateBeneficiary != null)
                thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingSSITemplates =
                    thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingSSITemplates1 =
                        thisWire.ReceivingAccount.UltimateBeneficiary.onBoardingSSITemplates2 = null;
        }


        private static string GetWireStatusLabel(WireDataManager.WireStatus wireStatus, WireDataManager.SwiftStatus swiftStatus)
        {
            switch(wireStatus)
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

        private static string GetSwiftStatusLabel(WireDataManager.SwiftStatus swiftStatus)
        {
            switch(swiftStatus)
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

        public static List<Row> ConstructWireDataRows(List<WireTicket> wireData, bool isExportOnly)
        {
            var rows = new List<Row>();

            foreach(var ticket in wireData)
            {
                var messageType = ticket.HMWire.hmsWireMessageType.MessageType;

                var thisRow = new Row
                {
                    ["WireId"] = ticket.WireId.ToString(),
                    ["Wire Status"] = isExportOnly ? ((WireDataManager.WireStatus)ticket.HMWire.WireStatusId).ToString() : GetWireStatusLabel((WireDataManager.WireStatus)ticket.HMWire.WireStatusId, (WireDataManager.SwiftStatus)ticket.HMWire.SwiftStatusId),
                    ["Swift Status"] = isExportOnly ? ((WireDataManager.SwiftStatus)ticket.HMWire.SwiftStatusId).ToString() : GetSwiftStatusLabel((WireDataManager.SwiftStatus)ticket.HMWire.SwiftStatusId),
                    ["Client"] = ticket.ClientLegalName,
                    ["Fund"] = ticket.PreferredFundName,
                    ["Admin"] = ticket.AdminName,
                    ["Custodian"] = ticket.CustodianName,
                    ["Counterparty"] = ticket.Counterparty,
                    ["Sending Account Name"] = ticket.SendingAccount.AccountName,
                    ["Sending Account Number"] = ticket.SendingAccountNumber,
                    ["Transfer Type"] = ticket.TransferType,
                    ["Source Report"] = ticket.HMWire.hmsWirePurposeLkup.ReportName,
                    ["Wire Purpose"] = ticket.HMWire.hmsWirePurposeLkup.Purpose,
                    ["Value Date", RuleHelper.DefaultDateFormat] = ticket.HMWire.ValueDate.ToString("yyyy-MM-dd"),
                    ["Currency"] = ticket.HMWire.Currency,
                    ["Amount", RuleHelper.DefaultCurrencyFormat] = ticket.HMWire.Amount.ToCurrency(),
                    ["Template Name"] = ticket.ReceivingAccountName,
                    //["Beneficiary Bank"] = ticket.BeneficiaryBank,
                    ["Ultimate Beneficiary"] = ticket.UltimateBeneficiary,
                    ["Ultimate Beneficiary A/C Number"] = ticket.UltimateBeneficiaryAccountNumber,
                    ["Wire Message Type"] = messageType,
                    ["Comments"] = messageType == "MT103" || messageType == "MT202" ? OutboundSwiftMsgCreator.GetField72(ticket, messageType).GetValue() : string.Empty,
                    ["Initiated By"] = ticket.WireCreatedBy,
                    ["Initiated At"] = ticket.HMWire.CreatedAt.ToString("MMM dd, yyyy hh:mm tt"),
                    ["Approved By"] = ticket.WireApprovedBy,
                    ["Approved At"] = ticket.HMWire.ApprovedAt != null ? (ticket.HMWire.ApprovedAt ?? new DateTime()).ToString("MMM dd, yyyy hh:mm tt") : "-"
                };

                switch(ticket.HMWire.hmsWireStatusLkup.Status)
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


    }

}
