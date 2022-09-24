using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq.Extensions;

namespace ModernRonin.ProjectRenamer;

public class Application
{
    readonly Configuration _configuration;
    readonly IDotnet _dotnet;
    readonly IFilesystem _filesystem;
    readonly IGit _git;
    readonly IInput _input;
    readonly ILogger _logger;
    readonly IRuntime _runtime;
    readonly ISettingsProvider _settingsProvider;
    readonly string _solutionPath;

    public Application(Configuration configuration,
        string solutionPath,
        IRuntime runtime,
        ILogger logger,
        IInput input,
        IGit git,
        IDotnet dotnet,
        IFilesystem filesystem,
        ISettingsProvider settingsProvider)
    {
        _configuration = configuration;
        _solutionPath = solutionPath;
        _runtime = runtime;
        _logger = logger;
        _input = input;
        _git = git;
        _dotnet = dotnet;
        _filesystem = filesystem;
        _settingsProvider = settingsProvider;
    }

    public void Run()
    {
        _git.EnsureIsClean();
        var settings = _settingsProvider.GetSettings(_solutionPath);
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
            dependents.ForEach(p => _dotnet.AddReference(p, settings.Destination.FullPath));
            dependencies.ForEach(d => _dotnet.AddReference(settings.Destination.FullPath, d));
        }

        void removeOldReferences()
        {
            dependents.ForEach(p => _dotnet.RemoveReference(p, settings.Source.FullPath));
            dependencies.ForEach(d => _dotnet.RemoveReference(settings.Source.FullPath, d));
        }

        (string[] dependents, string[] dependencies) analyzeReferences()
        {
            _logger.Info(
                "Analyzing references in your projects - depending on the number of projects this can take a bit...");

            return (
                allProjects().Where(doesNotEqualOldProjectPath).Where(hasReferenceToOldProject).ToArray(),
                getReferencedProjects(settings.Source.FullPath).ToArray());

            bool hasReferenceToOldProject(string p) => getReferencedProjects(p).Any(doesEqualOldProjectPath);
        }

        bool doesNotEqualOldProjectPath(string what) => !doesEqualOldProjectPath(what);
        bool doesEqualOldProjectPath(string what) => arePathsEqual(what, settings.Source.FullPath);

        IEnumerable<string> getReferencedProjects(string project)
        {
            var relativeReferences = _dotnet.GetReferencedProjects(project)
                .Select(p => p = Path.Combine(p.Split('\\')));
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
                    ? $"Moved {settings.Source.FullPath.ToRelativePath(_filesystem.CurrentDirectory)} to {settings.Destination.FullPath.ToRelativePath(_filesystem.CurrentDirectory)}"
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
            if (settings.DoPaketInstall) _dotnet.PaketInstall();
        }

        void updatePaketReference()
        {
            if (!settings.IsPaketUsed) return;
            const string restoreTargets = @"\.paket\Paket.Restore.targets";
            var nesting = Path.GetFullPath(settings.Destination.FullPath)
                              .Count(CommonExtensions.IsDirectorySeparator)                         -
                          _filesystem.CurrentDirectory.Count(CommonExtensions.IsDirectorySeparator) - 1;
            var paketPath = @"..\".Repeat(nesting)[..^1] + restoreTargets;
            var lines = File.ReadAllLines(settings.Destination.FullPath).Select(fixup);
            File.WriteAllLines(settings.Destination.FullPath, lines);

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
            _filesystem.EnsureDirectoryExists(Path.GetDirectoryName(settings.Destination.Directory));
            _git.Move(settings.Source.Directory, settings.Destination.Directory);
            var oldPath = Path.GetFileName(settings.Source.FullPath)
                .ToAbsolutePath(settings.Destination.Directory);
            if (oldPath != settings.Destination.FullPath) _git.Move(oldPath, settings.Destination.FullPath);
        }

        void addToSolution()
        {
            if (string.IsNullOrWhiteSpace(settings.Destination.SolutionFolder))
                _dotnet.AddToSolution(settings.Destination.FullPath);
            else _dotnet.AddToSolution(settings.Destination.FullPath, settings.Destination.SolutionFolder);
        }

        void removeFromSolution() => _dotnet.RemoveFromSolution(settings.Source.FullPath);

        string[] allProjects()
        {
            var all = filesIn(".");
            var excluded = string.IsNullOrEmpty(_configuration.ExcludedDirectory)
                ? Enumerable.Empty<string>()
                : filesIn($@".\{_configuration.ExcludedDirectory}");

            return all.Except(excluded).ToArray();

            string[] filesIn(string directory) =>
                _filesystem.FindProjectFiles(directory, true, _configuration.ProjectFileExtension);
        }
    }
}