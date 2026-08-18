// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Tests
{
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Recyclebin.Components;
    using Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands;
    using Dnn.PersonaBar.Users.Components;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]

    public class RestoreUserUnitTests : CommandTests<RestoreUser>
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IRecyclebinController> _recyclebinControllerMock;
        private FakeServiceProvider serviceProvider;

        protected override string CommandName => "Restore-User";

        [Test]
        public void Run_RestoreValidUserId_ReturnSuccessResponse()
        {
			_logger.LogDebug(">>> Entering Run_RestoreValidUserId_ReturnSuccessResponse()");
            // Arrange
            var userId = 2;
			_logger.LogDebug("Run_RestoreValidUserId_ReturnSuccessResponse(): userId → {UserId}", userId);
            UserInfo userInfo = this.GetUser(userId, true);
			_logger.LogDebug("Run_RestoreValidUserId_ReturnSuccessResponse(): userInfo → {UserInfo}", userInfo);
            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);
            var message = string.Empty;
            this._recyclebinControllerMock.Setup(r => r.RestoreUser(userInfo, out message)).Returns(true);

            // Act
            var result = this.RunCommand(userId.ToString());
			_logger.LogDebug("Run_RestoreValidUserId_ReturnSuccessResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.False);
			_logger.LogDebug("<<< Exiting Run_RestoreValidUserId_ReturnSuccessResponse()");
        }

        [Test]
        public void Run_RecycleBinControllerRestoringError_ReturnErrorResponse()
        {
			_logger.LogDebug(">>> Entering Run_RecycleBinControllerRestoringError_ReturnErrorResponse()");
            // Arrange
            var userId = 2;
			_logger.LogDebug("Run_RecycleBinControllerRestoringError_ReturnErrorResponse(): userId → {UserId}", userId);
            UserInfo userInfo = this.GetUser(userId, true);
			_logger.LogDebug("Run_RecycleBinControllerRestoringError_ReturnErrorResponse(): userInfo → {UserInfo}", userInfo);
            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);
            var message = string.Empty;
            this._recyclebinControllerMock.Setup(r => r.RestoreUser(userInfo, out message)).Returns(false);

            // Act
            var result = this.RunCommand(userId.ToString());
			_logger.LogDebug("Run_RecycleBinControllerRestoringError_ReturnErrorResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.True);
			_logger.LogDebug("<<< Exiting Run_RecycleBinControllerRestoringError_ReturnErrorResponse()");
        }

        [Test]
        public void Run_RestoreNotDeletedUser_ReturnErrorResponse()
        {
			_logger.LogDebug(">>> Entering Run_RestoreNotDeletedUser_ReturnErrorResponse()");
            // Arrange
            var userId = 2;
			_logger.LogDebug("Run_RestoreNotDeletedUser_ReturnErrorResponse(): userId → {UserId}", userId);
            UserInfo userinfo = this.GetUser(userId, false);
			_logger.LogDebug("Run_RestoreNotDeletedUser_ReturnErrorResponse(): userinfo → {Userinfo}", userinfo);
            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userinfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand(userId.ToString());
			_logger.LogDebug("Run_RestoreNotDeletedUser_ReturnErrorResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.True);
			_logger.LogDebug("<<< Exiting Run_RestoreNotDeletedUser_ReturnErrorResponse()");
        }

        [Test]
        public void Run_RestoreNullUserId_ReturnErrorResponse()
        {
			_logger.LogDebug(">>> Entering Run_RestoreNullUserId_ReturnErrorResponse()");
            // Arrange
            this.errorResultModel = new ConsoleErrorResultModel();

            UserInfo userinfo;
            this._userValidatorMock
                .Setup(u => u.ValidateUser(-1, this.portalSettings, null, out userinfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand();
			_logger.LogDebug("Run_RestoreNullUserId_ReturnErrorResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.True);
			_logger.LogDebug("<<< Exiting Run_RestoreNullUserId_ReturnErrorResponse()");
        }

        protected override RestoreUser CreateCommand()
        {
			_logger.LogDebug(">>> Entering CreateCommand()");
			_logger.LogDebug("<<< Exiting CreateCommand()");
            return new RestoreUser(this._userValidatorMock.Object, this._recyclebinControllerMock.Object);
        }

        [SetUp]
        protected override void ChildSetup()
        {
			_logger.LogDebug(">>> Entering ChildSetup()");
            this._userValidatorMock = new Mock<IUserValidator>();
            this._recyclebinControllerMock = new Mock<IRecyclebinController>();
			_logger.LogDebug("ChildSetup(): _recyclebinControllerMock → {_recyclebinControllerMock}", _recyclebinControllerMock);
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
