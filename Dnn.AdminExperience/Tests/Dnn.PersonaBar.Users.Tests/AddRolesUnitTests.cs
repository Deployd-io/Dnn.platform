// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Users.Tests
{
    using System;
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Roles.Components;
    using Dnn.PersonaBar.Users.Components;
    using Dnn.PersonaBar.Users.Components.Prompt.Commands;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class AddRolesUnitTests : CommandTests<AddRoles>
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private Mock<IUserValidator> userValidatorMock;
        private Mock<IUsersController> usersControllerMock;
        private Mock<IRolesController> rolesControllerMock;
        private FakeServiceProvider serviceProvider;

        protected override string CommandName => "Add-roles";

        [TestCase]
        public void Run_AddRolesWithValidArguments_ReturnSuccessResponse()
        {
			_logger.LogDebug(">>> Entering Run_AddRolesWithValidArguments_ReturnSuccessResponse()");
            // Arrange
            var userId = 2;
			_logger.LogDebug("Run_AddRolesWithValidArguments_ReturnSuccessResponse(): userId → {UserId}", userId);
            var userInfo = this.GetUser(userId, true);
			_logger.LogDebug("Run_AddRolesWithValidArguments_ReturnSuccessResponse(): userInfo → {UserInfo}", userInfo);

            this.userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            var userInfoList = new List<UserRoleInfo>(
                    new[]
                    {
                        new UserRoleInfo
                        {
                            RoleID = 1,
                            PortalID = this.testPortalId,
                            IsPublic = true,
                        },
                    });

            var total = 1;
			_logger.LogDebug("Run_AddRolesWithValidArguments_ReturnSuccessResponse(): total → {Total}", total);
            this.usersControllerMock
                .Setup(u => u.GetUserRoles(userInfo, string.Empty, out total, -1, -1))
                .Returns(userInfoList);

            var rolesList = new List<RoleInfo>(
                    new[]
                    {
                        new RoleInfo
                        {
                            RoleID = 1,
                            RoleName = "Tester",
                        },
                    });

            this.rolesControllerMock
                .Setup(r => r.GetRolesByNames(this.portalSettings, It.IsAny<int>(), It.IsAny<IList<string>>()))
                .Returns(rolesList);

            // Act
            var result = this.RunCommand(userId.ToString(), "--roles", "Tester");
			_logger.LogDebug("Run_AddRolesWithValidArguments_ReturnSuccessResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.False);
			_logger.LogDebug("<<< Exiting Run_AddRolesWithValidArguments_ReturnSuccessResponse()");
        }

        [TestCase]
        public void Run_AddRolesWhenUserNotValid_ReturnErrorResponse()
        {
			_logger.LogDebug(">>> Entering Run_AddRolesWhenUserNotValid_ReturnErrorResponse()");
            // Arrange
            var userId = 2;
			_logger.LogDebug("Run_AddRolesWhenUserNotValid_ReturnErrorResponse(): userId → {UserId}", userId);
            UserInfo userInfo = null;

            this.errorResultModel = new ConsoleErrorResultModel("Invalid userId");

            this.userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand(userId.ToString());
			_logger.LogDebug("Run_AddRolesWhenUserNotValid_ReturnErrorResponse(): result → {Result}", result);

            // Assert
            Assert.That(result.IsError, Is.True);
			_logger.LogDebug("<<< Exiting Run_AddRolesWhenUserNotValid_ReturnErrorResponse()");
        }

        [TestCase]
        public void Run_AddRolesWhenRoleNotValid_ThrowsException()
        {
			_logger.LogDebug(">>> Entering Run_AddRolesWhenRoleNotValid_ThrowsException()");
            // Arrange
            const int UserId = 2;
			_logger.LogDebug("Run_AddRolesWhenRoleNotValid_ThrowsException(): UserId → {UserId}", UserId);
            var userInfo = this.GetUser(UserId, true);
			_logger.LogDebug("Run_AddRolesWhenRoleNotValid_ThrowsException(): userInfo → {UserInfo}", userInfo);

            this.userValidatorMock
                .Setup(u => u.ValidateUser(UserId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            var userInfoList = new List<UserRoleInfo>(
            [
                new UserRoleInfo
                        {
                            RoleID = 1,
                            PortalID = this.testPortalId,
                            IsPublic = true,
                        }
            ]);

            var total = 1;
			_logger.LogDebug("Run_AddRolesWhenRoleNotValid_ThrowsException(): total → {Total}", total);
            this.usersControllerMock
                .Setup(u => u.GetUserRoles(userInfo, string.Empty, out total, -1, -1))
                .Returns(userInfoList);

            var rolesList = new List<RoleInfo>(
            [
                new RoleInfo
                        {
                            RoleID = 1,
                            RoleName = "Tester",
                        }
            ]);

            this.rolesControllerMock
                .Setup(r => r.GetRolesByNames(this.portalSettings, -1, It.IsAny<IList<string>>()))
                .Returns(rolesList);

            Assert.Throws<RoleNotFoundException>(() => this.RunCommand(UserId.ToString(), "--roles", "Not Tester"), "Should throw exception");
			_logger.LogDebug("<<< Exiting Run_AddRolesWhenRoleNotValid_ThrowsException()");
        }

        protected override void ChildSetup()
        {
			_logger.LogDebug(">>> Entering ChildSetup()");
            this.userValidatorMock = new Mock<IUserValidator>();
            this.usersControllerMock = new Mock<IUsersController>();
			_logger.LogDebug("ChildSetup(): usersControllerMock → {UsersControllerMock}", usersControllerMock);
            this.rolesControllerMock = new Mock<IRolesController>();
			_logger.LogDebug("ChildSetup(): rolesControllerMock → {RolesControllerMock}", rolesControllerMock);
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					_logger.LogDebug("ChildSetup(): serviceProvider → {ServiceProvider}", serviceProvider);
					_logger.LogDebug("ChildSetup(): userValidatorMock → {UserValidatorMock}", userValidatorMock);
                    services.AddSingleton(this.userValidatorMock.Object);
                    services.AddSingleton(this.usersControllerMock.Object);
                    services.AddSingleton(this.rolesControllerMock.Object);
                });
				_logger.LogDebug("<<< Exiting ChildSetup()");
        }

        [TearDown]
        protected override void ChildTearDown()
        {
			_logger.LogDebug(">>> Entering ChildTearDown()");
            this.serviceProvider.Dispose();
			_logger.LogDebug("<<< Exiting ChildTearDown()");
        }

        protected override AddRoles CreateCommand()
        {
			_logger.LogDebug(">>> Entering CreateCommand()");
			_logger.LogDebug("<<< Exiting CreateCommand()");
            return new AddRoles(this.userValidatorMock.Object, this.usersControllerMock.Object, this.rolesControllerMock.Object);
        }
    }
}
