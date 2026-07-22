// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Tests
{
    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Pages.Components.Prompt.Commands;
    using Dnn.PersonaBar.Pages.Components.Security;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GetPageUnitTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly int tabId = 21;
        private readonly int testPortalId = 1;

        private Mock<ITabController> tabControllerMock;
        private Mock<IContentVerifier> contentVerifierMock;
        private Mock<ISecurityService> securityServiceMock;
        private PortalSettings portalSettings;
        private IConsoleCommand getCommand;
        private TabInfo tab;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void RunBeforeAnyTest()
        {
			Logger.LogDebug(">>> Entering RunBeforeAnyTest()");
            this.tab = new TabInfo { TabID = this.tabId, PortalID = this.testPortalId, };

			Logger.LogDebug("RunBeforeAnyTest(): this.tab → {Arg0}", this.tab);
			Logger.LogDebug("RunBeforeAnyTest(): tab → {Tab}", tab);
            this.portalSettings = new PortalSettings { PortalId = this.testPortalId, };
			Logger.LogDebug("RunBeforeAnyTest(): portalSettings → {PortalSettings}", portalSettings);
			Logger.LogDebug("RunBeforeAnyTest(): this.portalSettings → {Arg0}", this.portalSettings);

            this.tabControllerMock = new Mock<ITabController>();
			Logger.LogDebug("RunBeforeAnyTest(): tabControllerMock → {TabControllerMock}", tabControllerMock);
            this.securityServiceMock = new Mock<ISecurityService>();
			Logger.LogDebug("RunBeforeAnyTest(): securityServiceMock → {SecurityServiceMock}", securityServiceMock);
            this.contentVerifierMock = new Mock<IContentVerifier>();
			Logger.LogDebug("RunBeforeAnyTest(): contentVerifierMock → {ContentVerifierMock}", contentVerifierMock);

            this.tabControllerMock.SetReturnsDefault(this.tab);
            this.securityServiceMock.SetReturnsDefault(true);
            this.contentVerifierMock.SetReturnsDefault(true);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("RunBeforeAnyTest(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("RunBeforeAnyTest(): services → {Services}", services);
                    services.AddSingleton(this.tabControllerMock.Object);
                    services.AddSingleton(this.securityServiceMock.Object);
                    services.AddSingleton(this.contentVerifierMock.Object);
                });
				Logger.LogDebug("<<< Exiting RunBeforeAnyTest()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void Run_GetPageWithValidCommand_ShouldSuccessResponse()
        {
			Logger.LogDebug(">>> Entering Run_GetPageWithValidCommand_ShouldSuccessResponse()");
            // Arrange
            this.tabControllerMock.Setup(t => t.GetTab(this.tabId, this.testPortalId)).Returns(this.tab);

            this.SetupCommand();

            // Act
            var result = this.getCommand.Run();

			Logger.LogDebug("Run_GetPageWithValidCommand_ShouldSuccessResponse(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsError, Is.False);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Records, Is.EqualTo(1));
                Assert.That(result is ConsoleErrorResultModel, Is.False);
            }
			Logger.LogDebug("<<< Exiting Run_GetPageWithValidCommand_ShouldSuccessResponse()");
        }

        [Test]
        public void Run_GetPageWithValidCommandForNonExistingTab_ShouldErrorResponse()
        {
			Logger.LogDebug(">>> Entering Run_GetPageWithValidCommandForNonExistingTab_ShouldErrorResponse()");
            // Arrange
            this.tab = null;
			Logger.LogDebug("Run_GetPageWithValidCommandForNonExistingTab_ShouldErrorResponse(): tab → {Tab}", tab);
			Logger.LogDebug("Run_GetPageWithValidCommandForNonExistingTab_ShouldErrorResponse(): this.tab → {Arg0}", this.tab);

            this.tabControllerMock.Setup(t => t.GetTab(this.tabId, this.testPortalId)).Returns(this.tab);

            this.SetupCommand();

            // Act
            var result = this.getCommand.Run();

			Logger.LogDebug("Run_GetPageWithValidCommandForNonExistingTab_ShouldErrorResponse(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsError, Is.True);
                Assert.That(result is ConsoleErrorResultModel, Is.True);
            }
			Logger.LogDebug("<<< Exiting Run_GetPageWithValidCommandForNonExistingTab_ShouldErrorResponse()");
        }

        [Test]
        public void Run_GetPageWithValidCommandForRequestedPortalNotAllowed_ShouldErrorResponse()
        {
			Logger.LogDebug(">>> Entering Run_GetPageWithValidCommandForRequestedPortalNotAllowed_ShouldErrorResponse()");
            // Arrange
            this.contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(this.testPortalId, this.portalSettings, false)).Returns(false);

            this.SetupCommand();

            // Act
            var result = this.getCommand.Run();

			Logger.LogDebug("Run_GetPageWithValidCommandForRequestedPortalNotAllowed_ShouldErrorResponse(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsError, Is.True);
                Assert.That(result is ConsoleErrorResultModel, Is.True);
            }
			Logger.LogDebug("<<< Exiting Run_GetPageWithValidCommandForRequestedPortalNotAllowed_ShouldErrorResponse()");
        }

        [Test]
        public void Run_GetPageWithValidCommandForPortalNotAllowed_ShouldErrorResponse()
        {
			Logger.LogDebug(">>> Entering Run_GetPageWithValidCommandForPortalNotAllowed_ShouldErrorResponse()");
            // Arrange
            this.securityServiceMock.Setup(s => s.CanManagePage(this.tabId)).Returns(false);

            this.SetupCommand();

            // Act
            var result = this.getCommand.Run();

			Logger.LogDebug("Run_GetPageWithValidCommandForPortalNotAllowed_ShouldErrorResponse(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsError, Is.True);
                Assert.That(result is ConsoleErrorResultModel, Is.True);
            }
			Logger.LogDebug("<<< Exiting Run_GetPageWithValidCommandForPortalNotAllowed_ShouldErrorResponse()");
        }

        private void SetupCommand()
        {
			Logger.LogDebug(">>> Entering SetupCommand()");
            this.getCommand = new GetPage(this.tabControllerMock.Object, this.securityServiceMock.Object, this.contentVerifierMock.Object);

			Logger.LogDebug("SetupCommand(): getCommand → {GetCommand}", getCommand);
            var args = new[] { "get-page", this.tabId.ToString() };
            this.getCommand.Initialize(args, this.portalSettings, null, this.tabId);
			Logger.LogDebug("<<< Exiting SetupCommand()");
			Logger.LogDebug("SetupCommand(): args → {Args}", args);
        }
    }
}
