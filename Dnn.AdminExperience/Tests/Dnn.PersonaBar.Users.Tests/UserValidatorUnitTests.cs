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
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class UserValidatorUnitTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private Mock<IPortalController> _portalControllerMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;
        private Mock<IContentVerifier> _contentVerifierMock;

        private UserValidator _userValidator;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void RunBeforeEachTest()
        {
			_logger.LogDebug(">>> Entering RunBeforeEachTest()");
            this._portalControllerMock = new Mock<IPortalController>();
            this._userControllerWrapperMock = new Mock<IUserControllerWrapper>();
			_logger.LogDebug("RunBeforeEachTest(): _userControllerWrapperMock → {_userControllerWrapperMock}", _userControllerWrapperMock);
			_logger.LogDebug("RunBeforeEachTest(): _portalControllerMock → {_portalControllerMock}", _portalControllerMock);
            this._contentVerifierMock = new Mock<IContentVerifier>();
			_logger.LogDebug("RunBeforeEachTest(): _contentVerifierMock → {_contentVerifierMock}", _contentVerifierMock);

            this._userValidator = new UserValidator(
                        this._portalControllerMock.Object,
                        this._userControllerWrapperMock.Object,
                        this._contentVerifierMock.Object);
						_logger.LogDebug("RunBeforeEachTest(): _userValidator → {_userValidator}", _userValidator);

            this.serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IHostSettings>()));
			_logger.LogDebug("RunBeforeEachTest(): serviceProvider → {ServiceProvider}", serviceProvider);
			_logger.LogDebug("<<< Exiting RunBeforeEachTest()");
        }

        [TearDown]
        public void TearDown()
        {
			_logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			_logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void ValidateUser_IfUserIdWithValidValue_ThenSuccessResponse()
        {
			_logger.LogDebug(">>> Entering ValidateUser_IfUserIdWithValidValue_ThenSuccessResponse()");
            // Arrange
            int? userId = 1;
            var userInfo = this.GetUserInfoWithProfile(userId.Value);
			_logger.LogDebug("ValidateUser_IfUserIdWithValidValue_ThenSuccessResponse(): userInfo → {UserInfo}", userInfo);
            this.SetupUserControllerWrapperMock(userInfo);

            // Act
            var result = this._userValidator.ValidateUser(userId, null, null, out userInfo);
			_logger.LogDebug("ValidateUser_IfUserIdWithValidValue_ThenSuccessResponse(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.Null);
			_logger.LogDebug("<<< Exiting ValidateUser_IfUserIdWithValidValue_ThenSuccessResponse()");
        }

        [Test]
        public void ValidateUser_IfUserAllowedInSiteGroup_ThenSuccessResponse()
        {
			_logger.LogDebug(">>> Entering ValidateUser_IfUserAllowedInSiteGroup_ThenSuccessResponse()");
            // Arrange
            int? userId = 1;
            this.SetupForSiteGroup(true, userId.Value);
            UserInfo userInfo;

            // Act
            var result = this._userValidator.ValidateUser(userId, null, null, out userInfo);
			_logger.LogDebug("ValidateUser_IfUserAllowedInSiteGroup_ThenSuccessResponse(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.Null);
			_logger.LogDebug("<<< Exiting ValidateUser_IfUserAllowedInSiteGroup_ThenSuccessResponse()");
        }

        [Test]
        public void ValidateUser_IfUserNotAllowedInSiteGroup_ThenErrorResponse()
        {
			_logger.LogDebug(">>> Entering ValidateUser_IfUserNotAllowedInSiteGroup_ThenErrorResponse()");
            // Arrange
            int? userId = 1;
            this.SetupForSiteGroup(false, userId.Value);
            UserInfo userInfo;

            // Act
            var result = this._userValidator.ValidateUser(userId, null, null, out userInfo);
			_logger.LogDebug("ValidateUser_IfUserNotAllowedInSiteGroup_ThenErrorResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.True);
			_logger.LogDebug("<<< Exiting ValidateUser_IfUserNotAllowedInSiteGroup_ThenErrorResponse()");
        }

        [Test]
        public void ValidateUser_IfUserIdNotFound_ThenErrorResponse()
        {
			_logger.LogDebug(">>> Entering ValidateUser_IfUserIdNotFound_ThenErrorResponse()");
            // Arrange
            int? userId = 1;
            UserInfo userInfo = null;
            this.SetupUserControllerWrapperMock(userInfo);

            ArrayList portals = new ArrayList();
			_logger.LogDebug("ValidateUser_IfUserIdNotFound_ThenErrorResponse(): portals → {Portals}", portals);
            this._portalControllerMock.Setup(p => p.GetPortals()).Returns(portals);

            // Act
            var result = this._userValidator.ValidateUser(userId, null, null, out userInfo);
			_logger.LogDebug("ValidateUser_IfUserIdNotFound_ThenErrorResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.True);
			_logger.LogDebug("<<< Exiting ValidateUser_IfUserIdNotFound_ThenErrorResponse()");
        }

        [Test]
        public void ValidateUser_IfUserIdWithoutValue_ThenErrorResponse()
        {
			_logger.LogDebug(">>> Entering ValidateUser_IfUserIdWithoutValue_ThenErrorResponse()");
            // Arrange
            int? userId = null;
            UserInfo userInfo;

            // Act
            var result = this._userValidator.ValidateUser(userId, null, null, out userInfo);
			_logger.LogDebug("ValidateUser_IfUserIdWithoutValue_ThenErrorResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.True);
			_logger.LogDebug("<<< Exiting ValidateUser_IfUserIdWithoutValue_ThenErrorResponse()");
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
				_logger.LogDebug("<<< Exiting SetupUserControllerWrapperMock(userInfo={UserInfo})", userInfo);
        }

        private void SetupForSiteGroup(bool isAllowed, int userId)
        {
			_logger.LogDebug(">>> Entering SetupForSiteGroup(isAllowed={IsAllowed},userId={UserId})", isAllowed, userId);
            var userInfo = this.GetUserInfoWithProfile(userId);

			_logger.LogDebug("SetupForSiteGroup(isAllowed={IsAllowed},userId={UserId}): userInfo → {UserInfo}", isAllowed, userId, userInfo);
            var otherPortalId = 2;
            var portals = new ArrayList();
			_logger.LogDebug("SetupForSiteGroup(isAllowed={IsAllowed},userId={UserId}): portals → {Portals}", isAllowed, userId, portals);
            portals.Add(new PortalInfo() { PortalID = otherPortalId });
            this._portalControllerMock.Setup(p => p.GetPortals()).Returns(portals);

            this._userControllerWrapperMock.Setup(u => u.GetUserById(It.IsAny<int>(), userId)).Returns(userInfo);
            this._contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(It.IsAny<int>(), It.IsAny<PortalSettings>(), true)).Returns(isAllowed);
			_logger.LogDebug("<<< Exiting SetupForSiteGroup(isAllowed={IsAllowed},userId={UserId})", isAllowed, userId);
        }

        private UserInfo GetUserInfoWithProfile(int userId)
        {
			_logger.LogDebug(">>> Entering GetUserInfoWithProfile(userId={UserId})", userId);
            var userInfo = new UserInfo();
            var profile = new UserProfile();
			_logger.LogDebug("GetUserInfoWithProfile(userId={UserId}): profile → {Profile}", userId, profile);
			_logger.LogDebug("GetUserInfoWithProfile(userId={UserId}): profile.FirstName → {Arg1}", userId, profile.FirstName);
			_logger.LogDebug("GetUserInfoWithProfile(userId={UserId}): userInfo.UserID → {Arg1}", userId, userInfo.UserID);
			_logger.LogDebug("GetUserInfoWithProfile(userId={UserId}): userInfo.Profile → {Arg1}", userId, userInfo.Profile);
			_logger.LogDebug("GetUserInfoWithProfile(userId={UserId}): userInfo → {UserInfo}", userId, userInfo);
            profile.FirstName = "testUser";
            userInfo.UserID = userId;
            userInfo.Profile = profile;
			_logger.LogDebug("<<< Exiting GetUserInfoWithProfile(userId={UserId})", userId);
            return userInfo;
        }
    }
}
