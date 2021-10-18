using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KepNotificationDev.Models
{
    public class BaseModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Info { get; set; }
    }
}