using System;
using AutofacContrib.NSubstitute;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace ModernRonin.ProjectRenamer.Tests;

[TestFixture]
public class ToolRunnerTests
{
    [SetUp]
    public void Setup()
    {
        _dependencies = AutoSubstitute.Configure().Provide("myTool").Build();
        _underTest = _dependencies.Resolve<ToolRunner>();
    }

    AutoSubstitute _dependencies;
    ToolRunner _underTest;
    IRuntime Runtime => _dependencies.Resolve<IRuntime>();

    [Test]
    public void Run_delegates_to_the_runtime()
    {
        // arrange
        // ReSharper disable once ConvertToLocalFunction
        var onError = () => { };
        // act
        _underTest.Run("-a -b", onError);
        // assert
        Runtime.Received().Run("myTool", "-a -b", onError);
    }

    [Test]
    public void Run_with_errorException_throws_the_errorException_on_error()
    {
        // arrange
        Action received = null;
        Runtime.When(r => r.Run("myTool", "-a -b", Arg.Any<Action>())).Do(ci => received = ci.Arg<Action>());
        var x = new AbortException();
        // act
        _underTest.Run("-a -b", x);
        // assert
        var action = () => received.Invoke();
        action.Should().Throw<AbortException>().Which.Should().BeSameAs(x);
    }

    [Test]
    public void Run_with_errorMessage_logs_the_errorMessage_on_error()
    {
        // arrange
        Action received = null;
        Runtime.When(r => r.Run("myTool", "-a -b", Arg.Any<Action>())).Do(ci => received = ci.Arg<Action>());
        // act
        _underTest.Run("-a -b", "bla");
        // assert
        received.Invoke();
        Runtime.Received().Error("bla");
    }

    [Test]
    public void Run_without_onError_throws_on_error()
    {
        // arrange
        Action received = null;
        Runtime.When(r => r.Run("myTool", "-a -b", Arg.Any<Action>())).Do(ci => received = ci.Arg<Action>());
        // act
        _underTest.Run("-a -b");
        // assert
        received.Should().NotBeNull();
        var action = () => received.Invoke();
        action.Should().Throw<AbortException>().WithMessage("call 'myTool -a -b' failed - aborting");
    }

    [Test]
    public void RunAndGetOutput_delegates_to_the_runtime()
    {
        // arrange
        // ReSharper disable once ConvertToLocalFunction
        var onError = () => { };
        Runtime.RunAndGetOutput("myTool", "-a -b", onError).Returns("bla");
        // act
        var result = _underTest.RunAndGetOutput("-a -b", onError);
        // assert
        result.Should().Be("bla");
    }

    [Test]
    public void RunAndGetOutput_without_onError_throws_on_error()
    {
        // arrange
        Action received = null;
        Runtime.When(r => r.RunAndGetOutput("myTool", "-a -b", Arg.Any<Action>()))
            .Do(ci => received = ci.Arg<Action>());
        // act
        _underTest.RunAndGetOutput("-a -b");
        // assert
        received.Should().NotBeNull();
        var action = () => received.Invoke();
        action.Should().Throw<AbortException>().WithMessage("call 'myTool -a -b' failed - aborting");
    }
}