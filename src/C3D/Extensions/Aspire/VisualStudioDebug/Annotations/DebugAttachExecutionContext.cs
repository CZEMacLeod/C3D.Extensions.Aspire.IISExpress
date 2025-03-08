using Aspire.Hosting.ApplicationModel;

namespace C3D.Extensions.Aspire.VisualStudioDebug.Annotations;

public sealed class DebugAttachExecutionContext
{
    public required ExecutableResource Resource { get; init; }
    public required DebugAttachAnnotation Annotation { get; init; }

    public required int ProcessId { get; init; }

    public required IServiceProvider ServiceProvider { get; init; }

}
