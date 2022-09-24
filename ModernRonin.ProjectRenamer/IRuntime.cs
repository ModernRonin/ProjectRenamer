using System;
using System.Diagnostics;

namespace ModernRonin.ProjectRenamer;

public interface IRuntime
{
    void Abort(int exitCode = -1);

    void DoWithTool(string tool,
        string arguments,
        Action onNonZeroExitCode,
        Action<ProcessStartInfo> configure,
        Action<string> onSuccess);

    void Run(string tool, string arguments, Action onNonZeroExitCode);
    string RunAndGetOutput(string tool, string arguments, Action onNonZeroExitCode);
}