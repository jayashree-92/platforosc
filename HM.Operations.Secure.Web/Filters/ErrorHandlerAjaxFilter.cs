using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;

namespace Web.Filters
{
    public class ErrorHandlerAjaxFilter : ActionFilterAttribute
    {

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var exception = actionExecutedContext.Exception as ApplicationException;
            if (exception != null)
            {
                var reasonPhrase = exception.Message.Length > 512 ? "Internal server error" : exception.Message;
                var httpResponseMessage = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.InternalServerError, ReasonPhrase = reasonPhrase, RequestMessage = actionExecutedContext.Request
                    };
                httpResponseMessage.Headers.Add("HMDErrorMessage",exception.Message);
                throw new HttpResponseException(httpResponseMessage);
            }

            base.OnActionExecuted(actionExecutedContext);
        }

    }
}