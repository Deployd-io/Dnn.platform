// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.Services.Social.Messaging.Data;
    using DotNetNuke.Tests.Core.Controllers.Messaging.Builders;
    using DotNetNuke.Tests.Core.Controllers.Messaging.Helpers;
    using DotNetNuke.Tests.Core.Controllers.Messaging.Mocks;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class UserPreferencesControllerTests
    {
        private UserPreferencesController userPrefencesController;
        private Mock<IDataService> mockDataService;
        private Mock<CachingProvider> mockCacheProvider;

        [SetUp]
        public void SetUp()
        {
			mockEventLogger.LogDebug(">>> Entering SetUp()");
            // Setup Mocks and Stub
            this.mockDataService = new Mock<IDataService>();
			mockEventLogger.LogDebug("SetUp(): mockDataService → {MockDataService}", mockDataService);
            this.mockCacheProvider = MockComponentProvider.CreateDataCacheProvider();
			mockEventLogger.LogDebug("SetUp(): mockCacheProvider → {MockCacheProvider}", mockCacheProvider);

            DataService.RegisterInstance(this.mockDataService.Object);
            SetupCachingProviderHelper.SetupCachingProvider(this.mockCacheProvider);

            // Setup SUT
            this.userPrefencesController = new UserPreferencesController();
			mockEventLogger.LogDebug("SetUp(): userPrefencesController → {UserPrefencesController}", userPrefencesController);
			mockEventLogger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
			mockEventLogger.LogDebug("TearDown(): ComponentFactory.Container → {Arg0}", ComponentFactory.Container);
            ComponentFactory.Container = null;
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void UserPreferencesController_ShouldThrowArgumentNullException_WhenNullDataServiceIsPassedInTheConstructor()
        {
			mockEventLogger.LogDebug(">>> Entering UserPreferencesController_ShouldThrowArgumentNullException_WhenNullDataServiceIsPassedInTheConstructor()");
            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => new UserPreferencesController(null));
			mockEventLogger.LogDebug("<<< Exiting UserPreferencesController_ShouldThrowArgumentNullException_WhenNullDataServiceIsPassedInTheConstructor()");
        }

        [Test]
        public void SetUserPreference_ShouldCallDataService_WhenNoError()
        {
			mockEventLogger.LogDebug(">>> Entering SetUserPreference_ShouldCallDataService_WhenNoError()");
            // Arrange
            var userPreference = new UserPreferenceBuilder().Build();
			mockEventLogger.LogDebug("SetUserPreference_ShouldCallDataService_WhenNoError(): userPreference → {UserPreference}", userPreference);

            this.mockDataService.Setup(ds => ds.SetUserPreference(
                userPreference.PortalId,
                userPreference.UserId,
                (int)userPreference.MessagesEmailFrequency,
                (int)userPreference.NotificationsEmailFrequency)).Verifiable();

            // Act
            this.userPrefencesController.SetUserPreference(userPreference);

            // Assert
            this.mockDataService.Verify(
                ds => ds.SetUserPreference(
                userPreference.PortalId,
                userPreference.UserId,
                (int)userPreference.MessagesEmailFrequency,
                (int)userPreference.NotificationsEmailFrequency), Times.Once);
				mockEventLogger.LogDebug("<<< Exiting SetUserPreference_ShouldCallDataService_WhenNoError()");
        }

        [Test]
        public void GetUserPreference_ShouldReturnNullObject_WhenUserDoesNotHavePreference()
        {
			mockEventLogger.LogDebug(">>> Entering GetUserPreference_ShouldReturnNullObject_WhenUserDoesNotHavePreference()");
            // Arrange
            var user = GetValidUser();
			mockEventLogger.LogDebug("GetUserPreference_ShouldReturnNullObject_WhenUserDoesNotHavePreference(): user → {User}", user);
            this.mockDataService.Setup(ds => ds.GetUserPreference(
                Constants.PORTAL_ValidPortalId,
                Constants.UserID_User12)).Returns(UserPreferenceDataReaderMockHelper.CreateEmptyUserPreferenceReader);

            // Act
            var userPreference = this.userPrefencesController.GetUserPreference(user);
			mockEventLogger.LogDebug("GetUserPreference_ShouldReturnNullObject_WhenUserDoesNotHavePreference(): userPreference → {UserPreference}", userPreference);

            // Assert
            Assert.That(userPreference, Is.Null);
			mockEventLogger.LogDebug("<<< Exiting GetUserPreference_ShouldReturnNullObject_WhenUserDoesNotHavePreference()");
        }

        [Test]
        public void GetUserPreference_ShouldReturnUserPreference_WhenUserHasPreference()
        {
			mockEventLogger.LogDebug(">>> Entering GetUserPreference_ShouldReturnUserPreference_WhenUserHasPreference()");
            // Arrange
            var expectedUserPreference = new UserPreferenceBuilder()
                .WithUserId(Constants.UserID_User12)
                .WithPortalId(Constants.PORTAL_ValidPortalId)
                .Build();

            var user = GetValidUser();
			mockEventLogger.LogDebug("GetUserPreference_ShouldReturnUserPreference_WhenUserHasPreference(): user → {User}", user);
            this.mockDataService.Setup(ds => ds.GetUserPreference(Constants.PORTAL_ValidPortalId, Constants.UserID_User12))
                .Returns(UserPreferenceDataReaderMockHelper.CreateUserPreferenceReader(expectedUserPreference));

            // Act
            var userPreference = this.userPrefencesController.GetUserPreference(user);
			mockEventLogger.LogDebug("GetUserPreference_ShouldReturnUserPreference_WhenUserHasPreference(): userPreference → {UserPreference}", userPreference);

            // Assert
            Assert.That(userPreference, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(userPreference.MessagesEmailFrequency, Is.EqualTo(expectedUserPreference.MessagesEmailFrequency));
                Assert.That(userPreference.NotificationsEmailFrequency, Is.EqualTo(expectedUserPreference.NotificationsEmailFrequency));
                Assert.That(userPreference.PortalId, Is.EqualTo(user.PortalID));
                Assert.That(userPreference.UserId, Is.EqualTo(user.UserID));
            }
			mockEventLogger.LogDebug("<<< Exiting GetUserPreference_ShouldReturnUserPreference_WhenUserHasPreference()");
        }

        private static UserInfo GetValidUser()
        {
			mockEventLogger.LogDebug(">>> Entering GetValidUser()");
			mockEventLogger.LogDebug("<<< Exiting GetValidUser()");
            return new UserInfo
            {
                DisplayName = Constants.UserDisplayName_User12,
                UserID = Constants.UserID_User12,
                PortalID = Constants.PORTAL_ValidPortalId,
            };
        }
    }
}
