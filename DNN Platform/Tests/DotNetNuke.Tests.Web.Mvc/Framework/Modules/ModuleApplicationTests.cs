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

    [TestFixture]
    public class ModuleApplicationTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string ActionName = "Action";
        private const string ControllerName = "Controller";

        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
			Logger.LogDebug(">>> Entering Setup()");
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("Setup(): services → {Services}", services);
				Logger.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton<IControllerFactory, DefaultControllerFactory>();
                });
				Logger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void Init_Is_Called_In_First_ExecuteRequest_And_Not_In_Subsequent_Requests()
        {
			Logger.LogDebug(">>> Entering Init_Is_Called_In_First_ExecuteRequest_And_Not_In_Subsequent_Requests()");
            // Arrange
            var app = new Mock<ModuleApplication> { CallBase = true };

            int initCounter = 0;
            app.Setup(a => a.Init())
                .Callback(() => initCounter++);

            var controller = new Mock<IDnnController>();

			Logger.LogDebug("Init_Is_Called_In_First_ExecuteRequest_And_Not_In_Subsequent_Requests(): controller → {Controller}", controller);
            var controllerFactory = SetupControllerFactory(controller.Object);
            app.SetupGet(a => a.ControllerFactory).Returns(controllerFactory.Object);
			Logger.LogDebug("Init_Is_Called_In_First_ExecuteRequest_And_Not_In_Subsequent_Requests(): controllerFactory → {ControllerFactory}", controllerFactory);

            // Act
            app.Object.ExecuteRequest(CreateModuleContext(ControllerName, ActionName));
            app.Object.ExecuteRequest(CreateModuleContext(ControllerName, ActionName));

            // Assert
            Assert.That(initCounter, Is.EqualTo(1));
			Logger.LogDebug("<<< Exiting Init_Is_Called_In_First_ExecuteRequest_And_Not_In_Subsequent_Requests()");
        }

        [Test]
        public void ExecuteRequest_Calls_ControllerFactory_To_Construct_Controller()
        {
			Logger.LogDebug(">>> Entering ExecuteRequest_Calls_ControllerFactory_To_Construct_Controller()");
            // Arrange
            var app = new ModuleApplication();

			Logger.LogDebug("ExecuteRequest_Calls_ControllerFactory_To_Construct_Controller(): app → {App}", app);
            var controllerFactory = new Mock<IControllerFactory>();
            RequestContext actualRequestContext = null;
			Logger.LogDebug("ExecuteRequest_Calls_ControllerFactory_To_Construct_Controller(): controllerFactory → {ControllerFactory}", controllerFactory);
            controllerFactory.Setup(f => f.CreateController(It.IsAny<RequestContext>(), ControllerName))
                                 .Callback<RequestContext, string>((c, n) => actualRequestContext = c)
                                 .Returns(new Mock<IDnnController>().Object);

            app.ControllerFactory = controllerFactory.Object;
			Logger.LogDebug("ExecuteRequest_Calls_ControllerFactory_To_Construct_Controller(): app.ControllerFactory → {Arg0}", app.ControllerFactory);

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

			Logger.LogDebug("ExecuteRequest_Calls_ControllerFactory_To_Construct_Controller(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);
            // Act
            app.ExecuteRequest(moduleRequestContext);

            // Assert
            controllerFactory.Verify(f => f.CreateController(It.IsAny<RequestContext>(), ControllerName));
            Assert.That(actualRequestContext.HttpContext, Is.SameAs(moduleRequestContext.HttpContext));
			Logger.LogDebug("<<< Exiting ExecuteRequest_Calls_ControllerFactory_To_Construct_Controller()");
        }

        [Test]
        public void ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController()
        {
			Logger.LogDebug(">>> Entering ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController()");
            // Arrange
            var app = new ModuleApplication();

			Logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController(): app → {App}", app);
            var controller = new Mock<IController>();
            var controllerFactory = SetupControllerFactory(controller.Object);
			Logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController(): controller → {Controller}", controller);
			Logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController(): controllerFactory → {ControllerFactory}", controllerFactory);
			Logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController(): app.ControllerFactory → {Arg0}", app.ControllerFactory);
            app.ControllerFactory = controllerFactory.Object;

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

			Logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);
            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => app.ExecuteRequest(moduleRequestContext));
			Logger.LogDebug("<<< Exiting ExecuteRequest_Throws_InvalidOperationException_If_Controller_Only_Implements_IController()");
        }

        [Test]
        public void ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker()
        {
			Logger.LogDebug(">>> Entering ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker()");
            // Arrange
            var app = new ModuleApplication();

			Logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker(): app → {App}", app);
            var controller = new Mock<Controller>();
            var invoker = new Mock<IActionInvoker>();
			Logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker(): controller → {Controller}", controller);
			Logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker(): invoker → {Invoker}", invoker);
            controller.Object.ActionInvoker = invoker.Object;

            var controllerFactory = SetupControllerFactory(controller.Object);
            app.ControllerFactory = controllerFactory.Object;
			Logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker(): controllerFactory → {ControllerFactory}", controllerFactory);
			Logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker(): app.ControllerFactory → {Arg0}", app.ControllerFactory);

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

			Logger.LogDebug("ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);
            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => app.ExecuteRequest(moduleRequestContext));
			Logger.LogDebug("<<< Exiting ExecuteRequest_Throws_InvalidOperationException_If_Controller_Has_NonStandard_Action_Invoker()");
        }

        [Test]
        public void ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController()
        {
			Logger.LogDebug(">>> Entering ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController()");
            // Arrange
            var app = new ModuleApplication();

			Logger.LogDebug("ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController(): app → {App}", app);
            var controller = new Mock<IController>();
            controller.As<IDnnController>();
			Logger.LogDebug("ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController(): controller → {Controller}", controller);

            var controllerFactory = SetupControllerFactory(controller.Object);
            app.ControllerFactory = controllerFactory.Object;
			Logger.LogDebug("ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController(): controllerFactory → {ControllerFactory}", controllerFactory);
			Logger.LogDebug("ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController(): app.ControllerFactory → {Arg0}", app.ControllerFactory);

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

			Logger.LogDebug("ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);
            // Act and Assert
            app.ExecuteRequest(moduleRequestContext);
			Logger.LogDebug("<<< Exiting ExecuteRequest_Does_Not_Throw_If_Controller_Implements_IDnnController()");
        }

        [Test]
        public void ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller()
        {
			Logger.LogDebug(">>> Entering ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller()");
            // Arrange
            var app = new ModuleApplication();

			Logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): app → {App}", app);
            ControllerContext controllerContext = MockHelper.CreateMockControllerContext();
            ActionResult actionResult = new Mock<ActionResult>().Object;
			Logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): controllerContext → {ControllerContext}", controllerContext);
			Logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): actionResult → {ActionResult}", actionResult);

            var controller = SetupMockController(actionResult, controllerContext);

			Logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): controller → {Controller}", controller);
            var controllerFactory = SetupControllerFactory(controller.Object);
            app.ControllerFactory = controllerFactory.Object;
			Logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): controllerFactory → {ControllerFactory}", controllerFactory);
			Logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): app.ControllerFactory → {Arg0}", app.ControllerFactory);

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

			Logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);
            // Act
            ModuleRequestResult result = app.ExecuteRequest(moduleRequestContext);

			Logger.LogDebug("ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.ActionResult, Is.SameAs(actionResult));
                Assert.That(result.ControllerContext, Is.SameAs(controllerContext));
            }
			Logger.LogDebug("<<< Exiting ExecuteRequest_Returns_Result_And_ControllerContext_From_Controller()");
        }

        [Test]
        public void ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext()
        {
			Logger.LogDebug(">>> Entering ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext()");
            // Arrange
            var app = new ModuleApplication();

			Logger.LogDebug("ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext(): app → {App}", app);
            var controller = new Mock<IController>();
            controller.As<IDnnController>();
			Logger.LogDebug("ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext(): controller → {Controller}", controller);

            var controllerFactory = SetupControllerFactory(controller.Object);
            app.ControllerFactory = controllerFactory.Object;
			Logger.LogDebug("ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext(): controllerFactory → {ControllerFactory}", controllerFactory);
			Logger.LogDebug("ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext(): app.ControllerFactory → {Arg0}", app.ControllerFactory);

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

			Logger.LogDebug("ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);
            // Act
            ModuleRequestResult result = app.ExecuteRequest(moduleRequestContext);

			Logger.LogDebug("ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext(): result → {Result}", result);
            // Assert
            controller.Verify(c => c.Execute(It.Is<RequestContext>(rc =>
                    rc.HttpContext == moduleRequestContext.HttpContext &&
                    rc.RouteData.GetRequiredString("controller") == ControllerName)));
					Logger.LogDebug("<<< Exiting ExecuteRequest_Executes_Constructed_Controller_And_Provides_RequestContext()");
        }

        [Test]
        public void ExecuteRequest_ReleasesController_After_Executing()
        {
			Logger.LogDebug(">>> Entering ExecuteRequest_ReleasesController_After_Executing()");
            // Arrange
            var app = new ModuleApplication();

			Logger.LogDebug("ExecuteRequest_ReleasesController_After_Executing(): app → {App}", app);
            var controller = new Mock<IDnnController>();

			Logger.LogDebug("ExecuteRequest_ReleasesController_After_Executing(): controller → {Controller}", controller);
            var controllerFactory = SetupControllerFactory(controller.Object);
            app.ControllerFactory = controllerFactory.Object;
			Logger.LogDebug("ExecuteRequest_ReleasesController_After_Executing(): controllerFactory → {ControllerFactory}", controllerFactory);
			Logger.LogDebug("ExecuteRequest_ReleasesController_After_Executing(): app.ControllerFactory → {Arg0}", app.ControllerFactory);

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

			Logger.LogDebug("ExecuteRequest_ReleasesController_After_Executing(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);
            // Act
            ModuleRequestResult result = app.ExecuteRequest(moduleRequestContext);

			Logger.LogDebug("ExecuteRequest_ReleasesController_After_Executing(): result → {Result}", result);
            // Assert
            controllerFactory.Verify(cf => cf.ReleaseController(controller.Object));
			Logger.LogDebug("<<< Exiting ExecuteRequest_ReleasesController_After_Executing()");
        }

        [Test]
        public void ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception()
        {
			Logger.LogDebug(">>> Entering ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception()");
            // Arrange
            var app = new ModuleApplication();

			Logger.LogDebug("ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception(): app → {App}", app);
            var controller = new Mock<IDnnController>();
            controller.Setup(c => c.Execute(It.IsAny<RequestContext>()))
                .Throws(new Exception("Uh Oh!"));
				Logger.LogDebug("ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception(): controller → {Controller}", controller);

            var controllerFactory = SetupControllerFactory(controller.Object);
            app.ControllerFactory = controllerFactory.Object;
			Logger.LogDebug("ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception(): controllerFactory → {ControllerFactory}", controllerFactory);
			Logger.LogDebug("ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception(): app.ControllerFactory → {Arg0}", app.ControllerFactory);

            ModuleRequestContext moduleRequestContext = CreateModuleContext(ControllerName, ActionName);

			Logger.LogDebug("ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception(): moduleRequestContext → {ModuleRequestContext}", moduleRequestContext);
            // Act (and verify the exception is thrown; also supresses the exception so it doesn't fail the test)
            Assert.Throws<Exception>(() => app.ExecuteRequest(moduleRequestContext));

            // Assert
            controllerFactory.Verify(f => f.ReleaseController(controller.Object));
			Logger.LogDebug("<<< Exiting ExecuteRequest_ReleasesController_Even_If_It_Throws_An_Exception()");
        }

        private static Mock<IDnnController> SetupMockController(ActionResult actionResult, ControllerContext controllerContext)
        {
            var mockController = new Mock<IDnnController>();
            mockController.Setup(c => c.ResultOfLastExecute)
                .Returns(actionResult);
            mockController.Setup(c => c.ControllerContext)
                .Returns(controllerContext);
            return mockController;
        }

        private static Mock<IControllerFactory> SetupControllerFactory(IController controller)
        {
            var controllerFactory = new Mock<IControllerFactory>();
            controllerFactory.Setup(cf => cf.CreateController(It.IsAny<RequestContext>(), It.IsAny<string>()))
                            .Returns(controller);
            return controllerFactory;
        }

        private static ModuleRequestContext CreateModuleContext(string controllerName, string actionName)
        {
			Logger.LogDebug(">>> Entering CreateModuleContext(controllerName={ControllerName},actionName={ActionName})", controllerName, actionName);
            var routeData = new RouteData();
            routeData.Values.Add("controller", controllerName);
			Logger.LogDebug("CreateModuleContext(controllerName={ControllerName},actionName={ActionName}): routeData → {RouteData}", controllerName, actionName, routeData);
            routeData.Values.Add("action", actionName);

            var moduleContext = new ModuleInstanceContext { Configuration = new ModuleInfo { ModuleID = 42 } };
			Logger.LogDebug("<<< Exiting CreateModuleContext(controllerName={ControllerName},actionName={ActionName})", controllerName, actionName);
            return new ModuleRequestContext
            {
                HttpContext = MockHelper.CreateMockHttpContext("http://localhost/Portal/Page/ModuleRoute"),
                RouteData = routeData,
                ModuleContext = moduleContext,
            };
        }
    }
}
