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
        private interface IScopedService
        { }

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
			eventLogger.LogDebug(">>> Entering FixtureSetUp()");
            var serviceCollection = new ServiceCollection();

			eventLogger.LogDebug("FixtureSetUp(): serviceCollection → {ServiceCollection}", serviceCollection);
            serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
            serviceCollection.AddTransient<IApplicationStatusInfo>(container => new DotNetNuke.Application.ApplicationStatusInfo(Mock.Of<IApplicationInfo>()));
            serviceCollection.AddScoped<IScopedService, ScopedService>();
            serviceCollection.AddSingleton<SingletonService>();

            Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
			eventLogger.LogDebug("<<< Exiting FixtureSetUp()");
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
			eventLogger.LogDebug(">>> Entering FixtureTearDown()");
			eventLogger.LogDebug("FixtureTearDown(): Globals.DependencyProvider → {Arg0}", Globals.DependencyProvider);
            Globals.DependencyProvider = null;
			eventLogger.LogDebug("<<< Exiting FixtureTearDown()");
        }

        [Test]
        public void CreateInstanceFromScope()
        {
			eventLogger.LogDebug(">>> Entering CreateInstanceFromScope()");
            // Arrange
            var scope = Globals.DependencyProvider.CreateScope();
			eventLogger.LogDebug("CreateInstanceFromScope(): scope → {Scope}", scope);

            HttpContextHelper.RegisterMockHttpContext();
            HttpContextSource.Current.SetScope(scope);

            var provider = new WebFormsServiceProvider();
			eventLogger.LogDebug("CreateInstanceFromScope(): provider → {Provider}", provider);

            // Act
            var instance = provider.GetService(typeof(IScopedService));
			eventLogger.LogDebug("CreateInstanceFromScope(): instance → {Instance}", instance);

            // Assert
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.EqualTo(scope.ServiceProvider.GetRequiredService<IScopedService>()));
			eventLogger.LogDebug("<<< Exiting CreateInstanceFromScope()");
        }

        [Test]
        public void CreateInstanceWithProtectedConstructor()
        {
			eventLogger.LogDebug(">>> Entering CreateInstanceWithProtectedConstructor()");
            // Arrange
            var scope = Globals.DependencyProvider.CreateScope();
			eventLogger.LogDebug("CreateInstanceWithProtectedConstructor(): scope → {Scope}", scope);

            HttpContextHelper.RegisterMockHttpContext();
            HttpContextSource.Current.SetScope(scope);

            var provider = new WebFormsServiceProvider();
			eventLogger.LogDebug("CreateInstanceWithProtectedConstructor(): provider → {Provider}", provider);

            // Act
            var instance = provider.GetService(typeof(PageHandlerFactory));
			eventLogger.LogDebug("CreateInstanceWithProtectedConstructor(): instance → {Instance}", instance);

            // Assert
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.InstanceOf<PageHandlerFactory>());
			eventLogger.LogDebug("<<< Exiting CreateInstanceWithProtectedConstructor()");
        }

        [Test]
        public void CreateModuleWithSingletonService()
        {
			eventLogger.LogDebug(">>> Entering CreateModuleWithSingletonService()");
            // Arrange
            var scope = Globals.DependencyProvider.CreateScope();
			eventLogger.LogDebug("CreateModuleWithSingletonService(): scope → {Scope}", scope);

            HttpContextHelper.RegisterMockHttpContext();
            HttpContextSource.Current.SetScope(scope);

            var provider = new WebFormsServiceProvider();
			eventLogger.LogDebug("CreateModuleWithSingletonService(): provider → {Provider}", provider);
            var service = Globals.DependencyProvider.GetRequiredService<SingletonService>();
			eventLogger.LogDebug("CreateModuleWithSingletonService(): service → {Service}", service);

            // Act
            var module = provider.GetService<TestModule<SingletonService>>();
			eventLogger.LogDebug("CreateModuleWithSingletonService(): module → {Module}", module);

            // Assert
            Assert.That(module, Is.Not.Null);
            Assert.That(module.DependencyService, Is.EqualTo(module.ConstructorService));
            Assert.That(service, Is.EqualTo(module.ConstructorService));
			eventLogger.LogDebug("<<< Exiting CreateModuleWithSingletonService()");
        }

        [Test]
        public void CreateModuleWithScopedService()
        {
			eventLogger.LogDebug(">>> Entering CreateModuleWithScopedService()");
            // Arrange
            var scope = Globals.DependencyProvider.CreateScope();
			eventLogger.LogDebug("CreateModuleWithScopedService(): scope → {Scope}", scope);

            HttpContextHelper.RegisterMockHttpContext();
            HttpContextSource.Current.SetScope(scope);

            var provider = new WebFormsServiceProvider();
			eventLogger.LogDebug("CreateModuleWithScopedService(): provider → {Provider}", provider);
            var serviceFromRequestScope = scope.ServiceProvider.GetRequiredService<IScopedService>();
			eventLogger.LogDebug("CreateModuleWithScopedService(): serviceFromRequestScope → {ServiceFromRequestScope}", serviceFromRequestScope);
            var serviceFromGlobalScope = Globals.DependencyProvider.GetRequiredService<IScopedService>();
			eventLogger.LogDebug("CreateModuleWithScopedService(): serviceFromGlobalScope → {ServiceFromGlobalScope}", serviceFromGlobalScope);

            // Act
            var module = provider.GetService<TestModule<IScopedService>>();
			eventLogger.LogDebug("CreateModuleWithScopedService(): module → {Module}", module);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(module, Is.Not.Null);
                Assert.That(module.DependencyService, Is.EqualTo(module.ConstructorService));
                Assert.That(serviceFromRequestScope, Is.EqualTo(module.ConstructorService));
                Assert.That(serviceFromGlobalScope, Is.Not.EqualTo(module.ConstructorService));
            }
			eventLogger.LogDebug("<<< Exiting CreateModuleWithScopedService()");
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
				eventLogger.LogDebug("<<< Exiting TestModule(service={Service})", service);
            }

            public T ConstructorService { get; }

            public T DependencyService { get; }
        }
    }
}
