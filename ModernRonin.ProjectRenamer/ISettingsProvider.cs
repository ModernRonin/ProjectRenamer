namespace ModernRonin.ProjectRenamer;

public interface ISettingsProvider
{
    Settings GetSettings(string solutionPath);
}