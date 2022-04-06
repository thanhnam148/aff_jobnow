using System.Web;
using System.Web.Optimization;

namespace Aff.WebApplication
{
    public class BundleConfig
    {
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
                      "~/assets/vendors/base/vendors.bundle.js",
                      "~/assets/demo/default/base/scripts.bundle.js",
                      "~/assets/vendors/custom/fullcalendar/fullcalendar.bundle.js",
                      "~/assets/app/js/dashboard.js",
                      "~/assets/scripts/default.js"));

            bundles.Add(new ScriptBundle("~/bundles/login").Include(
                      "~/assets/snippets/pages/user/login.js"));

            bundles.Add(new StyleBundle("~/assets/css").Include(
                      "~/assets/vendors/custom/fullcalendar/fullcalendar.bundle.css",
                      "~/assets/vendors/base/vendors.bundle.css",
                      "~/assets/demo/default/base/style.bundle.css",
                      "~/content/custom.css"));
        }
    }
}
