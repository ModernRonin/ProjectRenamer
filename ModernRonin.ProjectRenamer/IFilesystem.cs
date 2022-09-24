namespace ModernRonin.ProjectRenamer;

public interface IFilesystem
{
    /// <summary>
    ///     Returns the <b>absolute path</b> of the current directory.
    /// </summary>
    string CurrentDirectory { get; }

    bool DoesDirectoryExist(string directory);
    void EnsureDirectoryExists(string directory);
    string[] FindProjectFiles(string directory, bool doRecurse, string projectFileExtension);
    string[] FindSolutionFiles(string directory, bool doRecurse);
}