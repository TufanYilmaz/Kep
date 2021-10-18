using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Wizard.Services;
using DevExpress.Security.Resources;
using DevExpress.XtraReports.Security;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.ReportDesigner;
using KepNotificationDev.Helpers.Service;
using System;
using System.Collections.Generic;
using System.IO;
//using System.Data;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace KepNotificationDev {
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication {
        protected void Application_Start() {
            DevExpress.XtraReports.Web.WebDocumentViewer.Native.WebDocumentViewerBootstrapper.SessionState = System.Web.SessionState.SessionStateBehavior.Disabled;
            DevExpress.XtraReports.Web.ReportDesigner.Native.ReportDesignerBootstrapper.SessionState = System.Web.SessionState.SessionStateBehavior.Disabled;
            DevExpress.XtraReports.Web.QueryBuilder.Native.QueryBuilderBootstrapper.SessionState = System.Web.SessionState.SessionStateBehavior.Disabled;
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            ModelBinders.Binders.DefaultBinder = new DevExpress.Web.Mvc.DevExpressEditorsBinder();

            DefaultReportDesignerContainer.Register<ICustomQueryValidator, MyCustomValidator>();
            DefaultReportDesignerContainer.EnableCustomSql();

            DevExpress.Web.ASPxWebControl.CallbackError += Application_Error;
            DevExpress.Web.Mvc.MVCxQueryBuilder.StaticInitialize();
            DevExpress.Web.Mvc.MVCxReportDesigner.StaticInitialize();

            DevExpress.Security.Resources.AccessSettings.ReportingSpecificResources.SetRules(DirectoryAccessRule.Allow("C:\\StaticResources\\"), SerializationFormatRule.Deny(SerializationFormat.CodeDom));

            DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension.RegisterExtensionGlobal(new TemplateStorage());
            //DefaultReportDesignerContainer.RegisterDataSourceWizardConfigFileConnectionStringsProvider();
            //JobService.Start();
            //JobService.SenderStart();
            
        }

        protected void Application_Error(object sender, EventArgs e) {
            Exception exception = System.Web.HttpContext.Current.Server.GetLastError();
            //TODO: Handle Exception
            //File.WriteAllText("C:\\kep_error.txt", exception.ToString());
        }
    }
    public class MyCustomValidator : ICustomQueryValidator
    {
        public bool Validate(DataConnectionParametersBase connectionParameters, string sql, ref string message)
        {
            return true;
        }
    }
}