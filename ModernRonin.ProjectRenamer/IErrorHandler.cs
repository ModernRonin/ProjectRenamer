using JetBrains.Annotations;

namespace ModernRonin.ProjectRenamer;

public interface IErrorHandler
{
    /// <summary>
    ///     Will also exit the process.
    /// </summary>
    [ContractAnnotation("=> halt")]
    void Handle(string msg);

    /// <summary>
    ///     Will also exit the process.
    /// </summary>
    [ContractAnnotation("=> halt")]
    void Handle(string tool, string arguments);

    [ContractAnnotation("=> halt")]
    void HandleAndReset(string msg);
}