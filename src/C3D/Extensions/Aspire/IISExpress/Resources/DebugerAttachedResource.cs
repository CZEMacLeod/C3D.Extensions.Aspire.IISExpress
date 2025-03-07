using Aspire.Hosting.ApplicationModel;

namespace C3D.Extensions.Aspire.IISExpress.Resources;

internal class DebugerAttachedResource : IResourceAnnotation
{
    public int? DebuggerProcessId { get; init; }
}