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
    ILogger Logger => _dependencies.Resolve<ILogger>();
    IToolRunner Runner => _dependencies.Resolve<IToolRunner>();

    [Test]
    public void Commit_logs_errors()
    {
        // arrange
        Action received = null;
        Runner.WhenForAnyArgs(r => r.Run(default, default)).Do(ci => received = ci.Arg<Action>());
        // act
        _underTest.Commit("bla");
        // assert
        received.Should().NotBeNull();
        received.Invoke();
        Logger.Received().Error("'git commit -m \"bla\"' failed");
    }

    [Test]
    public void Commit_sends_the_right_command_to_git()
    {
        // act
        _underTest.Commit("bla");
        // assert
        Runner.Received().Run("commit -m \"bla\"", Arg.Any<Action>());
    }

    [Test]
    public void GetVersion_sends_the_right_command_to_runner()
    {
        // arrange
        Runner.RunAndGetOutput("--version").Returns("bla");
        // act
        var result = _underTest.GetVersion();
        // assert
        result.Should().Be("bla");
    }

    [Test]
    public void RollbackAllChanges_sends_the_right_command_to_runner()
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
        Runner.WhenForAnyArgs(r => r.Run(default, default)).Do(ci => received = ci.Arg<Action>());
        // act
        _underTest.RollbackAllChanges();
        // assert
        received.Should().NotBeNull();
        received.Invoke();
        Errors.ReceivedCalls().Should().BeEmpty();
        Logger.ReceivedCalls().Should().BeEmpty();
    }

    [Test]
    public void StageAllChanges_sends_the_right_command_to_runner()
    {
        // act
        _underTest.StageAllChanges();
        // assert
        Runner.Received().Run("add .");
    }
}