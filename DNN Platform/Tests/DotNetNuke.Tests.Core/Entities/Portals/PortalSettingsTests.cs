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
        private const int ValidPortalId = 0;
        private const int ValidTabId = 42;
        private const int InValidPortalId = -1;
        private const int InValidTabId = -1;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			mockEventLogger.LogDebug(">>> Entering SetUp()");
            this.serviceProvider = FakeServiceProvider.Setup(services =>
            {
				mockEventLogger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
                services.AddSingleton(Mock.Of<IPortalSettingsController>());
            });
			mockEventLogger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void Constructor_Creates_Registration_Property()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Creates_Registration_Property()");
            var settings = new PortalSettings();

			mockEventLogger.LogDebug("Constructor_Creates_Registration_Property(): settings → {Settings}", settings);
            Assert.That(settings.Registration, Is.Not.Null);
			mockEventLogger.LogDebug("<<< Exiting Constructor_Creates_Registration_Property()");
        }

        [Test]
        public void Constructor_Sets_PortalId_When_Passed_PortalId()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Sets_PortalId_When_Passed_PortalId()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Constructor_Sets_PortalId_When_Passed_PortalId(): mockPortalController → {MockPortalController}", mockPortalController);
            PortalController.SetTestableInstance(mockPortalController.Object);

            // Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Sets_PortalId_When_Passed_PortalId(): settings → {Settings}", settings);

            // Assert
            Assert.That(settings.PortalId, Is.EqualTo(ValidPortalId));
			mockEventLogger.LogDebug("<<< Exiting Constructor_Sets_PortalId_When_Passed_PortalId()");
        }

        [Test]
        public void Constructor_Sets_PortalId_When_Passed_PortalAlias()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Sets_PortalId_When_Passed_PortalAlias()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Constructor_Sets_PortalId_When_Passed_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Sets_PortalId_When_Passed_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);
			mockEventLogger.LogDebug("Constructor_Sets_PortalId_When_Passed_PortalAlias(): settings → {Settings}", settings);

            // Assert
            Assert.That(settings.PortalId, Is.EqualTo(ValidPortalId));
			mockEventLogger.LogDebug("<<< Exiting Constructor_Sets_PortalId_When_Passed_PortalAlias()");
        }

        [Test]
        public void Constructor_Sets_PortalAlias_When_Passed_PortalAlias()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Sets_PortalAlias_When_Passed_PortalAlias()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Constructor_Sets_PortalAlias_When_Passed_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Sets_PortalAlias_When_Passed_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);
			mockEventLogger.LogDebug("Constructor_Sets_PortalAlias_When_Passed_PortalAlias(): settings → {Settings}", settings);

            // Assert
            Assert.That(settings.PortalAlias, Is.EqualTo(portalAlias));
			mockEventLogger.LogDebug("<<< Exiting Constructor_Sets_PortalAlias_When_Passed_PortalAlias()");
        }

        [Test]
        public void Constructor_Sets_PortalId_When_Passed_Portal()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Sets_PortalId_When_Passed_Portal()");
            var portal = CreatePortal(ValidPortalId);

			mockEventLogger.LogDebug("Constructor_Sets_PortalId_When_Passed_Portal(): portal → {Portal}", portal);
            // Act
            var settings = new PortalSettings(ValidTabId, portal);
			mockEventLogger.LogDebug("Constructor_Sets_PortalId_When_Passed_Portal(): settings → {Settings}", settings);

            // Assert
            Assert.That(settings.PortalId, Is.EqualTo(ValidPortalId));
			mockEventLogger.LogDebug("<<< Exiting Constructor_Sets_PortalId_When_Passed_Portal()");
        }

        [Test]
        public void Constructor_Does_Not_Set_PortalId_When_Passed_Null_Portal()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Does_Not_Set_PortalId_When_Passed_Null_Portal()");
            var portal = CreatePortal(ValidPortalId);

			mockEventLogger.LogDebug("Constructor_Does_Not_Set_PortalId_When_Passed_Null_Portal(): portal → {Portal}", portal);
            // Act
            var settings = new PortalSettings(ValidTabId, (PortalInfo)null);
			mockEventLogger.LogDebug("Constructor_Does_Not_Set_PortalId_When_Passed_Null_Portal(): settings → {Settings}", settings);

            // Assert
            Assert.That(settings.PortalId, Is.EqualTo(InValidPortalId));
			mockEventLogger.LogDebug("<<< Exiting Constructor_Does_Not_Set_PortalId_When_Passed_Null_Portal()");
        }

        [Test]
        public void Constructor_Calls_PortalController_GetPortal_When_Passed_PortalId()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Calls_PortalController_GetPortal_When_Passed_PortalId()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Constructor_Calls_PortalController_GetPortal_When_Passed_PortalId(): mockPortalController → {MockPortalController}", mockPortalController);
            PortalController.SetTestableInstance(mockPortalController.Object);

            // Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Calls_PortalController_GetPortal_When_Passed_PortalId(): settings → {Settings}", settings);

            // Assert
            mockPortalController.Verify(c => c.GetPortal(ValidPortalId));
			mockEventLogger.LogDebug("<<< Exiting Constructor_Calls_PortalController_GetPortal_When_Passed_PortalId()");
        }

        [Test]
        public void Constructor_Calls_PortalController_GetPortal_When_Passed_PortalAlias()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Calls_PortalController_GetPortal_When_Passed_PortalAlias()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Constructor_Calls_PortalController_GetPortal_When_Passed_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Calls_PortalController_GetPortal_When_Passed_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);
			mockEventLogger.LogDebug("Constructor_Calls_PortalController_GetPortal_When_Passed_PortalAlias(): settings → {Settings}", settings);

            // Assert
            mockPortalController.Verify(c => c.GetPortal(ValidPortalId));
			mockEventLogger.LogDebug("<<< Exiting Constructor_Calls_PortalController_GetPortal_When_Passed_PortalAlias()");
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Portal()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Portal()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Portal(): portal → {Portal}", portal);

            // Act
            var settings = new PortalSettings(ValidTabId, portal);
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Portal(): settings → {Settings}", settings);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Portal(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController.Verify(c => c.LoadPortal(portal, settings));
			mockEventLogger.LogDebug("<<< Exiting Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Portal()");
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias(): portal → {Portal}", portal);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias(): settings → {Settings}", settings);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController.Verify(c => c.LoadPortal(portal, settings));
			mockEventLogger.LogDebug("<<< Exiting Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias()");
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Portal_Is_Null()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Portal_Is_Null()");
            var settings = new PortalSettings(ValidTabId, (PortalInfo)null);

			mockEventLogger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Portal_Is_Null(): settings → {Settings}", settings);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			mockEventLogger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Portal_Is_Null(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController.Verify(c => c.LoadPortal(It.IsAny<PortalInfo>(), settings), Times.Never);
			mockEventLogger.LogDebug("<<< Exiting Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Portal_Is_Null()");
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias(): portal → {Portal}", portal);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(InValidPortalId);
			mockEventLogger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);
			mockEventLogger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias(): settings → {Settings}", settings);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			mockEventLogger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController.Verify(c => c.LoadPortal(portal, settings), Times.Never);
			mockEventLogger.LogDebug("<<< Exiting Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias()");
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalId()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalId()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalId(): mockPortalController → {MockPortalController}", mockPortalController);
            PortalController.SetTestableInstance(mockPortalController.Object);

            // Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalId(): settings → {Settings}", settings);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalId(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController.Verify(c => c.LoadPortalSettings(settings));
			mockEventLogger.LogDebug("<<< Exiting Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalId()");
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal(): mockPortalController → {MockPortalController}", mockPortalController);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portal = CreatePortal(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal(): portal → {Portal}", portal);

            // Act
            var settings = new PortalSettings(ValidTabId, portal);
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal(): settings → {Settings}", settings);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController.Verify(c => c.LoadPortalSettings(settings));
			mockEventLogger.LogDebug("<<< Exiting Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal()");
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias(): settings → {Settings}", settings);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController.Verify(c => c.LoadPortalSettings(settings));
			mockEventLogger.LogDebug("<<< Exiting Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias()");
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Portal()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Portal()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Portal(): portal → {Portal}", portal);

            // Act
            var settings = new PortalSettings(ValidTabId, portal);
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Portal(): settings → {Settings}", settings);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Portal(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId, settings));
			mockEventLogger.LogDebug("<<< Exiting Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Portal()");
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias(): portal → {Portal}", portal);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias(): settings → {Settings}", settings);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			mockEventLogger.LogDebug("Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId, settings));
			mockEventLogger.LogDebug("<<< Exiting Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias()");
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Portal_Is_Null()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Portal_Is_Null()");
            var settings = new PortalSettings(ValidTabId, (PortalInfo)null);

			mockEventLogger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Portal_Is_Null(): settings → {Settings}", settings);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			mockEventLogger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Portal_Is_Null(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId, settings), Times.Never);
			mockEventLogger.LogDebug("<<< Exiting Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Portal_Is_Null()");
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias(): portal → {Portal}", portal);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(InValidPortalId);
			mockEventLogger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);
			mockEventLogger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias(): settings → {Settings}", settings);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			mockEventLogger.LogDebug("Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId, settings), Times.Never);
			mockEventLogger.LogDebug("<<< Exiting Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias()");
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId(): portal → {Portal}", portal);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController
                .Setup(c => c.GetActiveTab(ValidTabId, It.IsAny<PortalSettings>()))
                .Returns(new TabInfo());

            // Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId(): settings → {Settings}", settings);

            // Assert
            Assert.That(settings.ActiveTab, Is.Not.Null);
			mockEventLogger.LogDebug("<<< Exiting Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId()");
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId(): portal → {Portal}", portal);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController
                .Setup(c => c.GetActiveTab(InValidTabId, It.IsAny<PortalSettings>()))
                .Returns((TabInfo)null);

            // Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId(): settings → {Settings}", settings);

            // Assert
            Assert.That(settings.ActiveTab, Is.Null);
			mockEventLogger.LogDebug("<<< Exiting Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId()");
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_Portal()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_Portal()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_Portal(): portal → {Portal}", portal);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_Portal(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController
                .Setup(c => c.GetActiveTab(ValidTabId, It.IsAny<PortalSettings>()))
                .Returns(new TabInfo());

            // Act
            var settings = new PortalSettings(ValidTabId, portal);
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_Portal(): settings → {Settings}", settings);

            // Assert
            Assert.That(settings.ActiveTab, Is.Not.Null);
			mockEventLogger.LogDebug("<<< Exiting Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_Portal()");
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias()
        {
			mockEventLogger.LogDebug(">>> Entering Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias()");
            // Arrange
            var portal = CreatePortal(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias(): portal → {Portal}", portal);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias(): mockPortalSettingsController → {MockPortalSettingsController}", mockPortalSettingsController);
            mockPortalSettingsController
                .Setup(c => c.GetActiveTab(ValidTabId, It.IsAny<PortalSettings>()))
                .Returns(new TabInfo());

            var portalAlias = CreatePortalAlias(ValidPortalId);
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias(): portalAlias → {PortalAlias}", portalAlias);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);
			mockEventLogger.LogDebug("Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias(): settings → {Settings}", settings);

            // Assert
            Assert.That(settings.ActiveTab, Is.Not.Null);
			mockEventLogger.LogDebug("<<< Exiting Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias()");
        }

        private static PortalInfo CreatePortal(int portalId) =>
            new() { PortalID = portalId, };

        private static PortalAliasInfo CreatePortalAlias(int portalId) =>
            new() { PortalID = portalId, };
    }
}
