// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities.Mocks
{
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using Moq;

    public class MockComponentProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static Mock<T> CreateNew<T>(params object[] args)
            where T : class
        {
			Logger.LogDebug(">>> Entering CreateNew(args={Args})", args);
            if (ComponentFactory.Container == null)
            {
                ResetContainer();
            }

            // Try and get mock
            var mockComp = ComponentFactory.GetComponent<Mock<T>>();
            var objComp = ComponentFactory.GetComponent<T>();
			Logger.LogDebug("CreateNew(args={Args}): mockComp → {MockComp}", args, mockComp);
			Logger.LogDebug("CreateNew(args={Args}): objComp → {ObjComp}", args, objComp);

            if (mockComp == null)
            {
                mockComp = new Mock<T>(args);
                ComponentFactory.RegisterComponentInstance<Mock<T>>(mockComp);
            }

            if (objComp == null)
            {
                ComponentFactory.RegisterComponentInstance<T>(mockComp.Object);
            }

            return mockComp;
        }

        public static Mock<T> CreateNew<T>(string name, params object[] args)
            where T : class
        {
            if (ComponentFactory.Container == null)
			Logger.LogDebug("<<< Exiting CreateNew(args={Args})", args);
            {
                ResetContainer();
            }

            // Try and get mock
            var mockComp = ComponentFactory.GetComponent<Mock<T>>();
            var objComp = ComponentFactory.GetComponent<T>(name);
			Logger.LogDebug("CreateNew(name={Name},args={Args}): mockComp → {MockComp}", name, args, mockComp);
			Logger.LogDebug("CreateNew(name={Name},args={Args}): objComp → {ObjComp}", name, args, objComp);

            if (mockComp == null)
            {
                mockComp = new Mock<T>(args);
                ComponentFactory.RegisterComponentInstance<Mock<T>>(mockComp);
            }

            if (objComp == null)
            {
                ComponentFactory.RegisterComponentInstance<T>(name, mockComp.Object);
            }

            return mockComp;
        }

        public static Mock<CachingProvider> CreateDataCacheProvider(IHostSettings hostSettings = null)
        {
			Logger.LogDebug(">>> Entering CreateDataCacheProvider(hostSettings={HostSettings})", hostSettings);
			Logger.LogDebug("<<< Exiting CreateDataCacheProvider(hostSettings={HostSettings})", hostSettings);
            return CreateNew<CachingProvider>(hostSettings ?? Mock.Of<IHostSettings>());
        }

        public static Mock<EventLogController> CreateEventLogController()
        {
			Logger.LogDebug(">>> Entering CreateEventLogController()");
			Logger.LogDebug("<<< Exiting CreateEventLogController()");
            return CreateNew<EventLogController>();
        }

        public static Mock<DataProvider> CreateDataProvider(IApplicationStatusInfo appStatus = null)
        {
			Logger.LogDebug(">>> Entering CreateDataProvider(appStatus={AppStatus})", appStatus);
			Logger.LogDebug("<<< Exiting CreateDataProvider(appStatus={AppStatus})", appStatus);
            return CreateNew<DataProvider>(appStatus ?? Mock.Of<IApplicationStatusInfo>());
        }

        public static Mock<FolderProvider> CreateFolderProvider(string name)
        {
			Logger.LogDebug(">>> Entering CreateFolderProvider(name={Name})", name);
			Logger.LogDebug("<<< Exiting CreateFolderProvider(name={Name})", name);
            return CreateNew<FolderProvider>(name);
        }

        public static Mock<ILocalizationProvider> CreateLocalizationProvider()
        {
			Logger.LogDebug(">>> Entering CreateLocalizationProvider()");
			Logger.LogDebug("<<< Exiting CreateLocalizationProvider()");
            return CreateNew<ILocalizationProvider>();
        }

        public static Mock<ILocaleController> CreateLocaleController()
        {
			Logger.LogDebug(">>> Entering CreateLocaleController()");
			Logger.LogDebug("<<< Exiting CreateLocaleController()");
            return CreateNew<ILocaleController>();
        }

        public static Mock<RoleProvider> CreateRoleProvider()
        {
			Logger.LogDebug(">>> Entering CreateRoleProvider()");
			Logger.LogDebug("<<< Exiting CreateRoleProvider()");
            return CreateNew<RoleProvider>();
        }

        public static Mock<IPortalController> CreatePortalController()
        {
			Logger.LogDebug(">>> Entering CreatePortalController()");
			Logger.LogDebug("<<< Exiting CreatePortalController()");
            return CreateNew<IPortalController>();
        }

        public static void ResetContainer()
        {
			Logger.LogDebug(">>> Entering ResetContainer()");
            ComponentFactory.Container = new SimpleContainer();
			Logger.LogDebug("<<< Exiting ResetContainer()");
        }
    }
}
