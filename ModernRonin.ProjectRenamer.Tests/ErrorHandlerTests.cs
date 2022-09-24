using System;
using AutofacContrib.NSubstitute;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace ModernRonin.ProjectRenamer.Tests;

[TestFixture]
public class ErrorHandlerTests
{
    [SetUp]
    public void Setup()
    {
        _dependencies = new AutoSubstitute();
        _underTest = _dependencies.Resolve<ErrorHandler>();
    }

    AutoSubstitute _dependencies;
    ErrorHandler _underTest;
    Lazy<IGit> Git => _dependencies.Resolve<Lazy<IGit>>();
    ILogger Logger => _dependencies.Resolve<ILogger>();
    IRuntime Runtime => _dependencies.Resolve<IRuntime>();

    [Test]
    public void Handle_with_a_message_does_not_reset_git()
    {
        // act
        _underTest.Handle("bla");
        // assert
        Git.Value.ReceivedCalls().Should().BeEmpty();
    }

    [Test]
    public void Handle_with_a_message_logs_the_error_and_THEN_aborts()
    {
        // act
        _underTest.Handle("bla");
        // assert
        Received.InOrder(() =>
        {
            Logger.Error("bla");
            Runtime.Abort();
        });
    }

    [Test]
    public void Handle_with_a_tool_and_arguments_logs_an_error_resets_git_and_aborts()
    {
        // act
        _underTest.Handle("myTool", "-a -b");
        // assert
        Received.InOrder(() =>
        {
            Logger.Error("call 'myTool -a -b' failed - aborting");
            Git.Value.RollbackAllChanges();
            Runtime.Abort();
        });
    }

    [Test]
    public void HandleAndReset_logs_the_error_resets_git_and_aborts()
    {
        // act
        _underTest.HandleAndReset("bla");
        // assert
        Received.InOrder(() =>
        {
            Logger.Error("bla");
            Git.Value.RollbackAllChanges();
            Runtime.Abort();
        });
    }
}