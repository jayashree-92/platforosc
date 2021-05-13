using System.Collections.Generic;
using System.Linq;
using HedgeMark.Operations.Secure.DataModel;
using HedgeMark.Operations.Secure.Middleware;
using HedgeMark.Operations.Secure.Middleware.Util;
using HedgeMark.SwiftMessageHandler.Tests.TestExtensions;
using Kent.Boogaart.KBCsv;
using NUnit.Framework;

namespace HedgeMark.SwiftMessageHandler.Tests
{
    [TestFixture]
    public class GeneralSettingsTest
    {

        public class WireUsers
        {
            public int LoginId { get; set; }
            public string CommitId { get; set; }
            public string UserId { get; set; }
            public string LdapRole { get; set; }
        }

        //[Test]
        public void GetUser()
        {
            List<WireUsers> allDMAUsers;
            using (var context = new AdminContext())
            {
                allDMAUsers = (from aspUser in context.aspnet_Users
                               join usr in context.hLoginRegistrations on aspUser.UserName equals usr.varLoginID
                               join lap in context.LDAPUserDetails on usr.intLoginID equals lap.LoginID
                               where aspUser.aspnet_Roles.Any(r => AuthorizationManager.AuthorizedDmaUserRoles.Contains(r.RoleName)) && !usr.isDeleted
                               select new WireUsers { LoginId = lap.LoginID, UserId = usr.varLoginID, CommitId = lap.LDAPUserID }).ToList();
            }


            using (var writer = new CsvWriter(TestUtility.AssemblyDirectory + "\\" + "UserRoleMapping-OpsSecure.csv"))
            {
                writer.ValueSeparator = ',';
                writer.WriteHeaderRecord(new List<object>() { "LoginId", "CommitId", "UserId", "LdapRole" });


                foreach (var userId in allDMAUsers.Where(userId => !string.IsNullOrWhiteSpace(userId.CommitId)))
                {
                    var userRoleMap = new List<string> { userId.LoginId.ToString(), userId.CommitId, userId.UserId };

                    var ldapGroups = new UmsLibrary().GetLdapGroupsOfLdapUser(userId.CommitId);

                    var ldapRole = string.Empty;

                    if (ldapGroups.Contains(OpsSecureUserRoles.WireAdmin))
                        ldapRole = OpsSecureUserRoles.WireAdmin;
                    else if (ldapGroups.Contains(OpsSecureUserRoles.WireApprover))
                        ldapRole = OpsSecureUserRoles.WireApprover;
                    else if (ldapGroups.Contains(OpsSecureUserRoles.WireInitiator))
                        ldapRole = OpsSecureUserRoles.WireInitiator;

                    if (string.IsNullOrWhiteSpace(ldapRole))
                        continue;

                    userRoleMap.Add(ldapRole);
                    writer.WriteDataRecords(new List<string[]> { userRoleMap.ToArray() });
                }

                writer.Flush();
                writer.Close();

            }
        }
    }
}
