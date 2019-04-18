using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Com.HedgeMark.Commons;
using HedgeMark.Operations.Secure.DataModel;
using HedgeMark.Operations.Secure.DataModel.Models;

namespace HMOSecureWeb.Controllers
{
    public static class OpsSecureUserRoles
    {
        public const string DmaWireInitiator = "DMAWireInitiator";
        public const string DmaWireApprover = "DMAWireApprover";
    }

    public class AccountController : BaseController
    {
        public static readonly List<string> AllowedDomains = ConfigurationManagerWrapper.StringListSetting("AllowedDomains", "@hedgemark.com,@payoda.com,bnymellon.com,@inautix.co.in");
        public static readonly List<string> AllowedUserRoles = ConfigurationManagerWrapper.StringListSetting("AllowedUserRoles", "HM-Wire-Initiator,HM-Wire-Approver");

        public static hLoginRegistration GetUserDetail(string userName)
        {
            using (var context = new AdminContext())
            {
                return context.hLoginRegistrations.Single(s => s.varLoginID.Equals(userName));
            }
        }

        public static UserAccountDetails GetUserDetails(string userName)
        {
            var userDetails = new UserAccountDetails
            {
                User = GetUserDetail(userName),
                Name = userName,
                Role = Roles.IsUserInRole(OpsSecureUserRoles.DmaWireApprover)
                    ? OpsSecureUserRoles.DmaWireApprover
                    : Roles.IsUserInRole(OpsSecureUserRoles.DmaWireInitiator)
                        ? OpsSecureUserRoles.DmaWireInitiator
                                : "Unknown"
            };
            return userDetails;
        }

        [AllowAnonymous]
        public ActionResult LogOff(string reasonStr)
        {
            FormsAuthentication.SignOut();

            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();

            ViewBag.ReasonString = reasonStr;
            return View();
        }

        public static USP_NEXEN_GetUserDetails_Result GetEmailOfLdapUserId(string userName)
        {
            using (var context = new AdminContext())
            {
                return context.USP_NEXEN_GetUserDetails(userName, "SITEMINDER").FirstOrDefault();
            }
        }
    }
}