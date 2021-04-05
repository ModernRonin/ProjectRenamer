using System;

namespace ModernRonin.ProjectRenamer
{
    public interface IExecutor
    {
        void DotNet(string arguments);
        void DotNet(string arguments, Action onNonZeroExitCode);
        string DotNetRead(string arguments);
        void Error(string msg, bool doResetGit = false);
        void Git(string arguments);
        void Git(string arguments, Action onNonZeroExitCode);
        string GitRead(string arguments);
        void RollbackGit();
    }
}