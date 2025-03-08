using EnvDTE;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DTEProcess = EnvDTE90.Process3;
using Process = System.Diagnostics.Process;

namespace C3D.Extensions.Aspire.VisualStudioDebug;

/// <summary>
/// Based on code from <a href="https://gist.github.com/3813175">this gist</a>.
/// </summary>
internal static class VisualStudioAttacher
{
    [DllImport("ole32.dll")]
    internal static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

    [DllImport("ole32.dll")]
    internal static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

    public static string? GetSolutionForVisualStudio(Process visualStudioProcess)
    {
        if (TryGetVsInstance(visualStudioProcess.Id, out _DTE? visualStudioInstance))
        {
            try
            {
                return visualStudioInstance.Solution.FullName;
            }
            catch (Exception)
            {
            }
        }

        return null;
    }

    public static Process? GetAttachedVisualStudio(Process applicationProcess)
    {
        IEnumerable<Process> visualStudios = GetVisualStudioProcesses();

        foreach (Process visualStudio in visualStudios)
        {
            if (TryGetVsInstance(visualStudio.Id, out _DTE? visualStudioInstance))
            {
                try
                {
                    foreach (EnvDTE.Process debuggedProcess in visualStudioInstance.Debugger.DebuggedProcesses)
                    {
                        if (debuggedProcess.ProcessID == applicationProcess.Id)
                        {
                            return visualStudio;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        return null;
    }

    public static IEnumerable<(string id, string name)> GetDebugTransports(Process visualStudioProcess)
    {
        if (!TryGetVsInstance(visualStudioProcess.Id, out _DTE? visualStudioInstance))
            yield break;

        var dte = (EnvDTE80.DTE2)visualStudioInstance;
        var debugger = (EnvDTE90.Debugger3)dte.Debugger;
        foreach (EnvDTE80.Transport e in debugger.Transports)
        {
            yield return (e.ID, e.Name);
        }
    }

    public static IEnumerable<(int id, string name, bool isDebugged)> GetDebugProcesses(Process visualStudioProcess, string transport, string? qualifier)
    {
        if (!TryGetVsInstance(visualStudioProcess.Id, out _DTE? visualStudioInstance))
            yield break;

        var dte = (EnvDTE80.DTE2)visualStudioInstance;
        var debugger = (EnvDTE90.Debugger3)dte.Debugger;
        EnvDTE80.Transport port = debugger.Transports.Item(transport);
        if (port is null)
        {
            yield break;
        }
        Processes processes = debugger.GetProcesses(port, qualifier ?? string.Empty);
        foreach (EnvDTE80.Process2 e in processes)
        {
            yield return (e.ProcessID, e.Name, e.IsBeingDebugged);
        }
    }

    public static IEnumerable<(string id, string name, int result)> GetDebugEngines(Process visualStudioProcess, string transport = "default")
    {
        if (!TryGetVsInstance(visualStudioProcess.Id, out _DTE? visualStudioInstance))
            yield break;

        var dte = (EnvDTE80.DTE2)visualStudioInstance;
        var debugger = (EnvDTE90.Debugger3)dte.Debugger;
        var port = debugger.Transports.Item(transport);
        if (port is null)
        {
            yield break;
        }

        foreach (EnvDTE80.Engine e in port.Engines)
        {
            yield return (e.ID, e.Name, e.AttachResult);
        }
    }

    /// <summary>
    /// The method to use to attach visual studio to a specified process.
    /// </summary>
    /// <param name="visualStudioProcess">
    /// The visual studio process to attach to.
    /// </param>
    /// <param name="applicationProcess">
    /// The application process that needs to be debugged.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the application process is null.
    /// </exception>
    public static void AttachVisualStudioToProcess(Process visualStudioProcess, int processId, params string[] engines)
        => AttachVisualStudioToProcess(visualStudioProcess,
                vs => vs.Debugger.LocalProcesses
                        .Cast<DTEProcess>()
                        .FirstOrDefault(process => process.ProcessID == processId),
                engines);

    /// <summary>
    /// The method to use to attach visual studio to a process by specifying connection information.
    /// </summary>
    /// <param name="visualStudioProcess">
    /// The visual studio process to attach to.
    /// </param>
    /// <param name="applicationProcess">
    /// The application process that needs to be debugged.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the application process is null.
    /// </exception>
    public static void AttachVisualStudioToProcess(Process visualStudioProcess, string transport, string? qualifier, params string[] engines)
    => AttachVisualStudioToProcess(visualStudioProcess,
            vs =>
            {
                var dte = (EnvDTE80.DTE2)vs;
                var debugger = (EnvDTE90.Debugger3)dte.Debugger;
                EnvDTE80.Transport port = debugger.Transports.Item(transport);
                Processes processes = debugger.GetProcesses(port, qualifier ?? string.Empty);
                if (processes.Count!=1)
                {
                    throw new ArgumentOutOfRangeException(nameof(qualifier), "Qualifer did not result in a single process");
                }
                return (DTEProcess)processes.Item(1);
            },
            engines);

    private static void AttachVisualStudioToProcess(Process visualStudioProcess, Func<DTE, DTEProcess?> applicationProcess, params string[] engines)
    {

        if (TryGetVsInstance(visualStudioProcess.Id, out _DTE? visualStudioInstance))
        {
            // Find the process you want the VS instance to attach to...
            DTEProcess? processToAttachTo = applicationProcess(visualStudioInstance);

            // Attach to the process.
            if (processToAttachTo != null)
            {
                processToAttachTo.Attach2(engines);
            }
            else
            {
                throw new InvalidOperationException("Visual Studio cannot find specified application");
            }
        }
    }

    private static IEnumerable<Process> GetVisualStudioProcesses() =>
        Process.GetProcesses()
            .Where(o => o.ProcessName.Contains("devenv", StringComparison.OrdinalIgnoreCase))
            .ToArray();

    private static bool TryGetVsInstance(int processId, [NotNullWhen(true)] out _DTE? instance)
    {
        IntPtr numFetched = IntPtr.Zero;
        var monikers = new IMoniker[1];

        GetRunningObjectTable(0, out IRunningObjectTable runningObjectTable);
        runningObjectTable.EnumRunning(out IEnumMoniker monikerEnumerator);
        monikerEnumerator.Reset();

        while (monikerEnumerator.Next(1, monikers, numFetched) == 0)
        {
            CreateBindCtx(0, out IBindCtx ctx);

            monikers[0].GetDisplayName(ctx, null, out string runningObjectName);

            runningObjectTable.GetObject(monikers[0], out object runningObjectVal);

            if (runningObjectVal is _DTE && runningObjectName.StartsWith("!VisualStudio"))
            {
                int currentProcessId = int.Parse(runningObjectName.Split(':')[1]);

                if (currentProcessId == processId)
                {
                    instance = (_DTE)runningObjectVal;
                    return true;
                }
            }
        }

        instance = null;
        return false;
    }
}


