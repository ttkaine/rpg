using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using FluentScheduler;
using Warhammer.Core.Abstract;
using Warhammer.Mvc.Concrete;
using Warhammer.Mvc.Controllers;
using Warhammer.Mvc.Helpers;

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

            IExceptionLogHandler log = DependencyResolver.Current.GetService<IExceptionLogHandler>();

            JobManager.Initialize(new FluentSchedulerRegistry());
            JobManager.JobException += (info) => log.LogException(info.Exception, "JobManager", 99, DateTime.Now);
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
            string versions = "No Version Info Available";
            if (result.Versions != null)
            {
                versions = $"{newline}{newline} Database Versions: {newline}{string.Join(newline, result.Versions.Select(v => $"{v.Id} - {v.DateTime.ToShortDateString()} - {v.Comment}"))}";
            }
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

        protected void Application_Error(object sender, EventArgs e)
        {
            if (CustomErrorsAreOff())
            {
                return;
            }

            var httpContext = ((MvcApplication)sender).Context;
            var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
            var currentController = " ";
            var currentAction = " ";

            if (currentRouteData != null)
            {
                if (!string.IsNullOrEmpty(currentRouteData.Values["controller"]?.ToString()))
                {
                    currentController = currentRouteData.Values["controller"].ToString();
                }

                if (!string.IsNullOrEmpty(currentRouteData.Values["action"]?.ToString()))
                {
                    currentAction = currentRouteData.Values["action"].ToString();
                }
            }

            var exception = Server.GetLastError();
            exception = exception.LogExceptionSequence(DateTime.Now, new HttpRequestWrapper(Context.Request));

            var controller = new ErrorController();
            var routeData = new RouteData();

            httpContext.ClearError();
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = (exception as HttpException)?.GetHttpCode() ?? 500;
            httpContext.Response.TrySkipIisCustomErrors = true;

            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = "Index";

            controller.ViewData.Model = new HandleErrorInfo(exception, currentController, currentAction);
            ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
        }

        private static bool CustomErrorsAreOff()
        {
            CustomErrorsSection section =
                ConfigurationManager.GetSection("system.web/customErrors") as CustomErrorsSection;

            return section != null && section.Mode == CustomErrorsMode.Off;
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (!Context.Request.IsSecureConnection && !Request.Url.Host.Contains("localhost"))
            {
                Response.Redirect(Request.Url.AbsoluteUri.Replace("http://", "https://"));
            }
        }
    }
}
