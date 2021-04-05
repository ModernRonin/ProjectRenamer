using System;

namespace ModernRonin.ProjectRenamer
{
    public interface IExecutor
    {
        void Tool(string tool, string arguments, Action onNonZeroExitCode);
        void Tool(string tool, string arguments);
        string ToolRead(string tool, string arguments, Action onNonZeroExitCode);
    }
}