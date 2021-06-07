using System;
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
using HMOSecureWeb.Controllers;
using log4net;
using log4net.Config;
using System.Collections.Generic;
using HedgeMark.Monitoring;
using HedgeMark.Operations.Secure.DataModel;
using HedgeMark.Operations.Secure.Middleware;
using HedgeMark.Operations.Secure.Middleware.Util;

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

            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;

            AppHeartBeat.Start("HM-Operations-Secure", AppMnemonic.DMO, AppType.WebApp, "HMOpsSecureConnectionString");
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

            //Logger.Debug("New User Logging into the system - so Checking for Authorization");

            var smUserId = HttpContext.Current.Request.Headers[SiteMinderHeaderToken];
            var roles = new List<string>();
            if (HttpContext.Current.Request.IsLocal)
            {
                smUserId = ConfigurationManager.AppSettings["LocalSiteMinderCommitId"];
                roles.Add(OpsSecureUserRoles.WireApprover);
            }
            var userSso = AccountController.GetUserDetailByCommitId(smUserId);

            if (userSso == null)
            {
                Logger.Warn(string.Format("access denied to user '{0}', user not registered", smUserId));
                SiteMinderLogOff("access denied, user not registered");
                return;
            }

            var email = userSso.Name;

            if (AccountController.AllowedDomains.All(domain => !email.EndsWith(domain, StringComparison.InvariantCultureIgnoreCase)))
            {
                Logger.Warn(string.Format("access denied to user '{0}', invalid user domain", email));
                SiteMinderLogOff("User domain is invalid/not authorized");
                return;
            }

            if (!(roles.Contains(OpsSecureUserRoles.WireApprover) || roles.Contains(OpsSecureUserRoles.WireInitiator) || roles.Contains(OpsSecureUserRoles.WireAdmin)))
            {
                //Logger.InfoFormat(string.Format("LDAP ID: {0}", userSso.CommitId));

                if (!string.IsNullOrWhiteSpace(userSso.CommitId))
                {
                    var ldapGroups = UmsLibrary.GetLdapGroupsOfLdapUser(userSso.CommitId);
                    if (ldapGroups.Contains(OpsSecureUserRoles.WireApprover))
                        roles.Add(OpsSecureUserRoles.WireApprover);
                    else if (ldapGroups.Contains(OpsSecureUserRoles.WireInitiator))
                        roles.Add(OpsSecureUserRoles.WireInitiator);
                    else if (ldapGroups.Contains(OpsSecureUserRoles.WireAdmin))
                        roles.Add(OpsSecureUserRoles.WireAdmin);
                }
            }

            if (AccountController.AllowedUserRoles.All(role => !roles.Contains(role)))
            {
                Logger.Error(string.Format("access denied to user '{0}', user role not available", email));
                SiteMinderLogOff("User not authorized");
                return;
            }

            //Add ASPNET Role for Entitlements - as they are currently assigned that way. We need to change it when Permission engine release is scheduled
            using (var context = new AdminContext())
            {
                var userRole = (from aspUser in context.aspnet_Users
                                join usr in context.hLoginRegistrations on aspUser.UserName equals usr.varLoginID
                                where usr.intLoginID == userSso.LoginId && aspUser.aspnet_Roles.Any(r => AuthorizationManager.AuthorizedDmaUserRoles.Contains(r.RoleName)) && !usr.isDeleted
                                let role = aspUser.aspnet_Roles.Any(r => r.RoleName == OpsSecureUserRoles.DMAAdmin) ? OpsSecureUserRoles.DMAAdmin : OpsSecureUserRoles.DMAUser
                                select role).FirstOrDefault() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(userRole))
                    roles.Add(userRole);
            }

            var webIdentity = new GenericIdentity(email, "SiteMinder");
            var principal = new GenericPrincipal(webIdentity, roles.ToArray());
            HttpContext.Current.User = principal;
            Thread.CurrentPrincipal = principal;
            var userData = string.Join(",", roles);
            var ticket = new FormsAuthenticationTicket(1, email, DateTime.Now, DateTime.Now.AddMinutes(GlobalSessionTimeOut), true, userData, FormsAuthentication.FormsCookiePath);
            var encTicket = FormsAuthentication.Encrypt(ticket);
            var formCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket) { Domain = Utility.Util.Domain };
            Response.Cookies.Add(formCookie);


            if (ActiveUsers.Contains(User.Identity.Name) || HttpContext.Current.Request.IsLocal)
                return;

            ActiveUsers.Add(User.Identity.Name);
            var auditData = new hmsUserAuditLog
            {
                Action = "Logged In",
                Module = "Account",
                Log = "Signed into Secure System",
                CreatedAt = DateTime.Now,
                UserName = User.Identity.Name
            };
            AuditManager.LogAudit(auditData);
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
            Session["userName"] = User.Identity.Name;
            var smUserId = HttpContext.Current.Request.IsLocal ? ConfigurationManager.AppSettings["LocalSiteMinderCommitId"] : HttpContext.Current.Request.Headers[SiteMinderHeaderToken];
            Session[OpsSecureSessionVars.UserCommitId.ToString()] = smUserId;
        }

        public static ConcurrentBag<string> ActiveUsers = new ConcurrentBag<string>();

        public void Application_End(object sender, EventArgs e)
        {
            //JobStorage.Current.GetMonitoringApi().PurgeJobs();
            AppHeartBeat.Stop();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //HttpContext.Current.Response.AddHeader("X-Frame-Options", "DENY");
            HttpContext.Current.Response.Headers.Remove("Server");
            HttpContext.Current.Response.Headers.Remove("X-AspNet-Version");
            HttpContext.Current.Response.Headers.Remove("X-AspNetMvc-Version");
        }

        void Session_End(object sender, EventArgs e)
        {

            var auditData = new hmsUserAuditLog
            {
                Action = "Logged Out",
                Module = "Account",
                Log = "Signed out from Secure System",
                CreatedAt = DateTime.Now,
                UserName = User.Identity.Name
            };
            AuditManager.LogAudit(auditData);

            //var userName = Session["userName"] != null ? Session["userName"].ToString() : string.Empty;

            //string userNameOut;
            //ActiveUsers.TryTake(out userNameOut);

            // Delete the user details from Session.
            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();

            // Delete the authentication ticket and sign out.
            //FormsAuthentication.SignOut();

            //// Clear authentication cookie.
            //var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, "") { Expires = DateTime.Now.AddYears(-1) };
            //Response.Cookies.Add(cookie);


        }

        private void SiteMinderLogOff(string reasonStr)
        {
            Response.StatusCode = 303;
            Response.Redirect(string.Format("{0}?reasonStr={1}", SiteMinderLogOffUrl, reasonStr), false);
        }
    }
}
