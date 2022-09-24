using Autofac;
using ModernRonin.ProjectRenamer;

var container = wireUp();
var application = container.Resolve<Application>();
application.Run();

IContainer wireUp()
{
    var builder = new ContainerBuilder();
    builder.RegisterModule(new RenamerModule(args));
    return builder.Build();
}