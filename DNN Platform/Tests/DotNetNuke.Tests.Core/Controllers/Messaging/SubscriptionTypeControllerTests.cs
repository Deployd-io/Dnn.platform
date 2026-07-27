// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Controllers.Messaging
{
    using System;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Social.Subscriptions;
    using DotNetNuke.Services.Social.Subscriptions.Data;
    using DotNetNuke.Tests.Core.Controllers.Messaging.Builders;
    using DotNetNuke.Tests.Core.Controllers.Messaging.Mocks;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionTypeControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string SubscriptionTypesCacheKey = "DNN_" + DataCache.SubscriptionTypesCacheKey;

        private SubscriptionTypeController subscriptionTypeController;
        private Mock<IDataService> mockDataService;
        private Mock<CachingProvider> mockCacheProvider;
        private Mock<IHostController> mockHostController;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            // Setup Mocks and Stub
            this.mockDataService = new Mock<IDataService>();
			Logger.LogDebug("SetUp(): mockDataService → {MockDataService}", mockDataService);
            this.mockCacheProvider = MockComponentProvider.CreateDataCacheProvider();
			Logger.LogDebug("SetUp(): mockCacheProvider → {MockCacheProvider}", mockCacheProvider);
            this.mockHostController = new Mock<IHostController>();
			Logger.LogDebug("SetUp(): mockHostController → {MockHostController}", mockHostController);
            this.mockHostController.As<IHostSettingsService>();

            DataService.SetTestableInstance(this.mockDataService.Object);

            // Setup SUT
            this.subscriptionTypeController = new SubscriptionTypeController(this.mockDataService.Object, new HostSettings((IHostSettingsService)this.mockHostController.Object));
			Logger.LogDebug("SetUp(): subscriptionTypeController → {SubscriptionTypeController}", subscriptionTypeController);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("SetUp(): services → {Services}", services);
                    services.AddSingleton(this.mockDataService.Object);
                    services.AddSingleton(this.mockCacheProvider.Object);
                    services.AddSingleton(this.mockHostController.Object);
                    services.AddSingleton((IHostSettingsService)this.mockHostController.Object);
                });
				Logger.LogDebug("<<< Exiting SetUp()");
        }

        [Test]
        public void GetSubscriptionTypes_ShouldCallDataService_WhenNoError()
        {
			Logger.LogDebug(">>> Entering GetSubscriptionTypes_ShouldCallDataService_WhenNoError()");
            // Arrange
            this.mockHostController
                .Setup(c => c.GetString("PerformanceSetting"))
                .Returns("0");

            this.mockDataService
                .Setup(ds => ds.GetSubscriptionTypes())
                .Returns(SubscriptionTypeDataReaderMockHelper.CreateEmptySubscriptionTypeReader())
                .Verifiable();

            // Act
            this.subscriptionTypeController.GetSubscriptionTypes();

            // Assert
            this.mockDataService.Verify(ds => ds.GetSubscriptionTypes(), Times.Once());
			Logger.LogDebug("<<< Exiting GetSubscriptionTypes_ShouldCallDataService_WhenNoError()");
        }

        [Test]
        public void GetSubscriptionTypes_ShouldThrowArgumentNullException_WhenPredicateIsNull()
        {
			Logger.LogDebug(">>> Entering GetSubscriptionTypes_ShouldThrowArgumentNullException_WhenPredicateIsNull()");
            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => this.subscriptionTypeController.GetSubscriptionTypes(null));
			Logger.LogDebug("<<< Exiting GetSubscriptionTypes_ShouldThrowArgumentNullException_WhenPredicateIsNull()");
        }

        [Test]
        public void GetSubscriptionType_ShouldThrowArgumentNullException_WhenPredicateIsNull()
        {
			Logger.LogDebug(">>> Entering GetSubscriptionType_ShouldThrowArgumentNullException_WhenPredicateIsNull()");
            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => this.subscriptionTypeController.GetSubscriptionType(null));
			Logger.LogDebug("<<< Exiting GetSubscriptionType_ShouldThrowArgumentNullException_WhenPredicateIsNull()");
        }

        [Test]
        public void AddSubscriptionType_ShouldThrowArgumentNullException_WhenSubscriptionTypeIsNull()
        {
			Logger.LogDebug(">>> Entering AddSubscriptionType_ShouldThrowArgumentNullException_WhenSubscriptionTypeIsNull()");
            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => this.subscriptionTypeController.AddSubscriptionType(null));
			Logger.LogDebug("<<< Exiting AddSubscriptionType_ShouldThrowArgumentNullException_WhenSubscriptionTypeIsNull()");
        }

        [Test]
        public void AddSubscriptionType_ShouldFilledUpTheSubscriptionTypeIdPropertyOfTheInputSubscriptionTypeEntity_WhenNoError()
        {
			Logger.LogDebug(">>> Entering AddSubscriptionType_ShouldFilledUpTheSubscriptionTypeIdPropertyOfTheInputSubscriptionTypeEntity_WhenNoError()");
            // Arrange
            const int expectedSubscriptionTypeId = 12;
            var subscriptionType = new SubscriptionTypeBuilder().Build();

			Logger.LogDebug("AddSubscriptionType_ShouldFilledUpTheSubscriptionTypeIdPropertyOfTheInputSubscriptionTypeEntity_WhenNoError(): subscriptionType → {SubscriptionType}", subscriptionType);
            this.mockDataService
                .Setup(ds => ds.AddSubscriptionType(
                    subscriptionType.SubscriptionName,
                    subscriptionType.FriendlyName,
                    subscriptionType.DesktopModuleId))
                .Returns(expectedSubscriptionTypeId);

            // Act
            this.subscriptionTypeController.AddSubscriptionType(subscriptionType);

            // Assert
            Assert.That(subscriptionType.SubscriptionTypeId, Is.EqualTo(expectedSubscriptionTypeId));
			Logger.LogDebug("<<< Exiting AddSubscriptionType_ShouldFilledUpTheSubscriptionTypeIdPropertyOfTheInputSubscriptionTypeEntity_WhenNoError()");
        }

        [Test]
        public void AddSubscriptionType_ShouldCleanCache_WhenNoError()
        {
			Logger.LogDebug(">>> Entering AddSubscriptionType_ShouldCleanCache_WhenNoError()");
            // Arrange
            this.mockDataService.Setup(ds => ds.AddSubscriptionType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            this.mockCacheProvider.Setup(cp => cp.Remove(SubscriptionTypesCacheKey)).Verifiable();

            var subscriptionType = new SubscriptionTypeBuilder().Build();

			Logger.LogDebug("AddSubscriptionType_ShouldCleanCache_WhenNoError(): subscriptionType → {SubscriptionType}", subscriptionType);
            // Act
            this.subscriptionTypeController.AddSubscriptionType(subscriptionType);

            // Assert
            this.mockCacheProvider.Verify(cp => cp.Remove(SubscriptionTypesCacheKey), Times.Once());
			Logger.LogDebug("<<< Exiting AddSubscriptionType_ShouldCleanCache_WhenNoError()");
        }

        [Test]
        public void DeleteSubscriptionType_ShouldThrowArgumentOutOfRangeException_WhenSubscriptionTypeIdIsNegative()
        {
			Logger.LogDebug(">>> Entering DeleteSubscriptionType_ShouldThrowArgumentOutOfRangeException_WhenSubscriptionTypeIdIsNegative()");
            // Arrange
            var subscriptionType = new SubscriptionTypeBuilder()
                .WithSubscriptionTypeId(-1)
                .Build();

            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => this.subscriptionTypeController.DeleteSubscriptionType(subscriptionType));
			Logger.LogDebug("<<< Exiting DeleteSubscriptionType_ShouldThrowArgumentOutOfRangeException_WhenSubscriptionTypeIdIsNegative()");
        }

        [Test]
        public void DeleteSubscriptionType_ShouldThrowNullArgumentException_WhenSubscriptionTypeIsNull()
        {
			Logger.LogDebug(">>> Entering DeleteSubscriptionType_ShouldThrowNullArgumentException_WhenSubscriptionTypeIsNull()");
            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => this.subscriptionTypeController.DeleteSubscriptionType(null));
			Logger.LogDebug("<<< Exiting DeleteSubscriptionType_ShouldThrowNullArgumentException_WhenSubscriptionTypeIsNull()");
        }

        [Test]
        public void DeleteSubscriptionType_ShouldCallDataService_WhenNoError()
        {
			Logger.LogDebug(">>> Entering DeleteSubscriptionType_ShouldCallDataService_WhenNoError()");
            // Arrange
            var subscriptionType = new SubscriptionTypeBuilder().Build();

			Logger.LogDebug("DeleteSubscriptionType_ShouldCallDataService_WhenNoError(): subscriptionType → {SubscriptionType}", subscriptionType);
            this.mockDataService
                .Setup(ds => ds.DeleteSubscriptionType(subscriptionType.SubscriptionTypeId))
                .Verifiable();

            // Act
            this.subscriptionTypeController.DeleteSubscriptionType(subscriptionType);

            // Assert
            this.mockDataService.Verify(ds => ds.DeleteSubscriptionType(subscriptionType.SubscriptionTypeId), Times.Once());
			Logger.LogDebug("<<< Exiting DeleteSubscriptionType_ShouldCallDataService_WhenNoError()");
        }

        [Test]
        public void DeleteSubscriptionType_ShouldCleanCache_WhenNoError()
        {
			Logger.LogDebug(">>> Entering DeleteSubscriptionType_ShouldCleanCache_WhenNoError()");
            // Arrange
            var subscriptionType = new SubscriptionTypeBuilder().Build();

			Logger.LogDebug("DeleteSubscriptionType_ShouldCleanCache_WhenNoError(): subscriptionType → {SubscriptionType}", subscriptionType);
            this.mockDataService.Setup(ds => ds.DeleteSubscriptionType(subscriptionType.SubscriptionTypeId));
            this.mockCacheProvider.Setup(cp => cp.Remove(SubscriptionTypesCacheKey)).Verifiable();

            // Act
            this.subscriptionTypeController.DeleteSubscriptionType(subscriptionType);

            // Assert
            this.mockCacheProvider.Verify(cp => cp.Remove(SubscriptionTypesCacheKey), Times.Once());
			Logger.LogDebug("<<< Exiting DeleteSubscriptionType_ShouldCleanCache_WhenNoError()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            DataService.ClearInstance();
            MockComponentProvider.ResetContainer();
			Logger.LogDebug("<<< Exiting TearDown()");
        }
    }
}
