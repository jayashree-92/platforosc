using ExcelUtility.Operations.ManagedAccounts;
using HedgeMark.Operations.FileParseEngine.Models;
using HedgeMark.Operations.Secure.DataModel;
using HMOSecureMiddleware;
using HMOSecureMiddleware.Util;
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
            var swiftGroupData = AccountManager.GetAllSwiftGroup();
            var brokerLegalEntityData = OnBoardingDataManager.GetAllCounterpartyFamilies().Select(x => new { id = x.dmaCounterpartyFamilyId, text = x.CounterpartyFamily }).OrderBy(x => x.text).ToList();
            var swiftGroupStatusData = AccountManager.GetSwiftGroupStatus().Select(s => new { id = s.hmsSwiftGroupStatusLkpId, text = s.Status }).ToList();
            var counterpartyData = brokerLegalEntityData.GroupBy(s => s.id).ToDictionary(p => p.Key, v => v.FirstOrDefault().text);
            var swiftStatusData = swiftGroupStatusData.GroupBy(s => s.id).ToDictionary(p => p.Key, v => v.FirstOrDefault().text);
            var wireMessageTypes = WireDataManager.GetWireMessageTypes().Select(s => new { id = s.MessageType, text = s.MessageType }).ToList();
            return Json(new
            {
                brokerLegalEntityData,
                swiftGroupStatusData,
                wireMessageTypes,
                swiftGroupData = swiftGroupData.Select(s => new
                {
                    s.hmsSwiftGroupId,
                    s.SwiftGroup,
                    s.SendersBIC,
                    s.BrokerLegalEntityId,
                    Broker = counterpartyData.ContainsKey(s.BrokerLegalEntityId ?? 0) ? counterpartyData[s.BrokerLegalEntityId ?? 0] : string.Empty,
                    s.SwiftGroupStatusId,
                    SwiftGroupStatus = swiftStatusData.ContainsKey(s.SwiftGroupStatusId ?? 0) ? swiftStatusData[s.SwiftGroupStatusId ?? 0] : string.Empty,
                    s.AcceptedMessages,
                    s.IsDeleted,
                    s.Notes,
                    s.RecCreatedAt,
                    RecCreatedBy = s.RecCreatedBy.HumanizeEmail()
                })
            }, JsonRequestBehavior.AllowGet);
        }

        public void AddOrUpdateSwiftGroup(hmsSwiftGroup hmsSwiftGroup)
        {
            hmsSwiftGroup.RecCreatedBy = UserName;
            AccountManager.AddOrUpdateSwiftGroup(hmsSwiftGroup);
        }
    }
}