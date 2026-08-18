// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.Mvc.Framework
{
    using System.Linq;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Web.Mvc.Framework;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using DotNetNuke.Web.Mvc.Framework.Modules;
    using DotNetNuke.Web.Mvc.Routing;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class ModuleDelegatingViewEngineTests
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
        public void Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection()
        {
			_logger.LogDebug(">>> Entering Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection()");
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
			_logger.LogDebug("Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection(): mockEngines → {MockEngines}", mockEngines);
            var result = new ViewEngineResult(new[] { "foo", "bar", "baz" });
			_logger.LogDebug("Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection(): result → {Result}", result);
            var controller = new Mock<DnnController>();
			_logger.LogDebug("Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection(): controller → {Controller}", controller);
            controller.SetupAllProperties();
            var context = MockHelper.CreateMockControllerContext(controller.Object);
			_logger.LogDebug("Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection(): context → {Context}", context);

            // ReSharper disable ConvertToConstant.Local
            string viewName = "Foo";
			_logger.LogDebug("Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection(): viewName → {ViewName}", viewName);

            // ReSharper restore ConvertToConstant.Local
            mockEngines.Setup(e => e.FindPartialView(context, viewName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine(Mock.Of<IHostSettings>());
			_logger.LogDebug("Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection(): viewEngine → {ViewEngine}", viewEngine);

            // Act
            ViewEngineResult engineResult = viewEngine.FindPartialView(context, viewName, true);
			_logger.LogDebug("Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection(): engineResult → {EngineResult}", engineResult);

            // Assert
            mockEngines.Verify(e => e.FindPartialView(context, viewName));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(engineResult.SearchedLocations.ElementAt(0), Is.EqualTo("foo"));
                Assert.That(engineResult.SearchedLocations.ElementAt(1), Is.EqualTo("bar"));
                Assert.That(engineResult.SearchedLocations.ElementAt(2), Is.EqualTo("baz"));
            }
			_logger.LogDebug("<<< Exiting Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection()");
        }

        [Test]
        public void Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection()
        {
			_logger.LogDebug(">>> Entering Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection()");
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
			_logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): mockEngines → {MockEngines}", mockEngines);
            var result = new ViewEngineResult(new[] { "foo", "bar", "baz" });
			_logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): result → {Result}", result);
            var controller = new Mock<DnnController>();
			_logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): controller → {Controller}", controller);
            controller.SetupAllProperties();
            var context = MockHelper.CreateMockControllerContext(controller.Object);
			_logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): context → {Context}", context);

            // ReSharper disable ConvertToConstant.Local
            var viewName = "Foo";
			_logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): viewName → {ViewName}", viewName);
            var masterName = "Bar";
			_logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): masterName → {MasterName}", masterName);

            // ReSharper restore ConvertToConstant.Local
            mockEngines.Setup(e => e.FindView(context, viewName, masterName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine();
			_logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): viewEngine → {ViewEngine}", viewEngine);

            // Act
            var engineResult = viewEngine.FindView(context, viewName, masterName, true);
			_logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): engineResult → {EngineResult}", engineResult);

            // Assert
            mockEngines.Verify(e => e.FindView(context, viewName, masterName));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(engineResult.SearchedLocations.ElementAt(0), Is.EqualTo("foo"));
                Assert.That(engineResult.SearchedLocations.ElementAt(1), Is.EqualTo("bar"));
                Assert.That(engineResult.SearchedLocations.ElementAt(2), Is.EqualTo("baz"));
            }
			_logger.LogDebug("<<< Exiting Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection()");
        }

        [Test]
        public void Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately()
        {
			_logger.LogDebug(">>> Entering Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately()");
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): mockEngines → {MockEngines}", mockEngines);
            var mockEngine = new Mock<IViewEngine>();
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): mockEngine → {MockEngine}", mockEngine);
            var mockView = new Mock<IView>();
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): mockView → {MockView}", mockView);
            var result = new ViewEngineResult(mockView.Object, mockEngine.Object);
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): result → {Result}", result);
            var controller = new Mock<DnnController>();
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): controller → {Controller}", controller);
            controller.SetupAllProperties();
            var context = MockHelper.CreateMockControllerContext(controller.Object);
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): context → {Context}", context);

            // ReSharper disable ConvertToConstant.Local
            string viewName = "Foo";
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): viewName → {ViewName}", viewName);
            string masterName = "Bar";
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): masterName → {MasterName}", masterName);

            // ReSharper restore ConvertToConstant.Local
            mockEngines.Setup(e => e.FindView(context, viewName, masterName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine();
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): viewEngine → {ViewEngine}", viewEngine);

            // Act
            ViewEngineResult engineResult = viewEngine.FindView(context, viewName, masterName, true);
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): engineResult → {EngineResult}", engineResult);
            viewEngine.ReleaseView(context, engineResult.View);

            // Assert
            mockEngine.Verify(e => e.ReleaseView(context, mockView.Object));
			_logger.LogDebug("<<< Exiting Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately()");
        }

        [Test]
        public void Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately()
        {
			_logger.LogDebug(">>> Entering Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately()");
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): mockEngines → {MockEngines}", mockEngines);
            var mockEngine = new Mock<IViewEngine>();
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): mockEngine → {MockEngine}", mockEngine);
            var mockView = new Mock<IView>();
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): mockView → {MockView}", mockView);
            var result = new ViewEngineResult(mockView.Object, mockEngine.Object);
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): result → {Result}", result);
            var controller = new Mock<DnnController>();
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): controller → {Controller}", controller);
            controller.SetupAllProperties();
            var context = MockHelper.CreateMockControllerContext(controller.Object);
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): context → {Context}", context);

            // ReSharper disable ConvertToConstant.Local
            string viewName = "Foo";
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): viewName → {ViewName}", viewName);

            // ReSharper restore ConvertToConstant.Local
            mockEngines.Setup(e => e.FindPartialView(context, viewName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine();
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): viewEngine → {ViewEngine}", viewEngine);

            // Act
            ViewEngineResult engineResult = viewEngine.FindPartialView(context, viewName, true);
			_logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): engineResult → {EngineResult}", engineResult);
            viewEngine.ReleaseView(context, engineResult.View);

            // Assert
            mockEngine.Verify(e => e.ReleaseView(context, mockView.Object));
			_logger.LogDebug("<<< Exiting Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately()");
        }

        [Test]
        public void Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application()
        {
			_logger.LogDebug(">>> Entering Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application()");
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
			_logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application(): mockEngines → {MockEngines}", mockEngines);
            var viewEngine = new ModuleDelegatingViewEngine();
			_logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application(): viewEngine → {ViewEngine}", viewEngine);
            var controller = new Mock<DnnController>();
			_logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application(): controller → {Controller}", controller);
            controller.SetupAllProperties();
            var context = MockHelper.CreateMockControllerContext(controller.Object);
			_logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application(): context → {Context}", context);

            // Act
            var engineResult = viewEngine.FindView(context, "Foo", "Bar", true);
			_logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application(): engineResult → {EngineResult}", engineResult);

            // Assert
            Assert.That(engineResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(engineResult.View, Is.Null);
                Assert.That(engineResult.SearchedLocations.Count(), Is.EqualTo(0));
            }
			_logger.LogDebug("<<< Exiting Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application()");
        }

        [Test]
        public void Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application()
        {
			_logger.LogDebug(">>> Entering Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application()");
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
			_logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application(): mockEngines → {MockEngines}", mockEngines);
            var viewEngine = new ModuleDelegatingViewEngine();
			_logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application(): viewEngine → {ViewEngine}", viewEngine);
            var controller = new Mock<DnnController>();
			_logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application(): controller → {Controller}", controller);
            controller.SetupAllProperties();
            var context = MockHelper.CreateMockControllerContext(controller.Object);
			_logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application(): context → {Context}", context);

            // Act
            var engineResult = viewEngine.FindPartialView(context, "Foo", true);
			_logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application(): engineResult → {EngineResult}", engineResult);

            // Assert
            Assert.That(engineResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(engineResult.View, Is.Null);
                Assert.That(engineResult.SearchedLocations.Count(), Is.EqualTo(0));
            }
			_logger.LogDebug("<<< Exiting Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application()");
        }

        private static void SetupMockModuleApplication(ControllerContext context, ViewEngineCollection engines)
        {
            var mockApp = new Mock<ModuleApplication>();
            mockApp.Object.ViewEngines = engines;
            (context.Controller as IDnnController).ViewEngineCollectionEx = engines;

            var activeModuleRequest = new ModuleRequestResult
            {
                ModuleApplication = mockApp.Object,
            };

            context.HttpContext.SetModuleRequestResult(activeModuleRequest);
			_logger.LogDebug("<<< Exiting SetupMockModuleApplication(context={Context},engines={Engines})", context, engines);
        }
    }
}
