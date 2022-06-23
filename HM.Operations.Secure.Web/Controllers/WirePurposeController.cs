using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HedgeMark.Operations.FileParseEngine.Models;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware;
using HM.Operations.Secure.Middleware.Util;
using HM.Operations.Secure.Web.Utility;

namespace HM.Operations.Secure.Web.Controllers
{
    public class WirePurposeController : WireUserBaseController
    {
        // GET: WirePurpose
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetWirePurposesForAdhocReport()
        {
            List<hmsWirePurposeLkup> wirePurposes;
            List<hmsWireTransferTypeLKup> wireTransferTypes;
            List<hmsWirePurposeControl> wirePurposeControls;
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                wirePurposes = context.hmsWirePurposeLkups.Where(s => s.ReportName == "Adhoc Report").ToList();
                wireTransferTypes = context.hmsWireTransferTypeLKups.ToList();
                wirePurposeControls = context.hmsWirePurposeControls.ToList();
            }

            var userIds = wirePurposes.Select(s => s.CreatedBy)
                .Union(wirePurposes.Where(s => s.ModifiedBy != null).Select(s => (int)s.ModifiedBy)).ToList();           
            var users = FileSystemManager.GetUsersList(userIds);

            var wireControlRows = new List<Row>();

            foreach (var purpose in wirePurposes)
            {
                var row = new Row
                {
                    ["Report Name"] = purpose.ReportName,
                    ["WirePurposeId"] = purpose.hmsWirePurposeId.ToString(),
                    ["Wire Purpose"] = purpose.Purpose
                };

                var controlOfPurpose = wirePurposeControls.Where(s => s.hmsWirePurposeId == purpose.hmsWirePurposeId).ToList();
                foreach (var transferType in wireTransferTypes)
                {
                    var controlOfPurposeAndType = controlOfPurpose.FirstOrDefault(s => s.WireTransferTypeId == transferType.WireTransferTypeId);
                    row[transferType.TransferType] = controlOfPurposeAndType == null ? "Blocked"
                        : controlOfPurposeAndType.IsApprovalRequested ? "Requested"
                        : controlOfPurposeAndType.IsApproved ? "Approved" : "Blocked";
                }

                row["CreatedBy"] = purpose.CreatedBy == -1 ? "#System" : users.ContainsKey(purpose.CreatedBy) ? users[purpose.CreatedBy] : "Unknown User";
                row["CreatedAt"] = purpose.CreatedAt.ToString("yyyy-MM-dd hh:mm tt");
                //     row["IsApproved"] = purpose.IsApproved ? "Approved" : "Not Approved";
                //     row["ModifiedBy"] = users.ContainsKey(purpose.ModifiedBy ?? 0) ? users[purpose.ModifiedBy ?? 0] : "Unknown User";
                //     row["ModifiedAt"] = purpose.ModifiedAt?.ToString("yyyy-MM-dd hh:mm tt");
                wireControlRows.Add(row);
            }
            return Json(JsonHelper.GetJson(wireControlRows,hiddenHeaders:new List<string>(){ "WirePurposeId" }));
        }

        private class WireControls
        {
            public WireControls()
            {
                WireControl = new hmsWirePurposeControl();
            }
            public hmsWirePurposeControl WireControl { get; set; }
            public string ControlStatus => WireControl.IsApproved ? "Approved" : WireControl.IsApprovalRequested ? "Requested" : "Blocked";
            public bool IsAuthorizedToApprove { get; set; }
            public string WirePurpose { get; set; }
            public string TransferType { get; set; }
            public string RecCreatedBy { get; set; }
            public string LastModifiedBy { get; set; }
        }

        public JsonResult GetWireControl(int wirePurposeId, string wirePurposeName)
        {
            List<hmsWirePurposeControl> wireControls;
            List<hmsWireTransferTypeLKup> transferTypes;
            using (var context = new OperationsSecureContext())
            {
                transferTypes = context.hmsWireTransferTypeLKups.ToList();

                wireControls = context.hmsWirePurposeControls.Include(s => s.hmsWirePurposeLkup)
                   .Where(s => s.hmsWirePurposeId == wirePurposeId).ToList();

            }

            var userIds = wireControls.Select(s => s.LastModifiedById).Union(wireControls.Select(s => s.RecCreatedById)).Distinct().ToList();
            var users = FileSystemManager.GetUsersList(userIds);
            
            users.Add(-1, "#Data Retrofit");
            var wireCtrls = new List<WireControls>();
            foreach (var transferType in transferTypes)
            {
                var wireCtrl = new WireControls()
                {
                    WireControl = new hmsWirePurposeControl()
                    {
                        WireTransferTypeId = transferType.WireTransferTypeId,
                        hmsWirePurposeId = wirePurposeId,
                        IsApproved = false,
                        IsApprovalRequested = false,
                    },

                    TransferType = transferType.TransferType,
                    WirePurpose = wirePurposeName,
                    RecCreatedBy = "-",
                    LastModifiedBy = "-"
                };

                var thisCtrl = wireControls.FirstOrDefault(s => s.WireTransferTypeId == transferType.WireTransferTypeId);

                if (thisCtrl != null)
                {
                    wireCtrl.WireControl = thisCtrl;
                    wireCtrl.RecCreatedBy = users.ContainsKey(thisCtrl.RecCreatedById) ? users[thisCtrl.RecCreatedById] : "-";
                    wireCtrl.LastModifiedBy = users.ContainsKey(thisCtrl.LastModifiedById) ? users[thisCtrl.LastModifiedById] : "-";
                    wireCtrl.IsAuthorizedToApprove = thisCtrl.IsApprovalRequested && thisCtrl.LastModifiedById != UserId;
                }

                if (wireCtrl.RecCreatedBy == wireCtrl.LastModifiedBy)
                {
                    wireCtrl.LastModifiedBy = "-";
                    wireCtrl.WireControl.LastModifiedAt = new DateTime();
                }

                wireCtrl.WireControl.hmsWirePurposeLkup = null;
                wireCtrl.WireControl.hmsWireTransferTypeLKup = null;

                wireCtrls.Add(wireCtrl);

            }

            return Json(wireCtrls);
        }

        public JsonResult GetWirePurposesForOtherReports()
        {
            List<hmsWirePurposeLkup> wirePurposes;
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                wirePurposes = context.hmsWirePurposeLkups.Where(s => s.ReportName != "Adhoc Report").ToList();
            }

            var allPurposes = wirePurposes.Select(wirePurpose =>
                new
                {
                    hmsWirePurposeId = wirePurpose.hmsWirePurposeId,
                    ReportName = wirePurpose.ReportName,
                    Purpose = wirePurpose.Purpose,
                });

            return Json(allPurposes);

        }

        public void AddWirePurpose(string reportName, string purpose)
        {
            using (var context = new OperationsSecureContext())
            {
                var wirePurpose = new hmsWirePurposeLkup()
                {
                    ReportName = reportName,
                    Purpose = purpose,
                    CreatedBy = UserDetails.Id,
                    CreatedAt = DateTime.Now,
                    IsApproved = true
                };

                context.hmsWirePurposeLkups.Add(wirePurpose);
                context.SaveChanges();
            }
        }

        public void ApproveWirePurposeControl(int wirePurposeId, int transferTypeId)
        {
            using (var context = new OperationsSecureContext())
            {
                var wireControl = context.hmsWirePurposeControls.FirstOrDefault(s => s.hmsWirePurposeId == wirePurposeId && s.WireTransferTypeId == transferTypeId);

                if (wireControl == null)
                    return;


                if (wireControl.RecCreatedById == UserId)
                    throw new InvalidOperationException("Same user who requested cannot approve this control");

                wireControl.IsApproved = true;
                wireControl.IsApprovalRequested = false;
                wireControl.LastModifiedAt = DateTime.Now;
                wireControl.LastModifiedById = UserId;

                context.SaveChanges();
            }
        }


        public void BlockWirePurposeControl(int wirePurposeId, int transferTypeId)
        {
            using (var context = new OperationsSecureContext())
            {
                var wireControl = context.hmsWirePurposeControls.FirstOrDefault(s => s.hmsWirePurposeId == wirePurposeId && s.WireTransferTypeId == transferTypeId);

                if (wireControl == null)
                    return;

                wireControl.IsApproved = false;
                wireControl.IsApprovalRequested = false;
                wireControl.LastModifiedAt = DateTime.Now;
                wireControl.LastModifiedById = UserId;

                context.SaveChanges();
            }
        }

        public void RequestWirePurposeControl(int wirePurposeId, int transferTypeId)
        {
            using (var context = new OperationsSecureContext())
            {
                var wireControl = context.hmsWirePurposeControls.FirstOrDefault(s => s.hmsWirePurposeId == wirePurposeId && s.WireTransferTypeId == transferTypeId) ?? new hmsWirePurposeControl()
                {
                    IsApprovalRequested = true,
                    hmsWirePurposeId = wirePurposeId,
                    WireTransferTypeId = transferTypeId,
                    IsApproved = false,
                };

                wireControl.LastModifiedAt = DateTime.Now;
                wireControl.LastModifiedById = UserId;
                wireControl.RecCreatedAt = DateTime.Now;
                wireControl.RecCreatedById = UserId;
                wireControl.IsApproved = false;
                wireControl.IsApprovalRequested = true;

                context.hmsWirePurposeControls.AddOrUpdate(wireControl);
                context.SaveChanges();
            }
        }

        public JsonResult GetApprovedPurposes(int transferTypeId)
        {
            using (var context = new OperationsSecureContext())
            {
                var wirePurposes = context.hmsWirePurposeControls.Where(s => s.WireTransferTypeId == transferTypeId && s.IsApproved && s.hmsWirePurposeLkup.ReportName == ReportName.AdhocWireReport)
                    .Select(s => new { id = s.hmsWirePurposeId, text = s.hmsWirePurposeLkup.Purpose }).OrderBy(s=>s.text).ToList();
                return Json(wirePurposes);
            }
        }
    }
}