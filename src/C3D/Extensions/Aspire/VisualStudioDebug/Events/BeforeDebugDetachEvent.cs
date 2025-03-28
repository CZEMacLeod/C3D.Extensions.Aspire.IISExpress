using Aspire.Hosting.Eventing;
using C3D.Extensions.Aspire.VisualStudioDebug.Annotations;

namespace C3D.Extensions.Aspire.VisualStudioDebug.Events;

public class BeforeDebugDetachEvent : IDistributedApplicationEvent
{
    public BeforeDebugDetachEvent(DebugDetachExecutionContext context)
    {
        Context = context;
    }

    public DebugDetachExecutionContext Context { get; }
}
