using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KepNotificationDev.Models
{
    public class MyUploadedFile
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Info { get; set; }
        public byte[] Data { get; set; }
        public string DataBase64 { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}