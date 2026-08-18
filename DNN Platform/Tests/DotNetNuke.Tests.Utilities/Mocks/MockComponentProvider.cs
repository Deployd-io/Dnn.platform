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
	using Microsoft.Extensions.Logging;

    public class MockComponentProvider
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public static Mock<T> CreateNew<T>(params object[] args)
            where T : class
        {
			_logger.LogDebug(">>> Entering CreateNew(args={Args})", args);
            if (ComponentFactory.Container == null)
            {
                ResetContainer();
            }

            // Try and get mock
            var mockComp = ComponentFactory.GetComponent<Mock<T>>();
			_logger.LogDebug("CreateNew(args={Args}): mockComp → {MockComp}", args, mockComp);
            var objComp = ComponentFactory.GetComponent<T>();
			_logger.LogDebug("CreateNew(args={Args}): objComp → {ObjComp}", args, objComp);

            if (mockComp == null)
            {
                mockComp = new Mock<T>(args);
                ComponentFactory.RegisterComponentInstance<Mock<T>>(mockComp);
            }

            if (objComp == null)
            {
                ComponentFactory.RegisterComponentInstance<T>(mockComp.Object);
            }

			_logger.LogDebug("<<< Exiting CreateNew(args={Args})", args);
			_logger.LogDebug("<<< Exiting CreateNew(name={Name},args={Args})", name, args);
            return mockComp;
        }

        public static Mock<T> CreateNew<T>(string name, params object[] args)
            where T : class
        {
            if (ComponentFactory.Container == null)
            {
                ResetContainer();
            }

            // Try and get mock
            var mockComp = ComponentFactory.GetComponent<Mock<T>>();
			_logger.LogDebug("CreateNew(name={Name},args={Args}): mockComp → {MockComp}", name, args, mockComp);
            var objComp = ComponentFactory.GetComponent<T>(name);
			_logger.LogDebug("CreateNew(name={Name},args={Args}): objComp → {ObjComp}", name, args, objComp);

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
			_logger.LogDebug(">>> Entering CreateDataCacheProvider(hostSettings={HostSettings})", hostSettings);
			_logger.LogDebug("<<< Exiting CreateDataCacheProvider(hostSettings={HostSettings})", hostSettings);
            return CreateNew<CachingProvider>(hostSettings ?? Mock.Of<IHostSettings>());
        }

        public static Mock<EventLogController> CreateEventLogController()
        {
			_logger.LogDebug(">>> Entering CreateEventLogController()");
			_logger.LogDebug("<<< Exiting CreateEventLogController()");
            return CreateNew<EventLogController>();
        }

        public static Mock<DataProvider> CreateDataProvider(IApplicationStatusInfo appStatus = null)
        {
			_logger.LogDebug(">>> Entering CreateDataProvider(appStatus={AppStatus})", appStatus);
			_logger.LogDebug("<<< Exiting CreateDataProvider(appStatus={AppStatus})", appStatus);
            return CreateNew<DataProvider>(appStatus ?? Mock.Of<IApplicationStatusInfo>());
        }

        public static Mock<FolderProvider> CreateFolderProvider(string name)
        {
			_logger.LogDebug(">>> Entering CreateFolderProvider(name={Name})", name);
			_logger.LogDebug("<<< Exiting CreateFolderProvider(name={Name})", name);
            return CreateNew<FolderProvider>(name);
        }

        public static Mock<ILocalizationProvider> CreateLocalizationProvider()
        {
			_logger.LogDebug(">>> Entering CreateLocalizationProvider()");
			_logger.LogDebug("<<< Exiting CreateLocalizationProvider()");
            return CreateNew<ILocalizationProvider>();
        }

        public static Mock<ILocaleController> CreateLocaleController()
        {
			_logger.LogDebug(">>> Entering CreateLocaleController()");
			_logger.LogDebug("<<< Exiting CreateLocaleController()");
            return CreateNew<ILocaleController>();
        }

        public static Mock<RoleProvider> CreateRoleProvider()
        {
			_logger.LogDebug(">>> Entering CreateRoleProvider()");
			_logger.LogDebug("<<< Exiting CreateRoleProvider()");
            return CreateNew<RoleProvider>();
        }

        public static Mock<IPortalController> CreatePortalController()
        {
			_logger.LogDebug(">>> Entering CreatePortalController()");
			_logger.LogDebug("<<< Exiting CreatePortalController()");
            return CreateNew<IPortalController>();
        }

        public static void ResetContainer()
        {
			_logger.LogDebug(">>> Entering ResetContainer()");
            ComponentFactory.Container = new SimpleContainer();
			_logger.LogDebug("<<< Exiting ResetContainer()");
        }
    }
}
