// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Hosting;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Web.Api;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DnnApiControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            PortalController.ClearInstance();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void GetsModuleInfoViaTheTabModuleInfoProviders()
        {
			Logger.LogDebug(">>> Entering GetsModuleInfoViaTheTabModuleInfoProviders()");
            // Arrange
            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
			Logger.LogDebug("GetsModuleInfoViaTheTabModuleInfoProviders(): request → {Request}", request);
			Logger.LogDebug("GetsModuleInfoViaTheTabModuleInfoProviders(): configuration → {Configuration}", configuration);
            var provider = new Mock<ITabAndModuleInfoProvider>();
            var expectedModule = new ModuleInfo();
			Logger.LogDebug("GetsModuleInfoViaTheTabModuleInfoProviders(): provider → {Provider}", provider);
			Logger.LogDebug("GetsModuleInfoViaTheTabModuleInfoProviders(): expectedModule → {ExpectedModule}", expectedModule);
            provider.Setup(x => x.TryFindModuleInfo(request, out expectedModule)).Returns(true);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            var controller = new DnnApiControllerHelper { Request = request };

            // Act
            var result = controller.ActiveModule;

			Logger.LogDebug("GetsModuleInfoViaTheTabModuleInfoProviders(): result → {Result}", result);
            // Assert
            Assert.That(result, Is.EqualTo(expectedModule));
            provider.Verify(x => x.TryFindModuleInfo(request, out expectedModule), Times.Once());
			Logger.LogDebug("<<< Exiting GetsModuleInfoViaTheTabModuleInfoProviders()");
        }

        [Test]
        public void GetsPortalSettingsViaTestablePortalController()
        {
			Logger.LogDebug(">>> Entering GetsPortalSettingsViaTestablePortalController()");
            // Arrange
            var controller = new DnnApiControllerHelper();
            var mockPortalController = new Mock<IPortalController>();
			Logger.LogDebug("GetsPortalSettingsViaTestablePortalController(): controller → {Controller}", controller);
			Logger.LogDebug("GetsPortalSettingsViaTestablePortalController(): mockPortalController → {MockPortalController}", mockPortalController);
            var expectedPortalSettings = new PortalSettings();
            mockPortalController.Setup(x => x.GetCurrentPortalSettings()).Returns(expectedPortalSettings);
			Logger.LogDebug("GetsPortalSettingsViaTestablePortalController(): expectedPortalSettings → {ExpectedPortalSettings}", expectedPortalSettings);
            PortalController.SetTestableInstance(mockPortalController.Object);

            // Act
            var result = controller.PortalSettings;

			Logger.LogDebug("GetsPortalSettingsViaTestablePortalController(): result → {Result}", result);
            // Assert
            mockPortalController.Verify(x => x.GetCurrentPortalSettings(), Times.Once());
            Assert.That(result, Is.EqualTo(expectedPortalSettings));
			Logger.LogDebug("<<< Exiting GetsPortalSettingsViaTestablePortalController()");
        }

        internal class DnnApiControllerHelper : DnnApiController
        {
        }

        // A test that would be nice to run, but I see not good way to test the source of the
        // userinfo
        //        [Test]
        //        public void UserInfoComesFromPortalSettings()
        //        {
        //            //Arrange
        //            var controller = new DnnApiControllerHelper();
        //            var mockPortalController = new Mock<IPortalController>();
        //            var expectedPortalSettings = new PortalSettings();
        // expectedPortalSettings.UserInfo = ??????
        //            mockPortalController.Setup(x => x.GetCurrentPortalSettings()).Returns(expectedPortalSettings);
        //            TestablePortalController.SetTestableInstance(mockPortalController.Object);
        //
        //            //Act
        //            var result = controller.PortalSettings;
        //
        //            //Assert
        //            mockPortalController.Verify(x => x.GetCurrentPortalSettings(), Times.Once());
        //            Assert.AreEqual(expectedPortalSettings, result);
        //        }
    }
}
