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
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class PurgePageUnitTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private Mock<ITabController> tabControllerMock;
        private Mock<IRecyclebinController> recycleBinControllerMock;
        private Mock<IContentVerifier> contentVerifierMock;
        private Mock<IPortalController> portalControllerMock;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void RunBeforeAnyTest()
        {
			_logger.LogDebug(">>> Entering RunBeforeAnyTest()");
            this.tabControllerMock = new Mock<ITabController>();
            this.recycleBinControllerMock = new Mock<IRecyclebinController>();
			_logger.LogDebug("RunBeforeAnyTest(): recycleBinControllerMock → {RecycleBinControllerMock}", recycleBinControllerMock);
			_logger.LogDebug("RunBeforeAnyTest(): tabControllerMock → {TabControllerMock}", tabControllerMock);
            this.contentVerifierMock = new Mock<IContentVerifier>();
			_logger.LogDebug("RunBeforeAnyTest(): contentVerifierMock → {ContentVerifierMock}", contentVerifierMock);
            this.portalControllerMock = new Mock<IPortalController>();
			_logger.LogDebug("RunBeforeAnyTest(): portalControllerMock → {PortalControllerMock}", portalControllerMock);

            LocalizationProvider.SetTestableInstance(Mock.Of<ILocalizationProvider>());

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					_logger.LogDebug("RunBeforeAnyTest(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(this.tabControllerMock.Object);
                    services.AddSingleton(this.recycleBinControllerMock.Object);
                    services.AddSingleton(this.contentVerifierMock.Object);
                    services.AddSingleton(this.portalControllerMock.Object);
                });
				_logger.LogDebug("<<< Exiting RunBeforeAnyTest()");
        }

        [TearDown]
        public void TearDown()
        {
			_logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            LocalizationProvider.ClearInstance();
			_logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse()
        {
			_logger.LogDebug(">>> Entering Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse()");
            // Arrange
            int tabId = 91;
            int testPortalId = 1;
            TabInfo tab = new TabInfo();
			_logger.LogDebug("Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse(): tab → {Tab}", tab);
			_logger.LogDebug("Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse(): tab.TabID → {Arg0}", tab.TabID);
			_logger.LogDebug("Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse(): tab.PortalID → {Arg0}", tab.PortalID);
            tab.TabID = tabId;
            tab.PortalID = testPortalId;
            PortalSettings portalSettings = new PortalSettings();
			_logger.LogDebug("Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse(): portalSettings → {PortalSettings}", portalSettings);
			_logger.LogDebug("Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse(): portalSettings.PortalId → {Arg0}", portalSettings.PortalId);
            portalSettings.PortalId = testPortalId;

            this.tabControllerMock.Setup(t => t.GetTab(tabId, testPortalId)).Returns(tab);
            this.contentVerifierMock.Setup(p => p.IsContentExistsForRequestedPortal(testPortalId, portalSettings, It.IsAny<bool>())).Returns(true);

            var purgeCommand = new PurgePage(this.tabControllerMock.Object, this.recycleBinControllerMock.Object, this.contentVerifierMock.Object);
			_logger.LogDebug("Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse(): purgeCommand → {PurgeCommand}", purgeCommand);

            var args = new[] { "purge-page", tabId.ToString() };
			_logger.LogDebug("Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse(): args → {Args}", args);
            purgeCommand.Initialize(args, portalSettings, null, 0);

            // Act
            var result = purgeCommand.Run();
			_logger.LogDebug("Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.False);
			_logger.LogDebug("<<< Exiting Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse()");
        }

        [Test]
        public void Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse()
        {
			_logger.LogDebug(">>> Entering Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse()");
            // Arrange
            int tabId = 91;
            int portalId = 1;
            TabInfo tab = new TabInfo();
			_logger.LogDebug("Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse(): tab → {Tab}", tab);
			_logger.LogDebug("Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse(): tab.TabID → {Arg0}", tab.TabID);
			_logger.LogDebug("Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse(): tab.PortalID → {Arg0}", tab.PortalID);
            tab.TabID = tabId;
            tab.PortalID = portalId;
            PortalSettings portalSettings = new PortalSettings();
			_logger.LogDebug("Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse(): portalSettings → {PortalSettings}", portalSettings);
			_logger.LogDebug("Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse(): portalSettings.PortalId → {Arg0}", portalSettings.PortalId);
            portalSettings.PortalId = portalId;

            this.tabControllerMock.Setup(t => t.GetTab(tabId, portalId)).Returns(tab);
            this.contentVerifierMock.Setup(p => p.IsContentExistsForRequestedPortal(portalId, portalSettings, It.IsAny<bool>())).Returns(false);

            var purgeCommand = new PurgePage(this.tabControllerMock.Object, this.recycleBinControllerMock.Object, this.contentVerifierMock.Object);
			_logger.LogDebug("Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse(): purgeCommand → {PurgeCommand}", purgeCommand);

            var args = new[] { "purge-page", tabId.ToString() };
			_logger.LogDebug("Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse(): args → {Args}", args);
            purgeCommand.Initialize(args, portalSettings, null, 0);

            // Act
            var result = purgeCommand.Run();
			_logger.LogDebug("Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.True);
			_logger.LogDebug("<<< Exiting Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse()");
        }

        [Test]
        public void Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse()
        {
			_logger.LogDebug(">>> Entering Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse()");
            // Arrange
            int tabId = 919;
            PortalSettings portalSettings = new PortalSettings();
			_logger.LogDebug("Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse(): portalSettings → {PortalSettings}", portalSettings);

            var purgeCommand = new PurgePage(this.tabControllerMock.Object, this.recycleBinControllerMock.Object, this.contentVerifierMock.Object);
			_logger.LogDebug("Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse(): purgeCommand → {PurgeCommand}", purgeCommand);

            var args = new[] { "purge-page", tabId.ToString() };
			_logger.LogDebug("Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse(): args → {Args}", args);
            purgeCommand.Initialize(args, portalSettings, null, 0);

            // Act
            var result = purgeCommand.Run();
			_logger.LogDebug("Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.True);
            this.tabControllerMock.Verify(t => t.GetTab(tabId, portalSettings.PortalId));
			_logger.LogDebug("<<< Exiting Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse()");
        }
    }
}
