using DevExpress.Web.ASPxHtmlEditor;
using DevExpress.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


namespace KepNotificationDev.Models
{
    public class MyMailMessage
    {
        [Display(Name = "Alıcılar")]
        public List<int> Recievers { get; set; }
        public List<MessageChannel> MessageChannels { get; set; }
        public bool AutoSend { get; set; }
        [Display(Name = "Konu"),Required]
        public string Subject { get; set; }
        [Display(Name = "Mesaj"),Required]
        [AllowHtml, HtmlSettings(AllowScripts = false, ResourcePathMode = ResourcePathMode.RootRelative, AllowedDocumentType = AllowedDocumentType.HTML5)]
        public string Body { get; set; }
        [Display(Name = "Ekler")]
        public object Attachments { get; set; }
    }
}