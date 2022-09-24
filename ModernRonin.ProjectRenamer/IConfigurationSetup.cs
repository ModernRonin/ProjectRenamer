namespace ModernRonin.ProjectRenamer;

public interface IConfigurationSetup
{
    (Verb configuration, string solutionPath) Get(string[] commandLineArguments);
}