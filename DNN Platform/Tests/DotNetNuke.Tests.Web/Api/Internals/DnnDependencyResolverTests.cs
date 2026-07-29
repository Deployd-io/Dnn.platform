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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private IServiceProvider serviceProvider;
        private IDependencyResolver dependencyResolver;

        private interface ITestService
        {
        }

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
			Logger.LogDebug(">>> Entering FixtureSetUp()");
            var services = new ServiceCollection();
            services.AddSingleton<IScopeAccessor>(sp => new FakeScopeAccessor(sp.CreateScope()));
			Logger.LogDebug("FixtureSetUp(): services → {Services}", services);
            services.AddScoped(typeof(ITestService), typeof(TestService));

            this.serviceProvider = services.BuildServiceProvider();
			Logger.LogDebug("FixtureSetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
            this.dependencyResolver = new DnnDependencyResolver(this.serviceProvider);
			Logger.LogDebug("<<< Exiting FixtureSetUp()");
			Logger.LogDebug("FixtureSetUp(): dependencyResolver → {DependencyResolver}", dependencyResolver);
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
			Logger.LogDebug(">>> Entering FixtureTearDown()");
            this.dependencyResolver?.Dispose();
            this.dependencyResolver = null;
			Logger.LogDebug("FixtureTearDown(): dependencyResolver → {DependencyResolver}", dependencyResolver);
			Logger.LogDebug("FixtureTearDown(): this.dependencyResolver → {Arg0}", this.dependencyResolver);

            if (this.serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            this.serviceProvider = null;
			Logger.LogDebug("<<< Exiting FixtureTearDown()");
			Logger.LogDebug("FixtureTearDown(): serviceProvider → {ServiceProvider}", serviceProvider);
			Logger.LogDebug("FixtureTearDown(): this.serviceProvider → {Arg0}", this.serviceProvider);
        }

        [Test]
        public void NotNull()
        {
			Logger.LogDebug(">>> Entering NotNull()");
            Assert.That(this.dependencyResolver, Is.Not.Null);
			Logger.LogDebug("<<< Exiting NotNull()");
        }

        [Test]
        public void IsOfTypeDnnDependencyResolver()
        {
			Logger.LogDebug(">>> Entering IsOfTypeDnnDependencyResolver()");
            Assert.That(this.dependencyResolver, Is.InstanceOf<DnnDependencyResolver>());
			Logger.LogDebug("<<< Exiting IsOfTypeDnnDependencyResolver()");
        }

        [Test]
        public void GetTestService()
        {
			Logger.LogDebug(">>> Entering GetTestService()");
            var expected = new TestService();
            var actual = this.dependencyResolver.GetService(typeof(ITestService));
			Logger.LogDebug("GetTestService(): expected → {Expected}", expected);
			Logger.LogDebug("GetTestService(): actual → {Actual}", actual);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.GetType(), Is.EqualTo(expected.GetType()));
			Logger.LogDebug("<<< Exiting GetTestService()");
        }

        [Test]
        public void GetTestServices()
        {
			Logger.LogDebug(">>> Entering GetTestServices()");
            var expected = new TestService();
            var actual = this.dependencyResolver.GetServices(typeof(ITestService)).ToArray();
			Logger.LogDebug("GetTestServices(): expected → {Expected}", expected);
			Logger.LogDebug("GetTestServices(): actual → {Actual}", actual);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Has.Length.EqualTo(1));
            Assert.That(actual[0].GetType(), Is.EqualTo(expected.GetType()));
			Logger.LogDebug("<<< Exiting GetTestServices()");
        }

        [Test]
        public void BeginScope()
        {
			Logger.LogDebug(">>> Entering BeginScope()");
            var actual = this.dependencyResolver.BeginScope();

			Logger.LogDebug("BeginScope(): actual → {Actual}", actual);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.InstanceOf<DnnDependencyResolver>());
			Logger.LogDebug("<<< Exiting BeginScope()");
        }

        [Test]
        public void BeginScope_GetService()
        {
			Logger.LogDebug(">>> Entering BeginScope_GetService()");
            var scope = this.dependencyResolver.BeginScope();

			Logger.LogDebug("BeginScope_GetService(): scope → {Scope}", scope);
            var expected = new TestService();
            var actual = scope.GetService(typeof(ITestService));
			Logger.LogDebug("BeginScope_GetService(): expected → {Expected}", expected);
			Logger.LogDebug("BeginScope_GetService(): actual → {Actual}", actual);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.GetType(), Is.EqualTo(expected.GetType()));
			Logger.LogDebug("<<< Exiting BeginScope_GetService()");
        }

        [Test]
        public void BeginScope_GetServices()
        {
			Logger.LogDebug(">>> Entering BeginScope_GetServices()");
            var scope = this.dependencyResolver.BeginScope();

			Logger.LogDebug("BeginScope_GetServices(): scope → {Scope}", scope);
            var expected = new TestService();
            var actual = scope.GetServices(typeof(ITestService)).ToArray();
			Logger.LogDebug("BeginScope_GetServices(): expected → {Expected}", expected);
			Logger.LogDebug("BeginScope_GetServices(): actual → {Actual}", actual);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Has.Length.EqualTo(1));
            Assert.That(actual[0].GetType(), Is.EqualTo(expected.GetType()));
			Logger.LogDebug("<<< Exiting BeginScope_GetServices()");
        }

        private class TestService : ITestService
        {
        }

        private class FakeScopeAccessor : IScopeAccessor
        {
            private IServiceScope fakeScope;

            public FakeScopeAccessor(IServiceScope fakeScope)
            {
				Logger.LogDebug(">>> Entering FakeScopeAccessor(fakeScope={FakeScope})", fakeScope);
                this.fakeScope = fakeScope;
				Logger.LogDebug("<<< Exiting FakeScopeAccessor(fakeScope={FakeScope})", fakeScope);
				Logger.LogDebug("FakeScopeAccessor(fakeScope={FakeScope}): fakeScope → {FakeScope1}", fakeScope, fakeScope);
				Logger.LogDebug("FakeScopeAccessor(fakeScope={FakeScope}): this.fakeScope → {Arg1}", fakeScope, this.fakeScope);
            }

            public IServiceScope GetScope()
            {
				Logger.LogDebug(">>> Entering GetScope()");
				Logger.LogDebug("<<< Exiting GetScope()");
                return this.fakeScope;
            }
        }
    }
}
