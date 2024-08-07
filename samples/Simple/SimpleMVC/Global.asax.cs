using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

[assembly: System.Web.PreApplicationStartMethod(typeof(SimpleMVC.MvcApplication),
    nameof(SimpleMVC.MvcApplication.CheckDebugger))]

namespace SimpleMVC
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private ServiceProvider _serviceProvider;

        public static void CheckDebugger()
        {
            // This env variable is passed from Aspire as we don't have a clean way to attach the debugger to IIS Express
            // Use this if you don't enable the vsjitdebugger.exe hook
            if (System.Environment.GetEnvironmentVariable("Launch_Debugger_On_Start") == "true")
            {
                Debugger.Launch();
            }
        }

        protected void Application_Start()
        {
            var services = new ServiceCollection();

            services.AddLogging(logging =>
            {
                logging.Configure(options =>
                {
                    options.ActivityTrackingOptions =
                        ActivityTrackingOptions.SpanId |
                        ActivityTrackingOptions.TraceId |
                        ActivityTrackingOptions.ParentId;
                });
                logging.AddOpenTelemetry(ot =>
                {
                    ot.IncludeFormattedMessage = true;
                    ot.IncludeScopes= true;
                    ot.ParseStateValues = true;
                });
            });

            services.AddOpenTelemetry()
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetInstrumentation()
                        .AddHttpClientInstrumentation();
                })
                .WithMetrics(metrics=>
                {
                    metrics
                        .AddAspNetInstrumentation()
                        .AddAspNetInstrumentation()
                        .AddRuntimeInstrumentation();
                })
                .WithLogging(logging=>
                {
                })
            .UseOtlpExporter();

            _serviceProvider = services.BuildServiceProvider();

            var meterProvider = _serviceProvider.GetService<MeterProvider>();
            var tracerProvider = _serviceProvider.GetService<TracerProvider>();
            var loggerProvider = _serviceProvider.GetService<LoggerProvider>();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_End()
        {
            _serviceProvider.Dispose();
        }
    }
}
