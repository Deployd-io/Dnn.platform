﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Controllers.Portal
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Portals.Data;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class PortalGroupControllerTests
    {
        private Mock<DataProvider> mockData;
#pragma warning disable 649
        private UserCopiedCallback userCopied;
#pragma warning restore 649

        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			mockEventLogger.LogDebug(">>> Entering SetUp()");
            this.mockData = MockComponentProvider.CreateDataProvider();

			mockEventLogger.LogDebug("SetUp(): mockData → {MockData}", mockData);
            var hostSettingsTable = new DataTable("HostSettings");
			mockEventLogger.LogDebug("SetUp(): hostSettingsTable → {HostSettingsTable}", hostSettingsTable);
            hostSettingsTable.PrimaryKey = new[] { hostSettingsTable.Columns.Add("SettingName"), };
            hostSettingsTable.Columns.Add("SettingValue");
            hostSettingsTable.Columns.Add("SettingIsSecure");
            hostSettingsTable.Rows.Add("PerformanceSetting", "0", false);
            this.mockData.Setup(c => c.GetHostSettings()).Returns(hostSettingsTable.CreateDataReader());

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					mockEventLogger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(this.mockData.Object);
                });
				mockEventLogger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            MockComponentProvider.ResetContainer();
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void PortalGroupController_AddPortalToGroup_Throws_On_Null_PortalGroup()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_AddPortalToGroup_Throws_On_Null_PortalGroup()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalToGroup_Throws_On_Null_PortalGroup(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalToGroup_Throws_On_Null_PortalGroup(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_AddPortalToGroup_Throws_On_Null_PortalGroup(): controller → {Controller}", controller);
            var portal = new PortalInfo { PortalID = Constants.PORTAL_ValidPortalId };

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => controller.AddPortalToGroup(portal, null, this.userCopied));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_AddPortalToGroup_Throws_On_Null_PortalGroup()");
        }

        [Test]
        public void PortalGroupController_AddPortalToGroup_Throws_On_Null_Portal()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_AddPortalToGroup_Throws_On_Null_Portal()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalToGroup_Throws_On_Null_Portal(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalToGroup_Throws_On_Null_Portal(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_AddPortalToGroup_Throws_On_Null_Portal(): controller → {Controller}", controller);
            var portalGroup = new PortalGroupInfo { PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId };

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => controller.AddPortalToGroup(null, portalGroup, this.userCopied));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_AddPortalToGroup_Throws_On_Null_Portal()");
        }

        [Test]
        public void PortalGroupController_AddPortalToGroup_Throws_On_Negative_PortalGroupId()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_AddPortalToGroup_Throws_On_Negative_PortalGroupId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalToGroup_Throws_On_Negative_PortalGroupId(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalToGroup_Throws_On_Negative_PortalGroupId(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_AddPortalToGroup_Throws_On_Negative_PortalGroupId(): controller → {Controller}", controller);

            var portal = new PortalInfo { PortalID = Constants.PORTAL_ValidPortalId };

            PortalGroupInfo portalGroup = new PortalGroupInfo { PortalGroupId = -1 };

            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.AddPortalToGroup(portal, portalGroup, this.userCopied));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_AddPortalToGroup_Throws_On_Negative_PortalGroupId()");
        }

        [Test]
        public void PortalGroupController_AddPortalToGroup_Throws_On_Negative_PortalId()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_AddPortalToGroup_Throws_On_Negative_PortalId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalToGroup_Throws_On_Negative_PortalId(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalToGroup_Throws_On_Negative_PortalId(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_AddPortalToGroup_Throws_On_Negative_PortalId(): controller → {Controller}", controller);

            var portal = new PortalInfo { PortalID = -1 };

            PortalGroupInfo portalGroup = new PortalGroupInfo { PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId };

            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.AddPortalToGroup(portal, portalGroup, this.userCopied));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_AddPortalToGroup_Throws_On_Negative_PortalId()");
        }

        [Test]
        public void PortalGroupController_AddPortalGroup_Throws_On_Null_PortalGroup()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_AddPortalGroup_Throws_On_Null_PortalGroup()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Throws_On_Null_PortalGroup(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Throws_On_Null_PortalGroup(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Throws_On_Null_PortalGroup(): controller → {Controller}", controller);

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => controller.AddPortalGroup(null));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_AddPortalGroup_Throws_On_Null_PortalGroup()");
        }

        [Test]
        public void PortalGroupController_AddPortalGroup_Calls_DataService_On_Valid_Arguments()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_AddPortalGroup_Calls_DataService_On_Valid_Arguments()");
            // Arrange
            MockComponentProvider.CreateDataCacheProvider();
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Calls_DataService_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Calls_DataService_On_Valid_Arguments(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Calls_DataService_On_Valid_Arguments(): controller → {Controller}", controller);

            PortalGroupInfo portalGroup = CreateValidPortalGroup();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Calls_DataService_On_Valid_Arguments(): portalGroup → {PortalGroup}", portalGroup);
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Calls_DataService_On_Valid_Arguments(): portalGroup.PortalGroupId → {Arg0}", portalGroup.PortalGroupId);
            portalGroup.PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId;

            // Act
            controller.AddPortalGroup(portalGroup);

            // Assert
            mockDataService.Verify(ds => ds.AddPortalGroup(portalGroup, It.IsAny<int>()));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_AddPortalGroup_Calls_DataService_On_Valid_Arguments()");
        }

        [Test]
        public void PortalGroupController_AddPortalGroup_Calls_PortalController_On_Valid_Arguments()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_AddPortalGroup_Calls_PortalController_On_Valid_Arguments()");
            // Arrange
            MockComponentProvider.CreateDataCacheProvider();
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Calls_PortalController_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
            var masterPortal = new PortalInfo { PortalID = Constants.PORTAL_ValidPortalId };
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Calls_PortalController_On_Valid_Arguments(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(pc => pc.GetPortal(Constants.PORTAL_ValidPortalId))
                        .Returns(masterPortal);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Calls_PortalController_On_Valid_Arguments(): controller → {Controller}", controller);

            PortalGroupInfo portalGroup = CreateValidPortalGroup();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Calls_PortalController_On_Valid_Arguments(): portalGroup → {PortalGroup}", portalGroup);
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Calls_PortalController_On_Valid_Arguments(): portalGroup.PortalGroupId → {Arg0}", portalGroup.PortalGroupId);
            portalGroup.PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId;

            // Act
            controller.AddPortalGroup(portalGroup);

            // Assert
            mockPortalController.Verify(pc => pc.GetPortal(portalGroup.MasterPortalId));
            mockPortalController.Verify(pc => pc.UpdatePortalInfo(masterPortal));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_AddPortalGroup_Calls_PortalController_On_Valid_Arguments()");
        }

        [Test]
        public void PortalGroupController_AddPortalGroup_Returns_ValidId_On_Valid_PortalGroup()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_AddPortalGroup_Returns_ValidId_On_Valid_PortalGroup()");
            // Arrange
            MockComponentProvider.CreateDataCacheProvider();
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Returns_ValidId_On_Valid_PortalGroup(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.AddPortalGroup(It.IsAny<PortalGroupInfo>(), It.IsAny<int>())).Returns(Constants.PORTALGROUP_AddPortalGroupId);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Returns_ValidId_On_Valid_PortalGroup(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Returns_ValidId_On_Valid_PortalGroup(): controller → {Controller}", controller);

            PortalGroupInfo portalGroup = CreateValidPortalGroup();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Returns_ValidId_On_Valid_PortalGroup(): portalGroup → {PortalGroup}", portalGroup);
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Returns_ValidId_On_Valid_PortalGroup(): portalGroup.PortalGroupId → {Arg0}", portalGroup.PortalGroupId);
            portalGroup.PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId;

            // Act
            int portalGroupId = controller.AddPortalGroup(portalGroup);
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Returns_ValidId_On_Valid_PortalGroup(): portalGroupId → {PortalGroupId}", portalGroupId);

            // Assert
            Assert.That(portalGroupId, Is.EqualTo(Constants.PORTALGROUP_AddPortalGroupId));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_AddPortalGroup_Returns_ValidId_On_Valid_PortalGroup()");
        }

        [Test]
        public void PortalGroupController_AddPortalGroup_Sets_PortalGroupId_Property_On_Valid_PortalGroup()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_AddPortalGroup_Sets_PortalGroupId_Property_On_Valid_PortalGroup()");
            // Arrange
            MockComponentProvider.CreateDataCacheProvider();
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Sets_PortalGroupId_Property_On_Valid_PortalGroup(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.AddPortalGroup(It.IsAny<PortalGroupInfo>(), It.IsAny<int>())).Returns(Constants.PORTALGROUP_AddPortalGroupId);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Sets_PortalGroupId_Property_On_Valid_PortalGroup(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Sets_PortalGroupId_Property_On_Valid_PortalGroup(): controller → {Controller}", controller);

            PortalGroupInfo portalGroup = CreateValidPortalGroup();
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Sets_PortalGroupId_Property_On_Valid_PortalGroup(): portalGroup → {PortalGroup}", portalGroup);
			mockEventLogger.LogDebug("PortalGroupController_AddPortalGroup_Sets_PortalGroupId_Property_On_Valid_PortalGroup(): portalGroup.PortalGroupId → {Arg0}", portalGroup.PortalGroupId);
            portalGroup.PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId;

            // Act
            controller.AddPortalGroup(portalGroup);

            // Assert
            Assert.That(portalGroup.PortalGroupId, Is.EqualTo(Constants.PORTALGROUP_AddPortalGroupId));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_AddPortalGroup_Sets_PortalGroupId_Property_On_Valid_PortalGroup()");
        }

        [Test]
        public void PortalGroupController_DeletePortalGroup_Throws_On_Null_PortalGroup()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_DeletePortalGroup_Throws_On_Null_PortalGroup()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_DeletePortalGroup_Throws_On_Null_PortalGroup(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_DeletePortalGroup_Throws_On_Null_PortalGroup(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_DeletePortalGroup_Throws_On_Null_PortalGroup(): controller → {Controller}", controller);

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => controller.DeletePortalGroup(null));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_DeletePortalGroup_Throws_On_Null_PortalGroup()");
        }

        [Test]
        public void PortalGroupController_DeletePortalGroup_Throws_On_Negative_PortalGroupId()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_DeletePortalGroup_Throws_On_Negative_PortalGroupId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_DeletePortalGroup_Throws_On_Negative_PortalGroupId(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_DeletePortalGroup_Throws_On_Negative_PortalGroupId(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_DeletePortalGroup_Throws_On_Negative_PortalGroupId(): controller → {Controller}", controller);

            PortalGroupInfo portalGroup = CreateValidPortalGroup();
			mockEventLogger.LogDebug("PortalGroupController_DeletePortalGroup_Throws_On_Negative_PortalGroupId(): portalGroup → {PortalGroup}", portalGroup);
			mockEventLogger.LogDebug("PortalGroupController_DeletePortalGroup_Throws_On_Negative_PortalGroupId(): portalGroup.PortalGroupId → {Arg0}", portalGroup.PortalGroupId);
            portalGroup.PortalGroupId = Null.NullInteger;

            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.DeletePortalGroup(portalGroup));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_DeletePortalGroup_Throws_On_Negative_PortalGroupId()");
        }

        [Test]
        public void PortalGroupController_DeletePortalGroup_Calls_DataService_On_Valid_PortalGroupId()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_DeletePortalGroup_Calls_DataService_On_Valid_PortalGroupId()");
            // Arrange
            MockComponentProvider.CreateDataCacheProvider();
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_DeletePortalGroup_Calls_DataService_On_Valid_PortalGroupId(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_DeletePortalGroup_Calls_DataService_On_Valid_PortalGroupId(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_DeletePortalGroup_Calls_DataService_On_Valid_PortalGroupId(): controller → {Controller}", controller);

            PortalGroupInfo portalGroup = CreateValidPortalGroup();
			mockEventLogger.LogDebug("PortalGroupController_DeletePortalGroup_Calls_DataService_On_Valid_PortalGroupId(): portalGroup → {PortalGroup}", portalGroup);
			mockEventLogger.LogDebug("PortalGroupController_DeletePortalGroup_Calls_DataService_On_Valid_PortalGroupId(): portalGroup.PortalGroupId → {Arg0}", portalGroup.PortalGroupId);
            portalGroup.PortalGroupId = Constants.PORTALGROUP_DeletePortalGroupId;

            // Act
            controller.DeletePortalGroup(portalGroup);

            // Assert
            mockDataService.Verify(ds => ds.DeletePortalGroup(portalGroup));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_DeletePortalGroup_Calls_DataService_On_Valid_PortalGroupId()");
        }

        [Test]
        public void PortalGroupController_GetPortalGroups_Calls_DataService()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_GetPortalGroups_Calls_DataService()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();
			mockEventLogger.LogDebug("PortalGroupController_GetPortalGroups_Calls_DataService(): mockCache → {MockCache}", mockCache);
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(DataCache.PortalGroupsCacheKey))).Returns(null);

            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_GetPortalGroups_Calls_DataService(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetPortalGroups()).Returns(CreateValidPortalGroupsReader(0, Constants.USER_ValidId));

            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_GetPortalGroups_Calls_DataService(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_GetPortalGroups_Calls_DataService(): controller → {Controller}", controller);

            // Act
            controller.GetPortalGroups();

            // Assert
            mockDataService.Verify(ds => ds.GetPortalGroups());
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_GetPortalGroups_Calls_DataService()");
        }

        [Test]
        public void PortalGroupController_GetPortalGroups_Returns_EmptyList_If_No_Items()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_GetPortalGroups_Returns_EmptyList_If_No_Items()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();
			mockEventLogger.LogDebug("PortalGroupController_GetPortalGroups_Returns_EmptyList_If_No_Items(): mockCache → {MockCache}", mockCache);
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(DataCache.PortalGroupsCacheKey))).Returns(null);

            Mock<IDataService> mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_GetPortalGroups_Returns_EmptyList_If_No_Items(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetPortalGroups()).Returns(CreateValidPortalGroupsReader(0, Constants.USER_ValidId));

            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_GetPortalGroups_Returns_EmptyList_If_No_Items(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_GetPortalGroups_Returns_EmptyList_If_No_Items(): controller → {Controller}", controller);

            // Act
            IEnumerable<PortalGroupInfo> portalGroups = controller.GetPortalGroups();
			mockEventLogger.LogDebug("PortalGroupController_GetPortalGroups_Returns_EmptyList_If_No_Items(): portalGroups → {PortalGroups}", portalGroups);

            // Assert
            Assert.That(portalGroups.Count(), Is.EqualTo(0));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_GetPortalGroups_Returns_EmptyList_If_No_Items()");
        }

        [Test]
        public void PortalGroupController_GetPortalGroups_Returns_List_Of_PortalGroups()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_GetPortalGroups_Returns_List_Of_PortalGroups()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();
			mockEventLogger.LogDebug("PortalGroupController_GetPortalGroups_Returns_List_Of_PortalGroups(): mockCache → {MockCache}", mockCache);
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(DataCache.PortalGroupsCacheKey))).Returns(null);

            Mock<IDataService> mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_GetPortalGroups_Returns_List_Of_PortalGroups(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetPortalGroups()).Returns(CreateValidPortalGroupsReader(
                Constants.PORTALGROUP_ValidPortalGroupCount,
                Constants.USER_ValidId));

            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_GetPortalGroups_Returns_List_Of_PortalGroups(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_GetPortalGroups_Returns_List_Of_PortalGroups(): controller → {Controller}", controller);

            // Act
            IEnumerable<PortalGroupInfo> portalGroups = controller.GetPortalGroups();
			mockEventLogger.LogDebug("PortalGroupController_GetPortalGroups_Returns_List_Of_PortalGroups(): portalGroups → {PortalGroups}", portalGroups);

            // Assert
            Assert.That(portalGroups.Count(), Is.EqualTo(Constants.PORTALGROUP_ValidPortalGroupCount));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_GetPortalGroups_Returns_List_Of_PortalGroups()");
        }

        [Test]
        public void PortalGroupController_RemovePortalFromGroup_Throws_On_Null_PortalGroup()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_RemovePortalFromGroup_Throws_On_Null_PortalGroup()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_RemovePortalFromGroup_Throws_On_Null_PortalGroup(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_RemovePortalFromGroup_Throws_On_Null_PortalGroup(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_RemovePortalFromGroup_Throws_On_Null_PortalGroup(): controller → {Controller}", controller);
            var portal = new PortalInfo { PortalID = Constants.PORTAL_ValidPortalId };

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => controller.RemovePortalFromGroup(portal, null, false, this.userCopied));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_RemovePortalFromGroup_Throws_On_Null_PortalGroup()");
        }

        [Test]
        public void PortalGroupController_RemovePortalFromGroup_Throws_On_Null_Portal()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_RemovePortalFromGroup_Throws_On_Null_Portal()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_RemovePortalFromGroup_Throws_On_Null_Portal(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_RemovePortalFromGroup_Throws_On_Null_Portal(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_RemovePortalFromGroup_Throws_On_Null_Portal(): controller → {Controller}", controller);
            var portalGroup = new PortalGroupInfo { PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId };

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => controller.RemovePortalFromGroup(null, portalGroup, false, this.userCopied));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_RemovePortalFromGroup_Throws_On_Null_Portal()");
        }

        [Test]
        public void PortalGroupController_RemovePortalFromGroup_Throws_On_Negative_PortalGroupId()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_RemovePortalFromGroup_Throws_On_Negative_PortalGroupId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_RemovePortalFromGroup_Throws_On_Negative_PortalGroupId(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_RemovePortalFromGroup_Throws_On_Negative_PortalGroupId(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_RemovePortalFromGroup_Throws_On_Negative_PortalGroupId(): controller → {Controller}", controller);

            var portal = new PortalInfo { PortalID = Constants.PORTAL_ValidPortalId };

            PortalGroupInfo portalGroup = new PortalGroupInfo { PortalGroupId = -1 };

            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.RemovePortalFromGroup(portal, portalGroup, false, this.userCopied));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_RemovePortalFromGroup_Throws_On_Negative_PortalGroupId()");
        }

        [Test]
        public void PortalGroupController_RemovePortalFromGroup_Throws_On_Negative_PortalId()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_RemovePortalFromGroup_Throws_On_Negative_PortalId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_RemovePortalFromGroup_Throws_On_Negative_PortalId(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_RemovePortalFromGroup_Throws_On_Negative_PortalId(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_RemovePortalFromGroup_Throws_On_Negative_PortalId(): controller → {Controller}", controller);

            var portal = new PortalInfo { PortalID = -1 };

            PortalGroupInfo portalGroup = new PortalGroupInfo { PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId };

            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.RemovePortalFromGroup(portal, portalGroup, false, this.userCopied));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_RemovePortalFromGroup_Throws_On_Negative_PortalId()");
        }

        [Test]
        public void PortalGroupController_UpdatePortalGroup_Throws_On_Null_PortalGroup()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_UpdatePortalGroup_Throws_On_Null_PortalGroup()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_UpdatePortalGroup_Throws_On_Null_PortalGroup(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_UpdatePortalGroup_Throws_On_Null_PortalGroup(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_UpdatePortalGroup_Throws_On_Null_PortalGroup(): controller → {Controller}", controller);

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => controller.UpdatePortalGroup(null));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_UpdatePortalGroup_Throws_On_Null_PortalGroup()");
        }

        [Test]
        public void PortalGroupController_UpdatePortalGroup_Throws_On_Negative_PortalGroupId()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_UpdatePortalGroup_Throws_On_Negative_PortalGroupId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_UpdatePortalGroup_Throws_On_Negative_PortalGroupId(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_UpdatePortalGroup_Throws_On_Negative_PortalGroupId(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_UpdatePortalGroup_Throws_On_Negative_PortalGroupId(): controller → {Controller}", controller);

            var portalGroup = new PortalGroupInfo();
			mockEventLogger.LogDebug("PortalGroupController_UpdatePortalGroup_Throws_On_Negative_PortalGroupId(): portalGroup → {PortalGroup}", portalGroup);
			mockEventLogger.LogDebug("PortalGroupController_UpdatePortalGroup_Throws_On_Negative_PortalGroupId(): portalGroup.PortalGroupId → {Arg0}", portalGroup.PortalGroupId);
            portalGroup.PortalGroupId = Null.NullInteger;

            Assert.Throws<ArgumentOutOfRangeException>(() => controller.UpdatePortalGroup(portalGroup));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_UpdatePortalGroup_Throws_On_Negative_PortalGroupId()");
        }

        [Test]
        public void PortalGroupController_UpdatePortalGroup_Calls_DataService_On_Valid_PortalGroup()
        {
			mockEventLogger.LogDebug(">>> Entering PortalGroupController_UpdatePortalGroup_Calls_DataService_On_Valid_PortalGroup()");
            // Arrange
            MockComponentProvider.CreateDataCacheProvider();
            var mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("PortalGroupController_UpdatePortalGroup_Calls_DataService_On_Valid_PortalGroup(): mockDataService → {MockDataService}", mockDataService);
            var mockPortalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("PortalGroupController_UpdatePortalGroup_Calls_DataService_On_Valid_PortalGroup(): mockPortalController → {MockPortalController}", mockPortalController);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
			mockEventLogger.LogDebug("PortalGroupController_UpdatePortalGroup_Calls_DataService_On_Valid_PortalGroup(): controller → {Controller}", controller);

            var portalGroup = CreateValidPortalGroup();
			mockEventLogger.LogDebug("PortalGroupController_UpdatePortalGroup_Calls_DataService_On_Valid_PortalGroup(): portalGroup → {PortalGroup}", portalGroup);
			mockEventLogger.LogDebug("PortalGroupController_UpdatePortalGroup_Calls_DataService_On_Valid_PortalGroup(): portalGroup.PortalGroupId → {Arg0}", portalGroup.PortalGroupId);
			mockEventLogger.LogDebug("PortalGroupController_UpdatePortalGroup_Calls_DataService_On_Valid_PortalGroup(): portalGroup.PortalGroupName → {Arg0}", portalGroup.PortalGroupName);
			mockEventLogger.LogDebug("PortalGroupController_UpdatePortalGroup_Calls_DataService_On_Valid_PortalGroup(): portalGroup.PortalGroupDescription → {Arg0}", portalGroup.PortalGroupDescription);
            portalGroup.PortalGroupId = Constants.PORTALGROUP_UpdatePortalGroupId;
            portalGroup.PortalGroupName = Constants.PORTALGROUP_UpdateName;
            portalGroup.PortalGroupDescription = Constants.PORTALGROUP_UpdateDescription;

            // Act
            controller.UpdatePortalGroup(portalGroup);

            // Assert
            mockDataService.Verify(ds => ds.UpdatePortalGroup(portalGroup, It.IsAny<int>()));
			mockEventLogger.LogDebug("<<< Exiting PortalGroupController_UpdatePortalGroup_Calls_DataService_On_Valid_PortalGroup()");
        }

        private static DataTable CreatePortalGroupTable()
        {
			mockEventLogger.LogDebug(">>> Entering CreatePortalGroupTable()");
            // Create Categories table.
            DataTable table = new DataTable();
			mockEventLogger.LogDebug("CreatePortalGroupTable(): table → {Table}", table);

            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("PortalGroupID", typeof(int));
			mockEventLogger.LogDebug("CreatePortalGroupTable(): idColumn → {IdColumn}", idColumn);
            table.Columns.Add("MasterPortalID", typeof(int));
            table.Columns.Add("PortalGroupName", typeof(string));
            table.Columns.Add("PortalGroupDescription", typeof(string));
            table.Columns.Add("AuthenticationDomain", typeof(string));
            table.Columns.Add("CreatedByUserID", typeof(int));
            table.Columns.Add("CreatedOnDate", typeof(DateTime));
            table.Columns.Add("LastModifiedByUserID", typeof(int));
            table.Columns.Add("LastModifiedOnDate", typeof(DateTime));

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };
			mockEventLogger.LogDebug("CreatePortalGroupTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			mockEventLogger.LogDebug("<<< Exiting CreatePortalGroupTable()");
            return table;
        }

        private static string GetName(int i)
        {
			mockEventLogger.LogDebug(">>> Entering GetName(i={I})", i);
			mockEventLogger.LogDebug("<<< Exiting GetName(i={I})", i);
            return string.Format(string.Format(Constants.PORTALGROUP_ValidNameFormat, i));
        }

        private static string GetDescription(int i)
        {
			mockEventLogger.LogDebug(">>> Entering GetDescription(i={I})", i);
			mockEventLogger.LogDebug("<<< Exiting GetDescription(i={I})", i);
            return string.Format(string.Format(Constants.PORTALGROUP_ValidDescriptionFormat, i));
        }

        private static PortalGroupInfo CreateValidPortalGroup()
        {
			mockEventLogger.LogDebug(">>> Entering CreateValidPortalGroup()");
            var portalGroup = new PortalGroupInfo
            {
                PortalGroupName = Constants.PORTALGROUP_ValidName,
                PortalGroupDescription = Constants.PORTALGROUP_ValidDescription,
                MasterPortalId = Constants.PORTAL_ValidPortalId,
            };
			mockEventLogger.LogDebug("<<< Exiting CreateValidPortalGroup()");
            return portalGroup;
        }

        private static IDataReader CreateValidPortalGroupsReader(int count, int startUserId)
        {
			mockEventLogger.LogDebug(">>> Entering CreateValidPortalGroupsReader(count={Count},startUserId={StartUserId})", count, startUserId);
            DataTable table = CreatePortalGroupTable();
            for (int i = Constants.PORTALGROUP_ValidPortalGroupId; i < Constants.PORTALGROUP_ValidPortalGroupId + count; i++)
			mockEventLogger.LogDebug("CreateValidPortalGroupsReader(count={Count},startUserId={StartUserId}): table → {Table}", count, startUserId, table);
            {
                string name = (count == 1) ? Constants.PORTALGROUP_ValidName : GetName(i);
                string description = (count == 1) ? Constants.PORTALGROUP_ValidDescription : GetDescription(i);
                const string domain = "mydomain.com";
                int userId = (startUserId == Null.NullInteger) ? Constants.USER_ValidId + i : startUserId;
				mockEventLogger.LogDebug("CreateValidPortalGroupsReader(count={Count},startUserId={StartUserId}): userId → {UserId}", count, startUserId, userId);

                table.Rows.Add(new object[]
                                   {
                                       i,
                                       -1,
                                       name,
                                       description,
                                       domain,
                                       userId,
                                   });
            }

			mockEventLogger.LogDebug("<<< Exiting CreateValidPortalGroupsReader(count={Count},startUserId={StartUserId})", count, startUserId);
            return table.CreateDataReader();
        }
    }

    // ReSharper restore InconsistentNaming
}
