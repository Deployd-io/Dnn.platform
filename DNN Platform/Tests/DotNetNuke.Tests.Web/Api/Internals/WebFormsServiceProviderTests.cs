// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.Api.Internals
{
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Web.Common;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    public class WebFormsServiceProviderTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private interface IScopedService
        { }

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
			Logger.LogDebug(">>> Entering FixtureSetUp()");
            var serviceCollection = new ServiceCollection();

			Logger.LogDebug("FixtureSetUp(): serviceCollection → {ServiceCollection}", serviceCollection);
            serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
            serviceCollection.AddTransient<IApplicationStatusInfo>(container => new DotNetNuke.Application.ApplicationStatusInfo(Mock.Of<IApplicationInfo>()));
            serviceCollection.AddScoped<IScopedService, ScopedService>();
            serviceCollection.AddSingleton<SingletonService>();

            Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
			Logger.LogDebug("<<< Exiting FixtureSetUp()");
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
			Logger.LogDebug(">>> Entering FixtureTearDown()");
			Logger.LogDebug("FixtureTearDown(): Globals.DependencyProvider → {Arg0}", Globals.DependencyProvider);
            Globals.DependencyProvider = null;
			Logger.LogDebug("<<< Exiting FixtureTearDown()");
        }

        [Test]
        public void CreateInstanceFromScope()
        {
			Logger.LogDebug(">>> Entering CreateInstanceFromScope()");
            // Arrange
            var scope = Globals.DependencyProvider.CreateScope();

			Logger.LogDebug("CreateInstanceFromScope(): scope → {Scope}", scope);
            HttpContextHelper.RegisterMockHttpContext();
            HttpContextSource.Current.SetScope(scope);

            var provider = new WebFormsServiceProvider();

			Logger.LogDebug("CreateInstanceFromScope(): provider → {Provider}", provider);
            // Act
            var instance = provider.GetService(typeof(IScopedService));

			Logger.LogDebug("CreateInstanceFromScope(): instance → {Instance}", instance);
            // Assert
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.EqualTo(scope.ServiceProvider.GetRequiredService<IScopedService>()));
			Logger.LogDebug("<<< Exiting CreateInstanceFromScope()");
        }

        [Test]
        public void CreateInstanceWithProtectedConstructor()
        {
			Logger.LogDebug(">>> Entering CreateInstanceWithProtectedConstructor()");
            // Arrange
            var scope = Globals.DependencyProvider.CreateScope();

			Logger.LogDebug("CreateInstanceWithProtectedConstructor(): scope → {Scope}", scope);
            HttpContextHelper.RegisterMockHttpContext();
            HttpContextSource.Current.SetScope(scope);

            var provider = new WebFormsServiceProvider();

			Logger.LogDebug("CreateInstanceWithProtectedConstructor(): provider → {Provider}", provider);
            // Act
            var instance = provider.GetService(typeof(PageHandlerFactory));

			Logger.LogDebug("CreateInstanceWithProtectedConstructor(): instance → {Instance}", instance);
            // Assert
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.InstanceOf<PageHandlerFactory>());
			Logger.LogDebug("<<< Exiting CreateInstanceWithProtectedConstructor()");
        }

        [Test]
        public void CreateModuleWithSingletonService()
        {
			Logger.LogDebug(">>> Entering CreateModuleWithSingletonService()");
            // Arrange
            var scope = Globals.DependencyProvider.CreateScope();

			Logger.LogDebug("CreateModuleWithSingletonService(): scope → {Scope}", scope);
            HttpContextHelper.RegisterMockHttpContext();
            HttpContextSource.Current.SetScope(scope);

            var provider = new WebFormsServiceProvider();
            var service = Globals.DependencyProvider.GetRequiredService<SingletonService>();
			Logger.LogDebug("CreateModuleWithSingletonService(): provider → {Provider}", provider);
			Logger.LogDebug("CreateModuleWithSingletonService(): service → {Service}", service);

            // Act
            var module = provider.GetService<TestModule<SingletonService>>();

			Logger.LogDebug("CreateModuleWithSingletonService(): module → {Module}", module);
            // Assert
            Assert.That(module, Is.Not.Null);
            Assert.That(module.DependencyService, Is.EqualTo(module.ConstructorService));
            Assert.That(service, Is.EqualTo(module.ConstructorService));
			Logger.LogDebug("<<< Exiting CreateModuleWithSingletonService()");
        }

        [Test]
        public void CreateModuleWithScopedService()
        {
			Logger.LogDebug(">>> Entering CreateModuleWithScopedService()");
            // Arrange
            var scope = Globals.DependencyProvider.CreateScope();

			Logger.LogDebug("CreateModuleWithScopedService(): scope → {Scope}", scope);
            HttpContextHelper.RegisterMockHttpContext();
            HttpContextSource.Current.SetScope(scope);

            var provider = new WebFormsServiceProvider();
            var serviceFromRequestScope = scope.ServiceProvider.GetRequiredService<IScopedService>();
			Logger.LogDebug("CreateModuleWithScopedService(): provider → {Provider}", provider);
			Logger.LogDebug("CreateModuleWithScopedService(): serviceFromRequestScope → {ServiceFromRequestScope}", serviceFromRequestScope);
            var serviceFromGlobalScope = Globals.DependencyProvider.GetRequiredService<IScopedService>();

			Logger.LogDebug("CreateModuleWithScopedService(): serviceFromGlobalScope → {ServiceFromGlobalScope}", serviceFromGlobalScope);
            // Act
            var module = provider.GetService<TestModule<IScopedService>>();

			Logger.LogDebug("CreateModuleWithScopedService(): module → {Module}", module);
            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(module, Is.Not.Null);
                Assert.That(module.DependencyService, Is.EqualTo(module.ConstructorService));
                Assert.That(serviceFromRequestScope, Is.EqualTo(module.ConstructorService));
                Assert.That(serviceFromGlobalScope, Is.Not.EqualTo(module.ConstructorService));
            }
			Logger.LogDebug("<<< Exiting CreateModuleWithScopedService()");
        }

        private class ScopedService : IScopedService
        { }

        private class SingletonService
        { }

        public class TestModule<T> : PortalModuleBase
        {
            public TestModule(T service)
            {
                ConstructorService = service;
                DependencyService = DependencyProvider.GetRequiredService<T>();
            }

            public T ConstructorService { get; }

            public T DependencyService { get; }
        }
    }
}
