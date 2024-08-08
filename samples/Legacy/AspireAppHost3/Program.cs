var builder = DistributedApplication.CreateBuilder(args);

var legacy = builder.AddProject<Projects.LegacyWebApplication>("legacy")
    // ports are not automatically picked up - need to manually configure
    .WithHttpsEndpoint(44372, isProxied: false)
    .WithHttpEndpoint(57816, isProxied: false);

builder.Build().Run();
