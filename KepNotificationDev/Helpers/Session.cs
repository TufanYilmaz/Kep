using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KepNotificationDev.Helpers
{
    public static class Session
    {
        public static string Username { get; set; }
        public static int UserId { get; internal set; }

        public static Dictionary<int,string> Reports { get; set; } = DBHelper.GetTemplateCodes();
        public static Guid SessionId { get; internal set; }
    }
}