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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
			Logger.LogDebug(">>> Entering FixtureSetUp()");
            var serviceCollection = new ServiceCollection();

			Logger.LogDebug("FixtureSetUp(): serviceCollection → {ServiceCollection}", serviceCollection);
            serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
            serviceCollection.AddTransient<IApplicationStatusInfo>(container => new DotNetNuke.Application.ApplicationStatusInfo(Mock.Of<IApplicationInfo>()));

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
        public void CreateScopeWhenRequestDoesNotExists()
        {
			Logger.LogDebug(">>> Entering CreateScopeWhenRequestDoesNotExists()");
            Assert.That(HttpContextSource.Current?.GetScope(), Is.Null);

            var container = ServiceScopeContainer.GetRequestOrCreateScope();

			Logger.LogDebug("CreateScopeWhenRequestDoesNotExists(): container → {Container}", container);
            Assert.That(container.ShouldDispose, Is.True);
			Logger.LogDebug("<<< Exiting CreateScopeWhenRequestDoesNotExists()");
        }

        [Test]
        public void UseRequestScopeWhenPossible()
        {
			Logger.LogDebug(">>> Entering UseRequestScopeWhenPossible()");
            var scope = Globals.DependencyProvider.CreateScope();

			Logger.LogDebug("UseRequestScopeWhenPossible(): scope → {Scope}", scope);
            HttpContextHelper.RegisterMockHttpContext();
            HttpContextSource.Current.SetScope(scope);

            Assert.That(HttpContextSource.Current?.GetScope(), Is.Not.Null);

            var container = ServiceScopeContainer.GetRequestOrCreateScope();

			Logger.LogDebug("UseRequestScopeWhenPossible(): container → {Container}", container);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(container.ShouldDispose, Is.False);
                Assert.That(container.ServiceScope, Is.EqualTo(scope));
            }
			Logger.LogDebug("<<< Exiting UseRequestScopeWhenPossible()");
        }
    }
}
