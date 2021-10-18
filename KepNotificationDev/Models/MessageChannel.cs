using KepNotificationDev.Helpers.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace KepNotificationDev.Models
{
    [Flags]
    public enum MessageChannels
    {
        [Display(Name = "Yönetim GSM")]
        ManagementGsm = 1,
        [Display(Name = "Muhasebe GSM")]
        AccountGsm = 2,
        [Display(Name = "Teknik GSM")]
        TechnicGsm = 4,
        [Display(Name = "E-Posta")]
        Email = 8,
        [Display(Name = "Muhasebe E-Posta")]
        AccountEmail = 16,
        [Display(Name = "Kep Mail")]
        KepMail = 32,
    }
    public class MessageChannel
    {
        public int Id { get; set; }
        public string ChannelCaption { get; set; }
        public string Channel { get; set; }
        public MessageChannels MessageChannelEnum => (MessageChannels)this.Id;
        public static readonly IEnumerable<MessageChannel> MessageChannels = new[] {
            new MessageChannel() {Id=1,Channel="ManagementGsm",ChannelCaption="Yönetim GSM" },
            new MessageChannel() {Id=2,Channel="AccountGsm",ChannelCaption="Muhasebe GSM" }, //sadece abone
            new MessageChannel() {Id=4,Channel="TechnicGsm",ChannelCaption="Teknik GSM" }, //sadece abone
            new MessageChannel() {Id=8,Channel="Email",ChannelCaption="E-Posta" },
            new MessageChannel() {Id=16,Channel="AccountEmail",ChannelCaption="Muhasebe E-Posta" }, //sadece abone
            new MessageChannel() {Id=32,Channel="KepMail",ChannelCaption="Kep Mail" },
        };
        public static List<MessageChannel> GetMessageChannels()
        {
            var result = new List<MessageChannel>();
            result = MessageChannels.ToList();
            return result;
        }
        public static List<MessageChannel> GetMessageChannelsWithId(params int[] Ids)
        {
            var result = new List<MessageChannel>();
            result = MessageChannels.Where(m => Ids.Contains(m.Id)).ToList();
            return result;
        }
        public static List<MessageChannel> GetMessageChannelsWithReceiverType(DatasetType type)
        {
            var result = new List<MessageChannel>();
            switch (type)
            {
                case DatasetType.Firma:
                    result = MessageChannels.Where(m => (new int[] { 1, 4, 6 }).Contains(m.Id)).ToList();
                    break;
                case DatasetType.Abone:
                    result = MessageChannels.ToList();
                    break;
            }
            return result;
        }
        public static List<MessageChannel> GetMessageChannelsWithReceiverTypeAndId(DatasetType type, params int[] Ids)
        {
            var result = new List<MessageChannel>();
            result = GetMessageChannelsWithReceiverType(type).Where(m => Ids.Contains(m.Id)).ToList();
            return result;
        }
        public static int GetMessageChannelsMaxId()
        {
            return MessageChannels.Max(m => m.Id);
        }
    }
}