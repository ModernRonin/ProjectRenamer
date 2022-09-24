namespace ModernRonin.ProjectRenamer;

public record Settings(string OldProjectPath,
    string SolutionFolderPath,
    string OldDir,
    string NewDir,
    string NewProjectPath,
    bool IsPaketUsed);