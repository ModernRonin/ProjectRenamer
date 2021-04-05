using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using MoreLinq.Extensions;

namespace ModernRonin.ProjectRenamer
{
    public class Application
    {
        readonly Configuration _configuration;
        readonly IDotnet _dotnet;
        readonly IErrorHandler _errors;
        readonly IFilesystem _filesystem;
        readonly IGit _git;
        readonly IInput _input;
        readonly ILogger _logger;
        readonly IRuntime _runtime;
        readonly string _solutionPath;

        public Application(Configuration configuration,
            string solutionPath,
            IRuntime runtime,
            ILogger logger,
            IInput input,
            IGit git,
            IErrorHandler errors,
            IDotnet dotnet,
            IFilesystem filesystem)
        {
            _configuration = configuration;
            _solutionPath = solutionPath;
            _runtime = runtime;
            _logger = logger;
            _input = input;
            _git = git;
            _errors = errors;
            _dotnet = dotnet;
            _filesystem = filesystem;
        }

        public void Run()
        {
            _git.EnsureIsClean();

            var (wasFound, oldProjectPath, solutionFolderPath) = findProject();
            if (!wasFound) _errors.Handle($"{_configuration.OldProjectName} cannot be found in the solution");

            var oldDir = Path.GetDirectoryName(oldProjectPath);
            var newBase = _configuration.NewProjectName.Any(CommonExtensions.IsDirectorySeparator)
                ? CurrentDirectoryAbsolute
                : Path.GetDirectoryName(oldDir);
            var newDir = _configuration.NewProjectName.ToAbsolutePath(newBase);
            var newFileName = Path.GetFileName(_configuration.NewProjectName);
            var newProjectPath = Path.Combine(newDir, $"{newFileName}{Constants.ProjectFileExtension}");
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
                    $"at:                        {newProjectPath})",
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
                if (!_input.AskUser(string.Join(Environment.NewLine, lines))) _runtime.Abort();
            }

            var (dependents, dependencies) = analyzeReferences();
            removeFromSolution();
            removeOldReferences();
            gitMove();
            updatePaketReference();
            addNewReferences();
            addToSolution();
            updatePaket();
            _git.StageAllChanges();
            build();
            commit();

            void addNewReferences()
            {
                dependents.ForEach(p => _dotnet.AddReference(p, newProjectPath));
                dependencies.ForEach(d => _dotnet.AddReference(newProjectPath, d));
            }

            void removeOldReferences()
            {
                dependents.ForEach(p => _dotnet.RemoveReference(p, oldProjectPath));
                dependencies.ForEach(d => _dotnet.RemoveReference(oldProjectPath, d));
            }

            (string[] dependents, string[] dependencies) analyzeReferences()
            {
                _logger.Info(
                    "Analyzing references in your projects - depending on the number of projects this can take a bit...");

                return (
                    allProjects().Where(doesNotEqualOldProjectPath).Where(hasReferenceToOldProject).ToArray(),
                    getReferencedProjects(oldProjectPath).ToArray());

                bool hasReferenceToOldProject(string p) =>
                    getReferencedProjects(p).Any(doesEqualOldProjectPath);
            }

            bool doesNotEqualOldProjectPath(string what) => !doesEqualOldProjectPath(what);
            bool doesEqualOldProjectPath(string what) => arePathsEqual(what, oldProjectPath);

            IEnumerable<string> getReferencedProjects(string project)
            {
                var relativeReferences = _dotnet.GetReferencedProjects(project);
                var baseDirectory = Path.GetFullPath(Path.GetDirectoryName(project));
                return relativeReferences.Select(r => r.ToAbsolutePath(baseDirectory));
            }

            bool arePathsEqual(string lhs, string rhs) => Path.GetFullPath(lhs) == Path.GetFullPath(rhs);

            void commit()
            {
                if (!_configuration.DontCreateCommit)
                {
                    var wasMove = _configuration.NewProjectName.Any(CommonExtensions.IsDirectorySeparator);
                    var msg = wasMove
                        ? $"Moved {oldProjectPath.ToRelativePath(CurrentDirectoryAbsolute)} to {newProjectPath.ToRelativePath(CurrentDirectoryAbsolute)}"
                        : $"Renamed {_configuration.OldProjectName} to {_configuration.NewProjectName}";
                    _git.Commit(msg);
                }
            }

            void build()
            {
                if (_configuration.DoRunBuild)
                {
                    _dotnet.BuildSolution(() =>
                    {
                        if (_input.AskUser(
                            "dotnet build returned an error or warning - do you want to rollback all changes?")
                        )
                        {
                            _git.RollbackAllChanges();
                            _runtime.Abort();
                        }
                    });
                }
            }

            void updatePaket()
            {
                if (isPaketUsed && !_configuration.DontRunPaketInstall) _dotnet.PaketInstall();
            }

            void updatePaketReference()
            {
                if (!isPaketUsed) return;
                const string restoreTargets = @"\.paket\Paket.Restore.targets";
                var nesting = Path.GetFullPath(newProjectPath).Count(CommonExtensions.IsDirectorySeparator) -
                              CurrentDirectoryAbsolute.Count(CommonExtensions.IsDirectorySeparator) - 1;
                var paketPath = @"..\".Repeat(nesting)[..^1] + restoreTargets;
                var lines = File.ReadAllLines(newProjectPath).Select(fixup);
                File.WriteAllLines(newProjectPath, lines);

                string fixup(string line) =>
                    isPaketReference(line) ? $"<Import Project=\"{paketPath}\" />" : line;

                bool isPaketReference(string line)
                {
                    var trimmed = line.Trim();
                    if (!trimmed.StartsWith("<Import Project")) return false;
                    if (!trimmed.Contains(restoreTargets)) return false;
                    return true;
                }
            }

            void gitMove()
            {
                _filesystem.EnsureDirectoryExists(Path.GetDirectoryName(newDir));
                _git.Move(oldDir, newDir);
                var oldPath = Path.GetFileName(oldProjectPath).ToAbsolutePath(newDir);
                if (oldPath != newProjectPath) _git.Move(oldPath, newProjectPath);
            }

            void addToSolution()
            {
                if (string.IsNullOrWhiteSpace(solutionFolderPath)) _dotnet.AddToSolution(newProjectPath);
                else _dotnet.AddToSolution(newProjectPath, solutionFolderPath);
            }

            void removeFromSolution() => _dotnet.RemoveFromSolution(oldProjectPath);

            (bool wasFound, string projectPath, string solutionFolder) findProject()
            {
                var solution = SolutionFile.Parse(_solutionPath);
                var project = solution.ProjectsInOrder.FirstOrDefault(p =>
                    p.ProjectName.EndsWith(_configuration.OldProjectName,
                        StringComparison.InvariantCultureIgnoreCase));
                return project switch
                {
                    null => (false, null, null),
                    _ when project.ParentProjectGuid == null => (true, project.AbsolutePath, null),
                    _ => (true, project.AbsolutePath,
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

            string[] allProjects()
            {
                var all = filesIn(".");
                var excluded = string.IsNullOrEmpty(_configuration.ExcludedDirectory)
                    ? Enumerable.Empty<string>()
                    : filesIn($@".\{_configuration.ExcludedDirectory}");

                return all.Except(excluded).ToArray();

                string[] filesIn(string directory) => _filesystem.FindProjectFiles(directory, true);
            }
        }

        string CurrentDirectoryAbsolute => Path.GetFullPath(_filesystem.CurrentDirectory);
    }
}