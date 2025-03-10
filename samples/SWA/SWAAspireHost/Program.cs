using C3D.Extensions.Aspire.IISExpress;


var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions()
{
    Args = args,
    AllowUnsecuredTransport = true
});

var framework = builder.AddIISExpressProject<Projects.SWAFramework>("framework", IISExpressBitness.IISExpress64Bit)
    .WithSystemWebAdapters()
    .WithHttpHealthCheck("/debug", 204);

builder.AddProject<Projects.SWACore>("core")
    .WithSystemWebAdapters(framework)
    .WithHttpsHealthCheck("/alive");

builder.Build().Run();
