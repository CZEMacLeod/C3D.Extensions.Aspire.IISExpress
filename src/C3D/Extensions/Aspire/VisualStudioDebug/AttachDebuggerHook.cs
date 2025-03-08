using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Eventing;
using C3D.Extensions.Aspire.VisualStudioDebug.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace C3D.Extensions.Aspire.VisualStudioDebug;

internal class AttachDebuggerHook : BackgroundService
{
    private readonly ILogger logger;
    private readonly ResourceNotificationService resourceNotificationService;
    private readonly IDistributedApplicationEventing distributedApplicationEventing;
    private readonly IOptions<DebuggerHookOptions> options;
    private readonly IServiceProvider serviceProvider;

    public AttachDebuggerHook(
        ILogger<AttachDebuggerHook> logger,
        ResourceNotificationService resourceNotificationService,
        IDistributedApplicationEventing distributedApplicationEventing,
        IOptions<DebuggerHookOptions> options,
        IServiceProvider serviceProvider
        )
    {
        this.logger = logger;
        this.resourceNotificationService = resourceNotificationService;
        this.distributedApplicationEventing = distributedApplicationEventing;
        this.options = options;
        this.serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var notification in resourceNotificationService
            .WatchAsync(stoppingToken)
            .WithCancellation(stoppingToken))
        {
            if (notification.Resource is ExecutableResource resource &&
                resource.HasAnnotationOfType<DebugAttachAnnotation>(a =>
                    a.DebugMode == DebugMode.VisualStudio &&
                    a.DebuggerProcessId is null))
            {
                try
                {
                    await DebugAttachAsync(notification, resource, stoppingToken);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "An error occurred trying to attach debugger for {name}: {message}", resource.Name, e.Message);
                }
            }
        }
    }


    private async Task DebugAttachAsync(ResourceEvent notification, ExecutableResource resource, CancellationToken cancellationToken)
    {
        var processId = notification.Snapshot.Properties.SingleOrDefault(prp => prp.Name == "executable.pid")?.Value as int? ?? 0;
        if (processId == 0)
        {
            logger.LogDebug("{name} executable.pid == 0", resource.Name);
            return;
        }

        if (!resource.TryGetLastAnnotation<DebugAttachAnnotation>(out var annotation))
        {
            annotation = new DebugAttachAnnotation();
            resource.Annotations.Add(annotation);
        }

        IDisposable? token = null;
        try
        {
            if (annotation.TryLock(out token))
            {
                if (annotation.DebuggerProcessId is not null) return;

                var context = new DebugAttachExecutionContext()
                {
                    Resource = resource,
                    Annotation = annotation,
                    ProcessId = processId,
                    ServiceProvider = serviceProvider
                };
                await DebugAttachAsync(context, cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred trying to attach debugger for {name}: {message}", resource.Name, e.Message);
        }
        finally
        {
            token?.Dispose();
        }
    }

    private async Task DebugAttachAsync(DebugAttachExecutionContext context, CancellationToken cancellationToken)
    {

        Process? target = null;
        if (!context.Resource.HasAnnotationOfType<DebugAttachTransportAnnotation>())
        {
            target = System.Diagnostics.Process.GetProcessById(context.ProcessId);
            var vs1 = VisualStudioAttacher.GetAttachedVisualStudio(target);
            if (vs1 is not null)
            {
                logger.LogWarning("Debugger {vs}:{vsId} already attached to {target}:{targetId}", vs1.ProcessName, vs1.Id, target.ProcessName, target.Id);

                context.Annotation.DebuggerProcessId = vs1.Id;
                return;
            }
        }

        var aspireHost = Process.GetCurrentProcess();
        var vs = VisualStudioAttacher.GetAttachedVisualStudio(aspireHost);
        if (vs is null)
        {
            logger.LogError("Could not get debugger for aspire host {host}", aspireHost);
            return;
        }

        string[] engines;
        if (context.Resource.TryGetAnnotationsOfType<DebugAttachEngineAnnotation>(out var engineAnnotations))
        {
            engines = engineAnnotations.Select(a => a.Engine).Distinct().ToArray();
        }
        else
        {
            engines = [];
        }

        await distributedApplicationEventing.PublishAsync(new Events.BeforeDebugEvent(context), cancellationToken);

        if (context.Resource.TryGetLastAnnotation<DebugAttachTransportAnnotation>(out var ta))
        {
            ShowEngines(vs, ta.Transport);
            ShowProcesses(vs, ta.Transport, ta.Qualifier);
            logger.LogInformation("Attaching {vs}:{vsId} to {transport} {id} for {applicationName}", vs.ProcessName, vs.Id, ta.Transport, ta.Qualifier, context.Resource.Name);
            VisualStudioAttacher.AttachVisualStudioToProcess(vs, ta.Transport, ta.Qualifier, engines);
        }
        else
        {
            logger.LogInformation("Attaching {vs}:{vsId} to {target}:{targetId} for {applicationName}", vs.ProcessName, vs.Id, target!.ProcessName, target.Id, context.Resource.Name);
            ShowTransports(vs);
            ShowEngines(vs, "default");
            ShowProcesses(vs, "default", null);
            VisualStudioAttacher.AttachVisualStudioToProcess(vs, context.ProcessId, engines);
        }

        logger.LogInformation("Debugger {vs}:{vsId} attached to target", vs.ProcessName, vs.Id);
        context.Annotation.DebuggerProcessId = vs.Id;

        await distributedApplicationEventing.PublishAsync(new Events.AfterDebugEvent(context), cancellationToken);
    }

    private void ShowProcesses(Process vs, string transport, string? qualifier)
    {
        if (options.Value.ShowProcesses)
        {
            var availableProcesses = VisualStudioAttacher.GetDebugProcesses(vs, transport, qualifier);
            foreach (var process in availableProcesses)
            {
                logger.LogInformation("Available {transport} process {id} {name} {isDebugged}", transport, process.id, process.name, process.isDebugged);
            }
        }
    }

    private void ShowTransports(Process vs)
    {
        if (options.Value.ShowTransports)
        {
            var availableTransports = VisualStudioAttacher.GetDebugTransports(vs);
            foreach (var transport in availableTransports)
            {
                logger.LogInformation("Available transport {id} {name}",  transport.id, transport.name);
            }
        }
    }

    private void ShowEngines(Process vs, string transport)
    {
        if (options.Value.ShowEngines)
        {
            var availableEngines = VisualStudioAttacher.GetDebugEngines(vs, transport);
            foreach (var engine in availableEngines)
            {
                logger.LogInformation("Available {transport} engine {id} {name} {result}", transport, engine.id, engine.name, engine.result);
            }
        }
    }
}