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
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class GetPageUnitTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
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
			_logger.LogDebug(">>> Entering RunBeforeAnyTest()");
            this.tab = new TabInfo { TabID = this.tabId, PortalID = this.testPortalId, };

			_logger.LogDebug("RunBeforeAnyTest(): this.tab → {Arg0}", this.tab);
			_logger.LogDebug("RunBeforeAnyTest(): tab → {Tab}", tab);
            this.portalSettings = new PortalSettings { PortalId = this.testPortalId, };
			_logger.LogDebug("RunBeforeAnyTest(): portalSettings → {PortalSettings}", portalSettings);
			_logger.LogDebug("RunBeforeAnyTest(): this.portalSettings → {Arg0}", this.portalSettings);

            this.tabControllerMock = new Mock<ITabController>();
			_logger.LogDebug("RunBeforeAnyTest(): tabControllerMock → {TabControllerMock}", tabControllerMock);
            this.securityServiceMock = new Mock<ISecurityService>();
			_logger.LogDebug("RunBeforeAnyTest(): securityServiceMock → {SecurityServiceMock}", securityServiceMock);
            this.contentVerifierMock = new Mock<IContentVerifier>();
			_logger.LogDebug("RunBeforeAnyTest(): contentVerifierMock → {ContentVerifierMock}", contentVerifierMock);

            this.tabControllerMock.SetReturnsDefault(this.tab);
            this.securityServiceMock.SetReturnsDefault(true);
            this.contentVerifierMock.SetReturnsDefault(true);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					_logger.LogDebug("RunBeforeAnyTest(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(this.tabControllerMock.Object);
                    services.AddSingleton(this.securityServiceMock.Object);
                    services.AddSingleton(this.contentVerifierMock.Object);
                });
				_logger.LogDebug("<<< Exiting RunBeforeAnyTest()");
        }

        [TearDown]
        public void TearDown()
        {
			_logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			_logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void Run_GetPageWithValidCommand_ShouldSuccessResponse()
        {
			_logger.LogDebug(">>> Entering Run_GetPageWithValidCommand_ShouldSuccessResponse()");
            // Arrange
            this.tabControllerMock.Setup(t => t.GetTab(this.tabId, this.testPortalId)).Returns(this.tab);

            this.SetupCommand();

            // Act
            var result = this.getCommand.Run();
			_logger.LogDebug("Run_GetPageWithValidCommand_ShouldSuccessResponse(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsError, Is.False);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Records, Is.EqualTo(1));
                Assert.That(result is ConsoleErrorResultModel, Is.False);
            }
			_logger.LogDebug("<<< Exiting Run_GetPageWithValidCommand_ShouldSuccessResponse()");
        }

        [Test]
        public void Run_GetPageWithValidCommandForNonExistingTab_ShouldErrorResponse()
        {
			_logger.LogDebug(">>> Entering Run_GetPageWithValidCommandForNonExistingTab_ShouldErrorResponse()");
            // Arrange
            this.tab = null;
			_logger.LogDebug("Run_GetPageWithValidCommandForNonExistingTab_ShouldErrorResponse(): tab → {Tab}", tab);
			_logger.LogDebug("Run_GetPageWithValidCommandForNonExistingTab_ShouldErrorResponse(): this.tab → {Arg0}", this.tab);

            this.tabControllerMock.Setup(t => t.GetTab(this.tabId, this.testPortalId)).Returns(this.tab);

            this.SetupCommand();

            // Act
            var result = this.getCommand.Run();
			_logger.LogDebug("Run_GetPageWithValidCommandForNonExistingTab_ShouldErrorResponse(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsError, Is.True);
                Assert.That(result is ConsoleErrorResultModel, Is.True);
            }
			_logger.LogDebug("<<< Exiting Run_GetPageWithValidCommandForNonExistingTab_ShouldErrorResponse()");
        }

        [Test]
        public void Run_GetPageWithValidCommandForRequestedPortalNotAllowed_ShouldErrorResponse()
        {
			_logger.LogDebug(">>> Entering Run_GetPageWithValidCommandForRequestedPortalNotAllowed_ShouldErrorResponse()");
            // Arrange
            this.contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(this.testPortalId, this.portalSettings, false)).Returns(false);

            this.SetupCommand();

            // Act
            var result = this.getCommand.Run();
			_logger.LogDebug("Run_GetPageWithValidCommandForRequestedPortalNotAllowed_ShouldErrorResponse(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsError, Is.True);
                Assert.That(result is ConsoleErrorResultModel, Is.True);
            }
			_logger.LogDebug("<<< Exiting Run_GetPageWithValidCommandForRequestedPortalNotAllowed_ShouldErrorResponse()");
        }

        [Test]
        public void Run_GetPageWithValidCommandForPortalNotAllowed_ShouldErrorResponse()
        {
			_logger.LogDebug(">>> Entering Run_GetPageWithValidCommandForPortalNotAllowed_ShouldErrorResponse()");
            // Arrange
            this.securityServiceMock.Setup(s => s.CanManagePage(this.tabId)).Returns(false);

            this.SetupCommand();

            // Act
            var result = this.getCommand.Run();
			_logger.LogDebug("Run_GetPageWithValidCommandForPortalNotAllowed_ShouldErrorResponse(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsError, Is.True);
                Assert.That(result is ConsoleErrorResultModel, Is.True);
            }
			_logger.LogDebug("<<< Exiting Run_GetPageWithValidCommandForPortalNotAllowed_ShouldErrorResponse()");
        }

        private void SetupCommand()
        {
			_logger.LogDebug(">>> Entering SetupCommand()");
            this.getCommand = new GetPage(this.tabControllerMock.Object, this.securityServiceMock.Object, this.contentVerifierMock.Object);

			_logger.LogDebug("SetupCommand(): getCommand → {GetCommand}", getCommand);
            var args = new[] { "get-page", this.tabId.ToString() };
			_logger.LogDebug("SetupCommand(): args → {Args}", args);
            this.getCommand.Initialize(args, this.portalSettings, null, this.tabId);
			_logger.LogDebug("<<< Exiting SetupCommand()");
        }
    }
}
