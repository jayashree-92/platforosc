using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace Web.Filters
{
    public class ValidationActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (!actionContext.ModelState.IsValid)
            {
                var error = string.Join(",", actionContext.ModelState.Where(e => e.Value.Errors.Count > 0).Select(e => e.Value.Errors.First().ErrorMessage));

                actionContext.Response =  new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        ReasonPhrase = error,
                        RequestMessage = actionContext.Request
                    };
            }
        }
    }
}