// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.Api.Internals
{
    using System;
    using System.Linq;
    using System.Web.Http.Dependencies;

    using DotNetNuke.Services.DependencyInjection;
    using DotNetNuke.Web.Api.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class DnnDependencyResolverTests
    {
        private IServiceProvider serviceProvider;
        private IDependencyResolver dependencyResolver;

        private interface ITestService
        {
        }

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
			eventLogger.LogDebug(">>> Entering FixtureSetUp()");
            var services = new ServiceCollection();
            services.AddSingleton<IScopeAccessor>(sp => new FakeScopeAccessor(sp.CreateScope()));
			eventLogger.LogDebug("FixtureSetUp(): services → {Services}", services);
            services.AddScoped(typeof(ITestService), typeof(TestService));

            this.serviceProvider = services.BuildServiceProvider();
			eventLogger.LogDebug("FixtureSetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
            this.dependencyResolver = new DnnDependencyResolver(this.serviceProvider);
			eventLogger.LogDebug("FixtureSetUp(): dependencyResolver → {DependencyResolver}", dependencyResolver);
			eventLogger.LogDebug("<<< Exiting FixtureSetUp()");
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
			eventLogger.LogDebug(">>> Entering FixtureTearDown()");
            this.dependencyResolver?.Dispose();
            this.dependencyResolver = null;
			eventLogger.LogDebug("FixtureTearDown(): dependencyResolver → {DependencyResolver}", dependencyResolver);
			eventLogger.LogDebug("FixtureTearDown(): this.dependencyResolver → {Arg0}", this.dependencyResolver);

            if (this.serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            this.serviceProvider = null;
			eventLogger.LogDebug("FixtureTearDown(): serviceProvider → {ServiceProvider}", serviceProvider);
			eventLogger.LogDebug("FixtureTearDown(): this.serviceProvider → {Arg0}", this.serviceProvider);
			eventLogger.LogDebug("<<< Exiting FixtureTearDown()");
        }

        [Test]
        public void NotNull()
        {
			eventLogger.LogDebug(">>> Entering NotNull()");
            Assert.That(this.dependencyResolver, Is.Not.Null);
			eventLogger.LogDebug("<<< Exiting NotNull()");
        }

        [Test]
        public void IsOfTypeDnnDependencyResolver()
        {
			eventLogger.LogDebug(">>> Entering IsOfTypeDnnDependencyResolver()");
            Assert.That(this.dependencyResolver, Is.InstanceOf<DnnDependencyResolver>());
			eventLogger.LogDebug("<<< Exiting IsOfTypeDnnDependencyResolver()");
        }

        [Test]
        public void GetTestService()
        {
			eventLogger.LogDebug(">>> Entering GetTestService()");
            var expected = new TestService();
            var actual = this.dependencyResolver.GetService(typeof(ITestService));
			eventLogger.LogDebug("GetTestService(): actual → {Actual}", actual);
			eventLogger.LogDebug("GetTestService(): expected → {Expected}", expected);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.GetType(), Is.EqualTo(expected.GetType()));
			eventLogger.LogDebug("<<< Exiting GetTestService()");
        }

        [Test]
        public void GetTestServices()
        {
			eventLogger.LogDebug(">>> Entering GetTestServices()");
            var expected = new TestService();
            var actual = this.dependencyResolver.GetServices(typeof(ITestService)).ToArray();
			eventLogger.LogDebug("GetTestServices(): actual → {Actual}", actual);
			eventLogger.LogDebug("GetTestServices(): expected → {Expected}", expected);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Has.Length.EqualTo(1));
            Assert.That(actual[0].GetType(), Is.EqualTo(expected.GetType()));
			eventLogger.LogDebug("<<< Exiting GetTestServices()");
        }

        [Test]
        public void BeginScope()
        {
			eventLogger.LogDebug(">>> Entering BeginScope()");
            var actual = this.dependencyResolver.BeginScope();

			eventLogger.LogDebug("BeginScope(): actual → {Actual}", actual);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.InstanceOf<DnnDependencyResolver>());
			eventLogger.LogDebug("<<< Exiting BeginScope()");
        }

        [Test]
        public void BeginScope_GetService()
        {
			eventLogger.LogDebug(">>> Entering BeginScope_GetService()");
            var scope = this.dependencyResolver.BeginScope();

			eventLogger.LogDebug("BeginScope_GetService(): scope → {Scope}", scope);
            var expected = new TestService();
			eventLogger.LogDebug("BeginScope_GetService(): expected → {Expected}", expected);
            var actual = scope.GetService(typeof(ITestService));
			eventLogger.LogDebug("BeginScope_GetService(): actual → {Actual}", actual);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.GetType(), Is.EqualTo(expected.GetType()));
			eventLogger.LogDebug("<<< Exiting BeginScope_GetService()");
        }

        [Test]
        public void BeginScope_GetServices()
        {
			eventLogger.LogDebug(">>> Entering BeginScope_GetServices()");
            var scope = this.dependencyResolver.BeginScope();

			eventLogger.LogDebug("BeginScope_GetServices(): scope → {Scope}", scope);
            var expected = new TestService();
			eventLogger.LogDebug("BeginScope_GetServices(): expected → {Expected}", expected);
            var actual = scope.GetServices(typeof(ITestService)).ToArray();
			eventLogger.LogDebug("BeginScope_GetServices(): actual → {Actual}", actual);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Has.Length.EqualTo(1));
            Assert.That(actual[0].GetType(), Is.EqualTo(expected.GetType()));
			eventLogger.LogDebug("<<< Exiting BeginScope_GetServices()");
        }

        private class TestService : ITestService
        {
        }

        private class FakeScopeAccessor : IScopeAccessor
        {
            private IServiceScope fakeScope;

            public FakeScopeAccessor(IServiceScope fakeScope)
            {
				eventLogger.LogDebug(">>> Entering FakeScopeAccessor(fakeScope={FakeScope})", fakeScope);
                this.fakeScope = fakeScope;
				eventLogger.LogDebug("FakeScopeAccessor(fakeScope={FakeScope}): fakeScope → {FakeScope1}", fakeScope, fakeScope);
				eventLogger.LogDebug("FakeScopeAccessor(fakeScope={FakeScope}): this.fakeScope → {Arg1}", fakeScope, this.fakeScope);
				eventLogger.LogDebug("<<< Exiting FakeScopeAccessor(fakeScope={FakeScope})", fakeScope);
            }

            public IServiceScope GetScope()
            {
				eventLogger.LogDebug(">>> Entering GetScope()");
				eventLogger.LogDebug("<<< Exiting GetScope()");
                return this.fakeScope;
            }
        }
    }
}
