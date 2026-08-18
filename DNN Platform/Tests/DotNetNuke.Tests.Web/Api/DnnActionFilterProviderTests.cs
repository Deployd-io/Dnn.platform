// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.Api
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class DnnActionFilterProviderTests
    {
        [SetUp]
        public void SetUp()
        {
			eventLogger.LogDebug(">>> Entering SetUp()");
            CBO.SetTestableInstance(new MockCBO());
			eventLogger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			eventLogger.LogDebug(">>> Entering TearDown()");
            CBO.ClearInstance();
			eventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent()
        {
			eventLogger.LogDebug(">>> Entering RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent()");
            // Arrange
            var adMock = new Mock<HttpActionDescriptor>();
			eventLogger.LogDebug("RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent(): adMock → {AdMock}", adMock);
            adMock.Setup(ad => ad.GetFilters()).Returns(new Collection<IFilter>());

            var cdMock = new Mock<HttpControllerDescriptor>();
			eventLogger.LogDebug("RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent(): cdMock → {CdMock}", cdMock);
            cdMock.Setup(cd => cd.GetFilters()).Returns(new Collection<IFilter>());

            HttpActionDescriptor actionDescriptor = adMock.Object;
            actionDescriptor.ControllerDescriptor = cdMock.Object;
			eventLogger.LogDebug("RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent(): actionDescriptor.ControllerDescriptor → {Arg0}", actionDescriptor.ControllerDescriptor);

            var configuration = new HttpConfiguration();
			eventLogger.LogDebug("RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent(): configuration → {Configuration}", configuration);

            // Act
            var filterProvider = new DnnActionFilterProvider(Mock.Of<IServiceProvider>());
			eventLogger.LogDebug("RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent(): filterProvider → {FilterProvider}", filterProvider);
            var filters = filterProvider.GetFilters(configuration, actionDescriptor).ToList();
			eventLogger.LogDebug("RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent(): filters → {Filters}", filters);

            // Assert
            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters.First().Instance, Is.InstanceOf<RequireHostAttribute>());
			eventLogger.LogDebug("<<< Exiting RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent()");
        }

        [Test]
        public void RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent()
        {
			eventLogger.LogDebug(">>> Entering RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent()");
            // Arrange
            var adMock = new Mock<HttpActionDescriptor>();
			eventLogger.LogDebug("RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent(): adMock → {AdMock}", adMock);
            adMock.Setup(ad => ad.GetFilters()).Returns(new Collection<IFilter>(new[] { new DnnAuthorizeAttribute() }));

            var cdMock = new Mock<HttpControllerDescriptor>();
			eventLogger.LogDebug("RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent(): cdMock → {CdMock}", cdMock);
            cdMock.Setup(cd => cd.GetFilters()).Returns(new Collection<IFilter>());

            HttpActionDescriptor actionDescriptor = adMock.Object;
            actionDescriptor.ControllerDescriptor = cdMock.Object;
			eventLogger.LogDebug("RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent(): actionDescriptor.ControllerDescriptor → {Arg0}", actionDescriptor.ControllerDescriptor);

            var configuration = new HttpConfiguration();
			eventLogger.LogDebug("RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent(): configuration → {Configuration}", configuration);

            // Act
            var filterProvider = new DnnActionFilterProvider(Mock.Of<IServiceProvider>());
			eventLogger.LogDebug("RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent(): filterProvider → {FilterProvider}", filterProvider);
            var filters = filterProvider.GetFilters(configuration, actionDescriptor).ToList();
			eventLogger.LogDebug("RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent(): filters → {Filters}", filters);

            // Assert
            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters.First().Instance, Is.InstanceOf<DnnAuthorizeAttribute>());
			eventLogger.LogDebug("<<< Exiting RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent()");
        }

        [Test]
        public void RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent()
        {
			eventLogger.LogDebug(">>> Entering RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent()");
            // Arrange
            var adMock = new Mock<HttpActionDescriptor>();
			eventLogger.LogDebug("RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent(): adMock → {AdMock}", adMock);
            adMock.Setup(ad => ad.GetFilters()).Returns(new Collection<IFilter>(new[] { new AuthorizeAttribute() }));

            var cdMock = new Mock<HttpControllerDescriptor>();
			eventLogger.LogDebug("RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent(): cdMock → {CdMock}", cdMock);
            cdMock.Setup(cd => cd.GetFilters()).Returns(new Collection<IFilter>());

            HttpActionDescriptor actionDescriptor = adMock.Object;
            actionDescriptor.ControllerDescriptor = cdMock.Object;
			eventLogger.LogDebug("RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent(): actionDescriptor.ControllerDescriptor → {Arg0}", actionDescriptor.ControllerDescriptor);

            var configuration = new HttpConfiguration();
			eventLogger.LogDebug("RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent(): configuration → {Configuration}", configuration);

            // Act
            var filterProvider = new DnnActionFilterProvider(Mock.Of<IServiceProvider>());
			eventLogger.LogDebug("RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent(): filterProvider → {FilterProvider}", filterProvider);
            var filters = filterProvider.GetFilters(configuration, actionDescriptor).ToList();
			eventLogger.LogDebug("RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent(): filters → {Filters}", filters);

            // Assert
            Assert.That(filters, Has.Count.EqualTo(2));
            Assert.That(filters.Last().Instance, Is.InstanceOf<RequireHostAttribute>());
			eventLogger.LogDebug("<<< Exiting RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent()");
        }
    }
}
