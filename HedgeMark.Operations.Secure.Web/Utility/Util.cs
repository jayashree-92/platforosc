﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;
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

            if (principal.IsInRole(OpsSecureUserRoles.DmaWireInitiator)) return OpsSecureUserRoles.DmaWireInitiator;
            if (principal.IsInRole(OpsSecureUserRoles.DmaWireApprover)) return OpsSecureUserRoles.DmaWireApprover;
            if (principal.IsInRole(OpsSecureUserRoles.DmaAdminUser)) return OpsSecureUserRoles.DmaAdminUser;

            return "Unknown";
        }
    }
}