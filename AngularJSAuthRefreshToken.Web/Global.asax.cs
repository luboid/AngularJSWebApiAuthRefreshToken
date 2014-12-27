using AngularJSAuthRefreshToken.Web.Models;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AngularJSAuthRefreshToken.Web
{
    // Note: For instructions on enabling IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=301868
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest()
        {
            if (Request.ApplicationPath == Request.Path && Request.HttpMethod == "GET")
            {
                //redirect to app where it behaves
                //and leave others exactly specified paths to do what they need to do
                Response.RedirectPermanent(string.Format("/{0}/", RouteConfig.AppPath), true);
            }

            Culture.Set();
        }
    }
}
