using System;
using System.IO;

namespace ModernRonin.ProjectRenamer;

public sealed class SettingsProvider : ISettingsProvider
{
    readonly Configuration _configuration;
    readonly IErrorHandler _errors;
    readonly IFilesystem _filesystem;
    readonly IGit _git;
    readonly IInput _input;
    readonly IProjectFinder _projectFinder;

    public SettingsProvider(Configuration configuration,
        IErrorHandler errors,
        IFilesystem filesystem,
        IGit git,
        IInput input,
        IProjectFinder projectFinder)
    {
        _configuration = configuration;
        _errors = errors;
        _filesystem = filesystem;
        _git = git;
        _input = input;
        _projectFinder = projectFinder;
    }

    public Settings GetSettings(string solutionPath)
    {
        var source = _projectFinder.FindProject(solutionPath, _configuration.OldProjectName);
        if (source is null)
            _errors.Handle($"{_configuration.OldProjectName} cannot be found in the solution");

        var isPaketUsed = _filesystem.DoesDirectoryExist(".paket");
        var result = _configuration.ToSettings();
        result = result with
        {
            IsPaketUsed = isPaketUsed,
            DoPaketInstall = result.DoPaketInstall && isPaketUsed,
            Source = source,
            Destination = source.Rename(_configuration.NewProjectName, _filesystem.CurrentDirectory)
        };
        var gitVersion = _git.GetVersion();
        if (!_configuration.DontReviewSettings)
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