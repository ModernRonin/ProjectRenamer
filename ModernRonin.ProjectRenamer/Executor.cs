using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace ModernRonin.ProjectRenamer
{
    public static class Executor
    {
        const string ToolDotnet = "dotnet";
        const string ToolGit = "git";
        public static void DotNet(string arguments) => Tool(ToolDotnet, arguments);

        public static void DotNet(string arguments, Action onNonZeroExitCode) =>
            Tool(ToolDotnet, arguments, onNonZeroExitCode);

        public static string DotNetRead(string arguments) =>
            ToolRead(ToolDotnet, arguments, () => StandardErrorHandler(ToolDotnet, arguments));

        public static void Git(string arguments) => Tool(ToolGit, arguments);

        public static void Git(string arguments, Action onNonZeroExitCode) =>
            Tool(ToolGit, arguments, onNonZeroExitCode);

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

        static void StandardErrorHandler(string tool, string arguments)
        {
            Runtime.Error($"call '{tool} {arguments}' failed - aborting", true);
        }

        static void Tool(string tool, string arguments, Action onNonZeroExitCode)
        {
            DoWithTool(tool, arguments, onNonZeroExitCode, psi => psi.RedirectStandardOutput = false,
                _ => { });
        }

        static void Tool(string tool, string arguments) =>
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