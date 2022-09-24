using System;

namespace ModernRonin.ProjectRenamer;

public class Git : IGit
{
    readonly IErrorHandler _errors;
    readonly ILogger _logger;
    readonly IToolRunner _runner;

    public Git(ILogger logger,
        IErrorHandler errors,
        Func<string, IToolRunner> toolRunnerFactory)
    {
        _logger = logger;
        _errors = errors;
        _runner = toolRunnerFactory("git");
    }

    public void Commit(string msg)
    {
        var arguments = $"commit -m \"{msg}\"";
        _runner.Run(arguments, onError);

        void onError() => _logger.Error($"'git {arguments}' failed");
    }

    public void EnsureIsClean()
    {
        run("update-index -q --refresh");
        run("diff-index --quiet --cached HEAD --");
        run("diff-files --quiet");
        run("ls-files --exclude-standard --others");

        void run(string arguments) => _runner.Run(arguments, onError);

        void onError() => _errors.Handle("git does not seem to be clean, check git status");
    }

    public string GetVersion() => _runner.RunAndGetOutput("--version");

    public void Move(string oldPath, string newPath) =>
        _runner.Run($"mv {oldPath.EscapeForShell()} {newPath.EscapeForShell()}");

    public void RollbackAllChanges() => _runner.Run("reset --hard HEAD", () => { });

    public void StageAllChanges() => _runner.Run("add .");
}