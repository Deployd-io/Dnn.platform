// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Caching
{
    using System;
    using System.Web.Caching;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Providers.Caching;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DataCacheTests
    {
        private Mock<CachingProvider> mockCache;

        [SetUp]
        public void SetUp()
        {
			mockEventLogger.LogDebug(">>> Entering SetUp()");
            // Create a Container
            ComponentFactory.Container = new SimpleContainer();

            this.mockCache = MockComponentProvider.CreateDataCacheProvider();
			mockEventLogger.LogDebug("SetUp(): mockCache → {MockCache}", mockCache);
			mockEventLogger.LogDebug("<<< Exiting SetUp()");
        }

        [Test]
        public void DataCache_GetCache_Should_Return_On_Correct_CacheKey()
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_GetCache_Should_Return_On_Correct_CacheKey()");
            // Arrange
            this.mockCache.Setup(cache => cache.GetItem(this.GetDnnCacheKey(Constants.CACHEING_ValidKey))).ReturnsValidValue();

            // Act
            object cacheValue = DataCache.GetCache(Constants.CACHEING_ValidKey);
			mockEventLogger.LogDebug("DataCache_GetCache_Should_Return_On_Correct_CacheKey(): cacheValue → {CacheValue}", cacheValue);

            // Assert
            Assert.That(cacheValue, Is.InstanceOf<string>());
            Assert.That(cacheValue, Is.EqualTo(Constants.CACHEING_ValidValue));
			mockEventLogger.LogDebug("<<< Exiting DataCache_GetCache_Should_Return_On_Correct_CacheKey()");
        }

        [Test]
        public void DataCache_GetCache_Should_Return_Null_On_Incorrect_CacheKey()
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_GetCache_Should_Return_Null_On_Incorrect_CacheKey()");
            // Arrange
            this.mockCache.Setup(cache => cache.GetItem(this.GetDnnCacheKey(Constants.CACHEING_InValidKey))).Returns(null);

            // Act
            object cacheValue = DataCache.GetCache(Constants.CACHEING_InValidKey);
			mockEventLogger.LogDebug("DataCache_GetCache_Should_Return_Null_On_Incorrect_CacheKey(): cacheValue → {CacheValue}", cacheValue);

            // Assert
            Assert.That(cacheValue, Is.Null);
			mockEventLogger.LogDebug("<<< Exiting DataCache_GetCache_Should_Return_Null_On_Incorrect_CacheKey()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_GetCache_Should_Throw_On_NullOrEmpty_CacheKey(string key)
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_GetCache_Should_Throw_On_NullOrEmpty_CacheKey(key={Key})", key);
            Assert.Throws<ArgumentException>(() => DataCache.GetCache(key));
			mockEventLogger.LogDebug("<<< Exiting DataCache_GetCache_Should_Throw_On_NullOrEmpty_CacheKey(key={Key})", key);
        }

        [Test]
        public void DataCache_GetCacheOfT_Should_Return_On_Correct_CacheKey()
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_GetCacheOfT_Should_Return_On_Correct_CacheKey()");
            // Arrange
            this.mockCache.Setup(cache => cache.GetItem(this.GetDnnCacheKey(Constants.CACHEING_ValidKey))).ReturnsValidValue();

            // Act
            object cacheValue = DataCache.GetCache<string>(Constants.CACHEING_ValidKey);
			mockEventLogger.LogDebug("DataCache_GetCacheOfT_Should_Return_On_Correct_CacheKey(): cacheValue → {CacheValue}", cacheValue);

            // Assert
            Assert.That(cacheValue, Is.InstanceOf<string>());
            Assert.That(cacheValue, Is.EqualTo(Constants.CACHEING_ValidValue));
			mockEventLogger.LogDebug("<<< Exiting DataCache_GetCacheOfT_Should_Return_On_Correct_CacheKey()");
        }

        [Test]
        public void DataCache_GetCacheOfT_Should_Return_Null_On_Incorrect_CacheKey()
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_GetCacheOfT_Should_Return_Null_On_Incorrect_CacheKey()");
            // Arrange
            this.mockCache.Setup(cache => cache.GetItem(this.GetDnnCacheKey(Constants.CACHEING_InValidKey))).Returns(null);

            // Act
            object cacheValue = DataCache.GetCache<string>(Constants.CACHEING_InValidKey);
			mockEventLogger.LogDebug("DataCache_GetCacheOfT_Should_Return_Null_On_Incorrect_CacheKey(): cacheValue → {CacheValue}", cacheValue);

            // Assert
            Assert.That(cacheValue, Is.Null);
			mockEventLogger.LogDebug("<<< Exiting DataCache_GetCacheOfT_Should_Return_Null_On_Incorrect_CacheKey()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_GetCacheOfT_Should_Throw_On_NullOrEmpty_CacheKey(string key)
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_GetCacheOfT_Should_Throw_On_NullOrEmpty_CacheKey(key={Key})", key);
            Assert.Throws<ArgumentException>(() => DataCache.GetCache<string>(key));
			mockEventLogger.LogDebug("<<< Exiting DataCache_GetCacheOfT_Should_Throw_On_NullOrEmpty_CacheKey(key={Key})", key);
        }

        [Test]
        public void DataCache_RemoveCache_Should_Succeed_On_Valid_CacheKey()
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_RemoveCache_Should_Succeed_On_Valid_CacheKey()");
            // Arrange

            // Act
            DataCache.RemoveCache(Constants.CACHEING_ValidKey);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(this.GetDnnCacheKey(Constants.CACHEING_ValidKey)));
			mockEventLogger.LogDebug("<<< Exiting DataCache_RemoveCache_Should_Succeed_On_Valid_CacheKey()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_RemoveCache_Should_Throw_On_NullOrEmpty_CacheKey(string key)
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_RemoveCache_Should_Throw_On_NullOrEmpty_CacheKey(key={Key})", key);
            Assert.Throws<ArgumentException>(() => DataCache.RemoveCache(key));
			mockEventLogger.LogDebug("<<< Exiting DataCache_RemoveCache_Should_Throw_On_NullOrEmpty_CacheKey(key={Key})", key);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_SetCache_Should_Throw_On_Null_CacheKey(string key)
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_SetCache_Should_Throw_On_Null_CacheKey(key={Key})", key);
            Assert.Throws<ArgumentException>(() => DataCache.SetCache(key, Constants.CACHEING_ValidValue));
			mockEventLogger.LogDebug("<<< Exiting DataCache_SetCache_Should_Throw_On_Null_CacheKey(key={Key})", key);
        }

        [Test]
        public void DataCache_SetCache_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_SetCache_Should_Succeed_On_Valid_CacheKey_And_Any_Value()");
            // Arrange

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue);

            // Assert
            DNNCacheDependency dep = null;
            this.mockCache.Verify(
                cache =>
                cache.Insert(
                    this.GetDnnCacheKey(Constants.CACHEING_ValidKey),
                    Constants.CACHEING_ValidValue,
                    dep,
                    Cache.NoAbsoluteExpiration,
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    DataCache.ItemRemovedCallback));
					mockEventLogger.LogDebug("<<< Exiting DataCache_SetCache_Should_Succeed_On_Valid_CacheKey_And_Any_Value()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_SetCache_With_Dependency_Should_Throw_On_Null_CacheKey(string key)
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_SetCache_With_Dependency_Should_Throw_On_Null_CacheKey(key={Key})", key);
            DNNCacheDependency dep = this.CreateTestDependency(); // Dependency type or value doesn't matter
            Assert.Throws<ArgumentException>(() => DataCache.SetCache(key, Constants.CACHEING_ValidValue, dep));
			mockEventLogger.LogDebug("DataCache_SetCache_With_Dependency_Should_Throw_On_Null_CacheKey(key={Key}): dep → {Dep}", key, dep);
			mockEventLogger.LogDebug("<<< Exiting DataCache_SetCache_With_Dependency_Should_Throw_On_Null_CacheKey(key={Key})", key);
        }

        [Test]
        public void DataCache_SetCache_With_Dependency_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_SetCache_With_Dependency_Should_Succeed_On_Valid_CacheKey_And_Any_Value()");
            // Arrange
            DNNCacheDependency dep = this.CreateTestDependency(); // Dependency type or value doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_Dependency_Should_Succeed_On_Valid_CacheKey_And_Any_Value(): dep → {Dep}", dep);

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, dep);

            // Assert
            this.mockCache.Verify(
                cache =>
                cache.Insert(
                    this.GetDnnCacheKey(Constants.CACHEING_ValidKey),
                    Constants.CACHEING_ValidValue,
                    dep,
                    Cache.NoAbsoluteExpiration,
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    DataCache.ItemRemovedCallback));
					mockEventLogger.LogDebug("<<< Exiting DataCache_SetCache_With_Dependency_Should_Succeed_On_Valid_CacheKey_And_Any_Value()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_SetCache_With_AbsoluteExpiration_Should_Throw_On_Null_CacheKey(string key)
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_SetCache_With_AbsoluteExpiration_Should_Throw_On_Null_CacheKey(key={Key})", key);
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
            Assert.Throws<ArgumentException>(() => DataCache.SetCache(key, Constants.CACHEING_ValidValue, absExpiry));
			mockEventLogger.LogDebug("DataCache_SetCache_With_AbsoluteExpiration_Should_Throw_On_Null_CacheKey(key={Key}): absExpiry → {AbsExpiry}", key, absExpiry);
			mockEventLogger.LogDebug("<<< Exiting DataCache_SetCache_With_AbsoluteExpiration_Should_Throw_On_Null_CacheKey(key={Key})", key);
        }

        [Test]
        public void DataCache_SetCache_With_AbsoluteExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_SetCache_With_AbsoluteExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value()");
            // Arrange
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_AbsoluteExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value(): absExpiry → {AbsExpiry}", absExpiry);

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, absExpiry);

            // Assert
            DNNCacheDependency dep = null;
            this.mockCache.Verify(
                cache =>
                cache.Insert(
                    this.GetDnnCacheKey(Constants.CACHEING_ValidKey),
                    Constants.CACHEING_ValidValue,
                    dep,
                    absExpiry,
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    DataCache.ItemRemovedCallback));
					mockEventLogger.LogDebug("<<< Exiting DataCache_SetCache_With_AbsoluteExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_SetCache_With_SlidingExpiration_Should_Throw_On_Null_CacheKey(string key)
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_SetCache_With_SlidingExpiration_Should_Throw_On_Null_CacheKey(key={Key})", key);
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
            Assert.Throws<ArgumentException>(() => DataCache.SetCache(key, Constants.CACHEING_ValidValue, slidingExpiry));
			mockEventLogger.LogDebug("DataCache_SetCache_With_SlidingExpiration_Should_Throw_On_Null_CacheKey(key={Key}): slidingExpiry → {SlidingExpiry}", key, slidingExpiry);
			mockEventLogger.LogDebug("<<< Exiting DataCache_SetCache_With_SlidingExpiration_Should_Throw_On_Null_CacheKey(key={Key})", key);
        }

        [Test]
        public void DataCache_SetCache_With_SlidingExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_SetCache_With_SlidingExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value()");
            // Arrange
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_SlidingExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value(): slidingExpiry → {SlidingExpiry}", slidingExpiry);

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, slidingExpiry);

            // Assert
            // Assert
            DNNCacheDependency dep = null;
            this.mockCache.Verify(
                cache =>
                cache.Insert(
                    this.GetDnnCacheKey(Constants.CACHEING_ValidKey),
                    Constants.CACHEING_ValidValue,
                    dep,
                    Cache.NoAbsoluteExpiration,
                    slidingExpiry,
                    CacheItemPriority.Normal,
                    DataCache.ItemRemovedCallback));
					mockEventLogger.LogDebug("<<< Exiting DataCache_SetCache_With_SlidingExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Throw_On_Null_CacheKey(string key)
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Throw_On_Null_CacheKey(key={Key})", key);
            DNNCacheDependency dep = this.CreateTestDependency(); // Dependency type or value doesn't matter
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Throw_On_Null_CacheKey(key={Key}): absExpiry → {AbsExpiry}", key, absExpiry);
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Throw_On_Null_CacheKey(key={Key}): slidingExpiry → {SlidingExpiry}", key, slidingExpiry);
			mockEventLogger.LogDebug("DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Throw_On_Null_CacheKey(key={Key}): dep → {Dep}", key, dep);
            Assert.Throws<ArgumentException>(() => DataCache.SetCache(key, Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry));
			mockEventLogger.LogDebug("<<< Exiting DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Throw_On_Null_CacheKey(key={Key})", key);
        }

        [Test]
        public void DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value()");
            // Arrange
            DNNCacheDependency dep = this.CreateTestDependency(); // Dependency type or value doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value(): dep → {Dep}", dep);
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value(): absExpiry → {AbsExpiry}", absExpiry);
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value(): slidingExpiry → {SlidingExpiry}", slidingExpiry);

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry);

            // Assert
            this.mockCache.Verify(
                cache =>
                cache.Insert(this.GetDnnCacheKey(Constants.CACHEING_ValidKey), Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, CacheItemPriority.Normal, DataCache.ItemRemovedCallback));
				mockEventLogger.LogDebug("<<< Exiting DataCache_SetCache_With_CacheDependency_AbsoluteExpiration_SlidingExpiration_Should_Succeed_On_Valid_CacheKey_And_Any_Value()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DataCache_SetCache_With_Priority_Should_Throw_On_Null_CacheKey(string key)
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_SetCache_With_Priority_Should_Throw_On_Null_CacheKey(key={Key})", key);
            DNNCacheDependency dep = this.CreateTestDependency(); // Dependency type or value doesn't matter
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_Priority_Should_Throw_On_Null_CacheKey(key={Key}): absExpiry → {AbsExpiry}", key, absExpiry);
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_Priority_Should_Throw_On_Null_CacheKey(key={Key}): slidingExpiry → {SlidingExpiry}", key, slidingExpiry);
			mockEventLogger.LogDebug("DataCache_SetCache_With_Priority_Should_Throw_On_Null_CacheKey(key={Key}): dep → {Dep}", key, dep);
            CacheItemPriority priority = CacheItemPriority.High; // Priority doesn't matter
            Assert.Throws<ArgumentException>(() => DataCache.SetCache(key, Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, priority, null));
			mockEventLogger.LogDebug("<<< Exiting DataCache_SetCache_With_Priority_Should_Throw_On_Null_CacheKey(key={Key})", key);
        }

        [Test]
        public void DataCache_SetCache_With_Priority_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_SetCache_With_Priority_Should_Succeed_On_Valid_CacheKey_And_Any_Value()");
            // Arrange
            DNNCacheDependency dep = this.CreateTestDependency(); // Dependency type or value doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_Priority_Should_Succeed_On_Valid_CacheKey_And_Any_Value(): dep → {Dep}", dep);
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_Priority_Should_Succeed_On_Valid_CacheKey_And_Any_Value(): absExpiry → {AbsExpiry}", absExpiry);
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_Priority_Should_Succeed_On_Valid_CacheKey_And_Any_Value(): slidingExpiry → {SlidingExpiry}", slidingExpiry);
            CacheItemPriority priority = CacheItemPriority.High; // Priority doesn't matter

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, priority, null);

            // Assert
            this.mockCache.Verify(cache => cache.Insert(this.GetDnnCacheKey(Constants.CACHEING_ValidKey), Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, priority, DataCache.ItemRemovedCallback));
			mockEventLogger.LogDebug("<<< Exiting DataCache_SetCache_With_Priority_Should_Succeed_On_Valid_CacheKey_And_Any_Value()");
        }

        [Test]
        public void DataCache_SetCache_With_Callback_Should_Succeed_On_Valid_CacheKey_And_Any_Value()
        {
			mockEventLogger.LogDebug(">>> Entering DataCache_SetCache_With_Callback_Should_Succeed_On_Valid_CacheKey_And_Any_Value()");
            // Arrange
            DNNCacheDependency dep = this.CreateTestDependency(); // Dependency type or value doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_Callback_Should_Succeed_On_Valid_CacheKey_And_Any_Value(): dep → {Dep}", dep);
            DateTime absExpiry = DateTime.Today.AddDays(1); // DateTime doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_Callback_Should_Succeed_On_Valid_CacheKey_And_Any_Value(): absExpiry → {AbsExpiry}", absExpiry);
            TimeSpan slidingExpiry = TimeSpan.FromMinutes(5); // TimeSpan doesn't matter
			mockEventLogger.LogDebug("DataCache_SetCache_With_Callback_Should_Succeed_On_Valid_CacheKey_And_Any_Value(): slidingExpiry → {SlidingExpiry}", slidingExpiry);
            CacheItemRemovedCallback callback = this.ItemRemovedCallback;

            // Act
            DataCache.SetCache(Constants.CACHEING_ValidKey, Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, CacheItemPriority.Normal, this.ItemRemovedCallback);

            // Assert
            this.mockCache.Verify(
                cache => cache.Insert(this.GetDnnCacheKey(Constants.CACHEING_ValidKey), Constants.CACHEING_ValidValue, dep, absExpiry, slidingExpiry, CacheItemPriority.Normal, this.ItemRemovedCallback));
				mockEventLogger.LogDebug("<<< Exiting DataCache_SetCache_With_Callback_Should_Succeed_On_Valid_CacheKey_And_Any_Value()");
        }

        private DNNCacheDependency CreateTestDependency()
        {
			mockEventLogger.LogDebug(">>> Entering CreateTestDependency()");
			mockEventLogger.LogDebug("<<< Exiting CreateTestDependency()");
            return new DNNCacheDependency("C:\\testdependency.txt");
        }

        private string GetDnnCacheKey(string cacheKey)
        {
			mockEventLogger.LogDebug(">>> Entering GetDnnCacheKey(cacheKey={CacheKey})", cacheKey);
			mockEventLogger.LogDebug("<<< Exiting GetDnnCacheKey(cacheKey={CacheKey})", cacheKey);
            return string.Format("DNN_{0}", cacheKey);
        }

        private void ItemRemovedCallback(string key, object value, CacheItemRemovedReason removedReason)
        {
			mockEventLogger.LogDebug(">>> Entering ItemRemovedCallback(key={Key},value={Value},removedReason={RemovedReason})", key, value, removedReason);
            // do nothing
			mockEventLogger.LogDebug("<<< Exiting ItemRemovedCallback(key={Key},value={Value},removedReason={RemovedReason})", key, value, removedReason);
        }
    }
}
