using KepNotificationDev.Helpers.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace KepNotificationDev.Models.ViewModels
{
    public class SenderViewModel
    {
        public List<Receiver> Receivers { get; set; } = new List<Receiver>();
        //public DataTable SubscibersDatatable { get; set; }
        public MyMailMessage MyMailMessage { get; set; } = new MyMailMessage();
        //public List<Datasets> Recievers { get; set; } = new List<Datasets>();
        public List<Template> Templates { get; set; } = new List<Template>();
        //public List<Template2> HtmlTemplates { get; set; }
        public List<MessageChannel> ReceiverChannels { get; set; }
    }
}