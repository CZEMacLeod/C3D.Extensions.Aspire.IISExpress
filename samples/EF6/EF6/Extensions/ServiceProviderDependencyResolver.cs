using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace C3D.Extensions.DependencyInjection;

internal class ServiceProviderDependencyResolver : IDependencyResolver
{
    private readonly IServiceProvider serviceProvider;

    public ServiceProviderDependencyResolver(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

    public object GetService(Type serviceType) => serviceProvider.GetService(serviceType);

    public IEnumerable<object?> GetServices(Type serviceType) => serviceProvider.GetServices(serviceType);
}