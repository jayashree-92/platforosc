using System;
using System.Web.Mvc;
using Com.HedgeMark.Commons;

namespace HMOSecureWeb.Filters
{
    public class RedirectToHttps : RequireHttpsAttribute
    {
        private static readonly bool ShouldRedirect = ConfigurationManagerWrapper.BooleanSetting(Config.ShouldRedirectHttpRequest, true);
        protected override void HandleNonHttpsRequest(AuthorizationContext filterContext)
        {
            if (!ShouldRedirect) return;
            if (!string.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only GET methods will be redirected");

            var url = string.Format("https://{0}{1}", filterContext.HttpContext.Request.Url.Host, filterContext.HttpContext.Request.RawUrl);
            filterContext.Result = new RedirectResult(url);
        }
    }
}