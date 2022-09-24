namespace ModernRonin.ProjectRenamer;

public interface IProjectFinder
{
    ProjectInfo FindProject(string solutionPath, string projectName);
}