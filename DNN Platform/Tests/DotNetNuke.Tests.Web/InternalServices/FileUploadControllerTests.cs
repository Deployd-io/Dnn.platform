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
        private Mock<DataProvider> mockDataProvider;
        private FileUploadController testInstance;
        private Mock<CachingProvider> mockCachingProvider;
        private Mock<IPortalController> mockPortalController;
        private TestSynchronizationContext synchronizationContext = new TestSynchronizationContext();
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			eventLogger.LogDebug(">>> Entering SetUp()");
            this.SetupDataProvider();
            this.SetupCachingProvider();
            this.SetupPortalSettings();
            this.SetupServiceProvider();
            this.SetupSynchronizationContext();
			eventLogger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			eventLogger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            this.testInstance?.Dispose();
			eventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        [SetCulture("tr-TR")]
        public async Task UploadFromLocal_ShouldUploadFile_WithTrCultureAsync()
        {
			eventLogger.LogDebug(">>> Entering UploadFromLocal_ShouldUploadFile_WithTrCultureAsync()");
            var formDataContent = new MultipartFormDataContent();
            formDataContent.Add(new StringContent("Hello World!"), "\"postfile\"", "\"testing\"");
			eventLogger.LogDebug("UploadFromLocal_ShouldUploadFile_WithTrCultureAsync(): formDataContent → {FormDataContent}", formDataContent);

            var request = new HttpRequestMessage();
			eventLogger.LogDebug("UploadFromLocal_ShouldUploadFile_WithTrCultureAsync(): request → {Request}", request);
			eventLogger.LogDebug("UploadFromLocal_ShouldUploadFile_WithTrCultureAsync(): request.Content → {Arg0}", request.Content);
            request.Content = formDataContent;
            this.testInstance.Request = request;
            await this.testInstance.UploadFromLocal(-1);

            Assert.That(this.synchronizationContext.IsUploadFileCalled(), Is.True);
			eventLogger.LogDebug("<<< Exiting UploadFromLocal_ShouldUploadFile_WithTrCultureAsync()");
        }

        private void SetupPortalSettings()
        {
			eventLogger.LogDebug(">>> Entering SetupPortalSettings()");
            this.mockPortalController = MockComponentProvider.CreatePortalController();
            this.mockPortalController.Setup(x => x.GetCurrentPortalSettings()).Returns(new PortalSettings() { PortalId = 0 });
			eventLogger.LogDebug("SetupPortalSettings(): mockPortalController → {MockPortalController}", mockPortalController);
            PortalController.SetTestableInstance(this.mockPortalController.Object);
			eventLogger.LogDebug("<<< Exiting SetupPortalSettings()");
        }

        private void SetupDataProvider()
        {
			eventLogger.LogDebug(">>> Entering SetupDataProvider()");
            this.mockDataProvider = MockComponentProvider.CreateDataProvider();
            this.mockDataProvider.Setup(x => x.GetListEntriesByListName("ImageTypes", string.Empty, It.IsAny<int>()))
                .Returns(Mock.Of<IDataReader>());
				eventLogger.LogDebug("SetupDataProvider(): mockDataProvider → {MockDataProvider}", mockDataProvider);
				eventLogger.LogDebug("<<< Exiting SetupDataProvider()");
        }

        private void SetupCachingProvider()
        {
			eventLogger.LogDebug(">>> Entering SetupCachingProvider()");
            this.mockCachingProvider = MockComponentProvider.CreateDataCacheProvider();
            this.mockCachingProvider.Setup(x => x.GetItem(It.IsAny<string>()))
                .Returns(Enumerable.Empty<ListEntryInfo>());
				eventLogger.LogDebug("SetupCachingProvider(): mockCachingProvider → {MockCachingProvider}", mockCachingProvider);
				eventLogger.LogDebug("<<< Exiting SetupCachingProvider()");
        }

        private void SetupServiceProvider()
        {
			eventLogger.LogDebug(">>> Entering SetupServiceProvider()");
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					eventLogger.LogDebug("SetupServiceProvider(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(this.mockPortalController.Object);
                    services.AddSingleton(this.mockCachingProvider.Object);
                    services.AddSingleton(this.mockDataProvider.Object);
                });
				eventLogger.LogDebug("<<< Exiting SetupServiceProvider()");
        }

        private void SetupSynchronizationContext()
        {
			eventLogger.LogDebug(">>> Entering SetupSynchronizationContext()");
            this.synchronizationContext = new TestSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(this.synchronizationContext);
			eventLogger.LogDebug("SetupSynchronizationContext(): synchronizationContext → {SynchronizationContext}", synchronizationContext);
            this.testInstance = new FileUploadController();
			eventLogger.LogDebug("SetupSynchronizationContext(): testInstance → {TestInstance}", testInstance);
			eventLogger.LogDebug("<<< Exiting SetupSynchronizationContext()");
        }

        private class TestSynchronizationContext : SynchronizationContext
        {
            private bool isUploadFileCalled;

            public override void Post(SendOrPostCallback d, object state)
            {
				eventLogger.LogDebug(">>> Entering Post(d={D},state={State})", d, state);
                d(state);
				eventLogger.LogDebug("<<< Exiting Post(d={D},state={State})", d, state);
            }

            public override void Send(SendOrPostCallback d, object state)
            {
				eventLogger.LogDebug(">>> Entering Send(d={D},state={State})", d, state);
                isUploadFileCalled = true;
				eventLogger.LogDebug("Send(d={D},state={State}): isUploadFileCalled → {IsUploadFileCalled}", d, state, isUploadFileCalled);
				eventLogger.LogDebug("<<< Exiting Send(d={D},state={State})", d, state);
            }

            public bool IsUploadFileCalled()
            {
				eventLogger.LogDebug(">>> Entering IsUploadFileCalled()");
				eventLogger.LogDebug("<<< Exiting IsUploadFileCalled()");
                return isUploadFileCalled;
            }
        }
    }
}
