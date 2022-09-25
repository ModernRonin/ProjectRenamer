using System;
using AutofacContrib.NSubstitute;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace ModernRonin.ProjectRenamer.Tests;

[TestFixture]
public class GitTests
{
    [SetUp]
    public void Setup()
    {
        _dependencies = AutoSubstitute.Configure().Provide(createRunner).Build();
        _underTest = _dependencies.Resolve<Git>();

        IToolRunner createRunner(string tool)
        {
            tool.Should().Be("git");
            return Runner;
        }
    }

    AutoSubstitute _dependencies;
    Git _underTest;
    IErrorHandler Errors => _dependencies.Resolve<IErrorHandler>();
    IRuntime Runtime => _dependencies.Resolve<IRuntime>();
    IToolRunner Runner => _dependencies.Resolve<IToolRunner>();

    [Test]
    public void Commit_sends_the_right_command_to_the_runner()
    {
        // act
        _underTest.Commit("bla");
        // assert
        Runner.Received().Run("commit -m \"bla\"", "'git commit -m \"bla\"' failed");
    }

    [Test]
    public void EnsureIsClean_passes_a_meaningfull_message_for_the_errorException()
    {
        // arrange
        // act
        _underTest.EnsureIsClean();
        // assert
        Runner.Received()
            .Run(Arg.Any<string>(),
                Arg.Is<AbortException>(x => x.Message == "git does not seem to be clean, check git status"));
    }

    [Test]
    public void EnsureIsClean_sets_up_a_series_of_git_commands()
    {
        // act
        _underTest.EnsureIsClean();
        // assert
        Received.InOrder(() =>
        {
            Runner.Run("update-index -q --refresh", Arg.Any<AbortException>());
            Runner.Run("diff-index --quiet --cached HEAD --", Arg.Any<AbortException>());
            Runner.Run("diff-files --quiet", Arg.Any<AbortException>());
            Runner.Run("ls-files --exclude-standard --others", Arg.Any<AbortException>());
        });
    }

    [Test]
    public void GetVersion_sends_the_right_command_to_the_runner()
    {
        // arrange
        Runner.RunAndGetOutput("--version").Returns("bla");
        // act
        var result = _underTest.GetVersion();
        // assert
        result.Should().Be("bla");
    }

    [Test]
    public void Move_sends_the_right_command_to_the_runner()
    {
        // act
        _underTest.Move("./x/a", "./x/b");
        // assert
        Runner.Received().Run("mv \"./x/a\" \"./x/b\"");
    }

    [Test]
    public void RollbackAllChanges_sends_the_right_command_to_the_runner()
    {
        // act
        _underTest.RollbackAllChanges();
        // assert
        Runner.Received().Run("reset --hard HEAD", Arg.Any<Action>());
    }

    [Test]
    public void RollbackAllChanges_swallows_errors()
    {
        // because if rollback fails we really cannot do anything about this
        // and we don't need to log either because there will be already
        // some error message from Git in the console

        // arrange
        Action received = null;
        Runner.WhenForAnyArgs(r => r.Run(default, (Action)default)).Do(ci => received = ci.Arg<Action>());
        // act
        _underTest.RollbackAllChanges();
        // assert
        received.Should().NotBeNull();
        received.Invoke();
        Errors.ReceivedCalls().Should().BeEmpty();
        Runtime.ReceivedCalls().Should().BeEmpty();
    }

    [Test]
    public void StageAllChanges_sends_the_right_command_to_the_runner()
    {
        // act
        _underTest.StageAllChanges();
        // assert
        Runner.Received().Run("add .");
    }
}