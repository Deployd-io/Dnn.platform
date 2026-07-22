// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Content
{
    using System;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Content.Mocks;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    /// <summary>  Summary description for ContentTypeTests.</summary>
    [TestFixture]
    public class ContentTypeControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private Mock<CachingProvider> mockCache;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.mockCache = MockComponentProvider.CreateDataCacheProvider();
            var dataProvider = MockComponentProvider.CreateDataProvider();
			Logger.LogDebug("SetUp(): mockCache → {MockCache}", mockCache);
			Logger.LogDebug("SetUp(): dataProvider → {DataProvider}", dataProvider);
            dataProvider.Setup(c => c.GetProviderPath()).Returns(string.Empty);
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("SetUp(): services → {Services}", services);
                    services.AddSingleton(this.mockCache.Object);
                    services.AddSingleton(dataProvider.Object);
                });
				Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            MockComponentProvider.ResetContainer();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void ContentTypeController_AddContentType_Throws_On_Null_ContentType()
        {
			Logger.LogDebug(">>> Entering ContentTypeController_AddContentType_Throws_On_Null_ContentType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			Logger.LogDebug("ContentTypeController_AddContentType_Throws_On_Null_ContentType(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentTypeController_AddContentType_Throws_On_Null_ContentType(): contentTypeController → {ContentTypeController}", contentTypeController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.AddContentType(null));
			Logger.LogDebug("<<< Exiting ContentTypeController_AddContentType_Throws_On_Null_ContentType()");
        }

        [Test]
        public void ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments()
        {
			Logger.LogDebug(">>> Entering ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			Logger.LogDebug("ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments(): contentTypeController → {ContentTypeController}", contentTypeController);

            ContentType contentType = ContentTestHelper.CreateValidContentType();

			Logger.LogDebug("ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments(): contentType → {ContentType}", contentType);
            // Act
            int contentTypeId = contentTypeController.AddContentType(contentType);

			Logger.LogDebug("ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments(): contentTypeId → {ContentTypeId}", contentTypeId);
            // Assert
            mockDataService.Verify(ds => ds.AddContentType(contentType));
			Logger.LogDebug("<<< Exiting ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments()");
        }

        [Test]
        public void ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType()
        {
			Logger.LogDebug(">>> Entering ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddContentType(It.IsAny<ContentType>())).Returns(Constants.CONTENTTYPE_AddContentTypeId);
			Logger.LogDebug("ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType(): mockDataService → {MockDataService}", mockDataService);
            var contentTypeController = new ContentTypeController(mockDataService.Object);

			Logger.LogDebug("ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType(): contentTypeController → {ContentTypeController}", contentTypeController);
            var contentType = ContentTestHelper.CreateValidContentType();

			Logger.LogDebug("ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType(): contentType → {ContentType}", contentType);
            // Act
            int contentTypeId = contentTypeController.AddContentType(contentType);

			Logger.LogDebug("ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType(): contentTypeId → {ContentTypeId}", contentTypeId);
            // Assert
            Assert.That(contentTypeId, Is.EqualTo(Constants.CONTENTTYPE_AddContentTypeId));
			Logger.LogDebug("<<< Exiting ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType()");
        }

        [Test]
        public void ContentTypeController_AddContentType_Sets_ValidId_On_Valid_ContentType()
        {
			Logger.LogDebug(">>> Entering ContentTypeController_AddContentType_Sets_ValidId_On_Valid_ContentType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddContentType(It.IsAny<ContentType>())).Returns(Constants.CONTENTTYPE_AddContentTypeId);
			Logger.LogDebug("ContentTypeController_AddContentType_Sets_ValidId_On_Valid_ContentType(): mockDataService → {MockDataService}", mockDataService);
            var contentTypeController = new ContentTypeController(mockDataService.Object);

			Logger.LogDebug("ContentTypeController_AddContentType_Sets_ValidId_On_Valid_ContentType(): contentTypeController → {ContentTypeController}", contentTypeController);
            var contentType = ContentTestHelper.CreateValidContentType();

			Logger.LogDebug("ContentTypeController_AddContentType_Sets_ValidId_On_Valid_ContentType(): contentType → {ContentType}", contentType);
            // Act
            contentTypeController.AddContentType(contentType);

            // Assert
            Assert.That(contentType.ContentTypeId, Is.EqualTo(Constants.CONTENTTYPE_AddContentTypeId));
			Logger.LogDebug("<<< Exiting ContentTypeController_AddContentType_Sets_ValidId_On_Valid_ContentType()");
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Throws_On_Null_ContentType()
        {
			Logger.LogDebug(">>> Entering ContentTypeController_DeleteContentType_Throws_On_Null_ContentType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			Logger.LogDebug("ContentTypeController_DeleteContentType_Throws_On_Null_ContentType(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentTypeController_DeleteContentType_Throws_On_Null_ContentType(): contentTypeController → {ContentTypeController}", contentTypeController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.DeleteContentType(null));
			Logger.LogDebug("<<< Exiting ContentTypeController_DeleteContentType_Throws_On_Null_ContentType()");
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId()
        {
			Logger.LogDebug(">>> Entering ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			Logger.LogDebug("ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId(): contentTypeController → {ContentTypeController}", contentTypeController);

            ContentType contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentTypeId = Null.NullInteger;
			Logger.LogDebug("ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId(): contentType → {ContentType}", contentType);
			Logger.LogDebug("ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId(): contentType.ContentTypeId → {Arg0}", contentType.ContentTypeId);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.DeleteContentType(contentType));
			Logger.LogDebug("<<< Exiting ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId()");
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId()
        {
			Logger.LogDebug(">>> Entering ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			Logger.LogDebug("ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId(): contentTypeController → {ContentTypeController}", contentTypeController);

            var contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
			Logger.LogDebug("ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId(): contentType → {ContentType}", contentType);
			Logger.LogDebug("ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId(): contentType.ContentTypeId → {Arg0}", contentType.ContentTypeId);

            // Act
            contentTypeController.DeleteContentType(contentType);

            // Assert
            mockDataService.Verify(ds => ds.DeleteContentType(contentType));
			Logger.LogDebug("<<< Exiting ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId()");
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Calls_DataService()
        {
			Logger.LogDebug(">>> Entering ContentTypeController_GetContentTypes_Calls_DataService()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetContentTypes()).Returns(MockHelper.CreateValidContentTypesReader(Constants.CONTENTTYPE_ValidContentTypeCount));
			Logger.LogDebug("ContentTypeController_GetContentTypes_Calls_DataService(): mockDataService → {MockDataService}", mockDataService);
            var contentTypeController = new ContentTypeController(mockDataService.Object);

			Logger.LogDebug("ContentTypeController_GetContentTypes_Calls_DataService(): contentTypeController → {ContentTypeController}", contentTypeController);
            // Act
            var contentTypes = contentTypeController.GetContentTypes();

			Logger.LogDebug("ContentTypeController_GetContentTypes_Calls_DataService(): contentTypes → {ContentTypes}", contentTypes);
            // Assert
            mockDataService.Verify(ds => ds.GetContentTypes());
			Logger.LogDebug("<<< Exiting ContentTypeController_GetContentTypes_Calls_DataService()");
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()
        {
			Logger.LogDebug(">>> Entering ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetContentTypes()).Returns(MockHelper.CreateEmptyContentTypeReader());
			Logger.LogDebug("ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes(): mockDataService → {MockDataService}", mockDataService);
            var contentTypeController = new ContentTypeController(mockDataService.Object);

			Logger.LogDebug("ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes(): contentTypeController → {ContentTypeController}", contentTypeController);
            // Act
            var contentTypes = contentTypeController.GetContentTypes();

			Logger.LogDebug("ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes(): contentTypes → {ContentTypes}", contentTypes);
            // Assert
            Assert.That(contentTypes, Is.Not.Null);
            Assert.That(contentTypes.Count(), Is.EqualTo(0));
			Logger.LogDebug("<<< Exiting ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()");
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Returns_List_Of_ContentTypes()
        {
			Logger.LogDebug(">>> Entering ContentTypeController_GetContentTypes_Returns_List_Of_ContentTypes()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetContentTypes()).Returns(MockHelper.CreateValidContentTypesReader(Constants.CONTENTTYPE_ValidContentTypeCount));
			Logger.LogDebug("ContentTypeController_GetContentTypes_Returns_List_Of_ContentTypes(): mockDataService → {MockDataService}", mockDataService);
            var contentTypeController = new ContentTypeController(mockDataService.Object);

			Logger.LogDebug("ContentTypeController_GetContentTypes_Returns_List_Of_ContentTypes(): contentTypeController → {ContentTypeController}", contentTypeController);
            // Act
            var contentTypes = contentTypeController.GetContentTypes();

			Logger.LogDebug("ContentTypeController_GetContentTypes_Returns_List_Of_ContentTypes(): contentTypes → {ContentTypes}", contentTypes);
            // Assert
            Assert.That(contentTypes.Count(), Is.EqualTo(Constants.CONTENTTYPE_ValidContentTypeCount));
			Logger.LogDebug("<<< Exiting ContentTypeController_GetContentTypes_Returns_List_Of_ContentTypes()");
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Throws_On_Null_ContentType()
        {
			Logger.LogDebug(">>> Entering ContentTypeController_UpdateContentType_Throws_On_Null_ContentType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			Logger.LogDebug("ContentTypeController_UpdateContentType_Throws_On_Null_ContentType(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentTypeController_UpdateContentType_Throws_On_Null_ContentType(): contentTypeController → {ContentTypeController}", contentTypeController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.UpdateContentType(null));
			Logger.LogDebug("<<< Exiting ContentTypeController_UpdateContentType_Throws_On_Null_ContentType()");
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId()
        {
			Logger.LogDebug(">>> Entering ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			Logger.LogDebug("ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId(): contentTypeController → {ContentTypeController}", contentTypeController);

            var contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentType = Constants.CONTENTTYPE_InValidContentType;
			Logger.LogDebug("ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId(): contentType → {ContentType}", contentType);
			Logger.LogDebug("ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId(): contentType.ContentType → {Arg0}", contentType.ContentType);

            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.UpdateContentType(contentType));
			Logger.LogDebug("<<< Exiting ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId()");
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType()
        {
			Logger.LogDebug(">>> Entering ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			Logger.LogDebug("ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType(): contentTypeController → {ContentTypeController}", contentTypeController);

            var contentType = ContentTestHelper.CreateValidContentType();
            contentType.ContentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId;
			Logger.LogDebug("ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType(): contentType → {ContentType}", contentType);
			Logger.LogDebug("ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType(): contentType.ContentTypeId → {Arg0}", contentType.ContentTypeId);
			Logger.LogDebug("ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType(): contentType.ContentType → {Arg0}", contentType.ContentType);
            contentType.ContentType = Constants.CONTENTTYPE_UpdateContentType;

            // Act
            contentTypeController.UpdateContentType(contentType);

            // Assert
            mockDataService.Verify(ds => ds.UpdateContentType(contentType));
			Logger.LogDebug("<<< Exiting ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType()");
        }
    }
}
