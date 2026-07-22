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

    [TestFixture]
    internal class DnnModuleAuthorizeAttributeTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private MockRepository _mockRepository;
        private Mock<ActionDescriptor> _mockActionDescriptor;
        private Mock<ControllerDescriptor> _mockControllerDescriptor;
        private Mock<DnnModuleAuthorizeAttribute> _mockDnnModuleAuthorizeAttribute;

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

            this._mockDnnModuleAuthorizeAttribute = this._mockRepository.Create<DnnModuleAuthorizeAttribute>();
			Logger.LogDebug("Setup(): _mockDnnModuleAuthorizeAttribute → {_mockDnnModuleAuthorizeAttribute}", _mockDnnModuleAuthorizeAttribute);
            this._mockDnnModuleAuthorizeAttribute.CallBase = true;
			Logger.LogDebug("<<< Exiting Setup()");
        }

        [Test]
        public void AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent()
        {
			Logger.LogDebug(">>> Entering AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            this._mockDnnModuleAuthorizeAttribute.Protected().Setup("HandleUnauthorizedRequest", ItExpr.IsAny<AuthorizationContext>());
            var sut = this._mockDnnModuleAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var fakeController = new Fakes.FakeDnnController { ModuleContext = new ModuleInstanceContext() };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
			Logger.LogDebug("AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent(): fakeController → {FakeController}", fakeController);
			Logger.LogDebug("AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent(): controllerContext → {ControllerContext}", controllerContext);
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);

			Logger.LogDebug("AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent(): context → {Context}", context);
            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent()");
			Logger.LogInformation("AnonymousUser_IsNotAllowed_If_AllowAnonymousAttribute_IsNotPresent(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel()
        {
			Logger.LogDebug(">>> Entering AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var sut = this._mockDnnModuleAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var fakeController = new Fakes.FakeDnnController { ModuleContext = new ModuleInstanceContext() };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
			Logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel(): fakeController → {FakeController}", fakeController);
			Logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel(): controllerContext → {ControllerContext}", controllerContext);
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);

			Logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel(): context → {Context}", context);
            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel()");
			Logger.LogInformation("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtControllerLevel(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        }

        [Test]
        public void AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel()
        {
			Logger.LogDebug(">>> Entering AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var sut = this._mockDnnModuleAuthorizeAttribute.Object;

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(true);

            var fakeController = new Fakes.FakeDnnController { ModuleContext = new ModuleInstanceContext() };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
			Logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel(): fakeController → {FakeController}", fakeController);
			Logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel(): controllerContext → {ControllerContext}", controllerContext);
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);

			Logger.LogDebug("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel(): context → {Context}", context);
            // Act
            sut.OnAuthorization(context);

            // Assert
            // Assert.IsTrue(a.IsAuthorized);
            this._mockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel()");
			Logger.LogInformation("AnonymousUser_IsAllowed_If_AllowAnonymousAttribute_IsAtActionLevel(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        }

        [Test]
        public void RegisteredUser_IsAllowed_IfHasModuleAccess()
        {
			Logger.LogDebug(">>> Entering RegisteredUser_IsAllowed_IfHasModuleAccess()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var sut = this._mockDnnModuleAuthorizeAttribute.Object;
            this._mockDnnModuleAuthorizeAttribute.Protected().Setup<bool>("HasModuleAccess").Returns(true);
            this._mockDnnModuleAuthorizeAttribute.Protected().Setup("HandleAuthorizedRequest", ItExpr.IsAny<AuthorizationContext>());

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var moduleContext = new ModuleInstanceContext();
            moduleContext.Configuration = new ModuleInfo();
			Logger.LogDebug("RegisteredUser_IsAllowed_IfHasModuleAccess(): moduleContext → {ModuleContext}", moduleContext);
            var fakeController = new Fakes.FakeDnnController { ModuleContext = moduleContext };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);
			Logger.LogDebug("RegisteredUser_IsAllowed_IfHasModuleAccess(): controllerContext → {ControllerContext}", controllerContext);
			Logger.LogDebug("RegisteredUser_IsAllowed_IfHasModuleAccess(): context → {Context}", context);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting RegisteredUser_IsAllowed_IfHasModuleAccess()");
			Logger.LogInformation("RegisteredUser_IsAllowed_IfHasModuleAccess(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        }

        [Test]
        public void RegisteredUser_IsDenied_IfHasNoModuleAccess()
        {
			Logger.LogDebug(">>> Entering RegisteredUser_IsDenied_IfHasNoModuleAccess()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var sut = this._mockDnnModuleAuthorizeAttribute.Object;
            this._mockDnnModuleAuthorizeAttribute.Protected().Setup<bool>("HasModuleAccess").Returns(false);
            this._mockDnnModuleAuthorizeAttribute.Protected().Setup("HandleUnauthorizedRequest", ItExpr.IsAny<AuthorizationContext>());

            this._mockActionDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockControllerDescriptor.Setup(x => x.IsDefined(typeof(AllowAnonymousAttribute), true)).Returns(false);
            this._mockActionDescriptor.SetupGet(x => x.ControllerDescriptor).Returns(this._mockControllerDescriptor.Object);

            var moduleContext = new ModuleInstanceContext();
            moduleContext.Configuration = new ModuleInfo();
			Logger.LogDebug("RegisteredUser_IsDenied_IfHasNoModuleAccess(): moduleContext → {ModuleContext}", moduleContext);
            var fakeController = new Fakes.FakeDnnController { ModuleContext = moduleContext };
            var controllerContext = new ControllerContext(new RequestContext(), fakeController);
            var context = new AuthorizationContext(controllerContext, this._mockActionDescriptor.Object);
			Logger.LogDebug("RegisteredUser_IsDenied_IfHasNoModuleAccess(): controllerContext → {ControllerContext}", controllerContext);
			Logger.LogDebug("RegisteredUser_IsDenied_IfHasNoModuleAccess(): context → {Context}", context);

            // Act
            sut.OnAuthorization(context);

            // Assert
            this._mockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting RegisteredUser_IsDenied_IfHasNoModuleAccess()");
			Logger.LogInformation("RegisteredUser_IsDenied_IfHasNoModuleAccess(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        }
    }
}
