// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.Api
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Routing;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.Internal.Reflection;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using ServicesRoutingManager = DotNetNuke.Web.Api.Internal.ServicesRoutingManager;

    [TestFixture]
    public class ServiceRoutingManagerTests
    {
        private static readonly List<string[]> EmptyStringArrays = [null, [], [string.Empty,], [null,],];

        private Mock<IPortalController> mockPortalController;
        private IPortalController portalController;
        private Mock<IPortalAliasService> mockPortalAliasService;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
			eventLogger.LogDebug(">>> Entering Setup()");
            FakeServiceRouteMapper.RegistrationCalls = 0;

			eventLogger.LogDebug("Setup(): FakeServiceRouteMapper.RegistrationCalls → {Arg0}", FakeServiceRouteMapper.RegistrationCalls);
            this.mockPortalController = new Mock<IPortalController>();
			eventLogger.LogDebug("Setup(): mockPortalController → {MockPortalController}", mockPortalController);
            this.portalController = this.mockPortalController.Object;
			eventLogger.LogDebug("Setup(): portalController → {PortalController}", portalController);
			eventLogger.LogDebug("Setup(): this.portalController → {Arg0}", this.portalController);
            PortalController.SetTestableInstance(this.portalController);

            this.mockPortalAliasService = new Mock<IPortalAliasService>();
			eventLogger.LogDebug("Setup(): mockPortalAliasService → {MockPortalAliasService}", mockPortalAliasService);
            this.mockPortalAliasService.As<IPortalAliasController>();

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					eventLogger.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(this.mockPortalAliasService.Object);
                    services.AddSingleton((IPortalAliasController)this.mockPortalAliasService.Object);
                    services.AddSingleton(this.mockPortalController.Object);
                });
				eventLogger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			eventLogger.LogDebug(">>> Entering TearDown()");
            PortalController.ClearInstance();

            this.serviceProvider.Dispose();
            this.mockPortalAliasService = null;
			eventLogger.LogDebug("TearDown(): this.mockPortalAliasService → {Arg0}", this.mockPortalAliasService);
			eventLogger.LogDebug("TearDown(): mockPortalAliasService → {MockPortalAliasService}", mockPortalAliasService);
			eventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void LocatesAllServiceRouteMappers()
        {
			eventLogger.LogDebug(">>> Entering LocatesAllServiceRouteMappers()");
            var assemblyLocator = new Mock<IAssemblyLocator>();

			eventLogger.LogDebug("LocatesAllServiceRouteMappers(): assemblyLocator → {AssemblyLocator}", assemblyLocator);
            // including the assembly with object ensures that the assignability is done correctly
            var assembliesToReflect = new IAssembly[2];
            assembliesToReflect[0] = new AssemblyWrapper(this.GetType().Assembly);
            assembliesToReflect[1] = new AssemblyWrapper(typeof(object).Assembly);

            assemblyLocator.Setup(x => x.Assemblies).Returns(assembliesToReflect);

            var locator = new TypeLocator { AssemblyLocator = assemblyLocator.Object };

            var types = locator.GetAllMatchingTypes(ServicesRoutingManager.IsValidServiceRouteMapper).ToList();
			eventLogger.LogDebug("LocatesAllServiceRouteMappers(): types → {Types}", types);

            // if new ServiceRouteMapper classes are added to the assembly they will likely need to be added here
            Assert.That(
                types,
                Is.EquivalentTo(
                [
                    typeof(FakeServiceRouteMapper),
                    typeof(ReflectedServiceRouteMappers.EmbeddedServiceRouteMapper),
                    typeof(ExceptionOnCreateInstanceServiceRouteMapper),
                    typeof(ExceptionOnRegisterServiceRouteMapper)
                ]));
				eventLogger.LogDebug("<<< Exiting LocatesAllServiceRouteMappers()");
        }

        [Test]
        public void NameSpaceRequiredOnMapRouteCalls([ValueSource(nameof(EmptyStringArrays))] string[] namespaces)
        {
			eventLogger.LogDebug(">>> Entering NameSpaceRequiredOnMapRouteCalls(namespaces={Namespaces})", namespaces);
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, this.mockPortalAliasService.Object, new RouteCollection());

			eventLogger.LogDebug("NameSpaceRequiredOnMapRouteCalls(namespaces={Namespaces}): srm → {Srm}", namespaces, srm);
            Assert.Throws<ArgumentException>(() => srm.MapHttpRoute("usm", "default", "url", null, namespaces));
			eventLogger.LogDebug("<<< Exiting NameSpaceRequiredOnMapRouteCalls(namespaces={Namespaces})", namespaces);
        }

        [Test]
        public void RegisterRoutesIsCalledOnAllServiceRouteMappersEvenWhenSomeThrowExceptions()
        {
			eventLogger.LogDebug(">>> Entering RegisterRoutesIsCalledOnAllServiceRouteMappersEvenWhenSomeThrowExceptions()");
            FakeServiceRouteMapper.RegistrationCalls = 0;
            var assembly = new Mock<IAssembly>();
			eventLogger.LogDebug("RegisterRoutesIsCalledOnAllServiceRouteMappersEvenWhenSomeThrowExceptions(): assembly → {Assembly}", assembly);
            assembly.Setup(x => x.GetTypes())
                .Returns(
                [
                    typeof(ExceptionOnRegisterServiceRouteMapper),
                    typeof(ExceptionOnCreateInstanceServiceRouteMapper),
                    typeof(FakeServiceRouteMapper)
                ]);
				eventLogger.LogDebug("RegisterRoutesIsCalledOnAllServiceRouteMappersEvenWhenSomeThrowExceptions(): FakeServiceRouteMapper.RegistrationCalls → {Arg0}", FakeServiceRouteMapper.RegistrationCalls);
            var al = new Mock<IAssemblyLocator>();
			eventLogger.LogDebug("RegisterRoutesIsCalledOnAllServiceRouteMappersEvenWhenSomeThrowExceptions(): al → {Al}", al);
            al.Setup(x => x.Assemblies).Returns([assembly.Object,]);
            var tl = new TypeLocator { AssemblyLocator = al.Object };
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, this.mockPortalAliasService.Object, new RouteCollection()) { TypeLocator = tl, };
			eventLogger.LogDebug("RegisterRoutesIsCalledOnAllServiceRouteMappersEvenWhenSomeThrowExceptions(): srm → {Srm}", srm);

            srm.RegisterRoutes();

            Assert.That(FakeServiceRouteMapper.RegistrationCalls, Is.EqualTo(1));
			eventLogger.LogDebug("<<< Exiting RegisterRoutesIsCalledOnAllServiceRouteMappersEvenWhenSomeThrowExceptions()");
        }

        [Test]
        public void RegisterRoutesIsCalledOnServiceRouteMappers()
        {
			eventLogger.LogDebug(">>> Entering RegisterRoutesIsCalledOnServiceRouteMappers()");
            FakeServiceRouteMapper.RegistrationCalls = 0;
            var assembly = new Mock<IAssembly>();
			eventLogger.LogDebug("RegisterRoutesIsCalledOnServiceRouteMappers(): assembly → {Assembly}", assembly);
			eventLogger.LogDebug("RegisterRoutesIsCalledOnServiceRouteMappers(): FakeServiceRouteMapper.RegistrationCalls → {Arg0}", FakeServiceRouteMapper.RegistrationCalls);
            assembly.Setup(x => x.GetTypes()).Returns([typeof(FakeServiceRouteMapper),]);
            var al = new Mock<IAssemblyLocator>();
			eventLogger.LogDebug("RegisterRoutesIsCalledOnServiceRouteMappers(): al → {Al}", al);
            al.Setup(x => x.Assemblies).Returns([assembly.Object,]);
            var tl = new TypeLocator { AssemblyLocator = al.Object };
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, this.mockPortalAliasService.Object, new RouteCollection()) { TypeLocator = tl, };
			eventLogger.LogDebug("RegisterRoutesIsCalledOnServiceRouteMappers(): srm → {Srm}", srm);

            srm.RegisterRoutes();

            Assert.That(FakeServiceRouteMapper.RegistrationCalls, Is.EqualTo(1));
			eventLogger.LogDebug("<<< Exiting RegisterRoutesIsCalledOnServiceRouteMappers()");
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void UniqueNameRequiredOnMapRouteCalls(string uniqueName)
        {
			eventLogger.LogDebug(">>> Entering UniqueNameRequiredOnMapRouteCalls(uniqueName={UniqueName})", uniqueName);
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, this.mockPortalAliasService.Object, new RouteCollection());

			eventLogger.LogDebug("UniqueNameRequiredOnMapRouteCalls(uniqueName={UniqueName}): srm → {Srm}", uniqueName, srm);
            Assert.Throws<ArgumentException>(() => srm.MapHttpRoute(uniqueName, "default", "url", null, ["foo",]));
			eventLogger.LogDebug("<<< Exiting UniqueNameRequiredOnMapRouteCalls(uniqueName={UniqueName})", uniqueName);
        }

        [Test]
        public void UrlCanStartWithSlash()
        {
			eventLogger.LogDebug(">>> Entering UrlCanStartWithSlash()");
            // Arrange
            this.mockPortalController.Setup(x => x.GetPortals()).Returns(new ArrayList());

            // Act
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, this.mockPortalAliasService.Object, new RouteCollection());
			eventLogger.LogDebug("UrlCanStartWithSlash(): srm → {Srm}", srm);

            // Assert
            Assert.DoesNotThrow(() => srm.MapHttpRoute("name", "default", "/url", null, ["foo",]));
			eventLogger.LogDebug("<<< Exiting UrlCanStartWithSlash()");
        }

        [Test]
        public void NameIsInsertedInRouteDataTokens()
        {
			eventLogger.LogDebug(">>> Entering NameIsInsertedInRouteDataTokens()");
            // Arrange
            var portalInfo = new ArrayList { new PortalInfo { PortalID = 0 } };
            this.mockPortalController.Setup(x => x.GetPortals()).Returns(portalInfo);

            this.mockPortalAliasService.Setup(x => x.GetPortalAliasesByPortalId(0)).Returns([new PortalAliasInfo { HTTPAlias = "www.foo.com", },]);
            this.mockPortalAliasService.As<IPortalAliasController>().Setup(x => x.GetPortalAliasesByPortalId(0)).Returns([new PortalAliasInfo { HTTPAlias = "www.foo.com", },]);

            var routeCollection = new RouteCollection();
			eventLogger.LogDebug("NameIsInsertedInRouteDataTokens(): routeCollection → {RouteCollection}", routeCollection);
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, this.mockPortalAliasService.Object, routeCollection);
			eventLogger.LogDebug("NameIsInsertedInRouteDataTokens(): srm → {Srm}", srm);

            // Act
            srm.MapHttpRoute("folder", "default", "url", ["foo",]);

            // Assert
            var route = (Route)routeCollection[0];
			eventLogger.LogDebug("NameIsInsertedInRouteDataTokens(): route → {Route}", route);
            Assert.That(route.DataTokens["Name"], Is.EqualTo("folder-default-0"));
			eventLogger.LogDebug("<<< Exiting NameIsInsertedInRouteDataTokens()");
        }

        [Test]
        public void TwoRoutesOnTheSameFolderHaveSimilarNames()
        {
			eventLogger.LogDebug(">>> Entering TwoRoutesOnTheSameFolderHaveSimilarNames()");
            // Arrange
            var portalInfo = new ArrayList { new PortalInfo { PortalID = 0 } };
            this.mockPortalController.Setup(x => x.GetPortals()).Returns(portalInfo);

            this.mockPortalAliasService.Setup(x => x.GetPortalAliasesByPortalId(0)).Returns([new PortalAliasInfo { HTTPAlias = "www.foo.com", },]);
            this.mockPortalAliasService.As<IPortalAliasController>().Setup(x => x.GetPortalAliasesByPortalId(0)).Returns([new PortalAliasInfo { HTTPAlias = "www.foo.com", },]);

            var routeCollection = new RouteCollection();
			eventLogger.LogDebug("TwoRoutesOnTheSameFolderHaveSimilarNames(): routeCollection → {RouteCollection}", routeCollection);
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, this.mockPortalAliasService.Object, routeCollection);
			eventLogger.LogDebug("TwoRoutesOnTheSameFolderHaveSimilarNames(): srm → {Srm}", srm);

            // Act
            srm.MapHttpRoute("folder", "default", "url", ["foo",]);
            srm.MapHttpRoute("folder", "another", "alt/url", ["foo",]);

            // Assert
            var route = (Route)routeCollection[0];
			eventLogger.LogDebug("TwoRoutesOnTheSameFolderHaveSimilarNames(): route → {Route}", route);
            Assert.That(route.DataTokens["Name"], Is.EqualTo("folder-default-0"));
            route = (Route)routeCollection[1];
            Assert.That(route.DataTokens["Name"], Is.EqualTo("folder-default-0-old"));
			eventLogger.LogDebug("<<< Exiting TwoRoutesOnTheSameFolderHaveSimilarNames()");
        }

        [Test]
        public void RoutesShouldHaveBackwardCompability()
        {
			eventLogger.LogDebug(">>> Entering RoutesShouldHaveBackwardCompability()");
            // Arrange
            var portalInfo = new ArrayList { new PortalInfo { PortalID = 0 } };
            this.mockPortalController.Setup(x => x.GetPortals()).Returns(portalInfo);

            this.mockPortalAliasService.Setup(x => x.GetPortalAliasesByPortalId(0)).Returns([new PortalAliasInfo { HTTPAlias = "www.foo.com" }]);
            this.mockPortalAliasService.As<IPortalAliasController>().Setup(x => x.GetPortalAliasesByPortalId(0)).Returns([new PortalAliasInfo { HTTPAlias = "www.foo.com" }]);

            var routeCollection = new RouteCollection();
			eventLogger.LogDebug("RoutesShouldHaveBackwardCompability(): routeCollection → {RouteCollection}", routeCollection);
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, this.mockPortalAliasService.Object, routeCollection);
			eventLogger.LogDebug("RoutesShouldHaveBackwardCompability(): srm → {Srm}", srm);

            // Act
            srm.MapHttpRoute("folder", "default", "url", ["foo"]);

            // Assert
            var route = (Route)routeCollection[0];
			eventLogger.LogDebug("RoutesShouldHaveBackwardCompability(): route → {Route}", route);
            Assert.That(route.DataTokens["Name"], Is.EqualTo("folder-default-0"));
            route = (Route)routeCollection[1];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(route.DataTokens["Name"], Is.EqualTo("folder-default-0-old"));
                Assert.That(route.Url, Does.StartWith("DesktopModules"));
            }
			eventLogger.LogDebug("<<< Exiting RoutesShouldHaveBackwardCompability()");
        }
    }
}
