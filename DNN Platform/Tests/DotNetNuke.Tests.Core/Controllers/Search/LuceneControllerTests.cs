// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Controllers.Search
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.QueryParsers;
    using Lucene.Net.Search;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using Directory = System.IO.Directory;

    /// <summary> Testing various aspects of LuceneController.</summary>
    [TestFixture]
    public class LuceneControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string WriteLockFile = "write.lock";
        private const string Line1 = "the quick brown fox jumps over the lazy dog";
        private const string Line2 = "the quick gold fox jumped over the lazy black dog";
        private const string Line3 = "the quick fox jumps over the black dog";
        private const string Line4 = "the red fox jumped over the lazy dark gray dog";
        private const string Line_Chinese = "这里是中文的内容";

        private const string SearchKeyword_Line1 = "fox";
        private const string SearchKeyword_Chinese = "中文";

        private const string EmptyCustomAnalyzer = "";
        private const string InvalidCustomAnalyzer = "Lucene.Net.Analysis.Cn.ChineseInvalidAnalyzer";
        private const string ValidCustomAnalyzer = "Lucene.Net.Analysis.Cn.ChineseAnalyzer, Lucene.Net.Contrib.Analyzers";
        private const int DefaultSearchRetryTimes = 5;

        // Arrange
        private const int TotalTestDocs2Create = 5;
        private const string ContentFieldName = "content";
        private readonly double readerStaleTimeSpan = TimeSpan.FromMilliseconds(100).TotalSeconds;

        private FakeHostController fakeHostController;
        private LuceneControllerImpl luceneController;
        private Mock<CachingProvider> cachingProvider;
        private Mock<ISearchHelper> mockSearchHelper;
        private Mock<SearchQuery> mockSearchQuery;
        private FakeServiceProvider serviceProvider;

        private string SearchIndexFolder => this.fakeHostController.GetString(Constants.SearchIndexFolderKey, string.Empty);

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            ComponentFactory.Container = new SimpleContainer();
            this.cachingProvider = MockComponentProvider.CreateDataCacheProvider();
			Logger.LogDebug("SetUp(): cachingProvider → {CachingProvider}", cachingProvider);

            this.MockHostController();

            this.mockSearchHelper = new Mock<ISearchHelper>();
			Logger.LogDebug("SetUp(): mockSearchHelper → {MockSearchHelper}", mockSearchHelper);
            this.mockSearchHelper.Setup(c => c.GetSynonyms(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns<int, string, string>(this.GetSynonymsCallBack);
            this.mockSearchHelper.Setup(c => c.GetSearchStopWords(It.IsAny<int>(), It.IsAny<string>())).Returns(new SearchStopWords());
            this.mockSearchHelper.Setup(c => c.GetSearchMinMaxLength()).Returns(new Tuple<int, int>(Constants.DefaultMinLen, Constants.DefaultMaxLen));
            this.mockSearchHelper.Setup(x => x.StripTagsNoAttributes(It.IsAny<string>(), It.IsAny<bool>())).Returns((string html, bool _) => html);
            SearchHelper.SetTestableInstance(this.mockSearchHelper.Object);

            this.mockSearchQuery = new Mock<SearchQuery>();
			Logger.LogDebug("SetUp(): mockSearchQuery → {MockSearchQuery}", mockSearchQuery);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("SetUp(): services → {Services}", services);
                    services.AddSingleton(this.cachingProvider.Object);
                    services.AddSingleton<IHostController>(this.fakeHostController);
                    services.AddSingleton<IHostSettingsService>(this.fakeHostController);
                    services.AddSingleton(this.mockSearchHelper.Object);
                    services.AddSingleton<IApplicationStatusInfo>(new FakeApplicationStatusInfo());
                });

            this.DeleteIndexFolder();
            this.CreateNewLuceneControllerInstance();
			Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            LuceneController.ClearInstance();
            this.luceneController.Dispose();
            this.DeleteIndexFolder();
            SearchHelper.ClearInstance();
            this.serviceProvider.Dispose();

            this.fakeHostController = null;
			Logger.LogDebug("TearDown(): fakeHostController → {FakeHostController}", fakeHostController);
			Logger.LogDebug("TearDown(): this.fakeHostController → {Arg0}", this.fakeHostController);
			Logger.LogDebug("TearDown(): cachingProvider → {CachingProvider}", cachingProvider);
			Logger.LogDebug("TearDown(): this.cachingProvider → {Arg0}", this.cachingProvider);
			Logger.LogDebug("TearDown(): mockSearchHelper → {MockSearchHelper}", mockSearchHelper);
            this.luceneController = null;
            this.cachingProvider = null;
			Logger.LogDebug("TearDown(): luceneController → {LuceneController}", luceneController);
			Logger.LogDebug("TearDown(): this.luceneController → {Arg0}", this.luceneController);
            this.mockSearchHelper = null;
            this.mockSearchQuery = null;
			Logger.LogDebug("<<< Exiting TearDown()");
			Logger.LogDebug("TearDown(): this.mockSearchHelper → {Arg0}", this.mockSearchHelper);
			Logger.LogDebug("TearDown(): this.mockSearchQuery → {Arg0}", this.mockSearchQuery);
			Logger.LogDebug("TearDown(): mockSearchQuery → {MockSearchQuery}", mockSearchQuery);
        }

        private void MockHostController()
        {
			Logger.LogDebug(">>> Entering MockHostController()");
            this.fakeHostController = new FakeHostController(
                new Dictionary<string, string>
                {
                    { Constants.SearchIndexFolderKey, $@"App_Data\LuceneTests{DateTime.UtcNow.Ticks}" },
                    { Constants.SearchReaderRefreshTimeKey, this.readerStaleTimeSpan.ToString(CultureInfo.InvariantCulture) },
                    { Constants.SearchMinLengthKey, Constants.DefaultMinLen.ToString(CultureInfo.InvariantCulture) },
                    { Constants.SearchMaxLengthKey, Constants.DefaultMaxLen.ToString(CultureInfo.InvariantCulture) },
                    { Constants.SearchRetryTimesKey, DefaultSearchRetryTimes.ToString(CultureInfo.InvariantCulture) },
                });
					Logger.LogDebug("MockHostController(): fakeHostController → {FakeHostController}", fakeHostController);
        }

        [Test]
        public void LuceneController_SearchFolderIsAsExpected()
        {
			Logger.LogDebug(">>> Entering LuceneController_SearchFolderIsAsExpected()");
            var searchIndexFolder = this.fakeHostController.GetString(Constants.SearchIndexFolderKey, this.SearchIndexFolder);
            var inf1 = new DirectoryInfo(searchIndexFolder);
			Logger.LogDebug("LuceneController_SearchFolderIsAsExpected(): searchIndexFolder → {SearchIndexFolder}", searchIndexFolder);
			Logger.LogDebug("LuceneController_SearchFolderIsAsExpected(): inf1 → {Inf1}", inf1);
            var inf2 = new DirectoryInfo(this.luceneController.IndexFolder);
            Assert.That(inf2.Name, Is.EqualTo(inf1.Name));
			Logger.LogDebug("<<< Exiting LuceneController_SearchFolderIsAsExpected()");
			Logger.LogDebug("LuceneController_SearchFolderIsAsExpected(): inf2 → {Inf2}", inf2);
        }

        [Test]
        public void LuceneController_Add_Throws_On_Null_Document()
        {
			Logger.LogDebug(">>> Entering LuceneController_Add_Throws_On_Null_Document()");
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => this.luceneController.Add(null));
			Logger.LogDebug("<<< Exiting LuceneController_Add_Throws_On_Null_Document()");
        }

        public void LuceneController_Add_Throws_On_Null_Query()
        {
			Logger.LogDebug(">>> Entering LuceneController_Add_Throws_On_Null_Query()");
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => this.luceneController.Delete(null));
			Logger.LogDebug("<<< Exiting LuceneController_Add_Throws_On_Null_Query()");
        }

        [Test]
        public void LuceneController_Add_Empty_FiledsCollection_DoesNot_Create_Index()
        {
			Logger.LogDebug(">>> Entering LuceneController_Add_Empty_FiledsCollection_DoesNot_Create_Index()");
            // Arrange

            // Act
            this.luceneController.Add(new Document());
            this.luceneController.Commit();

            var numFiles = 0;
            this.DeleteIndexFolder();

            Assert.That(numFiles, Is.EqualTo(0));
			Logger.LogDebug("<<< Exiting LuceneController_Add_Empty_FiledsCollection_DoesNot_Create_Index()");
        }

        [Test]
        public void LuceneController_GetsHighlightedDesc()
        {
			Logger.LogDebug(">>> Entering LuceneController_GetsHighlightedDesc()");
            // Arrange
            const string fieldName = "content";
            const string fieldValue = Line1;

            // Act
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
			Logger.LogDebug("LuceneController_GetsHighlightedDesc(): field → {Field}", field);
			Logger.LogDebug("LuceneController_GetsHighlightedDesc(): doc → {Doc}", doc);
            doc.Add(field);

            this.luceneController.Add(doc);
            this.luceneController.Commit();

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(fieldName, "fox")) }));

			Logger.LogDebug("LuceneController_GetsHighlightedDesc(): hits → {Hits}", hits);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(hits.Results, Has.One.Items);
                Assert.That(hits.Results.ElementAt(0).ContentSnippet, Is.EqualTo("brown <b>fox</b> jumps over the lazy dog"));
            }
			Logger.LogDebug("<<< Exiting LuceneController_GetsHighlightedDesc()");
        }

        [Test]
        public void LuceneController_HighlightedDescHtmlEncodesOutput()
        {
			Logger.LogDebug(">>> Entering LuceneController_HighlightedDescHtmlEncodesOutput()");
            // Arrange
            const string fieldName = "content";
            const string fieldValue = "<script src='fox' type='text/javascript'></script>";
            const string expectedResult = " src=&#39;<b>fox</b>&#39; type=&#39;text/javascript&#39;&gt;&lt;/script&gt;";

            // Note that we mustn't get " src='<b>fox</b>' type='text/javascript'></script>" as this causes browser rendering issues

            // Act
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
			Logger.LogDebug("LuceneController_HighlightedDescHtmlEncodesOutput(): field → {Field}", field);
			Logger.LogDebug("LuceneController_HighlightedDescHtmlEncodesOutput(): doc → {Doc}", doc);
            doc.Add(field);

            this.luceneController.Add(doc);
            this.luceneController.Commit();

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(fieldName, "fox")) }));

			Logger.LogDebug("LuceneController_HighlightedDescHtmlEncodesOutput(): hits → {Hits}", hits);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(hits.Results, Has.One.Items);
                Assert.That(hits.Results.ElementAt(0).ContentSnippet, Is.EqualTo(expectedResult));
            }
			Logger.LogDebug("<<< Exiting LuceneController_HighlightedDescHtmlEncodesOutput()");
        }

        [Test]
        public void LuceneController_FindsResultsUsingNearRealtimeSearchWithoutCommit()
        {
			Logger.LogDebug(">>> Entering LuceneController_FindsResultsUsingNearRealtimeSearchWithoutCommit()");
            // Arrange
            const string fieldName = "content";
            const string fieldValue = Line1;

            // Act
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
			Logger.LogDebug("LuceneController_FindsResultsUsingNearRealtimeSearchWithoutCommit(): field → {Field}", field);
			Logger.LogDebug("LuceneController_FindsResultsUsingNearRealtimeSearchWithoutCommit(): doc → {Doc}", doc);
            doc.Add(field);
            this.luceneController.Add(doc);

            // DONOT commit here to enable testing near-realtime of search writer
            // _luceneController.Commit();
            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(fieldName, "fox")) }));

			Logger.LogDebug("LuceneController_FindsResultsUsingNearRealtimeSearchWithoutCommit(): hits → {Hits}", hits);
            // Assert
            Assert.That(hits.Results, Has.One.Items);
			Logger.LogDebug("<<< Exiting LuceneController_FindsResultsUsingNearRealtimeSearchWithoutCommit()");
        }

        [Test]
        public void LuceneController_Search_Returns_Correct_Total_Hits()
        {
			Logger.LogDebug(">>> Entering LuceneController_Search_Returns_Correct_Total_Hits()");
            // Arrange
            this.AddStandardDocs();

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(Constants.ContentTag, "fox")) }));

			Logger.LogDebug("LuceneController_Search_Returns_Correct_Total_Hits(): hits → {Hits}", hits);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(hits.TotalHits, Is.EqualTo(4));
                Assert.That(hits.Results.Count(), Is.EqualTo(4));
            }
			Logger.LogDebug("<<< Exiting LuceneController_Search_Returns_Correct_Total_Hits()");
        }

        [Test]
        public void LuceneController_Search_Request_For_1_Result_Returns_1_Record_But_More_TotalHits()
        {
			Logger.LogDebug(">>> Entering LuceneController_Search_Request_For_1_Result_Returns_1_Record_But_More_TotalHits()");
            // Arrange
            this.AddStandardDocs();

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(Constants.ContentTag, "fox")), PageIndex = 1, PageSize = 1 }));

			Logger.LogDebug("LuceneController_Search_Request_For_1_Result_Returns_1_Record_But_More_TotalHits(): hits → {Hits}", hits);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(hits.TotalHits, Is.EqualTo(4));
                Assert.That(hits.Results, Has.One.Items);
            }
			Logger.LogDebug("<<< Exiting LuceneController_Search_Request_For_1_Result_Returns_1_Record_But_More_TotalHits()");
        }

        [Test]
        public void LuceneController_Search_Request_For_4_Records_Returns_4_Records_With_4_TotalHits_Based_On_PageIndex1_PageSize4()
        {
			Logger.LogDebug(">>> Entering LuceneController_Search_Request_For_4_Records_Returns_4_Records_With_4_TotalHits_Based_On_PageIndex1_PageSize4()");
            // Arrange
            this.AddStandardDocs();

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(Constants.ContentTag, "fox")), PageIndex = 1, PageSize = 4 }));

			Logger.LogDebug("LuceneController_Search_Request_For_4_Records_Returns_4_Records_With_4_TotalHits_Based_On_PageIndex1_PageSize4(): hits → {Hits}", hits);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(hits.TotalHits, Is.EqualTo(4));
                Assert.That(hits.Results.Count(), Is.EqualTo(4));
            }
			Logger.LogDebug("<<< Exiting LuceneController_Search_Request_For_4_Records_Returns_4_Records_With_4_TotalHits_Based_On_PageIndex1_PageSize4()");
        }

        [Test]
        public void LuceneController_Search_Request_For_4_Records_Returns_4_Records_With_4_TotalHits_Based_On_PageIndex4_PageSize1()
        {
			Logger.LogDebug(">>> Entering LuceneController_Search_Request_For_4_Records_Returns_4_Records_With_4_TotalHits_Based_On_PageIndex4_PageSize1()");
            // Arrange
            this.AddStandardDocs();

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(Constants.ContentTag, "fox")), PageIndex = 1, PageSize = 4 }));

			Logger.LogDebug("LuceneController_Search_Request_For_4_Records_Returns_4_Records_With_4_TotalHits_Based_On_PageIndex4_PageSize1(): hits → {Hits}", hits);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(hits.TotalHits, Is.EqualTo(4));
                Assert.That(hits.Results.Count(), Is.EqualTo(4));
            }
			Logger.LogDebug("<<< Exiting LuceneController_Search_Request_For_4_Records_Returns_4_Records_With_4_TotalHits_Based_On_PageIndex4_PageSize1()");
        }

        [Test]
        public void LuceneController_Search_Request_For_NonExisting_PageNumbers_Returns_No_Record()
        {
			Logger.LogDebug(">>> Entering LuceneController_Search_Request_For_NonExisting_PageNumbers_Returns_No_Record()");
            // Arrange
            this.AddStandardDocs();

            var hits = this.luceneController.Search(this.CreateSearchContext(
                new LuceneQuery
                {
                    Query = new TermQuery(new Term(Constants.ContentTag, "fox")),
                    PageIndex = 5,
                    PageSize = 10,
                }));

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(hits.TotalHits, Is.EqualTo(4));
                Assert.That(hits.Results.Count(), Is.EqualTo(0));
            }
			Logger.LogDebug("<<< Exiting LuceneController_Search_Request_For_NonExisting_PageNumbers_Returns_No_Record()");
        }

        [Test]
        public void LuceneController_Search_Request_For_PagIndex2_PageSize1_Returns_2nd_Record_Only()
        {
			Logger.LogDebug(">>> Entering LuceneController_Search_Request_For_PagIndex2_PageSize1_Returns_2nd_Record_Only()");
            // Arrange
            this.AddStandardDocs();

            var query = new LuceneQuery
            {
                Query = new TermQuery(new Term(Constants.ContentTag, "quick")),
                PageIndex = 2,
                PageSize = 1,
            };

            var hits = this.luceneController.Search(this.CreateSearchContext(query));

			Logger.LogDebug("LuceneController_Search_Request_For_PagIndex2_PageSize1_Returns_2nd_Record_Only(): hits → {Hits}", hits);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(hits.TotalHits, Is.EqualTo(3));
                Assert.That(hits.Results, Has.One.Items);

                // for some reason, this search's docs have scoring as
                // Line1=0.3125, Line1=0.3125, Line2=0.3125, Line2=0.3750
                Assert.That(hits.Results.ElementAt(0).Document.GetField(Constants.ContentTag).StringValue, Is.EqualTo(Line1));
            }
			Logger.LogDebug("<<< Exiting LuceneController_Search_Request_For_PagIndex2_PageSize1_Returns_2nd_Record_Only()");
        }

        [Test]
        public void LuceneController_NumericRangeCheck()
        {
			Logger.LogDebug(">>> Entering LuceneController_NumericRangeCheck()");
            // Arrange
            const string fieldName = "content";

            // Act

            // Add first numeric field
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
			Logger.LogDebug("LuceneController_NumericRangeCheck(): doc1 → {Doc1}", doc1);
            this.luceneController.Add(doc1);

            // Add second numeric field
            var doc2 = new Document();
            doc2.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(2));
			Logger.LogDebug("LuceneController_NumericRangeCheck(): doc2 → {Doc2}", doc2);
            this.luceneController.Add(doc2);

            // Add third numeric field
            var doc3 = new Document();
            doc3.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(3));
			Logger.LogDebug("LuceneController_NumericRangeCheck(): doc3 → {Doc3}", doc3);
            this.luceneController.Add(doc3);

            // Add fourth numeric field
            var doc4 = new Document();
            doc4.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(4));
			Logger.LogDebug("LuceneController_NumericRangeCheck(): doc4 → {Doc4}", doc4);
            this.luceneController.Add(doc4);

            this.luceneController.Commit();

            var query = NumericRangeQuery.NewIntRange(fieldName, 2, 3, true, true);
            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = query }));
			Logger.LogDebug("LuceneController_NumericRangeCheck(): query → {Query}", query);
			Logger.LogDebug("LuceneController_NumericRangeCheck(): hits → {Hits}", hits);
            Assert.That(hits.Results.Count(), Is.EqualTo(2));
			Logger.LogDebug("<<< Exiting LuceneController_NumericRangeCheck()");
        }

        [Test]
        public void LuceneController_DateRangeCheck()
        {
			Logger.LogDebug(">>> Entering LuceneController_DateRangeCheck()");
            // Arrange
            const string fieldName = "content";
            var dates = new List<DateTime> { DateTime.Now.AddYears(-3), DateTime.Now.AddYears(-2), DateTime.Now.AddYears(-1), DateTime.Now };
			Logger.LogDebug("LuceneController_DateRangeCheck(): dates → {Dates}", dates);

            // Act
            foreach (var date in dates)
            {
                var doc = new Document();
                doc.Add(new NumericField(fieldName, Field.Store.YES, true).SetLongValue(long.Parse(date.ToString(Constants.DateTimeFormat))));
				Logger.LogDebug("LuceneController_DateRangeCheck(): doc → {Doc}", doc);
                this.luceneController.Add(doc);
            }

            this.luceneController.Commit();

            var futureTime = DateTime.Now.AddMinutes(1).ToString(Constants.DateTimeFormat);
            var query = NumericRangeQuery.NewLongRange(fieldName, long.Parse(futureTime), long.Parse(futureTime), true, true);
			Logger.LogDebug("LuceneController_DateRangeCheck(): futureTime → {FutureTime}", futureTime);
			Logger.LogDebug("LuceneController_DateRangeCheck(): query → {Query}", query);

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = query, }));
            Assert.That(hits.Results, Is.Empty);
			Logger.LogDebug("LuceneController_DateRangeCheck(): hits → {Hits}", hits);

            query = NumericRangeQuery.NewLongRange(fieldName, long.Parse(DateTime.Now.AddDays(-1).ToString(Constants.DateTimeFormat)), long.Parse(DateTime.Now.ToString(Constants.DateTimeFormat)), true, true);
            hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = query, }));
            Assert.That(hits.Results, Has.One.Items);

            query = NumericRangeQuery.NewLongRange(fieldName, long.Parse(DateTime.Now.AddDays(-368).ToString(Constants.DateTimeFormat)), long.Parse(DateTime.Now.ToString(Constants.DateTimeFormat)), true, true);
            hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = query, }));
            Assert.That(hits.Results.ToList(), Has.Count.EqualTo(2));
			Logger.LogDebug("<<< Exiting LuceneController_DateRangeCheck()");
        }

        [Test]
        public void LuceneController_Search_Throws_On_Null_LuceneQuery()
        {
			Logger.LogDebug(">>> Entering LuceneController_Search_Throws_On_Null_LuceneQuery()");
            Assert.Throws<ArgumentNullException>(() => this.luceneController.Search(this.CreateSearchContext(null)));
			Logger.LogDebug("<<< Exiting LuceneController_Search_Throws_On_Null_LuceneQuery()");
        }

        [Test]
        public void LuceneController_Search_Throws_On_Null_Query()
        {
			Logger.LogDebug(">>> Entering LuceneController_Search_Throws_On_Null_Query()");
            Assert.Throws<ArgumentNullException>(() => this.luceneController.Search(this.CreateSearchContext(new LuceneQuery())));
			Logger.LogDebug("<<< Exiting LuceneController_Search_Throws_On_Null_Query()");
        }

        [Test]
        public void LuceneController_Search_Throws_On_Zero_PageSize()
        {
			Logger.LogDebug(">>> Entering LuceneController_Search_Throws_On_Zero_PageSize()");
            Assert.Throws<ArgumentException>(() => this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new BooleanQuery(), PageSize = 0 })));
			Logger.LogDebug("<<< Exiting LuceneController_Search_Throws_On_Zero_PageSize()");
        }

        [Test]
        public void LuceneController_Search_Throws_On_Zero_PageIndex()
        {
			Logger.LogDebug(">>> Entering LuceneController_Search_Throws_On_Zero_PageIndex()");
            Assert.Throws<ArgumentException>(() => this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new BooleanQuery(), PageIndex = 0 })));
			Logger.LogDebug("<<< Exiting LuceneController_Search_Throws_On_Zero_PageIndex()");
        }

        [Test]
        [TestCase(EmptyCustomAnalyzer)]
        [TestCase(InvalidCustomAnalyzer)]
        [TestCase(ValidCustomAnalyzer)]
        public void LuceneController_Search_With_Chinese_Chars_And_Custom_Analyzer(string customAnalyzer = "")
        {
			Logger.LogDebug(">>> Entering LuceneController_Search_With_Chinese_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer})", customAnalyzer);
            this.fakeHostController.AddSetting(Constants.SearchCustomAnalyzer, customAnalyzer);

            // Arrange
            const string fieldName = "content";
            const string fieldValue = Line_Chinese;

            // Act
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
			Logger.LogDebug("LuceneController_Search_With_Chinese_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer}): field → {Field}", customAnalyzer, field);
			Logger.LogDebug("LuceneController_Search_With_Chinese_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer}): doc → {Doc}", customAnalyzer, doc);
            doc.Add(field);

            this.luceneController.Add(doc);
            this.luceneController.Commit();

            var analyzer = this.luceneController.GetCustomAnalyzer() ?? new SearchQueryAnalyzer(true);
            var keywordQuery = new BooleanQuery();
			Logger.LogDebug("LuceneController_Search_With_Chinese_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer}): analyzer → {Analyzer}", customAnalyzer, analyzer);
			Logger.LogDebug("LuceneController_Search_With_Chinese_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer}): keywordQuery → {KeywordQuery}", customAnalyzer, keywordQuery);
            var parserContent = new QueryParser(Constants.LuceneVersion, fieldName, analyzer);
            var parsedQueryContent = parserContent.Parse(SearchKeyword_Chinese);
			Logger.LogDebug("LuceneController_Search_With_Chinese_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer}): parserContent → {ParserContent}", customAnalyzer, parserContent);
			Logger.LogDebug("LuceneController_Search_With_Chinese_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer}): parsedQueryContent → {ParsedQueryContent}", customAnalyzer, parsedQueryContent);
            keywordQuery.Add(parsedQueryContent, Occur.SHOULD);

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = keywordQuery }));

			Logger.LogDebug("LuceneController_Search_With_Chinese_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer}): hits → {Hits}", customAnalyzer, hits);
            // Assert
            if (customAnalyzer == ValidCustomAnalyzer)
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(hits.Results, Has.One.Items);
                    Assert.That(hits.Results.ElementAt(0).ContentSnippet, Is.EqualTo(Line_Chinese.Replace(SearchKeyword_Chinese, string.Format("<b>{0}</b>", SearchKeyword_Chinese))));
                }
            }
            else
            {
                Assert.That(hits.Results.Count(), Is.EqualTo(0));
            }
			Logger.LogDebug("<<< Exiting LuceneController_Search_With_Chinese_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer})", customAnalyzer);
        }

        [Test]
        [TestCase(EmptyCustomAnalyzer)]
        [TestCase(InvalidCustomAnalyzer)]
        [TestCase(ValidCustomAnalyzer)]
        public void LuceneController_Search_With_English_Chars_And_Custom_Analyzer(string customAnalyzer = "")
        {
			Logger.LogDebug(">>> Entering LuceneController_Search_With_English_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer})", customAnalyzer);
            this.fakeHostController.AddSetting(Constants.SearchCustomAnalyzer, customAnalyzer);

            // Arrange
            const string fieldName = "content";
            const string fieldValue = Line1;

            // Act
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
			Logger.LogDebug("LuceneController_Search_With_English_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer}): field → {Field}", customAnalyzer, field);
			Logger.LogDebug("LuceneController_Search_With_English_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer}): doc → {Doc}", customAnalyzer, doc);
            doc.Add(field);

            this.luceneController.Add(doc);
            this.luceneController.Commit();

            var analyzer = this.luceneController.GetCustomAnalyzer() ?? new SearchQueryAnalyzer(true);
            var keywordQuery = new BooleanQuery();
			Logger.LogDebug("LuceneController_Search_With_English_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer}): analyzer → {Analyzer}", customAnalyzer, analyzer);
			Logger.LogDebug("LuceneController_Search_With_English_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer}): keywordQuery → {KeywordQuery}", customAnalyzer, keywordQuery);
            var parserContent = new QueryParser(Constants.LuceneVersion, fieldName, analyzer);
            var parsedQueryContent = parserContent.Parse(SearchKeyword_Line1);
			Logger.LogDebug("LuceneController_Search_With_English_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer}): parserContent → {ParserContent}", customAnalyzer, parserContent);
			Logger.LogDebug("LuceneController_Search_With_English_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer}): parsedQueryContent → {ParsedQueryContent}", customAnalyzer, parsedQueryContent);
            keywordQuery.Add(parsedQueryContent, Occur.SHOULD);

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = keywordQuery }));

			Logger.LogDebug("LuceneController_Search_With_English_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer}): hits → {Hits}", customAnalyzer, hits);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(hits.Results, Has.One.Items);
                Assert.That(hits.Results.ElementAt(0).ContentSnippet, Is.EqualTo("brown <b>fox</b> jumps over the lazy dog"));
            }
			Logger.LogDebug("<<< Exiting LuceneController_Search_With_English_Chars_And_Custom_Analyzer(customAnalyzer={CustomAnalyzer})", customAnalyzer);
        }

        [Test]
        public void LuceneController_Search_Single_FuzzyQuery()
        {
			Logger.LogDebug(">>> Entering LuceneController_Search_Single_FuzzyQuery()");
            // Arrange
            string[] docs =
            {
                "fuzzy",
                "wuzzy",
                };
            const string keyword = "wuzza";

            this.AddLinesAsSearchDocs(docs);

            // Act
            var luceneQuery = new LuceneQuery { Query = new FuzzyQuery(new Term(Constants.ContentTag, keyword)) };
            var previews = this.luceneController.Search(this.CreateSearchContext(luceneQuery));
			Logger.LogDebug("LuceneController_Search_Single_FuzzyQuery(): luceneQuery → {LuceneQuery}", luceneQuery);
			Logger.LogDebug("LuceneController_Search_Single_FuzzyQuery(): previews → {Previews}", previews);

            // Assert
            Assert.That(previews.Results.Count(), Is.EqualTo(2));
			Logger.LogDebug("<<< Exiting LuceneController_Search_Single_FuzzyQuery()");
        }

        [Test]
        public void LuceneController_Search_Double_FuzzyQuery()
        {
			Logger.LogDebug(">>> Entering LuceneController_Search_Double_FuzzyQuery()");
            // Arrange
            string[] docs =
            {
                "home",
                "homez", // note home and homes could be returned by PorterFilter
                "fuzzy",
                "wuzzy",
                };

            string[] keywords =
            {
                "wuzza",
                "homy",
                };

            this.AddLinesAsSearchDocs(docs);

            // Act
            var finalQuery = new BooleanQuery();
            foreach (var keyword in keywords)
            {
			Logger.LogDebug("LuceneController_Search_Double_FuzzyQuery(): finalQuery → {FinalQuery}", finalQuery);
                finalQuery.Add(new FuzzyQuery(new Term(Constants.ContentTag, keyword)), Occur.SHOULD);
            }

            var luceneQuery = new LuceneQuery { Query = finalQuery };
            var previews = this.luceneController.Search(this.CreateSearchContext(luceneQuery));

			Logger.LogDebug("LuceneController_Search_Double_FuzzyQuery(): previews → {Previews}", previews);
            // Assert
            Assert.That(previews.Results.Count(), Is.EqualTo(3));
			Logger.LogDebug("<<< Exiting LuceneController_Search_Double_FuzzyQuery()");
        }

        [Test]
        public void LuceneController_Throws_SearchIndexEmptyException_WhenNoDataInSearch()
        {
			Logger.LogDebug(">>> Entering LuceneController_Throws_SearchIndexEmptyException_WhenNoDataInSearch()");
            Assert.Throws<SearchIndexEmptyException>(() => { this.luceneController.GetSearcher(); });
			Logger.LogDebug("<<< Exiting LuceneController_Throws_SearchIndexEmptyException_WhenNoDataInSearch()");
        }

        [Test]
        public void LuceneController_ReaderNotChangedBeforeTimeSpanElapsed()
        {
			Logger.LogDebug(">>> Entering LuceneController_ReaderNotChangedBeforeTimeSpanElapsed()");
            // Arrange
            const string fieldName = "content";

            // Act

            // Add first numeric field
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
			Logger.LogDebug("LuceneController_ReaderNotChangedBeforeTimeSpanElapsed(): doc1 → {Doc1}", doc1);
            this.luceneController.Add(doc1);
            this.luceneController.Commit();

            var reader = this.luceneController.GetSearcher();
            Thread.Sleep(TimeSpan.FromSeconds(this.readerStaleTimeSpan / 2));
			Logger.LogDebug("LuceneController_ReaderNotChangedBeforeTimeSpanElapsed(): reader → {Reader}", reader);

            Assert.That(this.luceneController.GetSearcher(), Is.SameAs(reader));
			Logger.LogDebug("<<< Exiting LuceneController_ReaderNotChangedBeforeTimeSpanElapsed()");
        }

        [Test]
        public void LuceneController_ReaderNotChangedIfNoIndexUpdated()
        {
			Logger.LogDebug(">>> Entering LuceneController_ReaderNotChangedIfNoIndexUpdated()");
            // Arrange
            const string fieldName = "content";

            // Act

            // Add first numeric field
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
			Logger.LogDebug("LuceneController_ReaderNotChangedIfNoIndexUpdated(): doc1 → {Doc1}", doc1);
            this.luceneController.Add(doc1);
            this.luceneController.Commit();

            var reader = this.luceneController.GetSearcher();
            Thread.Sleep(TimeSpan.FromSeconds(this.readerStaleTimeSpan * 1.1));
			Logger.LogDebug("LuceneController_ReaderNotChangedIfNoIndexUpdated(): reader → {Reader}", reader);

            Assert.That(this.luceneController.GetSearcher(), Is.SameAs(reader));
			Logger.LogDebug("<<< Exiting LuceneController_ReaderNotChangedIfNoIndexUpdated()");
        }

        [Test]
        public void LuceneController_ReaderIsChangedWhenIndexIsUpdatedAndTimeIsElapsed()
        {
			Logger.LogDebug(">>> Entering LuceneController_ReaderIsChangedWhenIndexIsUpdatedAndTimeIsElapsed()");
            // Arrange
            const string fieldName = "content";

            // Act

            // Add first numeric field
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
			Logger.LogDebug("LuceneController_ReaderIsChangedWhenIndexIsUpdatedAndTimeIsElapsed(): doc1 → {Doc1}", doc1);
            this.luceneController.Add(doc1);
            this.luceneController.Commit();

            var reader = this.luceneController.GetSearcher();
            Thread.Sleep(TimeSpan.FromSeconds(this.readerStaleTimeSpan * 1.1));
			Logger.LogDebug("LuceneController_ReaderIsChangedWhenIndexIsUpdatedAndTimeIsElapsed(): reader → {Reader}", reader);

            // Add second numeric field
            var doc2 = new Document();
            doc2.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(2));
			Logger.LogDebug("LuceneController_ReaderIsChangedWhenIndexIsUpdatedAndTimeIsElapsed(): doc2 → {Doc2}", doc2);
            this.luceneController.Add(doc2);

            // var lastAcccess = Directory.GetLastWriteTime(_luceneController.IndexFolder);
            // Directory.SetLastWriteTime(_luceneController.IndexFolder, lastAcccess + TimeSpan.FromSeconds(1));
            Assert.That(this.luceneController.GetSearcher(), Is.Not.SameAs(reader));
			Logger.LogDebug("<<< Exiting LuceneController_ReaderIsChangedWhenIndexIsUpdatedAndTimeIsElapsed()");
        }

        [Test]
        public void LuceneController_LockFileWhenExistsDoesNotCauseProblemForFirstIController()
        {
			Logger.LogDebug(">>> Entering LuceneController_LockFileWhenExistsDoesNotCauseProblemForFirstIController()");
            // Arrange
            const string fieldName = "content";
            var searchIndexFolder = this.fakeHostController.GetString(Constants.SearchIndexFolderKey, this.SearchIndexFolder);
			Logger.LogDebug("LuceneController_LockFileWhenExistsDoesNotCauseProblemForFirstIController(): searchIndexFolder → {SearchIndexFolder}", searchIndexFolder);
            var lockFile = Path.Combine(searchIndexFolder, WriteLockFile);
            if (!Directory.Exists(searchIndexFolder))
			Logger.LogDebug("LuceneController_LockFileWhenExistsDoesNotCauseProblemForFirstIController(): lockFile → {LockFile}", lockFile);
            {
                Directory.CreateDirectory(searchIndexFolder);
            }

            if (!File.Exists(lockFile))
            {
                File.Create(lockFile).Close();
            }

            // Act
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
			Logger.LogDebug("LuceneController_LockFileWhenExistsDoesNotCauseProblemForFirstIController(): doc1 → {Doc1}", doc1);

            // Assert
            Assert.That(lockFile, Does.Exist);
            Assert.DoesNotThrow(() => this.luceneController.Add(doc1));
			Logger.LogDebug("<<< Exiting LuceneController_LockFileWhenExistsDoesNotCauseProblemForFirstIController()");
        }

        [Test]
        public void LuceneController_LockFileCanBeObtainedByOnlySingleController()
        {
			Logger.LogDebug(">>> Entering LuceneController_LockFileCanBeObtainedByOnlySingleController()");
            // Arrange
            const string fieldName = "content";
            var lockFile = Path.Combine(this.luceneController.IndexFolder, WriteLockFile);
			Logger.LogDebug("LuceneController_LockFileCanBeObtainedByOnlySingleController(): lockFile → {LockFile}", lockFile);

            // Act
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
			Logger.LogDebug("LuceneController_LockFileCanBeObtainedByOnlySingleController(): doc1 → {Doc1}", doc1);
            this.luceneController.Add(doc1);

            // create another controller then try to access the already locked index by the first one
            var secondController = new LuceneControllerImpl(this.fakeHostController, new FakeApplicationStatusInfo());

			Logger.LogDebug("LuceneController_LockFileCanBeObtainedByOnlySingleController(): secondController → {SecondController}", secondController);
            // Assert
            Assert.That(lockFile, Does.Exist);
            Assert.Throws<SearchException>(() => secondController.Add(doc1));
			Logger.LogDebug("<<< Exiting LuceneController_LockFileCanBeObtainedByOnlySingleController()");
        }

        [Test]
        public void LuceneController_DocumentMaxAndCountAreCorrect()
        {
			Logger.LogDebug(">>> Entering LuceneController_DocumentMaxAndCountAreCorrect()");
            this.AddTestDocs();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.luceneController.MaxDocsCount(), Is.EqualTo(TotalTestDocs2Create));
                Assert.That(this.luceneController.SearchbleDocsCount(), Is.EqualTo(TotalTestDocs2Create));
            }
			Logger.LogDebug("<<< Exiting LuceneController_DocumentMaxAndCountAreCorrect()");
        }

        [Test]
        public void LuceneController_TestDeleteBeforeOptimize()
        {
			Logger.LogDebug(">>> Entering LuceneController_TestDeleteBeforeOptimize()");
            // Arrange
            this.AddTestDocs();
            var delCount = this.DeleteTestDocs();

			Logger.LogDebug("LuceneController_TestDeleteBeforeOptimize(): delCount → {DelCount}", delCount);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.luceneController.HasDeletions(), Is.True);
                Assert.That(this.luceneController.MaxDocsCount(), Is.EqualTo(TotalTestDocs2Create));
                Assert.That(this.luceneController.SearchbleDocsCount(), Is.EqualTo(TotalTestDocs2Create - delCount));
            }
			Logger.LogDebug("<<< Exiting LuceneController_TestDeleteBeforeOptimize()");
        }

        [Test]
        public void LuceneController_TestDeleteAfterOptimize()
        {
			Logger.LogDebug(">>> Entering LuceneController_TestDeleteAfterOptimize()");
            // Arrange
            this.AddTestDocs();
            var delCount = this.DeleteTestDocs();

			Logger.LogDebug("LuceneController_TestDeleteAfterOptimize(): delCount → {DelCount}", delCount);
            this.luceneController.OptimizeSearchIndex(true);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.luceneController.MaxDocsCount(), Is.EqualTo(TotalTestDocs2Create));
                Assert.That(this.luceneController.SearchbleDocsCount(), Is.EqualTo(TotalTestDocs2Create - delCount));
            }
			Logger.LogDebug("<<< Exiting LuceneController_TestDeleteAfterOptimize()");
        }

        [Test]
        public void LuceneController_TestGetSearchStatistics()
        {
			Logger.LogDebug(">>> Entering LuceneController_TestGetSearchStatistics()");
            // Arrange
            var addedCount = this.AddTestDocs();
            var delCount = this.DeleteTestDocs();
			Logger.LogDebug("LuceneController_TestGetSearchStatistics(): addedCount → {AddedCount}", addedCount);
			Logger.LogDebug("LuceneController_TestGetSearchStatistics(): delCount → {DelCount}", delCount);
            var statistics = this.luceneController.GetSearchStatistics();

			Logger.LogDebug("LuceneController_TestGetSearchStatistics(): statistics → {Statistics}", statistics);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(statistics, Is.Not.Null);
                Assert.That(delCount, Is.EqualTo(statistics.TotalDeletedDocuments));
                Assert.That(addedCount - delCount, Is.EqualTo(statistics.TotalActiveDocuments));
            }
			Logger.LogDebug("<<< Exiting LuceneController_TestGetSearchStatistics()");
        }

        [Test]
        public void SearchController_LuceneControllerReaderIsNotNullWhenWriterIsNull()
        {
			Logger.LogDebug(">>> Entering SearchController_LuceneControllerReaderIsNotNullWhenWriterIsNull()");
            // Arrange
            this.AddTestDocs();
            this.CreateNewLuceneControllerInstance(); // to force a new reader for the next assertion

            // Assert
            Assert.That(this.luceneController.GetSearcher(), Is.Not.Null);
			Logger.LogDebug("<<< Exiting SearchController_LuceneControllerReaderIsNotNullWhenWriterIsNull()");
        }

        private void CreateNewLuceneControllerInstance()
        {
			Logger.LogDebug(">>> Entering CreateNewLuceneControllerInstance()");
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

        private void DeleteIndexFolder()
        {
			Logger.LogDebug(">>> Entering DeleteIndexFolder()");
            try
            {
                if (Directory.Exists(this.SearchIndexFolder))
                {
                    Directory.Delete(this.SearchIndexFolder, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
			Logger.LogDebug("<<< Exiting DeleteIndexFolder()");
        }

        /// <summary>Adds standarad SearchDocs in Lucene Index.</summary>
        private void AddStandardDocs()
        {
			Logger.LogDebug(">>> Entering AddStandardDocs()");
            string[] lines =
            {
                Line1, Line2, Line3, Line4,
                };

            this.AddLinesAsSearchDocs(lines);
			Logger.LogDebug("<<< Exiting AddStandardDocs()");
        }

        private void AddLinesAsSearchDocs(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                var field = new Field(Constants.ContentTag, line, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                var doc = new Document();
                doc.Add(field);
                this.luceneController.Add(doc);
            }

            this.luceneController.Commit();
        }

        private int AddTestDocs()
        {
			Logger.LogDebug(">>> Entering AddTestDocs()");
            // Act
            for (var i = 0; i < TotalTestDocs2Create; i++)
            {
                var doc = new Document();

				Logger.LogDebug("AddTestDocs(): doc → {Doc}", doc);
                // format to "D#" because LengthFilter will not consider words of length < 3 or > 255 characters in length (defaults)
                doc.Add(new Field(ContentFieldName, i.ToString("D" + Constants.DefaultMinLen), Field.Store.YES, Field.Index.ANALYZED));
                this.luceneController.Add(doc);
            }

            this.luceneController.Commit();
			Logger.LogDebug("<<< Exiting AddTestDocs()");
            return TotalTestDocs2Create;
        }

        private int DeleteTestDocs()
        {
			Logger.LogDebug(">>> Entering DeleteTestDocs()");
            // Act
            // delete odd docs => [1, 3]
            var delCount = 0;
            for (var i = 1; i < TotalTestDocs2Create; i += 2)
            {
                // format to "D#" because LengthFilter will not consider the defaults for these values
                this.luceneController.Delete(new TermQuery(new Term(ContentFieldName, i.ToString("D" + Constants.DefaultMinLen))));
                delCount++;
            }

            this.luceneController.Commit();
			Logger.LogDebug("<<< Exiting DeleteTestDocs()");
            return delCount;
        }

        private LuceneSearchContext CreateSearchContext(LuceneQuery luceneQuery)
        {
            return new LuceneSearchContext { LuceneQuery = luceneQuery, SearchQuery = this.mockSearchQuery.Object };
        }
    }
}
