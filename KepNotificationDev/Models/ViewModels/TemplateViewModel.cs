using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KepNotificationDev.Models.ViewModels
{
    public class TemplateViewModel
    {
        public Template Template { get; set; }
        public List<Datasets> Datasets { get; set; }
        public XtraReport ReportDesign { get; set; } = new XtraReport();

    }
}