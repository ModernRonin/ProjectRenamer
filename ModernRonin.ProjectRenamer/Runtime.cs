using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace ModernRonin.ProjectRenamer;

/// <summary>
///     Unfortunately, this is fundamentally untestable, but at least we've concentrated almost all the untestable stuff
///     into one
///     type.
/// </summary>
public class Runtime : IRuntime
{
    public void Abort(int exitCode = -1) => Environment.Exit(exitCode);

    public bool AskUser(string question)
    {
        Console.WriteLine($"{question} [Enter=Yes, any other key=No]");
        var key = Console.ReadKey();
        return key.Key == ConsoleKey.Enter;
    }

    public void Error(string msg) => Console.Error.WriteLine(msg);
    public void Info(string msg) => Console.WriteLine(msg);

    public void Run(string tool, string arguments, Action onNonZeroExitCode) =>
        Run(tool, arguments, onNonZeroExitCode, psi => psi.RedirectStandardOutput = false, _ => { });

    public string RunAndGetOutput(string tool, string arguments, Action onNonZeroExitCode)
    {
        var result = string.Empty;
        Run(tool, arguments, onNonZeroExitCode, psi => psi.RedirectStandardOutput = true,
            o => result = o);
        return result;
    }

    void Run(string tool,
        string arguments,
        Action onNonZeroExitCode,
        Action<ProcessStartInfo> configure,
        Action<string> onSuccess)
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
            var output = psi.RedirectStandardOutput ? process.StandardOutput.ReadToEnd() : string.Empty;
            process.WaitForExit();
            if (process.ExitCode != 0) onNonZeroExitCode();
            else onSuccess(output);
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
            Error($"{tool} could not be found - make sure it's on your PATH.");
            onNonZeroExitCode();
        }
    }
}