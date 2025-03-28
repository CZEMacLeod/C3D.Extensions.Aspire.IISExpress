using Aspire.Hosting.Eventing;
using C3D.Extensions.Aspire.VisualStudioDebug.Annotations;

namespace C3D.Extensions.Aspire.VisualStudioDebug.Events;

public class AfterDebugAttachEvent : IDistributedApplicationEvent
{
    public AfterDebugAttachEvent(DebugAttachExecutionContext context)
    {
        Context = context;
    }

    public DebugAttachExecutionContext Context { get; }
}
