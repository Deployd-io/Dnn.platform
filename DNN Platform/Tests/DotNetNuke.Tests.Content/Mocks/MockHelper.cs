// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Content.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;

    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Web.Validators;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using FileController = DotNetNuke.Entities.Content.AttachmentController;

    public static class MockHelper
    {
        internal static IQueryable<Vocabulary> TestVocabularies
        {
            get
            {
                List<Vocabulary> vocabularies = [];

                for (int i = Constants.VOCABULARY_ValidVocabularyId; i < Constants.VOCABULARY_ValidVocabularyId + Constants.VOCABULARY_ValidCount; i++)
                {
                    Vocabulary vocabulary = new Vocabulary
                    {
                        VocabularyId = i,
                        Name = ContentTestHelper.GetVocabularyName(i),
                        Type =
                            (i == Constants.VOCABULARY_HierarchyVocabularyId)
                                ? VocabularyType.Hierarchy
                                : VocabularyType.Simple,
                        Description = ContentTestHelper.GetVocabularyName(i),
                        ScopeTypeId = Constants.SCOPETYPE_ValidScopeTypeId,
                        Weight = Constants.VOCABULARY_ValidWeight,
                    };

                    vocabularies.Add(vocabulary);
                }

                return vocabularies.AsQueryable();
            }
        }

        internal static IDataReader CreateEmptyContentItemReader()
        {
			Logger.LogDebug(">>> Entering CreateEmptyContentItemReader()");
			Logger.LogDebug("<<< Exiting CreateEmptyContentItemReader()");
            return CreateContentItemTable().CreateDataReader();
        }

        internal static IDataReader CreateEmptyContentTypeReader()
        {
			Logger.LogDebug(">>> Entering CreateEmptyContentTypeReader()");
			Logger.LogDebug("<<< Exiting CreateEmptyContentTypeReader()");
            return CreateContentTypeTable().CreateDataReader();
        }

        internal static IDataReader CreateEmptyScopeTypeReader()
        {
			Logger.LogDebug(">>> Entering CreateEmptyScopeTypeReader()");
			Logger.LogDebug("<<< Exiting CreateEmptyScopeTypeReader()");
            return CreateScopeTypeTable().CreateDataReader();
        }

        internal static IDataReader CreateEmptyTermReader()
        {
			Logger.LogDebug(">>> Entering CreateEmptyTermReader()");
			Logger.LogDebug("<<< Exiting CreateEmptyTermReader()");
            return CreateTermTable().CreateDataReader();
        }

        internal static Mock<HttpContextBase> CreateMockHttpContext()
        {
			Logger.LogDebug(">>> Entering CreateMockHttpContext()");
            Mock<HttpContextBase> httpContext = new Mock<HttpContextBase>();
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();
			Logger.LogDebug("CreateMockHttpContext(): httpContext → {HttpContext}", httpContext);
			Logger.LogDebug("CreateMockHttpContext(): httpResponse → {HttpResponse}", httpResponse);
            httpContext.Setup(h => h.Response).Returns(httpResponse.Object);

			Logger.LogDebug("<<< Exiting CreateMockHttpContext()");
            return httpContext;
        }

        internal static Mock<IVocabularyController> CreateMockVocabularyController()
        {
			Logger.LogDebug(">>> Entering CreateMockVocabularyController()");
            var needsServiceProvider = Globals.DependencyProvider is null;
            var provider = needsServiceProvider ? FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<ICryptographyProvider>())) : Globals.DependencyProvider;
            try
			Logger.LogDebug("CreateMockVocabularyController(): provider → {Provider}", provider);
            {
                // Create the mock
                var mockVocabularies = new Mock<IVocabularyController>();
                mockVocabularies.Setup(v => v.GetVocabularies()).Returns(TestVocabularies);
				Logger.LogDebug("CreateMockVocabularyController(): mockVocabularies → {MockVocabularies}", mockVocabularies);

                // Register Mock
				Logger.LogDebug("<<< Exiting CreateMockVocabularyController()");
                return RegisterMockController(mockVocabularies);
            }
            finally
            {
                if (provider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        internal static IDataReader CreateValidContentItemReader()
        {
			Logger.LogDebug(">>> Entering CreateValidContentItemReader()");
            DataTable table = CreateContentItemTable();
            AddContentItemToTable(
                table,
                Constants.CONTENT_ValidContentItemId,
                ContentTestHelper.GetContent(Constants.CONTENT_ValidContentItemId),
                ContentTestHelper.GetContentKey(Constants.CONTENT_ValidContentItemId),
                true,
                Constants.USER_ValidId,
                Null.NullString);
				Logger.LogDebug("CreateValidContentItemReader(): table → {Table}", table);

			Logger.LogDebug("<<< Exiting CreateValidContentItemReader()");
            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidContentItemReader(ContentItem contentItem)
        {
            DataTable table = CreateContentItemTable();

            AddContentItemToTable(
                table,
                contentItem.ContentItemId,
                ContentTestHelper.GetContent(contentItem.ContentItemId),
                ContentTestHelper.GetContentKey(contentItem.ContentItemId),
                true,
                Constants.USER_ValidId,
                Null.NullString);

			Logger.LogDebug("<<< Exiting CreateValidContentItemReader(contentItem={ContentItem})", contentItem);
            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidContentItemsReader(int count, bool indexed, int startUserId, string term)
        {
			Logger.LogDebug(">>> Entering CreateValidContentItemsReader(count={Count},indexed={Indexed},startUserId={StartUserId},term={Term})", count, indexed, startUserId, term);
            DataTable table = CreateContentItemTable();
            for (int i = Constants.CONTENT_ValidContentItemId; i < Constants.CONTENT_ValidContentItemId + count; i++)
			Logger.LogDebug("CreateValidContentItemsReader(count={Count},indexed={Indexed},startUserId={StartUserId},term={Term}): table → {Table}", count, indexed, startUserId, term, table);
            {
                string content = (count == 1) ? Constants.CONTENT_ValidContent : ContentTestHelper.GetContent(i);
                string contentKey = (count == 1) ? Constants.CONTENT_ValidContentKey : ContentTestHelper.GetContentKey(i);
                int userId = (startUserId == Null.NullInteger) ? Constants.USER_ValidId + i : startUserId;

				Logger.LogDebug("CreateValidContentItemsReader(count={Count},indexed={Indexed},startUserId={StartUserId},term={Term}): userId → {UserId}", count, indexed, startUserId, term, userId);
                AddContentItemToTable(table, i, content, contentKey, indexed, startUserId, term);
            }

			Logger.LogDebug("<<< Exiting CreateValidContentItemsReader(count={Count},indexed={Indexed},startUserId={StartUserId},term={Term})", count, indexed, startUserId, term);
            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidContentTypesReader(int count)
        {
			Logger.LogDebug(">>> Entering CreateValidContentTypesReader(count={Count})", count);
            DataTable table = CreateContentTypeTable();
            for (int i = Constants.CONTENTTYPE_ValidContentTypeId; i < Constants.CONTENTTYPE_ValidContentTypeId + count; i++)
			Logger.LogDebug("CreateValidContentTypesReader(count={Count}): table → {Table}", count, table);
            {
                string contentType = (count == 1) ? Constants.CONTENTTYPE_ValidContentType : ContentTestHelper.GetContentType(i);

                AddContentTypeToTable(table, i, contentType);
            }

			Logger.LogDebug("<<< Exiting CreateValidContentTypesReader(count={Count})", count);
            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidScopeTypesReader(int count)
        {
			Logger.LogDebug(">>> Entering CreateValidScopeTypesReader(count={Count})", count);
            DataTable table = CreateScopeTypeTable();
            for (int i = Constants.SCOPETYPE_ValidScopeTypeId; i < Constants.SCOPETYPE_ValidScopeTypeId + count; i++)
			Logger.LogDebug("CreateValidScopeTypesReader(count={Count}): table → {Table}", count, table);
            {
                string scopeType = (count == 1) ? Constants.SCOPETYPE_ValidScopeType : ContentTestHelper.GetScopeType(i);

                AddScopeTypeToTable(table, i, scopeType);
            }

			Logger.LogDebug("<<< Exiting CreateValidScopeTypesReader(count={Count})", count);
            return table.CreateDataReader();
        }

        internal static DataTable CreateMetaDataTable()
        {
			Logger.LogDebug(">>> Entering CreateMetaDataTable()");
            var table = new DataTable();
            table.Columns.Add("MetaDataName", typeof(string));
			Logger.LogDebug("CreateMetaDataTable(): table → {Table}", table);
            table.Columns.Add("MetaDataValue", typeof(string));

			Logger.LogDebug("<<< Exiting CreateMetaDataTable()");
            return table;
        }

        internal static IDataReader CreateValidMetaDataReader()
        {
			Logger.LogDebug(">>> Entering CreateValidMetaDataReader()");
            // Create Categories table.
            using (var table = CreateMetaDataTable())
            {
                for (int i = 0; i < Constants.CONTENT_MetaDataCount; i++)
                {
                    table.Rows.Add(new object[] { string.Format("{0} {1}", Constants.CONTENT_ValidMetaDataName, i), Constants.CONTENT_ValidMetaDataValue });
                }

				Logger.LogDebug("<<< Exiting CreateValidMetaDataReader()");
                return table.CreateDataReader();
            }
        }

        internal static IDataReader CreateMetaDataReaderWithFiles(IList<IFileInfo> files, IList<IFileInfo> videos, IList<IFileInfo> images)
        {
            using (var table = CreateMetaDataTable())
            {
                if (files.Any())
                {
                    table.Rows.Add(FileController.FilesKey, FileController.SerializeFileInfo(files));
                }

                if (videos.Any())
                {
                    table.Rows.Add(FileController.VideoKey, FileController.SerializeFileInfo(videos));
                }

                if (images.Any())
                {
                    table.Rows.Add(FileController.ImageKey, FileController.SerializeFileInfo(images));
                }

                return table.CreateDataReader();
            }
        }

        internal static IDataReader CreateMetaDataReaderFromDictionary(IDictionary<string, string> map)
        {
            using (var table = CreateMetaDataTable())
            {
                foreach (var kvp in map)
                {
                    table.Rows.Add(kvp.Key, kvp.Value);
                }

                return table.CreateDataReader();
            }
        }

        internal static IDataReader CreateEmptyMetaDataReader()
        {
			Logger.LogDebug(">>> Entering CreateEmptyMetaDataReader()");
            using (var table = CreateMetaDataTable())
            {
				Logger.LogDebug("<<< Exiting CreateEmptyMetaDataReader()");
                return table.CreateDataReader();
            }
        }

        internal static IDataReader CreateValidTermReader()
        {
			private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
            DataTable table = CreateTermTable();
			Logger.LogDebug(">>> Entering CreateValidTermReader()");
            AddTermToTable(
                table,
                Constants.TERM_ValidTermId,
                Constants.TERM_ValidContent1,
                Constants.TERM_ValidVocabulary1,
                Constants.TERM_ValidName,
                Constants.TERM_ValidName,
                Constants.TERM_ValidWeight,
                Constants.TERM_ValidParentTermId);

			Logger.LogDebug("<<< Exiting CreateValidTermReader()");
            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidTermsReader(int count, Func<int, int> vocabularyIdFunction, Func<int, int> contentIdFunction)
        {
            DataTable table = CreateTermTable();
            for (int i = Constants.TERM_ValidTermId; i < Constants.TERM_ValidTermId + count; i++)
            {
                string name = (count == 1) ? Constants.TERM_ValidName : ContentTestHelper.GetTermName(i);
                string description = (count == 1) ? Constants.VOCABULARY_ValidName : ContentTestHelper.GetTermName(i);
                int weight = Constants.TERM_ValidWeight;
                int parentId = Constants.TERM_ValidParentTermId;
                AddTermToTable(table, i, contentIdFunction(i), vocabularyIdFunction(i), name, description, weight, parentId);
            }

            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidVocabulariesReader(int count)
        {
			Logger.LogDebug(">>> Entering CreateValidVocabulariesReader(count={Count})", count);
            DataTable table = CreateVocabularyTable();
            for (int i = Constants.VOCABULARY_ValidVocabularyId; i < Constants.VOCABULARY_ValidVocabularyId + count; i++)
			Logger.LogDebug("CreateValidVocabulariesReader(count={Count}): table → {Table}", count, table);
            {
                string name = (count == 1) ? Constants.VOCABULARY_ValidName : ContentTestHelper.GetVocabularyName(i);
                int typeId = Constants.VOCABULARY_SimpleTypeId;
                string description = (count == 1) ? Constants.VOCABULARY_ValidName : ContentTestHelper.GetVocabularyName(i);
                int weight = Constants.VOCABULARY_ValidWeight;
                AddVocabularyToTable(table, i, typeId, name, description, Constants.VOCABULARY_ValidScopeId, Constants.VOCABULARY_ValidScopeTypeId, weight);
            }

			Logger.LogDebug("<<< Exiting CreateValidVocabulariesReader(count={Count})", count);
            return table.CreateDataReader();
        }

        internal static IFileInfo CreateRandomFile(int fileId)
        {
			Logger.LogDebug(">>> Entering CreateRandomFile(fileId={FileId})", fileId);
            var files = Directory.EnumerateFiles(@"C:\").ToArray();

			Logger.LogDebug("CreateRandomFile(fileId={FileId}): files → {Files}", fileId, files);
            var random = new Random((int)DateTime.UtcNow.Ticks);

			Logger.LogDebug("CreateRandomFile(fileId={FileId}): random → {Random}", fileId, random);
            var fileInfo = new Mock<IFileInfo>();

			Logger.LogDebug("CreateRandomFile(fileId={FileId}): fileInfo → {FileInfo}", fileId, fileInfo);
            fileInfo.SetupGet(f => f.FileId).Returns(fileId);
            fileInfo.SetupGet(f => f.Extension).Returns(".txt");
            fileInfo.SetupGet(f => f.FileName).Returns(random.Next() + ".txt");
            fileInfo.SetupGet(f => f.Folder).Returns(@"\");
            fileInfo.SetupGet(f => f.FolderId).Returns(0);
            fileInfo.SetupGet(f => f.FolderMappingID).Returns(-1);
            fileInfo.SetupGet(f => f.IsCached).Returns(false);
            fileInfo.SetupGet(f => f.LastModificationTime).Returns(DateTime.UtcNow);
            fileInfo.SetupGet(f => f.PhysicalPath).Returns(() => files[random.Next() % (files.Length - 1)]);
            fileInfo.SetupGet(f => f.PortalId).Returns(0);
            fileInfo.SetupGet(f => f.RelativePath).Returns(@"..\..\");
            fileInfo.SetupGet(f => f.UniqueId).Returns(Guid.NewGuid());
            fileInfo.SetupGet(f => f.VersionGuid).Returns(Guid.NewGuid());
            fileInfo.SetupGet(f => f.Size).Returns(random.Next() % (1024 * 1024));

			Logger.LogDebug("<<< Exiting CreateRandomFile(fileId={FileId})", fileId);
            return fileInfo.Object;
        }

        internal static Mock<IFileManager> CreateMockFileManager()
        {
			Logger.LogDebug(">>> Entering CreateMockFileManager()");
            var fm = new Mock<IFileManager>();

			Logger.LogDebug("CreateMockFileManager(): fm → {Fm}", fm);
            fm.Setup(f => f.GetFile(It.IsAny<int>())).Returns((int fileId) => CreateRandomFile(fileId));
			Logger.LogDebug("<<< Exiting CreateMockFileManager()");
            return fm;
        }

        private static void AddBaseEntityColumns(DataTable table)
        {
            table.Columns.Add("CreatedByUserID", typeof(int));
            table.Columns.Add("CreatedOnDate", typeof(DateTime));
            table.Columns.Add("LastModifiedByUserID", typeof(int));
            table.Columns.Add("LastModifiedOnDate", typeof(DateTime));
        }

        private static void AddContentItemToTable(DataTable table, int id, string content, string contentKey, bool indexed, int userId, string term)
        {
            table.Rows.Add(new object[] { id, content, Null.NullInteger, Null.NullInteger, Null.NullInteger, contentKey, indexed, userId, term });
        }

        private static void AddContentTypeToTable(DataTable table, int id, string contentType)
        {
            table.Rows.Add(new object[] { id, contentType });
        }

        private static void AddScopeTypeToTable(DataTable table, int id, string scopeType)
        {
            table.Rows.Add(new object[] { id, scopeType });
        }

        private static void AddTermToTable(DataTable table, int id, int contentItemId, int vocabularyId, string name, string description, int weight, int parentId)
        {
            table.Rows.Add(new object[] { id, contentItemId, vocabularyId, name, description, weight, parentId });
        }

        private static void AddVocabularyToTable(DataTable table, int id, int typeId, string name, string description, int scopeId, int scopeTypeId, int weight)
        {
            table.Rows.Add(new object[] { id, typeId, false, name, description, scopeId, scopeTypeId, weight });
        }

        private static DataTable CreateContentItemTable()
        {
			Logger.LogDebug(">>> Entering CreateContentItemTable()");
            // Create Categories table.
            DataTable table = new DataTable();

			Logger.LogDebug("CreateContentItemTable(): table → {Table}", table);
            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("ContentItemID", typeof(int));
            table.Columns.Add("Content", typeof(string));
			Logger.LogDebug("CreateContentItemTable(): idColumn → {IdColumn}", idColumn);
            table.Columns.Add("ContentTypeID", typeof(int));
            table.Columns.Add("TabID", typeof(int));
            table.Columns.Add("ModuleID", typeof(int));
            table.Columns.Add("ContentKey", typeof(string));
            table.Columns.Add("Indexed", typeof(bool));
            table.Columns.Add("UserID", typeof(int));
            table.Columns.Add("Term", typeof(string));
            table.Columns.Add("StateID", typeof(int));
            AddBaseEntityColumns(table);

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };
			Logger.LogDebug("CreateContentItemTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			Logger.LogDebug("<<< Exiting CreateContentItemTable()");
            return table;
        }

        private static DataTable CreateContentTypeTable()
        {
			Logger.LogDebug(">>> Entering CreateContentTypeTable()");
            // Create ContentTypes table.
            DataTable table = new DataTable();

			Logger.LogDebug("CreateContentTypeTable(): table → {Table}", table);
            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("ContentTypeID", typeof(int));
            table.Columns.Add("ContentType", typeof(string));
			Logger.LogDebug("CreateContentTypeTable(): idColumn → {IdColumn}", idColumn);

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };
			Logger.LogDebug("CreateContentTypeTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			Logger.LogDebug("<<< Exiting CreateContentTypeTable()");
            return table;
        }

        private static DataTable CreateScopeTypeTable()
        {
			Logger.LogDebug(">>> Entering CreateScopeTypeTable()");
            // Create ScopeTypes table.
            DataTable table = new DataTable();

			Logger.LogDebug("CreateScopeTypeTable(): table → {Table}", table);
            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("ScopeTypeID", typeof(int));
            table.Columns.Add("ScopeType", typeof(string));
			Logger.LogDebug("CreateScopeTypeTable(): idColumn → {IdColumn}", idColumn);

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };
			Logger.LogDebug("CreateScopeTypeTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			Logger.LogDebug("<<< Exiting CreateScopeTypeTable()");
            return table;
        }

        private static DataTable CreateTermTable()
        {
			Logger.LogDebug(">>> Entering CreateTermTable()");
            // Create Vocabulary table.
            DataTable table = new DataTable();

			Logger.LogDebug("CreateTermTable(): table → {Table}", table);
            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("TermID", typeof(int));
            table.Columns.Add("ContentItemID", typeof(int));
			Logger.LogDebug("CreateTermTable(): idColumn → {IdColumn}", idColumn);
            table.Columns.Add("VocabularyID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Weight", typeof(int));
            table.Columns.Add("ParentTermID", typeof(int));
            table.Columns.Add("TermLeft", typeof(int));
            table.Columns.Add("TermRight", typeof(int));
            AddBaseEntityColumns(table);

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };
			Logger.LogDebug("CreateTermTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			Logger.LogDebug("<<< Exiting CreateTermTable()");
            return table;
        }

        private static DataTable CreateVocabularyTable()
        {
			Logger.LogDebug(">>> Entering CreateVocabularyTable()");
            // Create Vocabulary table.
            DataTable table = new DataTable();

			Logger.LogDebug("CreateVocabularyTable(): table → {Table}", table);
            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("VocabularyID", typeof(int));
            table.Columns.Add("VocabularyTypeID", typeof(int));
			Logger.LogDebug("CreateVocabularyTable(): idColumn → {IdColumn}", idColumn);
            table.Columns.Add("IsSystem", typeof(bool));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("ScopeID", typeof(int));
            table.Columns.Add("ScopeTypeID", typeof(int));
            table.Columns.Add("Weight", typeof(int));
            AddBaseEntityColumns(table);

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };
			Logger.LogDebug("CreateVocabularyTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			Logger.LogDebug("<<< Exiting CreateVocabularyTable()");
            return table;
        }

        private static Mock<TMock> RegisterMockController<TMock>(Mock<TMock> mock)
            where TMock : class
        {
			Logger.LogDebug(">>> Entering RegisterMockController(mock={Mock})", mock);
            if (ComponentFactory.Container == null)
            {
                // Create a Container
                ComponentFactory.Container = new SimpleContainer();
            }

            // Try and get mock
            var getMock = ComponentFactory.GetComponent<Mock<TMock>>();

			Logger.LogDebug("RegisterMockController(mock={Mock}): getMock → {GetMock}", mock, getMock);
            if (getMock == null)
            {
                // Create the mock
                getMock = mock;

                // Add both mock and mock.Object to Container
                ComponentFactory.RegisterComponentInstance<Mock<TMock>>(getMock);
                ComponentFactory.RegisterComponentInstance<TMock>(getMock.Object);
            }

            return getMock;
        }
    }
}
