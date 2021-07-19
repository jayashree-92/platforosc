using System.Web.Http.Filters;
using StructureMap;

namespace Web.Filters
{
    public class DisposeContextApiAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            ObjectFactory.ReleaseAndDisposeAllHttpScopedObjects();
        }
    }
}