// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class FileContentTypeManagerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
			Logger.LogDebug(">>> Entering Setup()");
            var mockData = MockComponentProvider.CreateDataProvider();
            var mockCache = MockComponentProvider.CreateDataCacheProvider();
			Logger.LogDebug("Setup(): mockData → {MockData}", mockData);
			Logger.LogDebug("Setup(): mockCache → {MockCache}", mockCache);
            var mockGlobals = new Mock<IGlobals>();
            var mockCbo = new Mock<ICBO>();
			Logger.LogDebug("Setup(): mockGlobals → {MockGlobals}", mockGlobals);
			Logger.LogDebug("Setup(): mockCbo → {MockCbo}", mockCbo);

            mockData.Setup(m => m.GetProviderPath()).Returns(string.Empty);

            TestableGlobals.SetTestableInstance(mockGlobals.Object);
            CBO.SetTestableInstance(mockCbo.Object);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("Setup(): services → {Services}", services);
                    services.AddSingleton(mockCache.Object);
                    services.AddSingleton(mockData.Object);
                    services.AddSingleton(mockGlobals.Object);
                    services.AddSingleton(mockCbo.Object);
                });
				Logger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            TestableGlobals.ClearInstance();
            CBO.ClearInstance();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void GetContentType_Returns_Known_Value_When_Extension_Is_Not_Managed()
        {
			Logger.LogDebug(">>> Entering GetContentType_Returns_Known_Value_When_Extension_Is_Not_Managed()");
            const string notManagedExtension = "asdf609vas21AS:F,l/&%/(%$";

            var contentType = FileContentTypeManager.Instance.GetContentType(notManagedExtension);

			Logger.LogDebug("GetContentType_Returns_Known_Value_When_Extension_Is_Not_Managed(): contentType → {ContentType}", contentType);
            Assert.That(contentType, Is.EqualTo("application/octet-stream"));
			Logger.LogDebug("<<< Exiting GetContentType_Returns_Known_Value_When_Extension_Is_Not_Managed()");
        }

        [Test]
        public void GetContentType_Returns_Correct_Value_For_Extension()
        {
			Logger.LogDebug(">>> Entering GetContentType_Returns_Correct_Value_For_Extension()");
            const string notManagedExtension = "htm";

            var contentType = FileContentTypeManager.Instance.GetContentType(notManagedExtension);

			Logger.LogDebug("GetContentType_Returns_Correct_Value_For_Extension(): contentType → {ContentType}", contentType);
            Assert.That(contentType, Is.EqualTo("text/html"));
			Logger.LogDebug("<<< Exiting GetContentType_Returns_Correct_Value_For_Extension()");
        }
    }
}
