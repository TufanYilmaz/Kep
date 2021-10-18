using DevExpress.XtraReports.UI;
using KepNotificationDev.Helpers;
using KepNotificationDev.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KepNotificationDev.Controllers
{
    [Authorize(Roles = "1")]
    public class TemplateController : Controller
    {
        // GET: Template
        public ActionResult Index()
        {
            var model = DBHelper.GetTemplates();
            return View(model);
        }
        public ActionResult EditTemplate(int Id)
        {
            Models.ViewModels.TemplateViewModel editingViewModel = new Models.ViewModels.TemplateViewModel();
            Template editingTemplate = DBHelper.TemplateGetById(Id);
            if (editingTemplate == null)
            {
                editingTemplate = new Template();
                editingTemplate.Id = -1;
            }
            else
            {
                using (MemoryStream ms = new MemoryStream(editingTemplate.Content))
                {
                    editingViewModel.ReportDesign = new XtraReport();
                    editingViewModel.ReportDesign.LoadLayout(ms);
                    editingViewModel.ReportDesign.DisplayName = editingTemplate.Code;
                }
            }
            editingViewModel.Datasets = DBHelper.GetDatasets().ToList();
            editingViewModel.Template = editingTemplate;
            return View("TemplateEditForm", editingViewModel);
        }
        public ActionResult AddTemplate(Template ds)
        {
            var res = DBHelper.TemplateInsert(ds);
            if (res.IndexOf("başarılı") > 0)
            {
                var t = res.Split('#');
                var addedId = Convert.ToInt32(t[1]);
                return RedirectToAction("EditTemplate", new { Id = addedId });
            }
            return View();
        }
        public ActionResult DeleteTemplate(int Id)
        {
            DBHelper.TemplateDelete(Id);
            return RedirectToAction("Index");
        }
    }
}