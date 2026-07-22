// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Content
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Tests.Content.Mocks;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    /// <summary>  Summary description for ContentItemTests.</summary>
    [TestFixture]
    public class ContentControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const int ModuleSearchTypeId = 1;

        private Mock<CachingProvider> mockCache;
        private Mock<DataProvider> mockDataProvider;
        private Mock<Services.Search.Internals.ISearchHelper> mockSearchHelper;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.mockCache = MockComponentProvider.CreateDataCacheProvider();
            this.mockDataProvider = MockComponentProvider.CreateDataProvider();
			Logger.LogDebug("SetUp(): mockDataProvider → {MockDataProvider}", mockDataProvider);
			Logger.LogDebug("SetUp(): mockCache → {MockCache}", mockCache);
            this.mockSearchHelper = new Mock<Services.Search.Internals.ISearchHelper>();
			Logger.LogDebug("SetUp(): mockSearchHelper → {MockSearchHelper}", mockSearchHelper);

            Services.Search.Internals.SearchHelper.SetTestableInstance(this.mockSearchHelper.Object);

            this.mockSearchHelper.Setup(x => x.GetSearchTypeByName(It.IsAny<string>())).Returns<string>(
                (string searchTypeName) => new SearchType { SearchTypeName = searchTypeName, SearchTypeId = ModuleSearchTypeId });

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("SetUp(): services → {Services}", services);
                    services.AddSingleton(this.mockCache.Object);
                    services.AddSingleton(this.mockDataProvider.Object);
                    services.AddSingleton(this.mockSearchHelper.Object);
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
        public void ContentController_AddContentItem_Throws_On_Null_ContentItem()
        {
			Logger.LogDebug(">>> Entering ContentController_AddContentItem_Throws_On_Null_ContentItem()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_AddContentItem_Throws_On_Null_ContentItem(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_AddContentItem_Throws_On_Null_ContentItem(): controller → {Controller}", controller);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => controller.AddContentItem(null));
			Logger.LogDebug("<<< Exiting ContentController_AddContentItem_Throws_On_Null_ContentItem()");
        }

        [Test]
        public void ContentController_AddContentItem_Calls_DataService_On_Valid_Arguments()
        {
			Logger.LogDebug(">>> Entering ContentController_AddContentItem_Calls_DataService_On_Valid_Arguments()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();

			Logger.LogDebug("ContentController_AddContentItem_Calls_DataService_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
            ContentController controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_AddContentItem_Calls_DataService_On_Valid_Arguments(): controller → {Controller}", controller);
            ComponentFactory.RegisterComponentInstance<IContentController>(controller);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
            content.ContentItemId = Constants.CONTENT_ValidContentItemId;
			Logger.LogDebug("ContentController_AddContentItem_Calls_DataService_On_Valid_Arguments(): content → {Content}", content);
			Logger.LogDebug("ContentController_AddContentItem_Calls_DataService_On_Valid_Arguments(): content.ContentItemId → {Arg0}", content.ContentItemId);

            // Act
            int contentId = controller.AddContentItem(content);

			Logger.LogDebug("ContentController_AddContentItem_Calls_DataService_On_Valid_Arguments(): contentId → {ContentId}", contentId);
            // Assert
            mockDataService.Verify(ds => ds.AddContentItem(content, It.IsAny<int>()));
			Logger.LogDebug("<<< Exiting ContentController_AddContentItem_Calls_DataService_On_Valid_Arguments()");
        }

        [Test]
        public void ContentController_AddContentItem_Returns_ValidId_On_Valid_ContentItem()
        {
			Logger.LogDebug(">>> Entering ContentController_AddContentItem_Returns_ValidId_On_Valid_ContentItem()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddContentItem(It.IsAny<ContentItem>(), It.IsAny<int>())).Returns(Constants.CONTENT_AddContentItemId);
			Logger.LogDebug("ContentController_AddContentItem_Returns_ValidId_On_Valid_ContentItem(): mockDataService → {MockDataService}", mockDataService);
            ContentController controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_AddContentItem_Returns_ValidId_On_Valid_ContentItem(): controller → {Controller}", controller);
            ComponentFactory.RegisterComponentInstance<IContentController>(controller);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
            content.ContentItemId = Constants.CONTENT_ValidContentItemId;
			Logger.LogDebug("ContentController_AddContentItem_Returns_ValidId_On_Valid_ContentItem(): content → {Content}", content);
			Logger.LogDebug("ContentController_AddContentItem_Returns_ValidId_On_Valid_ContentItem(): content.ContentItemId → {Arg0}", content.ContentItemId);

            // Act
            int contentId = controller.AddContentItem(content);

			Logger.LogDebug("ContentController_AddContentItem_Returns_ValidId_On_Valid_ContentItem(): contentId → {ContentId}", contentId);
            // Assert
            Assert.That(contentId, Is.EqualTo(Constants.CONTENT_AddContentItemId));
			Logger.LogDebug("<<< Exiting ContentController_AddContentItem_Returns_ValidId_On_Valid_ContentItem()");
        }

        [Test]
        public void ContentController_AddContentItem_Sets_ContentItemId_Property_On_Valid_ContentItem()
        {
			Logger.LogDebug(">>> Entering ContentController_AddContentItem_Sets_ContentItemId_Property_On_Valid_ContentItem()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddContentItem(It.IsAny<ContentItem>(), It.IsAny<int>())).Returns(Constants.CONTENT_AddContentItemId);
			Logger.LogDebug("ContentController_AddContentItem_Sets_ContentItemId_Property_On_Valid_ContentItem(): mockDataService → {MockDataService}", mockDataService);
            ContentController controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_AddContentItem_Sets_ContentItemId_Property_On_Valid_ContentItem(): controller → {Controller}", controller);
            ComponentFactory.RegisterComponentInstance<IContentController>(controller);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
            content.ContentItemId = Constants.CONTENT_ValidContentItemId;
			Logger.LogDebug("ContentController_AddContentItem_Sets_ContentItemId_Property_On_Valid_ContentItem(): content → {Content}", content);
			Logger.LogDebug("ContentController_AddContentItem_Sets_ContentItemId_Property_On_Valid_ContentItem(): content.ContentItemId → {Arg0}", content.ContentItemId);

            // Act
            int contentId = controller.AddContentItem(content);

			Logger.LogDebug("ContentController_AddContentItem_Sets_ContentItemId_Property_On_Valid_ContentItem(): contentId → {ContentId}", contentId);
            // Assert
            Assert.That(content.ContentItemId, Is.EqualTo(Constants.CONTENT_AddContentItemId));
			Logger.LogDebug("<<< Exiting ContentController_AddContentItem_Sets_ContentItemId_Property_On_Valid_ContentItem()");
        }

        [Test]
        public void ContentController_AddContentItem_Sets_CreatedByUserId_And_LastModifiedUserId()
        {
			Logger.LogDebug(">>> Entering ContentController_AddContentItem_Sets_CreatedByUserId_And_LastModifiedUserId()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddContentItem(It.IsAny<ContentItem>(), It.IsAny<int>())).Returns(Constants.CONTENT_AddContentItemId);
			Logger.LogDebug("ContentController_AddContentItem_Sets_CreatedByUserId_And_LastModifiedUserId(): mockDataService → {MockDataService}", mockDataService);
            ContentController controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_AddContentItem_Sets_CreatedByUserId_And_LastModifiedUserId(): controller → {Controller}", controller);
            ComponentFactory.RegisterComponentInstance<IContentController>(controller);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
            content.ContentItemId = Constants.CONTENT_ValidContentItemId;
			Logger.LogDebug("ContentController_AddContentItem_Sets_CreatedByUserId_And_LastModifiedUserId(): content → {Content}", content);
			Logger.LogDebug("ContentController_AddContentItem_Sets_CreatedByUserId_And_LastModifiedUserId(): content.ContentItemId → {Arg0}", content.ContentItemId);
			Logger.LogDebug("ContentController_AddContentItem_Sets_CreatedByUserId_And_LastModifiedUserId(): content.CreatedByUserID → {Arg0}", content.CreatedByUserID);
            content.CreatedByUserID = 1;

            // Act
            int contentId = controller.AddContentItem(content);

			Logger.LogDebug("ContentController_AddContentItem_Sets_CreatedByUserId_And_LastModifiedUserId(): contentId → {ContentId}", contentId);
            // Assert
            mockDataService.Verify(
                service =>
                    service.AddContentItem(It.IsAny<ContentItem>(), content.CreatedByUserID), Times.Once);
					Logger.LogDebug("ContentController_AddContentItem_Sets_CreatedByUserId_And_LastModifiedUserId(): service → {Service}", service);
        }

        [Test]
        public void ContentController_AddContentItem_Sets_Override_CreatedByUserId_And_LastModifiedUserId()
        {
			Logger.LogDebug(">>> Entering ContentController_AddContentItem_Sets_Override_CreatedByUserId_And_LastModifiedUserId()");
            // Arrange
            int expectedUserId = 5;
            Mock<IUserController> mockUserController = new Mock<IUserController>();
            mockUserController.Setup(user => user.GetCurrentUserInfo()).Returns(new UserInfo { UserID = expectedUserId });
			Logger.LogDebug("ContentController_AddContentItem_Sets_Override_CreatedByUserId_And_LastModifiedUserId(): mockUserController → {MockUserController}", mockUserController);
            UserController.SetTestableInstance(mockUserController.Object);

            Mock<ICBO> mockCBO = new Mock<ICBO>();
            CBO.SetTestableInstance(mockCBO.Object);
			Logger.LogDebug("ContentController_AddContentItem_Sets_Override_CreatedByUserId_And_LastModifiedUserId(): mockCBO → {MockCBO}", mockCBO);

            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddContentItem(It.IsAny<ContentItem>(), It.IsAny<int>())).Returns(Constants.CONTENT_AddContentItemId);
			Logger.LogDebug("ContentController_AddContentItem_Sets_Override_CreatedByUserId_And_LastModifiedUserId(): mockDataService → {MockDataService}", mockDataService);

            ContentController controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_AddContentItem_Sets_Override_CreatedByUserId_And_LastModifiedUserId(): controller → {Controller}", controller);
            ComponentFactory.RegisterComponentInstance<IContentController>(controller);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
            content.ContentItemId = Constants.CONTENT_ValidContentItemId;
			Logger.LogDebug("ContentController_AddContentItem_Sets_Override_CreatedByUserId_And_LastModifiedUserId(): content → {Content}", content);
			Logger.LogDebug("ContentController_AddContentItem_Sets_Override_CreatedByUserId_And_LastModifiedUserId(): content.ContentItemId → {Arg0}", content.ContentItemId);

            // Act
            int contentId = controller.AddContentItem(content);

			Logger.LogDebug("ContentController_AddContentItem_Sets_Override_CreatedByUserId_And_LastModifiedUserId(): contentId → {ContentId}", contentId);
            // Assert
            mockDataService.Verify(
                service =>
                    service.AddContentItem(It.IsAny<ContentItem>(), expectedUserId), Times.Once);
					Logger.LogDebug("ContentController_AddContentItem_Sets_Override_CreatedByUserId_And_LastModifiedUserId(): service → {Service}", service);

            // Cleanup
            UserController.ClearInstance();
            CBO.ClearInstance();
			Logger.LogDebug("<<< Exiting ContentController_AddContentItem_Sets_Override_CreatedByUserId_And_LastModifiedUserId()");
        }

        [Test]
        public void ContentController_DeleteContentItem_Throws_On_Null_ContentItem()
        {
			Logger.LogDebug(">>> Entering ContentController_DeleteContentItem_Throws_On_Null_ContentItem()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_DeleteContentItem_Throws_On_Null_ContentItem(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_DeleteContentItem_Throws_On_Null_ContentItem(): controller → {Controller}", controller);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => controller.DeleteContentItem(null));
			Logger.LogDebug("<<< Exiting ContentController_DeleteContentItem_Throws_On_Null_ContentItem()");
        }

        [Test]
        public void ContentController_DeleteContentItem_Throws_On_Negative_ContentItemId()
        {
			Logger.LogDebug(">>> Entering ContentController_DeleteContentItem_Throws_On_Negative_ContentItemId()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_DeleteContentItem_Throws_On_Negative_ContentItemId(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_DeleteContentItem_Throws_On_Negative_ContentItemId(): controller → {Controller}", controller);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
            content.ContentItemId = Null.NullInteger;
			Logger.LogDebug("ContentController_DeleteContentItem_Throws_On_Negative_ContentItemId(): content → {Content}", content);
			Logger.LogDebug("ContentController_DeleteContentItem_Throws_On_Negative_ContentItemId(): content.ContentItemId → {Arg0}", content.ContentItemId);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.DeleteContentItem(content));
			Logger.LogDebug("<<< Exiting ContentController_DeleteContentItem_Throws_On_Negative_ContentItemId()");
        }

        [Test]
        public void ContentController_DeleteContentItem_Calls_DataService_On_Valid_ContentItemId()
        {
			Logger.LogDebug(">>> Entering ContentController_DeleteContentItem_Calls_DataService_On_Valid_ContentItemId()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_DeleteContentItem_Calls_DataService_On_Valid_ContentItemId(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_DeleteContentItem_Calls_DataService_On_Valid_ContentItemId(): controller → {Controller}", controller);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
            content.ContentItemId = Constants.CONTENT_DeleteContentItemId;
			Logger.LogDebug("ContentController_DeleteContentItem_Calls_DataService_On_Valid_ContentItemId(): content → {Content}", content);
			Logger.LogDebug("ContentController_DeleteContentItem_Calls_DataService_On_Valid_ContentItemId(): content.ContentItemId → {Arg0}", content.ContentItemId);

            // Act
            controller.DeleteContentItem(content);

            // Assert
            mockDataService.Verify(ds => ds.DeleteContentItem(content.ContentItemId));
			Logger.LogDebug("<<< Exiting ContentController_DeleteContentItem_Calls_DataService_On_Valid_ContentItemId()");
        }

        [Test]
        public void ContentController_GetContentItem_Throws_On_Negative_ContentItemId()
        {
			Logger.LogDebug(">>> Entering ContentController_GetContentItem_Throws_On_Negative_ContentItemId()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_GetContentItem_Throws_On_Negative_ContentItemId(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_GetContentItem_Throws_On_Negative_ContentItemId(): controller → {Controller}", controller);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.GetContentItem(Null.NullInteger));
			Logger.LogDebug("<<< Exiting ContentController_GetContentItem_Throws_On_Negative_ContentItemId()");
        }

        [Test]
        public void ContentController_GetContentItem_Returns_Null_On_InValid_ContentItemId()
        {
			Logger.LogDebug(">>> Entering ContentController_GetContentItem_Returns_Null_On_InValid_ContentItemId()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetContentItem(Constants.CONTENT_InValidContentItemId)).Returns(MockHelper.CreateEmptyContentItemReader());
			Logger.LogDebug("ContentController_GetContentItem_Returns_Null_On_InValid_ContentItemId(): mockDataService → {MockDataService}", mockDataService);
            ContentController controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_GetContentItem_Returns_Null_On_InValid_ContentItemId(): controller → {Controller}", controller);
            // Act
            ContentItem content = controller.GetContentItem(Constants.CONTENT_InValidContentItemId);

			Logger.LogDebug("ContentController_GetContentItem_Returns_Null_On_InValid_ContentItemId(): content → {Content}", content);
            // Assert
            Assert.That(content, Is.Null);
			Logger.LogDebug("<<< Exiting ContentController_GetContentItem_Returns_Null_On_InValid_ContentItemId()");
        }

        [Test]
        public void ContentController_GetContentItem_Calls_DataService_On_Valid_ContentItemId()
        {
			Logger.LogDebug(">>> Entering ContentController_GetContentItem_Calls_DataService_On_Valid_ContentItemId()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetContentItem(Constants.CONTENT_ValidContentItemId)).Returns(MockHelper.CreateValidContentItemReader());
			Logger.LogDebug("ContentController_GetContentItem_Calls_DataService_On_Valid_ContentItemId(): mockDataService → {MockDataService}", mockDataService);
            ContentController controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_GetContentItem_Calls_DataService_On_Valid_ContentItemId(): controller → {Controller}", controller);
            // Act
            ContentItem content = controller.GetContentItem(Constants.CONTENT_ValidContentItemId);

			Logger.LogDebug("ContentController_GetContentItem_Calls_DataService_On_Valid_ContentItemId(): content → {Content}", content);
            // Assert
            mockDataService.Verify(ds => ds.GetContentItem(Constants.CONTENT_ValidContentItemId));
			Logger.LogDebug("<<< Exiting ContentController_GetContentItem_Calls_DataService_On_Valid_ContentItemId()");
        }

        [Test]
        public void ContentController_GetContentItem_Returns_ContentItem_On_Valid_ContentItemId()
        {
			Logger.LogDebug(">>> Entering ContentController_GetContentItem_Returns_ContentItem_On_Valid_ContentItemId()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetContentItem(Constants.CONTENT_ValidContentItemId)).Returns(MockHelper.CreateValidContentItemReader());
			Logger.LogDebug("ContentController_GetContentItem_Returns_ContentItem_On_Valid_ContentItemId(): mockDataService → {MockDataService}", mockDataService);
            ContentController controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_GetContentItem_Returns_ContentItem_On_Valid_ContentItemId(): controller → {Controller}", controller);
            // Act
            ContentItem content = controller.GetContentItem(Constants.CONTENT_ValidContentItemId);

			Logger.LogDebug("ContentController_GetContentItem_Returns_ContentItem_On_Valid_ContentItemId(): content → {Content}", content);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(content.ContentItemId, Is.EqualTo(Constants.CONTENT_ValidContentItemId));
                Assert.That(content.Content, Is.EqualTo(ContentTestHelper.GetContent(Constants.CONTENT_ValidContentItemId)));
                Assert.That(content.ContentKey, Is.EqualTo(ContentTestHelper.GetContentKey(Constants.CONTENT_ValidContentItemId)));
            }
			Logger.LogDebug("<<< Exiting ContentController_GetContentItem_Returns_ContentItem_On_Valid_ContentItemId()");
        }

        [Test]
        public void ContentController_GetContentItemsByTerm_Throws_On_Null_Term()
        {
			Logger.LogDebug(">>> Entering ContentController_GetContentItemsByTerm_Throws_On_Null_Term()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_GetContentItemsByTerm_Throws_On_Null_Term(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_GetContentItemsByTerm_Throws_On_Null_Term(): controller → {Controller}", controller);

            // Act, Arrange
            Assert.Throws<ArgumentException>(() => controller.GetContentItemsByTerm(Null.NullString));
			Logger.LogDebug("<<< Exiting ContentController_GetContentItemsByTerm_Throws_On_Null_Term()");
        }

        [Test]
        public void ContentController_GetContentItemsByTerm_Calls_DataService()
        {
			Logger.LogDebug(">>> Entering ContentController_GetContentItemsByTerm_Calls_DataService()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetContentItemsByTerm(Constants.TERM_ValidName)).Returns(MockHelper.CreateValidContentItemReader());
			Logger.LogDebug("ContentController_GetContentItemsByTerm_Calls_DataService(): mockDataService → {MockDataService}", mockDataService);
            ContentController controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_GetContentItemsByTerm_Calls_DataService(): controller → {Controller}", controller);
            // Act
            IQueryable<ContentItem> contentItems = controller.GetContentItemsByTerm(Constants.TERM_ValidName);

			Logger.LogDebug("ContentController_GetContentItemsByTerm_Calls_DataService(): contentItems → {ContentItems}", contentItems);
            // Assert
            mockDataService.Verify(ds => ds.GetContentItemsByTerm(Constants.TERM_ValidName));
			Logger.LogDebug("<<< Exiting ContentController_GetContentItemsByTerm_Calls_DataService()");
        }

        [Test]
        public void ContentController_GetContentItemsByTerm_Returns_Empty_List_If_Term_Not_Used()
        {
			Logger.LogDebug(">>> Entering ContentController_GetContentItemsByTerm_Returns_Empty_List_If_Term_Not_Used()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetContentItemsByTerm(Constants.TERM_UnusedName)).Returns(MockHelper.CreateEmptyContentItemReader());
			Logger.LogDebug("ContentController_GetContentItemsByTerm_Returns_Empty_List_If_Term_Not_Used(): mockDataService → {MockDataService}", mockDataService);
            ContentController controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_GetContentItemsByTerm_Returns_Empty_List_If_Term_Not_Used(): controller → {Controller}", controller);
            // Act
            IQueryable<ContentItem> contentItems = controller.GetContentItemsByTerm(Constants.TERM_UnusedName);

			Logger.LogDebug("ContentController_GetContentItemsByTerm_Returns_Empty_List_If_Term_Not_Used(): contentItems → {ContentItems}", contentItems);
            // Assert
            Assert.That(contentItems.Count(), Is.EqualTo(0));
			Logger.LogDebug("<<< Exiting ContentController_GetContentItemsByTerm_Returns_Empty_List_If_Term_Not_Used()");
        }

        [Test]
        public void ContentController_GetContentItemsByTerm_Returns_List_Of_ContentItems()
        {
			Logger.LogDebug(">>> Entering ContentController_GetContentItemsByTerm_Returns_List_Of_ContentItems()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetContentItemsByTerm(Constants.TERM_ValidName)).Returns(MockHelper.CreateValidContentItemsReader(
                Constants.CONTENT_TaggedItemCount,
                Constants.CONTENT_IndexedFalse,
                Null.NullInteger,
                Constants.TERM_ValidName));
				Logger.LogDebug("ContentController_GetContentItemsByTerm_Returns_List_Of_ContentItems(): mockDataService → {MockDataService}", mockDataService);
            ContentController controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_GetContentItemsByTerm_Returns_List_Of_ContentItems(): controller → {Controller}", controller);
            // Act
            IQueryable<ContentItem> contentItems = controller.GetContentItemsByTerm(Constants.TERM_ValidName);

			Logger.LogDebug("ContentController_GetContentItemsByTerm_Returns_List_Of_ContentItems(): contentItems → {ContentItems}", contentItems);
            // Assert
            Assert.That(contentItems.Count(), Is.EqualTo(Constants.CONTENT_TaggedItemCount));
			Logger.LogDebug("<<< Exiting ContentController_GetContentItemsByTerm_Returns_List_Of_ContentItems()");
        }

        [Test]
        public void ContentController_GetContentItemsByContentType_Returns_Results()
        {
			Logger.LogDebug(">>> Entering ContentController_GetContentItemsByContentType_Returns_Results()");
            var mock = new Mock<IDataService>();
            mock.Setup(ds => ds.GetContentItemsByContentType(It.IsAny<int>()))
                .Returns(MockHelper.CreateValidContentItemsReader(10, true, 0, null));
				Logger.LogDebug("ContentController_GetContentItemsByContentType_Returns_Results(): mock → {Mock}", mock);

            var controller = new ContentController(mock.Object);

			Logger.LogDebug("ContentController_GetContentItemsByContentType_Returns_Results(): controller → {Controller}", controller);
            var items = controller.GetContentItemsByContentType(10).ToArray();

			Logger.LogDebug("ContentController_GetContentItemsByContentType_Returns_Results(): items → {Items}", items);
            Assert.That(items, Has.Length.EqualTo(10));
			Logger.LogDebug("<<< Exiting ContentController_GetContentItemsByContentType_Returns_Results()");
        }

        [Test]
        public void ContentController_GetContentItemsByContentType_Invalid_Id_Returns_No_Elements()
        {
			Logger.LogDebug(">>> Entering ContentController_GetContentItemsByContentType_Invalid_Id_Returns_No_Elements()");
            var mock = new Mock<IDataService>();
            mock.Setup(ds => ds.GetContentItemsByContentType(It.IsAny<int>())).Returns(MockHelper.CreateEmptyContentItemReader());
			Logger.LogDebug("ContentController_GetContentItemsByContentType_Invalid_Id_Returns_No_Elements(): mock → {Mock}", mock);

            var controller = new ContentController(mock.Object);

			Logger.LogDebug("ContentController_GetContentItemsByContentType_Invalid_Id_Returns_No_Elements(): controller → {Controller}", controller);
            var items = controller.GetContentItemsByContentType(-1).ToArray();

			Logger.LogDebug("ContentController_GetContentItemsByContentType_Invalid_Id_Returns_No_Elements(): items → {Items}", items);
            Assert.That(items, Is.Empty);
			Logger.LogDebug("<<< Exiting ContentController_GetContentItemsByContentType_Invalid_Id_Returns_No_Elements()");
        }

        [Test]
        public void GetContentItemsByModuleId_With_Negative_ModuleId_Returns_ContentItems()
        {
			Logger.LogDebug(">>> Entering GetContentItemsByModuleId_With_Negative_ModuleId_Returns_ContentItems()");
            var mock = new Mock<IDataService>();
            mock.Setup(ds => ds.GetContentItemsByModuleId(-1)).Returns(MockHelper.CreateValidContentItemsReader(10, false, 0, null));
			Logger.LogDebug("GetContentItemsByModuleId_With_Negative_ModuleId_Returns_ContentItems(): mock → {Mock}", mock);
            mock.Setup(ds => ds.GetContentItemsByModuleId(0)).Returns(MockHelper.CreateValidContentItemReader());

            var controller = new ContentController(mock.Object);

			Logger.LogDebug("GetContentItemsByModuleId_With_Negative_ModuleId_Returns_ContentItems(): controller → {Controller}", controller);
            var negative = controller.GetContentItemsByModuleId(-1).ToArray();
            var positive = controller.GetContentItemsByModuleId(0).ToArray();
			Logger.LogDebug("GetContentItemsByModuleId_With_Negative_ModuleId_Returns_ContentItems(): negative → {Negative}", negative);
			Logger.LogDebug("GetContentItemsByModuleId_With_Negative_ModuleId_Returns_ContentItems(): positive → {Positive}", positive);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(negative, Has.Length.EqualTo(10));
                Assert.That(positive, Has.Length.EqualTo(1));
            }
			Logger.LogDebug("<<< Exiting GetContentItemsByModuleId_With_Negative_ModuleId_Returns_ContentItems()");
        }

        [Test]
        public void ContentController_GetUnIndexedContentItems_Calls_DataService()
        {
			Logger.LogDebug(">>> Entering ContentController_GetUnIndexedContentItems_Calls_DataService()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetUnIndexedContentItems()).Returns(MockHelper.CreateValidContentItemReader());
			Logger.LogDebug("ContentController_GetUnIndexedContentItems_Calls_DataService(): mockDataService → {MockDataService}", mockDataService);
            ContentController controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_GetUnIndexedContentItems_Calls_DataService(): controller → {Controller}", controller);
            // Act
            IQueryable<ContentItem> contentItems = controller.GetUnIndexedContentItems();

			Logger.LogDebug("ContentController_GetUnIndexedContentItems_Calls_DataService(): contentItems → {ContentItems}", contentItems);
            // Assert
            mockDataService.Verify(ds => ds.GetUnIndexedContentItems());
			Logger.LogDebug("<<< Exiting ContentController_GetUnIndexedContentItems_Calls_DataService()");
        }

        [Test]
        public void ContentController_GetUnIndexedContentItems_Returns_EmptyList_If_No_UnIndexed_Items()
        {
			Logger.LogDebug(">>> Entering ContentController_GetUnIndexedContentItems_Returns_EmptyList_If_No_UnIndexed_Items()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetUnIndexedContentItems()).Returns(MockHelper.CreateEmptyContentItemReader());
			Logger.LogDebug("ContentController_GetUnIndexedContentItems_Returns_EmptyList_If_No_UnIndexed_Items(): mockDataService → {MockDataService}", mockDataService);

            ContentController controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_GetUnIndexedContentItems_Returns_EmptyList_If_No_UnIndexed_Items(): controller → {Controller}", controller);
            // Act
            IQueryable<ContentItem> contentItems = controller.GetUnIndexedContentItems();

			Logger.LogDebug("ContentController_GetUnIndexedContentItems_Returns_EmptyList_If_No_UnIndexed_Items(): contentItems → {ContentItems}", contentItems);
            // Assert
            Assert.That(contentItems.Count(), Is.EqualTo(0));
			Logger.LogDebug("<<< Exiting ContentController_GetUnIndexedContentItems_Returns_EmptyList_If_No_UnIndexed_Items()");
        }

        [Test]
        public void ContentController_GetUnIndexedContentItems_Returns_List_Of_UnIndexedContentItems()
        {
			Logger.LogDebug(">>> Entering ContentController_GetUnIndexedContentItems_Returns_List_Of_UnIndexedContentItems()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetUnIndexedContentItems()).Returns(MockHelper.CreateValidContentItemsReader(
                Constants.CONTENT_IndexedFalseItemCount,
                Constants.CONTENT_IndexedFalse,
                Null.NullInteger,
                Null.NullString));
				Logger.LogDebug("ContentController_GetUnIndexedContentItems_Returns_List_Of_UnIndexedContentItems(): mockDataService → {MockDataService}", mockDataService);

            ContentController controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_GetUnIndexedContentItems_Returns_List_Of_UnIndexedContentItems(): controller → {Controller}", controller);
            // Act
            IQueryable<ContentItem> contentItems = controller.GetUnIndexedContentItems();

			Logger.LogDebug("ContentController_GetUnIndexedContentItems_Returns_List_Of_UnIndexedContentItems(): contentItems → {ContentItems}", contentItems);
            // Assert
            Assert.That(contentItems.Count(), Is.EqualTo(Constants.CONTENT_IndexedFalseItemCount));
            foreach (ContentItem content in contentItems)
            {
                Assert.That(content.Indexed, Is.False);
            }
			Logger.LogDebug("<<< Exiting ContentController_GetUnIndexedContentItems_Returns_List_Of_UnIndexedContentItems()");
        }

        [Test]
        public void ContentController_UpdateContentItem_Throws_On_Null_ContentItem()
        {
			Logger.LogDebug(">>> Entering ContentController_UpdateContentItem_Throws_On_Null_ContentItem()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_UpdateContentItem_Throws_On_Null_ContentItem(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_UpdateContentItem_Throws_On_Null_ContentItem(): controller → {Controller}", controller);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => controller.UpdateContentItem(null));
			Logger.LogDebug("<<< Exiting ContentController_UpdateContentItem_Throws_On_Null_ContentItem()");
        }

        [Test]
        public void ContentController_UpdateContentItem_Throws_On_Negative_ContentItemId()
        {
			Logger.LogDebug(">>> Entering ContentController_UpdateContentItem_Throws_On_Negative_ContentItemId()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_UpdateContentItem_Throws_On_Negative_ContentItemId(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_UpdateContentItem_Throws_On_Negative_ContentItemId(): controller → {Controller}", controller);

            ContentItem content = new ContentItem();
            content.ContentItemId = Null.NullInteger;
			Logger.LogDebug("ContentController_UpdateContentItem_Throws_On_Negative_ContentItemId(): content → {Content}", content);
			Logger.LogDebug("ContentController_UpdateContentItem_Throws_On_Negative_ContentItemId(): content.ContentItemId → {Arg0}", content.ContentItemId);

            Assert.Throws<ArgumentOutOfRangeException>(() => controller.UpdateContentItem(content));
			Logger.LogDebug("<<< Exiting ContentController_UpdateContentItem_Throws_On_Negative_ContentItemId()");
        }

        [Test]
        public void ContentController_UpdateContentItem_Calls_DataService_On_Valid_ContentItem()
        {
			Logger.LogDebug(">>> Entering ContentController_UpdateContentItem_Calls_DataService_On_Valid_ContentItem()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_UpdateContentItem_Calls_DataService_On_Valid_ContentItem(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_UpdateContentItem_Calls_DataService_On_Valid_ContentItem(): controller → {Controller}", controller);

            ComponentFactory.RegisterComponentInstance<IContentController>(controller);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
            content.ContentItemId = Constants.CONTENT_UpdateContentItemId;
			Logger.LogDebug("ContentController_UpdateContentItem_Calls_DataService_On_Valid_ContentItem(): content → {Content}", content);
			Logger.LogDebug("ContentController_UpdateContentItem_Calls_DataService_On_Valid_ContentItem(): content.ContentItemId → {Arg0}", content.ContentItemId);
			Logger.LogDebug("ContentController_UpdateContentItem_Calls_DataService_On_Valid_ContentItem(): content.Content → {Arg0}", content.Content);
			Logger.LogDebug("ContentController_UpdateContentItem_Calls_DataService_On_Valid_ContentItem(): content.ContentKey → {Arg0}", content.ContentKey);
            content.Content = Constants.CONTENT_UpdateContent;
            content.ContentKey = Constants.CONTENT_UpdateContentKey;

            // Act
            controller.UpdateContentItem(content);

            // Assert
            mockDataService.Verify(ds => ds.UpdateContentItem(content, It.IsAny<int>()));
			Logger.LogDebug("<<< Exiting ContentController_UpdateContentItem_Calls_DataService_On_Valid_ContentItem()");
        }

        [Test]
        public void ContentController_AddMetaData_Throws_On_Null_ContentItem()
        {
			Logger.LogDebug(">>> Entering ContentController_AddMetaData_Throws_On_Null_ContentItem()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_AddMetaData_Throws_On_Null_ContentItem(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_AddMetaData_Throws_On_Null_ContentItem(): controller → {Controller}", controller);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => controller.AddMetaData(null, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue));
			Logger.LogDebug("<<< Exiting ContentController_AddMetaData_Throws_On_Null_ContentItem()");
        }

        [Test]
        public void ContentController_AddMetaData_Throws_On_Negative_ContentItemId()
        {
			Logger.LogDebug(">>> Entering ContentController_AddMetaData_Throws_On_Negative_ContentItemId()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_AddMetaData_Throws_On_Negative_ContentItemId(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_AddMetaData_Throws_On_Negative_ContentItemId(): controller → {Controller}", controller);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
            content.ContentItemId = Null.NullInteger;
			Logger.LogDebug("ContentController_AddMetaData_Throws_On_Negative_ContentItemId(): content → {Content}", content);
			Logger.LogDebug("ContentController_AddMetaData_Throws_On_Negative_ContentItemId(): content.ContentItemId → {Arg0}", content.ContentItemId);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.AddMetaData(content, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue));
			Logger.LogDebug("<<< Exiting ContentController_AddMetaData_Throws_On_Negative_ContentItemId()");
        }

        [Test]
        public void ContentController_AddMetaData_Throws_On_Null_MetaDataName()
        {
			Logger.LogDebug(">>> Entering ContentController_AddMetaData_Throws_On_Null_MetaDataName()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_AddMetaData_Throws_On_Null_MetaDataName(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_AddMetaData_Throws_On_Null_MetaDataName(): controller → {Controller}", controller);

            ContentItem content = ContentTestHelper.CreateValidContentItem();

			Logger.LogDebug("ContentController_AddMetaData_Throws_On_Null_MetaDataName(): content → {Content}", content);
            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.AddMetaData(content, Null.NullString, Constants.CONTENT_ValidMetaDataValue));
			Logger.LogDebug("<<< Exiting ContentController_AddMetaData_Throws_On_Null_MetaDataName()");
        }

        [Test]
        public void ContentController_AddMetaData_Calls_DataService_On_Valid_Arguments()
        {
			Logger.LogDebug(">>> Entering ContentController_AddMetaData_Calls_DataService_On_Valid_Arguments()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_AddMetaData_Calls_DataService_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_AddMetaData_Calls_DataService_On_Valid_Arguments(): controller → {Controller}", controller);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
            content.ContentItemId = Constants.CONTENT_ValidContentItemId;
			Logger.LogDebug("ContentController_AddMetaData_Calls_DataService_On_Valid_Arguments(): content → {Content}", content);
			Logger.LogDebug("ContentController_AddMetaData_Calls_DataService_On_Valid_Arguments(): content.ContentItemId → {Arg0}", content.ContentItemId);

            // Act
            controller.AddMetaData(content, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue);

            // Assert
            mockDataService.Verify(ds => ds.AddMetaData(content, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue));
			Logger.LogDebug("<<< Exiting ContentController_AddMetaData_Calls_DataService_On_Valid_Arguments()");
        }

        [Test]
        public void ContentController_DeleteMetaData_Throws_On_Null_ContentItem()
        {
			Logger.LogDebug(">>> Entering ContentController_DeleteMetaData_Throws_On_Null_ContentItem()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_DeleteMetaData_Throws_On_Null_ContentItem(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_DeleteMetaData_Throws_On_Null_ContentItem(): controller → {Controller}", controller);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => controller.AddMetaData(null, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue));
			Logger.LogDebug("<<< Exiting ContentController_DeleteMetaData_Throws_On_Null_ContentItem()");
        }

        [Test]
        public void ContentController_DeleteMetaData_Throws_On_Negative_ContentItemId()
        {
			Logger.LogDebug(">>> Entering ContentController_DeleteMetaData_Throws_On_Negative_ContentItemId()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_DeleteMetaData_Throws_On_Negative_ContentItemId(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_DeleteMetaData_Throws_On_Negative_ContentItemId(): controller → {Controller}", controller);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
            content.ContentItemId = Null.NullInteger;
			Logger.LogDebug("ContentController_DeleteMetaData_Throws_On_Negative_ContentItemId(): content → {Content}", content);
			Logger.LogDebug("ContentController_DeleteMetaData_Throws_On_Negative_ContentItemId(): content.ContentItemId → {Arg0}", content.ContentItemId);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.DeleteMetaData(content, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue));
			Logger.LogDebug("<<< Exiting ContentController_DeleteMetaData_Throws_On_Negative_ContentItemId()");
        }

        [Test]
        public void ContentController_DeleteMetaData_Throws_On_Null_MetaDataName()
        {
			Logger.LogDebug(">>> Entering ContentController_DeleteMetaData_Throws_On_Null_MetaDataName()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_DeleteMetaData_Throws_On_Null_MetaDataName(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_DeleteMetaData_Throws_On_Null_MetaDataName(): controller → {Controller}", controller);

            ContentItem content = ContentTestHelper.CreateValidContentItem();

			Logger.LogDebug("ContentController_DeleteMetaData_Throws_On_Null_MetaDataName(): content → {Content}", content);
            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.AddMetaData(content, Null.NullString, Constants.CONTENT_ValidMetaDataValue));
			Logger.LogDebug("<<< Exiting ContentController_DeleteMetaData_Throws_On_Null_MetaDataName()");
        }

        [Test]
        public void ContentController_DeleteMetaData_Calls_DataService_On_Valid_Arguments()
        {
			Logger.LogDebug(">>> Entering ContentController_DeleteMetaData_Calls_DataService_On_Valid_Arguments()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_DeleteMetaData_Calls_DataService_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_DeleteMetaData_Calls_DataService_On_Valid_Arguments(): controller → {Controller}", controller);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
            content.ContentItemId = Constants.CONTENT_ValidContentItemId;
			Logger.LogDebug("ContentController_DeleteMetaData_Calls_DataService_On_Valid_Arguments(): content → {Content}", content);
			Logger.LogDebug("ContentController_DeleteMetaData_Calls_DataService_On_Valid_Arguments(): content.ContentItemId → {Arg0}", content.ContentItemId);

            // Act
            controller.DeleteMetaData(content, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue);

            // Assert
            mockDataService.Verify(ds => ds.DeleteMetaData(content, Constants.CONTENT_ValidMetaDataName, Constants.CONTENT_ValidMetaDataValue));
			Logger.LogDebug("<<< Exiting ContentController_DeleteMetaData_Calls_DataService_On_Valid_Arguments()");
        }

        [Test]
        public void ContentController_GetMetaData_Throws_On_Negative_ContentItemId()
        {
			Logger.LogDebug(">>> Entering ContentController_GetMetaData_Throws_On_Negative_ContentItemId()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            ContentController controller = new ContentController(mockDataService.Object);
			Logger.LogDebug("ContentController_GetMetaData_Throws_On_Negative_ContentItemId(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ContentController_GetMetaData_Throws_On_Negative_ContentItemId(): controller → {Controller}", controller);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.GetMetaData(Null.NullInteger));
			Logger.LogDebug("<<< Exiting ContentController_GetMetaData_Throws_On_Negative_ContentItemId()");
        }

        [Test]
        public void ContentController_GetMetaData_Calls_DataService_On_Valid_Arguments()
        {
			Logger.LogDebug(">>> Entering ContentController_GetMetaData_Calls_DataService_On_Valid_Arguments()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetMetaData(Constants.CONTENT_ValidContentItemId)).Returns(MockHelper.CreateValidMetaDataReader());
			Logger.LogDebug("ContentController_GetMetaData_Calls_DataService_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
            ContentController controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_GetMetaData_Calls_DataService_On_Valid_Arguments(): controller → {Controller}", controller);
            // Act
            NameValueCollection metaData = controller.GetMetaData(Constants.CONTENT_ValidContentItemId);

			Logger.LogDebug("ContentController_GetMetaData_Calls_DataService_On_Valid_Arguments(): metaData → {MetaData}", metaData);
            // Assert
            mockDataService.Verify(ds => ds.GetMetaData(Constants.CONTENT_ValidContentItemId));
			Logger.LogDebug("<<< Exiting ContentController_GetMetaData_Calls_DataService_On_Valid_Arguments()");
        }

        [Test]
        public void ContentController_GetMetaData_Returns_NameValueCollection_Of_MetaData()
        {
			Logger.LogDebug(">>> Entering ContentController_GetMetaData_Returns_NameValueCollection_Of_MetaData()");
            // Arrange
            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetMetaData(Constants.CONTENT_ValidContentItemId)).Returns(MockHelper.CreateValidMetaDataReader());
			Logger.LogDebug("ContentController_GetMetaData_Returns_NameValueCollection_Of_MetaData(): mockDataService → {MockDataService}", mockDataService);

            mockDataService.Setup(ds =>
             ds.SynchronizeMetaData(
                 It.IsAny<ContentItem>(),
                 It.IsAny<IEnumerable<KeyValuePair<string, string>>>(),
                 It.IsAny<IEnumerable<KeyValuePair<string, string>>>()))
             .Callback<ContentItem, IEnumerable<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>>(
                 (ci, added, deleted) =>
                 {
                     deleted.ToList().ForEach(
                         item => mockDataService.Object.DeleteMetaData(ci, item.Key, item.Value));
						Logger.LogDebug("ContentController_GetMetaData_Returns_NameValueCollection_Of_MetaData(): item → {Item}", item);

                     added.ToList().ForEach(
                         item => mockDataService.Object.AddMetaData(ci, item.Key, item.Value));
                 });

            var controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_GetMetaData_Returns_NameValueCollection_Of_MetaData(): controller → {Controller}", controller);
            // Act
            var metaData = controller.GetMetaData(Constants.CONTENT_ValidContentItemId);

			Logger.LogDebug("ContentController_GetMetaData_Returns_NameValueCollection_Of_MetaData(): metaData → {MetaData}", metaData);
            // Assert
            Assert.That(metaData, Has.Count.EqualTo(Constants.CONTENT_MetaDataCount));
			Logger.LogDebug("<<< Exiting ContentController_GetMetaData_Returns_NameValueCollection_Of_MetaData()");
        }

        [Test]
        public void ContentController_Title_Is_Saved_On_Add()
        {
			Logger.LogDebug(">>> Entering ContentController_Title_Is_Saved_On_Add()");
            var mockDataService = new Mock<IDataService>();

			Logger.LogDebug("ContentController_Title_Is_Saved_On_Add(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(
                ds =>
                    ds.AddContentItem(
                    It.IsAny<ContentItem>(),
                    It.IsAny<int>()))
                .Returns(Constants.CONTENT_AddContentItemId);

            mockDataService.Setup(ds =>
             ds.SynchronizeMetaData(
                 It.IsAny<ContentItem>(),
                 It.IsAny<IEnumerable<KeyValuePair<string, string>>>(),
                 It.IsAny<IEnumerable<KeyValuePair<string, string>>>()))
             .Callback<ContentItem, IEnumerable<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>>(
                 (ci, added, deleted) =>
                 {
                     deleted.ToList().ForEach(
                         item => mockDataService.Object.DeleteMetaData(ci, item.Key, item.Value));
						Logger.LogDebug("ContentController_Title_Is_Saved_On_Add(): item → {Item}", item);

                     added.ToList().ForEach(
                         item => mockDataService.Object.AddMetaData(ci, item.Key, item.Value));
                 });

            // Return empty set of metadata.
            mockDataService.Setup(ds => ds.GetMetaData(It.IsAny<int>())).Returns(MockHelper.CreateValidMetaDataReader);

            var controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_Title_Is_Saved_On_Add(): controller → {Controller}", controller);
            // The ContentExtensions methods look this up.
            ComponentFactory.RegisterComponentInstance<IContentController>(controller);

            var content = ContentTestHelper.CreateValidContentItem();
            content.ContentItemId = Constants.CONTENT_ValidContentItemId;
			Logger.LogDebug("ContentController_Title_Is_Saved_On_Add(): content → {Content}", content);
			Logger.LogDebug("ContentController_Title_Is_Saved_On_Add(): content.ContentItemId → {Arg0}", content.ContentItemId);
			Logger.LogDebug("ContentController_Title_Is_Saved_On_Add(): content.ContentTitle → {Arg0}", content.ContentTitle);
            content.ContentTitle = Constants.CONTENT_ValidTitle;

            // Act
            controller.AddContentItem(content);

            // Assert
            mockDataService.Verify(ds => ds.AddMetaData(content, AttachmentController.TitleKey, Constants.CONTENT_ValidTitle));
			Logger.LogDebug("<<< Exiting ContentController_Title_Is_Saved_On_Add()");
        }

        [Test]
        public void ContentController_Title_Is_Saved_On_Update()
        {
			Logger.LogDebug(">>> Entering ContentController_Title_Is_Saved_On_Update()");
            var mockDataService = new Mock<IDataService>();

			Logger.LogDebug("ContentController_Title_Is_Saved_On_Update(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.AddContentItem(It.IsAny<ContentItem>(), It.IsAny<int>()))
                .Returns(Constants.CONTENT_AddContentItemId);

            mockDataService.Setup(ds =>
             ds.SynchronizeMetaData(
                 It.IsAny<ContentItem>(),
                 It.IsAny<IEnumerable<KeyValuePair<string, string>>>(),
                 It.IsAny<IEnumerable<KeyValuePair<string, string>>>()))
             .Callback<ContentItem, IEnumerable<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>>(
                 (ci, added, deleted) =>
                 {
                     deleted.ToList().ForEach(
                         item => mockDataService.Object.DeleteMetaData(ci, item.Key, item.Value));
						Logger.LogDebug("ContentController_Title_Is_Saved_On_Update(): item → {Item}", item);

                     added.ToList().ForEach(
                         item => mockDataService.Object.AddMetaData(ci, item.Key, item.Value));
                 });

            // Return empty set of metadata.
            mockDataService.Setup(ds => ds.GetMetaData(It.IsAny<int>())).Returns(MockHelper.CreateValidMetaDataReader);

            var controller = new ContentController(mockDataService.Object);

			Logger.LogDebug("ContentController_Title_Is_Saved_On_Update(): controller → {Controller}", controller);
            // The ContentExtensions methods look this up.
            ComponentFactory.RegisterComponentInstance<IContentController>(controller);

            var content = ContentTestHelper.CreateValidContentItem();
            content.ContentItemId = Constants.CONTENT_ValidContentItemId;
			Logger.LogDebug("ContentController_Title_Is_Saved_On_Update(): content → {Content}", content);
			Logger.LogDebug("ContentController_Title_Is_Saved_On_Update(): content.ContentItemId → {Arg0}", content.ContentItemId);
			Logger.LogDebug("ContentController_Title_Is_Saved_On_Update(): content.ContentTitle → {Arg0}", content.ContentTitle);
            content.ContentTitle = Constants.CONTENT_ValidTitle;

            // Act
            controller.AddContentItem(content);

            content.ContentTitle = Constants.CONTENT_ValidTitle2;
            controller.UpdateContentItem(content);

            // Assert
            mockDataService.Verify(ds => ds.AddMetaData(content, AttachmentController.TitleKey, Constants.CONTENT_ValidTitle));
            mockDataService.Verify(ds => ds.AddMetaData(content, AttachmentController.TitleKey, Constants.CONTENT_ValidTitle2));
			Logger.LogDebug("<<< Exiting ContentController_Title_Is_Saved_On_Update()");
        }
    }
}
