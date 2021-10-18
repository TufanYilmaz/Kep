using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KepNotificationDev.Models
{
    public class Template
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Kod")]
        public string Code { get; set; }
        [Display(Name = "Açıklama")]
        public string Info { get; set; }
        [Display(Name ="İçerik")]
        public byte[] Content { get; set; }
        [Display(Name ="Parametreler")]
        public string Params { get; set; }
        public string RefTable { get; set; }
        public string Created_By { get; set; }
        public DateTime? Created_Date { get; set; }

    }
}