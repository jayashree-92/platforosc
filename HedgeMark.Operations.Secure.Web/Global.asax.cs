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
using HedgeMark.Operations.Secure.DataModel;
using HMOSecureMiddleware;
using HMOSecureWeb.Controllers;
using HMOSecureWeb.Utility;
using log4net;
using log4net.Config;

namespace HMOSecureWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MvcApplication));
        private static readonly string SiteMinderLogOffUrl = ConfigurationManagerWrapper.StringSetting("SiteminderLogoffUri", "/Account/LogOff");
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

            //Boot up requied assemblies to middleware
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

            string email;
            if (HttpContext.Current.Request.IsLocal)
            {
                email = ConfigurationManager.AppSettings["LocalSiteminderUser"];
            }
            else
            {
                var smUserId = HttpContext.Current.Request.Headers[SiteMinderHeaderToken];
                var userSso = AccountController.GetEmailOfLdapUserId(smUserId);
                if (userSso == null)
                {
                    SiteMinderLogOff("access denied, user not registered");
                    return;
                }

                email = userSso.varLoginID;
            }

            if (AccountController.AllowedDomains.All(domain => !email.EndsWith(domain)))
            {
                Logger.Error(string.Format("access denied to user '{0}', invalid user domain", email));
                SiteMinderLogOff("User domain is invalid/not authorized");
                return;
            }

            if (AccountController.AllowedUserRoles.All(role => !Roles.IsUserInRole(email, role)))
            {
                Logger.Error(string.Format("access denied to user '{0}', unauthorized", email));
                SiteMinderLogOff("User not authorized");
                return;
            }

            var webIdentity = new GenericIdentity(email, "SiteMinder");
            var roles = Roles.GetRolesForUser(email);
            var principal = new GenericPrincipal(webIdentity, roles);
            HttpContext.Current.User = principal;
            Thread.CurrentPrincipal = principal;
            var userData = string.Join(",", roles);
            var ticket = new FormsAuthenticationTicket(1, email, DateTime.Now, DateTime.Now.AddDays(30), true, userData, FormsAuthentication.FormsCookiePath);

            var encTicket = FormsAuthentication.Encrypt(ticket);
            var formCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket) { Domain = Utility.Util.Domain };
            HttpContext.Current.Response.Cookies.Add(formCookie);

            var userRole = User.GetRole();
            if (string.IsNullOrWhiteSpace(userRole) || userRole == "Unknown")
            {
                Logger.Error(string.Format("access denied to user '{0}', no valid user role", email));
                SiteMinderLogOff("User not authorized");
            }
        }
        void Session_Start(object sender, EventArgs e)
        {
            Session.Timeout = ConfigurationManagerWrapper.IntegerSetting("GlobalSessionTimeOut", 60);
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

            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();

            string userNameOut;
            ActiveUsers.TryTake(out userNameOut);
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
            if (Request.Cookies["SMSESSION"] != null)
            {
                var smCookie = new HttpCookie("SMSESSION", "NO")
                {
                    Domain = Utility.Util.Domain,
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(smCookie);
            }
            if (Request.Cookies["SMUSRMSG"] != null)
            {
                var smUsrCookie = new HttpCookie("SMUSRMSG", "NO")
                {
                    Domain = Utility.Util.Domain,
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(smUsrCookie);
            }
            
            Response.StatusCode = 303;
            Response.Redirect(string.Format("{0}?reasonStr={1}", SiteMinderLogOffUrl, reasonStr), false);
        }
    }
}
