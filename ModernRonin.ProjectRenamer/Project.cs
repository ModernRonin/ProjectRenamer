using System.IO;
using System.Linq;

namespace ModernRonin.ProjectRenamer;

/// <summary>
///     <paramref name="FullPath" /> must be absolute.
/// </summary>
public sealed record Project(string FullPath, string SolutionFolder, string Extension)
{
    public Project Rename(string newName, string currentDirectory)
    {
        var newBaseDir = newName.Any(CommonExtensions.IsDirectorySeparator)
            ? currentDirectory
            : Path.GetDirectoryName(Directory);
        var newDir = newName.ToAbsolutePath(newBaseDir);
        var newFileName = Path.GetFileName(newName);
        var newProjectPath = Path.Combine(newDir, $"{newFileName}{Extension}");
        return this with { FullPath = newProjectPath };
    }

    public string Directory => Path.GetDirectoryName(FullPath);
    public string Name => Path.GetFileNameWithoutExtension(FullPath);
}