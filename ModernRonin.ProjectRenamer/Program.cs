using System;
using System.IO;
using System.Linq;
using ModernRonin.FluentArgumentParser;
using ModernRonin.FluentArgumentParser.Help;
using ModernRonin.FluentArgumentParser.Parsing;
using static ModernRonin.ProjectRenamer.Runtime;

namespace ModernRonin.ProjectRenamer
{
    class Program
    {
        static void Main(string[] args)
        {
            var solutionFiles =
                Directory.EnumerateFiles(".", "*.sln", SearchOption.TopDirectoryOnly).ToArray();
            if (1 != solutionFiles.Length)
                Error("Needs to be run from a directory with exactly one *.sln file in it.");
            var solutionPath = Path.GetFullPath(solutionFiles.First());
            switch (parseCommandLine())
            {
                case (_, HelpResult help):
                    Console.WriteLine(help.Text);
                    Environment.Exit(help.IsResultOfInvalidInput ? -1 : 0);
                    break;
                case (var helpOverview, Configuration configuration):
                    if (configuration.OldProjectName.Any(isDirectorySeparator))
                        Error("Do not specify paths for input/'old' project names, please.");
                    configuration.OldProjectName = removeProjectFileExtension(configuration.OldProjectName);
                    configuration.NewProjectName = removeProjectFileExtension(configuration.NewProjectName);

                    new Application(configuration, solutionPath).Run();
                    break;
                default:
                    Error(
                        "Something went seriously wrong. Please create an issue at https://github.com/ModernRonin/ProjectRenamer with as much detail as possible.");
                    break;
            }

            (string, object) parseCommandLine()
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
                return (parser.HelpOverview, parser.Parse(args));
            }

            bool isDirectorySeparator(char c) =>
                c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;

            string removeProjectFileExtension(string projectName) =>
                projectName.EndsWith(Constants.ProjectFileExtension,
                    StringComparison.InvariantCultureIgnoreCase)
                    ? projectName[..^Constants.ProjectFileExtension.Length]
                    : projectName;
        }
    }
}