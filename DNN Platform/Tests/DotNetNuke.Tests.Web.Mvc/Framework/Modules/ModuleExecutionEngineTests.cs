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
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class ModuleExecutionEngineTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
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
        public void ExecuteModule_Requires_NonNull_ModuleRequestContext()
        {
			_logger.LogDebug(">>> Entering ExecuteModule_Requires_NonNull_ModuleRequestContext()");
            var engine = new ModuleExecutionEngine();
            Assert.Throws<ArgumentNullException>(() => engine.ExecuteModule(null));
			_logger.LogDebug("ExecuteModule_Requires_NonNull_ModuleRequestContext(): engine → {Engine}", engine);
			_logger.LogDebug("<<< Exiting ExecuteModule_Requires_NonNull_ModuleRequestContext()");
        }

        [Test]
        public void ExecuteModule_Returns_Null_If_Application_Is_Null()
        {
			_logger.LogDebug(">>> Entering ExecuteModule_Returns_Null_If_Application_Is_Null()");
            // Arrange
            var engine = new ModuleExecutionEngine();
			_logger.LogDebug("ExecuteModule_Returns_Null_If_Application_Is_Null(): engine → {Engine}", engine);
            var requestContext = new ModuleRequestContext() { ModuleApplication = null };
			_logger.LogDebug("ExecuteModule_Returns_Null_If_Application_Is_Null(): requestContext → {RequestContext}", requestContext);

            // Act
            ModuleRequestResult result = engine.ExecuteModule(requestContext);
			_logger.LogDebug("ExecuteModule_Returns_Null_If_Application_Is_Null(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.Null);
			_logger.LogDebug("<<< Exiting ExecuteModule_Returns_Null_If_Application_Is_Null()");
        }

        [Test]
        public void ExecuteModule_Executes_ModuleApplication_If_Not_Null()
        {
			_logger.LogDebug(">>> Entering ExecuteModule_Executes_ModuleApplication_If_Not_Null()");
            // Arrange
            var engine = new ModuleExecutionEngine();
			_logger.LogDebug("ExecuteModule_Executes_ModuleApplication_If_Not_Null(): engine → {Engine}", engine);

            var moduleApp = new Mock<ModuleApplication>();
			_logger.LogDebug("ExecuteModule_Executes_ModuleApplication_If_Not_Null(): moduleApp → {ModuleApp}", moduleApp);
            var requestContext = new ModuleRequestContext() { ModuleApplication = moduleApp.Object };
			_logger.LogDebug("ExecuteModule_Executes_ModuleApplication_If_Not_Null(): requestContext → {RequestContext}", requestContext);

            // Act
            engine.ExecuteModule(requestContext);

            // Assert
            moduleApp.Verify(app => app.ExecuteRequest(It.IsAny<ModuleRequestContext>()));
			_logger.LogDebug("<<< Exiting ExecuteModule_Executes_ModuleApplication_If_Not_Null()");
        }

        [Test]
        public void ExecuteModule_Returns_Result_Of_Executing_ModuleApplication()
        {
			_logger.LogDebug(">>> Entering ExecuteModule_Returns_Result_Of_Executing_ModuleApplication()");
            // Arrange
            var engine = new ModuleExecutionEngine();
			_logger.LogDebug("ExecuteModule_Returns_Result_Of_Executing_ModuleApplication(): engine → {Engine}", engine);
            var expected = new ModuleRequestResult();
			_logger.LogDebug("ExecuteModule_Returns_Result_Of_Executing_ModuleApplication(): expected → {Expected}", expected);

            var moduleApp = new Mock<ModuleApplication>();
			_logger.LogDebug("ExecuteModule_Returns_Result_Of_Executing_ModuleApplication(): moduleApp → {ModuleApp}", moduleApp);
            moduleApp.Setup(app => app.ExecuteRequest(It.IsAny<ModuleRequestContext>()))
                    .Returns(expected);
            var requestContext = new ModuleRequestContext() { ModuleApplication = moduleApp.Object };
			_logger.LogDebug("ExecuteModule_Returns_Result_Of_Executing_ModuleApplication(): requestContext → {RequestContext}", requestContext);

            // Act
            ModuleRequestResult actual = engine.ExecuteModule(requestContext);
			_logger.LogDebug("ExecuteModule_Returns_Result_Of_Executing_ModuleApplication(): actual → {Actual}", actual);

            // Assert
            Assert.That(actual, Is.SameAs(expected));
			_logger.LogDebug("<<< Exiting ExecuteModule_Returns_Result_Of_Executing_ModuleApplication()");
        }

        [Test]
        public void ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult()
        {
			_logger.LogDebug(">>> Entering ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult()");
            // Arrange
            var engine = new ModuleExecutionEngine();
			_logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult(): engine → {Engine}", engine);

            var actionResultMock = new Mock<ActionResult>();
			_logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult(): actionResultMock → {ActionResultMock}", actionResultMock);

            var viewResultMock = actionResultMock.As<IDnnViewResult>();
			_logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult(): viewResultMock → {ViewResultMock}", viewResultMock);

            var controllerContext = MockHelper.CreateMockControllerContext();
			_logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult(): controllerContext → {ControllerContext}", controllerContext);
            var moduleRequestResult = new ModuleRequestResult
            {
                ActionResult = actionResultMock.Object,
                ControllerContext = controllerContext,
            };

            // Act
            engine.ExecuteModuleResult(moduleRequestResult, new StringWriter());

            // Arrange
            viewResultMock.Verify(v => v.ExecuteResult(It.IsAny<ControllerContext>(), It.IsAny<TextWriter>()));
			_logger.LogDebug("<<< Exiting ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult()");
        }

        [Test]
        public void ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult_With_ModuleRequestResult_ControllerContext()
        {
			_logger.LogDebug(">>> Entering ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult_With_ModuleRequestResult_ControllerContext()");
            // Arrange
            var engine = new ModuleExecutionEngine();
			_logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult_With_ModuleRequestResult_ControllerContext(): engine → {Engine}", engine);

            var actionResultMock = new Mock<ActionResult>();
			_logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult_With_ModuleRequestResult_ControllerContext(): actionResultMock → {ActionResultMock}", actionResultMock);

            var viewResultMock = actionResultMock.As<IDnnViewResult>();
			_logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult_With_ModuleRequestResult_ControllerContext(): viewResultMock → {ViewResultMock}", viewResultMock);

            var controllerContext = MockHelper.CreateMockControllerContext();
			_logger.LogDebug("ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult_With_ModuleRequestResult_ControllerContext(): controllerContext → {ControllerContext}", controllerContext);
            var moduleRequestResult = new ModuleRequestResult
            {
                ActionResult = actionResultMock.Object,
                ControllerContext = controllerContext,
            };

            // Act
            engine.ExecuteModuleResult(moduleRequestResult, new StringWriter());

            // Arrange
            viewResultMock.Verify(v => v.ExecuteResult(controllerContext, It.IsAny<TextWriter>()));
			_logger.LogDebug("<<< Exiting ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult_With_ModuleRequestResult_ControllerContext()");
        }
    }
}
