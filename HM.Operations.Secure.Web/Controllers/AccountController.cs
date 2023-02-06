using Com.HedgeMark.Commons;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.DataModel.Models;
using HM.Operations.Secure.Middleware;
using log4net;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace HM.Operations.Secure.Web.Controllers
{
    public class AccountController : Controller
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AccountController));
        public static readonly List<string> AllowedUserRoles = ConfigurationManagerWrapper.StringListSetting("AllowedUserRoles", $"{OpsSecureUserRoles.WireInitiator},{OpsSecureUserRoles.WireApprover},{OpsSecureUserRoles.WireAdmin},{OpsSecureUserRoles.WireReadOnly}");

        public static hLoginRegistration GetUserDetail(string userName)
        {
            using var context = new AdminContext();
            return context.hLoginRegistrations.Single(s => s.varLoginID.Equals(userName));
        }

        public ActionResult Index()
        {
            if(!Request.IsAuthenticated)
            {
                return View();
            }
            else
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie != null)
                    FormsAuthentication.SignOut();
            }
            if(!Utility.Util.IsWireUser(User))
            {
                ViewBag.errorMsg = "Unauthorized User";
                return View();
            }

            return Redirect(new UrlHelper(Request.RequestContext).Action("Index", "Home"));
        }


        public void Login()
        {
            if(!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/Account/Index" },
                      OpenIdConnectAuthenticationDefaults.AuthenticationType);

                Redirect(new UrlHelper(Request.RequestContext).Action("Index", "Account"));
            }
            else
            {
                Response.Redirect(new UrlHelper(Request.RequestContext).Action("Index", "Home"));
            }
        }

        public static Dictionary<string, string> AuthorizeRoleObjectMap = new Dictionary<string, string>()
        {
             { OpsSecureUserRoles.WireAdmin , ConfigurationManagerWrapper.GetAzureConfig("HM-Wire-Admin-Role-Id")},
             { OpsSecureUserRoles.WireApprover ,ConfigurationManagerWrapper.GetAzureConfig("HM-Wire-Approver-Role-Id")},
             { OpsSecureUserRoles.WireInitiator, ConfigurationManagerWrapper.GetAzureConfig("HM-Wire-Initiator-Role-Id")},
             { OpsSecureUserRoles.WireReadOnly, ConfigurationManagerWrapper.GetAzureConfig("HM-Wire-Readonly-Role-Id")},
        };

        public static UserAccountDetails GetUserDetails(IPrincipal user)
        {
            var userDetail = GetUserDetailByCommitId(user.Identity.Name);


            var userDetails = new UserAccountDetails
            {
                User = userDetail,
                Name = userDetail.Name,
                Role = ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AuthorizeRoleObjectMap[OpsSecureUserRoles.WireReadOnly])
                    ? OpsSecureUserRoles.WireReadOnly
                : ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AuthorizeRoleObjectMap[OpsSecureUserRoles.WireApprover])
                    ? OpsSecureUserRoles.WireApprover
                    : ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AuthorizeRoleObjectMap[OpsSecureUserRoles.WireInitiator])
                        ? OpsSecureUserRoles.WireInitiator
                            : ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AuthorizeRoleObjectMap[OpsSecureUserRoles.WireAdmin])
                            ? OpsSecureUserRoles.WireAdmin
                                : "Unknown"
            };
            return userDetails;
        }

        [AllowAnonymous]
        public ActionResult LogOff(string reasonStr)
        {
            Logger.Debug("Logging off- Clearing Cookie and Authentication with name:" + FormsAuthentication.FormsCookieName);


            var auditData = new hmsUserAuditLog
            {
                Action = "Logged Out",
                Module = "Account",
                Log = "Signed out from Secure System",
                CreatedAt = DateTime.Now,
                UserName = User.Identity.Name
            };
            AuditManager.LogAudit(auditData);

            // Delete the user details from Session.
            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();

            // Delete the authentication ticket and sign out.
            FormsAuthentication.SignOut();

            // Clear authentication cookie.
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, "") { Expires = DateTime.Now.AddYears(-30) };
            Response.Cookies.Add(cookie);

            ViewBag.ReasonString = reasonStr ?? string.Empty;
            HttpContext.GetOwinContext().Authentication.SignOut(OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);

            return View();
        }

        //public static HMUser GetUserDetailByCommitId(string commitId)
        //{
        //    using(var context = new AdminContext())
        //    {
        //        var userDetail = context.USP_NEXEN_GetUserDetails(commitId, "SITEMINDER").Select(s => new HMUser()
        //        {
        //            LoginId = s.intLoginID,
        //            Name = s.varLoginID,
        //            CommitId = s.LDAPUserID
        //        }).First();

        //        SetAllowedWireAmountLimit(userDetail);
        //        return userDetail;

        //    }
        //}

        public static HMUser GetUserDetailByCommitId(string email)
        {
            using var context = new AdminContext();
            var userDetail = context.hLoginRegistrations.Where(s => s.varLoginID == email).Select(s => new HMUser()
            {
                LoginId = s.intLoginID,
                Name = s.varLoginID,
            }).FirstOrDefault();
            SetAllowedWireAmountLimit(userDetail);
            return userDetail;
        }

        public static double GetTotalYearsOfExperience(string commitId)
        {
            var userDetails = GetUserCoverageDetails(commitId);

            var totalExperienceInHM = (DateTime.Today - userDetails.JoinedHedgemarkOn).TotalDays / 365;
            return Math.Round(userDetails.TotalYearsofExperiencePriortoHedgemark + totalExperienceInHM, 1);
        }

        private static vw_hmUserCoverageDetails GetUserCoverageDetails(string commitId)
        {
            vw_hmUserCoverageDetails userDetails;
            using var context = new AdminContext();
            userDetails = context.vw_hmUserCoverageDetails.FirstOrDefault(s => s.CommitId == commitId) ?? new vw_hmUserCoverageDetails()
            {
                CommitId = commitId,
                JoinedHedgemarkOn = DateTime.Today,
                TotalYearsofExperiencePriortoHedgemark = 0
            };

            return userDetails;
        }

        public static void SetAllowedWireAmountLimit(HMUser user)
        {
            var userDetails = GetUserCoverageDetails(user.Name);

            var totalExperienceInHM = (DateTime.Today - userDetails.JoinedHedgemarkOn).TotalDays / 365;
            user.TotalYearsOfExperienceInHM = Math.Round(totalExperienceInHM, 1);
            user.TotalYearsOfExperience = Math.Round(userDetails.TotalYearsofExperiencePriortoHedgemark + totalExperienceInHM, 1);

            //---->>>>
            user.IsUserVp = userDetails.IsVPAndAbove;

            if(user.TotalYearsOfExperience < 1)
                user.AllowedWireAmountLimit = 10000000;
            else if(totalExperienceInHM <= 0.5)
                user.AllowedWireAmountLimit = user.IsUserVp ? 100000000 : 10000000;
            else
                user.AllowedWireAmountLimit = user.IsUserVp ? 500000000 : 100000000;

        }
    }
}