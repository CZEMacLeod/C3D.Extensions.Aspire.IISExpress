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

    public static IEnumerable<string>? GetDebugEngines(Process visualStudioProcess)
    {
        if (!TryGetVsInstance(visualStudioProcess.Id, out _DTE? visualStudioInstance))
            return null;

        var dte = (EnvDTE80.DTE2)visualStudioInstance;
        var debugger = (EnvDTE90.Debugger3)dte.Debugger;
        var transport = debugger.Transports.Item("default");

        var list = new List<string>();
        foreach(EnvDTE80.Engine e in transport.Engines)
        {
            list.Add(e.Name);
        }
        return list;
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
    public static void AttachVisualStudioToProcess(Process visualStudioProcess, Process applicationProcess, params string[] engines)
    {

        if (TryGetVsInstance(visualStudioProcess.Id, out _DTE? visualStudioInstance))
        {
            // Find the process you want the VS instance to attach to...
            DTEProcess? processToAttachTo =
                visualStudioInstance.Debugger.LocalProcesses.Cast<DTEProcess>()
                                    .FirstOrDefault(process => process.ProcessID == applicationProcess.Id);

            // Attach to the process.
            if (processToAttachTo != null)
            {
                processToAttachTo.Attach2(engines);
            }
            else
            {
                throw new InvalidOperationException(
                    "Visual Studio process cannot find specified application '" + applicationProcess.Id + "'");
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


