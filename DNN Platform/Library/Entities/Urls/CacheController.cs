// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Web.Caching;

    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Log.EventLog;

    public class CacheController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CacheController));
        internal const string VanityUrlLookupKey = "url_VanityUrlLookup_{0}";

        private const string PageIndexKey = "url_PageIndex";
        private const string PageIndexDepthKey = "url_PageIndexDepth";
        private const string UrlDictKey = "url_UrlDict";
        private const string UrlPortalsKey = "url_UrlPortals";
        private const string CustomAliasTabsKey = "url_CustomAliasTabsKey";
        private const string PortalAliasListKey = "url_PortalAliasList";
        private const string PortalsKey = "url_Portals";
        private const string CustomPortalAliasesKey = "url_CustomAliasList";
        private const string FriendlyUrlSettingsKey = "url_FriendlyUrlSettings";
        private const string RedirectActionsKey = "url_ParameterRedirectActions_{0}";
        private const string RewriteActionsKey = "url_ParameterRewriteActions_{0}";
        private const string PortalAliasesKey = "url_PortalAliases";
        private const string UserProfileActionsKey = "url_UserProfileActions";
        private const string PortalModuleProvidersForTabKey = "url_ModuleProvidersForTab_{0}_{1}";
        private const string PortalModuleProvidersAllTabsKey = "url_ModuleProvidersAllTabs_{0}";
        private const string PortalModuleProviderTabsKey = "url_PortalModuleProviderTabs_{0}";
        private const string AlwaysCallProviderTabsKey = "url_AlwaysCallProviderTabs_{0}";
        private const string HomePageSkinsKey = "url_HomePageSkins_{0}";
        private const string TabPathsKey = "url_TabPathsKey_{0}";
        private static readonly string[] PortalCacheDependencyKeys = ["DNN_PortalDictionary",];
        private static CacheItemRemovedReason cacheItemRemovedReason;
        private static bool logRemovedReason;
        private CacheItemRemovedCallback onRemovePageIndex;

        public static void FlushFriendlyUrlSettingsFromCache()
        {
			Logger.LogDebug(">>> Entering FlushFriendlyUrlSettingsFromCache()");
            DataCache.RemoveCache(FriendlyUrlSettingsKey);
			Logger.LogDebug("<<< Exiting FlushFriendlyUrlSettingsFromCache()");
        }

        public static void FlushPageIndexFromCache()
        {
			Logger.LogDebug(">>> Entering FlushPageIndexFromCache()");
            DataCache.RemoveCache(UrlDictKey);
            DataCache.RemoveCache(PageIndexKey);
            DataCache.RemoveCache(PageIndexDepthKey);
            DataCache.RemoveCache(UrlPortalsKey);
            DataCache.RemoveCache(UserProfileActionsKey);
            DataCache.RemoveCache(PortalAliasListKey);
            DataCache.RemoveCache(PortalAliasesKey);
            DataCache.RemoveCache(TabPathsKey);
			Logger.LogDebug("<<< Exiting FlushPageIndexFromCache()");
        }

        /// <summary>Returns a portal info object for the portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="exceptionOnNull">Whether to throw an exception if the portal is not found.</param>
        /// <remarks>This method wraps the PortalController.GetPortal method, and adds a check if the result is null.</remarks>.
        /// <returns>A <see cref="PortalInfo"/> instance or <see langword="null"/> if the portal isn't found and <paramref name="exceptionOnNull"/> is <see langword="false"/>.</returns>
        public static PortalInfo GetPortal(int portalId, bool exceptionOnNull)
        {
			Logger.LogDebug(">>> Entering GetPortal(portalId={PortalId},exceptionOnNull={ExceptionOnNull})", portalId, exceptionOnNull);
            PortalInfo pi = null;

			Logger.LogDebug("GetPortal(portalId={PortalId},exceptionOnNull={ExceptionOnNull}): pi → {Pi}", portalId, exceptionOnNull, pi);
            // 775 : change to use threadsafe dictionary
            SharedDictionary<int, PortalInfo> portals = (SharedDictionary<int, PortalInfo>)DataCache.GetCache(PortalsKey) ??
                                                            new SharedDictionary<int, PortalInfo>();

            using (portals.GetWriteLock())
            {
                if (portals.TryGetValue(portalId, out var portal))
                {
                    // portal found, return
                    pi = portal;
                }
                else
                {
                    try
                    {
                        // if not found, get from database
                        pi = PortalController.Instance.GetPortal(portalId);

                        if (pi == null)
                        {
                            // Home page redirect loop when using default language not en-US and first request with secondary language
                            // calls get portal using culture code to support
                            string cultureCode = PortalController.GetActivePortalLanguage(portalId);
                            pi = PortalController.Instance.GetPortal(portalId, cultureCode);
                        }

                        if (pi != null)
                        {
                            // Home page redirect loop when using default language not en-US and first request with secondary language
                            // check for correct, default language code in portal object
                            string portalCultureCode = pi.CultureCode;
                            if (portalCultureCode != null && !string.Equals(portalCultureCode, pi.DefaultLanguage, StringComparison.Ordinal))
                            {
                                // portal culture code and default culture code are not the same.
                                // this means we will get the incorrect home page tab id
                                // call back and get the correct one as per the default language
                                PortalInfo defaultLangPortal = PortalController.Instance.GetPortal(portalId, pi.DefaultLanguage);
								Logger.LogDebug("GetPortal(portalId={PortalId},exceptionOnNull={ExceptionOnNull}): defaultLangPortal → {DefaultLangPortal}", portalId, exceptionOnNull, defaultLangPortal);
                                if (defaultLangPortal != null)
                                {
                                    pi = defaultLangPortal;
                                }
                            }
                        }

                        if (pi != null)
                        {
                            // add to dictionary and re-store in cache
                            portals.Add(pi.PortalID, pi);
                            DataCache.SetCache(PortalsKey, portals); // store back in dictionary
                        }
                    }

                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
						Logger.LogError("Exception in GetPortal(portalId={PortalId},exceptionOnNull={ExceptionOnNull}): {Arg2}", portalId, exceptionOnNull, ex.Message);
                        // 912: capture as fall back any exception resulting from doing a portal lookup in 6.x
                        // this happens when portalId = -1
                        // no long, no handling, just pass onwards with null portal
                    }
                }
            }

            if (exceptionOnNull && pi == null)
            {
				Logger.LogWarning("Throwing PortalNotFoundException from GetPortal(portalId={PortalId},exceptionOnNull={ExceptionOnNull})", portalId, exceptionOnNull);
			Logger.LogDebug("<<< Exiting GetPortal(portalId={PortalId},exceptionOnNull={ExceptionOnNull})", portalId, exceptionOnNull);
                throw new PortalNotFoundException($"No Portal Found for portalid : {portalId}");
            }

            return pi;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void RemovedPageIndexCallBack(string k, object v, CacheItemRemovedReason r)
        {
			Logger.LogDebug(">>> Entering RemovedPageIndexCallBack(k={K},v={V},r={R})", k, v, r);
			Logger.LogError("Exception in RemovedPageIndexCallBack(k={K},v={V},r={R}): {Arg3}", k, v, r, ConfigurationErrorsException.getMessage());
			Logger.LogDebug("<<< Exiting RemovedPageIndexCallBack(k={K},v={V},r={R})", k, v, r);
            cacheItemRemovedReason = r;
#if DEBUG
			Logger.LogDebug("RemovedPageIndexCallBack(k={K},v={V},r={R}): cacheItemRemovedReason → {CacheItemRemovedReason}", k, v, r, cacheItemRemovedReason);
            if (logRemovedReason)
            {
                var log = new LogInfo { LogTypeKey = "HOST_ALERT", };
				Logger.LogDebug("RemovedPageIndexCallBack(k={K},v={V},r={R}): log → {Log}", k, v, r, log);

                string itemName;
                string count;
                List<string> portalCounts = null;
				Logger.LogDebug("RemovedPageIndexCallBack(k={K},v={V},r={R}): portalCounts → {PortalCounts}", k, v, r, portalCounts);
                switch (k)
                {
                    case "DNN_" + PageIndexKey:
                        itemName = "Page Index";

						Logger.LogDebug("RemovedPageIndexCallBack(k={K},v={V},r={R}): itemName → {ItemName}", k, v, r, itemName);
                        // user profile actions
                        try
                        {
                            DataCache.RemoveCache(UserProfileActionsKey);
                        }
                        catch (ConfigurationErrorsException)
                        {
                            // do nothing, this means the web.config file was overwritten, and thus the cache
                            // was cleared.
                        }

                        if (v is SharedDictionary<string, string> stringDict)
                        {
                            count = "Item Count: " + stringDict.Values.Count;
                        }
                        else
                        {
                            count = "N/a";
                        }

                        break;
                    case "DNN_" + UrlDictKey:
                        itemName = "Friendly Url List";
                        if (v is SharedDictionary<int, SharedDictionary<string, string>> friendlyUrls)
                        {
                            portalCounts = [];
                            using (friendlyUrls.GetReadLock())
                            {
                                count = $"Portal Count: {friendlyUrls.Count}";
                                foreach (int key in friendlyUrls.Keys)
                                {
                                    var portalUrls = friendlyUrls[key];
                                    using (portalUrls.GetReadLock())
                                    {
                                        portalCounts.Add($"Portal {key} Item Count :{portalUrls.Count}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            count = "N/a";
                        }

                        break;
                    default:
                        itemName = "Url Rewriter Cache Item";
                        count = string.Empty;
                        break;
                }

                // add log values
                log.AddProperty("Url Rewriting Caching Message", itemName + " Cache item Removed.");
                log.AddProperty("Reason", cacheItemRemovedReason.ToString());
                log.AddProperty("Cache Item Key", k);
                log.AddProperty("Item Count", count);
                if (portalCounts != null)
                {
                    int i = 0;
                    foreach (string item in portalCounts)
                    {
                        log.AddProperty($"Item {i}", item);
                        i++;
                    }
                }

                ////System.Diagnostics.Trace.Assert(k != null, "k == " + k);
                LogController.Instance.AddLog(log);
            }
#endif
        }

        /// <summary>Finds the best match friendlyurlparms.config file path.</summary>
        /// <param name="portalId">The portalId to search for. -1 if all portals required.</param>
        /// <param name="portalSpecificFound">Whether a portal specific config file was found.</param>
        /// <returns>If a non-zero length string, a valid file path.  If a zero length string, no file was found.</returns>
        /// <remarks>
        /// First priority is a file called n.friendlyurlparms.config, in the root path
        /// Next priority is a file called portals\n\friendlyurlparms.config, in the portal path
        /// Last priority is the friendlyurlparms.config file, in the root path
        /// Task no 807.
        /// </remarks>
        internal static string FindFriendlyUrlParmsConfigFilePath(int portalId, out bool portalSpecificFound)
        {
			Logger.LogDebug(">>> Entering FindFriendlyUrlParmsConfigFilePath(portalId={PortalId})", portalId);
            string result = string.Empty;
            portalSpecificFound = false;
            const string fileName = "friendlyUrlParms.config";
			Logger.LogDebug("FindFriendlyUrlParmsConfigFilePath(portalId={PortalId}): portalSpecificFound → {PortalSpecificFound}", portalId, portalSpecificFound);
            string rootPath = $@"{Globals.ApplicationMapPath}\";

            string filePath;
			Logger.LogDebug("FindFriendlyUrlParmsConfigFilePath(portalId={PortalId}): result → {Result}", portalId, result);
            if (portalId > -1)
            {
                // specific portal
                // first look in the root folder with the portalid as a prefix
                filePath = $"{rootPath}{portalId.ToString(CultureInfo.InvariantCulture)}.{fileName}";
            }
            else
            {
                // no specific portal
                filePath = rootPath + fileName; // just the pathname
            }

            if (File.Exists(filePath))
            {
                result = filePath;
                if (portalId > -1)
                {
                    // if there was a portal specified, this was a portal specific path
                    portalSpecificFound = true;
                }
            }
            else
            {
                // no portal specific name in the root folder
                if (portalId > Null.NullInteger)
                {
                    // at this point, only interested if a specific portal is requested
                    var portal = PortalController.Instance.GetPortal(portalId);
					Logger.LogDebug("FindFriendlyUrlParmsConfigFilePath(portalId={PortalId}): portal → {Portal}", portalId, portal);
                    if (portal != null)
                    {
                        // looking for the file in the portal folder
                        var portalPath = portal.HomeDirectoryMapPath;
                        filePath = portalPath + fileName; // just the pathname
                        if (File.Exists(filePath))
                        {
                            result = filePath;
                            portalSpecificFound = true;
                        }
                    }
                }

                if (string.IsNullOrEmpty(result))
                {
                    // nothing matching found : now just looking for the root path
                    filePath = rootPath + fileName;
                    if (File.Exists(filePath))
                    {
                        result = filePath;
                        portalSpecificFound = false;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting FindFriendlyUrlParmsConfigFilePath(portalId={PortalId})", portalId);
            return result;
        }

        internal static List<int> GetAlwaysCallProviderTabs(int portalId)
        {
			Logger.LogDebug(">>> Entering GetAlwaysCallProviderTabs(portalId={PortalId})", portalId);
            List<int> result = null;
            string key = string.Format(CultureInfo.InvariantCulture, AlwaysCallProviderTabsKey, portalId);
			Logger.LogWarning("GetAlwaysCallProviderTabs(portalId={PortalId}): key → {Key}", portalId, key);
            var tabIdsToAlwaysCall = (int[])DataCache.GetCache(key);
			Logger.LogDebug("GetAlwaysCallProviderTabs(portalId={PortalId}): tabIdsToAlwaysCall → {TabIdsToAlwaysCall}", portalId, tabIdsToAlwaysCall);
			Logger.LogDebug("GetAlwaysCallProviderTabs(portalId={PortalId}): result → {Result}", portalId, result);
            if (tabIdsToAlwaysCall != null)
            {
                result = new List<int>(tabIdsToAlwaysCall);
            }

			Logger.LogDebug("<<< Exiting GetAlwaysCallProviderTabs(portalId={PortalId})", portalId);
            return result;
        }

        // 770 : customised portal alias per-tab
        internal static List<string> GetCustomAliasesFromCache()
        {
			Logger.LogDebug(">>> Entering GetCustomAliasesFromCache()");
            object raw = DataCache.GetCache(CustomPortalAliasesKey);
			Logger.LogDebug("GetCustomAliasesFromCache(): raw → {Raw}", raw);
			Logger.LogDebug("<<< Exiting GetCustomAliasesFromCache()");
            return (List<string>)raw;
        }

        internal static void ClearCustomAliasesCache()
        {
			Logger.LogDebug(">>> Entering ClearCustomAliasesCache()");
            DataCache.ClearCache(CustomPortalAliasesKey);
			Logger.LogDebug("<<< Exiting ClearCustomAliasesCache()");
        }

        internal static Hashtable GetHomePageSkinsFromCache(int portalId)
        {
			Logger.LogDebug(">>> Entering GetHomePageSkinsFromCache(portalId={PortalId})", portalId);
            string key = string.Format(CultureInfo.InvariantCulture, HomePageSkinsKey, portalId);
			Logger.LogWarning("GetHomePageSkinsFromCache(portalId={PortalId}): key → {Key}", portalId, key);
			Logger.LogDebug("<<< Exiting GetHomePageSkinsFromCache(portalId={PortalId})", portalId);
            return (Hashtable)DataCache.GetCache(key);
        }

        internal static List<int> GetListOfTabsWithProviders(int portalId, FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering GetListOfTabsWithProviders(portalId={PortalId},settings={Settings})", portalId, settings);
            List<int> result = null;
            string key = string.Format(CultureInfo.InvariantCulture, PortalModuleProviderTabsKey, portalId);
			Logger.LogWarning("GetListOfTabsWithProviders(portalId={PortalId},settings={Settings}): key → {Key}", portalId, settings, key);
            var tabIdsForPortal = (int[])DataCache.GetCache(key);
			Logger.LogDebug("GetListOfTabsWithProviders(portalId={PortalId},settings={Settings}): tabIdsForPortal → {TabIdsForPortal}", portalId, settings, tabIdsForPortal);
			Logger.LogDebug("GetListOfTabsWithProviders(portalId={PortalId},settings={Settings}): result → {Result}", portalId, settings, result);
            if (tabIdsForPortal != null)
            {
                result = new List<int>(tabIdsForPortal);
            }

			Logger.LogDebug("<<< Exiting GetListOfTabsWithProviders(portalId={PortalId},settings={Settings})", portalId, settings);
            return result;
        }

        internal static Dictionary<int, List<ParameterRedirectAction>> GetParameterRedirects(FriendlyUrlSettings settings, int portalId, ref List<string> messages)
        {
            string redirectActionKey = string.Format(CultureInfo.InvariantCulture, RedirectActionsKey, portalId); // cached one portal at a time
            if (messages == null)
            {
                messages = [];
            }

            var redirectActions = (Dictionary<int, List<ParameterRedirectAction>>)DataCache.GetCache(redirectActionKey);
            if (redirectActions == null)
            {
                try
                {
                    redirectActions = new Dictionary<int, List<ParameterRedirectAction>>();

                    // 807 : look for portal specific files
                    string fileName = FindFriendlyUrlParmsConfigFilePath(portalId, out var portalSpecific);
                    if (fileName != string.Empty)
                    {
                        redirectActions.LoadFromXmlFile(fileName, portalId, portalSpecific, ref messages);
                    }

                    CacheDependency fileDependency = null;
                    if (File.Exists(fileName))
                    {
                        fileDependency = new CacheDependency(fileName);
                    }

                    if (settings != null)
                    {
                        DateTime absoluteExpiration = DateTime.Now.Add(settings.CacheTime);
                        DataCache.SetCache(redirectActionKey, redirectActions, new DNNCacheDependency(fileDependency), absoluteExpiration, Cache.NoSlidingExpiration);
                    }
                    else
                    {
                        DataCache.SetCache(redirectActionKey, redirectActions, new DNNCacheDependency(fileDependency));
                    }
                }
                catch (Exception ex)
                {
                    // log the exception
                    Services.Exceptions.Exceptions.LogException(ex);
                    messages.Add("Exception: " + ex.Message + "\n" + ex.StackTrace);
                }
            }

            return redirectActions;
        }

        internal static Dictionary<int, List<ParameterReplaceAction>> GetParameterReplacements(FriendlyUrlSettings settings, int portalId, ref List<string> messages)
        {
            string replaceActionKey = "replaceActions:" + portalId.ToString(CultureInfo.InvariantCulture);
            var replaceActions = (Dictionary<int, List<ParameterReplaceAction>>)DataCache.GetCache(replaceActionKey);
            if (messages == null)
            {
                messages = new List<string>();
            }

            if (replaceActions == null)
            {
                replaceActions = new Dictionary<int, List<ParameterReplaceAction>>();
                try
                {
                    bool portalSpecific;
                    string fileName = FindFriendlyUrlParmsConfigFilePath(portalId, out portalSpecific);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        replaceActions.LoadFromXmlFile(fileName, portalId, portalSpecific, ref messages);
                    }

                    CacheDependency cacheDependency = null;
                    if (replaceActions.Count > 0)
                    {
                        // only set filename dependency if file exists
                        cacheDependency = new CacheDependency(fileName);
                    }

                    DateTime expiration = DateTime.MaxValue;
                    DataCache.SetCache(replaceActionKey, replaceActions, new DNNCacheDependency(cacheDependency), expiration, settings.CacheTime);
                }
                catch (Exception ex)
                {
                    Services.Exceptions.Exceptions.LogException(ex);
                    messages.Add("Exception: " + ex.Message + "\n" + ex.StackTrace);
                }
            }

            return replaceActions;
        }

        internal static Dictionary<int, SharedList<ParameterRewriteAction>> GetParameterRewrites(int portalId, ref List<string> messages, Guid parentTraceId)
        {
            string rewriteActionKey = string.Format(CultureInfo.InvariantCulture, RewriteActionsKey, portalId);
            if (messages == null)
            {
                messages = [];
            }

            var rewriteActions = (Dictionary<int, SharedList<ParameterRewriteAction>>)DataCache.GetCache(rewriteActionKey);
            if (rewriteActions == null)
            {
                try
                {
                    rewriteActions = new Dictionary<int, SharedList<ParameterRewriteAction>>();

                    // 807 : new change to look for portal rule files in portal specific locations
                    string filename = FindFriendlyUrlParmsConfigFilePath(portalId, out var portalSpecific);
                    if (!string.IsNullOrEmpty(filename))
                    {
                        rewriteActions.LoadFromXmlFile(filename, portalId, portalSpecific, ref messages);
                    }

                    CacheDependency fileDependency = null;
                    if (File.Exists(filename))
                    {
                        fileDependency = new CacheDependency(filename);
                    }

                    DataCache.SetCache(rewriteActionKey, rewriteActions, new DNNCacheDependency(fileDependency));
                }
                catch (Exception ex)
                {
                    Services.Exceptions.Exceptions.LogException(ex);
                    messages.Add("Exception: " + ex.Message + "\n" + ex.StackTrace);
                }
            }

            return rewriteActions;
        }

        internal static OrderedDictionary GetPortalAliasesFromCache()
        {
			Logger.LogDebug(">>> Entering GetPortalAliasesFromCache()");
            object raw = DataCache.GetCache(PortalAliasesKey);
			Logger.LogDebug("GetPortalAliasesFromCache(): raw → {Raw}", raw);
			Logger.LogDebug("<<< Exiting GetPortalAliasesFromCache()");
            return (raw != null) ? (OrderedDictionary)raw : null;
        }

        internal static List<ExtensionUrlProvider> GetProvidersForTabAndPortal(
            int tabId,
            int portalId,
            FriendlyUrlSettings settings,
            out bool noSuchProvider,
            Guid parentTraceId)
        {
            // get list of tabids in this portal that have providers
			Logger.LogDebug(">>> Entering GetProvidersForTabAndPortal(tabId={TabId},portalId={PortalId},settings={Settings},parentTraceId={ParentTraceId})", tabId, portalId, settings, parentTraceId);
            noSuchProvider = false;
            List<ExtensionUrlProvider> allCachedProviders = null;
			Logger.LogDebug("GetProvidersForTabAndPortal(tabId={TabId},portalId={PortalId},settings={Settings},parentTraceId={ParentTraceId}): allCachedProviders → {AllCachedProviders}", tabId, portalId, settings, parentTraceId, allCachedProviders);
			Logger.LogDebug("GetProvidersForTabAndPortal(tabId={TabId},portalId={PortalId},settings={Settings},parentTraceId={ParentTraceId}): noSuchProvider → {NoSuchProvider}", tabId, portalId, settings, parentTraceId, noSuchProvider);

            List<int> tabsWithProviders = GetListOfTabsWithProviders(portalId, settings);
			Logger.LogDebug("GetProvidersForTabAndPortal(tabId={TabId},portalId={PortalId},settings={Settings},parentTraceId={ParentTraceId}): tabsWithProviders → {TabsWithProviders}", tabId, portalId, settings, parentTraceId, tabsWithProviders);
            bool checkThisTab = true, checkAllTabs = true; // going to check all tabs, unless find otherwise in the cache
			Logger.LogDebug("GetProvidersForTabAndPortal(tabId={TabId},portalId={PortalId},settings={Settings},parentTraceId={ParentTraceId}): checkThisTab → {CheckThisTab}", tabId, portalId, settings, parentTraceId, checkThisTab);
            if (tabsWithProviders != null)
            {
                checkAllTabs = tabsWithProviders.Contains(RewriteController.AllTabsRewrite);

				Logger.LogDebug("GetProvidersForTabAndPortal(tabId={TabId},portalId={PortalId},settings={Settings},parentTraceId={ParentTraceId}): checkAllTabs → {CheckAllTabs}", tabId, portalId, settings, parentTraceId, checkAllTabs);
                checkThisTab = tabsWithProviders.Contains(tabId);

                if (!checkThisTab && !checkAllTabs)
                {
                    noSuchProvider = true; // we got the list of tabs, there is no provider for this tab
                }
            }

            if (checkAllTabs)
            {
                // the portal has an 'all tabs' provider in it
                string allTabsKey = string.Format(CultureInfo.InvariantCulture, PortalModuleProvidersAllTabsKey, portalId);
                var cachedAllTabsProviders = (List<ExtensionUrlProvider>)DataCache.GetCache(allTabsKey);
				Logger.LogDebug("GetProvidersForTabAndPortal(tabId={TabId},portalId={PortalId},settings={Settings},parentTraceId={ParentTraceId}): cachedAllTabsProviders → {CachedAllTabsProviders}", tabId, portalId, settings, parentTraceId, cachedAllTabsProviders);
                if (cachedAllTabsProviders != null)
                {
                    allCachedProviders = [];
                    allCachedProviders.AddRange(cachedAllTabsProviders);
                }
            }

            // the specified tab
            if (checkThisTab)
            {
                // tab exists, get the providers for this tab
                string key = string.Format(CultureInfo.InvariantCulture, PortalModuleProvidersForTabKey, portalId, tabId);
				Logger.LogWarning("GetProvidersForTabAndPortal(tabId={TabId},portalId={PortalId},settings={Settings},parentTraceId={ParentTraceId}): key → {Key}", tabId, portalId, settings, parentTraceId, key);
                var cachedTabProviders = (List<ExtensionUrlProvider>)DataCache.GetCache(key);
				Logger.LogDebug("GetProvidersForTabAndPortal(tabId={TabId},portalId={PortalId},settings={Settings},parentTraceId={ParentTraceId}): cachedTabProviders → {CachedTabProviders}", tabId, portalId, settings, parentTraceId, cachedTabProviders);
                if (cachedTabProviders != null)
                {
                    if (allCachedProviders == null)
                    {
                        allCachedProviders = new List<ExtensionUrlProvider>();
                    }

                    allCachedProviders.AddRange(cachedTabProviders);
                }
            }

			Logger.LogDebug("<<< Exiting GetProvidersForTabAndPortal(tabId={TabId},portalId={PortalId},settings={Settings},parentTraceId={ParentTraceId})", tabId, portalId, settings, parentTraceId);
            return allCachedProviders;
        }

        internal static SharedDictionary<string, string> GetTabPathsFromCache(int portalId)
        {
            return (SharedDictionary<string, string>)DataCache.GetCache(string.Format(CultureInfo.InvariantCulture, TabPathsKey, portalId));
        }

        internal static void StoreAlwaysCallProviderTabs(int portalId, List<int> alwaysCallTabIds, FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering StoreAlwaysCallProviderTabs(portalId={PortalId},alwaysCallTabIds={AlwaysCallTabIds},settings={Settings})", portalId, alwaysCallTabIds, settings);
            if (alwaysCallTabIds != null)
            {
                SetPageCache(string.Format(CultureInfo.InvariantCulture, AlwaysCallProviderTabsKey, portalId), alwaysCallTabIds.ToArray(), settings);
            }
			Logger.LogDebug("<<< Exiting StoreAlwaysCallProviderTabs(portalId={PortalId},alwaysCallTabIds={AlwaysCallTabIds},settings={Settings})", portalId, alwaysCallTabIds, settings);
        }

        internal static void StoreCustomAliasesInCache(List<string> aliases, FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering StoreCustomAliasesInCache(aliases={Aliases},settings={Settings})", aliases, settings);
            DateTime absoluteExpiration = DateTime.Now.Add(new TimeSpan(24, 0, 0));
            if (settings != null)
            {
                absoluteExpiration = DateTime.Now.Add(settings.CacheTime);
            }

            DataCache.SetCache(CustomPortalAliasesKey, aliases, absoluteExpiration);
			Logger.LogDebug("<<< Exiting StoreCustomAliasesInCache(aliases={Aliases},settings={Settings})", aliases, settings);
        }

        internal static void StoreHomePageSkinsInCache(int portalId, Hashtable homePageSkins)
        {
			Logger.LogDebug(">>> Entering StoreHomePageSkinsInCache(portalId={PortalId},homePageSkins={HomePageSkins})", portalId, homePageSkins);
            if (homePageSkins is { Count: > 0, })
            {
                DataCache.SetCache(string.Format(CultureInfo.InvariantCulture, HomePageSkinsKey, portalId), homePageSkins);
            }
			Logger.LogDebug("<<< Exiting StoreHomePageSkinsInCache(portalId={PortalId},homePageSkins={HomePageSkins})", portalId, homePageSkins);
        }

        /// <summary>
        /// This method stores a list of tabIds for the specific portal in the cache
        /// This is used to lookup and see if there are any providers to load for a tab,
        /// without having to store individual tabid/portaldId provider lists for every tab
        /// If a tab doesn't appear on this cached list, then the cache isn't checked
        /// for that particular tabid/portalId combination.
        /// </summary>
        /// <param name="providers">A list of extension providers.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settings">The friendly URL settings.</param>
        internal static void StoreListOfTabsWithProviders(List<ExtensionUrlProvider> providers, int portalId, FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering StoreListOfTabsWithProviders(providers={Providers},portalId={PortalId},settings={Settings})", providers, portalId, settings);
            // go through the list of providers and store all the tabs that they are configured for
            var providersWithTabs = new List<int>();
			Logger.LogDebug("StoreListOfTabsWithProviders(providers={Providers},portalId={PortalId},settings={Settings}): providersWithTabs → {ProvidersWithTabs}", providers, portalId, settings, providersWithTabs);
            var providersWithTabsStr = new List<string>();
			Logger.LogDebug("StoreListOfTabsWithProviders(providers={Providers},portalId={PortalId},settings={Settings}): providersWithTabsStr → {ProvidersWithTabsStr}", providers, portalId, settings, providersWithTabsStr);
            foreach (ExtensionUrlProvider provider in providers)
            {
                if (provider.ProviderConfig.AllTabs)
                {
                    if (providersWithTabs.Contains(RewriteController.AllTabsRewrite) == false)
                    {
                        providersWithTabs.Add(RewriteController.AllTabsRewrite);
                        providersWithTabsStr.Add("AllTabs");
                    }
                }

                if (provider.AlwaysUsesDnnPagePath(portalId) == false)
                {
                    if (providersWithTabs.Contains(RewriteController.SiteRootRewrite) == false)
                    {
                        providersWithTabs.Add(RewriteController.SiteRootRewrite);
                        providersWithTabsStr.Add("NoPath");
                    }
                }

                foreach (int providerTabId in provider.ProviderConfig.TabIds)
                {
                    if (!providersWithTabs.Contains(providerTabId))
                    {
                        providersWithTabs.Add(providerTabId);
                        providersWithTabsStr.Add(providerTabId.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }

            // store list as array in cache
            string key = string.Format(CultureInfo.InvariantCulture, PortalModuleProviderTabsKey, portalId);
			Logger.LogWarning("StoreListOfTabsWithProviders(providers={Providers},portalId={PortalId},settings={Settings}): key → {Key}", providers, portalId, settings, key);
            SetPageCache(key, providersWithTabs.ToArray(), settings);
            if (settings.LogCacheMessages)
            {
                var log = new LogInfo { LogTypeKey = "HOST_ALERT" };
				Logger.LogDebug("StoreListOfTabsWithProviders(providers={Providers},portalId={PortalId},settings={Settings}): log → {Log}", providers, portalId, settings, log);
                log.AddProperty("Url Rewriting Caching Message", "Portal Module Providers Tab List stored in cache");
                log.AddProperty("Cache Item Key", key);
                log.AddProperty("PortalId", portalId.ToString(CultureInfo.InvariantCulture));
                log.AddProperty("Provider With Tabs", string.Join(",", providersWithTabsStr.ToArray()));
                log.AddProperty("Thread Id", Environment.CurrentManagedThreadId.ToString(CultureInfo.InvariantCulture));
                LogController.Instance.AddLog(log);
            }
			Logger.LogDebug("<<< Exiting StoreListOfTabsWithProviders(providers={Providers},portalId={PortalId},settings={Settings})", providers, portalId, settings);
        }

        internal static void StoreModuleProvidersForPortal(int portalId, FriendlyUrlSettings settings, List<ExtensionUrlProvider> providers)
        {
			Logger.LogDebug(">>> Entering StoreModuleProvidersForPortal(portalId={PortalId},settings={Settings},providers={Providers})", portalId, settings, providers);
            // get the key for the portal module providers
            string allTabsKey = string.Format(CultureInfo.InvariantCulture, PortalModuleProvidersAllTabsKey, portalId);

            // get the providers that are on all tabs
            var allTabsProviders = new List<ExtensionUrlProvider>();
			Logger.LogDebug("StoreModuleProvidersForPortal(portalId={PortalId},settings={Settings},providers={Providers}): allTabsProviders → {AllTabsProviders}", portalId, settings, providers, allTabsProviders);

            // holds all providers, indexed by tabId
            var tabsProviders = new Dictionary<int, List<ExtensionUrlProvider>>();
			Logger.LogDebug("StoreModuleProvidersForPortal(portalId={PortalId},settings={Settings},providers={Providers}): tabsProviders → {TabsProviders}", portalId, settings, providers, tabsProviders);
            var tabIdStr = new List<string>();
			Logger.LogDebug("StoreModuleProvidersForPortal(portalId={PortalId},settings={Settings},providers={Providers}): tabIdStr → {TabIdStr}", portalId, settings, providers, tabIdStr);
            int providerCount = 0;
            foreach (ExtensionUrlProvider provider in providers)
            {
                if (provider.ProviderConfig.AllTabs)
                {
                    allTabsProviders.Add(provider);
                }
                else
                {
                    foreach (int tabId in provider.ProviderConfig.TabIds)
                    {
                        List<ExtensionUrlProvider> thisTabProviders;
                        if (tabsProviders.TryGetValue(tabId, out var tabProviders))
                        {
                            thisTabProviders = tabProviders;
                            thisTabProviders.Add(provider);
							Logger.LogDebug("StoreModuleProvidersForPortal(portalId={PortalId},settings={Settings},providers={Providers}): thisTabProviders → {ThisTabProviders}", portalId, settings, providers, thisTabProviders);
                            tabsProviders[tabId] = thisTabProviders; // assign back to position in tabs
                        }
                        else
                        {
                            thisTabProviders = [provider,];
                            tabsProviders.Add(tabId, thisTabProviders);
                            tabIdStr.Add(tabId.ToString(CultureInfo.InvariantCulture));
                        }
                    }

                    providerCount++;
                }

                // store the list of providers where the provider might be called with no valid TabId, because
                // the provider allows for Urls with no DNN Page path, which means the TabId can't be identified
                // by the Url Rewriter.  This identifies the Provider as using a 'siteRootRewrite'
                if (!provider.AlwaysUsesDnnPagePath(portalId))
                {
                    List<ExtensionUrlProvider> noPathProviders;

                    // add this one
                    if (tabsProviders.TryGetValue(RewriteController.SiteRootRewrite, out var pathProviders))
                    {
                        noPathProviders = pathProviders;
                        noPathProviders.Add(provider);
						Logger.LogDebug("StoreModuleProvidersForPortal(portalId={PortalId},settings={Settings},providers={Providers}): noPathProviders → {NoPathProviders}", portalId, settings, providers, noPathProviders);
                        tabsProviders[RewriteController.SiteRootRewrite] = noPathProviders;

                        // assign back to position in tabs
                    }
                    else
                    {
                        noPathProviders = [provider,];
                        tabsProviders.Add(RewriteController.SiteRootRewrite, noPathProviders);
                        tabIdStr.Add("NoPath");
                    }
                }
            }

            // now add the two collections to the cache
            if (allTabsProviders.Count > 0)
            {
                SetPageCache(allTabsKey, allTabsProviders, settings);
            }

            if (tabsProviders.Count > 0)
            {
                foreach (int tabId in tabsProviders.Keys)
                {
                    SetPageCache(string.Format(CultureInfo.InvariantCulture, PortalModuleProvidersForTabKey, portalId, tabId), tabsProviders[tabId], settings);
                }
            }

            if (settings.LogCacheMessages)
            {
                var log = new LogInfo { LogTypeKey = "HOST_ALERT" };
				Logger.LogDebug("StoreModuleProvidersForPortal(portalId={PortalId},settings={Settings},providers={Providers}): log → {Log}", portalId, settings, providers, log);
                log.AddProperty("Url Rewriting Caching Message", "Extension Url Providers stored in cache");
                log.AddProperty("PortalId/TabIds", portalId.ToString(CultureInfo.InvariantCulture) + "/" + string.Join(",", tabIdStr.ToArray()));
                log.AddProperty("All Tabs Providers Count", allTabsProviders.Count.ToString(CultureInfo.InvariantCulture));
                log.AddProperty("Portal Tabs Providers Count", providerCount.ToString(CultureInfo.InvariantCulture));
                log.AddProperty("Thread Id", Environment.CurrentManagedThreadId.ToString(CultureInfo.InvariantCulture));
                LogController.Instance.AddLog(log);
            }
			Logger.LogDebug("<<< Exiting StoreModuleProvidersForPortal(portalId={PortalId},settings={Settings},providers={Providers})", portalId, settings, providers);
        }

        internal static void StorePortalAliasesInCache(SharedDictionary<string, PortalAliasInfo> aliases, FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering StorePortalAliasesInCache(aliases={Aliases},settings={Settings})", aliases, settings);
            SetPortalCache(PortalAliasListKey, aliases, settings);
			Logger.LogDebug("<<< Exiting StorePortalAliasesInCache(aliases={Aliases},settings={Settings})", aliases, settings);
        }

        internal static void StorePortalAliasesInCache(OrderedDictionary aliasList, FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering StorePortalAliasesInCache(aliasList={AliasList},settings={Settings})", aliasList, settings);
            SetPortalCache(PortalAliasesKey, aliasList, settings);
			Logger.LogDebug("<<< Exiting StorePortalAliasesInCache(aliasList={AliasList},settings={Settings})", aliasList, settings);
        }

        /// <summary>Retrieve the Url Dictionary for the installation.</summary>
        /// <param name="urlDict">A dictionary of tabs for all portals.</param>
        /// <param name="urlPortals">The portal IDs.</param>
        /// <param name="customAliasTabs">A dictionary of portals for which we've retrieved the tabs.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal void GetFriendlyUrlIndexFromCache(
            out SharedDictionary<int, SharedDictionary<string, string>> urlDict,
            out ConcurrentBag<int> urlPortals,
            out SharedDictionary<string, string> customAliasTabs)
        {
			Logger.LogDebug(">>> Entering GetFriendlyUrlIndexFromCache()");
            urlDict = null;
            urlPortals = null;
            customAliasTabs = null;
            object rawDict = DataCache.GetCache(UrlDictKey); // contains a dictionary of tabs for all portals
			Logger.LogDebug("GetFriendlyUrlIndexFromCache(): rawDict → {RawDict}", rawDict);
			Logger.LogDebug("GetFriendlyUrlIndexFromCache(): urlDict → {UrlDict}", urlDict);
			Logger.LogDebug("GetFriendlyUrlIndexFromCache(): urlPortals → {UrlPortals}", urlPortals);
			Logger.LogDebug("GetFriendlyUrlIndexFromCache(): customAliasTabs → {CustomAliasTabs}", customAliasTabs);
            object rawPortals = DataCache.GetCache(UrlPortalsKey);
			Logger.LogDebug("GetFriendlyUrlIndexFromCache(): rawPortals → {RawPortals}", rawPortals);

            // contains a list of portals for which we have retrieved the tabs
            object rawCustomAliasTabs = DataCache.GetCache(CustomAliasTabsKey);
			Logger.LogDebug("GetFriendlyUrlIndexFromCache(): rawCustomAliasTabs → {RawCustomAliasTabs}", rawCustomAliasTabs);

            // contains a dictionary of tabs with custom aliases, for all portals
            if (rawDict != null)
            {
                urlDict = (SharedDictionary<int, SharedDictionary<string, string>>)rawDict;
            }

            if (rawPortals != null)
            {
                urlPortals = (ConcurrentBag<int>)rawPortals;
            }

            if (rawCustomAliasTabs != null)
            {
                customAliasTabs = (SharedDictionary<string, string>)rawCustomAliasTabs;
            }
			Logger.LogDebug("<<< Exiting GetFriendlyUrlIndexFromCache()");
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal void GetPageIndexFromCache(
            out SharedDictionary<string, string> dict,
            out SharedDictionary<int, PathSizes> portalDepthInfo,
            FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering GetPageIndexFromCache(settings={Settings})", settings);
            object raw = DataCache.GetCache(PageIndexKey);
            if (raw != null)
            {
                dict = (SharedDictionary<string, string>)raw;
                raw = DataCache.GetCache(PageIndexDepthKey);
                portalDepthInfo = (SharedDictionary<int, PathSizes>)raw;
            }
            else
            {
                dict = null;
                portalDepthInfo = null;
            }
			Logger.LogDebug("<<< Exiting GetPageIndexFromCache(settings={Settings})", settings);
        }

        /// <summary>Store the Url Dictionary (all tab urls / tabids) for the installation.</summary>
        /// <param name="urlDict">A dictionary of tabs for all portals.</param>
        /// <param name="urlPortals">The portal IDs.</param>
        /// <param name="customAliasTabs">A dictionary of portals for which we've retrieved the tabs.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <param name="reason">The reason for rebuilding the index (for logging).</param>
        internal void StoreFriendlyUrlIndexInCache(
            SharedDictionary<int, SharedDictionary<string, string>> urlDict,
            ConcurrentBag<int> urlPortals,
            SharedDictionary<string, string> customAliasTabs,
            FriendlyUrlSettings settings,
            string reason)
        {
			Logger.LogDebug(">>> Entering StoreFriendlyUrlIndexInCache(urlDict={UrlDict},urlPortals={UrlPortals},customAliasTabs={CustomAliasTabs},settings={Settings},reason={Reason})", urlDict, urlPortals, customAliasTabs, settings, reason);
            if (settings.LogCacheMessages)
            {
                this.onRemovePageIndex = this.RemovedPageIndexCallBack;
				Logger.LogDebug("StoreFriendlyUrlIndexInCache(urlDict={UrlDict},urlPortals={UrlPortals},customAliasTabs={CustomAliasTabs},settings={Settings},reason={Reason}): onRemovePageIndex → {OnRemovePageIndex}", urlDict, urlPortals, customAliasTabs, settings, reason, onRemovePageIndex);
				Logger.LogDebug("StoreFriendlyUrlIndexInCache(urlDict={UrlDict},urlPortals={UrlPortals},customAliasTabs={CustomAliasTabs},settings={Settings},reason={Reason}): this.onRemovePageIndex → {Arg5}", urlDict, urlPortals, customAliasTabs, settings, reason, this.onRemovePageIndex);
            }
            else
            {
                this.onRemovePageIndex = null;
            }

            logRemovedReason = settings.LogCacheMessages;
			Logger.LogDebug("StoreFriendlyUrlIndexInCache(urlDict={UrlDict},urlPortals={UrlPortals},customAliasTabs={CustomAliasTabs},settings={Settings},reason={Reason}): logRemovedReason → {LogRemovedReason}", urlDict, urlPortals, customAliasTabs, settings, reason, logRemovedReason);

            SetPageCache(UrlDictKey, urlDict, new DNNCacheDependency(GetTabsCacheDependency(urlPortals)), settings, this.onRemovePageIndex);
            SetPageCache(UrlPortalsKey, urlPortals, settings);
            SetPageCache(CustomAliasTabsKey, customAliasTabs, settings);

            if (settings.LogCacheMessages)
            {
                var log = new LogInfo { LogTypeKey = "HOST_ALERT" };
				Logger.LogDebug("StoreFriendlyUrlIndexInCache(urlDict={UrlDict},urlPortals={UrlPortals},customAliasTabs={CustomAliasTabs},settings={Settings},reason={Reason}): log → {Log}", urlDict, urlPortals, customAliasTabs, settings, reason, log);
                log.AddProperty("Url Rewriting Caching Message", "Friendly Url Index built and Stored in Cache.");
                log.AddProperty("Build Reason", reason);
                log.AddProperty("Cache Key", UrlDictKey);
                using (urlDict.GetReadLock())
                {
                    log.AddProperty("Item Count", urlDict.Values.Count.ToString(CultureInfo.InvariantCulture));
                }

                log.AddProperty("Thread Id", Environment.CurrentManagedThreadId.ToString(CultureInfo.InvariantCulture));
                log.AddProperty("Item added to cache", "Url Portals object added to cache.  Key:" + UrlPortalsKey + "  Items: " + urlPortals.Count.ToString(CultureInfo.InvariantCulture));
                using (customAliasTabs.GetReadLock())
                {
                    log.AddProperty("Item added to cache", "Custom Alias Tabs added to cache.  Key:" + CustomAliasTabsKey + " Items: " + customAliasTabs.Count.ToString(CultureInfo.InvariantCulture));
                }

                LogController.Instance.AddLog(log);
            }
			Logger.LogDebug("<<< Exiting StoreFriendlyUrlIndexInCache(urlDict={UrlDict},urlPortals={UrlPortals},customAliasTabs={CustomAliasTabs},settings={Settings},reason={Reason})", urlDict, urlPortals, customAliasTabs, settings, reason);
        }

        internal void StorePageIndexInCache(
            SharedDictionary<string, string> tabDictionary,
            SharedDictionary<int, PathSizes> portalDepthInfo,
            FriendlyUrlSettings settings,
            string reason)
        {
			Logger.LogDebug(">>> Entering StorePageIndexInCache(tabDictionary={TabDictionary},portalDepthInfo={PortalDepthInfo},settings={Settings},reason={Reason})", tabDictionary, portalDepthInfo, settings, reason);
            this.onRemovePageIndex = settings.LogCacheMessages ? (CacheItemRemovedCallback)this.RemovedPageIndexCallBack : null;
			Logger.LogDebug("StorePageIndexInCache(tabDictionary={TabDictionary},portalDepthInfo={PortalDepthInfo},settings={Settings},reason={Reason}): onRemovePageIndex → {OnRemovePageIndex}", tabDictionary, portalDepthInfo, settings, reason, onRemovePageIndex);

            // get list of portal ids for the portals we are storing in the page index
            var portalIds = new List<int>();
			Logger.LogDebug("StorePageIndexInCache(tabDictionary={TabDictionary},portalDepthInfo={PortalDepthInfo},settings={Settings},reason={Reason}): portalIds → {PortalIds}", tabDictionary, portalDepthInfo, settings, reason, portalIds);
            using (portalDepthInfo.GetReadLock())
            {
                portalIds.AddRange(portalDepthInfo.Keys);
            }

            // 783 : use cache dependency to manage page index instead of triggerDictionaryRebuild regex.
            SetPageCache(PageIndexKey, tabDictionary, new DNNCacheDependency(GetTabsCacheDependency(portalIds)), settings, this.onRemovePageIndex);

            SetPageCache(PageIndexDepthKey, portalDepthInfo, settings);

            logRemovedReason = settings.LogCacheMessages;
			Logger.LogDebug("StorePageIndexInCache(tabDictionary={TabDictionary},portalDepthInfo={PortalDepthInfo},settings={Settings},reason={Reason}): logRemovedReason → {LogRemovedReason}", tabDictionary, portalDepthInfo, settings, reason, logRemovedReason);

            if (settings.LogCacheMessages)
            {
                var log = new LogInfo { LogTypeKey = "HOST_ALERT" };
				Logger.LogDebug("StorePageIndexInCache(tabDictionary={TabDictionary},portalDepthInfo={PortalDepthInfo},settings={Settings},reason={Reason}): log → {Log}", tabDictionary, portalDepthInfo, settings, reason, log);

                log.AddProperty("Url Rewriting Caching Message", "Page Index built and Stored in Cache");
                log.AddProperty("Reason", reason);
                log.AddProperty("Cache Item Key", PageIndexKey);
                using (tabDictionary.GetReadLock())
                {
                    log.AddProperty("Item Count", tabDictionary.Count.ToString(CultureInfo.InvariantCulture));
                }

                log.AddProperty("Thread Id", Environment.CurrentManagedThreadId.ToString(CultureInfo.InvariantCulture));
                LogController.Instance.AddLog(log);
            }
			Logger.LogDebug("<<< Exiting StorePageIndexInCache(tabDictionary={TabDictionary},portalDepthInfo={PortalDepthInfo},settings={Settings},reason={Reason})", tabDictionary, portalDepthInfo, settings, reason);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal void StoreTabPathsInCache(int portalId, SharedDictionary<string, string> tabPathDictionary, FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering StoreTabPathsInCache(portalId={PortalId},tabPathDictionary={TabPathDictionary},settings={Settings})", portalId, tabPathDictionary, settings);
            SetPageCache(
                string.Format(CultureInfo.InvariantCulture, TabPathsKey, portalId),
                tabPathDictionary,
                new DNNCacheDependency(GetTabsCacheDependency([portalId,])),
                settings,
                null);
				Logger.LogDebug("<<< Exiting StoreTabPathsInCache(portalId={PortalId},tabPathDictionary={TabPathDictionary},settings={Settings})", portalId, tabPathDictionary, settings);
        }

        private static CacheDependency GetPortalsCacheDependency()
        {
			Logger.LogDebug(">>> Entering GetPortalsCacheDependency()");
			Logger.LogDebug("<<< Exiting GetPortalsCacheDependency()");
            return new CacheDependency(null, PortalCacheDependencyKeys);
        }

        private static void SetPageCache(string key, object value, FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering SetPageCache(key={Key},value={Value},settings={Settings})", key, value, settings);
            SetPageCache(key, value, null, settings, null);
			Logger.LogDebug("<<< Exiting SetPageCache(key={Key},value={Value},settings={Settings})", key, value, settings);
        }

        private static void SetPageCache(string key, object value, DNNCacheDependency dependency, FriendlyUrlSettings settings, CacheItemRemovedCallback callback)
        {
			Logger.LogDebug(">>> Entering SetPageCache(key={Key},value={Value},dependency={Dependency},settings={Settings},callback={Callback})", key, value, dependency, settings, callback);
            DateTime absoluteExpiration = DateTime.Now.Add(settings.CacheTime);
            DataCache.SetCache(
                key,
                value,
                dependency,
                absoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.AboveNormal,
                callback);
				Logger.LogDebug("SetPageCache(key={Key},value={Value},dependency={Dependency},settings={Settings},callback={Callback}): absoluteExpiration → {AbsoluteExpiration}", key, value, dependency, settings, callback, absoluteExpiration);
				Logger.LogDebug("<<< Exiting SetPageCache(key={Key},value={Value},settings={Settings})", key, value, settings);
        }

        private static void SetPortalCache(string key, object value, FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering SetPortalCache(key={Key},value={Value},settings={Settings})", key, value, settings);
            var absoluteExpiration = DateTime.Now.Add(new TimeSpan(24, 0, 0));
            if (settings != null)
            {
                absoluteExpiration = DateTime.Now.Add(settings.CacheTime);
            }

            // 857 : use cache dependency for portal alias cache
            if (settings != null)
            {
                DataCache.SetCache(
                    key,
                    value,
                    new DNNCacheDependency(GetPortalsCacheDependency()),
                    absoluteExpiration,
                    Cache.NoSlidingExpiration);
            }
            else
            {
                DataCache.SetCache(key, value, absoluteExpiration);
            }
			Logger.LogDebug("<<< Exiting SetPortalCache(key={Key},value={Value},settings={Settings})", key, value, settings);
        }

        private static CacheDependency GetTabsCacheDependency(IEnumerable<int> portalIds)
        {
			Logger.LogDebug(">>> Entering GetTabsCacheDependency(portalIds={PortalIds})", portalIds);
            var keys = new List<string>();
            foreach (int portalId in portalIds)
            {
			Logger.LogDebug("GetTabsCacheDependency(portalIds={PortalIds}): keys → {Keys}", portalIds, keys);
                const string cacheKey = DataCache.TabCacheKey;
                string key = string.Format(CultureInfo.InvariantCulture, cacheKey, portalId);
				Logger.LogWarning("GetTabsCacheDependency(portalIds={PortalIds}): key → {Key}", portalIds, key);
                key = "DNN_" + key; // add on the DNN_ prefix
                keys.Add(key);
            }

            // get the portals list dependency
            var portalKeys = new List<string>();
			Logger.LogDebug("GetTabsCacheDependency(portalIds={PortalIds}): portalKeys → {PortalKeys}", portalIds, portalKeys);
            if (portalKeys.Count > 0)
            {
                keys.AddRange(portalKeys);
            }

            var tabsDependency = new CacheDependency(null, keys.ToArray());
			Logger.LogDebug("GetTabsCacheDependency(portalIds={PortalIds}): tabsDependency → {TabsDependency}", portalIds, tabsDependency);
			Logger.LogDebug("<<< Exiting GetTabsCacheDependency(portalIds={PortalIds})", portalIds);
            return tabsDependency;
        }
    }
}
