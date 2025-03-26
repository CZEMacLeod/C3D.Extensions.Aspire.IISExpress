using Aspire.Hosting.ApplicationModel;
using C3D.Extensions.Aspire.VisualStudioDebug.Annotations;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace C3D.Extensions.Aspire.VisualStudioDebug.HealthChecks;

public class DebugHealthCheck : IHealthCheck
{
    private readonly IResource resource;

    public DebugHealthCheck(DistributedApplicationModel model, string resourceName) => 
        resource = model.Resources.Single(r => r.Name == resourceName);

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!resource.TryGetLastAnnotation<DebugAttachAnnotation>(out var a))
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("No debug annotation found"));
        }

        if (a.DebuggerProcessId is null)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Debugger not attached"));
        }

        return Task.FromResult(HealthCheckResult.Healthy("Debugger Attached"));
    }
}