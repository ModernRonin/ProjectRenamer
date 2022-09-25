using System;
using System.IO;
using AutofacContrib.NSubstitute;
using FluentAssertions;
using ModernRonin.FluentArgumentParser.Help;
using ModernRonin.FluentArgumentParser.Parsing;
using NSubstitute;
using NUnit.Framework;

namespace ModernRonin.ProjectRenamer.Tests;

[TestFixture]
public class InputSourceTests
{
    [SetUp]
    public void Setup()
    {
        _dependencies = AutoSubstitute.Configure()
            .Provide(new[]
            {
                "a1",
                "a2"
            })
            .Build();
        _underTest = _dependencies.Resolve<InputSource>();
    }

    AutoSubstitute _dependencies;
    InputSource _underTest;
    string[] CommandLineArguments => _dependencies.Resolve<string[]>();
    IFilesystem Filesystem => _dependencies.Resolve<IFilesystem>();
    IBindingCommandLineParser Parser => _dependencies.Resolve<IBindingCommandLineParser>();

    [Test]
    public void Get_returns_the_full_absolute_path_to_the_solution_file()
    {
        // arrange
        Filesystem.FindSolutionFiles(".", false)
            .Returns(new[] { "s1.sln" });
        var userVerb = new Verb
        {
            OldProjectName = "a.b.csproj",
            NewProjectName = "changed.b.csproj",
            ProjectFileExtension = ".csproj"
        };
        Parser.Parse(CommandLineArguments)
            .Returns(userVerb);
        // act
        var (verb, _) = _underTest.Get();
        verb.Should().BeSameAs(userVerb);
        verb.OldProjectName.Should().Be("a.b");
        verb.NewProjectName.Should().Be("changed.b");
    }

    [Test]
    public void Get_returns_the_verb_with_extensions_removed_from_project_names()
    {
        // arrange
        Filesystem.FindSolutionFiles(".", false)
            .Returns(new[] { "./tmp/s1.sln" });
        var userVerb = new Verb
        {
            OldProjectName = "a.b.csproj",
            NewProjectName = "changed.b.csproj",
            ProjectFileExtension = ".csproj"
        };
        Parser.Parse(CommandLineArguments)
            .Returns(userVerb);
        // act
        var (_, solutionPath) = _underTest.Get();
        solutionPath.Should()
            .Be(Path.Combine(Directory.GetCurrentDirectory(),
                OperatingSystem.IsWindows() ? "tmp\\s1.sln" : "tmp/s1.sln"));
    }

    [Test]
    public void Get_throws_if_the_given_source_project_name_seems_to_be_a_path_instead_of_a_name()
    {
        // arrange
        Filesystem.FindSolutionFiles(".", false)
            .Returns(new[] { "s1" });
        Parser.Parse(CommandLineArguments)
            .Returns(new Verb { OldProjectName = "a/b" });
        // act
        var action = () => _underTest.Get();
        // assert
        action.Should()
            .Throw<AbortException>()
            .Which.Message.Should()
            .StartWith("Do not specify paths for input/'old' project names, please.");
    }

    [Test]
    public void Get_throws_if_the_parser_returns_help([Values] bool isBecauseOfBadInput)
    {
        // arrange
        Filesystem.FindSolutionFiles(".", false)
            .Returns(new[] { "s1" });
        Parser.Parse(CommandLineArguments)
            .Returns(new HelpResult
            {
                IsResultOfInvalidInput = isBecauseOfBadInput,
                Text = "bla"
            });
        // act
        var action = () => _underTest.Get();
        // assert
        action.Should()
            .Throw<AbortException>()
            .WithMessage("bla")
            .Which.ExitCode.Should()
            .Be(isBecauseOfBadInput ? -1 : 0);
    }

    [Test]
    public void Get_throws_if_the_parser_unexpected_result()
    {
        // arrange
        Filesystem.FindSolutionFiles(".", false)
            .Returns(new[] { "s1" });
        Parser.Parse(CommandLineArguments)
            .Returns("bla");
        // act
        var action = () => _underTest.Get();
        // assert
        action.Should()
            .Throw<AbortException>()
            .WithMessage(
                "Something went seriously wrong. Please create an issue at https://github.com/ModernRonin/ProjectRenamer with as much detail as possible.");
    }

    [Test]
    public void Get_throws_if_there_is_more_than_one_solution_in_the_current_directory()
    {
        // arrange
        Filesystem.FindSolutionFiles(".", false)
            .Returns(new[]
            {
                "s1",
                "s2"
            });
        // act
        var action = () => _underTest.Get();
        // assert
        action.Should()
            .Throw<AbortException>()
            .WithMessage("Needs to be run from a directory with exactly one *.sln file in it.");
    }

    [Test]
    public void Get_throws_if_there_is_no_solution_in_the_current_directory()
    {
        // arrange
        Filesystem.FindSolutionFiles(".", false).Returns(Array.Empty<string>());
        // act
        var action = () => _underTest.Get();
        // assert
        action.Should()
            .Throw<AbortException>()
            .WithMessage("Needs to be run from a directory with exactly one *.sln file in it.");
    }
}