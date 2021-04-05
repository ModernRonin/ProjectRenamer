using FluentAssertions;
using NUnit.Framework;

namespace ModernRonin.ProjectRenamer.Tests
{
    [TestFixture]
    public class CommonExtensionsTests
    {
        [Test]
        public void AsText_with_false_returns_no() => false.AsText().Should().Be("no");

        [Test]
        public void AsText_with_true_returns_yes() => true.AsText().Should().Be("yes");
    }
}