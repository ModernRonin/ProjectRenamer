using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace ModernRonin.ProjectRenamer
{
    public class Executor : IExecutor
    {
        const string ToolDotnet = "dotnet";
        const string ToolGit = "git";
        public void DotNet(string arguments) => Tool(ToolDotnet, arguments);

        public void DotNet(string arguments, Action onNonZeroExitCode) =>
            Tool(ToolDotnet, arguments, onNonZeroExitCode);

        public string DotNetRead(string arguments) =>
            ToolRead(ToolDotnet, arguments, () => StandardErrorHandler(ToolDotnet, arguments));

        public void Git(string arguments) => Tool(ToolGit, arguments);

        public void Git(string arguments, Action onNonZeroExitCode) =>
            Tool(ToolGit, arguments, onNonZeroExitCode);

        public string GitRead(string arguments) =>
            ToolRead(ToolGit, arguments, () => StandardErrorHandler(ToolGit, arguments));

        public void Error(string msg, bool doResetGit = false)
        {
            Console.Error.WriteLine(msg);
            if (doResetGit)
            {
                Console.Error.WriteLine("...running git reset to undo any changes...");
                RollbackGit();
            }

            Runtime.Abort();
        }

        public void RollbackGit() => Git("reset --hard HEAD", () => { });

        static void DoWithTool(string tool,
            string arguments,
            Action onNonZeroExitCode,
            Action<ProcessStartInfo> configure,
            Action<Process> onSuccess)
        {
            var psi = new ProcessStartInfo
            {
                FileName = tool,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = false
            };
            configure(psi);
            try
            {
                var process = Process.Start(psi);
                process.WaitForExit();
                if (process.ExitCode != 0) onNonZeroExitCode();
                else onSuccess(process);
            }
            catch (Win32Exception)
            {
                onProcessStartProblem();
            }
            catch (FileNotFoundException)
            {
                onProcessStartProblem();
            }

            void onProcessStartProblem()
            {
                Console.Error.WriteLine($"{tool} could not be found - make sure it's on your PATH.");
                onNonZeroExitCode();
            }
        }

        void StandardErrorHandler(string tool, string arguments)
        {
            Error($"call '{tool} {arguments}' failed - aborting", true);
        }

        static void Tool(string tool, string arguments, Action onNonZeroExitCode)
        {
            DoWithTool(tool, arguments, onNonZeroExitCode, psi => psi.RedirectStandardOutput = false,
                _ => { });
        }

        void Tool(string tool, string arguments) =>
            Tool(tool, arguments, () => StandardErrorHandler(tool, arguments));

        static string ToolRead(string tool, string arguments, Action onNonZeroExitCode)
        {
            var result = string.Empty;
            DoWithTool(tool, arguments, onNonZeroExitCode, psi => psi.RedirectStandardOutput = true,
                p => result = p.StandardOutput.ReadToEnd());
            return result;
        }
    }
}