using System;
using Autofac;
using FluentAssertions;
using NUnit.Framework;

namespace ModernRonin.ProjectRenamer.Tests
{
    [TestFixture]
    public class RenamerModuleTests
    {
        [SetUp]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<RenamerModule>();
            _container = builder.Build();
        }

        IContainer _container;

        [Test]
        public void Application_factory_can_be_resolved()
        {
            var factory = _container.Resolve<Func<Configuration, string, Application>>();
            factory.Should().NotBeNull();
            factory(new Configuration(), string.Empty).Should().NotBeNull();
        }

        [Test]
        public void IConfigurationSetup_can_be_resolved()
        {
            _container.Resolve<IConfigurationSetup>().Should().NotBeNull();
        }
    }
}