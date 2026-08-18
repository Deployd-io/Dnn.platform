// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;

    using DotNetNuke.Web.Api;
    using NUnit.Framework;

    [TestFixture]
    public class HttpConfigurationExtensionTests
    {
        [Test]
        public void GetTabAndModuleInfoProvidersReturnsEmptyWhenNoProvidersAdded()
        {
			eventLogger.LogDebug(">>> Entering GetTabAndModuleInfoProvidersReturnsEmptyWhenNoProvidersAdded()");
            // Arrange
            var configuration = new HttpConfiguration();
			eventLogger.LogDebug("GetTabAndModuleInfoProvidersReturnsEmptyWhenNoProvidersAdded(): configuration → {Configuration}", configuration);

            // Act
            var providers = configuration.GetTabAndModuleInfoProviders();
			eventLogger.LogDebug("GetTabAndModuleInfoProvidersReturnsEmptyWhenNoProvidersAdded(): providers → {Providers}", providers);

            // Assert
            Assert.That(providers, Is.Empty);
			eventLogger.LogDebug("<<< Exiting GetTabAndModuleInfoProvidersReturnsEmptyWhenNoProvidersAdded()");
        }

        [Test]
        public void AddTabAndModuleInfoProviderWorksForFirstProvider()
        {
			eventLogger.LogDebug(">>> Entering AddTabAndModuleInfoProviderWorksForFirstProvider()");
            // Arrange
            var configuration = new HttpConfiguration();
			eventLogger.LogDebug("AddTabAndModuleInfoProviderWorksForFirstProvider(): configuration → {Configuration}", configuration);

            // Act
            configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());

            // Assert
            Assert.That(((IEnumerable<ITabAndModuleInfoProvider>)configuration.Properties["TabAndModuleInfoProvider"]).Count(), Is.EqualTo(1));
			eventLogger.LogDebug("<<< Exiting AddTabAndModuleInfoProviderWorksForFirstProvider()");
        }

        [Test]
        public void AddTabAndModuleInfoProviderWorksForManyProviders()
        {
			eventLogger.LogDebug(">>> Entering AddTabAndModuleInfoProviderWorksForManyProviders()");
            // Arrange
            var configuration = new HttpConfiguration();
			eventLogger.LogDebug("AddTabAndModuleInfoProviderWorksForManyProviders(): configuration → {Configuration}", configuration);

            // Act
            configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());
            configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());
            configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());

            // Assert
            Assert.That(((IEnumerable<ITabAndModuleInfoProvider>)configuration.Properties["TabAndModuleInfoProvider"]).Count(), Is.EqualTo(3));
			eventLogger.LogDebug("<<< Exiting AddTabAndModuleInfoProviderWorksForManyProviders()");
        }
    }
}
