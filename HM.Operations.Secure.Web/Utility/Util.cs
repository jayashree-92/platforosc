using Com.HedgeMark.Commons;
using HM.Operations.Secure.Middleware;
using HM.Operations.Secure.Middleware.Util;
using HM.Operations.Secure.Web.Controllers;
using System.Configuration;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Configuration;

namespace HM.Operations.Secure.Web.Utility
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

        public static bool IsLowerEnvironment => !Environment.Equals("Production");

        public static string LowerEnvironmentFlag => !IsLowerEnvironment ? string.Empty : $"| {Environment}";

        public static string OnBoardingSubDomainPath => ConfigurationManagerWrapper.StringSetting("OnBoardingSubDomainPath", "https://hm-admin-test01.bnymellon.com/onboard/");
        public static string InternalSitePrefix => ConfigurationManagerWrapper.StringSetting("InternalSitePrefix", "DMA");

        public static string GetRole(this IPrincipal principal)
        {
            if(principal == null)
                return "Unknown";

            if(ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AccountController.AuthorizeRoleObjectMap[OpsSecureUserRoles.WireAdmin])) return OpsSecureUserRoles.WireAdmin.Titleize();
            if(ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AccountController.AuthorizeRoleObjectMap[OpsSecureUserRoles.WireReadOnly])) return OpsSecureUserRoles.WireReadOnly.Titleize();
            if(ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AccountController.AuthorizeRoleObjectMap[OpsSecureUserRoles.WireInitiator])) return OpsSecureUserRoles.WireInitiator.Titleize();
            if(ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AccountController.AuthorizeRoleObjectMap[OpsSecureUserRoles.WireApprover])) return OpsSecureUserRoles.WireApprover.Titleize();

            return "Unknown";
        }

        public static bool IsWireAdmin(this IPrincipal principal)
        {
            return ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AccountController.AuthorizeRoleObjectMap[OpsSecureUserRoles.WireAdmin]);
        }

        public static bool IsWireApprover(this IPrincipal principal)
        {
            return ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AccountController.AuthorizeRoleObjectMap[OpsSecureUserRoles.WireApprover]);
        }

        public static bool IsWireInitiator(this IPrincipal principal)
        {
            return ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AccountController.AuthorizeRoleObjectMap[OpsSecureUserRoles.WireInitiator]);
        }

        public static bool IsWireReadOnly(this IPrincipal principal)
        {
            return ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AccountController.AuthorizeRoleObjectMap[OpsSecureUserRoles.WireReadOnly]);
        }

        public static bool IsWireUser(this IPrincipal principal)
        {
            return ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AccountController.AuthorizeRoleObjectMap[OpsSecureUserRoles.WireInitiator])
                   || ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AccountController.AuthorizeRoleObjectMap[OpsSecureUserRoles.WireApprover]);
        }

        public static bool IsSecurePortalAccessible(this IPrincipal principal)
        {
            return ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AccountController.AuthorizeRoleObjectMap[OpsSecureUserRoles.WireInitiator])
                   || ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AccountController.AuthorizeRoleObjectMap[OpsSecureUserRoles.WireApprover])
                   || ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AccountController.AuthorizeRoleObjectMap[OpsSecureUserRoles.WireReadOnly])
                   || ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AccountController.AuthorizeRoleObjectMap[OpsSecureUserRoles.WireAdmin]);
        }
    }
}