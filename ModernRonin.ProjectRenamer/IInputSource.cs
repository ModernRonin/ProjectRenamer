namespace ModernRonin.ProjectRenamer;

public interface IInputSource
{
    UserInput Get(string[] commandLineArguments);
}