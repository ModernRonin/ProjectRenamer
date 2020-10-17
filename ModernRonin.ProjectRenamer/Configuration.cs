namespace ModernRonin.ProjectRenamer
{
    public class Configuration
    {
        public bool DoRunPaketInstall { get; set; } = true;
        public bool DoRunBuild { get; set; } = false;
        public bool DoCreateCommit { get; set; } = true;
        public bool DoReviewSettings { get; set; } = true;
        public string OldProjectName { get; set; }
        public string NewProjectName { get; set; }

        public string[] ProjectNames =>
            new[]
            {
                OldProjectName,
                NewProjectName
            };
    }
}