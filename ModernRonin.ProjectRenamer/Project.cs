using System.IO;

namespace ModernRonin.ProjectRenamer;

/// <summary>
///     <paramref name="FullPath" /> must be absolute.
/// </summary>
public sealed record Project(string FullPath, string SolutionFolder, string Extension)
{
    public Project Rename(string newName, string currentDirectory)
    {
        var cd = new FileSystemPath(currentDirectory);
        var nn = new FileSystemPath(newName);
        var d = new FileSystemPath(Directory);

        var newBase = nn.ContainsDirectory ? cd : d.Parent;
        var newFn = nn[^1].WithExtension(Extension);
        var npp = newBase.Append(nn).Append(newFn);
        return this with { FullPath = npp.ToString() };

        //var newBaseDir = newName.Any(CommonExtensions.IsDirectorySeparator)
        //    ? currentDirectory
        //    : Path.GetDirectoryName(Directory);
        //var newDir = newName.ToAbsolutePath(newBaseDir);
        //var newFileName = Path.GetFileName(newName);
        //var newProjectPath = Path.Combine(newDir, $"{newFileName}{Extension}");
        //return this with { FullPath = newProjectPath };
    }

    public string Directory => Path.GetDirectoryName(FullPath);
    public string Name => Path.GetFileNameWithoutExtension(FullPath);
}