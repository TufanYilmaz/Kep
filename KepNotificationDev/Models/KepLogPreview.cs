using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KepNotificationDev.Models
{
    public class KepLogPreview
    {
        public List<MyUploadedFile> Attachments { get; set; } = new List<MyUploadedFile>();
        public string LastKepState { get; set; }
        public object CONTENT { get; set; }
    }
}