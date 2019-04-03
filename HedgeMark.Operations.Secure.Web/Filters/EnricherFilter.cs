using System.Web.Mvc;
using Web.Models;

namespace Web.Filters
{
    public class EnricherFilter : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var user = filterContext.HttpContext.User;
            string userName = user == null ? "Unknown User" : user.Identity.Name;
            new AjaxEnricher().Enrich(filterContext.ActionParameters, userName);

            base.OnActionExecuting(filterContext);
        }
    }
}