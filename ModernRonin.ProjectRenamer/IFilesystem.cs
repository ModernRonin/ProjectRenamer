namespace ModernRonin.ProjectRenamer
{
    public interface IFilesystem
    {
        string CurrentDirectory { get; }
        bool DoesDirectoryExist(string directory);
        void EnsureDirectoryExists(string directory);
        string[] FindProjectFiles(string directory, bool doRecurse, string projectFileExtension);
        string[] FindSolutionFiles(string directory, bool doRecurse);
    }
}