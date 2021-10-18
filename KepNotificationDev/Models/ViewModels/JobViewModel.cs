using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KepNotificationDev.Models.ViewModels
{
    public class JobViewModel
    {
        public Job Job { get; set; }
        //public List<Datasets> Datasets { get; set; }
        public List<Datasets> RecieverDatasets { get; set; }
        public List<Template> Templates { get; set; }
        public List<string> Parameters { get; set; }
        public Dictionary<int,string> TimesTypes { get; set; }
    }
}