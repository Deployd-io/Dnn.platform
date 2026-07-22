// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Controllers.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Text;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.Services.Social.Messaging.Internal;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Services.Social.Notifications.Data;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class NotificationsControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private Mock<IDataService> mockDataService;
        private Mock<IPortalController> portalController;
        private Mock<IPortalGroupController> portalGroupController;
        private Mock<DotNetNuke.Services.Social.Messaging.Data.IDataService> mockMessagingDataService;
        private Mock<IMessagingController> mockMessagingController;
        private Mock<IInternalMessagingController> mockInternalMessagingController;
        private NotificationsController notificationsController;
        private Mock<NotificationsController> mockNotificationsController;
        private Mock<DataProvider> dataProvider;
        private Mock<CachingProvider> cachingProvider;
        private FakeServiceProvider serviceProvider;

        private DataTable dtNotificationTypes;
        private DataTable dtNotificationTypeActions;
        private DataTable dtNotificationActions;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            ComponentFactory.Container = new SimpleContainer();

            this.mockDataService = new Mock<IDataService>();
			Logger.LogDebug("SetUp(): mockDataService → {MockDataService}", mockDataService);
            this.portalController = new Mock<IPortalController>();
			Logger.LogDebug("SetUp(): portalController → {PortalController}", portalController);
            this.portalGroupController = new Mock<IPortalGroupController>();
			Logger.LogDebug("SetUp(): portalGroupController → {PortalGroupController}", portalGroupController);

            this.mockMessagingDataService = new Mock<DotNetNuke.Services.Social.Messaging.Data.IDataService>();
			Logger.LogDebug("SetUp(): mockMessagingDataService → {MockMessagingDataService}", mockMessagingDataService);
            this.dataProvider = MockComponentProvider.CreateDataProvider();
			Logger.LogDebug("SetUp(): dataProvider → {DataProvider}", dataProvider);
            this.cachingProvider = MockComponentProvider.CreateDataCacheProvider();
			Logger.LogDebug("SetUp(): cachingProvider → {CachingProvider}", cachingProvider);

            this.notificationsController = new NotificationsController(this.mockDataService.Object, this.mockMessagingDataService.Object, Mock.Of<IHostSettings>());
			Logger.LogDebug("SetUp(): notificationsController → {NotificationsController}", notificationsController);
            this.mockNotificationsController = new Mock<NotificationsController> { CallBase = true };
			Logger.LogDebug("SetUp(): mockNotificationsController → {MockNotificationsController}", mockNotificationsController);
			Logger.LogDebug("SetUp(): this.mockNotificationsController → {Arg0}", this.mockNotificationsController);

            this.mockMessagingController = new Mock<IMessagingController>();
			Logger.LogDebug("SetUp(): mockMessagingController → {MockMessagingController}", mockMessagingController);
            MessagingController.SetTestableInstance(this.mockMessagingController.Object);
            PortalController.SetTestableInstance(this.portalController.Object);
            PortalGroupController.RegisterInstance(this.portalGroupController.Object);

            this.mockInternalMessagingController = new Mock<IInternalMessagingController>();
			Logger.LogDebug("SetUp(): mockInternalMessagingController → {MockInternalMessagingController}", mockInternalMessagingController);
            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);

            DataService.RegisterInstance(this.mockDataService.Object);
            DotNetNuke.Services.Social.Messaging.Data.DataService.RegisterInstance(this.mockMessagingDataService.Object);

            this.SetupDataProvider();
            this.SetupDataTables();

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("SetUp(): services → {Services}", services);
                    services.AddSingleton(this.mockDataService.Object);
                    services.AddSingleton(this.cachingProvider.Object);
                    services.AddSingleton(this.dataProvider.Object);
                    services.AddSingleton(this.portalController.Object);
                    services.AddSingleton(this.portalGroupController.Object);
                    services.AddSingleton(this.mockMessagingDataService.Object);
                    services.AddSingleton(this.mockMessagingController.Object);
                    services.AddSingleton(this.mockInternalMessagingController.Object);
                });
				Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            ComponentFactory.Container = null;
			Logger.LogDebug("TearDown(): ComponentFactory.Container → {Arg0}", ComponentFactory.Container);
            MessagingController.ClearInstance();
            PortalController.ClearInstance();
            InternalMessagingController.ClearInstance();
            this.dtNotificationTypes?.Dispose();
            this.dtNotificationTypeActions?.Dispose();
            this.dtNotificationActions?.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void CreateNotificationType_Throws_On_Null_NotificationType()
        {
			Logger.LogDebug(">>> Entering CreateNotificationType_Throws_On_Null_NotificationType()");
            Assert.Throws<ArgumentNullException>(() => this.notificationsController.CreateNotificationType(null));
			Logger.LogDebug("<<< Exiting CreateNotificationType_Throws_On_Null_NotificationType()");
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void CreateNotificationType_Throws_On_Null_Or_Empty_Name(string name)
        {
			Logger.LogDebug(">>> Entering CreateNotificationType_Throws_On_Null_Or_Empty_Name(name={Name})", name);
            var notificationType = CreateNewNotificationType();
            notificationType.Name = name;
			Logger.LogDebug("CreateNotificationType_Throws_On_Null_Or_Empty_Name(name={Name}): notificationType → {NotificationType}", name, notificationType);
			Logger.LogDebug("CreateNotificationType_Throws_On_Null_Or_Empty_Name(name={Name}): notificationType.Name → {Arg1}", name, notificationType.Name);

            Assert.Throws<ArgumentException>(() => this.notificationsController.CreateNotificationType(notificationType));
			Logger.LogDebug("<<< Exiting CreateNotificationType_Throws_On_Null_Or_Empty_Name(name={Name})", name);
        }

        [Test]
        public void CreateNotificationType_Calls_DataService_CreateNotificationType()
        {
			Logger.LogDebug(">>> Entering CreateNotificationType_Calls_DataService_CreateNotificationType()");
            this.mockNotificationsController.Setup(nc => nc.GetCurrentUserId()).Returns(Constants.UserID_User12);

            this.mockDataService
                .Setup(ds => ds.CreateNotificationType(Constants.Messaging_NotificationTypeName, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), Constants.UserID_User12, It.IsAny<bool>()))
                .Verifiable();

            this.mockNotificationsController.Object.CreateNotificationType(CreateNewNotificationType());

            this.mockDataService.Verify();
			Logger.LogDebug("<<< Exiting CreateNotificationType_Calls_DataService_CreateNotificationType()");
        }

        [Test]
        [TestCase(int.MaxValue, int.MaxValue)]
        [TestCase(1, 1)]
        [TestCase(0, 0)]
        public void CreateNotificationType_Returns_Object_With_Valid_TimeToLive(int actualTimeToLiveTotalMinutes, int expectedTimeToLiveTotalMinutes)
        {
			Logger.LogDebug(">>> Entering CreateNotificationType_Returns_Object_With_Valid_TimeToLive(actualTimeToLiveTotalMinutes={ActualTimeToLiveTotalMinutes},expectedTimeToLiveTotalMinutes={ExpectedTimeToLiveTotalMinutes})", actualTimeToLiveTotalMinutes, expectedTimeToLiveTotalMinutes);
            var actualTimeToLive = TimeSpan.FromMinutes(actualTimeToLiveTotalMinutes);

			Logger.LogDebug("CreateNotificationType_Returns_Object_With_Valid_TimeToLive(actualTimeToLiveTotalMinutes={ActualTimeToLiveTotalMinutes},expectedTimeToLiveTotalMinutes={ExpectedTimeToLiveTotalMinutes}): actualTimeToLive → {ActualTimeToLive}", actualTimeToLiveTotalMinutes, expectedTimeToLiveTotalMinutes, actualTimeToLive);
            var notificationType = CreateNewNotificationType();
            notificationType.TimeToLive = actualTimeToLive;
			Logger.LogDebug("CreateNotificationType_Returns_Object_With_Valid_TimeToLive(actualTimeToLiveTotalMinutes={ActualTimeToLiveTotalMinutes},expectedTimeToLiveTotalMinutes={ExpectedTimeToLiveTotalMinutes}): notificationType → {NotificationType}", actualTimeToLiveTotalMinutes, expectedTimeToLiveTotalMinutes, notificationType);
			Logger.LogDebug("CreateNotificationType_Returns_Object_With_Valid_TimeToLive(actualTimeToLiveTotalMinutes={ActualTimeToLiveTotalMinutes},expectedTimeToLiveTotalMinutes={ExpectedTimeToLiveTotalMinutes}): notificationType.TimeToLive → {Arg2}", actualTimeToLiveTotalMinutes, expectedTimeToLiveTotalMinutes, notificationType.TimeToLive);
            this.notificationsController.CreateNotificationType(notificationType);

            Assert.That((int)notificationType.TimeToLive.TotalMinutes, Is.EqualTo(expectedTimeToLiveTotalMinutes));
			Logger.LogDebug("<<< Exiting CreateNotificationType_Returns_Object_With_Valid_TimeToLive(actualTimeToLiveTotalMinutes={ActualTimeToLiveTotalMinutes},expectedTimeToLiveTotalMinutes={ExpectedTimeToLiveTotalMinutes})", actualTimeToLiveTotalMinutes, expectedTimeToLiveTotalMinutes);
        }

        [Test]
        public void CreateNotificationType_Makes_Valid_Object()
        {
			Logger.LogDebug(">>> Entering CreateNotificationType_Makes_Valid_Object()");
            var expectedNotificationType = CreateValidNotificationType();

			Logger.LogDebug("CreateNotificationType_Makes_Valid_Object(): expectedNotificationType → {ExpectedNotificationType}", expectedNotificationType);
            this.mockNotificationsController.Setup(nc => nc.GetCurrentUserId()).Returns(Constants.UserID_User12);

            this.mockDataService
                .Setup(ds => ds.CreateNotificationType(
                    expectedNotificationType.Name,
                    expectedNotificationType.Description,
                    (int)expectedNotificationType.TimeToLive.TotalMinutes,
                    expectedNotificationType.DesktopModuleId,
                    Constants.UserID_User12, false))
                .Returns(Constants.Messaging_NotificationTypeId);

            var actualNotificationType = CreateNewNotificationType();
            this.mockNotificationsController.Object.CreateNotificationType(actualNotificationType);
			Logger.LogDebug("CreateNotificationType_Makes_Valid_Object(): actualNotificationType → {ActualNotificationType}", actualNotificationType);

            Assert.That(actualNotificationType, Is.EqualTo(expectedNotificationType).Using(new NotificationTypeComparer()));
			Logger.LogDebug("<<< Exiting CreateNotificationType_Makes_Valid_Object()");
        }

        [Test]
        public void DeleteNotificationType_Calls_DataService_DeleteNotificationType()
        {
			Logger.LogDebug(">>> Entering DeleteNotificationType_Calls_DataService_DeleteNotificationType()");
            this.mockDataService.Setup(ds => ds.DeleteNotificationType(Constants.Messaging_NotificationTypeId)).Verifiable();
            this.mockNotificationsController.Setup(nc => nc.RemoveNotificationTypeCache());
            this.mockNotificationsController.Object.DeleteNotificationType(Constants.Messaging_NotificationTypeId);
            this.mockDataService.Verify();
			Logger.LogDebug("<<< Exiting DeleteNotificationType_Calls_DataService_DeleteNotificationType()");
        }

        [Test]
        public void DeleteNotificationType_Removes_Cache_Object()
        {
			Logger.LogDebug(">>> Entering DeleteNotificationType_Removes_Cache_Object()");
            this.mockDataService.Setup(ds => ds.DeleteNotificationType(Constants.Messaging_NotificationTypeId));
            this.mockNotificationsController.Setup(nc => nc.RemoveNotificationTypeCache()).Verifiable();
            this.mockNotificationsController.Object.DeleteNotificationType(Constants.Messaging_NotificationTypeId);
            this.mockNotificationsController.Verify();
			Logger.LogDebug("<<< Exiting DeleteNotificationType_Removes_Cache_Object()");
        }

        [Test]
        public void GetNotificationType_By_Id_Gets_Object_From_Cache()
        {
			Logger.LogDebug(">>> Entering GetNotificationType_By_Id_Gets_Object_From_Cache()");
            this.cachingProvider.Object.PurgeCache();
            this.cachingProvider.Setup(cp => cp.GetItem(It.IsAny<string>())).Verifiable();
            this.notificationsController.GetNotificationType(Constants.Messaging_NotificationTypeId);
            this.cachingProvider.Verify();
			Logger.LogDebug("<<< Exiting GetNotificationType_By_Id_Gets_Object_From_Cache()");
        }

        [Test]
        public void GetNotificationType_By_Id_Calls_DataService_GetNotificationType_When_Object_Is_Not_In_Cache()
        {
			Logger.LogDebug(">>> Entering GetNotificationType_By_Id_Calls_DataService_GetNotificationType_When_Object_Is_Not_In_Cache()");
            this.cachingProvider.Object.PurgeCache();

            var messageTypeDataTable = new DataTable();

			Logger.LogDebug("GetNotificationType_By_Id_Calls_DataService_GetNotificationType_When_Object_Is_Not_In_Cache(): messageTypeDataTable → {MessageTypeDataTable}", messageTypeDataTable);
            this.mockDataService
                .Setup(ds => ds.GetNotificationType(Constants.Messaging_NotificationTypeId))
                .Returns(messageTypeDataTable.CreateDataReader())
                .Verifiable();

            this.notificationsController.GetNotificationType(Constants.Messaging_NotificationTypeId);

            this.mockDataService.Verify();
			Logger.LogDebug("<<< Exiting GetNotificationType_By_Id_Calls_DataService_GetNotificationType_When_Object_Is_Not_In_Cache()");
        }

        [Test]
        public void GetNotificationType_By_Id_Returns_Valid_Object()
        {
			Logger.LogDebug(">>> Entering GetNotificationType_By_Id_Returns_Valid_Object()");
            this.cachingProvider.Object.PurgeCache();

            var expectedNotificationType = CreateValidNotificationType();

			Logger.LogDebug("GetNotificationType_By_Id_Returns_Valid_Object(): expectedNotificationType → {ExpectedNotificationType}", expectedNotificationType);
            this.dtNotificationTypes.Rows.Clear();

            this.dtNotificationTypes.Rows.Add(
                expectedNotificationType.NotificationTypeId,
                expectedNotificationType.Name,
                expectedNotificationType.Description,
                (int)expectedNotificationType.TimeToLive.TotalMinutes,
                expectedNotificationType.DesktopModuleId);

            this.mockDataService
                .Setup(ds => ds.GetNotificationType(Constants.Messaging_NotificationTypeId))
                .Returns(this.dtNotificationTypes.CreateDataReader());

            var actualNotificationType = this.notificationsController.GetNotificationType(Constants.Messaging_NotificationTypeId);

			Logger.LogDebug("GetNotificationType_By_Id_Returns_Valid_Object(): actualNotificationType → {ActualNotificationType}", actualNotificationType);
            Assert.That(actualNotificationType, Is.EqualTo(expectedNotificationType).Using(new NotificationTypeComparer()));
			Logger.LogDebug("<<< Exiting GetNotificationType_By_Id_Returns_Valid_Object()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void GetNotificationType_By_Name_Throws_On_Null_Or_Empty_Name(string name)
        {
			Logger.LogDebug(">>> Entering GetNotificationType_By_Name_Throws_On_Null_Or_Empty_Name(name={Name})", name);
            Assert.Throws<ArgumentException>(() => this.notificationsController.GetNotificationType(name));
			Logger.LogDebug("<<< Exiting GetNotificationType_By_Name_Throws_On_Null_Or_Empty_Name(name={Name})", name);
        }

        [Test]
        public void GetNotificationType_By_Name_Gets_Object_From_Cache()
        {
			Logger.LogDebug(">>> Entering GetNotificationType_By_Name_Gets_Object_From_Cache()");
            this.cachingProvider.Object.PurgeCache();
            this.cachingProvider.Setup(cp => cp.GetItem(It.IsAny<string>())).Verifiable();
            this.notificationsController.GetNotificationType(Constants.Messaging_NotificationTypeName);
            this.cachingProvider.Verify();
			Logger.LogDebug("<<< Exiting GetNotificationType_By_Name_Gets_Object_From_Cache()");
        }

        [Test]
        public void GetNotificationType_By_Name_Calls_DataService_GetNotificationTypeByName_When_Object_Is_Not_In_Cache()
        {
			Logger.LogDebug(">>> Entering GetNotificationType_By_Name_Calls_DataService_GetNotificationTypeByName_When_Object_Is_Not_In_Cache()");
            this.cachingProvider.Object.PurgeCache();

            this.mockDataService
                .Setup(ds => ds.GetNotificationTypeByName(Constants.Messaging_NotificationTypeName))
                .Returns(this.dtNotificationTypes.CreateDataReader())
                .Verifiable();

            this.notificationsController.GetNotificationType(Constants.Messaging_NotificationTypeName);

            this.mockDataService.Verify();
			Logger.LogDebug("<<< Exiting GetNotificationType_By_Name_Calls_DataService_GetNotificationTypeByName_When_Object_Is_Not_In_Cache()");
        }

        [Test]
        public void GetNotificationType_By_Name_Returns_Valid_Object()
        {
			Logger.LogDebug(">>> Entering GetNotificationType_By_Name_Returns_Valid_Object()");
            this.cachingProvider.Object.PurgeCache();

            var expectedNotificationType = CreateValidNotificationType();

			Logger.LogDebug("GetNotificationType_By_Name_Returns_Valid_Object(): expectedNotificationType → {ExpectedNotificationType}", expectedNotificationType);
            this.dtNotificationTypes.Rows.Clear();

            this.dtNotificationTypes.Rows.Add(
                expectedNotificationType.NotificationTypeId,
                expectedNotificationType.Name,
                expectedNotificationType.Description,
                (int)expectedNotificationType.TimeToLive.TotalMinutes,
                expectedNotificationType.DesktopModuleId);

            this.mockDataService
                .Setup(ds => ds.GetNotificationTypeByName(Constants.Messaging_NotificationTypeName))
                .Returns(this.dtNotificationTypes.CreateDataReader());

            var actualNotificationType = this.notificationsController.GetNotificationType(Constants.Messaging_NotificationTypeName);

			Logger.LogDebug("GetNotificationType_By_Name_Returns_Valid_Object(): actualNotificationType → {ActualNotificationType}", actualNotificationType);
            Assert.That(actualNotificationType, Is.EqualTo(expectedNotificationType).Using(new NotificationTypeComparer()));
			Logger.LogDebug("<<< Exiting GetNotificationType_By_Name_Returns_Valid_Object()");
        }

        [Test]
        public void SetNotificationTypeActions_Throws_On_Null()
        {
			Logger.LogDebug(">>> Entering SetNotificationTypeActions_Throws_On_Null()");
            Assert.Throws<ArgumentNullException>(() => this.notificationsController.SetNotificationTypeActions(null, Constants.Messaging_NotificationTypeId));
			Logger.LogDebug("<<< Exiting SetNotificationTypeActions_Throws_On_Null()");
        }

        [Test]
        public void SetNotificationTypeActions_Throws_On_EmptyList()
        {
			Logger.LogDebug(">>> Entering SetNotificationTypeActions_Throws_On_EmptyList()");
            Assert.Throws<ArgumentException>(() => this.notificationsController.SetNotificationTypeActions(new List<NotificationTypeAction>(), Constants.Messaging_NotificationTypeId));
			Logger.LogDebug("<<< Exiting SetNotificationTypeActions_Throws_On_EmptyList()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void SetNotificationTypeActions_Throws_On_Null_Or_Empty_Name(string name)
        {
			Logger.LogDebug(">>> Entering SetNotificationTypeActions_Throws_On_Null_Or_Empty_Name(name={Name})", name);
            var action = CreateNewNotificationTypeAction();
            action.NameResourceKey = name;
			Logger.LogDebug("SetNotificationTypeActions_Throws_On_Null_Or_Empty_Name(name={Name}): action → {Action}", name, action);
			Logger.LogDebug("SetNotificationTypeActions_Throws_On_Null_Or_Empty_Name(name={Name}): action.NameResourceKey → {Arg1}", name, action.NameResourceKey);
            Assert.Throws<ArgumentException>(() => this.notificationsController.SetNotificationTypeActions(new[] { action }, Constants.Messaging_NotificationTypeId));
			Logger.LogDebug("<<< Exiting SetNotificationTypeActions_Throws_On_Null_Or_Empty_Name(name={Name})", name);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void SetNotificationTypeActions_Throws_On_Null_Or_Empty_APICall(string apiCall)
        {
			Logger.LogDebug(">>> Entering SetNotificationTypeActions_Throws_On_Null_Or_Empty_APICall(apiCall={ApiCall})", apiCall);
            var action = CreateNewNotificationTypeAction();
            action.APICall = apiCall;
			Logger.LogDebug("SetNotificationTypeActions_Throws_On_Null_Or_Empty_APICall(apiCall={ApiCall}): action → {Action}", apiCall, action);
			Logger.LogDebug("SetNotificationTypeActions_Throws_On_Null_Or_Empty_APICall(apiCall={ApiCall}): action.APICall → {Arg1}", apiCall, action.APICall);
            Assert.Throws<ArgumentException>(() => this.notificationsController.SetNotificationTypeActions(new[] { action }, Constants.Messaging_NotificationTypeId));
			Logger.LogDebug("<<< Exiting SetNotificationTypeActions_Throws_On_Null_Or_Empty_APICall(apiCall={ApiCall})", apiCall);
        }

        [Test]
        public void SetNotificationTypeActions_Calls_DataService_AddNotificationTypeAction_For_Each_Of_Two_Actions()
        {
			Logger.LogDebug(">>> Entering SetNotificationTypeActions_Calls_DataService_AddNotificationTypeAction_For_Each_Of_Two_Actions()");
            this.mockNotificationsController.Setup(nc => nc.GetCurrentUserId()).Returns(Constants.UserID_User12);

            // mockDataService
            //                .Setup(ds => ds.AddNotificationTypeAction(
            //                    Constants.Messaging_NotificationTypeId,
            //                    Constants.Messaging_NotificationTypeActionNameResourceKey,
            //                    Constants.Messaging_NotificationTypeActionDescriptionResourceKey,
            //                    Constants.Messaging_NotificationTypeActionConfirmResourceKey,
            //                    Constants.Messaging_NotificationTypeActionAPICall,
            //                    Constants.UserID_User12))
            //                .Verifiable();
            this.mockNotificationsController.Setup(nc => nc.GetNotificationTypeAction(It.IsAny<int>()));

            this.mockNotificationsController.Object.SetNotificationTypeActions(
                new[] { CreateNewNotificationTypeAction(), CreateNewNotificationTypeAction() },
                Constants.Messaging_NotificationTypeId);

            this.mockDataService.Verify(
                x => x.AddNotificationTypeAction(
                Constants.Messaging_NotificationTypeId,
                Constants.Messaging_NotificationTypeActionNameResourceKey,
                Constants.Messaging_NotificationTypeActionDescriptionResourceKey,
                Constants.Messaging_NotificationTypeActionConfirmResourceKey,
                Constants.Messaging_NotificationTypeActionAPICall,
                Constants.UserID_User12), Times.Exactly(2));
				Logger.LogDebug("<<< Exiting SetNotificationTypeActions_Calls_DataService_AddNotificationTypeAction_For_Each_Of_Two_Actions()");
        }

        [Test]
        public void SetNotificationTypeActions_Sets_NotificationTypeActionId_And_NotificationTypeId()
        {
			Logger.LogDebug(">>> Entering SetNotificationTypeActions_Sets_NotificationTypeActionId_And_NotificationTypeId()");
            var expectedNotificationTypeAction = CreateValidNotificationTypeAction();

			Logger.LogDebug("SetNotificationTypeActions_Sets_NotificationTypeActionId_And_NotificationTypeId(): expectedNotificationTypeAction → {ExpectedNotificationTypeAction}", expectedNotificationTypeAction);
            this.mockNotificationsController.Setup(nc => nc.GetCurrentUserId()).Returns(Constants.UserID_User12);

            this.mockDataService
                .Setup(ds => ds.AddNotificationTypeAction(
                    expectedNotificationTypeAction.NotificationTypeId,
                    expectedNotificationTypeAction.NameResourceKey,
                    expectedNotificationTypeAction.DescriptionResourceKey,
                    expectedNotificationTypeAction.ConfirmResourceKey,
                    expectedNotificationTypeAction.APICall,
                    Constants.UserID_User12))
                .Returns(expectedNotificationTypeAction.NotificationTypeActionId);

            this.mockNotificationsController
                .Setup(nc => nc.GetNotificationTypeAction(expectedNotificationTypeAction.NotificationTypeActionId))
                .Returns(expectedNotificationTypeAction);

            var action = CreateNewNotificationTypeAction();
            this.mockNotificationsController.Object.SetNotificationTypeActions([action], expectedNotificationTypeAction.NotificationTypeId);
			Logger.LogDebug("SetNotificationTypeActions_Sets_NotificationTypeActionId_And_NotificationTypeId(): action → {Action}", action);

            Assert.That(action, Is.EqualTo(expectedNotificationTypeAction).Using(new NotificationTypeActionComparer()));
			Logger.LogDebug("<<< Exiting SetNotificationTypeActions_Sets_NotificationTypeActionId_And_NotificationTypeId()");
        }

        [Test]
        public void DeleteNotificationTypeAction_Calls_DataService_DeleteNotificationTypeAction()
        {
			Logger.LogDebug(">>> Entering DeleteNotificationTypeAction_Calls_DataService_DeleteNotificationTypeAction()");
            this.mockDataService.Setup(ds => ds.DeleteNotificationTypeAction(Constants.Messaging_NotificationTypeActionId)).Verifiable();
            this.mockNotificationsController.Setup(nc => nc.RemoveNotificationTypeActionCache());
            this.mockNotificationsController.Object.DeleteNotificationTypeAction(Constants.Messaging_NotificationTypeActionId);
            this.mockDataService.Verify();
			Logger.LogDebug("<<< Exiting DeleteNotificationTypeAction_Calls_DataService_DeleteNotificationTypeAction()");
        }

        [Test]
        public void DeleteNotificationTypeAction_Removes_Cache_Object()
        {
			Logger.LogDebug(">>> Entering DeleteNotificationTypeAction_Removes_Cache_Object()");
            this.mockDataService.Setup(ds => ds.DeleteNotificationTypeAction(Constants.Messaging_NotificationTypeActionId));
            this.mockNotificationsController.Setup(nc => nc.RemoveNotificationTypeActionCache()).Verifiable();
            this.mockNotificationsController.Object.DeleteNotificationTypeAction(Constants.Messaging_NotificationTypeActionId);
            this.mockNotificationsController.Verify();
			Logger.LogDebug("<<< Exiting DeleteNotificationTypeAction_Removes_Cache_Object()");
        }

        [Test]
        public void GetNotificationTypeAction_By_Id_Gets_Object_From_Cache()
        {
			Logger.LogDebug(">>> Entering GetNotificationTypeAction_By_Id_Gets_Object_From_Cache()");
            this.cachingProvider.Object.PurgeCache();
            this.cachingProvider.Setup(cp => cp.GetItem(It.IsAny<string>())).Verifiable();
            this.notificationsController.GetNotificationTypeAction(Constants.Messaging_NotificationTypeActionId);
            this.cachingProvider.Verify();
			Logger.LogDebug("<<< Exiting GetNotificationTypeAction_By_Id_Gets_Object_From_Cache()");
        }

        [Test]
        public void GetNotificationTypeAction_By_Id_Calls_DataService_GetNotificationTypeAction_When_Object_Is_Not_In_Cache()
        {
			Logger.LogDebug(">>> Entering GetNotificationTypeAction_By_Id_Calls_DataService_GetNotificationTypeAction_When_Object_Is_Not_In_Cache()");
            this.cachingProvider.Object.PurgeCache();

            this.mockDataService
                .Setup(ds => ds.GetNotificationTypeAction(Constants.Messaging_NotificationTypeActionId))
                .Returns(this.dtNotificationTypeActions.CreateDataReader())
                .Verifiable();

            this.notificationsController.GetNotificationTypeAction(Constants.Messaging_NotificationTypeActionId);

            this.mockDataService.Verify();
			Logger.LogDebug("<<< Exiting GetNotificationTypeAction_By_Id_Calls_DataService_GetNotificationTypeAction_When_Object_Is_Not_In_Cache()");
        }

        [Test]
        public void GetNotificationTypeAction_By_Id_Returns_Valid_Object()
        {
			Logger.LogDebug(">>> Entering GetNotificationTypeAction_By_Id_Returns_Valid_Object()");
            this.cachingProvider.Object.PurgeCache();

            var expectedNotificationTypeAction = CreateValidNotificationTypeAction();

			Logger.LogDebug("GetNotificationTypeAction_By_Id_Returns_Valid_Object(): expectedNotificationTypeAction → {ExpectedNotificationTypeAction}", expectedNotificationTypeAction);
            this.dtNotificationTypeActions.Clear();

            this.dtNotificationTypeActions.Rows.Add(
                expectedNotificationTypeAction.NotificationTypeActionId,
                expectedNotificationTypeAction.NotificationTypeId,
                expectedNotificationTypeAction.NameResourceKey,
                expectedNotificationTypeAction.DescriptionResourceKey,
                expectedNotificationTypeAction.ConfirmResourceKey,
                expectedNotificationTypeAction.Order,
                expectedNotificationTypeAction.APICall);

            this.mockDataService
                .Setup(ds => ds.GetNotificationTypeAction(Constants.Messaging_NotificationTypeActionId))
                .Returns(this.dtNotificationTypeActions.CreateDataReader);

            var actualNotificationTypeAction = this.notificationsController.GetNotificationTypeAction(Constants.Messaging_NotificationTypeActionId);

			Logger.LogDebug("GetNotificationTypeAction_By_Id_Returns_Valid_Object(): actualNotificationTypeAction → {ActualNotificationTypeAction}", actualNotificationTypeAction);
            Assert.That(actualNotificationTypeAction, Is.EqualTo(expectedNotificationTypeAction).Using(new NotificationTypeActionComparer()));
			Logger.LogDebug("<<< Exiting GetNotificationTypeAction_By_Id_Returns_Valid_Object()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void GetNotificationTypeAction_By_Name_Throws_On_Null_Or_Empty_Name(string name)
        {
			Logger.LogDebug(">>> Entering GetNotificationTypeAction_By_Name_Throws_On_Null_Or_Empty_Name(name={Name})", name);
            Assert.Throws<ArgumentException>(() => this.notificationsController.GetNotificationTypeAction(Constants.Messaging_NotificationTypeId, name));
			Logger.LogDebug("<<< Exiting GetNotificationTypeAction_By_Name_Throws_On_Null_Or_Empty_Name(name={Name})", name);
        }

        [Test]
        public void GetNotificationTypeAction_By_Name_Gets_Object_From_Cache()
        {
			Logger.LogDebug(">>> Entering GetNotificationTypeAction_By_Name_Gets_Object_From_Cache()");
            this.cachingProvider.Object.PurgeCache();
            this.cachingProvider.Setup(cp => cp.GetItem(It.IsAny<string>())).Verifiable();
            this.notificationsController.GetNotificationTypeAction(Constants.Messaging_NotificationTypeId, Constants.Messaging_NotificationTypeActionNameResourceKey);
            this.cachingProvider.Verify();
			Logger.LogDebug("<<< Exiting GetNotificationTypeAction_By_Name_Gets_Object_From_Cache()");
        }

        [Test]
        public void GetNotificationTypeAction_By_Name_Calls_DataService_GetNotificationTypeActionByName_When_Object_Is_Not_In_Cache()
        {
			Logger.LogDebug(">>> Entering GetNotificationTypeAction_By_Name_Calls_DataService_GetNotificationTypeActionByName_When_Object_Is_Not_In_Cache()");
            this.cachingProvider.Object.PurgeCache();

            this.mockDataService
                .Setup(ds => ds.GetNotificationTypeActionByName(
                    Constants.Messaging_NotificationTypeId,
                    Constants.Messaging_NotificationTypeActionNameResourceKey))
                .Returns(this.dtNotificationTypeActions.CreateDataReader())
                .Verifiable();

            this.notificationsController.GetNotificationTypeAction(Constants.Messaging_NotificationTypeId, Constants.Messaging_NotificationTypeActionNameResourceKey);

            this.mockDataService.Verify();
			Logger.LogDebug("<<< Exiting GetNotificationTypeAction_By_Name_Calls_DataService_GetNotificationTypeActionByName_When_Object_Is_Not_In_Cache()");
        }

        [Test]
        public void GetNotificationTypeAction_By_Name_Returns_Valid_Object()
        {
			Logger.LogDebug(">>> Entering GetNotificationTypeAction_By_Name_Returns_Valid_Object()");
            this.cachingProvider.Object.PurgeCache();

            var expectedNotificationTypeAction = CreateValidNotificationTypeAction();

			Logger.LogDebug("GetNotificationTypeAction_By_Name_Returns_Valid_Object(): expectedNotificationTypeAction → {ExpectedNotificationTypeAction}", expectedNotificationTypeAction);
            this.dtNotificationTypeActions.Clear();

            this.dtNotificationTypeActions.Rows.Add(
                expectedNotificationTypeAction.NotificationTypeActionId,
                expectedNotificationTypeAction.NotificationTypeId,
                expectedNotificationTypeAction.NameResourceKey,
                expectedNotificationTypeAction.DescriptionResourceKey,
                expectedNotificationTypeAction.ConfirmResourceKey,
                expectedNotificationTypeAction.Order,
                expectedNotificationTypeAction.APICall);

            this.mockDataService
                .Setup(ds => ds.GetNotificationTypeActionByName(
                    Constants.Messaging_NotificationTypeId,
                    Constants.Messaging_NotificationTypeActionNameResourceKey))
                .Returns(this.dtNotificationTypeActions.CreateDataReader());

            var actualNotificationTypeAction = this.notificationsController.GetNotificationTypeAction(Constants.Messaging_NotificationTypeId, Constants.Messaging_NotificationTypeActionNameResourceKey);

			Logger.LogDebug("GetNotificationTypeAction_By_Name_Returns_Valid_Object(): actualNotificationTypeAction → {ActualNotificationTypeAction}", actualNotificationTypeAction);
            Assert.That(actualNotificationTypeAction, Is.EqualTo(expectedNotificationTypeAction).Using(new NotificationTypeActionComparer()));
			Logger.LogDebug("<<< Exiting GetNotificationTypeAction_By_Name_Returns_Valid_Object()");
        }

        [Test]
        public void GetNotificationTypeActions_Calls_DataService_GetNotificationTypeActions()
        {
			Logger.LogDebug(">>> Entering GetNotificationTypeActions_Calls_DataService_GetNotificationTypeActions()");
            this.mockDataService
                .Setup(ds => ds.GetNotificationTypeActions(Constants.Messaging_NotificationTypeId))
                .Returns(this.dtNotificationTypeActions.CreateDataReader())
                .Verifiable();

            this.notificationsController.GetNotificationTypeActions(Constants.Messaging_NotificationTypeId);

            this.mockDataService.Verify();
			Logger.LogDebug("<<< Exiting GetNotificationTypeActions_Calls_DataService_GetNotificationTypeActions()");
        }

        [Test]
        public void GetNotificationTypeActions_Returns_Valid_Object()
        {
			Logger.LogDebug(">>> Entering GetNotificationTypeActions_Returns_Valid_Object()");
            var expectedNotificationTypeAction = CreateValidNotificationTypeAction();

			Logger.LogDebug("GetNotificationTypeActions_Returns_Valid_Object(): expectedNotificationTypeAction → {ExpectedNotificationTypeAction}", expectedNotificationTypeAction);
            this.dtNotificationTypeActions.Clear();

            this.dtNotificationTypeActions.Rows.Add(
                expectedNotificationTypeAction.NotificationTypeActionId,
                expectedNotificationTypeAction.NotificationTypeId,
                expectedNotificationTypeAction.NameResourceKey,
                expectedNotificationTypeAction.DescriptionResourceKey,
                expectedNotificationTypeAction.ConfirmResourceKey,
                expectedNotificationTypeAction.Order,
                expectedNotificationTypeAction.APICall);

            this.mockDataService.Setup(ds => ds.GetNotificationTypeActions(Constants.Messaging_NotificationTypeId)).Returns(this.dtNotificationTypeActions.CreateDataReader());

            var actualNotificationTypeActions = this.notificationsController.GetNotificationTypeActions(Constants.Messaging_NotificationTypeId);

			Logger.LogDebug("GetNotificationTypeActions_Returns_Valid_Object(): actualNotificationTypeActions → {ActualNotificationTypeActions}", actualNotificationTypeActions);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(actualNotificationTypeActions, Has.Count.EqualTo(1));
                Assert.That(actualNotificationTypeActions[0], Is.EqualTo(expectedNotificationTypeAction).Using(new NotificationTypeActionComparer()));
            }
			Logger.LogDebug("<<< Exiting GetNotificationTypeActions_Returns_Valid_Object()");
        }

        [Test]
        public void SendNotification_Sets_Empty_SenderUserId_With_Admin()
        {
			Logger.LogDebug(">>> Entering SendNotification_Sets_Empty_SenderUserId_With_Admin()");
            var adminUser = new UserInfo
            {
                UserID = Constants.UserID_Admin,
                DisplayName = Constants.UserDisplayName_Admin,
                PortalID = Constants.CONTENT_ValidPortalId,
            };

            this.mockNotificationsController.Setup(nc => nc.GetAdminUser()).Returns(adminUser);

            this.mockNotificationsController
                .Setup(nc => nc.SendNotification(
                    It.IsAny<Notification>(),
                    Constants.PORTAL_Zero,
                    It.IsAny<IList<RoleInfo>>(),
                    It.IsAny<IList<UserInfo>>()));

            var notification = CreateUnsavedNotification();

			Logger.LogDebug("SendNotification_Sets_Empty_SenderUserId_With_Admin(): notification → {Notification}", notification);
            this.mockNotificationsController.Object.SendNotification(
                notification,
                Constants.PORTAL_Zero,
                new List<RoleInfo>(),
                new List<UserInfo>());

            Assert.That(notification.SenderUserID, Is.EqualTo(adminUser.UserID));
			Logger.LogDebug("<<< Exiting SendNotification_Sets_Empty_SenderUserId_With_Admin()");
        }

        [Test]
        [TestCase(null, null)]
        [TestCase(null, "")]
        [TestCase("", null)]
        [TestCase("", "")]
        public void SendNotification_Throws_On_Null_Or_Empty_Subject_And_Body(string subject, string body)
        {
            var notification = CreateUnsavedNotification();
            notification.Subject = subject;
            notification.Body = body;

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this.portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);

            Assert.Throws<ArgumentException>(() => this.notificationsController.SendNotification(notification, Constants.PORTAL_Zero, new List<RoleInfo>(), new List<UserInfo>()));
        }

        [Test]
        public void SendNotification_Throws_On_Null_Roles_And_Users()
        {
			Logger.LogDebug(">>> Entering SendNotification_Throws_On_Null_Roles_And_Users()");
            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this.portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);
			Logger.LogDebug("SendNotification_Throws_On_Null_Roles_And_Users(): mockPortalInfo → {MockPortalInfo}", mockPortalInfo);

            Assert.Throws<ArgumentException>(() => this.notificationsController.SendNotification(CreateUnsavedNotification(), Constants.PORTAL_Zero, null, null));
			Logger.LogDebug("<<< Exiting SendNotification_Throws_On_Null_Roles_And_Users()");
        }

        [Test]
        public void SendNotification_Throws_On_Large_Subject()
        {
			Logger.LogDebug(">>> Entering SendNotification_Throws_On_Large_Subject()");
            var notification = CreateUnsavedNotification();
            var subject = new StringBuilder();
			Logger.LogDebug("SendNotification_Throws_On_Large_Subject(): notification → {Notification}", notification);
			Logger.LogDebug("SendNotification_Throws_On_Large_Subject(): subject → {Subject}", subject);
            for (var i = 0; i <= 40; i++)
            {
                subject.Append("1234567890");
            }

            notification.Subject = subject.ToString();

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this.portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);
			Logger.LogDebug("SendNotification_Throws_On_Large_Subject(): mockPortalInfo → {MockPortalInfo}", mockPortalInfo);

            Assert.Throws<ArgumentException>(() => this.notificationsController.SendNotification(notification, Constants.PORTAL_Zero, new List<RoleInfo>(), new List<UserInfo>()));
			Logger.LogDebug("<<< Exiting SendNotification_Throws_On_Large_Subject()");
        }

        [Test]
        public void SendNotification_Throws_On_Roles_And_Users_With_No_DisplayNames()
        {
			Logger.LogDebug(">>> Entering SendNotification_Throws_On_Roles_And_Users_With_No_DisplayNames()");
            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this.portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);
			Logger.LogDebug("SendNotification_Throws_On_Roles_And_Users_With_No_DisplayNames(): mockPortalInfo → {MockPortalInfo}", mockPortalInfo);

            Assert.Throws<ArgumentException>(() => this.notificationsController.SendNotification(CreateUnsavedNotification(), Constants.PORTAL_Zero, new List<RoleInfo>(), new List<UserInfo>()));
			Logger.LogDebug("<<< Exiting SendNotification_Throws_On_Roles_And_Users_With_No_DisplayNames()");
        }

        [Test]
        public void SendNotification_Throws_On_Large_To_List()
        {
			Logger.LogDebug(">>> Entering SendNotification_Throws_On_Large_To_List()");
            var roles = new List<RoleInfo>();
            var users = new List<UserInfo>();
			Logger.LogDebug("SendNotification_Throws_On_Large_To_List(): roles → {Roles}", roles);
			Logger.LogDebug("SendNotification_Throws_On_Large_To_List(): users → {Users}", users);

            for (var i = 0; i <= 100; i++)
            {
                roles.Add(new RoleInfo { RoleName = "1234567890" });
                users.Add(new UserInfo { DisplayName = "1234567890" });
            }

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this.portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);
			Logger.LogDebug("SendNotification_Throws_On_Large_To_List(): mockPortalInfo → {MockPortalInfo}", mockPortalInfo);

            Assert.Throws<ArgumentException>(() => this.notificationsController.SendNotification(CreateUnsavedNotification(), Constants.PORTAL_Zero, roles, users));
			Logger.LogDebug("<<< Exiting SendNotification_Throws_On_Large_To_List()");
        }

        [Test]
        public void SendNotification_Calls_DataService_On_Valid_Notification()
        {
			Logger.LogDebug(">>> Entering SendNotification_Calls_DataService_On_Valid_Notification()");
            var adminUser = new UserInfo
            {
                UserID = Constants.UserID_Admin,
                DisplayName = Constants.UserDisplayName_Admin,
                PortalID = Constants.PORTAL_Zero,
            };

            this.mockInternalMessagingController.Setup(mc => mc.RecipientLimit(adminUser.PortalID)).Returns(10);
            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this.portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);
			Logger.LogDebug("SendNotification_Calls_DataService_On_Valid_Notification(): mockPortalInfo → {MockPortalInfo}", mockPortalInfo);

            var roles = new List<RoleInfo>();
            var users = new List<UserInfo>
							Logger.LogDebug("SendNotification_Calls_DataService_On_Valid_Notification(): roles → {Roles}", roles);
                            {
                                new UserInfo
                                    {
                                        UserID = Constants.UserID_User12,
                                        DisplayName = Constants.UserDisplayName_User12,
                                    },
                            };

            this.mockDataService
                .Setup(ds => ds.SendNotification(
                    It.IsAny<Notification>(),
                    Constants.PORTAL_Zero))
                .Verifiable();

            this.mockNotificationsController.Setup(nc => nc.GetExpirationDate(It.IsAny<int>())).Returns(DateTime.MinValue);

            var notification = CreateUnsavedNotification();
            notification.SenderUserID = adminUser.UserID;
			Logger.LogDebug("SendNotification_Calls_DataService_On_Valid_Notification(): notification → {Notification}", notification);
			Logger.LogDebug("SendNotification_Calls_DataService_On_Valid_Notification(): notification.SenderUserID → {Arg0}", notification.SenderUserID);
			Logger.LogDebug("SendNotification_Calls_DataService_On_Valid_Notification(): notification.SendToast → {Arg0}", notification.SendToast);
            notification.SendToast = false;

            this.mockNotificationsController.Object.SendNotification(
                notification,
                Constants.PORTAL_Zero,
                roles,
                users);

            this.mockDataService.Verify();
			Logger.LogDebug("<<< Exiting SendNotification_Calls_DataService_On_Valid_Notification()");
        }

        [Test]
        public void SendNotification_Calls_DataService_On_Valid_Notification_When_Portal_Is_In_Group()
        {
			Logger.LogDebug(">>> Entering SendNotification_Calls_DataService_On_Valid_Notification_When_Portal_Is_In_Group()");
            var adminUser = new UserInfo
            {
                UserID = Constants.UserID_Admin,
                DisplayName = Constants.UserDisplayName_Admin,
                PortalID = Constants.PORTAL_Zero,
            };

            this.mockInternalMessagingController.Setup(mc => mc.RecipientLimit(adminUser.PortalID)).Returns(10);

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Constants.PORTALGROUP_ValidPortalGroupId);
            this.portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);
			Logger.LogDebug("SendNotification_Calls_DataService_On_Valid_Notification_When_Portal_Is_In_Group(): mockPortalInfo → {MockPortalInfo}", mockPortalInfo);

            List<PortalGroupInfo> portalGroups = new List<PortalGroupInfo>() { CreatePortalGroupInfo(Constants.PORTALGROUP_ValidPortalGroupId, Constants.PORTAL_Zero) }; // CreatePortalGroupInfo(Constants.PORTALGROUP_ValidPortalGroupId, Constants.PORTAL_Zero);
            this.portalGroupController.Setup(pgc => pgc.GetPortalGroups()).Returns(portalGroups);
			Logger.LogDebug("SendNotification_Calls_DataService_On_Valid_Notification_When_Portal_Is_In_Group(): portalGroups → {PortalGroups}", portalGroups);

            var roles = new List<RoleInfo>();
            var users = new List<UserInfo>
							Logger.LogDebug("SendNotification_Calls_DataService_On_Valid_Notification_When_Portal_Is_In_Group(): roles → {Roles}", roles);
                            {
                                new UserInfo
                                    {
                                        UserID = Constants.UserID_User12,
                                        DisplayName = Constants.UserDisplayName_User12,
                                    },
                            };

            this.mockDataService
                .Setup(ds => ds.SendNotification(
                    It.IsAny<Notification>(),
                    Constants.PORTAL_Zero))
                .Verifiable();

            this.mockNotificationsController.Setup(nc => nc.GetExpirationDate(It.IsAny<int>())).Returns(DateTime.MinValue);

            var notification = CreateUnsavedNotification();
            notification.SenderUserID = adminUser.UserID;
			Logger.LogDebug("SendNotification_Calls_DataService_On_Valid_Notification_When_Portal_Is_In_Group(): notification → {Notification}", notification);
			Logger.LogDebug("SendNotification_Calls_DataService_On_Valid_Notification_When_Portal_Is_In_Group(): notification.SenderUserID → {Arg0}", notification.SenderUserID);
			Logger.LogDebug("SendNotification_Calls_DataService_On_Valid_Notification_When_Portal_Is_In_Group(): notification.SendToast → {Arg0}", notification.SendToast);
            notification.SendToast = false;

            this.mockNotificationsController.Object.SendNotification(
                notification,
                Constants.PORTAL_Zero,
                roles,
                users);

            this.mockDataService.Verify();
			Logger.LogDebug("<<< Exiting SendNotification_Calls_DataService_On_Valid_Notification_When_Portal_Is_In_Group()");
        }

        [Test]
        public void SendNotification_Calls_Messaging_DataService_CreateSocialMessageRecipientsForRole_When_Passing_Roles()
        {
			Logger.LogDebug(">>> Entering SendNotification_Calls_Messaging_DataService_CreateSocialMessageRecipientsForRole_When_Passing_Roles()");
            var adminUser = new UserInfo
            {
                UserID = Constants.UserID_Admin,
                DisplayName = Constants.UserDisplayName_Admin,
                PortalID = Constants.PORTAL_Zero,
            };

            this.mockInternalMessagingController.Setup(mc => mc.RecipientLimit(adminUser.PortalID)).Returns(10);
            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this.portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);
			Logger.LogDebug("SendNotification_Calls_Messaging_DataService_CreateSocialMessageRecipientsForRole_When_Passing_Roles(): mockPortalInfo → {MockPortalInfo}", mockPortalInfo);

            var roles = new List<RoleInfo>
                            {
                                new RoleInfo
                                    {
                                        RoleID = Constants.RoleID_RegisteredUsers,
                                        RoleName = Constants.RoleName_RegisteredUsers,
                                    },
                            };
            var users = new List<UserInfo>();

			Logger.LogDebug("SendNotification_Calls_Messaging_DataService_CreateSocialMessageRecipientsForRole_When_Passing_Roles(): users → {Users}", users);
            this.mockDataService
                .Setup(ds => ds.SendNotification(
                    It.IsAny<Notification>(),
                    Constants.PORTAL_Zero))
                .Returns(Constants.Messaging_MessageId_1);

            this.mockMessagingDataService
                .Setup(mds => mds.CreateMessageRecipientsForRole(
                    Constants.Messaging_MessageId_1,
                    Constants.RoleID_RegisteredUsers.ToString(CultureInfo.InvariantCulture),
                    It.IsAny<int>()))
                .Verifiable();

            this.mockNotificationsController.Setup(nc => nc.GetExpirationDate(It.IsAny<int>())).Returns(DateTime.MinValue);

            var notification = CreateUnsavedNotification();
            notification.SenderUserID = adminUser.UserID;
			Logger.LogDebug("SendNotification_Calls_Messaging_DataService_CreateSocialMessageRecipientsForRole_When_Passing_Roles(): notification → {Notification}", notification);
			Logger.LogDebug("SendNotification_Calls_Messaging_DataService_CreateSocialMessageRecipientsForRole_When_Passing_Roles(): notification.SenderUserID → {Arg0}", notification.SenderUserID);
			Logger.LogDebug("SendNotification_Calls_Messaging_DataService_CreateSocialMessageRecipientsForRole_When_Passing_Roles(): notification.SendToast → {Arg0}", notification.SendToast);
            notification.SendToast = false;

            this.mockNotificationsController.Object.SendNotification(
                notification,
                Constants.PORTAL_Zero,
                roles,
                users);

            this.mockMessagingDataService.Verify();
			Logger.LogDebug("<<< Exiting SendNotification_Calls_Messaging_DataService_CreateSocialMessageRecipientsForRole_When_Passing_Roles()");
        }

        [Test]
        public void SendNotification_Calls_Messaging_DataService_SaveSocialMessageRecipient_When_Passing_Users()
        {
			Logger.LogDebug(">>> Entering SendNotification_Calls_Messaging_DataService_SaveSocialMessageRecipient_When_Passing_Users()");
            var adminUser = new UserInfo
            {
                UserID = Constants.UserID_Admin,
                DisplayName = Constants.UserDisplayName_Admin,
                PortalID = Constants.PORTAL_Zero,
            };

            this.mockInternalMessagingController.Setup(mc => mc.RecipientLimit(adminUser.PortalID)).Returns(10);
            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this.portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);
			Logger.LogDebug("SendNotification_Calls_Messaging_DataService_SaveSocialMessageRecipient_When_Passing_Users(): mockPortalInfo → {MockPortalInfo}", mockPortalInfo);

            var roles = new List<RoleInfo>();
            var users = new List<UserInfo>
							Logger.LogDebug("SendNotification_Calls_Messaging_DataService_SaveSocialMessageRecipient_When_Passing_Users(): roles → {Roles}", roles);
                            {
                                new UserInfo
                                    {
                                        UserID = Constants.UserID_User12,
                                        DisplayName = Constants.UserDisplayName_User12,
                                    },
                            };

            this.mockDataService
                .Setup(ds => ds.SendNotification(
                    It.IsAny<Notification>(),
                    Constants.PORTAL_Zero))
                .Returns(Constants.Messaging_MessageId_1);

            this.mockInternalMessagingController
                .Setup(mc => mc.GetMessageRecipient(
                    Constants.Messaging_MessageId_1,
                    Constants.UserID_User12))
                .Returns((MessageRecipient)null);

            this.mockMessagingDataService
                .Setup(mds => mds.SaveMessageRecipient(
                    It.Is<MessageRecipient>(mr =>
                                            mr.MessageID == Constants.Messaging_MessageId_1 &&
                                            mr.UserID == Constants.UserID_User12 &&
                                            mr.Read == false &&
                                            mr.RecipientID == Null.NullInteger),
                    It.IsAny<int>()))
                .Verifiable();

            this.mockNotificationsController.Setup(nc => nc.GetExpirationDate(It.IsAny<int>())).Returns(DateTime.MinValue);

            var notification = CreateUnsavedNotification();
            notification.SenderUserID = adminUser.UserID;
			Logger.LogDebug("SendNotification_Calls_Messaging_DataService_SaveSocialMessageRecipient_When_Passing_Users(): notification → {Notification}", notification);
			Logger.LogDebug("SendNotification_Calls_Messaging_DataService_SaveSocialMessageRecipient_When_Passing_Users(): notification.SenderUserID → {Arg0}", notification.SenderUserID);
			Logger.LogDebug("SendNotification_Calls_Messaging_DataService_SaveSocialMessageRecipient_When_Passing_Users(): notification.SendToast → {Arg0}", notification.SendToast);
            notification.SendToast = false;

            this.mockNotificationsController.Object.SendNotification(
                notification,
                Constants.PORTAL_Zero,
                roles,
                users);

            this.mockMessagingDataService.Verify();
			Logger.LogDebug("<<< Exiting SendNotification_Calls_Messaging_DataService_SaveSocialMessageRecipient_When_Passing_Users()");
        }

        [Test]
        public void SendNotification_Returns_Valid_Object()
        {
			Logger.LogDebug(">>> Entering SendNotification_Returns_Valid_Object()");
            var expectedNotification = CreateValidNotification();

			Logger.LogDebug("SendNotification_Returns_Valid_Object(): expectedNotification → {ExpectedNotification}", expectedNotification);
            var adminUser = new UserInfo
            {
                UserID = Constants.UserID_Admin,
                DisplayName = Constants.UserDisplayName_Admin,
                PortalID = Constants.PORTAL_Zero,
            };

            this.mockInternalMessagingController.Setup(mc => mc.RecipientLimit(adminUser.PortalID)).Returns(10);

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this.portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);
			Logger.LogDebug("SendNotification_Returns_Valid_Object(): mockPortalInfo → {MockPortalInfo}", mockPortalInfo);

            var roles = new List<RoleInfo>();
            var users = new List<UserInfo>
							Logger.LogDebug("SendNotification_Returns_Valid_Object(): roles → {Roles}", roles);
                            {
                                new UserInfo
                                    {
                                        UserID = Constants.UserID_User12,
                                        DisplayName = Constants.UserDisplayName_User12,
                                    },
                            };

            this.mockDataService
                .Setup(ds => ds.SendNotification(
                    It.IsAny<Notification>(),
                    Constants.PORTAL_Zero))
                .Returns(Constants.Messaging_MessageId_1);

            this.mockMessagingDataService
                .Setup(mds => mds.CreateMessageRecipientsForRole(
                    Constants.Messaging_MessageId_1,
                    Constants.RoleID_RegisteredUsers.ToString(CultureInfo.InvariantCulture),
                    It.IsAny<int>()));

            this.mockInternalMessagingController
                .Setup(mc => mc.GetMessageRecipient(
                    Constants.Messaging_MessageId_1,
                    Constants.UserID_User12))
                .Returns((MessageRecipient)null);

            this.mockMessagingDataService
                .Setup(mds => mds.SaveMessageRecipient(
                    It.Is<MessageRecipient>(mr =>
                                            mr.MessageID == Constants.Messaging_MessageId_1 &&
                                            mr.UserID == Constants.UserID_User12 &&
                                            mr.Read == false &&
                                            mr.RecipientID == Null.NullInteger),
                    It.IsAny<int>()));

            this.mockNotificationsController.Setup(nc => nc.GetExpirationDate(It.IsAny<int>())).Returns(DateTime.MinValue);

            var notification = CreateUnsavedNotification();
            notification.SenderUserID = adminUser.UserID;
			Logger.LogDebug("SendNotification_Returns_Valid_Object(): notification → {Notification}", notification);
			Logger.LogDebug("SendNotification_Returns_Valid_Object(): notification.SenderUserID → {Arg0}", notification.SenderUserID);
			Logger.LogDebug("SendNotification_Returns_Valid_Object(): notification.SendToast → {Arg0}", notification.SendToast);
            notification.SendToast = false;

            this.mockNotificationsController.Object.SendNotification(
                notification,
                Constants.PORTAL_Zero,
                roles,
                users);

            Assert.That(notification, Is.EqualTo(expectedNotification).Using(new NotificationComparer()));
			Logger.LogDebug("<<< Exiting SendNotification_Returns_Valid_Object()");
        }

        [Test]
        public void DeleteNotification_Calls_DataService_DeleteNotification()
        {
			Logger.LogDebug(">>> Entering DeleteNotification_Calls_DataService_DeleteNotification()");
            var messageRecipients = new List<MessageRecipient>
                                        {
                                            new MessageRecipient(),
                                        };

            this.mockInternalMessagingController.Setup(mc => mc.GetMessageRecipients(Constants.Messaging_MessageId_1)).Returns(messageRecipients);

            this.mockDataService.Setup(ds => ds.DeleteNotification(Constants.Messaging_MessageId_1)).Verifiable();
            this.notificationsController.DeleteNotification(Constants.Messaging_MessageId_1);
            this.mockDataService.Verify();
			Logger.LogDebug("<<< Exiting DeleteNotification_Calls_DataService_DeleteNotification()");
        }

        [Test]
        public void GetNotifications_Calls_DataService_GetNotifications()
        {
			Logger.LogDebug(">>> Entering GetNotifications_Calls_DataService_GetNotifications()");
            this.mockDataService
                .Setup(ds => ds.GetNotifications(Constants.UserID_User12, Constants.PORTAL_Zero, It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new DataTable().CreateDataReader())
                .Verifiable();

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this.portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);
			Logger.LogDebug("GetNotifications_Calls_DataService_GetNotifications(): mockPortalInfo → {MockPortalInfo}", mockPortalInfo);

            this.notificationsController.GetNotifications(Constants.UserID_User12, Constants.PORTAL_Zero, 0, 10);
            this.mockDataService.Verify();
			Logger.LogDebug("<<< Exiting GetNotifications_Calls_DataService_GetNotifications()");
        }

        [Test]
        public void GetNotifications_Calls_DataService_GetNotifications_When_Portal_Is_In_Group()
        {
			Logger.LogDebug(">>> Entering GetNotifications_Calls_DataService_GetNotifications_When_Portal_Is_In_Group()");
            this.mockDataService
                .Setup(ds => ds.GetNotifications(Constants.UserID_User12, Constants.PORTAL_Zero, It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new DataTable().CreateDataReader())
                .Verifiable();

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Constants.PORTALGROUP_ValidPortalGroupId);
            this.portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);
			Logger.LogDebug("GetNotifications_Calls_DataService_GetNotifications_When_Portal_Is_In_Group(): mockPortalInfo → {MockPortalInfo}", mockPortalInfo);

            List<PortalGroupInfo> portalGroups = new List<PortalGroupInfo>() { CreatePortalGroupInfo(Constants.PORTALGROUP_ValidPortalGroupId, Constants.PORTAL_Zero) }; // CreatePortalGroupInfo(Constants.PORTALGROUP_ValidPortalGroupId, Constants.PORTAL_Zero);
            this.portalGroupController.Setup(pgc => pgc.GetPortalGroups()).Returns(portalGroups);
			Logger.LogDebug("GetNotifications_Calls_DataService_GetNotifications_When_Portal_Is_In_Group(): portalGroups → {PortalGroups}", portalGroups);

            this.notificationsController.GetNotifications(Constants.UserID_User12, Constants.PORTAL_Zero, 0, 10);
            this.mockDataService.Verify();
			Logger.LogDebug("<<< Exiting GetNotifications_Calls_DataService_GetNotifications_When_Portal_Is_In_Group()");
        }

        [Test]
        public void GetNotifications_Calls_DataService_GetNotificationByContext()
        {
			Logger.LogDebug(">>> Entering GetNotifications_Calls_DataService_GetNotificationByContext()");
            this.mockDataService
                .Setup(ds => ds.GetNotificationByContext(Constants.Messaging_NotificationTypeId, Constants.Messaging_NotificationContext))
                .Returns(new DataTable().CreateDataReader())
                .Verifiable();

            this.notificationsController.GetNotificationByContext(Constants.Messaging_NotificationTypeId, Constants.Messaging_NotificationContext);
            this.mockDataService.Verify();
			Logger.LogDebug("<<< Exiting GetNotifications_Calls_DataService_GetNotificationByContext()");
        }

        [Test]
        public void CountNotifications_Calls_DataService_CountNotifications()
        {
			Logger.LogDebug(">>> Entering CountNotifications_Calls_DataService_CountNotifications()");
            this.mockDataService.Setup(ds => ds.CountNotifications(Constants.UserID_User12, Constants.PORTAL_Zero)).Verifiable();
            this.notificationsController.CountNotifications(Constants.UserID_User12, Constants.PORTAL_Zero);
            this.mockDataService.Verify();
			Logger.LogDebug("<<< Exiting CountNotifications_Calls_DataService_CountNotifications()");
        }

        [Test]
        public void DeleteNotificationRecipient_Calls_MessagingController_DeleteMessageRecipient()
        {
			Logger.LogDebug(">>> Entering DeleteNotificationRecipient_Calls_MessagingController_DeleteMessageRecipient()");
            this.mockInternalMessagingController.Setup(mc => mc.DeleteMessageRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12)).Verifiable();
            this.mockInternalMessagingController.Setup(mc => mc.GetMessageRecipients(Constants.Messaging_MessageId_1)).Returns(new List<MessageRecipient>());
            this.notificationsController.DeleteNotificationRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12);
            this.mockMessagingController.Verify();
			Logger.LogDebug("<<< Exiting DeleteNotificationRecipient_Calls_MessagingController_DeleteMessageRecipient()");
        }

        [Test]
        public void DeleteNotificationRecipientByContext_Calls_DeleteMessageRecipient()
        {
			Logger.LogDebug(">>> Entering DeleteNotificationRecipientByContext_Calls_DeleteMessageRecipient()");
            this.mockNotificationsController.Setup(mc => mc.DeleteNotificationRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12)).Verifiable();
            this.mockNotificationsController.Setup(mc => mc.GetNotificationByContext(Constants.Messaging_NotificationTypeId, Constants.Messaging_NotificationContext))
                .Returns(new List<Notification> { new Notification { NotificationID = Constants.Messaging_MessageId_1 } });
            this.mockNotificationsController.Object.DeleteNotificationRecipient(Constants.Messaging_NotificationTypeId, Constants.Messaging_NotificationContext, Constants.UserID_User12);
            this.mockMessagingController.Verify();
			Logger.LogDebug("<<< Exiting DeleteNotificationRecipientByContext_Calls_DeleteMessageRecipient()");
        }

        [Test]
        public void DeleteNotificationRecipient_Does_Not_Delete_Notification_When_There_Are_More_Recipients()
        {
			Logger.LogDebug(">>> Entering DeleteNotificationRecipient_Does_Not_Delete_Notification_When_There_Are_More_Recipients()");
            var messageRecipients = new List<MessageRecipient>
                                        {
                                            new MessageRecipient(),
                                        };

            this.mockInternalMessagingController.Setup(mc => mc.DeleteMessageRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12));
            this.mockInternalMessagingController.Setup(mc => mc.GetMessageRecipients(Constants.Messaging_MessageId_1)).Returns(messageRecipients);
            this.mockNotificationsController.Object.DeleteNotificationRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12);

            this.mockNotificationsController.Verify(nc => nc.DeleteNotification(Constants.Messaging_MessageId_1), Times.Never());
			Logger.LogDebug("<<< Exiting DeleteNotificationRecipient_Does_Not_Delete_Notification_When_There_Are_More_Recipients()");
        }

        [Test]
        public void DeleteNotificationRecipient_Deletes_Notification_When_There_Are_No_More_Recipients()
        {
			Logger.LogDebug(">>> Entering DeleteNotificationRecipient_Deletes_Notification_When_There_Are_No_More_Recipients()");
            this.mockInternalMessagingController.Setup(mc => mc.DeleteMessageRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12));
            this.mockInternalMessagingController.Setup(mc => mc.GetMessageRecipients(Constants.Messaging_MessageId_1)).Returns(new List<MessageRecipient>());
            this.mockNotificationsController.Object.DeleteNotificationRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12);
            this.mockNotificationsController.Verify(nc => nc.DeleteNotification(Constants.Messaging_MessageId_1));
			Logger.LogDebug("<<< Exiting DeleteNotificationRecipient_Deletes_Notification_When_There_Are_No_More_Recipients()");
        }

        [Test]
        public void DeleteAllNotificationRecipients_Calls_DeleteNotificationRecipient_For_Each_Recipient()
        {
			Logger.LogDebug(">>> Entering DeleteAllNotificationRecipients_Calls_DeleteNotificationRecipient_For_Each_Recipient()");
            var recipients = new List<MessageRecipient>
                                 {
                                     new MessageRecipient { RecipientID = Constants.Messaging_RecipientId_1 },
                                     new MessageRecipient { RecipientID = Constants.Messaging_RecipientId_2 },
                                 };

            this.mockInternalMessagingController.Setup(imc => imc.GetMessageRecipients(Constants.Messaging_MessageId_1)).Returns(recipients);

            this.mockNotificationsController.Object.DeleteAllNotificationRecipients(Constants.Messaging_MessageId_1);

            this.mockNotificationsController.Verify(nc => nc.DeleteNotificationRecipient(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));
			Logger.LogDebug("<<< Exiting DeleteAllNotificationRecipients_Calls_DeleteNotificationRecipient_For_Each_Recipient()");
        }

        [Test]
        public void DeleteAllNotificationRecipients_Does_Not_Call_DeleteNotificationRecipient_When_Notification_Has_No_Recipients()
        {
			Logger.LogDebug(">>> Entering DeleteAllNotificationRecipients_Does_Not_Call_DeleteNotificationRecipient_When_Notification_Has_No_Recipients()");
            this.mockInternalMessagingController.Setup(imc => imc.GetMessageRecipients(Constants.Messaging_MessageId_1)).Returns(new List<MessageRecipient>());
            this.mockNotificationsController.Object.DeleteAllNotificationRecipients(Constants.Messaging_MessageId_1);
            this.mockNotificationsController.Verify(nc => nc.DeleteNotificationRecipient(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
			Logger.LogDebug("<<< Exiting DeleteAllNotificationRecipients_Does_Not_Call_DeleteNotificationRecipient_When_Notification_Has_No_Recipients()");
        }

        private static Notification CreateUnsavedNotification()
        {
			Logger.LogDebug(">>> Entering CreateUnsavedNotification()");
			Logger.LogDebug("<<< Exiting CreateUnsavedNotification()");
            return new Notification
            {
                NotificationTypeID = Constants.Messaging_NotificationTypeId,
                Subject = Constants.Messaging_NotificationSubject,
                Body = Constants.Messaging_NotificationBody,
                To = Constants.UserDisplayName_User12,
                From = Constants.UserDisplayName_Admin,
                SenderUserID = Constants.UserID_Admin,
                Context = Constants.Messaging_NotificationContext,
                SendToast = false,
            };
        }

        private static Notification CreateValidNotification()
        {
			Logger.LogDebug(">>> Entering CreateValidNotification()");
            var notification = CreateUnsavedNotification();
            notification.NotificationID = Constants.Messaging_MessageId_1;
			Logger.LogDebug("CreateValidNotification(): notification → {Notification}", notification);
			Logger.LogDebug("CreateValidNotification(): notification.NotificationID → {Arg0}", notification.NotificationID);

			Logger.LogDebug("<<< Exiting CreateValidNotification()");
            return notification;
        }

        private static NotificationType CreateValidNotificationType()
        {
			Logger.LogDebug(">>> Entering CreateValidNotificationType()");
            var nt = CreateNewNotificationType();
            nt.NotificationTypeId = Constants.Messaging_NotificationTypeId;
			Logger.LogDebug("CreateValidNotificationType(): nt → {Nt}", nt);
			Logger.LogDebug("CreateValidNotificationType(): nt.NotificationTypeId → {Arg0}", nt.NotificationTypeId);
			Logger.LogDebug("<<< Exiting CreateValidNotificationType()");
            return nt;
        }

        private static NotificationType CreateNewNotificationType()
        {
			Logger.LogDebug(">>> Entering CreateNewNotificationType()");
			Logger.LogDebug("<<< Exiting CreateNewNotificationType()");
            return new NotificationType
            {
                Name = Constants.Messaging_NotificationTypeName,
                Description = Constants.Messaging_NotificationTypeDescription,
                TimeToLive = new TimeSpan(0, Constants.Messaging_NotificationTypeTTL, 0),
                DesktopModuleId = Constants.Messaging_NotificationTypeDesktopModuleId,
                IsTask = false,
            };
        }

        private static NotificationTypeAction CreateValidNotificationTypeAction()
        {
			Logger.LogDebug(">>> Entering CreateValidNotificationTypeAction()");
            var action = CreateNewNotificationTypeAction();

			Logger.LogDebug("CreateValidNotificationTypeAction(): action → {Action}", action);
			Logger.LogDebug("CreateValidNotificationTypeAction(): action.NotificationTypeActionId → {Arg0}", action.NotificationTypeActionId);
			Logger.LogDebug("CreateValidNotificationTypeAction(): action.NotificationTypeId → {Arg0}", action.NotificationTypeId);
            action.NotificationTypeActionId = Constants.Messaging_NotificationTypeActionId;
            action.NotificationTypeId = Constants.Messaging_NotificationTypeId;

			Logger.LogDebug("<<< Exiting CreateValidNotificationTypeAction()");
            return action;
        }

        private static NotificationTypeAction CreateNewNotificationTypeAction()
        {
			Logger.LogDebug(">>> Entering CreateNewNotificationTypeAction()");
			Logger.LogDebug("<<< Exiting CreateNewNotificationTypeAction()");
            return new NotificationTypeAction
            {
                APICall = Constants.Messaging_NotificationTypeActionAPICall,
                ConfirmResourceKey = Constants.Messaging_NotificationTypeActionConfirmResourceKey,
                DescriptionResourceKey = Constants.Messaging_NotificationTypeActionDescriptionResourceKey,
                NameResourceKey = Constants.Messaging_NotificationTypeActionNameResourceKey,
            };
        }

        private static PortalInfo CreatePortalInfo(int portalId, int portalGroupId)
        {
			Logger.LogDebug(">>> Entering CreatePortalInfo(portalId={PortalId},portalGroupId={PortalGroupId})", portalId, portalGroupId);
            var mockPortalInfo = new PortalInfo { PortalID = portalId, PortalGroupID = portalGroupId };
			Logger.LogDebug("<<< Exiting CreatePortalInfo(portalId={PortalId},portalGroupId={PortalGroupId})", portalId, portalGroupId);
            return mockPortalInfo;
        }

        private static PortalGroupInfo CreatePortalGroupInfo(int portalGroupId, int masterPortalId)
        {
			Logger.LogDebug(">>> Entering CreatePortalGroupInfo(portalGroupId={PortalGroupId},masterPortalId={MasterPortalId})", portalGroupId, masterPortalId);
            var mockPortalGroupInfo = new PortalGroupInfo
            {
                PortalGroupId = portalGroupId,
                MasterPortalId = masterPortalId,
                PortalGroupName = Constants.PORTALGROUP_ValidName,
                PortalGroupDescription = Constants.PORTALGROUP_ValidDescription,
            };

			Logger.LogDebug("<<< Exiting CreatePortalGroupInfo(portalGroupId={PortalGroupId},masterPortalId={MasterPortalId})", portalGroupId, masterPortalId);
            return mockPortalGroupInfo;
        }

        private void SetupDataProvider()
        {
			Logger.LogDebug(">>> Entering SetupDataProvider()");
            // Standard DataProvider Path for Logging
            this.dataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);
			Logger.LogDebug("<<< Exiting SetupDataProvider()");
        }

        private void SetupDataTables()
        {
			Logger.LogDebug(">>> Entering SetupDataTables()");
            this.dtNotificationTypes = new DataTable();
            this.dtNotificationTypes.Columns.Add("NotificationTypeID", typeof(int));
			Logger.LogDebug("SetupDataTables(): dtNotificationTypes → {DtNotificationTypes}", dtNotificationTypes);
            this.dtNotificationTypes.Columns.Add("Name", typeof(string));
            this.dtNotificationTypes.Columns.Add("Description", typeof(string));
            this.dtNotificationTypes.Columns.Add("TTL", typeof(int));
            this.dtNotificationTypes.Columns.Add("DesktopModuleID", typeof(int));
            this.dtNotificationTypes.Columns.Add("CreatedByUserID", typeof(int));
            this.dtNotificationTypes.Columns.Add("CreatedOnDate", typeof(DateTime));
            this.dtNotificationTypes.Columns.Add("LastModifiedByUserID", typeof(int));
            this.dtNotificationTypes.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            this.dtNotificationTypes.Columns.Add("IsTask", typeof(bool));

            this.dtNotificationTypeActions = new DataTable();
			Logger.LogDebug("SetupDataTables(): dtNotificationTypeActions → {DtNotificationTypeActions}", dtNotificationTypeActions);
            this.dtNotificationTypeActions.Columns.Add("NotificationTypeActionID", typeof(int));
            this.dtNotificationTypeActions.Columns.Add("NotificationTypeID", typeof(int));
            this.dtNotificationTypeActions.Columns.Add("NameResourceKey", typeof(string));
            this.dtNotificationTypeActions.Columns.Add("DescriptionResourceKey", typeof(string));
            this.dtNotificationTypeActions.Columns.Add("ConfirmResourceKey", typeof(string));
            this.dtNotificationTypeActions.Columns.Add("Order", typeof(int));
            this.dtNotificationTypeActions.Columns.Add("APICall", typeof(string));
            this.dtNotificationTypeActions.Columns.Add("CreatedByUserID", typeof(int));
            this.dtNotificationTypeActions.Columns.Add("CreatedOnDate", typeof(DateTime));
            this.dtNotificationTypeActions.Columns.Add("LastModifiedByUserID", typeof(int));
            this.dtNotificationTypeActions.Columns.Add("LastModifiedOnDate", typeof(DateTime));

            this.dtNotificationActions = new DataTable();
			Logger.LogDebug("SetupDataTables(): dtNotificationActions → {DtNotificationActions}", dtNotificationActions);
            this.dtNotificationActions.Columns.Add("NotificationActionID");
            this.dtNotificationActions.Columns.Add("MessageID");
            this.dtNotificationActions.Columns.Add("NotificationTypeActionID");
            this.dtNotificationActions.Columns.Add("Key");
            this.dtNotificationActions.Columns.Add("CreatedByUserID", typeof(int));
            this.dtNotificationActions.Columns.Add("CreatedOnDate", typeof(DateTime));
            this.dtNotificationActions.Columns.Add("LastModifiedByUserID", typeof(int));
            this.dtNotificationActions.Columns.Add("LastModifiedOnDate", typeof(DateTime));
			Logger.LogDebug("<<< Exiting SetupDataTables()");
        }

        private class NotificationTypeComparer : IEqualityComparer<NotificationType>
        {
            public bool Equals(NotificationType x, NotificationType y)
            {
                if (x == null)
                {
                    return y == null;
                }

                if (y == null)
                {
                    return false;
                }

					Logger.LogDebug("<<< Exiting Equals(x={X},y={Y})", x, y);
                return
                    x.NotificationTypeId == y.NotificationTypeId &&
                    x.Name == y.Name &&
                    x.Description == y.Description &&
                    x.TimeToLive == y.TimeToLive &&
                    x.IsTask == y.IsTask &&
                    x.DesktopModuleId == y.DesktopModuleId;
            }

            public int GetHashCode(NotificationType obj)
            {
                throw new NotImplementedException();
            }
        }

        private class NotificationTypeActionComparer : IEqualityComparer<NotificationTypeAction>
        {
            public bool Equals(NotificationTypeAction x, NotificationTypeAction y)
            {
                if (x == null)
                {
                    return y == null;
                }

                if (y == null)
                {
                    return false;
                }

                return
                    x.NotificationTypeActionId == y.NotificationTypeActionId &&
                    x.NotificationTypeId == y.NotificationTypeId &&
                    x.NameResourceKey == y.NameResourceKey &&
                    x.DescriptionResourceKey == y.DescriptionResourceKey &&
                    x.ConfirmResourceKey == y.ConfirmResourceKey &&
                    x.APICall == y.APICall;
            }

            public int GetHashCode(NotificationTypeAction obj)
            {
				Logger.LogDebug(">>> Entering GetHashCode(obj={Obj})", obj);
                throw new NotImplementedException();
            }
        }

        private class NotificationComparer : IEqualityComparer<Notification>
        {
            public bool Equals(Notification x, Notification y)
            {
                if (x == null)
                {
                    return y == null;
                }

                if (y == null)
                {
                    return false;
                }

                return
                    x.NotificationID == y.NotificationID &&
                    x.NotificationTypeID == y.NotificationTypeID &&
                    x.Subject == y.Subject &&
                    x.Body == y.Body &&
                    x.To == y.To &&
                    x.From == y.From &&
                    x.SenderUserID == y.SenderUserID &&
                    x.Context == y.Context &&
                    x.IncludeDismissAction == y.IncludeDismissAction;
            }

            public int GetHashCode(Notification obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}

// ReSharper restore InconsistentNaming
