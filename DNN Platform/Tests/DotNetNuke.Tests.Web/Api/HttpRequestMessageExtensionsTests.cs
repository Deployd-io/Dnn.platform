// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Hosting;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Web.Api;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class HttpRequestMessageExtensionsTests
    {
        [Test]
        public void FindTabIdTriesAllProviders()
        {
			eventLogger.LogDebug(">>> Entering FindTabIdTriesAllProviders()");
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("FindTabIdTriesAllProviders(): request → {Request}", request);
            var configuration = new HttpConfiguration();
			eventLogger.LogDebug("FindTabIdTriesAllProviders(): configuration → {Configuration}", configuration);
            var provider = new Mock<ITabAndModuleInfoProvider>();
			eventLogger.LogDebug("FindTabIdTriesAllProviders(): provider → {Provider}", provider);
            int i;
            provider.Setup(x => x.TryFindTabId(request, out i)).Returns(false);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            // Act
            request.FindTabId();

            // Assert
            provider.Verify(x => x.TryFindTabId(request, out i), Times.Exactly(2));
			eventLogger.LogDebug("<<< Exiting FindTabIdTriesAllProviders()");
        }

        [Test]
        public void FindTabIdStopCallingProvidersAfterOneSuccess()
        {
			eventLogger.LogDebug(">>> Entering FindTabIdStopCallingProvidersAfterOneSuccess()");
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("FindTabIdStopCallingProvidersAfterOneSuccess(): request → {Request}", request);
            var configuration = new HttpConfiguration();
			eventLogger.LogDebug("FindTabIdStopCallingProvidersAfterOneSuccess(): configuration → {Configuration}", configuration);
            var provider = new Mock<ITabAndModuleInfoProvider>();
			eventLogger.LogDebug("FindTabIdStopCallingProvidersAfterOneSuccess(): provider → {Provider}", provider);
            int i;
            provider.Setup(x => x.TryFindTabId(request, out i)).Returns(true);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            // Act
            request.FindTabId();

            // Assert
            provider.Verify(x => x.TryFindTabId(request, out i), Times.Once());
			eventLogger.LogDebug("<<< Exiting FindTabIdStopCallingProvidersAfterOneSuccess()");
        }

        [Test]
        public void FindModuleIdTriesAllProviders()
        {
			eventLogger.LogDebug(">>> Entering FindModuleIdTriesAllProviders()");
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("FindModuleIdTriesAllProviders(): request → {Request}", request);
            var configuration = new HttpConfiguration();
			eventLogger.LogDebug("FindModuleIdTriesAllProviders(): configuration → {Configuration}", configuration);
            var provider = new Mock<ITabAndModuleInfoProvider>();
			eventLogger.LogDebug("FindModuleIdTriesAllProviders(): provider → {Provider}", provider);
            int i;
            provider.Setup(x => x.TryFindModuleId(request, out i)).Returns(false);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            // Act
            request.FindModuleId();

            // Assert
            provider.Verify(x => x.TryFindModuleId(request, out i), Times.Exactly(2));
			eventLogger.LogDebug("<<< Exiting FindModuleIdTriesAllProviders()");
        }

        [Test]
        public void FindModuleIdStopCallingProvidersAfterOneSuccess()
        {
			eventLogger.LogDebug(">>> Entering FindModuleIdStopCallingProvidersAfterOneSuccess()");
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("FindModuleIdStopCallingProvidersAfterOneSuccess(): request → {Request}", request);
            var configuration = new HttpConfiguration();
			eventLogger.LogDebug("FindModuleIdStopCallingProvidersAfterOneSuccess(): configuration → {Configuration}", configuration);
            var provider = new Mock<ITabAndModuleInfoProvider>();
			eventLogger.LogDebug("FindModuleIdStopCallingProvidersAfterOneSuccess(): provider → {Provider}", provider);
            int i;
            provider.Setup(x => x.TryFindModuleId(request, out i)).Returns(true);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            // Act
            request.FindModuleId();

            // Assert
            provider.Verify(x => x.TryFindModuleId(request, out i), Times.Once());
			eventLogger.LogDebug("<<< Exiting FindModuleIdStopCallingProvidersAfterOneSuccess()");
        }

        [Test]
        public void FindModuleInfoTriesAllProviders()
        {
			eventLogger.LogDebug(">>> Entering FindModuleInfoTriesAllProviders()");
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("FindModuleInfoTriesAllProviders(): request → {Request}", request);
            var configuration = new HttpConfiguration();
			eventLogger.LogDebug("FindModuleInfoTriesAllProviders(): configuration → {Configuration}", configuration);
            var provider = new Mock<ITabAndModuleInfoProvider>();
			eventLogger.LogDebug("FindModuleInfoTriesAllProviders(): provider → {Provider}", provider);
            ModuleInfo moduleInfo;
            provider.Setup(x => x.TryFindModuleInfo(request, out moduleInfo)).Returns(false);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            // Act
            request.FindModuleInfo();

            // Assert
            provider.Verify(x => x.TryFindModuleInfo(request, out moduleInfo), Times.Exactly(2));
			eventLogger.LogDebug("<<< Exiting FindModuleInfoTriesAllProviders()");
        }

        [Test]
        public void FindModuleInfoStopCallingProvidersAfterOneSuccess()
        {
			eventLogger.LogDebug(">>> Entering FindModuleInfoStopCallingProvidersAfterOneSuccess()");
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("FindModuleInfoStopCallingProvidersAfterOneSuccess(): request → {Request}", request);
            var configuration = new HttpConfiguration();
			eventLogger.LogDebug("FindModuleInfoStopCallingProvidersAfterOneSuccess(): configuration → {Configuration}", configuration);
            var provider = new Mock<ITabAndModuleInfoProvider>();
			eventLogger.LogDebug("FindModuleInfoStopCallingProvidersAfterOneSuccess(): provider → {Provider}", provider);
            ModuleInfo moduleInfo;
            provider.Setup(x => x.TryFindModuleInfo(request, out moduleInfo)).Returns(true);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            // Act
            request.FindModuleInfo();

            // Assert
            provider.Verify(x => x.TryFindModuleInfo(request, out moduleInfo), Times.Once());
			eventLogger.LogDebug("<<< Exiting FindModuleInfoStopCallingProvidersAfterOneSuccess()");
        }
    }
}
