using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KepNotificationDev.Controllers
{
    public class ViewerController : Controller
    {
        // GET: Viewer
        public ActionResult Viewlog(string token)
        {

            return View();
        }
    }
}