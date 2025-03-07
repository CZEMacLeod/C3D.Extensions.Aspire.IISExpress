using EnvDTE;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DTEProcess = EnvDTE90.Process3;
using Process = System.Diagnostics.Process;

// Based on https://gist.github.com/atruskie/3813175#file-visualstudioattacher-cs

namespace C3D.Extensions.Aspire.IISExpress;

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoAttachVs.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Example taken from this gist.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

/// <summary>
/// Example taken from <a href="https://gist.github.com/3813175">this gist</a>.
/// </summary>
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation",
    Justification = "Reviewed. Suppression is OK here.", Scope = "class")]
public static class VisualStudioAttacher
{
    [DllImport("ole32.dll")]
    internal static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

    [DllImport("ole32.dll")]
    internal static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern bool SetForegroundWindow(IntPtr hWnd);

    //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //internal static extern IntPtr SetFocus(IntPtr hWnd);

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

        return transport.Engines.Cast<EnvDTE80.Engine>().Select(e=>e.Name).ToList();
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

                ShowWindow((int)visualStudioProcess.MainWindowHandle, 3);
                SetForegroundWindow(visualStudioProcess.MainWindowHandle);
            }
            else
            {
                throw new InvalidOperationException(
                    "Visual Studio process cannot find specified application '" + applicationProcess.Id + "'");
            }
        }
    }

    /// <summary>
    /// The get visual studio for solutions.
    /// </summary>
    /// <param name="solutionNames">
    /// The solution names.
    /// </param>
    /// <returns>
    /// The <see cref="Process"/>.
    /// </returns>
    public static Process? GetVisualStudioForSolutions(List<string> solutionNames)
    {
        foreach (string solution in solutionNames)
        {
            var visualStudioForSolution = GetVisualStudioForSolution(solution);
            if (visualStudioForSolution is not null)
            {
                return visualStudioForSolution;
            }
        }

        return null;
    }

    /// <summary>
    /// The get visual studio process that is running and has the specified solution loaded.
    /// </summary>
    /// <param name="solutionName">
    /// The solution name to look for.
    /// </param>
    /// <returns>
    /// The visual studio <see cref="Process"/> with the specified solution name.
    /// </returns>
    public static Process? GetVisualStudioForSolution(string solutionName)
    {
        IEnumerable<Process> visualStudios = GetVisualStudioProcesses();

        foreach (Process visualStudio in visualStudios)
        {
            if (TryGetVsInstance(visualStudio.Id, out _DTE? visualStudioInstance))
            {
                try
                {
                    string actualSolutionName = Path.GetFileName(visualStudioInstance.Solution.FullName);

                    if (string.Compare(
                        actualSolutionName,
                        solutionName,
                        StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        return visualStudio;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        return null;
    }

    [DllImport("User32")]
    private static extern int ShowWindow(int hwnd, int nCmdShow);

    private static IEnumerable<Process> GetVisualStudioProcesses()
    {
        Process[] processes = Process.GetProcesses();
        return processes.Where(o => o.ProcessName.Contains("devenv", StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    private static bool TryGetVsInstance(int processId, [NotNullWhen(true)] out _DTE? instance)
    {
        IntPtr numFetched = IntPtr.Zero;
        IRunningObjectTable runningObjectTable;
        IEnumMoniker monikerEnumerator;
        IMoniker[] monikers = new IMoniker[1];

        GetRunningObjectTable(0, out runningObjectTable);
        runningObjectTable.EnumRunning(out monikerEnumerator);
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


