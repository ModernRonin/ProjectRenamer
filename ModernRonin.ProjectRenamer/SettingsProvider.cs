using System;
using System.IO;

namespace ModernRonin.ProjectRenamer;

public sealed class SettingsProvider : ISettingsProvider
{
    readonly IErrorHandler _errors;
    readonly IFilesystem _filesystem;
    readonly IGit _git;
    readonly IInput _input;
    readonly IProjectFinder _projectFinder;
    readonly Verb _verb;

    public SettingsProvider(Verb verb,
        IErrorHandler errors,
        IFilesystem filesystem,
        IGit git,
        IInput input,
        IProjectFinder projectFinder)
    {
        _verb = verb;
        _errors = errors;
        _filesystem = filesystem;
        _git = git;
        _input = input;
        _projectFinder = projectFinder;
    }

    public Settings GetSettings(string solutionPath)
    {
        var source = _projectFinder.FindProject(solutionPath, _verb.OldProjectName);
        if (source is null) _errors.Handle($"{_verb.OldProjectName} cannot be found in the solution");

        var isPaketUsed = _filesystem.DoesDirectoryExist(".paket");
        var result = _verb.ToSettings();
        result = result with
        {
            IsPaketUsed = isPaketUsed,
            DoPaketInstall = result.DoPaketInstall && isPaketUsed,
            Source = source,
            Destination = source.Rename(_verb.NewProjectName, _filesystem.CurrentDirectory)
        };
        var gitVersion = _git.GetVersion();
        if (!_verb.DontReviewSettings)
        {
            var lines = new[]
            {
                "Please review the following settings:",
                $"Project:                   {result.Source.Name}",
                $"found at:                  {result.Source.FullPath}",
                $"Rename to:                 {Path.GetFileName(result.Destination.FullPath)}",
                $"at:                        {result.Destination.Directory}",
                $"VS Solution folder:        {result.Source.SolutionFolder ?? "none"}",
                $"exclude:                   {result.ExcludedDirectory}",
                $"Run paket install:         {result.DoPaketInstall.AsText()}",
                $"Run build after rename:    {result.DoBuild.AsText()}",
                $"Create automatic commit:   {result.DoCreateCommit.AsText()}",
                $"Git version:               {gitVersion}",
                "-----------------------------------------------",
                "Do you want to continue with the rename operation?"
            };
            if (!_input.AskUser(string.Join(Environment.NewLine, lines)))
                _errors.Handle("You decided to abort.");
        }

        return result;
    }
}