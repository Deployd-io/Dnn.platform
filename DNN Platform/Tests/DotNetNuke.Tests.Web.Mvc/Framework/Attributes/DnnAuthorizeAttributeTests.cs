// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Framework.Attributes
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Web.Mvc.Framework.ActionFilters;
    using Moq;
    using Moq.Protected;
    using NUnit.Framework;

    [TestFixture]
    internal class DnnAuthorizeAttributeTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private MockRepository _mockRepository;
        private Mock<ActionDescriptor> _mockActionDescriptor;
        private Mock<ControllerDescriptor> _mockControllerDescriptor;
        private Mock<IRoleController> _mockRoleController;
        private Mock<DnnAuthorizeAttribute> _mockDnnAuthorizeAttribute;

        [SetUp]
        public void Setup()
        {
			Logger.LogDebug(">>> Entering Setup()");
            this._mockRepository = new MockRepository(MockBehavior.Default);

			Logger.LogDebug("Setup(): _mockRepository → {_mockRepository}", _mockRepository);
            this._mockActionDescriptor = this._mockRepository.Create<ActionDescriptor>();
			Logger.LogDebug("Setup(): _mockActionDescriptor → {_mockActionDescriptor}", _mockActionDescriptor);
            this._mockControllerDescriptor = this._mockRepository.Create<ControllerDescriptor>();
			Logger.LogDebug("Setup(): _mockControllerDescriptor → {_mockControllerDescriptor}", _mockControllerDescriptor);
            this._mockRoleController = this._mockRepository.Create<IRoleController>();
			Logger.LogDebug("Setup(): _mockRoleController → {_mockRoleController}", _mockRoleController);

            this._mockDnnAuthorizeAttribute = this._mockRepository.Create<DnnAuthorizeAttribute>();
			Logger.LogDebug("Setup(): _mockDnnAuthorizeAttribute → {_mockDnnAuthorizeAttribute}", _mockDnnAuthorizeAttribute);
            this._mockDnnAuthorizeAttribute.CallBase = true;
			Logger.LogDebug("<<< Exiting Setup()");
        }

        [Test]
        public void AnonymousUser_IsNotAllowed_If_AllowAnonymousAtribute_IsNotPresent()
        {
			Logger.LogDebug(">>> Entering AnonymousUser_IsNotAllowed_If_AllowAnonymousAtribute_IsNotPresent()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            this._mockDnnAuthorizeAttribute.Protected().Setup("HandleUnauthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            var sut = this._mockDnnAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);
			Logger.LogDebug("AnonymousUser_IsNotAllowed_If_AllowAnonymousAtribute_IsNotPresent(): controllerContext → {ControllerContext}", controllerContext);
			Logger.LogDebug("AnonymousUser_IsNotAllowed_If_AllowAnonymousAtribute_IsNotPresent(): context → {Context}", context);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting AnonymousUser_IsNotAllowed_If_AllowAnonymousAtribute_IsNotPresent()");
			Logger.LogInformation("AnonymousUser_IsNotAllowed_If_AllowAnonymousAtribute_IsNotPresent(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtControllerLevel()
        {
			Logger.LogDebug(">>> Entering AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtControllerLevel()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var sut = this._mockDnnAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);
			Logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtControllerLevel(): controllerContext → {ControllerContext}", controllerContext);
			Logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtControllerLevel(): context → {Context}", context);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtControllerLevel()");
			Logger.LogInformation("AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtControllerLevel(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtActionLevel()
        {
			Logger.LogDebug(">>> Entering AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtActionLevel()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var sut = this._mockDnnAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);
			Logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtActionLevel(): controllerContext → {ControllerContext}", controllerContext);
			Logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtActionLevel(): context → {Context}", context);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtActionLevel()");
			Logger.LogInformation("AnonymousUser_IsAllowed_If_AllowAnonymousAtribute_IsAtActionLevel(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        }

        [Test]
        public void RegisteredUser_IsAllowed_ByDefault()
        {
			Logger.LogDebug(">>> Entering RegisteredUser_IsAllowed_ByDefault()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            this._mockDnnAuthorizeAttribute.Protected().Setup<bool>("IsAuthenticated").Returns(true);
            this._mockDnnAuthorizeAttribute.Protected().Setup("HandleAuthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            var sut = this._mockDnnAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);
			Logger.LogDebug("RegisteredUser_IsAllowed_ByDefault(): controllerContext → {ControllerContext}", controllerContext);
			Logger.LogDebug("RegisteredUser_IsAllowed_ByDefault(): context → {Context}", context);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting RegisteredUser_IsAllowed_ByDefault()");
			Logger.LogInformation("RegisteredUser_IsAllowed_ByDefault(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        }

        [Test]
        public void RegisteredUser_IsDenied_If_IncludedIn_DeniedRoles()
        {
			Logger.LogDebug(">>> Entering RegisteredUser_IsDenied_If_IncludedIn_DeniedRoles()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            const string roleName = "MyRole";

            var user = this.SetUpUserWithRole(roleName);

			Logger.LogDebug("RegisteredUser_IsDenied_If_IncludedIn_DeniedRoles(): user → {User}", user);
            this._mockDnnAuthorizeAttribute.Protected().Setup<bool>("IsAuthenticated").Returns(true);
            this._mockDnnAuthorizeAttribute.Protected().Setup("HandleUnauthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            this._mockDnnAuthorizeAttribute.Protected().Setup<UserInfo>("GetCurrentUser").Returns(user);
            var sut = this._mockDnnAuthorizeAttribute.Object;
            sut.DenyRoles = roleName;
			Logger.LogDebug("RegisteredUser_IsDenied_If_IncludedIn_DeniedRoles(): sut.DenyRoles → {Arg0}", sut.DenyRoles);

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);
			Logger.LogDebug("RegisteredUser_IsDenied_If_IncludedIn_DeniedRoles(): controllerContext → {ControllerContext}", controllerContext);
			Logger.LogDebug("RegisteredUser_IsDenied_If_IncludedIn_DeniedRoles(): context → {Context}", context);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting RegisteredUser_IsDenied_If_IncludedIn_DeniedRoles()");
			Logger.LogInformation("RegisteredUser_IsDenied_If_IncludedIn_DeniedRoles(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        }

        [Test]
        public void RegisteredUser_IsAllowed_If_IncludedIn_DeniedRoles_But_IsSuperUser()
        {
			Logger.LogDebug(">>> Entering RegisteredUser_IsAllowed_If_IncludedIn_DeniedRoles_But_IsSuperUser()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            const string roleName = "MyRole";

            var user = new UserInfo { IsSuperUser = true };

            this._mockDnnAuthorizeAttribute.Protected().Setup<bool>("IsAuthenticated").Returns(true);
            this._mockDnnAuthorizeAttribute.Protected().Setup("HandleAuthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            this._mockDnnAuthorizeAttribute.Protected().Setup<UserInfo>("GetCurrentUser").Returns(user);
            var sut = this._mockDnnAuthorizeAttribute.Object;
            sut.DenyRoles = roleName;
			Logger.LogDebug("RegisteredUser_IsAllowed_If_IncludedIn_DeniedRoles_But_IsSuperUser(): sut.DenyRoles → {Arg0}", sut.DenyRoles);

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);
			Logger.LogDebug("RegisteredUser_IsAllowed_If_IncludedIn_DeniedRoles_But_IsSuperUser(): controllerContext → {ControllerContext}", controllerContext);
			Logger.LogDebug("RegisteredUser_IsAllowed_If_IncludedIn_DeniedRoles_But_IsSuperUser(): context → {Context}", context);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting RegisteredUser_IsAllowed_If_IncludedIn_DeniedRoles_But_IsSuperUser()");
			Logger.LogInformation("RegisteredUser_IsAllowed_If_IncludedIn_DeniedRoles_But_IsSuperUser(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        }

        [Test]
        public void RegisteredUser_IsDenied_If_IncludedIn_StaticRoles()
        {
			Logger.LogDebug(">>> Entering RegisteredUser_IsDenied_If_IncludedIn_StaticRoles()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            const string roleName = "MyRole";

            var user = this.SetUpUserWithRole(roleName);

			Logger.LogDebug("RegisteredUser_IsDenied_If_IncludedIn_StaticRoles(): user → {User}", user);
            this._mockDnnAuthorizeAttribute.Protected().Setup<bool>("IsAuthenticated").Returns(true);
            this._mockDnnAuthorizeAttribute.Protected().Setup("HandleAuthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            this._mockDnnAuthorizeAttribute.Protected().Setup<UserInfo>("GetCurrentUser").Returns(user);
            var sut = this._mockDnnAuthorizeAttribute.Object;
            sut.StaticRoles = roleName;
			Logger.LogDebug("RegisteredUser_IsDenied_If_IncludedIn_StaticRoles(): sut.StaticRoles → {Arg0}", sut.StaticRoles);

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var controllerContext = new ControllerContext();
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);
			Logger.LogDebug("RegisteredUser_IsDenied_If_IncludedIn_StaticRoles(): controllerContext → {ControllerContext}", controllerContext);
			Logger.LogDebug("RegisteredUser_IsDenied_If_IncludedIn_StaticRoles(): context → {Context}", context);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting RegisteredUser_IsDenied_If_IncludedIn_StaticRoles()");
			Logger.LogInformation("RegisteredUser_IsDenied_If_IncludedIn_StaticRoles(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        }

        private UserInfo SetUpUserWithRole(string roleName)
        {
			Logger.LogDebug(">>> Entering SetUpUserWithRole(roleName={RoleName})", roleName);
            var user = new UserInfo
            {
                UserID = 1,
                PortalID = 1,
            };

            var roles = new List<UserRoleInfo>
            {
                new UserRoleInfo
                {
                    RoleName = roleName,
                    Status = RoleStatus.Approved,
                },
            };

            this._mockRoleController.Setup(x => x.GetUserRoles(user, true)).Returns(roles);
            RoleController.SetTestableInstance(this._mockRoleController.Object);

			Logger.LogDebug("<<< Exiting SetUpUserWithRole(roleName={RoleName})", roleName);
            return user;
        }
    }
}
