﻿using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using Com.HedgeMark.Commons;
using HMOSecureMiddleware;
using HMOSecureWeb.Controllers;
using HMOSecureWeb.Utility;
using log4net;
using log4net.Config;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace HMOSecureWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MvcApplication));
        private static readonly string SiteMinderLogOffUrl = ConfigurationManagerWrapper.StringSetting("SiteMinderLogoffUri", "/Account/LogOff");
        private static readonly int GlobalSessionTimeOut = ConfigurationManagerWrapper.IntegerSetting("GlobalSessionTimeOut", 20);
        private const string SiteMinderHeaderToken = "SMUSER";

        protected void Application_Start()
        {
            //Log4Net Instantiator
            XmlConfigurator.ConfigureAndWatch(new FileInfo(string.Format("{0}web.config", AppDomain.CurrentDomain.BaseDirectory)));

            //Security measure - hide server details in the response header
            MvcHandler.DisableMvcResponseHeader = true;

            AppDomain.CurrentDomain.UnhandledException += GlobalUnhandledException;

            AreaRegistration.RegisterAllAreas();
            BundleTable.Bundles.ResetAll();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Boot up required assemblies to middleware
            BootUpMiddleware.BootUp();

        }
        void GlobalUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error(e.ExceptionObject);
        }

        protected void Application_PostAuthorizeRequest()
        {
            if (IsWebApiRequest())
            {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
        }

        private static bool IsWebApiRequest()
        {
            return HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath != null && HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/api");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Response.Filter.Dispose();
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            if (HttpContext.Current.Request.Url.AbsolutePath == SiteMinderLogOffUrl)
                return;

            if (IsAlreadyAuthenticated())
                return;

            Logger.Debug("New User Logging into the system - so Checking for Authorization");

            var smUserId = HttpContext.Current.Request.Headers[SiteMinderHeaderToken];
            var roles = new List<string>();
            if (HttpContext.Current.Request.IsLocal)
            {
                smUserId = ConfigurationManager.AppSettings["LocalSiteMinderCommitId"];
                roles.Add(OpsSecureUserRoles.WireApprover);
            }

            var userSso = AccountController.GetEmailOfLdapUserId(smUserId);
            if (userSso == null)
            {
                Logger.Error(string.Format("access denied to user '{0}', user not registered", smUserId));
                SiteMinderLogOff("access denied, user not registered");
                return;
            }

            var email = userSso.varLoginID;

            if (AccountController.AllowedDomains.All(domain => !email.EndsWith(domain, StringComparison.InvariantCultureIgnoreCase)))
            {
                Logger.Error(string.Format("access denied to user '{0}', invalid user domain", email));
                SiteMinderLogOff("User domain is invalid/not authorized");
                return;
            }


            //---------------------------
            //Aspnet roles will be re-placed by LDAP roles
            //if (AccountController.AllowedUserRoles.All(role => !Roles.IsUserInRole(email, role)))
            //{
            //    Logger.Error(string.Format("access denied to user '{0}', unauthorized", email));
            //    SiteMinderLogOff("User not authorized");
            //    return;
            //}

            //Overriding Role - until LDAP groups can be identified
            roles.Add(OpsSecureUserRoles.WireApprover);

            //---------------------------

            var webIdentity = new GenericIdentity(email, "SiteMinder");
            var principal = new GenericPrincipal(webIdentity, roles.ToArray());
            HttpContext.Current.User = principal;
            Thread.CurrentPrincipal = principal;
            var userData = string.Join(",", roles);
            var ticket = new FormsAuthenticationTicket(1, email, DateTime.Now, DateTime.Now.AddMinutes(GlobalSessionTimeOut), true, userData, FormsAuthentication.FormsCookiePath);
            var encTicket = FormsAuthentication.Encrypt(ticket);
            var formCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket) { Domain = Utility.Util.Domain };
            Response.Cookies.Add(formCookie);
        }

        private bool IsAlreadyAuthenticated()
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null)
                return false;

            FormsAuthenticationTicket authTicket = null;
            authTicket = FormsAuthentication.Decrypt(authCookie.Value);

            if (authTicket == null || authTicket.Expired)
                return false;

            if (FormsAuthentication.SlidingExpiration)
                authTicket = FormsAuthentication.RenewTicketIfOld(authTicket);

            if (authTicket == null)
                return false;

            var webIdentity = new GenericIdentity(authTicket.Name, "SiteMinder");
            var principal = new GenericPrincipal(webIdentity, authTicket.UserData.Split(','));
            HttpContext.Current.User = principal;
            Thread.CurrentPrincipal = principal;
            return true;
        }

        void Session_Start(object sender, EventArgs e)
        {
            Session.Timeout = GlobalSessionTimeOut;
            Session["SessionStartTime"] = DateTime.Now;
            //if (!ActiveUsers.Contains(User.Identity.Name))
            //{
            //    ActiveUsers.Add(User.Identity.Name);
            //    hmsUserAuditLog auditData = new hmsUserAuditLog
            //    {
            //        Action = "Logged In",
            //        Module = "Account",
            //        Log = "Signed into Secure System",
            //        CreatedAt = DateTime.Now,
            //        UserName = User.Identity.Name
            //    };
            //    AuditManager.LogAudit(auditData);
            //}

            Session["userName"] = User.Identity.Name;
        }

        public static ConcurrentBag<string> ActiveUsers = new ConcurrentBag<string>();

        public void Application_End(object sender, EventArgs e)
        {
            //JobStorage.Current.GetMonitoringApi().PurgeJobs();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpContext.Current.Response.AddHeader("X-Frame-Options", "DENY");
            HttpContext.Current.Response.Headers.Remove("Server");
            HttpContext.Current.Response.Headers.Remove("X-AspNet-Version");
            HttpContext.Current.Response.Headers.Remove("X-AspNetMvc-Version");
        }

        void Session_End(object sender, EventArgs e)
        {
            //var userName = Session["userName"] != null ? Session["userName"].ToString() : string.Empty;

            string userNameOut;
            ActiveUsers.TryTake(out userNameOut);

            // Delete the user details from Session.
            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();

            // Delete the authentication ticket and sign out.
            //FormsAuthentication.SignOut();

            // Clear authentication cookie.
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, "") { Expires = DateTime.Now.AddYears(-1) };
            Response.Cookies.Add(cookie);


            //hmsUserAuditLog auditData = new hmsUserAuditLog
            //{
            //    Action = "Log Out",
            //    Module = "Account",
            //    Log = "Signed out from Secure System",
            //    CreatedAt = DateTime.Now,
            //    UserName = User.Identity.Name
            //};
            //AuditManager.LogAudit(auditData);

        }

        private void SiteMinderLogOff(string reasonStr)
        {
            Response.StatusCode = 303;
            Response.Redirect(string.Format("{0}?reasonStr={1}", SiteMinderLogOffUrl, reasonStr), false);
        }
    }
}
