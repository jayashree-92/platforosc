using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Com.HedgeMark.Commons;
using HM.Operations.Secure.Middleware;
using Humanizer;

namespace HM.Operations.Secure.Web.Controllers
{
    public class ConfigurationController : WireUserBaseController
    {
        // GET: Configuration
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetSwitchModules()
        {
            return Json(SystemSwitches.AllModules, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSwitchList()
        {
            return Json(SystemSwitches.GroupedSwitchList.Select(s => new { module = s.Key, switches = s.Value.Select(s1 => new { key = s1.Key.ToString(), label = s1.Key.Humanize(), value = s1.Value, type = s1.Type }) }), JsonRequestBehavior.AllowGet);
        }
        
        public void SetSwitchValue(Switches.SwitchKey key, string value)
        {
            OpsSecureSwitches.SetSwitch(key, HttpUtility.UrlDecode(value), UserName);
        }
    }
}