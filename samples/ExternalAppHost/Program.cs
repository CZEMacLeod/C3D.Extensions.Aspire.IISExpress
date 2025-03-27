var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions()
{
    Args = args,
    AllowUnsecuredTransport = true
});

var wcf = builder.AddIISExpressProject<Projects.ExampleWCFWebApplication>("wcf")
    .WithUrls(u =>
    {
        if (u.Resource.TryGetEndpoints(out var endpoints))
        {
            foreach (var ep in endpoints)
            {
                u.Urls.Add(new()
                {
                    Url = new UriBuilder()
                    {
                        Scheme = ep.UriScheme,
                        Host = ep.AllocatedEndpoint!.Address,
                        Port = ep.AllocatedEndpoint!.Port,
                        Path = "service.svc"
                    }.ToString(),
                    DisplayText = $"{ep.UriScheme} WCF Service"
                });
                u.Urls.Add(new()
                {
                    Url = new UriBuilder()
                    {
                        Scheme = ep.UriScheme,
                        Host = ep.AllocatedEndpoint!.Address,
                        Port = ep.AllocatedEndpoint!.Port,
                        Path = "service2/Add",
                        Query = "n1=420&n2=0.69"
                    }.ToString(),
                    DisplayText = $"{ep.UriScheme} Add(420,0.69)"
                });
            }
        }
    });

builder.Build().Run();
