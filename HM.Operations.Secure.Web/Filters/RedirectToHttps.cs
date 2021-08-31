using System;
using System.Web.Mvc;
using Com.HedgeMark.Commons;

namespace HM.Operations.Secure.Web.Filters
{
    public class RedirectToHttps : RequireHttpsAttribute
    {
        private static readonly bool ShouldRedirect = ConfigurationManagerWrapper.BooleanSetting(Config.ShouldRedirectHttpRequest, true);
        protected override void HandleNonHttpsRequest(AuthorizationContext filterContext)
        {
            if (!ShouldRedirect) return;
            if (!string.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only GET methods will be redirected");

            var url = $"https://{filterContext.HttpContext.Request.Url.Host}{filterContext.HttpContext.Request.RawUrl}";
            filterContext.Result = new RedirectResult(url);
        }
    }
}