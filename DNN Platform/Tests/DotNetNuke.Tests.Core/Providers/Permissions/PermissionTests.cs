// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Permissions
{
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PermissionTests : DnnUnitTest
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const int UserId = 400;
        private FakeServiceProvider serviceProvider;

        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
			Logger.LogDebug(">>> Entering OneTimeSetUp()");
            base.OneTimeSetUp();
			Logger.LogDebug("<<< Exiting OneTimeSetUp()");
        }

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.serviceProvider = FakeServiceProvider.Setup(services =>
            {
				Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
                services.AddSingleton(Mock.Of<ICryptographyProvider>());
                services.AddSingleton(new ListController(Mock.Of<IEventLogger>(), Mock.Of<IHostSettings>()));
                services.AddSingleton(Mock.Of<IPortalController>());
                services.AddSingleton(Mock.Of<IApplicationStatusInfo>());
                services.AddSingleton(Mock.Of<IPortalGroupController>());
            });
			Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider?.Dispose();
            PortalController.ClearInstance();
            RoleController.ClearInstance();
            RelationshipController.ClearInstance();
            UserController.ClearInstance();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void PortalSecurity_IsInRoles_Super_User_Is_Always_True()
        {
			Logger.LogDebug(">>> Entering PortalSecurity_IsInRoles_Super_User_Is_Always_True()");
            var user = new UserInfo() { IsSuperUser = true, };
            const string roles = "";
			Logger.LogDebug("PortalSecurity_IsInRoles_Super_User_Is_Always_True(): user → {User}", user);
            var portalSettings = SetupPortalSettings();

			Logger.LogDebug("PortalSecurity_IsInRoles_Super_User_Is_Always_True(): portalSettings → {PortalSettings}", portalSettings);
            Assert.That(PortalSecurity.IsInRoles(user, portalSettings, roles), Is.True);
			Logger.LogDebug("<<< Exiting PortalSecurity_IsInRoles_Super_User_Is_Always_True()");
        }

        [Test]
        public void PortalSecurity_IsInRoles_All_Users_Role_Is_Always_True()
        {
			Logger.LogDebug(">>> Entering PortalSecurity_IsInRoles_All_Users_Role_Is_Always_True()");
            var user = new UserInfo() { IsSuperUser = false, };
            const string roles = Globals.glbRoleAllUsersName;
			Logger.LogDebug("PortalSecurity_IsInRoles_All_Users_Role_Is_Always_True(): user → {User}", user);
            var portalSettings = SetupPortalSettings();

			Logger.LogDebug("PortalSecurity_IsInRoles_All_Users_Role_Is_Always_True(): portalSettings → {PortalSettings}", portalSettings);
            Assert.That(PortalSecurity.IsInRoles(user, portalSettings, roles), Is.True);
			Logger.LogDebug("<<< Exiting PortalSecurity_IsInRoles_All_Users_Role_Is_Always_True()");
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdmin_IsInRole_Is_True()
        {
			Logger.LogDebug(">>> Entering PortalSecurity_IsInRoles_NonAdmin_IsInRole_Is_True()");
            var user = new UserInfo { IsSuperUser = false, UserID = UserId };

            var mockRoleController = new Mock<IRoleController>();
            mockRoleController.Setup(rc => rc.GetUserRoles(It.Is<UserInfo>(u => u.UserID == UserId), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { new UserRoleInfo() { RoleName = "SomeRoleName", Status = RoleStatus.Approved } });
			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdmin_IsInRole_Is_True(): mockRoleController → {MockRoleController}", mockRoleController);
            RoleController.SetTestableInstance(mockRoleController.Object);

            const string roles = "SomeRoleName";
            var portalSettings = SetupPortalSettings();
			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdmin_IsInRole_Is_True(): portalSettings → {PortalSettings}", portalSettings);
            Assert.That(PortalSecurity.IsInRoles(user, portalSettings, roles), Is.True);
			Logger.LogDebug("<<< Exiting PortalSecurity_IsInRoles_NonAdmin_IsInRole_Is_True()");
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFollowerRoleAndRoleEntityIsFollowedByUser()
        {
			Logger.LogDebug(">>> Entering PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFollowerRoleAndRoleEntityIsFollowedByUser()");
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = Constants.USER_TenId };
            var relatedUser = new UserInfo { IsSuperUser = false, UserID = Constants.USER_ValidId };
            string roles = "FOLLOWER:" + relatedUser.UserID;

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), Constants.USER_ValidId)).Returns(relatedUser);
			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFollowerRoleAndRoleEntityIsFollowedByUser(): mockUserController → {MockUserController}", mockUserController);
            UserController.SetTestableInstance(mockUserController.Object);

            var mockRelationShipController = new Mock<IRelationshipController>();
            mockRelationShipController.Setup(
                rsc =>
                    rsc.GetFollowerRelationship(It.Is<UserInfo>(u => u.UserID == Constants.USER_TenId), It.Is<UserInfo>(u => u.UserID == Constants.USER_ValidId)))
                        .Returns(new UserRelationship() { Status = RelationshipStatus.Accepted });
				Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFollowerRoleAndRoleEntityIsFollowedByUser(): mockRelationShipController → {MockRelationShipController}", mockRelationShipController);
				Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFollowerRoleAndRoleEntityIsFollowedByUser(): rsc → {Rsc}", rsc);
            RelationshipController.SetTestableInstance(mockRelationShipController.Object);

            var portalSettings = SetupPortalSettings();

			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFollowerRoleAndRoleEntityIsFollowedByUser(): portalSettings → {PortalSettings}", portalSettings);
            // Act and Assert
            Assert.That(PortalSecurity.IsInRoles(user, portalSettings, roles), Is.True);
			Logger.LogDebug("<<< Exiting PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFollowerRoleAndRoleEntityIsFollowedByUser()");
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRoleEntityIsNotFollowedByUser()
        {
			Logger.LogDebug(">>> Entering PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRoleEntityIsNotFollowedByUser()");
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = Constants.USER_TenId };
            var relatedUser = new UserInfo { IsSuperUser = false, UserID = Constants.USER_ValidId };
            string roles = "FOLLOWER:" + relatedUser.UserID;

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), Constants.USER_ValidId)).Returns(relatedUser);
			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRoleEntityIsNotFollowedByUser(): mockUserController → {MockUserController}", mockUserController);
            UserController.SetTestableInstance(mockUserController.Object);

            var mockRelationShipController = new Mock<IRelationshipController>();
            mockRelationShipController.Setup(
                rsc =>
                    rsc.GetFollowerRelationship(It.Is<UserInfo>(u => u.UserID == Constants.USER_TenId), It.Is<UserInfo>(u => u.UserID == Constants.USER_ValidId)))
                        .Returns(() => null);
				Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRoleEntityIsNotFollowedByUser(): mockRelationShipController → {MockRelationShipController}", mockRelationShipController);
				Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRoleEntityIsNotFollowedByUser(): rsc → {Rsc}", rsc);
            RelationshipController.SetTestableInstance(mockRelationShipController.Object);

            var portalSettings = SetupPortalSettings();

			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRoleEntityIsNotFollowedByUser(): portalSettings → {PortalSettings}", portalSettings);
            // Act and Assert
            Assert.That(PortalSecurity.IsInRoles(user, portalSettings, roles), Is.False);
			Logger.LogDebug("<<< Exiting PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRoleEntityIsNotFollowedByUser()");
        }

        [Test]
        [TestCase(RelationshipStatus.None)]
        [TestCase(RelationshipStatus.Pending)]
        public void PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRelationshipIsNotAccepted(RelationshipStatus relationshipStatus)
        {
			Logger.LogDebug(">>> Entering PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRelationshipIsNotAccepted(relationshipStatus={RelationshipStatus})", relationshipStatus);
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = Constants.USER_TenId };
            var relatedUser = new UserInfo { IsSuperUser = false, UserID = Constants.USER_ValidId };
            string roles = "FOLLOWER:" + relatedUser.UserID;

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), Constants.USER_ValidId)).Returns(relatedUser);
			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRelationshipIsNotAccepted(relationshipStatus={RelationshipStatus}): mockUserController → {MockUserController}", relationshipStatus, mockUserController);
            UserController.SetTestableInstance(mockUserController.Object);

            var mockRelationShipController = new Mock<IRelationshipController>();
            mockRelationShipController.Setup(
                rsc =>
                    rsc.GetFollowerRelationship(It.Is<UserInfo>(u => u.UserID == Constants.USER_TenId), It.Is<UserInfo>(u => u.UserID == Constants.USER_ValidId)))
                        .Returns(new UserRelationship() { Status = relationshipStatus });
				Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRelationshipIsNotAccepted(relationshipStatus={RelationshipStatus}): mockRelationShipController → {MockRelationShipController}", relationshipStatus, mockRelationShipController);
				Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRelationshipIsNotAccepted(relationshipStatus={RelationshipStatus}): rsc → {Rsc}", relationshipStatus, rsc);
            RelationshipController.SetTestableInstance(mockRelationShipController.Object);

            var portalSettings = SetupPortalSettings();

			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRelationshipIsNotAccepted(relationshipStatus={RelationshipStatus}): portalSettings → {PortalSettings}", relationshipStatus, portalSettings);
            // Act and Assert
            Assert.That(PortalSecurity.IsInRoles(user, portalSettings, roles), Is.False);
			Logger.LogDebug("<<< Exiting PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRelationshipIsNotAccepted(relationshipStatus={RelationshipStatus})", relationshipStatus);
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFriendRoleAndRoleEntityIsFriend()
        {
			Logger.LogDebug(">>> Entering PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFriendRoleAndRoleEntityIsFriend()");
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = Constants.USER_TenId };
            var relatedUser = new UserInfo { IsSuperUser = false, UserID = Constants.USER_ValidId };
            string roles = "FRIEND:" + relatedUser.UserID;

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), Constants.USER_ValidId)).Returns(relatedUser);
			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFriendRoleAndRoleEntityIsFriend(): mockUserController → {MockUserController}", mockUserController);
            UserController.SetTestableInstance(mockUserController.Object);

            var mockRelationShipController = new Mock<IRelationshipController>();
            mockRelationShipController.Setup(
                rsc =>
                    rsc.GetFriendRelationship(It.Is<UserInfo>(u => u.UserID == Constants.USER_TenId), It.Is<UserInfo>(u => u.UserID == Constants.USER_ValidId)))
                        .Returns(new UserRelationship() { Status = RelationshipStatus.Accepted });
				Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFriendRoleAndRoleEntityIsFriend(): mockRelationShipController → {MockRelationShipController}", mockRelationShipController);
				Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFriendRoleAndRoleEntityIsFriend(): rsc → {Rsc}", rsc);
            RelationshipController.SetTestableInstance(mockRelationShipController.Object);

            var portalSettings = SetupPortalSettings();

			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFriendRoleAndRoleEntityIsFriend(): portalSettings → {PortalSettings}", portalSettings);
            // Act and Assert
            Assert.That(PortalSecurity.IsInRoles(user, portalSettings, roles), Is.True);
			Logger.LogDebug("<<< Exiting PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFriendRoleAndRoleEntityIsFriend()");
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRoleEntityIsNotFriend()
        {
			Logger.LogDebug(">>> Entering PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRoleEntityIsNotFriend()");
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = Constants.USER_TenId };
            var relatedUser = new UserInfo { IsSuperUser = false, UserID = Constants.USER_ValidId };
            string roles = "FRIEND:" + relatedUser.UserID;

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), Constants.USER_ValidId)).Returns(relatedUser);
			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRoleEntityIsNotFriend(): mockUserController → {MockUserController}", mockUserController);
            UserController.SetTestableInstance(mockUserController.Object);

            var mockRelationShipController = new Mock<IRelationshipController>();
            mockRelationShipController.Setup(
                rsc =>
                    rsc.GetFriendRelationship(It.Is<UserInfo>(u => u.UserID == Constants.USER_TenId), It.Is<UserInfo>(u => u.UserID == Constants.USER_ValidId)))
                        .Returns(() => null);
				Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRoleEntityIsNotFriend(): mockRelationShipController → {MockRelationShipController}", mockRelationShipController);
				Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRoleEntityIsNotFriend(): rsc → {Rsc}", rsc);
            RelationshipController.SetTestableInstance(mockRelationShipController.Object);

            var portalSettings = SetupPortalSettings();

			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRoleEntityIsNotFriend(): portalSettings → {PortalSettings}", portalSettings);
            // Act and Assert
            Assert.That(PortalSecurity.IsInRoles(user, portalSettings, roles), Is.False);
			Logger.LogDebug("<<< Exiting PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRoleEntityIsNotFriend()");
        }

        [Test]
        [TestCase(RelationshipStatus.None)]
        [TestCase(RelationshipStatus.Pending)]
        public void PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRelationshipIsNotAccepted(RelationshipStatus relationshipStatus)
        {
			Logger.LogDebug(">>> Entering PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRelationshipIsNotAccepted(relationshipStatus={RelationshipStatus})", relationshipStatus);
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = Constants.USER_TenId };
            var relatedUser = new UserInfo { IsSuperUser = false, UserID = Constants.USER_ValidId };
            string roles = "FRIEND:" + relatedUser.UserID;

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), Constants.USER_ValidId)).Returns(relatedUser);
			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRelationshipIsNotAccepted(relationshipStatus={RelationshipStatus}): mockUserController → {MockUserController}", relationshipStatus, mockUserController);
            UserController.SetTestableInstance(mockUserController.Object);

            var mockRelationShipController = new Mock<IRelationshipController>();
            mockRelationShipController.Setup(
                rsc =>
                    rsc.GetFriendRelationship(It.Is<UserInfo>(u => u.UserID == Constants.USER_TenId), It.Is<UserInfo>(u => u.UserID == Constants.USER_ValidId)))
                        .Returns(new UserRelationship() { Status = relationshipStatus });
				Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRelationshipIsNotAccepted(relationshipStatus={RelationshipStatus}): mockRelationShipController → {MockRelationShipController}", relationshipStatus, mockRelationShipController);
				Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRelationshipIsNotAccepted(relationshipStatus={RelationshipStatus}): rsc → {Rsc}", relationshipStatus, rsc);
            RelationshipController.SetTestableInstance(mockRelationShipController.Object);

            var portalSettings = SetupPortalSettings();

			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRelationshipIsNotAccepted(relationshipStatus={RelationshipStatus}): portalSettings → {PortalSettings}", relationshipStatus, portalSettings);
            // Act and Assert
            Assert.That(PortalSecurity.IsInRoles(user, portalSettings, roles), Is.False);
			Logger.LogDebug("<<< Exiting PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRelationshipIsNotAccepted(relationshipStatus={RelationshipStatus})", relationshipStatus);
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsOwnerRoleAndRoleEntityIsUser()
        {
			Logger.LogDebug(">>> Entering PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsOwnerRoleAndRoleEntityIsUser()");
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = UserId };
            string roles = "OWNER:" + UserId;

            var portalSettings = SetupPortalSettings();

			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsOwnerRoleAndRoleEntityIsUser(): portalSettings → {PortalSettings}", portalSettings);
            // Act and Assert
            Assert.That(PortalSecurity.IsInRoles(user, portalSettings, roles), Is.True);
			Logger.LogDebug("<<< Exiting PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsOwnerRoleAndRoleEntityIsUser()");
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsOwnerRoleAndRoleEntityIsNotUser()
        {
			Logger.LogDebug(">>> Entering PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsOwnerRoleAndRoleEntityIsNotUser()");
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = UserId };
            string roles = "OWNER:" + UserId + 1;

            var portalSettings = SetupPortalSettings();

			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsOwnerRoleAndRoleEntityIsNotUser(): portalSettings → {PortalSettings}", portalSettings);
            // Act and Assert
            Assert.That(PortalSecurity.IsInRoles(user, portalSettings, roles), Is.False);
			Logger.LogDebug("<<< Exiting PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsOwnerRoleAndRoleEntityIsNotUser()");
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdmin_In_Deny_Role_Is_False()
        {
			Logger.LogDebug(">>> Entering PortalSecurity_IsInRoles_NonAdmin_In_Deny_Role_Is_False()");
            var user = new UserInfo { IsSuperUser = false, UserID = UserId };

            var mockRoleController = new Mock<IRoleController>();
            mockRoleController.Setup(rc => rc.GetUserRoles(It.Is<UserInfo>(u => u.UserID == UserId), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { new UserRoleInfo() { RoleName = "SomeRoleName", Status = RoleStatus.Approved } });
			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdmin_In_Deny_Role_Is_False(): mockRoleController → {MockRoleController}", mockRoleController);
            RoleController.SetTestableInstance(mockRoleController.Object);

            const string roles = "!SomeRoleName";
            var portalSettings = SetupPortalSettings();
			Logger.LogDebug("PortalSecurity_IsInRoles_NonAdmin_In_Deny_Role_Is_False(): portalSettings → {PortalSettings}", portalSettings);
            Assert.That(PortalSecurity.IsInRoles(user, portalSettings, roles), Is.False);
			Logger.LogDebug("<<< Exiting PortalSecurity_IsInRoles_NonAdmin_In_Deny_Role_Is_False()");
        }

        ////[Test]
        ////public void CorePermissionProvider_HasModuleAccess_Super_User_Is_Always_True()
        ////{
        ////   CreateUser(true, new List<string>() { "SomeRoleName" });

        ////var permissionProvider = new CorePermissionProvider();
        ////   Assert.IsTrue(permissionProvider.HasModuleAccess(SecurityAccessLevel.Anonymous, "", new ModuleInfo()));
        ////}

        ////[Test]
        ////public void CorePermissionProvider_HasModuleAccess_SecurityLevelAnonymous_Is_Always_True()
        ////{
        ////   CreateUser(false, new List<string>() { "SomeRoleName" });

        ////var permissionProvider = new CorePermissionProvider();
        ////   Assert.IsTrue(permissionProvider.HasModuleAccess(SecurityAccessLevel.Anonymous, "", new ModuleInfo()));
        ////}

        ////[Test]
        ////public void CorePermissionProvider_HasModuleAccess_SecurityLevelHost_Is_False_For_Non_SuperUser()
        ////{
        ////   CreateUser(false, new List<string>() { "SomeRoleName" });

        ////var permissionProvider = new CorePermissionProvider();
        ////   Assert.IsFalse(permissionProvider.HasModuleAccess(SecurityAccessLevel.Host, "", new ModuleInfo()));
        ////}

        ////[Test]
        ////public void CorePermissionProvider_HasModuleAccess_SecurityLevelView_Is_False_For_User_Without_View_Role()
        ////{
        ////   CreateUser(false, new List<string>() { "RoleWithoutViewPermission" });
        ////   var modulePermissionCollection = new ModulePermissionCollection();
        ////   AddModulePermission(modulePermissionCollection, "View", Convert.ToInt32(SetupPortalSettings().AdministratorRoleId));
        ////   var module = new ModuleInfo {InheritViewPermissions = false, ModulePermissions = modulePermissionCollection};

        ////var permissionProvider = new CorePermissionProvider();
        ////   Assert.IsFalse(permissionProvider.HasModuleAccess(SecurityAccessLevel.View, "", module));
        ////}

        private static PortalSettings SetupPortalSettings()
        {
			Logger.LogDebug(">>> Entering SetupPortalSettings()");
            var mockPortalController = new Mock<IPortalController>();
            var portalSettings = new PortalSettings { PortalId = 0, AdministratorId = 1 };
			Logger.LogDebug("SetupPortalSettings(): mockPortalController → {MockPortalController}", mockPortalController);
            mockPortalController.Setup(x => x.GetCurrentPortalSettings()).Returns(portalSettings);
            PortalController.SetTestableInstance(mockPortalController.Object);
			Logger.LogDebug("<<< Exiting SetupPortalSettings()");
            return portalSettings;
        }

        ////private static void AddModulePermission(IPermissionDefinitionService permissionDefinitionService, ModulePermissionCollection permissions, string key, int roleId)
        ////{
        ////    var permission = permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_MODULE_DEFINITION", key).First();
        ////    var modulePermission = new ModulePermissionInfo { PermissionID = permission.PermissionId, RoleID = roleId, AllowAccess = true };
        ////    permissions.Add(modulePermission);
        ////}

        ////private void CreateUser(bool isSuperUser, IEnumerable<string> Roles)
        ////{
        ////    var user = new UserInfo { IsSuperUser = isSuperUser, UserID = UserId };
        ////    var mockRoleProvider = MockComponentProvider.CreateRoleProvider();
        ////    var userRoles = new List<UserRoleInfo>();
        ////    foreach (var role in Roles)
        ////    {
        ////        userRoles.Add(new UserRoleInfo() { RoleName = role, Status = RoleStatus.Approved });
        ////    }

        ////    mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == UserId), It.IsAny<bool>())).Returns(userRoles);
        ////    var simulator = new Instance.Utilities.HttpSimulator.HttpSimulator(this.WebsitePhysicalAppPath);
        ////    simulator.SimulateRequest();
        ////    HttpContextBase httpContextBase = new HttpContextWrapper(HttpContext.Current);
        ////    HttpContextSource.RegisterInstance(httpContextBase);
        ////    HttpContext.Current.Items["UserInfo"] = user;
        ////}
    }
}
