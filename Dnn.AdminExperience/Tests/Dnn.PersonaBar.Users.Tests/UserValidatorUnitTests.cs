// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;

    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Users.Components;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class UserValidatorUnitTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private Mock<IPortalController> _portalControllerMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;
        private Mock<IContentVerifier> _contentVerifierMock;

        private UserValidator _userValidator;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void RunBeforeEachTest()
        {
			Logger.LogDebug(">>> Entering RunBeforeEachTest()");
            this._portalControllerMock = new Mock<IPortalController>();
            this._userControllerWrapperMock = new Mock<IUserControllerWrapper>();
			Logger.LogDebug("RunBeforeEachTest(): _userControllerWrapperMock → {_userControllerWrapperMock}", _userControllerWrapperMock);
			Logger.LogDebug("RunBeforeEachTest(): _portalControllerMock → {_portalControllerMock}", _portalControllerMock);
            this._contentVerifierMock = new Mock<IContentVerifier>();
			Logger.LogDebug("RunBeforeEachTest(): _contentVerifierMock → {_contentVerifierMock}", _contentVerifierMock);

            this._userValidator = new UserValidator(
                        this._portalControllerMock.Object,
                        this._userControllerWrapperMock.Object,
                        this._contentVerifierMock.Object);
						Logger.LogDebug("RunBeforeEachTest(): _userValidator → {_userValidator}", _userValidator);

            this.serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IHostSettings>()));
			Logger.LogDebug("<<< Exiting RunBeforeEachTest()");
			Logger.LogDebug("RunBeforeEachTest(): serviceProvider → {ServiceProvider}", serviceProvider);
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void ValidateUser_IfUserIdWithValidValue_ThenSuccessResponse()
        {
			Logger.LogDebug(">>> Entering ValidateUser_IfUserIdWithValidValue_ThenSuccessResponse()");
            // Arrange
            int? userId = 1;
            var userInfo = this.GetUserInfoWithProfile(userId.Value);
            this.SetupUserControllerWrapperMock(userInfo);
			Logger.LogDebug("ValidateUser_IfUserIdWithValidValue_ThenSuccessResponse(): userInfo → {UserInfo}", userInfo);

            // Act
            var result = this._userValidator.ValidateUser(userId, null, null, out userInfo);

			Logger.LogDebug("ValidateUser_IfUserIdWithValidValue_ThenSuccessResponse(): result → {Result}", result);
            // Assert
            Assert.That(result, Is.Null);
			Logger.LogDebug("<<< Exiting ValidateUser_IfUserIdWithValidValue_ThenSuccessResponse()");
        }

        [Test]
        public void ValidateUser_IfUserAllowedInSiteGroup_ThenSuccessResponse()
        {
			Logger.LogDebug(">>> Entering ValidateUser_IfUserAllowedInSiteGroup_ThenSuccessResponse()");
            // Arrange
            int? userId = 1;
            this.SetupForSiteGroup(true, userId.Value);
            UserInfo userInfo;

            // Act
            var result = this._userValidator.ValidateUser(userId, null, null, out userInfo);

			Logger.LogDebug("ValidateUser_IfUserAllowedInSiteGroup_ThenSuccessResponse(): result → {Result}", result);
            // Assert
            Assert.That(result, Is.Null);
			Logger.LogDebug("<<< Exiting ValidateUser_IfUserAllowedInSiteGroup_ThenSuccessResponse()");
        }

        [Test]
        public void ValidateUser_IfUserNotAllowedInSiteGroup_ThenErrorResponse()
        {
			Logger.LogDebug(">>> Entering ValidateUser_IfUserNotAllowedInSiteGroup_ThenErrorResponse()");
            // Arrange
            int? userId = 1;
            this.SetupForSiteGroup(false, userId.Value);
            UserInfo userInfo;

            // Act
            var result = this._userValidator.ValidateUser(userId, null, null, out userInfo);

			Logger.LogDebug("ValidateUser_IfUserNotAllowedInSiteGroup_ThenErrorResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.True);
			Logger.LogDebug("<<< Exiting ValidateUser_IfUserNotAllowedInSiteGroup_ThenErrorResponse()");
        }

        [Test]
        public void ValidateUser_IfUserIdNotFound_ThenErrorResponse()
        {
			Logger.LogDebug(">>> Entering ValidateUser_IfUserIdNotFound_ThenErrorResponse()");
            // Arrange
            int? userId = 1;
            UserInfo userInfo = null;
            this.SetupUserControllerWrapperMock(userInfo);

            ArrayList portals = new ArrayList();
            this._portalControllerMock.Setup(p => p.GetPortals()).Returns(portals);
			Logger.LogDebug("ValidateUser_IfUserIdNotFound_ThenErrorResponse(): portals → {Portals}", portals);

            // Act
            var result = this._userValidator.ValidateUser(userId, null, null, out userInfo);

			Logger.LogDebug("ValidateUser_IfUserIdNotFound_ThenErrorResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.True);
			Logger.LogDebug("<<< Exiting ValidateUser_IfUserIdNotFound_ThenErrorResponse()");
        }

        [Test]
        public void ValidateUser_IfUserIdWithoutValue_ThenErrorResponse()
        {
			Logger.LogDebug(">>> Entering ValidateUser_IfUserIdWithoutValue_ThenErrorResponse()");
            // Arrange
            int? userId = null;
            UserInfo userInfo;

            // Act
            var result = this._userValidator.ValidateUser(userId, null, null, out userInfo);

			Logger.LogDebug("ValidateUser_IfUserIdWithoutValue_ThenErrorResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.True);
			Logger.LogDebug("<<< Exiting ValidateUser_IfUserIdWithoutValue_ThenErrorResponse()");
        }

        private void SetupUserControllerWrapperMock(UserInfo userInfo)
        {
            KeyValuePair<HttpStatusCode, string> response;

            this._userControllerWrapperMock
                .Setup(
                    u => u.GetUser(
                        It.IsAny<int>(),
                        It.IsAny<PortalSettings>(),
                        It.IsAny<UserInfo>(),
                        out response))
                .Returns(userInfo);
        }

        private void SetupForSiteGroup(bool isAllowed, int userId)
        {
			Logger.LogDebug(">>> Entering SetupForSiteGroup(isAllowed={IsAllowed},userId={UserId})", isAllowed, userId);
            var userInfo = this.GetUserInfoWithProfile(userId);

			Logger.LogDebug("SetupForSiteGroup(isAllowed={IsAllowed},userId={UserId}): userInfo → {UserInfo}", isAllowed, userId, userInfo);
            var otherPortalId = 2;
            var portals = new ArrayList();
            portals.Add(new PortalInfo() { PortalID = otherPortalId });
			Logger.LogDebug("SetupForSiteGroup(isAllowed={IsAllowed},userId={UserId}): portals → {Portals}", isAllowed, userId, portals);
            this._portalControllerMock.Setup(p => p.GetPortals()).Returns(portals);

            this._userControllerWrapperMock.Setup(u => u.GetUserById(It.IsAny<int>(), userId)).Returns(userInfo);
            this._contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(It.IsAny<int>(), It.IsAny<PortalSettings>(), true)).Returns(isAllowed);
			Logger.LogDebug("<<< Exiting SetupForSiteGroup(isAllowed={IsAllowed},userId={UserId})", isAllowed, userId);
        }

        private UserInfo GetUserInfoWithProfile(int userId)
        {
			Logger.LogDebug(">>> Entering GetUserInfoWithProfile(userId={UserId})", userId);
            var userInfo = new UserInfo();
            var profile = new UserProfile();
			Logger.LogDebug("GetUserInfoWithProfile(userId={UserId}): userInfo → {UserInfo}", userId, userInfo);
			Logger.LogDebug("GetUserInfoWithProfile(userId={UserId}): profile → {Profile}", userId, profile);
			Logger.LogDebug("GetUserInfoWithProfile(userId={UserId}): profile.FirstName → {Arg1}", userId, profile.FirstName);
			Logger.LogDebug("GetUserInfoWithProfile(userId={UserId}): userInfo.UserID → {Arg1}", userId, userInfo.UserID);
            profile.FirstName = "testUser";
            userInfo.UserID = userId;
            userInfo.Profile = profile;
			Logger.LogDebug("GetUserInfoWithProfile(userId={UserId}): userInfo.Profile → {Arg1}", userId, userInfo.Profile);
			Logger.LogDebug("<<< Exiting GetUserInfoWithProfile(userId={UserId})", userId);
            return userInfo;
        }
    }
}
