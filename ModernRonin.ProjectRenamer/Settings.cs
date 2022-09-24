namespace ModernRonin.ProjectRenamer;

public sealed record Settings(bool DoBuild,
    bool DoCreateCommit,
    bool DoPaketInstall,
    bool IsPaketUsed,
    string ExcludedDirectory,
    Project Source,
    Project Destination);