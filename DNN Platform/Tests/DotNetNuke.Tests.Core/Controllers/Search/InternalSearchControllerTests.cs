// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Controllers.Search
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    /// <summary> Testing various aspects of SearchController.</summary>
    [TestFixture]
    public class InternalSearchControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const int ModuleSearchTypeId = (int)SearchTypeIds.ModuleSearchTypeId;
        private const int TabSearchTypeId = (int)SearchTypeIds.TabSearchTypeId;
        private const int DocumentSearchTypeId = (int)SearchTypeIds.DocumentSearchTypeId;

        private const int UrlSearchTypeId = (int)SearchTypeIds.UrlSearchTypeId;
        private const int OtherSearchTypeId = (int)SearchTypeIds.OtherSearchTypeId;

        private const int PortalId0 = 0;
        private const int PortalId1 = 1;

        private const string TermDNN = "DNN";
        private const string TermDotNetNuke = "DotnetNuke";
        private const string TermLaptop = "Laptop";
        private const string TermNotebook = "Notebook";
        private const string TermJump = "Jump";
        private const string TermLeap = "Leap";
        private const string TermHop = "Hop";

        private const string ModuleSearchTypeName = "module";
        private const string OtherSearchTypeName = "other";
        private const string TabSearchTypeName = "tab";
        private const string DocumentSearchTypeName = "document";
        private const string UrlSearchTypeName = "url";
        private const string FakeResultControllerClass = "DotNetNuke.Tests.Core.Controllers.Search.FakeResultController, DotNetNuke.Tests.Core";

        private const string NoPermissionFakeResultControllerClass =
            "DotNetNuke.Tests.Core.Controllers.Search.NoPermissionFakeResultController, DotNetNuke.Tests.Core";

        private const string CultureEnUs = "en-US";
        private const string CultureEnCa = "en-CA";
        private const string CultureItIt = "it-IT";
        private const string CultureEsEs = "es-ES";
        private const int LanguageIdEnUs = 1;
        private const int LanguageIdEnFr = 2;
        private const int LanguageIdItIt = 3;
        private const int LanguageIdEsEs = 4;

        private const string SearchIndexFolder = @"App_Data\InternalSearchTests";
        private readonly double readerStaleTimeSpan = TimeSpan.FromMilliseconds(100).TotalSeconds;
        private FakeHostController fakeHostController;
        private Mock<CachingProvider> mockCachingProvider;
        private Mock<DataProvider> mockDataProvider;
        private Mock<ILocaleController> mockLocaleController;
        private Mock<ISearchHelper> mockSearchHelper;
        private Mock<IUserController> mockUserController;
        private FakeServiceProvider serviceProvider;

        private IInternalSearchController internalSearchController;
        private LuceneControllerImpl luceneController;

        public enum SearchTypeIds
        {
            ModuleSearchTypeId = 1,
            TabSearchTypeId,
            DocumentSearchTypeId,
            UrlSearchTypeId,
            OtherSearchTypeId,
            UnknownSearchTypeId,
        }

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            ComponentFactory.Container = new SimpleContainer();
            MockComponentProvider.ResetContainer();

            this.SetupHostController();

            this.mockDataProvider = MockComponentProvider.CreateDataProvider();
			Logger.LogDebug("SetUp(): mockDataProvider → {MockDataProvider}", mockDataProvider);
            this.mockLocaleController = MockComponentProvider.CreateLocaleController();
			Logger.LogDebug("SetUp(): mockLocaleController → {MockLocaleController}", mockLocaleController);
            this.mockCachingProvider = MockComponentProvider.CreateDataCacheProvider();
			Logger.LogDebug("SetUp(): mockCachingProvider → {MockCachingProvider}", mockCachingProvider);
            this.mockUserController = new Mock<IUserController>();
			Logger.LogDebug("SetUp(): mockUserController → {MockUserController}", mockUserController);

            this.mockSearchHelper = new Mock<ISearchHelper>();
			Logger.LogDebug("SetUp(): mockSearchHelper → {MockSearchHelper}", mockSearchHelper);
            this.SetupDataProvider();
            this.SetupSearchHelper();
            this.SetupLocaleController();

            this.mockUserController.Setup(c => c.GetUserById(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int portalId, int userId) => this.GetUserByIdCallback(portalId, userId));
            UserController.SetTestableInstance(this.mockUserController.Object);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("SetUp(): services → {Services}", services);
                    services.AddSingleton<IHostController>(this.fakeHostController);
                    services.AddSingleton<IHostSettingsService>(this.fakeHostController);
                    services.AddSingleton(this.mockCachingProvider.Object);
                    services.AddSingleton(this.mockDataProvider.Object);
                    services.AddSingleton(this.mockLocaleController.Object);
                    services.AddSingleton(this.mockSearchHelper.Object);
                    services.AddSingleton(this.mockUserController.Object);
                    services.AddSingleton<IApplicationStatusInfo>(new FakeApplicationStatusInfo());
                    services.AddSingleton(Mock.Of<IPortalController>());
                });

            this.CreateNewLuceneControllerInstance();
			Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.DeleteIndexFolder();
            this.fakeHostController = null;
			Logger.LogDebug("TearDown(): fakeHostController → {FakeHostController}", fakeHostController);
			Logger.LogDebug("TearDown(): this.fakeHostController → {Arg0}", this.fakeHostController);
            this.serviceProvider.Dispose();
            this.luceneController.Dispose();
            InternalSearchController.ClearInstance();
            UserController.ClearInstance();
            SearchHelper.ClearInstance();
            LuceneController.ClearInstance();
            this.luceneController = null;
			Logger.LogDebug("<<< Exiting TearDown()");
			Logger.LogDebug("TearDown(): luceneController → {LuceneController}", luceneController);
			Logger.LogDebug("TearDown(): this.luceneController → {Arg0}", this.luceneController);
        }

        [Test]
        public void SearchController_Add_Throws_On_Null_SearchDocument()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Throws_On_Null_SearchDocument()");
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => this.internalSearchController.AddSearchDocument(null));
			Logger.LogDebug("<<< Exiting SearchController_Add_Throws_On_Null_SearchDocument()");
        }

        [Test]
        public void SearchController_Add_Throws_On_Null_Or_Empty_UniqueuKey()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Throws_On_Null_Or_Empty_UniqueuKey()");
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentException>(() => this.internalSearchController.AddSearchDocument(new SearchDocument()));
			Logger.LogDebug("<<< Exiting SearchController_Add_Throws_On_Null_Or_Empty_UniqueuKey()");
        }

        [Test]
        public void SearchController_Add_Throws_On_Null_OrEmpty_Title()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Throws_On_Null_OrEmpty_Title()");
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(
                () => this.internalSearchController.AddSearchDocument(new SearchDocument { UniqueKey = Guid.NewGuid().ToString() }));
				Logger.LogDebug("<<< Exiting SearchController_Add_Throws_On_Null_OrEmpty_Title()");
        }

        [Test]
        public void SearchController_AddSearchDocuments_Does_Not_Throw_On_Null_OrEmpty_Title()
        {
			Logger.LogDebug(">>> Entering SearchController_AddSearchDocuments_Does_Not_Throw_On_Null_OrEmpty_Title()");
            // Arrange
            var documents = new List<SearchDocument> { new SearchDocument { UniqueKey = Guid.NewGuid().ToString() } };

			Logger.LogDebug("SearchController_AddSearchDocuments_Does_Not_Throw_On_Null_OrEmpty_Title(): documents → {Documents}", documents);
            // Act, Assert
            this.internalSearchController.AddSearchDocuments(documents);
			Logger.LogDebug("<<< Exiting SearchController_AddSearchDocuments_Does_Not_Throw_On_Null_OrEmpty_Title()");
        }

        [Test]
        public void SearchController_AddSearchDocuments_Does_Not_Throw_On_Empty_Search_Document()
        {
			Logger.LogDebug(">>> Entering SearchController_AddSearchDocuments_Does_Not_Throw_On_Empty_Search_Document()");
            // Arrange
            var documents = new List<SearchDocument> { new SearchDocument() };

			Logger.LogDebug("SearchController_AddSearchDocuments_Does_Not_Throw_On_Empty_Search_Document(): documents → {Documents}", documents);
            // Act, Assert
            this.internalSearchController.AddSearchDocuments(documents);
			Logger.LogDebug("<<< Exiting SearchController_AddSearchDocuments_Does_Not_Throw_On_Empty_Search_Document()");
        }

        [Test]
        public void SearchController_Add_Throws_On_Zero_SearchTypeId()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Throws_On_Zero_SearchTypeId()");
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(
                () => this.internalSearchController.AddSearchDocument(new SearchDocument { UniqueKey = Guid.NewGuid().ToString() }));
				Logger.LogDebug("<<< Exiting SearchController_Add_Throws_On_Zero_SearchTypeId()");
        }

        [Test]
        public void SearchController_Add_Throws_On_Negative_SearchTypeId()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Throws_On_Negative_SearchTypeId()");
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    this.internalSearchController.AddSearchDocument(new SearchDocument { UniqueKey = Guid.NewGuid().ToString(), Title = "title", SearchTypeId = -1 }));
					Logger.LogDebug("<<< Exiting SearchController_Add_Throws_On_Negative_SearchTypeId()");
        }

        [Test]
        public void SearchController_Add_Throws_On_DateTimeMin_ModifiedTimeUtc()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Throws_On_DateTimeMin_ModifiedTimeUtc()");
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentException>(
                () => this.internalSearchController.AddSearchDocument(new SearchDocument { UniqueKey = Guid.NewGuid().ToString(), Title = "title", SearchTypeId = 1 }));
				Logger.LogDebug("<<< Exiting SearchController_Add_Throws_On_DateTimeMin_ModifiedTimeUtc()");
        }

        [Test]
        public void SearchController_Add_Then_Delete_ModuleDefinition_WorksAsExpected()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Then_Delete_ModuleDefinition_WorksAsExpected()");
            // Arrange
            const int totalDocs = 10;
            var now = DateTime.UtcNow;

            // Act
            for (var i = 1; i <= totalDocs; i++)
            {
                var doc = new SearchDocument
                {
                    ModuleDefId = i,
                    ModuleId = 100,
                    SearchTypeId = ModuleSearchTypeId,
                    PortalId = PortalId0,
                    UniqueKey = Guid.NewGuid().ToString(),
                    ModifiedTimeUtc = now,
                };

                this.internalSearchController.AddSearchDocument(doc);
            }

            // Assert
            var stats = this.GetSearchStatistics();
            Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs));
			Logger.LogDebug("SearchController_Add_Then_Delete_ModuleDefinition_WorksAsExpected(): stats → {Stats}", stats);

            // Act - delete last item
            var searchDoc = new SearchDocument { ModuleDefId = totalDocs };
            this.internalSearchController.DeleteSearchDocument(searchDoc);
			Logger.LogDebug("SearchController_Add_Then_Delete_ModuleDefinition_WorksAsExpected(): searchDoc → {SearchDoc}", searchDoc);

            // Assert
            stats = this.GetSearchStatistics();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs - 1));
                Assert.That(stats.TotalDeletedDocuments, Is.EqualTo(1));
            }

            // Act - delete first item
            searchDoc = new SearchDocument { ModuleDefId = 1 };
            this.internalSearchController.DeleteSearchDocument(searchDoc);

            // Assert
            stats = this.GetSearchStatistics();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs - 2));
                Assert.That(stats.TotalDeletedDocuments, Is.EqualTo(2));
            }
			Logger.LogDebug("<<< Exiting SearchController_Add_Then_Delete_ModuleDefinition_WorksAsExpected()");
        }

        [Test]
        public void SearchController_Add_Then_Delete_Module_WorksAsExpected()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Then_Delete_Module_WorksAsExpected()");
            // Arrange
            const int totalDocs = 10;
            var now = DateTime.UtcNow;

            // Act
            for (var i = 1; i <= totalDocs; i++)
            {
                var doc = new SearchDocument
                {
                    ModuleId = i,
                    ModuleDefId = 10,
                    PortalId = PortalId0,
                    UniqueKey = Guid.NewGuid().ToString(),
                    SearchTypeId = ModuleSearchTypeId,
                    ModifiedTimeUtc = now,
                };

                this.internalSearchController.AddSearchDocument(doc);
            }

            // Assert
            var stats = this.GetSearchStatistics();
            Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs));
			Logger.LogDebug("SearchController_Add_Then_Delete_Module_WorksAsExpected(): stats → {Stats}", stats);

            // Act - delete last item
            var searchDoc = new SearchDocument { ModuleId = totalDocs };
            this.internalSearchController.DeleteSearchDocument(searchDoc);
			Logger.LogDebug("SearchController_Add_Then_Delete_Module_WorksAsExpected(): searchDoc → {SearchDoc}", searchDoc);

            // Assert
            stats = this.GetSearchStatistics();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs - 1));
                Assert.That(stats.TotalDeletedDocuments, Is.EqualTo(1));
            }

            // Act - delete first item
            searchDoc = new SearchDocument { ModuleId = 1 };
            this.internalSearchController.DeleteSearchDocument(searchDoc);

            // Assert
            stats = this.GetSearchStatistics();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs - 2));
                Assert.That(stats.TotalDeletedDocuments, Is.EqualTo(2));
            }
			Logger.LogDebug("<<< Exiting SearchController_Add_Then_Delete_Module_WorksAsExpected()");
        }

        [Test]
        public void SearchController_Add_Then_Delete_Portals_WorksAsExpected()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Then_Delete_Portals_WorksAsExpected()");
            // Arrange
            const int totalDocs = 10; // must be even
            var now = DateTime.UtcNow;

            // Act
            for (var i = 1; i <= totalDocs; i++)
            {
                var doc = new SearchDocument
                {
                    PortalId = i <= (totalDocs / 2) ? PortalId0 : PortalId1,
                    UniqueKey = Guid.NewGuid().ToString(),
                    SearchTypeId = ModuleSearchTypeId,
                    ModifiedTimeUtc = now,
                    ModuleId = 100,
                    ModuleDefId = 10,
                };

                this.internalSearchController.AddSearchDocument(doc);
            }

            // Assert
            var stats = this.GetSearchStatistics();
            Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs));
			Logger.LogDebug("SearchController_Add_Then_Delete_Portals_WorksAsExpected(): stats → {Stats}", stats);

            // Act - delete all portal 1 items
            var searchDoc = new SearchDocument { PortalId = PortalId1 };
            this.internalSearchController.DeleteSearchDocument(searchDoc);

            // Assert - delete all portal 1
            stats = this.GetSearchStatistics();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs / 2));
                Assert.That(stats.TotalDeletedDocuments, Is.EqualTo(totalDocs / 2));
            }
			Logger.LogDebug("<<< Exiting SearchController_Add_Then_Delete_Portals_WorksAsExpected()");
        }

        [Test]
        public void SearchController_Add_Then_Delete_Roles_WorksAsExpected()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Then_Delete_Roles_WorksAsExpected()");
            // Arrange
            const int totalDocs = 10;
            var now = DateTime.UtcNow;

            // Act
            for (var i = 1; i <= totalDocs; i++)
            {
                var doc = new SearchDocument
                {
                    RoleId = i,
                    PortalId = PortalId0,
                    UniqueKey = Guid.NewGuid().ToString(),
                    SearchTypeId = ModuleSearchTypeId,
                    ModifiedTimeUtc = now,
                    ModuleId = 100,
                    ModuleDefId = 10,
                };

                this.internalSearchController.AddSearchDocument(doc);
            }

            // Assert
            var stats = this.GetSearchStatistics();
            Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs));
			Logger.LogDebug("SearchController_Add_Then_Delete_Roles_WorksAsExpected(): stats → {Stats}", stats);

            // Act - delete last item
            var searchDoc = new SearchDocument { RoleId = totalDocs };
            this.internalSearchController.DeleteSearchDocument(searchDoc);
			Logger.LogDebug("SearchController_Add_Then_Delete_Roles_WorksAsExpected(): searchDoc → {SearchDoc}", searchDoc);

            // Assert
            stats = this.GetSearchStatistics();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs - 1));
                Assert.That(stats.TotalDeletedDocuments, Is.EqualTo(1));
            }

            // Act - delete first item
            searchDoc = new SearchDocument { RoleId = 1 };
            this.internalSearchController.DeleteSearchDocument(searchDoc);

            // Assert
            stats = this.GetSearchStatistics();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs - 2));
                Assert.That(stats.TotalDeletedDocuments, Is.EqualTo(2));
            }
			Logger.LogDebug("<<< Exiting SearchController_Add_Then_Delete_Roles_WorksAsExpected()");
        }

        [Test]
        public void SearchController_Add_Then_Delete_Tabs_WorksAsExpected()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Then_Delete_Tabs_WorksAsExpected()");
            // Arrange
            const int totalDocs = 10;
            var now = DateTime.UtcNow;

            // Act
            for (var i = 1; i <= totalDocs; i++)
            {
                var doc = new SearchDocument
                {
                    TabId = i,
                    PortalId = PortalId0,
                    UniqueKey = Guid.NewGuid().ToString(),
                    SearchTypeId = TabSearchTypeId,
                    ModifiedTimeUtc = now,
                };

                this.internalSearchController.AddSearchDocument(doc);
            }

            // Assert
            var stats = this.GetSearchStatistics();
            Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs));
			Logger.LogDebug("SearchController_Add_Then_Delete_Tabs_WorksAsExpected(): stats → {Stats}", stats);

            // Act - delete last item
            var searchDoc = new SearchDocument { TabId = totalDocs };
            this.internalSearchController.DeleteSearchDocument(searchDoc);
			Logger.LogDebug("SearchController_Add_Then_Delete_Tabs_WorksAsExpected(): searchDoc → {SearchDoc}", searchDoc);

            // Assert
            stats = this.GetSearchStatistics();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs - 1));
                Assert.That(stats.TotalDeletedDocuments, Is.EqualTo(1));
            }

            // Act - delete first item
            searchDoc = new SearchDocument { TabId = 1 };
            this.internalSearchController.DeleteSearchDocument(searchDoc);

            // Assert
            stats = this.GetSearchStatistics();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs - 2));
                Assert.That(stats.TotalDeletedDocuments, Is.EqualTo(2));
            }
			Logger.LogDebug("<<< Exiting SearchController_Add_Then_Delete_Tabs_WorksAsExpected()");
        }

        [Test]
        public void SearchController_Add_Then_Delete_Users_WorksAsExpected()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Then_Delete_Users_WorksAsExpected()");
            // Arrange
            const int totalDocs = 10;
            var now = DateTime.UtcNow;

            // Act
            for (var i = 1; i <= totalDocs; i++)
            {
                var doc = new SearchDocument
                {
                    AuthorUserId = i,
                    PortalId = PortalId0,
                    UniqueKey = Guid.NewGuid().ToString(),
                    SearchTypeId = ModuleSearchTypeId,
                    ModifiedTimeUtc = now,
                    ModuleId = 100,
                    ModuleDefId = 10,
                };

                this.internalSearchController.AddSearchDocument(doc);
            }

            // Assert
            var stats = this.GetSearchStatistics();
            Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs));
			Logger.LogDebug("SearchController_Add_Then_Delete_Users_WorksAsExpected(): stats → {Stats}", stats);

            // Act - delete last item
            var searchDoc = new SearchDocument { AuthorUserId = totalDocs, };
            this.internalSearchController.DeleteSearchDocument(searchDoc);
			Logger.LogDebug("SearchController_Add_Then_Delete_Users_WorksAsExpected(): searchDoc → {SearchDoc}", searchDoc);

            // Assert
            stats = this.GetSearchStatistics();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs - 1));
                Assert.That(stats.TotalDeletedDocuments, Is.EqualTo(1));
            }

            // Act - delete first item
            searchDoc = new SearchDocument { AuthorUserId = 1, };
            this.internalSearchController.DeleteSearchDocument(searchDoc);

            // Assert
            stats = this.GetSearchStatistics();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(stats.TotalActiveDocuments, Is.EqualTo(totalDocs - 2));
                Assert.That(stats.TotalDeletedDocuments, Is.EqualTo(2));
            }
			Logger.LogDebug("<<< Exiting SearchController_Add_Then_Delete_Users_WorksAsExpected()");
        }

        private void CreateNewLuceneControllerInstance()
        {
			Logger.LogDebug(">>> Entering CreateNewLuceneControllerInstance()");
            this.DeleteIndexFolder();
            InternalSearchController.SetTestableInstance(new InternalSearchControllerImpl());
            this.internalSearchController = InternalSearchController.Instance;
			Logger.LogDebug("CreateNewLuceneControllerInstance(): internalSearchController → {InternalSearchController}", internalSearchController);
			Logger.LogDebug("CreateNewLuceneControllerInstance(): this.internalSearchController → {Arg0}", this.internalSearchController);

            if (this.luceneController != null)
            {
                LuceneController.ClearInstance();
                this.luceneController.Dispose();
            }

            this.luceneController = new LuceneControllerImpl(this.fakeHostController, new FakeApplicationStatusInfo());
			Logger.LogDebug("CreateNewLuceneControllerInstance(): luceneController → {LuceneController}", luceneController);
            LuceneController.SetTestableInstance(this.luceneController);
			Logger.LogDebug("<<< Exiting CreateNewLuceneControllerInstance()");
        }

        private void SetupHostController()
        {
			Logger.LogDebug(">>> Entering SetupHostController()");
            this.fakeHostController = new FakeHostController(new Dictionary<string, string>
            {
				Logger.LogDebug("SetupHostController(): fakeHostController → {FakeHostController}", fakeHostController);
                { Constants.SearchIndexFolderKey, SearchIndexFolder + DateTime.UtcNow.Ticks },
                { Constants.SearchReaderRefreshTimeKey, this.readerStaleTimeSpan.ToString(CultureInfo.InvariantCulture) },
                { Constants.SearchTitleBoostSetting, Constants.DefaultSearchTitleBoost.ToString(CultureInfo.InvariantCulture) },
                { Constants.SearchTagBoostSetting, Constants.DefaultSearchTagBoost.ToString(CultureInfo.InvariantCulture) },
                { Constants.SearchContentBoostSetting, Constants.DefaultSearchKeywordBoost.ToString(CultureInfo.InvariantCulture) },
                { Constants.SearchDescriptionBoostSetting, Constants.DefaultSearchDescriptionBoost.ToString(CultureInfo.InvariantCulture) },
                { Constants.SearchAuthorBoostSetting, Constants.DefaultSearchAuthorBoost.ToString(CultureInfo.InvariantCulture) },
                { Constants.SearchMinLengthKey, Constants.DefaultMinLen.ToString(CultureInfo.InvariantCulture) },
                { Constants.SearchMaxLengthKey, Constants.DefaultMaxLen.ToString(CultureInfo.InvariantCulture) },
            });
			Logger.LogDebug("<<< Exiting SetupHostController()");
        }

        private void SetupLocaleController()
        {
			Logger.LogDebug(">>> Entering SetupLocaleController()");
            this.mockLocaleController.Setup(l => l.GetLocale(It.IsAny<string>())).Returns(new Locale { LanguageId = -1, Code = string.Empty });
            this.mockLocaleController.Setup(l => l.GetLocale(CultureEnUs)).Returns(new Locale { LanguageId = LanguageIdEnUs, Code = CultureEnUs });
            this.mockLocaleController.Setup(l => l.GetLocale(CultureEnCa)).Returns(new Locale { LanguageId = LanguageIdEnFr, Code = CultureEnCa });
            this.mockLocaleController.Setup(l => l.GetLocale(CultureItIt)).Returns(new Locale { LanguageId = LanguageIdItIt, Code = CultureItIt });
            this.mockLocaleController.Setup(l => l.GetLocale(CultureEsEs)).Returns(new Locale { LanguageId = LanguageIdEsEs, Code = CultureEsEs });

            this.mockLocaleController.Setup(l => l.GetLocale(It.IsAny<int>())).Returns(new Locale { LanguageId = LanguageIdEnUs, Code = CultureEnUs });
            this.mockLocaleController.Setup(l => l.GetLocale(LanguageIdEnUs)).Returns(new Locale { LanguageId = LanguageIdEnUs, Code = CultureEnUs });
            this.mockLocaleController.Setup(l => l.GetLocale(LanguageIdEnFr)).Returns(new Locale { LanguageId = LanguageIdEnFr, Code = CultureEnCa });
            this.mockLocaleController.Setup(l => l.GetLocale(LanguageIdItIt)).Returns(new Locale { LanguageId = LanguageIdItIt, Code = CultureItIt });
            this.mockLocaleController.Setup(l => l.GetLocale(LanguageIdEsEs)).Returns(new Locale { LanguageId = LanguageIdEsEs, Code = CultureEsEs });
			Logger.LogDebug("<<< Exiting SetupLocaleController()");
        }

        private void SetupDataProvider()
        {
			Logger.LogDebug(">>> Entering SetupDataProvider()");
            // Standard DataProvider Path for Logging
            this.mockDataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);

            DataTableReader searchTypes = null;
            this.mockDataProvider.Setup(ds => ds.GetAllSearchTypes())
                .Callback(() => searchTypes = this.GetAllSearchTypes().CreateDataReader())
                .Returns(() => searchTypes);

            this.mockDataProvider.Setup(d => d.GetPortals(It.IsAny<string>())).Returns<string>(this.GetPortalsCallBack);
			Logger.LogDebug("<<< Exiting SetupDataProvider()");
        }

        private IDataReader GetPortalsCallBack(string culture)
        {
			Logger.LogDebug(">>> Entering GetPortalsCallBack(culture={Culture})", culture);
			Logger.LogDebug("<<< Exiting GetPortalsCallBack(culture={Culture})", culture);
            return this.GetPortalCallBack(PortalId0, CultureEnUs);
        }

        private IDataReader GetPortalCallBack(int portalId, string culture)
        {
			Logger.LogDebug(">>> Entering GetPortalCallBack(portalId={PortalId},culture={Culture})", portalId, culture);
            var table = new DataTable("Portal");

			Logger.LogDebug("GetPortalCallBack(portalId={PortalId},culture={Culture}): table → {Table}", portalId, culture, table);
            var cols = new[]
            {
                "PortalID", "PortalGroupID", "PortalName", "LogoFile", "FooterText", "ExpiryDate", "UserRegistration", "BannerAdvertising", "AdministratorId",
                "Currency", "HostFee",
                "HostSpace", "PageQuota", "UserQuota", "AdministratorRoleId", "RegisteredRoleId", "Description", "KeyWords", "BackgroundFile", "GUID",
                "PaymentProcessor", "ProcessorUserId",
                "ProcessorPassword", "SiteLogHistory", "Email", "DefaultLanguage", "TimezoneOffset", "AdminTabId", "HomeDirectory", "SplashTabId", "HomeTabId",
                "LoginTabId", "RegisterTabId",
                "UserTabId", "SearchTabId", "Custom404TabId", "Custom500TabId", "TermsTabId", "PrivacyTabId", "SuperTabId", "CreatedByUserID", "CreatedOnDate", "LastModifiedByUserID",
                "LastModifiedOnDate", "CultureCode",
            };

            foreach (var col in cols)
            {
                table.Columns.Add(col);
            }

            const int homePage = 1;
            table.Rows.Add(portalId, null, "My Website", "Logo.png", "Copyright 2011 by DotNetNuke Corporation", null,
                "2", "0", "2", "USD", "0", "0", "0", "0", "0", "1", "My Website", "DotNetNuke, DNN, Content, Management, CMS", null,
                "1057AC7A-3C08-4849-A3A6-3D2AB4662020", null, null, null, "0", "admin@changeme.invalid", "en-US", "-8", "58", "Portals/0",
                null, homePage.ToString("D"), null, null, "57", "56", "-1", "-1", "7", null, null, "-1", "2011-08-25 07:34:11", "-1", "2011-08-25 07:34:29", culture);

			Logger.LogDebug("<<< Exiting GetPortalCallBack(portalId={PortalId},culture={Culture})", portalId, culture);
            return table.CreateDataReader();
        }

        private void SetupSearchHelper()
        {
			Logger.LogDebug(">>> Entering SetupSearchHelper()");
            this.mockSearchHelper.Setup(c => c.GetSearchMinMaxLength()).Returns(new Tuple<int, int>(Constants.DefaultMinLen, Constants.DefaultMaxLen));
            this.mockSearchHelper.Setup(c => c.GetSynonyms(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<int, string, string>(this.GetSynonymsCallBack);
            this.mockSearchHelper.Setup(x => x.GetSearchTypeByName(It.IsAny<string>()))
                .Returns((string name) => new SearchType { SearchTypeId = 0, SearchTypeName = name });
            this.mockSearchHelper.Setup(x => x.GetSearchTypeByName(It.IsAny<string>())).Returns<string>(this.GetSearchTypeByNameCallback);
            this.mockSearchHelper.Setup(x => x.GetSearchTypes()).Returns(this.GetSearchTypes());
            this.mockSearchHelper.Setup(c => c.GetSynonymsGroups(It.IsAny<int>(), It.IsAny<string>())).Returns(this.GetSynonymsGroupsCallBack);
            this.mockSearchHelper.Setup(x => x.GetSearchStopWords(0, CultureEsEs)).Returns(
                new SearchStopWords
                {
                    PortalId = 0,
                    CultureCode = CultureEsEs,
                    StopWords = "los,de,el",
                });
            this.mockSearchHelper.Setup(x => x.RephraseSearchText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns<string, bool, bool>(new SearchHelperImpl(Mock.Of<IHostSettings>(), Mock.Of<IHostSettingsService>(), Mock.Of<IPortalController>(), new FakeApplicationStatusInfo()).RephraseSearchText);
            this.mockSearchHelper.Setup(x => x.StripTagsNoAttributes(It.IsAny<string>(), It.IsAny<bool>())).Returns((string html, bool _) => html);
            SearchHelper.SetTestableInstance(this.mockSearchHelper.Object);
			Logger.LogDebug("<<< Exiting SetupSearchHelper()");
        }

        private IList<SynonymsGroup> GetSynonymsGroupsCallBack()
        {
			Logger.LogDebug(">>> Entering GetSynonymsGroupsCallBack()");
            var groups = new List<SynonymsGroup>
            {
                new SynonymsGroup { PortalId = 0, SynonymsGroupId = 1, SynonymsTags = string.Join(",", TermDNN, TermDotNetNuke) },
                new SynonymsGroup { PortalId = 0, SynonymsGroupId = 2, SynonymsTags = string.Join(",", TermLaptop, TermNotebook) },
                new SynonymsGroup { PortalId = 0, SynonymsGroupId = 3, SynonymsTags = string.Join(",", TermJump, TermLeap, TermHop) },
            };

			Logger.LogDebug("<<< Exiting GetSynonymsGroupsCallBack()");
            return groups;
        }

        private SearchType GetSearchTypeByNameCallback(string searchTypeName)
        {
			Logger.LogDebug(">>> Entering GetSearchTypeByNameCallback(searchTypeName={SearchTypeName})", searchTypeName);
            var searchType = new SearchType { SearchTypeName = searchTypeName, SearchTypeId = 0 };
            switch (searchTypeName)
            {
                case ModuleSearchTypeName:
                    searchType.SearchTypeId = ModuleSearchTypeId;
					Logger.LogDebug("GetSearchTypeByNameCallback(searchTypeName={SearchTypeName}): searchType.SearchTypeId → {Arg1}", searchTypeName, searchType.SearchTypeId);
                    break;
                case TabSearchTypeName:
                    searchType.SearchTypeId = TabSearchTypeId;
                    break;
                case OtherSearchTypeName:
                    searchType.SearchTypeId = OtherSearchTypeId;
                    break;
                case DocumentSearchTypeName:
                    searchType.SearchTypeId = DocumentSearchTypeId;
                    break;
                case UrlSearchTypeName:
                    searchType.SearchTypeId = UrlSearchTypeId;
                    break;
            }

			Logger.LogDebug("<<< Exiting GetSearchTypeByNameCallback(searchTypeName={SearchTypeName})", searchTypeName);
            return searchType;
        }

        private IList<string> GetSynonymsCallBack(int portalId, string cultureCode, string term)
        {
			Logger.LogDebug(">>> Entering GetSynonymsCallBack(portalId={PortalId},cultureCode={CultureCode},term={Term})", portalId, cultureCode, term);
            var synonyms = new List<string>();
            if (term == "fox")
			Logger.LogDebug("GetSynonymsCallBack(portalId={PortalId},cultureCode={CultureCode},term={Term}): synonyms → {Synonyms}", portalId, cultureCode, term, synonyms);
            {
                synonyms.Add("wolf");
            }

			Logger.LogDebug("<<< Exiting GetSynonymsCallBack(portalId={PortalId},cultureCode={CultureCode},term={Term})", portalId, cultureCode, term);
            return synonyms;
        }

        private UserInfo GetUserByIdCallback(int portalId, int userId)
        {
			Logger.LogDebug(">>> Entering GetUserByIdCallback(portalId={PortalId},userId={UserId})", portalId, userId);
			Logger.LogDebug("<<< Exiting GetUserByIdCallback(portalId={PortalId},userId={UserId})", portalId, userId);
            return new UserInfo { UserID = userId, DisplayName = "User" + userId, PortalID = portalId };
        }

        private DataTable GetAllSearchTypes()
        {
			Logger.LogDebug(">>> Entering GetAllSearchTypes()");
            var dtSearchTypes = new DataTable("SearchTypes");
            var pkId = dtSearchTypes.Columns.Add("SearchTypeId", typeof(int));
			Logger.LogDebug("GetAllSearchTypes(): dtSearchTypes → {DtSearchTypes}", dtSearchTypes);
			Logger.LogDebug("GetAllSearchTypes(): pkId → {PkId}", pkId);
            dtSearchTypes.Columns.Add("SearchTypeName", typeof(string));
            dtSearchTypes.Columns.Add("SearchResultClass", typeof(string));
            dtSearchTypes.PrimaryKey = new[] { pkId };
			Logger.LogDebug("GetAllSearchTypes(): dtSearchTypes.PrimaryKey → {Arg0}", dtSearchTypes.PrimaryKey);

            // Create default Crawler
            dtSearchTypes.Rows.Add(ModuleSearchTypeId, ModuleSearchTypeName, FakeResultControllerClass);
            dtSearchTypes.Rows.Add(TabSearchTypeId, TabSearchTypeName, FakeResultControllerClass);
            dtSearchTypes.Rows.Add(OtherSearchTypeId, OtherSearchTypeName, FakeResultControllerClass);
            dtSearchTypes.Rows.Add(DocumentSearchTypeId, DocumentSearchTypeName, NoPermissionFakeResultControllerClass);
            dtSearchTypes.Rows.Add(UrlSearchTypeId, UrlSearchTypeName, FakeResultControllerClass);

			Logger.LogDebug("<<< Exiting GetAllSearchTypes()");
            return dtSearchTypes;
        }

        private IEnumerable<SearchType> GetSearchTypes()
        {
			Logger.LogDebug(">>> Entering GetSearchTypes()");
            var searchTypes = new List<SearchType>
            {
                new SearchType { SearchTypeId = ModuleSearchTypeId, SearchTypeName = ModuleSearchTypeName, SearchResultClass = FakeResultControllerClass },
                new SearchType { SearchTypeId = TabSearchTypeId, SearchTypeName = TabSearchTypeName, SearchResultClass = FakeResultControllerClass },
                new SearchType { SearchTypeId = OtherSearchTypeId, SearchTypeName = OtherSearchTypeName, SearchResultClass = FakeResultControllerClass },
                new SearchType
                {
                    SearchTypeId = DocumentSearchTypeId,
                    SearchTypeName = DocumentSearchTypeName,
                    SearchResultClass = NoPermissionFakeResultControllerClass,
                },
                new SearchType { SearchTypeId = UrlSearchTypeId, SearchTypeName = UrlSearchTypeName, SearchResultClass = FakeResultControllerClass },
            };

			Logger.LogDebug("<<< Exiting GetSearchTypes()");
            return searchTypes;
        }

        private void DeleteIndexFolder()
        {
			Logger.LogDebug(">>> Entering DeleteIndexFolder()");
            try
            {
                var searchIndexFolder = this.fakeHostController.GetString(Constants.SearchIndexFolderKey, SearchIndexFolder);
                if (Directory.Exists(searchIndexFolder))
				Logger.LogDebug("DeleteIndexFolder(): searchIndexFolder → {SearchIndexFolder}", searchIndexFolder);
                {
                    Directory.Delete(searchIndexFolder, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
			Logger.LogDebug("<<< Exiting DeleteIndexFolder()");
        }

        private SearchStatistics GetSearchStatistics()
        {
			Logger.LogDebug(">>> Entering GetSearchStatistics()");
            this.internalSearchController.Commit();
            Thread.Sleep((int)(this.readerStaleTimeSpan * 1000)); // time to flush data to Lucene
			Logger.LogDebug("<<< Exiting GetSearchStatistics()");
            return this.internalSearchController.GetSearchStatistics();
        }

#if false // the rules have changed and these are invalid tests now
        [Test]
        public void SearchController_Delete_Throws_On_Null_Or_Empty_UniqueuKey()
        {
			Logger.LogDebug(">>> Entering SearchController_Delete_Throws_On_Null_Or_Empty_UniqueuKey()");
            //Arrange            

            //Act, Assert
            var searchDoc = new SearchDocument() { UniqueKey = null, PortalId = 0, SearchTypeId = 1 };
            Assert.Throws<ArgumentException>(() => _internalSearchController.DeleteSearchDocument(searchDoc));
			Logger.LogDebug("<<< Exiting SearchController_Delete_Throws_On_Null_Or_Empty_UniqueuKey()");
			Logger.LogDebug("SearchController_Delete_Throws_On_Null_Or_Empty_UniqueuKey(): searchDoc → {SearchDoc}", searchDoc);
        }

        [Test]
        public void SearchController_Delete_Throws_On_Zero_SearchTypeId()
        {
			Logger.LogDebug(">>> Entering SearchController_Delete_Throws_On_Zero_SearchTypeId()");
            //Arrange            

            //Act, Assert
            var searchDoc = new SearchDocument() { UniqueKey = "key", PortalId = 0, SearchTypeId = 0 };
            Assert.Throws<ArgumentException>(() => _internalSearchController.DeleteSearchDocument(searchDoc));
			Logger.LogDebug("<<< Exiting SearchController_Delete_Throws_On_Zero_SearchTypeId()");
			Logger.LogDebug("SearchController_Delete_Throws_On_Zero_SearchTypeId(): searchDoc → {SearchDoc}", searchDoc);
        }

        [Test]
        public void SearchController_Delete_Throws_On_Negative_SearchTypeId()
        {
			Logger.LogDebug(">>> Entering SearchController_Delete_Throws_On_Negative_SearchTypeId()");
            //Arrange            

            //Act, Assert
            var searchDoc = new SearchDocument() { UniqueKey = "key", PortalId = 0, SearchTypeId = -1 };
            Assert.Throws<ArgumentOutOfRangeException>(() => _internalSearchController.DeleteSearchDocument(searchDoc));
			Logger.LogDebug("<<< Exiting SearchController_Delete_Throws_On_Negative_SearchTypeId()");
			Logger.LogDebug("SearchController_Delete_Throws_On_Negative_SearchTypeId(): searchDoc → {SearchDoc}", searchDoc);
        }
#endif

    }
}
