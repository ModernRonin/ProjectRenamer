using System;

namespace ModernRonin.ProjectRenamer;

public class Git : IGit
{
    readonly IToolRunner _runner;

    public Git(IRuntime runtime,
        Func<string, IToolRunner> toolRunnerFactory) =>
        _runner = toolRunnerFactory("git");

    public void Commit(string msg)
    {
        var arguments = $"commit -m \"{msg}\"";
        _runner.Run(arguments, $"'git {arguments}' failed");
    }

    public void EnsureIsClean()
    {
        run("update-index -q --refresh");
        run("diff-index --quiet --cached HEAD --");
        run("diff-files --quiet");
        run("ls-files --exclude-standard --others");

        void run(string arguments) =>
            _runner.Run(arguments, new AbortException("git does not seem to be clean, check git status"));
    }

    public string GetVersion() => _runner.RunAndGetOutput("--version");

    public void Move(string oldPath, string newPath) =>
        _runner.Run($"mv {oldPath.EscapeForShell()} {newPath.EscapeForShell()}");

    public void RollbackAllChanges() => _runner.Run("reset --hard HEAD", () => { });

    public void StageAllChanges() => _runner.Run("add .");
}