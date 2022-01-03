using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware;
using HM.Operations.Secure.Middleware.Models;
using HM.Operations.Secure.Middleware.Util;
using log4net;

namespace HM.Operations.Secure.Web.Jobs
{
    public class WireUserListRefresher : OperationsSecureSystemSchedule
    {
        public new const string JobName = "WireUserList-Refresher";

        private static readonly ILog Logger = LogManager.GetLogger(typeof(WireUserListRefresher));

        [DisplayName("WireUserList-Refresher")]
        public static void RefreshWireUserList()
        {
            var allDMAUsers = GetAllDMAUsers();
            var attrbs = new List<string>() { "MELLONECOMMERCEAPPACCESS" };
            var userList = new List<hmsUser>();

            foreach (var user in allDMAUsers.Where(userId => !string.IsNullOrWhiteSpace(userId.CommitId)))
            {
                var result = UmsLibrary.LookupUserByUserId(user.CommitId, attrbs);
                if (result == null)
                    continue;

                var ldapGroups = (result.userAttributes[0].value != null) ? result.userAttributes[0].value.ToList() : new List<string>();

                var ldapRole = string.Empty;

                if (ldapGroups.Contains(OpsSecureUserRoles.WireReadOnly))
                    ldapRole = OpsSecureUserRoles.WireReadOnly;
                else if (ldapGroups.Contains(OpsSecureUserRoles.WireApprover))
                    ldapRole = OpsSecureUserRoles.WireApprover;
                else if (ldapGroups.Contains(OpsSecureUserRoles.WireInitiator))
                    ldapRole = OpsSecureUserRoles.WireInitiator;

                if (string.IsNullOrWhiteSpace(ldapRole))
                    continue;

                user.User.LdapRole = ldapRole;
                user.User.AccountStatus = result.accountStatus;
                userList.Add(user.User);
            }

            using (var context = new OperationsSecureContext())
            {
                var allUserIds = userList.Select(s => s.hmLoginId).Distinct().ToList();
                var allExistingUsers = context.hmsUsers.ToList();
                var missingList = allExistingUsers.Where(s => !allUserIds.Contains(s.hmLoginId)).ToList();
                if (missingList.Any())
                {
                    context.hmsUsers.RemoveRange(missingList);
                    context.SaveChanges();
                }

                context.hmsUsers.AddOrUpdate(user => new { user.hmLoginId }, userList.ToArray());
                context.SaveChanges();
            }
        }

        private static IEnumerable<WireUsers> GetAllDMAUsers()
        {
            List<WireUsers> allDMAUsers;
            using (var context = new AdminContext())
            {
                allDMAUsers = (from aspUser in context.aspnet_Users
                               join usr in context.hLoginRegistrations on aspUser.UserName equals usr.varLoginID
                               join lap in context.LDAPUserDetails on usr.intLoginID equals lap.LoginID
                               where aspUser.aspnet_Roles.Any(r => AuthorizationManager.AuthorizedDmaUserRoles.Contains(r.RoleName)) && !usr.isDeleted
                               select new WireUsers()
                               {
                                   User = new hmsUser()
                                   {
                                       hmLoginId = lap.LoginID,
                                       LdapRole = string.Empty,
                                       AccountStatus = string.Empty
                                   },
                                   CommitId = lap.LDAPUserID
                               }).ToList();
            }

            return allDMAUsers;
        }
    }
}