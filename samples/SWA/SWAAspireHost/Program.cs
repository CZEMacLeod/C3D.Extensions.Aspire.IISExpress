using Aspire.Hosting;
using C3D.Extensions.Aspire.IISExpress;
using C3D.Extensions.Aspire.IISExpress.Resources;


var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions()
{
    Args = args,
    AllowUnsecuredTransport = true
});

var key = Guid.NewGuid().ToString();

builder.AddIISExpressConfiguration(ThisAssembly.Project.SolutionName, ThisAssembly.Project.SolutionDir);

var framework = builder.AddIISExpressProject<Projects.SWAFramework>("framework", IISExpressBitness.IISExpress64Bit)
    .WithEnvironment("RemoteAppApiKey", key)
    .WithDebugger(DebugMode.VisualStudio)
    ;

builder.AddProject<Projects.SWACore>("core")
    .WithEnvironment("RemoteApp__Key", key)
    .WithEnvironment("RemoteApp__Url", framework.GetEndpoint("http"))
    .WithHttpsHealthCheck("/alive");

builder.Build().Run();
