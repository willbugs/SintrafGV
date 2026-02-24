using System.Web.Optimization;

namespace Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate.js",
                "~/Scripts/jquery.validate.unobtrusive.js",
                "~/Scripts/jquery.unobtrusive-ajax.min.js",
                "~/Scripts/moment.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/umd/popper.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/jquery-validate.bootstrap-tooltip.js",
                "~/Scripts/jquery-confirm.min.js",
                "~/Scripts/jquery.mask.js",
                "~/Scripts/funcoes.js"));


            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/assets/css/Footer-Basic.css",
                "~/Content/assets/css/Header-Blue.css",
                "~/Content/assets/css/Login-Form-Clean.css",
                "~/Content/assets/css/Registration-Form-with-Photo.css",
                "~/Content/assets/css/styles.css",
                "~/Content/jquery-confirm.min.css",
                "~/Content/erros.css",
                "~/Content/Site.css"));
        }
    }
}
