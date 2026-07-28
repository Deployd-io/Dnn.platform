// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.Mvc.Framework.Modules
{
    using System;
    using System.IO;
    using System.Web.Mvc;

    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Web.Mvc.Framework.ActionResults;
    using DotNetNuke.Web.Mvc.Framework.Modules;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ModuleExecutionEngineTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
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
        public void ExecuteModule_Requires_NonNull_ModuleRequestContext()
        {
			Logger.LogDebug(">>> Entering ExecuteModule_Requires_NonNull_ModuleRequestContext()");
            var engine = new ModuleExecutionEngine();
            Assert.Throws<ArgumentNullException>(() => engine.ExecuteModule(null));
			Logger.LogDebug("<<< Exiting ExecuteModule_Requires_NonNull_ModuleRequestContext()");
			Logger.LogDebug("ExecuteModule_Requires_NonNull_ModuleRequestContext(): engine → {Engine}", engine);
        }

        [Test]
        public void ExecuteModule_Returns_Null_If_Application_Is_Null()
        {
			Logger.LogDebug(">>> Entering ExecuteModule_Returns_Null_If_Application_Is_Null()");
            // Arrange
            var engine = new ModuleExecutionEngine();
            var requestContext = new ModuleRequestContext() { ModuleApplication = null };
			Logger.LogDebug("ExecuteModule_Returns_Null_If_Application_Is_Null(): engine → {Engine}", engine);
			Logger.LogDebug("ExecuteModule_Returns_Null_If_Application_Is_Null(): requestContext → {RequestContext}", requestContext);

            // Act
            ModuleRequestResult result = engine.ExecuteModule(requestContext);

			Logger.LogDebug("ExecuteModule_Returns_Null_If_Application_Is_Null(): result → {Result}", result);
            // Assert
            Assert.That(result, Is.Null);
			Logger.LogDebug("<<< Exiting ExecuteModule_Returns_Null_If_Application_Is_Null()");
        }

        [Test]
        public void ExecuteModule_Executes_ModuleApplication_If_Not_Null()
        {
			Logger.LogDebug(">>> Entering ExecuteModule_Executes_ModuleApplication_If_Not_Null()");
            // Arrange
            var engine = new ModuleExecutionEngine();

			Logger.LogDebug("ExecuteModule_Executes_ModuleApplication_If_Not_Null(): engine → {Engine}", engine);
            var moduleApp = new Mock<ModuleApplication>();
            var requestContext = new ModuleRequestContext() { ModuleApplication = moduleApp.Object };
			Logger.LogDebug("ExecuteModule_Executes_ModuleApplication_If_Not_Null(): moduleApp → {ModuleApp}", moduleApp);
			Logger.LogDebug("ExecuteModule_Executes_ModuleApplication_If_Not_Null(): requestContext → {RequestContext}", requestContext);

            // Act
            engine.ExecuteModule(requestContext);

            // Assert
            moduleApp.Verify(app => app.ExecuteRequest(It.IsAny<ModuleRequestContext>()));
			Logger.LogDebug("<<< Exiting ExecuteModule_Executes_ModuleApplication_If_Not_Null()");
        }

        [Test]
        public void ExecuteModule_Returns_Result_Of_Executing_ModuleApplication()
        {
			Logger.LogDebug(">>> Entering ExecuteModule_Returns_Result_Of_Executing_ModuleApplication()");
            // Arrange
            var engine = new ModuleExecutionEngine();
            var expected = new ModuleRequestResult();
			Logger.LogDebug("ExecuteModule_Returns_Result_Of_Executing_ModuleApplication(): engine → {Engine}", engine);
			Logger.LogDebug("ExecuteModule_Returns_Result_Of_Executing_ModuleApplication(): expected → {Expected}", expected);

            var moduleApp = new Mock<ModuleApplication>();
            moduleApp.Setup(app => app.ExecuteRequest(It.IsAny<ModuleRequestContext>()))
                    .Returns(expected);
					Logger.LogDebug("ExecuteModule_Returns_Result_Of_Executing_ModuleApplication(): moduleApp → {ModuleApp}", moduleApp);
            var requestContext = new ModuleRequestContext() { ModuleApplication = moduleApp.Object };

			Logger.LogDebug("ExecuteModule_Returns_Result_Of_Executing_ModuleApplication(): requestContext → {RequestContext}", requestContext);
            // Act
            ModuleRequestResult actual = engine.ExecuteModule(requestContext);

			Logger.LogDebug("ExecuteModule_Returns_Result_Of_Executing_ModuleApplication(): actual → {Actual}", actual);
            // Assert
            Assert.That(actual, Is.SameAs(expected));
			Logger.LogDebug("<<< Exiting ExecuteModule_Returns_Result_Of_Executing_ModuleApplication()");
        }

        [Test]
        public void ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult()
        {
			Logger.LogDebug(">>> Entering ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult()");
            // Arrange
            var engine = new ModuleExecutionEngine();

			Logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult(): engine → {Engine}", engine);
            var actionResultMock = new Mock<ActionResult>();

			Logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult(): actionResultMock → {ActionResultMock}", actionResultMock);
            var viewResultMock = actionResultMock.As<IDnnViewResult>();

			Logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult(): viewResultMock → {ViewResultMock}", viewResultMock);
            var controllerContext = MockHelper.CreateMockControllerContext();
            var moduleRequestResult = new ModuleRequestResult
			Logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult(): controllerContext → {ControllerContext}", controllerContext);
            {
                ActionResult = actionResultMock.Object,
                ControllerContext = controllerContext,
            };

            // Act
            engine.ExecuteModuleResult(moduleRequestResult, new StringWriter());

            // Arrange
            viewResultMock.Verify(v => v.ExecuteResult(It.IsAny<ControllerContext>(), It.IsAny<TextWriter>()));
			Logger.LogDebug("<<< Exiting ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult()");
        }

        [Test]
        public void ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult_With_ModuleRequestResult_ControllerContext()
        {
			Logger.LogDebug(">>> Entering ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult_With_ModuleRequestResult_ControllerContext()");
            // Arrange
            var engine = new ModuleExecutionEngine();

			Logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult_With_ModuleRequestResult_ControllerContext(): engine → {Engine}", engine);
            var actionResultMock = new Mock<ActionResult>();

			Logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult_With_ModuleRequestResult_ControllerContext(): actionResultMock → {ActionResultMock}", actionResultMock);
            var viewResultMock = actionResultMock.As<IDnnViewResult>();

			Logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult_With_ModuleRequestResult_ControllerContext(): viewResultMock → {ViewResultMock}", viewResultMock);
            var controllerContext = MockHelper.CreateMockControllerContext();
            var moduleRequestResult = new ModuleRequestResult
			Logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult_With_ModuleRequestResult_ControllerContext(): controllerContext → {ControllerContext}", controllerContext);
            {
                ActionResult = actionResultMock.Object,
                ControllerContext = controllerContext,
            };

            // Act
            engine.ExecuteModuleResult(moduleRequestResult, new StringWriter());

            // Arrange
            viewResultMock.Verify(v => v.ExecuteResult(controllerContext, It.IsAny<TextWriter>()));
			Logger.LogDebug("<<< Exiting ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult_With_ModuleRequestResult_ControllerContext()");
        }
    }
}
