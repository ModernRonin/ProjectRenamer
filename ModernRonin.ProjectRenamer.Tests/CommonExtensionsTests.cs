using FluentAssertions;
using NUnit.Framework;

namespace ModernRonin.ProjectRenamer.Tests
{
    [TestFixture]
    public class CommonExtensionsTests
    {
        [TestCase('/')]
        [TestCase('\\')]
        public void ToAbsolutePath_handles_leading_directory_separators_in_self(char separator)
        {
            $"{separator}subdir/alpha/bravo.txt".ToAbsolutePath("c:/rootdir")
                .Should()
                .Be(@"c:\rootdir\subdir\alpha\bravo.txt");
        }

        [TestCase('a')]
        [TestCase('1')]
        [TestCase('!')]
        [TestCase(':')]
        public void IsDirectorySeparator_returns_false_for_other_characters(char c) =>
            c.IsDirectorySeparator().Should().BeFalse();

        [Test]
        public void AsText_with_false_returns_no() => false.AsText().Should().Be("no");

        [Test]
        public void AsText_with_true_returns_yes() => true.AsText().Should().Be("yes");

        [Test]
        public void EscapeForShell_returns_argument_embedded_into_doublequotes() =>
            "alpha".EscapeForShell().Should().Be(@"""alpha""");

        [Test]
        public void IsDirectorySeparator_returns_true_for_linux_separator() =>
            '/'.IsDirectorySeparator().Should().BeTrue();

        [Test]
        public void IsDirectorySeparator_returns_true_for_windows_separator() =>
            '\\'.IsDirectorySeparator().Should().BeTrue();

        [Test]
        public void
            On_Windows_EnsurePlatformDirectorySeparators_replaces_forward_slashes_with_backward_slashes()
        {
            @"alpha/bravo\charlie/delta".EnsurePlatformDirectorySeparators()
                .Should()
                .Be(@"alpha\bravo\charlie\delta");
        }

        [Test]
        public void Repeat_returns_the_string_repeated_the_passed_number_of_times() =>
            "alpha".Repeat(3).Should().Be("alphaalphaalpha");

        [Test]
        public void ToAbsolutePath_returns_the_absolute_path_of_self_in_base_directory()
        {
            "subdir/alpha/bravo.txt".ToAbsolutePath("c:/rootdir")
                .Should()
                .Be(@"c:\rootdir\subdir\alpha\bravo.txt");
        }

        [Test]
        public void ToRelativePath_returns_the_relate_path_of_self_in_baseDirectory_with_linux_separators()
        {
            @"c:/rootdir/subdir/alpha/bravo.txt".ToRelativePath("c:/rootdir")
                .Should()
                .Be(@"subdir\alpha\bravo.txt");
        }

        [Test]
        public void ToRelativePath_returns_the_relate_path_of_self_in_baseDirectory_with_windows_separators()
        {
            @"c:\rootdir\subdir\alpha\bravo.txt".ToRelativePath("c:/rootdir")
                .Should()
                .Be(@"subdir\alpha\bravo.txt");
        }
    }
}