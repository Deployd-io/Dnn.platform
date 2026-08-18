// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Tests
{
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Users.Components;
    using Dnn.PersonaBar.Users.Components.Prompt.Commands;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]

    public class DeleteUserUnitTests : CommandTests<DeleteUser>
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;
        private FakeServiceProvider serviceProvider;

        protected override string CommandName => "Delete-User";

        [Test]
        public void Run_DeleteValidUserId_ReturnSuccessResponse()
        {
			_logger.LogDebug(">>> Entering Run_DeleteValidUserId_ReturnSuccessResponse()");
            // Arrange
            int userId = 2;
			_logger.LogDebug("Run_DeleteValidUserId_ReturnSuccessResponse(): userId → {UserId}", userId);

            UserInfo userInfo = this.GetUser(userId, false);
			_logger.LogDebug("Run_DeleteValidUserId_ReturnSuccessResponse(): userInfo → {UserInfo}", userInfo);

            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);
            this._userValidatorMock
                .Setup(u => u.ValidateUser(-1, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            this._userControllerWrapperMock
                .Setup(u => u.DeleteUserAndClearCache(ref userInfo, false, false))
                .Returns(true);
            this._userControllerWrapperMock
                .Setup(u => u.GetUserById(this.testPortalId, userId))
                .Returns(userInfo);

            // Act
            var result = this.RunCommand(userId.ToString());
			_logger.LogDebug("Run_DeleteValidUserId_ReturnSuccessResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.False);
			_logger.LogDebug("<<< Exiting Run_DeleteValidUserId_ReturnSuccessResponse()");
        }

        [Test]
        public void Run_DeleteAlreadyDeletedUser_ReturnErrorResponse()
        {
			_logger.LogDebug(">>> Entering Run_DeleteAlreadyDeletedUser_ReturnErrorResponse()");
            // Arrange
            int userId = 2;
			_logger.LogDebug("Run_DeleteAlreadyDeletedUser_ReturnErrorResponse(): userId → {UserId}", userId);

            UserInfo userInfo = this.GetUser(userId, true);
			_logger.LogDebug("Run_DeleteAlreadyDeletedUser_ReturnErrorResponse(): userInfo → {UserInfo}", userInfo);

            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand(userId.ToString());
			_logger.LogDebug("Run_DeleteAlreadyDeletedUser_ReturnErrorResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.True);
			_logger.LogDebug("<<< Exiting Run_DeleteAlreadyDeletedUser_ReturnErrorResponse()");
        }

        [Test]
        public void Run_DeleteUserFailed_ReturnErrorResponse()
        {
			_logger.LogDebug(">>> Entering Run_DeleteUserFailed_ReturnErrorResponse()");
            // Arrange
            int userId = 2;
			_logger.LogDebug("Run_DeleteUserFailed_ReturnErrorResponse(): userId → {UserId}", userId);

            UserInfo userInfo = this.GetUser(userId, false);
			_logger.LogDebug("Run_DeleteUserFailed_ReturnErrorResponse(): userInfo → {UserInfo}", userInfo);

            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);
            this._userValidatorMock
                .Setup(u => u.ValidateUser(-1, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            this._userControllerWrapperMock
                .Setup(u => u.DeleteUserAndClearCache(ref userInfo, false, false))
                .Returns(false);

            // Act
            var result = this.RunCommand(userId.ToString());
			_logger.LogDebug("Run_DeleteUserFailed_ReturnErrorResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.True);
			_logger.LogDebug("<<< Exiting Run_DeleteUserFailed_ReturnErrorResponse()");
        }

        [Test]
        public void Run_DeleteNullUserId_ReturnErrorResponse()
        {
			_logger.LogDebug(">>> Entering Run_DeleteNullUserId_ReturnErrorResponse()");
            // Arrange
            UserInfo userinfo;
            this.errorResultModel = new ConsoleErrorResultModel();
            this._userValidatorMock
                .Setup(u => u.ValidateUser(-1, this.portalSettings, null, out userinfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand();
			_logger.LogDebug("Run_DeleteNullUserId_ReturnErrorResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.True);
			_logger.LogDebug("<<< Exiting Run_DeleteNullUserId_ReturnErrorResponse()");
        }

        protected override DeleteUser CreateCommand()
        {
			_logger.LogDebug(">>> Entering CreateCommand()");
			_logger.LogDebug("<<< Exiting CreateCommand()");
            return new DeleteUser(this._userValidatorMock.Object, this._userControllerWrapperMock.Object);
        }

        protected override void ChildSetup()
        {
			_logger.LogDebug(">>> Entering ChildSetup()");
            this._userValidatorMock = new Mock<IUserValidator>();
            this._userControllerWrapperMock = new Mock<IUserControllerWrapper>();
			_logger.LogDebug("ChildSetup(): _userControllerWrapperMock → {_userControllerWrapperMock}", _userControllerWrapperMock);
			_logger.LogDebug("ChildSetup(): _userValidatorMock → {_userValidatorMock}", _userValidatorMock);
            this.serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IHostSettings>()));
			_logger.LogDebug("ChildSetup(): serviceProvider → {ServiceProvider}", serviceProvider);
			_logger.LogDebug("<<< Exiting ChildSetup()");
        }

        [TearDown]
        protected override void ChildTearDown()
        {
			_logger.LogDebug(">>> Entering ChildTearDown()");
            this.serviceProvider.Dispose();
			_logger.LogDebug("<<< Exiting ChildTearDown()");
        }
    }
}
