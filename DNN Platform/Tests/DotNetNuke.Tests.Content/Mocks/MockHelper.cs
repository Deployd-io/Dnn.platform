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
			_logger.LogDebug(">>> Entering CreateEmptyContentItemReader()");
			_logger.LogDebug("<<< Exiting CreateEmptyContentItemReader()");
            return CreateContentItemTable().CreateDataReader();
        }

        internal static IDataReader CreateEmptyContentTypeReader()
        {
			_logger.LogDebug(">>> Entering CreateEmptyContentTypeReader()");
			_logger.LogDebug("<<< Exiting CreateEmptyContentTypeReader()");
            return CreateContentTypeTable().CreateDataReader();
        }

        internal static IDataReader CreateEmptyScopeTypeReader()
        {
			_logger.LogDebug(">>> Entering CreateEmptyScopeTypeReader()");
			_logger.LogDebug("<<< Exiting CreateEmptyScopeTypeReader()");
            return CreateScopeTypeTable().CreateDataReader();
        }

        internal static IDataReader CreateEmptyTermReader()
        {
			_logger.LogDebug(">>> Entering CreateEmptyTermReader()");
			_logger.LogDebug("<<< Exiting CreateEmptyTermReader()");
            return CreateTermTable().CreateDataReader();
        }

        internal static Mock<HttpContextBase> CreateMockHttpContext()
        {
			_logger.LogDebug(">>> Entering CreateMockHttpContext()");
            Mock<HttpContextBase> httpContext = new Mock<HttpContextBase>();
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();
			_logger.LogDebug("CreateMockHttpContext(): httpResponse → {HttpResponse}", httpResponse);
			_logger.LogDebug("CreateMockHttpContext(): httpContext → {HttpContext}", httpContext);
            httpContext.Setup(h => h.Response).Returns(httpResponse.Object);

			_logger.LogDebug("<<< Exiting CreateMockHttpContext()");
            return httpContext;
        }

        internal static Mock<IVocabularyController> CreateMockVocabularyController()
        {
			_logger.LogDebug(">>> Entering CreateMockVocabularyController()");
            var needsServiceProvider = Globals.DependencyProvider is null;
            var provider = needsServiceProvider ? FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<ICryptographyProvider>())) : Globals.DependencyProvider;
			_logger.LogDebug("CreateMockVocabularyController(): provider → {Provider}", provider);
            try
            {
                // Create the mock
                var mockVocabularies = new Mock<IVocabularyController>();
				_logger.LogDebug("CreateMockVocabularyController(): mockVocabularies → {MockVocabularies}", mockVocabularies);
                mockVocabularies.Setup(v => v.GetVocabularies()).Returns(TestVocabularies);

                // Register Mock
				_logger.LogDebug("<<< Exiting CreateMockVocabularyController()");
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
			_logger.LogDebug(">>> Entering CreateValidContentItemReader()");
            DataTable table = CreateContentItemTable();
            AddContentItemToTable(
                table,
                Constants.CONTENT_ValidContentItemId,
                ContentTestHelper.GetContent(Constants.CONTENT_ValidContentItemId),
                ContentTestHelper.GetContentKey(Constants.CONTENT_ValidContentItemId),
                true,
                Constants.USER_ValidId,
                Null.NullString);
				_logger.LogDebug("CreateValidContentItemReader(): table → {Table}", table);

			_logger.LogDebug("<<< Exiting CreateValidContentItemReader()");
			_logger.LogDebug("<<< Exiting CreateValidContentItemReader(contentItem={ContentItem})", contentItem);
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

            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidContentItemsReader(int count, bool indexed, int startUserId, string term)
        {
			_logger.LogDebug(">>> Entering CreateValidContentItemsReader(count={Count},indexed={Indexed},startUserId={StartUserId},term={Term})", count, indexed, startUserId, term);
            DataTable table = CreateContentItemTable();
            for (int i = Constants.CONTENT_ValidContentItemId; i < Constants.CONTENT_ValidContentItemId + count; i++)
			_logger.LogDebug("CreateValidContentItemsReader(count={Count},indexed={Indexed},startUserId={StartUserId},term={Term}): table → {Table}", count, indexed, startUserId, term, table);
            {
                string content = (count == 1) ? Constants.CONTENT_ValidContent : ContentTestHelper.GetContent(i);
                string contentKey = (count == 1) ? Constants.CONTENT_ValidContentKey : ContentTestHelper.GetContentKey(i);
                int userId = (startUserId == Null.NullInteger) ? Constants.USER_ValidId + i : startUserId;
				_logger.LogDebug("CreateValidContentItemsReader(count={Count},indexed={Indexed},startUserId={StartUserId},term={Term}): userId → {UserId}", count, indexed, startUserId, term, userId);

                AddContentItemToTable(table, i, content, contentKey, indexed, startUserId, term);
            }

			_logger.LogDebug("<<< Exiting CreateValidContentItemsReader(count={Count},indexed={Indexed},startUserId={StartUserId},term={Term})", count, indexed, startUserId, term);
            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidContentTypesReader(int count)
        {
			_logger.LogDebug(">>> Entering CreateValidContentTypesReader(count={Count})", count);
            DataTable table = CreateContentTypeTable();
            for (int i = Constants.CONTENTTYPE_ValidContentTypeId; i < Constants.CONTENTTYPE_ValidContentTypeId + count; i++)
			_logger.LogDebug("CreateValidContentTypesReader(count={Count}): table → {Table}", count, table);
            {
                string contentType = (count == 1) ? Constants.CONTENTTYPE_ValidContentType : ContentTestHelper.GetContentType(i);

                AddContentTypeToTable(table, i, contentType);
            }

			_logger.LogDebug("<<< Exiting CreateValidContentTypesReader(count={Count})", count);
            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidScopeTypesReader(int count)
        {
			_logger.LogDebug(">>> Entering CreateValidScopeTypesReader(count={Count})", count);
            DataTable table = CreateScopeTypeTable();
            for (int i = Constants.SCOPETYPE_ValidScopeTypeId; i < Constants.SCOPETYPE_ValidScopeTypeId + count; i++)
			_logger.LogDebug("CreateValidScopeTypesReader(count={Count}): table → {Table}", count, table);
            {
                string scopeType = (count == 1) ? Constants.SCOPETYPE_ValidScopeType : ContentTestHelper.GetScopeType(i);

                AddScopeTypeToTable(table, i, scopeType);
            }

			_logger.LogDebug("<<< Exiting CreateValidScopeTypesReader(count={Count})", count);
            return table.CreateDataReader();
        }

        internal static DataTable CreateMetaDataTable()
        {
			_logger.LogDebug(">>> Entering CreateMetaDataTable()");
            var table = new DataTable();
            table.Columns.Add("MetaDataName", typeof(string));
			_logger.LogDebug("CreateMetaDataTable(): table → {Table}", table);
            table.Columns.Add("MetaDataValue", typeof(string));

			_logger.LogDebug("<<< Exiting CreateMetaDataTable()");
            return table;
        }

        internal static IDataReader CreateValidMetaDataReader()
        {
			_logger.LogDebug(">>> Entering CreateValidMetaDataReader()");
            // Create Categories table.
            using (var table = CreateMetaDataTable())
            {
                for (int i = 0; i < Constants.CONTENT_MetaDataCount; i++)
                {
                    table.Rows.Add(new object[] { string.Format("{0} {1}", Constants.CONTENT_ValidMetaDataName, i), Constants.CONTENT_ValidMetaDataValue });
                }

				_logger.LogDebug("<<< Exiting CreateValidMetaDataReader()");
                return table.CreateDataReader();
            }
        }

        internal static IDataReader CreateMetaDataReaderWithFiles(IList<IFileInfo> files, IList<IFileInfo> videos, IList<IFileInfo> images)
        {
			_logger.LogDebug(">>> Entering CreateMetaDataReaderWithFiles(files={Files},videos={Videos},images={Images})", files, videos, images);
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

				_logger.LogDebug("<<< Exiting CreateMetaDataReaderWithFiles(files={Files},videos={Videos},images={Images})", files, videos, images);
                return table.CreateDataReader();
            }
        }

        internal static IDataReader CreateMetaDataReaderFromDictionary(IDictionary<string, string> map)
        {
			_logger.LogDebug(">>> Entering CreateMetaDataReaderFromDictionary(map={Map})", map);
            using (var table = CreateMetaDataTable())
            {
                foreach (var kvp in map)
                {
                    table.Rows.Add(kvp.Key, kvp.Value);
                }

				_logger.LogDebug("<<< Exiting CreateMetaDataReaderFromDictionary(map={Map})", map);
                return table.CreateDataReader();
            }
        }

        internal static IDataReader CreateEmptyMetaDataReader()
        {
			_logger.LogDebug(">>> Entering CreateEmptyMetaDataReader()");
            using (var table = CreateMetaDataTable())
			using Microsoft.Extensions.Logging;
            {
				_logger.LogDebug("<<< Exiting CreateEmptyMetaDataReader()");
                return table.CreateDataReader();
            }
        }

        internal static IDataReader CreateValidTermReader()
        {
			private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            DataTable table = CreateTermTable();
			_logger.LogDebug(">>> Entering CreateValidTermReader()");
            AddTermToTable(
                table,
                Constants.TERM_ValidTermId,
                Constants.TERM_ValidContent1,
                Constants.TERM_ValidVocabulary1,
                Constants.TERM_ValidName,
                Constants.TERM_ValidName,
                Constants.TERM_ValidWeight,
                Constants.TERM_ValidParentTermId);

			_logger.LogDebug("<<< Exiting CreateValidTermReader()");
            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidTermsReader(int count, Func<int, int> vocabularyIdFunction, Func<int, int> contentIdFunction)
        {
			_logger.LogDebug(">>> Entering CreateValidTermsReader(count={Count},vocabularyIdFunction={VocabularyIdFunction},contentIdFunction={ContentIdFunction})", count, vocabularyIdFunction, contentIdFunction);
            DataTable table = CreateTermTable();
            for (int i = Constants.TERM_ValidTermId; i < Constants.TERM_ValidTermId + count; i++)
			_logger.LogDebug("CreateValidTermsReader(count={Count},vocabularyIdFunction={VocabularyIdFunction},contentIdFunction={ContentIdFunction}): table → {Table}", count, vocabularyIdFunction, contentIdFunction, table);
            {
                string name = (count == 1) ? Constants.TERM_ValidName : ContentTestHelper.GetTermName(i);
                string description = (count == 1) ? Constants.VOCABULARY_ValidName : ContentTestHelper.GetTermName(i);
                int weight = Constants.TERM_ValidWeight;
                int parentId = Constants.TERM_ValidParentTermId;
                AddTermToTable(table, i, contentIdFunction(i), vocabularyIdFunction(i), name, description, weight, parentId);
            }

			_logger.LogDebug("<<< Exiting CreateValidTermsReader(count={Count},vocabularyIdFunction={VocabularyIdFunction},contentIdFunction={ContentIdFunction})", count, vocabularyIdFunction, contentIdFunction);
            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidVocabulariesReader(int count)
        {
			_logger.LogDebug(">>> Entering CreateValidVocabulariesReader(count={Count})", count);
            DataTable table = CreateVocabularyTable();
            for (int i = Constants.VOCABULARY_ValidVocabularyId; i < Constants.VOCABULARY_ValidVocabularyId + count; i++)
			_logger.LogDebug("CreateValidVocabulariesReader(count={Count}): table → {Table}", count, table);
            {
                string name = (count == 1) ? Constants.VOCABULARY_ValidName : ContentTestHelper.GetVocabularyName(i);
                int typeId = Constants.VOCABULARY_SimpleTypeId;
                string description = (count == 1) ? Constants.VOCABULARY_ValidName : ContentTestHelper.GetVocabularyName(i);
                int weight = Constants.VOCABULARY_ValidWeight;
                AddVocabularyToTable(table, i, typeId, name, description, Constants.VOCABULARY_ValidScopeId, Constants.VOCABULARY_ValidScopeTypeId, weight);
            }

			_logger.LogDebug("<<< Exiting CreateValidVocabulariesReader(count={Count})", count);
            return table.CreateDataReader();
        }

        internal static IFileInfo CreateRandomFile(int fileId)
        {
			_logger.LogDebug(">>> Entering CreateRandomFile(fileId={FileId})", fileId);
            var files = Directory.EnumerateFiles(@"C:\").ToArray();

            var random = new Random((int)DateTime.UtcNow.Ticks);

            var fileInfo = new Mock<IFileInfo>();

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

            return fileInfo.Object;
        }

        internal static Mock<IFileManager> CreateMockFileManager()
        {
			_logger.LogDebug(">>> Entering CreateMockFileManager()");
            var fm = new Mock<IFileManager>();

			_logger.LogDebug("CreateMockFileManager(): fm → {Fm}", fm);
            fm.Setup(f => f.GetFile(It.IsAny<int>())).Returns((int fileId) => CreateRandomFile(fileId));
			_logger.LogDebug("<<< Exiting CreateMockFileManager()");
            return fm;
        }

        private static void AddBaseEntityColumns(DataTable table)
        {
			_logger.LogDebug(">>> Entering AddBaseEntityColumns(table={Table})", table);
            table.Columns.Add("CreatedByUserID", typeof(int));
            table.Columns.Add("CreatedOnDate", typeof(DateTime));
            table.Columns.Add("LastModifiedByUserID", typeof(int));
            table.Columns.Add("LastModifiedOnDate", typeof(DateTime));
			_logger.LogDebug("<<< Exiting AddBaseEntityColumns(table={Table})", table);
        }

        private static void AddContentItemToTable(DataTable table, int id, string content, string contentKey, bool indexed, int userId, string term)
        {
			_logger.LogDebug(">>> Entering AddContentItemToTable(table={Table},id={Id},content={Content},contentKey={ContentKey},indexed={Indexed},userId={UserId},term={Term})", table, id, content, contentKey, indexed, userId, term);
            table.Rows.Add(new object[] { id, content, Null.NullInteger, Null.NullInteger, Null.NullInteger, contentKey, indexed, userId, term });
			_logger.LogDebug("<<< Exiting AddContentItemToTable(table={Table},id={Id},content={Content},contentKey={ContentKey},indexed={Indexed},userId={UserId},term={Term})", table, id, content, contentKey, indexed, userId, term);
        }

        private static void AddContentTypeToTable(DataTable table, int id, string contentType)
        {
			_logger.LogDebug(">>> Entering AddContentTypeToTable(table={Table},id={Id},contentType={ContentType})", table, id, contentType);
            table.Rows.Add(new object[] { id, contentType });
			_logger.LogDebug("<<< Exiting AddContentTypeToTable(table={Table},id={Id},contentType={ContentType})", table, id, contentType);
        }

        private static void AddScopeTypeToTable(DataTable table, int id, string scopeType)
        {
			_logger.LogDebug(">>> Entering AddScopeTypeToTable(table={Table},id={Id},scopeType={ScopeType})", table, id, scopeType);
            table.Rows.Add(new object[] { id, scopeType });
			_logger.LogDebug("<<< Exiting AddScopeTypeToTable(table={Table},id={Id},scopeType={ScopeType})", table, id, scopeType);
        }

        private static void AddTermToTable(DataTable table, int id, int contentItemId, int vocabularyId, string name, string description, int weight, int parentId)
        {
			_logger.LogDebug(">>> Entering AddTermToTable(table={Table},id={Id},contentItemId={ContentItemId},vocabularyId={VocabularyId},name={Name},description={Description},weight={Weight},parentId={ParentId})", table, id, contentItemId, vocabularyId, name, description, weight, parentId);
            table.Rows.Add(new object[] { id, contentItemId, vocabularyId, name, description, weight, parentId });
			_logger.LogDebug("<<< Exiting AddTermToTable(table={Table},id={Id},contentItemId={ContentItemId},vocabularyId={VocabularyId},name={Name},description={Description},weight={Weight},parentId={ParentId})", table, id, contentItemId, vocabularyId, name, description, weight, parentId);
        }

        private static void AddVocabularyToTable(DataTable table, int id, int typeId, string name, string description, int scopeId, int scopeTypeId, int weight)
        {
			_logger.LogDebug(">>> Entering AddVocabularyToTable(table={Table},id={Id},typeId={TypeId},name={Name},description={Description},scopeId={ScopeId},scopeTypeId={ScopeTypeId},weight={Weight})", table, id, typeId, name, description, scopeId, scopeTypeId, weight);
            table.Rows.Add(new object[] { id, typeId, false, name, description, scopeId, scopeTypeId, weight });
			_logger.LogDebug("<<< Exiting AddVocabularyToTable(table={Table},id={Id},typeId={TypeId},name={Name},description={Description},scopeId={ScopeId},scopeTypeId={ScopeTypeId},weight={Weight})", table, id, typeId, name, description, scopeId, scopeTypeId, weight);
        }

        private static DataTable CreateContentItemTable()
        {
			_logger.LogDebug(">>> Entering CreateContentItemTable()");
            // Create Categories table.
            DataTable table = new DataTable();
			_logger.LogDebug("CreateContentItemTable(): table → {Table}", table);

            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("ContentItemID", typeof(int));
			_logger.LogDebug("CreateContentItemTable(): idColumn → {IdColumn}", idColumn);
            table.Columns.Add("Content", typeof(string));
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
			_logger.LogDebug("CreateContentItemTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			_logger.LogDebug("<<< Exiting CreateContentItemTable()");
            return table;
        }

        private static DataTable CreateContentTypeTable()
        {
			_logger.LogDebug(">>> Entering CreateContentTypeTable()");
            // Create ContentTypes table.
            DataTable table = new DataTable();
			_logger.LogDebug("CreateContentTypeTable(): table → {Table}", table);

            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("ContentTypeID", typeof(int));
			_logger.LogDebug("CreateContentTypeTable(): idColumn → {IdColumn}", idColumn);
            table.Columns.Add("ContentType", typeof(string));

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };
			_logger.LogDebug("CreateContentTypeTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			_logger.LogDebug("<<< Exiting CreateContentTypeTable()");
            return table;
        }

        private static DataTable CreateScopeTypeTable()
        {
			_logger.LogDebug(">>> Entering CreateScopeTypeTable()");
            // Create ScopeTypes table.
            DataTable table = new DataTable();
			_logger.LogDebug("CreateScopeTypeTable(): table → {Table}", table);

            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("ScopeTypeID", typeof(int));
			_logger.LogDebug("CreateScopeTypeTable(): idColumn → {IdColumn}", idColumn);
            table.Columns.Add("ScopeType", typeof(string));

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };
			_logger.LogDebug("CreateScopeTypeTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			_logger.LogDebug("<<< Exiting CreateScopeTypeTable()");
            return table;
        }

        private static DataTable CreateTermTable()
        {
			_logger.LogDebug(">>> Entering CreateTermTable()");
            // Create Vocabulary table.
            DataTable table = new DataTable();
			_logger.LogDebug("CreateTermTable(): table → {Table}", table);

            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("TermID", typeof(int));
			_logger.LogDebug("CreateTermTable(): idColumn → {IdColumn}", idColumn);
            table.Columns.Add("ContentItemID", typeof(int));
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
			_logger.LogDebug("CreateTermTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			_logger.LogDebug("<<< Exiting CreateTermTable()");
            return table;
        }

        private static DataTable CreateVocabularyTable()
        {
			_logger.LogDebug(">>> Entering CreateVocabularyTable()");
            // Create Vocabulary table.
            DataTable table = new DataTable();
			_logger.LogDebug("CreateVocabularyTable(): table → {Table}", table);

            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("VocabularyID", typeof(int));
			_logger.LogDebug("CreateVocabularyTable(): idColumn → {IdColumn}", idColumn);
            table.Columns.Add("VocabularyTypeID", typeof(int));
            table.Columns.Add("IsSystem", typeof(bool));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("ScopeID", typeof(int));
            table.Columns.Add("ScopeTypeID", typeof(int));
            table.Columns.Add("Weight", typeof(int));
            AddBaseEntityColumns(table);

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };
			_logger.LogDebug("CreateVocabularyTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			_logger.LogDebug("<<< Exiting CreateVocabularyTable()");
            return table;
        }

        private static Mock<TMock> RegisterMockController<TMock>(Mock<TMock> mock)
            where TMock : class
        {
			_logger.LogDebug(">>> Entering RegisterMockController(mock={Mock})", mock);
            if (ComponentFactory.Container == null)
            {
                // Create a Container
                ComponentFactory.Container = new SimpleContainer();
            }

            // Try and get mock
            var getMock = ComponentFactory.GetComponent<Mock<TMock>>();
			_logger.LogDebug("RegisterMockController(mock={Mock}): getMock → {GetMock}", mock, getMock);

            if (getMock == null)
            {
                // Create the mock
                getMock = mock;

                // Add both mock and mock.Object to Container
                ComponentFactory.RegisterComponentInstance<Mock<TMock>>(getMock);
                ComponentFactory.RegisterComponentInstance<TMock>(getMock.Object);
            }

			_logger.LogDebug("<<< Exiting RegisterMockController(mock={Mock})", mock);
            return getMock;
        }
    }
}
