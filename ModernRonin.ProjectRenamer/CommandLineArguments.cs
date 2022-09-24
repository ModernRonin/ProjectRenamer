using ModernRonin.FluentArgumentParser;
using ModernRonin.FluentArgumentParser.Parsing;

namespace ModernRonin.ProjectRenamer;

public static class CommandLineArguments
{
    public static IBindingCommandLineParser CreateParser()
    {
        var result = ParserFactory.Create("renameproject",
            "Rename C# projects comfortably, including renaming directories, updating references, keeping your git history intact, creating a git commit and updating paket, if needed.");
        var cfg = result.DefaultVerb<Configuration>();
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
        return result;
    }
}