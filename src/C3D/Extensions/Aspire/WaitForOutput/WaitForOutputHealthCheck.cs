using Aspire.Hosting.ApplicationModel;
using C3D.Extensions.Aspire.OutputWatcher.Annotations;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace C3D.Extensions.Aspire.OutputWatcher;

public class WaitForOutputHealthCheck : IHealthCheck
{
    private readonly IResource resource;
    private readonly ILogger<WaitForOutputHealthCheck> logger;

    public WaitForOutputHealthCheck(IResource resource, ILogger<WaitForOutputHealthCheck> logger)
    {
        this.resource = resource;
        this.logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Checking health for {Resource}", resource.Name);
        if (resource.TryGetLastAnnotation<OutputWatcherAnnotation>(out var annotation))
        {
            return Task.FromResult(
                annotation.Message is not null
                    ? HealthCheckResult.Healthy(annotation.Message)
                    : HealthCheckResult.Unhealthy());
        }
        return Task.FromResult(HealthCheckResult.Unhealthy("No annotation found!"));
    }
}