using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace gdocerous
{

    public class MvcApplication : System.Web.HttpApplication
    {
        public const string APPNAME = "gDocerous";

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("documents", "{action}/{folder}/{document}", new { controller = "Default", action = "Index", folder = UrlParameter.Optional, document = UrlParameter.Optional });
            routes.MapRoute("folders", "{action}/{folder}", new { controller = "Default", action = "Index", folder = UrlParameter.Optional } );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}