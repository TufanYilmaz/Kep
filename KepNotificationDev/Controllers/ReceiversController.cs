using KepNotificationDev.Helpers;
using KepNotificationDev.Helpers.Service;
using KepNotificationDev.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace KepNotificationDev.Controllers
{
    public class ReceiversController : Controller
    {
        // GET: Receivers
        public ActionResult Index()
        {
            var model = new List<Receiver>();
            return View(model);
        }
        //public ActionResult RecieversGridPartial()
        //{
        //    var model = DBHelper.GetRecievers(DatasetType.Abone,all:true);
        //    return PartialView("RecieversGridPartial", model);
        //}
        [HttpPost, ValidateInput(false)]
        public ActionResult RecieversGridUpdatePartial(Receiver subsciber)
        {
            var model = new List<Receiver>();
            string datasource = "";
            try
            {
                datasource = Request.Params["Reciever"];
            }
            catch (Exception)
            {
            }
            if (ModelState.IsValid)
            {
                if (datasource.Contains("Firma"))
                {
                    DBHelper.RecieverUpdateMail(subsciber, DatasetType.Firma);
                    model = DBHelper.GetAccounts(true);
                }
                else if (datasource.Contains("Abone"))
                {
                    DBHelper.RecieverUpdateMail(subsciber, DatasetType.Abone);
                    string department = Request.Params["Departmen"];
                    if (!string.IsNullOrEmpty(department))
                    {
                        model = DBHelper.GetRecievers(DatasetType.Abone, departmentId: department,all:true);
                    }
                    else
                    {
                        model = DBHelper.GetRecievers(DatasetType.Abone,all:true);
                    }
                }
            }
            else
                ViewData["EditError"] = "Lütfen Hataları gideriniz.";
            return PartialView("RecieversGridPartial", model);
        }
        public ActionResult RecieversGridPartial()
        {
            ViewData["IsVisible"] = false;
            var model = new List<Receiver>();
            string reciever = Request.Params["Reciever"];
            if (!string.IsNullOrEmpty(reciever))
            {
                if (reciever.Contains("Firma"))
                {
                    model = DBHelper.GetAccounts(true);
                }
                else if (reciever.Contains("Abone"))
                {
                    ViewData["IsVisible"] = true;
                    string department = Request.Params["Departmen"];
                    if (!string.IsNullOrEmpty(department))
                    {
                        model = DBHelper.GetRecievers(DatasetType.Abone, departmentId: department, all:true);
                    }
                    else
                    {
                        model = DBHelper.GetRecievers(DatasetType.Abone, all:true);
                    }
                }
            }
            return PartialView("RecieversGridPartial", model);
        }
        public ActionResult DataSourcePartial()
        {
            return PartialView("DataSourcePartial");
        }
        public ActionResult DepartmentPartial()
        {
            return PartialView("DepartmentPartial");
        }

    }
}