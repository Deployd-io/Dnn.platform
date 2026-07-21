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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            CBO.SetTestableInstance(new MockCBO());
			Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            CBO.ClearInstance();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent()
        {
			Logger.LogDebug(">>> Entering RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent()");
            // Arrange
            var adMock = new Mock<HttpActionDescriptor>();
            adMock.Setup(ad => ad.GetFilters()).Returns(new Collection<IFilter>());
			Logger.LogDebug("RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent(): adMock → {AdMock}", adMock);

            var cdMock = new Mock<HttpControllerDescriptor>();
            cdMock.Setup(cd => cd.GetFilters()).Returns(new Collection<IFilter>());
			Logger.LogDebug("RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent(): cdMock → {CdMock}", cdMock);

            HttpActionDescriptor actionDescriptor = adMock.Object;
            actionDescriptor.ControllerDescriptor = cdMock.Object;
			Logger.LogDebug("RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent(): actionDescriptor.ControllerDescriptor → {Arg0}", actionDescriptor.ControllerDescriptor);

            var configuration = new HttpConfiguration();

			Logger.LogDebug("RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent(): configuration → {Configuration}", configuration);
            // Act
            var filterProvider = new DnnActionFilterProvider(Mock.Of<IServiceProvider>());
            var filters = filterProvider.GetFilters(configuration, actionDescriptor).ToList();
			Logger.LogDebug("RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent(): filterProvider → {FilterProvider}", filterProvider);
			Logger.LogDebug("RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent(): filters → {Filters}", filters);

            // Assert
            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters.First().Instance, Is.InstanceOf<RequireHostAttribute>());
			Logger.LogDebug("<<< Exiting RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent()");
        }

        [Test]
        public void RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent()
        {
			Logger.LogDebug(">>> Entering RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent()");
            // Arrange
            var adMock = new Mock<HttpActionDescriptor>();
            adMock.Setup(ad => ad.GetFilters()).Returns(new Collection<IFilter>(new[] { new DnnAuthorizeAttribute() }));
			Logger.LogDebug("RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent(): adMock → {AdMock}", adMock);

            var cdMock = new Mock<HttpControllerDescriptor>();
            cdMock.Setup(cd => cd.GetFilters()).Returns(new Collection<IFilter>());
			Logger.LogDebug("RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent(): cdMock → {CdMock}", cdMock);

            HttpActionDescriptor actionDescriptor = adMock.Object;
            actionDescriptor.ControllerDescriptor = cdMock.Object;
			Logger.LogDebug("RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent(): actionDescriptor.ControllerDescriptor → {Arg0}", actionDescriptor.ControllerDescriptor);

            var configuration = new HttpConfiguration();

			Logger.LogDebug("RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent(): configuration → {Configuration}", configuration);
            // Act
            var filterProvider = new DnnActionFilterProvider(Mock.Of<IServiceProvider>());
            var filters = filterProvider.GetFilters(configuration, actionDescriptor).ToList();
			Logger.LogDebug("RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent(): filterProvider → {FilterProvider}", filterProvider);
			Logger.LogDebug("RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent(): filters → {Filters}", filters);

            // Assert
            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters.First().Instance, Is.InstanceOf<DnnAuthorizeAttribute>());
			Logger.LogDebug("<<< Exiting RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent()");
        }

        [Test]
        public void RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent()
        {
			Logger.LogDebug(">>> Entering RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent()");
            // Arrange
            var adMock = new Mock<HttpActionDescriptor>();
            adMock.Setup(ad => ad.GetFilters()).Returns(new Collection<IFilter>(new[] { new AuthorizeAttribute() }));
			Logger.LogDebug("RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent(): adMock → {AdMock}", adMock);

            var cdMock = new Mock<HttpControllerDescriptor>();
            cdMock.Setup(cd => cd.GetFilters()).Returns(new Collection<IFilter>());
			Logger.LogDebug("RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent(): cdMock → {CdMock}", cdMock);

            HttpActionDescriptor actionDescriptor = adMock.Object;
            actionDescriptor.ControllerDescriptor = cdMock.Object;
			Logger.LogDebug("RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent(): actionDescriptor.ControllerDescriptor → {Arg0}", actionDescriptor.ControllerDescriptor);

            var configuration = new HttpConfiguration();

			Logger.LogDebug("RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent(): configuration → {Configuration}", configuration);
            // Act
            var filterProvider = new DnnActionFilterProvider(Mock.Of<IServiceProvider>());
            var filters = filterProvider.GetFilters(configuration, actionDescriptor).ToList();
			Logger.LogDebug("RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent(): filterProvider → {FilterProvider}", filterProvider);
			Logger.LogDebug("RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent(): filters → {Filters}", filters);

            // Assert
            Assert.That(filters, Has.Count.EqualTo(2));
            Assert.That(filters.Last().Instance, Is.InstanceOf<RequireHostAttribute>());
			Logger.LogDebug("<<< Exiting RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent()");
        }
    }
}
