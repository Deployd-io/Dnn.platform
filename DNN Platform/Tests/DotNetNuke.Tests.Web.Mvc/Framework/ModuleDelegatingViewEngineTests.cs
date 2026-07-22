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

    [TestFixture]
    public class ModuleDelegatingViewEngineTests
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
        public void Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection()
        {
			Logger.LogDebug(">>> Entering Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection()");
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var result = new ViewEngineResult(new[] { "foo", "bar", "baz" });
			Logger.LogDebug("Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection(): mockEngines → {MockEngines}", mockEngines);
			Logger.LogDebug("Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection(): result → {Result}", result);
            var controller = new Mock<DnnController>();
            controller.SetupAllProperties();
			Logger.LogDebug("Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection(): controller → {Controller}", controller);
            var context = MockHelper.CreateMockControllerContext(controller.Object);

			Logger.LogDebug("Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection(): context → {Context}", context);
            // ReSharper disable ConvertToConstant.Local
            string viewName = "Foo";

			Logger.LogDebug("Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection(): viewName → {ViewName}", viewName);
            // ReSharper restore ConvertToConstant.Local
            mockEngines.Setup(e => e.FindPartialView(context, viewName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine(Mock.Of<IHostSettings>());

			Logger.LogDebug("Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection(): viewEngine → {ViewEngine}", viewEngine);
            // Act
            ViewEngineResult engineResult = viewEngine.FindPartialView(context, viewName, true);

			Logger.LogDebug("Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection(): engineResult → {EngineResult}", engineResult);
            // Assert
            mockEngines.Verify(e => e.FindPartialView(context, viewName));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(engineResult.SearchedLocations.ElementAt(0), Is.EqualTo("foo"));
                Assert.That(engineResult.SearchedLocations.ElementAt(1), Is.EqualTo("bar"));
                Assert.That(engineResult.SearchedLocations.ElementAt(2), Is.EqualTo("baz"));
            }
			Logger.LogDebug("<<< Exiting Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection()");
        }

        [Test]
        public void Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection()
        {
			Logger.LogDebug(">>> Entering Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection()");
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var result = new ViewEngineResult(new[] { "foo", "bar", "baz" });
			Logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): mockEngines → {MockEngines}", mockEngines);
			Logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): result → {Result}", result);
            var controller = new Mock<DnnController>();
            controller.SetupAllProperties();
			Logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): controller → {Controller}", controller);
            var context = MockHelper.CreateMockControllerContext(controller.Object);

			Logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): context → {Context}", context);
            // ReSharper disable ConvertToConstant.Local
            var viewName = "Foo";
            var masterName = "Bar";
			Logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): viewName → {ViewName}", viewName);
			Logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): masterName → {MasterName}", masterName);

            // ReSharper restore ConvertToConstant.Local
            mockEngines.Setup(e => e.FindView(context, viewName, masterName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine();

			Logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): viewEngine → {ViewEngine}", viewEngine);
            // Act
            var engineResult = viewEngine.FindView(context, viewName, masterName, true);

			Logger.LogDebug("Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection(): engineResult → {EngineResult}", engineResult);
            // Assert
            mockEngines.Verify(e => e.FindView(context, viewName, masterName));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(engineResult.SearchedLocations.ElementAt(0), Is.EqualTo("foo"));
                Assert.That(engineResult.SearchedLocations.ElementAt(1), Is.EqualTo("bar"));
                Assert.That(engineResult.SearchedLocations.ElementAt(2), Is.EqualTo("baz"));
            }
			Logger.LogDebug("<<< Exiting Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection()");
        }

        [Test]
        public void Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately()
        {
			Logger.LogDebug(">>> Entering Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately()");
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var mockEngine = new Mock<IViewEngine>();
			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): mockEngines → {MockEngines}", mockEngines);
			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): mockEngine → {MockEngine}", mockEngine);
            var mockView = new Mock<IView>();
            var result = new ViewEngineResult(mockView.Object, mockEngine.Object);
			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): mockView → {MockView}", mockView);
			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): result → {Result}", result);
            var controller = new Mock<DnnController>();
            controller.SetupAllProperties();
			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): controller → {Controller}", controller);
            var context = MockHelper.CreateMockControllerContext(controller.Object);

			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): context → {Context}", context);
            // ReSharper disable ConvertToConstant.Local
            string viewName = "Foo";
            string masterName = "Bar";
			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): viewName → {ViewName}", viewName);
			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): masterName → {MasterName}", masterName);

            // ReSharper restore ConvertToConstant.Local
            mockEngines.Setup(e => e.FindView(context, viewName, masterName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine();

			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): viewEngine → {ViewEngine}", viewEngine);
            // Act
            ViewEngineResult engineResult = viewEngine.FindView(context, viewName, masterName, true);
            viewEngine.ReleaseView(context, engineResult.View);
			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately(): engineResult → {EngineResult}", engineResult);

            // Assert
            mockEngine.Verify(e => e.ReleaseView(context, mockView.Object));
			Logger.LogDebug("<<< Exiting Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately()");
        }

        [Test]
        public void Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately()
        {
			Logger.LogDebug(">>> Entering Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately()");
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var mockEngine = new Mock<IViewEngine>();
			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): mockEngines → {MockEngines}", mockEngines);
			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): mockEngine → {MockEngine}", mockEngine);
            var mockView = new Mock<IView>();
            var result = new ViewEngineResult(mockView.Object, mockEngine.Object);
			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): mockView → {MockView}", mockView);
			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): result → {Result}", result);
            var controller = new Mock<DnnController>();
            controller.SetupAllProperties();
			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): controller → {Controller}", controller);
            var context = MockHelper.CreateMockControllerContext(controller.Object);

			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): context → {Context}", context);
            // ReSharper disable ConvertToConstant.Local
            string viewName = "Foo";

			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): viewName → {ViewName}", viewName);
            // ReSharper restore ConvertToConstant.Local
            mockEngines.Setup(e => e.FindPartialView(context, viewName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine();

			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): viewEngine → {ViewEngine}", viewEngine);
            // Act
            ViewEngineResult engineResult = viewEngine.FindPartialView(context, viewName, true);
            viewEngine.ReleaseView(context, engineResult.View);
			Logger.LogDebug("Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately(): engineResult → {EngineResult}", engineResult);

            // Assert
            mockEngine.Verify(e => e.ReleaseView(context, mockView.Object));
			Logger.LogDebug("<<< Exiting Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately()");
        }

        [Test]
        public void Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application()
        {
			Logger.LogDebug(">>> Entering Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application()");
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var viewEngine = new ModuleDelegatingViewEngine();
			Logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application(): mockEngines → {MockEngines}", mockEngines);
			Logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application(): viewEngine → {ViewEngine}", viewEngine);
            var controller = new Mock<DnnController>();
            controller.SetupAllProperties();
			Logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application(): controller → {Controller}", controller);
            var context = MockHelper.CreateMockControllerContext(controller.Object);

			Logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application(): context → {Context}", context);
            // Act
            var engineResult = viewEngine.FindView(context, "Foo", "Bar", true);

			Logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application(): engineResult → {EngineResult}", engineResult);
            // Assert
            Assert.That(engineResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(engineResult.View, Is.Null);
                Assert.That(engineResult.SearchedLocations.Count(), Is.EqualTo(0));
            }
			Logger.LogDebug("<<< Exiting Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application()");
        }

        [Test]
        public void Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application()
        {
			Logger.LogDebug(">>> Entering Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application()");
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var viewEngine = new ModuleDelegatingViewEngine();
			Logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application(): mockEngines → {MockEngines}", mockEngines);
			Logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application(): viewEngine → {ViewEngine}", viewEngine);
            var controller = new Mock<DnnController>();
            controller.SetupAllProperties();
			Logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application(): controller → {Controller}", controller);
            var context = MockHelper.CreateMockControllerContext(controller.Object);

			Logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application(): context → {Context}", context);
            // Act
            var engineResult = viewEngine.FindPartialView(context, "Foo", true);

			Logger.LogDebug("Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application(): engineResult → {EngineResult}", engineResult);
            // Assert
            Assert.That(engineResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(engineResult.View, Is.Null);
                Assert.That(engineResult.SearchedLocations.Count(), Is.EqualTo(0));
            }
			Logger.LogDebug("<<< Exiting Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application()");
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
        }
    }
}
