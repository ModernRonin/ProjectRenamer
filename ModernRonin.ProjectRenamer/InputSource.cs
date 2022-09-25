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

    public InputSource(IFilesystem filesystem,
        IBindingCommandLineParser parser,
        string[] commandLineArguments)
    {
        _filesystem = filesystem;
        _parser = parser;
        _commandLineArguments = commandLineArguments;
    }

    public UserInput Get()
    {
        var solutionFiles = _filesystem.FindSolutionFiles(".", false);
        if (1 != solutionFiles.Length)
            throw new AbortException("Needs to be run from a directory with exactly one *.sln file in it.");
        var parseResult = _parser.Parse(_commandLineArguments);
        if (parseResult is HelpResult help)
            throw new AbortException(help.Text, exitCode: help.IsResultOfInvalidInput ? -1 : 0);

        if (parseResult is not Verb verb)
        {
            throw new AbortException(
                "Something went seriously wrong. Please create an issue at https://github.com/ModernRonin/ProjectRenamer with as much detail as possible.");
        }

        if (verb.OldProjectName.Any(CommonExtensions.IsDirectorySeparator))
        {
            throw new AbortException(
                $"Do not specify paths for input/'old' project names, please.{Environment.NewLine}{Environment.NewLine}{_parser.HelpOverview}");
        }

        verb.OldProjectName = RemoveProjectFileExtension(verb.OldProjectName,
            verb.ProjectFileExtension);
        verb.NewProjectName = RemoveProjectFileExtension(verb.NewProjectName,
            verb.ProjectFileExtension);

        return new UserInput(verb, Path.GetFullPath(solutionFiles.First()));
    }

    static string RemoveProjectFileExtension(string projectName, string projectFileExtension) =>
        projectName.EndsWith(projectFileExtension, StringComparison.InvariantCultureIgnoreCase)
            ? projectName[..^projectFileExtension.Length]
            : projectName;
}