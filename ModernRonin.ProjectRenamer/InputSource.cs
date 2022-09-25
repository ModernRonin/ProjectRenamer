using System;
using System.IO;
using System.Linq;
using ModernRonin.FluentArgumentParser.Help;
using ModernRonin.FluentArgumentParser.Parsing;

namespace ModernRonin.ProjectRenamer;

public class InputSource : IInputSource
{
    readonly string[] _commandLineArguments;
    readonly IFilesystem _filesystem;
    readonly IBindingCommandLineParser _parser;
    readonly IRuntime _runtime;

    public InputSource(IRuntime runtime,
        IFilesystem filesystem,
        IBindingCommandLineParser parser,
        string[] commandLineArguments)
    {
        _runtime = runtime;
        _filesystem = filesystem;
        _parser = parser;
        _commandLineArguments = commandLineArguments;
    }

    public UserInput Get()
    {
        var solutionFiles = _filesystem.FindSolutionFiles(".", false);
        if (1 != solutionFiles.Length)
            throw new AbortException("Needs to be run from a directory with exactly one *.sln file in it.");
        var solutionPath = Path.GetFullPath(solutionFiles.First());
        switch (_parser.HelpOverview, _parser.Parse(_commandLineArguments))
        {
            case (_, HelpResult help):
                _runtime.Info(help.Text);
                throw new AbortException(help.IsResultOfInvalidInput ? -1 : 0);
            case (var helpOverview, Verb verb):
                if (verb.OldProjectName.Any(CommonExtensions.IsDirectorySeparator))
                {
                    throw new AbortException(
                        $"Do not specify paths for input/'old' project names, please.{Environment.NewLine}{Environment.NewLine}{helpOverview}");
                }

                verb.OldProjectName = RemoveProjectFileExtension(verb.OldProjectName,
                    verb.ProjectFileExtension);
                verb.NewProjectName = RemoveProjectFileExtension(verb.NewProjectName,
                    verb.ProjectFileExtension);

                return new UserInput(verb, solutionPath);
            default:
                throw new AbortException(
                    "Something went seriously wrong. Please create an issue at https://github.com/ModernRonin/ProjectRenamer with as much detail as possible.");
        }
    }

    static string RemoveProjectFileExtension(string projectName, string projectFileExtension) =>
        projectName.EndsWith(projectFileExtension, StringComparison.InvariantCultureIgnoreCase)
            ? projectName[..^projectFileExtension.Length]
            : projectName;
}