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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        public void GetTabAndModuleInfoProvidersReturnsEmptyWhenNoProvidersAdded()
        {
			Logger.LogDebug(">>> Entering GetTabAndModuleInfoProvidersReturnsEmptyWhenNoProvidersAdded()");
            // Arrange
            var configuration = new HttpConfiguration();

			Logger.LogDebug("GetTabAndModuleInfoProvidersReturnsEmptyWhenNoProvidersAdded(): configuration → {Configuration}", configuration);
            // Act
            var providers = configuration.GetTabAndModuleInfoProviders();

			Logger.LogDebug("GetTabAndModuleInfoProvidersReturnsEmptyWhenNoProvidersAdded(): providers → {Providers}", providers);
            // Assert
            Assert.That(providers, Is.Empty);
			Logger.LogDebug("<<< Exiting GetTabAndModuleInfoProvidersReturnsEmptyWhenNoProvidersAdded()");
        }

        [Test]
        public void AddTabAndModuleInfoProviderWorksForFirstProvider()
        {
			Logger.LogDebug(">>> Entering AddTabAndModuleInfoProviderWorksForFirstProvider()");
            // Arrange
            var configuration = new HttpConfiguration();

			Logger.LogDebug("AddTabAndModuleInfoProviderWorksForFirstProvider(): configuration → {Configuration}", configuration);
            // Act
            configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());

            // Assert
            Assert.That(((IEnumerable<ITabAndModuleInfoProvider>)configuration.Properties["TabAndModuleInfoProvider"]).Count(), Is.EqualTo(1));
			Logger.LogDebug("<<< Exiting AddTabAndModuleInfoProviderWorksForFirstProvider()");
        }

        [Test]
        public void AddTabAndModuleInfoProviderWorksForManyProviders()
        {
			Logger.LogDebug(">>> Entering AddTabAndModuleInfoProviderWorksForManyProviders()");
            // Arrange
            var configuration = new HttpConfiguration();

			Logger.LogDebug("AddTabAndModuleInfoProviderWorksForManyProviders(): configuration → {Configuration}", configuration);
            // Act
            configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());
            configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());
            configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());

            // Assert
            Assert.That(((IEnumerable<ITabAndModuleInfoProvider>)configuration.Properties["TabAndModuleInfoProvider"]).Count(), Is.EqualTo(3));
			Logger.LogDebug("<<< Exiting AddTabAndModuleInfoProviderWorksForManyProviders()");
        }
    }
}
