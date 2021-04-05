using System;

namespace ModernRonin.ProjectRenamer
{
    public interface IExecutor
    {
        void DotNet(string arguments);
        void DotNet(string arguments, Action onNonZeroExitCode);
        string DotNetRead(string arguments);
        void Tool(string tool, string arguments, Action onNonZeroExitCode);
        void Tool(string tool, string arguments);
        string ToolRead(string tool, string arguments, Action onNonZeroExitCode);
    }
}