using System;

namespace ModernRonin.ProjectRenamer;

public interface IToolRunner
{
    void Run(string arguments);
    void Run(string arguments, Action onError);
    void Run(string arguments, string errorMessage);
    void Run(string arguments, AbortException errorException);
    string RunAndGetOutput(string arguments, Action onError);
    string RunAndGetOutput(string arguments);
}