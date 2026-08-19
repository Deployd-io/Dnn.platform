// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.OutputCache;

    using Microsoft.Extensions.DependencyInjection;

    [Obsolete("Deprecated in DotNetNuke 9.13.8. This type has no known use. Scheduled for removal in v11.0.0.")]
    public enum CoreCacheType
    {
        /// <summary>Host level.</summary>
        Host = 1,

        /// <summary>Portal level.</summary>
        Portal = 2,

        /// <summary>Tab level.</summary>
        Tab = 3,
    }

    /// <summary>The DataCache class is a facade class for the CachingProvider Instance's.</summary>
    public partial class DataCache
    {
        // Host keys
        public const string SecureHostSettingsCacheKey = "SecureHostSettings";
        public const string UnSecureHostSettingsCacheKey = "UnsecureHostSettings";
        public const string HostSettingsCacheKey = "HostSettings";
        public const CacheItemPriority HostSettingsCachePriority = CacheItemPriority.NotRemovable;
        public const int HostSettingsCacheTimeOut = 20;

        // Portal keys
        public const string PortalAliasCacheKey = "PortalAlias";
        public const CacheItemPriority PortalAliasCachePriority = CacheItemPriority.NotRemovable;
        public const int PortalAliasCacheTimeOut = 200;

        public const string PortalSettingsCacheKey = "PortalSettings{0}{1}";
        public const CacheItemPriority PortalSettingsCachePriority = CacheItemPriority.NotRemovable;
        public const int PortalSettingsCacheTimeOut = 20;

        public const string PortalDictionaryCacheKey = "PortalDictionary";
        public const CacheItemPriority PortalDictionaryCachePriority = CacheItemPriority.High;
        public const int PortalDictionaryTimeOut = 20;

        public const string PortalCacheKey = "Portal{0}_{1}";
        public const CacheItemPriority PortalCachePriority = CacheItemPriority.High;
        public const int PortalCacheTimeOut = 20;

        public const string AllPortalsCacheKey = "AllPortals";
        public const CacheItemPriority AllPortalsCachePriority = CacheItemPriority.High;
        public const int AllPortalsCacheTimeOut = 20;

        public const string PortalUserCountCacheKey = "PortalUserCount{0}";
        public const CacheItemPriority PortalUserCountCachePriority = CacheItemPriority.High;
        public const int PortalUserCountCacheTimeOut = 20;

        public const string PortalGroupsCacheKey = "PortalGroups";
        public const CacheItemPriority PortalGroupsCachePriority = CacheItemPriority.High;
        public const int PortalGroupsCacheTimeOut = 20;

        public const string PortalPermissionCacheKey = "PortalPermission{0}";
        public const CacheItemPriority PortalPermissionCachePriority = CacheItemPriority.High;
        public const int PortalPermissionCacheTimeOut = 20;

        /// <summary> The portal styles cache key.</summary>
        public const string PortalStylesCacheKey = "Dnn_Css_Custom_Properties_{0}";

        // Tab cache keys
        public const string TabCacheKey = "Tab_Tabs{0}";
        public const string TabSettingsCacheKey = "TabSettings{0}";
        public const CacheItemPriority TabCachePriority = CacheItemPriority.High;
        public const int TabCacheTimeOut = 20;
        public const string TabPathCacheKey = "Tab_TabPathDictionary{0}_{1}";
        public const CacheItemPriority TabPathCachePriority = CacheItemPriority.High;
        public const int TabPathCacheTimeOut = 20;
        public const string TabPermissionCacheKey = "Tab_TabPermissions{0}";
        public const CacheItemPriority TabPermissionCachePriority = CacheItemPriority.High;
        public const int TabPermissionCacheTimeOut = 20;
        public const string TabAliasSkinCacheKey = "Tab_TabAliasSkins{0}";
        public const CacheItemPriority TabAliasSkinCachePriority = CacheItemPriority.High;
        public const int TabAliasSkinCacheTimeOut = 20;
        public const string TabCustomAliasCacheKey = "Tab_TabCustomAliases{0}";
        public const CacheItemPriority TabCustomAliasCachePriority = CacheItemPriority.High;
        public const int TabCustomAliasCacheTimeOut = 20;
        public const string TabUrlCacheKey = "Tab_TabUrls{0}";
        public const CacheItemPriority TabUrlCachePriority = CacheItemPriority.High;
        public const int TabUrlCacheTimeOut = 20;
        public const string TabVersionsCacheKey = "Tab_TabVersions{0}";
        public const CacheItemPriority TabVersionsCachePriority = CacheItemPriority.High;
        public const int TabVersionsCacheTimeOut = 20;
        public const string TabVersionDetailsCacheKey = "Tab_TabVersionDetails{0}";
        public const CacheItemPriority TabVersionDetailsCachePriority = CacheItemPriority.High;
        public const int TabVersionDetailsCacheTimeOut = 20;

        public const string AuthenticationServicesCacheKey = "AuthenticationServices";
        public const CacheItemPriority AuthenticationServicesCachePriority = CacheItemPriority.NotRemovable;
        public const int AuthenticationServicesCacheTimeOut = 20;

        public const string DesktopModulePermissionCacheKey = "DesktopModulePermissions";
        public const CacheItemPriority DesktopModulePermissionCachePriority = CacheItemPriority.High;
        public const int DesktopModulePermissionCacheTimeOut = 20;

        public const string DesktopModuleCacheKey = "DesktopModulesByPortal{0}";
        public const CacheItemPriority DesktopModuleCachePriority = CacheItemPriority.High;
        public const int DesktopModuleCacheTimeOut = 20;

        public const string PortalDesktopModuleCacheKey = "PortalDesktopModules{0}";
        public const CacheItemPriority PortalDesktopModuleCachePriority = CacheItemPriority.AboveNormal;
        public const int PortalDesktopModuleCacheTimeOut = 20;

        public const string ModuleDefinitionCacheKey = "ModuleDefinitions";
        public const CacheItemPriority ModuleDefinitionCachePriority = CacheItemPriority.High;
        public const int ModuleDefinitionCacheTimeOut = 20;

        public const string ModuleControlsCacheKey = "ModuleControls";
        public const CacheItemPriority ModuleControlsCachePriority = CacheItemPriority.High;
        public const int ModuleControlsCacheTimeOut = 20;

        public const string TabModuleCacheKey = "TabModules{0}";
        public const string TabModuleSettingsCacheKey = "TabModuleSettings{0}";
        public const string SingleTabModuleCacheKey = "SingleTabModule{0}";
        public const string TabModuleSettingsNameCacheKey = "TabModuleSettingsName:{0}:{1}";
        public const CacheItemPriority TabModuleCachePriority = CacheItemPriority.AboveNormal;
        public const int TabModuleCacheTimeOut = 20;

        public const string PublishedTabModuleCacheKey = "PublishedTabModules{0}";
        public const CacheItemPriority PublishedTabModuleCachePriority = CacheItemPriority.AboveNormal;
        public const int PublishedTabModuleCacheTimeOut = 20;

        public const string ModulePermissionCacheKey = "ModulePermissions{0}";
        public const CacheItemPriority ModulePermissionCachePriority = CacheItemPriority.AboveNormal;
        public const int ModulePermissionCacheTimeOut = 20;

        public const string ModuleCacheKey = "Modules{0}";
        public const string ModuleSettingsCacheKey = "ModuleSettings{0}";
        public const int ModuleCacheTimeOut = 20;
        public const CacheItemPriority ModuleCachePriority = CacheItemPriority.AboveNormal;

        public const string SharedModulesByPortalCacheKey = "SharedModulesByPortal{0}";
        public const int SharedModulesByPortalCacheTimeOut = 20;
        public const CacheItemPriority SharedModulesByPortalCachePriority = CacheItemPriority.Normal;

        public const string ContentItemsCacheKey = "ContentItems{0}";
        public const int ContentItemsCacheTimeOut = 20;
        public const CacheItemPriority ContentItemsCachePriority = CacheItemPriority.Normal;

        public const string SharedModulesWithPortalCacheKey = "SharedModulesWithPortal{0}";
        public const int SharedModulesWithPortalCacheTimeOut = 20;
        public const CacheItemPriority SharedModulesWithPortalCachePriority = CacheItemPriority.Normal;

        public const string FolderCacheKey = "Folders{0}";
        public const int FolderCacheTimeOut = 20;
        public const CacheItemPriority FolderCachePriority = CacheItemPriority.Normal;

        public const string FolderUserCacheKey = "Folders|{0}|{1}|{2}";
        public const int FolderUserCacheTimeOut = 20;
        public const CacheItemPriority FolderUserCachePriority = CacheItemPriority.Normal;

        public const string FolderMappingCacheKey = "FolderMapping|{0}";
        public const int FolderMappingCacheTimeOut = 20;
        public const CacheItemPriority FolderMappingCachePriority = CacheItemPriority.High;

        public const string FolderPermissionCacheKey = "FolderPermissions{0}"; // parent cache dependency key
        public const string FolderPathPermissionCacheKey = "FolderPathPermissions|{0}|{1}";
        public const CacheItemPriority FolderPermissionCachePriority = CacheItemPriority.Normal;
        public const int FolderPermissionCacheTimeOut = 20;

        public const string ListsCacheKey = "Lists{0}";
        public const string ListEntriesCacheKey = "ListEntries|{0}|{1}";
        public const CacheItemPriority ListsCachePriority = CacheItemPriority.Normal;
        public const int ListsCacheTimeOut = 20;

        public const string ProfileDefinitionsCacheKey = "ProfileDefinitions{0}";
        public const int ProfileDefinitionsCacheTimeOut = 20;

        public const string UserCacheKey = "UserInfo|{0}|{1}";
        public const int UserCacheTimeOut = 1;
        public const CacheItemPriority UserCachePriority = CacheItemPriority.Normal;

        public const string UserProfileCacheKey = "UserProfile|{0}|{1}";
        public const int UserProfileCacheTimeOut = UserCacheTimeOut;

        public const string UserNotificationsConversationCountCacheKey = "UserNitifConversationCount|{0}|{1}";
        public const string UserNotificationsCountCacheKey = "UserNotificationsCount|{0}|{1}";
        public const string UserNewThreadsCountCacheKey = "UserNewThreadsCount|{0}|{1}";
        public const int NotificationsCacheTimeInSec = 30;

        public const string UserPersonalizationCacheKey = "UserPersonalization|{0}|{1}";
        public const int UserPersonalizationCacheTimeout = 5;
        public const CacheItemPriority UserPersonalizationCachePriority = CacheItemPriority.Normal;

        public const string UserLookupCacheKey = "UserLookup|{0}";
        public const int UserLookupCacheTimeOut = 20;
        public const CacheItemPriority UserLookupCachePriority = CacheItemPriority.High;

        public const string LocalesCacheKey = "Locales{0}";
        public const CacheItemPriority LocalesCachePriority = CacheItemPriority.Normal;
        public const int LocalesCacheTimeOut = 20;

        public const string SkinDefaultsCacheKey = "SkinDefaults_{0}";
        public const CacheItemPriority SkinDefaultsCachePriority = CacheItemPriority.Normal;
        public const int SkinDefaultsCacheTimeOut = 20;

        public const CacheItemPriority ResourceFilesCachePriority = CacheItemPriority.Normal;
        public const int ResourceFilesCacheTimeOut = 20;

        public const string ResourceFileLookupDictionaryCacheKey = "ResourceFileLookupDictionary";
        public const CacheItemPriority ResourceFileLookupDictionaryCachePriority = CacheItemPriority.NotRemovable;
        public const int ResourceFileLookupDictionaryTimeOut = 200;

        public const string SpaModulesContentHtmlFileCacheKey = "SpaModulesContentHtmlFile|{0}";
        public const string SpaModulesFileExistsCacheKey = "SpaModulesFileExists|{0}";
        public const CacheItemPriority SpaModulesHtmlFileCachePriority = CacheItemPriority.Normal;
        public const int SpaModulesHtmlFileTimeOut = 200;

        public const string SkinsCacheKey = "GetSkins{0}";

        public const string BannersCacheKey = "Banners:{0}:{1}:{2}";
        public const CacheItemPriority BannersCachePriority = CacheItemPriority.Normal;
        public const int BannersCacheTimeOut = 20;

        public const string RedirectionsCacheKey = "Redirections:{0}";
        public const CacheItemPriority RedirectionsCachePriority = CacheItemPriority.Default;
        public const int RedirectionsCacheTimeOut = 20;

        public const string PreviewProfilesCacheKey = "PreviewProfiles:{0}";
        public const CacheItemPriority PreviewProfilesCachePriority = CacheItemPriority.Default;
        public const int PreviewProfilesCacheTimeOut = 20;

        public const string RelationshipTypesCacheKey = "RelationshipTypes";
        public const CacheItemPriority RelationshipTypesCachePriority = CacheItemPriority.Default;
        public const int RelationshipTypesCacheTimeOut = 20;

        public const string RelationshipByPortalIDCacheKey = "RelationshipByPortalID:{0}";
        public const CacheItemPriority RelationshipByPortalIDCachePriority = CacheItemPriority.Default;
        public const int RelationshipByPortalIDCacheTimeOut = 20;

        public const string RolesCacheKey = "Roles:{0}";
        public const CacheItemPriority RolesCachePriority = CacheItemPriority.Default;
        public const int RolesCacheTimeOut = 20;

        public const string RoleGroupsCacheKey = "RoleGroups:{0}";
        public const CacheItemPriority RoleGroupsCachePriority = CacheItemPriority.Default;
        public const int RoleGroupsCacheTimeOut = 20;

        public const string JournalTypesCacheKey = "JournalTypes:{0}";
        public const CacheItemPriority JournalTypesCachePriority = CacheItemPriority.Default;
        public const int JournalTypesTimeOut = 20;

        public const string NotificationTypesCacheKey = "NotificationTypes:{0}";
        public const CacheItemPriority NotificationTypesCachePriority = CacheItemPriority.Default;
        public const int NotificationTypesTimeOut = 20;

        public const string NotificationTypeActionsCacheKey = "NotificationTypeActions:{0}";
        public const string NotificationTypeActionsByNameCacheKey = "NotificationTypeActions:{0}|{1}";
        public const CacheItemPriority NotificationTypeActionsPriority = CacheItemPriority.Default;
        public const int NotificationTypeActionsTimeOut = 20;

        public const string SubscriptionTypesCacheKey = "SubscriptionTypes";
        public const CacheItemPriority SubscriptionTypesCachePriority = CacheItemPriority.Default;
        public const int SubscriptionTypesTimeOut = 20;

        public const string PackagesCacheKey = "Packages_{0}";
        public const string PackageDependenciesCacheKey = "Packages_Dependencies";
        public const CacheItemPriority PackagesCachePriority = CacheItemPriority.Default;
        public const int PackagesCacheTimeout = 20;

        public const string ContentTypesCacheKey = "ContentTypes";
        public const CacheItemPriority ContentTypesCachePriority = CacheItemPriority.AboveNormal;
        public const int ContentTypesCacheTimeOut = 20;

        public const string PermissionsCacheKey = "Permissions";
        public const CacheItemPriority PermissionsCachePriority = CacheItemPriority.AboveNormal;
        public const int PermissionsCacheTimeout = 20;

        public const string PackageTypesCacheKey = "PackagesTypes";
        public const CacheItemPriority PackageTypesCachePriority = CacheItemPriority.AboveNormal;
        public const int PackageTypesCacheTimeout = 20;

        public const string JavaScriptLibrariesCacheKey = "JavaScriptLibraries";
        public const CacheItemPriority JavaScriptLibrariesCachePriority = CacheItemPriority.AboveNormal;
        public const int JavaScriptLibrariesCacheTimeout = 20;

        public const string CaptchaCacheKey = "Captcha_{0}";
        public const CacheItemPriority CaptchaCachePriority = CacheItemPriority.NotRemovable;
        public const int CaptchaCacheTimeout = 2;

        public const string ContentWorkflowCacheKey = "ContentWorkflows:{0}";
        public const string ContentWorkflowStateCacheKey = "ContentWorkflowStates_{0}";
        public const CacheItemPriority WorkflowsCachePriority = CacheItemPriority.Low;
        public const int WorkflowsCacheTimeout = 2;

        public const string ScopeTypesCacheKey = "ScopeTypes";
        public const string VocabularyCacheKey = "Vocabularies";
        public const string TermCacheKey = "Terms_{0}";

        internal const string UserIdListToClearDiskImageCacheKey = "UserIdListToClearDiskImage_{0}";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DataCache));

        private static readonly ReaderWriterLockSlim DictionaryLock = new ReaderWriterLockSlim();
        private static readonly Dictionary<string, object> LockDictionary = new Dictionary<string, object>();

        private static readonly SharedDictionary<string, object> DictionaryCache = new SharedDictionary<string, object>();

        private static readonly TimeSpan FiveSeconds = new TimeSpan(0, 0, 5);

        private static string cachePersistenceEnabled = string.Empty;

        public static bool CachePersistenceEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(cachePersistenceEnabled))
                {
                    cachePersistenceEnabled = Config.GetSetting("EnableCachePersistence") ?? "false";
                }

                return bool.Parse(cachePersistenceEnabled);
            }
        }

        public static void ClearCache()
        {
			Logger.LogDebug(">>> Entering ClearCache()");
            CachingProvider.Instance().Clear("Prefix", "DNN_");
            using (DictionaryCache.GetWriteLock())
            {
                DictionaryCache.Clear();
            }

            // log the cache clear event
            var log = new LogInfo { LogTypeKey = nameof(EventLogType.CACHE_REFRESH), };
			Logger.LogDebug("ClearCache(): log → {Log}", log);
            log.LogProperties.Add(new LogDetailInfo("*", "Refresh"));
            LogController.Instance.AddLog(log);
			Logger.LogDebug("<<< Exiting ClearCache()");
        }

        public static void ClearCache(string cachePrefix)
        {
			Logger.LogDebug(">>> Entering ClearCache(cachePrefix={CachePrefix})", cachePrefix);
            CachingProvider.Instance().Clear("Prefix", GetDnnCacheKey(cachePrefix));
			Logger.LogDebug("<<< Exiting ClearCache(cachePrefix={CachePrefix})", cachePrefix);
        }

        public static void ClearFolderCache(int portalId)
        {
			Logger.LogDebug(">>> Entering ClearFolderCache(portalId={PortalId})", portalId);
            CachingProvider.Instance().Clear("Folder", portalId.ToString(CultureInfo.InvariantCulture));
			Logger.LogDebug("<<< Exiting ClearFolderCache(portalId={PortalId})", portalId);
        }

        public static void ClearHostCache(bool cascade)
        {
			Logger.LogDebug(">>> Entering ClearHostCache(cascade={Cascade})", cascade);
            if (cascade)
            {
                ClearCache();
            }
            else
            {
                CachingProvider.Instance().Clear("Host", string.Empty);
            }
			Logger.LogDebug("<<< Exiting ClearHostCache(cascade={Cascade})", cascade);
        }

        /// <summary>Clear the module cache.</summary>
        /// <param name="tabId">The tab ID.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IHostSettings")]
        public static partial void ClearModuleCache(int tabId)
            => ClearModuleCache(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(),
                tabId);

        /// <summary>Clear the module cache.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="tabId">The tab ID.</param>
        public static void ClearModuleCache(IHostSettings hostSettings, DataProvider dataProvider, int tabId)
        {
			Logger.LogDebug(">>> Entering ClearModuleCache(hostSettings={HostSettings},dataProvider={DataProvider},tabId={TabId})", hostSettings, dataProvider, tabId);
            CachingProvider.Instance().Clear("Module", tabId.ToString(CultureInfo.InvariantCulture));
			Logger.LogDebug("<<< Exiting ClearModuleCache(tabId={TabId})", tabId);
            var portals = PortalController.GetPortalDictionary(hostSettings, dataProvider);
			Logger.LogDebug("ClearModuleCache(hostSettings={HostSettings},dataProvider={DataProvider},tabId={TabId}): portals → {Portals}", hostSettings, dataProvider, tabId, portals);
            if (portals.TryGetValue(tabId, out var portalId))
            {
                var tabSettings = TabController.Instance.GetTabSettings(tabId);
				Logger.LogDebug("ClearModuleCache(hostSettings={HostSettings},dataProvider={DataProvider},tabId={TabId}): tabSettings → {TabSettings}", hostSettings, dataProvider, tabId, tabSettings);
                if (tabSettings["CacheProvider"] != null && tabSettings["CacheProvider"].ToString().Length > 0)
                {
                    var outputProvider = OutputCachingProvider.Instance(tabSettings["CacheProvider"].ToString());
					Logger.LogDebug("ClearModuleCache(hostSettings={HostSettings},dataProvider={DataProvider},tabId={TabId}): outputProvider → {OutputProvider}", hostSettings, dataProvider, tabId, outputProvider);
                    outputProvider?.Remove(tabId);
                }

                RemoveCache(string.Format(CultureInfo.InvariantCulture, SharedModulesByPortalCacheKey, portalId));
                RemoveCache(string.Format(CultureInfo.InvariantCulture, SharedModulesWithPortalCacheKey, portalId));
            }
        }

        public static void ClearModulePermissionsCachesByPortal(int portalId)
        {
			Logger.LogDebug(">>> Entering ClearModulePermissionsCachesByPortal(portalId={PortalId})", portalId);
            CachingProvider.Instance().Clear("ModulePermissionsByPortal", portalId.ToString(CultureInfo.InvariantCulture));
			Logger.LogDebug("<<< Exiting ClearModulePermissionsCachesByPortal(portalId={PortalId})", portalId);
        }

        public static void ClearPortalCache(int portalId, bool cascade)
        {
			Logger.LogDebug(">>> Entering ClearPortalCache(portalId={PortalId},cascade={Cascade})", portalId, cascade);
            CachingProvider.Instance().Clear(cascade ? "PortalCascade" : "Portal", portalId.ToString(CultureInfo.InvariantCulture));
			Logger.LogDebug("<<< Exiting ClearPortalCache(portalId={PortalId},cascade={Cascade})", portalId, cascade);
        }

        public static void ClearTabsCache(int portalId)
        {
			Logger.LogDebug(">>> Entering ClearTabsCache(portalId={PortalId})", portalId);
            CachingProvider.Instance().Clear("Tab", portalId.ToString(CultureInfo.InvariantCulture));
			Logger.LogDebug("<<< Exiting ClearTabsCache(portalId={PortalId})", portalId);
        }

        public static void ClearDefinitionsCache(int portalId)
        {
			Logger.LogDebug(">>> Entering ClearDefinitionsCache(portalId={PortalId})", portalId);
            RemoveCache(string.Format(CultureInfo.InvariantCulture, ProfileDefinitionsCacheKey, portalId));
			Logger.LogDebug("<<< Exiting ClearDefinitionsCache(portalId={PortalId})", portalId);
        }

        public static void ClearDesktopModulePermissionsCache()
        {
			Logger.LogDebug(">>> Entering ClearDesktopModulePermissionsCache()");
            RemoveCache(DesktopModulePermissionCacheKey);
			Logger.LogDebug("<<< Exiting ClearDesktopModulePermissionsCache()");
        }

        public static void ClearFolderPermissionsCache(int portalId)
        {
			Logger.LogDebug(">>> Entering ClearFolderPermissionsCache(portalId={PortalId})", portalId);
            PermissionProvider.ResetCacheDependency(
                portalId,
                () => RemoveCache(string.Format(CultureInfo.InvariantCulture, FolderPermissionCacheKey, portalId)));
				Logger.LogDebug("<<< Exiting ClearFolderPermissionsCache(portalId={PortalId})", portalId);
        }

        public static void ClearListsCache(int portalId)
        {
			Logger.LogDebug(">>> Entering ClearListsCache(portalId={PortalId})", portalId);
            RemoveCache(string.Format(CultureInfo.InvariantCulture, ListsCacheKey, portalId));
			Logger.LogDebug("<<< Exiting ClearListsCache(portalId={PortalId})", portalId);
			Logger.LogDebug(">>> Entering RemoveCache(cacheKey={CacheKey})", cacheKey);
			Logger.LogDebug("<<< Exiting RemoveCache(cacheKey={CacheKey})", cacheKey);
        }

        public static void ClearModulePermissionsCache(int tabId)
        {
			Logger.LogDebug(">>> Entering ClearModulePermissionsCache(tabId={TabId})", tabId);
            RemoveCache(string.Format(CultureInfo.InvariantCulture, ModulePermissionCacheKey, tabId));
			Logger.LogDebug("<<< Exiting ClearModulePermissionsCache(tabId={TabId})", tabId);
        }

        public static void ClearTabPermissionsCache(int portalId)
        {
			Logger.LogDebug(">>> Entering ClearTabPermissionsCache(portalId={PortalId})", portalId);
            RemoveCache(string.Format(CultureInfo.InvariantCulture, TabPermissionCacheKey, portalId));
			Logger.LogDebug("<<< Exiting ClearTabPermissionsCache(portalId={PortalId})", portalId);
        }

        public static void ClearPortalPermissionsCache(int portalId)
        {
			Logger.LogDebug(">>> Entering ClearPortalPermissionsCache(portalId={PortalId})", portalId);
            RemoveCache(string.Format(CultureInfo.InvariantCulture, PortalPermissionCacheKey, portalId));
			Logger.LogDebug("<<< Exiting ClearPortalPermissionsCache(portalId={PortalId})", portalId);
        }

        public static void ClearUserCache(int portalId, string username)
        {
			Logger.LogDebug(">>> Entering ClearUserCache(portalId={PortalId},username={Username})", portalId, username);
            RemoveCache(string.Format(CultureInfo.InvariantCulture, UserCacheKey, portalId, username));
            RemoveCache(string.Format(CultureInfo.InvariantCulture, UserProfileCacheKey, portalId, username));
			Logger.LogDebug("<<< Exiting ClearUserCache(portalId={PortalId},username={Username})", portalId, username);
        }

        public static void ClearPortalUserCountCache(int portalID)
        {
			Logger.LogDebug(">>> Entering ClearPortalUserCountCache(portalID={PortalID})", portalID);
            CachingProvider.Instance().Remove(string.Format(CultureInfo.InvariantCulture, PortalUserCountCacheKey, portalID));
			Logger.LogDebug("<<< Exiting ClearPortalUserCountCache(portalID={PortalID})", portalID);
        }

        public static void ClearUserPersonalizationCache(int portalId, int userId)
        {
			Logger.LogDebug(">>> Entering ClearUserPersonalizationCache(portalId={PortalId},userId={UserId})", portalId, userId);
            RemoveCache(string.Format(CultureInfo.InvariantCulture, UserPersonalizationCacheKey, portalId, userId));
			Logger.LogDebug("<<< Exiting ClearUserPersonalizationCache(portalId={PortalId},userId={UserId})", portalId, userId);
        }

        public static void ClearPackagesCache(int portalId)
        {
			Logger.LogDebug(">>> Entering ClearPackagesCache(portalId={PortalId})", portalId);
            RemoveCache(string.Format(CultureInfo.InvariantCulture, PackagesCacheKey, portalId));
			Logger.LogDebug("<<< Exiting ClearPackagesCache(portalId={PortalId})", portalId);
        }

        /// <summary>Get cached data.</summary>
        /// <param name="cacheItemArgs">Information about the cache item.</param>
        /// <param name="cacheItemExpired">The callback to supply an item when it's not in the cache.</param>
        /// <typeparam name="TObject">The type of the object to retrieve from the cache.</typeparam>
        /// <returns>The <typeparamref name="TObject"/> instance or <see langword="default"/>.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial TObject GetCachedData<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired)
            => GetCachedData<TObject>(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), cacheItemArgs, cacheItemExpired);

        public static TObject GetCachedData<TObject>(IHostSettings hostSettings, CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired)
        {
			Logger.LogDebug(">>> Entering GetCachedData(hostSettings={HostSettings},cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired})", hostSettings, cacheItemArgs, cacheItemExpired);
            // declare local object and try and retrieve item from the cache
			Logger.LogDebug("<<< Exiting GetCachedData(cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired})", cacheItemArgs, cacheItemExpired);
			Logger.LogDebug("<<< Exiting GetCachedData(performance={Performance},cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired},storeInDictionary={StoreInDictionary})", performance, cacheItemArgs, cacheItemExpired, storeInDictionary);
            return GetCachedData<TObject>(hostSettings.PerformanceSetting, cacheItemArgs, cacheItemExpired, false);
        }

        public static TObject GetCache<TObject>(string cacheKey)
        {
			Logger.LogDebug(">>> Entering GetCache(cacheKey={CacheKey})", cacheKey);
            object objObject = GetCache(cacheKey);
            if (objObject == null)
			Logger.LogDebug("GetCache(cacheKey={CacheKey}): objObject → {ObjObject}", cacheKey, objObject);
            {
                return default(TObject);
            }

			Logger.LogDebug("<<< Exiting GetCache(cacheKey={CacheKey})", cacheKey);
            return (TObject)objObject;
        }

        public static object GetCache(string cacheKey)
        {
            return CachingProvider.Instance().GetItem(GetDnnCacheKey(cacheKey));
        }

        public static void RemoveCache(string cacheKey)
        {
            CachingProvider.Instance().Remove(GetDnnCacheKey(cacheKey));
        }

        public static void RemoveFromPrivateDictionary(string dnnCacheKey)
        {
			Logger.LogDebug(">>> Entering RemoveFromPrivateDictionary(dnnCacheKey={DnnCacheKey})", dnnCacheKey);
            using (DictionaryCache.GetWriteLock())
            {
                DictionaryCache.Remove(CleanCacheKey(dnnCacheKey));
            }
			Logger.LogDebug("<<< Exiting RemoveFromPrivateDictionary(dnnCacheKey={DnnCacheKey})", dnnCacheKey);
        }

        public static void SetCache(string cacheKey, object objObject)
        {
			Logger.LogDebug(">>> Entering SetCache(cacheKey={CacheKey},objObject={ObjObject})", cacheKey, objObject);
            SetCache(cacheKey, objObject, (DNNCacheDependency)null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
			Logger.LogDebug("<<< Exiting SetCache(cacheKey={CacheKey},objObject={ObjObject})", cacheKey, objObject);
        }

        public static void SetCache(string cacheKey, object objObject, DNNCacheDependency objDependency)
        {
			Logger.LogDebug(">>> Entering SetCache(cacheKey={CacheKey},objObject={ObjObject},objDependency={ObjDependency})", cacheKey, objObject, objDependency);
            SetCache(cacheKey, objObject, objDependency, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
			Logger.LogDebug("<<< Exiting SetCache(cacheKey={CacheKey},objObject={ObjObject},objDependency={ObjDependency})", cacheKey, objObject, objDependency);
        }

        public static void SetCache(string cacheKey, object objObject, DateTime absoluteExpiration)
        {
			Logger.LogDebug(">>> Entering SetCache(cacheKey={CacheKey},objObject={ObjObject},absoluteExpiration={AbsoluteExpiration})", cacheKey, objObject, absoluteExpiration);
            SetCache(cacheKey, objObject, (DNNCacheDependency)null, absoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
			Logger.LogDebug("<<< Exiting SetCache(cacheKey={CacheKey},objObject={ObjObject},absoluteExpiration={AbsoluteExpiration})", cacheKey, objObject, absoluteExpiration);
        }

        public static void SetCache(string cacheKey, object objObject, TimeSpan slidingExpiration)
        {
			Logger.LogDebug(">>> Entering SetCache(cacheKey={CacheKey},objObject={ObjObject},slidingExpiration={SlidingExpiration})", cacheKey, objObject, slidingExpiration);
            SetCache(cacheKey, objObject, (DNNCacheDependency)null, Cache.NoAbsoluteExpiration, slidingExpiration, CacheItemPriority.Normal, null);
			Logger.LogDebug("<<< Exiting SetCache(cacheKey={CacheKey},objObject={ObjObject},slidingExpiration={SlidingExpiration})", cacheKey, objObject, slidingExpiration);
        }

        public static void SetCache(string cacheKey, object objObject, DNNCacheDependency objDependency, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
			Logger.LogDebug(">>> Entering SetCache(cacheKey={CacheKey},objObject={ObjObject},objDependency={ObjDependency},absoluteExpiration={AbsoluteExpiration},slidingExpiration={SlidingExpiration})", cacheKey, objObject, objDependency, absoluteExpiration, slidingExpiration);
            SetCache(cacheKey, objObject, objDependency, absoluteExpiration, slidingExpiration, CacheItemPriority.Normal, null);
			Logger.LogDebug("<<< Exiting SetCache(cacheKey={CacheKey},objObject={ObjObject},objDependency={ObjDependency},absoluteExpiration={AbsoluteExpiration},slidingExpiration={SlidingExpiration})", cacheKey, objObject, objDependency, absoluteExpiration, slidingExpiration);
        }

        public static void SetCache(string cacheKey, object objObject, DNNCacheDependency objDependency, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
			Logger.LogDebug(">>> Entering SetCache(cacheKey={CacheKey},objObject={ObjObject},objDependency={ObjDependency},absoluteExpiration={AbsoluteExpiration},slidingExpiration={SlidingExpiration},priority={Priority},onRemoveCallback={OnRemoveCallback})", cacheKey, objObject, objDependency, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
            if (objObject != null)
            {
                // if no OnRemoveCallback value is specified, use the default method
                if (onRemoveCallback == null)
                {
                    onRemoveCallback = ItemRemovedCallback;
                }

                CachingProvider.Instance().Insert(GetDnnCacheKey(cacheKey), objObject, objDependency, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
            }
			Logger.LogDebug("<<< Exiting SetCache(cacheKey={CacheKey},objObject={ObjObject},objDependency={ObjDependency},absoluteExpiration={AbsoluteExpiration},slidingExpiration={SlidingExpiration},priority={Priority},onRemoveCallback={OnRemoveCallback})", cacheKey, objObject, objDependency, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
        }

        internal static void ItemRemovedCallback(string key, object value, CacheItemRemovedReason removedReason)
        {
			Logger.LogDebug(">>> Entering ItemRemovedCallback(key={Key},value={Value},removedReason={RemovedReason})", key, value, removedReason);
            // if the item was removed from the cache, log the key and reason to the event log
            try
            {
                if (Globals.Status == Globals.UpgradeStatus.None)
                {
                    var log = new LogInfo();
					Logger.LogDebug("ItemRemovedCallback(key={Key},value={Value},removedReason={RemovedReason}): log → {Log}", key, value, removedReason, log);
                    switch (removedReason)
                    {
                        case CacheItemRemovedReason.Removed:
                            log.LogTypeKey = nameof(EventLogType.CACHE_REMOVED);
                            break;
                        case CacheItemRemovedReason.Expired:
                            log.LogTypeKey = nameof(EventLogType.CACHE_EXPIRED);
                            break;
                        case CacheItemRemovedReason.Underused:
                            log.LogTypeKey = nameof(EventLogType.CACHE_UNDERUSED);
                            break;
                        case CacheItemRemovedReason.DependencyChanged:
                            log.LogTypeKey = nameof(EventLogType.CACHE_DEPENDENCYCHANGED);
                            break;
                    }

                    log.LogProperties.Add(new LogDetailInfo(key, removedReason.ToString()));
                    LogController.Instance.AddLog(log);
                }
            }
            catch (Exception exc)
            {
                // Swallow exception
                Logger.Error(exc);
            }
			Logger.LogDebug("<<< Exiting ItemRemovedCallback(key={Key},value={Value},removedReason={RemovedReason})", key, value, removedReason);
        }

        internal static TObject GetCachedData<TObject>(PerformanceSettings performance, CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired, bool storeInDictionary)
        {
            object objObject = storeInDictionary
                                   ? GetCachedDataFromDictionary(cacheItemArgs, cacheItemExpired)
                                   : GetCachedDataFromRuntimeCache(performance, cacheItemArgs, cacheItemExpired);

            // return the object
            if (objObject == null)
            {
				Logger.LogDebug(">>> Entering GetCachedDataFromDictionary(cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired})", cacheItemArgs, cacheItemExpired);
				Logger.LogError("Exception in GetCachedDataFromDictionary(cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired}): {Arg2}", cacheItemArgs, cacheItemExpired, ex.getMessage());
				Logger.LogDebug("<<< Exiting GetCachedDataFromDictionary(cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired})", cacheItemArgs, cacheItemExpired);
                return default(TObject);
            }

            return (TObject)objObject;
        }

        private static string GetDnnCacheKey(string cacheKey)
        {
			Logger.LogDebug(">>> Entering GetDnnCacheKey(cacheKey={CacheKey})", cacheKey);
			Logger.LogDebug("<<< Exiting GetDnnCacheKey(cacheKey={CacheKey})", cacheKey);
            return CachingProvider.GetCacheKey(cacheKey);
        }

        private static string CleanCacheKey(string cacheKey)
        {
			Logger.LogDebug(">>> Entering CleanCacheKey(cacheKey={CacheKey})", cacheKey);
			Logger.LogDebug("<<< Exiting CleanCacheKey(cacheKey={CacheKey})", cacheKey);
            return CachingProvider.CleanCacheKey(cacheKey);
        }

        private static object GetCachedDataFromRuntimeCache(PerformanceSettings performance, CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired)
        {
			Logger.LogDebug(">>> Entering GetCachedDataFromRuntimeCache(performance={Performance},cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired})", performance, cacheItemArgs, cacheItemExpired);
            object objObject = GetCache(cacheItemArgs.CacheKey);

			Logger.LogDebug("GetCachedDataFromRuntimeCache(performance={Performance},cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired}): objObject → {ObjObject}", performance, cacheItemArgs, cacheItemExpired, objObject);
            // if item is not cached
            if (objObject == null)
            {
                // Get Unique Lock for cacheKey
                object @lock = GetUniqueLockObject(cacheItemArgs.CacheKey);

                // prevent other threads from entering this block while we regenerate the cache
                lock (@lock)
                {
                    // try to retrieve object from the cache again (in case another thread loaded the object since we first checked)
                    objObject = GetCache(cacheItemArgs.CacheKey);

                    // if object was still not retrieved
                    if (objObject == null)
                    {
                        // get object from data source using delegate
                        try
                        {
                            objObject = cacheItemExpired(cacheItemArgs);
                        }
                        catch (Exception ex)
                        {
							Logger.LogError("Exception in GetCachedDataFromRuntimeCache(performance={Performance},cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired}): {Arg3}", performance, cacheItemArgs, cacheItemExpired, ex.getMessage());
                            objObject = null;
                            Exceptions.LogException(ex);
                        }

                        // set cache timeout
                        int timeOut = cacheItemArgs.CacheTimeOut * (int)performance;
						Logger.LogDebug("GetCachedDataFromRuntimeCache(performance={Performance},cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired}): timeOut → {TimeOut}", performance, cacheItemArgs, cacheItemExpired, timeOut);

                        // if we retrieved a valid object and we are using caching
                        if (objObject != null && timeOut > 0)
                        {
                            // save the object in the cache
                            SetCache(
                                cacheItemArgs.CacheKey,
                                objObject,
                                cacheItemArgs.CacheDependency,
                                Cache.NoAbsoluteExpiration,
                                TimeSpan.FromMinutes(timeOut),
                                cacheItemArgs.CachePriority,
                                cacheItemArgs.CacheCallback);

                            // check if the item was actually saved in the cache
                            if (GetCache(cacheItemArgs.CacheKey) == null)
                            {
                                // log the event if the item was not saved in the cache ( likely because we are out of memory )
                                var log = new LogInfo { LogTypeKey = nameof(EventLogType.CACHE_OVERFLOW) };
								Logger.LogDebug("GetCachedDataFromRuntimeCache(performance={Performance},cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired}): log → {Log}", performance, cacheItemArgs, cacheItemExpired, log);
                                log.LogProperties.Add(new LogDetailInfo(cacheItemArgs.CacheKey, "Overflow - Item Not Cached"));
                                LogController.Instance.AddLog(log);
                            }
                        }

                        // This thread won so remove unique Lock from collection
                        RemoveUniqueLockObject(cacheItemArgs.CacheKey);
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetCachedDataFromRuntimeCache(performance={Performance},cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired})", performance, cacheItemArgs, cacheItemExpired);
            return objObject;
        }

        private static object GetCachedDataFromDictionary(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired)
        {
			Logger.LogDebug(">>> Entering GetCachedDataFromDictionary(cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired})", cacheItemArgs, cacheItemExpired);
            object cachedObject;

            bool isFound;
            using (DictionaryCache.GetReadLock())
            {
                isFound = DictionaryCache.TryGetValue(cacheItemArgs.CacheKey, out cachedObject);
            }
			Logger.LogDebug("GetCachedDataFromDictionary(cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired}): isFound → {IsFound}", cacheItemArgs, cacheItemExpired, isFound);

            if (!isFound)
            {
                // get object from data source using delegate
                try
                {
                    cachedObject = cacheItemExpired != null ? cacheItemExpired(cacheItemArgs) : null;
                }
                catch (Exception ex)
                {
					Logger.LogError("Exception in GetCachedDataFromDictionary(cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired}): {Arg2}", cacheItemArgs, cacheItemExpired, ex.getMessage());
                    cachedObject = null;
                    Exceptions.LogException(ex);
					Logger.LogDebug("GetCachedDataFromDictionary(cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired}): cachedObject → {CachedObject}", cacheItemArgs, cacheItemExpired, cachedObject);
                }

                using (DictionaryCache.GetWriteLock())
                {
                    if (!DictionaryCache.ContainsKey(cacheItemArgs.CacheKey))
                    {
                        if (cachedObject != null)
                        {
                            DictionaryCache[cacheItemArgs.CacheKey] = cachedObject;
                        }
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetCachedDataFromDictionary(cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired})", cacheItemArgs, cacheItemExpired);
            return cachedObject;
        }

        private static object GetUniqueLockObject(string key)
        {
			Logger.LogDebug(">>> Entering GetUniqueLockObject(key={Key})", key);
            object @lock = null;
            if (DictionaryLock.TryEnterReadLock(FiveSeconds))
            {
                try
                {
                    // Try to get lock Object (for key) from Dictionary
                    if (LockDictionary.TryGetValue(key, out var value))
                    {
                        @lock = value;
                    }
                }
                finally
                {
                    DictionaryLock.ExitReadLock();
                }
            }

            if (@lock == null)
            {
                if (DictionaryLock.TryEnterWriteLock(FiveSeconds))
                {
                    try
                    {
                        // Double check dictionary
                        if (!LockDictionary.TryGetValue(key, out var value))
                        {
                            // Create new lock
                            value = new object();
                            LockDictionary[key] = value;
							Logger.LogDebug("GetUniqueLockObject(key={Key}): value → {Value}", key, value);
                        }

                        // Retrieve lock
                        @lock = value;
                    }
                    finally
                    {
                        DictionaryLock.ExitWriteLock();
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetUniqueLockObject(key={Key})", key);
            return @lock;
        }

        private static void RemoveUniqueLockObject(string key)
        {
			Logger.LogDebug(">>> Entering RemoveUniqueLockObject(key={Key})", key);
            if (!DictionaryLock.TryEnterWriteLock(FiveSeconds))
            {
                return;
            }

            try
            {
                // check dictionary
                if (LockDictionary.ContainsKey(key))
                {
                    // Remove lock
                    LockDictionary.Remove(key);
                }
            }
            finally
            {
                DictionaryLock.ExitWriteLock();
            }
			Logger.LogDebug("<<< Exiting RemoveUniqueLockObject(key={Key})", key);
        }
    }
}
