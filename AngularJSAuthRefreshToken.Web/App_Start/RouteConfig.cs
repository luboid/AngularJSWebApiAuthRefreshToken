using System.Web.Mvc;
using System.Web.Routing;

namespace AngularJSAuthRefreshToken.Web
{
    public static class RouteConfig
    {
        public static readonly string AppPath = "ea";

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "AngularJS-App",
                url: RouteConfig.AppPath + "/{*pathInfo}",
                defaults: new { controller = RouteConfig.AppPath, action = "Index" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}