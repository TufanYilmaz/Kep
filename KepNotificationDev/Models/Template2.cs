using DevExpress.Web.ASPxHtmlEditor;
using DevExpress.Web.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KepNotificationDev.Models
{
    public class Template2
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Kod")]
        public string Code { get; set; }
        [Display(Name = "Açıklama")]
        public string Info { get; set; }
        [Display(Name = "İçerik")]
        [AllowHtml, HtmlSettings(AllowScripts = false, ResourcePathMode = ResourcePathMode.RootRelative, AllowedDocumentType = AllowedDocumentType.HTML5)]
        public string Content { get; set; }
        [Display(Name = "Parametreler")]
        public string Params { get; set; }
        public int Created_By { get; set; }
        public DateTime? Created_Date { get; set; }
    }
}