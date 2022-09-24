using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;

namespace ModernRonin.ProjectRenamer;

public sealed class SettingsProvider : ISettingsProvider
{
    readonly Configuration _configuration;
    readonly IErrorHandler _errors;
    readonly IFilesystem _filesystem;
    readonly IGit _git;
    readonly IInput _input;

    public SettingsProvider(Configuration configuration,
        IErrorHandler errors,
        IFilesystem filesystem,
        IGit git,
        IInput input)
    {
        _configuration = configuration;
        _errors = errors;
        _filesystem = filesystem;
        _git = git;
        _input = input;
    }

    public Settings GetSettings(string solutionPath)
    {
        var (wasFound, oldProjectPath, solutionFolderPath) = findProject();
        if (!wasFound) _errors.Handle($"{_configuration.OldProjectName} cannot be found in the solution");

        var oldDir = Path.GetDirectoryName(oldProjectPath);
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
                $"found at:                  {oldProjectPath}",
                $"Rename to:                 {newFileName}",
                $"at:                        {newProjectPath}",
                $"VS Solution folder:        {solutionFolderPath ?? "none"}",
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

        return new Settings(oldProjectPath, solutionFolderPath, oldDir, newDir, newProjectPath, isPaketUsed);

        (bool wasFound, string projectPath, string solutionFolder) findProject()
        {
            var solution = SolutionFile.Parse(solutionPath);
            var project = solution.ProjectsInOrder.FirstOrDefault(p =>
                p.ProjectName.EndsWith(_configuration.OldProjectName,
                    StringComparison.InvariantCultureIgnoreCase));
            return project switch
            {
                null => (false, null, null),
                _ when project.ParentProjectGuid == null => (true,
                    Path.Combine(project.AbsolutePath.Split('\\')),
                    null),
                _ => (true, Path.Combine(project.AbsolutePath.Split('\\')),
                    path(solution.ProjectsByGuid[project.ParentProjectGuid]))
            };

            string path(ProjectInSolution p)
            {
                if (p.ParentProjectGuid == null) return p.ProjectName;
                var parent = solution.ProjectsByGuid[p.ParentProjectGuid];
                var parentPath = path(parent);
                return $"{parentPath}/{p.ProjectName}";
            }
        }
    }
}