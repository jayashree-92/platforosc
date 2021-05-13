using System.Web.Mvc;

namespace HMOSecureWeb.Controllers
{
    public class WireLogController : WireUserBaseController
    {
        // GET: WireLog
        public ActionResult Index()
        {
            return View();
        }
    }
}