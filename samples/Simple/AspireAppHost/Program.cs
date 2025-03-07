using C3D.Extensions.Aspire.IISExpress;
using C3D.Extensions.Aspire.IISExpress.Resources;

var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions()
{
    Args = args,
    AllowUnsecuredTransport = true
});


builder.AddIISExpressConfiguration(ThisAssembly.Project.SolutionName, ThisAssembly.Project.SolutionDir);

builder.AddIISExpressProject<Projects.SimpleMVC>()
    .WithDebugger(DebugMode.VisualStudio)
    ;

builder.Build().Run();
