using System;
using System.ComponentModel.DataAnnotations;

namespace KepNotificationDev.Helpers.Service
{
    public enum DelilTipi
    {
        [Display(Name = "Gönderilmedi")]
        NotSend = 0,
        [Display(Name = "Sistem Kabul")]
        SubmissionAcceptance = 1,
        [Display(Name = "Sistem Red")]
        SubmissionRejection,
        [Display(Name = "Sistem Kabul HS")]
        REMMDAcceptance,
        [Display(Name = "Sistem Red HS")]
        REMMDRejection,
        [Display(Name = "Aktarım Hatası")]
        RelayToREMMDFailure,
        [Display(Name = "Posta Kutusuna Teslim")]
        DeliveryToRecipient,
        [Display(Name = "Teslim Edilemedi")]
        NonDeliveryToRecipient,
        [Display(Name = "Okundu")]
        RetrievalByRecipient,
        [Display(Name = "Okuma Zamanaşımı")]
        NonRetrievalByRecipient,
        [Display(Name = "Bilinmeyen")]
        Unknown = 99,

    }
    public enum DatasetType
    {
        [Display(Name = "Firma Tanımı")]
        Firma = 0,
        [Display(Name = "Abone Tanımı")]
        Abone = 1,
        [Display(Name = "Diğer")]
        Diğer
    }
    public enum JobStatus
    {
        [Display(Name = "Taslak")]
        Draft = 0,
        [Display(Name = "Onaylı/İmzalı")]
        Signed = 1,
        [Display(Name = "Gönderildi")]
        Sent = 2,
        [Display(Name = "Hata oluştu")]
        Error = 3,
        [Display(Name = "İptal Edildi")]
        Canceled = 4,
    }
    public enum IntervalTypes
    {
        [Display(Name = "Seçiniz")]
        Seçiniz = 0,
        [Display(Name = "Saat")]
        Saat = 1,
        [Display(Name = "Gün")]
        Gün = 2,
        [Display(Name = "Ay")]
        Ay = 3
    }
    public enum ReceiverType
    {
        [Display(Name = "Sabit Alıcı")]
        Sabit = 0,
        [Display(Name = "Abone Seti")]
        AboneSeti = 1,

    }

    [Flags]
    public enum SendType
    {
        [Display(Name = "Email")]
        Email = 1,
        [Display(Name = "SMS")]
        Sms = 2,
        [Display(Name = "KEP")]
        Kep = 4,
    }

}