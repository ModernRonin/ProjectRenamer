using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace ModernRonin.ProjectRenamer.Tests;

[TestFixture]
public class DotnetTests
{
    [SetUp]
    public void Setup()
    {
        _underTest = new Dotnet(createRunner);

        IToolRunner createRunner(string tool)
        {
            tool.Should().Be("dotnet");
            _runner = Substitute.For<IToolRunner>();
            return _runner;
        }
    }

    IToolRunner _runner;
    Dotnet _underTest;

    [Test]
    public void AddReference()
    {
        // act
        _underTest.AddReference("p1", "r1");
        // assert
        _runner.Received().Run("add \"p1\" reference \"r1\"");
    }

    [Test]
    public void AddToSolution_with_solutionFolder()
    {
        // act
        _underTest.AddToSolution("c:/myproject/myproject.csproj", "Features/.shared");
        // assert
        _runner.Received().Run(@"sln add -s ""Features\.shared"" ""c:/myproject/myproject.csproj""");
    }

    [Test]
    public void AddToSolution_without_solutionFolder()
    {
        // act
        _underTest.AddToSolution("p1");
        // assert
        _runner.Received().Run("sln add \"p1\"");
    }

    [Test]
    public void BuildSolution()
    {
        // arrange
        // ReSharper disable once ConvertToLocalFunction
        var onError = () => { };
        // act
        _underTest.BuildSolution(onError);
        // assert
        _runner.Received().Run("build", onError);
    }

    [Test]
    public void GetReferencedProjects()
    {
        // arrange
        _runner.RunAndGetOutput("list \"p1\" reference")
            .Returns(@"
Project reference(s)
--------------------
..\ModernRonin.ProjectRenamer\ModernRonin.ProjectRenamer.csproj
.\SomeOther.csproj
");
        // act
        var result = _underTest.GetReferencedProjects("p1");
        // assert
        result.Should()
            .Equal("..\\ModernRonin.ProjectRenamer\\ModernRonin.ProjectRenamer.csproj",
                ".\\SomeOther.csproj");
    }

    [Test]
    public void PaketInstall()
    {
        // act
        _underTest.PaketInstall();
        // assert
        _runner.Received().Run("paket install");
    }

    [Test]
    public void RemoveFromSolution()
    {
        // act
        _underTest.RemoveFromSolution("p1");
        // assert
        _runner.Received().Run("sln remove \"p1\"");
    }

    [Test]
    public void RemoveReference()
    {
        // act
        _underTest.RemoveReference("p1", "r1");
        // assert
        _runner.Received().Run("remove \"p1\" reference \"r1\"");
    }
}