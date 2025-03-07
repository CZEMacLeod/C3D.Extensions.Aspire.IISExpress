using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Web.Hosting;

namespace C3D.Extensions.SystemWeb.OpenTelemetry.Application;

internal class OpenTelemetryRunner : IRegisteredObject
{
    private readonly ServiceProvider serviceProvider;

    public OpenTelemetryRunner(ServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        _ = serviceProvider.GetService<MeterProvider>();
        _ = serviceProvider.GetService<TracerProvider>();
        _ = serviceProvider.GetService<LoggerProvider>();
    }

    public void Stop(bool immediate)
    {
        serviceProvider.Dispose();
        HostingEnvironment.UnregisterObject(this);
    }
}
