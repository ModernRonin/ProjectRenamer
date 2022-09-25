using System;

namespace ModernRonin.ProjectRenamer;

public sealed class AbortException : Exception
{
    public AbortException(string msg, bool doResetGit = false, int exitCode = -1) : base(msg)
    {
        DoResetGit = doResetGit;
        ExitCode = exitCode;
    }

    public AbortException() : this(string.Empty, true) { }

    public AbortException(int exitCode) : this(string.Empty, false, exitCode) { }

    public AbortException(string tool, string arguments) : this(
        $"call '{tool} {arguments}' failed - aborting", true) { }

    public void Deconstruct(out string message, out bool doResetGit, out int exitCode)
    {
        message = Message;
        doResetGit = DoResetGit;
        exitCode = ExitCode;
    }

    public bool DoResetGit { get; }
    public int ExitCode { get; }
}