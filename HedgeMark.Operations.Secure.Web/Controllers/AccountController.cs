using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Com.HedgeMark.Commons;
using HedgeMark.Operations.Secure.DataModel;
using HedgeMark.Operations.Secure.DataModel.Models;

namespace HMOSecureWeb.Controllers
{

    public class LoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

    }

    public class AccountController : BaseController
    {

        private static readonly List<string> AllowedDomains = ConfigurationManagerWrapper.StringListSetting("AllowedDomains", "@hedgemark.com,@payoda.com,bnymellon.com,@inautix.co.in");
        public static readonly List<string> AllowedUserRoles = ConfigurationManagerWrapper.StringListSetting("AllowedUserRoles", "DMAUser,DMAAdmin,DMAReviewer,HMDataUser");

        [AllowAnonymous]
        public ActionResult Index(string returnUrl)
        {
            if (Request.IsAuthenticated && AllowedDomains.Any(UserName.EndsWith))
            {
                if (AllowedUserRoles.Any(role => Roles.IsUserInRole(UserName, role)))
                {
                    if (!Utility.Util.IsSiteminder)
                        FormsAuthentication.SetAuthCookie(UserName, true);

                    SetSessionValue("userName", UserName);

                    var userDetails = GetUserDetails(UserName);

                    SetSessionValue("UserDetails", userDetails);
                }
                return RedirectToHome();
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginModel());
        }

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
                        : Roles.IsUserInRole(OpsSecureUserRoles.DmaAdminUser)
                            ? OpsSecureUserRoles.DmaAdminUser
                                : "Unknown"
            };
            return userDetails;
        }

        [HttpGet]
        public ActionResult LogOff()
        {
            SiteMinderLogOff();
            return View();
        }

        private RedirectToRouteResult RedirectToHome()
        {
            return RedirectToAction("Index", "Home");
        }

        private void SiteMinderLogOff()
        {
            string[] cookies = Request.Cookies.AllKeys;
            foreach (string cookie in cookies)
            {
                HttpCookie httpCookie = Response.Cookies[cookie];
                if (httpCookie != null) httpCookie.Expires = DateTime.Now.AddDays(-1);
            }
            if (Request.Cookies["SMSESSION"] != null)
            {
                var smCookie = new HttpCookie("SMSESSION", "NO");
                smCookie.Domain = Utility.Util.Domain;
                smCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(smCookie);
            }
            if (Request.Cookies["SMUSRMSG"] != null)
            {
                var smUsrCookie = new HttpCookie("SMUSRMSG", "NO");
                smUsrCookie.Domain = Utility.Util.Domain;
                smUsrCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(smUsrCookie);
            }

            FormsAuthentication.SignOut();
            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();
        }

    }
}