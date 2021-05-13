using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HMOSecureWeb.Controllers
{
    public class UserController : WireAdminBaseController
    {
        // GET: UserOperations
        public ActionResult Index()
        {
            return View();
        }
    }
}