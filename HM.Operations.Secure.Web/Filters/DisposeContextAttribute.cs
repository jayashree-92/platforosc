using System.Web.Mvc;
using StructureMap;

namespace Web.Filters
{
    public class DisposeContextAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            ObjectFactory.ReleaseAndDisposeAllHttpScopedObjects();
        }
    }
}