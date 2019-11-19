using HedgeMark.Operations.Secure.DataModel;
using HMOSecureMiddleware;
using HMOSecureMiddleware.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using HMOSecureMiddleware.Util;
using System.IO;

namespace HMOSecureWeb.Controllers
{
    public class AuditController : BaseController
    {
        // GET: Audit
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult MqLogs()
        {
            return View();
        }

        public JsonResult GetMQLogs(DateTime startDate, DateTime endDate)
        {
            var mqLogs = WireDataManager.GetMQLogs(startDate, endDate);
            return Json(mqLogs);
        }


        public JsonResult GetWireAuditLogs(DateTime startDate, DateTime endDate, string module)
        {
            var auditLogs = AuditManager.GetConsolidatedLogs(startDate, endDate, module);
            auditLogs.ForEach(log => log.UserName = log.UserName.HumanizeEmail());
            return Json(auditLogs);
        }

        public JsonResult GetBulkUploadLogs(DateTime startDate, DateTime endDate, bool isFundAccountLog)
        {
            var auditLogs = AuditManager.GetBulkUploadLogs(isFundAccountLog, startDate, endDate);
            auditLogs.ForEach(log => log.UserName = log.UserName.HumanizeEmail());
            return Json(auditLogs);
        }

        public FileResult DownloadLogFile(string fileName, bool isFundAccountLog, DateTime createdDate)
        {
            var file = new FileInfo(string.Format("{0}\\{1}\\{2}\\{3}", FileSystemManager.OpsSecureBulkFileUploads, isFundAccountLog ? "FundAccount" : "SSITemplate", createdDate.ToString("yyyy-MM-dd"), fileName));
            return DownloadFile(file, fileName);
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