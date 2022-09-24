namespace ModernRonin.ProjectRenamer;

public sealed class Settings
{
    public Project Destination { get; init; }
    public bool DoBuild { get; init; }
    public bool DoCreateCommit { get; init; }
    public bool DoPaketInstall { get; init; }
    public string ExcludedDirectory { get; init; }
    public bool IsPaketUsed { get; init; }
    public Project Source { get; init; }
}