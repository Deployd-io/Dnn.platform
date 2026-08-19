// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.Api
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class PortalAliasRouteManagerTests
    {
        private Mock<IPortalAliasService> mockPortalAliasService;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			eventLogger.LogDebug(">>> Entering SetUp()");
            this.mockPortalAliasService = new Mock<IPortalAliasService>();
            this.mockPortalAliasService.As<IPortalAliasController>();
			eventLogger.LogDebug("SetUp(): mockPortalAliasService → {MockPortalAliasService}", mockPortalAliasService);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					eventLogger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(this.mockPortalAliasService.Object);
                });
				eventLogger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			eventLogger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();

            this.mockPortalAliasService = null;
			eventLogger.LogDebug("TearDown(): mockPortalAliasService → {MockPortalAliasService}", mockPortalAliasService);
			eventLogger.LogDebug("TearDown(): this.mockPortalAliasService → {Arg0}", this.mockPortalAliasService);

            PortalController.ClearInstance();
            TestableGlobals.ClearInstance();
			eventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        [TestCase("mfn", "url", 0, "API/mfn/url")]
        [TestCase("mfn", "url", 1, "{prefix0}/API/mfn/url")]
        [TestCase("mfn", "url", 2, "{prefix0}/{prefix1}/API/mfn/url")]
        [TestCase("fee/foo", "{contoller}/{action}/{id}", 4, "{prefix0}/{prefix1}/{prefix2}/{prefix3}/API/fee/foo/{contoller}/{action}/{id}")]
        public void GetRouteUrl(string moduleFolderName, string url, int count, string expected)
        {
			eventLogger.LogDebug(">>> Entering GetRouteUrl(moduleFolderName={ModuleFolderName},url={Url},count={Count},expected={Expected})", moduleFolderName, url, count, expected);
            // Arrange

            // Act
            string result = new PortalAliasRouteManager().GetRouteUrl(moduleFolderName, url, count);
			eventLogger.LogDebug("GetRouteUrl(moduleFolderName={ModuleFolderName},url={Url},count={Count},expected={Expected}): result → {Result}", moduleFolderName, url, count, expected, result);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
			eventLogger.LogDebug("<<< Exiting GetRouteUrl(moduleFolderName={ModuleFolderName},url={Url},count={Count},expected={Expected})", moduleFolderName, url, count, expected);
        }

        [Test]
        [TestCase("", 0, typeof(ArgumentException), "moduleFolderName should be required")]
        [TestCase("name", -1, typeof(ArgumentOutOfRangeException), "count should be >= 0")]
        public void GetRouteUrlThrowsOnBadArguments(string moduleFolderName, int count, Type expectedException, string message)
        {
			eventLogger.LogDebug(">>> Entering GetRouteUrlThrowsOnBadArguments(moduleFolderName={ModuleFolderName},count={Count},expectedException={ExpectedException},message={Message})", moduleFolderName, count, expectedException, message);
            // Arrange

            // Act
            try
            {
                new PortalAliasRouteManager().GetRouteUrl(moduleFolderName, "url", count);
            }
            catch (Exception e)
            {
				eventLogger.LogError("Exception in GetRouteUrlThrowsOnBadArguments(moduleFolderName={ModuleFolderName},count={Count},expectedException={ExpectedException},message={Message}): {Arg4}", moduleFolderName, count, expectedException, message, e.Message);
                if (e.GetType() == expectedException)
                {
                    Assert.Pass();
                    return;
                }
            }

            // Assert
            Assert.Fail(message);
			eventLogger.LogDebug("<<< Exiting GetRouteUrlThrowsOnBadArguments(moduleFolderName={ModuleFolderName},count={Count},expectedException={ExpectedException},message={Message})", moduleFolderName, count, expectedException, message);
        }

        [Test]
        public void ParentPortalOnVirtualDirReturnsAnEmptyPrefix()
        {
			eventLogger.LogDebug(">>> Entering ParentPortalOnVirtualDirReturnsAnEmptyPrefix()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
			eventLogger.LogDebug("ParentPortalOnVirtualDirReturnsAnEmptyPrefix(): mockPortalController → {MockPortalController}", mockPortalController);
            var portals = new ArrayList { new PortalInfo { PortalID = 1 } };
            mockPortalController.Setup(x => x.GetPortals()).Returns(portals);
            PortalController.SetTestableInstance(mockPortalController.Object);

            this.mockPortalAliasService.Setup(x => x.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(new[]
                {
                    new PortalAliasInfo { HTTPAlias = "valid.lvh.me/vdir" },
                });
            this.mockPortalAliasService.As<IPortalAliasController>().Setup(x => x.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(new[]
                {
                    new PortalAliasInfo { HTTPAlias = "valid.lvh.me/vdir" },
                });

            var mockGlobals = new Mock<IGlobals>();
			eventLogger.LogDebug("ParentPortalOnVirtualDirReturnsAnEmptyPrefix(): mockGlobals → {MockGlobals}", mockGlobals);
            mockGlobals.Setup(x => x.ApplicationPath).Returns("/vdir");
            TestableGlobals.SetTestableInstance(mockGlobals.Object);

            // Act
            List<int> prefixes = new PortalAliasRouteManager().GetRoutePrefixCounts().ToList();
			eventLogger.LogDebug("ParentPortalOnVirtualDirReturnsAnEmptyPrefix(): prefixes → {Prefixes}", prefixes);

            // Assert
            Assert.That(prefixes, Is.EquivalentTo(new[] { 0 }));
			eventLogger.LogDebug("<<< Exiting ParentPortalOnVirtualDirReturnsAnEmptyPrefix()");
        }

        [Test]
        public void SingleParentPortalReturnsAnEmptyPrefix()
        {
			eventLogger.LogDebug(">>> Entering SingleParentPortalReturnsAnEmptyPrefix()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
			eventLogger.LogDebug("SingleParentPortalReturnsAnEmptyPrefix(): mockPortalController → {MockPortalController}", mockPortalController);
            var portals = new ArrayList { new PortalInfo { PortalID = 1 } };
            mockPortalController.Setup(x => x.GetPortals()).Returns(portals);
            PortalController.SetTestableInstance(mockPortalController.Object);

            this.mockPortalAliasService.Setup(x => x.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(new[]
                {
                    new PortalAliasInfo { HTTPAlias = "valid.lvh.me" },
                });
            this.mockPortalAliasService.As<IPortalAliasController>().Setup(x => x.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(new[]
                {
                    new PortalAliasInfo { HTTPAlias = "valid.lvh.me" },
                });

            var mockGlobals = new Mock<IGlobals>();
			eventLogger.LogDebug("SingleParentPortalReturnsAnEmptyPrefix(): mockGlobals → {MockGlobals}", mockGlobals);
            mockGlobals.Setup(x => x.ApplicationPath).Returns(string.Empty);
            TestableGlobals.SetTestableInstance(mockGlobals.Object);

            // Act
            List<int> prefixes = new PortalAliasRouteManager().GetRoutePrefixCounts().ToList();
			eventLogger.LogDebug("SingleParentPortalReturnsAnEmptyPrefix(): prefixes → {Prefixes}", prefixes);

            // Assert
            Assert.That(prefixes, Is.EquivalentTo(new[] { 0 }));
			eventLogger.LogDebug("<<< Exiting SingleParentPortalReturnsAnEmptyPrefix()");
        }

        [Test]
        public void PrefixCountsAreCached()
        {
			eventLogger.LogDebug(">>> Entering PrefixCountsAreCached()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
			eventLogger.LogDebug("PrefixCountsAreCached(): mockPortalController → {MockPortalController}", mockPortalController);
            var portals = new ArrayList { new PortalInfo { PortalID = 1 } };
            mockPortalController.Setup(x => x.GetPortals()).Returns(portals);
            PortalController.SetTestableInstance(mockPortalController.Object);

            this.mockPortalAliasService.Setup(x => x.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(new[] { new PortalAliasInfo { HTTPAlias = "valid.lvh.me" } });
            this.mockPortalAliasService.As<IPortalAliasController>().Setup(x => x.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(new[] { new PortalAliasInfo { HTTPAlias = "valid.lvh.me" } });

            var mockGlobals = new Mock<IGlobals>();
			eventLogger.LogDebug("PrefixCountsAreCached(): mockGlobals → {MockGlobals}", mockGlobals);
            mockGlobals.Setup(x => x.ApplicationPath).Returns(string.Empty);
            TestableGlobals.SetTestableInstance(mockGlobals.Object);

            // Act
            var parm = new PortalAliasRouteManager();
			eventLogger.LogDebug("PrefixCountsAreCached(): parm → {Parm}", parm);
            parm.GetRoutePrefixCounts();
            parm.GetRoutePrefixCounts();

            // Assert
            mockPortalController.Verify(x => x.GetPortals(), Times.Once());
			eventLogger.LogDebug("<<< Exiting PrefixCountsAreCached()");
        }

        [Test]
        public void PrefixCountsCacheCanBeCleared()
        {
			eventLogger.LogDebug(">>> Entering PrefixCountsCacheCanBeCleared()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
			eventLogger.LogDebug("PrefixCountsCacheCanBeCleared(): mockPortalController → {MockPortalController}", mockPortalController);
            var portals = new ArrayList { new PortalInfo { PortalID = 1 } };
            mockPortalController.Setup(x => x.GetPortals()).Returns(portals);
            PortalController.SetTestableInstance(mockPortalController.Object);

            this.mockPortalAliasService.Setup(x => x.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(new[] { new PortalAliasInfo { HTTPAlias = "valid.lvh.me" } });
            this.mockPortalAliasService.As<IPortalAliasController>().Setup(x => x.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(new[] { new PortalAliasInfo { HTTPAlias = "valid.lvh.me" } });

            var mockGlobals = new Mock<IGlobals>();
			eventLogger.LogDebug("PrefixCountsCacheCanBeCleared(): mockGlobals → {MockGlobals}", mockGlobals);
            mockGlobals.Setup(x => x.ApplicationPath).Returns(string.Empty);
            TestableGlobals.SetTestableInstance(mockGlobals.Object);

            // Act
            var parm = new PortalAliasRouteManager();
			eventLogger.LogDebug("PrefixCountsCacheCanBeCleared(): parm → {Parm}", parm);
            parm.GetRoutePrefixCounts();
            parm.ClearCachedData();
            parm.GetRoutePrefixCounts();

            // Assert
            mockPortalController.Verify(x => x.GetPortals(), Times.Exactly(2));
			eventLogger.LogDebug("<<< Exiting PrefixCountsCacheCanBeCleared()");
        }

        [Test]
        public void VirtralDirWithChildPortalHasABlankAndASinglePrefix()
        {
			eventLogger.LogDebug(">>> Entering VirtralDirWithChildPortalHasABlankAndASinglePrefix()");
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
			eventLogger.LogDebug("VirtralDirWithChildPortalHasABlankAndASinglePrefix(): mockPortalController → {MockPortalController}", mockPortalController);
            var portals = new ArrayList { new PortalInfo { PortalID = 1 } };
            mockPortalController.Setup(x => x.GetPortals()).Returns(portals);
            PortalController.SetTestableInstance(mockPortalController.Object);

            this.mockPortalAliasService.Setup(x => x.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(new[]
                {
                    new PortalAliasInfo { HTTPAlias = "valid.lvh.me/vdir" },
                    new PortalAliasInfo { HTTPAlias = "valid.lvh.me/vdir/child" },
                });
            this.mockPortalAliasService.As<IPortalAliasController>()
                .Setup(x => x.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(new[]
                {
                    new PortalAliasInfo { HTTPAlias = "valid.lvh.me/vdir" },
                    new PortalAliasInfo { HTTPAlias = "valid.lvh.me/vdir/child" },
                });

            var mockGlobals = new Mock<IGlobals>();
			eventLogger.LogDebug("VirtralDirWithChildPortalHasABlankAndASinglePrefix(): mockGlobals → {MockGlobals}", mockGlobals);
            mockGlobals.Setup(x => x.ApplicationPath).Returns("/vdir");
            TestableGlobals.SetTestableInstance(mockGlobals.Object);

            // Act
            List<int> prefixes = new PortalAliasRouteManager().GetRoutePrefixCounts().ToList();
			eventLogger.LogDebug("VirtralDirWithChildPortalHasABlankAndASinglePrefix(): prefixes → {Prefixes}", prefixes);

            // Assert
            Assert.That(prefixes, Is.EqualTo(new[] { 1, 0 }).AsCollection);
			eventLogger.LogDebug("<<< Exiting VirtralDirWithChildPortalHasABlankAndASinglePrefix()");
        }

        [Test]
        [TestCase("mfn", "name", 0, "mfn-name-0")]
        [TestCase("mfn", "", 1, "mfn--1")]
        [TestCase("first", "second", 99, "first-second-99")]
        public void GetRouteNameHashesNameInCorrectFormat(string moduleFolderName, string routeName, int count, string expected)
        {
			eventLogger.LogDebug(">>> Entering GetRouteNameHashesNameInCorrectFormat(moduleFolderName={ModuleFolderName},routeName={RouteName},count={Count},expected={Expected})", moduleFolderName, routeName, count, expected);
            // Arrange

            // Act
            var result = new PortalAliasRouteManager().GetRouteName(moduleFolderName, routeName, count);
			eventLogger.LogDebug("GetRouteNameHashesNameInCorrectFormat(moduleFolderName={ModuleFolderName},routeName={RouteName},count={Count},expected={Expected}): result → {Result}", moduleFolderName, routeName, count, expected, result);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
			eventLogger.LogDebug("<<< Exiting GetRouteNameHashesNameInCorrectFormat(moduleFolderName={ModuleFolderName},routeName={RouteName},count={Count},expected={Expected})", moduleFolderName, routeName, count, expected);
        }

        [Test]
        [TestCase("mfn", "name", "ce.lvh.me", "mfn-name-0")]
        [TestCase("mfn", "", "ce.lvh.me/child", "mfn--1")]
        [TestCase("first", "second", "ce.lvh.me/child1/child2/child3/child4/child5", "first-second-5")]
        public void GetRouteNameWithPortalAliasInfoHashesNameInCorrectFormat(string moduleFolderName, string routeName, string httpAlias, string expected)
        {
			eventLogger.LogDebug(">>> Entering GetRouteNameWithPortalAliasInfoHashesNameInCorrectFormat(moduleFolderName={ModuleFolderName},routeName={RouteName},httpAlias={HttpAlias},expected={Expected})", moduleFolderName, routeName, httpAlias, expected);
            // Arrange

            // Act
            var result = new PortalAliasRouteManager().GetRouteName(moduleFolderName, routeName, new PortalAliasInfo { HTTPAlias = httpAlias });
			eventLogger.LogDebug("GetRouteNameWithPortalAliasInfoHashesNameInCorrectFormat(moduleFolderName={ModuleFolderName},routeName={RouteName},httpAlias={HttpAlias},expected={Expected}): result → {Result}", moduleFolderName, routeName, httpAlias, expected, result);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
			eventLogger.LogDebug("<<< Exiting GetRouteNameWithPortalAliasInfoHashesNameInCorrectFormat(moduleFolderName={ModuleFolderName},routeName={RouteName},httpAlias={HttpAlias},expected={Expected})", moduleFolderName, routeName, httpAlias, expected);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void GetRouteNameThrowsOnEmptyModuleFolderName(string moduleFolderName)
        {
			eventLogger.LogDebug(">>> Entering GetRouteNameThrowsOnEmptyModuleFolderName(moduleFolderName={ModuleFolderName})", moduleFolderName);
            // Arrange

            // Act
            Assert.Throws<ArgumentException>(() => new PortalAliasRouteManager().GetRouteName(moduleFolderName, string.Empty, 0));

            // Assert
			eventLogger.LogDebug("<<< Exiting GetRouteNameThrowsOnEmptyModuleFolderName(moduleFolderName={ModuleFolderName})", moduleFolderName);
        }

        [Test]
        public void GetRouteNameThrowsOnCountLessThan0()
        {
			eventLogger.LogDebug(">>> Entering GetRouteNameThrowsOnCountLessThan0()");
            // Arrange

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() => new PortalAliasRouteManager().GetRouteName("foo", string.Empty, -1));

            // Assert
			eventLogger.LogDebug("<<< Exiting GetRouteNameThrowsOnCountLessThan0()");
        }

        [Test]
        public void GetAllRouteValuesWorksWithNullRouteValues()
        {
			eventLogger.LogDebug(">>> Entering GetAllRouteValuesWorksWithNullRouteValues()");
            // Arrange

            // Act
            new PortalAliasRouteManager().GetAllRouteValues(new PortalAliasInfo { HTTPAlias = string.Empty }, null);

            // Assert
            Assert.Pass();
			eventLogger.LogDebug("<<< Exiting GetAllRouteValuesWorksWithNullRouteValues()");
        }

        [Test]
        public void GetAllRouteValuesPreservesPassedInRouteValues()
        {
			eventLogger.LogDebug(">>> Entering GetAllRouteValuesPreservesPassedInRouteValues()");
            // Arrange

            // Act
            var result = new PortalAliasRouteManager().GetAllRouteValues(
                new PortalAliasInfo { HTTPAlias = string.Empty },
                new { value1 = 1, value2 = 2 });

            // Assert
            var expected = new Dictionary<string, object> { { "value1", 1 }, { "value2", 2 } };
			eventLogger.LogDebug("GetAllRouteValuesPreservesPassedInRouteValues(): expected → {Expected}", expected);
            Assert.That(result, Is.EquivalentTo(expected));
			eventLogger.LogDebug("<<< Exiting GetAllRouteValuesPreservesPassedInRouteValues()");
        }

        [Test]
        public void GetAllRouteValuesExtractsChildPortalParams()
        {
			eventLogger.LogDebug(">>> Entering GetAllRouteValuesExtractsChildPortalParams()");
            // Arrange

            // Act
            var result =
                new PortalAliasRouteManager().GetAllRouteValues(
                    new PortalAliasInfo { HTTPAlias = "ce.lvh.me/child" },
                    null);

            // Assert
            var expected = new Dictionary<string, object> { { "prefix0", "child" } };
			eventLogger.LogDebug("GetAllRouteValuesExtractsChildPortalParams(): expected → {Expected}", expected);
            Assert.That(result, Is.EquivalentTo(expected));
			eventLogger.LogDebug("<<< Exiting GetAllRouteValuesExtractsChildPortalParams()");
        }

        [Test]
        public void GetAllRouteValuesExtractsManyChildPortalParamsAndPreservesRouteValues()
        {
			eventLogger.LogDebug(">>> Entering GetAllRouteValuesExtractsManyChildPortalParamsAndPreservesRouteValues()");
            // Arrange

            // Act
            var result =
                new PortalAliasRouteManager().GetAllRouteValues(
                    new PortalAliasInfo { HTTPAlias = "ce.lvh.me/child0/child1/child2/child3" },
                    new { value1 = 1, value2 = 2 });

            // Assert
            var expected = new Dictionary<string, object> { { "prefix0", "child0" }, { "prefix1", "child1" }, { "prefix2", "child2" }, { "prefix3", "child3" }, { "value1", 1 }, { "value2", 2 } };
			eventLogger.LogDebug("GetAllRouteValuesExtractsManyChildPortalParamsAndPreservesRouteValues(): expected → {Expected}", expected);
            Assert.That(result, Is.EquivalentTo(expected));
			eventLogger.LogDebug("<<< Exiting GetAllRouteValuesExtractsManyChildPortalParamsAndPreservesRouteValues()");
        }
    }
}
