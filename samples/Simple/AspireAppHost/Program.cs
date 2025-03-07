var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions()
{
    Args = args,
    AllowUnsecuredTransport = true
});

builder.AddIISExpressProject<Projects.SimpleMVC>();

builder.Build().Run();
