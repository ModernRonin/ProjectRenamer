using Autofac;

namespace ModernRonin.ProjectRenamer
{
    public class RenamerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConsoleWrapper>().AsImplementedInterfaces();
            builder.RegisterType<Runtime>().AsImplementedInterfaces();
            builder.RegisterType<Executor>().AsImplementedInterfaces();
            builder.RegisterType<ErrorHandler>().AsImplementedInterfaces();
            builder.RegisterType<ConfigurationSetup>().AsImplementedInterfaces();
            builder.RegisterType<Git>().AsImplementedInterfaces();
            builder.RegisterType<Dotnet>().AsImplementedInterfaces();
            builder.RegisterType<Application>().AsSelf();
        }
    }
}