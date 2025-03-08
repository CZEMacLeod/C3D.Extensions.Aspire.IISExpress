using Aspire.Hosting.Eventing;
using C3D.Extensions.Aspire.VisualStudioDebug.Annotations;

namespace C3D.Extensions.Aspire.VisualStudioDebug.Events;

public class AfterDebugEvent : IDistributedApplicationEvent
{
    public AfterDebugEvent(DebugAttachExecutionContext context)
    {
        Context = context;
    }

    public DebugAttachExecutionContext Context { get; }
}
