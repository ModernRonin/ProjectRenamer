using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace ModernRonin.ProjectRenamer
{
    public static class Executor
    {
        public static void Git(string arguments) => Tool("git", arguments);

        public static void Tool(string tool, string arguments) =>
            Tool(tool, arguments, () => Runtime.Error($"call '{tool} {arguments}' failed - aborting", true));

        public static void Tool(string tool, string arguments, Action onNonZeroExitCode)
        {
            var psi = new ProcessStartInfo
            {
                FileName = tool,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = false
            };
            try
            {
                var process = Process.Start(psi);
                process.WaitForExit();
                if (process.ExitCode != 0) onNonZeroExitCode();
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

        public static void DotNet(string arguments) => Tool("dotnet", arguments);
    }
}