using System.IO;
using System.Linq;

namespace ModernRonin.ProjectRenamer;

public class Filesystem : IFilesystem
{
    public string CurrentDirectory => Path.GetFullPath(Directory.GetCurrentDirectory());
    public bool DoesDirectoryExist(string directory) => Directory.Exists(directory);
    public void EnsureDirectoryExists(string directory) => Directory.CreateDirectory(directory);

    public string[] FindProjectFiles(string directory, bool doRecurse, string projectFileExtension) =>
        Directory
            .EnumerateFiles(directory, $"*{projectFileExtension}", SearchOption(doRecurse))
            .ToArray();

    public string[] FindSolutionFiles(string directory, bool doRecurse) =>
        Directory.EnumerateFiles(".", $"*{Constants.SolutionFileExtension}", SearchOption(doRecurse))
            .ToArray();

    static SearchOption SearchOption(bool doRecurse) =>
        doRecurse ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
}