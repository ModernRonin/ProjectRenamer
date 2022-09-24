﻿using System;
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
    IErrorHandler Errors => _dependencies.Resolve<IErrorHandler>();
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
    public void Run_without_onError_passes_standard_error_handler_to_runtime()
    {
        // arrange
        Action received = null;
        Runtime.When(r => r.Run("myTool", "-a -b", Arg.Any<Action>())).Do(ci => received = ci.Arg<Action>());
        // act
        _underTest.Run("-a -b");
        // assert
        received.Should().NotBeNull();
        received.Invoke();
        Errors.Received().Handle("myTool", "-a -b");
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
    public void RunAndGetOutput_without_onError_passes_standard_error_handler_to_runtime()
    {
        // arrange
        Action received = null;
        Runtime.When(r => r.RunAndGetOutput("myTool", "-a -b", Arg.Any<Action>()))
            .Do(ci => received = ci.Arg<Action>());
        // act
        _underTest.RunAndGetOutput("-a -b");
        // assert
        received.Should().NotBeNull();
        received.Invoke();
        Errors.Received().Handle("myTool", "-a -b");
    }
}