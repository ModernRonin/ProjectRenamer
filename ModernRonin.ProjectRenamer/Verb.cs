namespace ModernRonin.ProjectRenamer;

public class Verb
{
    public Settings ToSettings() =>
        new(DoRunBuild, !DontCreateCommit, !DontRunPaketInstall, default, ExcludedDirectory, default,
            default);

    public bool DontCreateCommit { get; set; }
    public bool DontReviewSettings { get; set; }
    public bool DontRunPaketInstall { get; set; }
    public bool DoRunBuild { get; set; }
    public string ExcludedDirectory { get; set; } = string.Empty;
    public string NewProjectName { get; set; }
    public string OldProjectName { get; set; }
    public string ProjectFileExtension { get; set; } = Constants.CSharpProjectFileExtension;
}