using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using KepNotificationDev.Helpers;
using KepNotificationDev.Helpers.Service;
using KepNotificationDev.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KepService
{
    public static class Services
    {

        static string islemYetkiliKodu = ConfigurationManager.AppSettings["islemYetkiliKodu"];
        static string kullaniciAdi = ConfigurationManager.AppSettings["kullaniciAdi"];
        static string kullaniciSifresi = ConfigurationManager.AppSettings["kullaniciSifresi"];
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
                    result = Encoding.UTF8.GetString(msHTML.ToArray());
                }
            }
            return result;
        }
        public static int ConvertReportToAttachment(int subscriberId, Template template)
        {
            int result = 0;
            using (MemoryStream ms = new MemoryStream(template.Content))
            {
                XtraReport report = new XtraReport();
                report.LoadLayout(ms);
                report.Parameters[0].Value = subscriberId;
                using (MemoryStream msPdf = new MemoryStream())
                {
                    HtmlExportOptions options = new HtmlExportOptions();
                    options.EmbedImagesInHTML = true;
                    report.ExportToPdf(msPdf);
                    var byteAttachment = msPdf.ToArray();
                    result = DBHelper.AttachmentInsert(template.Code + ".pdf", byteAttachment, "Servis ile otomatik oluşturulan ek dosyası");
                }
            }
            return result;
        }
        public static List<Job> Jobs => DBHelper.GetJobs().ToList();
        static Services()
        {
            //new Thread(Service).Start();
            //new Thread(SenderService).Start();
            //new Thread(StatusCheckService).Start();
        }

        public static bool IsServiceRunning { get; internal set; } = true; // başlar başlamaz çalışsın mı
        public static bool IsSenderServiceRunning { get; internal set; } = false; // başlar başlamaz çalışsın mı
        public static bool IsCheckServiceRunning { get; internal set; } = false; // başlar başlamaz çalışsın mı


        public static void JobService()
        {
            Task.Run(() => Service());
        }

        public static void SenderStart()
        {
            Task.Run(() => SenderService());

        }
        public static void StatusCheckStart()
        {
            Task.Run(() => StatusCheckService());

        }


        internal static void Execute(this Job job)
        {
            var keplogs = new List<MessageLog>();
            var dataset = DBHelper.DatasetGetById(job.ReceiverId);
            var Subscibers = DBHelper.GetRecieversDatatable(dataset.Dataset);
            var recieversRows = Subscibers.AsEnumerable();
            bool subscriptionReciever = dataset.ReferanceTable == "tb_SUBSCRIPTION";
            if (recieversRows.Count() > 0)
            {
                if (!subscriptionReciever)
                {
                    recieversRows = recieversRows.Where(r =>
                       !string.IsNullOrWhiteSpace(r.Field<string>("KEP_MAIL")) ||
                       !string.IsNullOrWhiteSpace(r.Field<string>("Gsm")) ||
                       !string.IsNullOrWhiteSpace(r.Field<string>("EMail"))
                       ); 
                }
                else
                {
                    recieversRows = recieversRows.Where(r =>
                       !string.IsNullOrWhiteSpace(r.Field<string>("KEP_MAIL")) ||
                       !string.IsNullOrWhiteSpace(r.Field<string>("Gsm")) ||
                       !string.IsNullOrWhiteSpace(r.Field<string>("EMail")) ||
                       !string.IsNullOrWhiteSpace(r.Field<string>("Gsm2")) ||
                       !string.IsNullOrWhiteSpace(r.Field<string>("Gsm3")) ||
                       !string.IsNullOrWhiteSpace(r.Field<string>("ACCOUNT_EMAIL"))
                       );
                }
                if (recieversRows.ToList().Count() <= 0)
                {
                    return;
                }
                var Template = DBHelper.TemplateGetById(job.ReportId);
                var recievers = recieversRows.ToList().CopyToDataTable();

                List<int> createdRefIds = new List<int>();
                string columnName = "ID";
                //string kepColumnName = "KEP_MAIL";
                try
                {
                    if (recievers.Rows[0]["SUBSCRIPTION_ID"] != null || recievers.Rows[0]["SUBSCRIPTION_ID"] != DBNull.Value)
                    {
                        columnName = "SUBSCRIPTION_ID";
                    }
                    else
                    {
                    }
                }
                catch (Exception)
                {
                    return;
                }
                int logCount = 0;
                foreach (DataRow itemRow in recievers.Rows)
                {
                    bool autoSend = job.AutoSend;
                    int refId = Convert.ToInt32(itemRow[columnName]);
                    int attachId = ConvertReportToAttachment(refId, Template);
                    string tmpReceiver = "";
                    var temp = new MessageLog()
                    {
                        Subject = job.Info,
                        RefTable = dataset.ReferanceTable,
                        Content = "İletim servisi tarafından oluşturunal\r\n" + job.Info + "konulu iletidir.",
                        RefId = refId,
                        JobId = job.Id,
                        JobInfo = job.Info,
                        StatusType = autoSend ? JobStatus.Signed : JobStatus.Draft,
                        HasAttachments = true,
                        CreatedBy="Service",
                        CreatedDate=DateTime.Now,
                    };
                    if ((itemRow["KEP_MAIL"] != null && itemRow["KEP_MAIL"].ToString() != "") || itemRow["KEP_MAIL"] != DBNull.Value)
                    {
                        temp.StatusType = JobStatus.Draft;
                        temp.Receiver = itemRow["KEP_MAIL"].ToString();
                        temp.MessageChannel = MessageChannels.KepMail;
                        int insertedId = DBHelper.MessageLogInsert(temp, "Servis");
                        DBHelper.MessegerLogAttachmentAdd(insertedId, attachId);
                        tmpReceiver += temp.Receiver + ";";
                        logCount++;
                    }
                    if ((itemRow["Gsm"] != null && itemRow["Gsm"].ToString() !="") || itemRow["Gsm"] != DBNull.Value)
                    {
                        temp.Receiver = itemRow["Gsm"].ToString();
                        temp.MessageChannel = MessageChannels.ManagementGsm;
                        int insertedId = DBHelper.MessageLogInsert(temp, "Servis");
                        DBHelper.MessegerLogAttachmentAdd(insertedId, attachId);
                        tmpReceiver += temp.Receiver + ";";
                        logCount++;
                    }
                    if ((itemRow["EMail"] != null && itemRow["EMail"].ToString() != "") || itemRow["EMail"] != DBNull.Value)
                    {
                        temp.Receiver = itemRow["EMail"].ToString();
                        temp.MessageChannel = MessageChannels.Email;
                        int insertedId = DBHelper.MessageLogInsert(temp, "Servis");
                        DBHelper.MessegerLogAttachmentAdd(insertedId, attachId);
                        tmpReceiver += temp.Receiver + ";";
                        logCount++;
                    }
                    if (subscriptionReciever)
                    {
                        if ((itemRow["Gsm2"] != null && itemRow["Gsm2"].ToString() != "") || itemRow["Gsm2"] != DBNull.Value)
                        {
                            temp.Receiver = itemRow["Gsm2"].ToString();
                            temp.MessageChannel = MessageChannels.AccountGsm;
                            int insertedId = DBHelper.MessageLogInsert(temp, "Servis");
                            DBHelper.MessegerLogAttachmentAdd(insertedId, attachId);
                            tmpReceiver += temp.Receiver + ";";
                            logCount++;
                        }
                        if ((itemRow["Gsm3"] != null && itemRow["Gsm3"].ToString() != "") || itemRow["Gsm3"] != DBNull.Value)
                        {
                            temp.Receiver = itemRow["Gsm3"].ToString();
                            temp.MessageChannel = MessageChannels.TechnicGsm;
                            int insertedId = DBHelper.MessageLogInsert(temp, "Servis");
                            DBHelper.MessegerLogAttachmentAdd(insertedId, attachId);
                            tmpReceiver += temp.Receiver + ";";
                            logCount++;
                        }
                        if ((itemRow["ACCOUNT_EMAIL"] != null && itemRow["ACCOUNT_EMAIL"].ToString() != "") || itemRow["ACCOUNT_EMAIL"] != DBNull.Value)
                        {
                            temp.Receiver = itemRow["ACCOUNT_EMAIL"].ToString();
                            temp.MessageChannel = MessageChannels.AccountGsm;
                            int insertedId = DBHelper.MessageLogInsert(temp, "Servis");
                            DBHelper.MessegerLogAttachmentAdd(insertedId, attachId);
                            tmpReceiver += temp.Receiver + ";";
                            logCount++;
                        }
                    }
                    temp.Receiver = tmpReceiver.Substring(0, temp.Receiver.Length - 1);
                    keplogs.Add(temp);
                    createdRefIds.Add(refId);
                }
                var idRef = StringHelper.StringBetween(dataset.Dataset, "tb_KEP_RECORD_CONTROL", ")");
                if (idRef.Count > 0)
                {
                    var table = StringHelper.StringBetween(idRef[0], "DATASET_NAME='", "'");
                    DBHelper.InsertKepLogControl(table[0], createdRefIds);
                }
                try
                {
                    Mailer.LoadMailUser();
                    Mailer.SendJobNotification(job.RelevantAddress, job.Info, keplogs, logCount);
                }
                catch (Exception ex)
                {
                }
                Logger.WriteLine(job.Info + " işi çalıştı ve " + logCount + " adet ilet oluşturuldu.");
            }
            else
            {
                Logger.WriteLine(job.Info + " işi çalıştı ama ileti oluşturulacak kayıt bulunamadı");
                //ToDo: iş çalışltı ama gönderilecek abone bulunamadı
            }
        }

        public static void Service()
        {
            while (true)
            {
                while (IsServiceRunning)
                {
                    DateTime now = DateTime.Now;
                    foreach (var Job in Jobs)
                    {
                        Logger.WriteLine(Job.Info + " işine bakılıyor");
                        if (!Job.Active)
                        {
                            Logger.WriteLine(Job.Info + " işi Aktif değil");
                            continue;
                        }
                        if (Job.LastExecutedTime == null && Job.StartTime > now)
                        {
                            Logger.WriteLine(Job.Info + " işi Zamanı gelmemiş");
                            continue;
                        }
                        if (Job.LastExecutedTime == null && Job.StartTime < now)
                        {
                            Job.Execute();
                            Job.LastExecutedTime = now;
                            DBHelper.JobUpdate(Job);
                            continue;
                        }

                        TimeSpan span = now.Subtract((DateTime)Job.LastExecutedTime);
                        switch (Job.IntervalType)
                        {
                            case IntervalTypes.Seçiniz:
                                if (Job.StartTime < now)
                                {
                                    //Job.Execute();
                                }

                                break;
                            case IntervalTypes.Saat:
                                if (span.Hours > Job.Interval)
                                {
                                    Job.Execute();
                                    Job.LastExecutedTime = now;
                                    DBHelper.JobUpdate(Job);
                                }

                                break;
                            case IntervalTypes.Gün:
                                if (span.Days > Job.Interval)
                                {
                                    Job.Execute();
                                    Job.LastExecutedTime = now;
                                    DBHelper.JobUpdate(Job);
                                }

                                break;
                            case IntervalTypes.Ay:
                                var month = ((now.Year - Job.LastExecutedTime.Value.Year) * 12) + now.Month - Job.LastExecutedTime.Value.Month;
                                if (month > Job.Interval)
                                {
                                    Job.Execute();
                                    Job.LastExecutedTime = now;
                                    DBHelper.JobUpdate(Job);
                                }

                                break;
                            default:
                                break;
                        }
                        //var dataset = DBHelper.DatasetGetById(Job.DatasetId);
                        var template = DBHelper.TemplateGetById(Job.ReportId);
                    }

                    Thread.Sleep(60000);
                }
                Thread.Sleep(60000);
            }
        }

        public static void SenderService()
        {
            while (true)
            {
                while (IsSenderServiceRunning)
                {
                    var logs = DBHelper.GetMessageLogs(JobStatus.Signed);
                    if (logs.Count > 0)
                    {
                        KepGonderimi.KEPGonderimClient kepClient = new KepGonderimi.KEPGonderimClient();
                        var KepGonderimiDogrulama = new KepGonderimi.authInput()
                        {
                            islemYetkiliKodu = islemYetkiliKodu,
                            kullaniciAdi = kullaniciAdi,
                            kullaniciSifresi = kullaniciSifresi
                        };

                        for (int i = 0; i < logs.Count; i++)
                        {

                            if (logs[i].MessageChannel.HasFlag(MessageChannels.KepMail) || logs[i].MessageChannel.Equals(MessageChannels.KepMail))
                            {
                                var base64EML = Convert.ToBase64String(Encoding.UTF8.GetBytes(logs[i].SignedContent));
                                var kepIcerigi = new KepGonderimi.postaIcerik()
                                {
                                    iletiIcerigi = base64EML//EML
                                };
                                try
                                {
                                    var result = kepClient.KEPGonderimi(KepGonderimiDogrulama, kepIcerigi);
                                    logs[i].SentDatetime = DateTime.Now;
                                    if (result.sonucKodu == "200")
                                    {
                                        logs[i].StatusType = JobStatus.Sent;
                                        Logger.WriteLine(logs[i].Receiver + " alıcısına " + logs[i].Subject + "Konulu ilti gönderildi.");
                                    }
                                    else
                                    {
                                        logs[i].StatusType = JobStatus.Error;
                                        DBHelper.LogSentErrorInsert(result.sonucKodu, result.sonucu, "Servis", logs[i]);
                                        Logger.WriteLine(logs[i].Receiver + " alıcısına " + logs[i].Subject + "Konulu ilti gönderildi.", false);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DBHelper.LogSentErrorInsert(ex.HResult.ToString(), ex.Message.Substring(0, 99), "Exception", logs[i]);
                                } 
                            }else 
                            if(logs[i].MessageChannel.HasFlag(MessageChannels.AccountGsm) ||
                                logs[i].MessageChannel.HasFlag(MessageChannels.ManagementGsm) ||
                                logs[i].MessageChannel.HasFlag(MessageChannels.TechnicGsm) ||
                                logs[i].MessageChannel.Equals(MessageChannels.AccountGsm) ||
                                logs[i].MessageChannel.Equals(MessageChannels.ManagementGsm) ||
                                logs[i].MessageChannel.Equals(MessageChannels.TechnicGsm))
                            {
                                string TOKEN = logs[i].Token;
                                string receivers = logs[i].Receiver;
                                string smsContent = logs[i].Subject + "\r\n" + logs[i].Content + "website" + "Viewer/Viewlog?token=" + TOKEN;
                                //todo: sms servisi ekle Service.Send(receievers,smsContent);
                            }
                            else 
                            if(logs[i].MessageChannel.HasFlag(MessageChannels.Email) ||
                                logs[i].MessageChannel.HasFlag(MessageChannels.AccountEmail) ||
                                logs[i].MessageChannel.Equals(MessageChannels.Email) ||
                                logs[i].MessageChannel.Equals(MessageChannels.AccountEmail))
                            {

                            }
                        }
                        DBHelper.MessageLogUpdate(logs);
                    }
                    Thread.Sleep(5000);
                }
                Thread.Sleep(10000);
            }
        }

        public static void StatusCheckService()
        {
            while (true)
            {
                var retrievalClient = new KepAlimi.KEPRetrievalClient();

                while (IsCheckServiceRunning)
                {
                    var tomorrow = DateTime.Now.AddDays(1);
                    var aMounthEarly = tomorrow.AddMonths(-1);
                    var UID = DBHelper.GetKepUIDLast();
                    try
                    {

                        var res = retrievalClient.KEPPostaKutusuListesi(new KepAlimi.authInput()
                        {
                            islemYetkiliKodu = islemYetkiliKodu,
                            kullaniciAdi = kullaniciAdi,
                            kullaniciSifresi = kullaniciSifresi,
                        }, new KepAlimi.mailListInput()
                        {
                            baslangicTarihi = aMounthEarly.ToString("yyyy-MM-dd"),
                            bitisTarihi = tomorrow.ToString("yyyy-MM-dd"),
                            iletiTuru = "Message",
                            listelemeTuru = "ALL",
                        }, new KepAlimi.paging()
                        {
                            kepUIDStartFrom = UID,
                            kepUIDEndTo = UID + 20,
                        });

                        if (res.sonucKodu == "200" && res.mailList.Length > 0)
                        {
                            long UIDLast = KepHelper.ProcessInbox(res.mailList);
                        }
                    }
                    catch (Exception ex)
                    {
                        //todo:işle
                        //throw ex;
                        Logger.WriteLine(" Kep Delili sorgu hatası " + ex.Message);
                    }
                    Thread.Sleep(10000);
                }
            }
        }
    }

}