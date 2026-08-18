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

    public class GetUserUnitTests : CommandTests<GetUser>
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private int _userId;
        private UserInfo _userInfo;

        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;
        private FakeServiceProvider serviceProvider;

        protected override string CommandName => "Get-User";

        [Test]
        public void Run_GetUserByEmailWithValidCommand_ShouldSuccessResponse()
        {
			_logger.LogDebug(">>> Entering Run_GetUserByEmailWithValidCommand_ShouldSuccessResponse()");
            // Arrange
            var recCount = 0;

            this._userControllerWrapperMock
                .Setup(c => c.GetUsersByEmail(
                    this.testPortalId,
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    ref recCount,
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(this._userId);

            this._userValidatorMock
                .Setup(u => u.ValidateUser(this._userId, this.portalSettings, null, out this._userInfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand("--email", "user1@g.com");
			_logger.LogDebug("Run_GetUserByEmailWithValidCommand_ShouldSuccessResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.False);
			_logger.LogDebug("<<< Exiting Run_GetUserByEmailWithValidCommand_ShouldSuccessResponse()");
        }

        [TestCase]
        public void Run_GetUserByUserNameWithValidCommand_ShouldSuccessResponse()
        {
			_logger.LogDebug(">>> Entering Run_GetUserByUserNameWithValidCommand_ShouldSuccessResponse()");
            // Arrange
            var recCount = 0;

            this._userControllerWrapperMock
                .Setup(c => c.GetUsersByUserName(
                    this.testPortalId,
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    ref recCount,
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(this._userId);

            this._userValidatorMock
                .Setup(u => u.ValidateUser(this._userId, this.portalSettings, null, out this._userInfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand("--username", "user1");
			_logger.LogDebug("Run_GetUserByUserNameWithValidCommand_ShouldSuccessResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.False);
			_logger.LogDebug("<<< Exiting Run_GetUserByUserNameWithValidCommand_ShouldSuccessResponse()");
        }

        [Test]
        public void Run_GetUserWithValidCommand_ShouldSuccessResponse()
        {
			_logger.LogDebug(">>> Entering Run_GetUserWithValidCommand_ShouldSuccessResponse()");
            // Arrange
            this._userValidatorMock
             .Setup(u => u.ValidateUser(this._userId, this.portalSettings, null, out this._userInfo))
             .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand(this._userId.ToString());
			_logger.LogDebug("Run_GetUserWithValidCommand_ShouldSuccessResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.False);
			_logger.LogDebug("<<< Exiting Run_GetUserWithValidCommand_ShouldSuccessResponse()");
        }

        [Test]
        public void Run_GetUserWithValidCommand_ShouldErrorResponse()
        {
			_logger.LogDebug(">>> Entering Run_GetUserWithValidCommand_ShouldErrorResponse()");
            // Arrange
            this.errorResultModel = new ConsoleErrorResultModel();

            this._userValidatorMock
                .Setup(u => u.ValidateUser(this._userId, this.portalSettings, null, out this._userInfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand(this._userId.ToString());
			_logger.LogDebug("Run_GetUserWithValidCommand_ShouldErrorResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.True);
			_logger.LogDebug("<<< Exiting Run_GetUserWithValidCommand_ShouldErrorResponse()");
        }

        [SetUp]
        protected override void ChildSetup()
        {
			_logger.LogDebug(">>> Entering ChildSetup()");
            this._userId = 3;
            this._userInfo = this.GetUser(this._userId, false);
			_logger.LogDebug("ChildSetup(): this._userId → {Arg0}", this._userId);
			_logger.LogDebug("ChildSetup(): _userInfo → {_userInfo}", _userInfo);
			_logger.LogDebug("ChildSetup(): _userId → {_userId}", _userId);

            this._userValidatorMock = new Mock<IUserValidator>();
			_logger.LogDebug("ChildSetup(): _userValidatorMock → {_userValidatorMock}", _userValidatorMock);
            this._userControllerWrapperMock = new Mock<IUserControllerWrapper>();
			_logger.LogDebug("ChildSetup(): _userControllerWrapperMock → {_userControllerWrapperMock}", _userControllerWrapperMock);
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

        protected override GetUser CreateCommand()
        {
			_logger.LogDebug(">>> Entering CreateCommand()");
			_logger.LogDebug("<<< Exiting CreateCommand()");
            return new GetUser(this._userValidatorMock.Object, this._userControllerWrapperMock.Object);
        }
    }
}
