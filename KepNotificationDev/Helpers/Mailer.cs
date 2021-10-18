using KepNotificationDev.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace KepNotificationDev.Helpers
{
    public class MailUser
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SMTP { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
    }
    public static class Mailer
    {
        public static MailUser user = new MailUser();
        public static bool LoadMailUser()
        {
            var result = true;
            var data = DBHelper.GetJobNotifyMailUser();
            foreach (DataRow R in data.Rows)
            {
                try
                {

                    if (R["CODE"].ToString() == "JobEmailUser")
                    {
                        user.UserName = R["VALUE"].ToString();
                    }
                    else
                    if (R["CODE"].ToString() == "JobEmailPassword")
                    {
                        user.Password = R["VALUE"].ToString();
                    }
                    else
                    if (R["CODE"].ToString() == "JobEmailSMTP")
                    {
                        user.SMTP = R["VALUE"].ToString();
                    }
                    else
                    if (R["CODE"].ToString() == "JobEmailPort")
                    {
                        user.Port = Convert.ToInt32(R["VALUE"]);
                    }
                    else
                    if (R["CODE"].ToString() == "JobEmailSSL")
                    {
                        user.UseSSL = Convert.ToBoolean(R["VALUE"]);
                    }
                }
                catch (Exception ex)
                {
                    result = false;
                }
            }
            return result;
        }
        public static bool SendJobNotification(string recievers,string jobTitle,List<MessageLog> create,int count=0)
        {
            var result = false;
            MailMessage msg = new MailMessage
            {
                From = new MailAddress(user.UserName),
                Subject = jobTitle + " Görevli İş Çalıştırıldı",
                Body = jobTitle+ " adlı görev "+DateTime.Now+" tarihinde çalışmış olup "+count+" adet ileti oluşturulmuştur özeti aşığıdadır.\r\n"
                        +GetMyTable(create,x=>x.Receiver,x=>x.Subject),
                SubjectEncoding = Encoding.UTF8,
                HeadersEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };
            var Recievers = recievers.Split(new char[] { ';', ':', '/', '*' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Recievers.Length; i++)
            {
                msg.To.Add(Recievers[i]);
            }
            msg.Bcc.Add("tufan@merkez.com.tr");
            using (SmtpClient Smtp = new SmtpClient(user.SMTP, user.Port))
            {
                var Credential = new NetworkCredential
                {
                    UserName = user.UserName,
                    Password = user.Password
                };
                Smtp.Credentials = Credential;
                Smtp.EnableSsl = user.UseSSL;
                try
                {

                    Smtp.Send(msg);
                }
                catch (System.Exception ex)
                {
                    result = false;
                }

            }

            return result;
        }
        public static string GetMyTable<T>(IEnumerable<T> list, params Func<T, object>[] fxns)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append(@"<table style=""border: 1px solid black"">"+"\n");
            foreach (var item in list)
            {
                sb.Append(@"<tr style=""border: 1px solid black"">" + "\n");
                foreach (var fxn in fxns)
                {
                    sb.Append(@"<td style=""border: 1px solid black"">" + "\n");
                    sb.Append(fxn(item));
                    sb.Append("</td>");
                }
                sb.Append("</tr>\n");
            }
            sb.Append("</table>");

            return sb.ToString();
        }

    }
}