// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Tests
{
    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Recyclebin.Components;
    using Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PurgePageUnitTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private Mock<ITabController> tabControllerMock;
        private Mock<IRecyclebinController> recycleBinControllerMock;
        private Mock<IContentVerifier> contentVerifierMock;
        private Mock<IPortalController> portalControllerMock;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void RunBeforeAnyTest()
        {
			Logger.LogDebug(">>> Entering RunBeforeAnyTest()");
            this.tabControllerMock = new Mock<ITabController>();
            this.recycleBinControllerMock = new Mock<IRecyclebinController>();
			Logger.LogDebug("RunBeforeAnyTest(): recycleBinControllerMock → {RecycleBinControllerMock}", recycleBinControllerMock);
			Logger.LogDebug("RunBeforeAnyTest(): tabControllerMock → {TabControllerMock}", tabControllerMock);
            this.contentVerifierMock = new Mock<IContentVerifier>();
			Logger.LogDebug("RunBeforeAnyTest(): contentVerifierMock → {ContentVerifierMock}", contentVerifierMock);
            this.portalControllerMock = new Mock<IPortalController>();
			Logger.LogDebug("RunBeforeAnyTest(): portalControllerMock → {PortalControllerMock}", portalControllerMock);

            LocalizationProvider.SetTestableInstance(Mock.Of<ILocalizationProvider>());

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("RunBeforeAnyTest(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("RunBeforeAnyTest(): services → {Services}", services);
                    services.AddSingleton(this.tabControllerMock.Object);
                    services.AddSingleton(this.recycleBinControllerMock.Object);
                    services.AddSingleton(this.contentVerifierMock.Object);
                    services.AddSingleton(this.portalControllerMock.Object);
                });
				Logger.LogDebug("<<< Exiting RunBeforeAnyTest()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            LocalizationProvider.ClearInstance();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse()
        {
			Logger.LogDebug(">>> Entering Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse()");
            // Arrange
            int tabId = 91;
            int testPortalId = 1;
            TabInfo tab = new TabInfo();
            tab.TabID = tabId;
			Logger.LogDebug("Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse(): tab → {Tab}", tab);
			Logger.LogDebug("Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse(): tab.TabID → {Arg0}", tab.TabID);
			Logger.LogDebug("Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse(): tab.PortalID → {Arg0}", tab.PortalID);
            tab.PortalID = testPortalId;
            PortalSettings portalSettings = new PortalSettings();
            portalSettings.PortalId = testPortalId;
			Logger.LogDebug("Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse(): portalSettings → {PortalSettings}", portalSettings);

            this.tabControllerMock.Setup(t => t.GetTab(tabId, testPortalId)).Returns(tab);
            this.contentVerifierMock.Setup(p => p.IsContentExistsForRequestedPortal(testPortalId, portalSettings, It.IsAny<bool>())).Returns(true);

            var purgeCommand = new PurgePage(this.tabControllerMock.Object, this.recycleBinControllerMock.Object, this.contentVerifierMock.Object);

			Logger.LogDebug("Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse(): purgeCommand → {PurgeCommand}", purgeCommand);
            var args = new[] { "purge-page", tabId.ToString() };
            purgeCommand.Initialize(args, portalSettings, null, 0);
			Logger.LogDebug("Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse(): args → {Args}", args);

            // Act
            var result = purgeCommand.Run();

			Logger.LogDebug("Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.False);
			Logger.LogDebug("<<< Exiting Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse()");
        }

        [Test]
        public void Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse()
        {
			Logger.LogDebug(">>> Entering Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse()");
            // Arrange
            int tabId = 91;
            int portalId = 1;
            TabInfo tab = new TabInfo();
            tab.TabID = tabId;
			Logger.LogDebug("Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse(): tab → {Tab}", tab);
			Logger.LogDebug("Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse(): tab.TabID → {Arg0}", tab.TabID);
			Logger.LogDebug("Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse(): tab.PortalID → {Arg0}", tab.PortalID);
            tab.PortalID = portalId;
            PortalSettings portalSettings = new PortalSettings();
            portalSettings.PortalId = portalId;

            this.tabControllerMock.Setup(t => t.GetTab(tabId, portalId)).Returns(tab);
            this.contentVerifierMock.Setup(p => p.IsContentExistsForRequestedPortal(portalId, portalSettings, It.IsAny<bool>())).Returns(false);

            var purgeCommand = new PurgePage(this.tabControllerMock.Object, this.recycleBinControllerMock.Object, this.contentVerifierMock.Object);

			Logger.LogDebug("Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse(): purgeCommand → {PurgeCommand}", purgeCommand);
            var args = new[] { "purge-page", tabId.ToString() };
            purgeCommand.Initialize(args, portalSettings, null, 0);
			Logger.LogDebug("Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse(): args → {Args}", args);

            // Act
            var result = purgeCommand.Run();

			Logger.LogDebug("Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.True);
			Logger.LogDebug("<<< Exiting Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse()");
        }

        [Test]
        public void Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse()
        {
			Logger.LogDebug(">>> Entering Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse()");
            // Arrange
            int tabId = 919;
            PortalSettings portalSettings = new PortalSettings();

			Logger.LogDebug("Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse(): portalSettings → {PortalSettings}", portalSettings);
            var purgeCommand = new PurgePage(this.tabControllerMock.Object, this.recycleBinControllerMock.Object, this.contentVerifierMock.Object);

			Logger.LogDebug("Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse(): purgeCommand → {PurgeCommand}", purgeCommand);
            var args = new[] { "purge-page", tabId.ToString() };
            purgeCommand.Initialize(args, portalSettings, null, 0);
			Logger.LogDebug("Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse(): args → {Args}", args);

            // Act
            var result = purgeCommand.Run();

			Logger.LogDebug("Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.True);
            this.tabControllerMock.Verify(t => t.GetTab(tabId, portalSettings.PortalId));
			Logger.LogDebug("<<< Exiting Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse()");
        }
    }
}
