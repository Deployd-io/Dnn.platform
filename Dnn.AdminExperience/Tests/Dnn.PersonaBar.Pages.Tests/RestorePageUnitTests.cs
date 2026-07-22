// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Tests
{
    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Models;
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
    public class RestorePageUnitTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly int tabId = 91;
        private readonly int testPortalId = 1;

        private TabInfo tab;
        private PortalSettings portalSettings;
        private IConsoleCommand restorePage;

        private Mock<ITabController> tabControllerMock;
        private Mock<IRecyclebinController> recycleBinControllerMock;
        private Mock<IContentVerifier> contentVerifierMock;
        private Mock<IPortalController> portalControllerMock;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void RunBeforeAnyTest()
        {
			Logger.LogDebug(">>> Entering RunBeforeAnyTest()");
            this.tab = new TabInfo { TabID = this.tabId, PortalID = this.testPortalId, };
            this.portalSettings = new PortalSettings { PortalId = this.testPortalId, };
			Logger.LogDebug("RunBeforeAnyTest(): this.tab → {Arg0}", this.tab);
			Logger.LogDebug("RunBeforeAnyTest(): portalSettings → {PortalSettings}", portalSettings);
			Logger.LogDebug("RunBeforeAnyTest(): this.portalSettings → {Arg0}", this.portalSettings);
			Logger.LogDebug("RunBeforeAnyTest(): tab → {Tab}", tab);

            this.tabControllerMock = new Mock<ITabController>();
            this.recycleBinControllerMock = new Mock<IRecyclebinController>();
			Logger.LogDebug("RunBeforeAnyTest(): tabControllerMock → {TabControllerMock}", tabControllerMock);
			Logger.LogDebug("RunBeforeAnyTest(): recycleBinControllerMock → {RecycleBinControllerMock}", recycleBinControllerMock);
            this.contentVerifierMock = new Mock<IContentVerifier>();
            this.portalControllerMock = new Mock<IPortalController>();
			Logger.LogDebug("RunBeforeAnyTest(): contentVerifierMock → {ContentVerifierMock}", contentVerifierMock);
			Logger.LogDebug("RunBeforeAnyTest(): portalControllerMock → {PortalControllerMock}", portalControllerMock);

            LocalizationProvider.SetTestableInstance(Mock.Of<ILocalizationProvider>());

            this.tabControllerMock.SetReturnsDefault(this.tab);
            this.contentVerifierMock.SetReturnsDefault(true);
            string message;
            this.recycleBinControllerMock.Setup(r => r.RestoreTab(this.tab, out message));

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
        public void Run_RestorePage_WithValidCommand_ShouldReturnSuccessResponse()
        {
			Logger.LogDebug(">>> Entering Run_RestorePage_WithValidCommand_ShouldReturnSuccessResponse()");
            // Arrange
            this.SetupCommand();

            // Act
            var result = this.restorePage.Run();

			Logger.LogDebug("Run_RestorePage_WithValidCommand_ShouldReturnSuccessResponse(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsError, Is.False);
                Assert.That(result.Records, Is.EqualTo(1));
                Assert.That(result is ConsoleErrorResultModel, Is.False);
            }
			Logger.LogDebug("<<< Exiting Run_RestorePage_WithValidCommand_ShouldReturnSuccessResponse()");
        }

        [Test]
        public void Run_RestorePage_WithValidCommandForNonExistingTab_ShouldReturnErrorResponse()
        {
			Logger.LogDebug(">>> Entering Run_RestorePage_WithValidCommandForNonExistingTab_ShouldReturnErrorResponse()");
            // Arrange
            this.tab = null;
			Logger.LogDebug("Run_RestorePage_WithValidCommandForNonExistingTab_ShouldReturnErrorResponse(): tab → {Tab}", tab);
			Logger.LogDebug("Run_RestorePage_WithValidCommandForNonExistingTab_ShouldReturnErrorResponse(): this.tab → {Arg0}", this.tab);

            this.tabControllerMock.Setup(t => t.GetTab(this.tabId, this.testPortalId)).Returns(this.tab);

            this.SetupCommand();

            // Act
            var result = this.restorePage.Run();

			Logger.LogDebug("Run_RestorePage_WithValidCommandForNonExistingTab_ShouldReturnErrorResponse(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsError, Is.True);
                Assert.That(result is ConsoleErrorResultModel, Is.True);
            }
			Logger.LogDebug("<<< Exiting Run_RestorePage_WithValidCommandForNonExistingTab_ShouldReturnErrorResponse()");
        }

        [Test]
        public void Run_RestorePage_WithValidCommandForRequestedPortalNotAllowed_ShouldReturnErrorResponse()
        {
			Logger.LogDebug(">>> Entering Run_RestorePage_WithValidCommandForRequestedPortalNotAllowed_ShouldReturnErrorResponse()");
            // Arrange
            this.contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(this.testPortalId, this.portalSettings, false)).Returns(false);

            this.SetupCommand();

            // Act
            var result = this.restorePage.Run();

			Logger.LogDebug("Run_RestorePage_WithValidCommandForRequestedPortalNotAllowed_ShouldReturnErrorResponse(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsError, Is.True);
                Assert.That(result is ConsoleErrorResultModel, Is.True);
            }
			Logger.LogDebug("<<< Exiting Run_RestorePage_WithValidCommandForRequestedPortalNotAllowed_ShouldReturnErrorResponse()");
        }

        [Test]
        public void Run_RestorePage_WithValidCommandForNotRestoreTab_ShouldReturnErrorResponse()
        {
			Logger.LogDebug(">>> Entering Run_RestorePage_WithValidCommandForNotRestoreTab_ShouldReturnErrorResponse()");
            // Arrange
            var message = "Tab not found";
            this.recycleBinControllerMock.Setup(r => r.RestoreTab(this.tab, out message));
			Logger.LogDebug("Run_RestorePage_WithValidCommandForNotRestoreTab_ShouldReturnErrorResponse(): message → {Message}", message);

            this.SetupCommand();

            // Act
            var result = this.restorePage.Run();

			Logger.LogDebug("Run_RestorePage_WithValidCommandForNotRestoreTab_ShouldReturnErrorResponse(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsError, Is.True);
                Assert.That(result is ConsoleErrorResultModel, Is.True);
            }
			Logger.LogDebug("<<< Exiting Run_RestorePage_WithValidCommandForNotRestoreTab_ShouldReturnErrorResponse()");
        }

        private void SetupCommand()
        {
			Logger.LogDebug(">>> Entering SetupCommand()");
            this.restorePage = new RestorePage(this.tabControllerMock.Object, this.recycleBinControllerMock.Object, this.contentVerifierMock.Object);

			Logger.LogDebug("SetupCommand(): restorePage → {RestorePage}", restorePage);
            var args = new[] { "restore-page", this.tabId.ToString() };
            this.restorePage.Initialize(args, this.portalSettings, null, 0);
			Logger.LogDebug("<<< Exiting SetupCommand()");
			Logger.LogDebug("SetupCommand(): args → {Args}", args);
        }
    }
}
