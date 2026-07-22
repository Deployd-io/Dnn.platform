// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Search
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Services.Search.Internals;
    using NUnit.Framework;

    [TestFixture]
    public class SearchQueryStringParserTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public void GetLastModifiedDate_ShouldReturnOneDayAgoDate_WhenKeywordsHaveAfterDay()
        {
			Logger.LogDebug(">>> Entering GetLastModifiedDate_ShouldReturnOneDayAgoDate_WhenKeywordsHaveAfterDay()");
            // Arrange
            const string keywords = "mysearch after:day";
            var outputKeywords = string.Empty;

            // Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

			Logger.LogDebug("GetLastModifiedDate_ShouldReturnOneDayAgoDate_WhenKeywordsHaveAfterDay(): date → {Date}", date);
            // Assert
            Assert.That(date.Date, Is.EqualTo(DateTime.UtcNow.AddDays(-1).Date));
			Logger.LogDebug("<<< Exiting GetLastModifiedDate_ShouldReturnOneDayAgoDate_WhenKeywordsHaveAfterDay()");
        }

        [Test]
        public void GetLastModifiedDate_ShouldReturnOneWeekAgoDate_WhenKeywordsHaveAfterWeek()
        {
			Logger.LogDebug(">>> Entering GetLastModifiedDate_ShouldReturnOneWeekAgoDate_WhenKeywordsHaveAfterWeek()");
            // Arrange
            const string keywords = "mysearch after:week";
            var outputKeywords = string.Empty;

            // Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

			Logger.LogDebug("GetLastModifiedDate_ShouldReturnOneWeekAgoDate_WhenKeywordsHaveAfterWeek(): date → {Date}", date);
            // Assert
            Assert.That(date.Date, Is.EqualTo(DateTime.UtcNow.AddDays(-7).Date));
			Logger.LogDebug("<<< Exiting GetLastModifiedDate_ShouldReturnOneWeekAgoDate_WhenKeywordsHaveAfterWeek()");
        }

        [Test]
        public void GetLastModifiedDate_ShouldReturnOneMonthAgoDate_WhenKeywordsHaveAfterMonth()
        {
			Logger.LogDebug(">>> Entering GetLastModifiedDate_ShouldReturnOneMonthAgoDate_WhenKeywordsHaveAfterMonth()");
            // Arrange
            const string keywords = "mysearch after:month";
            var outputKeywords = string.Empty;

            // Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

			Logger.LogDebug("GetLastModifiedDate_ShouldReturnOneMonthAgoDate_WhenKeywordsHaveAfterMonth(): date → {Date}", date);
            // Assert
            Assert.That(date.Date, Is.EqualTo(DateTime.UtcNow.AddMonths(-1).Date));
			Logger.LogDebug("<<< Exiting GetLastModifiedDate_ShouldReturnOneMonthAgoDate_WhenKeywordsHaveAfterMonth()");
        }

        [Test]
        public void GetLastModifiedDate_ShouldReturnOneQuarterAgoDate_WhenKeywordsHaveAfterQuarter()
        {
			Logger.LogDebug(">>> Entering GetLastModifiedDate_ShouldReturnOneQuarterAgoDate_WhenKeywordsHaveAfterQuarter()");
            // Arrange
            const string keywords = "mysearch after:quarter";
            var outputKeywords = string.Empty;

            // Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

			Logger.LogDebug("GetLastModifiedDate_ShouldReturnOneQuarterAgoDate_WhenKeywordsHaveAfterQuarter(): date → {Date}", date);
            // Assert
            Assert.That(date.Date, Is.EqualTo(DateTime.UtcNow.AddMonths(-3).Date));
			Logger.LogDebug("<<< Exiting GetLastModifiedDate_ShouldReturnOneQuarterAgoDate_WhenKeywordsHaveAfterQuarter()");
        }

        [Test]
        public void GetLastModifiedDate_ShouldReturnOneYearAgoDate_WhenKeywordsHaveAfterYear()
        {
			Logger.LogDebug(">>> Entering GetLastModifiedDate_ShouldReturnOneYearAgoDate_WhenKeywordsHaveAfterYear()");
            // Arrange
            const string keywords = "mysearch after:year";
            var outputKeywords = string.Empty;

            // Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

			Logger.LogDebug("GetLastModifiedDate_ShouldReturnOneYearAgoDate_WhenKeywordsHaveAfterYear(): date → {Date}", date);
            // Assert
            Assert.That(date.Date, Is.EqualTo(DateTime.UtcNow.AddYears(-1).Date));
			Logger.LogDebug("<<< Exiting GetLastModifiedDate_ShouldReturnOneYearAgoDate_WhenKeywordsHaveAfterYear()");
        }

        [Test]
        public void GetLastModifiedDate_ShouldReturnMinDate_WhenKeywordsDoNotHaveAfterParameter()
        {
			Logger.LogDebug(">>> Entering GetLastModifiedDate_ShouldReturnMinDate_WhenKeywordsDoNotHaveAfterParameter()");
            // Arrange
            const string keywords = "mysearch ";
            var outputKeywords = string.Empty;

            // Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

			Logger.LogDebug("GetLastModifiedDate_ShouldReturnMinDate_WhenKeywordsDoNotHaveAfterParameter(): date → {Date}", date);
            // Assert
            Assert.That(date, Is.EqualTo(DateTime.MinValue));
			Logger.LogDebug("<<< Exiting GetLastModifiedDate_ShouldReturnMinDate_WhenKeywordsDoNotHaveAfterParameter()");
        }

        [Test]
        public void GetLastModifiedDate_ShouldSetOutputKeywordsEqualsToTrimmedKeywordsWithoutAfterParameter_WhenKeywordsHaveAfterParameter()
        {
			Logger.LogDebug(">>> Entering GetLastModifiedDate_ShouldSetOutputKeywordsEqualsToTrimmedKeywordsWithoutAfterParameter_WhenKeywordsHaveAfterParameter()");
            // Arrange
            const string keywords = " mysearch [tag1][tag2][tag3] after:week ";
            const string expectedOutputKeywords = "mysearch [tag1][tag2][tag3]";
            var outputKeywords = string.Empty;

            // Act
            SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            // Assert
            Assert.That(outputKeywords, Is.EqualTo(expectedOutputKeywords));
			Logger.LogDebug("<<< Exiting GetLastModifiedDate_ShouldSetOutputKeywordsEqualsToTrimmedKeywordsWithoutAfterParameter_WhenKeywordsHaveAfterParameter()");
        }

        [Test]
        public void GetLastModifiedDate_ShouldSetOutputKeywordsEqualsToTrimmedKeywords_WhenKeywordsDoNotHaveAfterParameter()
        {
			Logger.LogDebug(">>> Entering GetLastModifiedDate_ShouldSetOutputKeywordsEqualsToTrimmedKeywords_WhenKeywordsDoNotHaveAfterParameter()");
            // Arrange
            const string keywords = " mysearch [tag1][tag2][tag3] ";
            const string expectedOutputKeywords = "mysearch [tag1][tag2][tag3]";
            var outputKeywords = string.Empty;

            // Act
            SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            // Assert
            Assert.That(outputKeywords, Is.EqualTo(expectedOutputKeywords));
			Logger.LogDebug("<<< Exiting GetLastModifiedDate_ShouldSetOutputKeywordsEqualsToTrimmedKeywords_WhenKeywordsDoNotHaveAfterParameter()");
        }

        [Test]
        public void GetSearchTypeList_ShouldReturnSearchTypeList_WhenKeywordsHaveTypeParameter()
        {
			Logger.LogDebug(">>> Entering GetSearchTypeList_ShouldReturnSearchTypeList_WhenKeywordsHaveTypeParameter()");
            // Arrange
            const string keywords = "mysearch type:Documents,Pages";
            var expectedSearchTypeList = new List<string> { "Documents", "Pages" };
			Logger.LogDebug("GetSearchTypeList_ShouldReturnSearchTypeList_WhenKeywordsHaveTypeParameter(): expectedSearchTypeList → {ExpectedSearchTypeList}", expectedSearchTypeList);
            var outputKeywords = string.Empty;

            // Act
            var searchTypeList = SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out outputKeywords);

			Logger.LogDebug("GetSearchTypeList_ShouldReturnSearchTypeList_WhenKeywordsHaveTypeParameter(): searchTypeList → {SearchTypeList}", searchTypeList);
            // Assert
            Assert.That(searchTypeList, Is.EqualTo(expectedSearchTypeList));
			Logger.LogDebug("<<< Exiting GetSearchTypeList_ShouldReturnSearchTypeList_WhenKeywordsHaveTypeParameter()");
        }

        [Test]
        public void GetSearchTypeList_ShouldReturnEmptySearchTypeList_WhenKeywordsDoNotHaveTypeParameter()
        {
			Logger.LogDebug(">>> Entering GetSearchTypeList_ShouldReturnEmptySearchTypeList_WhenKeywordsDoNotHaveTypeParameter()");
            // Arrange
            const string keywords = "mysearch ";
            var outputKeywords = string.Empty;

            // Act
            var searchTypeList = SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out outputKeywords);

			Logger.LogDebug("GetSearchTypeList_ShouldReturnEmptySearchTypeList_WhenKeywordsDoNotHaveTypeParameter(): searchTypeList → {SearchTypeList}", searchTypeList);
            // Assert
            Assert.That(searchTypeList, Is.Empty);
			Logger.LogDebug("<<< Exiting GetSearchTypeList_ShouldReturnEmptySearchTypeList_WhenKeywordsDoNotHaveTypeParameter()");
        }

        [Test]
        public void GetSearchTypeList_ShouldSetOutputKeywordsEqualsToTrimmedKeywordsWithoutTypeParameter_WhenKeywordsHaveTypeParameter()
        {
			Logger.LogDebug(">>> Entering GetSearchTypeList_ShouldSetOutputKeywordsEqualsToTrimmedKeywordsWithoutTypeParameter_WhenKeywordsHaveTypeParameter()");
            // Arrange
            const string keywords = " mysearch [tag1][tag2][tag3] type:Documents ";
            const string expectedOutputKeywords = "mysearch [tag1][tag2][tag3]";
            var outputKeywords = string.Empty;

            // Act
            SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out outputKeywords);

            // Assert
            Assert.That(outputKeywords, Is.EqualTo(expectedOutputKeywords));
			Logger.LogDebug("<<< Exiting GetSearchTypeList_ShouldSetOutputKeywordsEqualsToTrimmedKeywordsWithoutTypeParameter_WhenKeywordsHaveTypeParameter()");
        }

        [Test]
        public void GetSearchTypeList_ShouldSetOutputKeywordsEqualsToTrimmedKeywords_WhenKeywordsDoNotHaveTypeParameter()
        {
			Logger.LogDebug(">>> Entering GetSearchTypeList_ShouldSetOutputKeywordsEqualsToTrimmedKeywords_WhenKeywordsDoNotHaveTypeParameter()");
            // Arrange
            const string keywords = " mysearch [tag1][tag2][tag3] ";
            const string expectedOutputKeywords = "mysearch [tag1][tag2][tag3]";
            var outputKeywords = string.Empty;

            // Act
            SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out outputKeywords);

            // Assert
            Assert.That(outputKeywords, Is.EqualTo(expectedOutputKeywords));
			Logger.LogDebug("<<< Exiting GetSearchTypeList_ShouldSetOutputKeywordsEqualsToTrimmedKeywords_WhenKeywordsDoNotHaveTypeParameter()");
        }

        [Test]
        public void GetTags_ShouldReturnTagsList_WhenKeywordsHaveTags()
        {
			Logger.LogDebug(">>> Entering GetTags_ShouldReturnTagsList_WhenKeywordsHaveTags()");
            // Arrange
            const string keywords = "mysearch [tag][tag2]";
            var expectedTagsList = new List<string> { "tag", "tag2" };
			Logger.LogDebug("GetTags_ShouldReturnTagsList_WhenKeywordsHaveTags(): expectedTagsList → {ExpectedTagsList}", expectedTagsList);
            var outputKeywords = string.Empty;

            // Act
            var tagsList = SearchQueryStringParser.Instance.GetTags(keywords, out outputKeywords);

			Logger.LogDebug("GetTags_ShouldReturnTagsList_WhenKeywordsHaveTags(): tagsList → {TagsList}", tagsList);
            // Assert
            Assert.That(tagsList, Is.EqualTo(expectedTagsList));
			Logger.LogDebug("<<< Exiting GetTags_ShouldReturnTagsList_WhenKeywordsHaveTags()");
        }

        [Test]
        public void GetTags_ShouldReturnEmptyTagList_WhenKeywordsDoNotHaveTags()
        {
			Logger.LogDebug(">>> Entering GetTags_ShouldReturnEmptyTagList_WhenKeywordsDoNotHaveTags()");
            // Arrange
            const string keywords = "mysearch ";
            var outputKeywords = string.Empty;

            // Act
            var tagsList = SearchQueryStringParser.Instance.GetTags(keywords, out outputKeywords);

			Logger.LogDebug("GetTags_ShouldReturnEmptyTagList_WhenKeywordsDoNotHaveTags(): tagsList → {TagsList}", tagsList);
            // Assert
            Assert.That(tagsList, Is.Empty);
			Logger.LogDebug("<<< Exiting GetTags_ShouldReturnEmptyTagList_WhenKeywordsDoNotHaveTags()");
        }

        [Test]
        public void GetTags_ShouldSetOutputKeywordsEqualsToTrimmedKeywordsWithoutTags_WhenKeywordsHaveTags()
        {
			Logger.LogDebug(">>> Entering GetTags_ShouldSetOutputKeywordsEqualsToTrimmedKeywordsWithoutTags_WhenKeywordsHaveTags()");
            // Arrange
            const string keywords = "mysearch [tag][tag2][tag3]";
            const string expectedOutputKeywords = "mysearch";
            var outputKeywords = string.Empty;

            // Act
            SearchQueryStringParser.Instance.GetTags(keywords, out outputKeywords);

            // Assert
            Assert.That(outputKeywords, Is.EqualTo(expectedOutputKeywords));
			Logger.LogDebug("<<< Exiting GetTags_ShouldSetOutputKeywordsEqualsToTrimmedKeywordsWithoutTags_WhenKeywordsHaveTags()");
        }

        [Test]
        public void GetTags_ShouldSetOutputKeywordsEqualsToTrimmedKeywords_WhenKeywordsDoNotHaveTags()
        {
			Logger.LogDebug(">>> Entering GetTags_ShouldSetOutputKeywordsEqualsToTrimmedKeywords_WhenKeywordsDoNotHaveTags()");
            // Arrange
            const string keywords = " mysearch after:week ";
            const string expectedOutputKeywords = "mysearch after:week";
            var outputKeywords = string.Empty;

            // Act
            SearchQueryStringParser.Instance.GetTags(keywords, out outputKeywords);

            // Assert
            Assert.That(outputKeywords, Is.EqualTo(expectedOutputKeywords));
			Logger.LogDebug("<<< Exiting GetTags_ShouldSetOutputKeywordsEqualsToTrimmedKeywords_WhenKeywordsDoNotHaveTags()");
        }
    }
}
