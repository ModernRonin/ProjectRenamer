﻿namespace ModernRonin.ProjectRenamer
{
    public class Configuration
    {
        public bool DontCreateCommit { get; set; }
        public bool DontReviewSettings { get; set; }
        public bool DontRunPaketInstall { get; set; }
        public bool DoRunBuild { get; set; }
        public string NewProjectName { get; set; }
        public string OldProjectName { get; set; }
    }
}