// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging
{
    using System;

    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Social.Subscriptions;
    using DotNetNuke.Services.Social.Subscriptions.Data;
    using DotNetNuke.Services.Social.Subscriptions.Entities;
    using DotNetNuke.Tests.Core.Controllers.Messaging.Builders;
    using DotNetNuke.Tests.Core.Controllers.Messaging.Mocks;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private Mock<IDataService> mockDataService;
        private Mock<ISubscriptionSecurityController> subscriptionSecurityController;
        private Mock<CachingProvider> mockCacheProvider;

        private SubscriptionController subscriptionController;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            // Setup Mocks and Stub
            this.mockDataService = new Mock<IDataService>();
			Logger.LogDebug("SetUp(): mockDataService → {MockDataService}", mockDataService);
            this.mockCacheProvider = MockComponentProvider.CreateDataCacheProvider();
			Logger.LogDebug("SetUp(): mockCacheProvider → {MockCacheProvider}", mockCacheProvider);
            this.subscriptionSecurityController = new Mock<ISubscriptionSecurityController>();
			Logger.LogDebug("SetUp(): subscriptionSecurityController → {SubscriptionSecurityController}", subscriptionSecurityController);

            DataService.SetTestableInstance(this.mockDataService.Object);
            SubscriptionSecurityController.SetTestableInstance(this.subscriptionSecurityController.Object);

            // Setup SUT
            this.subscriptionController = new SubscriptionController();
			Logger.LogDebug("<<< Exiting SetUp()");
			Logger.LogDebug("SetUp(): subscriptionController → {SubscriptionController}", subscriptionController);
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            DataService.ClearInstance();
            SubscriptionSecurityController.ClearInstance();
            MockComponentProvider.ResetContainer();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void IsSubscribed_ShouldReturnFalse_IfUserIsNotSubscribed()
        {
			Logger.LogDebug(">>> Entering IsSubscribed_ShouldReturnFalse_IfUserIsNotSubscribed()");
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            this.mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                It.IsAny<int>(),
                It.IsAny<int>())).Returns(SubscriptionDataReaderMockHelper.CreateEmptySubscriptionReader());

            // Act
            var isSubscribed = this.subscriptionController.IsSubscribed(subscription);

			Logger.LogDebug("IsSubscribed_ShouldReturnFalse_IfUserIsNotSubscribed(): isSubscribed → {IsSubscribed}", isSubscribed);
            // Assert
            Assert.That(isSubscribed, Is.EqualTo(false));
			Logger.LogDebug("<<< Exiting IsSubscribed_ShouldReturnFalse_IfUserIsNotSubscribed()");
        }

        [Test]
        public void IsSubscribed_ShouldReturnFalse_WhenUserDoesNotHavePermissionOnTheSubscription()
        {
			Logger.LogDebug(">>> Entering IsSubscribed_ShouldReturnFalse_WhenUserDoesNotHavePermissionOnTheSubscription()");
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            var subscriptionCollection = new[] { subscription };

            this.mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                It.IsAny<int>(),
                It.IsAny<int>())).Returns(SubscriptionDataReaderMockHelper.CreateSubscriptionReader(subscriptionCollection));

            this.subscriptionSecurityController
                .Setup(ssc => ssc.HasPermission(It.IsAny<Subscription>())).Returns(false);

            // Act
            var isSubscribed = this.subscriptionController.IsSubscribed(subscription);

			Logger.LogDebug("IsSubscribed_ShouldReturnFalse_WhenUserDoesNotHavePermissionOnTheSubscription(): isSubscribed → {IsSubscribed}", isSubscribed);
            // Assert
            Assert.That(isSubscribed, Is.EqualTo(false));
			Logger.LogDebug("<<< Exiting IsSubscribed_ShouldReturnFalse_WhenUserDoesNotHavePermissionOnTheSubscription()");
        }

        [Test]
        public void IsSubscribed_ShouldReturnTrue_WhenUserHasPermissionOnTheSubscription()
        {
			Logger.LogDebug(">>> Entering IsSubscribed_ShouldReturnTrue_WhenUserHasPermissionOnTheSubscription()");
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            var subscriptionCollection = new[] { subscription };

            this.mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                It.IsAny<int>(),
                It.IsAny<int>())).Returns(SubscriptionDataReaderMockHelper.CreateSubscriptionReader(subscriptionCollection));

            this.subscriptionSecurityController
                .Setup(ssc => ssc.HasPermission(It.IsAny<Subscription>())).Returns(true);

            // Act
            var isSubscribed = this.subscriptionController.IsSubscribed(subscription);

			Logger.LogDebug("IsSubscribed_ShouldReturnTrue_WhenUserHasPermissionOnTheSubscription(): isSubscribed → {IsSubscribed}", isSubscribed);
            // Assert
            Assert.That(isSubscribed, Is.EqualTo(true));
			Logger.LogDebug("<<< Exiting IsSubscribed_ShouldReturnTrue_WhenUserHasPermissionOnTheSubscription()");
        }

        [Test]
        public void IsSubscribed_ShouldCallDataService_WhenNoError()
        {
			Logger.LogDebug(">>> Entering IsSubscribed_ShouldCallDataService_WhenNoError()");
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            this.mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.ModuleId,
                subscription.TabId)).Returns(SubscriptionDataReaderMockHelper.CreateEmptySubscriptionReader()).Verifiable();

            // Act
            this.subscriptionController.IsSubscribed(subscription);

            // Assert
            this.mockDataService.Verify(
                ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.ModuleId,
                subscription.TabId), Times.Once);
				Logger.LogDebug("<<< Exiting IsSubscribed_ShouldCallDataService_WhenNoError()");
        }

        [Test]
        public void AddSubscription_ShouldThrowArgumentNullException_WhenSubscriptionIsNull()
        {
			Logger.LogDebug(">>> Entering AddSubscription_ShouldThrowArgumentNullException_WhenSubscriptionIsNull()");
            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => this.subscriptionController.AddSubscription(null));
			Logger.LogDebug("<<< Exiting AddSubscription_ShouldThrowArgumentNullException_WhenSubscriptionIsNull()");
        }

        [Test]
        public void AddSubscription_ShouldThrowArgumentOutOfRangeException_WhenSubscriptionUserIdPropertyIsNegative()
        {
			Logger.LogDebug(">>> Entering AddSubscription_ShouldThrowArgumentOutOfRangeException_WhenSubscriptionUserIdPropertyIsNegative()");
            // Arrange
            var subscription = new SubscriptionBuilder()
                .WithUserId(-1)
                .Build();

            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => this.subscriptionController.AddSubscription(subscription));
			Logger.LogDebug("<<< Exiting AddSubscription_ShouldThrowArgumentOutOfRangeException_WhenSubscriptionUserIdPropertyIsNegative()");
        }

        [Test]
        public void AddSubscription_ShouldThrowArgumentOutOfRangeException_WhenSubscriptionSubscriptionTypePropertyIsNegative()
        {
			Logger.LogDebug(">>> Entering AddSubscription_ShouldThrowArgumentOutOfRangeException_WhenSubscriptionSubscriptionTypePropertyIsNegative()");
            // Arrange
            var subscription = new SubscriptionBuilder()
                .WithSubscriptionTypeId(-1)
                .Build();

            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => this.subscriptionController.AddSubscription(subscription));
			Logger.LogDebug("<<< Exiting AddSubscription_ShouldThrowArgumentOutOfRangeException_WhenSubscriptionSubscriptionTypePropertyIsNegative()");
        }

        [Test]
        public void AddSubscription_ShouldThrowArgumentNullException_WhenSubscriptionObjectKeyIsNull()
        {
			Logger.LogDebug(">>> Entering AddSubscription_ShouldThrowArgumentNullException_WhenSubscriptionObjectKeyIsNull()");
            // Arrange
            var subscription = new SubscriptionBuilder()
                .WithObjectKey(null)
                .Build();

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => this.subscriptionController.AddSubscription(subscription));
			Logger.LogDebug("<<< Exiting AddSubscription_ShouldThrowArgumentNullException_WhenSubscriptionObjectKeyIsNull()");
        }

        [Test]
        public void AddSubscription_ShouldCallDataService_WhenNoError()
        {
			Logger.LogDebug(">>> Entering AddSubscription_ShouldCallDataService_WhenNoError()");
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            this.mockDataService.Setup(ds => ds.AddSubscription(
                subscription.UserId,
                subscription.PortalId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.Description,
                subscription.ModuleId,
                subscription.TabId,
                subscription.ObjectData)).Verifiable();

            // Act
            this.subscriptionController.AddSubscription(subscription);

            // Assert
            this.mockDataService.Verify(
                ds => ds.AddSubscription(
                subscription.UserId,
                subscription.PortalId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.Description,
                subscription.ModuleId,
                subscription.TabId,
                subscription.ObjectData), Times.Once);
				Logger.LogDebug("<<< Exiting AddSubscription_ShouldCallDataService_WhenNoError()");
        }

        [Test]
        public void AddSubscription_ShouldFilledUpTheSubscriptionIdPropertyOfTheInputSubscriptionEntity_WhenNoError()
        {
			Logger.LogDebug(">>> Entering AddSubscription_ShouldFilledUpTheSubscriptionIdPropertyOfTheInputSubscriptionEntity_WhenNoError()");
            // Arrange
            const int expectedSubscriptionId = 12;

            var subscription = new SubscriptionBuilder()
                .Build();

            this.mockDataService.Setup(ds => ds.AddSubscription(
                subscription.UserId,
                subscription.PortalId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.Description,
                subscription.ModuleId,
                subscription.TabId,
                subscription.ObjectData)).Returns(expectedSubscriptionId);

            // Act
            this.subscriptionController.AddSubscription(subscription);

            // Assert
            Assert.That(subscription.SubscriptionId, Is.EqualTo(expectedSubscriptionId));
			Logger.LogDebug("<<< Exiting AddSubscription_ShouldFilledUpTheSubscriptionIdPropertyOfTheInputSubscriptionEntity_WhenNoError()");
        }

        [Test]
        public void DeleteSubscription_ShouldThrowArgumentNullException_WhenSubscriptionIsNull()
        {
			Logger.LogDebug(">>> Entering DeleteSubscription_ShouldThrowArgumentNullException_WhenSubscriptionIsNull()");
            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => this.subscriptionController.DeleteSubscription(null));
			Logger.LogDebug("<<< Exiting DeleteSubscription_ShouldThrowArgumentNullException_WhenSubscriptionIsNull()");
        }

        [Test]
        public void DeleteSubscriptionType_ShouldCallDeleteSubscriptionDataService_WhenSubscriptionExists()
        {
			Logger.LogDebug(">>> Entering DeleteSubscriptionType_ShouldCallDeleteSubscriptionDataService_WhenSubscriptionExists()");
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            this.mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                It.IsAny<int>(),
                It.IsAny<int>())).Returns(SubscriptionDataReaderMockHelper.CreateSubscriptionReader(new[] { subscription }));

            this.mockDataService.Setup(ds => ds.DeleteSubscription(It.IsAny<int>())).Verifiable();

            // Act
            this.subscriptionController.DeleteSubscription(subscription);

            // Assert
            this.mockDataService.Verify(ds => ds.DeleteSubscription(It.IsAny<int>()), Times.Once);
			Logger.LogDebug("<<< Exiting DeleteSubscriptionType_ShouldCallDeleteSubscriptionDataService_WhenSubscriptionExists()");
        }

        [Test]
        public void DeleteSubscriptionType_ShouldNotCallDeleteSubscriptionDataService_WhenSubscriptionDoesNotExist()
        {
			Logger.LogDebug(">>> Entering DeleteSubscriptionType_ShouldNotCallDeleteSubscriptionDataService_WhenSubscriptionDoesNotExist()");
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            this.mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                It.IsAny<int>(),
                It.IsAny<int>())).Returns(SubscriptionDataReaderMockHelper.CreateEmptySubscriptionReader());

            // Act
            this.subscriptionController.DeleteSubscription(subscription);

            // Assert
            this.mockDataService.Verify(ds => ds.DeleteSubscription(It.IsAny<int>()), Times.Never);
			Logger.LogDebug("<<< Exiting DeleteSubscriptionType_ShouldNotCallDeleteSubscriptionDataService_WhenSubscriptionDoesNotExist()");
        }
    }
}
