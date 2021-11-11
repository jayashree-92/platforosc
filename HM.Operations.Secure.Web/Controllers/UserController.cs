using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Com.HedgeMark.Commons;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.Secrets.Management.Services;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware;
using HM.Operations.Secure.Middleware.Models;
using HM.Operations.Secure.Middleware.Util;
using HM.Operations.Secure.Web.Jobs;
using PDFUtility.Operations.ManagedAccounts;

namespace HM.Operations.Secure.Web.Controllers
{
    public class UserController : WireAdminBaseController
    {
        // GET: UserOperations
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetWireUsers()
        {
            List<hmsUser> users;
            Dictionary<int, int> initiatorCount, approvedCount;
            Dictionary<int, DateTime> lastAccessedOnMap;

            using (var context = new OperationsSecureContext())
            {
                users = context.hmsUsers.ToList();
                var allWires = context.hmsWires.Select(s => new { s.WireStatusId, s.CreatedBy, s.ApprovedBy, s.LastUpdatedBy, s.LastModifiedAt }).ToList();
                initiatorCount = allWires.Where(s => s.WireStatusId != (int)WireDataManager.WireStatus.Cancelled && s.WireStatusId != (int)WireDataManager.WireStatus.Failed).GroupBy(s => s.CreatedBy).ToDictionary(s => s.Key, v => v.Count());
                approvedCount = allWires.Where(s => s.ApprovedBy != null && s.WireStatusId != (int)WireDataManager.WireStatus.Cancelled && s.WireStatusId != (int)WireDataManager.WireStatus.Failed).GroupBy(s => s.ApprovedBy).ToDictionary(s => s.Key.ToInt(), v => v.Count());
                lastAccessedOnMap = allWires.GroupBy(s => s.LastUpdatedBy).ToDictionary(s => s.Key, v => v.Max(s1 => s1.LastModifiedAt.DateTime));
            }

            Dictionary<int, string> userEmailMap, userGroupMap;
            var allUserIds = users.Select(s => s.hmLoginId).Distinct().ToList();

            using (var context = new AdminContext())
            {
                userEmailMap = context.hLoginRegistrations.Where(s => allUserIds.Contains(s.intLoginID)).ToDictionary(s => s.intLoginID, v => v.varLoginID.HumanizeEmail());
                userGroupMap = context.onBoardingAssignmentUserGroupMaps.Include(s => s.onBoardingAssignmentUserGroup).Where(s => s.onBoardingAssignmentUserGroup.IsActive && s.onBoardingAssignmentUserGroup.IsPrimaryGroup && allUserIds.Contains(s.UserId)).GroupBy(s => s.UserId).ToDictionary(s => s.Key, v => v.Select(s1 => s1.onBoardingAssignmentUserGroup.GroupDescription).FirstOrDefault());
            }

            var allWireUsers = (from hmsUser in users
                                let loginId = hmsUser.hmLoginId
                                select new WireUsers
                                {
                                    User = hmsUser,
                                    Email = userEmailMap.ContainsKey(loginId) ? userEmailMap[loginId] : $"Unknown-user-{loginId}",
                                    UserGroup = userGroupMap.ContainsKey(loginId) ? userGroupMap[loginId] : "-Un-categorized User-",
                                    AuthorizationCode = UserAuthorizationCode.AuthorizedToHandleAllWires,
                                    LastAccessedOn = lastAccessedOnMap.ContainsKey(loginId) ? lastAccessedOnMap[loginId] : new DateTime(),
                                    TotalWiresApproved = approvedCount.ContainsKey(loginId) ? approvedCount[loginId] : 0,
                                    TotalWiresInitiated = initiatorCount.ContainsKey(loginId) ? initiatorCount[loginId] : 0
                                }).ToList();

            SetSessionValue(OpsSecureSessionVars.WireUserGroupData.ToString(), allWireUsers);

            return Json(allWireUsers);
        }


        public FileResult ExportReport(string groupOption = "All_Groups")
        {
            var auditData = new hmsUserAuditLog
            {
                Action = "Download",
                Module = "User Management",
                Log = groupOption + " Exported by  " + UserName.HumanizeEmail(),
                CreatedAt = DateTime.Now,
                UserName = User.Identity.Name
            };
            AuditManager.LogAudit(auditData);

            var allWireUsers = (List<WireUsers>)GetSessionValue(OpsSecureSessionVars.WireUserGroupData.ToString());
            var fileName = $"HMAuthTransfer_{DateTime.Today:yyyy_MM_dd}_{groupOption}.pdf";
            var exportFileInfo = new FileInfo($"{FileSystemManager.UploadTemporaryFilesPath}{fileName}");

            switch (groupOption)
            {
                case "Group_A_only":
                    allWireUsers = allWireUsers.Where(s => s.Role == "hm-wire-approver").ToList();
                    break;
                case "Group_B_Only":
                    allWireUsers = allWireUsers.Where(s => s.Role == "hm-wire-initiator").ToList();
                    break;
            }
            var userRows = ConstructWireUserRows(allWireUsers);
            //Create PDF Files using allWireUsers

            var digiSignInfo = new DigitalSignatureInfo()
            {
                PfxFile = new FileInfo(FileSystemManager.OpsSecureInternalConfigFiles + "\\" + ConfigurationManagerWrapper.StringSetting("DigiSignatureFileName", "HedgeMarkOperationsQA.pfx")),
                Password = SecretManagementService.GetGenericSecret("DigiSignaturePassword").Value
            };

            exportFileInfo = SecureExporter.ExportSignedReport(userRows, groupOption, exportFileInfo, digiSignInfo.PfxFile.Exists ? digiSignInfo : null);
            return DownloadAndDeleteFile(exportFileInfo);
        }

        public static List<WireUserExportContent> ConstructWireUserRows(List<WireUsers> wireUsers)
        {
            return wireUsers.Select(user => new WireUserExportContent
            {
                UserName = user.UserName,
                UserGroup = user.UserGroup,
                UserRole = user.User.LdapRole
            }).ToList();

        }

        public void RefreshUserList()
        {
            WireUserListRefresher.RefreshWireUserList();
        }

    }
}