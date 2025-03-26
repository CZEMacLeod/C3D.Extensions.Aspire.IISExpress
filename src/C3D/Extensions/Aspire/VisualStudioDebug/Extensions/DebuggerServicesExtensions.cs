using C3D.Extensions.Aspire.VisualStudioDebug;
using C3D.Extensions.VisualStudioDebug;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aspire.Hosting;

public static class DebuggerServicesExtensions
{
    public static IServiceCollection AddAttachDebuggerHook(this IServiceCollection services)
    {
        if (OperatingSystem.IsWindows())
        {
            services
                .InsertHostedService<AttachDebuggerHook>()
                .AddOptions<DebuggerHookOptions>()
                .BindConfiguration("DebuggerHook");
            services.TryAddSingleton<VisualStudioInstances>();
        }
        return services;

    }
}
