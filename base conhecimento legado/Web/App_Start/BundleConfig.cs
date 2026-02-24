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
                "~/Scripts/jquery.unobtrusive-ajax.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/umd/popper.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/jquery.signalR-2.4.1.js",
                "~/Scripts/jquery-validate.bootstrap-tooltip.js",
                "~/Scripts/template/coreui/coreui-pro/js/coreui.min.js",
                "~/Scripts/jquery-confirm.min.js",
                "~/Scripts/jquery.mask.js",
                "~/Scripts/funcoes.js",
                "~/Scripts/perfil.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Scripts/template/coreui/icons/css/coreui-icons.min.css",
                "~/Scripts/template/flag-icon-css/css/flag-icon.min.css",
                "~/Scripts/template/font-awesome/css/font-awesome.min.css",
                "~/Scripts/template/simple-line-icons/css/simple-line-icons.css",
                "~/Content/style.css",
                "~/Content/jquery-confirm.min.css",
                "~/Content/erros.css",
                "~/Content/Site.css"));
        }
    }
}
