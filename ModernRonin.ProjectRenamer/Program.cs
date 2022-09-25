using Autofac;
using ModernRonin.ProjectRenamer;

var container = wireUp();
var (application, errorHandler) = (container.Resolve<Application>(), container.Resolve<IErrorHandler>());

try
{
    application.Run();
}
catch (AbortException x)
{
    errorHandler.HandleException(x);
}

IContainer wireUp()
{
    var builder = new ContainerBuilder();
    builder.RegisterModule(new RenamerModule(args));
    return builder.Build();
}