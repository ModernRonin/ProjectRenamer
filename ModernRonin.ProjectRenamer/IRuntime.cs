using System;

namespace ModernRonin.ProjectRenamer;

public interface IRuntime
{
    void Abort(int exitCode = -1);
    bool AskUser(string question);

    void Run(string tool, string arguments, Action onNonZeroExitCode);
    string RunAndGetOutput(string tool, string arguments, Action onNonZeroExitCode);
}