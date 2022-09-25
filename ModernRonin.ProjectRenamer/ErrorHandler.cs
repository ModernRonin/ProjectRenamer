using System;

namespace ModernRonin.ProjectRenamer;

public class ErrorHandler : IErrorHandler
{
    readonly Lazy<IGit> _git;
    readonly ILogger _logger;
    readonly IRuntime _runtime;

    public ErrorHandler(ILogger logger, Lazy<IGit> git, IRuntime runtime)
    {
        _logger = logger;
        _git = git;
        _runtime = runtime;
    }

    public void HandleException(AbortException exception)
    {
        var (msg, doResetGit, exitCode) = exception;
        _logger.Error(msg);
        if (doResetGit)
        {
            _logger.Error("...running git reset to undo any changes...");
            _git.Value.RollbackAllChanges();
        }

        _runtime.Abort(exitCode);
    }
}