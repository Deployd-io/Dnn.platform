// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Data
{
    using System;
    using System.Collections.Generic;
    using System.Web.Caching;

    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Data.Fakes;
    using DotNetNuke.Tests.Data.Models;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Moq;
    using Moq.Protected;

    using NUnit.Framework;

    [TestFixture]
    public class RepositoryBaseTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            MockComponentProvider.ResetContainer();
            this.serviceProvider = FakeServiceProvider.Setup();
			Logger.LogDebug("<<< Exiting SetUp()");
			Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_CacheArgs_Null_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Constructor_Sets_CacheArgs_Null_If_Not_Cacheable()");
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();

			Logger.LogDebug("RepositoryBase_Constructor_Sets_CacheArgs_Null_If_Not_Cacheable(): repo → {Repo}", repo);
            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Dog>, CacheItemArgs>(baseRepo, "CacheArgs"), Is.Null);
			Logger.LogDebug("<<< Exiting RepositoryBase_Constructor_Sets_CacheArgs_Null_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_CacheArgs_If_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Constructor_Sets_CacheArgs_If_Cacheable()");
            // Arrange

            // Act
            var repo = new FakeRepository<CacheableDog>();

			Logger.LogDebug("RepositoryBase_Constructor_Sets_CacheArgs_If_Cacheable(): repo → {Repo}", repo);
            // Assert
            var baseRepo = repo as RepositoryBase<CacheableDog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<CacheableDog>, CacheItemArgs>(baseRepo, "CacheArgs"), Is.Not.Null);
			Logger.LogDebug("<<< Exiting RepositoryBase_Constructor_Sets_CacheArgs_If_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_Valid_CacheArgs_If_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Constructor_Sets_Valid_CacheArgs_If_Cacheable()");
            // Arrange

            // Act
            var repo = new FakeRepository<CacheableDog>();

			Logger.LogDebug("RepositoryBase_Constructor_Sets_Valid_CacheArgs_If_Cacheable(): repo → {Repo}", repo);
            // Assert
            var cacheArgs = Util.GetPrivateMember<FakeRepository<CacheableDog>, CacheItemArgs>(repo, "CacheArgs");
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("RepositoryBase_Constructor_Sets_Valid_CacheArgs_If_Cacheable(): cacheArgs → {CacheArgs}", cacheArgs);
                Assert.That(cacheArgs.CacheKey, Is.EqualTo(Constants.CACHE_DogsKey));
                Assert.That(cacheArgs.CachePriority, Is.EqualTo(Constants.CACHE_Priority));
                Assert.That(cacheArgs.CacheTimeOut, Is.EqualTo(Constants.CACHE_TimeOut));
            }
			Logger.LogDebug("<<< Exiting RepositoryBase_Constructor_Sets_Valid_CacheArgs_If_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_IsCacheable_False_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Constructor_Sets_IsCacheable_False_If_Not_Cacheable()");
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();

			Logger.LogDebug("RepositoryBase_Constructor_Sets_IsCacheable_False_If_Not_Cacheable(): repo → {Repo}", repo);
            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Dog>, bool>(baseRepo, "IsCacheable"), Is.False);
			Logger.LogDebug("<<< Exiting RepositoryBase_Constructor_Sets_IsCacheable_False_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_IsCacheable_True_If_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Constructor_Sets_IsCacheable_True_If_Cacheable()");
            // Arrange

            // Act
            var repo = new FakeRepository<CacheableDog>();

			Logger.LogDebug("RepositoryBase_Constructor_Sets_IsCacheable_True_If_Cacheable(): repo → {Repo}", repo);
            // Assert
            var baseRepo = repo as RepositoryBase<CacheableDog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<CacheableDog>, bool>(baseRepo, "IsCacheable"), Is.True);
			Logger.LogDebug("<<< Exiting RepositoryBase_Constructor_Sets_IsCacheable_True_If_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_IsScoped_False_If_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Constructor_Sets_IsScoped_False_If_Not_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();

			Logger.LogDebug("RepositoryBase_Constructor_Sets_IsScoped_False_If_Not_Scoped(): repo → {Repo}", repo);
            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Dog>, bool>(baseRepo, "IsScoped"), Is.False);
			Logger.LogDebug("<<< Exiting RepositoryBase_Constructor_Sets_IsScoped_False_If_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_IsScoped_False_If_Cacheable_And_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Constructor_Sets_IsScoped_False_If_Cacheable_And_Not_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<CacheableDog>();

			Logger.LogDebug("RepositoryBase_Constructor_Sets_IsScoped_False_If_Cacheable_And_Not_Scoped(): repo → {Repo}", repo);
            // Assert
            var baseRepo = repo as RepositoryBase<CacheableDog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<CacheableDog>, bool>(baseRepo, "IsScoped"), Is.False);
			Logger.LogDebug("<<< Exiting RepositoryBase_Constructor_Sets_IsScoped_False_If_Cacheable_And_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_IsScoped_True_If_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Constructor_Sets_IsScoped_True_If_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<Cat>();

			Logger.LogDebug("RepositoryBase_Constructor_Sets_IsScoped_True_If_Scoped(): repo → {Repo}", repo);
            // Assert
            var baseRepo = repo as RepositoryBase<Cat>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Cat>, bool>(baseRepo, "IsScoped"), Is.True);
			Logger.LogDebug("<<< Exiting RepositoryBase_Constructor_Sets_IsScoped_True_If_Scoped()");
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_IsScoped_True_If_Cacheable_And_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Constructor_Sets_IsScoped_True_If_Cacheable_And_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<CacheableCat>();

			Logger.LogDebug("RepositoryBase_Constructor_Sets_IsScoped_True_If_Cacheable_And_Scoped(): repo → {Repo}", repo);
            // Assert
            var baseRepo = repo as RepositoryBase<CacheableCat>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<CacheableCat>, bool>(baseRepo, "IsScoped"), Is.True);
			Logger.LogDebug("<<< Exiting RepositoryBase_Constructor_Sets_IsScoped_True_If_Cacheable_And_Scoped()");
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_Scope_Empty_If_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Constructor_Sets_Scope_Empty_If_Not_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();

			Logger.LogDebug("RepositoryBase_Constructor_Sets_Scope_Empty_If_Not_Scoped(): repo → {Repo}", repo);
            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Dog>, string>(baseRepo, "Scope"), Is.EqualTo(string.Empty));
			Logger.LogDebug("<<< Exiting RepositoryBase_Constructor_Sets_Scope_Empty_If_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_Scope_Empty_If_Cacheable_And_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Constructor_Sets_Scope_Empty_If_Cacheable_And_Not_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<CacheableDog>();

			Logger.LogDebug("RepositoryBase_Constructor_Sets_Scope_Empty_If_Cacheable_And_Not_Scoped(): repo → {Repo}", repo);
            // Assert
            var baseRepo = repo as RepositoryBase<CacheableDog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<CacheableDog>, string>(baseRepo, "Scope"), Is.EqualTo(string.Empty));
			Logger.LogDebug("<<< Exiting RepositoryBase_Constructor_Sets_Scope_Empty_If_Cacheable_And_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_Scope_If_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Constructor_Sets_Scope_If_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<Cat>();

			Logger.LogDebug("RepositoryBase_Constructor_Sets_Scope_If_Scoped(): repo → {Repo}", repo);
            // Assert
            var baseRepo = repo as RepositoryBase<Cat>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Cat>, string>(baseRepo, "Scope"), Is.EqualTo(Constants.CACHE_ScopeModule));
			Logger.LogDebug("<<< Exiting RepositoryBase_Constructor_Sets_Scope_If_Scoped()");
        }

        [Test]
        public void RepositoryBase_Constructor_Sets_Scope_If_Cacheable_And_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Constructor_Sets_Scope_If_Cacheable_And_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<CacheableCat>();

			Logger.LogDebug("RepositoryBase_Constructor_Sets_Scope_If_Cacheable_And_Scoped(): repo → {Repo}", repo);
            // Assert
            var baseRepo = repo as RepositoryBase<CacheableCat>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<CacheableCat>, string>(baseRepo, "Scope"), Is.EqualTo(Constants.CACHE_ScopeModule));
			Logger.LogDebug("<<< Exiting RepositoryBase_Constructor_Sets_Scope_If_Cacheable_And_Scoped()");
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_CacheArgs_Null_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Initialize_Sets_CacheArgs_Null_If_Not_Cacheable()");
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(string.Empty);
			Logger.LogDebug("RepositoryBase_Initialize_Sets_CacheArgs_Null_If_Not_Cacheable(): repo → {Repo}", repo);

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Dog>, CacheItemArgs>(baseRepo, "CacheArgs"), Is.Null);
			Logger.LogDebug("<<< Exiting RepositoryBase_Initialize_Sets_CacheArgs_Null_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_CacheArgs_If_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Initialize_Sets_CacheArgs_If_Cacheable()");
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(Constants.CACHE_DogsKey);
			Logger.LogDebug("RepositoryBase_Initialize_Sets_CacheArgs_If_Cacheable(): repo → {Repo}", repo);

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Dog>, CacheItemArgs>(baseRepo, "CacheArgs"), Is.Not.Null);
			Logger.LogDebug("<<< Exiting RepositoryBase_Initialize_Sets_CacheArgs_If_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_Valid_CacheArgs_If_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Initialize_Sets_Valid_CacheArgs_If_Cacheable()");
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(Constants.CACHE_DogsKey, Constants.CACHE_TimeOut, Constants.CACHE_Priority);
			Logger.LogDebug("RepositoryBase_Initialize_Sets_Valid_CacheArgs_If_Cacheable(): repo → {Repo}", repo);

            // Assert
            var cacheArgs = Util.GetPrivateMember<FakeRepository<Dog>, CacheItemArgs>(repo, "CacheArgs");
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("RepositoryBase_Initialize_Sets_Valid_CacheArgs_If_Cacheable(): cacheArgs → {CacheArgs}", cacheArgs);
                Assert.That(cacheArgs.CacheKey, Is.EqualTo(Constants.CACHE_DogsKey));
                Assert.That(cacheArgs.CachePriority, Is.EqualTo(Constants.CACHE_Priority));
                Assert.That(cacheArgs.CacheTimeOut, Is.EqualTo(Constants.CACHE_TimeOut));
            }
			Logger.LogDebug("<<< Exiting RepositoryBase_Initialize_Sets_Valid_CacheArgs_If_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_IsCacheable_False_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Initialize_Sets_IsCacheable_False_If_Not_Cacheable()");
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();

			Logger.LogDebug("RepositoryBase_Initialize_Sets_IsCacheable_False_If_Not_Cacheable(): repo → {Repo}", repo);
            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Dog>, bool>(baseRepo, "IsCacheable"), Is.False);
			Logger.LogDebug("<<< Exiting RepositoryBase_Initialize_Sets_IsCacheable_False_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_IsCacheable_True_If_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Initialize_Sets_IsCacheable_True_If_Cacheable()");
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(Constants.CACHE_DogsKey);
			Logger.LogDebug("RepositoryBase_Initialize_Sets_IsCacheable_True_If_Cacheable(): repo → {Repo}", repo);

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Dog>, bool>(baseRepo, "IsCacheable"), Is.True);
			Logger.LogDebug("<<< Exiting RepositoryBase_Initialize_Sets_IsCacheable_True_If_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_IsScoped_False_If_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Initialize_Sets_IsScoped_False_If_Not_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(string.Empty);
			Logger.LogDebug("RepositoryBase_Initialize_Sets_IsScoped_False_If_Not_Scoped(): repo → {Repo}", repo);

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Dog>, bool>(baseRepo, "IsScoped"), Is.False);
			Logger.LogDebug("<<< Exiting RepositoryBase_Initialize_Sets_IsScoped_False_If_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_IsScoped_False_If_Cacheable_And_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Initialize_Sets_IsScoped_False_If_Cacheable_And_Not_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(Constants.CACHE_DogsKey, Constants.CACHE_TimeOut, Constants.CACHE_Priority);
			Logger.LogDebug("RepositoryBase_Initialize_Sets_IsScoped_False_If_Cacheable_And_Not_Scoped(): repo → {Repo}", repo);

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Dog>, bool>(baseRepo, "IsScoped"), Is.False);
			Logger.LogDebug("<<< Exiting RepositoryBase_Initialize_Sets_IsScoped_False_If_Cacheable_And_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_IsScoped_True_If_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Initialize_Sets_IsScoped_True_If_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<Cat>();
            repo.Initialize(string.Empty, 20, CacheItemPriority.Default, Constants.CACHE_ScopeModule);
			Logger.LogDebug("RepositoryBase_Initialize_Sets_IsScoped_True_If_Scoped(): repo → {Repo}", repo);

            // Assert
            var baseRepo = repo as RepositoryBase<Cat>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Cat>, bool>(baseRepo, "IsScoped"), Is.True);
			Logger.LogDebug("<<< Exiting RepositoryBase_Initialize_Sets_IsScoped_True_If_Scoped()");
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_IsScoped_True_If_Cacheable_And_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Initialize_Sets_IsScoped_True_If_Cacheable_And_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<Cat>();
            repo.Initialize(Constants.CACHE_CatsKey, Constants.CACHE_TimeOut, Constants.CACHE_Priority, Constants.CACHE_ScopeModule);
			Logger.LogDebug("RepositoryBase_Initialize_Sets_IsScoped_True_If_Cacheable_And_Scoped(): repo → {Repo}", repo);

            // Assert
            var baseRepo = repo as RepositoryBase<Cat>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Cat>, bool>(baseRepo, "IsScoped"), Is.True);
			Logger.LogDebug("<<< Exiting RepositoryBase_Initialize_Sets_IsScoped_True_If_Cacheable_And_Scoped()");
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_Scope_Empty_If_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Initialize_Sets_Scope_Empty_If_Not_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(string.Empty);
			Logger.LogDebug("RepositoryBase_Initialize_Sets_Scope_Empty_If_Not_Scoped(): repo → {Repo}", repo);

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Dog>, string>(baseRepo, "Scope"), Is.EqualTo(string.Empty));
			Logger.LogDebug("<<< Exiting RepositoryBase_Initialize_Sets_Scope_Empty_If_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_Scope_Empty_If_Cacheable_And_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Initialize_Sets_Scope_Empty_If_Cacheable_And_Not_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<Dog>();
            repo.Initialize(Constants.CACHE_DogsKey, Constants.CACHE_TimeOut, Constants.CACHE_Priority);
			Logger.LogDebug("RepositoryBase_Initialize_Sets_Scope_Empty_If_Cacheable_And_Not_Scoped(): repo → {Repo}", repo);

            // Assert
            var baseRepo = repo as RepositoryBase<Dog>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Dog>, string>(baseRepo, "Scope"), Is.EqualTo(string.Empty));
			Logger.LogDebug("<<< Exiting RepositoryBase_Initialize_Sets_Scope_Empty_If_Cacheable_And_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_Scope_If_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Initialize_Sets_Scope_If_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<Cat>();
            repo.Initialize(string.Empty, 20, CacheItemPriority.Default, Constants.CACHE_ScopeModule);
			Logger.LogDebug("RepositoryBase_Initialize_Sets_Scope_If_Scoped(): repo → {Repo}", repo);

            // Assert
            var baseRepo = repo as RepositoryBase<Cat>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Cat>, string>(baseRepo, "Scope"), Is.EqualTo(Constants.CACHE_ScopeModule));
			Logger.LogDebug("<<< Exiting RepositoryBase_Initialize_Sets_Scope_If_Scoped()");
        }

        [Test]
        public void RepositoryBase_Initialize_Sets_Scope_If_Cacheable_And_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Initialize_Sets_Scope_If_Cacheable_And_Scoped()");
            // Arrange

            // Act
            var repo = new FakeRepository<Cat>();
            repo.Initialize(Constants.CACHE_CatsKey, Constants.CACHE_TimeOut, Constants.CACHE_Priority, Constants.CACHE_ScopeModule);
			Logger.LogDebug("RepositoryBase_Initialize_Sets_Scope_If_Cacheable_And_Scoped(): repo → {Repo}", repo);

            // Assert
            var baseRepo = repo as RepositoryBase<Cat>;
            Assert.That(Util.GetPrivateMember<RepositoryBase<Cat>, string>(baseRepo, "Scope"), Is.EqualTo(Constants.CACHE_ScopeModule));
			Logger.LogDebug("<<< Exiting RepositoryBase_Initialize_Sets_Scope_If_Cacheable_And_Scoped()");
        }

        [Test]
        public void RepositoryBase_Delete_Clears_Cache_If_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Delete_Clears_Cache_If_Cacheable()");
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(Constants.CACHE_DogsKey);

			Logger.LogDebug("RepositoryBase_Delete_Clears_Cache_If_Cacheable(): cacheKey → {CacheKey}", cacheKey);
            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableDog>());
			Logger.LogDebug("RepositoryBase_Delete_Clears_Cache_If_Cacheable(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

			Logger.LogDebug("RepositoryBase_Delete_Clears_Cache_If_Cacheable(): mockRepository → {MockRepository}", mockRepository);
            // Act
            mockRepository.Object.Delete(new CacheableDog());

            // Assert
            mockCache.Verify(c => c.Remove(cacheKey), Times.Once());
			Logger.LogDebug("<<< Exiting RepositoryBase_Delete_Clears_Cache_If_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Delete_Clears_Cache_If_Cacheable_And_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Delete_Clears_Cache_If_Cacheable_And_Scoped()");
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

			Logger.LogDebug("RepositoryBase_Delete_Clears_Cache_If_Cacheable_And_Scoped(): cacheKey → {CacheKey}", cacheKey);
            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());
			Logger.LogDebug("RepositoryBase_Delete_Clears_Cache_If_Cacheable_And_Scoped(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

			Logger.LogDebug("RepositoryBase_Delete_Clears_Cache_If_Cacheable_And_Scoped(): mockRepository → {MockRepository}", mockRepository);
            // Act
            mockRepository.Object.Delete(new CacheableCat { ModuleId = Constants.MODULE_ValidId });

            // Assert
            mockCache.Verify(c => c.Remove(cacheKey), Times.Once());
			Logger.LogDebug("<<< Exiting RepositoryBase_Delete_Clears_Cache_If_Cacheable_And_Scoped()");
        }

        [Test]
        public void RepositoryBase_Delete_Does_Not_Clear_Cache_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Delete_Does_Not_Clear_Cache_If_Not_Cacheable()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

			Logger.LogDebug("RepositoryBase_Delete_Does_Not_Clear_Cache_If_Not_Cacheable(): mockCache → {MockCache}", mockCache);
            var mockRepository = new Mock<RepositoryBase<Dog>>();

			Logger.LogDebug("RepositoryBase_Delete_Does_Not_Clear_Cache_If_Not_Cacheable(): mockRepository → {MockRepository}", mockRepository);
            // Act
            mockRepository.Object.Delete(new Dog());

            // Assert
            mockCache.Verify(c => c.Remove(It.IsAny<string>()), Times.Never());
			Logger.LogDebug("<<< Exiting RepositoryBase_Delete_Does_Not_Clear_Cache_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Delete_Calls_DeleteInternal()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Delete_Calls_DeleteInternal()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();
            mockRepository.Protected().Setup("DeleteInternal", ItExpr.IsAny<Dog>());

			Logger.LogDebug("RepositoryBase_Delete_Calls_DeleteInternal(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_Delete_Calls_DeleteInternal(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            mockRepository.Object.Delete(new Dog());

            // Assert
            mockRepository.Protected().Verify("DeleteInternal", Times.Once(), ItExpr.IsAny<Dog>());
			Logger.LogDebug("<<< Exiting RepositoryBase_Delete_Calls_DeleteInternal()");
        }

        [Test]
        public void RepositoryBase_Get_Checks_Cache_If_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Get_Checks_Cache_If_Cacheable()");
            // Arrange
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");
			Logger.LogDebug("RepositoryBase_Get_Checks_Cache_If_Cacheable(): mockHostController → {MockHostController}", mockHostController);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey))).Returns(new List<CacheableDog>());
			Logger.LogDebug("RepositoryBase_Get_Checks_Cache_If_Cacheable(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

			Logger.LogDebug("RepositoryBase_Get_Checks_Cache_If_Cacheable(): mockRepository → {MockRepository}", mockRepository);
            // Act
            var list = mockRepository.Object.Get();

			Logger.LogDebug("RepositoryBase_Get_Checks_Cache_If_Cacheable(): list → {List}", list);
            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)));
			Logger.LogDebug("<<< Exiting RepositoryBase_Get_Checks_Cache_If_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Get_Does_Not_Check_Cache_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Get_Does_Not_Check_Cache_If_Not_Cacheable()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

			Logger.LogDebug("RepositoryBase_Get_Does_Not_Check_Cache_If_Not_Cacheable(): mockCache → {MockCache}", mockCache);
            var mockRepository = new Mock<RepositoryBase<Dog>>();

			Logger.LogDebug("RepositoryBase_Get_Does_Not_Check_Cache_If_Not_Cacheable(): mockRepository → {MockRepository}", mockRepository);
            // Act
            var list = mockRepository.Object.Get();

			Logger.LogDebug("RepositoryBase_Get_Does_Not_Check_Cache_If_Not_Cacheable(): list → {List}", list);
            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)), Times.Never());
			Logger.LogDebug("<<< Exiting RepositoryBase_Get_Does_Not_Check_Cache_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Get_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Get_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

			Logger.LogDebug("RepositoryBase_Get_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped(): mockCache → {MockCache}", mockCache);
            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

			Logger.LogDebug("RepositoryBase_Get_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped(): mockRepository → {MockRepository}", mockRepository);
            // Act
            var list = mockRepository.Object.Get();

			Logger.LogDebug("RepositoryBase_Get_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped(): list → {List}", list);
            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_CatsKey)), Times.Never());
			Logger.LogDebug("<<< Exiting RepositoryBase_Get_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_And_Cache_Expired()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_And_Cache_Expired()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");
			Logger.LogDebug("RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_And_Cache_Expired(): mockHostController → {MockHostController}", mockHostController);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey))).Returns(null);
			Logger.LogDebug("RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_And_Cache_Expired(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();
            mockRepository.Protected().Setup("GetInternal");

			Logger.LogDebug("RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_And_Cache_Expired(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_And_Cache_Expired(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            var list = mockRepository.Object.Get();

			Logger.LogDebug("RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_And_Cache_Expired(): list → {List}", list);
            // Assert
            mockRepository.Protected().Verify("GetInternal", Times.Once());
			Logger.LogDebug("<<< Exiting RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_And_Cache_Expired()");
        }

        [Test]
        public void RepositoryBase_Get_Calls_GetAllInternal_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Get_Calls_GetAllInternal_If_Not_Cacheable()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();
            mockRepository.Protected().Setup("GetInternal");

			Logger.LogDebug("RepositoryBase_Get_Calls_GetAllInternal_If_Not_Cacheable(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_Get_Calls_GetAllInternal_If_Not_Cacheable(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            var list = mockRepository.Object.Get();

			Logger.LogDebug("RepositoryBase_Get_Calls_GetAllInternal_If_Not_Cacheable(): list → {List}", list);
            // Assert
            mockRepository.Protected().Verify("GetInternal", Times.Once());
			Logger.LogDebug("<<< Exiting RepositoryBase_Get_Calls_GetAllInternal_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_But_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_But_Not_Scoped()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();
            mockRepository.Protected().Setup("GetInternal");

			Logger.LogDebug("RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_But_Not_Scoped(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_But_Not_Scoped(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            var list = mockRepository.Object.Get();

			Logger.LogDebug("RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_But_Not_Scoped(): list → {List}", list);
            // Assert
            mockRepository.Protected().Verify("GetInternal", Times.Once());
			Logger.LogDebug("<<< Exiting RepositoryBase_Get_Calls_GetAllInternal_If_Cacheable_But_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_Get_Does_Not_Call_GetAllInternal_If_Cacheable_And_Cache_Valid()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Get_Does_Not_Call_GetAllInternal_If_Cacheable_And_Cache_Valid()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");
			Logger.LogDebug("RepositoryBase_Get_Does_Not_Call_GetAllInternal_If_Cacheable_And_Cache_Valid(): mockHostController → {MockHostController}", mockHostController);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey))).Returns(new List<CacheableDog>());
			Logger.LogDebug("RepositoryBase_Get_Does_Not_Call_GetAllInternal_If_Cacheable_And_Cache_Valid(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();
            mockRepository.Protected().Setup("GetInternal");

			Logger.LogDebug("RepositoryBase_Get_Does_Not_Call_GetAllInternal_If_Cacheable_And_Cache_Valid(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_Get_Does_Not_Call_GetAllInternal_If_Cacheable_And_Cache_Valid(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            var list = mockRepository.Object.Get();

			Logger.LogDebug("RepositoryBase_Get_Does_Not_Call_GetAllInternal_If_Cacheable_And_Cache_Valid(): list → {List}", list);
            // Assert
            mockRepository.Protected().Verify("GetInternal", Times.Never());
			Logger.LogDebug("<<< Exiting RepositoryBase_Get_Does_Not_Call_GetAllInternal_If_Cacheable_And_Cache_Valid()");
        }

        [Test]
        public void RepositoryBase_Get_Overload_Checks_Cache_If_Cacheable_And_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Get_Overload_Checks_Cache_If_Cacheable_And_Scoped()");
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

			Logger.LogDebug("RepositoryBase_Get_Overload_Checks_Cache_If_Cacheable_And_Scoped(): cacheKey → {CacheKey}", cacheKey);
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");
			Logger.LogDebug("RepositoryBase_Get_Overload_Checks_Cache_If_Cacheable_And_Scoped(): mockHostController → {MockHostController}", mockHostController);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());
			Logger.LogDebug("RepositoryBase_Get_Overload_Checks_Cache_If_Cacheable_And_Scoped(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

			Logger.LogDebug("RepositoryBase_Get_Overload_Checks_Cache_If_Cacheable_And_Scoped(): mockRepository → {MockRepository}", mockRepository);
            // Act
            var list = mockRepository.Object.Get<int>(Constants.MODULE_ValidId);

			Logger.LogDebug("RepositoryBase_Get_Overload_Checks_Cache_If_Cacheable_And_Scoped(): list → {List}", list);
            // Assert
            mockCache.Verify(c => c.GetItem(cacheKey));
			Logger.LogDebug("<<< Exiting RepositoryBase_Get_Overload_Checks_Cache_If_Cacheable_And_Scoped()");
        }

        [Test]
        public void RepositoryBase_Get_Overload_Throws_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Get_Overload_Throws_If_Not_Cacheable()");
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();

			Logger.LogDebug("RepositoryBase_Get_Overload_Throws_If_Not_Cacheable(): mockRepository → {MockRepository}", mockRepository);
            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.Get<int>(Constants.MODULE_ValidId));
			Logger.LogDebug("<<< Exiting RepositoryBase_Get_Overload_Throws_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Get_Overload_Throws_If_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Get_Overload_Throws_If_Not_Scoped()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

			Logger.LogDebug("RepositoryBase_Get_Overload_Throws_If_Not_Scoped(): mockCache → {MockCache}", mockCache);
            var mockRepository = new Mock<RepositoryBase<Dog>>();

			Logger.LogDebug("RepositoryBase_Get_Overload_Throws_If_Not_Scoped(): mockRepository → {MockRepository}", mockRepository);
            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.Get<int>(Constants.MODULE_ValidId));
			Logger.LogDebug("<<< Exiting RepositoryBase_Get_Overload_Throws_If_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_Get_Overload_Throws_If_Cacheable_But_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Get_Overload_Throws_If_Cacheable_But_Not_Scoped()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

			Logger.LogDebug("RepositoryBase_Get_Overload_Throws_If_Cacheable_But_Not_Scoped(): mockCache → {MockCache}", mockCache);
            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

			Logger.LogDebug("RepositoryBase_Get_Overload_Throws_If_Cacheable_But_Not_Scoped(): mockRepository → {MockRepository}", mockRepository);
            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.Get<int>(Constants.MODULE_ValidId));
			Logger.LogDebug("<<< Exiting RepositoryBase_Get_Overload_Throws_If_Cacheable_But_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Not_Cacheable_And_Is_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Not_Cacheable_And_Is_Scoped()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Cat>>();
            mockRepository.Protected().Setup<IEnumerable<Cat>>("GetByScopeInternal", ItExpr.IsAny<object>());

			Logger.LogDebug("RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Not_Cacheable_And_Is_Scoped(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Not_Cacheable_And_Is_Scoped(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            var list = mockRepository.Object.Get<int>(Constants.MODULE_ValidId);

			Logger.LogDebug("RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Not_Cacheable_And_Is_Scoped(): list → {List}", list);
            // Assert
            mockRepository.Protected().Verify<IEnumerable<Cat>>("GetByScopeInternal", Times.Once(), ItExpr.IsAny<object>());
			Logger.LogDebug("<<< Exiting RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Not_Cacheable_And_Is_Scoped()");
        }

        [Test]
        public void RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

			Logger.LogDebug("RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired(): cacheKey → {CacheKey}", cacheKey);
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");
			Logger.LogDebug("RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired(): mockHostController → {MockHostController}", mockHostController);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(cacheKey))).Returns(null);
			Logger.LogDebug("RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();
            mockRepository.Protected().Setup<IEnumerable<CacheableCat>>("GetByScopeInternal", ItExpr.IsAny<object>());

			Logger.LogDebug("RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            var list = mockRepository.Object.Get<int>(Constants.MODULE_ValidId);

			Logger.LogDebug("RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired(): list → {List}", list);
            // Assert
            mockRepository.Protected().Verify<IEnumerable<CacheableCat>>("GetByScopeInternal", Times.Once(), ItExpr.IsAny<object>());
			Logger.LogDebug("<<< Exiting RepositoryBase_Get_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired()");
        }

        [Test]
        public void RepositoryBase_Get_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Get_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

			Logger.LogDebug("RepositoryBase_Get_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid(): cacheKey → {CacheKey}", cacheKey);
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");
			Logger.LogDebug("RepositoryBase_Get_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid(): mockHostController → {MockHostController}", mockHostController);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());
			Logger.LogDebug("RepositoryBase_Get_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();
            mockRepository.Protected().Setup<IEnumerable<CacheableCat>>("GetByScopeInternal", ItExpr.IsAny<object>());

			Logger.LogDebug("RepositoryBase_Get_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_Get_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            var list = mockRepository.Object.Get<int>(Constants.MODULE_ValidId);

			Logger.LogDebug("RepositoryBase_Get_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid(): list → {List}", list);
            // Assert
            mockRepository.Protected().Verify<IEnumerable<CacheableCat>>("GetByScopeInternal", Times.Never(), ItExpr.IsAny<object>());
			Logger.LogDebug("<<< Exiting RepositoryBase_Get_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid()");
        }

        [Test]
        public void RepositoryBase_GetById_Checks_Cache_If_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetById_Checks_Cache_If_Cacheable()");
            // Arrange
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");
			Logger.LogDebug("RepositoryBase_GetById_Checks_Cache_If_Cacheable(): mockHostController → {MockHostController}", mockHostController);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey))).Returns(new List<CacheableDog>());
			Logger.LogDebug("RepositoryBase_GetById_Checks_Cache_If_Cacheable(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

			Logger.LogDebug("RepositoryBase_GetById_Checks_Cache_If_Cacheable(): mockRepository → {MockRepository}", mockRepository);
            // Act
            var dog = mockRepository.Object.GetById(Constants.PETAPOCO_ValidDogId);

			Logger.LogDebug("RepositoryBase_GetById_Checks_Cache_If_Cacheable(): dog → {Dog}", dog);
            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)));
			Logger.LogDebug("<<< Exiting RepositoryBase_GetById_Checks_Cache_If_Cacheable()");
        }

        [Test]
        public void RepositoryBase_GetById_Does_Not_Check_Cache_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetById_Does_Not_Check_Cache_If_Not_Cacheable()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

			Logger.LogDebug("RepositoryBase_GetById_Does_Not_Check_Cache_If_Not_Cacheable(): mockCache → {MockCache}", mockCache);
            var mockRepository = new Mock<RepositoryBase<Dog>>();

			Logger.LogDebug("RepositoryBase_GetById_Does_Not_Check_Cache_If_Not_Cacheable(): mockRepository → {MockRepository}", mockRepository);
            // Act
            var dog = mockRepository.Object.GetById(Constants.PETAPOCO_ValidDogId);

			Logger.LogDebug("RepositoryBase_GetById_Does_Not_Check_Cache_If_Not_Cacheable(): dog → {Dog}", dog);
            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)), Times.Never());
			Logger.LogDebug("<<< Exiting RepositoryBase_GetById_Does_Not_Check_Cache_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_GetById_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetById_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

			Logger.LogDebug("RepositoryBase_GetById_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped(): mockCache → {MockCache}", mockCache);
            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

			Logger.LogDebug("RepositoryBase_GetById_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped(): mockRepository → {MockRepository}", mockRepository);
            // Act
            var cat = mockRepository.Object.GetById(Constants.PETAPOCO_ValidCatId);

			Logger.LogDebug("RepositoryBase_GetById_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped(): cat → {Cat}", cat);
            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)), Times.Never());
			Logger.LogDebug("<<< Exiting RepositoryBase_GetById_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_GetById_Calls_GetByIdInternal_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetById_Calls_GetByIdInternal_If_Not_Cacheable()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();
            mockRepository.Protected().Setup<Dog>("GetByIdInternal", ItExpr.IsAny<object>());

			Logger.LogDebug("RepositoryBase_GetById_Calls_GetByIdInternal_If_Not_Cacheable(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_GetById_Calls_GetByIdInternal_If_Not_Cacheable(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            var dog = mockRepository.Object.GetById(Constants.PETAPOCO_ValidDogId);

			Logger.LogDebug("RepositoryBase_GetById_Calls_GetByIdInternal_If_Not_Cacheable(): dog → {Dog}", dog);
            // Assert
            mockRepository.Protected().Verify<Dog>("GetByIdInternal", Times.Once(), ItExpr.IsAny<object>());
			Logger.LogDebug("<<< Exiting RepositoryBase_GetById_Calls_GetByIdInternal_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_GetById_Calls_GetByIdInternal_If_Cacheable_But_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetById_Calls_GetByIdInternal_If_Cacheable_But_Not_Scoped()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();
            mockRepository.Protected().Setup<CacheableCat>("GetByIdInternal", ItExpr.IsAny<object>());

			Logger.LogDebug("RepositoryBase_GetById_Calls_GetByIdInternal_If_Cacheable_But_Not_Scoped(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_GetById_Calls_GetByIdInternal_If_Cacheable_But_Not_Scoped(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            var cat = mockRepository.Object.GetById(Constants.PETAPOCO_ValidCatId);

			Logger.LogDebug("RepositoryBase_GetById_Calls_GetByIdInternal_If_Cacheable_But_Not_Scoped(): cat → {Cat}", cat);
            // Assert
            mockRepository.Protected().Verify<CacheableCat>("GetByIdInternal", Times.Once(), ItExpr.IsAny<object>());
			Logger.LogDebug("<<< Exiting RepositoryBase_GetById_Calls_GetByIdInternal_If_Cacheable_But_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_GetById_Does_Not_Call_GetByIdInternal_If_Cacheable_And_Cache_Valid()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetById_Does_Not_Call_GetByIdInternal_If_Cacheable_And_Cache_Valid()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");
			Logger.LogDebug("RepositoryBase_GetById_Does_Not_Call_GetByIdInternal_If_Cacheable_And_Cache_Valid(): mockHostController → {MockHostController}", mockHostController);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey))).Returns(new List<CacheableDog>());
			Logger.LogDebug("RepositoryBase_GetById_Does_Not_Call_GetByIdInternal_If_Cacheable_And_Cache_Valid(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();
            mockRepository.Protected().Setup<CacheableDog>("GetByIdInternal", ItExpr.IsAny<object>());

			Logger.LogDebug("RepositoryBase_GetById_Does_Not_Call_GetByIdInternal_If_Cacheable_And_Cache_Valid(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_GetById_Does_Not_Call_GetByIdInternal_If_Cacheable_And_Cache_Valid(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            var dog = mockRepository.Object.GetById(Constants.PETAPOCO_ValidDogId);

			Logger.LogDebug("RepositoryBase_GetById_Does_Not_Call_GetByIdInternal_If_Cacheable_And_Cache_Valid(): dog → {Dog}", dog);
            // Assert
            mockRepository.Protected().Verify<CacheableDog>("GetByIdInternal", Times.Never(), ItExpr.IsAny<object>());
			Logger.LogDebug("<<< Exiting RepositoryBase_GetById_Does_Not_Call_GetByIdInternal_If_Cacheable_And_Cache_Valid()");
        }

        [Test]
        public void RepositoryBase_GetById_Overload_Checks_Cache_If_Cacheable_And_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetById_Overload_Checks_Cache_If_Cacheable_And_Scoped()");
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

			Logger.LogDebug("RepositoryBase_GetById_Overload_Checks_Cache_If_Cacheable_And_Scoped(): cacheKey → {CacheKey}", cacheKey);
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");
			Logger.LogDebug("RepositoryBase_GetById_Overload_Checks_Cache_If_Cacheable_And_Scoped(): mockHostController → {MockHostController}", mockHostController);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());
			Logger.LogDebug("RepositoryBase_GetById_Overload_Checks_Cache_If_Cacheable_And_Scoped(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

			Logger.LogDebug("RepositoryBase_GetById_Overload_Checks_Cache_If_Cacheable_And_Scoped(): mockRepository → {MockRepository}", mockRepository);
            // Act
            var cat = mockRepository.Object.GetById(Constants.PETAPOCO_ValidCatId, Constants.MODULE_ValidId);

			Logger.LogDebug("RepositoryBase_GetById_Overload_Checks_Cache_If_Cacheable_And_Scoped(): cat → {Cat}", cat);
            // Assert
            mockCache.Verify(c => c.GetItem(cacheKey));
			Logger.LogDebug("<<< Exiting RepositoryBase_GetById_Overload_Checks_Cache_If_Cacheable_And_Scoped()");
        }

        [Test]
        public void RepositoryBase_GetById_Overload_Throws_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetById_Overload_Throws_If_Not_Cacheable()");
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();

			Logger.LogDebug("RepositoryBase_GetById_Overload_Throws_If_Not_Cacheable(): mockRepository → {MockRepository}", mockRepository);
            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.GetById(Constants.PETAPOCO_ValidDogId, Constants.MODULE_ValidId));
			Logger.LogDebug("<<< Exiting RepositoryBase_GetById_Overload_Throws_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_GetById_Overload_Throws__If_Cacheable_But_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetById_Overload_Throws__If_Cacheable_But_Not_Scoped()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

			Logger.LogDebug("RepositoryBase_GetById_Overload_Throws__If_Cacheable_But_Not_Scoped(): mockCache → {MockCache}", mockCache);
            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

			Logger.LogDebug("RepositoryBase_GetById_Overload_Throws__If_Cacheable_But_Not_Scoped(): mockRepository → {MockRepository}", mockRepository);
            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.GetById(Constants.PETAPOCO_ValidDogId, Constants.MODULE_ValidId));
			Logger.LogDebug("<<< Exiting RepositoryBase_GetById_Overload_Throws__If_Cacheable_But_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_GetPage_Checks_Cache_If_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetPage_Checks_Cache_If_Cacheable()");
            // Arrange
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");
			Logger.LogDebug("RepositoryBase_GetPage_Checks_Cache_If_Cacheable(): mockHostController → {MockHostController}", mockHostController);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey))).Returns(new List<CacheableDog>());
			Logger.LogDebug("RepositoryBase_GetPage_Checks_Cache_If_Cacheable(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

			Logger.LogDebug("RepositoryBase_GetPage_Checks_Cache_If_Cacheable(): mockRepository → {MockRepository}", mockRepository);
            // Act
            var dogs = mockRepository.Object.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

			Logger.LogDebug("RepositoryBase_GetPage_Checks_Cache_If_Cacheable(): dogs → {Dogs}", dogs);
            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)));
			Logger.LogDebug("<<< Exiting RepositoryBase_GetPage_Checks_Cache_If_Cacheable()");
        }

        [Test]
        public void RepositoryBase_GetPage_Does_Not_Check_Cache_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetPage_Does_Not_Check_Cache_If_Not_Cacheable()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

			Logger.LogDebug("RepositoryBase_GetPage_Does_Not_Check_Cache_If_Not_Cacheable(): mockCache → {MockCache}", mockCache);
            var mockRepository = new Mock<RepositoryBase<Dog>>();

			Logger.LogDebug("RepositoryBase_GetPage_Does_Not_Check_Cache_If_Not_Cacheable(): mockRepository → {MockRepository}", mockRepository);
            // Act
            var dogs = mockRepository.Object.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

			Logger.LogDebug("RepositoryBase_GetPage_Does_Not_Check_Cache_If_Not_Cacheable(): dogs → {Dogs}", dogs);
            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)), Times.Never());
			Logger.LogDebug("<<< Exiting RepositoryBase_GetPage_Does_Not_Check_Cache_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_GetPage_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetPage_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

			Logger.LogDebug("RepositoryBase_GetPage_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped(): mockCache → {MockCache}", mockCache);
            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

			Logger.LogDebug("RepositoryBase_GetPage_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped(): mockRepository → {MockRepository}", mockRepository);
            // Act
            var cats = mockRepository.Object.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

			Logger.LogDebug("RepositoryBase_GetPage_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped(): cats → {Cats}", cats);
            // Assert
            mockCache.Verify(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)), Times.Never());
			Logger.LogDebug("<<< Exiting RepositoryBase_GetPage_Does_Not_Check_Cache_If_Cacheable_But_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_GetPage_Calls_GetAllByPageInternal_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetPage_Calls_GetAllByPageInternal_If_Not_Cacheable()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();
            mockRepository.Protected().Setup<IPagedList<Dog>>("GetPageInternal", ItExpr.IsAny<int>(), ItExpr.IsAny<int>());

			Logger.LogDebug("RepositoryBase_GetPage_Calls_GetAllByPageInternal_If_Not_Cacheable(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_GetPage_Calls_GetAllByPageInternal_If_Not_Cacheable(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            var dogs = mockRepository.Object.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

			Logger.LogDebug("RepositoryBase_GetPage_Calls_GetAllByPageInternal_If_Not_Cacheable(): dogs → {Dogs}", dogs);
            // Assert
            mockRepository.Protected().Verify<IPagedList<Dog>>("GetPageInternal", Times.Once(), ItExpr.IsAny<int>(), ItExpr.IsAny<int>());
			Logger.LogDebug("<<< Exiting RepositoryBase_GetPage_Calls_GetAllByPageInternal_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_GetPage_Calls_GetAllByPageInternal_If_Cacheable_But_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetPage_Calls_GetAllByPageInternal_If_Cacheable_But_Not_Scoped()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();
            mockRepository.Protected().Setup<IPagedList<CacheableCat>>("GetPageInternal", ItExpr.IsAny<int>(), ItExpr.IsAny<int>());

			Logger.LogDebug("RepositoryBase_GetPage_Calls_GetAllByPageInternal_If_Cacheable_But_Not_Scoped(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_GetPage_Calls_GetAllByPageInternal_If_Cacheable_But_Not_Scoped(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            var cats = mockRepository.Object.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

			Logger.LogDebug("RepositoryBase_GetPage_Calls_GetAllByPageInternal_If_Cacheable_But_Not_Scoped(): cats → {Cats}", cats);
            // Assert
            mockRepository.Protected().Verify<IPagedList<CacheableCat>>("GetPageInternal", Times.Once(), ItExpr.IsAny<int>(), ItExpr.IsAny<int>());
			Logger.LogDebug("<<< Exiting RepositoryBase_GetPage_Calls_GetAllByPageInternal_If_Cacheable_But_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_GetPage_Does_Not_Call_GetAllByPageInternal_If_Cacheable_And_Cache_Valid()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetPage_Does_Not_Call_GetAllByPageInternal_If_Cacheable_And_Cache_Valid()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");
			Logger.LogDebug("RepositoryBase_GetPage_Does_Not_Call_GetAllByPageInternal_If_Cacheable_And_Cache_Valid(): mockHostController → {MockHostController}", mockHostController);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(Constants.CACHE_DogsKey)))
                        .Returns(new List<CacheableDog>() { new CacheableDog() });
						Logger.LogDebug("RepositoryBase_GetPage_Does_Not_Call_GetAllByPageInternal_If_Cacheable_And_Cache_Valid(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();
            mockRepository.Protected().Setup<IPagedList<CacheableDog>>("GetPageInternal", ItExpr.IsAny<int>(), ItExpr.IsAny<int>());

			Logger.LogDebug("RepositoryBase_GetPage_Does_Not_Call_GetAllByPageInternal_If_Cacheable_And_Cache_Valid(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_GetPage_Does_Not_Call_GetAllByPageInternal_If_Cacheable_And_Cache_Valid(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            var dogs = mockRepository.Object.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

			Logger.LogDebug("RepositoryBase_GetPage_Does_Not_Call_GetAllByPageInternal_If_Cacheable_And_Cache_Valid(): dogs → {Dogs}", dogs);
            // Assert
            mockRepository.Protected().Verify<IPagedList<CacheableDog>>("GetPageInternal", Times.Never(), ItExpr.IsAny<int>(), ItExpr.IsAny<int>());
			Logger.LogDebug("<<< Exiting RepositoryBase_GetPage_Does_Not_Call_GetAllByPageInternal_If_Cacheable_And_Cache_Valid()");
        }

        [Test]
        public void RepositoryBase_GetPage_Overload_Checks_Cache_If_Cacheable_And_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetPage_Overload_Checks_Cache_If_Cacheable_And_Scoped()");
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

			Logger.LogDebug("RepositoryBase_GetPage_Overload_Checks_Cache_If_Cacheable_And_Scoped(): cacheKey → {CacheKey}", cacheKey);
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");
			Logger.LogDebug("RepositoryBase_GetPage_Overload_Checks_Cache_If_Cacheable_And_Scoped(): mockHostController → {MockHostController}", mockHostController);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());
			Logger.LogDebug("RepositoryBase_GetPage_Overload_Checks_Cache_If_Cacheable_And_Scoped(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

			Logger.LogDebug("RepositoryBase_GetPage_Overload_Checks_Cache_If_Cacheable_And_Scoped(): mockRepository → {MockRepository}", mockRepository);
            // Act
            var cats = mockRepository.Object.GetPage<int>(Constants.MODULE_ValidId, Constants.PAGE_First, Constants.PAGE_RecordCount);

			Logger.LogDebug("RepositoryBase_GetPage_Overload_Checks_Cache_If_Cacheable_And_Scoped(): cats → {Cats}", cats);
            // Assert
            mockCache.Verify(c => c.GetItem(cacheKey));
			Logger.LogDebug("<<< Exiting RepositoryBase_GetPage_Overload_Checks_Cache_If_Cacheable_And_Scoped()");
        }

        [Test]
        public void RepositoryBase_GetPage_Overload_Throws_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetPage_Overload_Throws_If_Not_Cacheable()");
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();

			Logger.LogDebug("RepositoryBase_GetPage_Overload_Throws_If_Not_Cacheable(): mockRepository → {MockRepository}", mockRepository);
            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.GetPage<int>(Constants.MODULE_ValidId, Constants.PAGE_First, Constants.PAGE_RecordCount));
			Logger.LogDebug("<<< Exiting RepositoryBase_GetPage_Overload_Throws_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_GetPage_Overload_Throws_If_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetPage_Overload_Throws_If_Not_Scoped()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

			Logger.LogDebug("RepositoryBase_GetPage_Overload_Throws_If_Not_Scoped(): mockCache → {MockCache}", mockCache);
            var mockRepository = new Mock<RepositoryBase<Dog>>();

			Logger.LogDebug("RepositoryBase_GetPage_Overload_Throws_If_Not_Scoped(): mockRepository → {MockRepository}", mockRepository);
            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.GetPage<int>(Constants.MODULE_ValidId, Constants.PAGE_First, Constants.PAGE_RecordCount));
			Logger.LogDebug("<<< Exiting RepositoryBase_GetPage_Overload_Throws_If_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_GetPage_Overload_Throws_If_Cacheable_But_Not_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetPage_Overload_Throws_If_Cacheable_But_Not_Scoped()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

			Logger.LogDebug("RepositoryBase_GetPage_Overload_Throws_If_Cacheable_But_Not_Scoped(): mockCache → {MockCache}", mockCache);
            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

			Logger.LogDebug("RepositoryBase_GetPage_Overload_Throws_If_Cacheable_But_Not_Scoped(): mockRepository → {MockRepository}", mockRepository);
            // Act, Assert
            Assert.Throws<NotSupportedException>(() => mockRepository.Object.GetPage<int>(Constants.MODULE_ValidId, Constants.PAGE_First, Constants.PAGE_RecordCount));
			Logger.LogDebug("<<< Exiting RepositoryBase_GetPage_Overload_Throws_If_Cacheable_But_Not_Scoped()");
        }

        [Test]
        public void RepositoryBase_GetPage_Overload_Calls_GetAllByScopeAndPageInternal_If_Not_Cacheable_And_Is_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetPage_Overload_Calls_GetAllByScopeAndPageInternal_If_Not_Cacheable_And_Is_Scoped()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Cat>>();
            mockRepository.Protected().Setup<IEnumerable<Cat>>("GetPageByScopeInternal", ItExpr.IsAny<object>(), ItExpr.IsAny<int>(), ItExpr.IsAny<int>());

			Logger.LogDebug("RepositoryBase_GetPage_Overload_Calls_GetAllByScopeAndPageInternal_If_Not_Cacheable_And_Is_Scoped(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_GetPage_Overload_Calls_GetAllByScopeAndPageInternal_If_Not_Cacheable_And_Is_Scoped(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            var cats = mockRepository.Object.GetPage<int>(Constants.MODULE_ValidId, Constants.PAGE_First, Constants.PAGE_RecordCount);

			Logger.LogDebug("RepositoryBase_GetPage_Overload_Calls_GetAllByScopeAndPageInternal_If_Not_Cacheable_And_Is_Scoped(): cats → {Cats}", cats);
            // Assert
            mockRepository.Protected().Verify<IEnumerable<Cat>>("GetPageByScopeInternal", Times.Once(), ItExpr.IsAny<object>(), ItExpr.IsAny<int>(), ItExpr.IsAny<int>());
			Logger.LogDebug("<<< Exiting RepositoryBase_GetPage_Overload_Calls_GetAllByScopeAndPageInternal_If_Not_Cacheable_And_Is_Scoped()");
        }

        [Test]
        public void RepositoryBase_GetPage_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetPage_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

			Logger.LogDebug("RepositoryBase_GetPage_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired(): cacheKey → {CacheKey}", cacheKey);
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("0");
			Logger.LogDebug("RepositoryBase_GetPage_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired(): mockHostController → {MockHostController}", mockHostController);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(cacheKey))).Returns(null);
			Logger.LogDebug("RepositoryBase_GetPage_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();
            mockRepository.Protected().Setup<IEnumerable<CacheableCat>>("GetByScopeInternal", ItExpr.IsAny<object>())
                                    .Returns(new List<CacheableCat>());
									Logger.LogDebug("RepositoryBase_GetPage_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired(): mockRepository → {MockRepository}", mockRepository);
									Logger.LogInformation("RepositoryBase_GetPage_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            var mockData = MockComponentProvider.CreateDataProvider();
            mockData.Setup(d => d.GetProviderPath()).Returns(string.Empty);
			Logger.LogDebug("RepositoryBase_GetPage_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired(): mockData → {MockData}", mockData);

            // Act
            var cats = mockRepository.Object.GetPage<int>(Constants.MODULE_ValidId, Constants.PAGE_First, Constants.PAGE_RecordCount);

			Logger.LogDebug("RepositoryBase_GetPage_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired(): cats → {Cats}", cats);
            // Assert
            mockRepository.Protected().Verify<IEnumerable<CacheableCat>>("GetByScopeInternal", Times.Once(), ItExpr.IsAny<object>());
			Logger.LogDebug("<<< Exiting RepositoryBase_GetPage_Overload_Calls_GetAllByScopeInternal_If_Cacheable_And_Cache_Expired()");
        }

        [Test]
        public void RepositoryBase_GetPage_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_GetPage_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

			Logger.LogDebug("RepositoryBase_GetPage_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid(): cacheKey → {CacheKey}", cacheKey);
            var mockHostController = MockComponentProvider.CreateNew<IHostController>();
            mockHostController.Setup(h => h.GetString("PerformanceSetting")).Returns("3");
			Logger.LogDebug("RepositoryBase_GetPage_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid(): mockHostController → {MockHostController}", mockHostController);

            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());
			Logger.LogDebug("RepositoryBase_GetPage_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();
            mockRepository.Protected().Setup<IEnumerable<CacheableCat>>("GetByScopeInternal", ItExpr.IsAny<object>());

			Logger.LogDebug("RepositoryBase_GetPage_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_GetPage_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            var cats = mockRepository.Object.GetPage<int>(Constants.MODULE_ValidId, Constants.PAGE_First, Constants.PAGE_RecordCount);

			Logger.LogDebug("RepositoryBase_GetPage_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid(): cats → {Cats}", cats);
            // Assert
            mockRepository.Protected().Verify<IEnumerable<CacheableCat>>("GetByScopeInternal", Times.Never(), ItExpr.IsAny<object>());
			Logger.LogDebug("<<< Exiting RepositoryBase_GetPage_Overload_Does_Not_Call_GetAllByScopeInternal_If_Cacheable_And_Cache_Valid()");
        }

        [Test]
        public void RepositoryBase_Insert_Clears_Cache_If_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Insert_Clears_Cache_If_Cacheable()");
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(Constants.CACHE_DogsKey);

			Logger.LogDebug("RepositoryBase_Insert_Clears_Cache_If_Cacheable(): cacheKey → {CacheKey}", cacheKey);
            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableDog>());
			Logger.LogDebug("RepositoryBase_Insert_Clears_Cache_If_Cacheable(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

			Logger.LogDebug("RepositoryBase_Insert_Clears_Cache_If_Cacheable(): mockRepository → {MockRepository}", mockRepository);
            // Act
            mockRepository.Object.Insert(new CacheableDog());

            // Assert
            mockCache.Verify(c => c.Remove(cacheKey), Times.Once());
			Logger.LogDebug("<<< Exiting RepositoryBase_Insert_Clears_Cache_If_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Insert_Clears_Cache_If_Cacheable_And_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Insert_Clears_Cache_If_Cacheable_And_Scoped()");
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

			Logger.LogDebug("RepositoryBase_Insert_Clears_Cache_If_Cacheable_And_Scoped(): cacheKey → {CacheKey}", cacheKey);
            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());
			Logger.LogDebug("RepositoryBase_Insert_Clears_Cache_If_Cacheable_And_Scoped(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

			Logger.LogDebug("RepositoryBase_Insert_Clears_Cache_If_Cacheable_And_Scoped(): mockRepository → {MockRepository}", mockRepository);
            // Act
            mockRepository.Object.Insert(new CacheableCat { ModuleId = Constants.MODULE_ValidId });

            // Assert
            mockCache.Verify(c => c.Remove(cacheKey), Times.Once());
			Logger.LogDebug("<<< Exiting RepositoryBase_Insert_Clears_Cache_If_Cacheable_And_Scoped()");
        }

        [Test]
        public void RepositoryBase_Insert_Does_Not_Clear_Cache_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Insert_Does_Not_Clear_Cache_If_Not_Cacheable()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

			Logger.LogDebug("RepositoryBase_Insert_Does_Not_Clear_Cache_If_Not_Cacheable(): mockCache → {MockCache}", mockCache);
            var mockRepository = new Mock<RepositoryBase<Dog>>();

			Logger.LogDebug("RepositoryBase_Insert_Does_Not_Clear_Cache_If_Not_Cacheable(): mockRepository → {MockRepository}", mockRepository);
            // Act
            mockRepository.Object.Insert(new Dog());

            // Assert
            mockCache.Verify(c => c.Remove(It.IsAny<string>()), Times.Never());
			Logger.LogDebug("<<< Exiting RepositoryBase_Insert_Does_Not_Clear_Cache_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Insert_Calls_InsertInternal()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Insert_Calls_InsertInternal()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();
            mockRepository.Protected().Setup("InsertInternal", ItExpr.IsAny<Dog>());

			Logger.LogDebug("RepositoryBase_Insert_Calls_InsertInternal(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_Insert_Calls_InsertInternal(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            mockRepository.Object.Insert(new Dog());

            // Assert
            mockRepository.Protected().Verify("InsertInternal", Times.Once(), ItExpr.IsAny<Dog>());
			Logger.LogDebug("<<< Exiting RepositoryBase_Insert_Calls_InsertInternal()");
        }

        [Test]
        public void RepositoryBase_Update_Clears_Cache_If_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Update_Clears_Cache_If_Cacheable()");
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(Constants.CACHE_DogsKey);

			Logger.LogDebug("RepositoryBase_Update_Clears_Cache_If_Cacheable(): cacheKey → {CacheKey}", cacheKey);
            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableDog>());
			Logger.LogDebug("RepositoryBase_Update_Clears_Cache_If_Cacheable(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableDog>>();

			Logger.LogDebug("RepositoryBase_Update_Clears_Cache_If_Cacheable(): mockRepository → {MockRepository}", mockRepository);
            // Act
            mockRepository.Object.Update(new CacheableDog());

            // Assert
            mockCache.Verify(c => c.Remove(cacheKey), Times.Once());
			Logger.LogDebug("<<< Exiting RepositoryBase_Update_Clears_Cache_If_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Update_Clears_Cache_If_Cacheable_And_Scoped()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Update_Clears_Cache_If_Cacheable_And_Scoped()");
            // Arrange
            var cacheKey = CachingProvider.GetCacheKey(string.Format(Constants.CACHE_CatsKey + "_" + Constants.CACHE_ScopeModule + "_{0}", Constants.MODULE_ValidId));

			Logger.LogDebug("RepositoryBase_Update_Clears_Cache_If_Cacheable_And_Scoped(): cacheKey → {CacheKey}", cacheKey);
            var mockCache = MockComponentProvider.CreateDataCacheProvider();
            mockCache.Setup(c => c.GetItem(cacheKey)).Returns(new List<CacheableCat>());
			Logger.LogDebug("RepositoryBase_Update_Clears_Cache_If_Cacheable_And_Scoped(): mockCache → {MockCache}", mockCache);

            var mockRepository = new Mock<RepositoryBase<CacheableCat>>();

			Logger.LogDebug("RepositoryBase_Update_Clears_Cache_If_Cacheable_And_Scoped(): mockRepository → {MockRepository}", mockRepository);
            // Act
            mockRepository.Object.Update(new CacheableCat { ModuleId = Constants.MODULE_ValidId });

            // Assert
            mockCache.Verify(c => c.Remove(cacheKey), Times.Once());
			Logger.LogDebug("<<< Exiting RepositoryBase_Update_Clears_Cache_If_Cacheable_And_Scoped()");
        }

        [Test]
        public void RepositoryBase_Update_Does_Not_Clear_Cache_If_Not_Cacheable()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Update_Does_Not_Clear_Cache_If_Not_Cacheable()");
            // Arrange
            var mockCache = MockComponentProvider.CreateDataCacheProvider();

			Logger.LogDebug("RepositoryBase_Update_Does_Not_Clear_Cache_If_Not_Cacheable(): mockCache → {MockCache}", mockCache);
            var mockRepository = new Mock<RepositoryBase<Dog>>();

			Logger.LogDebug("RepositoryBase_Update_Does_Not_Clear_Cache_If_Not_Cacheable(): mockRepository → {MockRepository}", mockRepository);
            // Act
            mockRepository.Object.Update(new Dog());

            // Assert
            mockCache.Verify(c => c.Remove(It.IsAny<string>()), Times.Never());
			Logger.LogDebug("<<< Exiting RepositoryBase_Update_Does_Not_Clear_Cache_If_Not_Cacheable()");
        }

        [Test]
        public void RepositoryBase_Update_Calls_UpdateInternal()
        {
			Logger.LogDebug(">>> Entering RepositoryBase_Update_Calls_UpdateInternal()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var mockRepository = new Mock<RepositoryBase<Dog>>();
            mockRepository.Protected().Setup("UpdateInternal", ItExpr.IsAny<Dog>());

			Logger.LogDebug("RepositoryBase_Update_Calls_UpdateInternal(): mockRepository → {MockRepository}", mockRepository);
			Logger.LogInformation("RepositoryBase_Update_Calls_UpdateInternal(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Act
            mockRepository.Object.Update(new Dog());

            // Assert
            mockRepository.Protected().Verify("UpdateInternal", Times.Once(), ItExpr.IsAny<Dog>());
			Logger.LogDebug("<<< Exiting RepositoryBase_Update_Calls_UpdateInternal()");
        }

        // ReSharper restore InconsistentNaming
    }
}
