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

        //public static string AppPath => ConfigurationManager.AppSettings["AppPath"];
        //public static void KepGonder(MyMailMessage myMsg)
        //{
        //    KEP_Mail_Message kepMsg = new KEP_Mail_Message();
        //    kepMsg.permitTestKEPAddress(true);
        //    kepMsg.addKepFrom("Tufan", "merkez.test@kep.turkkep.com.tr");
        //    foreach (var reciever in myMsg.Recievers)
        //    {
        //        //kepMsg.addKepTo("", reciever);
        //    }
        //    kepMsg.addKepSubject(myMsg.Subject);
        //    kepMsg.addKepIletiID("");
        //    kepMsg.addKepIletiTip("standart");
        //    kepMsg.addKepBodyHTML_UTF8(myMsg.Body);
        //    KepMailMessageSign(kepMsg);
        //    var EMLKepIletisi = kepMsg.getKEPIletiToString();

        //    var kepGonderimDogrulama = new KEPGonderim.authInput()
        //    {
        //        islemYetkiliKodu = "001",
        //        kullaniciAdi = "merkez.test@kep.turkkep.com.tr",
        //        kullaniciSifresi = "My6031*+"
        //    };
        //    var base64EML = Convert.ToBase64String(Encoding.UTF8.GetBytes(EMLKepIletisi));

        //    var kepIcerigi = new KEPGonderim.postaIcerik()
        //    {
        //        iletiIcerigi = base64EML//EML
        //    };


        //    KEPGonderim.KEPGonderimClient kepClient = new KEPGonderim.KEPGonderimClient();
        //    var result = kepClient.KEPGonderimi(kepGonderimDogrulama, kepIcerigi);
        //}
        //public static KEP_Mail_Message KepMailMessageSign(KEP_Mail_Message msg)
        //{
        //    msg.kep_imza.setTokenPIN("9687");
        //    msg.kep_imza.setPolicyPath(AppPath + @"config\certval-policy-test.xml");
        //    msg.kep_imza.setEsyaLicensePath(AppPath + @"lisans\lisans.xml");
        //    var res = msg.kep_imza.getListOfTerminalsAndCertInfo();
        //    msg.kep_imza.setTerminal(res[0].Key);
        //    msg.kepImzala();
        //    return msg;
        //}
        //public static void PerformSendTest()
        //{
        //    KEP_Mail_Message testMsg = new KEP_Mail_Message();
        //    testMsg.cleanKEPMessageContents();
        //    testMsg.kep_imza.setTokenPIN("9687");
        //    testMsg.kep_imza.setPolicyPath(AppPath + @"\config\certval-policy-test.xml");
        //    testMsg.kep_imza.setEsyaLicensePath(AppPath + @"\lisans\lisans.xml");

        //    var res = testMsg.kep_imza.getListOfTerminalsAndCertInfo();
        //    testMsg.permitTestKEPAddress(true);
        //    testMsg.addKepFrom("Tufan", "merkez.test@kep.turkkep.com.tr");
        //    testMsg.addKepTo("", "tufan@merkez.com.tr");
        //    testMsg.addKepTo("", "test@kep.turkkep.com.tr");
        //    testMsg.addKepSubject("Konu deneme");
        //    testMsg.addKepIletiID("");
        //    testMsg.addKepIletiTip("standart");
        //    testMsg.addKepBodyText_UTF8("Deneme İçeriği");
        //    testMsg.addKepBodyHTML_UTF8("");
        //    //testMsg.addKepBodyAttachment
        //    testMsg.kep_imza.setTerminal(res[0].Key);
        //    testMsg.kepImzala();
        //    var EMLKepIletisi = testMsg.getKEPIletiToString();
        //    var kepGonderimDogrulama = new KEPGonderim.authInput()
        //    {
        //        islemYetkiliKodu = "001",
        //        kullaniciAdi = "merkez.test@kep.turkkep.com.tr",
        //        kullaniciSifresi = "My6031*+"
        //    };
        //    var base64EML = Convert.ToBase64String(Encoding.UTF8.GetBytes(EMLKepIletisi));

        //    var kepIcerigi = new KEPGonderim.postaIcerik()
        //    {
        //        iletiIcerigi = base64EML//EML
        //    };


        //    KEPGonderim.KEPGonderimClient kepClient = new KEPGonderim.KEPGonderimClient();
        //    var result = kepClient.KEPGonderimi(kepGonderimDogrulama, kepIcerigi);
        //}

        //public static long ProcessInbox(KEPAlim.mailListOutput[] mailList)
        //{
        //    var result =
        //        mailList
        //        .GroupBy(x => x.kepUAMessageIdentifier)
        //        .Select(g => g.OrderByDescending(y => y.kepUID)
        //            .First()
        //         );
        //    var max = result.Max(m => m.kepUID);
        //    var min = result.Min(m => m.kepUID);
        //    bool transactionFailure = false;
        //    foreach (var item in result)
        //    {
        //        try
        //        {
        //            DelilTipi tip;
        //            var kepEvidenceType = item.kepEvidenceType;
        //            var evidenceCodePart = item.kepEventCode.Split('#');
        //            var evidenceCode = evidenceCodePart[1];
        //            if (kepEvidenceType == "SubmissionAcceptanceRejection")
        //            {
        //                if (evidenceCode == "Acceptance")
        //                {
        //                    tip = DelilTipi.SubmissionAcceptance;
        //                }
        //                else
        //                {
        //                    tip = DelilTipi.SubmissionRejection;
        //                }
        //            }
        //            else if (kepEvidenceType == "REMMDAcceptanceRejection")
        //            {
        //                if (evidenceCode == "Acceptance")
        //                {
        //                    tip = DelilTipi.REMMDAcceptance;
        //                }
        //                else
        //                {
        //                    tip = DelilTipi.REMMDRejection;
        //                }
        //            }
        //            else if (kepEvidenceType == "RelayToREMMDFailure")
        //            {
        //                tip = DelilTipi.RelayToREMMDFailure;
        //            }
        //            else if (kepEvidenceType == "DeliveryNonDeliveryToRecipient")
        //            {
        //                if (evidenceCode == "Delivery")
        //                {
        //                    tip = DelilTipi.DeliveryToRecipient;
        //                }
        //                else
        //                {
        //                    tip = DelilTipi.NonDeliveryToRecipient;
        //                }
        //            }
        //            else if (kepEvidenceType == "RetrievalNonRetrievalByRecipient")
        //            {
        //                if (evidenceCode == "Retrieval")
        //                {
        //                    tip = DelilTipi.RetrievalByRecipient;
        //                }
        //                else
        //                {
        //                    tip = DelilTipi.NonRetrievalByRecipient;
        //                }
        //            }
        //            else
        //            {
        //                tip = DelilTipi.Unknown;
        //            }
        //            int KepStatus = DBHelper.GetKepStatusByMessageId(item.kepUAMessageIdentifier);
        //            if (KepStatus > 0)
        //            {
        //                if (KepStatus>(int)tip)
        //                {
        //                    continue;
        //                }
        //            }
        //            //item.kepSendDate
        //            DBHelper.UpdateKepStatusByMessageId(item.kepUAMessageIdentifier, (int)tip,item.kepSendDate);
        //        }
        //        catch (Exception ex)
        //        {
        //            transactionFailure = true;
        //        }
        //    }
        //    if(!transactionFailure)
        //    {
        //        DBHelper.SetKepUIDLast(max.ToString());
        //        return max;
        //    }
        //    return min;
        //}
    }
}