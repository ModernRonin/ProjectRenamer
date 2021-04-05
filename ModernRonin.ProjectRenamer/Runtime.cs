using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace ModernRonin.ProjectRenamer
{
    public class Runtime : IRuntime
    {
        public void Abort() => Environment.Exit(-1);

        public void DoWithTool(string tool,
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
    }
}