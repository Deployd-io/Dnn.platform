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

    public class SetUserUnitTests : CommandTests<SetUser>
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IUsersController> _usersControllerMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;
        private FakeServiceProvider serviceProvider;

        protected override string CommandName => "Set-User";

        [Test]
        public void Run_UserIdNull_ReturnErrorResponse()
        {
			_logger.LogDebug(">>> Entering Run_UserIdNull_ReturnErrorResponse()");
            // Arrange
            UserInfo userInfo;
            this.errorResultModel = new ConsoleErrorResultModel();
            this._userValidatorMock
                .Setup(u => u.ValidateUser(-1, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand("--username", "testusername", "--firstname", "testfirstname", "--lastname", "testlastname");
			_logger.LogDebug("Run_UserIdNull_ReturnErrorResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.True);
			_logger.LogDebug("<<< Exiting Run_UserIdNull_ReturnErrorResponse()");
        }

        [TestCase("--email", "user@gm.com")]
        [TestCase("--password", "pass1233")]
        [TestCase("--username", "user4pmt")]
        [TestCase("--displayname", "user4displayname")]
        [TestCase("--approved", "true")]
        public void Run_ValidCommand_ReturnSuccessResponse(string attributeName, string attributeValue)
        {
			_logger.LogDebug(">>> Entering Run_ValidCommand_ReturnSuccessResponse(attributeName={AttributeName},attributeValue={AttributeValue})", attributeName, attributeValue);
            // Arrange
            var userId = 4;
			_logger.LogDebug("Run_ValidCommand_ReturnSuccessResponse(attributeName={AttributeName},attributeValue={AttributeValue}): userId → {UserId}", attributeName, attributeValue, userId);
            var userInfo = this.GetUser(userId, false);
			_logger.LogDebug("Run_ValidCommand_ReturnSuccessResponse(attributeName={AttributeName},attributeValue={AttributeValue}): userInfo → {UserInfo}", attributeName, attributeValue, userInfo);
			_logger.LogDebug("Run_ValidCommand_ReturnSuccessResponse(attributeName={AttributeName},attributeValue={AttributeValue}): userInfo.FirstName → {Arg2}", attributeName, attributeValue, userInfo.FirstName);
			_logger.LogDebug("Run_ValidCommand_ReturnSuccessResponse(attributeName={AttributeName},attributeValue={AttributeValue}): userInfo.LastName → {Arg2}", attributeName, attributeValue, userInfo.LastName);
			_logger.LogDebug("Run_ValidCommand_ReturnSuccessResponse(attributeName={AttributeName},attributeValue={AttributeValue}): userInfo.Email → {Arg2}", attributeName, attributeValue, userInfo.Email);
            userInfo.FirstName = "userFirstName";
            userInfo.LastName = "userLastName";
            userInfo.Email = "user@email.com";

            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);
            this._userControllerWrapperMock
                .Setup(w => w.GetUserById(this.testPortalId, userId))
                .Returns(userInfo);

            // Act
            var result = this.RunCommand(userId.ToString(), "--firstname", "user4", "--lastname", "user4", attributeName, attributeValue);
			_logger.LogDebug("Run_ValidCommand_ReturnSuccessResponse(attributeName={AttributeName},attributeValue={AttributeValue}): result → {Result}", attributeName, attributeValue, result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsError, Is.False);
                Assert.That(result.Records, Is.EqualTo(1));
            }
			_logger.LogDebug("<<< Exiting Run_ValidCommand_ReturnSuccessResponse(attributeName={AttributeName},attributeValue={AttributeValue})", attributeName, attributeValue);
        }

        [SetUp]
        protected override void ChildSetup()
        {
			_logger.LogDebug(">>> Entering ChildSetup()");
            this._userValidatorMock = new Mock<IUserValidator>();
            this._usersControllerMock = new Mock<IUsersController>();
			_logger.LogDebug("ChildSetup(): _usersControllerMock → {_usersControllerMock}", _usersControllerMock);
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

        protected override SetUser CreateCommand()
        {
			_logger.LogDebug(">>> Entering CreateCommand()");
			_logger.LogDebug("<<< Exiting CreateCommand()");
            return new SetUser(this._userValidatorMock.Object, this._usersControllerMock.Object, this._userControllerWrapperMock.Object);
        }
    }
}
