using System;

namespace ModernRonin.ProjectRenamer
{
    public class Executor : IExecutor
    {
        const string ToolDotnet = "dotnet";
        readonly IErrorHandler _errors;
        readonly IRuntime _runtime;

        public Executor(IRuntime runtime, IErrorHandler errors)
        {
            _runtime = runtime;
            _errors = errors;
        }

        public void DotNet(string arguments) => Tool(ToolDotnet, arguments);

        public void DotNet(string arguments, Action onNonZeroExitCode) =>
            Tool(ToolDotnet, arguments, onNonZeroExitCode);

        public string DotNetRead(string arguments) =>
            ToolRead(ToolDotnet, arguments, () => _errors.Handle(ToolDotnet, arguments));

        public void Tool(string tool, string arguments, Action onNonZeroExitCode)
        {
            _runtime.DoWithTool(tool, arguments, onNonZeroExitCode, psi => psi.RedirectStandardOutput = false,
                _ => { });
        }

        public void Tool(string tool, string arguments) =>
            Tool(tool, arguments, () => _errors.Handle(tool, arguments));

        public string ToolRead(string tool, string arguments, Action onNonZeroExitCode)
        {
            var result = string.Empty;
            _runtime.DoWithTool(tool, arguments, onNonZeroExitCode, psi => psi.RedirectStandardOutput = true,
                p => result = p.StandardOutput.ReadToEnd());
            return result;
        }
    }
}