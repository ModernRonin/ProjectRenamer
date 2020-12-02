using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Construction;
using MoreLinq.Extensions;
using static ModernRonin.ProjectRenamer.Executor;
using static ModernRonin.ProjectRenamer.Runtime;

namespace ModernRonin.ProjectRenamer
{
    public class Application
    {
        readonly Configuration _configuration;
        readonly Encoding _projectFileEncoding = Encoding.UTF8;
        readonly string _solutionPath;

        public Application(Configuration configuration, string solutionPath)
        {
            _configuration = configuration;
            _solutionPath = solutionPath;
        }

        public void Run()
        {
            EnsureGitIsClean();

            var (wasFound, oldProjectPath, solutionFolderPath) = findProject();
            if (!wasFound) Error($"{_configuration.OldProjectName} cannot be found in the solution");

            var oldDir = Path.GetDirectoryName(oldProjectPath);
            var newBase = _configuration.NewProjectName.Any(CommonExtensions.IsDirectorySeparator)
                ? CurrentDirectoryAbsolute
                : Path.GetDirectoryName(oldDir);
            var newDir = Path.Combine(newBase, _configuration.NewProjectName);
            var newFileName = Path.GetFileName(_configuration.NewProjectName);
            var newProjectPath =
                Path.GetFullPath(Path.Combine(newDir, $"{newFileName}{Constants.ProjectFileExtension}"));
            var isPaketUsed = Directory.Exists(".paket");

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
                    $"Paket in use:              {isPaketUsed.AsText()}",
                    $"Run paket install:         {(!_configuration.DontRunPaketInstall).AsText()}",
                    $"Run build after rename:    {_configuration.DoRunBuild.AsText()}",
                    $"Create automatic commit:   {(!_configuration.DontCreateCommit).AsText()}",
                    "-----------------------------------------------",
                    "Do you want to continue with the rename operation?"
                };
                if (!DoesUserAgree(string.Join(Environment.NewLine, lines))) Abort();
            }

            var (dependents, dependencies) = analyzeReferences();
            removeFromSolution();
            removeOldReferences();
            gitMove();
            updatePaketReference();
            addNewReferences();
            addToSolution();
            updatePaket();
            stageAllChanges();
            build();
            commit();

            void addNewReferences()
            {
                dependents.ForEach(p => addReference(p, newProjectPath));
                dependencies.ForEach(d => addReference(newProjectPath, d));

                void addReference(string project, string reference) =>
                    DotNet($"add {project} reference {reference}");
            }

            void removeOldReferences()
            {
                dependents.ForEach(p => removeReference(p, oldProjectPath));
                dependencies.ForEach(d => removeReference(oldProjectPath, d));

                void removeReference(string project, string reference) =>
                    DotNet($"remove {project} reference {reference}");
            }

            (string[] dependents, string[] dependencies) analyzeReferences()
            {
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
                var baseDirectory = Path.GetFullPath(Path.GetDirectoryName(project));
                var relativeReferences = DotNetRead($"list {project} reference")
                    .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                    .Skip(2);
                return relativeReferences.Select(r => r.ToAbsolutePath(baseDirectory));
            }

            bool arePathsEqual(string lhs, string rhs) => Path.GetFullPath(lhs) == Path.GetFullPath(rhs);

            void commit()
            {
                if (!_configuration.DontCreateCommit)
                {
                    var arguments =
                        $"commit -m \"Renamed {_configuration.OldProjectName} to {_configuration.NewProjectName}\"";
                    Git(arguments,
                        () => { Console.Error.WriteLine($"'git {arguments}' failed"); });
                }
            }

            void build()
            {
                if (_configuration.DoRunBuild)
                {
                    DotNet("build", () =>
                    {
                        if (DoesUserAgree(
                            "dotnet build returned an error or warning - do you want to rollback all changes?")
                        )
                        {
                            RollbackGit();
                            Abort();
                        }
                    });
                }
            }

            void stageAllChanges() => Git("add .");

            void updatePaket()
            {
                if (isPaketUsed && !_configuration.DontRunPaketInstall) DotNet("paket install");
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
                Git($"mv {oldDir} {newDir}");
                var oldPath = Path.GetFullPath(Path.Combine(newDir, Path.GetFileName(oldProjectPath)));
                if (oldPath != newProjectPath) Git($"mv {oldPath} {newProjectPath}");
            }

            void addToSolution()
            {
                var solutionFolderArgument = string.IsNullOrWhiteSpace(solutionFolderPath)
                    ? string.Empty
                    : $"-s {solutionFolderPath}";
                DotNet($"sln add {solutionFolderArgument} {newProjectPath}");
            }

            void removeFromSolution() => DotNet($"sln remove {oldProjectPath}");

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

            string[] allProjects() =>
                Directory
                    .EnumerateFiles(".", $"*{Constants.ProjectFileExtension}", SearchOption.AllDirectories)
                    .ToArray();
        }

        bool DoesUserAgree(string question)
        {
            Console.WriteLine($"{question} [Enter=Yes, any other key=No]");
            var key = Console.ReadKey();
            return key.Key == ConsoleKey.Enter;
        }

        void EnsureGitIsClean()
        {
            run("update-index -q --refresh");
            run("diff-index --quiet --cached HEAD --");
            run("diff-files --quiet");
            run("ls-files --exclude-standard --others");

            void run(string arguments) =>
                Git(arguments,
                    () => Error("git does not seem to be clean, check git status"));
        }

        static string CurrentDirectoryAbsolute => Path.GetFullPath(Directory.GetCurrentDirectory());
    }
}