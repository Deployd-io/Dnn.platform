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
	using Microsoft.Extensions.Logging;

    /// <summary>  Summary description for ContentTypeTests.</summary>
    [TestFixture]
    public class ContentTypeControllerTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private Mock<CachingProvider> mockCache;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			_logger.LogDebug(">>> Entering SetUp()");
            this.mockCache = MockComponentProvider.CreateDataCacheProvider();
            var dataProvider = MockComponentProvider.CreateDataProvider();
			_logger.LogDebug("SetUp(): dataProvider → {DataProvider}", dataProvider);
			_logger.LogDebug("SetUp(): mockCache → {MockCache}", mockCache);
            dataProvider.Setup(c => c.GetProviderPath()).Returns(string.Empty);
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					_logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(this.mockCache.Object);
                    services.AddSingleton(dataProvider.Object);
                });
				_logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			_logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            MockComponentProvider.ResetContainer();
			_logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void ContentTypeController_AddContentType_Throws_On_Null_ContentType()
        {
			_logger.LogDebug(">>> Entering ContentTypeController_AddContentType_Throws_On_Null_ContentType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ContentTypeController_AddContentType_Throws_On_Null_ContentType(): mockDataService → {MockDataService}", mockDataService);
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			_logger.LogDebug("ContentTypeController_AddContentType_Throws_On_Null_ContentType(): contentTypeController → {ContentTypeController}", contentTypeController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.AddContentType(null));
			_logger.LogDebug("<<< Exiting ContentTypeController_AddContentType_Throws_On_Null_ContentType()");
        }

        [Test]
        public void ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments()
        {
			_logger.LogDebug(">>> Entering ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			_logger.LogDebug("ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments(): contentTypeController → {ContentTypeController}", contentTypeController);

            ContentType contentType = ContentTestHelper.CreateValidContentType();
			_logger.LogDebug("ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments(): contentType → {ContentType}", contentType);

            // Act
            int contentTypeId = contentTypeController.AddContentType(contentType);
			_logger.LogDebug("ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments(): contentTypeId → {ContentTypeId}", contentTypeId);

            // Assert
            mockDataService.Verify(ds => ds.AddContentType(contentType));
			_logger.LogDebug("<<< Exiting ContentTypeController_AddContentType_Calls_DataService_On_Valid_Arguments()");
        }

        [Test]
        public void ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType()
        {
			_logger.LogDebug(">>> Entering ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.AddContentType(It.IsAny<ContentType>())).Returns(Constants.CONTENTTYPE_AddContentTypeId);
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			_logger.LogDebug("ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType(): contentTypeController → {ContentTypeController}", contentTypeController);

            var contentType = ContentTestHelper.CreateValidContentType();
			_logger.LogDebug("ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType(): contentType → {ContentType}", contentType);

            // Act
            int contentTypeId = contentTypeController.AddContentType(contentType);
			_logger.LogDebug("ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType(): contentTypeId → {ContentTypeId}", contentTypeId);

            // Assert
            Assert.That(contentTypeId, Is.EqualTo(Constants.CONTENTTYPE_AddContentTypeId));
			_logger.LogDebug("<<< Exiting ContentTypeController_AddContentType_Returns_ValidId_On_Valid_ContentType()");
        }

        [Test]
        public void ContentTypeController_AddContentType_Sets_ValidId_On_Valid_ContentType()
        {
			_logger.LogDebug(">>> Entering ContentTypeController_AddContentType_Sets_ValidId_On_Valid_ContentType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ContentTypeController_AddContentType_Sets_ValidId_On_Valid_ContentType(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.AddContentType(It.IsAny<ContentType>())).Returns(Constants.CONTENTTYPE_AddContentTypeId);
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			_logger.LogDebug("ContentTypeController_AddContentType_Sets_ValidId_On_Valid_ContentType(): contentTypeController → {ContentTypeController}", contentTypeController);

            var contentType = ContentTestHelper.CreateValidContentType();
			_logger.LogDebug("ContentTypeController_AddContentType_Sets_ValidId_On_Valid_ContentType(): contentType → {ContentType}", contentType);

            // Act
            contentTypeController.AddContentType(contentType);

            // Assert
            Assert.That(contentType.ContentTypeId, Is.EqualTo(Constants.CONTENTTYPE_AddContentTypeId));
			_logger.LogDebug("<<< Exiting ContentTypeController_AddContentType_Sets_ValidId_On_Valid_ContentType()");
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Throws_On_Null_ContentType()
        {
			_logger.LogDebug(">>> Entering ContentTypeController_DeleteContentType_Throws_On_Null_ContentType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ContentTypeController_DeleteContentType_Throws_On_Null_ContentType(): mockDataService → {MockDataService}", mockDataService);
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			_logger.LogDebug("ContentTypeController_DeleteContentType_Throws_On_Null_ContentType(): contentTypeController → {ContentTypeController}", contentTypeController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.DeleteContentType(null));
			_logger.LogDebug("<<< Exiting ContentTypeController_DeleteContentType_Throws_On_Null_ContentType()");
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId()
        {
			_logger.LogDebug(">>> Entering ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId(): mockDataService → {MockDataService}", mockDataService);
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			_logger.LogDebug("ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId(): contentTypeController → {ContentTypeController}", contentTypeController);

            ContentType contentType = ContentTestHelper.CreateValidContentType();
			_logger.LogDebug("ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId(): contentType → {ContentType}", contentType);
			_logger.LogDebug("ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId(): contentType.ContentTypeId → {Arg0}", contentType.ContentTypeId);
            contentType.ContentTypeId = Null.NullInteger;

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.DeleteContentType(contentType));
			_logger.LogDebug("<<< Exiting ContentTypeController_DeleteContentType_Throws_On_Negative_ContentTypeId()");
        }

        [Test]
        public void ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId()
        {
			_logger.LogDebug(">>> Entering ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId(): mockDataService → {MockDataService}", mockDataService);
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			_logger.LogDebug("ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId(): contentTypeController → {ContentTypeController}", contentTypeController);

            var contentType = ContentTestHelper.CreateValidContentType();
			_logger.LogDebug("ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId(): contentType → {ContentType}", contentType);
			_logger.LogDebug("ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId(): contentType.ContentTypeId → {Arg0}", contentType.ContentTypeId);
            contentType.ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            // Act
            contentTypeController.DeleteContentType(contentType);

            // Assert
            mockDataService.Verify(ds => ds.DeleteContentType(contentType));
			_logger.LogDebug("<<< Exiting ContentTypeController_DeleteContentType_Calls_DataService_On_Valid_ContentTypeId()");
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Calls_DataService()
        {
			_logger.LogDebug(">>> Entering ContentTypeController_GetContentTypes_Calls_DataService()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ContentTypeController_GetContentTypes_Calls_DataService(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetContentTypes()).Returns(MockHelper.CreateValidContentTypesReader(Constants.CONTENTTYPE_ValidContentTypeCount));
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			_logger.LogDebug("ContentTypeController_GetContentTypes_Calls_DataService(): contentTypeController → {ContentTypeController}", contentTypeController);

            // Act
            var contentTypes = contentTypeController.GetContentTypes();
			_logger.LogDebug("ContentTypeController_GetContentTypes_Calls_DataService(): contentTypes → {ContentTypes}", contentTypes);

            // Assert
            mockDataService.Verify(ds => ds.GetContentTypes());
			_logger.LogDebug("<<< Exiting ContentTypeController_GetContentTypes_Calls_DataService()");
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()
        {
			_logger.LogDebug(">>> Entering ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetContentTypes()).Returns(MockHelper.CreateEmptyContentTypeReader());
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			_logger.LogDebug("ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes(): contentTypeController → {ContentTypeController}", contentTypeController);

            // Act
            var contentTypes = contentTypeController.GetContentTypes();
			_logger.LogDebug("ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes(): contentTypes → {ContentTypes}", contentTypes);

            // Assert
            Assert.That(contentTypes, Is.Not.Null);
            Assert.That(contentTypes.Count(), Is.EqualTo(0));
			_logger.LogDebug("<<< Exiting ContentTypeController_GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()");
        }

        [Test]
        public void ContentTypeController_GetContentTypes_Returns_List_Of_ContentTypes()
        {
			_logger.LogDebug(">>> Entering ContentTypeController_GetContentTypes_Returns_List_Of_ContentTypes()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ContentTypeController_GetContentTypes_Returns_List_Of_ContentTypes(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetContentTypes()).Returns(MockHelper.CreateValidContentTypesReader(Constants.CONTENTTYPE_ValidContentTypeCount));
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			_logger.LogDebug("ContentTypeController_GetContentTypes_Returns_List_Of_ContentTypes(): contentTypeController → {ContentTypeController}", contentTypeController);

            // Act
            var contentTypes = contentTypeController.GetContentTypes();
			_logger.LogDebug("ContentTypeController_GetContentTypes_Returns_List_Of_ContentTypes(): contentTypes → {ContentTypes}", contentTypes);

            // Assert
            Assert.That(contentTypes.Count(), Is.EqualTo(Constants.CONTENTTYPE_ValidContentTypeCount));
			_logger.LogDebug("<<< Exiting ContentTypeController_GetContentTypes_Returns_List_Of_ContentTypes()");
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Throws_On_Null_ContentType()
        {
			_logger.LogDebug(">>> Entering ContentTypeController_UpdateContentType_Throws_On_Null_ContentType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ContentTypeController_UpdateContentType_Throws_On_Null_ContentType(): mockDataService → {MockDataService}", mockDataService);
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			_logger.LogDebug("ContentTypeController_UpdateContentType_Throws_On_Null_ContentType(): contentTypeController → {ContentTypeController}", contentTypeController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.UpdateContentType(null));
			_logger.LogDebug("<<< Exiting ContentTypeController_UpdateContentType_Throws_On_Null_ContentType()");
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId()
        {
			_logger.LogDebug(">>> Entering ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId(): mockDataService → {MockDataService}", mockDataService);
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			_logger.LogDebug("ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId(): contentTypeController → {ContentTypeController}", contentTypeController);

            var contentType = ContentTestHelper.CreateValidContentType();
			_logger.LogDebug("ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId(): contentType → {ContentType}", contentType);
			_logger.LogDebug("ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId(): contentType.ContentType → {Arg0}", contentType.ContentType);
            contentType.ContentType = Constants.CONTENTTYPE_InValidContentType;

            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.UpdateContentType(contentType));
			_logger.LogDebug("<<< Exiting ContentTypeController_UpdateContentType_Throws_On_Negative_ContentTypeId()");
        }

        [Test]
        public void ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType()
        {
			_logger.LogDebug(">>> Entering ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType(): mockDataService → {MockDataService}", mockDataService);
            var contentTypeController = new ContentTypeController(mockDataService.Object);
			_logger.LogDebug("ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType(): contentTypeController → {ContentTypeController}", contentTypeController);

            var contentType = ContentTestHelper.CreateValidContentType();
			_logger.LogDebug("ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType(): contentType → {ContentType}", contentType);
			_logger.LogDebug("ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType(): contentType.ContentTypeId → {Arg0}", contentType.ContentTypeId);
			_logger.LogDebug("ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType(): contentType.ContentType → {Arg0}", contentType.ContentType);
            contentType.ContentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId;
            contentType.ContentType = Constants.CONTENTTYPE_UpdateContentType;

            // Act
            contentTypeController.UpdateContentType(contentType);

            // Assert
            mockDataService.Verify(ds => ds.UpdateContentType(contentType));
			_logger.LogDebug("<<< Exiting ContentTypeController_UpdateContentType_Calls_DataService_On_Valid_ContentType()");
        }
    }
}
