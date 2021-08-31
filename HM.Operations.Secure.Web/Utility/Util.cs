﻿using System.Configuration;
using System.Security.Principal;
using System.Web.Configuration;
using Com.HedgeMark.Commons;
using HM.Operations.Secure.Middleware;
using HM.Operations.Secure.Middleware.Util;

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

        public static bool IsSiteminder => !string.IsNullOrWhiteSpace(System.Web.HttpContext.Current.Request.Headers["SMUSER"]);

        public static bool IsLowerEnvironment => !Environment.Equals("Prod");

        public static string LowerEnvironmentFlag => !IsLowerEnvironment ? string.Empty : $"| {Environment}";

        public static string OnBoardingSubDomainPath => ConfigurationManagerWrapper.StringSetting("OnBoardingSubDomainPath", "https://hm-admin-test01.bnymellon.com/onboard/");

        public static string GetRole(this IPrincipal principal)
        {
            if (principal == null)
                return "Unknown";

            if (principal.IsInRole(OpsSecureUserRoles.WireAdmin)) return OpsSecureUserRoles.WireAdmin.Titleize();
            if (principal.IsInRole(OpsSecureUserRoles.WireInitiator)) return OpsSecureUserRoles.WireInitiator.Titleize();
            if (principal.IsInRole(OpsSecureUserRoles.WireApprover)) return OpsSecureUserRoles.WireApprover.Titleize();

            return "Unknown";
        }

        public static bool IsWireAdmin(this IPrincipal principal)
        {
            return principal != null && principal.IsInRole(OpsSecureUserRoles.WireAdmin);
        }

        public static bool IsWireApprover(this IPrincipal principal)
        {
            return principal != null && principal.IsInRole(OpsSecureUserRoles.WireApprover);
        }

        public static bool IsWireInitiator(this IPrincipal principal)
        {
            return principal != null && principal.IsInRole(OpsSecureUserRoles.WireInitiator);
        }

        public static bool IsWireUser(this IPrincipal principal)
        {
            return principal != null && (principal.IsInRole(OpsSecureUserRoles.WireInitiator) || principal.IsInRole(OpsSecureUserRoles.WireApprover));
        }
    }
}