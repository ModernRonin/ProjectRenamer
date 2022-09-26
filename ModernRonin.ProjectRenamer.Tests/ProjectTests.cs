using System;
using FluentAssertions;
using NUnit.Framework;

namespace ModernRonin.ProjectRenamer.Tests;

[TestFixture]
public class ProjectTests
{
    static string ProjectPath =>
        OperatingSystem.IsWindows() ? @"c:\tmp\myproj\myproj.csproj" : "/tmp/myproj/myproj.csproj";

    static string ExpectedDirectory => OperatingSystem.IsWindows() ? @"c:\tmp\myproj" : "/tmp/myproj";
    static string CurrentDirectory => OperatingSystem.IsWindows() ? @"c:\tmp" : "/tmp";

    static string ExpectedFullPathForRenameOnly =>
        OperatingSystem.IsWindows()
            ? @"c:\tmp\otherdir\changed\changed.csproj"
            : "/tmp/otherdir/changed/changed.csproj";

    static string ExpectedFullPathForMoveAndRename =>
        OperatingSystem.IsWindows() ? @"c:\tmp\changed\changed.csproj" : "/tmp/changed/changed.csproj";

    [Test]
    public void Directory_gets_the_directory_in_which_the_project_file_is_located()
    {
        // arrange
        var underTest = new Project(ProjectPath, "libs", ".csproj");
        // act & assert
        underTest.Directory.Should().Be(ExpectedDirectory);
    }

    [Test]
    public void Name_gets_the_project_file_name_without_extension()
    {
        // arrange
        var underTest = new Project(ProjectPath, "libs", ".csproj");
        // act & assert
        underTest.Name.Should().Be("myproj");
    }

    [Test]
    public void Rename_if_the_new_name_is_a_path()
    {
        // arrange
        var underTest = new Project(ProjectPath, "libs", ".csproj");
        // act
        var result = underTest.Rename("otherdir/changed", CurrentDirectory);
        // assert
        result.Should().Be(new Project(ExpectedFullPathForRenameOnly, "libs", ".csproj"));
    }

    [Test]
    public void Rename_if_the_new_name_is_not_a_path()
    {
        // arrange
        var underTest = new Project(ProjectPath, "libs", ".csproj");
        // act
        var result = underTest.Rename("changed", CurrentDirectory);
        // assert
        result.Should().Be(new Project(ExpectedFullPathForMoveAndRename, "libs", ".csproj"));
    }
}