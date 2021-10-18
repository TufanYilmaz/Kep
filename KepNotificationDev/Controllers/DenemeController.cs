using KepNotificationDev.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using DevExpress.XtraReports.UI;
using System.Text;

namespace KepNotificationDev.Controllers
{
    [Authorize(Roles ="1")]
	// Processes Query Builder requests.
    public class DenemeController : DevExpress.Web.Mvc.Controllers.QueryBuilderApiController
    {
        public override ActionResult Invoke() {
            return base.Invoke();
        }
        public ActionResult Index()
        {
            
            
            return View();
        }
    }
}