using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Builder;

public static class SystemWebAdaptersYARPExtensions
{
    public static IEndpointConventionBuilder MapRemoteApp(this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern = "/{**catch-all}",
        int order = int.MaxValue) => endpoints
            .MapForwarder(pattern,
                endpoints.ServiceProvider.GetRequiredService<IOptions<RemoteAppClientOptions>>()
                    .Value.RemoteAppUrl.AbsoluteUri)
            .WithOrder(order);
}
