using System.IO;
using System.Text;

namespace ModernRonin.ProjectRenamer
{
    public static class CommonExtensions
    {
        public static string AsText(this bool self) => self ? "yes" : "no";

        public static bool IsDirectorySeparator(this char self) =>
            self == Path.DirectorySeparatorChar || self == Path.AltDirectorySeparatorChar;

        public static string Repeat(this string self, int count)
        {
            var result = new StringBuilder(self.Length * count);
            for (var i = 0; i < count; ++i) result.Append(self);
            return result.ToString();
        }

        public static string ToAbsolutePath(this string self, string baseDirectory) =>
            Path.GetFullPath(self, baseDirectory);

        public static string ToRelativePath(this string self, string baseDirectory) =>
            Path.GetRelativePath(baseDirectory, self);
    }
}