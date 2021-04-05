using System;
using Autofac;

namespace ModernRonin.ProjectRenamer
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = WireUp();
            var setup = container.Resolve<IConfigurationSetup>();
            var (configuration, solutionPath) = setup.Get(args);
            if (configuration == default) return;

            container.Resolve<Func<Configuration, string, Application>>()(configuration, solutionPath)
                .Run();
        }

        static IContainer WireUp()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ConsoleWrapper>().AsImplementedInterfaces();
            builder.RegisterType<Runtime>().AsImplementedInterfaces();
            builder.RegisterType<Executor>().AsImplementedInterfaces();
            builder.RegisterType<ConfigurationSetup>().AsImplementedInterfaces();
            builder.RegisterType<Application>().AsSelf();

            return builder.Build();
        }
    }
}