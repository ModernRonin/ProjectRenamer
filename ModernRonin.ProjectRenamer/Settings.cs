namespace ModernRonin.ProjectRenamer;

public sealed class Settings
{
    public Project Destination { get; set; }
    public bool DoBuild { get; set; }
    public bool DoCreateCommit { get; set; }
    public bool DoPaketInstall { get; set; }
    public string ExcludedDirectory { get; set; }
    public bool IsPaketUsed { get; set; }
    public Project Source { get; set; }
}