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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        public void FindTabIdTriesAllProviders()
        {
			Logger.LogDebug(">>> Entering FindTabIdTriesAllProviders()");
            // Arrange
            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
			Logger.LogDebug("FindTabIdTriesAllProviders(): request → {Request}", request);
			Logger.LogDebug("FindTabIdTriesAllProviders(): configuration → {Configuration}", configuration);
            var provider = new Mock<ITabAndModuleInfoProvider>();
            int i;
			Logger.LogDebug("FindTabIdTriesAllProviders(): provider → {Provider}", provider);
            provider.Setup(x => x.TryFindTabId(request, out i)).Returns(false);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            // Act
            request.FindTabId();

            // Assert
            provider.Verify(x => x.TryFindTabId(request, out i), Times.Exactly(2));
			Logger.LogDebug("<<< Exiting FindTabIdTriesAllProviders()");
        }

        [Test]
        public void FindTabIdStopCallingProvidersAfterOneSuccess()
        {
			Logger.LogDebug(">>> Entering FindTabIdStopCallingProvidersAfterOneSuccess()");
            // Arrange
            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
			Logger.LogDebug("FindTabIdStopCallingProvidersAfterOneSuccess(): request → {Request}", request);
			Logger.LogDebug("FindTabIdStopCallingProvidersAfterOneSuccess(): configuration → {Configuration}", configuration);
            var provider = new Mock<ITabAndModuleInfoProvider>();
            int i;
			Logger.LogDebug("FindTabIdStopCallingProvidersAfterOneSuccess(): provider → {Provider}", provider);
            provider.Setup(x => x.TryFindTabId(request, out i)).Returns(true);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            // Act
            request.FindTabId();

            // Assert
            provider.Verify(x => x.TryFindTabId(request, out i), Times.Once());
			Logger.LogDebug("<<< Exiting FindTabIdStopCallingProvidersAfterOneSuccess()");
        }

        [Test]
        public void FindModuleIdTriesAllProviders()
        {
			Logger.LogDebug(">>> Entering FindModuleIdTriesAllProviders()");
            // Arrange
            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
			Logger.LogDebug("FindModuleIdTriesAllProviders(): request → {Request}", request);
			Logger.LogDebug("FindModuleIdTriesAllProviders(): configuration → {Configuration}", configuration);
            var provider = new Mock<ITabAndModuleInfoProvider>();
            int i;
			Logger.LogDebug("FindModuleIdTriesAllProviders(): provider → {Provider}", provider);
            provider.Setup(x => x.TryFindModuleId(request, out i)).Returns(false);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            // Act
            request.FindModuleId();

            // Assert
            provider.Verify(x => x.TryFindModuleId(request, out i), Times.Exactly(2));
			Logger.LogDebug("<<< Exiting FindModuleIdTriesAllProviders()");
        }

        [Test]
        public void FindModuleIdStopCallingProvidersAfterOneSuccess()
        {
			Logger.LogDebug(">>> Entering FindModuleIdStopCallingProvidersAfterOneSuccess()");
            // Arrange
            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
			Logger.LogDebug("FindModuleIdStopCallingProvidersAfterOneSuccess(): request → {Request}", request);
			Logger.LogDebug("FindModuleIdStopCallingProvidersAfterOneSuccess(): configuration → {Configuration}", configuration);
            var provider = new Mock<ITabAndModuleInfoProvider>();
            int i;
			Logger.LogDebug("FindModuleIdStopCallingProvidersAfterOneSuccess(): provider → {Provider}", provider);
            provider.Setup(x => x.TryFindModuleId(request, out i)).Returns(true);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            // Act
            request.FindModuleId();

            // Assert
            provider.Verify(x => x.TryFindModuleId(request, out i), Times.Once());
			Logger.LogDebug("<<< Exiting FindModuleIdStopCallingProvidersAfterOneSuccess()");
        }

        [Test]
        public void FindModuleInfoTriesAllProviders()
        {
			Logger.LogDebug(">>> Entering FindModuleInfoTriesAllProviders()");
            // Arrange
            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
			Logger.LogDebug("FindModuleInfoTriesAllProviders(): request → {Request}", request);
			Logger.LogDebug("FindModuleInfoTriesAllProviders(): configuration → {Configuration}", configuration);
            var provider = new Mock<ITabAndModuleInfoProvider>();
            ModuleInfo moduleInfo;
			Logger.LogDebug("FindModuleInfoTriesAllProviders(): provider → {Provider}", provider);
            provider.Setup(x => x.TryFindModuleInfo(request, out moduleInfo)).Returns(false);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            // Act
            request.FindModuleInfo();

            // Assert
            provider.Verify(x => x.TryFindModuleInfo(request, out moduleInfo), Times.Exactly(2));
			Logger.LogDebug("<<< Exiting FindModuleInfoTriesAllProviders()");
        }

        [Test]
        public void FindModuleInfoStopCallingProvidersAfterOneSuccess()
        {
			Logger.LogDebug(">>> Entering FindModuleInfoStopCallingProvidersAfterOneSuccess()");
            // Arrange
            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
			Logger.LogDebug("FindModuleInfoStopCallingProvidersAfterOneSuccess(): request → {Request}", request);
			Logger.LogDebug("FindModuleInfoStopCallingProvidersAfterOneSuccess(): configuration → {Configuration}", configuration);
            var provider = new Mock<ITabAndModuleInfoProvider>();
            ModuleInfo moduleInfo;
			Logger.LogDebug("FindModuleInfoStopCallingProvidersAfterOneSuccess(): provider → {Provider}", provider);
            provider.Setup(x => x.TryFindModuleInfo(request, out moduleInfo)).Returns(true);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            // Act
            request.FindModuleInfo();

            // Assert
            provider.Verify(x => x.TryFindModuleInfo(request, out moduleInfo), Times.Once());
			Logger.LogDebug("<<< Exiting FindModuleInfoStopCallingProvidersAfterOneSuccess()");
        }
    }
}
