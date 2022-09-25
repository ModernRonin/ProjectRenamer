using System;
using System.IO;

namespace ModernRonin.ProjectRenamer;

public sealed class SettingsProvider : ISettingsProvider
{
    readonly IFilesystem _filesystem;
    readonly IGit _git;
    readonly IInputSource _inputSource;
    readonly IProjectFinder _projectFinder;
    readonly IRuntime _runtime;

    public SettingsProvider(IInputSource inputSource,
        IFilesystem filesystem,
        IGit git,
        IProjectFinder projectFinder,
        IRuntime runtime)
    {
        _inputSource = inputSource;
        _filesystem = filesystem;
        _git = git;
        _projectFinder = projectFinder;
        _runtime = runtime;
    }

    public Settings GetSettings()
    {
        var (verb, solutionPath) = _inputSource.Get();
        var source = _projectFinder.FindProject(solutionPath, verb.OldProjectName);
        if (source is null)
            throw new AbortException($"{verb.OldProjectName} cannot be found in the solution");

        var isPaketUsed = _filesystem.DoesDirectoryExist(".paket");
        var result = verb.ToSettings();
        result = result with
        {
            IsPaketUsed = isPaketUsed,
            DoPaketInstall = result.DoPaketInstall && isPaketUsed,
            Source = source,
            Destination = source.Rename(verb.NewProjectName, _filesystem.CurrentDirectory)
        };
        var gitVersion = _git.GetVersion();
        if (!verb.DontReviewSettings)
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
            if (!_runtime.AskUser(string.Join(Environment.NewLine, lines)))
                throw new AbortException("You decided to abort.");
        }

        return result;
    }
}