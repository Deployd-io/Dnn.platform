// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.Api
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Hosting;
    using System.Web.Http.Routing;

    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Web.Api;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DnnHttpControllerSelectorTests
    {
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
			eventLogger.LogDebug(">>> Entering Setup()");
            this.serviceProvider = FakeServiceProvider.Setup();
			eventLogger.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
			eventLogger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			eventLogger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			eventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [TestCase("Sample")]
        [TestCase("SAmple")]
        [TestCase("SAMPLE")]
        public void SelectControllerIsCaseInsensitive(string controllerTypeName)
        {
			eventLogger.LogDebug(">>> Entering SelectControllerIsCaseInsensitive(controllerTypeName={ControllerTypeName})", controllerTypeName);
            const string expectedNamespace = "ExpectedNamespace";

            var configuration = new HttpConfiguration();
			eventLogger.LogDebug("SelectControllerIsCaseInsensitive(controllerTypeName={ControllerTypeName}): configuration → {Configuration}", controllerTypeName, configuration);
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
			eventLogger.LogDebug("SelectControllerIsCaseInsensitive(controllerTypeName={ControllerTypeName}): controllerTypeResolver → {ControllerTypeResolver}", controllerTypeName, controllerTypeResolver);
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            Type controllerType = GetMockControllerType("Sample", expectedNamespace);
			eventLogger.LogDebug("SelectControllerIsCaseInsensitive(controllerTypeName={ControllerTypeName}): controllerType → {ControllerType}", controllerTypeName, controllerType);
            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> { controllerType });

            var request = new HttpRequestMessage();
			eventLogger.LogDebug("SelectControllerIsCaseInsensitive(controllerTypeName={ControllerTypeName}): request → {Request}", controllerTypeName, request);
            IHttpRouteData routeData = GetRouteData();
			eventLogger.LogDebug("SelectControllerIsCaseInsensitive(controllerTypeName={ControllerTypeName}): routeData → {RouteData}", controllerTypeName, routeData);
            routeData.Values["controller"] = controllerTypeName;
            routeData.Route.SetNameSpaces(new[] { expectedNamespace });
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;

            var selector = new DnnHttpControllerSelector(configuration);
			eventLogger.LogDebug("SelectControllerIsCaseInsensitive(controllerTypeName={ControllerTypeName}): selector → {Selector}", controllerTypeName, selector);

            // Act
            HttpControllerDescriptor descriptor = selector.SelectController(request);
			eventLogger.LogDebug("SelectControllerIsCaseInsensitive(controllerTypeName={ControllerTypeName}): descriptor → {Descriptor}", controllerTypeName, descriptor);

            // Assert
            Assert.That(descriptor.ControllerType, Is.EqualTo(controllerType));
			eventLogger.LogDebug("<<< Exiting SelectControllerIsCaseInsensitive(controllerTypeName={ControllerTypeName})", controllerTypeName);
        }

        [Test]
        public void SelectControllerIgnoresControllersInUnexpectedNamespaces()
        {
			eventLogger.LogDebug(">>> Entering SelectControllerIgnoresControllersInUnexpectedNamespaces()");
            const string expectedNamespace = "ExpectedNamespace";
            const string unexpectedNamespace = "UnexpectedNamespace";
            const string controllerName = "Sample";

            var configuration = new HttpConfiguration();
			eventLogger.LogDebug("SelectControllerIgnoresControllersInUnexpectedNamespaces(): configuration → {Configuration}", configuration);
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
			eventLogger.LogDebug("SelectControllerIgnoresControllersInUnexpectedNamespaces(): controllerTypeResolver → {ControllerTypeResolver}", controllerTypeResolver);
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            Type controllerType = GetMockControllerType(controllerName, unexpectedNamespace);
			eventLogger.LogDebug("SelectControllerIgnoresControllersInUnexpectedNamespaces(): controllerType → {ControllerType}", controllerType);
            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> { controllerType });

            var request = new HttpRequestMessage();
			eventLogger.LogDebug("SelectControllerIgnoresControllersInUnexpectedNamespaces(): request → {Request}", request);
            IHttpRouteData routeData = GetRouteData();
			eventLogger.LogDebug("SelectControllerIgnoresControllersInUnexpectedNamespaces(): routeData → {RouteData}", routeData);
            routeData.Values["controller"] = controllerName;
            routeData.Route.SetNameSpaces(new[] { expectedNamespace });
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;

            var selector = new DnnHttpControllerSelector(configuration);
			eventLogger.LogDebug("SelectControllerIgnoresControllersInUnexpectedNamespaces(): selector → {Selector}", selector);

            // Act
            Assert.Throws<HttpResponseException>(() => selector.SelectController(request));
			eventLogger.LogDebug("<<< Exiting SelectControllerIgnoresControllersInUnexpectedNamespaces()");
        }

        [Test]
        public void SelectControllerThrowsOnAmgiuousControllers()
        {
			eventLogger.LogDebug(">>> Entering SelectControllerThrowsOnAmgiuousControllers()");
            const string expectedNamespace = "ExpectedNamespace";
            const string expectedNamespace2 = "ExpectedNamespace2";
            const string controllerName = "Sample";

            var configuration = new HttpConfiguration();
			eventLogger.LogDebug("SelectControllerThrowsOnAmgiuousControllers(): configuration → {Configuration}", configuration);
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
			eventLogger.LogDebug("SelectControllerThrowsOnAmgiuousControllers(): controllerTypeResolver → {ControllerTypeResolver}", controllerTypeResolver);
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            Type controllerType = GetMockControllerType(controllerName, expectedNamespace);
			eventLogger.LogDebug("SelectControllerThrowsOnAmgiuousControllers(): controllerType → {ControllerType}", controllerType);
            Type controllerType2 = GetMockControllerType(controllerName, expectedNamespace2);
			eventLogger.LogDebug("SelectControllerThrowsOnAmgiuousControllers(): controllerType2 → {ControllerType2}", controllerType2);
            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> { controllerType, controllerType2 });

            var request = new HttpRequestMessage();
			eventLogger.LogDebug("SelectControllerThrowsOnAmgiuousControllers(): request → {Request}", request);
            IHttpRouteData routeData = GetRouteData();
			eventLogger.LogDebug("SelectControllerThrowsOnAmgiuousControllers(): routeData → {RouteData}", routeData);
            routeData.Values["controller"] = controllerName;
            routeData.Route.SetNameSpaces(new[] { expectedNamespace, expectedNamespace2 });
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;

            var selector = new DnnHttpControllerSelector(configuration);
			eventLogger.LogDebug("SelectControllerThrowsOnAmgiuousControllers(): selector → {Selector}", selector);

            // Act
            Assert.Throws<HttpResponseException>(() => selector.SelectController(request));
			eventLogger.LogDebug("<<< Exiting SelectControllerThrowsOnAmgiuousControllers()");
        }

        private static IHttpRouteData GetRouteData()
        {
			eventLogger.LogDebug(">>> Entering GetRouteData()");
            var mockRoute = new Mock<IHttpRoute>();
            mockRoute.SetupGet(x => x.DataTokens).Returns(new Dictionary<string, object>());
			eventLogger.LogDebug("GetRouteData(): mockRoute → {MockRoute}", mockRoute);
            var routeData = new HttpRouteData(mockRoute.Object);
			eventLogger.LogDebug("GetRouteData(): routeData → {RouteData}", routeData);

			eventLogger.LogDebug("<<< Exiting GetRouteData()");
            return routeData;
        }

        private static Type GetMockControllerType(string controllerName, string @namespace)
        {
			eventLogger.LogDebug(">>> Entering GetMockControllerType(controllerName={ControllerName})", controllerName);
            var mockType = new Mock<Type>();

			eventLogger.LogDebug("GetMockControllerType(controllerName={ControllerName}): mockType → {MockType}", controllerName, mockType);
            mockType.Setup(t => t.Name).Returns(controllerName + "Controller");
            mockType.Setup(t => t.FullName).Returns(string.Format("{0}.{1}Controller", @namespace, controllerName));
			eventLogger.LogDebug("<<< Exiting GetMockControllerType(controllerName={ControllerName})", controllerName);
            return mockType.Object;
        }
    }
}
