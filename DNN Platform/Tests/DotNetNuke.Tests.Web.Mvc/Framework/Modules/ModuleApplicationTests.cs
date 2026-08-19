// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.Mvc.Framework.Modules
{
    using System;

    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using DotNetNuke.Web.Mvc.Framework.Modules;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class ModuleApplicationTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const string ActionName = "Action";
        private const string ControllerName = "Controller";

        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
			_logger.LogDebug(">>> Entering Setup()");
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					_logger.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton<IControllerFactory, DefaultControllerFactory>();
                });
				_logger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			_logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			_logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void Init_Is_Called_In_First_ExecuteRequest_And_Not_In_Subsequent_Requests()
        {
			_logger.LogDebug(">>> Entering Init_Is_Called_In_First_ExecuteRequest_And_Not_In_Subsequent_Requests()");
            // Arrange
            var app = new Mock<ModuleApplication> { CallBase = true };

            int initCounter = 0;
            app.Setup(a => a.Init())
                .Callback(() => initCounter++);

            var controller = new Mock<IDnnController>();
			_logger.LogDebug("Init_Is_Called_In_First_ExecuteRequest_And_Not_In_Subsequent_Requests(): controller → {Controller}", controller);

            var controllerFactory = SetupControllerFactory(controller.Object);
			_logger.LogDebug("Init_Is_Called_In_First_ExecuteRequest_And_Not_In_Subsequent_Requests(): controllerFactory → {ControllerFactory}", controllerFactory);
            app.SetupGet(a => a.ControllerFactory).Returns(controllerFactory.Object);

            // Act
            app.Object.ExecuteRequest(CreateModuleContext(ControllerName, ActionName));
            app.Object.ExecuteRequest(CreateModuleContext(ControllerName, ActionName));

            // Assert
            Assert.That(initCounter, Is.EqualTo(1));
			_logger.LogDebug("<<< Exiting Init_Is_Called_In_First_ExecuteRequest_And_Not_In_Subsequent_Requests()");
        }

        [Test]
        public void ExecuteRequest_Calls_ControllerFactory_To_Construct_Controller()
        {
			_logger.LogDebug(">>> Entering ExecuteRequest_Calls_ControllerFactory_To_Construct_Controller()");
            // Arrange
            var app = new ModuleApplication();
			_logger.LogDebug("ExecuteRequest_Calls_ControllerFactory_To_Construct_Controller(): app → {App}", app);

            var controllerFactory = new Mock<IControllerFactory>();
			_logger.LogDebug("ExecuteRequest_Calls_ControllerFactory_To_Construct_Controller(): controllerFactory → {ControllerFactory}", controllerFactory);
            RequestContext actualRequestContext = null;
            controllerFactory.Setup(f => f.CreateController(It.IsAny<RequestContext>(), ControllerName))
                                 .Callback<RequestContext, string>((c, n) => actualRequestContext = c)
                                 .Returns(new Mock<IDnnController>().Object);

            app.ControllerFactory = controllerFactory.Object;
			_logger.LogDebug("ExecuteRequest_Calls_ControllerFactory_To_Construct_Controller(): app.ControllerFactory → {Arg0}", app.ControllerFactory);

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);
			_logger.LogDebug("ExecuteRequest_Calls_ControllerFactory_To_Construct_Controller(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);

            // Act
            app.ExecuteRequest(moduleRequestContext);

            // Assert
            controllerFactory.Verify(f => f.CreateController(It.IsAny<RequestContext>(), ControllerName));
            Assert.That(actualRequestContext.HttpContext, Is.SameAs(moduleRequestContext.HttpContext));
			_logger.LogDebug("<<< Exiting ExecuteRequest_Calls_ControllerFactory_To_Construct_Controller()");
        }

        [Test]
        public void ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController()
        {
			_logger.LogDebug(">>> Entering ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController()");
            // Arrange
            var app = new ModuleApplication();
			_logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController(): app → {App}", app);

            var controller = new Mock<IController>();
			_logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController(): controller → {Controller}", controller);
			_logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController(): app.ControllerFactory → {Arg0}", app.ControllerFactory);
            var controllerFactory = SetupControllerFactory(controller.Object);
			_logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController(): controllerFactory → {ControllerFactory}", controllerFactory);
            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);
			_logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => app.ExecuteRequest(moduleRequestContext));
			_logger.LogDebug("<<< Exiting ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController()");
        }

        [Test]
        public void ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker()
        {
			_logger.LogDebug(">>> Entering ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker()");
            // Arrange
            var app = new ModuleApplication();
			_logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker(): app → {App}", app);

            var controller = new Mock<Controller>();
			_logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker(): controller → {Controller}", controller);
            var invoker = new Mock<IActionInvoker>();
			_logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker(): invoker → {Invoker}", invoker);
            controller.Object.ActionInvoker = invoker.Object;

            var controllerFactory = SetupControllerFactory(controller.Object);
			_logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker(): controllerFactory → {ControllerFactory}", controllerFactory);
			_logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker(): app.ControllerFactory → {Arg0}", app.ControllerFactory);
            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);
			_logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => app.ExecuteRequest(moduleRequestContext));
			_logger.LogDebug("<<< Exiting ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker()");
        }

        [Test]
        public void ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController()
        {
			_logger.LogDebug(">>> Entering ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController()");
            // Arrange
            var app = new ModuleApplication();
			_logger.LogDebug("ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController(): app → {App}", app);

            var controller = new Mock<IController>();
			_logger.LogDebug("ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController(): controller → {Controller}", controller);
            controller.As<IDnnController>();

            var controllerFactory = SetupControllerFactory(controller.Object);
			_logger.LogDebug("ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController(): controllerFactory → {ControllerFactory}", controllerFactory);
			_logger.LogDebug("ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController(): app.ControllerFactory → {Arg0}", app.ControllerFactory);
            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);
			_logger.LogDebug("ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);

            // Act and Assert
            app.ExecuteRequest(moduleRequestContext);
			_logger.LogDebug("<<< Exiting ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController()");
        }

        [Test]
        public void ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller()
        {
			_logger.LogDebug(">>> Entering ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller()");
            // Arrange
            var app = new ModuleApplication();
			_logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): app → {App}", app);

            ControllerContext controllerContext = MockHelper.CreateMockControllerContext();
			_logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): controllerContext → {ControllerContext}", controllerContext);
            ActionResult actionResult = new Mock<ActionResult>().Object;
			_logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): actionResult → {ActionResult}", actionResult);

            var controller = SetupMockController(actionResult, controllerContext);
			_logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): controller → {Controller}", controller);

            var controllerFactory = SetupControllerFactory(controller.Object);
			_logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): controllerFactory → {ControllerFactory}", controllerFactory);
			_logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): app.ControllerFactory → {Arg0}", app.ControllerFactory);
            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);
			_logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);

            // Act
            ModuleRequestResult result = app.ExecuteRequest(moduleRequestContext);
			_logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.ActionResult, Is.SameAs(actionResult));
                Assert.That(result.ControllerContext, Is.SameAs(controllerContext));
            }
			_logger.LogDebug("<<< Exiting ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller()");
        }

        [Test]
        public void ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext()
        {
			_logger.LogDebug(">>> Entering ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext()");
            // Arrange
            var app = new ModuleApplication();
			_logger.LogDebug("ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext(): app → {App}", app);

            var controller = new Mock<IController>();
			_logger.LogDebug("ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext(): controller → {Controller}", controller);
            controller.As<IDnnController>();

            var controllerFactory = SetupControllerFactory(controller.Object);
			_logger.LogDebug("ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext(): controllerFactory → {ControllerFactory}", controllerFactory);
			_logger.LogDebug("ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext(): app.ControllerFactory → {Arg0}", app.ControllerFactory);
            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);
			_logger.LogDebug("ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);

            // Act
            ModuleRequestResult result = app.ExecuteRequest(moduleRequestContext);
			_logger.LogDebug("ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext(): result → {Result}", result);

            // Assert
            controller.Verify(c => c.Execute(It.Is<RequestContext>(rc =>
                    rc.HttpContext == moduleRequestContext.HttpContext &&
                    rc.RouteData.GetRequiredString("controller") == ControllerName)));
					_logger.LogDebug("<<< Exiting ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext()");
        }

        [Test]
        public void ExecuteRequest_ReleasesController_After_Executing()
        {
			_logger.LogDebug(">>> Entering ExecuteRequest_ReleasesController_After_Executing()");
            // Arrange
            var app = new ModuleApplication();
			_logger.LogDebug("ExecuteRequest_ReleasesController_After_Executing(): app → {App}", app);

            var controller = new Mock<IDnnController>();
			_logger.LogDebug("ExecuteRequest_ReleasesController_After_Executing(): controller → {Controller}", controller);

            var controllerFactory = SetupControllerFactory(controller.Object);
			_logger.LogDebug("ExecuteRequest_ReleasesController_After_Executing(): controllerFactory → {ControllerFactory}", controllerFactory);
			_logger.LogDebug("ExecuteRequest_ReleasesController_After_Executing(): app.ControllerFactory → {Arg0}", app.ControllerFactory);
            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);
			_logger.LogDebug("ExecuteRequest_ReleasesController_After_Executing(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);

            // Act
            ModuleRequestResult result = app.ExecuteRequest(moduleRequestContext);
			_logger.LogDebug("ExecuteRequest_ReleasesController_After_Executing(): result → {Result}", result);

            // Assert
            controllerFactory.Verify(cf => cf.ReleaseController(controller.Object));
			_logger.LogDebug("<<< Exiting ExecuteRequest_ReleasesController_After_Executing()");
        }

        [Test]
        public void ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception()
        {
			_logger.LogDebug(">>> Entering ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception()");
            // Arrange
            var app = new ModuleApplication();
			_logger.LogDebug("ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception(): app → {App}", app);

            var controller = new Mock<IDnnController>();
			_logger.LogDebug("ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception(): controller → {Controller}", controller);
            controller.Setup(c => c.Execute(It.IsAny<RequestContext>()))
                .Throws(new Exception("Uh Oh!"));

            var controllerFactory = SetupControllerFactory(controller.Object);
			_logger.LogDebug("ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception(): controllerFactory → {ControllerFactory}", controllerFactory);
			_logger.LogDebug("ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception(): app.ControllerFactory → {Arg0}", app.ControllerFactory);
            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);
			_logger.LogDebug("ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);

            // Act (and verify the exception is thrown; also supresses the exception so it doesn't fail the test)
            Assert.Throws<Exception>(() => app.ExecuteRequest(moduleRequestContext));

            // Assert
            controllerFactory.Verify(f => f.ReleaseController(controller.Object));
			_logger.LogDebug("<<< Exiting ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception()");
        }

        private static Mock<IDnnController> SetupMockController(ActionResult actionResult, ControllerContext controllerContext)
        {
			_logger.LogDebug(">>> Entering SetupMockController(actionResult={ActionResult},controllerContext={ControllerContext})", actionResult, controllerContext);
            var mockController = new Mock<IDnnController>();
            mockController.Setup(c => c.ResultOfLastExecute)
                .Returns(actionResult);
				_logger.LogDebug("SetupMockController(actionResult={ActionResult},controllerContext={ControllerContext}): mockController → {MockController}", actionResult, controllerContext, mockController);
            mockController.Setup(c => c.ControllerContext)
                .Returns(controllerContext);
			_logger.LogDebug("<<< Exiting SetupMockController(actionResult={ActionResult},controllerContext={ControllerContext})", actionResult, controllerContext);
            return mockController;
        }

        private static Mock<IControllerFactory> SetupControllerFactory(IController controller)
        {
			_logger.LogDebug(">>> Entering SetupControllerFactory(controller={Controller})", controller);
            var controllerFactory = new Mock<IControllerFactory>();
            controllerFactory.Setup(cf => cf.CreateController(It.IsAny<RequestContext>(), It.IsAny<string>()))
                            .Returns(controller);
							_logger.LogDebug("SetupControllerFactory(controller={Controller}): controllerFactory → {ControllerFactory}", controller, controllerFactory);
			_logger.LogDebug("<<< Exiting SetupControllerFactory(controller={Controller})", controller);
            return controllerFactory;
        }

        private static ModuleRequestContext CreateModuleContext(string controllerName, string actionName)
        {
			_logger.LogDebug(">>> Entering CreateModuleContext(controllerName={ControllerName},actionName={ActionName})", controllerName, actionName);
            var routeData = new RouteData();
            routeData.Values.Add("controller", controllerName);
			_logger.LogDebug("CreateModuleContext(controllerName={ControllerName},actionName={ActionName}): routeData → {RouteData}", controllerName, actionName, routeData);
            routeData.Values.Add("action", actionName);

            var moduleContext = new ModuleInstanceContext { Configuration = new ModuleInfo { ModuleID = 42 } };
			_logger.LogDebug("<<< Exiting CreateModuleContext(controllerName={ControllerName},actionName={ActionName})", controllerName, actionName);
            return new ModuleRequestContext
            {
                HttpContext = MockHelper.CreateMockHttpContext("http://localhost/Portal/Page/ModuleRoute"),
                RouteData = routeData,
                ModuleContext = moduleContext,
            };
        }
    }
}
