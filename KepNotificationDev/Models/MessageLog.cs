using KepNotificationDev.Helpers;
using KepNotificationDev.Helpers.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KepNotificationDev.Models
{
    public class MessageLog
    {
        public int Id { get; set; }
        [Display(Name ="Alıcı Tipi")]
        public MessageChannels MessageChannel { get; set; } 
        [Display(Name ="Alıcı")]
        public string Receiver { get; set; }
        public int RefId { get; set; }
        public string RefTable { get; set; }
        public int? JobId { get; set; }
        public string Token { get; set; }
        [Display(Name = "Konu")]
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Ileti_Id { get; set; }
        public string UnsignedContent { get; set; }
        public string SignedContent { get; set; }
        public bool HasAttachments { get; set; }
        public JobStatus StatusType { get; set; }
        [Display(Name = "Delil Tarihi")]
        public DateTime? KepStatusDatetime { get; set; }
        [Display(Name ="Gönderilme Tarihi")]
        public DateTime? SentDatetime { get; set; }
        public string KepStatusInfo => KepStatus.ToString();
        [Display(Name ="Delil Durumu")]
        public DelilTipi KepStatus { get; set; }
        [Display(Name ="Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        #region NonDatabaseFields
        [Display(Name ="Açıklama")]
        public string RefInfo { get; set; }
        [Display(Name ="Kod")]
        public string RefCode { get; set; }
        [Display(Name ="Görev Açıklaması")]
        public string JobInfo { get; set; }
        [Display(Name = "Durumu")]
        public string StatusCode
        {
            get 
            {
                return EnumHelper<JobStatus>.GetDisplayValue(StatusType);
            }
        }
        //MessageChannel
        [Display(Name = "Gönderim Tipi")]
        public string MessageChannelCode
        {
            get
            {
                return EnumHelper<MessageChannels>.GetDisplayValue(MessageChannel);
            }
        }
        //public string KepStatus { get; set; }
        public int TemplateId { get; set; }
        public List<MyUploadedFile> Attachments { get; set; } = null;
        [Display(Name = "Alıcı Tipi")]
        public DatasetType RefType
        { get
            {
                DatasetType res = DatasetType.Diğer;
                if(!string.IsNullOrEmpty( RefTable))
                {
                    if(RefTable.Contains("SUBSCR"))
                    {
                        res = DatasetType.Abone;
                    }else if(RefTable.Contains("ACCOUNTS"))
                    {
                        res = DatasetType.Firma;
                    }
                }
                return res;
            } 
        }
        #endregion

    }
}