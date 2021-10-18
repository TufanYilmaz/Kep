using DevExpress.Web;
using DevExpress.Web.Mvc;
using KepNotificationDev.Helpers;
using KepNotificationDev.Helpers.Service;
using KepNotificationDev.Models;
using KepNotificationDev.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace KepNotificationDev.Controllers
{
    [Authorize(Roles = "1,0")]
    public class SendController : Controller
    {
        // GET: Send
        public ActionResult Index()
        {
            SenderViewModel viewModel = new SenderViewModel();
            //viewModel.Subscibers = DBHelper.GetSubscribers();
            //viewModel.Recievers = DBHelper.GetDatasets().ToList();
            //viewModel.Templates = DBHelper.GetTemplates().ToList();
            //viewModel.SubscibersDatatable = DBHelper.GetSubscibersDataTable();

            return View(viewModel);
        }
        //[ValidateAntiForgeryToken()]
        [ValidateInput(false), HttpPost]
        public ActionResult Sender(SenderViewModel mailMessage)
        {
            var res = new MyMailMessage();
            var ReceiverIds = GridLookupExtension.GetSelectedValues<int>("MyMailMessage_Recievers");
            var messageChannelIds = CheckBoxListExtension.GetSelectedValues<int>("cbReceiverChannels");
            var autoSend = CheckBoxExtension.GetValue<bool>("cbAutoSend");
            string recievertype = "";
            try
            {
                recievertype = EditorExtension.GetValue<string>("cmbDataset");
            }
            catch (Exception)
            {
            }
            int reportId = 0;
            List<UploadedFile> files = new List<UploadedFile>();
            try
            {
                reportId = EditorExtension.GetValue<int>("cmbReport");
            }
            catch (Exception)
            {
            }
            try
            {
                files = UploadControlExtension.GetUploadedFiles("ucMultiSelection", UploadContolHelper.UploadValidationSettings, UploadContolHelper.FileUploadComplete).ToList();
                files = files.Where(f => f.ContentLength != 0).ToList();
            }
            catch (Exception ex)
            {
            }
            DatasetType refType = DatasetType.Diğer;
            if (recievertype.Contains("Abone"))
            {
                refType = DatasetType.Abone;
            }
            else if (recievertype.Contains("Firma"))
            {
                refType = DatasetType.Firma;
            }
            res.MessageChannels = MessageChannel.GetMessageChannelsWithReceiverTypeAndId(refType, messageChannelIds);
            res.AutoSend = autoSend;
            res.Recievers = ReceiverIds == null ? new List<int>() : ReceiverIds.ToList();
            res.Subject = mailMessage.MyMailMessage.Subject;
            res.Body = mailMessage.MyMailMessage.Body;
            if (res.Recievers.Count > 0)
            {
                KepHelper.KepLogInsert(reportId, refType, res, Session["Username"].ToString(), files);
            }

            return RedirectToAction("Index");
        }

        public ActionResult RecieverSelectPartial()
        {
            ViewData["IsVisible"] = false;
            var model = new List<Receiver>();
            string reciever = Request.Params["Reciever"];
            if (!string.IsNullOrEmpty(reciever))
            {
                if (reciever.Contains("Firma"))
                {
                    model = DBHelper.GetAccounts();
                }
                else if (reciever.Contains("Abone"))
                {
                    ViewData["IsVisible"] = true;
                    string department = Request.Params["Departmen"];
                    if (!string.IsNullOrEmpty(department))
                    {
                        model = DBHelper.GetRecievers(DatasetType.Abone, null, department);
                    }
                    else
                    {
                        model = DBHelper.GetRecievers(DatasetType.Abone);
                    }
                }
            }
            return PartialView("RecieverSelectPartial", model);
        }
        public ActionResult ReceiverDataSourcePartial()
        {
            return PartialView("ReceiverDataSourcePartial");
        }
        public ActionResult RecieverDepartmentPartial()
        {
            return PartialView("RecieverDepartmentPartial");
        }
        public ActionResult GetTemplates()
        {
            string recievers = (Request.Params["dataset"] != null) ? Request.Params["dataset"] : "None";
            string reftable = "";
            if (recievers.Contains("Firma"))
            {
                reftable = "tb_ACCOUNTS";
            }
            else if (recievers.Contains("Abone"))
            {
                reftable = "tb_SUBSCRIPTION";
            }
            var result = DBHelper.GetTemplatesByReferance(reftable);
            return PartialView("TemplatePartial", result);


        }

        public ActionResult KepBodyHtmlEditorPartial()
        {
            return PartialView("_KepBodyHtmlEditorPartial");
        }
        public ActionResult KepBodyHtmlEditorPartialImageSelectorUpload()
        {
            HtmlEditorExtension.SaveUploadedImage("KepBodyHtmlEditor", SendControllerKepBodyHtmlEditorSettings.ImageSelectorSettings);
            return null;
        }
        public ActionResult KepBodyHtmlEditorPartialImageUpload()
        {
            HtmlEditorExtension.SaveUploadedFile("KepBodyHtmlEditor", SendControllerKepBodyHtmlEditorSettings.ImageUploadValidationSettings, SendControllerKepBodyHtmlEditorSettings.ImageUploadDirectory);
            return null;
        }
        public ActionResult MultiSelectionFileUpload(IEnumerable<UploadedFile> ucMultiSelection)
        {
            return null;
        }
        public ActionResult UploadedFilesContainer(bool useExtendedPopup)
        {
            ViewData["UseExtendedPopup"] = useExtendedPopup;
            return PartialView("UploadedFilesContainer");
        }
    }
    public class SendControllerKepBodyHtmlEditorSettings
    {
        public const string ImageUploadDirectory = "~/Content/UploadImages/";
        public const string ImageSelectorThumbnailDirectory = "~/Content/Thumb/";

        public static DevExpress.Web.UploadControlValidationSettings ImageUploadValidationSettings = new DevExpress.Web.UploadControlValidationSettings()
        {
            AllowedFileExtensions = new string[] { ".jpg", ".jpeg", ".jpe", ".gif", ".png" },
            MaxFileSize = 4000000
        };

        static DevExpress.Web.Mvc.MVCxHtmlEditorImageSelectorSettings imageSelectorSettings;
        public static DevExpress.Web.Mvc.MVCxHtmlEditorImageSelectorSettings ImageSelectorSettings
        {
            get
            {
                if (imageSelectorSettings == null)
                {
                    imageSelectorSettings = new DevExpress.Web.Mvc.MVCxHtmlEditorImageSelectorSettings(null);
                    imageSelectorSettings.Enabled = true;
                    imageSelectorSettings.UploadCallbackRouteValues = new { Controller = "Send", Action = "KepBodyHtmlEditorPartialImageSelectorUpload" };
                    imageSelectorSettings.CommonSettings.RootFolder = ImageUploadDirectory;
                    imageSelectorSettings.CommonSettings.ThumbnailFolder = ImageSelectorThumbnailDirectory;
                    imageSelectorSettings.CommonSettings.AllowedFileExtensions = new string[] { ".jpg", ".jpeg", ".jpe", ".gif" };
                    imageSelectorSettings.UploadSettings.Enabled = true;
                }
                return imageSelectorSettings;
            }
        }
    }

}