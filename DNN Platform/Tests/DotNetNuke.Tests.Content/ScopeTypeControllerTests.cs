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

    /// <summary>  Summary description for ScopeTypeTests.</summary>
    [TestFixture]
    public class ScopeTypeControllerTests
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
        public void ScopeTypeController_AddScopeType_Throws_On_Null_ScopeType()
        {
			Logger.LogDebug(">>> Entering ScopeTypeController_AddScopeType_Throws_On_Null_ScopeType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			Logger.LogDebug("ScopeTypeController_AddScopeType_Throws_On_Null_ScopeType(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ScopeTypeController_AddScopeType_Throws_On_Null_ScopeType(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => scopeTypeController.AddScopeType(null));
			Logger.LogDebug("<<< Exiting ScopeTypeController_AddScopeType_Throws_On_Null_ScopeType()");
        }

        [Test]
        public void ScopeTypeController_AddScopeType_Calls_DataService_On_Valid_Arguments()
        {
			Logger.LogDebug(">>> Entering ScopeTypeController_AddScopeType_Calls_DataService_On_Valid_Arguments()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			Logger.LogDebug("ScopeTypeController_AddScopeType_Calls_DataService_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ScopeTypeController_AddScopeType_Calls_DataService_On_Valid_Arguments(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            var scopeType = ContentTestHelper.CreateValidScopeType();

			Logger.LogDebug("ScopeTypeController_AddScopeType_Calls_DataService_On_Valid_Arguments(): scopeType → {ScopeType}", scopeType);
            // Act
            int scopeTypeId = scopeTypeController.AddScopeType(scopeType);

			Logger.LogDebug("ScopeTypeController_AddScopeType_Calls_DataService_On_Valid_Arguments(): scopeTypeId → {ScopeTypeId}", scopeTypeId);
            // Assert
            mockDataService.Verify(ds => ds.AddScopeType(scopeType));
			Logger.LogDebug("<<< Exiting ScopeTypeController_AddScopeType_Calls_DataService_On_Valid_Arguments()");
        }

        [Test]
        public void ScopeTypeController_AddScopeType_Returns_ValidId_On_Valid_ScopeType()
        {
			Logger.LogDebug(">>> Entering ScopeTypeController_AddScopeType_Returns_ValidId_On_Valid_ScopeType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddScopeType(It.IsAny<ScopeType>())).Returns(Constants.SCOPETYPE_AddScopeTypeId);
			Logger.LogDebug("ScopeTypeController_AddScopeType_Returns_ValidId_On_Valid_ScopeType(): mockDataService → {MockDataService}", mockDataService);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
            ScopeType scopeType = ContentTestHelper.CreateValidScopeType();
			Logger.LogDebug("ScopeTypeController_AddScopeType_Returns_ValidId_On_Valid_ScopeType(): scopeTypeController → {ScopeTypeController}", scopeTypeController);
			Logger.LogDebug("ScopeTypeController_AddScopeType_Returns_ValidId_On_Valid_ScopeType(): scopeType → {ScopeType}", scopeType);

            // Act
            int scopeTypeId = scopeTypeController.AddScopeType(scopeType);

			Logger.LogDebug("ScopeTypeController_AddScopeType_Returns_ValidId_On_Valid_ScopeType(): scopeTypeId → {ScopeTypeId}", scopeTypeId);
            // Assert
            Assert.That(scopeTypeId, Is.EqualTo(Constants.SCOPETYPE_AddScopeTypeId));
			Logger.LogDebug("<<< Exiting ScopeTypeController_AddScopeType_Returns_ValidId_On_Valid_ScopeType()");
        }

        [Test]
        public void ScopeTypeController_AddScopeType_Sets_ValidId_On_Valid_ScopeType()
        {
			Logger.LogDebug(">>> Entering ScopeTypeController_AddScopeType_Sets_ValidId_On_Valid_ScopeType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddScopeType(It.IsAny<ScopeType>())).Returns(Constants.SCOPETYPE_AddScopeTypeId);
			Logger.LogDebug("ScopeTypeController_AddScopeType_Sets_ValidId_On_Valid_ScopeType(): mockDataService → {MockDataService}", mockDataService);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
            var scopeType = ContentTestHelper.CreateValidScopeType();
			Logger.LogDebug("ScopeTypeController_AddScopeType_Sets_ValidId_On_Valid_ScopeType(): scopeTypeController → {ScopeTypeController}", scopeTypeController);
			Logger.LogDebug("ScopeTypeController_AddScopeType_Sets_ValidId_On_Valid_ScopeType(): scopeType → {ScopeType}", scopeType);

            // Act
            scopeTypeController.AddScopeType(scopeType);

            // Assert
            Assert.That(scopeType.ScopeTypeId, Is.EqualTo(Constants.SCOPETYPE_AddScopeTypeId));
			Logger.LogDebug("<<< Exiting ScopeTypeController_AddScopeType_Sets_ValidId_On_Valid_ScopeType()");
        }

        [Test]
        public void ScopeTypeController_DeleteScopeType_Throws_On_Null_ScopeType()
        {
			Logger.LogDebug(">>> Entering ScopeTypeController_DeleteScopeType_Throws_On_Null_ScopeType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			Logger.LogDebug("ScopeTypeController_DeleteScopeType_Throws_On_Null_ScopeType(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ScopeTypeController_DeleteScopeType_Throws_On_Null_ScopeType(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => scopeTypeController.DeleteScopeType(null));
			Logger.LogDebug("<<< Exiting ScopeTypeController_DeleteScopeType_Throws_On_Null_ScopeType()");
        }

        [Test]
        public void ScopeTypeController_DeleteScopeType_Throws_On_Negative_ScopeTypeId()
        {
			Logger.LogDebug(">>> Entering ScopeTypeController_DeleteScopeType_Throws_On_Negative_ScopeTypeId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			Logger.LogDebug("ScopeTypeController_DeleteScopeType_Throws_On_Negative_ScopeTypeId(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ScopeTypeController_DeleteScopeType_Throws_On_Negative_ScopeTypeId(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            ScopeType scopeType = ContentTestHelper.CreateValidScopeType();
            scopeType.ScopeTypeId = Null.NullInteger;
			Logger.LogDebug("ScopeTypeController_DeleteScopeType_Throws_On_Negative_ScopeTypeId(): scopeType → {ScopeType}", scopeType);
			Logger.LogDebug("ScopeTypeController_DeleteScopeType_Throws_On_Negative_ScopeTypeId(): scopeType.ScopeTypeId → {Arg0}", scopeType.ScopeTypeId);

            Assert.Throws<ArgumentOutOfRangeException>(() => scopeTypeController.DeleteScopeType(scopeType));
			Logger.LogDebug("<<< Exiting ScopeTypeController_DeleteScopeType_Throws_On_Negative_ScopeTypeId()");
        }

        [Test]
        public void ScopeTypeController_DeleteScopeType_Calls_DataService_On_Valid_ContentTypeId()
        {
			Logger.LogDebug(">>> Entering ScopeTypeController_DeleteScopeType_Calls_DataService_On_Valid_ContentTypeId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			Logger.LogDebug("ScopeTypeController_DeleteScopeType_Calls_DataService_On_Valid_ContentTypeId(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ScopeTypeController_DeleteScopeType_Calls_DataService_On_Valid_ContentTypeId(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            var scopeType = ContentTestHelper.CreateValidScopeType();
            scopeType.ScopeTypeId = Constants.SCOPETYPE_ValidScopeTypeId;
			Logger.LogDebug("ScopeTypeController_DeleteScopeType_Calls_DataService_On_Valid_ContentTypeId(): scopeType → {ScopeType}", scopeType);
			Logger.LogDebug("ScopeTypeController_DeleteScopeType_Calls_DataService_On_Valid_ContentTypeId(): scopeType.ScopeTypeId → {Arg0}", scopeType.ScopeTypeId);

            // Act
            scopeTypeController.DeleteScopeType(scopeType);

            // Assert
            mockDataService.Verify(ds => ds.DeleteScopeType(scopeType));
			Logger.LogDebug("<<< Exiting ScopeTypeController_DeleteScopeType_Calls_DataService_On_Valid_ContentTypeId()");
        }

        [Test]
        public void ScopeTypeController_GetScopeTypes_Calls_DataService()
        {
			Logger.LogDebug(">>> Entering ScopeTypeController_GetScopeTypes_Calls_DataService()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetScopeTypes()).Returns(MockHelper.CreateValidScopeTypesReader(Constants.SCOPETYPE_ValidScopeTypeCount));
			Logger.LogDebug("ScopeTypeController_GetScopeTypes_Calls_DataService(): mockDataService → {MockDataService}", mockDataService);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);

			Logger.LogDebug("ScopeTypeController_GetScopeTypes_Calls_DataService(): scopeTypeController → {ScopeTypeController}", scopeTypeController);
            // Act
            IQueryable<ScopeType> scopeTypes = scopeTypeController.GetScopeTypes();

			Logger.LogDebug("ScopeTypeController_GetScopeTypes_Calls_DataService(): scopeTypes → {ScopeTypes}", scopeTypes);
            // Assert
            mockDataService.Verify(ds => ds.GetScopeTypes());
			Logger.LogDebug("<<< Exiting ScopeTypeController_GetScopeTypes_Calls_DataService()");
        }

        [Test]
        public void ScopeTypeController_GetScopeTypes_Returns_Empty_List_Of_ScopeTypes_If_No_ScopeTypes()
        {
			Logger.LogDebug(">>> Entering ScopeTypeController_GetScopeTypes_Returns_Empty_List_Of_ScopeTypes_If_No_ScopeTypes()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetScopeTypes()).Returns(MockHelper.CreateEmptyScopeTypeReader());
			Logger.LogDebug("ScopeTypeController_GetScopeTypes_Returns_Empty_List_Of_ScopeTypes_If_No_ScopeTypes(): mockDataService → {MockDataService}", mockDataService);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);

			Logger.LogDebug("ScopeTypeController_GetScopeTypes_Returns_Empty_List_Of_ScopeTypes_If_No_ScopeTypes(): scopeTypeController → {ScopeTypeController}", scopeTypeController);
            // Act
            var scopeTypes = scopeTypeController.GetScopeTypes();

			Logger.LogDebug("ScopeTypeController_GetScopeTypes_Returns_Empty_List_Of_ScopeTypes_If_No_ScopeTypes(): scopeTypes → {ScopeTypes}", scopeTypes);
            // Assert
            Assert.That(scopeTypes, Is.Not.Null);
            Assert.That(scopeTypes.Count(), Is.EqualTo(0));
			Logger.LogDebug("<<< Exiting ScopeTypeController_GetScopeTypes_Returns_Empty_List_Of_ScopeTypes_If_No_ScopeTypes()");
        }

        [Test]
        public void ScopeTypeController_GetScopeTypes_Returns_List_Of_ScopeTypes()
        {
			Logger.LogDebug(">>> Entering ScopeTypeController_GetScopeTypes_Returns_List_Of_ScopeTypes()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetScopeTypes()).Returns(MockHelper.CreateValidScopeTypesReader(Constants.SCOPETYPE_ValidScopeTypeCount));
			Logger.LogDebug("ScopeTypeController_GetScopeTypes_Returns_List_Of_ScopeTypes(): mockDataService → {MockDataService}", mockDataService);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);

			Logger.LogDebug("ScopeTypeController_GetScopeTypes_Returns_List_Of_ScopeTypes(): scopeTypeController → {ScopeTypeController}", scopeTypeController);
            // Act
            var scopeTypes = scopeTypeController.GetScopeTypes();

			Logger.LogDebug("ScopeTypeController_GetScopeTypes_Returns_List_Of_ScopeTypes(): scopeTypes → {ScopeTypes}", scopeTypes);
            // Assert
            Assert.That(scopeTypes.Count(), Is.EqualTo(Constants.SCOPETYPE_ValidScopeTypeCount));
			Logger.LogDebug("<<< Exiting ScopeTypeController_GetScopeTypes_Returns_List_Of_ScopeTypes()");
        }

        [Test]
        public void ScopeTypeController_UpdateScopeType_Throws_On_Null_ScopeType()
        {
			Logger.LogDebug(">>> Entering ScopeTypeController_UpdateScopeType_Throws_On_Null_ScopeType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			Logger.LogDebug("ScopeTypeController_UpdateScopeType_Throws_On_Null_ScopeType(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ScopeTypeController_UpdateScopeType_Throws_On_Null_ScopeType(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => scopeTypeController.UpdateScopeType(null));
			Logger.LogDebug("<<< Exiting ScopeTypeController_UpdateScopeType_Throws_On_Null_ScopeType()");
        }

        [Test]
        public void ScopeTypeController_UpdateScopeType_Throws_On_Negative_ScopeTypeId()
        {
			Logger.LogDebug(">>> Entering ScopeTypeController_UpdateScopeType_Throws_On_Negative_ScopeTypeId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			Logger.LogDebug("ScopeTypeController_UpdateScopeType_Throws_On_Negative_ScopeTypeId(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ScopeTypeController_UpdateScopeType_Throws_On_Negative_ScopeTypeId(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            ScopeType scopeType = ContentTestHelper.CreateValidScopeType();
            scopeType.ScopeType = Constants.SCOPETYPE_InValidScopeType;
			Logger.LogDebug("ScopeTypeController_UpdateScopeType_Throws_On_Negative_ScopeTypeId(): scopeType → {ScopeType}", scopeType);
			Logger.LogDebug("ScopeTypeController_UpdateScopeType_Throws_On_Negative_ScopeTypeId(): scopeType.ScopeType → {Arg0}", scopeType.ScopeType);

            Assert.Throws<ArgumentOutOfRangeException>(() => scopeTypeController.UpdateScopeType(scopeType));
			Logger.LogDebug("<<< Exiting ScopeTypeController_UpdateScopeType_Throws_On_Negative_ScopeTypeId()");
        }

        [Test]
        public void ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType()
        {
			Logger.LogDebug(">>> Entering ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			Logger.LogDebug("ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType(): mockDataService → {MockDataService}", mockDataService);
			Logger.LogDebug("ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            ScopeType scopeType = ContentTestHelper.CreateValidScopeType();
            scopeType.ScopeTypeId = Constants.SCOPETYPE_UpdateScopeTypeId;
			Logger.LogDebug("ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType(): scopeType → {ScopeType}", scopeType);
			Logger.LogDebug("ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType(): scopeType.ScopeTypeId → {Arg0}", scopeType.ScopeTypeId);
			Logger.LogDebug("ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType(): scopeType.ScopeType → {Arg0}", scopeType.ScopeType);
            scopeType.ScopeType = Constants.SCOPETYPE_UpdateScopeType;

            // Act
            scopeTypeController.UpdateScopeType(scopeType);

            // Assert
            mockDataService.Verify(ds => ds.UpdateScopeType(scopeType));
			Logger.LogDebug("<<< Exiting ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType()");
        }
    }
}
