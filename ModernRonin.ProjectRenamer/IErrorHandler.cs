namespace ModernRonin.ProjectRenamer;

public interface IErrorHandler
{
    void HandleException(AbortException exception);
}