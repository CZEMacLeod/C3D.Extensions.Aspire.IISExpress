using Aspire.Hosting.ApplicationModel;

namespace C3D.Extensions.Aspire.VisualStudioDebug.Annotations;

public class DebugAttachTransportAnnotation : IResourceAnnotation
{
    public required string Transport { get; init; }
    public string? Qualifier { get; init; }
}