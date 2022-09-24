using System;

namespace ModernRonin.ProjectRenamer;

public sealed class ToolRunner : IToolRunner
{
    readonly IErrorHandler _errors;
    readonly IRuntime _runtime;
    readonly string _tool;

    public ToolRunner(IRuntime runtime, IErrorHandler errors, string tool)
    {
        _tool = tool;
        _errors = errors;
        _runtime = runtime;
    }

    public void Run(string arguments) => Run(arguments, OnError(arguments));

    public void Run(string arguments, Action onError) => _runtime.Run(_tool, arguments, onError);

    public string RunAndGetOutput(string arguments, Action onError) =>
        _runtime.RunAndGetOutput(_tool, arguments, onError);

    public string RunAndGetOutput(string arguments) => RunAndGetOutput(arguments, OnError(arguments));

    Action OnError(string arguments)
    {
        return () => _errors.Handle(_tool, arguments);
    }
}