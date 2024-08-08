using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Web.Hosting;

namespace C3D.Extensions.SystemWeb.OpenTelemetry.Application
{
    internal class OpenTelemetryRunner : IRegisteredObject
    {
        private ServiceProvider serviceProvider;

        public OpenTelemetryRunner(ServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            var meterProvider = serviceProvider.GetService<MeterProvider>();
            var tracerProvider = serviceProvider.GetService<TracerProvider>();
            var loggerProvider = serviceProvider.GetService<LoggerProvider>();
        }

        public void Stop(bool immediate)
        {
            serviceProvider.Dispose();
            HostingEnvironment.UnregisterObject(this);
        }
    }
}
