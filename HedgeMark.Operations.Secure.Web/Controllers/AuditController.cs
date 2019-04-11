﻿using HedgeMark.Operations.Secure.DataModel;
using HMOSecureMiddleware;
using HMOSecureMiddleware.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HMOSecureWeb.Controllers
{
    public class AuditController : BaseController
    {
        // GET: Audit
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult InboundLogs()
        {
            return View();
        }

        public JsonResult GetInboundMQLogs(DateTime startDate, DateTime endDate)
        {
            var inBoundMQLogs = WireDataManager.GetInboundMQLogs(startDate, endDate);
            return Json(inBoundMQLogs);
        }


        public JsonResult GetWireAuditLogs(DateTime startDate, DateTime endDate, string module)
        {
            var auditLogs = AuditManager.GetConsolidatedLogs(startDate, endDate, module);
            auditLogs.ForEach(log => log.UserName = log.UserName.HumanizeEmail());
           
            return Json(auditLogs);
        }

        public ActionResult GetAuditLogsModule()
        {
            var auditLogs = AuditManager.GetModuleNames();
            return Json(new
            {
                aaData = auditLogs.Select(x => new object[]
                {
                    x.Key,
                    x.Value
                })
            }, JsonRequestBehavior.AllowGet);
        }

        public void AuditWireLogs(AuditLogData auditLogData)
        {
            AuditManager.LogAudit(auditLogData, UserName);
        }

        public JsonResult GetMessageTypesForAudits()
        {
            using (var context = new OperationsSecureContext())
            {
                var wireMessageTypes = context.hmsWireMessageTypes.Where(s => s.IsOutbound).ToList();
                return Json(wireMessageTypes.Select(s => new { id = s.hmsWireMessageTypeId, text = s.MessageType }).ToList());
            }
        }
    }
}