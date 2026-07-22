// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Portals
{
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PortalSettingsTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const int ValidPortalId = 0;
        private const int ValidTabId = 42;
        private const int InValidPortalId = -1;
        private const int InValidTabId = -1;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.serviceProvider = FakeServiceProvider.Setup(services =>
            {
				Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
                services.AddSingleton(Mock.Of<IPortalSettingsController>());
            });
			Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void Constructor_Creates_Registration_Property()
        {
			Logger.LogDebug(">>> Entering Constructor_Creates_Registration_Property()");
            var settings = new PortalSettings();

			Logger.LogDebug("Constructor_Creates_Registration_Property(): settings → {Settings}", settings);
            Assert.That(settings.Registration, Is.Not.Null);
			Logger.LogDebug("<<< Exiting Constructor_Creates_Registration_Property()");
        }

        [Test]
        public void Constructor_Sets_PortalId_When_Passed_PortalId()
        {
			Logger.LogDebug(">>> Entering Constructor_Sets_PortalId_When_Passed_PortalId()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
			Logger.LogDebug("Constructor_Sets_PortalId_When_Passed_PortalId(): mockPortalController → {MockPortalController}", mockPortalController);

            // Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);

			Logger.LogDebug("Constructor_Sets_PortalId_When_Passed_PortalId(): settings → {Settings}", settings);
            // Assert
            Assert.That(settings.PortalId, Is.EqualTo(ValidPortalId));
			Logger.LogDebug("<<< Exiting Constructor_Sets_PortalId_When_Passed_PortalId()");
        }

        [Test]
        public void Constructor_Sets_PortalId_When_Passed_PortalAlias()
        {
			Logger.LogDebug(">>> Entering Constructor_Sets_PortalId_When_Passed_PortalAlias()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
			Logger.LogDebug("Constructor_Sets_PortalId_When_Passed_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);

            var portalAlias = CreatePortalAlias(ValidPortalId);

			Logger.LogDebug("Constructor_Sets_PortalId_When_Passed_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);
            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

			Logger.LogDebug("Constructor_Sets_PortalId_When_Passed_PortalAlias(): settings → {Settings}", settings);
            // Assert
            Assert.That(settings.PortalId, Is.EqualTo(ValidPortalId));
			Logger.LogDebug("<<< Exiting Constructor_Sets_PortalId_When_Passed_PortalAlias()");
        }

        [Test]
        public void Constructor_Sets_PortalAlias_When_Passed_PortalAlias()
        {
			Logger.LogDebug(">>> Entering Constructor_Sets_PortalAlias_When_Passed_PortalAlias()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
			Logger.LogDebug("Constructor_Sets_PortalAlias_When_Passed_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);

            var portalAlias = CreatePortalAlias(ValidPortalId);

			Logger.LogDebug("Constructor_Sets_PortalAlias_When_Passed_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);
            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

			Logger.LogDebug("Constructor_Sets_PortalAlias_When_Passed_PortalAlias(): settings → {Settings}", settings);
            // Assert
            Assert.That(settings.PortalAlias, Is.EqualTo(portalAlias));
			Logger.LogDebug("<<< Exiting Constructor_Sets_PortalAlias_When_Passed_PortalAlias()");
        }

        [Test]
        public void Constructor_Sets_PortalId_When_Passed_Portal()
        {
			Logger.LogDebug(">>> Entering Constructor_Sets_PortalId_When_Passed_Portal()");
            var portal = CreatePortal(ValidPortalId);

			Logger.LogDebug("Constructor_Sets_PortalId_When_Passed_Portal(): portal → {Portal}", portal);
            // Act
            var settings = new PortalSettings(ValidTabId, portal);

			Logger.LogDebug("Constructor_Sets_PortalId_When_Passed_Portal(): settings → {Settings}", settings);
            // Assert
            Assert.That(settings.PortalId, Is.EqualTo(ValidPortalId));
			Logger.LogDebug("<<< Exiting Constructor_Sets_PortalId_When_Passed_Portal()");
        }

        [Test]
        public void Constructor_Does_Not_Set_PortalId_When_Passed_Null_Portal()
        {
			Logger.LogDebug(">>> Entering Constructor_Does_Not_Set_PortalId_When_Passed_Null_Portal()");
            var portal = CreatePortal(ValidPortalId);

			Logger.LogDebug("Constructor_Does_Not_Set_PortalId_When_Passed_Null_Portal(): portal → {Portal}", portal);
            // Act
            var settings = new PortalSettings(ValidTabId, (PortalInfo)null);

			Logger.LogDebug("Constructor_Does_Not_Set_PortalId_When_Passed_Null_Portal(): settings → {Settings}", settings);
            // Assert
            Assert.That(settings.PortalId, Is.EqualTo(InValidPortalId));
			Logger.LogDebug("<<< Exiting Constructor_Does_Not_Set_PortalId_When_Passed_Null_Portal()");
        }

        [Test]
        public void Constructor_Calls_PortalController_GetPortal_When_Passed_PortalId()
        {
			Logger.LogDebug(">>> Entering Constructor_Calls_PortalController_GetPortal_When_Passed_PortalId()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
			Logger.LogDebug("Constructor_Calls_PortalController_GetPortal_When_Passed_PortalId(): mockPortalController → {MockPortalController}", mockPortalController);

            // Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);

			Logger.LogDebug("Constructor_Calls_PortalController_GetPortal_When_Passed_PortalId(): settings → {Settings}", settings);
            // Assert
            mockPortalController.Verify(c => c.GetPortal(ValidPortalId));
			Logger.LogDebug("<<< Exiting Constructor_Calls_PortalController_GetPortal_When_Passed_PortalId()");
        }

        [Test]
        public void Constructor_Calls_PortalController_GetPortal_When_Passed_PortalAlias()
        {
			Logger.LogDebug(">>> Entering Constructor_Calls_PortalController_GetPortal_When_Passed_PortalAlias()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
			Logger.LogDebug("Constructor_Calls_PortalController_GetPortal_When_Passed_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);

            var portalAlias = CreatePortalAlias(ValidPortalId);

			Logger.LogDebug("Constructor_Calls_PortalController_GetPortal_When_Passed_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);
            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

			Logger.LogDebug("Constructor_Calls_PortalController_GetPortal_When_Passed_PortalAlias(): settings → {Settings}", settings);
            // Assert
            mockPortalController.Verify(c => c.GetPortal(ValidPortalId));
			Logger.LogDebug("<<< Exiting Constructor_Calls_PortalController_GetPortal_When_Passed_PortalAlias()");
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Portal()
        {
			Logger.LogDebug(">>> Entering Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Portal()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);

			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Portal(): portal → {Portal}", portal);
            // Act
            var settings = new PortalSettings(ValidTabId, portal);

			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Portal(): settings → {Settings}", settings);
            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.LoadPortal(portal, settings));
			Logger.LogDebug("<<< Exiting Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Portal()");
			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Portal(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias()
        {
			Logger.LogDebug(">>> Entering Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias(): portal → {Portal}", portal);
			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(ValidPortalId);

			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);
            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias(): settings → {Settings}", settings);
            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.LoadPortal(portal, settings));
			Logger.LogDebug("<<< Exiting Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias()");
			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Portal_Is_Null()
        {
			Logger.LogDebug(">>> Entering Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Portal_Is_Null()");
            var settings = new PortalSettings(ValidTabId, (PortalInfo)null);

			Logger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Portal_Is_Null(): settings → {Settings}", settings);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.LoadPortal(It.IsAny<PortalInfo>(), settings), Times.Never);
			Logger.LogDebug("<<< Exiting Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Portal_Is_Null()");
			Logger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Portal_Is_Null(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias()
        {
			Logger.LogDebug(">>> Entering Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
			Logger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias(): portal → {Portal}", portal);
			Logger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(InValidPortalId);

			Logger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);
            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

			Logger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias(): settings → {Settings}", settings);
            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.LoadPortal(portal, settings), Times.Never);
			Logger.LogDebug("<<< Exiting Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias()");
			Logger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalId()
        {
			Logger.LogDebug(">>> Entering Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalId()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalId(): mockPortalController → {MockPortalController}", mockPortalController);

            // Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);

			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalId(): settings → {Settings}", settings);
            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.LoadPortalSettings(settings));
			Logger.LogDebug("<<< Exiting Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalId()");
			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalId(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal()
        {
			Logger.LogDebug(">>> Entering Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal(): mockPortalController → {MockPortalController}", mockPortalController);

            var portal = CreatePortal(ValidPortalId);

			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal(): portal → {Portal}", portal);
            // Act
            var settings = new PortalSettings(ValidTabId, portal);

			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal(): settings → {Settings}", settings);
            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.LoadPortalSettings(settings));
			Logger.LogDebug("<<< Exiting Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal()");
			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias()
        {
			Logger.LogDebug(">>> Entering Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);

            var portalAlias = CreatePortalAlias(ValidPortalId);

			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);
            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias(): settings → {Settings}", settings);
            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.LoadPortalSettings(settings));
			Logger.LogDebug("<<< Exiting Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias()");
			Logger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Portal()
        {
			Logger.LogDebug(">>> Entering Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Portal()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);

			Logger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Portal(): portal → {Portal}", portal);
            // Act
            var settings = new PortalSettings(ValidTabId, portal);

			Logger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Portal(): settings → {Settings}", settings);
            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId, settings));
			Logger.LogDebug("<<< Exiting Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Portal()");
			Logger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Portal(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias()
        {
			Logger.LogDebug(">>> Entering Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
			Logger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias(): portal → {Portal}", portal);
			Logger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(ValidPortalId);

			Logger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);
            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

			Logger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias(): settings → {Settings}", settings);
            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId, settings));
			Logger.LogDebug("<<< Exiting Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias()");
			Logger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Portal_Is_Null()
        {
			Logger.LogDebug(">>> Entering Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Portal_Is_Null()");
            var settings = new PortalSettings(ValidTabId, (PortalInfo)null);

			Logger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Portal_Is_Null(): settings → {Settings}", settings);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId, settings), Times.Never);
			Logger.LogDebug("<<< Exiting Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Portal_Is_Null()");
			Logger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Portal_Is_Null(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias()
        {
			Logger.LogDebug(">>> Entering Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
			Logger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias(): portal → {Portal}", portal);
			Logger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(InValidPortalId);

			Logger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);
            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

			Logger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias(): settings → {Settings}", settings);
            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId, settings), Times.Never);
			Logger.LogDebug("<<< Exiting Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias()");
			Logger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId()
        {
			Logger.LogDebug(">>> Entering Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
			Logger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId(): portal → {Portal}", portal);
			Logger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController
				Logger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
                .Setup(c => c.GetActiveTab(ValidTabId, It.IsAny<PortalSettings>()))
                .Returns(new TabInfo());

            // Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);

			Logger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId(): settings → {Settings}", settings);
            // Assert
            Assert.That(settings.ActiveTab, Is.Not.Null);
			Logger.LogDebug("<<< Exiting Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId()");
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId()
        {
			Logger.LogDebug(">>> Entering Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
			Logger.LogDebug("Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId(): portal → {Portal}", portal);
			Logger.LogDebug("Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController
				Logger.LogDebug("Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
                .Setup(c => c.GetActiveTab(InValidTabId, It.IsAny<PortalSettings>()))
                .Returns((TabInfo)null);

            // Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);

			Logger.LogDebug("Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId(): settings → {Settings}", settings);
            // Assert
            Assert.That(settings.ActiveTab, Is.Null);
			Logger.LogDebug("<<< Exiting Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId()");
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_Portal()
        {
			Logger.LogDebug(">>> Entering Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_Portal()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			Logger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_Portal(): portal → {Portal}", portal);
			Logger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_Portal(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController
                .Setup(c => c.GetActiveTab(ValidTabId, It.IsAny<PortalSettings>()))
                .Returns(new TabInfo());

            // Act
            var settings = new PortalSettings(ValidTabId, portal);

			Logger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_Portal(): settings → {Settings}", settings);
            // Assert
            Assert.That(settings.ActiveTab, Is.Not.Null);
			Logger.LogDebug("<<< Exiting Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_Portal()");
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias()
        {
			Logger.LogDebug(">>> Entering Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
			Logger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias(): portal → {Portal}", portal);
			Logger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController
				Logger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
                .Setup(c => c.GetActiveTab(ValidTabId, It.IsAny<PortalSettings>()))
                .Returns(new TabInfo());

            var portalAlias = CreatePortalAlias(ValidPortalId);

			Logger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);
            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

			Logger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias(): settings → {Settings}", settings);
            // Assert
            Assert.That(settings.ActiveTab, Is.Not.Null);
			Logger.LogDebug("<<< Exiting Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias()");
        }

        private static PortalInfo CreatePortal(int portalId) =>
            new() { PortalID = portalId, };
			Logger.LogDebug(">>> Entering CreatePortal(portalId={PortalId})", portalId);

        private static PortalAliasInfo CreatePortalAlias(int portalId) =>
            new() { PortalID = portalId, };
			Logger.LogDebug(">>> Entering CreatePortalAlias(portalId={PortalId})", portalId);
    }
}
