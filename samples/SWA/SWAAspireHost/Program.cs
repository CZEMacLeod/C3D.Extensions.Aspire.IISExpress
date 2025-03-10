using C3D.Extensions.Aspire.IISExpress;


var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions()
{
    Args = args,
    AllowUnsecuredTransport = true
});

var key = Guid.NewGuid().ToString();

var framework = builder.AddIISExpressProject<Projects.SWAFramework>("framework", IISExpressBitness.IISExpress64Bit)
    .WithEnvironment("RemoteApp__ApiKey", key);

builder.AddProject<Projects.SWACore>("core")
    .WithEnvironment("RemoteApp__ApiKey", key)
    .WithEnvironment("RemoteApp__RemoteAppUrl", framework.GetEndpoint("http"))
    .WithHttpsHealthCheck("/alive")
    .WithRelationship(framework.Resource, "YARP");

builder.Build().Run();
