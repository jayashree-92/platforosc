using Com.HedgeMark.Commons;
using System.IO.Compression;
using System.Web.Mvc;

namespace Web.Filters
{
    public class CompressAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (ConfigurationManagerWrapper.BooleanSetting(Config.ShouldEnableGzipCompression))
            {
                var request = filterContext.HttpContext.Request;
                var response = filterContext.HttpContext.Response;
                var acceptEncoding = request.Headers["Accept-Encoding"];
                if (!string.IsNullOrEmpty(acceptEncoding) && acceptEncoding.ToLower().Contains("gzip"))
                {
                    response.Filter = new GZipStream(response.Filter, CompressionLevel.Optimal);
                    response.AppendHeader("Content-Encoding", "gzip");
                }
            }
        }
    }
}