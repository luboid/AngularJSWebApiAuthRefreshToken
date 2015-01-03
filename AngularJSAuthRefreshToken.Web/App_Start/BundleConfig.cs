using System.Collections.Generic;
using System.Web.Optimization;

namespace AngularJSAuthRefreshToken.Web
{
    public static class BundleConfig
    {
        #region AppJSFirstOrdering
        public class AppJSFirstOrdering : IBundleOrderer
        {
            public System.Collections.Generic.IEnumerable<BundleFile> OrderFiles(BundleContext context, System.Collections.Generic.IEnumerable<BundleFile> files)
            {
                var buffer = new List<BundleFile>(); var buffering = true;
                foreach (var file in files)
                {
                    //if (file.VirtualFile.Name.StartsWith("messages.", System.StringComparison.InvariantCultureIgnoreCase))
                    //{
                    //    continue;
                    //}
                    if (buffering && file.VirtualFile.Name.StartsWith("app.js", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        buffering = false;
                        yield return file;
                        foreach (var j in buffer)
                        {
                            yield return file;
                        }
                        continue;
                    }

                    if (buffering)
                    {
                        buffer.Add(file);
                    }
                    else
                    {
                        yield return file;
                    }
                }
            }
        }

        public static Bundle SetAppJSFirstOrdering(this Bundle bundle)
        {
            bundle.Orderer = new AppJSFirstOrdering();
            return bundle;
        }
        #endregion

        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-theme.css",
                      "~/Content/loading-bar.css",
                      "~/Content/angular-csp.css",
                      "~/Content/angular-toastr.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/angularjs")
                .Include("~/scripts/angular.js")
                .Include("~/scripts/angular-sanitize.js")
                .Include("~/scripts/angular-animate.js")
                .Include("~/scripts/angular-ui/ui-bootstrap-tpls.js")
                .Include("~/scripts/angular-ui-router.js")
                .Include("~/scripts/loading-bar.js")
                .Include("~/scripts/angular-toastr.js")
                .Include("~/scripts/ngtoast.js")
                .Include("~/scripts/i18n/angular-locale_bg-bg.js"));

            bundles.Add(new ScriptBundle("~/bundles/appjs")
                .IncludeDirectory("~/app", "*.js", true)
                .SetAppJSFirstOrdering());

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            // BundleTable.EnableOptimizations = true;
        }
    }
}
