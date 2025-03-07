using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Web.Hosting;

namespace C3D.Extensions.SystemWeb.OpenTelemetry.Application;

public class OpenTelemeteryApplication : System.Web.HttpApplication
{

    protected virtual void Application_Start()
    {
        var services = CreateServiceCollection();

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
                ot.IncludeScopes = true;
                ot.ParseStateValues = true;
            });
        });

        services.AddOpenTelemetry()
            .WithTracing(ConfigureTracing)
            .WithMetrics(ConfigureMetrics)
            .WithLogging(ConfigureLogging)
        .UseOtlpExporter();

        ConfigureServiceProvider(services);

        var serviceProvider = services.BuildServiceProvider();

        UseServiceProvider(serviceProvider);

        HostingEnvironment.RegisterObject(new OpenTelemetryRunner(serviceProvider));
    }

    protected virtual void ConfigureLogging(LoggerProviderBuilder logging)
    {

    }

    protected virtual void ConfigureMetrics(MeterProviderBuilder metrics)
    {
        metrics
            .AddAspNetInstrumentation()
            .AddRuntimeInstrumentation();
    }

    protected virtual void ConfigureTracing(TracerProviderBuilder tracing)
    {
        tracing
            .AddAspNetInstrumentation()
            .AddHttpClientInstrumentation();
    }

    protected virtual ServiceCollection CreateServiceCollection() => new ServiceCollection();

    protected virtual void ConfigureServiceProvider(ServiceCollection services)
    {
    }

    protected virtual void UseServiceProvider(ServiceProvider serviceProvider)
    {
    }
}
