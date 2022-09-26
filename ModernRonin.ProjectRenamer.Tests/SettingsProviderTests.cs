using AutofacContrib.NSubstitute;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace ModernRonin.ProjectRenamer.Tests;

[TestFixture]
public class SettingsProviderTests
{
    [SetUp]
    public void Setup()
    {
        _dependencies = new AutoSubstitute();
        _underTest = _dependencies.Resolve<SettingsProvider>();
    }

    AutoSubstitute _dependencies;
    SettingsProvider _underTest;
    IFilesystem Filesystem => _dependencies.Resolve<IFilesystem>();
    IGit Git => _dependencies.Resolve<IGit>();
    IInputSource InputSource => _dependencies.Resolve<IInputSource>();
    IProjectFinder ProjectFinder => _dependencies.Resolve<IProjectFinder>();
    IRuntime Runtime => _dependencies.Resolve<IRuntime>();

    [Test]
    public void GetSettings_throws_if_the_source_project_cannot_be_found()
    {
        // arrange
        InputSource.Get().Returns(new UserInput(new Verb { OldProjectName = "p1" }, "s1.sln"));
        ProjectFinder.FindProject("s1.sln", "p1").Returns(default(Project));
        // act
        var action = () => _underTest.GetSettings();
        // assert
        action.Should().Throw<AbortException>().WithMessage("p1 cannot be found in the solution");
    }

    [Test]
    [Ignore("until we abstract paths")]
    public void GetSettings_throws_if_the_user_does_not_confirm()
    {
        // arrange
        InputSource.Get()
            .Returns(new UserInput(new Verb
            {
                OldProjectName = "myproj",
                NewProjectName = "changed"
            }, "s1.sln"));
        ProjectFinder.FindProject("s1.sln", "myproj")
            .Returns(new Project("/tmp/myproj/myproj.csproj", "libs", ".csproj"));
        // act
        var action = () => _underTest.GetSettings();
        // assert
        action.Should().Throw<AbortException>().WithMessage("p1 cannot be found in the solution");
    }
}