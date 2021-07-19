using System.Web.Mvc;

namespace HM.Operations.Secure.Web.Controllers
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