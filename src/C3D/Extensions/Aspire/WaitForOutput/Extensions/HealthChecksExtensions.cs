using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Aspire.Hosting;

public static class HealthChecksExtensions
{
    public static ResourceHealthChecksBuilder<TResource> WithLocalHealthChecks<TResource>(this
        IResourceBuilder<TResource> resourceBuilder)
        where TResource : IResource => 
        new(resourceBuilder, resourceBuilder.ApplicationBuilder.Services.AddHealthChecks());

    public class ResourceHealthChecksBuilder<TResource> : IResourceBuilder<TResource>, IHealthChecksBuilder
        where TResource : IResource
    {
        private readonly IResourceBuilder<TResource> resourceBuilder;
        private readonly IHealthChecksBuilder healthChecksBuilder;

        internal ResourceHealthChecksBuilder(IResourceBuilder<TResource> resourceBuilder, 
            IHealthChecksBuilder healthChecksBuilder)
        {
            this.resourceBuilder = resourceBuilder;
            this.healthChecksBuilder = healthChecksBuilder;
        }

        public IServiceCollection Services => healthChecksBuilder.Services;

        public IDistributedApplicationBuilder ApplicationBuilder => resourceBuilder.ApplicationBuilder;

        public TResource Resource => resourceBuilder.Resource;

        public IHealthChecksBuilder Add(HealthCheckRegistration registration)
        {
            healthChecksBuilder.Add(registration);
            resourceBuilder.WithHealthCheck(registration.Name);
            return this;
        }

        public IResourceBuilder<TResource> WithAnnotation<TAnnotation>(TAnnotation annotation,
            ResourceAnnotationMutationBehavior behavior = ResourceAnnotationMutationBehavior.Append)
            where TAnnotation : IResourceAnnotation
        {
            resourceBuilder.WithAnnotation(annotation, behavior);
            return this;
        }
    }
}
