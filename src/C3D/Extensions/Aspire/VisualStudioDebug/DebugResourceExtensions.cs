using Aspire.Hosting.ApplicationModel;
using C3D.Extensions.Aspire.VisualStudioDebug;
using Microsoft.Extensions.DependencyInjection;
using C3D.Extensions.Aspire.VisualStudioDebug.Annotations;

namespace Aspire.Hosting;

public static class DebugResourceExtensions
{
    public static IServiceCollection AddAttachDebuggerHook(this IServiceCollection services) => 
        services.AddHostedService<AttachDebuggerHook>();

    public static IResourceBuilder<TResource> WithDebugger<TResource>(
        this IResourceBuilder<TResource> resourceBuilder,
        DebugMode debugMode = DebugMode.VisualStudio,
        params string[] engines)
        where TResource : ExecutableResource
    {
        resourceBuilder.ApplicationBuilder.Services.AddAttachDebuggerHook();
        return resourceBuilder
            .WithEnvironment("Launch_Debugger_On_Start",
                             debugMode == DebugMode.Environment ? "true" : null)
            .WithAnnotation<DebugAttachAnnotation>(new()
            {
                DebugMode = debugMode,
                Engines = engines //["Managed (.NET Framework 4.x)"]
            },
                ResourceAnnotationMutationBehavior.Replace);
    }

    internal static bool HasAnnotationOfType<T>(this IResource resource, Func<T, bool> predecate)
        where T : IResourceAnnotation => resource.Annotations.Any(a => a is T t && predecate(t));
}
