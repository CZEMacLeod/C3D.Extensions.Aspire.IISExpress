using Aspire.Hosting.ApplicationModel;
using C3D.Extensions.Aspire.IISExpress;
using C3D.Extensions.Aspire.IISExpress.Annotations;
using C3D.Extensions.Aspire.IISExpress.Resources;
using C3D.Extensions.Aspire.VisualStudioDebug;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aspire.Hosting;

public static class IISExpressEntensions
{
    public static IResourceBuilder<IISExpressProjectResource> WithDebugger(this IResourceBuilder<IISExpressProjectResource> resourceBuilder,
        DebugMode debugMode = DebugMode.VisualStudio) =>
        DebugResourceExtensions.WithDebugger(resourceBuilder, debugMode)
            .WithDebugEngine(C3D.Extensions.VisualStudioDebug.WellKnown.Engines.Net4)
            .WithDebuggerHealthcheck();

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

        builder.Services.AddAttachDebuggerHook();

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
            .WithAnnotation(new AppPoolArgumentAnnotation())
            .WithAnnotation(new SiteArgumentAnnotation(appName))
            .WithArgs(c =>
                {
                    foreach (var arg in resource.Annotations.OfType<IISExpressArgumentAnnotation>())
                    {
                        c.Args.Add(arg);
                    }
                })
            .WithOtlpExporter()
            .ExcludeFromManifest();

        if (builder.Environment.IsDevelopment())
        {
            resourceBuilder.WithDebugger();
        }

        return resourceBuilder;
    }

}
