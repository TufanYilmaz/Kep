using KepNotificationDev.Helpers;
using KepNotificationDev.Helpers.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KepNotificationDev.Models
{
    public class Job
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Kod"),Required]
        public string Code { get; set; }
        [Display(Name = "Açıklama"),Required]
        public string Info { get; set; }
        //[Display(Name = "İleti Konusu")]
        //public string Subject { get; set; }
        [Display(Name ="Aralık Türü"),Required]
        public IntervalTypes IntervalType { get; set; }
        //[Display(Name = "Aralık Birimi"),Required]
        //public int DateTimesType { get; set; }
        [Display(Name = "Aralık")]
        public int Interval { get; set; }
        [Display(Name = "Alıcı Tipi")]
        public ReceiverType ReceiverType  { get; set; }
        [Display(Name = "Alıcı Seti"), Range(1,500000,ErrorMessage ="Geçerli bir alıcı seti seçiniz")]
        public int ReceiverId { get; set; }
        //[Display(Name = "Alıcı Email(leri)")]
        //public string ReceiverEmails { get; set; }
        //[Display(Name = "Alıcı GSM(leri)")]
        //public string ReceiverGSMs { get; set; }
        [Display(Name = "Otomatik Gönderilsin mi?")]
        public bool AutoSend { get; set; } = true;
        [Display(Name = "Şablon"), Range(1, 500000, ErrorMessage = "Geçerli bir Rapor seçiniz")]
        public int ReportId { get; set; }
        [Display(Name = "Gönderim Kanalları")]
        public MessageChannels MessageChannels { get; set; }
        [Display(Name = "Başlama Zamanı")]
        public DateTime StartTime{ get; set; }
        [Display(Name = "Sonraki Gönderme Zamanı")]
        public DateTime? LastExecutedTime { get; set; } = null;
        [Display(Name = "Aktif")]
        public bool Active { get; set; } = true;
        public string Created_By { get; set; }
        public DateTime? Created_Date { get; set; }
        //[Required(ErrorMessage ="İlgili kişi mail adresi zorunludur")]
        [Display(Name = "İlgili Kişi Mail Adresi")]
        [EmailAddress(ErrorMessage = "Geçersiz mail adresi")]
        public string RelevantAddress { get; set; }

    }
}