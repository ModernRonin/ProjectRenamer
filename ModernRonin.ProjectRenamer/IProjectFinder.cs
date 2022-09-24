namespace ModernRonin.ProjectRenamer;

public interface IProjectFinder
{
    Project FindProject(string solutionPath, string projectName);
}