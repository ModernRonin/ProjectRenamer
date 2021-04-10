using System.Runtime.InteropServices;
using AutofacContrib.NSubstitute;
using NSubstitute;
using NUnit.Framework;

namespace ModernRonin.ProjectRenamer.Tests
{
    [TestFixture]
    public class DotnetTests
    {
        [SetUp]
        public void Setup()
        {
            _dependencies = new AutoSubstitute();
            _underTest = _dependencies.Resolve<Dotnet>();
        }

        AutoSubstitute _dependencies;
        Dotnet _underTest;

        IExecutor Executor => _dependencies.Resolve<IExecutor>();

        [Test(Description = "fix for https://github.com/ModernRonin/ProjectRenamer/issues/24")]
        public void AddToSolution_replaces_forward_slashes_with_backward_slashes_in_solutionFolder()
        {
            _underTest.AddToSolution("c:/myproject/myproject.csproj", "Features/.shared");
            var expectedArguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"sln add -s ""Features\.shared"" ""c:/myproject/myproject.csproj"""
                : @"sln add -s ""Features/.shared"" ""c:/myproject/myproject.csproj""";
            Executor.Received()
                .Tool("dotnet", expectedArguments);
        }
    }
}