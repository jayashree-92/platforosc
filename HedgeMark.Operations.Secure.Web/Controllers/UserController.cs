using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                initiatorCount = context.hmsWires.Where(s => s.WireStatusId != (int)WireDataManager.WireStatus.Cancelled && s.WireStatusId != (int)WireDataManager.WireStatus.Failed).GroupBy(s => s.CreatedBy).ToDictionary(s => s.Key, v => v.Count());
                approvedCount = context.hmsWires.Where(s => s.ApprovedBy != null && s.WireStatusId != (int)WireDataManager.WireStatus.Cancelled && s.WireStatusId != (int)WireDataManager.WireStatus.Failed).GroupBy(s => s.ApprovedBy).ToDictionary(s => s.Key.ToInt(), v => v.Count());
                lastAccessedOnMap = context.hmsWires.GroupBy(s => s.LastUpdatedBy).ToDictionary(s => s.Key, v => v.Max(s1 => s1.LastModifiedAt));
            }

            Dictionary<int, string> userEmailMap;
            var allUserIds = users.Select(s => s.hmLoginId).Union(users.Select(s => s.CreatedBy).Union(users.Where(s => s.ApprovedBy != null).Select(s => (int)s.ApprovedBy))).Distinct().ToList();

            using (var context = new AdminContext())
            {
                userEmailMap = context.hLoginRegistrations.Where(s => allUserIds.Contains(s.intLoginID)).ToDictionary(s => s.intLoginID, v => v.varLoginID.HumanizeEmail());
                //var userGroup = context.onb
            }

            var allWireUsers = new List<WireUsers>();
            foreach (var thisUser in from hmsUser in users
                                     let loginId = hmsUser.hmLoginId
                                     select new WireUsers
                                     {
                                         User = hmsUser,
                                         Email = userEmailMap.ContainsKey(loginId) ? userEmailMap[loginId] : string.Format("Unknown-user-{0}", loginId),
                                         UserGroup = "",//hmsUser.hmsUserGroup.GroupName,
                                         AuthorizationCode = UserAuthorizationCode.AuthorizedToHandleAllWires,
                                         LastAccessedOn = lastAccessedOnMap.ContainsKey(loginId) ? lastAccessedOnMap[loginId] : new DateTime(),
                                         TotalWiresApproved = approvedCount.ContainsKey(loginId) ? approvedCount[loginId] : 0,
                                         TotalWiresInitiated = initiatorCount.ContainsKey(loginId) ? initiatorCount[loginId] : 0,
                                         CreatedBy = userEmailMap.ContainsKey(hmsUser.CreatedBy) ? userEmailMap[hmsUser.CreatedBy] : string.Format("Unknown-user-{0}", hmsUser.CreatedBy),
                                         ApprovedBy = hmsUser.ApprovedBy == null ? "-" : userEmailMap.ContainsKey(hmsUser.ApprovedBy ?? 0) ? userEmailMap[hmsUser.ApprovedBy ?? 0] : string.Format("Unknown-user-{0}", hmsUser.CreatedBy),
                                     })
            {
                allWireUsers.Add(thisUser);
            }

            return Json(allWireUsers);
        }
    }
}