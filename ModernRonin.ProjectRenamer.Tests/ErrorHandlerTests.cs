using System;
using AutofacContrib.NSubstitute;
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
    IRuntime Runtime => _dependencies.Resolve<IRuntime>();

    [Test]
    public void HandleException_does_not_roll_back_changes_if_doResetGit_is_false()
    {
        // arrange
        var ex = new AbortException("bla", false, -13);
        // act
        _underTest.HandleException(ex);
        // arrange
        Git.Value.DidNotReceive().RollbackAllChanges();
    }

    [Test]
    public void HandleException_logs_an_error_and_aborts_the_process([Values] bool doResetGit)
    {
        // arrange
        var ex = new AbortException("bla", doResetGit, -13);
        // act
        _underTest.HandleException(ex);
        // arrange
        Received.InOrder(() =>
        {
            Runtime.Error("bla");
            Runtime.Abort(-13);
        });
    }

    [Test]
    public void HandleException_rolls_back_all_changes_before_aborting_the_process_if_doResetGit_is_true()
    {
        // arrange
        var ex = new AbortException("bla", true, -13);
        // act
        _underTest.HandleException(ex);
        // arrange
        Received.InOrder(() =>
        {
            Git.Value.RollbackAllChanges();
            Runtime.Abort(-13);
        });
    }
}