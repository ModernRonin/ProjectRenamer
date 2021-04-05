namespace ModernRonin.ProjectRenamer
{
    public class Git : IGit
    {
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
            _executor.Git(arguments,
                () => { _logger.Error($"'git {arguments}' failed"); });
        }

        public void EnsureIsClean()
        {
            run("update-index -q --refresh");
            run("diff-index --quiet --cached HEAD --");
            run("diff-files --quiet");
            run("ls-files --exclude-standard --others");

            void run(string arguments) =>
                _executor.Git(arguments,
                    () => _errors.Handle("git does not seem to be clean, check git status"));
        }

        public string GetVersion() => _executor.GitRead("--version");

        public void Move(string oldPath, string newPath) => _executor.Git($"mv {oldPath} {newPath}");
        public void RollbackAllChanges() => _executor.Git("reset --hard HEAD", () => { });
        public void StageAllChanges() => _executor.Git("add .");
    }
}