using Aspire.Hosting.ApplicationModel;
using C3D.Extensions.Aspire.OutputWatcher;
using C3D.Extensions.Aspire.OutputWatcher.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Aspire.Hosting;

public static class WaitForOutputExtensions
{
    public static IResourceBuilder<TResource> WaitForOutput<TResource>(this
        IResourceBuilder<TResource> resourceBuilder,
        IResourceBuilder<IResource> dependency,
        string match,
        StringComparison comparer = StringComparison.InvariantCulture,
        bool isSecret = false,
        string? key = null)
        where TResource : IResourceWithWaitSupport =>
            dependency
                .WithOutputWatcher(match, comparer, isSecret, key)
                .WaitForOutput(resourceBuilder);

    public static IResourceBuilder<TResource> WaitForOutput<TResource>(this
        IResourceBuilder<TResource> resourceBuilder,
        IResourceBuilder<IResource> dependency,
        Regex matcher,
        bool isSecret = false,
        string? key = null)
        where TResource : IResourceWithWaitSupport =>
            dependency.WithOutputWatcher(matcher, isSecret, key)
                .WaitForOutput(resourceBuilder);

    public static IResourceBuilder<TResource> WaitForOutput<TResource>(this
        IResourceBuilder<TResource> resourceBuilder,
        IResourceBuilder<IResource> dependency,
        Func<string, bool> predicate,
        bool isSecret = false,
        string? key = null)
        where TResource : IResourceWithWaitSupport => 
            dependency.WithOutputWatcher(predicate, isSecret, key)
                .WaitForOutput(resourceBuilder);

    private static IResourceBuilder<TResource> WaitForOutput<TResource, TAnnotation>(
        this OutputWatcherExtensions.OutputWatcherBuilder<IResource, TAnnotation> builder,
        IResourceBuilder<TResource> resourceBuilder)
        where TResource : IResourceWithWaitSupport
        where TAnnotation : OutputWatcherAnnotationBase
    {
        var key = builder.Key;
        builder
            .WithLocalHealthChecks()
            .AddTypeActivatedCheck<WaitForOutputHealthCheck>(key, builder.Resource);

        return resourceBuilder.WaitFor(builder.ResourceBuilder);
    }
}
