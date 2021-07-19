using System;
using System.Net;
using System.Web.Mvc;

namespace Web.Filters
{
    public class ErrorHandlerFilter : ActionFilterAttribute
    {

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var exception = filterContext.Exception as ApplicationException;
            if (exception != null)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.InternalServerError, exception.Message);
            }

            base.OnActionExecuted(filterContext);
        }

    }
}