using Aspire.Hosting.ApplicationModel;

namespace C3D.Extensions.Aspire.VisualStudioDebug.Annotations;

public class DebugAttachEngineAnnotation : IResourceAnnotation
{
    public required string Engine { get; init; }
}
