using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;
using KepNotificationDev.Helpers;
using KepNotificationDev.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KepNotificationDev
{
    public class TemplateStorage : ReportStorageWebExtension
    {
        public override bool CanSetData(string url)
        {
            return true;
        }
        
        public override bool IsValidUrl(string url)
        {
            return true;
        }

        public override byte[] GetData(string url)
        {
            var template = DBHelper.TemplateGetByCode(url);
            XtraReport report = new XtraReport();
//#if DEBUG
//            using (MemoryStream ms = new MemoryStream(template.Content))
//            {
//                report = new XtraReport();
//                report.LoadLayout(ms);
//                report.DisplayName = url;
//            }
//            using (MemoryStream ms = new MemoryStream())
//            {
//                Template temp = new Template();
//                report.SaveLayout(ms);
//                return ms.ToArray();
//            }
//#endif
            return template.Content;
        }

        public override Dictionary<string, string> GetUrls()
        {
            var res = new Dictionary<string, string>();
            var templates = DBHelper.GetTemplates();
            foreach (var item in templates)
            {
                res.Add(item.Code, item.Code);
            }
            return res;
        }

        public override void SetData(XtraReport report, string url)
        {
            string parameterName = (report.DataSource as DevExpress.DataAccess.Sql.SqlDataSource).Queries?.First().Parameters?.First().Name ?? "";
            string refTable = GetReferansTableFromParameter(parameterName);
            int id = Session.Reports.FirstOrDefault(x => x.Value == url).Key;
            using (MemoryStream ms = new MemoryStream())
            {
                Template temp = new Template();
                report.DisplayName = url;
                report.SaveLayout(ms);
                //DBHelper.TemplateUpdate(id, ms.ToArray(),refTable);
                DBHelper.TemplateUpdate(url, ms.ToArray(), refTable);
            }
        }

        public override string SetNewData(XtraReport report, string defaultUrl)
        {
            if (Session.Reports.ContainsValue(defaultUrl))
            {
                SetData(report, defaultUrl);
                return defaultUrl;
            }
            using (MemoryStream ms = new MemoryStream())
            {
                
                string parameterName = (report.DataSource as DevExpress.DataAccess.Sql.SqlDataSource).Queries?.First().Parameters?.First().Name ?? "";
                string refTable = GetReferansTableFromParameter(parameterName);
                
                Template temp = new Template();
                report.DisplayName = defaultUrl;
                report.SaveLayout(ms);
                temp.Code = defaultUrl;
                temp.Info = defaultUrl;
                temp.Content = ms.ToArray();
                temp.RefTable = refTable;
                DBHelper.TemplateInsert(temp);
            }
            return defaultUrl;//base.SetNewData(report, defaultUrl);
        }
        string GetReferansTableFromParameter(string parameterName)
        {
            string refTable = "";
            if (parameterName.Contains("SUB"))
            {
                refTable = "tb_SUBSCRIPTION";
            }
            else if (parameterName.Contains("ACCOUNT"))
            {
                refTable = "tb_ACCOUNTS";
            }
            return refTable;
        }
    }
}
