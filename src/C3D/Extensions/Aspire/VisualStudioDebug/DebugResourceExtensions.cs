using Aspire.Hosting.ApplicationModel;
using C3D.Extensions.Aspire.VisualStudioDebug;
using C3D.Extensions.Aspire.VisualStudioDebug.Annotations;
using C3D.Extensions.Aspire.VisualStudioDebug.HealthChecks;
using C3D.Extensions.VisualStudioDebug;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IO;

namespace Aspire.Hosting;

public interface IDebugBuilder<TResource> : IResourceBuilder<TResource>
    where TResource : IResource
{
    public IResourceBuilder<TResource> ResourceBuilder { get; }
}

public static class DebugResourceExtensions
{
    public static IServiceCollection AddAttachDebuggerHook(this IServiceCollection services)
    {
        services
            .AddHostedService<AttachDebuggerHook>()
            .AddOptions<DebuggerHookOptions>()
            .BindConfiguration("DebuggerHook");
        services.TryAddSingleton<VisualStudioInstances>();
        return services;
    }

    public static IDebugBuilder<TResource> WithDebugEngine<TResource>(this IDebugBuilder<TResource> debugBuilder,
        string engine)
        where TResource : IResource
    {
        debugBuilder.ResourceBuilder.WithAnnotation<DebugAttachEngineAnnotation>(new() { Engine = engine });
        return debugBuilder;
    }

    public static IDebugBuilder<TResource> WithDebugSkip<TResource>(this IDebugBuilder<TResource> debugBuilder,
        bool skip = true)
        where TResource : IResource
    {
        if (debugBuilder.ResourceBuilder.Resource.TryGetLastAnnotation<DebugAttachAnnotation>(out var annotation))
        {
            annotation.Skip = skip;
        }
        return debugBuilder;
    }

    public static IDebugBuilder<TResource> WithDebugEngines<TResource>(this IDebugBuilder<TResource> debugBuilder,
        params string[] engines)
    where TResource : IResource
    {
        if (engines is not null)
        {
            foreach (var engine in engines)
            {
                debugBuilder.ResourceBuilder.WithAnnotation<DebugAttachEngineAnnotation>(new() { Engine = engine });
            }
        }
        return debugBuilder;
    }

    public static IDebugBuilder<TResource> WithDebugTransport<TResource>(this IDebugBuilder<TResource> debugBuilder,
        string transport, string? qualifier = null)
        where TResource : IResource
    {
        debugBuilder.ResourceBuilder.WithAnnotation<DebugAttachTransportAnnotation>(new() { Transport = transport, Qualifier = qualifier });
        return debugBuilder;
    }

    public static IDebugBuilder<TResource> WithDebuggerHealthcheck<TResource>(this IDebugBuilder<TResource> debugBuilder)
        where TResource : IResource
    {
        var healthCheckKey = $"{debugBuilder.ResourceBuilder.Resource.Name}_debugger_check";
        if (debugBuilder.ResourceBuilder.Resource.TryGetAnnotationsOfType<HealthCheckAnnotation>(out var annotations)
            && annotations.Any(a=>a.Key==healthCheckKey))
        {
            return debugBuilder;
        }

        debugBuilder.ResourceBuilder.ApplicationBuilder.Services.AddHealthChecks()
            .AddTypeActivatedCheck<DebugHealthCheck>(healthCheckKey, debugBuilder.ResourceBuilder.Resource.Name);

        debugBuilder.ResourceBuilder.WithHealthCheck(healthCheckKey);
        
        return debugBuilder;
    }

    public static IDebugBuilder<TResource> WithDebugger<TResource>(
        this IResourceBuilder<TResource> resourceBuilder,
        DebugMode debugMode = DebugMode.VisualStudio)
        where TResource : ExecutableResource
    {
        resourceBuilder.ApplicationBuilder.Services.AddAttachDebuggerHook();
        return new DebugBuilder<TResource>(
            resourceBuilder
                .WithEnvironment("Launch_Debugger_On_Start",
                                 debugMode == DebugMode.Environment ? "true" : null)
                .WithAnnotation<DebugAttachAnnotation>(new()
                {
                    DebugMode = debugMode
                },
                    ResourceAnnotationMutationBehavior.Replace));
    }

    internal static bool HasAnnotationOfType<T>(this IResource resource, Func<T, bool> predecate)
        where T : IResourceAnnotation => resource.Annotations.Any(a => a is T t && predecate(t));

    private class DebugBuilder<TResource> : IDebugBuilder<TResource>
        where TResource : IResource
    {
        private readonly IResourceBuilder<TResource> resourceBuilder;

        public DebugBuilder(IResourceBuilder<TResource> resourceBuilder) => 
            this.resourceBuilder = resourceBuilder;

        public IResourceBuilder<TResource> ResourceBuilder => resourceBuilder;

        public IDistributedApplicationBuilder ApplicationBuilder => resourceBuilder.ApplicationBuilder;

        public TResource Resource => resourceBuilder.Resource;

        public IResourceBuilder<TResource> WithAnnotation<TAnnotation>(TAnnotation annotation, ResourceAnnotationMutationBehavior behavior = ResourceAnnotationMutationBehavior.Append) where TAnnotation : IResourceAnnotation => 
            resourceBuilder.WithAnnotation(annotation, behavior);
    }
}
