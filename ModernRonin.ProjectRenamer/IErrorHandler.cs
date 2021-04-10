namespace ModernRonin.ProjectRenamer
{
    public interface IErrorHandler
    {
        void Handle(string msg);
        void Handle(string tool, string arguments);
        void HandleAndReset(string msg);
    }
}