// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable ObjectCreationAsStatement
namespace DotNetNuke.Tests.Web.Mvc.Helpers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Framework;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Web.Mvc.Fakes;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Framework;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using DotNetNuke.Web.Mvc.Helpers;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DnnHtmlHelperTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.serviceProvider = FakeServiceProvider.Setup(services =>
            {
				Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
                services.AddSingleton(Mock.Of<IClientResourceController>());
                services.AddSingleton(Mock.Of<IApplicationStatusInfo>());
                services.AddSingleton(Mock.Of<IEventLogger>());
            });
			Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void Constructor_Throws_On_Null_ViewContext()
        {
			Logger.LogDebug(">>> Entering Constructor_Throws_On_Null_ViewContext()");
            // Arrange
            var viewDataContainer = new Mock<IViewDataContainer>();

			Logger.LogDebug("Constructor_Throws_On_Null_ViewContext(): viewDataContainer → {ViewDataContainer}", viewDataContainer);
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHtmlHelper(null, viewDataContainer.Object));
			Logger.LogDebug("<<< Exiting Constructor_Throws_On_Null_ViewContext()");
        }

        [Test]
        public void Strongly_Typed_Constructor_Throws_On_Null_ViewContext()
        {
			Logger.LogDebug(">>> Entering Strongly_Typed_Constructor_Throws_On_Null_ViewContext()");
            // Arrange
            var viewDataContainer = new Mock<IViewDataContainer>();

			Logger.LogDebug("Strongly_Typed_Constructor_Throws_On_Null_ViewContext(): viewDataContainer → {ViewDataContainer}", viewDataContainer);
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHtmlHelper<Dog>(null, viewDataContainer.Object));
			Logger.LogDebug("<<< Exiting Strongly_Typed_Constructor_Throws_On_Null_ViewContext()");
        }

        [Test]
        public void Constructor_Throws_On_Null_ViewDataContainer()
        {
			Logger.LogDebug(">>> Entering Constructor_Throws_On_Null_ViewDataContainer()");
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHtmlHelper(new ViewContext(), null));
			Logger.LogDebug("<<< Exiting Constructor_Throws_On_Null_ViewDataContainer()");
        }

        [Test]
        public void Strongly_Typed_Constructor_Throws_On_Null_ViewDataContainer()
        {
			Logger.LogDebug(">>> Entering Strongly_Typed_Constructor_Throws_On_Null_ViewDataContainer()");
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHtmlHelper<Dog>(new ViewContext(), null));
			Logger.LogDebug("<<< Exiting Strongly_Typed_Constructor_Throws_On_Null_ViewDataContainer()");
        }

        [Test]
        public void Constructor_Throws_On_Null_RouteCollection()
        {
			Logger.LogDebug(">>> Entering Constructor_Throws_On_Null_RouteCollection()");
            // Arrange
            var viewDataContainer = new Mock<IViewDataContainer>();

			Logger.LogDebug("Constructor_Throws_On_Null_RouteCollection(): viewDataContainer → {ViewDataContainer}", viewDataContainer);
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHtmlHelper(new ViewContext(), viewDataContainer.Object, null, Mock.Of<IServicesFramework>()));
			Logger.LogDebug("<<< Exiting Constructor_Throws_On_Null_RouteCollection()");
        }

        [Test]
        public void Strongly_Typed_Constructor_Throws_On_Null_RouteCollection()
        {
			Logger.LogDebug(">>> Entering Strongly_Typed_Constructor_Throws_On_Null_RouteCollection()");
            // Arrange
            var viewDataContainer = new Mock<IViewDataContainer>();

			Logger.LogDebug("Strongly_Typed_Constructor_Throws_On_Null_RouteCollection(): viewDataContainer → {ViewDataContainer}", viewDataContainer);
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHtmlHelper<Dog>(new ViewContext(), viewDataContainer.Object, null, Mock.Of<IServicesFramework>()));
			Logger.LogDebug("<<< Exiting Strongly_Typed_Constructor_Throws_On_Null_RouteCollection()");
        }

        [Test]
        public void Constructor_Throws_On_Invalid_Controller_Property()
        {
			Logger.LogDebug(">>> Entering Constructor_Throws_On_Invalid_Controller_Property()");
            // Arrange
            var viewDataContainer = new Mock<IViewDataContainer>();
            var mockController = new Mock<ControllerBase>();
			Logger.LogDebug("Constructor_Throws_On_Invalid_Controller_Property(): viewDataContainer → {ViewDataContainer}", viewDataContainer);
			Logger.LogDebug("Constructor_Throws_On_Invalid_Controller_Property(): mockController → {MockController}", mockController);
            var viewContext = new ViewContext { Controller = mockController.Object };

            // Act,Assert
            Assert.Throws<InvalidOperationException>(() => new DnnHtmlHelper(viewContext, viewDataContainer.Object));
			Logger.LogDebug("<<< Exiting Constructor_Throws_On_Invalid_Controller_Property()");
        }

        [Test]
        public void Strongly_Typed_Constructor_Throws_On_Invalid_Controller_Property()
        {
			Logger.LogDebug(">>> Entering Strongly_Typed_Constructor_Throws_On_Invalid_Controller_Property()");
            // Arrange
            var mockViewDataContainer = new Mock<IViewDataContainer>();
            mockViewDataContainer.Setup(d => d.ViewData).Returns(new ViewDataDictionary());
			Logger.LogDebug("Strongly_Typed_Constructor_Throws_On_Invalid_Controller_Property(): mockViewDataContainer → {MockViewDataContainer}", mockViewDataContainer);
            var mockController = new Mock<ControllerBase>();
            var viewContext = new ViewContext { Controller = mockController.Object };
			Logger.LogDebug("Strongly_Typed_Constructor_Throws_On_Invalid_Controller_Property(): mockController → {MockController}", mockController);

            // Act,Assert
            Assert.Throws<InvalidOperationException>(() => new DnnHtmlHelper<Dog>(viewContext, mockViewDataContainer.Object));
			Logger.LogDebug("<<< Exiting Strongly_Typed_Constructor_Throws_On_Invalid_Controller_Property()");
        }

        [Test]
        public void Constructor_Sets_ModuleContext_Property()
        {
			Logger.LogDebug(">>> Entering Constructor_Sets_ModuleContext_Property()");
            // Arrange
            var viewDataContainer = new Mock<IViewDataContainer>();
            var mockController = new Mock<ControllerBase>();
			Logger.LogDebug("Constructor_Sets_ModuleContext_Property(): viewDataContainer → {ViewDataContainer}", viewDataContainer);
			Logger.LogDebug("Constructor_Sets_ModuleContext_Property(): mockController → {MockController}", mockController);
            var mockDnnController = mockController.As<IDnnController>();
            var expectedContext = new ModuleInstanceContext();
			Logger.LogDebug("Constructor_Sets_ModuleContext_Property(): mockDnnController → {MockDnnController}", mockDnnController);
			Logger.LogDebug("Constructor_Sets_ModuleContext_Property(): expectedContext → {ExpectedContext}", expectedContext);
            mockDnnController.Setup(c => c.ModuleContext).Returns(expectedContext);
            var viewContext = new ViewContext { Controller = mockController.Object };

            // Act
            var helper = new DnnHtmlHelper(viewContext, viewDataContainer.Object);

			Logger.LogDebug("Constructor_Sets_ModuleContext_Property(): helper → {Helper}", helper);
            // Assert
            Assert.That(helper.ModuleContext, Is.EqualTo(expectedContext));
			Logger.LogDebug("<<< Exiting Constructor_Sets_ModuleContext_Property()");
        }

        [Test]
        public void Strongly_Typed_Constructor_Sets_ModuleContext_Property()
        {
			Logger.LogDebug(">>> Entering Strongly_Typed_Constructor_Sets_ModuleContext_Property()");
            // Arrange
            var mockViewDataContainer = new Mock<IViewDataContainer>();
            mockViewDataContainer.Setup(d => d.ViewData).Returns(new ViewDataDictionary());
			Logger.LogDebug("Strongly_Typed_Constructor_Sets_ModuleContext_Property(): mockViewDataContainer → {MockViewDataContainer}", mockViewDataContainer);
            var mockController = new Mock<ControllerBase>();

			Logger.LogDebug("Strongly_Typed_Constructor_Sets_ModuleContext_Property(): mockController → {MockController}", mockController);
            var mockDnnController = mockController.As<IDnnController>();
            var expectedContext = new ModuleInstanceContext();
			Logger.LogDebug("Strongly_Typed_Constructor_Sets_ModuleContext_Property(): mockDnnController → {MockDnnController}", mockDnnController);
			Logger.LogDebug("Strongly_Typed_Constructor_Sets_ModuleContext_Property(): expectedContext → {ExpectedContext}", expectedContext);
            mockDnnController.Setup(c => c.ModuleContext).Returns(expectedContext);
            var viewContext = new ViewContext { Controller = mockController.Object };

            // Act
            var helper = new DnnHtmlHelper<Dog>(viewContext, mockViewDataContainer.Object);

			Logger.LogDebug("Strongly_Typed_Constructor_Sets_ModuleContext_Property(): helper → {Helper}", helper);
            // Assert
            Assert.That(helper.ModuleContext, Is.EqualTo(expectedContext));
			Logger.LogDebug("<<< Exiting Strongly_Typed_Constructor_Sets_ModuleContext_Property()");
        }

        [Test]
        public void ViewContext_Property_Returns_Passed_in_ViewContext()
        {
			Logger.LogDebug(">>> Entering ViewContext_Property_Returns_Passed_in_ViewContext()");
            // Arrange
            var mockViewDataContainer = new Mock<IViewDataContainer>();
            mockViewDataContainer.Setup(d => d.ViewData).Returns(new ViewDataDictionary());
			Logger.LogDebug("ViewContext_Property_Returns_Passed_in_ViewContext(): mockViewDataContainer → {MockViewDataContainer}", mockViewDataContainer);
            var mockController = new Mock<ControllerBase>();
            var mockDnnController = mockController.As<IDnnController>();
			Logger.LogDebug("ViewContext_Property_Returns_Passed_in_ViewContext(): mockController → {MockController}", mockController);
			Logger.LogDebug("ViewContext_Property_Returns_Passed_in_ViewContext(): mockDnnController → {MockDnnController}", mockDnnController);
            mockDnnController.Setup(c => c.ModuleContext).Returns(new ModuleInstanceContext());
            var viewContext = new ViewContext { Controller = mockController.Object };

            // Act
            var helper = new DnnHtmlHelper(viewContext, mockViewDataContainer.Object);

			Logger.LogDebug("ViewContext_Property_Returns_Passed_in_ViewContext(): helper → {Helper}", helper);
            // Assert
            Assert.That(helper.ViewContext, Is.EqualTo(viewContext));
			Logger.LogDebug("<<< Exiting ViewContext_Property_Returns_Passed_in_ViewContext()");
        }

        [Test]
        public void ViewDataContainer_Property_Returns_Passed_in_ViewDataContainer()
        {
			Logger.LogDebug(">>> Entering ViewDataContainer_Property_Returns_Passed_in_ViewDataContainer()");
            // Arrange
            var mockViewDataContainer = new Mock<IViewDataContainer>();
            var expectedViewData = new ViewDataDictionary();
			Logger.LogDebug("ViewDataContainer_Property_Returns_Passed_in_ViewDataContainer(): mockViewDataContainer → {MockViewDataContainer}", mockViewDataContainer);
			Logger.LogDebug("ViewDataContainer_Property_Returns_Passed_in_ViewDataContainer(): expectedViewData → {ExpectedViewData}", expectedViewData);
            mockViewDataContainer.Setup(d => d.ViewData).Returns(expectedViewData);

            var mockController = new Mock<ControllerBase>();
            var mockDnnController = mockController.As<IDnnController>();
			Logger.LogDebug("ViewDataContainer_Property_Returns_Passed_in_ViewDataContainer(): mockController → {MockController}", mockController);
			Logger.LogDebug("ViewDataContainer_Property_Returns_Passed_in_ViewDataContainer(): mockDnnController → {MockDnnController}", mockDnnController);
            mockDnnController.Setup(c => c.ModuleContext).Returns(new ModuleInstanceContext());
            var viewContext = new ViewContext { Controller = mockController.Object };

            // Act
            var helper = new DnnHtmlHelper(viewContext, mockViewDataContainer.Object);

			Logger.LogDebug("ViewDataContainer_Property_Returns_Passed_in_ViewDataContainer(): helper → {Helper}", helper);
            // Assert
            Assert.That(helper.ViewData, Is.EqualTo(expectedViewData));
			Logger.LogDebug("<<< Exiting ViewDataContainer_Property_Returns_Passed_in_ViewDataContainer()");
        }

        [Test]
        public void Strongly_Typed_ViewDataContainer_Property_Returns_Passed_in_Model()
        {
			Logger.LogDebug(">>> Entering Strongly_Typed_ViewDataContainer_Property_Returns_Passed_in_Model()");
            // Arrange
            var mockViewDataContainer = new Mock<IViewDataContainer>();
            var expectedViewData = new ViewDataDictionary<Dog>();
			Logger.LogDebug("Strongly_Typed_ViewDataContainer_Property_Returns_Passed_in_Model(): mockViewDataContainer → {MockViewDataContainer}", mockViewDataContainer);
			Logger.LogDebug("Strongly_Typed_ViewDataContainer_Property_Returns_Passed_in_Model(): expectedViewData → {ExpectedViewData}", expectedViewData);
			Logger.LogDebug("Strongly_Typed_ViewDataContainer_Property_Returns_Passed_in_Model(): expectedViewData.Model → {Arg0}", expectedViewData.Model);
            var expectedModel = new Dog();
            expectedViewData.Model = expectedModel;
			Logger.LogDebug("Strongly_Typed_ViewDataContainer_Property_Returns_Passed_in_Model(): expectedModel → {ExpectedModel}", expectedModel);
            mockViewDataContainer.Setup(d => d.ViewData).Returns(expectedViewData);

            var mockController = new Mock<ControllerBase>();
            var mockDnnController = mockController.As<IDnnController>();
			Logger.LogDebug("Strongly_Typed_ViewDataContainer_Property_Returns_Passed_in_Model(): mockController → {MockController}", mockController);
			Logger.LogDebug("Strongly_Typed_ViewDataContainer_Property_Returns_Passed_in_Model(): mockDnnController → {MockDnnController}", mockDnnController);
            mockDnnController.Setup(c => c.ModuleContext).Returns(new ModuleInstanceContext());
            var viewContext = new ViewContext { Controller = mockController.Object };

            // Act
            var helper = new DnnHtmlHelper<Dog>(viewContext, mockViewDataContainer.Object);

			Logger.LogDebug("Strongly_Typed_ViewDataContainer_Property_Returns_Passed_in_Model(): helper → {Helper}", helper);
            // Assert
            Assert.That(helper.ViewData.Model, Is.EqualTo(expectedModel));
			Logger.LogDebug("<<< Exiting Strongly_Typed_ViewDataContainer_Property_Returns_Passed_in_Model()");
        }

        [Test]
        public void EnableClientValidation_Updates_Context()
        {
			Logger.LogDebug(">>> Entering EnableClientValidation_Updates_Context()");
            // Arrange
            var mockViewPage = new Mock<DnnWebViewPage>() { CallBase = true };
            var mockController = new Mock<ControllerBase>();
			Logger.LogDebug("EnableClientValidation_Updates_Context(): mockViewPage → {MockViewPage}", mockViewPage);
			Logger.LogDebug("EnableClientValidation_Updates_Context(): mockController → {MockController}", mockController);
            var mockDnnController = mockController.As<IDnnController>();
            var mockViewContext = new Mock<ViewContext>();
			Logger.LogDebug("EnableClientValidation_Updates_Context(): mockDnnController → {MockDnnController}", mockDnnController);
			Logger.LogDebug("EnableClientValidation_Updates_Context(): mockViewContext → {MockViewContext}", mockViewContext);
            mockViewContext.SetupProperty(x => x.ClientValidationEnabled);
            mockViewContext.Setup(c => c.Controller).Returns(mockController.Object);
            mockViewPage.Object.ViewContext = mockViewContext.Object;
            mockViewPage.Object.InitHelpers();

            // Act
            mockViewPage.Object.Html.EnableClientValidation(true);

            // Assert
            Assert.That(mockViewPage.Object.ViewContext.ClientValidationEnabled, Is.True);
			Logger.LogDebug("<<< Exiting EnableClientValidation_Updates_Context()");
        }

        [Test]
        public void EnableUnobtrusiveJavaScript_Updates_Context()
        {
			Logger.LogDebug(">>> Entering EnableUnobtrusiveJavaScript_Updates_Context()");
            // Arrange
            var mockViewPage = new Mock<DnnWebViewPage>() { CallBase = true };
            var mockController = new Mock<ControllerBase>();
			Logger.LogDebug("EnableUnobtrusiveJavaScript_Updates_Context(): mockViewPage → {MockViewPage}", mockViewPage);
			Logger.LogDebug("EnableUnobtrusiveJavaScript_Updates_Context(): mockController → {MockController}", mockController);
            var mockDnnController = mockController.As<IDnnController>();
            var mockViewContext = new Mock<ViewContext>();
			Logger.LogDebug("EnableUnobtrusiveJavaScript_Updates_Context(): mockDnnController → {MockDnnController}", mockDnnController);
			Logger.LogDebug("EnableUnobtrusiveJavaScript_Updates_Context(): mockViewContext → {MockViewContext}", mockViewContext);
            mockViewContext.SetupProperty(x => x.UnobtrusiveJavaScriptEnabled);
            mockViewContext.Setup(c => c.Controller).Returns(mockController.Object);
            mockViewPage.Object.ViewContext = mockViewContext.Object;
            mockViewPage.Object.InitHelpers();

            // Act
            mockViewPage.Object.Html.EnableUnobtrusiveJavaScript(true);

            // Assert
            Assert.That(mockViewPage.Object.ViewContext.UnobtrusiveJavaScriptEnabled, Is.True);
			Logger.LogDebug("<<< Exiting EnableUnobtrusiveJavaScript_Updates_Context()");
        }
    }
}
