using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Construction;

namespace ModernRonin.ProjectRenamer
{
    class Program
    {
        const string ProjectFileExtension = ".csproj";
        static readonly Encoding _projectFileEncoding = Encoding.UTF8;

        static void Main(string[] args)
        {
            if (args.Length != 2 || args.Any(a =>
                a.Contains('\\') || a.Contains('/') ||
                a.Contains(ProjectFileExtension, StringComparison.InvariantCultureIgnoreCase)))
            {
                Error(
                    "Usage: <oldProjectName> <newProjectName> where project names contain neither path nor extension. Example: not ./utilities/My.Wonderful.Utilities.csproj, but My.Wonderful.Utilities");
            }

            var solutionFiles =
                Directory.EnumerateFiles(".", "*.sln", SearchOption.TopDirectoryOnly).ToArray();
            if (1 != solutionFiles.Length)
                Error("Needs to be run from a directory with exactly one *.sln file in it.");

            EnsureGitIsClean();

            var solutionPath = Path.GetFullPath(solutionFiles.First());
            Run(solutionPath, args.First(), args.Last());
        }

        static void EnsureGitIsClean()
        {
            run("update-index -q --refresh");
            run("diff-index --quiet --cached HEAD --");
            run("diff-files --quiet");
            run("ls-files --exclude-standard --others");

            void run(string arguments) =>
                RunTool("git", arguments, () => Error("git does not seem to be clean, check git status"));
        }

        static void Error(string msg, bool doResetGit = false)
        {
            Console.Error.WriteLine(msg);
            if (doResetGit)
            {
                Console.Error.WriteLine("...running git reset to undo any changes...");
                RollbackGit();
            }

            Abort();
        }

        static void Abort() => Environment.Exit(-1);

        static void RollbackGit() => RunTool("git", "reset --hard HEAD", () => { });

        static void Run(string solutionPath, string oldProjectName, string newProjectName)
        {
            var (wasFound, oldProjectPath, solutionFolderPath) = findProject();
            if (!wasFound) Error($"{oldProjectName} cannot be found in the solution");

            var oldDir = Path.GetDirectoryName(oldProjectPath);
            var newDir = Path.Combine(Path.GetDirectoryName(oldDir), newProjectName);
            var newProjectPath = Path.Combine(newDir, $"{newProjectName}{ProjectFileExtension}");

            removeFromSolution();
            gitMove();
            replaceReferences();
            addToSolution();
            updatePaket();
            stageAllChanges();

            if (doesUserAgree("Finished - do you want to run a dotnet build to see whether all went well?"))
            {
                RunTool("dotnet", "build", () =>
                {
                    if (doesUserAgree(
                        "dotnet build returned an error or warning - do you want to rollback all changes?"))
                    {
                        RollbackGit();
                        Abort();
                    }
                });
            }

            if (doesUserAgree("Do you want me to create a commit for you?"))
            {
                var arguments = $"commit -m \"Renamed {oldProjectName} to {newProjectName}\"";
                RunTool("git", arguments, () => { Console.Error.WriteLine($"'git {arguments}' failed"); });
            }

            bool doesUserAgree(string question)
            {
                Console.WriteLine($"{question} [Enter=Yes, any other key=No]");
                var key = Console.ReadKey();
                return key.Key == ConsoleKey.Enter;
            }

            void stageAllChanges() => RunGit("add .");

            void updatePaket()
            {
                if (Directory.Exists(".paket") &&
                    doesUserAgree("This solution uses paket - do you want to run paket install?"))
                    runDotNet("paket install");
            }

            void replaceReferences()
            {
                var projectFiles = Directory
                    .EnumerateFiles(".", $"*{ProjectFileExtension}", SearchOption.AllDirectories)
                    .ToList();
                var (oldReference, newReference) =
                    (searchPattern(oldProjectName), searchPattern(newProjectName));
                projectFiles.ForEach(replaceIn);

                void replaceIn(string projectFile)
                {
                    var contents = File.ReadAllText(projectFile, _projectFileEncoding);
                    contents = contents.Replace(oldReference, newReference);
                    File.WriteAllText(projectFile, contents, _projectFileEncoding);
                }

                string searchPattern(string name) => Path.Combine(name, name) + ProjectFileExtension;
            }

            void gitMove()
            {
                RunGit($"mv {oldDir} {newDir}");
                var oldPath = Path.Combine(newDir, Path.GetFileName(oldProjectPath));
                RunGit($"mv {oldPath} {newProjectPath}");
            }

            void addToSolution() => runDotNet($"sln add -s {solutionFolderPath} {newProjectPath}");
            void removeFromSolution() => runDotNet($"sln remove {oldProjectPath}");

            void runDotNet(string arguments) => RunTool("dotnet", arguments);

            (bool wasFound, string projectPath, string solutionFolder) findProject()
            {
                var solution = SolutionFile.Parse(solutionPath);
                var project = solution.ProjectsInOrder.FirstOrDefault(p =>
                    p.ProjectName.EndsWith(oldProjectName, StringComparison.InvariantCultureIgnoreCase));
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
        }

        static void RunGit(string arguments) => RunTool("git", arguments);

        static void RunTool(string tool, string arguments)
        {
            RunTool(tool, arguments, () => Error($"call '{tool} {arguments}' failed - aborting", true));
        }

        static void RunTool(string tool, string arguments, Action onNonZeroExitCode)
        {
            var psi = new ProcessStartInfo
            {
                FileName = tool,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = false
            };
            var process = Process.Start(psi);
            process.WaitForExit();
            if (process.ExitCode != 0) onNonZeroExitCode();
        }
    }
}