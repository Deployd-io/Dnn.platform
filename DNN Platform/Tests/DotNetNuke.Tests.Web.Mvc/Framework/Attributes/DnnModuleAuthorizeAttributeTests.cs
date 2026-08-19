// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Framework.Attributes
{
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Framework.ActionFilters;
    using Moq;
    using Moq.Protected;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    internal class DnnModuleAuthorizeAttributeTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private MockRepository _mockRepository;
        private Mock<ActionDescriptor> _mockActionDescriptor;
        private Mock<ControllerDescriptor> _mockControllerDescriptor;
        private Mock<DnnModuleAuthorizeAttribute> _mockDnnModuleAuthorizeAttribute;

        [SetUp]
        public void Setup()
        {
			_logger.LogDebug(">>> Entering Setup()");
            this._mockRepository = new MockRepository(MockBehavior.Default);

			_logger.LogDebug("Setup(): _mockRepository → {_mockRepository}", _mockRepository);
            this._mockActionDescriptor = this._mockRepository.Create<ActionDescriptor>();
			_logger.LogDebug("Setup(): _mockActionDescriptor → {_mockActionDescriptor}", _mockActionDescriptor);
            this._mockControllerDescriptor = this._mockRepository.Create<ControllerDescriptor>();
			_logger.LogDebug("Setup(): _mockControllerDescriptor → {_mockControllerDescriptor}", _mockControllerDescriptor);

            this._mockDnnModuleAuthorizeAttribute = this._mockRepository.Create<DnnModuleAuthorizeAttribute>();
			_logger.LogDebug("Setup(): _mockDnnModuleAuthorizeAttribute → {_mockDnnModuleAuthorizeAttribute}", _mockDnnModuleAuthorizeAttribute);
            this._mockDnnModuleAuthorizeAttribute.CallBase = true;
			_logger.LogDebug("<<< Exiting Setup()");
        }

        [Test]
        public void AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent()
        {
			_logger.LogDebug(">>> Entering AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent()");
            // Arrange
            this._mockDnnModuleAuthorizeAttribute.Protected().Setup("HandleUnauthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            var sut = this._mockDnnModuleAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var fakeController = new Fakes.FakeDnnController { ModuleContext = new ModuleInstanceContext() };
			_logger.LogDebug("AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent(): fakeController → {FakeController}", fakeController);
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
			_logger.LogDebug("AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent(): controllerContext → {ControllerContext}", controllerContext);
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);
			_logger.LogDebug("AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent(): context → {Context}", context);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
			_logger.LogInformation("AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
			_logger.LogDebug("<<< Exiting AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent()");
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel()
        {
			_logger.LogDebug(">>> Entering AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel()");
            // Arrange
            var sut = this._mockDnnModuleAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var fakeController = new Fakes.FakeDnnController { ModuleContext = new ModuleInstanceContext() };
			_logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel(): fakeController → {FakeController}", fakeController);
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
			_logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel(): controllerContext → {ControllerContext}", controllerContext);
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);
			_logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel(): context → {Context}", context);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			_logger.LogInformation("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
			_logger.LogDebug("<<< Exiting AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel()");
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel()
        {
			_logger.LogDebug(">>> Entering AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel()");
            // Arrange
            var sut = this._mockDnnModuleAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);

            var fakeController = new Fakes.FakeDnnController { ModuleContext = new ModuleInstanceContext() };
			_logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel(): fakeController → {FakeController}", fakeController);
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
			_logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel(): controllerContext → {ControllerContext}", controllerContext);
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);
			_logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel(): context → {Context}", context);

            // Act
            sut.OnAuthorization(context);

            // Assert
            // Assert.IsTrue(a.IsAuthorized);
            this._mockRepository.VerifyAll();
			_logger.LogInformation("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
			_logger.LogDebug("<<< Exiting AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel()");
        }

        [Test]
        public void RegisteredUser_IsAllowed_IfHasModuleAccess()
        {
			_logger.LogDebug(">>> Entering RegisteredUser_IsAllowed_IfHasModuleAccess()");
            // Arrange
            var sut = this._mockDnnModuleAuthorizeAttribute.Object;
            this._mockDnnModuleAuthorizeAttribute.Protected().Setup<bool>("HasModuleAccess").Returns(true);
            this._mockDnnModuleAuthorizeAttribute.Protected().Setup("HandleAuthorizedRequest", ItExpr.IsAny<AuthorizationContext>());

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var moduleContext = new ModuleInstanceContext();
			_logger.LogDebug("RegisteredUser_IsAllowed_IfHasModuleAccess(): moduleContext → {ModuleContext}", moduleContext);
            moduleContext.Configuration = new ModuleInfo();
            var fakeController = new Fakes.FakeDnnController { ModuleContext = moduleContext };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
			_logger.LogDebug("RegisteredUser_IsAllowed_IfHasModuleAccess(): controllerContext → {ControllerContext}", controllerContext);
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);
			_logger.LogDebug("RegisteredUser_IsAllowed_IfHasModuleAccess(): context → {Context}", context);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			_logger.LogInformation("RegisteredUser_IsAllowed_IfHasModuleAccess(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
			_logger.LogDebug("<<< Exiting RegisteredUser_IsAllowed_IfHasModuleAccess()");
        }

        [Test]
        public void RegisteredUser_IsDenied_IfHasNoModuleAccess()
        {
			_logger.LogDebug(">>> Entering RegisteredUser_IsDenied_IfHasNoModuleAccess()");
            // Arrange
            var sut = this._mockDnnModuleAuthorizeAttribute.Object;
            this._mockDnnModuleAuthorizeAttribute.Protected().Setup<bool>("HasModuleAccess").Returns(false);
            this._mockDnnModuleAuthorizeAttribute.Protected().Setup("HandleUnauthorizedRequest", ItExpr.IsAny<AuthorizationContext>());

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var moduleContext = new ModuleInstanceContext();
			_logger.LogDebug("RegisteredUser_IsDenied_IfHasNoModuleAccess(): moduleContext → {ModuleContext}", moduleContext);
            moduleContext.Configuration = new ModuleInfo();
            var fakeController = new Fakes.FakeDnnController { ModuleContext = moduleContext };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
			_logger.LogDebug("RegisteredUser_IsDenied_IfHasNoModuleAccess(): controllerContext → {ControllerContext}", controllerContext);
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);
			_logger.LogDebug("RegisteredUser_IsDenied_IfHasNoModuleAccess(): context → {Context}", context);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
			_logger.LogInformation("RegisteredUser_IsDenied_IfHasNoModuleAccess(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
			_logger.LogDebug("<<< Exiting RegisteredUser_IsDenied_IfHasNoModuleAccess()");
        }
    }
}
