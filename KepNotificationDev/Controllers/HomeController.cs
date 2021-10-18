using DevExpress.XtraReports.UI;
using KepNotificationDev.Helpers;
using KepNotificationDev.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Http;
using System.Web.Mvc;

namespace KepNotificationDev.Controllers
{
    [System.Web.Mvc.Authorize(Roles = "1,0")]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //Helpers.Service.JobService.Start();
            var model = DBHelper.GetMessageLogs(Helpers.Service.JobStatus.Draft);
            return View(model);
        }
        public ActionResult KepLogGridViewPartial()
        {
            var model = DBHelper.GetMessageLogs(Helpers.Service.JobStatus.Draft);
            return PartialView("KepLogsGridView", model);
        }
        public ActionResult KepLogPreviewPartial(int logId, int templateId, int subId)
        {
            //List<Models.MyUploadedFile> attachments = new List<Models.MyUploadedFile>();
            var Model = new KepLogPreview();
            var log = DBHelper.GetMessageLogById(logId, true);
            Model.LastKepState = log.KepStatusInfo;
            if (log.HasAttachments)
            {
                Model.Attachments = DBHelper.GetLogAttachments(logId);
                foreach (var attac in Model.Attachments)
                {
                    attac.DataBase64 = Convert.ToBase64String(attac.Data);
                }
            }
            if (templateId > 0)
            {
                var temp = DBHelper.TemplateGetById(templateId);
                XtraReport report = new XtraReport();
                using (MemoryStream ms = new MemoryStream(temp.Content))
                {
                    report.LoadLayout(ms);
                    report.Parameters[0].Value = Convert.ToInt32(subId);
                    using (MemoryStream msHTML = new MemoryStream())
                    {
                        report.ExportToHtml(msHTML);
                        var res = Encoding.UTF8.GetString(msHTML.ToArray());
                    }
                }
                Model.CONTENT = report;
                return PartialView("KepLogPreview", Model);
            }
            else
            {
                var model = new List<string>();
                //var log = DBHelper.KepLogGetById(logId);
                model.Add(log.Subject);
                model.Add(log.Content);
                Model.CONTENT = model;
                return PartialView("KepLogPreview", Model);
            }
        }
        public ActionResult AllMessages()
        {
            //Helpers.Service.JobService.Start();
            var model = DBHelper.GetMessageLogs();
            return View(model);
        }
        public ActionResult AllMessagesGridViewPartial()
        {
            var model = DBHelper.GetMessageLogs();
            return PartialView("AllMessagesGridViewPartial", model);
        }
        public ActionResult CancelLogs([FromBody] int[] Ids)
        {
            string result;
            if (DBHelper.MessageLogUpdateStatus(Ids, Helpers.Service.JobStatus.Canceled))
            {
                result = Ids.Length + " adet kayýt iptal edildi.";
            }
            else
            {
                result = "Sistemsel bir hata oluþtu";
            }

            return Content(result);
        }
    }
}