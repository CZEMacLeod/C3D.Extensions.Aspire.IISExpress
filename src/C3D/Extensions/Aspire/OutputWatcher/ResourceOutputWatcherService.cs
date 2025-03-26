using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Eventing;
using C3D.Extensions.Aspire.OutputWatcher.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace C3D.Extensions.Aspire.OutputWatcher;

public class ResourceOutputWatcherService : BackgroundService
{
    private readonly ILogger<ResourceOutputWatcherService> logger;
    private readonly ResourceLoggerService resourceLoggerService;
    private readonly IDistributedApplicationEventing distributedApplicationEventing;
    private readonly DistributedApplicationModel model;
    private readonly IServiceProvider serviceProvider;
    private readonly TimeProvider timeProvider;

    public ResourceOutputWatcherService(
        ILogger<ResourceOutputWatcherService> logger,
        ResourceLoggerService resourceLoggerService,
        DistributedApplicationModel model,
        IDistributedApplicationEventing distributedApplicationEventing,
        IServiceProvider serviceProvider,
        TimeProvider? timeProvider = null)
    {
        this.logger = logger;
        this.resourceLoggerService = resourceLoggerService;
        this.distributedApplicationEventing = distributedApplicationEventing;
        this.model = model;
        this.serviceProvider = serviceProvider;
        this.timeProvider = timeProvider ?? TimeProvider.System;
        logger.LogInformation("ConsoleOutputWatcherService created.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ConsoleOutputWatcherService started.");
        var tasks = model.Resources.Where(r => r.HasAnnotationOfType<OutputWatcherAnnotationBase>())
            .Select(r => WatchResourceAsync(r, stoppingToken))
            .ToArray();
        await Task.WhenAll(tasks);
        logger.LogInformation("ConsoleOutputWatcherService stopped.");
    }

    public const string ConsoleLogsTimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffffffK";
    private readonly static int ConsoleLogsTimestampFormatLength = ConsoleLogsTimestampFormat.Length;
    private readonly static int MessageStartOffset = ConsoleLogsTimestampFormatLength+1;

    private async Task WatchResourceAsync(IResource resource, CancellationToken stoppingToken)
    {
        

        logger.LogInformation("Waiting for {Resource} output", resource.Name);
        if (resource.TryGetAnnotationsOfType<OutputWatcherAnnotationBase>(out var annotations))
        {
            bool isSecret = annotations.Any(a => a.IsSecret);
            await foreach (var output in resourceLoggerService.WatchAsync(resource).WithCancellation(stoppingToken))
            {
                foreach (var line in output)
                {
                    if (isSecret)
                    {
                        logger.LogDebug("Received {Resource} output: {LineNumber} <Redacted>", resource.Name, line.LineNumber);
                    }
                    else
                    {
                        if (line.IsErrorMessage)
                        {
                            logger.LogWarning("Received {Resource} output: {LineNumber} {Content}", resource.Name, line.LineNumber, line.Content);
                        }
                        else
                        {
                            logger.LogDebug("Received {Resource} output: {LineNumber} {Content}", resource.Name, line.LineNumber, line.Content);
                        }
                    }
                    try
                    {
                        var message = DateTimeOffset.TryParseExact(
                            line.Content.AsSpan()[..ConsoleLogsTimestampFormatLength], ConsoleLogsTimestampFormat,
                            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timeStamp)
                            ? line.Content[MessageStartOffset..] : line.Content;

                        foreach (var annotation in annotations)
                        {
                            await ProcessLineAsync(resource, annotation, timeStamp, message, stoppingToken);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Failed to process {Resource} output {Message}", resource.Name, e.Message);
                    }
                }
            }
        }
    }

    private async Task ProcessLineAsync(IResource resource, OutputWatcherAnnotationBase annotation,
            DateTimeOffset? timeStamp, string line, CancellationToken stoppingToken)
    {
        if (annotation.IsMatch(line))
        {
            logger.LogInformation("{Resource} output matched {predicate} {key}.", resource.Name, annotation.PredicateName, annotation.Key);
            annotation.Message = annotation.IsSecret ? "<Redacted>" : line;
            annotation.TimeStamp = timeStamp ?? timeProvider.GetUtcNow();

            await distributedApplicationEventing.PublishAsync(ActivatorUtilities.CreateInstance<OutputMatchedEvent>(
                serviceProvider,
                resource,
                annotation.Key,
                annotation.Message,
                annotation.Properties
                ), stoppingToken);
        }
    }
}
