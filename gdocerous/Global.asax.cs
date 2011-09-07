using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MvcMiniProfiler;
using gdocerous.Code;

namespace gdocerous
{

    public class MvcApplication : System.Web.HttpApplication
    {
        public const string APPNAME = "gdocerous";

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("error", "Error/{action}", new { controller = "Error", action = "General", exception = UrlParameter.Optional });

            routes.MapRoute("folders", "{action}/{folder}", new { controller = "Default", action = "Index", folder = UrlParameter.Optional });
            routes.MapRoute("document", "{action}/{document}", new { controller = "Default", action = "Index", document = UrlParameter.Optional });
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            //http://stackoverflow.com/questions/6648249/c-mini-mvc-profiler-appears-to-be-displaying-profile-times-for-every-static-res
            var ignore = MiniProfiler.Settings.IgnoredPaths.ToList();
            ignore.Add("/Content/");
            ignore.Add("/script/");
            ignore.Add(".js");
            ignore.Add("ScriptResource.axd");
            ignore.Add("WebResource.axd");
            ignore.Add("/App_Themes/");

            MiniProfiler.Settings.IgnoredPaths = ignore.ToArray();
            MiniProfiler.Settings.PopupRenderPosition = RenderPosition.Right;
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            MiniProfiler.Start();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            MiniProfiler.Stop();
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
            var httpException = exception as HttpException;

            Elmah.ErrorLog.GetDefault(HttpContext.Current).Log(new Elmah.Error(exception));

            Response.Clear();
            Server.ClearError();

            var routeData = new RouteData();
            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = "General";
            routeData.Values["exception"] = exception;
            Response.StatusCode = 500;
            if (httpException != null)
            {
                Response.StatusCode = httpException.GetHttpCode();
                switch (Response.StatusCode)
                {
                    case 404:
                        routeData.Values["action"] = "Http404";
                        break;
                }
            }
            // Avoid IIS7 getting in the middle
            Response.TrySkipIisCustomErrors = true;
            IController errorsController = new gdocerous.Controllers.ErrorController();
            HttpContextWrapper wrapper = new HttpContextWrapper(Context);
            var rc = new RequestContext(wrapper, routeData);
            errorsController.Execute(rc);
        }
    }
}