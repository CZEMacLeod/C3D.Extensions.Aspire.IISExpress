using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using C3D.Extensions.Aspire.VisualStudioDebug.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace C3D.Extensions.Aspire.VisualStudioDebug;

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
            if (notification.Resource is ExecutableResource resource &&
                resource.HasAnnotationOfType<DebugAttachAnnotation>(dar => 
                    dar.DebugMode == DebugMode.VisualStudio &&
                    dar.DebuggerProcessId is null))
            {
                try
                {
                    DebugAttach(notification, notification.Resource);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "An error occurred trying to attach debugger for {name}: {message}", resource.Name, e.Message);
                }
            }
        }
    }

    private void DebugAttach(ResourceEvent notification, IResource resource)
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
            if (!resource.TryGetLastAnnotation<DebugAttachAnnotation>(out var annotation))
            {
                annotation = new DebugAttachAnnotation();
                resource.Annotations.Add(annotation);
            }
            annotation.DebuggerProcessId = vs.Id;
            return;
        }

        var aspireHost = Process.GetCurrentProcess();
        vs = VisualStudioAttacher.GetAttachedVisualStudio(aspireHost);
        if (vs is null)
        {
            logger.LogError("Could not get debugger for aspire host {host}", aspireHost);
            return;
        }

        var engines = resource.Annotations.OfType<DebugAttachAnnotation>().SelectMany(d => d.Engines ?? []).ToArray();
        if (engines.Length == 0)
        {
            var availableEngines = VisualStudioAttacher.GetDebugEngines(vs);
            logger.LogInformation("Available debug engines {engines}", availableEngines);
        }

        logger.LogInformation("Attaching {vs}:{vsId} to {target}:{targetId} for {applicationName}", vs.ProcessName, vs.Id, target.ProcessName, target.Id, notification.Resource.Name);
        VisualStudioAttacher.AttachVisualStudioToProcess(vs, target, engines);

        vs = VisualStudioAttacher.GetAttachedVisualStudio(target);
        if (vs is not null)
        {
            logger.LogInformation("Debugger {vs}:{vsId} attached to {target}:{targetId}", vs.ProcessName, vs.Id, target.ProcessName, target.Id);
            if (!resource.TryGetLastAnnotation<DebugAttachAnnotation>(out var annotation))
            {
                annotation = new DebugAttachAnnotation();
                resource.Annotations.Add(annotation);
            }
            annotation.DebuggerProcessId = vs.Id;
        }

    }
}