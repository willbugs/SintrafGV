using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Web.InfraEstrutura;

namespace Web
{
    public class MvcApplication : HttpApplication
    {
        [Obsolete("Obsolete")]
        protected void Application_Start()
        {
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;
            DependencyResolver.SetResolver(new NinjectDependencyResolver(virtualPath));
            FilterProviders.Providers.Clear();
            FilterProviders.Providers.Add(new FiltroProviderCustom());
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
