using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using KepNotificationDev.Helpers.Service;
using KepNotificationDev.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KepNotificationDev.Helpers
{
    public static class KepHelper
    {
        public static int KepLogInsert(int reportId, DatasetType refType, MyMailMessage res, string Username, List<DevExpress.Web.UploadedFile> files)
        {
            bool hasAttech = false;
            Template template = new Template();
            if (reportId > 0)
            {
                template = DBHelper.TemplateGetById(reportId);

            }
            List<int> insertedAttaches = new List<int>();
            if (files.Count > 0)
            {
                insertedAttaches = DBHelper.UploadedAttachmentsInsert(files, res.Subject);
                hasAttech = true;
            }
            var subscribers = DBHelper.GetRecievers(refType, res.Recievers);


            if (res.MessageChannels.Count > 0)
            {
                foreach (var sub in subscribers)
                {

                    foreach (var messageChannel in res.MessageChannels)
                    {
                        string receiver = "";
                        switch (messageChannel.MessageChannelEnum)
                        {
                            case MessageChannels.ManagementGsm:
                                receiver = sub.ManagementGsm;
                                break;
                            case MessageChannels.AccountGsm:
                                receiver = sub.AccountGsm;
                                break;
                            case MessageChannels.TechnicGsm:
                                receiver = sub.TechnicGsm;
                                break;
                            case MessageChannels.Email:
                                receiver = sub.Email;
                                break;
                            case MessageChannels.AccountEmail:
                                receiver = sub.AccountEmail;
                                break;
                            case MessageChannels.KepMail:
                                receiver = sub.KepMail;
                                break;
                        }
                        if(string.IsNullOrEmpty(receiver))
                        {
                            continue;
                        }
                        var insertedId = DBHelper.MessageLogInsert(new MessageLog()
                        {
                            Receiver = receiver,
                            MessageChannel = messageChannel.MessageChannelEnum,
                            RefId = sub.Id,
                            RefTable = refType == DatasetType.Abone ? "tb_SUBSCRIPTION" : "tb_ACCOUNTS",
                            RefCode = sub.Code,
                            RefInfo = sub.Info,
                            Content = reportId > 0 ? ConvertReportToHTML(sub.Id, template) : res.Body,
                            Subject = res.Subject,
                            StatusType = (messageChannel.MessageChannelEnum==MessageChannels.KepMail ? JobStatus.Draft : ( res.AutoSend ? JobStatus.Signed : JobStatus.Draft )),
                            HasAttachments = hasAttech,
                        }, Username);
                        if (files.Count > 0)
                        {
                            DBHelper.MessegerLogAttachmentAdd(insertedId, insertedAttaches.ToArray());
                        }
                    }
                }
            }


            return 0;
            //DBHelper.KepLogInsert();
        }

        private static string ConvertReportToHTML(int subscriberId, Template template)
        {
            string result = "";
            using (MemoryStream ms = new MemoryStream(template.Content))
            {
                XtraReport report = new XtraReport();
                report.LoadLayout(ms);
                report.Parameters[0].Value = subscriberId;
                using (MemoryStream msHTML = new MemoryStream())
                {
                    HtmlExportOptions options = new HtmlExportOptions();
                    options.EmbedImagesInHTML = true;
                    report.ExportToHtml(msHTML);
                    //report.ExportToPdf(msHTML);
                    result = Encoding.UTF8.GetString(msHTML.ToArray());
                }
            }
            return result;
        }

    }
}