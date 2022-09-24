using System;
using Autofac;

namespace ModernRonin.ProjectRenamer;

class Program
{
    static void Main(string[] args)
    {
        var container = WireUp();
        var setup = container.Resolve<IConfigurationSetup>();
        var (configuration, solutionPath) = setup.Get(args);
        if (configuration == default) return;

        var applicationFactory = container.Resolve<Func<Verb, string, Application>>();
        var application = applicationFactory(configuration, solutionPath);
        application.Run();
    }

    static IContainer WireUp()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<RenamerModule>();
        return builder.Build();
    }
}