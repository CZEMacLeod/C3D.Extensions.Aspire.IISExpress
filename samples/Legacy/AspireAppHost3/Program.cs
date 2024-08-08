var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions()
{
    Args = args,
    // NET Framework has an issue pushing OTLP data using GRPC and SSL - use http instead of https
    AllowUnsecuredTransport = true
});

var legacy = builder.AddProject<Projects.LegacyWebApplication>("legacy")
    // ports are not automatically picked up - need to manually configure
    .WithHttpsEndpoint(44372, isProxied: false)
    .WithHttpEndpoint(57816, isProxied: false)
    .WithOtlpExporter();

builder.Build().Run();
