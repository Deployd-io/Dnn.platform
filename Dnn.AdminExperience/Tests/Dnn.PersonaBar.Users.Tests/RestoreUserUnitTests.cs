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

    [TestFixture]

    public class RestoreUserUnitTests : CommandTests<RestoreUser>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IRecyclebinController> _recyclebinControllerMock;
        private FakeServiceProvider serviceProvider;

        protected override string CommandName => "Restore-User";

        [Test]
        public void Run_RestoreValidUserId_ReturnSuccessResponse()
        {
			Logger.LogDebug(">>> Entering Run_RestoreValidUserId_ReturnSuccessResponse()");
            // Arrange
            var userId = 2;
            UserInfo userInfo = this.GetUser(userId, true);
			Logger.LogDebug("Run_RestoreValidUserId_ReturnSuccessResponse(): userId → {UserId}", userId);
			Logger.LogDebug("Run_RestoreValidUserId_ReturnSuccessResponse(): userInfo → {UserInfo}", userInfo);
            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);
            var message = string.Empty;
            this._recyclebinControllerMock.Setup(r => r.RestoreUser(userInfo, out message)).Returns(true);

            // Act
            var result = this.RunCommand(userId.ToString());

			Logger.LogDebug("Run_RestoreValidUserId_ReturnSuccessResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.False);
			Logger.LogDebug("<<< Exiting Run_RestoreValidUserId_ReturnSuccessResponse()");
        }

        [Test]
        public void Run_RecycleBinControllerRestoringError_ReturnErrorResponse()
        {
			Logger.LogDebug(">>> Entering Run_RecycleBinControllerRestoringError_ReturnErrorResponse()");
            // Arrange
            var userId = 2;
            UserInfo userInfo = this.GetUser(userId, true);
			Logger.LogDebug("Run_RecycleBinControllerRestoringError_ReturnErrorResponse(): userId → {UserId}", userId);
			Logger.LogDebug("Run_RecycleBinControllerRestoringError_ReturnErrorResponse(): userInfo → {UserInfo}", userInfo);
            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);
            var message = string.Empty;
            this._recyclebinControllerMock.Setup(r => r.RestoreUser(userInfo, out message)).Returns(false);

            // Act
            var result = this.RunCommand(userId.ToString());

			Logger.LogDebug("Run_RecycleBinControllerRestoringError_ReturnErrorResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.True);
			Logger.LogDebug("<<< Exiting Run_RecycleBinControllerRestoringError_ReturnErrorResponse()");
        }

        [Test]
        public void Run_RestoreNotDeletedUser_ReturnErrorResponse()
        {
			Logger.LogDebug(">>> Entering Run_RestoreNotDeletedUser_ReturnErrorResponse()");
            // Arrange
            var userId = 2;
            UserInfo userinfo = this.GetUser(userId, false);
			Logger.LogDebug("Run_RestoreNotDeletedUser_ReturnErrorResponse(): userId → {UserId}", userId);
			Logger.LogDebug("Run_RestoreNotDeletedUser_ReturnErrorResponse(): userinfo → {Userinfo}", userinfo);
            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userinfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand(userId.ToString());

			Logger.LogDebug("Run_RestoreNotDeletedUser_ReturnErrorResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.True);
			Logger.LogDebug("<<< Exiting Run_RestoreNotDeletedUser_ReturnErrorResponse()");
        }

        [Test]
        public void Run_RestoreNullUserId_ReturnErrorResponse()
        {
			Logger.LogDebug(">>> Entering Run_RestoreNullUserId_ReturnErrorResponse()");
            // Arrange
            this.errorResultModel = new ConsoleErrorResultModel();

            UserInfo userinfo;
            this._userValidatorMock
                .Setup(u => u.ValidateUser(-1, this.portalSettings, null, out userinfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand();

			Logger.LogDebug("Run_RestoreNullUserId_ReturnErrorResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.True);
			Logger.LogDebug("<<< Exiting Run_RestoreNullUserId_ReturnErrorResponse()");
        }

        protected override RestoreUser CreateCommand()
        {
			Logger.LogDebug(">>> Entering CreateCommand()");
			Logger.LogDebug("<<< Exiting CreateCommand()");
            return new RestoreUser(this._userValidatorMock.Object, this._recyclebinControllerMock.Object);
        }

        [SetUp]
        protected override void ChildSetup()
        {
			Logger.LogDebug(">>> Entering ChildSetup()");
            this._userValidatorMock = new Mock<IUserValidator>();
            this._recyclebinControllerMock = new Mock<IRecyclebinController>();
			Logger.LogDebug("ChildSetup(): _recyclebinControllerMock → {_recyclebinControllerMock}", _recyclebinControllerMock);
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
