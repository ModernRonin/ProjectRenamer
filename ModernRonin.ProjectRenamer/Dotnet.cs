using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernRonin.ProjectRenamer;

public class Dotnet : IDotnet
{
    readonly IToolRunner _runner;

    public Dotnet(Func<string, IToolRunner> toolRunnerFactory) => _runner = toolRunnerFactory("dotnet");

    public void AddReference(string project, string reference) =>
        ProjectReferenceCommand("add", project, reference);

    public void AddToSolution(string pathToProject, string solutionFolder) =>
        SolutionCommand($"add -s {solutionFolder.ReplaceSlashesWithBackslashes().EscapeForShell()}",
            pathToProject);

    public void AddToSolution(string pathToProject) => SolutionCommand("add", pathToProject);

    public void BuildSolution(Action onNonZeroExitCode) => _runner.Run("build", onNonZeroExitCode);

    public IEnumerable<string> GetReferencedProjects(string project) =>
        _runner.RunAndGetOutput($"list {project.EscapeForShell()} reference")
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Skip(2);

    public void PaketInstall() => _runner.Run("paket install");
    public void RemoveFromSolution(string pathToProject) => SolutionCommand("remove", pathToProject);

    public void RemoveReference(string project, string reference) =>
        ProjectReferenceCommand("remove", project, reference);

    void ProjectReferenceCommand(string command, string project, string reference) =>
        _runner.Run($"{command} {project.EscapeForShell()} reference {reference.EscapeForShell()}");

    void SolutionCommand(string command, string projectPath) =>
        _runner.Run($"sln {command} {projectPath.EscapeForShell()}");
}