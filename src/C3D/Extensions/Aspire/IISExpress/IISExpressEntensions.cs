using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using C3D.Extensions.Aspire.IISExpress.Annotations;
using C3D.Extensions.Aspire.IISExpress.Configuration;
using C3D.Extensions.Aspire.IISExpress.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;
using System.Xml.Serialization;

namespace C3D.Extensions.Aspire.IISExpress;

public static class IISExpressEntensions
{
    public static IServiceCollection WithAttachDebugger(this IServiceCollection services) => services.AddHostedService<AttachDebuggerHook>();

    public static IResourceBuilder<IISExpressProjectResource> WithDebugger(this IResourceBuilder<IISExpressProjectResource> resourceBuilder,
        DebugMode debugMode = DebugMode.VisualStudio) =>
        resourceBuilder
            .WithEnvironment("Launch_Debugger_On_Start", debugMode == DebugMode.Environment ? "true" : null)
            .WithAnnotation<DebugAttachResource>(new()
                {
                    DebugMode = debugMode,
                    Engines = ["Managed (.NET Framework 4.x)"]
                },
                ResourceAnnotationMutationBehavior.Replace);

    public static IDistributedApplicationBuilder AddIISExpressConfiguration(this IDistributedApplicationBuilder builder,
        Action<IISExpressOptions>? options = null)
    {
        options ??= _ => { };
        var o = builder.Services
            .AddOptions<IISExpressOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();
        if (options is not null)
            o.Configure(options);

        builder.Services.AddTransient<IISEndPointConfigurator>();
        
        builder.Eventing.Subscribe<BeforeStartEvent>((@event, cancellationToken) =>
        {
            var services = @event.Services;
            var configurator = services.GetRequiredService<IISEndPointConfigurator>();

            configurator.Configure();

            return Task.CompletedTask;
        });

        builder.Services.WithAttachDebugger();

        return builder;
    }

    private static readonly Dictionary<IISExpressBitness, string> iisExpressPath = new();

    public static IResourceBuilder<IISExpressProjectResource> AddIISExpressProject<T>(this IDistributedApplicationBuilder builder,
        [ResourceName] string? resourceName = null,
        IISExpressBitness? bitness = null)
        where T : IProjectMetadata, new()
    {
        builder.AddIISExpressConfiguration();

        var app = new T();

        var appName = app.GetType().Name;
        var projectPath = System.IO.Path.GetDirectoryName(app.ProjectPath)!;

        bitness ??= Environment.Is64BitOperatingSystem ? IISExpressBitness.IISExpress64Bit : IISExpressBitness.IISExpress32Bit;

        if (!iisExpressPath.TryGetValue(bitness.Value, out var iisExpress))
        {
            var programFiles = System.Environment.GetFolderPath(bitness == IISExpressBitness.IISExpress32Bit ?
                Environment.SpecialFolder.ProgramFilesX86 :
                Environment.SpecialFolder.ProgramFiles);
            iisExpress = System.IO.Path.Combine(programFiles, "IIS Express", "iisexpress.exe");
            iisExpressPath[bitness.Value] = iisExpress;
        }

        resourceName ??= appName;
        var resource = new IISExpressProjectResource(resourceName, iisExpress, projectPath);

        var resourceBuilder = builder.AddResource(resource)
            .WithAnnotation(app)
            .WithAnnotation(new Annotations.AppPoolArgumentAnnotation())
            .WithAnnotation(new Annotations.SiteArgumentAnnotation(appName))
            .WithArgs(c =>
                {
                    foreach (var arg in resource.Annotations.OfType<IISExpressArgumentAnnotation>())
                    {
                        c.Args.Add(arg);
                    }
                })
            .WithOtlpExporter()
            .ExcludeFromManifest();

        return resourceBuilder;
    }

}
