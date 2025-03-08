using Aspire.Hosting.Eventing;
using C3D.Extensions.Aspire.VisualStudioDebug.Annotations;

namespace C3D.Extensions.Aspire.VisualStudioDebug.Events;

public class BeforeDebugEvent : IDistributedApplicationEvent
{
    public BeforeDebugEvent(DebugAttachExecutionContext context)
    {
        Context = context;
    }

    public DebugAttachExecutionContext Context { get; }
}
