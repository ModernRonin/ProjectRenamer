namespace ModernRonin.ProjectRenamer
{
    public static class BooleanExtensions
    {
        public static string AsText(this bool self) => self ? "yes" : "no";
    }
}