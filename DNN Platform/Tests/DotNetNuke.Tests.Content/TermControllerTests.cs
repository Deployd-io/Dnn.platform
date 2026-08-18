// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Content
{
    using System;
    using System.Linq;

    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Content.Mocks;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    /// <summary>  Summary description for TermTests.</summary>
    [TestFixture]
    public class TermControllerTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private Mock<CachingProvider> mockCache;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			_logger.LogDebug(">>> Entering SetUp()");
            var vocabularyController = MockHelper.CreateMockVocabularyController();
            var dataProvider = MockComponentProvider.CreateDataProvider();
			_logger.LogDebug("SetUp(): dataProvider → {DataProvider}", dataProvider);
			_logger.LogDebug("SetUp(): vocabularyController → {VocabularyController}", vocabularyController);
            dataProvider.Setup(c => c.GetProviderPath()).Returns(string.Empty);

            this.mockCache = MockComponentProvider.CreateDataCacheProvider();
			_logger.LogDebug("SetUp(): mockCache → {MockCache}", mockCache);
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					_logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(vocabularyController.Object);
                    services.AddSingleton(dataProvider.Object);
                    services.AddSingleton(this.mockCache.Object);
                    services.AddSingleton(Mock.Of<ICryptographyProvider>());
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
        public void TermController_AddTerm_Throws_On_Null_Term()
        {
			_logger.LogDebug(">>> Entering TermController_AddTerm_Throws_On_Null_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_AddTerm_Throws_On_Null_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_AddTerm_Throws_On_Null_Term(): termController → {TermController}", termController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => termController.AddTerm(null));
			_logger.LogDebug("<<< Exiting TermController_AddTerm_Throws_On_Null_Term()");
        }

        [Test]
        public void TermController_AddTerm_Throws_On_Invalid_Term()
        {
			_logger.LogDebug(">>> Entering TermController_AddTerm_Throws_On_Invalid_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_AddTerm_Throws_On_Invalid_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_AddTerm_Throws_On_Invalid_Term(): termController → {TermController}", termController);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
			_logger.LogDebug("TermController_AddTerm_Throws_On_Invalid_Term(): term → {Term}", term);
			_logger.LogDebug("TermController_AddTerm_Throws_On_Invalid_Term(): term.Name → {Arg0}", term.Name);
            term.Name = Constants.TERM_InValidName;

            // Act, Arrange
            Assert.Throws<ArgumentException>(() => termController.AddTerm(term));
			_logger.LogDebug("<<< Exiting TermController_AddTerm_Throws_On_Invalid_Term()");
        }

        [Test]
        public void TermController_AddTerm_Throws_On_Negative_VocabularyId()
        {
			_logger.LogDebug(">>> Entering TermController_AddTerm_Throws_On_Negative_VocabularyId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_AddTerm_Throws_On_Negative_VocabularyId(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_AddTerm_Throws_On_Negative_VocabularyId(): termController → {TermController}", termController);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Null.NullInteger);
			_logger.LogDebug("TermController_AddTerm_Throws_On_Negative_VocabularyId(): term → {Term}", term);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.AddTerm(term));
			_logger.LogDebug("<<< Exiting TermController_AddTerm_Throws_On_Negative_VocabularyId()");
        }

        [Test]
        public void TermController_AddTerm_Should_Call_DataService_AddSimpleTerm_If_Term_Is_Simple_Term()
        {
			_logger.LogDebug(">>> Entering TermController_AddTerm_Should_Call_DataService_AddSimpleTerm_If_Term_Is_Simple_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_AddTerm_Should_Call_DataService_AddSimpleTerm_If_Term_Is_Simple_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_AddTerm_Should_Call_DataService_AddSimpleTerm_If_Term_Is_Simple_Term(): termController → {TermController}", termController);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
			_logger.LogDebug("TermController_AddTerm_Should_Call_DataService_AddSimpleTerm_If_Term_Is_Simple_Term(): term → {Term}", term);

            // Act
            int termId = termController.AddTerm(term);
			_logger.LogDebug("TermController_AddTerm_Should_Call_DataService_AddSimpleTerm_If_Term_Is_Simple_Term(): termId → {TermId}", termId);

            // Assert
            mockDataService.Verify(ds => ds.AddSimpleTerm(term, UserController.Instance.GetCurrentUserInfo().UserID));
			_logger.LogDebug("<<< Exiting TermController_AddTerm_Should_Call_DataService_AddSimpleTerm_If_Term_Is_Simple_Term()");
        }

        [Test]
        public void TermController_AddTerm_Should_Call_DataService_AddHeirarchicalTerm_If_Term_Is_Heirarchical_Term()
        {
			_logger.LogDebug(">>> Entering TermController_AddTerm_Should_Call_DataService_AddHeirarchicalTerm_If_Term_Is_Heirarchical_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_AddTerm_Should_Call_DataService_AddHeirarchicalTerm_If_Term_Is_Heirarchical_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_AddTerm_Should_Call_DataService_AddHeirarchicalTerm_If_Term_Is_Heirarchical_Term(): termController → {TermController}", termController);

            Term term = ContentTestHelper.CreateValidHeirarchicalTerm(Constants.VOCABULARY_HierarchyVocabularyId, Constants.TERM_ValidParentTermId);
			_logger.LogDebug("TermController_AddTerm_Should_Call_DataService_AddHeirarchicalTerm_If_Term_Is_Heirarchical_Term(): term → {Term}", term);

            // Act
            int termId = termController.AddTerm(term);
			_logger.LogDebug("TermController_AddTerm_Should_Call_DataService_AddHeirarchicalTerm_If_Term_Is_Heirarchical_Term(): termId → {TermId}", termId);

            // Assert
            mockDataService.Verify(ds => ds.AddHeirarchicalTerm(term, UserController.Instance.GetCurrentUserInfo().UserID));
			_logger.LogDebug("<<< Exiting TermController_AddTerm_Should_Call_DataService_AddHeirarchicalTerm_If_Term_Is_Heirarchical_Term()");
        }

        [Test]
        public void TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Simple_Term()
        {
			_logger.LogDebug(">>> Entering TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Simple_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Simple_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Simple_Term(): termController → {TermController}", termController);
            mockDataService.Setup(ds => ds.AddSimpleTerm(It.IsAny<Term>(), It.IsAny<int>())).Returns(Constants.TERM_AddTermId);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
			_logger.LogDebug("TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Simple_Term(): term → {Term}", term);

            // Act
            int termId = termController.AddTerm(term);
			_logger.LogDebug("TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Simple_Term(): termId → {TermId}", termId);

            // Assert
            Assert.That(termId, Is.EqualTo(Constants.TERM_AddTermId));
			_logger.LogDebug("<<< Exiting TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Simple_Term()");
        }

        [Test]
        public void TermController_AddTerm_Sets_Valid_Id_On_Valid_Term_If_Term_Is_Simple_Term()
        {
			_logger.LogDebug(">>> Entering TermController_AddTerm_Sets_Valid_Id_On_Valid_Term_If_Term_Is_Simple_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_AddTerm_Sets_Valid_Id_On_Valid_Term_If_Term_Is_Simple_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_AddTerm_Sets_Valid_Id_On_Valid_Term_If_Term_Is_Simple_Term(): termController → {TermController}", termController);
            mockDataService.Setup(ds => ds.AddSimpleTerm(It.IsAny<Term>(), It.IsAny<int>())).Returns(Constants.TERM_AddTermId);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
			_logger.LogDebug("TermController_AddTerm_Sets_Valid_Id_On_Valid_Term_If_Term_Is_Simple_Term(): term → {Term}", term);

            // Act
            termController.AddTerm(term);

            // Assert
            Assert.That(term.TermId, Is.EqualTo(Constants.TERM_AddTermId));
			_logger.LogDebug("<<< Exiting TermController_AddTerm_Sets_Valid_Id_On_Valid_Term_If_Term_Is_Simple_Term()");
        }

        [Test]
        public void TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Heirarchical_Term()
        {
			_logger.LogDebug(">>> Entering TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Heirarchical_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Heirarchical_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Heirarchical_Term(): termController → {TermController}", termController);
            mockDataService.Setup(ds => ds.AddHeirarchicalTerm(It.IsAny<Term>(), It.IsAny<int>())).Returns(Constants.TERM_AddTermId);

            Term term = ContentTestHelper.CreateValidHeirarchicalTerm(Constants.VOCABULARY_HierarchyVocabularyId, Constants.TERM_ValidParentTermId);
			_logger.LogDebug("TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Heirarchical_Term(): term → {Term}", term);

            // Act
            int termId = termController.AddTerm(term);
			_logger.LogDebug("TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Heirarchical_Term(): termId → {TermId}", termId);

            // Assert
            Assert.That(termId, Is.EqualTo(Constants.TERM_AddTermId));
			_logger.LogDebug("<<< Exiting TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Heirarchical_Term()");
        }

        [Test]
        public void TermController_AddTerm_Sets_Valid_Id_On_Valid_Term_If_Term_Is_Heirarchical_Term()
        {
			_logger.LogDebug(">>> Entering TermController_AddTerm_Sets_Valid_Id_On_Valid_Term_If_Term_Is_Heirarchical_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_AddTerm_Sets_Valid_Id_On_Valid_Term_If_Term_Is_Heirarchical_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_AddTerm_Sets_Valid_Id_On_Valid_Term_If_Term_Is_Heirarchical_Term(): termController → {TermController}", termController);
            mockDataService.Setup(ds => ds.AddHeirarchicalTerm(It.IsAny<Term>(), It.IsAny<int>())).Returns(Constants.TERM_AddTermId);

            Term term = ContentTestHelper.CreateValidHeirarchicalTerm(Constants.VOCABULARY_HierarchyVocabularyId, Constants.TERM_ValidParentTermId);
			_logger.LogDebug("TermController_AddTerm_Sets_Valid_Id_On_Valid_Term_If_Term_Is_Heirarchical_Term(): term → {Term}", term);

            // Act
            termController.AddTerm(term);

            // Assert
            Assert.That(term.TermId, Is.EqualTo(Constants.TERM_AddTermId));
			_logger.LogDebug("<<< Exiting TermController_AddTerm_Sets_Valid_Id_On_Valid_Term_If_Term_Is_Heirarchical_Term()");
        }

        [Test]
        public void TermController_AddTerm_Clears_Term_Cache_On_Valid_Term()
        {
			_logger.LogDebug(">>> Entering TermController_AddTerm_Clears_Term_Cache_On_Valid_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_AddTerm_Clears_Term_Cache_On_Valid_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_AddTerm_Clears_Term_Cache_On_Valid_Term(): termController → {TermController}", termController);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
			_logger.LogDebug("TermController_AddTerm_Clears_Term_Cache_On_Valid_Term(): term → {Term}", term);

            // Act
            termController.AddTerm(term);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(string.Format(Constants.TERM_CacheKey, Constants.VOCABULARY_ValidVocabularyId)));
			_logger.LogDebug("<<< Exiting TermController_AddTerm_Clears_Term_Cache_On_Valid_Term()");
        }

        [Test]
        public void TermController_AddTermToContent_Throws_On_Null_Term()
        {
			_logger.LogDebug(">>> Entering TermController_AddTermToContent_Throws_On_Null_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_AddTermToContent_Throws_On_Null_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_AddTermToContent_Throws_On_Null_Term(): termController → {TermController}", termController);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
			_logger.LogDebug("TermController_AddTermToContent_Throws_On_Null_Term(): content → {Content}", content);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => termController.AddTermToContent(null, content));
			_logger.LogDebug("<<< Exiting TermController_AddTermToContent_Throws_On_Null_Term()");
        }

        [Test]
        public void TermController_AddTermToContent_Throws_On_Null_ContentItem()
        {
			_logger.LogDebug(">>> Entering TermController_AddTermToContent_Throws_On_Null_ContentItem()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_AddTermToContent_Throws_On_Null_ContentItem(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_AddTermToContent_Throws_On_Null_ContentItem(): termController → {TermController}", termController);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
			_logger.LogDebug("TermController_AddTermToContent_Throws_On_Null_ContentItem(): term → {Term}", term);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => termController.AddTermToContent(term, null));
			_logger.LogDebug("<<< Exiting TermController_AddTermToContent_Throws_On_Null_ContentItem()");
        }

        [Test]
        public void TermController_AddTermToContent_Should_Call_DataService_If_Valid_Params()
        {
			_logger.LogDebug(">>> Entering TermController_AddTermToContent_Should_Call_DataService_If_Valid_Params()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_AddTermToContent_Should_Call_DataService_If_Valid_Params(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_AddTermToContent_Should_Call_DataService_If_Valid_Params(): termController → {TermController}", termController);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
			_logger.LogDebug("TermController_AddTermToContent_Should_Call_DataService_If_Valid_Params(): content → {Content}", content);
            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
			_logger.LogDebug("TermController_AddTermToContent_Should_Call_DataService_If_Valid_Params(): term → {Term}", term);

            // Act
            termController.AddTermToContent(term, content);

            // Assert
            mockDataService.Verify(ds => ds.AddTermToContent(term, content));
			_logger.LogDebug("<<< Exiting TermController_AddTermToContent_Should_Call_DataService_If_Valid_Params()");
        }

        [Test]
        public void TermController_DeleteTerm_Throws_On_Null_Term()
        {
			_logger.LogDebug(">>> Entering TermController_DeleteTerm_Throws_On_Null_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_DeleteTerm_Throws_On_Null_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_DeleteTerm_Throws_On_Null_Term(): termController → {TermController}", termController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => termController.DeleteTerm(null));
			_logger.LogDebug("<<< Exiting TermController_DeleteTerm_Throws_On_Null_Term()");
        }

        [Test]
        public void TermController_DeleteTerm_Throws_On_Negative_TermId()
        {
			_logger.LogDebug(">>> Entering TermController_DeleteTerm_Throws_On_Negative_TermId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_DeleteTerm_Throws_On_Negative_TermId(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_DeleteTerm_Throws_On_Negative_TermId(): termController → {TermController}", termController);

            var term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
			_logger.LogDebug("TermController_DeleteTerm_Throws_On_Negative_TermId(): term → {Term}", term);
			_logger.LogDebug("TermController_DeleteTerm_Throws_On_Negative_TermId(): term.TermId → {Arg0}", term.TermId);
            term.TermId = Null.NullInteger;

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.DeleteTerm(term));
			_logger.LogDebug("<<< Exiting TermController_DeleteTerm_Throws_On_Negative_TermId()");
        }

        [Test]
        public void TermController_DeleteTerm_Should_Call_DataService_DeleteSimpleTerm_If_Term_Is_Simple_Term()
        {
			_logger.LogDebug(">>> Entering TermController_DeleteTerm_Should_Call_DataService_DeleteSimpleTerm_If_Term_Is_Simple_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_DeleteTerm_Should_Call_DataService_DeleteSimpleTerm_If_Term_Is_Simple_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_DeleteTerm_Should_Call_DataService_DeleteSimpleTerm_If_Term_Is_Simple_Term(): termController → {TermController}", termController);

            var term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
			_logger.LogDebug("TermController_DeleteTerm_Should_Call_DataService_DeleteSimpleTerm_If_Term_Is_Simple_Term(): term → {Term}", term);
			_logger.LogDebug("TermController_DeleteTerm_Should_Call_DataService_DeleteSimpleTerm_If_Term_Is_Simple_Term(): term.TermId → {Arg0}", term.TermId);
            term.TermId = Constants.TERM_DeleteTermId;

            // Act
            termController.DeleteTerm(term);

            // Assert
            mockDataService.Verify(ds => ds.DeleteSimpleTerm(term));
			_logger.LogDebug("<<< Exiting TermController_DeleteTerm_Should_Call_DataService_DeleteSimpleTerm_If_Term_Is_Simple_Term()");
        }

        [Test]
        public void TermController_DeleteTerm_Should_Call_DataService_DeleteHeirarchicalTerm_If_Term_Is_Heirarchical_Term()
        {
			_logger.LogDebug(">>> Entering TermController_DeleteTerm_Should_Call_DataService_DeleteHeirarchicalTerm_If_Term_Is_Heirarchical_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_DeleteTerm_Should_Call_DataService_DeleteHeirarchicalTerm_If_Term_Is_Heirarchical_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_DeleteTerm_Should_Call_DataService_DeleteHeirarchicalTerm_If_Term_Is_Heirarchical_Term(): termController → {TermController}", termController);

            var term = ContentTestHelper.CreateValidHeirarchicalTerm(Constants.VOCABULARY_HierarchyVocabularyId, Constants.TERM_ValidParentTermId);
			_logger.LogDebug("TermController_DeleteTerm_Should_Call_DataService_DeleteHeirarchicalTerm_If_Term_Is_Heirarchical_Term(): term → {Term}", term);
			_logger.LogDebug("TermController_DeleteTerm_Should_Call_DataService_DeleteHeirarchicalTerm_If_Term_Is_Heirarchical_Term(): term.TermId → {Arg0}", term.TermId);
            term.TermId = Constants.TERM_DeleteTermId;

            // Act
            termController.DeleteTerm(term);

            // Assert
            mockDataService.Verify(ds => ds.DeleteHeirarchicalTerm(term));
			_logger.LogDebug("<<< Exiting TermController_DeleteTerm_Should_Call_DataService_DeleteHeirarchicalTerm_If_Term_Is_Heirarchical_Term()");
        }

        [Test]
        public void TermController_DeleteTerm_Clears_Term_Cache_On_Valid_Term()
        {
			_logger.LogDebug(">>> Entering TermController_DeleteTerm_Clears_Term_Cache_On_Valid_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_DeleteTerm_Clears_Term_Cache_On_Valid_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_DeleteTerm_Clears_Term_Cache_On_Valid_Term(): termController → {TermController}", termController);

            var term = new Term(Constants.VOCABULARY_ValidVocabularyId) { TermId = Constants.TERM_DeleteTermId };
			_logger.LogDebug("TermController_DeleteTerm_Clears_Term_Cache_On_Valid_Term(): term → {Term}", term);

            // Act
            termController.DeleteTerm(term);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(string.Format(Constants.TERM_CacheKey, Constants.VOCABULARY_ValidVocabularyId)));
			_logger.LogDebug("<<< Exiting TermController_DeleteTerm_Clears_Term_Cache_On_Valid_Term()");
        }

        [Test]
        public void TermController_GetTerm_Throws_On_Negative_TermId()
        {
			_logger.LogDebug(">>> Entering TermController_GetTerm_Throws_On_Negative_TermId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_GetTerm_Throws_On_Negative_TermId(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_GetTerm_Throws_On_Negative_TermId(): termController → {TermController}", termController);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.GetTerm(Null.NullInteger));
			_logger.LogDebug("<<< Exiting TermController_GetTerm_Throws_On_Negative_TermId()");
        }

        [Test]
        public void TermController_GetTerm_Returns_Null_On_InValidTermId()
        {
			_logger.LogDebug(">>> Entering TermController_GetTerm_Returns_Null_On_InValidTermId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_GetTerm_Returns_Null_On_InValidTermId(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetTerm(Constants.TERM_InValidTermId)).Returns(MockHelper.CreateEmptyTermReader());

            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_GetTerm_Returns_Null_On_InValidTermId(): termController → {TermController}", termController);

            // Act
            Term term = termController.GetTerm(Constants.TERM_InValidTermId);
			_logger.LogDebug("TermController_GetTerm_Returns_Null_On_InValidTermId(): term → {Term}", term);

            // Assert
            Assert.That(term, Is.Null);
			_logger.LogDebug("<<< Exiting TermController_GetTerm_Returns_Null_On_InValidTermId()");
        }

        [Test]
        public void TermController_GetTerm_Calls_DataService()
        {
			_logger.LogDebug(">>> Entering TermController_GetTerm_Calls_DataService()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_GetTerm_Calls_DataService(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetTerm(Constants.TERM_ValidTermId)).Returns(MockHelper.CreateValidTermReader());
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_GetTerm_Calls_DataService(): termController → {TermController}", termController);

            // Act
            Term term = termController.GetTerm(Constants.TERM_ValidTermId);
			_logger.LogDebug("TermController_GetTerm_Calls_DataService(): term → {Term}", term);

            // Assert
            mockDataService.Verify(ds => ds.GetTerm(Constants.TERM_ValidTermId));
			_logger.LogDebug("<<< Exiting TermController_GetTerm_Calls_DataService()");
        }

        [Test]
        public void TermController_GetTerm_Returns_Term_On_Valid_TermId()
        {
			_logger.LogDebug(">>> Entering TermController_GetTerm_Returns_Term_On_Valid_TermId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_GetTerm_Returns_Term_On_Valid_TermId(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetTerm(Constants.TERM_ValidTermId)).Returns(MockHelper.CreateValidTermReader());

            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_GetTerm_Returns_Term_On_Valid_TermId(): termController → {TermController}", termController);

            // Act
            var term = termController.GetTerm(Constants.TERM_ValidTermId);
			_logger.LogDebug("TermController_GetTerm_Returns_Term_On_Valid_TermId(): term → {Term}", term);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(term.TermId, Is.EqualTo(Constants.TERM_ValidTermId));
                Assert.That(term.Name, Is.EqualTo(Constants.TERM_ValidName));
            }
			_logger.LogDebug("<<< Exiting TermController_GetTerm_Returns_Term_On_Valid_TermId()");
        }

        [Test]
        public void TermController_GetTermsByContent_Throws_On_Invalid_ContentItemId()
        {
			_logger.LogDebug(">>> Entering TermController_GetTermsByContent_Throws_On_Invalid_ContentItemId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_GetTermsByContent_Throws_On_Invalid_ContentItemId(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_GetTermsByContent_Throws_On_Invalid_ContentItemId(): termController → {TermController}", termController);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.GetTermsByContent(Null.NullInteger));
			_logger.LogDebug("<<< Exiting TermController_GetTermsByContent_Throws_On_Invalid_ContentItemId()");
        }

        [Test]
        public void TermController_GetTermsByContent_Calls_DataService()
        {
			_logger.LogDebug(">>> Entering TermController_GetTermsByContent_Calls_DataService()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_GetTermsByContent_Calls_DataService(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetTermsByContent(Constants.TERM_ValidContent1)).Returns(MockHelper.CreateValidTermsReader(
                Constants.TERM_ValidCountForContent1,
                v => Constants.TERM_ValidVocabularyId,
                c => Constants.TERM_ValidContent1));
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_GetTermsByContent_Calls_DataService(): termController → {TermController}", termController);

            // Act
            IQueryable<Term> terms = termController.GetTermsByContent(Constants.TERM_ValidContent1);
			_logger.LogDebug("TermController_GetTermsByContent_Calls_DataService(): terms → {Terms}", terms);

            // Assert
            mockDataService.Verify(ds => ds.GetTermsByContent(Constants.TERM_ValidContent1));
			_logger.LogDebug("<<< Exiting TermController_GetTermsByContent_Calls_DataService()");
        }

        [Test]
        public void TermController_GetTermsByContent_Returns_Terms_On_Valid_ContentItemId()
        {
			_logger.LogDebug(">>> Entering TermController_GetTermsByContent_Returns_Terms_On_Valid_ContentItemId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_GetTermsByContent_Returns_Terms_On_Valid_ContentItemId(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetTermsByContent(Constants.TERM_ValidContent1)).Returns(MockHelper.CreateValidTermsReader(
                Constants.TERM_ValidCountForContent1,
                v => Constants.TERM_ValidVocabularyId,
                c => Constants.TERM_ValidContent1));

            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_GetTermsByContent_Returns_Terms_On_Valid_ContentItemId(): termController → {TermController}", termController);

            // Act
            var terms = termController.GetTermsByContent(Constants.TERM_ValidContent1).ToList();
			_logger.LogDebug("TermController_GetTermsByContent_Returns_Terms_On_Valid_ContentItemId(): terms → {Terms}", terms);

            // Assert
            Assert.That(terms, Has.Count.EqualTo(Constants.TERM_ValidCountForContent1));

            for (int i = 0; i < Constants.TERM_ValidCountForContent1; i++)
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(terms[i].TermId, Is.EqualTo(i + Constants.TERM_ValidTermId));
                    Assert.That(terms[i].Name, Is.EqualTo(ContentTestHelper.GetTermName(i + Constants.TERM_ValidTermId)));
                }
            }
			_logger.LogDebug("<<< Exiting TermController_GetTermsByContent_Returns_Terms_On_Valid_ContentItemId()");
        }

        [Test]
        public void TermController_GetTermsByVocabulary_Throws_On_Invalid_VocabularyId()
        {
			_logger.LogDebug(">>> Entering TermController_GetTermsByVocabulary_Throws_On_Invalid_VocabularyId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_GetTermsByVocabulary_Throws_On_Invalid_VocabularyId(): mockDataService → {MockDataService}", mockDataService);
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(string.Empty);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_GetTermsByVocabulary_Throws_On_Invalid_VocabularyId(): termController → {TermController}", termController);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.GetTermsByVocabulary(Null.NullInteger));
			_logger.LogDebug("<<< Exiting TermController_GetTermsByVocabulary_Throws_On_Invalid_VocabularyId()");
        }

        [Test]
        public void TermController_GetTermsByVocabulary_Returns_Terms_On_Valid_VocabularyId()
        {
			_logger.LogDebug(">>> Entering TermController_GetTermsByVocabulary_Returns_Terms_On_Valid_VocabularyId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_GetTermsByVocabulary_Returns_Terms_On_Valid_VocabularyId(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetTermsByVocabulary(Constants.TERM_ValidVocabulary1)).Returns(MockHelper.CreateValidTermsReader(
                Constants.TERM_ValidCountForVocabulary1,
                v => Constants.TERM_ValidVocabulary1,
                c => Constants.TERM_ValidContent1));
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(string.Empty);

            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_GetTermsByVocabulary_Returns_Terms_On_Valid_VocabularyId(): termController → {TermController}", termController);

            // Act
            var terms = termController.GetTermsByVocabulary(Constants.TERM_ValidVocabulary1).ToList();
			_logger.LogDebug("TermController_GetTermsByVocabulary_Returns_Terms_On_Valid_VocabularyId(): terms → {Terms}", terms);

            // Assert
            Assert.That(terms, Has.Count.EqualTo(Constants.TERM_ValidCountForVocabulary1));

            for (int i = 0; i < Constants.TERM_ValidCountForVocabulary1; i++)
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(terms[i].TermId, Is.EqualTo(i + Constants.TERM_ValidTermId));
                    Assert.That(terms[i].Name, Is.EqualTo(ContentTestHelper.GetTermName(i + Constants.TERM_ValidTermId)));
                }
            }
			_logger.LogDebug("<<< Exiting TermController_GetTermsByVocabulary_Returns_Terms_On_Valid_VocabularyId()");
        }

        [Test]
        public void TermController_GetTermsByVocabulary_Throws_On_Invalid_VocabularyName()
        {
			_logger.LogDebug(">>> Entering TermController_GetTermsByVocabulary_Throws_On_Invalid_VocabularyName()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_GetTermsByVocabulary_Throws_On_Invalid_VocabularyName(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_GetTermsByVocabulary_Throws_On_Invalid_VocabularyName(): termController → {TermController}", termController);

            // Act, Arrange
            Assert.Throws<ArgumentException>(() => termController.GetTermsByVocabulary(Null.NullString));
			_logger.LogDebug("<<< Exiting TermController_GetTermsByVocabulary_Throws_On_Invalid_VocabularyName()");
        }

        [Test]
        public void TermController_RemoveTermsFromContent_Throws_On_Null_ContentItem()
        {
			_logger.LogDebug(">>> Entering TermController_RemoveTermsFromContent_Throws_On_Null_ContentItem()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_RemoveTermsFromContent_Throws_On_Null_ContentItem(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_RemoveTermsFromContent_Throws_On_Null_ContentItem(): termController → {TermController}", termController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => termController.RemoveTermsFromContent(null));
			_logger.LogDebug("<<< Exiting TermController_RemoveTermsFromContent_Throws_On_Null_ContentItem()");
        }

        [Test]
        public void TermController_RemoveTermsFromContent_Should_Call_DataService_If_Valid_Params()
        {
			_logger.LogDebug(">>> Entering TermController_RemoveTermsFromContent_Should_Call_DataService_If_Valid_Params()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_RemoveTermsFromContent_Should_Call_DataService_If_Valid_Params(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_RemoveTermsFromContent_Should_Call_DataService_If_Valid_Params(): termController → {TermController}", termController);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
			_logger.LogDebug("TermController_RemoveTermsFromContent_Should_Call_DataService_If_Valid_Params(): content → {Content}", content);

            // Act
            termController.RemoveTermsFromContent(content);

            // Assert
            mockDataService.Verify(ds => ds.RemoveTermsFromContent(content));
			_logger.LogDebug("<<< Exiting TermController_RemoveTermsFromContent_Should_Call_DataService_If_Valid_Params()");
        }

        [Test]
        public void TermController_UpdateTerm_Throws_On_Null_Term()
        {
			_logger.LogDebug(">>> Entering TermController_UpdateTerm_Throws_On_Null_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_UpdateTerm_Throws_On_Null_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_UpdateTerm_Throws_On_Null_Term(): termController → {TermController}", termController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => termController.UpdateTerm(null));
			_logger.LogDebug("<<< Exiting TermController_UpdateTerm_Throws_On_Null_Term()");
        }

        [Test]
        public void TermController_UpdateTerm_Throws_On_Negative_TermId()
        {
			_logger.LogDebug(">>> Entering TermController_UpdateTerm_Throws_On_Negative_TermId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_UpdateTerm_Throws_On_Negative_TermId(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_UpdateTerm_Throws_On_Negative_TermId(): termController → {TermController}", termController);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Null.NullInteger);
			_logger.LogDebug("TermController_UpdateTerm_Throws_On_Negative_TermId(): term → {Term}", term);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.UpdateTerm(term));
			_logger.LogDebug("<<< Exiting TermController_UpdateTerm_Throws_On_Negative_TermId()");
        }

        [Test]
        public void TermController_UpdateTerm_Throws_On_Invalid_Term()
        {
			_logger.LogDebug(">>> Entering TermController_UpdateTerm_Throws_On_Invalid_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_UpdateTerm_Throws_On_Invalid_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_UpdateTerm_Throws_On_Invalid_Term(): termController → {TermController}", termController);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
			_logger.LogDebug("TermController_UpdateTerm_Throws_On_Invalid_Term(): term → {Term}", term);
			_logger.LogDebug("TermController_UpdateTerm_Throws_On_Invalid_Term(): term.Name → {Arg0}", term.Name);
            term.Name = Constants.TERM_InValidName;

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.UpdateTerm(term));
			_logger.LogDebug("<<< Exiting TermController_UpdateTerm_Throws_On_Invalid_Term()");
        }

        [Test]
        public void TermController_UpdateTerm_Throws_On_Negative_VocabularyId()
        {
			_logger.LogDebug(">>> Entering TermController_UpdateTerm_Throws_On_Negative_VocabularyId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_UpdateTerm_Throws_On_Negative_VocabularyId(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_UpdateTerm_Throws_On_Negative_VocabularyId(): termController → {TermController}", termController);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Null.NullInteger);
			_logger.LogDebug("TermController_UpdateTerm_Throws_On_Negative_VocabularyId(): term → {Term}", term);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.UpdateTerm(term));
			_logger.LogDebug("<<< Exiting TermController_UpdateTerm_Throws_On_Negative_VocabularyId()");
        }

        [Test]
        public void TermController_UpdateTerm_Should_Call_DataService_UpdateSimpleTerm_If_Term_Is_Simple_Term()
        {
			_logger.LogDebug(">>> Entering TermController_UpdateTerm_Should_Call_DataService_UpdateSimpleTerm_If_Term_Is_Simple_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_UpdateTerm_Should_Call_DataService_UpdateSimpleTerm_If_Term_Is_Simple_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_UpdateTerm_Should_Call_DataService_UpdateSimpleTerm_If_Term_Is_Simple_Term(): termController → {TermController}", termController);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
			_logger.LogDebug("TermController_UpdateTerm_Should_Call_DataService_UpdateSimpleTerm_If_Term_Is_Simple_Term(): term → {Term}", term);
			_logger.LogDebug("TermController_UpdateTerm_Should_Call_DataService_UpdateSimpleTerm_If_Term_Is_Simple_Term(): term.TermId → {Arg0}", term.TermId);
			_logger.LogDebug("TermController_UpdateTerm_Should_Call_DataService_UpdateSimpleTerm_If_Term_Is_Simple_Term(): term.Name → {Arg0}", term.Name);
			_logger.LogDebug("TermController_UpdateTerm_Should_Call_DataService_UpdateSimpleTerm_If_Term_Is_Simple_Term(): term.Weight → {Arg0}", term.Weight);
            term.TermId = Constants.TERM_UpdateTermId;
            term.Name = Constants.TERM_UpdateName;
            term.Weight = Constants.TERM_UpdateWeight;

            // Act
            termController.UpdateTerm(term);

            // Assert
            mockDataService.Verify(ds => ds.UpdateSimpleTerm(term, UserController.Instance.GetCurrentUserInfo().UserID));
			_logger.LogDebug("<<< Exiting TermController_UpdateTerm_Should_Call_DataService_UpdateSimpleTerm_If_Term_Is_Simple_Term()");
        }

        [Test]
        public void TermController_UpdateTerm_Should_Call_DataService_UpdateHeirarchicalTerm_If_Term_Is_Heirarchical_Term()
        {
			_logger.LogDebug(">>> Entering TermController_UpdateTerm_Should_Call_DataService_UpdateHeirarchicalTerm_If_Term_Is_Heirarchical_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_UpdateTerm_Should_Call_DataService_UpdateHeirarchicalTerm_If_Term_Is_Heirarchical_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_UpdateTerm_Should_Call_DataService_UpdateHeirarchicalTerm_If_Term_Is_Heirarchical_Term(): termController → {TermController}", termController);

            Term term = ContentTestHelper.CreateValidHeirarchicalTerm(Constants.VOCABULARY_HierarchyVocabularyId, Constants.TERM_ValidParentTermId);
			_logger.LogDebug("TermController_UpdateTerm_Should_Call_DataService_UpdateHeirarchicalTerm_If_Term_Is_Heirarchical_Term(): term → {Term}", term);
			_logger.LogDebug("TermController_UpdateTerm_Should_Call_DataService_UpdateHeirarchicalTerm_If_Term_Is_Heirarchical_Term(): term.TermId → {Arg0}", term.TermId);
			_logger.LogDebug("TermController_UpdateTerm_Should_Call_DataService_UpdateHeirarchicalTerm_If_Term_Is_Heirarchical_Term(): term.Name → {Arg0}", term.Name);
			_logger.LogDebug("TermController_UpdateTerm_Should_Call_DataService_UpdateHeirarchicalTerm_If_Term_Is_Heirarchical_Term(): term.Weight → {Arg0}", term.Weight);
            term.TermId = Constants.TERM_UpdateTermId;
            term.Name = Constants.TERM_UpdateName;
            term.Weight = Constants.TERM_UpdateWeight;

            // Act
            termController.UpdateTerm(term);

            // Assert
            mockDataService.Verify(ds => ds.UpdateHeirarchicalTerm(term, UserController.Instance.GetCurrentUserInfo().UserID));
			_logger.LogDebug("<<< Exiting TermController_UpdateTerm_Should_Call_DataService_UpdateHeirarchicalTerm_If_Term_Is_Heirarchical_Term()");
        }

        [Test]
        public void TermController_UpdateTerm_Clears_Term_Cache_On_Valid_Term()
        {
			_logger.LogDebug(">>> Entering TermController_UpdateTerm_Clears_Term_Cache_On_Valid_Term()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("TermController_UpdateTerm_Clears_Term_Cache_On_Valid_Term(): mockDataService → {MockDataService}", mockDataService);
            var termController = new TermController(mockDataService.Object);
			_logger.LogDebug("TermController_UpdateTerm_Clears_Term_Cache_On_Valid_Term(): termController → {TermController}", termController);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
			_logger.LogDebug("TermController_UpdateTerm_Clears_Term_Cache_On_Valid_Term(): term → {Term}", term);
			_logger.LogDebug("TermController_UpdateTerm_Clears_Term_Cache_On_Valid_Term(): term.TermId → {Arg0}", term.TermId);
			_logger.LogDebug("TermController_UpdateTerm_Clears_Term_Cache_On_Valid_Term(): term.Name → {Arg0}", term.Name);
			_logger.LogDebug("TermController_UpdateTerm_Clears_Term_Cache_On_Valid_Term(): term.Weight → {Arg0}", term.Weight);
            term.TermId = Constants.TERM_UpdateTermId;
            term.Name = Constants.TERM_UpdateName;
            term.Weight = Constants.TERM_UpdateWeight;

            // Act
            termController.UpdateTerm(term);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(string.Format(Constants.TERM_CacheKey, Constants.VOCABULARY_ValidVocabularyId)));
			_logger.LogDebug("<<< Exiting TermController_UpdateTerm_Clears_Term_Cache_On_Valid_Term()");
        }
    }
}
