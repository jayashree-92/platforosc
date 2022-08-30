using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Com.HedgeMark.Commons;
using Com.HedgeMark.Commons.Extensions;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.DataModel.Models;
using HM.Operations.Secure.Middleware;
using HM.Operations.Secure.Web.Utility;
using log4net;

namespace HM.Operations.Secure.Web.Controllers
{
    public class AccountController : BaseController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AccountController));
        public static readonly List<string> AllowedUserRoles = ConfigurationManagerWrapper.StringListSetting("AllowedUserRoles", $"{OpsSecureUserRoles.WireInitiator},{OpsSecureUserRoles.WireApprover},{OpsSecureUserRoles.WireAdmin},{OpsSecureUserRoles.WireReadOnly}");

        public static hLoginRegistration GetUserDetail(string userName)
        {
            using (var context = new AdminContext())
            {
                return context.hLoginRegistrations.Single(s => s.varLoginID.Equals(userName));
            }
        }

        public ActionResult Index(string returnUrl)
        {
            return RedirectToAction("Index", User.IsWireAdmin() ? "User" : "Home");
        }


        public static UserAccountDetails GetUserDetails(string commitId, IPrincipal user)
        {
            var userDetail = GetUserDetailByCommitId(commitId);
            var userDetails = new UserAccountDetails
            {
                User = userDetail,
                Name = userDetail.Name,
                Role = user.IsInRole(OpsSecureUserRoles.WireReadOnly)
                    ? OpsSecureUserRoles.WireReadOnly
                : user.IsInRole(OpsSecureUserRoles.WireApprover)
                    ? OpsSecureUserRoles.WireApprover
                    : user.IsInRole(OpsSecureUserRoles.WireInitiator)
                        ? OpsSecureUserRoles.WireInitiator
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

            //Clear Site-Minder Cookie
            ClearSiteMinder();

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

            return View();
        }

        private void ClearSiteMinder()
        {
            if (Request.Cookies["SMSESSION"] != null)
            {
                //var smCookie = new HttpCookie("SMSESSION", "NO")
                //{
                //    Domain = Utility.Util.Domain,
                //    Expires = DateTime.Now.AddDays(-30),
                //    Value = "LOGGEDOFF"
                //};
                //Response.Cookies.Add(smCookie);
                Response.Cookies.Clear();
            }
            if (Request.Cookies["SMUSRMSG"] != null)
            {
                var smUsrCookie = new HttpCookie("SMUSRMSG", "NO")
                {
                    Domain = Utility.Util.Domain,
                    Expires = DateTime.Now.AddDays(-30)
                };
                Response.Cookies.Add(smUsrCookie);
            }
        }

        public static HMUser GetUserDetailByCommitId(string commitId)
        {
            using (var context = new AdminContext())
            {
                var userDetail = context.USP_NEXEN_GetUserDetails(commitId, "SITEMINDER").Select(s => new HMUser()
                {
                    LoginId = s.intLoginID,
                    Name = s.varLoginID,
                    CommitId = s.LDAPUserID
                }).First();

                SetAllowedWireAmountLimit(userDetail);
                return userDetail;

            }
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
            using (var context = new AdminContext())
            {
                userDetails = context.vw_hmUserCoverageDetails.FirstOrDefault(s => s.CommitId == commitId) ?? new vw_hmUserCoverageDetails()
                {
                    CommitId = commitId,
                    JoinedHedgemarkOn = DateTime.Today,
                    TotalYearsofExperiencePriortoHedgemark = 0
                };
            }

            return userDetails;
        }

        public static void SetAllowedWireAmountLimit(HMUser user)
        {
            var userDetails = GetUserCoverageDetails(user.CommitId);

            var totalExperienceInHM = (DateTime.Today - userDetails.JoinedHedgemarkOn).TotalDays / 365;
            user.TotalYearsOfExperienceInHM = Math.Round(totalExperienceInHM, 1);
            user.TotalYearsOfExperience = Math.Round(userDetails.TotalYearsofExperiencePriortoHedgemark + totalExperienceInHM, 1);

            //---->>>>
            user.IsUserVp = userDetails.IsVPAndAbove;

            if (user.TotalYearsOfExperience < 1)
                user.AllowedWireAmountLimit = 10000000;
            else if (totalExperienceInHM <= 0.5)
                user.AllowedWireAmountLimit = user.IsUserVp ? 100000000 : 10000000;
            else
                user.AllowedWireAmountLimit = user.IsUserVp ? 500000000 : 100000000;

        }
    }
}