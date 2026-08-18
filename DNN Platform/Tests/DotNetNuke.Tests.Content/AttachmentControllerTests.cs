// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Content
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Tests.Content.Mocks;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using FileController = DotNetNuke.Entities.Content.AttachmentController;
    using Util = DotNetNuke.Entities.Content.Common.Util;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class AttachmentControllerTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private Mock<CachingProvider> mockCache;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			_logger.LogDebug(">>> Entering SetUp()");
            // Register MockCachingProvider
            this.mockCache = MockComponentProvider.CreateDataCacheProvider();
			_logger.LogDebug("SetUp(): mockCache → {MockCache}", mockCache);
            var dataProvider = MockComponentProvider.CreateDataProvider();
			_logger.LogDebug("SetUp(): dataProvider → {DataProvider}", dataProvider);
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
        public void Test_Add_File_To_Content_Item_Without_Metadata()
        {
			_logger.LogDebug(">>> Entering Test_Add_File_To_Content_Item_Without_Metadata()");
            var dataService = DataServiceFactory();

			_logger.LogDebug("Test_Add_File_To_Content_Item_Without_Metadata(): dataService → {DataService}", dataService);
            dataService.Setup(ds => ds.AddContentItem(It.IsAny<ContentItem>(), It.IsAny<int>()))
                .Returns(Constants.CONTENT_AddContentItemId);

            // Return empty set of metadata.
            dataService.Setup(ds => ds.GetMetaData(It.IsAny<int>())).Returns(MockHelper.CreateEmptyMetaDataReader);

            var content = ContentTestHelper.CreateValidContentItem();
			_logger.LogDebug("Test_Add_File_To_Content_Item_Without_Metadata(): content → {Content}", content);
            content.Metadata.Clear();

            var contentId = Util.GetContentController().AddContentItem(content);
			_logger.LogDebug("Test_Add_File_To_Content_Item_Without_Metadata(): contentId → {ContentId}", contentId);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(contentId, Is.EqualTo(Constants.CONTENT_AddContentItemId));
                Assert.That(content.Metadata, Is.Empty);
            }

            dataService.Setup(ds => ds.GetContentItem(It.IsAny<int>()))
                .Returns<int>(y => MockHelper.CreateValidContentItemReader(content));

            var fileController = ComponentFactory.GetComponent<IAttachmentController>();
			_logger.LogDebug("Test_Add_File_To_Content_Item_Without_Metadata(): fileController → {FileController}", fileController);

            fileController.AddFileToContent(contentId, ContentTestHelper.CreateValidFile(0));

            dataService.Verify(
                ds => ds.AddMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, new[] { 0 }.ToJson()));
				_logger.LogDebug("<<< Exiting Test_Add_File_To_Content_Item_Without_Metadata()");
        }

        [Test]
        public void Test_Load_Attachments_From_DataService()
        {
			_logger.LogDebug(">>> Entering Test_Load_Attachments_From_DataService()");
            var files = new List<IFileInfo>
                {
                    ContentTestHelper.CreateValidFile(0),
                    ContentTestHelper.CreateValidFile(1),
                    ContentTestHelper.CreateValidFile(2),
                };

            var dataService = DataServiceFactory();
			_logger.LogDebug("Test_Load_Attachments_From_DataService(): dataService → {DataService}", dataService);

            dataService.Setup(ds => ds.GetContentItem(It.IsAny<int>())).Returns(MockHelper.CreateValidContentItemReader);

            dataService.Setup(
                ds =>
                    ds.GetMetaData(It.IsAny<int>())).Returns(
                        () => MockHelper.CreateMetaDataReaderWithFiles(files, new IFileInfo[0], new IFileInfo[0]));

            var contentItem = Util.GetContentController().GetContentItem(Constants.CONTENT_ValidContentItemId);
			_logger.LogDebug("Test_Load_Attachments_From_DataService(): contentItem → {ContentItem}", contentItem);
            Assert.That(contentItem, Is.Not.Null);

            var serialized = contentItem.Metadata[FileController.FilesKey];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(serialized, Is.Not.Empty);

                Assert.That(contentItem.Files, Is.Not.Empty);
            }
            Assert.That(contentItem.Files, Has.Count.EqualTo(3));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(contentItem.Files[0].FileId, Is.EqualTo(0));
                Assert.That(contentItem.Files[1].FileId, Is.EqualTo(1));
                Assert.That(contentItem.Files[2].FileId, Is.EqualTo(2));
            }
			_logger.LogDebug("<<< Exiting Test_Load_Attachments_From_DataService()");
        }

        [Test]
        public void Test_Add_Attachments_With_FileController()
        {
			_logger.LogDebug(">>> Entering Test_Add_Attachments_With_FileController()");
            var dataService = DataServiceFactory();

			_logger.LogDebug("Test_Add_Attachments_With_FileController(): dataService → {DataService}", dataService);
            dataService.Setup(
                ds =>
                    ds.GetContentItem(It.IsAny<int>())).Returns(MockHelper.CreateValidContentItemReader);

            // Use a closure to store the metadata locally in this method.
            var data = new Dictionary<string, string>();
			_logger.LogDebug("Test_Add_Attachments_With_FileController(): data → {Data}", data);

            dataService.Setup(
                ds =>
                    ds.GetMetaData(It.IsAny<int>())).Returns(
                        () => MockHelper.CreateMetaDataReaderFromDictionary(data));

            dataService.Setup(
                    ds =>
                        ds.AddMetaData(
                            It.IsAny<ContentItem>(),
                            It.IsAny<string>(),
                            It.IsAny<string>()))
                .Callback<ContentItem, string, string>((ci, name, value) => data[name] = value);

            var contentController = Util.GetContentController();
			_logger.LogDebug("Test_Add_Attachments_With_FileController(): contentController → {ContentController}", contentController);

            var contentItem = contentController.GetContentItem(Constants.CONTENT_ValidContentItemId);
			_logger.LogDebug("Test_Add_Attachments_With_FileController(): contentItem → {ContentItem}", contentItem);
            Assert.That(contentItem, Is.Not.Null);

            var serialized = contentItem.Metadata[FileController.FilesKey];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(serialized, Is.Null);
                Assert.That(contentItem.Files, Is.Empty);
            }

            var fileManager = ComponentFactory.GetComponent<IFileManager>();
			_logger.LogDebug("Test_Add_Attachments_With_FileController(): fileManager → {FileManager}", fileManager);

            // Add some files.
            var fileController = ComponentFactory.GetComponent<IAttachmentController>();
			_logger.LogDebug("Test_Add_Attachments_With_FileController(): fileController → {FileController}", fileController);
            fileController.AddFileToContent(contentItem.ContentItemId, fileManager.GetFile(0));
            fileController.AddFileToContent(contentItem.ContentItemId, fileManager.GetFile(1));

            contentItem = contentController.GetContentItem(Constants.CONTENT_ValidContentItemId);

            Assert.That(contentItem.Files, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(contentItem.Files[0].FileId, Is.EqualTo(0));
                Assert.That(contentItem.Files[1].FileId, Is.EqualTo(1));
            }

            dataService.Verify(
                ds => ds.DeleteMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, "[0]"), Times.Once());

            dataService.Verify(
                ds => ds.AddMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, "[0]"), Times.Once());

            dataService.Verify(
                ds => ds.AddMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, "[0,1]"), Times.Once());
				_logger.LogDebug("<<< Exiting Test_Add_Attachments_With_FileController()");
        }

        [Test]
        public void Set_MetaData_To_Empty_Value_Deletes_Row()
        {
			_logger.LogDebug(">>> Entering Set_MetaData_To_Empty_Value_Deletes_Row()");
            var data = new Dictionary<string, string>();

			_logger.LogDebug("Set_MetaData_To_Empty_Value_Deletes_Row(): data → {Data}", data);
            var dataService = DataServiceFactoryWithLocalMetaData(ref data);
			_logger.LogDebug("Set_MetaData_To_Empty_Value_Deletes_Row(): dataService → {DataService}", dataService);

            dataService.Setup(ds => ds.GetContentItem(It.IsAny<int>())).Returns(MockHelper.CreateValidContentItemReader);

            var contentController = Util.GetContentController();
			_logger.LogDebug("Set_MetaData_To_Empty_Value_Deletes_Row(): contentController → {ContentController}", contentController);

            var fileManager = ComponentFactory.GetComponent<IFileManager>();
			_logger.LogDebug("Set_MetaData_To_Empty_Value_Deletes_Row(): fileManager → {FileManager}", fileManager);

            var contentItem = contentController.GetContentItem(Constants.CONTENT_ValidContentItemId);
			_logger.LogDebug("Set_MetaData_To_Empty_Value_Deletes_Row(): contentItem → {ContentItem}", contentItem);
            Assert.That(contentItem, Is.Not.Null);

            var serialized = contentItem.Metadata[FileController.FilesKey];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(serialized, Is.Null);
                Assert.That(contentItem.Files, Is.Empty);
            }

            // Add some files.
            var fileController = ComponentFactory.GetComponent<IAttachmentController>();
			_logger.LogDebug("Set_MetaData_To_Empty_Value_Deletes_Row(): fileController → {FileController}", fileController);
            fileController.AddFileToContent(contentItem.ContentItemId, fileManager.GetFile(0));
            fileController.AddFileToContent(contentItem.ContentItemId, fileManager.GetFile(1));

            contentItem = contentController.GetContentItem(Constants.CONTENT_ValidContentItemId);

            Assert.That(contentItem.Files, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(contentItem.Files[0].FileId, Is.EqualTo(0));
                Assert.That(contentItem.Files[1].FileId, Is.EqualTo(1));
            }
            Assert.That(contentItem.Metadata[FileController.FilesKey], Is.Not.Empty);

            contentItem.Files.Clear();

            contentController.UpdateContentItem(contentItem);

            dataService.Verify(ds => ds.DeleteMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, "[0]"), Times.Once());
            dataService.Verify(ds => ds.DeleteMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, "[0,1]"), Times.Once());
            dataService.Verify(ds => ds.AddMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, "[0]"), Times.Once());
            dataService.Verify(ds => ds.AddMetaData(It.IsAny<ContentItem>(), FileController.FilesKey, "[0,1]"), Times.Once());

            var emptyFiles = fileController.GetFilesByContent(contentItem.ContentItemId);
			_logger.LogDebug("Set_MetaData_To_Empty_Value_Deletes_Row(): emptyFiles → {EmptyFiles}", emptyFiles);
            Assert.That(emptyFiles, Is.Empty);
			_logger.LogDebug("<<< Exiting Set_MetaData_To_Empty_Value_Deletes_Row()");
        }

        /// <remarks>This test should be moved elsewhere (cb).</remarks>
        [Test]
        public void Set_MetaData_To_Same_Value_Doesnt_Update_Database_Entry()
        {
			_logger.LogDebug(">>> Entering Set_MetaData_To_Same_Value_Doesnt_Update_Database_Entry()");
            var metadata = new Dictionary<string, string>();

			_logger.LogDebug("Set_MetaData_To_Same_Value_Doesnt_Update_Database_Entry(): metadata → {Metadata}", metadata);
            var dataService = DataServiceFactoryWithLocalMetaData(ref metadata);
			_logger.LogDebug("Set_MetaData_To_Same_Value_Doesnt_Update_Database_Entry(): dataService → {DataService}", dataService);

            var contentController = Util.GetContentController();
			_logger.LogDebug("Set_MetaData_To_Same_Value_Doesnt_Update_Database_Entry(): contentController → {ContentController}", contentController);

            var contentItem = ContentTestHelper.CreateValidContentItem();
			_logger.LogDebug("Set_MetaData_To_Same_Value_Doesnt_Update_Database_Entry(): contentItem → {ContentItem}", contentItem);

            contentController.AddContentItem(contentItem);

            dataService.Verify(ds => ds.AddMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Never());
            dataService.Verify(ds => ds.DeleteMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Never());

            contentItem.ContentTitle = "Foobar";
			_logger.LogDebug("Set_MetaData_To_Same_Value_Doesnt_Update_Database_Entry(): contentItem.ContentTitle → {Arg0}", contentItem.ContentTitle);

            contentController.UpdateContentItem(contentItem);

            dataService.Verify(ds => ds.AddMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Once());
            dataService.Verify(ds => ds.DeleteMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Never());

            contentItem.ContentTitle = "Foobar";

            contentController.UpdateContentItem(contentItem);

            // Should be a no-op since no real data changed
            dataService.Verify(ds => ds.AddMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Once());
            dataService.Verify(ds => ds.DeleteMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Never());

            // Really update
            contentItem.ContentTitle = "SNAFU";

            contentController.UpdateContentItem(contentItem);

            dataService.Verify(ds => ds.AddMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Exactly(2));
            dataService.Verify(ds => ds.DeleteMetaData(contentItem, FileController.TitleKey, It.IsAny<string>()), Times.Once());
			_logger.LogDebug("<<< Exiting Set_MetaData_To_Same_Value_Doesnt_Update_Database_Entry()");
        }

        private static Mock<IDataService> DataServiceFactory()
        {
			_logger.LogDebug(">>> Entering DataServiceFactory()");
            var dataService = new Mock<IDataService>();

			_logger.LogDebug("DataServiceFactory(): dataService → {DataService}", dataService);
            dataService.Setup(ds =>
             ds.SynchronizeMetaData(
                 It.IsAny<ContentItem>(),
                 It.IsAny<IEnumerable<KeyValuePair<string, string>>>(),
                 It.IsAny<IEnumerable<KeyValuePair<string, string>>>()))
             .Callback<ContentItem, IEnumerable<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>>(
                 (ci, added, deleted) =>
                 {
                     deleted.ToList().ForEach(
                         item => dataService.Object.DeleteMetaData(ci, item.Key, item.Value));

                     added.ToList().ForEach(
                         item => dataService.Object.AddMetaData(ci, item.Key, item.Value));
                 });

            // Register controller types that are dependent on our IDataService.
            var contentController = new ContentController(dataService.Object);
			_logger.LogDebug("DataServiceFactory(): contentController → {ContentController}", contentController);

            ComponentFactory.RegisterComponentInstance<IAttachmentController>(new FileController(contentController));
            ComponentFactory.RegisterComponentInstance<IContentController>(contentController);
            ComponentFactory.RegisterComponentInstance<IFileManager>(MockHelper.CreateMockFileManager().Object);

			_logger.LogDebug("<<< Exiting DataServiceFactory()");
            return dataService;
        }

        private static Mock<IDataService> DataServiceFactoryWithLocalMetaData(ref Dictionary<string, string> metadata)
        {
			_logger.LogDebug(">>> Entering DataServiceFactoryWithLocalMetaData(metadata={Metadata})", metadata);
            var dataService = DataServiceFactory();

			_logger.LogDebug("DataServiceFactoryWithLocalMetaData(metadata={Metadata}): dataService → {DataService}", metadata, dataService);
            var closure = metadata;

            dataService.Setup(ds => ds.GetMetaData(It.IsAny<int>())).Returns(() => MockHelper.CreateMetaDataReaderFromDictionary(closure));
            dataService.Setup(ds => ds.AddMetaData(It.IsAny<ContentItem>(), It.IsAny<string>(), It.IsAny<string>())).
                Callback<ContentItem, string, string>((ci, name, value) => closure[name] = value);
            dataService.Setup(ds => ds.DeleteMetaData(It.IsAny<ContentItem>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<ContentItem, string, string>((ci, key, val) => closure.Remove(key));

			_logger.LogDebug("<<< Exiting DataServiceFactoryWithLocalMetaData(metadata={Metadata})", metadata);
            return dataService;
        }
    }
}
