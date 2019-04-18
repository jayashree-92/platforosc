using System.Configuration;
using System.Security.Principal;
using System.Web.Configuration;
using Com.HedgeMark.Commons;
using HMOSecureWeb.Controllers;

namespace HMOSecureWeb.Utility
{
    public static class Util
    {
        public static string Environment = ConfigurationManagerWrapper.StringSetting("Environment");
        public static string Domain
        {
            get
            {
                var authentication = (AuthenticationSection)ConfigurationManager.GetSection("system.web/authentication");
                return authentication.Forms.Domain;
            }
        }

        public static bool IsSiteminder
        {
            get { return !string.IsNullOrWhiteSpace(System.Web.HttpContext.Current.Request.Headers["SMUSER"]); }
        }

        public static bool IsLowerEnvironment
        {
            get { return !Environment.Equals("Prod"); }
        }

        public static string LowerEnvironmentFlag
        {
            get { return !IsLowerEnvironment ? string.Empty : string.Format("({0})", Environment); }
        }

        public static string GetRole(this IPrincipal principal)
        {
            if (principal == null)
                return "Unknown";

            if (principal.IsInRole(OpsSecureUserRoles.WireInitiator)) return OpsSecureUserRoles.WireInitiator;
            if (principal.IsInRole(OpsSecureUserRoles.WireApprover)) return OpsSecureUserRoles.WireApprover;

            return "Unknown";
        }
    }
}