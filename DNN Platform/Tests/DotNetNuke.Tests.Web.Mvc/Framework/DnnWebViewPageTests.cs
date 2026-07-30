// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Framework
{
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Web.Mvc.Fakes;
    using DotNetNuke.Web.Mvc.Framework;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using DotNetNuke.Web.Mvc.Helpers;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DnnWebViewPageTests
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
        public void InitHelpers_Sets_Dnn_Property()
        {
			Logger.LogDebug(">>> Entering InitHelpers_Sets_Dnn_Property()");
            // Arrange
            var mockViewPage = new Mock<DnnWebViewPage>() { CallBase = true };
            var mockController = new Mock<ControllerBase>();
			Logger.LogDebug("InitHelpers_Sets_Dnn_Property(): mockViewPage → {MockViewPage}", mockViewPage);
			Logger.LogDebug("InitHelpers_Sets_Dnn_Property(): mockController → {MockController}", mockController);
            var mockDnnController = mockController.As<IDnnController>();
            var viewContext = new ViewContext
			Logger.LogDebug("InitHelpers_Sets_Dnn_Property(): mockDnnController → {MockDnnController}", mockDnnController);
            {
                Controller = mockController.Object,
            };
            mockViewPage.Object.ViewContext = viewContext;

            // Act
            mockViewPage.Object.InitHelpers();

            // Assert
            Assert.That(mockViewPage.Object.Dnn, Is.Not.Null);
            Assert.That(mockViewPage.Object.Dnn, Is.InstanceOf<DnnHelper>());
			Logger.LogDebug("<<< Exiting InitHelpers_Sets_Dnn_Property()");
        }

        [Test]
        public void InitHelpers_Sets_Html_Property()
        {
			Logger.LogDebug(">>> Entering InitHelpers_Sets_Html_Property()");
            // Arrange
            var mockViewPage = new Mock<DnnWebViewPage>() { CallBase = true };
            var mockController = new Mock<ControllerBase>();
			Logger.LogDebug("InitHelpers_Sets_Html_Property(): mockViewPage → {MockViewPage}", mockViewPage);
			Logger.LogDebug("InitHelpers_Sets_Html_Property(): mockController → {MockController}", mockController);
            var mockDnnController = mockController.As<IDnnController>();
            var viewContext = new ViewContext
			Logger.LogDebug("InitHelpers_Sets_Html_Property(): mockDnnController → {MockDnnController}", mockDnnController);
            {
                Controller = mockController.Object,
            };
            mockViewPage.Object.ViewContext = viewContext;

            // Act
            mockViewPage.Object.InitHelpers();

            // Assert
            Assert.That(mockViewPage.Object.Html, Is.Not.Null);
            Assert.That(mockViewPage.Object.Html, Is.InstanceOf<DnnHtmlHelper>());
			Logger.LogDebug("<<< Exiting InitHelpers_Sets_Html_Property()");
        }

        [Test]
        public void InitHelpers_Sets_Dnn_Property_For_Strongly_Typed_Helper()
        {
			Logger.LogDebug(">>> Entering InitHelpers_Sets_Dnn_Property_For_Strongly_Typed_Helper()");
            // Arrange
            var mockViewPage = new Mock<DnnWebViewPage<Dog>>() { CallBase = true };
            var mockController = new Mock<ControllerBase>();
			Logger.LogDebug("InitHelpers_Sets_Dnn_Property_For_Strongly_Typed_Helper(): mockViewPage → {MockViewPage}", mockViewPage);
			Logger.LogDebug("InitHelpers_Sets_Dnn_Property_For_Strongly_Typed_Helper(): mockController → {MockController}", mockController);
            var mockDnnController = mockController.As<IDnnController>();
            var viewContext = new ViewContext
			Logger.LogDebug("InitHelpers_Sets_Dnn_Property_For_Strongly_Typed_Helper(): mockDnnController → {MockDnnController}", mockDnnController);
            {
                Controller = mockController.Object,
            };
            mockViewPage.Object.ViewContext = viewContext;

            // Act
            mockViewPage.Object.InitHelpers();

            // Assert
            Assert.That(mockViewPage.Object.Html, Is.Not.Null);
            Assert.That(mockViewPage.Object.Html, Is.InstanceOf<DnnHtmlHelper<Dog>>());
			Logger.LogDebug("<<< Exiting InitHelpers_Sets_Dnn_Property_For_Strongly_Typed_Helper()");
        }

        [Test]
        public void InitHelpers_Sets_Url_Property()
        {
			Logger.LogDebug(">>> Entering InitHelpers_Sets_Url_Property()");
            // Arrange
            var mockViewPage = new Mock<DnnWebViewPage>() { CallBase = true };
            var mockController = new Mock<ControllerBase>();
			Logger.LogDebug("InitHelpers_Sets_Url_Property(): mockViewPage → {MockViewPage}", mockViewPage);
			Logger.LogDebug("InitHelpers_Sets_Url_Property(): mockController → {MockController}", mockController);
            var mockDnnController = mockController.As<IDnnController>();
            var viewContext = new ViewContext
			Logger.LogDebug("InitHelpers_Sets_Url_Property(): mockDnnController → {MockDnnController}", mockDnnController);
            {
                Controller = mockController.Object,
            };
            mockViewPage.Object.ViewContext = viewContext;

            // Act
            mockViewPage.Object.InitHelpers();

            // Assert
            Assert.That(mockViewPage.Object.Url, Is.Not.Null);
            Assert.That(mockViewPage.Object.Url, Is.InstanceOf<DnnUrlHelper>());
			Logger.LogDebug("<<< Exiting InitHelpers_Sets_Url_Property()");
        }
    }
}
