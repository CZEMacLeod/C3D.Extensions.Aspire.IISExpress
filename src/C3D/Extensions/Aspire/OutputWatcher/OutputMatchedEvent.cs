using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Eventing;

namespace C3D.Extensions.Aspire.OutputWatcher;

public class OutputMatchedEvent : IDistributedApplicationResourceEvent
{
    public OutputMatchedEvent(IServiceProvider serviceProvider, IResource resource, string key, string message, IReadOnlyDictionary<string, object> properties)
    {
        Resource = resource;
        ServiceProvider = serviceProvider;
        Key = key;
        Message = message;
        Properties = properties;
    }

    public IResource Resource { get; }
    public IServiceProvider ServiceProvider { get; }

    public string Key { get; }
    public string Message { get; }

    public IReadOnlyDictionary<string, object> Properties { get; }
}