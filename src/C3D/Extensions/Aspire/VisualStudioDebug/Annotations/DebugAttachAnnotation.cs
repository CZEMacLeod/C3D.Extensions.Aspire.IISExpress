using Aspire.Hosting.ApplicationModel;

namespace C3D.Extensions.Aspire.VisualStudioDebug.Annotations;

internal class DebugAttachAnnotation : IResourceAnnotation
{
    public DebugMode DebugMode { get; set; }

    public IEnumerable<string>? Engines { get; set; }

    public int? DebuggerProcessId { get; set; }
}