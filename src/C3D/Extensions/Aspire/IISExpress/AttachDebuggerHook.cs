using Aspire.Hosting.ApplicationModel;
using C3D.Extensions.Aspire.IISExpress.Resources;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace C3D.Extensions.Aspire.IISExpress;

internal class AttachDebuggerHook : BackgroundService
{
    private readonly ILogger logger;
    private readonly ResourceNotificationService resourceNotificationService;

    public AttachDebuggerHook(
        ILogger<AttachDebuggerHook> logger,
        ResourceNotificationService resourceNotificationService
        )
    {
        this.logger = logger;
        this.resourceNotificationService = resourceNotificationService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var notification in resourceNotificationService.WatchAsync(stoppingToken))
        {
            if (notification.Resource is IISExpressProjectResource resource &&
                resource.Annotations.OfType<DebugAttachResource>().Any(dar => dar.DebugMode == DebugMode.VisualStudio) &&
                !resource.Annotations.OfType<DebugerAttachedResource>().Any())
            {
                DebugAttach(notification, resource);
            }
        }
    }

    private void DebugAttach(ResourceEvent notification, IISExpressProjectResource resource)
    {
        var processId = notification.Snapshot.Properties.SingleOrDefault(prp => prp.Name == "executable.pid")?.Value as int? ?? 0;
        if (processId == 0)
        {
            logger.LogDebug("{name} executable.pid == 0", resource.Name);
            return;
        }

        var target = System.Diagnostics.Process.GetProcessById(processId);

        var vs = VisualStudioAttacher.GetAttachedVisualStudio(target);
        if (vs is not null)
        {
            logger.LogWarning("Debugger {vs}:{vsId} already attached to {target}:{targetId}", vs.ProcessName, vs.Id, target.ProcessName, target.Id);
            resource.Annotations.Add(new DebugerAttachedResource());
            return;
        }

        var aspireHost = Process.GetCurrentProcess();
        vs = VisualStudioAttacher.GetAttachedVisualStudio(aspireHost);
        if (vs is null)
        {
            logger.LogError("Could not get debugger for aspire host {host}", aspireHost);
            return;
        }

        var engines = VisualStudioAttacher.GetDebugEngines(vs);
        logger.LogInformation("Available debug engines {engines}", engines);

        logger.LogInformation("Attaching {vs}:{vsId} to {target}:{targetId} for {applicationName}", vs.ProcessName, vs.Id, target.ProcessName, target.Id, notification.Resource.Name);
        VisualStudioAttacher.AttachVisualStudioToProcess(vs, target, "Managed (.NET Framework 4.x)");

        vs = VisualStudioAttacher.GetAttachedVisualStudio(target);
        if (vs is not null)
        {
            logger.LogInformation("Debugger {vs}:{vsId} attached to {target}:{targetId}", vs.ProcessName, vs.Id, target.ProcessName, target.Id);

            resource.Annotations.Add(new DebugerAttachedResource());
        }

    }
}