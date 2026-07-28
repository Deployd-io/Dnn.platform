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

    /// <summary>  Summary description for VocabularyTests.</summary>
    [TestFixture]
    public class VocabularyControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private Mock<CachingProvider> mockCache;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.mockCache = MockComponentProvider.CreateDataCacheProvider();
            var dataProvider = MockComponentProvider.CreateDataProvider();
			Logger.LogDebug("SetUp(): mockCache → {MockCache}", mockCache);
			Logger.LogDebug("SetUp(): dataProvider → {DataProvider}", dataProvider);
            dataProvider.Setup(c => c.GetProviderPath()).Returns(string.Empty);
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("SetUp(): services → {Services}", services);
                    services.AddSingleton(this.mockCache.Object);
                    services.AddSingleton(dataProvider.Object);
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
        public void VocabularyController_AddVocabulary_Throws_On_Null_Vocabulary()
        {
			Logger.LogDebug(">>> Entering VocabularyController_AddVocabulary_Throws_On_Null_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);
			Logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Null_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Null_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => vocabularyController.AddVocabulary(null));
			Logger.LogDebug("<<< Exiting VocabularyController_AddVocabulary_Throws_On_Null_Vocabulary()");
        }

        [Test]
        public void VocabularyController_AddVocabulary_Throws_On_Invalid_Name()
        {
			Logger.LogDebug(">>> Entering VocabularyController_AddVocabulary_Throws_On_Invalid_Name()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);
			Logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Invalid_Name(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Invalid_Name(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.Name = Constants.VOCABULARY_InValidName;
			Logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Invalid_Name(): vocabulary → {Vocabulary}", vocabulary);
			Logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Invalid_Name(): vocabulary.Name → {Arg0}", vocabulary.Name);

            // Act, Arrange
            Assert.Throws<ArgumentException>(() => vocabularyController.AddVocabulary(vocabulary));
			Logger.LogDebug("<<< Exiting VocabularyController_AddVocabulary_Throws_On_Invalid_Name()");
        }

        [Test]
        public void VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID()
        {
			Logger.LogDebug(">>> Entering VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);
			Logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.ScopeTypeId = Null.NullInteger;
			Logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID(): vocabulary → {Vocabulary}", vocabulary);
			Logger.LogDebug("VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID(): vocabulary.ScopeTypeId → {Arg0}", vocabulary.ScopeTypeId);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.AddVocabulary(vocabulary));
			Logger.LogDebug("<<< Exiting VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID()");
        }

        [Test]
        public void VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments()
        {
			Logger.LogDebug(">>> Entering VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);
			Logger.LogDebug("VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();

			Logger.LogDebug("VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments(): vocabulary → {Vocabulary}", vocabulary);
            // Act
            int vocabularyId = vocabularyController.AddVocabulary(vocabulary);

			Logger.LogDebug("VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments(): vocabularyId → {VocabularyId}", vocabularyId);
            // Assert
            mockDataService.Verify(ds => ds.AddVocabulary(vocabulary, It.IsAny<int>()));
			Logger.LogDebug("<<< Exiting VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments()");
        }

        [Test]
        public void VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary()
        {
			Logger.LogDebug(">>> Entering VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddVocabulary(It.IsAny<Vocabulary>(), It.IsAny<int>())).Returns(Constants.VOCABULARY_AddVocabularyId);
			Logger.LogDebug("VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);

			Logger.LogDebug("VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);
            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();

			Logger.LogDebug("VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary(): vocabulary → {Vocabulary}", vocabulary);
            // Act
            int vocabularyId = vocabularyController.AddVocabulary(vocabulary);

			Logger.LogDebug("VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary(): vocabularyId → {VocabularyId}", vocabularyId);
            // Assert
            Assert.That(vocabularyId, Is.EqualTo(Constants.VOCABULARY_AddVocabularyId));
			Logger.LogDebug("<<< Exiting VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary()");
        }

        [Test]
        public void VocabularyController_AddVocabulary_Sets_ValidId_On_Valid_Vocabulary()
        {
			Logger.LogDebug(">>> Entering VocabularyController_AddVocabulary_Sets_ValidId_On_Valid_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddVocabulary(It.IsAny<Vocabulary>(), It.IsAny<int>())).Returns(Constants.VOCABULARY_AddVocabularyId);
			Logger.LogDebug("VocabularyController_AddVocabulary_Sets_ValidId_On_Valid_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);

			Logger.LogDebug("VocabularyController_AddVocabulary_Sets_ValidId_On_Valid_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);
            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();

			Logger.LogDebug("VocabularyController_AddVocabulary_Sets_ValidId_On_Valid_Vocabulary(): vocabulary → {Vocabulary}", vocabulary);
            // Act
            vocabularyController.AddVocabulary(vocabulary);

            // Assert
            Assert.That(vocabulary.VocabularyId, Is.EqualTo(Constants.VOCABULARY_AddVocabularyId));
			Logger.LogDebug("<<< Exiting VocabularyController_AddVocabulary_Sets_ValidId_On_Valid_Vocabulary()");
        }

        [Test]
        public void VocabularyController_AddVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()
        {
			Logger.LogDebug(">>> Entering VocabularyController_AddVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);
			Logger.LogDebug("VocabularyController_AddVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("VocabularyController_AddVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();

			Logger.LogDebug("VocabularyController_AddVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabulary → {Vocabulary}", vocabulary);
            // Act
            vocabularyController.AddVocabulary(vocabulary);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(Constants.VOCABULARY_CacheKey));
			Logger.LogDebug("<<< Exiting VocabularyController_AddVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()");
        }

        [Test]
        public void VocabularyController_DeleteVocabulary_Throws_On_Null_Vocabulary()
        {
			Logger.LogDebug(">>> Entering VocabularyController_DeleteVocabulary_Throws_On_Null_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);
			Logger.LogDebug("VocabularyController_DeleteVocabulary_Throws_On_Null_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("VocabularyController_DeleteVocabulary_Throws_On_Null_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => vocabularyController.DeleteVocabulary(null));
			Logger.LogDebug("<<< Exiting VocabularyController_DeleteVocabulary_Throws_On_Null_Vocabulary()");
        }

        [Test]
        public void VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId()
        {
			Logger.LogDebug(">>> Entering VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);
			Logger.LogDebug("VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = new Vocabulary();
            vocabulary.VocabularyId = Null.NullInteger;
			Logger.LogDebug("VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId(): vocabulary → {Vocabulary}", vocabulary);
			Logger.LogDebug("VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId(): vocabulary.VocabularyId → {Arg0}", vocabulary.VocabularyId);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.DeleteVocabulary(vocabulary));
			Logger.LogDebug("<<< Exiting VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId()");
        }

        [Test]
        public void VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments()
        {
			Logger.LogDebug(">>> Entering VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);
			Logger.LogDebug("VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.VocabularyId = Constants.VOCABULARY_ValidVocabularyId;
			Logger.LogDebug("VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments(): vocabulary → {Vocabulary}", vocabulary);
			Logger.LogDebug("VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments(): vocabulary.VocabularyId → {Arg0}", vocabulary.VocabularyId);

            // Act
            vocabularyController.DeleteVocabulary(vocabulary);

            // Assert
            mockDataService.Verify(ds => ds.DeleteVocabulary(vocabulary));
			Logger.LogDebug("<<< Exiting VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments()");
        }

        [Test]
        public void VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()
        {
			Logger.LogDebug(">>> Entering VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);
			Logger.LogDebug("VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.VocabularyId = Constants.VOCABULARY_ValidVocabularyId;
			Logger.LogDebug("VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabulary → {Vocabulary}", vocabulary);
			Logger.LogDebug("VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabulary.VocabularyId → {Arg0}", vocabulary.VocabularyId);

            // Act
            vocabularyController.DeleteVocabulary(vocabulary);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(Constants.VOCABULARY_CacheKey));
			Logger.LogDebug("<<< Exiting VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()");
        }

        [Test]
        public void VocabularyController_GetVocabularies_Calls_DataService()
        {
			Logger.LogDebug(">>> Entering VocabularyController_GetVocabularies_Calls_DataService()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetVocabularies()).Returns(MockHelper.CreateValidVocabulariesReader(Constants.VOCABULARY_ValidCount));
			Logger.LogDebug("VocabularyController_GetVocabularies_Calls_DataService(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);

			Logger.LogDebug("VocabularyController_GetVocabularies_Calls_DataService(): vocabularyController → {VocabularyController}", vocabularyController);
            // Act
            IQueryable<Vocabulary> vocabularys = vocabularyController.GetVocabularies();

			Logger.LogDebug("VocabularyController_GetVocabularies_Calls_DataService(): vocabularys → {Vocabularys}", vocabularys);
            // Assert
            mockDataService.Verify(ds => ds.GetVocabularies());
			Logger.LogDebug("<<< Exiting VocabularyController_GetVocabularies_Calls_DataService()");
        }

        [Test]
        public void VocabularyController_GetVocabularies_Returns_List_Of_Vocabularies()
        {
			Logger.LogDebug(">>> Entering VocabularyController_GetVocabularies_Returns_List_Of_Vocabularies()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetVocabularies()).Returns(MockHelper.CreateValidVocabulariesReader(Constants.VOCABULARY_ValidCount));
			Logger.LogDebug("VocabularyController_GetVocabularies_Returns_List_Of_Vocabularies(): mockDataService → {MockDataService}", mockDataService);
            var vocabularyController = new VocabularyController(mockDataService.Object);

			Logger.LogDebug("VocabularyController_GetVocabularies_Returns_List_Of_Vocabularies(): vocabularyController → {VocabularyController}", vocabularyController);
            // Act
            IQueryable<Vocabulary> vocabularys = vocabularyController.GetVocabularies();

			Logger.LogDebug("VocabularyController_GetVocabularies_Returns_List_Of_Vocabularies(): vocabularys → {Vocabularys}", vocabularys);
            // Assert
            Assert.That(vocabularys.Count(), Is.EqualTo(Constants.VOCABULARY_ValidCount));
			Logger.LogDebug("<<< Exiting VocabularyController_GetVocabularies_Returns_List_Of_Vocabularies()");
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Null_Vocabulary()
        {
			Logger.LogDebug(">>> Entering VocabularyController_UpdateVocabulary_Throws_On_Null_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Null_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Null_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => vocabularyController.UpdateVocabulary(null));
			Logger.LogDebug("<<< Exiting VocabularyController_UpdateVocabulary_Throws_On_Null_Vocabulary()");
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId()
        {
			Logger.LogDebug(">>> Entering VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.VocabularyId = Null.NullInteger;
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId(): vocabulary → {Vocabulary}", vocabulary);
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId(): vocabulary.VocabularyId → {Arg0}", vocabulary.VocabularyId);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.UpdateVocabulary(vocabulary));
			Logger.LogDebug("<<< Exiting VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId()");
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name()
        {
			Logger.LogDebug(">>> Entering VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.Name = Constants.VOCABULARY_InValidName;
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name(): vocabulary → {Vocabulary}", vocabulary);
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name(): vocabulary.Name → {Arg0}", vocabulary.Name);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.UpdateVocabulary(vocabulary));
			Logger.LogDebug("<<< Exiting VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name()");
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID()
        {
			Logger.LogDebug(">>> Entering VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.ScopeTypeId = Null.NullInteger;
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID(): vocabulary → {Vocabulary}", vocabulary);
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID(): vocabulary.ScopeTypeId → {Arg0}", vocabulary.ScopeTypeId);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.UpdateVocabulary(vocabulary));
			Logger.LogDebug("<<< Exiting VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID()");
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments()
        {
			Logger.LogDebug(">>> Entering VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.VocabularyId = Constants.VOCABULARY_UpdateVocabularyId;
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments(): vocabulary → {Vocabulary}", vocabulary);
			Logger.LogDebug("VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments(): vocabulary.VocabularyId → {Arg0}", vocabulary.VocabularyId);

            // Act
            vocabularyController.UpdateVocabulary(vocabulary);

            // Assert
            mockDataService.Verify(ds => ds.UpdateVocabulary(vocabulary, It.IsAny<int>()));
			Logger.LogDebug("<<< Exiting VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments()");
        }

        [Test]
        public void VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()
        {
			Logger.LogDebug(">>> Entering VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);
			Logger.LogDebug("VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabularyController → {VocabularyController}", vocabularyController);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.VocabularyId = Constants.VOCABULARY_UpdateVocabularyId;
			Logger.LogDebug("VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabulary → {Vocabulary}", vocabulary);
			Logger.LogDebug("VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary(): vocabulary.VocabularyId → {Arg0}", vocabulary.VocabularyId);

            // Act
            vocabularyController.UpdateVocabulary(vocabulary);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(Constants.VOCABULARY_CacheKey));
			Logger.LogDebug("<<< Exiting VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()");
        }
    }
}
