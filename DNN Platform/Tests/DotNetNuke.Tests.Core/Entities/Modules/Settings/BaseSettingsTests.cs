// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;

    public abstract class BaseSettingsTests
    {
        public static readonly object[] SettingsCases =
        {
            new object[] { "AbcdeF#2@kfdfdfds", 9, 1.45, false, new DateTime(2015, 11, 30, 13, 45, 16), TimeSpan.Zero, TestingEnum.Value1, default(ComplexType), },
            new object[] { "Bsskk41233[]#%&", -5, -13456.456, true, DateTime.Today.AddDays(-1), new TimeSpan(1, 5, 6, 7), TestingEnum.Value2, new ComplexType(8, -10), },
        };

        protected const string SettingNamePrefix = "UnitTestSetting_";
        protected const int ModuleId = 1234;
        protected const int TabModuleId = 653;
        protected const int TabId = 344597;
        protected const int PortalId = 246;

        protected MockRepository MockRepository;
        protected Hashtable MockCacheCollection;
        protected Mock<CachingProvider> MockCache;
        protected Mock<IHostController> MockHostController;
        protected Mock<IModuleController> MockModuleController;
        protected Mock<IPortalController> MockPortalController;
        private FakeServiceProvider serviceProvider;

        public enum TestingEnum
        {
			private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
            Value1 = 0,
            Value2 = 1,
        }

        protected static ModuleInfo GetModuleInfo => new ModuleInfo { ModuleID = ModuleId, TabModuleID = TabModuleId, TabID = TabId, PortalID = PortalId, };

        [OneTimeSetUp]
        public virtual void TestFixtureSetUp()
        {
			Logger.LogDebug(">>> Entering TestFixtureSetUp()");
            this.MockRepository = new MockRepository(MockBehavior.Default);
            this.MockHostController = this.MockRepository.Create<IHostController>();
			Logger.LogDebug("TestFixtureSetUp(): MockHostController → {MockHostController}", MockHostController);
			Logger.LogDebug("TestFixtureSetUp(): MockRepository → {MockRepository}", MockRepository);
            this.MockHostController.As<IHostSettingsService>();
			Logger.LogDebug("<<< Exiting TestFixtureSetUp()");
        }

        [SetUp]
        public virtual void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            // Mock Repository and component factory
            this.MockRepository = new MockRepository(MockBehavior.Default);
			Logger.LogDebug("SetUp(): MockRepository → {MockRepository}", MockRepository);
            ComponentFactory.Container = new SimpleContainer();

            // Setup Mock
            this.MockCache = MockComponentProvider.CreateDataCacheProvider();
			Logger.LogDebug("SetUp(): MockCache → {MockCache}", MockCache);
            HostController.RegisterInstance(this.MockHostController.Object);

            this.MockPortalController = this.MockRepository.Create<IPortalController>();
			Logger.LogDebug("SetUp(): MockPortalController → {MockPortalController}", MockPortalController);
            PortalController.SetTestableInstance(this.MockPortalController.Object);
            this.MockModuleController = this.MockRepository.Create<IModuleController>();
			Logger.LogDebug("SetUp(): MockModuleController → {MockModuleController}", MockModuleController);
            ModuleController.SetTestableInstance(this.MockModuleController.Object);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("SetUp(): services → {Services}", services);
                    services.AddSingleton(this.MockCache.Object);
                    services.AddSingleton(this.MockPortalController.Object);
                    services.AddSingleton(this.MockModuleController.Object);
                    services.AddSingleton(this.MockHostController.Object);
                    services.AddSingleton(this.MockHostController.As<IHostSettingsService>().Object);
                    this.SetupServiceProvider(services);
                });

            // Setup mock cache
            this.MockCacheCollection = new Hashtable();
			Logger.LogDebug("SetUp(): MockCacheCollection → {MockCacheCollection}", MockCacheCollection);
            this.MockHostController.Setup(hc => hc.GetString("PerformanceSetting")).Returns("3");
            this.MockCache.Setup(c => c.Insert(It.IsAny<string>(), It.IsAny<object>())).Callback((string cacheKey, object itemToCache) => this.MockCacheCollection[cacheKey] = itemToCache);
            this.MockCache.Setup(c => c.Insert(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DNNCacheDependency>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<CacheItemPriority>(), It.IsAny<CacheItemRemovedCallback>()))
                     .Callback((string cacheKey, object itemToCache, DNNCacheDependency dcd, DateTime dt, TimeSpan ts, CacheItemPriority cip, CacheItemRemovedCallback circ) => this.MockCacheCollection[cacheKey] = itemToCache);
            this.MockCache.Setup(c => c.GetItem(It.IsAny<string>())).Returns((string cacheKey) => this.MockCacheCollection[cacheKey]);
			Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public virtual void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            MockComponentProvider.ResetContainer();
            PortalController.ClearInstance();
            ModuleController.ClearInstance();
            this.serviceProvider?.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        protected static string CacheKey(ModuleInfo moduleInfo) => $"SettingsModule{moduleInfo.TabModuleID}";
		Logger.LogDebug(">>> Entering CacheKey(moduleInfo={ModuleInfo})", moduleInfo);

        protected static string ModuleSettingsCacheKey(ModuleInfo moduleInfo) => $"ModuleSettings{moduleInfo.TabID}";
		Logger.LogDebug(">>> Entering ModuleSettingsCacheKey(moduleInfo={ModuleInfo})", moduleInfo);

        protected static string TabModuleSettingsCacheKey(ModuleInfo moduleInfo) => $"TabModuleSettings{moduleInfo.TabID}";
		Logger.LogDebug(">>> Entering TabModuleSettingsCacheKey(moduleInfo={ModuleInfo})", moduleInfo);

        protected virtual void SetupServiceProvider(IServiceCollection services)
        {
		Logger.LogDebug(">>> Entering SetupServiceProvider(services={Services})", services);
		Logger.LogDebug("<<< Exiting SetupServiceProvider(services={Services})", services);
        }

        protected void MockPortalSettings(ModuleInfo moduleInfo, Dictionary<string, string> portalSettings)
        {
            this.MockPortalController.Setup(pc => pc.GetPortalSettings(moduleInfo.PortalID)).Returns(portalSettings);
        }

        protected void MockTabModuleSettings(ModuleInfo moduleInfo, Hashtable tabModuleSettings)
        {
			Logger.LogDebug(">>> Entering MockTabModuleSettings(moduleInfo={ModuleInfo},tabModuleSettings={TabModuleSettings})", moduleInfo, tabModuleSettings);
            this.MockCache.Setup(c => c.GetItem("DNN_" + TabModuleSettingsCacheKey(moduleInfo)))
                .Returns(new Dictionary<int, Hashtable> { { moduleInfo.TabModuleID, tabModuleSettings } });
				Logger.LogDebug("<<< Exiting MockTabModuleSettings(moduleInfo={ModuleInfo},tabModuleSettings={TabModuleSettings})", moduleInfo, tabModuleSettings);
        }

        protected void MockModuleSettings(ModuleInfo moduleInfo, Hashtable moduleSettings)
        {
			Logger.LogDebug(">>> Entering MockModuleSettings(moduleInfo={ModuleInfo},moduleSettings={ModuleSettings})", moduleInfo, moduleSettings);
            this.MockCache.Setup(c => c.GetItem("DNN_" + ModuleSettingsCacheKey(moduleInfo)))
                .Returns(new Dictionary<int, Hashtable> { { moduleInfo.ModuleID, moduleSettings } });
				Logger.LogDebug("<<< Exiting MockModuleSettings(moduleInfo={ModuleInfo},moduleSettings={ModuleSettings})", moduleInfo, moduleSettings);
        }
    }
}
