using Com.HedgeMark.Commons;
using HedgeMark.Monitoring;
using HM.Operations.Secure.Middleware;
using log4net;
using log4net.Config;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;

namespace HM.Operations.Secure.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MvcApplication));
        private static readonly int GlobalSessionTimeOut = ConfigurationManagerWrapper.IntegerSetting("GlobalSessionTimeOut", 20);
        private static readonly string InternalLogsDirectory = ConfigurationManagerWrapper.StringSetting("OpsSecureLogsPath", "C:\\InternalAppLogs\\");
        protected void Application_Start()
        {
            //Log4Net Initiation
            ConfigureLog4net();

            //Security measure - hide server details in the response header
            MvcHandler.DisableMvcResponseHeader = true;

            AppDomain.CurrentDomain.UnhandledException += GlobalUnhandledException;

            AreaRegistration.RegisterAllAreas();
            BundleTable.Bundles.ResetAll();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Boot up required assemblies to middleware
            Task.Factory.StartNew(BootUpMiddleware.BootUp);

            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;

            AppHeartBeat.Start("HM-Operations-Secure", AppMnemonic.DMO, AppType.WebApp, "HMOpsSecureConnectionString");
        }

        private static void ConfigureLog4net()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo($"{AppDomain.CurrentDomain.BaseDirectory}\\Web.config"));

            log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
            foreach(var appender in h.Root.Appenders)
            {
                if(appender is log4net.Appender.RollingFileAppender)
                {
                    if(appender.Name.Equals("RollingFileAppenderXML"))
                    {
                        log4net.Appender.RollingFileAppender fa = (log4net.Appender.RollingFileAppender)appender;
                        fa.File = $@"{InternalLogsDirectory}\IC_Ops-Secure\{DateTime.Today:yyyy-MM-dd}\{Environment.MachineName}\WebLogs\WebLog_";
                        fa.ActivateOptions();
                    }
                }
            }
        }

        void GlobalUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error(e.ExceptionObject);
        }

        protected void Application_PostAuthorizeRequest()
        {
            if(IsWebApiRequest())
            {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
        }

        protected void Application_PostAuthenticateRequest()
        {
            if(Request.IsAuthenticated)
            {

                //var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;

                //List<string> groups = new List<string>()
                //{
                //    "IC_App_Dev01-OpsSecure-Initiator",
                //    "IC_App_Dev01-OpsSecure-Approver",
                //    "IC_App_Dev01-OpsSecure-Admin",

                //    "0cadd7d9-bb68-4a99-9d5d-2888e1ff14c6",
                //    "1b9acd08-7cfd-4742-b01c-07a7dd8622b5",
                //    "631f07a3-cde6-4a44-81ac-086ff30408d0"

                //};


                //Dictionary<string, string> headers = new Dictionary<string, string>()
                //{
                //    { "631f07a3-cde6-4a44-81ac-086ff30408d0",OpsSecureUserRoles.WireAdmin },
                //    { "1b9acd08-7cfd-4742-b01c-07a7dd8622b5",OpsSecureUserRoles.WireApprover },
                //    { "0cadd7d9-bb68-4a99-9d5d-2888e1ff14c6",OpsSecureUserRoles.WireInitiator },
                //};

                //Claim claim = null;
                //foreach(var group in headers)
                //{
                //    claim = identity.Claims.Where(c => c.Value == group.Key && c.Type == "role").FirstOrDefault();

                //    if(claim != null)
                //        break;
                //}

                ////if(claim == null)
                ////{

                ////}

                ////string[] roles = GetRolesForUser(User.Identity.Name);
                //var claimsIdentity = ClaimsPrincipal.Current.Identities.First();
                //{
                //    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, headers[claim.Value]));
                //}
            }
        }

        //protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        //{
        //    System.Web.HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
        //    if(authCookie != null)
        //    {
        //        FormsAuthenticationTicket authTicket = null;
        //        authTicket = FormsAuthentication.Decrypt(authCookie.Value);

        //        if(authTicket != null && !authTicket.Expired)
        //        {
        //            FormsAuthenticationTicket newAuthTicket = authTicket;

        //            if(FormsAuthentication.SlidingExpiration)
        //            {
        //                newAuthTicket = FormsAuthentication.RenewTicketIfOld(authTicket);
        //            }
        //            string userData = newAuthTicket.UserData;
        //            string[] roles = userData.Split(',');

        //            System.Web.HttpContext.Current.User =
        //                new System.Security.Principal.GenericPrincipal(new FormsIdentity(newAuthTicket), roles);
        //        }
        //    }
        //}

        private static bool IsWebApiRequest()
        {
            return HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath != null && HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/api");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            Logger.Error($"Application Error:\n{ex.Message}", ex);
        }

        //protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        //{
        //    if (HttpContext.Current.Request.Url.AbsolutePath == SiteMinderLogOffUrl)
        //        return;

        //    if (IsAlreadyAuthenticated())
        //        return;

        //    //Logger.Debug("New User Logging into the system - so Checking for Authorization");

        //    var smUserId = HttpContext.Current.Request.Headers[SiteMinderHeaderToken];
        //    var roles = new List<string>();
        //    if (HttpContext.Current.Request.IsLocal)
        //    {
        //        smUserId = ConfigurationManager.AppSettings["LocalSiteMinderCommitId"];
        //        // var totalExperience = AccountController.GetTotalYearsOfExperience(smUserId);
        //        //roles.Add(totalExperience > 1 ? OpsSecureUserRoles.WireApprover : OpsSecureUserRoles.WireInitiator);
        //        roles.Add(OpsSecureUserRoles.WireApprover);
        //    }
        //    var userSso = AccountController.GetUserDetailByCommitId(smUserId);

        //    if (userSso == null)
        //    {
        //        Logger.Warn($"access denied to user '{smUserId}', user not registered");
        //        SiteMinderLogOff("access denied, user not registered");
        //        return;
        //    }

        //    var email = userSso.Name;

        //    if (!(roles.Contains(OpsSecureUserRoles.WireReadOnly) || roles.Contains(OpsSecureUserRoles.WireApprover) || roles.Contains(OpsSecureUserRoles.WireInitiator) || roles.Contains(OpsSecureUserRoles.WireAdmin)))
        //    {
        //        if (!string.IsNullOrWhiteSpace(userSso.CommitId))
        //        {
        //            var ldapGroups = UmsLibrary.GetLdapGroupsOfLdapUser(userSso.CommitId);
        //            if (ldapGroups.Contains(OpsSecureUserRoles.WireApprover))
        //            {
        //                var totalExperience = AccountController.GetTotalYearsOfExperience(userSso.CommitId);
        //                roles.Add(totalExperience > 1 ? OpsSecureUserRoles.WireApprover : OpsSecureUserRoles.WireInitiator);
        //            }
        //            else if (ldapGroups.Contains(OpsSecureUserRoles.WireInitiator))
        //                roles.Add(OpsSecureUserRoles.WireInitiator);
        //            else if (ldapGroups.Contains(OpsSecureUserRoles.WireAdmin))
        //                roles.Add(OpsSecureUserRoles.WireAdmin);
        //            else if (ldapGroups.Contains(OpsSecureUserRoles.WireReadOnly))
        //                roles.Add(OpsSecureUserRoles.WireReadOnly);
        //        }
        //    }

        //    if (AccountController.AllowedUserRoles.All(role => !roles.Contains(role)))
        //    {
        //        Logger.Error($"access denied to user '{email}', user role not available");
        //        SiteMinderLogOff("User not authorized");
        //        return;
        //    }

        //    //Add ASPNET Role for Entitlements - as they are currently assigned that way. We need to change it when Permission engine release is scheduled
        //    using (var context = new AdminContext())
        //    {
        //        var userRole = (from aspUser in context.aspnet_Users
        //                        join usr in context.hLoginRegistrations on aspUser.UserName equals usr.varLoginID
        //                        where usr.intLoginID == userSso.LoginId && aspUser.aspnet_Roles.Any(r => AuthorizationManager.AuthorizedDmaUserRoles.Contains(r.RoleName)) && !usr.isDeleted
        //                        let role = aspUser.aspnet_Roles.Any(r => r.RoleName == OpsSecureUserRoles.DMAAdmin) ? OpsSecureUserRoles.DMAAdmin : OpsSecureUserRoles.DMAUser
        //                        select role).FirstOrDefault() ?? string.Empty;

        //        if (!string.IsNullOrWhiteSpace(userRole))
        //            roles.Add(userRole);
        //    }

        //    var webIdentity = new GenericIdentity(email, "SiteMinder");
        //    var principal = new GenericPrincipal(webIdentity, roles.ToArray());
        //    HttpContext.Current.User = principal;
        //    Thread.CurrentPrincipal = principal;
        //    var userData = string.Join(",", roles);
        //    var ticket = new FormsAuthenticationTicket(1, email, DateTime.Now, DateTime.Now.AddMinutes(GlobalSessionTimeOut), true, userData, FormsAuthentication.FormsCookiePath);
        //    var encTicket = FormsAuthentication.Encrypt(ticket);
        //    var formCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket) { Domain = Utility.Util.Domain };
        //    Response.Cookies.Add(formCookie);


        //    if (ActiveUsers.Contains(User.Identity.Name) || HttpContext.Current.Request.IsLocal)
        //        return;

        //    ActiveUsers.Add(User.Identity.Name);
        //    var auditData = new hmsUserAuditLog
        //    {
        //        Action = "Logged In",
        //        Module = "Account",
        //        Log = "Signed into Secure System",
        //        CreatedAt = DateTime.Now,
        //        UserName = User.Identity.Name
        //    };
        //    AuditManager.LogAudit(auditData);
        //}

        //private bool IsAlreadyAuthenticated()
        //{
        //    var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
        //    if (authCookie == null)
        //        return false;

        //    FormsAuthenticationTicket authTicket;
        //    try
        //    {
        //        authTicket = FormsAuthentication.Decrypt(authCookie.Value);
        //    }
        //    catch (Exception)
        //    {
        //        Logger.Error($"Form Auth decryption failed for : {FormsAuthentication.FormsCookieName}");
        //        return false;
        //    }

        //    if (authTicket == null || authTicket.Expired)
        //        return false;

        //    if (FormsAuthentication.SlidingExpiration)
        //        authTicket = FormsAuthentication.RenewTicketIfOld(authTicket);

        //    if (authTicket == null)
        //        return false;

        //    var webIdentity = new GenericIdentity(authTicket.Name, "SiteMinder");
        //    var principal = new GenericPrincipal(webIdentity, authTicket.UserData.Split(','));
        //    HttpContext.Current.User = principal;
        //    Thread.CurrentPrincipal = principal;
        //    return true;
        //}

        void Session_Start(object sender, EventArgs e)
        {
            Session.Timeout = GlobalSessionTimeOut;
            Session["SessionStartTime"] = DateTime.Now;
            Session["userName"] = User.Identity.Name;
            Session["userRole"] = "";
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
            //var auditData = new hmsUserAuditLog
            //{
            //    Action = "Logged Out",
            //    Module = "Account",
            //    Log = "Signed out from Secure System",
            //    CreatedAt = DateTime.Now,
            //    UserName = (string)Session["userName"]// User.Identity.Name
            //};
            //AuditManager.LogAudit(auditData);

            // Delete the user details from Session.
            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();
        }
    }
}
