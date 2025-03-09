using EnvDTE;
using EnvDTE80;

namespace C3D.Extensions.VisualStudioDebug;

internal static class DTEExtensions
{

    internal static EnvDTE90.Debugger3 GetDebugger(this DTE visualStudioInstance)
    {
        var dte = (EnvDTE80.DTE2)visualStudioInstance;
        return (EnvDTE90.Debugger3)dte.Debugger;
    }

    internal static (EnvDTE90.Debugger3 debugger, Transport transport) GetDebugTransport(this DTE vs, string transport)
    {
        var debugger = vs.GetDebugger();
        Transport? port = null;
        if (Guid.TryParse(transport, out var _))
        {
            foreach (EnvDTE80.Transport e in debugger.Transports)
            {
                if (e.ID == transport)
                {
                    port = e;
                    break;
                }
            }
        }
        else
        {
            port = debugger.Transports.Item(transport);
        }
        if (port is null) throw new ArgumentOutOfRangeException(nameof(transport));
        return (debugger, port);
    }

    internal static IEnumerable<Engine> ResolveDebugEngines(this Transport transport, string[] engines)
    {
        foreach (var engine in engines)
        {
            if (Guid.TryParse(engine, out var id))
            {
                yield return transport.Engines.OfType<Engine>().First(e => e.ID == engine);
            }
            else
            {
                yield return transport.Engines.Item(engine);
            }
        }
    }
}


