// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable ObjectCreationAsStatement
namespace DotNetNuke.Tests.Web.Mvc.Helpers
{
    using System;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Tests.Utilities.Mocks;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using DotNetNuke.Web.Mvc.Helpers;
    using DotNetNuke.Web.Mvc.Routing;
    using Moq;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class DnnUrlHelperTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        [SetUp]
        public void SetUp()
        {
			_logger.LogDebug(">>> Entering SetUp()");
            MockComponentProvider.ResetContainer();
			_logger.LogDebug("<<< Exiting SetUp()");
        }

        [Test]
        public void Constructor_Throws_On_Null_ViewContext()
        {
			_logger.LogDebug(">>> Entering Constructor_Throws_On_Null_ViewContext()");
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnUrlHelper(null));
			_logger.LogDebug("<<< Exiting Constructor_Throws_On_Null_ViewContext()");
        }

        [Test]
        public void Constructor_Throws_On_Null_RequestContext()
        {
			_logger.LogDebug(">>> Entering Constructor_Throws_On_Null_RequestContext()");
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnUrlHelper(null, new Mock<IDnnController>().Object));
			_logger.LogDebug("<<< Exiting Constructor_Throws_On_Null_RequestContext()");
        }

        [Test]
        public void Constructor_Throws_On_Null_Controller()
        {
			_logger.LogDebug(">>> Entering Constructor_Throws_On_Null_Controller()");
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnUrlHelper(new Mock<RequestContext>().Object, null));
			_logger.LogDebug("<<< Exiting Constructor_Throws_On_Null_Controller()");
        }

        [Test]
        public void Constructor_Throws_On_Invalid_Controller_Property()
        {
			_logger.LogDebug(">>> Entering Constructor_Throws_On_Invalid_Controller_Property()");
            // Arrange
            var mockController = new Mock<ControllerBase>();
			_logger.LogDebug("Constructor_Throws_On_Invalid_Controller_Property(): mockController → {MockController}", mockController);
            var viewContext = new ViewContext { Controller = mockController.Object };

            // Act,Assert
            Assert.Throws<InvalidOperationException>(() => new DnnUrlHelper(viewContext));
			_logger.LogDebug("<<< Exiting Constructor_Throws_On_Invalid_Controller_Property()");
        }

        [Test]
        public void ViewContext_Constructor_Sets_ModuleContext_Property()
        {
			_logger.LogDebug(">>> Entering ViewContext_Constructor_Sets_ModuleContext_Property()");
            // Arrange
            var mockController = new Mock<ControllerBase>();
			_logger.LogDebug("ViewContext_Constructor_Sets_ModuleContext_Property(): mockController → {MockController}", mockController);
            var mockDnnController = mockController.As<IDnnController>();
			_logger.LogDebug("ViewContext_Constructor_Sets_ModuleContext_Property(): mockDnnController → {MockDnnController}", mockDnnController);
            var expectedContext = new ModuleInstanceContext();
			_logger.LogDebug("ViewContext_Constructor_Sets_ModuleContext_Property(): expectedContext → {ExpectedContext}", expectedContext);
            mockDnnController.Setup(c => c.ModuleContext).Returns(expectedContext);
            var viewContext = new ViewContext { Controller = mockController.Object };

            // Act
            var helper = new DnnUrlHelper(viewContext);
			_logger.LogDebug("ViewContext_Constructor_Sets_ModuleContext_Property(): helper → {Helper}", helper);

            // Assert
            Assert.That(helper.ModuleContext, Is.EqualTo(expectedContext));
			_logger.LogDebug("<<< Exiting ViewContext_Constructor_Sets_ModuleContext_Property()");
        }

        [Test]
        public void RequestContext_Constructor_Sets_ModuleContext_Property()
        {
			_logger.LogDebug(">>> Entering RequestContext_Constructor_Sets_ModuleContext_Property()");
            // Arrange
            var expectedContext = new ModuleInstanceContext();
			_logger.LogDebug("RequestContext_Constructor_Sets_ModuleContext_Property(): expectedContext → {ExpectedContext}", expectedContext);

            var mockController = new Mock<IDnnController>();
			_logger.LogDebug("RequestContext_Constructor_Sets_ModuleContext_Property(): mockController → {MockController}", mockController);
            mockController.SetupGet(c => c.ModuleContext)
                          .Returns(expectedContext);

            var requestContext = new RequestContext();
			_logger.LogDebug("RequestContext_Constructor_Sets_ModuleContext_Property(): requestContext → {RequestContext}", requestContext);

            // Act
            var helper = new DnnUrlHelper(requestContext, mockController.Object);
			_logger.LogDebug("RequestContext_Constructor_Sets_ModuleContext_Property(): helper → {Helper}", helper);

            // Assert
            Assert.That(helper, Is.Not.Null);
            Assert.That(helper.ModuleContext, Is.EqualTo(expectedContext));
			_logger.LogDebug("<<< Exiting RequestContext_Constructor_Sets_ModuleContext_Property()");
        }

        [Test]
        public void Action_Method_ViewContext_RetrievesRawUrl()
        {
			_logger.LogDebug(">>> Entering Action_Method_ViewContext_RetrievesRawUrl()");
            // Arrange
            var expectedContext = new ModuleInstanceContext();
			_logger.LogDebug("Action_Method_ViewContext_RetrievesRawUrl(): expectedContext → {ExpectedContext}", expectedContext);
            var rawUrl = "http://base.url/";
			_logger.LogDebug("Action_Method_ViewContext_RetrievesRawUrl(): rawUrl → {RawUrl}", rawUrl);
            var helper = ArrangeHelper(expectedContext, rawUrl);
			_logger.LogDebug("Action_Method_ViewContext_RetrievesRawUrl(): helper → {Helper}", helper);

            // Act
            var result = helper.Action();
			_logger.LogDebug("Action_Method_ViewContext_RetrievesRawUrl(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(rawUrl));
			_logger.LogDebug("<<< Exiting Action_Method_ViewContext_RetrievesRawUrl()");
        }

        [Test]
        public void Action_Method_RequestContext_RetrievesRawUrl()
        {
			_logger.LogDebug(">>> Entering Action_Method_RequestContext_RetrievesRawUrl()");
            // Arrange
            var expectedContext = new ModuleInstanceContext();
			_logger.LogDebug("Action_Method_RequestContext_RetrievesRawUrl(): expectedContext → {ExpectedContext}", expectedContext);
            var rawUrl = "http://base.url/";
			_logger.LogDebug("Action_Method_RequestContext_RetrievesRawUrl(): rawUrl → {RawUrl}", rawUrl);
            var helper = ArrangeHelper(expectedContext, rawUrl, false);
			_logger.LogDebug("Action_Method_RequestContext_RetrievesRawUrl(): helper → {Helper}", helper);

            // Act
            var result = helper.Action();
			_logger.LogDebug("Action_Method_RequestContext_RetrievesRawUrl(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(rawUrl));
			_logger.LogDebug("<<< Exiting Action_Method_RequestContext_RetrievesRawUrl()");
        }

        [Test]
        public void Action_Method_Calls_ModuleRouteProvider()
        {
			_logger.LogDebug(">>> Entering Action_Method_Calls_ModuleRouteProvider()");
            // Arrange
            var expectedContext = new ModuleInstanceContext();
			_logger.LogDebug("Action_Method_Calls_ModuleRouteProvider(): expectedContext → {ExpectedContext}", expectedContext);
            var helper = ArrangeHelper(expectedContext);
			_logger.LogDebug("Action_Method_Calls_ModuleRouteProvider(): helper → {Helper}", helper);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
			_logger.LogDebug("Action_Method_Calls_ModuleRouteProvider(): mockRouteProvider → {MockRouteProvider}", mockRouteProvider);
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            // Act
            helper.Action("foo");

            // Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
			_logger.LogDebug("<<< Exiting Action_Method_Calls_ModuleRouteProvider()");
        }

        [Test]
        public void Action_Overload_1__Method_Calls_ModuleRouteProvider()
        {
			_logger.LogDebug(">>> Entering Action_Overload_1__Method_Calls_ModuleRouteProvider()");
            // Arrange
            var expectedContext = new ModuleInstanceContext();
			_logger.LogDebug("Action_Overload_1__Method_Calls_ModuleRouteProvider(): expectedContext → {ExpectedContext}", expectedContext);
            var helper = ArrangeHelper(expectedContext);
			_logger.LogDebug("Action_Overload_1__Method_Calls_ModuleRouteProvider(): helper → {Helper}", helper);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
			_logger.LogDebug("Action_Overload_1__Method_Calls_ModuleRouteProvider(): mockRouteProvider → {MockRouteProvider}", mockRouteProvider);
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            // Act
            helper.Action("foo", new RouteValueDictionary());

            // Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
			_logger.LogDebug("<<< Exiting Action_Overload_1__Method_Calls_ModuleRouteProvider()");
        }

        [Test]
        public void Action_Overload_2_Method_Calls_ModuleRouteProvider()
        {
			_logger.LogDebug(">>> Entering Action_Overload_2_Method_Calls_ModuleRouteProvider()");
            // Arrange
            var expectedContext = new ModuleInstanceContext();
			_logger.LogDebug("Action_Overload_2_Method_Calls_ModuleRouteProvider(): expectedContext → {ExpectedContext}", expectedContext);
            var helper = ArrangeHelper(expectedContext);
			_logger.LogDebug("Action_Overload_2_Method_Calls_ModuleRouteProvider(): helper → {Helper}", helper);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
			_logger.LogDebug("Action_Overload_2_Method_Calls_ModuleRouteProvider(): mockRouteProvider → {MockRouteProvider}", mockRouteProvider);
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            // Act
            helper.Action("foo", "bar");

            // Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
			_logger.LogDebug("<<< Exiting Action_Overload_2_Method_Calls_ModuleRouteProvider()");
        }

        [Test]
        public void Action_Overload_3_Method_Calls_ModuleRouteProvider()
        {
			_logger.LogDebug(">>> Entering Action_Overload_3_Method_Calls_ModuleRouteProvider()");
            // Arrange
            var expectedContext = new ModuleInstanceContext();
			_logger.LogDebug("Action_Overload_3_Method_Calls_ModuleRouteProvider(): expectedContext → {ExpectedContext}", expectedContext);
            var helper = ArrangeHelper(expectedContext);
			_logger.LogDebug("Action_Overload_3_Method_Calls_ModuleRouteProvider(): helper → {Helper}", helper);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
			_logger.LogDebug("Action_Overload_3_Method_Calls_ModuleRouteProvider(): mockRouteProvider → {MockRouteProvider}", mockRouteProvider);
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            // Act
            helper.Action("foo", "bar", new RouteValueDictionary());

            // Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
			_logger.LogDebug("<<< Exiting Action_Overload_3_Method_Calls_ModuleRouteProvider()");
        }

        [Test]
        public void Action_Overload_4_Method_Calls_ModuleRouteProvider()
        {
			_logger.LogDebug(">>> Entering Action_Overload_4_Method_Calls_ModuleRouteProvider()");
            // Arrange
            var expectedContext = new ModuleInstanceContext();
			_logger.LogDebug("Action_Overload_4_Method_Calls_ModuleRouteProvider(): expectedContext → {ExpectedContext}", expectedContext);
            var helper = ArrangeHelper(expectedContext);
			_logger.LogDebug("Action_Overload_4_Method_Calls_ModuleRouteProvider(): helper → {Helper}", helper);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
			_logger.LogDebug("Action_Overload_4_Method_Calls_ModuleRouteProvider(): mockRouteProvider → {MockRouteProvider}", mockRouteProvider);
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            // Act
            helper.Action("foo", new { id = 5 });

            // Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
			_logger.LogDebug("<<< Exiting Action_Overload_4_Method_Calls_ModuleRouteProvider()");
        }

        [Test]
        public void Action_Overload_5_Method_Calls_ModuleRouteProvider()
        {
			_logger.LogDebug(">>> Entering Action_Overload_5_Method_Calls_ModuleRouteProvider()");
            // Arrange
            var expectedContext = new ModuleInstanceContext();
			_logger.LogDebug("Action_Overload_5_Method_Calls_ModuleRouteProvider(): expectedContext → {ExpectedContext}", expectedContext);
            var helper = ArrangeHelper(expectedContext);
			_logger.LogDebug("Action_Overload_5_Method_Calls_ModuleRouteProvider(): helper → {Helper}", helper);

            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
			_logger.LogDebug("Action_Overload_5_Method_Calls_ModuleRouteProvider(): mockRouteProvider → {MockRouteProvider}", mockRouteProvider);
            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            // Act
            helper.Action("foo", "bar", new { id = 5 });

            // Assert
            mockRouteProvider.Verify(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext));
			_logger.LogDebug("<<< Exiting Action_Overload_5_Method_Calls_ModuleRouteProvider()");
        }

        [Test]
        public void Content_Method_Calls_Returns_Correct_Url()
        {
			_logger.LogDebug(">>> Entering Content_Method_Calls_Returns_Correct_Url()");
            // Arrange
            var context = new ModuleInstanceContext();
			_logger.LogDebug("Content_Method_Calls_Returns_Correct_Url(): context → {Context}", context);
            var helper = ArrangeHelper(context, "http://foo.com/foo");
			_logger.LogDebug("Content_Method_Calls_Returns_Correct_Url(): helper → {Helper}", helper);
            string expectedResult = "/foo/test.css";

            // Act
            var url = helper.Content("~/test.css");
			_logger.LogDebug("Content_Method_Calls_Returns_Correct_Url(): url → {Url}", url);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(url, Is.Not.Null);
                Assert.That(url, Is.EqualTo(expectedResult));
            }
			_logger.LogDebug("<<< Exiting Content_Method_Calls_Returns_Correct_Url()");
        }

        [Test]
        public void IsLocalUrl_Method_Calls_Returns_Correct_Result()
        {
			_logger.LogDebug(">>> Entering IsLocalUrl_Method_Calls_Returns_Correct_Result()");
            // Arrange
            var context = new ModuleInstanceContext();
			_logger.LogDebug("IsLocalUrl_Method_Calls_Returns_Correct_Result(): context → {Context}", context);
            var helper = ArrangeHelper(context, "http://foo.com");
			_logger.LogDebug("IsLocalUrl_Method_Calls_Returns_Correct_Result(): helper → {Helper}", helper);

            // Act
            var withOuterUrl = helper.IsLocalUrl("http://dnnsoftware.com");
			_logger.LogDebug("IsLocalUrl_Method_Calls_Returns_Correct_Result(): withOuterUrl → {WithOuterUrl}", withOuterUrl);
            var withLocalUrl = helper.IsLocalUrl("~/foo/foo.html");
			_logger.LogDebug("IsLocalUrl_Method_Calls_Returns_Correct_Result(): withLocalUrl → {WithLocalUrl}", withLocalUrl);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(withOuterUrl, Is.False);
                Assert.That(withLocalUrl, Is.True);
            }
			_logger.LogDebug("<<< Exiting IsLocalUrl_Method_Calls_Returns_Correct_Result()");
        }

        [Test]
        public void GenerateUrl_Method_Passes_Correct_RouteValueCollection_To_ModuleRouteProvider()
        {
			_logger.LogDebug(">>> Entering GenerateUrl_Method_Passes_Correct_RouteValueCollection_To_ModuleRouteProvider()");
            // Arrange
            var expectedContext = new ModuleInstanceContext();
			_logger.LogDebug("GenerateUrl_Method_Passes_Correct_RouteValueCollection_To_ModuleRouteProvider(): expectedContext → {ExpectedContext}", expectedContext);
            var helper = ArrangeHelper(expectedContext);
			_logger.LogDebug("GenerateUrl_Method_Passes_Correct_RouteValueCollection_To_ModuleRouteProvider(): helper → {Helper}", helper);

            RouteValueDictionary routeValues = null;
            var mockRouteProvider = new Mock<ModuleRoutingProvider>();
			_logger.LogDebug("GenerateUrl_Method_Passes_Correct_RouteValueCollection_To_ModuleRouteProvider(): mockRouteProvider → {MockRouteProvider}", mockRouteProvider);
            mockRouteProvider.Setup(p => p.GenerateUrl(It.IsAny<RouteValueDictionary>(), expectedContext))
                                .Callback<RouteValueDictionary, ModuleInstanceContext>((r, c) => routeValues = r);

            ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(mockRouteProvider.Object);

            // Act
            helper.Action("foo", "bar", new { id = 5 });

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(routeValues.Values, Has.Count.EqualTo(3));
                Assert.That(routeValues, Does.ContainKey("action"));
                Assert.That(routeValues, Does.ContainKey("controller"));
                Assert.That(routeValues, Does.ContainKey("id"));
                Assert.That((string)routeValues["action"], Is.EqualTo("foo"));
                Assert.That((string)routeValues["controller"], Is.EqualTo("bar"));
                Assert.That((int)routeValues["id"], Is.EqualTo(5));
            }
			_logger.LogDebug("<<< Exiting GenerateUrl_Method_Passes_Correct_RouteValueCollection_To_ModuleRouteProvider()");
        }

        private static DnnUrlHelper ArrangeHelper(ModuleInstanceContext expectedContext, string url = null, bool isViewContext = true)
        {
            var mockController = new Mock<ControllerBase>();
            var mockDnnController = mockController.As<IDnnController>();

            var routeData = new RouteData();
            routeData.Values["controller"] = "bar";
            routeData.Values["action"] = "foo";
            var context = MockHelper.CreateMockControllerContext(url != null ? MockHelper.CreateMockHttpContext(url) : null, routeData);

            mockDnnController.Setup(c => c.ModuleContext).Returns(expectedContext);
            mockDnnController.Setup(c => c.ControllerContext).Returns(context);

            if (isViewContext)
            {
                var viewContext = new ViewContext { Controller = mockController.Object };

                if (!string.IsNullOrEmpty(url))
                {
                    viewContext.RequestContext = new RequestContext(MockHelper.CreateMockHttpContext(url), routeData);
                }

                return new DnnUrlHelper(viewContext);
            }

            var requestContext = new RequestContext(MockHelper.CreateMockHttpContext(url ?? "http://base/"), routeData);
            return new DnnUrlHelper(requestContext, mockDnnController.Object);
        }
    }
}
