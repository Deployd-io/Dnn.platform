// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Content
{
    using System;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Content.Mocks;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    /// <summary>  Summary description for VocabularyTests.</summary>
    [TestFixture]
    public class VocabularyControllerTests
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
        public void VocabularyController_AddVocabulary_Throws_On_Null_Vocabulary()
        {
			_logger.LogDebug(">>> Entering VocabularyController_AddVocabulary_Throws_On_Null_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Null_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Null_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => vocabularyController.AddVocabulary(null));
			_logger.LogDebug("<<< Exiting VocabularyController_AddVocabulary_Throws_On_Null_Vocabulary()");
        }

        [Test]
        public void VocabularyController_AddVocabulary_Throws_On_Invalid_Name()
        {
			_logger.LogDebug(">>> Entering VocabularyController_AddVocabulary_Throws_On_Invalid_Name()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Invalid_Name(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Invalid_Name(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
			_logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Invalid_Name(): vocabulary → {Vocabulary}", vocabulary);
			_logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Invalid_Name(): vocabulary.Name → {Arg0}", vocabulary.Name);
            vocabulary.Name = Constants.VOCABULARY_InValidName;

            // Act, Arrange
            Assert.Throws<ArgumentException>(() => vocabularyController.AddVocabulary(vocabulary));
			_logger.LogDebug("<<< Exiting VocabularyController_AddVocabulary_Throws_On_Invalid_Name()");
        }

        [Test]
        public void VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID()
        {
			_logger.LogDebug(">>> Entering VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
			_logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID(): vocabulary → {Vocabulary}", vocabulary);
			_logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID(): vocabulary.ScopeTypeId → {Arg0}", vocabulary.ScopeTypeId);
            vocabulary.ScopeTypeId = Null.NullInteger;

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.AddVocabulary(vocabulary));
			_logger.LogDebug("<<< Exiting VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID()");
        }

        [Test]
        public void VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments()
        {
			_logger.LogDebug(">>> Entering VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
			_logger.LogDebug("VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments(): vocabulary → {Vocabulary}", vocabulary);

            // Act
            int vocabularyId = vocabularyController.AddVocabulary(vocabulary);
			_logger.LogDebug("VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments(): vocabularyId → {VocabularyId}", vocabularyId);

            // Assert
            mockDataService.Verify(ds => ds.AddVocabulary(vocabulary, It.IsAny<int>()));
			_logger.LogDebug("<<< Exiting VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments()");
        }

        [Test]
        public void VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary()
        {
			_logger.LogDebug(">>> Entering VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.AddVocabulary(It.IsAny<Vocabulary>(), It.IsAny<int>())).Returns(Constants.VOCABULARY_AddVocabularyId);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
			_logger.LogDebug("VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary(): vocabulary → {Vocabulary}", vocabulary);

            // Act
            int vocabularyId = vocabularyController.AddVocabulary(vocabulary);
			_logger.LogDebug("VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary(): vocabularyId → {VocabularyId}", vocabularyId);

            // Assert
            Assert.That(vocabularyId, Is.EqualTo(Constants.VOCABULARY_AddVocabularyId));
			_logger.LogDebug("<<< Exiting VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary()");
        }

        [Test]
        public void VocabularyController_AddVocabulary_Sets_ValidId_On_Valid_Vocabulary()
        {
			_logger.LogDebug(">>> Entering VocabularyController_AddVocabulary_Sets_ValidId_On_Valid_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_AddVocabulary_Sets_ValidId_On_Valid_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.AddVocabulary(It.IsAny<Vocabulary>(), It.IsAny<int>())).Returns(Constants.VOCABULARY_AddVocabularyId);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_AddVocabulary_Sets_ValidId_On_Valid_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
			_logger.LogDebug("VocabularyController_AddVocabulary_Sets_ValidId_On_Valid_Vocabulary(): vocabulary → {Vocabulary}", vocabulary);

            // Act
            vocabularyController.AddVocabulary(vocabulary);

            // Assert
            Assert.That(vocabulary.VocabularyId, Is.EqualTo(Constants.VOCABULARY_AddVocabularyId));
			_logger.LogDebug("<<< Exiting VocabularyController_AddVocabulary_Sets_ValidId_On_Valid_Vocabulary()");
        }

        [Test]
        public void VocabularyController_AddVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()
        {
			_logger.LogDebug(">>> Entering VocabularyController_AddVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_AddVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_AddVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
			_logger.LogDebug("VocabularyController_AddVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabulary → {Vocabulary}", vocabulary);

            // Act
            vocabularyController.AddVocabulary(vocabulary);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(Constants.VOCABULARY_CacheKey));
			_logger.LogDebug("<<< Exiting VocabularyController_AddVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()");
        }

        [Test]
        public void VocabularyController_DeleteVocabulary_Throws_On_Null_Vocabulary()
        {
			_logger.LogDebug(">>> Entering VocabularyController_DeleteVocabulary_Throws_On_Null_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_DeleteVocabulary_Throws_On_Null_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_DeleteVocabulary_Throws_On_Null_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => vocabularyController.DeleteVocabulary(null));
			_logger.LogDebug("<<< Exiting VocabularyController_DeleteVocabulary_Throws_On_Null_Vocabulary()");
        }

        [Test]
        public void VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId()
        {
			_logger.LogDebug(">>> Entering VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = new Vocabulary();
			_logger.LogDebug("VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId(): vocabulary → {Vocabulary}", vocabulary);
			_logger.LogDebug("VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId(): vocabulary.VocabularyId → {Arg0}", vocabulary.VocabularyId);
            vocabulary.VocabularyId = Null.NullInteger;

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.DeleteVocabulary(vocabulary));
			_logger.LogDebug("<<< Exiting VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId()");
        }

        [Test]
        public void VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments()
        {
			_logger.LogDebug(">>> Entering VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
			_logger.LogDebug("VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments(): vocabulary → {Vocabulary}", vocabulary);
			_logger.LogDebug("VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments(): vocabulary.VocabularyId → {Arg0}", vocabulary.VocabularyId);
            vocabulary.VocabularyId = Constants.VOCABULARY_ValidVocabularyId;

            // Act
            vocabularyController.DeleteVocabulary(vocabulary);

            // Assert
            mockDataService.Verify(ds => ds.DeleteVocabulary(vocabulary));
			_logger.LogDebug("<<< Exiting VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments()");
        }

        [Test]
        public void VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()
        {
			_logger.LogDebug(">>> Entering VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
			_logger.LogDebug("VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabulary → {Vocabulary}", vocabulary);
			_logger.LogDebug("VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabulary.VocabularyId → {Arg0}", vocabulary.VocabularyId);
            vocabulary.VocabularyId = Constants.VOCABULARY_ValidVocabularyId;

            // Act
            vocabularyController.DeleteVocabulary(vocabulary);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(Constants.VOCABULARY_CacheKey));
			_logger.LogDebug("<<< Exiting VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()");
        }

        [Test]
        public void VocabularyController_GetVocabularies_Calls_DataService()
        {
			_logger.LogDebug(">>> Entering VocabularyController_GetVocabularies_Calls_DataService()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_GetVocabularies_Calls_DataService(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetVocabularies()).Returns(MockHelper.CreateValidVocabulariesReader(Constants.VOCABULARY_ValidCount));
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_GetVocabularies_Calls_DataService(): vocabularyController → {VocabularyController}", vocabularyController);

            // Act
            IQueryable<Vocabulary> vocabularys = vocabularyController.GetVocabularies();
			_logger.LogDebug("VocabularyController_GetVocabularies_Calls_DataService(): vocabularys → {Vocabularys}", vocabularys);

            // Assert
            mockDataService.Verify(ds => ds.GetVocabularies());
			_logger.LogDebug("<<< Exiting VocabularyController_GetVocabularies_Calls_DataService()");
        }

        [Test]
        public void VocabularyController_GetVocabularies_Returns_List_Of_Vocabularies()
        {
			_logger.LogDebug(">>> Entering VocabularyController_GetVocabularies_Returns_List_Of_Vocabularies()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_GetVocabularies_Returns_List_Of_Vocabularies(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetVocabularies()).Returns(MockHelper.CreateValidVocabulariesReader(Constants.VOCABULARY_ValidCount));
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_GetVocabularies_Returns_List_Of_Vocabularies(): vocabularyController → {VocabularyController}", vocabularyController);

            // Act
            IQueryable<Vocabulary> vocabularys = vocabularyController.GetVocabularies();
			_logger.LogDebug("VocabularyController_GetVocabularies_Returns_List_Of_Vocabularies(): vocabularys → {Vocabularys}", vocabularys);

            // Assert
            Assert.That(vocabularys.Count(), Is.EqualTo(Constants.VOCABULARY_ValidCount));
			_logger.LogDebug("<<< Exiting VocabularyController_GetVocabularies_Returns_List_Of_Vocabularies()");
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Null_Vocabulary()
        {
			_logger.LogDebug(">>> Entering VocabularyController_UpdateVocabulary_Throws_On_Null_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Null_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Null_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => vocabularyController.UpdateVocabulary(null));
			_logger.LogDebug("<<< Exiting VocabularyController_UpdateVocabulary_Throws_On_Null_Vocabulary()");
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId()
        {
			_logger.LogDebug(">>> Entering VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId(): vocabulary → {Vocabulary}", vocabulary);
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId(): vocabulary.VocabularyId → {Arg0}", vocabulary.VocabularyId);
            vocabulary.VocabularyId = Null.NullInteger;

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.UpdateVocabulary(vocabulary));
			_logger.LogDebug("<<< Exiting VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId()");
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name()
        {
			_logger.LogDebug(">>> Entering VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name(): vocabulary → {Vocabulary}", vocabulary);
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name(): vocabulary.Name → {Arg0}", vocabulary.Name);
            vocabulary.Name = Constants.VOCABULARY_InValidName;

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.UpdateVocabulary(vocabulary));
			_logger.LogDebug("<<< Exiting VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name()");
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID()
        {
			_logger.LogDebug(">>> Entering VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID(): vocabulary → {Vocabulary}", vocabulary);
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID(): vocabulary.ScopeTypeId → {Arg0}", vocabulary.ScopeTypeId);
            vocabulary.ScopeTypeId = Null.NullInteger;

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.UpdateVocabulary(vocabulary));
			_logger.LogDebug("<<< Exiting VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID()");
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments()
        {
			_logger.LogDebug(">>> Entering VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments(): vocabulary → {Vocabulary}", vocabulary);
			_logger.LogDebug("VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments(): vocabulary.VocabularyId → {Arg0}", vocabulary.VocabularyId);
            vocabulary.VocabularyId = Constants.VOCABULARY_UpdateVocabularyId;

            // Act
            vocabularyController.UpdateVocabulary(vocabulary);

            // Assert
            mockDataService.Verify(ds => ds.UpdateVocabulary(vocabulary, It.IsAny<int>()));
			_logger.LogDebug("<<< Exiting VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments()");
        }

        [Test]
        public void VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()
        {
			_logger.LogDebug(">>> Entering VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);
			_logger.LogDebug("VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
			_logger.LogDebug("VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabulary → {Vocabulary}", vocabulary);
			_logger.LogDebug("VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabulary.VocabularyId → {Arg0}", vocabulary.VocabularyId);
            vocabulary.VocabularyId = Constants.VOCABULARY_UpdateVocabularyId;

            // Act
            vocabularyController.UpdateVocabulary(vocabulary);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(Constants.VOCABULARY_CacheKey));
			_logger.LogDebug("<<< Exiting VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()");
        }
    }
}
