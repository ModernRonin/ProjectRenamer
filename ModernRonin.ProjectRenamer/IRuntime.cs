using System;
using System.Diagnostics;

namespace ModernRonin.ProjectRenamer
{
    public interface IRuntime
    {
        void Abort();

        void DoWithTool(string tool,
            string arguments,
            Action onNonZeroExitCode,
            Action<ProcessStartInfo> configure,
            Action<Process> onSuccess);
    }
}