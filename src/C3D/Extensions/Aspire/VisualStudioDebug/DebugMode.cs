using System.ComponentModel;

namespace C3D.Extensions.Aspire.VisualStudioDebug;

public enum DebugMode
{
    None,
    Environment,
    [Description("Visual Studio - Only available when running on Windows")]
    VisualStudio
}
