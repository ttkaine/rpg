using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Warhammer.Core.Abstract;

namespace Warhammer.Mvc
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DatabaseUpdate();
        }

        private void DatabaseUpdate()
        {
            IApplicationInitalizeHandler initalizeHandler =
                DependencyResolver.Current.GetService<IApplicationInitalizeHandler>();
            string dbUpdatePath = Server.MapPath("~/Content/DbUpdateScripts");
            try
            {
                DatabaseUpdateResult result = initalizeHandler.UpdateDatabase(dbUpdatePath);

                CreateDatabaseUpdateLogFile(result);

                if (!result.Successful)
                {
                    AppOffline(string.Join("<br />", result.ErrorMessages));
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                AppOffline(message);
            }
        }

        private void CreateDatabaseUpdateLogFile(DatabaseUpdateResult result)
        {
            string applcationOfflineLog = Server.MapPath("~/databaseupdate.log");
            string newline = Environment.NewLine;
            string errors = $"{newline}{newline} Errors: {newline}{string.Join(newline, result.ErrorMessages)}";
            string debug = $"{newline}{newline} Debug: {newline}{string.Join(newline, result.DebugMessages)}";
            string versions = $"{newline}{newline} Database Versions: {newline}{string.Join(newline, result.Versions.Select(v => $"{v.Id} - {v.DateTime.ToShortDateString()} - {v.Comment}"))}";
            string fileData = $"{DateTime.Now}{newline}{errors}{newline}{debug}{newline}{versions}{newline}";
            File.WriteAllText(applcationOfflineLog, fileData);
        }

        private void AppOffline(string message)
        {
            string applcationOfflineLog = Server.MapPath("~/app_offline.log");
            File.WriteAllText(applcationOfflineLog, message);
            string dataUpdateErrorPage = Server.MapPath("~/StaticPages/DatabaseError.html");
            string databaseFailPage = File.ReadAllText(dataUpdateErrorPage);
            databaseFailPage = databaseFailPage.Replace("_MESSAGE_", message);
            string applcationOfflinePage = Server.MapPath("~/app_offline.htm");
            File.WriteAllText(applcationOfflinePage, databaseFailPage);
        }
    }
}
