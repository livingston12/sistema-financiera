using System.Web;
using System.Web.Optimization;

namespace SistemaImbrino
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-ui.min.js"

                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/misScripts").Include(
                       "~/Scripts/GeneralScript.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));            

            bundles.Add(new Bundle("~/bundles/scripts").Include(                                   
                  "~/Scripts/SweetAlert2.js",
                  "~/Scripts/datatables.js",
                  "~/Scripts/select-mania.min.js",
                  "~/Scripts/sb-admin-2.min.js",
                   "~/Scripts/GeneralScript.js"
                  ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-theme.min.css",
                      "~/Content/Datatables.css",
                      "~/Content/site.css",
                      "~/Content/select-mania.min.css",
                      "~/Content/select-mania-theme-orange.css",
                      "~/Content/jquery-ui.min.css",
                      "~/Content/jquery-ui.theme.min.css"
                      ));
        }
    }
}
