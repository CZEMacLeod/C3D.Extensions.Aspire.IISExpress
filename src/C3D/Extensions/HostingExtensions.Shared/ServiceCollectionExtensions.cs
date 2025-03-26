using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection InsertHostedService<TService>(this IServiceCollection services, int index = 0)
        where TService : class, IHostedService
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));

        var hostedServices = services.Where(s =>
            s.ServiceType == typeof(IHostedService) &&
            s.ServiceKey is null &&
            s.ImplementationType != typeof(TService)).ToList();

        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, hostedServices.Count, nameof(index));

        hostedServices.Insert(index, ServiceDescriptor.Singleton<IHostedService, TService>());

        return services
            .RemoveAll<IHostedService>()   // remove all hosted services
            .Add(hostedServices);          // re-add the hosted services with the inserted service
    }
}
