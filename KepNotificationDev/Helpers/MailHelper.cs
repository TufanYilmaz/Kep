using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace KepNotificationDev.Helpers
{
    public static class MailHelper
    {
        public static string MailAddress { get; set; } = "debug@gmail.com";
        public static string Password { get; set; } = "";
        public static string SMTP { get; set; } = "smtp.gmail.com";
        public static int Port { get; set; } = 465;
        //public static bool? UseSSL = false;

        public static void SendNotificationMail(string Reciever, string Subject, string Content)
        {
            MailMessage msg = new MailMessage
            {
                From = new MailAddress(MailAddress),
                Subject = Subject + " Görevli İş Çalıştırıldı",
                Body = Content,
                SubjectEncoding = Encoding.UTF8,
                HeadersEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };
            msg.To.Add(Reciever);
            using (SmtpClient Smtp = new SmtpClient(SMTP, Port))
            {
                var Credential = new NetworkCredential
                {
                    UserName = MailAddress,
                    Password = Password
                };
                Smtp.Credentials = Credential;
                try
                {
                   
                    Smtp.Send(msg);
                }
                catch (System.Exception ex)
                {
                }

            }
        }
        public static string ConvertDataTableToHTML(DataTable dt)
        {
            string html = "<table>";
            //add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<td>" + dt.Columns[i].ColumnName + "</td>";
            html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }
    }
}