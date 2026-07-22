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

    [TestFixture]

    public class DeleteUserUnitTests : CommandTests<DeleteUser>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;
        private FakeServiceProvider serviceProvider;

        protected override string CommandName => "Delete-User";

        [Test]
        public void Run_DeleteValidUserId_ReturnSuccessResponse()
        {
			Logger.LogDebug(">>> Entering Run_DeleteValidUserId_ReturnSuccessResponse()");
            // Arrange
            int userId = 2;

			Logger.LogDebug("Run_DeleteValidUserId_ReturnSuccessResponse(): userId → {UserId}", userId);
            UserInfo userInfo = this.GetUser(userId, false);

			Logger.LogDebug("Run_DeleteValidUserId_ReturnSuccessResponse(): userInfo → {UserInfo}", userInfo);
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

			Logger.LogDebug("Run_DeleteValidUserId_ReturnSuccessResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.False);
			Logger.LogDebug("<<< Exiting Run_DeleteValidUserId_ReturnSuccessResponse()");
        }

        [Test]
        public void Run_DeleteAlreadyDeletedUser_ReturnErrorResponse()
        {
			Logger.LogDebug(">>> Entering Run_DeleteAlreadyDeletedUser_ReturnErrorResponse()");
            // Arrange
            int userId = 2;

			Logger.LogDebug("Run_DeleteAlreadyDeletedUser_ReturnErrorResponse(): userId → {UserId}", userId);
            UserInfo userInfo = this.GetUser(userId, true);

			Logger.LogDebug("Run_DeleteAlreadyDeletedUser_ReturnErrorResponse(): userInfo → {UserInfo}", userInfo);
            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand(userId.ToString());

			Logger.LogDebug("Run_DeleteAlreadyDeletedUser_ReturnErrorResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.True);
			Logger.LogDebug("<<< Exiting Run_DeleteAlreadyDeletedUser_ReturnErrorResponse()");
        }

        [Test]
        public void Run_DeleteUserFailed_ReturnErrorResponse()
        {
			Logger.LogDebug(">>> Entering Run_DeleteUserFailed_ReturnErrorResponse()");
            // Arrange
            int userId = 2;

			Logger.LogDebug("Run_DeleteUserFailed_ReturnErrorResponse(): userId → {UserId}", userId);
            UserInfo userInfo = this.GetUser(userId, false);

			Logger.LogDebug("Run_DeleteUserFailed_ReturnErrorResponse(): userInfo → {UserInfo}", userInfo);
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

			Logger.LogDebug("Run_DeleteUserFailed_ReturnErrorResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.True);
			Logger.LogDebug("<<< Exiting Run_DeleteUserFailed_ReturnErrorResponse()");
        }

        [Test]
        public void Run_DeleteNullUserId_ReturnErrorResponse()
        {
			Logger.LogDebug(">>> Entering Run_DeleteNullUserId_ReturnErrorResponse()");
            // Arrange
            UserInfo userinfo;
            this.errorResultModel = new ConsoleErrorResultModel();
            this._userValidatorMock
                .Setup(u => u.ValidateUser(-1, this.portalSettings, null, out userinfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand();

			Logger.LogDebug("Run_DeleteNullUserId_ReturnErrorResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.True);
			Logger.LogDebug("<<< Exiting Run_DeleteNullUserId_ReturnErrorResponse()");
        }

        protected override DeleteUser CreateCommand()
        {
			Logger.LogDebug(">>> Entering CreateCommand()");
			Logger.LogDebug("<<< Exiting CreateCommand()");
            return new DeleteUser(this._userValidatorMock.Object, this._userControllerWrapperMock.Object);
        }

        protected override void ChildSetup()
        {
			Logger.LogDebug(">>> Entering ChildSetup()");
            this._userValidatorMock = new Mock<IUserValidator>();
            this._userControllerWrapperMock = new Mock<IUserControllerWrapper>();
			Logger.LogDebug("ChildSetup(): _userControllerWrapperMock → {_userControllerWrapperMock}", _userControllerWrapperMock);
			Logger.LogDebug("ChildSetup(): _userValidatorMock → {_userValidatorMock}", _userValidatorMock);
            this.serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IHostSettings>()));
			Logger.LogDebug("<<< Exiting ChildSetup()");
			Logger.LogDebug("ChildSetup(): serviceProvider → {ServiceProvider}", serviceProvider);
        }

        [TearDown]
        protected override void ChildTearDown()
        {
			Logger.LogDebug(">>> Entering ChildTearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting ChildTearDown()");
        }
    }
}
