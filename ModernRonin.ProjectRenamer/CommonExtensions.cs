using System.IO;
using System.Linq;
using System.Text;

namespace ModernRonin.ProjectRenamer
{
    public static class CommonExtensions
    {
        public static string AsText(this bool self) => self ? "yes" : "no";

        public static string EscapeForShell(this string self) => $"\"{self}\"";

        public static bool IsDirectorySeparator(this char self) =>
            self == Path.DirectorySeparatorChar || self == Path.AltDirectorySeparatorChar;

        public static string MoveRelativePath(this string self,
            string oldBaseDirectory,
            string newBaseDirectory)
        {
            var absolute = self.ToAbsolutePath(oldBaseDirectory);
            return absolute.ToRelativePath(newBaseDirectory);
        }

        public static string Repeat(this string self, int count)
        {
            var result = new StringBuilder(self.Length * count);
            for (var i = 0; i < count; ++i) result.Append(self);
            return result.ToString();
        }

        public static string ReplaceBackslashesWithSlashesOnLinux(this string self)
        {
            if (Path.DirectorySeparatorChar != Path.AltDirectorySeparatorChar)
                return self; // we are on Windows
            return self.Replace('\\', '/');
        }

        public static string ReplaceSlashesWithBackslashes(this string self) => self.Replace('/', '\\');

        public static string ToAbsolutePath(this string self, string baseDirectory)
        {
            if (self.First().IsDirectorySeparator()) self = self[1..];
            self = self.ReplaceBackslashesWithSlashesOnLinux();
            return Path.GetFullPath(self, baseDirectory);
        }

        public static string ToRelativePath(this string self, string baseDirectory) =>
            Path.GetRelativePath(baseDirectory, self.ReplaceBackslashesWithSlashesOnLinux());
    }
}