using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Web.Models;

namespace Web.Filters
{
    public class EnricherAjaxFilter : ActionFilterAttribute
    {

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var user = HttpContext.Current.User;
            string userName = user == null ? "Unknown User" : user.Identity.Name;
            new AjaxEnricher().Enrich(actionContext.ActionArguments, userName);

            base.OnActionExecuting(actionContext);
        }
    }
}