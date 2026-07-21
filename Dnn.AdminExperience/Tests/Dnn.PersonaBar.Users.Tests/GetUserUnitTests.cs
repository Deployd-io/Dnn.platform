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

    public class GetUserUnitTests : CommandTests<GetUser>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private int _userId;
        private UserInfo _userInfo;

        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;
        private FakeServiceProvider serviceProvider;

        protected override string CommandName => "Get-User";

        [Test]
        public void Run_GetUserByEmailWithValidCommand_ShouldSuccessResponse()
        {
			Logger.LogDebug(">>> Entering Run_GetUserByEmailWithValidCommand_ShouldSuccessResponse()");
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

			Logger.LogDebug("Run_GetUserByEmailWithValidCommand_ShouldSuccessResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.False);
			Logger.LogDebug("<<< Exiting Run_GetUserByEmailWithValidCommand_ShouldSuccessResponse()");
        }

        [TestCase]
        public void Run_GetUserByUserNameWithValidCommand_ShouldSuccessResponse()
        {
			Logger.LogDebug(">>> Entering Run_GetUserByUserNameWithValidCommand_ShouldSuccessResponse()");
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

			Logger.LogDebug("Run_GetUserByUserNameWithValidCommand_ShouldSuccessResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.False);
			Logger.LogDebug("<<< Exiting Run_GetUserByUserNameWithValidCommand_ShouldSuccessResponse()");
        }

        [Test]
        public void Run_GetUserWithValidCommand_ShouldSuccessResponse()
        {
			Logger.LogDebug(">>> Entering Run_GetUserWithValidCommand_ShouldSuccessResponse()");
            // Arrange
            this._userValidatorMock
             .Setup(u => u.ValidateUser(this._userId, this.portalSettings, null, out this._userInfo))
             .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand(this._userId.ToString());

			Logger.LogDebug("Run_GetUserWithValidCommand_ShouldSuccessResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.False);
			Logger.LogDebug("<<< Exiting Run_GetUserWithValidCommand_ShouldSuccessResponse()");
        }

        [Test]
        public void Run_GetUserWithValidCommand_ShouldErrorResponse()
        {
			Logger.LogDebug(">>> Entering Run_GetUserWithValidCommand_ShouldErrorResponse()");
            // Arrange
            this.errorResultModel = new ConsoleErrorResultModel();

            this._userValidatorMock
                .Setup(u => u.ValidateUser(this._userId, this.portalSettings, null, out this._userInfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand(this._userId.ToString());

			Logger.LogDebug("Run_GetUserWithValidCommand_ShouldErrorResponse(): result → {Result}", result);
            // Assert
            Assert.That(result.IsError, Is.True);
			Logger.LogDebug("<<< Exiting Run_GetUserWithValidCommand_ShouldErrorResponse()");
        }

        [SetUp]
        protected override void ChildSetup()
        {
			Logger.LogDebug(">>> Entering ChildSetup()");
            this._userId = 3;
            this._userInfo = this.GetUser(this._userId, false);
			Logger.LogDebug("ChildSetup(): this._userId → {Arg0}", this._userId);
			Logger.LogDebug("ChildSetup(): _userInfo → {_userInfo}", _userInfo);
			Logger.LogDebug("ChildSetup(): _userId → {_userId}", _userId);

            this._userValidatorMock = new Mock<IUserValidator>();
			Logger.LogDebug("ChildSetup(): _userValidatorMock → {_userValidatorMock}", _userValidatorMock);
            this._userControllerWrapperMock = new Mock<IUserControllerWrapper>();
			Logger.LogDebug("ChildSetup(): _userControllerWrapperMock → {_userControllerWrapperMock}", _userControllerWrapperMock);
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

        protected override GetUser CreateCommand()
        {
			Logger.LogDebug(">>> Entering CreateCommand()");
			Logger.LogDebug("<<< Exiting CreateCommand()");
            return new GetUser(this._userValidatorMock.Object, this._userControllerWrapperMock.Object);
        }
    }
}
