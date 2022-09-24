using System;
using System.IO;
using System.Linq;

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
        var oldProject = _projectFinder.FindProject(solutionPath, _configuration.OldProjectName);
        if (oldProject is null)
            _errors.Handle($"{_configuration.OldProjectName} cannot be found in the solution");

        var oldDir = Path.GetDirectoryName(oldProject.Path);
        var newBaseDir = _configuration.NewProjectName.Any(CommonExtensions.IsDirectorySeparator)
            ? _filesystem.CurrentDirectory
            : Path.GetDirectoryName(oldDir);
        var newDir = _configuration.NewProjectName.ToAbsolutePath(newBaseDir);
        var newFileName = Path.GetFileName(_configuration.NewProjectName);
        var newProjectPath = Path.Combine(newDir, $"{newFileName}{_configuration.ProjectFileExtension}");
        var isPaketUsed = _filesystem.DoesDirectoryExist(".paket");
        var gitVersion = _git.GetVersion();
        if (!_configuration.DontReviewSettings)
        {
            var lines = new[]
            {
                "Please review the following settings:",
                $"Project:                   {_configuration.OldProjectName}",
                $"found at:                  {oldProject.Path}",
                $"Rename to:                 {newFileName}",
                $"at:                        {newProjectPath}",
                $"VS Solution folder:        {oldProject.SolutionFolder ?? "none"}",
                $"exclude:                   {_configuration.ExcludedDirectory}",
                $"Paket in use:              {isPaketUsed.AsText()}",
                $"Run paket install:         {(!_configuration.DontRunPaketInstall).AsText()}",
                $"Run build after rename:    {_configuration.DoRunBuild.AsText()}",
                $"Create automatic commit:   {(!_configuration.DontCreateCommit).AsText()}",
                $"Git version:               {gitVersion}",
                "-----------------------------------------------",
                "Do you want to continue with the rename operation?"
            };
            if (!_input.AskUser(string.Join(Environment.NewLine, lines)))
                _errors.Handle("You decided to abort.");
        }

        return new Settings(oldProject.Path, oldProject.SolutionFolder, oldDir, newDir, newProjectPath,
            isPaketUsed);
    }
}