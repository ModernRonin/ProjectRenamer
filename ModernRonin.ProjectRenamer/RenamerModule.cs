using Autofac;

namespace ModernRonin.ProjectRenamer;

public class RenamerModule : Module
{
    readonly string[] _commandLineArguments;

    public RenamerModule(string[] commandLineArguments) => _commandLineArguments = commandLineArguments;

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<InputSource>()
            .AsImplementedInterfaces()
            .WithParameter(new TypedParameter(typeof(string[]), _commandLineArguments));
        builder.RegisterType<Runtime>().AsImplementedInterfaces();
        builder.RegisterType<ErrorHandler>().AsImplementedInterfaces();
        builder.RegisterType<SettingsProvider>().AsImplementedInterfaces();
        builder.RegisterType<ProjectFinder>().AsImplementedInterfaces();
        builder.RegisterType<Git>().AsImplementedInterfaces();
        builder.RegisterType<Dotnet>().AsImplementedInterfaces();
        builder.RegisterType<Filesystem>().AsImplementedInterfaces();
        builder.RegisterType<ToolRunner>().AsImplementedInterfaces();
        builder.RegisterType<Application>().AsSelf();
        builder.RegisterInstance(CommandLineArguments.CreateParser());
    }
}