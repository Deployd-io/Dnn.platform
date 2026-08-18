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

    /// <summary>  Summary description for ScopeTypeTests.</summary>
    [TestFixture]
    public class ScopeTypeControllerTests
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
			_logger.LogDebug("<<< Exiting SetUp()");
                });

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
        public void ScopeTypeController_AddScopeType_Throws_On_Null_ScopeType()
        {
			_logger.LogDebug(">>> Entering ScopeTypeController_AddScopeType_Throws_On_Null_ScopeType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ScopeTypeController_AddScopeType_Throws_On_Null_ScopeType(): mockDataService → {MockDataService}", mockDataService);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			_logger.LogDebug("ScopeTypeController_AddScopeType_Throws_On_Null_ScopeType(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => scopeTypeController.AddScopeType(null));
			_logger.LogDebug("<<< Exiting ScopeTypeController_AddScopeType_Throws_On_Null_ScopeType()");
        }

        [Test]
        public void ScopeTypeController_AddScopeType_Calls_DataService_On_Valid_Arguments()
        {
			_logger.LogDebug(">>> Entering ScopeTypeController_AddScopeType_Calls_DataService_On_Valid_Arguments()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ScopeTypeController_AddScopeType_Calls_DataService_On_Valid_Arguments(): mockDataService → {MockDataService}", mockDataService);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			_logger.LogDebug("ScopeTypeController_AddScopeType_Calls_DataService_On_Valid_Arguments(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            var scopeType = ContentTestHelper.CreateValidScopeType();
			_logger.LogDebug("ScopeTypeController_AddScopeType_Calls_DataService_On_Valid_Arguments(): scopeType → {ScopeType}", scopeType);

            // Act
            int scopeTypeId = scopeTypeController.AddScopeType(scopeType);
			_logger.LogDebug("ScopeTypeController_AddScopeType_Calls_DataService_On_Valid_Arguments(): scopeTypeId → {ScopeTypeId}", scopeTypeId);

            // Assert
            mockDataService.Verify(ds => ds.AddScopeType(scopeType));
			_logger.LogDebug("<<< Exiting ScopeTypeController_AddScopeType_Calls_DataService_On_Valid_Arguments()");
        }

        [Test]
        public void ScopeTypeController_AddScopeType_Returns_ValidId_On_Valid_ScopeType()
        {
			_logger.LogDebug(">>> Entering ScopeTypeController_AddScopeType_Returns_ValidId_On_Valid_ScopeType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ScopeTypeController_AddScopeType_Returns_ValidId_On_Valid_ScopeType(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.AddScopeType(It.IsAny<ScopeType>())).Returns(Constants.SCOPETYPE_AddScopeTypeId);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			_logger.LogDebug("ScopeTypeController_AddScopeType_Returns_ValidId_On_Valid_ScopeType(): scopeTypeController → {ScopeTypeController}", scopeTypeController);
            ScopeType scopeType = ContentTestHelper.CreateValidScopeType();
			_logger.LogDebug("ScopeTypeController_AddScopeType_Returns_ValidId_On_Valid_ScopeType(): scopeType → {ScopeType}", scopeType);

            // Act
            int scopeTypeId = scopeTypeController.AddScopeType(scopeType);
			_logger.LogDebug("ScopeTypeController_AddScopeType_Returns_ValidId_On_Valid_ScopeType(): scopeTypeId → {ScopeTypeId}", scopeTypeId);

            // Assert
            Assert.That(scopeTypeId, Is.EqualTo(Constants.SCOPETYPE_AddScopeTypeId));
			_logger.LogDebug("<<< Exiting ScopeTypeController_AddScopeType_Returns_ValidId_On_Valid_ScopeType()");
        }

        [Test]
        public void ScopeTypeController_AddScopeType_Sets_ValidId_On_Valid_ScopeType()
        {
			_logger.LogDebug(">>> Entering ScopeTypeController_AddScopeType_Sets_ValidId_On_Valid_ScopeType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ScopeTypeController_AddScopeType_Sets_ValidId_On_Valid_ScopeType(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.AddScopeType(It.IsAny<ScopeType>())).Returns(Constants.SCOPETYPE_AddScopeTypeId);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			_logger.LogDebug("ScopeTypeController_AddScopeType_Sets_ValidId_On_Valid_ScopeType(): scopeTypeController → {ScopeTypeController}", scopeTypeController);
            var scopeType = ContentTestHelper.CreateValidScopeType();
			_logger.LogDebug("ScopeTypeController_AddScopeType_Sets_ValidId_On_Valid_ScopeType(): scopeType → {ScopeType}", scopeType);

            // Act
            scopeTypeController.AddScopeType(scopeType);

            // Assert
            Assert.That(scopeType.ScopeTypeId, Is.EqualTo(Constants.SCOPETYPE_AddScopeTypeId));
			_logger.LogDebug("<<< Exiting ScopeTypeController_AddScopeType_Sets_ValidId_On_Valid_ScopeType()");
        }

        [Test]
        public void ScopeTypeController_DeleteScopeType_Throws_On_Null_ScopeType()
        {
			_logger.LogDebug(">>> Entering ScopeTypeController_DeleteScopeType_Throws_On_Null_ScopeType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ScopeTypeController_DeleteScopeType_Throws_On_Null_ScopeType(): mockDataService → {MockDataService}", mockDataService);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			_logger.LogDebug("ScopeTypeController_DeleteScopeType_Throws_On_Null_ScopeType(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => scopeTypeController.DeleteScopeType(null));
			_logger.LogDebug("<<< Exiting ScopeTypeController_DeleteScopeType_Throws_On_Null_ScopeType()");
        }

        [Test]
        public void ScopeTypeController_DeleteScopeType_Throws_On_Negative_ScopeTypeId()
        {
			_logger.LogDebug(">>> Entering ScopeTypeController_DeleteScopeType_Throws_On_Negative_ScopeTypeId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ScopeTypeController_DeleteScopeType_Throws_On_Negative_ScopeTypeId(): mockDataService → {MockDataService}", mockDataService);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			_logger.LogDebug("ScopeTypeController_DeleteScopeType_Throws_On_Negative_ScopeTypeId(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            ScopeType scopeType = ContentTestHelper.CreateValidScopeType();
			_logger.LogDebug("ScopeTypeController_DeleteScopeType_Throws_On_Negative_ScopeTypeId(): scopeType → {ScopeType}", scopeType);
			_logger.LogDebug("ScopeTypeController_DeleteScopeType_Throws_On_Negative_ScopeTypeId(): scopeType.ScopeTypeId → {Arg0}", scopeType.ScopeTypeId);
            scopeType.ScopeTypeId = Null.NullInteger;

            Assert.Throws<ArgumentOutOfRangeException>(() => scopeTypeController.DeleteScopeType(scopeType));
			_logger.LogDebug("<<< Exiting ScopeTypeController_DeleteScopeType_Throws_On_Negative_ScopeTypeId()");
        }

        [Test]
        public void ScopeTypeController_DeleteScopeType_Calls_DataService_On_Valid_ContentTypeId()
        {
			_logger.LogDebug(">>> Entering ScopeTypeController_DeleteScopeType_Calls_DataService_On_Valid_ContentTypeId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ScopeTypeController_DeleteScopeType_Calls_DataService_On_Valid_ContentTypeId(): mockDataService → {MockDataService}", mockDataService);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			_logger.LogDebug("ScopeTypeController_DeleteScopeType_Calls_DataService_On_Valid_ContentTypeId(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            var scopeType = ContentTestHelper.CreateValidScopeType();
			_logger.LogDebug("ScopeTypeController_DeleteScopeType_Calls_DataService_On_Valid_ContentTypeId(): scopeType → {ScopeType}", scopeType);
			_logger.LogDebug("ScopeTypeController_DeleteScopeType_Calls_DataService_On_Valid_ContentTypeId(): scopeType.ScopeTypeId → {Arg0}", scopeType.ScopeTypeId);
            scopeType.ScopeTypeId = Constants.SCOPETYPE_ValidScopeTypeId;

            // Act
            scopeTypeController.DeleteScopeType(scopeType);

            // Assert
            mockDataService.Verify(ds => ds.DeleteScopeType(scopeType));
			_logger.LogDebug("<<< Exiting ScopeTypeController_DeleteScopeType_Calls_DataService_On_Valid_ContentTypeId()");
        }

        [Test]
        public void ScopeTypeController_GetScopeTypes_Calls_DataService()
        {
			_logger.LogDebug(">>> Entering ScopeTypeController_GetScopeTypes_Calls_DataService()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ScopeTypeController_GetScopeTypes_Calls_DataService(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetScopeTypes()).Returns(MockHelper.CreateValidScopeTypesReader(Constants.SCOPETYPE_ValidScopeTypeCount));
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			_logger.LogDebug("ScopeTypeController_GetScopeTypes_Calls_DataService(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            // Act
            IQueryable<ScopeType> scopeTypes = scopeTypeController.GetScopeTypes();
			_logger.LogDebug("ScopeTypeController_GetScopeTypes_Calls_DataService(): scopeTypes → {ScopeTypes}", scopeTypes);

            // Assert
            mockDataService.Verify(ds => ds.GetScopeTypes());
			_logger.LogDebug("<<< Exiting ScopeTypeController_GetScopeTypes_Calls_DataService()");
        }

        [Test]
        public void ScopeTypeController_GetScopeTypes_Returns_Empty_List_Of_ScopeTypes_If_No_ScopeTypes()
        {
			_logger.LogDebug(">>> Entering ScopeTypeController_GetScopeTypes_Returns_Empty_List_Of_ScopeTypes_If_No_ScopeTypes()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ScopeTypeController_GetScopeTypes_Returns_Empty_List_Of_ScopeTypes_If_No_ScopeTypes(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetScopeTypes()).Returns(MockHelper.CreateEmptyScopeTypeReader());
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			_logger.LogDebug("ScopeTypeController_GetScopeTypes_Returns_Empty_List_Of_ScopeTypes_If_No_ScopeTypes(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            // Act
            var scopeTypes = scopeTypeController.GetScopeTypes();
			_logger.LogDebug("ScopeTypeController_GetScopeTypes_Returns_Empty_List_Of_ScopeTypes_If_No_ScopeTypes(): scopeTypes → {ScopeTypes}", scopeTypes);

            // Assert
            Assert.That(scopeTypes, Is.Not.Null);
            Assert.That(scopeTypes.Count(), Is.EqualTo(0));
			_logger.LogDebug("<<< Exiting ScopeTypeController_GetScopeTypes_Returns_Empty_List_Of_ScopeTypes_If_No_ScopeTypes()");
        }

        [Test]
        public void ScopeTypeController_GetScopeTypes_Returns_List_Of_ScopeTypes()
        {
			_logger.LogDebug(">>> Entering ScopeTypeController_GetScopeTypes_Returns_List_Of_ScopeTypes()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ScopeTypeController_GetScopeTypes_Returns_List_Of_ScopeTypes(): mockDataService → {MockDataService}", mockDataService);
            mockDataService.Setup(ds => ds.GetScopeTypes()).Returns(MockHelper.CreateValidScopeTypesReader(Constants.SCOPETYPE_ValidScopeTypeCount));
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			_logger.LogDebug("ScopeTypeController_GetScopeTypes_Returns_List_Of_ScopeTypes(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            // Act
            var scopeTypes = scopeTypeController.GetScopeTypes();
			_logger.LogDebug("ScopeTypeController_GetScopeTypes_Returns_List_Of_ScopeTypes(): scopeTypes → {ScopeTypes}", scopeTypes);

            // Assert
            Assert.That(scopeTypes.Count(), Is.EqualTo(Constants.SCOPETYPE_ValidScopeTypeCount));
			_logger.LogDebug("<<< Exiting ScopeTypeController_GetScopeTypes_Returns_List_Of_ScopeTypes()");
        }

        [Test]
        public void ScopeTypeController_UpdateScopeType_Throws_On_Null_ScopeType()
        {
			_logger.LogDebug(">>> Entering ScopeTypeController_UpdateScopeType_Throws_On_Null_ScopeType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ScopeTypeController_UpdateScopeType_Throws_On_Null_ScopeType(): mockDataService → {MockDataService}", mockDataService);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			_logger.LogDebug("ScopeTypeController_UpdateScopeType_Throws_On_Null_ScopeType(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => scopeTypeController.UpdateScopeType(null));
			_logger.LogDebug("<<< Exiting ScopeTypeController_UpdateScopeType_Throws_On_Null_ScopeType()");
        }

        [Test]
        public void ScopeTypeController_UpdateScopeType_Throws_On_Negative_ScopeTypeId()
        {
			_logger.LogDebug(">>> Entering ScopeTypeController_UpdateScopeType_Throws_On_Negative_ScopeTypeId()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ScopeTypeController_UpdateScopeType_Throws_On_Negative_ScopeTypeId(): mockDataService → {MockDataService}", mockDataService);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			_logger.LogDebug("ScopeTypeController_UpdateScopeType_Throws_On_Negative_ScopeTypeId(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            ScopeType scopeType = ContentTestHelper.CreateValidScopeType();
			_logger.LogDebug("ScopeTypeController_UpdateScopeType_Throws_On_Negative_ScopeTypeId(): scopeType → {ScopeType}", scopeType);
			_logger.LogDebug("ScopeTypeController_UpdateScopeType_Throws_On_Negative_ScopeTypeId(): scopeType.ScopeType → {Arg0}", scopeType.ScopeType);
            scopeType.ScopeType = Constants.SCOPETYPE_InValidScopeType;

            Assert.Throws<ArgumentOutOfRangeException>(() => scopeTypeController.UpdateScopeType(scopeType));
			_logger.LogDebug("<<< Exiting ScopeTypeController_UpdateScopeType_Throws_On_Negative_ScopeTypeId()");
        }

        [Test]
        public void ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType()
        {
			_logger.LogDebug(">>> Entering ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType()");
            // Arrange
            var mockDataService = new Mock<IDataService>();
			_logger.LogDebug("ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType(): mockDataService → {MockDataService}", mockDataService);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
			_logger.LogDebug("ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType(): scopeTypeController → {ScopeTypeController}", scopeTypeController);

            ScopeType scopeType = ContentTestHelper.CreateValidScopeType();
			_logger.LogDebug("ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType(): scopeType → {ScopeType}", scopeType);
			_logger.LogDebug("ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType(): scopeType.ScopeTypeId → {Arg0}", scopeType.ScopeTypeId);
			_logger.LogDebug("ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType(): scopeType.ScopeType → {Arg0}", scopeType.ScopeType);
            scopeType.ScopeTypeId = Constants.SCOPETYPE_UpdateScopeTypeId;
            scopeType.ScopeType = Constants.SCOPETYPE_UpdateScopeType;

            // Act
            scopeTypeController.UpdateScopeType(scopeType);

            // Assert
            mockDataService.Verify(ds => ds.UpdateScopeType(scopeType));
			_logger.LogDebug("<<< Exiting ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType()");
        }
    }
}
