using System;

namespace ModernRonin.ProjectRenamer
{
    public class Git : IGit
    {
        const string ToolGit = "git";

        readonly IErrorHandler _errors;
        readonly IExecutor _executor;
        readonly ILogger _logger;

        public Git(IExecutor executor, ILogger logger, IErrorHandler errors)
        {
            _executor = executor;
            _logger = logger;
            _errors = errors;
        }

        public void Commit(string msg)
        {
            var arguments = $"commit -m \"{msg}\"";
            Run(arguments,
                () => _logger.Error($"'git {arguments}' failed"));
        }

        public void EnsureIsClean()
        {
            run("update-index -q --refresh");
            run("diff-index --quiet --cached HEAD --");
            run("diff-files --quiet");
            run("ls-files --exclude-standard --others");

            void run(string arguments) =>
                Run(arguments,
                    () => _errors.Handle("git does not seem to be clean, check git status"));
        }

        public string GetVersion() => Read("--version");

        public void Move(string oldPath, string newPath) => Run($"mv {oldPath} {newPath}");
        public void RollbackAllChanges() => Run("reset --hard HEAD", () => { });
        public void StageAllChanges() => Run("add .");

        string Read(string arguments) =>
            _executor.ToolRead(ToolGit, arguments, () => _errors.Handle(ToolGit, arguments));

        void Run(string arguments) => _executor.Tool(ToolGit, arguments);

        void Run(string arguments, Action onNonZeroExitCode) =>
            _executor.Tool(ToolGit, arguments, onNonZeroExitCode);
    }
}