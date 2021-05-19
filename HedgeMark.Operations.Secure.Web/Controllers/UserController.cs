using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.Operations.Secure.DataModel;
using HedgeMark.Operations.Secure.Middleware;
using HedgeMark.Operations.Secure.Middleware.Models;
using HedgeMark.Operations.Secure.Middleware.Util;

namespace HMOSecureWeb.Controllers
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
                lastAccessedOnMap = allWires.GroupBy(s => s.LastUpdatedBy).ToDictionary(s => s.Key, v => v.Max(s1 => s1.LastModifiedAt));
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
                                    Email = userEmailMap.ContainsKey(loginId) ? userEmailMap[loginId] : string.Format("Unknown-user-{0}", loginId),
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
            var allWireUsers = (List<WireUsers>)GetSessionValue(OpsSecureSessionVars.WiresDashboardData.ToString());
            var fileName = "HMAuthTrasnfer - " + DateTime.Today.ToString("MM.dd.yyyy") + groupOption + ".pdf";
            var exportFileInfo = new FileInfo(string.Format("{0}{1}", FileSystemManager.UploadTemporaryFilesPath, fileName));


            if (groupOption == "Group_A_only")
            {
                allWireUsers = allWireUsers.Where(s => s.Role == "hm-wire-approver").ToList();
            }
            else if (groupOption == "Group_B_Only")
            {
                allWireUsers = allWireUsers.Where(s => s.Role == "hm-wire-initiator").ToList();
            }
            //Create PDF Files using allWireUsers

            return DownloadAndDeleteFile(exportFileInfo);
        }
    }
}