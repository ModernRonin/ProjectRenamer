using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernRonin.ProjectRenamer
{
    public class Dotnet : IDotnet
    {
        const string ToolDotnet = "dotnet";
        readonly IErrorHandler _errors;
        readonly IExecutor _executor;

        public Dotnet(IExecutor executor, IErrorHandler errors)
        {
            _executor = executor;
            _errors = errors;
        }

        public void AddReference(string project, string reference) =>
            ProjectReferenceCommand("add", project, reference);

        public void AddToSolution(string pathToProject, string solutionFolder) =>
            SolutionCommand($"add -s {solutionFolder.EscapeForShell()}", pathToProject);

        public void AddToSolution(string pathToProject) => SolutionCommand("add", pathToProject);

        public void BuildSolution(Action onNonZeroExitCode) => Run("build", onNonZeroExitCode);

        public IEnumerable<string> GetReferencedProjects(string project) =>
            Read($"list {project.EscapeForShell()} reference")
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .Skip(2);

        public void PaketInstall() => Run("paket install");
        public void RemoveFromSolution(string pathToProject) => SolutionCommand("remove", pathToProject);

        public void RemoveReference(string project, string reference) =>
            ProjectReferenceCommand("remove", project, reference);

        void ProjectReferenceCommand(string command, string project, string reference) =>
            Run($"{command} {project.EscapeForShell()} reference {reference.EscapeForShell()}");

        string Read(string arguments) =>
            _executor.ToolRead(ToolDotnet, arguments, () => _errors.Handle(ToolDotnet, arguments));

        void Run(string arguments) => _executor.Tool(ToolDotnet, arguments);

        void Run(string arguments, Action onNonZeroExitCode) =>
            _executor.Tool(ToolDotnet, arguments, onNonZeroExitCode);

        void SolutionCommand(string command, string projectPath) =>
            Run($"sln {command} {projectPath.EscapeForShell()}");
    }
}