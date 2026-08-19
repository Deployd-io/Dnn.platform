// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api.Internals
{
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Tests.Utilities;
    using Moq;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class ServiceContainerTests
    {
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
			eventLogger.LogDebug(">>> Entering FixtureSetUp()");
            var serviceCollection = new ServiceCollection();

			eventLogger.LogDebug("FixtureSetUp(): serviceCollection → {ServiceCollection}", serviceCollection);
            serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
            serviceCollection.AddTransient<IApplicationStatusInfo>(container => new DotNetNuke.Application.ApplicationStatusInfo(Mock.Of<IApplicationInfo>()));

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
        public void CreateScopeWhenRequestDoesNotExists()
        {
			eventLogger.LogDebug(">>> Entering CreateScopeWhenRequestDoesNotExists()");
            Assert.That(HttpContextSource.Current?.GetScope(), Is.Null);

            var container = ServiceScopeContainer.GetRequestOrCreateScope();
			eventLogger.LogDebug("CreateScopeWhenRequestDoesNotExists(): container → {Container}", container);

            Assert.That(container.ShouldDispose, Is.True);
			eventLogger.LogDebug("<<< Exiting CreateScopeWhenRequestDoesNotExists()");
        }

        [Test]
        public void UseRequestScopeWhenPossible()
        {
			eventLogger.LogDebug(">>> Entering UseRequestScopeWhenPossible()");
            var scope = Globals.DependencyProvider.CreateScope();

			eventLogger.LogDebug("UseRequestScopeWhenPossible(): scope → {Scope}", scope);
            HttpContextHelper.RegisterMockHttpContext();
            HttpContextSource.Current.SetScope(scope);

            Assert.That(HttpContextSource.Current?.GetScope(), Is.Not.Null);

            var container = ServiceScopeContainer.GetRequestOrCreateScope();
			eventLogger.LogDebug("UseRequestScopeWhenPossible(): container → {Container}", container);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(container.ShouldDispose, Is.False);
                Assert.That(container.ServiceScope, Is.EqualTo(scope));
            }
			eventLogger.LogDebug("<<< Exiting UseRequestScopeWhenPossible()");
        }
    }
}
