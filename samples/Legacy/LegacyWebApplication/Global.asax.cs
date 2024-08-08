using Microsoft.Extensions.DependencyInjection;
using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace LegacyWebApplication
{
    public class MvcApplication : C3D.Extensions.SystemWeb.OpenTelemetry.Application.OpenTelemeteryApplication
    {
        private IServiceProvider serviceProvider;
        protected override void Application_Start()
        {
            base.Application_Start();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected override void UseServiceProvider(ServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            base.UseServiceProvider(serviceProvider);
        }
    }
}
