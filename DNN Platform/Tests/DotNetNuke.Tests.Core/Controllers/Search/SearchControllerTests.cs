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
    using System.Linq;
    using System.Threading;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Abstractions.Settings;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Search.Controllers;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using Constants = DotNetNuke.Services.Search.Internals.Constants;

    /// <summary> Testing various aspects of SearchController.</summary>
    [TestFixture]
    public class SearchControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const int ModuleSearchTypeId = (int)SearchTypeIds.ModuleSearchTypeId;
        private const int TabSearchTypeId = (int)SearchTypeIds.TabSearchTypeId;
        private const int DocumentSearchTypeId = (int)SearchTypeIds.DocumentSearchTypeId;
        private const int UrlSearchTypeId = (int)SearchTypeIds.UrlSearchTypeId;
        private const int OtherSearchTypeId = (int)SearchTypeIds.OtherSearchTypeId;
        private const int UnknownSearchTypeId = (int)SearchTypeIds.UnknownSearchTypeId;
        private const int PortalId0 = 0;
        private const int PortalId12 = 12;
        private const int IdeasModuleDefId = 201;
        private const int BlogsModuleDefId = 202;
        private const int AnswersModuleDefId = 203;
        private const int HtmlModuleDefId = 20;
        private const int HtmlModuleId = 25;
        private const int IdeasModuleId = 301;
        private const int BlogsModuleId = 302;
        private const int AnswersModuleId = 303;
        private const int RoleId731 = 731;
        private const int RoleId532 = 532;
        private const int RoleId0 = 0;
        private const string Tag0 = "tag0";
        private const string Tag0WithSpace = "tag0 hello";
        private const string Tag1 = "tag1";
        private const string Tag2 = "tag2";
        private const string Tag3 = "tag3";
        private const string Tag4 = "tag4";
        private const string TagTootsie = "tootsie";
        private const string TagLatest = "latest";
        private const string TagOldest = "oldest";
        private const string TagIt = "IT";
        private const string TagNeutral = "Neutral";
        private const string ModuleSearchTypeName = "module";
        private const string OtherSearchTypeName = "other";
        private const string TabSearchTypeName = "tab";
        private const string DocumentSearchTypeName = "document";
        private const string UrlSearchTypeName = "url";
        private const string ModuleResultControllerClass = "DotNetNuke.Services.Search.Crawlers.ModuleResultController, DotNetNuke";
        private const string FakeResultControllerClass = "DotNetNuke.Tests.Core.Controllers.Search.FakeResultController, DotNetNuke.Tests.Core";
        private const string NoPermissionFakeResultControllerClass = "DotNetNuke.Tests.Core.Controllers.Search.NoPermissionFakeResultController, DotNetNuke.Tests.Core";
        private const string CultureEnUs = "en-US";
        private const string CultureEnCa = "en-CA";
        private const string CultureItIt = "it-IT";
        private const string CultureEsEs = "es-ES";
        private const int LanguageIdEnUs = 1;
        private const int LanguageIdEnFr = 2;
        private const int LanguageIdItIt = 3;
        private const int LanguageIdEsEs = 4;
        private const int StandardAuthorId = 55;
        private const int StandardRoleId = 66;
        private const string StandardAuthorDisplayName = "Standard User";
        private const int StandardTabId = 99;
        private const string StandardPermission = "!Translator (en-US);![4];[5];[6];Administrators;ContentEditorRole;";
        private const string StandardQueryString = "cid=1";
        private const string NumericKey1 = "numerickey1";
        private const string NumericKey2 = "numerickey2";
        private const int NumericValue1 = 77777;
        private const int NumericValue2 = 55555;
        private const int NumericValue50 = 50;
        private const int NumericValue100 = 100;
        private const int NumericValue200 = 200;
        private const int NumericValue500 = 500;
        private const int NumericValue1000 = 1000;
        private const string KeyWord1Name = "keyword1";
        private const string KeyWord1Value = "value1";
        private const string KeyWord2Name = "keyword2";
        private const string KeyWord2Value = "value2";
        private const string KeyWord3Value = "value3";
        private const string KeyWord4Value = "value4";
        private const string KeyWord5Value = "value5";
        private const string Line1 = "The quick brown fox jumps over the lazy dog";
        private const string Line2 = "The quick gold fox jumped over the lazy black dog";
        private const string Line3 = "the quick fox jumps over the black dog - Italian";
        private const string Line4 = "the red fox jumped over the lazy dark gray dog";
        private const string Line5 = "the quick fox jumps over the white dog - los de el Espana";

        private const int CustomBoost = 80;

        private const string SearchIndexFolder = @"App_Data\SearchTests";
        private const int DefaultSearchRetryTimes = 5;
        private readonly double readerStaleTimeSpan = TimeSpan.FromMilliseconds(100).TotalSeconds;
        private Mock<CachingProvider> mockCachingProvider;
        private Mock<DataProvider> mockDataProvider;
        private Mock<ILocaleController> mockLocaleController;
        private Mock<ISearchHelper> mockSearchHelper;
        private Mock<IUserController> mockUserController;
        private FakeHostController hostController;
        private FakeServiceProvider serviceProvider;

        private SearchControllerImpl searchController;
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

            this.mockUserController = new Mock<IUserController>();
			Logger.LogDebug("SetUp(): mockUserController → {MockUserController}", mockUserController);
            this.mockUserController.Setup(c => c.GetUserById(It.IsAny<int>(), It.IsAny<int>())).Returns((int portalId, int userId) => this.GetUserByIdCallback(portalId, userId));
            UserController.SetTestableInstance(this.mockUserController.Object);
            this.mockDataProvider = MockComponentProvider.CreateDataProvider();
			Logger.LogDebug("SetUp(): mockDataProvider → {MockDataProvider}", mockDataProvider);
            this.mockLocaleController = MockComponentProvider.CreateLocaleController();
			Logger.LogDebug("SetUp(): mockLocaleController → {MockLocaleController}", mockLocaleController);
            this.mockCachingProvider = MockComponentProvider.CreateDataCacheProvider();
			Logger.LogDebug("SetUp(): mockCachingProvider → {MockCachingProvider}", mockCachingProvider);
            this.mockSearchHelper = new Mock<ISearchHelper>();
			Logger.LogDebug("SetUp(): mockSearchHelper → {MockSearchHelper}", mockSearchHelper);
            this.SetupSearchHelper();
            this.SetupDataProvider();
            this.SetupLocaleController();
            this.SetupHostController();
            using (_ = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<ICryptographyProvider>())))
            {
                PortalController.SetTestableInstance(new PortalController(Mock.Of<IBusinessControllerProvider>(), Mock.Of<IHostSettings>(), new FakeApplicationStatusInfo(), Mock.Of<IEventLogger>()));
            }

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("SetUp(): services → {Services}", services);
                    services.AddSingleton<IHostController>(this.hostController);
                    services.AddSingleton<IHostSettingsService>(this.hostController);
                    services.AddSingleton(this.mockCachingProvider.Object);
                    services.AddSingleton(this.mockDataProvider.Object);
                    services.AddSingleton(this.mockLocaleController.Object);
                    services.AddSingleton(this.mockSearchHelper.Object);
                    services.AddSingleton(this.mockUserController.Object);
                    services.AddSingleton<IApplicationStatusInfo>(new FakeApplicationStatusInfo());
                    services.AddTransient<FakeResultController>();
                    services.AddTransient<NoPermissionFakeResultController>();
                });

            this.CreateNewLuceneControllerInstance();
			Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            InternalSearchController.ClearInstance();
            UserController.ClearInstance();
            SearchHelper.ClearInstance();
            LuceneController.ClearInstance();
            this.luceneController.Dispose();
            this.luceneController = null;
			Logger.LogDebug("TearDown(): luceneController → {LuceneController}", luceneController);
			Logger.LogDebug("TearDown(): this.luceneController → {Arg0}", this.luceneController);
            this.serviceProvider.Dispose();
            this.DeleteIndexFolder();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void SearchController_Search_Throws_On_Null_Query()
        {
			Logger.LogDebug(">>> Entering SearchController_Search_Throws_On_Null_Query()");
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => this.searchController.SiteSearch(null));
			Logger.LogDebug("<<< Exiting SearchController_Search_Throws_On_Null_Query()");
        }

        [Test]
        public void SearchController_Search_Throws_On_Empty_TypeId_Collection()
        {
			Logger.LogDebug(">>> Entering SearchController_Search_Throws_On_Empty_TypeId_Collection()");
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentException>(() => this.searchController.SiteSearch(new SearchQuery { KeyWords = "word" }));
			Logger.LogDebug("<<< Exiting SearchController_Search_Throws_On_Empty_TypeId_Collection()");
        }

        [Test]
        public void SearchController_AddSearchDocument_Regex_Does_Not_Sleep_On_Bad_Text_During_Alt_Text_Parsing()
        {
			Logger.LogDebug(">>> Entering SearchController_AddSearchDocument_Regex_Does_Not_Sleep_On_Bad_Text_During_Alt_Text_Parsing()");
            // Arrange
            var document = new SearchDocument { UniqueKey = Guid.NewGuid().ToString(), Title = "<<Click here for the complete city listing by a... ", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

			Logger.LogDebug("SearchController_AddSearchDocument_Regex_Does_Not_Sleep_On_Bad_Text_During_Alt_Text_Parsing(): document → {Document}", document);
            // Act, Assert
            this.internalSearchController.AddSearchDocument(document);
			Logger.LogDebug("<<< Exiting SearchController_AddSearchDocument_Regex_Does_Not_Sleep_On_Bad_Text_During_Alt_Text_Parsing()");
        }

        [Test]
        public void SearchController_AddSearchDocument_Regex_Does_Not_Sleep_On_Bad_Text_During_Alt_Text_Parsing2()
        {
			Logger.LogDebug(">>> Entering SearchController_AddSearchDocument_Regex_Does_Not_Sleep_On_Bad_Text_During_Alt_Text_Parsing2()");
            // Arrange
            var document = new SearchDocument { UniqueKey = Guid.NewGuid().ToString(), Title = "<<Click here for the complete city listing by a... ", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

			Logger.LogDebug("SearchController_AddSearchDocument_Regex_Does_Not_Sleep_On_Bad_Text_During_Alt_Text_Parsing2(): document → {Document}", document);
            // Act, Assert
            Assert.DoesNotThrow(() => ExecuteWithTimeout(
                    () =>
                    {
                        this.internalSearchController.AddSearchDocument(document);
                        return false;
                    }, TimeSpan.FromSeconds(1)));
					Logger.LogDebug("<<< Exiting SearchController_AddSearchDocument_Regex_Does_Not_Sleep_On_Bad_Text_During_Alt_Text_Parsing2()");
        }

        [Test]
        public void SearchController_Added_Item_IsRetrieved()
        {
			Logger.LogDebug(">>> Entering SearchController_Added_Item_IsRetrieved()");
            // Arrange
            var doc = new SearchDocument { UniqueKey = "key01", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

			Logger.LogDebug("SearchController_Added_Item_IsRetrieved(): doc → {Doc}", doc);
            // Act
            this.internalSearchController.AddSearchDocument(doc);

            var result = this.SearchForKeyword("hello");

			Logger.LogDebug("SearchController_Added_Item_IsRetrieved(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.Results, Has.Count.EqualTo(1));
                Assert.That(doc.UniqueKey, Is.EqualTo(result.Results[0].UniqueKey));
                Assert.That(doc.Title, Is.EqualTo(result.Results[0].Title));
            }
			Logger.LogDebug("<<< Exiting SearchController_Added_Item_IsRetrieved()");
        }

        [Test]
        public void SearchController_EnsureIndexIsAppended_When_Index_Is_NotDeleted_InBetween()
        {
			Logger.LogDebug(">>> Entering SearchController_EnsureIndexIsAppended_When_Index_Is_NotDeleted_InBetween()");
            // Arrange
            string[] docs =
            [
                Line1,
                Line2
            ];

            // Act

            // Add first document
            var doc1 = new SearchDocument { Title = docs[0], UniqueKey = Guid.NewGuid().ToString(), SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            this.internalSearchController.AddSearchDocument(doc1);
			Logger.LogDebug("SearchController_EnsureIndexIsAppended_When_Index_Is_NotDeleted_InBetween(): doc1 → {Doc1}", doc1);

            // first luceneQuery
            var query1 = new SearchQuery { KeyWords = "fox", SearchTypeIds = new List<int> { OtherSearchTypeId } };
            var search1 = this.searchController.SiteSearch(query1);
			Logger.LogDebug("SearchController_EnsureIndexIsAppended_When_Index_Is_NotDeleted_InBetween(): query1 → {Query1}", query1);
			Logger.LogDebug("SearchController_EnsureIndexIsAppended_When_Index_Is_NotDeleted_InBetween(): search1 → {Search1}", search1);

            // Assert
            Assert.That(search1.Results, Has.Count.EqualTo(1));

            // Add second document
            var doc2 = new SearchDocument { Title = docs[1], UniqueKey = Guid.NewGuid().ToString(), SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            this.internalSearchController.AddSearchDocument(doc2);
			Logger.LogDebug("SearchController_EnsureIndexIsAppended_When_Index_Is_NotDeleted_InBetween(): doc2 → {Doc2}", doc2);
            this.CreateNewLuceneControllerInstance(); // to force a new reader for the next assertion

            // second luceneQuery
            var query2 = new SearchQuery { KeyWords = "fox", SearchTypeIds = new List<int> { OtherSearchTypeId } };
            var search2 = this.searchController.SiteSearch(query2);
			Logger.LogDebug("SearchController_EnsureIndexIsAppended_When_Index_Is_NotDeleted_InBetween(): query2 → {Query2}", query2);
			Logger.LogDebug("SearchController_EnsureIndexIsAppended_When_Index_Is_NotDeleted_InBetween(): search2 → {Search2}", search2);

            // Assert
            Assert.That(search2.Results, Has.Count.EqualTo(2));
			Logger.LogDebug("<<< Exiting SearchController_EnsureIndexIsAppended_When_Index_Is_NotDeleted_InBetween()");
        }

        [Test]
        public void SearchController_GetSearch_TwoTermsSearch()
        {
			Logger.LogDebug(">>> Entering SearchController_GetSearch_TwoTermsSearch()");
            // Arrange
            string[] docs =
            [
                Line1,
                Line2,
                Line3,
                Line4,
                Line5
            ];

            this.AddLinesAsSearchDocs(docs);

            // Act
            var search = this.SearchForKeyword("fox jumps");

			Logger.LogDebug("SearchController_GetSearch_TwoTermsSearch(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(docs.Length));

            ////Assert.AreEqual("brown <b>fox jumps</b> over the lazy dog ", search.Results[0].Snippet);
            ////Assert.AreEqual("quick <b>fox jumps</b> over the black dog ", search.Results[1].Snippet);
			Logger.LogDebug("<<< Exiting SearchController_GetSearch_TwoTermsSearch()");
        }

        [Test]
        public void SearchController_GetResult_TwoTermsSearch()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_TwoTermsSearch()");
            // Arrange
            string[] docs =
            [
                Line1,
                Line2,
                Line3,
                Line4,
                Line5
            ];

            this.AddLinesAsSearchDocs(docs);

            // Act
            var search = this.SearchForKeyword("fox jumps");

			Logger.LogDebug("SearchController_GetResult_TwoTermsSearch(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(docs.Length));

            ////Assert.AreEqual("brown <b>fox jumps</b> over the lazy dog ", search.Results[0].Snippet);
            ////Assert.AreEqual("quick <b>fox jumps</b> over the black dog ", search.Results[1].Snippet);
			Logger.LogDebug("<<< Exiting SearchController_GetResult_TwoTermsSearch()");
        }

        [Test]
        public void SearchController_GetResult_PortalIdSearch()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_PortalIdSearch()");
            // Arrange
            var added = this.AddStandardSearchDocs();

			Logger.LogDebug("SearchController_GetResult_PortalIdSearch(): added → {Added}", added);
            // Act
            var query = new SearchQuery { SearchTypeIds = [ModuleSearchTypeId,], PortalIds = new List<int> { PortalId0 } };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_PortalIdSearch(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_PortalIdSearch()");
        }

        [Test]
        public void SearchController_GetResult_SearchTypeIdSearch()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_SearchTypeIdSearch()");
            // Arrange
            var added = this.AddStandardSearchDocs();

			Logger.LogDebug("SearchController_GetResult_SearchTypeIdSearch(): added → {Added}", added);
            // Act
            var query = new SearchQuery { SearchTypeIds = [ModuleSearchTypeId,] };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_SearchTypeIdSearch(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_SearchTypeIdSearch()");
        }

        [Test]
        public void SearchController_SearchFindsAnalyzedVeryLongWords()
        {
			Logger.LogDebug(">>> Entering SearchController_SearchFindsAnalyzedVeryLongWords()");
            // Arrange
            ////const string fieldName = Constants.ContentTag;
            const string veryLongWord = // 107 characters
                "NowIsTheTimeForAllGoodMenToComeToTheAidOfTheirCountryalsoIsTheTimeForAllGoodMenToComeToTheAidOfTheirCountry";

            var doc = new SearchDocument
            {
                Title = veryLongWord,
                UniqueKey = Guid.NewGuid().ToString(),
                SearchTypeId = ModuleSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                ModuleId = 1,
                ModuleDefId = 1,
            };
            this.internalSearchController.AddSearchDocument(doc);

            // Act
            var query = new SearchQuery { KeyWords = veryLongWord, SearchTypeIds = [ModuleSearchTypeId,] };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_SearchFindsAnalyzedVeryLongWords(): search → {Search}", search);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(search.Results, Has.Count.EqualTo(1));
                Assert.That(StripEllipses(search.Results[0].Snippet).Trim(), Is.EqualTo("<b>" + veryLongWord + "</b>"));
            }
			Logger.LogDebug("<<< Exiting SearchController_SearchFindsAnalyzedVeryLongWords()");
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsNoResultsWhenHavingNoPermission()
        {
			Logger.LogDebug(">>> Entering SearchController_SecurityTrimmedTest_ReturnsNoResultsWhenHavingNoPermission()");
            // Arrange
            this.AddStandardSearchDocs(DocumentSearchTypeId);

            // Act
            var result = this.SearchForKeyword("fox", DocumentSearchTypeId);

			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsNoResultsWhenHavingNoPermission(): result → {Result}", result);
            // Assert
            // by default AuthorUserId = 0 which have no permission, so this passes
            Assert.That(result.Results, Is.Empty);
			Logger.LogDebug("<<< Exiting SearchController_SecurityTrimmedTest_ReturnsNoResultsWhenHavingNoPermission()");
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1A()
        {
			Logger.LogDebug(">>> Entering SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1A()");
            // Arrange
            const int MaxDocs = 30;
            const int SearchTypeId = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(MaxDocs, SearchTypeId);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 1,
                PageSize = 4,
                KeyWords = "fox",
                SearchTypeIds = [SearchTypeId,],
            };

            var result = this.searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1A(): result → {Result}", result);
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1A(): ids → {Ids}", ids);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(MaxDocs - 18));
                Assert.That(result.Results, Has.Count.EqualTo(query.PageSize));
                Assert.That(ids, Is.EqualTo([6, 7, 8, 9,]));
            }
			Logger.LogDebug("<<< Exiting SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1A()");
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1B()
        {
			Logger.LogDebug(">>> Entering SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1B()");
            // Arrange
            const int MaxDocs = 30;
            const int SearchTypeId = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(MaxDocs, SearchTypeId);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 1,
                PageSize = 6,
                KeyWords = "fox",
                SearchTypeIds = [SearchTypeId,],
            };

            var result = this.searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1B(): result → {Result}", result);
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1B(): ids → {Ids}", ids);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(MaxDocs - 18));
                Assert.That(result.Results, Has.Count.EqualTo(query.PageSize));
                Assert.That(ids, Is.EqualTo([6, 7, 8, 9, 16, 17,]));
            }
			Logger.LogDebug("<<< Exiting SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1B()");
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1C()
        {
			Logger.LogDebug(">>> Entering SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1C()");
            // Arrange
            const int MaxDocs = 30;
            const int SearchTypeId = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(MaxDocs, SearchTypeId);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 1,
                PageSize = 8,
                KeyWords = "fox",
                SearchTypeIds = [SearchTypeId,],
            };

            var result = this.searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1C(): result → {Result}", result);
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1C(): ids → {Ids}", ids);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(MaxDocs - 18));
                Assert.That(result.Results, Has.Count.EqualTo(query.PageSize));
                Assert.That(ids, Is.EqualTo([6, 7, 8, 9, 16, 17, 18, 19,]));
            }
			Logger.LogDebug("<<< Exiting SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1C()");
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1D()
        {
			Logger.LogDebug(">>> Entering SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1D()");
            // Arrange
            const int MaxDocs = 30;
            const int SearchTypeId = DocumentSearchTypeId; // user should have access to some documents here
            this.SetupSecurityTrimmingDocs(MaxDocs, SearchTypeId);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 1,
                PageSize = 100,
                KeyWords = "fox",
                SearchTypeIds = [SearchTypeId,],
            };

            var result = this.searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1D(): result → {Result}", result);
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1D(): ids → {Ids}", ids);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(4 * 3));
                Assert.That(result.Results, Has.Count.EqualTo(4 * 3));
                Assert.That(ids, Is.EqualTo([6, 7, 8, 9, 16, 17, 18, 19, 26, 27, 28, 29,]));
            }
			Logger.LogDebug("<<< Exiting SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1D()");
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1E()
        {
			Logger.LogDebug(">>> Entering SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1E()");
            // Arrange
            const int MaxDocs = 30;
            const int SearchTypeId = TabSearchTypeId; // user should have access to all documents here
            this.SetupSecurityTrimmingDocs(MaxDocs, SearchTypeId);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 1,
                PageSize = 10,
                KeyWords = "fox",
                SearchTypeIds = [SearchTypeId,],
            };

            var result = this.searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).Skip(1).ToArray();
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1E(): result → {Result}", result);
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1E(): ids → {Ids}", ids);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(MaxDocs));
                Assert.That(result.Results, Has.Count.EqualTo(query.PageSize));
                Assert.That(ids, Is.EqualTo(Enumerable.Range(1, 9).ToArray()));
            }
			Logger.LogDebug("<<< Exiting SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1E()");
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1F()
        {
			Logger.LogDebug(">>> Entering SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1F()");
            // Arrange
            const int MaxDocs = 100;
            const int SearchTypeId = TabSearchTypeId; // user should have access to all documents here
            this.SetupSecurityTrimmingDocs(MaxDocs, SearchTypeId);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 10,
                PageSize = 10,
                KeyWords = "fox",
                SearchTypeIds = [SearchTypeId,],
            };

            var result = this.searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1F(): result → {Result}", result);
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1F(): ids → {Ids}", ids);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(MaxDocs));
                Assert.That(result.Results, Has.Count.EqualTo(query.PageSize));
                Assert.That(ids, Is.EqualTo(Enumerable.Range(90, 10).ToArray()));
            }
			Logger.LogDebug("<<< Exiting SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1F()");
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2A()
        {
			Logger.LogDebug(">>> Entering SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2A()");
            // Arrange
            const int MaxDocs = 30;
            const int SearchTypeId = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(MaxDocs, SearchTypeId);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 2,
                PageSize = 5,
                KeyWords = "fox",
                SearchTypeIds = [SearchTypeId,],
            };

            var result = this.searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2A(): result → {Result}", result);
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2A(): ids → {Ids}", ids);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(MaxDocs - 18));
                Assert.That(result.Results, Has.Count.EqualTo(5));
                Assert.That(ids, Is.EqualTo([17, 18, 19, 26, 27,]));
            }
			Logger.LogDebug("<<< Exiting SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2A()");
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2B()
        {
			Logger.LogDebug(">>> Entering SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2B()");
            // Arrange
            const int MaxDocs = 30;
            const int SearchTypeId = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(MaxDocs, SearchTypeId);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 2,
                PageSize = 6,
                KeyWords = "fox",
                SearchTypeIds = [SearchTypeId,],
            };

            var result = this.searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2B(): result → {Result}", result);
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2B(): ids → {Ids}", ids);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(MaxDocs - 18));
                Assert.That(result.Results, Has.Count.EqualTo(6));
                Assert.That(ids, Is.EqualTo([18, 19, 26, 27, 28, 29,]));
            }
			Logger.LogDebug("<<< Exiting SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2B()");
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2C()
        {
			Logger.LogDebug(">>> Entering SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2C()");
            // Arrange
            const int MaxDocs = 30;
            const int SearchTypeId = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(MaxDocs, SearchTypeId);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 2,
                PageSize = 8,
                KeyWords = "fox",
                SearchTypeIds = [SearchTypeId,],
            };

            var result = this.searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2C(): result → {Result}", result);
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2C(): ids → {Ids}", ids);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(MaxDocs - 18));
                Assert.That(result.Results, Has.Count.EqualTo(4));
                Assert.That(ids, Is.EqualTo([26, 27, 28, 29,]));
            }
			Logger.LogDebug("<<< Exiting SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2C()");
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3A()
        {
			Logger.LogDebug(">>> Entering SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3A()");
            // Arrange
            const int MaxDocs = 30;
            const int SearchTypeId = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(MaxDocs, SearchTypeId);

            // Act
            var queryPg3 = new SearchQuery
            {
                PageIndex = 3,
                PageSize = 4,
                KeyWords = "fox",
                SearchTypeIds = [SearchTypeId,],
            };

            var result = this.searchController.SiteSearch(queryPg3);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3A(): result → {Result}", result);
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3A(): ids → {Ids}", ids);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(MaxDocs - 18));
                Assert.That(result.Results, Has.Count.EqualTo(queryPg3.PageSize));
                Assert.That(ids, Is.EqualTo([26, 27, 28, 29,]));
            }
			Logger.LogDebug("<<< Exiting SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3A()");
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3B()
        {
			Logger.LogDebug(">>> Entering SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3B()");
            // Arrange
            const int MaxDocs = 30;
            const int SearchTypeId = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(MaxDocs, SearchTypeId);

            // Act
            var queryPg3 = new SearchQuery
            {
                PageIndex = 3,
                PageSize = 5,
                KeyWords = "fox",
                SearchTypeIds = [SearchTypeId,],
            };

            var result = this.searchController.SiteSearch(queryPg3);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3B(): result → {Result}", result);
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3B(): ids → {Ids}", ids);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(MaxDocs - 18));
                Assert.That(result.Results, Has.Count.EqualTo(2));
                Assert.That(ids, Is.EqualTo([28, 29,]));
            }
			Logger.LogDebug("<<< Exiting SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3B()");
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3C()
        {
			Logger.LogDebug(">>> Entering SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3C()");
            // Arrange
            const int MaxDocs = 30;
            const int SearchTypeId = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(MaxDocs, SearchTypeId);

            // Act
            var queryPg3 = new SearchQuery
            {
                PageIndex = 3,
                PageSize = 8,
                KeyWords = "fox",
                SearchTypeIds = [SearchTypeId,],
            };

            var result = this.searchController.SiteSearch(queryPg3);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3C(): result → {Result}", result);
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3C(): ids → {Ids}", ids);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(MaxDocs - 18));
                Assert.That(result.Results, Is.Empty);
                Assert.That(ids, Is.EqualTo(new int[] { }));
            }
			Logger.LogDebug("<<< Exiting SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3C()");
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage5()
        {
			Logger.LogDebug(">>> Entering SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage5()");
            // Arrange
            const int MaxDocs = 100;
            const int SearchTypeId = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(MaxDocs, SearchTypeId);

            // Act
            var queryPg3 = new SearchQuery
            {
                PageIndex = 5,
                PageSize = 8,
                KeyWords = "fox",
                SearchTypeIds = [SearchTypeId,],
            };

            var result = this.searchController.SiteSearch(queryPg3);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage5(): result → {Result}", result);
			Logger.LogDebug("SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage5(): ids → {Ids}", ids);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(MaxDocs - (10 * 6)));
                Assert.That(result.Results, Has.Count.EqualTo(queryPg3.PageSize));
                Assert.That(ids, Is.EqualTo([86, 87, 88, 89, 96, 97, 98, 99,]));
            }
			Logger.LogDebug("<<< Exiting SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage5()");
        }

        [Test]
        public void SearchController_GetResult_Returns_Correct_SuppliedData_When_Optionals_Are_Supplied()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Returns_Correct_SuppliedData_When_Optionals_Are_Supplied()");
            // Arrange
            var modifiedDateTime = DateTime.UtcNow;
            var numericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue1 }, { NumericKey2, NumericValue2 } };
            var keywords = new Dictionary<string, string>() { { KeyWord1Name, KeyWord1Value }, { KeyWord2Name, KeyWord2Value } };
			Logger.LogDebug("SearchController_GetResult_Returns_Correct_SuppliedData_When_Optionals_Are_Supplied(): numericKeys → {NumericKeys}", numericKeys);
			Logger.LogDebug("SearchController_GetResult_Returns_Correct_SuppliedData_When_Optionals_Are_Supplied(): keywords → {Keywords}", keywords);
            var tags = new List<string> { Tag1, Tag2 };
            var doc = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key",
                SearchTypeId = ModuleSearchTypeId,
                ModifiedTimeUtc = modifiedDateTime,
                PortalId = PortalId12,
                TabId = StandardTabId,
                ModuleDefId = HtmlModuleDefId,
                ModuleId = HtmlModuleId,
                Description = "Description",
                Body = "Body",
                AuthorUserId = StandardAuthorId,
                RoleId = StandardRoleId,
                Permissions = StandardPermission,
                QueryString = StandardQueryString,
                Tags = tags,
                NumericKeys = numericKeys,
                Keywords = keywords,
            };
            this.internalSearchController.AddSearchDocument(doc);

            // run luceneQuery on common keyword between both the docs
            var search = this.SearchForKeywordInModule("Title");

			Logger.LogDebug("SearchController_GetResult_Returns_Correct_SuppliedData_When_Optionals_Are_Supplied(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(search.Results[0].PortalId, Is.EqualTo(PortalId12));
                Assert.That(search.Results[0].TabId, Is.EqualTo(StandardTabId));
                Assert.That(search.Results[0].ModuleDefId, Is.EqualTo(HtmlModuleDefId));
                Assert.That(search.Results[0].ModuleId, Is.EqualTo(HtmlModuleId));
                Assert.That(search.Results[0].SearchTypeId, Is.EqualTo(ModuleSearchTypeId));
                Assert.That(search.Results[0].Description, Is.EqualTo("Description"));
                Assert.That(search.Results[0].Body, Is.EqualTo("Body"));
                Assert.That(search.Results[0].AuthorUserId, Is.EqualTo(StandardAuthorId));
                Assert.That(search.Results[0].RoleId, Is.EqualTo(StandardRoleId));
                Assert.That(search.Results[0].ModifiedTimeUtc.ToString(Constants.DateTimeFormat), Is.EqualTo(modifiedDateTime.ToString(Constants.DateTimeFormat)));
                Assert.That(search.Results[0].Permissions, Is.EqualTo(StandardPermission));
                Assert.That(search.Results[0].QueryString, Is.EqualTo(StandardQueryString));
                Assert.That(search.Results[0].AuthorName, Is.EqualTo(StandardAuthorDisplayName));
                Assert.That(search.Results[0].Tags.Count(), Is.EqualTo(tags.Count));
                Assert.That(search.Results[0].Tags.ElementAt(0), Is.EqualTo(tags[0]));
                Assert.That(search.Results[0].Tags.ElementAt(1), Is.EqualTo(tags[1]));
                Assert.That(search.Results[0].NumericKeys, Has.Count.EqualTo(numericKeys.Count));
            }
            using (Assert.EnterMultipleScope())
            {
                Assert.That(search.Results[0].NumericKeys[NumericKey1], Is.EqualTo(numericKeys[NumericKey1]));
                Assert.That(search.Results[0].NumericKeys[NumericKey2], Is.EqualTo(numericKeys[NumericKey2]));
                Assert.That(search.Results[0].Keywords, Has.Count.EqualTo(keywords.Count));
            }
            using (Assert.EnterMultipleScope())
            {
                Assert.That(search.Results[0].Keywords[KeyWord1Name], Is.EqualTo(keywords[KeyWord1Name]));
                Assert.That(search.Results[0].Keywords[KeyWord2Name], Is.EqualTo(keywords[KeyWord2Name]));
            }
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Returns_Correct_SuppliedData_When_Optionals_Are_Supplied()");
        }

        [Test]
        public void SearchController_GetResult_Returns_EmptyData_When_Optionals_Are_Not_Supplied()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Returns_EmptyData_When_Optionals_Are_Not_Supplied()");
            // Arrange
            var modifiedDateTime = DateTime.UtcNow;
            var doc = new SearchDocument
            {
                PortalId = PortalId0,
                Title = "Title",
                UniqueKey = "key",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = modifiedDateTime,
            };
            this.internalSearchController.AddSearchDocument(doc);

            var search = this.SearchForKeyword("Title");

			Logger.LogDebug("SearchController_GetResult_Returns_EmptyData_When_Optionals_Are_Not_Supplied(): search → {Search}", search);
            // Assert -
            Assert.That(search.Results, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(search.Results[0].PortalId, Is.EqualTo(PortalId0));
                Assert.That(search.Results[0].TabId, Is.EqualTo(0));
                Assert.That(search.Results[0].ModuleDefId, Is.EqualTo(0));
                Assert.That(search.Results[0].ModuleId, Is.EqualTo(0));
                Assert.That(search.Results[0].SearchTypeId, Is.EqualTo(OtherSearchTypeId));
                Assert.That(search.Results[0].Description, Is.EqualTo(null));
                Assert.That(search.Results[0].Body, Is.EqualTo(null));
                Assert.That(search.Results[0].AuthorUserId, Is.EqualTo(0));
                Assert.That(search.Results[0].RoleId, Is.EqualTo(-1));
                Assert.That(search.Results[0].ModifiedTimeUtc.ToString(Constants.DateTimeFormat), Is.EqualTo(modifiedDateTime.ToString(Constants.DateTimeFormat)));
                Assert.That(search.Results[0].Permissions, Is.EqualTo(null));
                Assert.That(search.Results[0].QueryString, Is.EqualTo(null));
                Assert.That(search.Results[0].AuthorName, Is.EqualTo(null));
                Assert.That(search.Results[0].Tags.Count(), Is.EqualTo(0));
                Assert.That(search.Results[0].NumericKeys, Is.Empty);
                Assert.That(search.Results[0].Keywords, Is.Empty);
            }
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Returns_EmptyData_When_Optionals_Are_Not_Supplied()");
        }

        [Test]
        public void SearchController_GetsHighlightedDesc()
        {
			Logger.LogDebug(">>> Entering SearchController_GetsHighlightedDesc()");
            // Arrange
            string[] docs =
            [
                Line1,
                Line2,
                Line3,
                Line4,
                Line5
            ];
            this.AddLinesAsSearchDocs(docs);

            // Act
            var search = this.SearchForKeyword("fox");

			Logger.LogDebug("SearchController_GetsHighlightedDesc(): search → {Search}", search);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(search.Results, Has.Count.EqualTo(docs.Length));
                Assert.That(
                    search.Results.Select(r => StripEllipses(r.Snippet)),
                    Is.EqualTo(
                        [
                            "brown <b>fox</b> jumps over the lazy dog",
                            "quick <b>fox</b> jumps over the black dog - Italian",
                            "gold <b>fox</b> jumped over the lazy black dog",
                            "e red <b>fox</b> jumped over the lazy dark gray dog",
                            "quick <b>fox</b> jumps over the white dog - los de el Espana",
                        ])
                        .AsCollection,
                    "Found: " + string.Join(Environment.NewLine, search.Results.Select(r => r.Snippet)));
            }
			Logger.LogDebug("<<< Exiting SearchController_GetsHighlightedDesc()");
        }

        [Test]
        public void SearchController_CorrectDocumentCultureIsUsedAtIndexing()
        {
			Logger.LogDebug(">>> Entering SearchController_CorrectDocumentCultureIsUsedAtIndexing()");
            // Arrange
            // assign a culture that is different than the current one
            var isNonEnglishEnv = Thread.CurrentThread.CurrentCulture.Name != CultureEsEs;
            string cultureCode, title, searchWord;

            // Act
            if (isNonEnglishEnv)
            {
                cultureCode = CultureEsEs;
				Logger.LogDebug("SearchController_CorrectDocumentCultureIsUsedAtIndexing(): cultureCode → {CultureCode}", cultureCode);
                searchWord = "zorro";
                title = "los rápidos saltos de zorro sobre el perro negro";
				Logger.LogDebug("SearchController_CorrectDocumentCultureIsUsedAtIndexing(): searchWord → {SearchWord}", searchWord);
				Logger.LogDebug("SearchController_CorrectDocumentCultureIsUsedAtIndexing(): title → {Title}", title);
            }
            else
            {
                cultureCode = CultureEnUs;
                searchWord = "fox";
                title = Line3;
            }

            this.internalSearchController.AddSearchDocument(
                new SearchDocument
                {
                    Title = title,
                    UniqueKey = Guid.NewGuid().ToString(),
                    SearchTypeId = OtherSearchTypeId,
                    ModifiedTimeUtc = DateTime.UtcNow,
                    CultureCode = cultureCode,
                });
            this.internalSearchController.Commit();

            var searches = this.SearchForKeyword(searchWord);

			Logger.LogDebug("SearchController_CorrectDocumentCultureIsUsedAtIndexing(): searches → {Searches}", searches);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(searches.TotalHits, Is.EqualTo(1));
                Assert.That(searches.Results[0].CultureCode, Is.EqualTo(cultureCode));
            }
			Logger.LogDebug("<<< Exiting SearchController_CorrectDocumentCultureIsUsedAtIndexing()");
        }

        [Test]
        public void SearchController_GetResult_TimeRangeSearch_Ignores_When_Only_BeginDate_Specified()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_TimeRangeSearch_Ignores_When_Only_BeginDate_Specified()");
            // Arrange
            var added = this.AddStandardSearchDocs();

			Logger.LogDebug("SearchController_GetResult_TimeRangeSearch_Ignores_When_Only_BeginDate_Specified(): added → {Added}", added);
            // Act
            var query = new SearchQuery { SearchTypeIds = [ModuleSearchTypeId,], BeginModifiedTimeUtc = DateTime.Now };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_TimeRangeSearch_Ignores_When_Only_BeginDate_Specified(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_TimeRangeSearch_Ignores_When_Only_BeginDate_Specified()");
        }

        [Test]
        public void SearchController_GetResult_TimeRangeSearch_Returns_Scoped_Results_When_BeginDate_Is_After_End_Date()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_TimeRangeSearch_Returns_Scoped_Results_When_BeginDate_Is_After_End_Date()");
            // Arrange
            var added = this.AddStandardSearchDocs();

			Logger.LogDebug("SearchController_GetResult_TimeRangeSearch_Returns_Scoped_Results_When_BeginDate_Is_After_End_Date(): added → {Added}", added);
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                BeginModifiedTimeUtc = DateTime.Now,
                EndModifiedTimeUtc = DateTime.Now.AddSeconds(-1),
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_TimeRangeSearch_Returns_Scoped_Results_When_BeginDate_Is_After_End_Date(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_TimeRangeSearch_Returns_Scoped_Results_When_BeginDate_Is_After_End_Date()");
        }

        [Test]
        public void SearchController_GetResult_TimeRangeSearch_Returns_Scoped_Results_When_Both_Dates_Specified()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_TimeRangeSearch_Returns_Scoped_Results_When_Both_Dates_Specified()");
            // Arrange
            var added = this.AddStandardSearchDocs();
            int[] searchTypeIds = [ModuleSearchTypeId,];
			Logger.LogDebug("SearchController_GetResult_TimeRangeSearch_Returns_Scoped_Results_When_Both_Dates_Specified(): added → {Added}", added);
            var utcNow = DateTime.UtcNow.AddDays(1);
            const SortFields SortField = SortFields.LastModified;
			Logger.LogDebug("SearchController_GetResult_TimeRangeSearch_Returns_Scoped_Results_When_Both_Dates_Specified(): utcNow → {UtcNow}", utcNow);

            // Act and Assert - just a bit later
            var query = new SearchQuery { SearchTypeIds = searchTypeIds, SortField = SortField, BeginModifiedTimeUtc = utcNow.AddSeconds(1), EndModifiedTimeUtc = utcNow.AddDays(1) };
            var search = this.searchController.SiteSearch(query);
			Logger.LogDebug("SearchController_GetResult_TimeRangeSearch_Returns_Scoped_Results_When_Both_Dates_Specified(): query → {Query}", query);
			Logger.LogDebug("SearchController_GetResult_TimeRangeSearch_Returns_Scoped_Results_When_Both_Dates_Specified(): search → {Search}", search);
            Assert.That(search.Results, Is.Empty);

            // Act and Assert - 10 day
            query = new SearchQuery { SearchTypeIds = searchTypeIds, SortField = SortField, BeginModifiedTimeUtc = utcNow.AddDays(-10), EndModifiedTimeUtc = utcNow.AddDays(1) };
            search = this.searchController.SiteSearch(query);
            Assert.That(search.Results, Has.Count.EqualTo(1));
            Assert.That(search.Results[0].Title, Is.EqualTo(Line5));

            // Act and Assert - 1 year or so
            query = new SearchQuery { SearchTypeIds = searchTypeIds, SortField = SortField, BeginModifiedTimeUtc = utcNow.AddDays(-368), EndModifiedTimeUtc = utcNow.AddDays(1) };
            search = this.searchController.SiteSearch(query);
            Assert.That(search.Results, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(search.Results[0].Title, Is.EqualTo(Line5));
                Assert.That(search.Results[1].Title, Is.EqualTo(Line4));
            }

            // Act and Assert - 2 years or so
            query = new SearchQuery { SearchTypeIds = searchTypeIds, SortField = SortField, BeginModifiedTimeUtc = utcNow.AddDays(-800), EndModifiedTimeUtc = utcNow.AddDays(1) };
            search = this.searchController.SiteSearch(query);
            Assert.That(search.Results, Has.Count.EqualTo(3));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(search.Results[0].Title, Is.EqualTo(Line5));
                Assert.That(search.Results[1].Title, Is.EqualTo(Line4));
                Assert.That(search.Results[2].Title, Is.EqualTo(Line3));
            }

            // Act and Assert - 3 years or so
            query = new SearchQuery { SearchTypeIds = searchTypeIds, SortField = SortField, BeginModifiedTimeUtc = utcNow.AddDays(-1200), EndModifiedTimeUtc = utcNow.AddDays(1) };
            search = this.searchController.SiteSearch(query);
            Assert.That(search.Results, Has.Count.EqualTo(4));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(search.Results[0].Title, Is.EqualTo(Line5));
                Assert.That(search.Results[1].Title, Is.EqualTo(Line4));
                Assert.That(search.Results[2].Title, Is.EqualTo(Line3));
                Assert.That(search.Results[3].Title, Is.EqualTo(Line2));
            }

            // Act and Assert - 2 to 3 years or so
            query = new SearchQuery { SearchTypeIds = searchTypeIds, SortField = SortField, BeginModifiedTimeUtc = utcNow.AddDays(-1200), EndModifiedTimeUtc = utcNow.AddDays(-800) };
            search = this.searchController.SiteSearch(query);
            Assert.That(search.Results, Has.Count.EqualTo(1));
            Assert.That(search.Results[0].Title, Is.EqualTo(Line2));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_TimeRangeSearch_Returns_Scoped_Results_When_Both_Dates_Specified()");
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Single_Tag_Returns_Single_Result()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_TagSearch_Single_Tag_Returns_Single_Result()");
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery { SearchTypeIds = [ModuleSearchTypeId,], Tags = new List<string> { Tag0 } };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_TagSearch_Single_Tag_Returns_Single_Result(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(1));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_TagSearch_Single_Tag_Returns_Single_Result()");
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Single_Tag_With_Space_Returns_Single_Result()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_TagSearch_Single_Tag_With_Space_Returns_Single_Result()");
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery { SearchTypeIds = [ModuleSearchTypeId,], Tags = new List<string> { Tag0WithSpace } };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_TagSearch_Single_Tag_With_Space_Returns_Single_Result(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(1));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_TagSearch_Single_Tag_With_Space_Returns_Single_Result()");
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Lowercase_Search_Returns_ProperCaseTag_Single_Result()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_TagSearch_Lowercase_Search_Returns_ProperCaseTag_Single_Result()");
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery { SearchTypeIds = [ModuleSearchTypeId,], Tags = new List<string> { TagNeutral.ToLowerInvariant() } };
            var search = this.searchController.SiteSearch(query);
			Logger.LogDebug("SearchController_GetResult_TagSearch_Lowercase_Search_Returns_ProperCaseTag_Single_Result(): query → {Query}", query);
			Logger.LogDebug("SearchController_GetResult_TagSearch_Lowercase_Search_Returns_ProperCaseTag_Single_Result(): search → {Search}", search);

            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(1));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_TagSearch_Lowercase_Search_Returns_ProperCaseTag_Single_Result()");
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Single_Tag_Returns_Two_Results()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_TagSearch_Single_Tag_Returns_Two_Results()");
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery { SearchTypeIds = [ModuleSearchTypeId,], Tags = new List<string> { Tag1 } };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_TagSearch_Single_Tag_Returns_Two_Results(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(search.Results[0].Tags.ElementAt(0), Is.EqualTo(Tag0));
                Assert.That(search.Results[0].Tags.ElementAt(1), Is.EqualTo(Tag1));
                Assert.That(search.Results[1].Tags.ElementAt(0), Is.EqualTo(Tag1));
                Assert.That(search.Results[1].Tags.ElementAt(1), Is.EqualTo(Tag2));
            }
			Logger.LogDebug("<<< Exiting SearchController_GetResult_TagSearch_Single_Tag_Returns_Two_Results()");
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Two_Tags_Returns_Nothing()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_TagSearch_Two_Tags_Returns_Nothing()");
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery { SearchTypeIds = [ModuleSearchTypeId,], Tags = new List<string> { Tag0, Tag4 } };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_TagSearch_Two_Tags_Returns_Nothing(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Is.Empty);
			Logger.LogDebug("<<< Exiting SearchController_GetResult_TagSearch_Two_Tags_Returns_Nothing()");
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Two_Tags_Returns_Single_Results()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_TagSearch_Two_Tags_Returns_Single_Results()");
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery { SearchTypeIds = [ModuleSearchTypeId,], Tags = new List<string> { Tag1, Tag2 } };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_TagSearch_Two_Tags_Returns_Single_Results(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(search.Results[0].Tags.ElementAt(0), Is.EqualTo(Tag1));
                Assert.That(search.Results[0].Tags.ElementAt(1), Is.EqualTo(Tag2));
            }
			Logger.LogDebug("<<< Exiting SearchController_GetResult_TagSearch_Two_Tags_Returns_Single_Results()");
        }

        [Test]
        public void SearchController_GetResult_TagSearch_With_Vowel_Tags_Returns_Data()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_TagSearch_With_Vowel_Tags_Returns_Data()");
            // Arrange
            const string Keyword = "awesome";
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = Keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Tags = new List<string> { TagTootsie } };
			Logger.LogDebug("SearchController_GetResult_TagSearch_With_Vowel_Tags_Returns_Data(): doc1 → {Doc1}", doc1);

            // Act
            this.internalSearchController.AddSearchDocument(doc1);
            var query = new SearchQuery { KeyWords = Keyword, SearchTypeIds = [OtherSearchTypeId,], Tags = new List<string> { TagTootsie, } };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_TagSearch_With_Vowel_Tags_Returns_Data(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(1));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_TagSearch_With_Vowel_Tags_Returns_Data()");
        }

        [Test]
        public void SearchController_GetResult_Throws_When_CustomNumericField_Is_Specified_And_CustomSortField_Is_Not()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Throws_When_CustomNumericField_Is_Specified_And_CustomSortField_Is_Not()");
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.CustomNumericField,
            };

            Assert.Throws<ArgumentException>(() => this.searchController.SiteSearch(query));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Throws_When_CustomNumericField_Is_Specified_And_CustomSortField_Is_Not()");
        }

        [Test]
        public void SearchController_GetResult_Throws_When_CustomStringField_Is_Specified_And_CustomSortField_Is_Not()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Throws_When_CustomStringField_Is_Specified_And_CustomSortField_Is_Not()");
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.CustomStringField,
            };

            Assert.Throws<ArgumentException>(() => this.searchController.SiteSearch(query));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Throws_When_CustomStringField_Is_Specified_And_CustomSortField_Is_Not()");
        }

        [Test]
        public void SearchController_GetResult_Throws_When_NumericKey_Is_Specified_And_CustomSortField_Is_Not()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Throws_When_NumericKey_Is_Specified_And_CustomSortField_Is_Not()");
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.NumericKey,
            };

            Assert.Throws<ArgumentException>(() => this.searchController.SiteSearch(query));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Throws_When_NumericKey_Is_Specified_And_CustomSortField_Is_Not()");
        }

        [Test]
        public void SearchController_GetResult_Throws_When_Keyword_Is_Specified_And_CustomSortField_Is_Not()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Throws_When_Keyword_Is_Specified_And_CustomSortField_Is_Not()");
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.Keyword,
            };

            Assert.Throws<ArgumentException>(() => this.searchController.SiteSearch(query));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Throws_When_Keyword_Is_Specified_And_CustomSortField_Is_Not()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Date_Returns_Latest_Docs_First()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_Date_Returns_Latest_Docs_First()");
            // Arrange
            var added = this.AddStandardSearchDocs();

			Logger.LogDebug("SearchController_GetResult_Sort_By_Date_Returns_Latest_Docs_First(): added → {Added}", added);
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.LastModified,
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Sort_By_Date_Returns_Latest_Docs_First(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added), "Found: " + string.Join(Environment.NewLine, search.Results.Select(r => r.Title)));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(search.Results[1].Tags.ElementAt(0), Is.EqualTo(Tag3));
                Assert.That(search.Results[1].Tags.ElementAt(1), Is.EqualTo(Tag4));
                Assert.That(search.Results[1].Tags.ElementAt(2), Is.EqualTo(TagLatest));

                Assert.That(search.Results[0].Tags.ElementAt(0), Is.EqualTo(Tag2));
                Assert.That(search.Results[0].Tags.ElementAt(1), Is.EqualTo(Tag3));
                Assert.That(search.Results[0].Tags.ElementAt(2), Is.EqualTo(TagIt.ToLowerInvariant()));
            }
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_Date_Returns_Latest_Docs_First()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Date_Ascending_Returns_Earliest_Docs_First()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_Date_Ascending_Returns_Earliest_Docs_First()");
            // Arrange
            var added = this.AddStandardSearchDocs();

			Logger.LogDebug("SearchController_GetResult_Sort_By_Date_Ascending_Returns_Earliest_Docs_First(): added → {Added}", added);
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.LastModified,
                SortDirection = SortDirections.Ascending,
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Sort_By_Date_Ascending_Returns_Earliest_Docs_First(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(search.Results[1].DisplayModifiedTime, Is.GreaterThan(search.Results[0].DisplayModifiedTime));
                Assert.That(search.Results[2].DisplayModifiedTime, Is.GreaterThan(search.Results[1].DisplayModifiedTime));
                Assert.That(search.Results[3].DisplayModifiedTime, Is.GreaterThan(search.Results[2].DisplayModifiedTime));
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(search.Results[0].Tags.ElementAt(0), Is.EqualTo(Tag0));
                Assert.That(search.Results[0].Tags.ElementAt(1), Is.EqualTo(Tag1));
                Assert.That(search.Results[0].Tags.ElementAt(2), Is.EqualTo(TagOldest));
            }
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_Date_Ascending_Returns_Earliest_Docs_First()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_NumericKeys_Ascending_Returns_Smaller_Numbers_First()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_NumericKeys_Ascending_Returns_Smaller_Numbers_First()");
            var added = this.AddDocumentsWithNumericKeys();

			Logger.LogDebug("SearchController_GetResult_Sort_By_NumericKeys_Ascending_Returns_Smaller_Numbers_First(): added → {Added}", added);
            // Act
            var query = new SearchQuery
            {
                KeyWords = "Title",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.NumericKey,
                SortDirection = SortDirections.Ascending,
                CustomSortField = NumericKey1,
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Sort_By_NumericKeys_Ascending_Returns_Smaller_Numbers_First(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(search.Results[1].NumericKeys[NumericKey1], Is.GreaterThan(search.Results[0].NumericKeys[NumericKey1]));
                Assert.That(search.Results[2].NumericKeys[NumericKey1], Is.GreaterThan(search.Results[1].NumericKeys[NumericKey1]));
            }
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_NumericKeys_Ascending_Returns_Smaller_Numbers_First()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_NumericKeys_Descending_Returns_Bigger_Numbers_First()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_NumericKeys_Descending_Returns_Bigger_Numbers_First()");
            var added = this.AddDocumentsWithNumericKeys();

			Logger.LogDebug("SearchController_GetResult_Sort_By_NumericKeys_Descending_Returns_Bigger_Numbers_First(): added → {Added}", added);
            // Act
            var query = new SearchQuery
            {
                KeyWords = "Title",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.NumericKey,
                SortDirection = SortDirections.Descending,
                CustomSortField = NumericKey1,
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Sort_By_NumericKeys_Descending_Returns_Bigger_Numbers_First(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(search.Results[0].NumericKeys[NumericKey1], Is.GreaterThan(search.Results[1].NumericKeys[NumericKey1]));
                Assert.That(search.Results[1].NumericKeys[NumericKey1], Is.GreaterThan(search.Results[2].NumericKeys[NumericKey1]));
            }
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_NumericKeys_Descending_Returns_Bigger_Numbers_First()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Title_Ascending_Returns_Alphabetic_Ascending()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_Title_Ascending_Returns_Alphabetic_Ascending()");
            var titles = new List<string> { "cat", "ant", "dog", "antelope", "zebra", "yellow", " " };

			Logger.LogDebug("SearchController_GetResult_Sort_By_Title_Ascending_Returns_Alphabetic_Ascending(): titles → {Titles}", titles);
            var added = this.AddDocuments(titles, "animal");

			Logger.LogDebug("SearchController_GetResult_Sort_By_Title_Ascending_Returns_Alphabetic_Ascending(): added → {Added}", added);
            // Act
            var query = new SearchQuery
            {
                KeyWords = "animal",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.Title,
                SortDirection = SortDirections.Ascending,
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Sort_By_Title_Ascending_Returns_Alphabetic_Ascending(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));

            var count = 0;
            foreach (var title in titles.OrderBy(s => s))
            {
			Logger.LogDebug("SearchController_GetResult_Sort_By_Title_Ascending_Returns_Alphabetic_Ascending(): count → {Count}", count);
                Assert.That(search.Results[count++].Title, Is.EqualTo(title));
            }
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_Title_Ascending_Returns_Alphabetic_Ascending()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Title_Descending_Returns_Alphabetic_Descending()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_Title_Descending_Returns_Alphabetic_Descending()");
            var titles = new List<string> { "cat", "ant", "dog", "antelope", "zebra", "yellow", " " };

			Logger.LogDebug("SearchController_GetResult_Sort_By_Title_Descending_Returns_Alphabetic_Descending(): titles → {Titles}", titles);
            var added = this.AddDocuments(titles, "animal");

			Logger.LogDebug("SearchController_GetResult_Sort_By_Title_Descending_Returns_Alphabetic_Descending(): added → {Added}", added);
            // Act
            var query = new SearchQuery
            {
                KeyWords = "animal",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.Title,
                SortDirection = SortDirections.Descending,
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Sort_By_Title_Descending_Returns_Alphabetic_Descending(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));

            var count = 0;
            foreach (var title in titles.OrderByDescending(s => s))
            {
			Logger.LogDebug("SearchController_GetResult_Sort_By_Title_Descending_Returns_Alphabetic_Descending(): count → {Count}", count);
                Assert.That(search.Results[count++].Title, Is.EqualTo(title));
            }
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_Title_Descending_Returns_Alphabetic_Descending()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Keyword_Ascending_Returns_Alphabetic_Ascending()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_Keyword_Ascending_Returns_Alphabetic_Ascending()");
            var titles = new List<string> { "cat", "ant", "dog", "antelope", "zebra", "yellow", " " };

			Logger.LogDebug("SearchController_GetResult_Sort_By_Keyword_Ascending_Returns_Alphabetic_Ascending(): titles → {Titles}", titles);
            var added = this.AddDocumentsWithKeywords(titles, "animal");

			Logger.LogDebug("SearchController_GetResult_Sort_By_Keyword_Ascending_Returns_Alphabetic_Ascending(): added → {Added}", added);
            // Act
            var query = new SearchQuery
            {
                KeyWords = "animal",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.Keyword,
                SortDirection = SortDirections.Ascending,
                CustomSortField = KeyWord1Name,
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Sort_By_Keyword_Ascending_Returns_Alphabetic_Ascending(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));

            var count = 0;
            foreach (var title in titles.OrderBy(s => s))
            {
			Logger.LogDebug("SearchController_GetResult_Sort_By_Keyword_Ascending_Returns_Alphabetic_Ascending(): count → {Count}", count);
                Assert.That(search.Results[count++].Keywords[KeyWord1Name], Is.EqualTo(title));
            }
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_Keyword_Ascending_Returns_Alphabetic_Ascending()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Keyword_Descending_Returns_Alphabetic_Descending()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_Keyword_Descending_Returns_Alphabetic_Descending()");
            var titles = new List<string> { "cat", "ant", "dog", "antelope", "zebra", "yellow", " " };

			Logger.LogDebug("SearchController_GetResult_Sort_By_Keyword_Descending_Returns_Alphabetic_Descending(): titles → {Titles}", titles);
            var added = this.AddDocumentsWithKeywords(titles, "animal");

			Logger.LogDebug("SearchController_GetResult_Sort_By_Keyword_Descending_Returns_Alphabetic_Descending(): added → {Added}", added);
            // Act
            var query = new SearchQuery
            {
                KeyWords = "animal",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.Keyword,
                SortDirection = SortDirections.Descending,
                CustomSortField = KeyWord1Name,
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Sort_By_Keyword_Descending_Returns_Alphabetic_Descending(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));

            var count = 0;
            foreach (var title in titles.OrderByDescending(s => s))
            {
			Logger.LogDebug("SearchController_GetResult_Sort_By_Keyword_Descending_Returns_Alphabetic_Descending(): count → {Count}", count);
                Assert.That(search.Results[count++].Keywords[KeyWord1Name], Is.EqualTo(title));
            }
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_Keyword_Descending_Returns_Alphabetic_Descending()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Unknown_StringField_In_Descending_Order_Does_Not_Throw()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_Unknown_StringField_In_Descending_Order_Does_Not_Throw()");
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.CustomStringField,
                SortDirection = SortDirections.Descending,
                CustomSortField = "unknown",
            };
            this.searchController.SiteSearch(query);
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_Unknown_StringField_In_Descending_Order_Does_Not_Throw()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Unknown_StringField_In_Ascending_Order_Does_Not_Throw()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_Unknown_StringField_In_Ascending_Order_Does_Not_Throw()");
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.CustomStringField,
                SortDirection = SortDirections.Ascending,
                CustomSortField = "unknown",
            };
            this.searchController.SiteSearch(query);
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_Unknown_StringField_In_Ascending_Order_Does_Not_Throw()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Unknown_NumericField_In_Descending_Order_Does_Not_Throw()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_Unknown_NumericField_In_Descending_Order_Does_Not_Throw()");
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.CustomNumericField,
                SortDirection = SortDirections.Descending,
                CustomSortField = "unknown",
            };
            this.searchController.SiteSearch(query);
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_Unknown_NumericField_In_Descending_Order_Does_Not_Throw()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Unknown_NumericField_In_Ascending_Order_Does_Not_Throw()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_Unknown_NumericField_In_Ascending_Order_Does_Not_Throw()");
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.CustomNumericField,
                SortDirection = SortDirections.Ascending,
                CustomSortField = "unknown",
            };
            this.searchController.SiteSearch(query);
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_Unknown_NumericField_In_Ascending_Order_Does_Not_Throw()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Relevance_Returns_TopHit_Docs_First()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_Relevance_Returns_TopHit_Docs_First()");
            // Arrange
            var added = this.AddStandardSearchDocs();

			Logger.LogDebug("SearchController_GetResult_Sort_By_Relevance_Returns_TopHit_Docs_First(): added → {Added}", added);
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.Relevance,
                KeyWords = "brown OR fox",
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Sort_By_Relevance_Returns_TopHit_Docs_First(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));
            Assert.That(search.Results[0].Snippet, Does.Contain("brown"));
            Assert.That(search.Results[0].Snippet, Does.Contain("dog"));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_Relevance_Returns_TopHit_Docs_First()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_RelevanceAndTitleKeyword_Returns_TopHit_Docs_First()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_RelevanceAndTitleKeyword_Returns_TopHit_Docs_First()");
            this.hostController.AddSetting(Constants.SearchTitleBoostSetting, CustomBoost.ToString(CultureInfo.InvariantCulture));

            // Arrange
            var added = this.AddSearchDocsForCustomBoost();
            this.CreateNewLuceneControllerInstance(true);
			Logger.LogDebug("SearchController_GetResult_Sort_By_RelevanceAndTitleKeyword_Returns_TopHit_Docs_First(): added → {Added}", added);

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.Relevance,
                KeyWords = "Hello",
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Sort_By_RelevanceAndTitleKeyword_Returns_TopHit_Docs_First(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));
            Assert.That(search.Results[0].Body, Does.Contain("Hello1"));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_RelevanceAndTitleKeyword_Returns_TopHit_Docs_First()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_RelevanceAndSubjectKeyword_Returns_TopHit_Docs_First()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_RelevanceAndSubjectKeyword_Returns_TopHit_Docs_First()");
            this.hostController.AddSetting(Constants.SearchContentBoostSetting, CustomBoost.ToString(CultureInfo.InvariantCulture));
            this.CreateNewLuceneControllerInstance(true);

            // Arrange
            var added = this.AddSearchDocsForCustomBoost();

			Logger.LogDebug("SearchController_GetResult_Sort_By_RelevanceAndSubjectKeyword_Returns_TopHit_Docs_First(): added → {Added}", added);
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.Relevance,
                KeyWords = "Hello",
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Sort_By_RelevanceAndSubjectKeyword_Returns_TopHit_Docs_First(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));
            Assert.That(search.Results[0].Body, Does.Contain("Hello2"));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_RelevanceAndSubjectKeyword_Returns_TopHit_Docs_First()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_RelevanceAndCommentKeyword_Returns_TopHit_Docs_First()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_RelevanceAndCommentKeyword_Returns_TopHit_Docs_First()");
            this.hostController.AddSetting(Constants.SearchDescriptionBoostSetting, CustomBoost.ToString(CultureInfo.InvariantCulture));
            this.CreateNewLuceneControllerInstance(true);

            // Arrange
            var added = this.AddSearchDocsForCustomBoost();

			Logger.LogDebug("SearchController_GetResult_Sort_By_RelevanceAndCommentKeyword_Returns_TopHit_Docs_First(): added → {Added}", added);
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.Relevance,
                KeyWords = "Hello",
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Sort_By_RelevanceAndCommentKeyword_Returns_TopHit_Docs_First(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));
            Assert.That(search.Results[0].Body, Does.Contain("Hello3"));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_RelevanceAndCommentKeyword_Returns_TopHit_Docs_First()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_RelevanceAndAuthorKeyword_Returns_TopHit_Docs_First()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_RelevanceAndAuthorKeyword_Returns_TopHit_Docs_First()");
            this.hostController.AddSetting(Constants.SearchAuthorBoostSetting, CustomBoost.ToString(CultureInfo.InvariantCulture));
            this.CreateNewLuceneControllerInstance(true);

            // Arrange
            var added = this.AddSearchDocsForCustomBoost();

			Logger.LogDebug("SearchController_GetResult_Sort_By_RelevanceAndAuthorKeyword_Returns_TopHit_Docs_First(): added → {Added}", added);
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.Relevance,
                KeyWords = "Hello",
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Sort_By_RelevanceAndAuthorKeyword_Returns_TopHit_Docs_First(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));
            Assert.That(search.Results[0].Body, Does.Contain("Hello4"));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_RelevanceAndAuthorKeyword_Returns_TopHit_Docs_First()");
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Relevance_Ascending_Does_Not_Change_Sequence_Of_Results()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Sort_By_Relevance_Ascending_Does_Not_Change_Sequence_Of_Results()");
            // Arrange
            var added = this.AddStandardSearchDocs();

			Logger.LogDebug("SearchController_GetResult_Sort_By_Relevance_Ascending_Does_Not_Change_Sequence_Of_Results(): added → {Added}", added);
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.Relevance,
                SortDirection = SortDirections.Ascending,
                KeyWords = "brown OR fox",
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Sort_By_Relevance_Ascending_Does_Not_Change_Sequence_Of_Results(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(added));
            Assert.That(search.Results[0].Snippet.Contains("brown") && search.Results[0].Snippet.Contains("dog"), Is.EqualTo(true));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Sort_By_Relevance_Ascending_Does_Not_Change_Sequence_Of_Results()");
        }

        [Test]
        public void SearchController_GetResult_By_Locale_Returns_Specific_And_Neutral_Locales()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_By_Locale_Returns_Specific_And_Neutral_Locales()");
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = [ModuleSearchTypeId,],
                SortField = SortFields.LastModified,
                CultureCode = CultureItIt,
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_By_Locale_Returns_Specific_And_Neutral_Locales(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(2), "Found: " + string.Join(Environment.NewLine, search.Results.Select(r => r.Title)));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(search.Results[0].Title, Is.EqualTo(Line3));
                Assert.That(search.Results[1].Title, Is.EqualTo(Line1));
            }
			Logger.LogDebug("<<< Exiting SearchController_GetResult_By_Locale_Returns_Specific_And_Neutral_Locales()");
        }

        [Test]
        public void SearchController_EnsureOldDocument_Deleted_Upon_Second_Index_Content_With_Same_Key()
        {
			Logger.LogDebug(">>> Entering SearchController_EnsureOldDocument_Deleted_Upon_Second_Index_Content_With_Same_Key()");
            // Arrange
            string[] docs =
            {
                Line1,
                Line2,
                };
            const string docKey = "key1";

            // Act

            // Add first document
            var doc1 = new SearchDocument { Title = docs[0], UniqueKey = docKey, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            this.internalSearchController.AddSearchDocument(doc1);

            // Add second document with same key
            var doc2 = new SearchDocument { Title = docs[1], UniqueKey = docKey, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            this.internalSearchController.AddSearchDocument(doc2);

            // run luceneQuery on common keyword between both the docs
            var search = this.SearchForKeyword("fox");

			Logger.LogDebug("SearchController_EnsureOldDocument_Deleted_Upon_Second_Index_Content_With_Same_Key(): search → {Search}", search);
            // Assert - there should just be one entry - first one must have been removed.
            Assert.That(search.Results, Has.Count.EqualTo(1));
            Assert.That(search.Results[0].Title, Is.EqualTo(docs[1]));
			Logger.LogDebug("<<< Exiting SearchController_EnsureOldDocument_Deleted_Upon_Second_Index_Content_With_Same_Key()");
        }

        [Test]
        public void SearchController_Add_Does_Not_Throw_On_Empty_Url()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Does_Not_Throw_On_Empty_Url()");
            var doc = new SearchDocument
            {
                UniqueKey = Guid.NewGuid().ToString(),
                SearchTypeId = OtherSearchTypeId,
                Title = " ",
                ModifiedTimeUtc = DateTime.UtcNow,
                QueryString = "?foo=bar",
            };

            Assert.DoesNotThrow(() => this.internalSearchController.AddSearchDocument(doc));
			Logger.LogDebug("<<< Exiting SearchController_Add_Does_Not_Throw_On_Empty_Url()");
        }

        [Test]
        public void SearchController_Add_Does_Not_Throw_On_Empty_Title()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Does_Not_Throw_On_Empty_Title()");
            var doc = new SearchDocument
            {
                UniqueKey = Guid.NewGuid().ToString(),
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
            };

            Assert.DoesNotThrow(() => this.internalSearchController.AddSearchDocument(doc));
			Logger.LogDebug("<<< Exiting SearchController_Add_Does_Not_Throw_On_Empty_Title()");
        }

        [Test]
        public void SearchController_EnsureOldDocument_Deleted_Upon_Second_Index_When_IsActive_Is_False()
        {
			Logger.LogDebug(">>> Entering SearchController_EnsureOldDocument_Deleted_Upon_Second_Index_When_IsActive_Is_False()");
            // Arrange
            string[] docs =
            {
                Line1,
                Line2,
                };
            const string docKey = "key1";

            // Act

            // Add first document
            var doc1 = new SearchDocument { Title = docs[0], UniqueKey = docKey, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            this.internalSearchController.AddSearchDocument(doc1);

            // Add second document with same key
            var doc2 = new SearchDocument { Title = docs[1], UniqueKey = docKey, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, IsActive = false };
            this.internalSearchController.AddSearchDocument(doc2);

            // run luceneQuery on common keyword between both the docs
            var search = this.SearchForKeyword("fox");

			Logger.LogDebug("SearchController_EnsureOldDocument_Deleted_Upon_Second_Index_When_IsActive_Is_False(): search → {Search}", search);
            // Assert - there should not be any record.
            Assert.That(search.Results, Is.Empty);
			Logger.LogDebug("<<< Exiting SearchController_EnsureOldDocument_Deleted_Upon_Second_Index_When_IsActive_Is_False()");
        }

        // Note: these tests needs to pass through the analyzer which is utilized
        //       in SearchControllerImpl but not LuceneControllerImpl.
        [Test]
        public void SearchController_SearchFindsAccentedAndNonAccentedWords()
        {
			Logger.LogDebug(">>> Entering SearchController_SearchFindsAccentedAndNonAccentedWords()");
            // Arrange
            string[] lines =
            {
                "zèbre or panthère",
                "zebre without accent",
                "panthere without accent",
                };

            this.AddLinesAsSearchDocs(lines);

            // Act
            var searches1 = this.SearchForKeyword("zèbre");
            var searches2 = this.SearchForKeyword("zebre");
			Logger.LogDebug("SearchController_SearchFindsAccentedAndNonAccentedWords(): searches1 → {Searches1}", searches1);
			Logger.LogDebug("SearchController_SearchFindsAccentedAndNonAccentedWords(): searches2 → {Searches2}", searches2);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(searches1.TotalHits, Is.EqualTo(2));
                Assert.That(StripEllipses(searches1.Results[0].Snippet).Trim(), Is.EqualTo("<b>z&#232;bre</b> or panth&#232;re"));
                Assert.That(StripEllipses(searches1.Results[1].Snippet).Trim(), Is.EqualTo("<b>zebre</b> without accent"));

                Assert.That(searches2.TotalHits, Is.EqualTo(2));
                Assert.That(StripEllipses(searches2.Results[0].Snippet).Trim(), Is.EqualTo("<b>z&#232;bre</b> or panth&#232;re"));
                Assert.That(StripEllipses(searches2.Results[1].Snippet).Trim(), Is.EqualTo("<b>zebre</b> without accent"));
            }
			Logger.LogDebug("<<< Exiting SearchController_SearchFindsAccentedAndNonAccentedWords()");
        }

        [Test]
        public void SearchController_PorterFilterTest()
        {
			Logger.LogDebug(">>> Entering SearchController_PorterFilterTest()");
            // Arrange
            string[] lines =
            {
                "field1_value",
                "field2_value",
                };

            this.AddLinesAsSearchDocs(lines);

            // Act
            var search1 = this.SearchForKeyword(lines[0]);
            var search2 = this.SearchForKeyword("\"" + lines[1] + "\"");
			Logger.LogDebug("SearchController_PorterFilterTest(): search1 → {Search1}", search1);
			Logger.LogDebug("SearchController_PorterFilterTest(): search2 → {Search2}", search2);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(search1.TotalHits, Is.EqualTo(1));
                Assert.That(search2.TotalHits, Is.EqualTo(1));

                Assert.That(StripEllipses(search1.Results[0].Snippet).Trim(), Is.EqualTo("<b>" + lines[0] + "</b>"));
                Assert.That(StripEllipses(search2.Results[0].Snippet).Trim(), Is.EqualTo("<b>" + lines[1] + "</b>"));
            }
			Logger.LogDebug("<<< Exiting SearchController_PorterFilterTest()");
        }

        [Test]
        public void SearchController_SearchFindsStemmedWords()
        {
			Logger.LogDebug(">>> Entering SearchController_SearchFindsStemmedWords()");
            // Arrange
            string[] lines =
            {
                "I ride my bike to work",
                "All team are riding their bikes",
                "The boy rides his bike to school",
                "This sentence is missing the bike ri... word",
                };

            this.AddLinesAsSearchDocs(lines);

            // Act
            var search = this.SearchForKeyword("ride");

			Logger.LogDebug("SearchController_SearchFindsStemmedWords(): search → {Search}", search);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(search.TotalHits, Is.EqualTo(3));
                Assert.That(StripEllipses(search.Results[0].Snippet), Is.EqualTo("I <b>ride</b> my bike to work"));
                Assert.That(StripEllipses(search.Results[1].Snippet), Is.EqualTo("m are <b>riding</b> their bikes"));
                Assert.That(StripEllipses(search.Results[2].Snippet), Is.EqualTo("e boy <b>rides</b> his bike to school"));
            }
			Logger.LogDebug("<<< Exiting SearchController_SearchFindsStemmedWords()");
        }

        [Test]
        public void SearchController_Search_Synonym_Works()
        {
			Logger.LogDebug(">>> Entering SearchController_Search_Synonym_Works()");
            // Arrange
            var added = this.AddStandardSearchDocs();

			Logger.LogDebug("SearchController_Search_Synonym_Works(): added → {Added}", added);
            // Act
            var search = this.SearchForKeywordInModule("wolf");

			Logger.LogDebug("SearchController_Search_Synonym_Works(): search → {Search}", search);
            // Assert
            Assert.That(search.TotalHits, Is.EqualTo(added));

            var snippets = search.Results.Select(result => StripEllipses(result.Snippet)).OrderBy(s => s).ToArray();
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("SearchController_Search_Synonym_Works(): snippets → {Snippets}", snippets);
                Assert.That(snippets[0], Is.EqualTo("brown <b>fox</b> jumps over the lazy dog"));
                Assert.That(snippets[1], Is.EqualTo("e red <b>fox</b> jumped over the lazy dark gray dog"));
                Assert.That(snippets[2], Is.EqualTo("gold <b>fox</b> jumped over the lazy black dog"));
                Assert.That(snippets[3], Is.EqualTo("quick <b>fox</b> jumps over the black dog - Italian"));
				Logger.LogDebug(">>> Entering StripEllipses(text={Text})", text);
				Logger.LogDebug("<<< Exiting StripEllipses(text={Text})", text);
            }
        }

        [Test]
        public void SearchController_Title_Ranked_Higher_Than_Body()
        {
			Logger.LogDebug(">>> Entering SearchController_Title_Ranked_Higher_Than_Body()");
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "cow is gone", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "Hello World" };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "cow is gone" };
			Logger.LogDebug("SearchController_Title_Ranked_Higher_Than_Body(): doc1 → {Doc1}", doc1);
			Logger.LogDebug("SearchController_Title_Ranked_Higher_Than_Body(): doc2 → {Doc2}", doc2);
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "I'm here", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "random text" };

			Logger.LogDebug("SearchController_Title_Ranked_Higher_Than_Body(): doc3 → {Doc3}", doc3);
            // Act
            this.internalSearchController.AddSearchDocument(doc1);
            this.internalSearchController.AddSearchDocument(doc2);
            this.internalSearchController.AddSearchDocument(doc3);
            this.internalSearchController.Commit();

            var result = this.SearchForKeyword("cow");

			Logger.LogDebug("SearchController_Title_Ranked_Higher_Than_Body(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(2));
                Assert.That(result.Results[0].UniqueKey, Is.EqualTo(doc1.UniqueKey));
                Assert.That(result.Results[1].UniqueKey, Is.EqualTo(doc2.UniqueKey));
            }
			Logger.LogDebug("<<< Exiting SearchController_Title_Ranked_Higher_Than_Body()");
        }

        [Test]
        public void SearchController_Title_Ranked_Higher_Than_Body_Regardless_Of_Document_Sequence()
        {
			Logger.LogDebug(">>> Entering SearchController_Title_Ranked_Higher_Than_Body_Regardless_Of_Document_Sequence()");
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "cow is gone", };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "I'm here", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "random text", };
			Logger.LogDebug("SearchController_Title_Ranked_Higher_Than_Body_Regardless_Of_Document_Sequence(): doc1 → {Doc1}", doc1);
			Logger.LogDebug("SearchController_Title_Ranked_Higher_Than_Body_Regardless_Of_Document_Sequence(): doc2 → {Doc2}", doc2);
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "cow is gone", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, };

			Logger.LogDebug("SearchController_Title_Ranked_Higher_Than_Body_Regardless_Of_Document_Sequence(): doc3 → {Doc3}", doc3);
            // Act
            this.internalSearchController.AddSearchDocument(doc1);
            this.internalSearchController.AddSearchDocument(doc2);
            this.internalSearchController.AddSearchDocument(doc3);
            this.internalSearchController.Commit();

            var result = this.SearchForKeyword("cow");

			Logger.LogDebug("SearchController_Title_Ranked_Higher_Than_Body_Regardless_Of_Document_Sequence(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(2));
                Assert.That(result.Results[0].UniqueKey, Is.EqualTo(doc3.UniqueKey));
                Assert.That(result.Results[1].UniqueKey, Is.EqualTo(doc1.UniqueKey));
            }
			Logger.LogDebug("<<< Exiting SearchController_Title_Ranked_Higher_Than_Body_Regardless_Of_Document_Sequence()");
        }

        [Test]
        public void SearchController_Title_Ranked_Higher_Than_Tag()
        {
			Logger.LogDebug(">>> Entering SearchController_Title_Ranked_Higher_Than_Tag()");
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "cow", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "Hello World" };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Tags = new List<string> { "cow", "hello", "world" } };
			Logger.LogDebug("SearchController_Title_Ranked_Higher_Than_Tag(): doc1 → {Doc1}", doc1);
			Logger.LogDebug("SearchController_Title_Ranked_Higher_Than_Tag(): doc2 → {Doc2}", doc2);
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "I'm here", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "random text" };

			Logger.LogDebug("SearchController_Title_Ranked_Higher_Than_Tag(): doc3 → {Doc3}", doc3);
            // Act
            this.internalSearchController.AddSearchDocument(doc1);
            this.internalSearchController.AddSearchDocument(doc2);
            this.internalSearchController.AddSearchDocument(doc3);
            this.internalSearchController.Commit();
            var result = this.SearchForKeyword("cow");

			Logger.LogDebug("SearchController_Title_Ranked_Higher_Than_Tag(): result → {Result}", result);
            // Assert
            Assert.That(result.TotalHits, Is.EqualTo(2));
            Console.WriteLine("first score: {0}  {1}", result.Results[0].UniqueKey, result.Results[0].DisplayScore);
            Console.WriteLine("second score: {0}  {1}", result.Results[1].UniqueKey, result.Results[1].DisplayScore);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Results[0].UniqueKey, Is.EqualTo(doc1.UniqueKey));
                Assert.That(result.Results[1].UniqueKey, Is.EqualTo(doc2.UniqueKey));
            }
			Logger.LogDebug("<<< Exiting SearchController_Title_Ranked_Higher_Than_Tag()");
        }

        [Test]
        public void SearchController_RankingTest_With_Vowel()
        {
			Logger.LogDebug(">>> Entering SearchController_RankingTest_With_Vowel()");
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "tootsie", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Tags = new List<string> { "tootsie" } };
			Logger.LogDebug("SearchController_RankingTest_With_Vowel(): doc1 → {Doc1}", doc1);
			Logger.LogDebug("SearchController_RankingTest_With_Vowel(): doc2 → {Doc2}", doc2);
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Keywords = new Dictionary<string, string>() { { KeyWord1Name, "tootsie" } } };
            var doc4 = new SearchDocument { UniqueKey = "key04", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Description = "tootsie" };
			Logger.LogDebug("SearchController_RankingTest_With_Vowel(): doc3 → {Doc3}", doc3);
			Logger.LogDebug("SearchController_RankingTest_With_Vowel(): doc4 → {Doc4}", doc4);
            var doc5 = new SearchDocument { UniqueKey = "key05", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "hello tootsie" };

			Logger.LogDebug("SearchController_RankingTest_With_Vowel(): doc5 → {Doc5}", doc5);
            // Act
            this.internalSearchController.AddSearchDocument(doc1);
            this.internalSearchController.AddSearchDocument(doc2);
            this.internalSearchController.AddSearchDocument(doc3);
            this.internalSearchController.AddSearchDocument(doc4);
            this.internalSearchController.AddSearchDocument(doc5);

            this.internalSearchController.Commit();

            var result = this.SearchForKeyword("tootsie");

			Logger.LogDebug("SearchController_RankingTest_With_Vowel(): result → {Result}", result);
            // Assert
            Assert.That(result.TotalHits, Is.EqualTo(5));
            foreach (var searchResult in result.Results)
            {
                Console.WriteLine("{0} score: {1}", searchResult.UniqueKey, searchResult.DisplayScore);
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Results[0].UniqueKey, Is.EqualTo(doc1.UniqueKey));
                Assert.That(result.Results[1].UniqueKey, Is.EqualTo(doc2.UniqueKey));
                Assert.That(result.Results[2].UniqueKey, Is.EqualTo(doc3.UniqueKey));
                Assert.That(result.Results[3].UniqueKey, Is.EqualTo(doc4.UniqueKey));
            }
			Logger.LogDebug("<<< Exiting SearchController_RankingTest_With_Vowel()");
        }

        [Test]
        public void SearchController_FileNameTest_With_WildCard()
        {
			Logger.LogDebug(">>> Entering SearchController_FileNameTest_With_WildCard()");
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "file.ext", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

			Logger.LogDebug("SearchController_FileNameTest_With_WildCard(): doc1 → {Doc1}", doc1);
            // Act
            this.internalSearchController.AddSearchDocument(doc1);

            this.internalSearchController.Commit();

            var result = this.SearchForKeywordWithWildCard("file");

			Logger.LogDebug("SearchController_FileNameTest_With_WildCard(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(1));
                Assert.That(result.Results[0].UniqueKey, Is.EqualTo(doc1.UniqueKey));
            }
			Logger.LogDebug("<<< Exiting SearchController_FileNameTest_With_WildCard()");
        }

        [Test]
        public void SearchController_Full_FileNameTest_Without_WildCard()
        {
			Logger.LogDebug(">>> Entering SearchController_Full_FileNameTest_Without_WildCard()");
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "file.ext", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

			Logger.LogDebug("SearchController_Full_FileNameTest_Without_WildCard(): doc1 → {Doc1}", doc1);
            // Act
            this.internalSearchController.AddSearchDocument(doc1);

            this.internalSearchController.Commit();

            var result = this.SearchForKeywordWithWildCard("file.ext");

			Logger.LogDebug("SearchController_Full_FileNameTest_Without_WildCard(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(1));
                Assert.That(result.Results[0].UniqueKey, Is.EqualTo(doc1.UniqueKey));
            }
			Logger.LogDebug("<<< Exiting SearchController_Full_FileNameTest_Without_WildCard()");
        }

        [Test]
        public void SearchController_Full_FileNameTest_With_WildCard()
        {
			Logger.LogDebug(">>> Entering SearchController_Full_FileNameTest_With_WildCard()");
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "file.ext", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

			Logger.LogDebug("SearchController_Full_FileNameTest_With_WildCard(): doc1 → {Doc1}", doc1);
            // Act
            this.internalSearchController.AddSearchDocument(doc1);

            this.internalSearchController.Commit();

            var result = this.SearchForKeyword("file.ext");

			Logger.LogDebug("SearchController_Full_FileNameTest_With_WildCard(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(1));
                Assert.That(result.Results[0].UniqueKey, Is.EqualTo(doc1.UniqueKey));
            }
			Logger.LogDebug("<<< Exiting SearchController_Full_FileNameTest_With_WildCard()");
        }

        [Test]
        public void SearchController_Scope_By_FolderName()
        {
			Logger.LogDebug(">>> Entering SearchController_Scope_By_FolderName()");
            // Arrange
            this.AddFoldersAndFiles();

            // Act
            var result1 = this.SearchForKeyword("kw-folderName:Images/*");
            var result2 = this.SearchForKeyword("kw-folderName:Images/DNN/*");
			Logger.LogDebug("SearchController_Scope_By_FolderName(): result1 → {Result1}", result1);
			Logger.LogDebug("SearchController_Scope_By_FolderName(): result2 → {Result2}", result2);
            var result3 = this.SearchForKeywordWithWildCard("kw-folderName:Images/* AND spacer");

			Logger.LogDebug("SearchController_Scope_By_FolderName(): result3 → {Result3}", result3);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result1.TotalHits, Is.EqualTo(5));
                Assert.That(result2.TotalHits, Is.EqualTo(2));
                Assert.That(result3.TotalHits, Is.EqualTo(1));
            }
			Logger.LogDebug("<<< Exiting SearchController_Scope_By_FolderName()");
        }

        [Test]
        public void SearchController_Scope_By_FolderName_With_Spaces()
        {
			Logger.LogDebug(">>> Entering SearchController_Scope_By_FolderName_With_Spaces()");
            // Arrange
            this.AddFoldersAndFiles();

            // Act - Space is replaced by <
            var query1 = new SearchQuery { KeyWords = "kw-folderName:Images/*", SearchTypeIds = new[] { OtherSearchTypeId }, WildCardSearch = false };
            var query2 = new SearchQuery { KeyWords = "kw-folderName:my<Images/*", SearchTypeIds = new[] { OtherSearchTypeId }, WildCardSearch = true };
			Logger.LogDebug("SearchController_Scope_By_FolderName_With_Spaces(): query1 → {Query1}", query1);
			Logger.LogDebug("SearchController_Scope_By_FolderName_With_Spaces(): query2 → {Query2}", query2);
            var query3 = new SearchQuery { KeyWords = "kw-folderName:my<Images/my<dnn/*", SearchTypeIds = new[] { OtherSearchTypeId }, WildCardSearch = true };
            var result1 = this.searchController.SiteSearch(query1);
			Logger.LogDebug("SearchController_Scope_By_FolderName_With_Spaces(): query3 → {Query3}", query3);
			Logger.LogDebug("SearchController_Scope_By_FolderName_With_Spaces(): result1 → {Result1}", result1);
            var result2 = this.searchController.SiteSearch(query2);
            var result3 = this.searchController.SiteSearch(query3);
			Logger.LogDebug("SearchController_Scope_By_FolderName_With_Spaces(): result2 → {Result2}", result2);
			Logger.LogDebug("SearchController_Scope_By_FolderName_With_Spaces(): result3 → {Result3}", result3);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result1.TotalHits, Is.EqualTo(5));
                Assert.That(result2.TotalHits, Is.EqualTo(5));
                Assert.That(result3.TotalHits, Is.EqualTo(2));
            }
			Logger.LogDebug("<<< Exiting SearchController_Scope_By_FolderName_With_Spaces()");
        }

        [Test]
        public void SearchController_EmailTest_With_WildCard()
        {
			Logger.LogDebug(">>> Entering SearchController_EmailTest_With_WildCard()");
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "email@domain.com", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

			Logger.LogDebug("SearchController_EmailTest_With_WildCard(): doc1 → {Doc1}", doc1);
            // Act
            this.internalSearchController.AddSearchDocument(doc1);

            this.internalSearchController.Commit();

            var result = this.SearchForKeywordWithWildCard("email@");

			Logger.LogDebug("SearchController_EmailTest_With_WildCard(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(1));
                Assert.That(result.Results[0].UniqueKey, Is.EqualTo(doc1.UniqueKey));
            }
			Logger.LogDebug("<<< Exiting SearchController_EmailTest_With_WildCard()");
        }

        [Test]
        public void SearchController_Add_Use_Of_Module_Search_Type_Requires_ModuleDefinitionId()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Use_Of_Module_Search_Type_Requires_ModuleDefinitionId()");
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "awesome", SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

			Logger.LogDebug("SearchController_Add_Use_Of_Module_Search_Type_Requires_ModuleDefinitionId(): doc1 → {Doc1}", doc1);
            // Act
            Assert.Throws<ArgumentException>(() => this.internalSearchController.AddSearchDocument(doc1));
			Logger.LogDebug("<<< Exiting SearchController_Add_Use_Of_Module_Search_Type_Requires_ModuleDefinitionId()");
        }

        [Test]
        public void SearchController_Add_Use_Of_Module_Search_Type_Requires_ModuleId()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Use_Of_Module_Search_Type_Requires_ModuleId()");
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "awesome", SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = HtmlModuleDefId };

			Logger.LogDebug("SearchController_Add_Use_Of_Module_Search_Type_Requires_ModuleId(): doc1 → {Doc1}", doc1);
            // Act
            Assert.Throws<ArgumentException>(() => this.internalSearchController.AddSearchDocument(doc1));
			Logger.LogDebug("<<< Exiting SearchController_Add_Use_Of_Module_Search_Type_Requires_ModuleId()");
        }

        [Test]
        public void SearchController_Add_Non_Module_Type_SearchId_Should_Not_Provide_ModuleDefinitionId()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Non_Module_Type_SearchId_Should_Not_Provide_ModuleDefinitionId()");
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "awesome", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = HtmlModuleDefId };

			Logger.LogDebug("SearchController_Add_Non_Module_Type_SearchId_Should_Not_Provide_ModuleDefinitionId(): doc1 → {Doc1}", doc1);
            // Act
            Assert.Throws<ArgumentException>(() => this.internalSearchController.AddSearchDocument(doc1));
			Logger.LogDebug("<<< Exiting SearchController_Add_Non_Module_Type_SearchId_Should_Not_Provide_ModuleDefinitionId()");
        }

        [Test]
        public void SearchController_Add_Non_Module_Type_SearchId_Should_Not_Provide_ModuleId()
        {
			Logger.LogDebug(">>> Entering SearchController_Add_Non_Module_Type_SearchId_Should_Not_Provide_ModuleId()");
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "awesome", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleId = HtmlModuleId };

			Logger.LogDebug("SearchController_Add_Non_Module_Type_SearchId_Should_Not_Provide_ModuleId(): doc1 → {Doc1}", doc1);
            // Act
            Assert.Throws<ArgumentException>(() => this.internalSearchController.AddSearchDocument(doc1));
			Logger.LogDebug("<<< Exiting SearchController_Add_Non_Module_Type_SearchId_Should_Not_Provide_ModuleId()");
        }

        [Test]
        public void SearchController_Search_For_ModuleId_Must_Have_Only_One_Search_Type_Id_Specified()
        {
			Logger.LogDebug(">>> Entering SearchController_Search_For_ModuleId_Must_Have_Only_One_Search_Type_Id_Specified()");
            // Arrange
            const string keyword = "awesome";

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { DocumentSearchTypeId, OtherSearchTypeId }, ModuleId = IdeasModuleId };

            // Act
            Assert.Throws<ArgumentException>(() => this.searchController.SiteSearch(query));
			Logger.LogDebug("<<< Exiting SearchController_Search_For_ModuleId_Must_Have_Only_One_Search_Type_Id_Specified()");
        }

        [Test]
        public void SearchController_Search_For_ModuleId_Must_Have_Only_Module_Search_Type_Id_Specified()
        {
			Logger.LogDebug(">>> Entering SearchController_Search_For_ModuleId_Must_Have_Only_Module_Search_Type_Id_Specified()");
            // Arrange
            const string keyword = "awesome";

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId, OtherSearchTypeId }, ModuleId = IdeasModuleId };

            // Act
            Assert.Throws<ArgumentException>(() => this.searchController.SiteSearch(query));
			Logger.LogDebug("<<< Exiting SearchController_Search_For_ModuleId_Must_Have_Only_Module_Search_Type_Id_Specified()");
        }

        [Test]
        public void SearchController_Search_For_Unknown_SearchTypeId_Does_Not_Throw_Exception()
        {
			Logger.LogDebug(">>> Entering SearchController_Search_For_Unknown_SearchTypeId_Does_Not_Throw_Exception()");
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = UnknownSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            this.internalSearchController.AddSearchDocument(doc1);
			Logger.LogDebug("SearchController_Search_For_Unknown_SearchTypeId_Does_Not_Throw_Exception(): doc1 → {Doc1}", doc1);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { UnknownSearchTypeId } };

            // Act
            var result = this.searchController.SiteSearch(query);
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("SearchController_Search_For_Unknown_SearchTypeId_Does_Not_Throw_Exception(): result → {Result}", result);
                Assert.That(result.TotalHits, Is.EqualTo(0)); // 0 due to security trimming
                Assert.That(result.Results, Is.Empty);
            }
			Logger.LogDebug("<<< Exiting SearchController_Search_For_Unknown_SearchTypeId_Does_Not_Throw_Exception()");
        }

        [Test]
        public void SearchController_Search_For_GroupId_Zero_Ignores_GroupId()
        {
			Logger.LogDebug(">>> Entering SearchController_Search_For_GroupId_Zero_Ignores_GroupId()");
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId731 };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId0 };
			Logger.LogDebug("SearchController_Search_For_GroupId_Zero_Ignores_GroupId(): doc1 → {Doc1}", doc1);
			Logger.LogDebug("SearchController_Search_For_GroupId_Zero_Ignores_GroupId(): doc2 → {Doc2}", doc2);
            this.internalSearchController.AddSearchDocument(doc1);
            this.internalSearchController.AddSearchDocument(doc2);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { OtherSearchTypeId }, RoleId = 0 };

            // Act
            var result = this.searchController.SiteSearch(query);
            Assert.That(result.TotalHits, Is.EqualTo(2));
			Logger.LogDebug("<<< Exiting SearchController_Search_For_GroupId_Zero_Ignores_GroupId()");
			Logger.LogDebug("SearchController_Search_For_GroupId_Zero_Ignores_GroupId(): result → {Result}", result);
        }

        [Test]
        public void SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only()
        {
			Logger.LogDebug(">>> Entering SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only()");
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId731 };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId532 };
			Logger.LogDebug("SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only(): doc1 → {Doc1}", doc1);
			Logger.LogDebug("SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only(): doc2 → {Doc2}", doc2);
            this.internalSearchController.AddSearchDocument(doc1);
            this.internalSearchController.AddSearchDocument(doc2);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { OtherSearchTypeId }, RoleId = RoleId731 };

            // Act
            var result = this.searchController.SiteSearch(query);
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only(): result → {Result}", result);
                Assert.That(result.TotalHits, Is.EqualTo(1));
                Assert.That(result.Results[0].RoleId, Is.EqualTo(RoleId731));
            }
			Logger.LogDebug("<<< Exiting SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only()");
        }

        [Test]
        public void SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only_Even_In_Multi_SearchTypeId()
        {
			Logger.LogDebug(">>> Entering SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only_Even_In_Multi_SearchTypeId()");
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId731 };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId532 };
			Logger.LogDebug("SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only_Even_In_Multi_SearchTypeId(): doc1 → {Doc1}", doc1);
			Logger.LogDebug("SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only_Even_In_Multi_SearchTypeId(): doc2 → {Doc2}", doc2);
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };

			Logger.LogDebug("SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only_Even_In_Multi_SearchTypeId(): doc3 → {Doc3}", doc3);
            this.internalSearchController.AddSearchDocument(doc1);
            this.internalSearchController.AddSearchDocument(doc2);
            this.internalSearchController.AddSearchDocument(doc3);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { OtherSearchTypeId, ModuleSearchTypeId }, RoleId = RoleId731 };

            // Act
            var result = this.searchController.SiteSearch(query);
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only_Even_In_Multi_SearchTypeId(): result → {Result}", result);
                Assert.That(result.TotalHits, Is.EqualTo(1));
                Assert.That(result.Results[0].RoleId, Is.EqualTo(RoleId731));
            }
			Logger.LogDebug("<<< Exiting SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only_Even_In_Multi_SearchTypeId()");
        }

        [Test]
        public void SearchController_Search_For_Two_ModuleDefinitions_Returns_Two_Only()
        {
			Logger.LogDebug(">>> Entering SearchController_Search_For_Two_ModuleDefinitions_Returns_Two_Only()");
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
			Logger.LogDebug("SearchController_Search_For_Two_ModuleDefinitions_Returns_Two_Only(): doc1 → {Doc1}", doc1);
			Logger.LogDebug("SearchController_Search_For_Two_ModuleDefinitions_Returns_Two_Only(): doc2 → {Doc2}", doc2);
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = BlogsModuleDefId, ModuleId = BlogsModuleId };

			Logger.LogDebug("SearchController_Search_For_Two_ModuleDefinitions_Returns_Two_Only(): doc3 → {Doc3}", doc3);
            // Act
            this.internalSearchController.AddSearchDocument(doc1);
            this.internalSearchController.AddSearchDocument(doc2);
            this.internalSearchController.AddSearchDocument(doc3);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId }, ModuleDefIds = new[] { IdeasModuleDefId, AnswersModuleDefId } };

            var result = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_Search_For_Two_ModuleDefinitions_Returns_Two_Only(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(2));
                Assert.That(result.Results[0].UniqueKey, Is.EqualTo(doc1.UniqueKey));
                Assert.That(result.Results[1].UniqueKey, Is.EqualTo(doc2.UniqueKey));
            }
			Logger.LogDebug("<<< Exiting SearchController_Search_For_Two_ModuleDefinitions_Returns_Two_Only()");
        }

        [Test]
        public void SearchController_Search_For_ModuleId_Returns_from_that_module_Only()
        {
			Logger.LogDebug(">>> Entering SearchController_Search_For_ModuleId_Returns_from_that_module_Only()");
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
			Logger.LogDebug("SearchController_Search_For_ModuleId_Returns_from_that_module_Only(): doc1 → {Doc1}", doc1);
			Logger.LogDebug("SearchController_Search_For_ModuleId_Returns_from_that_module_Only(): doc2 → {Doc2}", doc2);
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
            var doc4 = new SearchDocument { UniqueKey = "key04", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = BlogsModuleDefId, ModuleId = BlogsModuleId };
			Logger.LogDebug("SearchController_Search_For_ModuleId_Returns_from_that_module_Only(): doc3 → {Doc3}", doc3);
			Logger.LogDebug("SearchController_Search_For_ModuleId_Returns_from_that_module_Only(): doc4 → {Doc4}", doc4);

            // Act
            this.internalSearchController.AddSearchDocument(doc1);
            this.internalSearchController.AddSearchDocument(doc2);
            this.internalSearchController.AddSearchDocument(doc3);
            this.internalSearchController.AddSearchDocument(doc4);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId }, ModuleDefIds = new[] { IdeasModuleDefId, AnswersModuleDefId }, ModuleId = IdeasModuleId };

            var result = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_Search_For_ModuleId_Returns_from_that_module_Only(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(2));
                Assert.That(result.Results[0].UniqueKey, Is.EqualTo(doc1.UniqueKey));
                Assert.That(result.Results[1].UniqueKey, Is.EqualTo(doc2.UniqueKey));
            }
			Logger.LogDebug("<<< Exiting SearchController_Search_For_ModuleId_Returns_from_that_module_Only()");
        }

        [Test]
        public void SearchController_Search_For_Module_Search_Type_And_Multiple_ModuleDefIds_Should_Return_From_Those_ModuleDefinitions_Only()
        {
			Logger.LogDebug(">>> Entering SearchController_Search_For_Module_Search_Type_And_Multiple_ModuleDefIds_Should_Return_From_Those_ModuleDefinitions_Only()");
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
			Logger.LogDebug("SearchController_Search_For_Module_Search_Type_And_Multiple_ModuleDefIds_Should_Return_From_Those_ModuleDefinitions_Only(): doc1 → {Doc1}", doc1);
			Logger.LogDebug("SearchController_Search_For_Module_Search_Type_And_Multiple_ModuleDefIds_Should_Return_From_Those_ModuleDefinitions_Only(): doc2 → {Doc2}", doc2);
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
            var doc4 = new SearchDocument { UniqueKey = "key04", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = BlogsModuleDefId, ModuleId = BlogsModuleId };
			Logger.LogDebug("SearchController_Search_For_Module_Search_Type_And_Multiple_ModuleDefIds_Should_Return_From_Those_ModuleDefinitions_Only(): doc3 → {Doc3}", doc3);
			Logger.LogDebug("SearchController_Search_For_Module_Search_Type_And_Multiple_ModuleDefIds_Should_Return_From_Those_ModuleDefinitions_Only(): doc4 → {Doc4}", doc4);

            // Act
            this.internalSearchController.AddSearchDocument(doc1);
            this.internalSearchController.AddSearchDocument(doc2);
            this.internalSearchController.AddSearchDocument(doc3);
            this.internalSearchController.AddSearchDocument(doc4);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId }, ModuleDefIds = new[] { IdeasModuleDefId, AnswersModuleDefId } };

            var result = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_Search_For_Module_Search_Type_And_Multiple_ModuleDefIds_Should_Return_From_Those_ModuleDefinitions_Only(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(3));
                Assert.That(result.Results[0].UniqueKey, Is.EqualTo(doc1.UniqueKey));
                Assert.That(result.Results[1].UniqueKey, Is.EqualTo(doc2.UniqueKey));
                Assert.That(result.Results[2].UniqueKey, Is.EqualTo(doc3.UniqueKey));
            }
			Logger.LogDebug("<<< Exiting SearchController_Search_For_Module_Search_Type_And_Multiple_ModuleDefIds_Should_Return_From_Those_ModuleDefinitions_Only()");
        }

        [Test]
        public void SearchController_Search_For_Multiple_Search_Types_Should_Return_Result_from_All_Sources()
        {
			Logger.LogDebug(">>> Entering SearchController_Search_For_Multiple_Search_Types_Should_Return_Result_from_All_Sources()");
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
			Logger.LogDebug("SearchController_Search_For_Multiple_Search_Types_Should_Return_Result_from_All_Sources(): doc1 → {Doc1}", doc1);
			Logger.LogDebug("SearchController_Search_For_Multiple_Search_Types_Should_Return_Result_from_All_Sources(): doc2 → {Doc2}", doc2);
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
            var doc4 = new SearchDocument { UniqueKey = "key04", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = BlogsModuleDefId, ModuleId = BlogsModuleId };
			Logger.LogDebug("SearchController_Search_For_Multiple_Search_Types_Should_Return_Result_from_All_Sources(): doc3 → {Doc3}", doc3);
			Logger.LogDebug("SearchController_Search_For_Multiple_Search_Types_Should_Return_Result_from_All_Sources(): doc4 → {Doc4}", doc4);
            var doc5 = new SearchDocument { UniqueKey = "key05", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

			Logger.LogDebug("SearchController_Search_For_Multiple_Search_Types_Should_Return_Result_from_All_Sources(): doc5 → {Doc5}", doc5);
            // Act
            this.internalSearchController.AddSearchDocument(doc1);
            this.internalSearchController.AddSearchDocument(doc2);
            this.internalSearchController.AddSearchDocument(doc3);
            this.internalSearchController.AddSearchDocument(doc4);
            this.internalSearchController.AddSearchDocument(doc5);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId, OtherSearchTypeId }, ModuleDefIds = new[] { IdeasModuleDefId, AnswersModuleDefId } };

            var result = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_Search_For_Multiple_Search_Types_Should_Return_Result_from_All_Sources(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(4));
                Assert.That(result.Results[0].UniqueKey, Is.EqualTo(doc1.UniqueKey));
                Assert.That(result.Results[1].UniqueKey, Is.EqualTo(doc2.UniqueKey));
                Assert.That(result.Results[2].UniqueKey, Is.EqualTo(doc3.UniqueKey));
                Assert.That(result.Results[3].UniqueKey, Is.EqualTo(doc5.UniqueKey));
            }
			Logger.LogDebug("<<< Exiting SearchController_Search_For_Multiple_Search_Types_Should_Return_Result_from_All_Sources()");
        }

        [Test]
        public void SearchController_Search_For_ModuleSearchTypeId_With_Two_ModuleDefinitions_And_OtherSearchTypeId_Returns_Correct_Results()
        {
			Logger.LogDebug(">>> Entering SearchController_Search_For_ModuleSearchTypeId_With_Two_ModuleDefinitions_And_OtherSearchTypeId_Returns_Correct_Results()");
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = HtmlModuleDefId, ModuleId = HtmlModuleId };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = HtmlModuleDefId, ModuleId = HtmlModuleId };
			Logger.LogDebug("SearchController_Search_For_ModuleSearchTypeId_With_Two_ModuleDefinitions_And_OtherSearchTypeId_Returns_Correct_Results(): doc1 → {Doc1}", doc1);
			Logger.LogDebug("SearchController_Search_For_ModuleSearchTypeId_With_Two_ModuleDefinitions_And_OtherSearchTypeId_Returns_Correct_Results(): doc2 → {Doc2}", doc2);
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
            var doc4 = new SearchDocument { UniqueKey = "key04", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = BlogsModuleDefId, ModuleId = BlogsModuleId };
			Logger.LogDebug("SearchController_Search_For_ModuleSearchTypeId_With_Two_ModuleDefinitions_And_OtherSearchTypeId_Returns_Correct_Results(): doc3 → {Doc3}", doc3);
			Logger.LogDebug("SearchController_Search_For_ModuleSearchTypeId_With_Two_ModuleDefinitions_And_OtherSearchTypeId_Returns_Correct_Results(): doc4 → {Doc4}", doc4);
            var doc5 = new SearchDocument { UniqueKey = "key05", Title = keyword, SearchTypeId = TabSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

			Logger.LogDebug("SearchController_Search_For_ModuleSearchTypeId_With_Two_ModuleDefinitions_And_OtherSearchTypeId_Returns_Correct_Results(): doc5 → {Doc5}", doc5);
            // Act
            this.internalSearchController.AddSearchDocument(doc1);
            this.internalSearchController.AddSearchDocument(doc2);
            this.internalSearchController.AddSearchDocument(doc3);
            this.internalSearchController.AddSearchDocument(doc4);
            this.internalSearchController.AddSearchDocument(doc5);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId, TabSearchTypeId }, ModuleDefIds = new[] { HtmlModuleDefId } };

            var result = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_Search_For_ModuleSearchTypeId_With_Two_ModuleDefinitions_And_OtherSearchTypeId_Returns_Correct_Results(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(3));
                Assert.That(result.Results[0].UniqueKey, Is.EqualTo(doc1.UniqueKey));
                Assert.That(result.Results[1].UniqueKey, Is.EqualTo(doc2.UniqueKey));
                Assert.That(result.Results[2].UniqueKey, Is.EqualTo(doc5.UniqueKey));
            }
			Logger.LogDebug("<<< Exiting SearchController_Search_For_ModuleSearchTypeId_With_Two_ModuleDefinitions_And_OtherSearchTypeId_Returns_Correct_Results()");
        }

        [Test]
        public void SearchController_GetResult_Works_With_Custom_Numeric_Querirs()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Works_With_Custom_Numeric_Querirs()");
            this.AddDocumentsWithNumericKeys();

            // Act
            var query = new SearchQuery
            {
                NumericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue50 } },
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                WildCardSearch = false,
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Works_With_Custom_Numeric_Querirs(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(1));
            Assert.That(search.Results[0].NumericKeys[NumericKey1], Is.EqualTo(NumericValue50));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Works_With_Custom_Numeric_Querirs()");
        }

        [Test]
        public void SearchController_GetResult_Works_With_CustomKeyword_Querirs()
        {
			Logger.LogDebug(">>> Entering SearchController_GetResult_Works_With_CustomKeyword_Querirs()");
            this.AddDocumentsWithKeywords();

            // Act
            var query = new SearchQuery
            {
                CustomKeywords = new Dictionary<string, string>() { { KeyWord1Name, KeyWord1Value } },
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                WildCardSearch = false,
            };
            var search = this.searchController.SiteSearch(query);

			Logger.LogDebug("SearchController_GetResult_Works_With_CustomKeyword_Querirs(): search → {Search}", search);
            // Assert
            Assert.That(search.Results, Has.Count.EqualTo(1));
            Assert.That(search.Results[0].Keywords[KeyWord1Name], Is.EqualTo(KeyWord1Value));
			Logger.LogDebug("<<< Exiting SearchController_GetResult_Works_With_CustomKeyword_Querirs()");
        }

        [Test]
        public void SearchController_EnableLeadingWildcard_Should_Not_Return_Results_When_Property_Is_False()
        {
			Logger.LogDebug(">>> Entering SearchController_EnableLeadingWildcard_Should_Not_Return_Results_When_Property_Is_False()");
            this.hostController.AddSetting("Search_AllowLeadingWildcard", "N");

            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "cow is gone", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };
			Logger.LogDebug("SearchController_EnableLeadingWildcard_Should_Not_Return_Results_When_Property_Is_False(): doc1 → {Doc1}", doc1);
			Logger.LogDebug("SearchController_EnableLeadingWildcard_Should_Not_Return_Results_When_Property_Is_False(): doc2 → {Doc2}", doc2);
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "I'm here", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };

			Logger.LogDebug("SearchController_EnableLeadingWildcard_Should_Not_Return_Results_When_Property_Is_False(): doc3 → {Doc3}", doc3);
            // Act
            this.internalSearchController.AddSearchDocument(doc1);
            this.internalSearchController.AddSearchDocument(doc2);
            this.internalSearchController.AddSearchDocument(doc3);
            this.internalSearchController.Commit();

            var result = this.SearchForKeyword("rld", OtherSearchTypeId, true, false);

			Logger.LogDebug("SearchController_EnableLeadingWildcard_Should_Not_Return_Results_When_Property_Is_False(): result → {Result}", result);
            // Assert
            Assert.That(result.TotalHits, Is.EqualTo(0));
			Logger.LogDebug("<<< Exiting SearchController_EnableLeadingWildcard_Should_Not_Return_Results_When_Property_Is_False()");
        }

        [Test]
        public void SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_True()
        {
			Logger.LogDebug(">>> Entering SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_True()");
            this.hostController.AddSetting("Search_AllowLeadingWildcard", "N");

            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "cow is gone", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };
			Logger.LogDebug("SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_True(): doc1 → {Doc1}", doc1);
			Logger.LogDebug("SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_True(): doc2 → {Doc2}", doc2);
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "I'm here", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };

			Logger.LogDebug("SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_True(): doc3 → {Doc3}", doc3);
            // Act
            this.internalSearchController.AddSearchDocument(doc1);
            this.internalSearchController.AddSearchDocument(doc2);
            this.internalSearchController.AddSearchDocument(doc3);
            this.internalSearchController.Commit();

            var result = this.SearchForKeyword("rld", OtherSearchTypeId, true, true);

			Logger.LogDebug("SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_True(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(1));
                Assert.That(result.Results[0].UniqueKey, Is.EqualTo(doc2.UniqueKey));
            }
			Logger.LogDebug("<<< Exiting SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_True()");
        }

        [Test]
        public void SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_False_But_Host_Setting_Is_True()
        {
			Logger.LogDebug(">>> Entering SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_False_But_Host_Setting_Is_True()");
            this.hostController.AddSetting("Search_AllowLeadingWildcard", "Y");

            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "cow is gone", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };
			Logger.LogDebug("SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_False_But_Host_Setting_Is_True(): doc1 → {Doc1}", doc1);
			Logger.LogDebug("SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_False_But_Host_Setting_Is_True(): doc2 → {Doc2}", doc2);
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "I'm here", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };

			Logger.LogDebug("SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_False_But_Host_Setting_Is_True(): doc3 → {Doc3}", doc3);
            // Act
            this.internalSearchController.AddSearchDocument(doc1);
            this.internalSearchController.AddSearchDocument(doc2);
            this.internalSearchController.AddSearchDocument(doc3);
            this.internalSearchController.Commit();

            var result = this.SearchForKeyword("rld", OtherSearchTypeId, true, false);

			Logger.LogDebug("SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_False_But_Host_Setting_Is_True(): result → {Result}", result);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.TotalHits, Is.EqualTo(1));
                Assert.That(result.Results[0].UniqueKey, Is.EqualTo(doc2.UniqueKey));
            }
			Logger.LogDebug("<<< Exiting SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_False_But_Host_Setting_Is_True()");
        }

        [Test]
        public void SearchController_Search_StopWords_Works()
        {
			Logger.LogDebug(">>> Entering SearchController_Search_StopWords_Works()");
            // Arrange
            var added = this.AddStandardSearchDocs();
            this.internalSearchController.Commit();
			Logger.LogDebug("SearchController_Search_StopWords_Works(): added → {Added}", added);

            // Act
            var search = this.SearchForKeywordInModule("the");

			Logger.LogDebug("SearchController_Search_StopWords_Works(): search → {Search}", search);
            // Assert
            // the word "the" is ignored in all languages except es-ES
            Assert.That(search.TotalHits, Is.EqualTo(1), "Found: " + string.Join(Environment.NewLine, search.Results.Select(r => r.Title)));

            // Act
            search = this.SearchForKeywordInModule("over");

            // Assert
            // we won't find "over" in neutral, en-US, and en-CA documents, but will find it in the es-ES and it-IT documents.
            Assert.That(search.TotalHits, Is.EqualTo(2), "Found: " + string.Join(Environment.NewLine, search.Results.Select(r => r.Title)));

            // Act
            search = this.SearchForKeywordInModule("los");

            // Assert
            // we won't find "los" in the es-ES document.
            Assert.That(search.TotalHits, Is.EqualTo(0), "Found: " + string.Join(Environment.NewLine, search.Results.Select(r => r.Title)));
			Logger.LogDebug("<<< Exiting SearchController_Search_StopWords_Works()");
        }

        /// <summary>Executes function proc on a separate thread respecting the given timeout value.</summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="proc">The function to execute.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <returns>R.</returns>
        /// <remarks>From: http://stackoverflow.com/questions/9460661/implementing-regex-timeout-in-net-4.</remarks>
        private static R ExecuteWithTimeout<R>(Func<R> proc, TimeSpan timeout)
        {
			Logger.LogDebug(">>> Entering ExecuteWithTimeout(proc={Proc},timeout={Timeout})", proc, timeout);
            var r = default(R); // init default return value
            Exception ex = null; // records inter-thread exception
			Logger.LogDebug("ExecuteWithTimeout(proc={Proc},timeout={Timeout}): r → {R}", proc, timeout, r);
			Logger.LogDebug("ExecuteWithTimeout(proc={Proc},timeout={Timeout}): ex → {Ex}", proc, timeout, ex);

            // define a thread to wrap 'proc'
            var t = new Thread(() =>
            {
                try
                {
                    r = proc();
                }
                catch (Exception e)
                {
                    // this can get set to ThreadAbortException
                    ex = e;

                    Console.WriteLine("Exception hit");
                }
            });

            t.Start(); // start running 'proc' thread wrapper

            // from docs: "The Start method does not return until the new thread has started running."
            if (t.Join(timeout) == false)
            {
                t.Abort(); // die evil thread!

                // Abort raises the ThreadAbortException
                int i = 0;
                while ((t.Join(1) == false) && (i < 20))
                {
                    // 20 ms wait possible here
                    i++;
                }

                if (i >= 20)
                {
                    // we didn't abort, might want to log this or take some other action
                    // this can happen if you are doing something indefinitely hinky in a
                    // finally block (cause the finally be will executed before the Abort
                    // completes.
                    Console.WriteLine("Abort didn't work as expected");
                }
            }

            if (ex != null)
            {
                throw ex; // oops
            }

            return r; // ah!
        }

        private static string StripEllipses(string text)
        {
			Logger.LogDebug(">>> Entering StripEllipses(text={Text})", text);
			Logger.LogDebug("<<< Exiting StripEllipses(text={Text})", text);
            return text.Replace("...", string.Empty).Trim();
        }

        private void CreateNewLuceneControllerInstance(bool reCreate = false)
        {
			Logger.LogDebug(">>> Entering CreateNewLuceneControllerInstance(reCreate={ReCreate})", reCreate);
            this.internalSearchController = new InternalSearchControllerImpl(this.hostController, Mock.Of<IHostSettings>(), this.serviceProvider);
            InternalSearchController.SetTestableInstance(this.internalSearchController);
			Logger.LogDebug("CreateNewLuceneControllerInstance(reCreate={ReCreate}): internalSearchController → {InternalSearchController}", reCreate, internalSearchController);
            this.searchController = new SearchControllerImpl(this.serviceProvider);
			Logger.LogDebug("CreateNewLuceneControllerInstance(reCreate={ReCreate}): searchController → {SearchController}", reCreate, searchController);

            if (!reCreate)
            {
                this.DeleteIndexFolder();

                if (this.luceneController != null)
                {
                    LuceneController.ClearInstance();
                    this.luceneController.Dispose();
                }

                this.luceneController = new LuceneControllerImpl(this.hostController, new FakeApplicationStatusInfo());
				Logger.LogDebug("CreateNewLuceneControllerInstance(reCreate={ReCreate}): luceneController → {LuceneController}", reCreate, luceneController);
                LuceneController.SetTestableInstance(this.luceneController);
            }
			Logger.LogDebug("<<< Exiting CreateNewLuceneControllerInstance(reCreate={ReCreate})", reCreate);
        }

        private void SetupHostController()
        {
			Logger.LogDebug(">>> Entering SetupHostController()");
            this.hostController = new FakeHostController(new Dictionary<string, IConfigurationSetting>
            {
				Logger.LogDebug("SetupHostController(): hostController → {HostController}", hostController);
                { Constants.SearchIndexFolderKey, new ConfigurationSetting { Value = SearchIndexFolder + DateTime.UtcNow.Ticks, } },
                { Constants.SearchReaderRefreshTimeKey, new ConfigurationSetting { Value = this.readerStaleTimeSpan.ToString(CultureInfo.InvariantCulture), } },
                { Constants.SearchTitleBoostSetting, new ConfigurationSetting { Value = Constants.DefaultSearchTitleBoost.ToString(CultureInfo.InvariantCulture), } },
                { Constants.SearchTagBoostSetting, new ConfigurationSetting { Value = Constants.DefaultSearchTagBoost.ToString(CultureInfo.InvariantCulture), } },
                { Constants.SearchContentBoostSetting, new ConfigurationSetting { Value = Constants.DefaultSearchKeywordBoost.ToString(CultureInfo.InvariantCulture), } },
                { Constants.SearchDescriptionBoostSetting, new ConfigurationSetting { Value = Constants.DefaultSearchDescriptionBoost.ToString(CultureInfo.InvariantCulture), } },
                { Constants.SearchAuthorBoostSetting, new ConfigurationSetting { Value = Constants.DefaultSearchAuthorBoost.ToString(CultureInfo.InvariantCulture), } },
                { Constants.SearchMinLengthKey, new ConfigurationSetting { Value = Constants.DefaultMinLen.ToString(CultureInfo.InvariantCulture), } },
                { Constants.SearchMaxLengthKey, new ConfigurationSetting { Value = Constants.DefaultMaxLen.ToString(CultureInfo.InvariantCulture), } },
                { Constants.SearchRetryTimesKey, new ConfigurationSetting { Value = DefaultSearchRetryTimes.ToString(CultureInfo.InvariantCulture), } },
            });
			Logger.LogDebug("<<< Exiting SetupHostController()");
        }

        private void SetupLocaleController()
        {
			Logger.LogDebug(">>> Entering SetupLocaleController()");
            this.mockLocaleController.Setup(l => l.GetLocale(It.IsAny<string>())).Returns(new Locale { LanguageId = -1, Code = string.Empty, });
            this.mockLocaleController.Setup(l => l.GetLocale(CultureEnUs)).Returns(new Locale { LanguageId = LanguageIdEnUs, Code = CultureEnUs, });
            this.mockLocaleController.Setup(l => l.GetLocale(CultureEnCa)).Returns(new Locale { LanguageId = LanguageIdEnFr, Code = CultureEnCa, });
            this.mockLocaleController.Setup(l => l.GetLocale(CultureItIt)).Returns(new Locale { LanguageId = LanguageIdItIt, Code = CultureItIt, });
            this.mockLocaleController.Setup(l => l.GetLocale(CultureEsEs)).Returns(new Locale { LanguageId = LanguageIdEsEs, Code = CultureEsEs, });

            this.mockLocaleController.Setup(l => l.GetLocale(It.IsAny<int>())).Returns(new Locale { LanguageId = LanguageIdEnUs, Code = CultureEnUs, });
            this.mockLocaleController.Setup(l => l.GetLocale(LanguageIdEnUs)).Returns(new Locale { LanguageId = LanguageIdEnUs, Code = CultureEnUs, });
            this.mockLocaleController.Setup(l => l.GetLocale(LanguageIdEnFr)).Returns(new Locale { LanguageId = LanguageIdEnFr, Code = CultureEnCa, });
            this.mockLocaleController.Setup(l => l.GetLocale(LanguageIdItIt)).Returns(new Locale { LanguageId = LanguageIdItIt, Code = CultureItIt, });
            this.mockLocaleController.Setup(l => l.GetLocale(LanguageIdEsEs)).Returns(new Locale { LanguageId = LanguageIdEsEs, Code = CultureEsEs, });
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
                            "PortalID", "PortalGroupID", "PortalName", "LogoFile", "FooterText", "ExpiryDate", "UserRegistration", "BannerAdvertising", "AdministratorId", "Currency", "HostFee",
                            "HostSpace", "PageQuota", "UserQuota", "AdministratorRoleId", "RegisteredRoleId", "Description", "KeyWords", "BackgroundFile", "GUID", "PaymentProcessor", "ProcessorUserId",
                            "ProcessorPassword", "SiteLogHistory", "Email", "DefaultLanguage", "TimezoneOffset", "AdminTabId", "HomeDirectory", "SplashTabId", "HomeTabId", "LoginTabId", "RegisterTabId",
                            "UserTabId", "SearchTabId", "Custom404TabId", "Custom500TabId", "TermsTabId", "PrivacyTabId", "SuperTabId", "CreatedByUserID", "CreatedOnDate", "LastModifiedByUserID", "LastModifiedOnDate", "CultureCode",
                        };

            foreach (var col in cols)
            {
                table.Columns.Add(col);
            }

            const int homePage = 1;
            table.Rows.Add(portalId, null, "My Website", "Logo.png", "Copyright 2011 by DotNetNuke Corporation", null,
                    "2", "0", "2", "USD", "0", "0", "0", "0", "0", "1", "My Website", "DotNetNuke, DNN, Content, Management, CMS", null,
                    "1057AC7A-3C08-4849-A3A6-3D2AB4662020", null, null, null, "0", "admin@changeme.invalid", "en-US", "-8", "58", "Portals/0",
                    null, homePage.ToString("D"), null, null, "57", "56", "-1", "-1", null, null, "7", "-1", "2011-08-25 07:34:11", "-1", "2011-08-25 07:34:29", culture);

			Logger.LogDebug("<<< Exiting GetPortalCallBack(portalId={PortalId},culture={Culture})", portalId, culture);
            return table.CreateDataReader();
        }

        private void SetupSearchHelper()
        {
			Logger.LogDebug(">>> Entering SetupSearchHelper()");
            this.mockSearchHelper.Setup(c => c.GetSearchMinMaxLength()).Returns(new Tuple<int, int>(Constants.DefaultMinLen, Constants.DefaultMaxLen));
            this.mockSearchHelper.Setup(c => c.GetSynonyms(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns<int, string, string>(this.GetSynonymsCallBack);
            this.mockSearchHelper.Setup(x => x.GetSearchTypeByName(It.IsAny<string>())).Returns((string name) => new SearchType { SearchTypeId = 0, SearchTypeName = name });
            this.mockSearchHelper.Setup(x => x.GetSearchTypeByName(It.IsAny<string>())).Returns<string>(this.GetSearchTypeByNameCallback);
            this.mockSearchHelper.Setup(x => x.GetSearchTypes()).Returns(this.GetSearchTypes());
            this.mockSearchHelper.Setup(x => x.GetSearchStopWords(It.IsAny<int>(), It.IsAny<string>())).Returns(new SearchStopWords());
            this.mockSearchHelper.Setup(x => x.GetSearchStopWords(0, CultureEsEs)).Returns(
                new SearchStopWords
                {
                    PortalId = 0,
                    CultureCode = CultureEsEs,
                    StopWords = "los,de,el",
                });
            this.mockSearchHelper.Setup(x => x.GetSearchStopWords(0, CultureEnUs)).Returns(
                new SearchStopWords
                {
                    PortalId = 0,
                    CultureCode = CultureEnUs,
                    StopWords = "the,over",
                });
            this.mockSearchHelper.Setup(x => x.GetSearchStopWords(0, CultureEnCa)).Returns(
                new SearchStopWords
                {
                    PortalId = 0,
                    CultureCode = CultureEnCa,
                    StopWords = "the,over",
                });

            this.mockSearchHelper.Setup(x => x.RephraseSearchText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns<string, bool, bool>(new SearchHelperImpl(Mock.Of<IHostSettings>(), Mock.Of<IHostSettingsService>(), Mock.Of<IPortalController>(), new FakeApplicationStatusInfo()).RephraseSearchText);
            this.mockSearchHelper.Setup(x => x.StripTagsNoAttributes(It.IsAny<string>(), It.IsAny<bool>())).Returns((string html, bool retainSpace) => html);
            SearchHelper.SetTestableInstance(this.mockSearchHelper.Object);
			Logger.LogDebug("<<< Exiting SetupSearchHelper()");
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
            if (portalId == PortalId12 && userId == StandardAuthorId)
            {
                return new UserInfo { UserID = userId, DisplayName = StandardAuthorDisplayName, };
            }

			Logger.LogDebug("<<< Exiting GetUserByIdCallback(portalId={PortalId},userId={UserId})", portalId, userId);
            return null;
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
            dtSearchTypes.PrimaryKey = [pkId,];
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
                    new SearchType { SearchTypeId = ModuleSearchTypeId, SearchTypeName = ModuleSearchTypeName, SearchResultClass = FakeResultControllerClass, },
                    new SearchType { SearchTypeId = TabSearchTypeId, SearchTypeName = TabSearchTypeName, SearchResultClass = FakeResultControllerClass, },
                    new SearchType { SearchTypeId = OtherSearchTypeId, SearchTypeName = OtherSearchTypeName, SearchResultClass = FakeResultControllerClass, },
                    new SearchType { SearchTypeId = DocumentSearchTypeId, SearchTypeName = DocumentSearchTypeName, SearchResultClass = NoPermissionFakeResultControllerClass, },
                    new SearchType { SearchTypeId = UrlSearchTypeId, SearchTypeName = UrlSearchTypeName, SearchResultClass = FakeResultControllerClass, },
                };

			Logger.LogDebug("<<< Exiting GetSearchTypes()");
            return searchTypes;
        }

        private void DeleteIndexFolder()
        {
			Logger.LogDebug(">>> Entering DeleteIndexFolder()");
            try
            {
                var searchIndexFolder = this.hostController.GetString(Constants.SearchIndexFolderKey, SearchIndexFolder);
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

        /// <summary>Returns few SearchDocs.</summary>
        private IEnumerable<SearchDocument> GetStandardSearchDocs(int searchTypeId = ModuleSearchTypeId)
        {
			Logger.LogDebug(">>> Entering GetStandardSearchDocs(searchTypeId={SearchTypeId})", searchTypeId);
            var searchDocs = new List<SearchDocument>
            {
                new SearchDocument { PortalId = PortalId0, Tags = new List<string> { Tag0, Tag1, TagOldest, Tag0WithSpace, }, Title = Line1, },
                new SearchDocument { PortalId = PortalId0, Tags = new List<string> { Tag1, Tag2, TagNeutral, }, Title = Line2, CultureCode = CultureEnUs, },
                new SearchDocument { PortalId = PortalId0, Tags = new List<string> { Tag2, Tag3, TagIt, }, Title = Line3, CultureCode = CultureItIt, },
                new SearchDocument { PortalId = PortalId0, Tags = new List<string> { Tag3, Tag4, TagLatest, }, Title = Line4, CultureCode = CultureEnCa, },
                new SearchDocument { PortalId = PortalId0, Tags = new List<string> { Tag2, Tag3, TagIt, }, Title = Line5, CultureCode = CultureEsEs, },
            };

            var now = DateTime.UtcNow.AddYears(-searchDocs.Count);
            var i = 0;
			Logger.LogDebug("GetStandardSearchDocs(searchTypeId={SearchTypeId}): now → {Now}", searchTypeId, now);

            foreach (var searchDocument in searchDocs)
            {
                searchDocument.SearchTypeId = searchTypeId;
				Logger.LogDebug("GetStandardSearchDocs(searchTypeId={SearchTypeId}): searchDocument.SearchTypeId → {Arg1}", searchTypeId, searchDocument.SearchTypeId);
                searchDocument.UniqueKey = Guid.NewGuid().ToString();
                searchDocument.ModuleId = (searchTypeId == ModuleSearchTypeId) ? HtmlModuleId : -1;
                searchDocument.ModuleDefId = (searchTypeId == ModuleSearchTypeId) ? HtmlModuleDefId : -1;
                searchDocument.ModifiedTimeUtc = now.AddYears(++i); // last added is the newest
            }

			Logger.LogDebug("<<< Exiting GetStandardSearchDocs(searchTypeId={SearchTypeId})", searchTypeId);
            return searchDocs;
        }

        private IEnumerable<SearchDocument> GetSearchDocsForCustomBoost(int searchTypeId = ModuleSearchTypeId)
        {
			Logger.LogDebug(">>> Entering GetSearchDocsForCustomBoost(searchTypeId={SearchTypeId})", searchTypeId);
            var searchDocs = new List<SearchDocument>
            {
                new SearchDocument { PortalId = PortalId0, Title = Line1, Keywords = { { "title", "Hello" }, }, Body = "Hello1 World", },
                new SearchDocument { PortalId = PortalId0, Title = Line2, Keywords = { { "subject", "Hello" }, }, Body = "Hello2 World", },
                new SearchDocument { PortalId = PortalId0, Title = Line3, Keywords = { { "comments", "Hello" }, }, Body = "Hello3 World", },
                new SearchDocument { PortalId = PortalId0, Title = Line4, Keywords = { { "authorname", "Hello" }, }, Body = "Hello4 World", },
            };

            var now = DateTime.UtcNow.AddYears(-searchDocs.Count);
            var i = 0;
			Logger.LogDebug("GetSearchDocsForCustomBoost(searchTypeId={SearchTypeId}): now → {Now}", searchTypeId, now);

            foreach (var searchDocument in searchDocs)
            {
                searchDocument.SearchTypeId = searchTypeId;
				Logger.LogDebug("GetSearchDocsForCustomBoost(searchTypeId={SearchTypeId}): searchDocument.SearchTypeId → {Arg1}", searchTypeId, searchDocument.SearchTypeId);
                searchDocument.UniqueKey = Guid.NewGuid().ToString();
                searchDocument.ModuleId = (searchTypeId == ModuleSearchTypeId) ? HtmlModuleId : -1;
                searchDocument.ModuleDefId = (searchTypeId == ModuleSearchTypeId) ? HtmlModuleDefId : -1;
                searchDocument.ModifiedTimeUtc = now.AddYears(++i); // last added is the newest
            }

			Logger.LogDebug("<<< Exiting GetSearchDocsForCustomBoost(searchTypeId={SearchTypeId})", searchTypeId);
            return searchDocs;
        }

        /// <summary>Adds standard SearchDocs in Lucene Index.</summary>
        /// <returns>Number of documents added.</returns>
        private int AddStandardSearchDocs(int searchTypeId = ModuleSearchTypeId)
        {
			Logger.LogDebug(">>> Entering AddStandardSearchDocs(searchTypeId={SearchTypeId})", searchTypeId);
            var docs = this.GetStandardSearchDocs(searchTypeId).ToArray();
            this.internalSearchController.AddSearchDocuments(docs);
			Logger.LogDebug("AddStandardSearchDocs(searchTypeId={SearchTypeId}): docs → {Docs}", searchTypeId, docs);
			Logger.LogDebug("<<< Exiting AddStandardSearchDocs(searchTypeId={SearchTypeId})", searchTypeId);
            return docs.Length;
        }

        private int AddSearchDocsForCustomBoost(int searchTypeId = ModuleSearchTypeId)
        {
			Logger.LogDebug(">>> Entering AddSearchDocsForCustomBoost(searchTypeId={SearchTypeId})", searchTypeId);
            var docs = this.GetSearchDocsForCustomBoost(searchTypeId).ToArray();
            this.internalSearchController.AddSearchDocuments(docs);
			Logger.LogDebug("AddSearchDocsForCustomBoost(searchTypeId={SearchTypeId}): docs → {Docs}", searchTypeId, docs);
			Logger.LogDebug("<<< Exiting AddSearchDocsForCustomBoost(searchTypeId={SearchTypeId})", searchTypeId);
            return docs.Length;
        }

        private int AddDocumentsWithNumericKeys(int searchTypeId = OtherSearchTypeId)
        {
			Logger.LogDebug(">>> Entering AddDocumentsWithNumericKeys(searchTypeId={SearchTypeId})", searchTypeId);
            var doc1 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key1",
                Body = "hello",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                NumericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue50 }, },
            };
            var doc2 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key2",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                NumericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue100 }, },
            };
            var doc3 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key3",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                NumericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue200 }, },
            };
            var doc4 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key4",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                NumericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue500 }, },
            };
            var doc5 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key5",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                NumericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue1000 }, },
            };

            var docs = new List<SearchDocument>() { doc1, doc2, doc3, doc4, doc5, };

			Logger.LogDebug("AddDocumentsWithNumericKeys(searchTypeId={SearchTypeId}): docs → {Docs}", searchTypeId, docs);
            this.internalSearchController.AddSearchDocuments(docs);

			Logger.LogDebug("<<< Exiting AddDocumentsWithNumericKeys(searchTypeId={SearchTypeId})", searchTypeId);
            return docs.Count;
        }

        private int AddDocumentsWithKeywords(int searchTypeId = OtherSearchTypeId)
        {
			Logger.LogDebug(">>> Entering AddDocumentsWithKeywords(searchTypeId={SearchTypeId})", searchTypeId);
            var doc1 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key1",
                Body = "hello",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                Keywords = new Dictionary<string, string>() { { KeyWord1Name, KeyWord1Value }, },
            };
            var doc2 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key2",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                Keywords = new Dictionary<string, string>() { { KeyWord1Name, KeyWord2Value }, },
            };
            var doc3 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key3",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                Keywords = new Dictionary<string, string>() { { KeyWord1Name, KeyWord3Value }, },
            };
            var doc4 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key4",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                Keywords = new Dictionary<string, string>() { { KeyWord1Name, KeyWord4Value }, },
            };
            var doc5 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key5",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                Keywords = new Dictionary<string, string>() { { KeyWord1Name, KeyWord5Value }, },
            };

            var docs = new List<SearchDocument>() { doc1, doc2, doc3, doc4, doc5 };

			Logger.LogDebug("AddDocumentsWithKeywords(searchTypeId={SearchTypeId}): docs → {Docs}", searchTypeId, docs);
            this.internalSearchController.AddSearchDocuments(docs);

            return docs.Count;
        }

        private int AddDocuments(IList<string> titles, string body, int searchTypeId = OtherSearchTypeId)
        {
            var count = 0;
            foreach (var doc in titles.Select(title => new SearchDocument
            {
                Title = title,
                UniqueKey = Guid.NewGuid().ToString(),
                Body = body,
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
            }))
            {
                this.internalSearchController.AddSearchDocument(doc);
                count++;
            }

            return count;
        }

        private int AddDocumentsWithKeywords(IEnumerable<string> keywords, string title, int searchTypeId = OtherSearchTypeId)
        {
            var count = 0;
            foreach (var doc in keywords.Select(keyword => new SearchDocument
            {
                Title = title,
                UniqueKey = Guid.NewGuid().ToString(),
                Keywords = new Dictionary<string, string>() { { KeyWord1Name, keyword } },
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
            }))
            {
                this.internalSearchController.AddSearchDocument(doc);
                count++;
            }

            return count;
        }

        private void AddLinesAsSearchDocs(IList<string> lines, int searchTypeId = OtherSearchTypeId)
        {
            var now = DateTime.UtcNow - TimeSpan.FromSeconds(lines.Count());
            var i = 0;

            this.internalSearchController.AddSearchDocuments(
                lines.Select(line =>
                    new SearchDocument
                    {
                        Title = line,
                        UniqueKey = Guid.NewGuid().ToString(),
                        SearchTypeId = searchTypeId,
                        ModifiedTimeUtc = now.AddSeconds(i++),
                    }).ToList());
        }

        private SearchResults SearchForKeyword(string keyword, int searchTypeId = OtherSearchTypeId, bool useWildcard = false, bool allowLeadingWildcard = false)
        {
			Logger.LogDebug(">>> Entering SearchForKeyword(keyword={Keyword},searchTypeId={SearchTypeId},useWildcard={UseWildcard},allowLeadingWildcard={AllowLeadingWildcard})", keyword, searchTypeId, useWildcard, allowLeadingWildcard);
            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = [searchTypeId,], WildCardSearch = useWildcard, AllowLeadingWildcard = allowLeadingWildcard, };
			Logger.LogDebug("<<< Exiting SearchForKeyword(keyword={Keyword},searchTypeId={SearchTypeId},useWildcard={UseWildcard},allowLeadingWildcard={AllowLeadingWildcard})", keyword, searchTypeId, useWildcard, allowLeadingWildcard);
            return this.searchController.SiteSearch(query);
        }

        private SearchResults SearchForKeywordWithWildCard(string keyword, int searchTypeId = OtherSearchTypeId)
        {
			Logger.LogDebug(">>> Entering SearchForKeywordWithWildCard(keyword={Keyword},searchTypeId={SearchTypeId})", keyword, searchTypeId);
            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = [searchTypeId,], WildCardSearch = true, };
			Logger.LogDebug("<<< Exiting SearchForKeywordWithWildCard(keyword={Keyword},searchTypeId={SearchTypeId})", keyword, searchTypeId);
            return this.searchController.SiteSearch(query);
        }

        private SearchResults SearchForKeywordInModule(string keyword, int searchTypeId = ModuleSearchTypeId)
        {
			Logger.LogDebug(">>> Entering SearchForKeywordInModule(keyword={Keyword},searchTypeId={SearchTypeId})", keyword, searchTypeId);
            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = [searchTypeId,], };
			Logger.LogDebug("<<< Exiting SearchForKeywordInModule(keyword={Keyword},searchTypeId={SearchTypeId})", keyword, searchTypeId);
            return this.searchController.SiteSearch(query);
        }

        /// <summary>
        /// Sets up some data for testing security trimming.
        /// In the tests below, the users will have access to the following documents
        /// { 6, 7, 8, 9, 16, 17, 18, 19, 26, 27, 28, 29, ..., etc. }
        /// The tests check that pagination with various page sizes returns the proper groupings.
        /// </summary>
        private void SetupSecurityTrimmingDocs(int totalDocs, int searchType = DocumentSearchTypeId)
        {
			Logger.LogDebug(">>> Entering SetupSecurityTrimmingDocs(totalDocs={TotalDocs},searchType={SearchType})", totalDocs, searchType);
            var docModifyTime = DateTime.UtcNow - TimeSpan.FromSeconds(totalDocs);
            for (var i = 0; i < totalDocs; i++)
			Logger.LogDebug("SetupSecurityTrimmingDocs(totalDocs={TotalDocs},searchType={SearchType}): docModifyTime → {DocModifyTime}", totalDocs, searchType, docModifyTime);
            {
                this.internalSearchController.AddSearchDocument(new SearchDocument
                {
                    AuthorUserId = i,
                    Title = "Fox and Dog",
                    Body = Line1,
                    Tags = [Tag0, Tag1,],
                    SearchTypeId = searchType,
                    UniqueKey = Guid.NewGuid().ToString(),
                    ModifiedTimeUtc = docModifyTime.AddSeconds(i),
                });
            }
			Logger.LogDebug("<<< Exiting SetupSecurityTrimmingDocs(totalDocs={TotalDocs},searchType={SearchType})", totalDocs, searchType);
        }

        private void AddFoldersAndFiles()
        {
			Logger.LogDebug(">>> Entering AddFoldersAndFiles()");
            var allFiles = new Dictionary<string, string>
                               {
                                { "Awesome-Cycles-Logo.png", "Images/" },
                                { "Banner1.jpg", "Images/" },
                                { "Banner2.jpg", "Images/" },
                                { "bike-powered.png", "Images/DNN/" },
                                { "Spacer.gif", "Images/DNN/" },
                                { "monday.png", "My<Images/" },
                                { "tuesday.jpg", "My<Images/" },
                                { "wednesday.jpg", "My<Images/" },
                                { "thursday.png", "My<Images/My<DNN/" },
                                { "friday.gif", "My<Images/My<DNN/" },
                               };

            foreach (var file in allFiles)
            {
                var doc = new SearchDocument
                {
                    Title = file.Key,
                    UniqueKey = Guid.NewGuid().ToString(),
                    SearchTypeId = OtherSearchTypeId,
                    ModifiedTimeUtc = DateTime.UtcNow,
                    Keywords = new Dictionary<string, string> { { "folderName", file.Value.ToLowerInvariant() }, },
                };
                this.internalSearchController.AddSearchDocument(doc);
            }

            this.internalSearchController.Commit();
			Logger.LogDebug("<<< Exiting AddFoldersAndFiles()");
        }
    }
}
