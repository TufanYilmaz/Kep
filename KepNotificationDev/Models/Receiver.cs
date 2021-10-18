using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KepNotificationDev.Models
{
    public class Receiver
    {
        public int Id { get; set; }
        [Display(Name = "Kod")]
        public string Code{ get; set; }
        [Display(Name = "Açıklama")]
        public string Info { get; set; }
        [Display(Name = "Kep Mail")]
        [EmailAddress(ErrorMessage = "Geçersiz mail adresi")]
        public string KepMail { get; set; }
        [Display(Name = "Muhasebe E-Posta")]
        public string AccountEmail { get; set; }
        [Display(Name = "Genel E-Posta")]
        public string Email { get; set; }
        [Display(Name = "İdari Yetkili Gsm")]
        public string ManagementGsm { get; set; }
        [Display(Name = "Muhasebe Gsm")]
        public string AccountGsm { get; set; }
        [Display(Name = "Teknik Yetkili Gsm")]
        public string TechnicGsm { get; set; }

    }
}