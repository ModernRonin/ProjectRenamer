﻿using System;
using System.IO;
using System.Linq;
using ModernRonin.FluentArgumentParser;
using ModernRonin.FluentArgumentParser.Help;
using ModernRonin.FluentArgumentParser.Parsing;

namespace ModernRonin.ProjectRenamer
{
    public class ConfigurationSetup : IConfigurationSetup
    {
        readonly ILogger _console;
        readonly IErrorHandler _errors;
        readonly IFilesystem _filesystem;
        readonly IRuntime _runtime;

        public ConfigurationSetup(ILogger console,
            IRuntime runtime,
            IErrorHandler errors,
            IFilesystem filesystem)
        {
            _console = console;
            _runtime = runtime;
            _errors = errors;
            _filesystem = filesystem;
        }

        public (Configuration configuration, string solutionPath) Get(string[] commandLineArguments)
        {
            var solutionFiles = _filesystem.FindSolutionFiles(".", false);
            if (1 != solutionFiles.Length)
                _errors.Handle("Needs to be run from a directory with exactly one *.sln file in it.");
            var solutionPath = Path.GetFullPath(solutionFiles.First());
            switch (ParseCommandLine(commandLineArguments))
            {
                case (_, HelpResult help):
                    _console.Info(help.Text);
                    _runtime.Abort(help.IsResultOfInvalidInput ? -1 : 0);
                    break;
                case (var helpOverview, Configuration configuration):
                    if (configuration.OldProjectName.Any(CommonExtensions.IsDirectorySeparator))
                    {
                        _errors.Handle(
                            $"Do not specify paths for input/'old' project names, please.{Environment.NewLine}{Environment.NewLine}{helpOverview}");
                    }

                    configuration.OldProjectName = RemoveProjectFileExtension(configuration.OldProjectName, configuration.ProjectFileExtension);
                    configuration.NewProjectName = RemoveProjectFileExtension(configuration.NewProjectName, configuration.ProjectFileExtension);

                    return (configuration, solutionPath);
                default:
                    _errors.Handle(
                        "Something went seriously wrong. Please create an issue at https://github.com/ModernRonin/ProjectRenamer with as much detail as possible.");
                    break;
            }

            return default;
        }

        static (string, object) ParseCommandLine(string[] args)
        {
            var parser = ParserFactory.Create("renameproject",
                "Rename C# projects comfortably, including renaming directories, updating references, keeping your git history intact, creating a git commit and updating paket, if needed.");
            var cfg = parser.DefaultVerb<Configuration>();
            cfg.Parameter(c => c.DontCreateCommit)
                .WithLongName("no-commit")
                .WithShortName("nc")
                .WithHelp("don't create a commit after renaming has finished");
            cfg.Parameter(c => c.DoRunBuild)
                .WithLongName("build")
                .WithShortName("b")
                .WithHelp(
                    "run a build after renaming (but before committing) to make sure everything worked fine.");
            cfg.Parameter(c => c.DontRunPaketInstall)
                .WithLongName("no-paket")
                .WithShortName("np")
                .WithHelp("don't run paket install (if your solution uses paket as a local tool)");
            cfg.Parameter(c => c.DontReviewSettings)
                .WithLongName("no-review")
                .WithShortName("nr")
                .WithHelp("don't review all settings before starting work");
            cfg.Parameter(c => c.OldProjectName)
                .WithLongName("old-name")
                .WithShortName("o")
                .ExpectAt(0)
                .WithHelp(
                    "the name of the existing project - don't provide path or extension, just the name as you see it in VS.");
            cfg.Parameter(c => c.NewProjectName)
                .WithLongName("new-name")
                .WithShortName("n")
                .ExpectAt(1)
                .WithHelp("the new desired project name, again without path or extension");
            cfg.Parameter(c => c.ExcludedDirectory)
                .MakeOptional()
                .ExpectAt(2)
                .WithLongName("exclude")
                .WithShortName("e")
                .WithHelp(
                    "exclude this directory from project reference updates; must be a relative path to the current directory");
            cfg.Parameter(c => c.ProjectFileExtension)
                .MakeOptional()
                .WithLongName("project-extension")
                .WithShortName("pe")
                .ExpectAt(3)
                .WithHelp("the file extension for the project, ex: .csproj, .vbproj");
            return (parser.HelpOverview, parser.Parse(args));
        }

        static string RemoveProjectFileExtension(string projectName, string projectFileExtension) =>
            projectName.EndsWith(projectFileExtension, StringComparison.InvariantCultureIgnoreCase)
                ? projectName[..^projectFileExtension.Length]
                : projectName;
    }
}