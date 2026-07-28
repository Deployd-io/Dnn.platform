// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.InternalServices
{
    using System.Data;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;
    using DotNetNuke.Web.InternalServices;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    /// <summary>Tests FileUploadController methods.</summary>
    [TestFixture]
    public class FileUploadControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private Mock<DataProvider> mockDataProvider;
        private FileUploadController testInstance;
        private Mock<CachingProvider> mockCachingProvider;
        private Mock<IPortalController> mockPortalController;
        private TestSynchronizationContext synchronizationContext = new TestSynchronizationContext();
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.SetupDataProvider();
            this.SetupCachingProvider();
            this.SetupPortalSettings();
            this.SetupServiceProvider();
            this.SetupSynchronizationContext();
			Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            this.testInstance?.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        [SetCulture("tr-TR")]
        public async Task UploadFromLocal_ShouldUploadFile_WithTrCultureAsync()
        {
			Logger.LogDebug(">>> Entering UploadFromLocal_ShouldUploadFile_WithTrCultureAsync()");
            var formDataContent = new MultipartFormDataContent();
            formDataContent.Add(new StringContent("Hello World!"), "\"postfile\"", "\"testing\"");
			Logger.LogDebug("UploadFromLocal_ShouldUploadFile_WithTrCultureAsync(): formDataContent → {FormDataContent}", formDataContent);

            var request = new HttpRequestMessage();
            request.Content = formDataContent;
			Logger.LogDebug("UploadFromLocal_ShouldUploadFile_WithTrCultureAsync(): request → {Request}", request);
			Logger.LogDebug("UploadFromLocal_ShouldUploadFile_WithTrCultureAsync(): request.Content → {Arg0}", request.Content);
            this.testInstance.Request = request;
            await this.testInstance.UploadFromLocal(-1);

            Assert.That(this.synchronizationContext.IsUploadFileCalled(), Is.True);
			Logger.LogDebug("<<< Exiting UploadFromLocal_ShouldUploadFile_WithTrCultureAsync()");
        }

        private void SetupPortalSettings()
        {
			Logger.LogDebug(">>> Entering SetupPortalSettings()");
            this.mockPortalController = MockComponentProvider.CreatePortalController();
            this.mockPortalController.Setup(x => x.GetCurrentPortalSettings()).Returns(new PortalSettings() { PortalId = 0 });
			Logger.LogDebug("SetupPortalSettings(): mockPortalController → {MockPortalController}", mockPortalController);
            PortalController.SetTestableInstance(this.mockPortalController.Object);
			Logger.LogDebug("<<< Exiting SetupPortalSettings()");
        }

        private void SetupDataProvider()
        {
			Logger.LogDebug(">>> Entering SetupDataProvider()");
            this.mockDataProvider = MockComponentProvider.CreateDataProvider();
            this.mockDataProvider.Setup(x => x.GetListEntriesByListName("ImageTypes", string.Empty, It.IsAny<int>()))
                .Returns(Mock.Of<IDataReader>());
				Logger.LogDebug("SetupDataProvider(): mockDataProvider → {MockDataProvider}", mockDataProvider);
        }

        private void SetupCachingProvider()
        {
			Logger.LogDebug(">>> Entering SetupCachingProvider()");
            this.mockCachingProvider = MockComponentProvider.CreateDataCacheProvider();
            this.mockCachingProvider.Setup(x => x.GetItem(It.IsAny<string>()))
                .Returns(Enumerable.Empty<ListEntryInfo>());
				Logger.LogDebug("SetupCachingProvider(): mockCachingProvider → {MockCachingProvider}", mockCachingProvider);
        }

        private void SetupServiceProvider()
        {
			Logger.LogDebug(">>> Entering SetupServiceProvider()");
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("SetupServiceProvider(): services → {Services}", services);
				Logger.LogDebug("SetupServiceProvider(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(this.mockPortalController.Object);
                    services.AddSingleton(this.mockCachingProvider.Object);
                    services.AddSingleton(this.mockDataProvider.Object);
                });
				Logger.LogDebug("<<< Exiting SetupServiceProvider()");
        }

        private void SetupSynchronizationContext()
        {
			Logger.LogDebug(">>> Entering SetupSynchronizationContext()");
            this.synchronizationContext = new TestSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(this.synchronizationContext);
			Logger.LogDebug("SetupSynchronizationContext(): synchronizationContext → {SynchronizationContext}", synchronizationContext);
            this.testInstance = new FileUploadController();
			Logger.LogDebug("<<< Exiting SetupSynchronizationContext()");
			Logger.LogDebug("SetupSynchronizationContext(): testInstance → {TestInstance}", testInstance);
        }

        private class TestSynchronizationContext : SynchronizationContext
        {
            private bool isUploadFileCalled;

            public override void Post(SendOrPostCallback d, object state)
            {
				Logger.LogDebug(">>> Entering Post(d={D},state={State})", d, state);
                d(state);
				Logger.LogDebug("<<< Exiting Post(d={D},state={State})", d, state);
            }

            public override void Send(SendOrPostCallback d, object state)
            {
				Logger.LogDebug(">>> Entering Send(d={D},state={State})", d, state);
                isUploadFileCalled = true;
				Logger.LogDebug("<<< Exiting Send(d={D},state={State})", d, state);
				Logger.LogDebug("Send(d={D},state={State}): isUploadFileCalled → {IsUploadFileCalled}", d, state, isUploadFileCalled);
            }

            public bool IsUploadFileCalled()
            {
				Logger.LogDebug(">>> Entering IsUploadFileCalled()");
				Logger.LogDebug("<<< Exiting IsUploadFileCalled()");
                return isUploadFileCalled;
            }
        }
    }
}
