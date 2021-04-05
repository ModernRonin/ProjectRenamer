namespace ModernRonin.ProjectRenamer
{
    public interface IConfigurationSetup
    {
        (Configuration configuration, string solutionPath) Get(string[] commandLineArguments);
    }
}