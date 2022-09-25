using System;

namespace ModernRonin.ProjectRenamer;

public class ErrorHandler : IErrorHandler
{
    readonly Lazy<IGit> _git;
    readonly IRuntime _runtime;

    public ErrorHandler(Lazy<IGit> git, IRuntime runtime)
    {
        _git = git;
        _runtime = runtime;
    }

    public void HandleException(AbortException exception)
    {
        var (msg, doResetGit, exitCode) = exception;
        _runtime.Error(msg);
        if (doResetGit)
        {
            _runtime.Error("...running git reset to undo any changes...");
            _git.Value.RollbackAllChanges();
        }

        _runtime.Abort(exitCode);
    }
}