// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    public class ExtensionUrlProviderController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExtensionUrlProviderController));
        private static readonly object ProvidersBuildLock = new object();

        private static readonly Regex RewrittenUrlRegex = new Regex(
            @"(?<tabid>(?:\?|&)tabid=\d+)(?<qs>&[^=]+=[^&]*)*",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static void DeleteProvider(ExtensionUrlProviderInfo urlProvider)
        {
			Logger.LogError(">>> Entering DeleteProvider(urlProvider={UrlProvider})", urlProvider);
            DataProvider.Instance().DeleteExtensionUrlProvider(urlProvider.ExtensionUrlProviderId);
            ClearCache();
			Logger.LogWarning("<<< Exiting DeleteProvider(urlProvider={UrlProvider})", urlProvider);
        }

        public static void DisableProvider(int providerId, int portalId)
        {
			Logger.LogError(">>> Entering DisableProvider(providerId=<redacted>,portalId={PortalId})", portalId);
            DataProvider.Instance().UpdateExtensionUrlProvider(providerId, false);
            ClearCache(portalId);
			Logger.LogWarning("<<< Exiting DisableProvider(providerId=<redacted>,portalId={PortalId})", portalId);
        }

        public static void EnableProvider(int providerId, int portalId)
        {
			Logger.LogError(">>> Entering EnableProvider(providerId=<redacted>,portalId={PortalId})", portalId);
            DataProvider.Instance().UpdateExtensionUrlProvider(providerId, true);
            ClearCache(portalId);
			Logger.LogWarning("<<< Exiting EnableProvider(providerId=<redacted>,portalId={PortalId})", portalId);
        }

        public static List<ExtensionUrlProviderInfo> GetProviders(int portalId)
        {
			Logger.LogError(">>> Entering GetProviders(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetProviders(portalId={PortalId})", portalId);
            return CBO.FillCollection<ExtensionUrlProviderInfo>(DataProvider.Instance().GetExtensionUrlProviders(portalId));
        }

        /// <summary>Loads the module providers.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="ExtensionUrlProvider"/> instances.</returns>
        /// <remarks>Note : similar copy for UI purposes in ConfigurationController.cs.</remarks>
        public static List<ExtensionUrlProvider> GetModuleProviders(int portalId)
            => GetModuleProviders(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), Globals.GetCurrentServiceProvider(), portalId);

        /// <summary>Loads the module providers.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="ExtensionUrlProvider"/> instances.</returns>
        /// <remarks>Note : similar copy for UI purposes in ConfigurationController.cs.</remarks>
        public static List<ExtensionUrlProvider> GetModuleProviders(IHostSettings hostSettings, IServiceProvider serviceProvider, int portalId)
        {
			Logger.LogError(">>> Entering GetModuleProviders(portalId={PortalId})", portalId);
            var cacheKey = FormattableString.Invariant($"ExtensionUrlProviders_{portalId}");
            var moduleProviders = CBO.GetCachedObject<List<ExtensionUrlProvider>>(
                hostSettings,
                new CacheItemArgs(cacheKey, 60, CacheItemPriority.High, portalId),
                c =>
                {
					Logger.LogError("GetModuleProviders(portalId={PortalId}): cacheKey → {CacheKey}", portalId, cacheKey);
					Logger.LogError("GetModuleProviders(hostSettings={HostSettings},serviceProvider={ServiceProvider},portalId={PortalId}): cacheKey → {CacheKey}", hostSettings, serviceProvider, portalId, cacheKey);
                    var id = (int)c.Params[0];
                    IDataReader dr = DataProvider.Instance().GetExtensionUrlProviders(id);
					Logger.LogError("GetModuleProviders(portalId={PortalId}): id → {Id}", portalId, id);
                    try
                    {
						Logger.LogError("GetModuleProviders(hostSettings={HostSettings},serviceProvider={ServiceProvider},portalId={PortalId}): id → {Id}", hostSettings, serviceProvider, portalId, id);
						Logger.LogError("GetModuleProviders(portalId={PortalId}): dr → {Dr}", portalId, dr);
						Logger.LogError("GetModuleProviders(hostSettings={HostSettings},serviceProvider={ServiceProvider},portalId={PortalId}): dr → {Dr}", hostSettings, serviceProvider, portalId, dr);
                        var providers = new List<ExtensionUrlProvider>();
                        var providerConfigs = CBO.FillCollection(dr, new List<ExtensionUrlProviderInfo>(), false);

						Logger.LogError("GetModuleProviders(portalId={PortalId}): providers → {Providers}", portalId, providers);
                        foreach (var providerConfig in providerConfigs)
                        {
							Logger.LogError("GetModuleProviders(hostSettings={HostSettings},serviceProvider={ServiceProvider},portalId={PortalId}): providers → {Providers}", hostSettings, serviceProvider, portalId, providers);
							Logger.LogError("GetModuleProviders(portalId={PortalId}): providerConfigs → {ProviderConfigs}", portalId, providerConfigs);
                            var providerType = Reflection.CreateType(providerConfig.ProviderType);
                            if (providerType == null)
                            {
								Logger.LogError("GetModuleProviders(hostSettings={HostSettings},serviceProvider={ServiceProvider},portalId={PortalId}): providerConfigs → {ProviderConfigs}", hostSettings, serviceProvider, portalId, providerConfigs);
								Logger.LogError("GetModuleProviders(portalId={PortalId}): providerType → {ProviderType}", portalId, providerType);
								Logger.LogError("GetModuleProviders(hostSettings={HostSettings},serviceProvider={ServiceProvider},portalId={PortalId}): providerType → {ProviderType}", hostSettings, serviceProvider, portalId, providerType);
                                continue;
                            }

                            if (Reflection.CreateObject(serviceProvider, providerType) is not ExtensionUrlProvider provider)
                            {
                                continue;
                            }

                            provider.ProviderConfig = providerConfig;
                            provider.ProviderConfig.PortalId = id;
                            providers.Add(provider);
                        }

                        if (dr.NextResult())
                        {
                            // Setup Settings
                            while (dr.Read())
                            {
                                var extensionUrlProviderId = Null.SetNullInteger(dr["ExtensionUrlProviderID"]);
								Logger.LogError("GetModuleProviders(portalId={PortalId}): extensionUrlProviderId → {ExtensionUrlProviderId}", portalId, extensionUrlProviderId);
								Logger.LogError("GetModuleProviders(hostSettings={HostSettings},serviceProvider={ServiceProvider},portalId={PortalId}): extensionUrlProviderId → {ExtensionUrlProviderId}", hostSettings, serviceProvider, portalId, extensionUrlProviderId);
                                var key = Null.SetNullString(dr["SettingName"]);
                                var value = Null.SetNullString(dr["SettingValue"]);
								Logger.LogError("GetModuleProviders(portalId={PortalId}): key → {Key}", portalId, key);
								Logger.LogError("GetModuleProviders(hostSettings={HostSettings},serviceProvider={ServiceProvider},portalId={PortalId}): key → {Key}", hostSettings, serviceProvider, portalId, key);

                                var provider = providers.SingleOrDefault(p =>
                                    p.ProviderConfig.ExtensionUrlProviderId == extensionUrlProviderId);
									Logger.LogError("GetModuleProviders(portalId={PortalId}): value → {Value}", portalId, value);
									Logger.LogError("GetModuleProviders(hostSettings={HostSettings},serviceProvider={ServiceProvider},portalId={PortalId}): value → {Value}", hostSettings, serviceProvider, portalId, value);
                                provider?.ProviderConfig.Settings[key] = value;
                            }
                        }

                        if (dr.NextResult())
                        {
                            // Setup Tabs
                            while (dr.Read())
                            {
                                var extensionUrlProviderId = Null.SetNullInteger(dr["ExtensionUrlProviderID"]);
                                var tabId = Null.SetNullInteger(dr["TabID"]);

								Logger.LogError("GetModuleProviders(portalId={PortalId}): tabId → {TabId}", portalId, tabId);
                                var provider = providers.SingleOrDefault(p =>
                                    p.ProviderConfig.ExtensionUrlProviderId == extensionUrlProviderId);
									Logger.LogError("GetModuleProviders(hostSettings={HostSettings},serviceProvider={ServiceProvider},portalId={PortalId}): tabId → {TabId}", hostSettings, serviceProvider, portalId, tabId);
                                if (provider != null && !provider.ProviderConfig.TabIds.Contains(tabId))
                                {
                                    provider.ProviderConfig.TabIds.Add(tabId);
                                }
                            }
                        }

                        return providers;
                    }
                    finally
                    {
                        // Close reader
                        CBO.CloseDataReader(dr, true);
                    }
                });

			Logger.LogWarning("<<< Exiting GetModuleProviders(hostSettings={HostSettings},serviceProvider={ServiceProvider},portalId={PortalId})", hostSettings, serviceProvider, portalId);
            return moduleProviders;
        }

        public static FriendlyUrlOptions GetOptionsFromSettings(FriendlyUrlSettings settings)
        {
			Logger.LogError(">>> Entering GetOptionsFromSettings(settings={Settings})", settings);
			Logger.LogWarning("<<< Exiting GetOptionsFromSettings(settings={Settings})", settings);
            return new FriendlyUrlOptions
            {
                PunctuationReplacement = (settings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing)
                                                ? settings.ReplaceSpaceWith
                                                : string.Empty,
                SpaceEncoding = settings.SpaceEncodingValue,
                MaxUrlPathLength = 200,
                ConvertDiacriticChars = settings.AutoAsciiConvert,
                RegexMatch = settings.RegexMatch,
                IllegalChars = settings.IllegalChars,
                ReplaceChars = settings.ReplaceChars,
                ReplaceDoubleChars = settings.ReplaceDoubleChars,
                ReplaceCharWithChar = settings.ReplaceCharacterDictionary,
                PageExtension = settings.PageExtensionUsageType == PageExtensionUsageType.Never ? string.Empty : settings.PageExtension,
            };
        }

        /// <summary>logs an exception related to a module provider once per cache-lifetime.</summary>
        /// <param name="ex">The exception.</param>
        /// <param name="status">The HTTP status.</param>
        /// <param name="provider">The extension URL provider.</param>
        /// <param name="result">The URL action.</param>
        /// <param name="messages">The list of messages to log.</param>
        public static void LogModuleProviderExceptionInRequest(Exception ex, string status, ExtensionUrlProvider provider, UrlAction result, List<string> messages)
        {
			Logger.LogError(">>> Entering LogModuleProviderExceptionInRequest(ex={Ex},status={Status},provider={Provider},result={Result},messages={Messages})", ex, status, provider, result, messages);
            if (ex != null)
            {
                string moduleProviderName = "Unknown Provider";
				Logger.LogError("LogModuleProviderExceptionInRequest(ex={Ex},status={Status},provider={Provider},result={Result},messages={Messages}): moduleProviderName → {ModuleProviderName}", ex, status, provider, result, messages, moduleProviderName);
                string moduleProviderVersion = "Unknown Version";
				Logger.LogError("LogModuleProviderExceptionInRequest(ex={Ex},status={Status},provider={Provider},result={Result},messages={Messages}): moduleProviderVersion → {ModuleProviderVersion}", ex, status, provider, result, messages, moduleProviderVersion);
                if (provider != null)
                {
                    moduleProviderName = provider.ProviderConfig.ProviderName;
                    moduleProviderVersion = provider.GetType().Assembly.GetName(false).Version.ToString();
                }

                // this logic prevents a site logging an exception for every request made.  Instead
                // the exception will be logged once for the life of the cache / application restart or 1 hour, whichever is shorter.
                // create a cache key for this exception type
                string cacheKey = ex.GetType().ToString();
				Logger.LogError("LogModuleProviderExceptionInRequest(ex={Ex},status={Status},provider={Provider},result={Result},messages={Messages}): cacheKey → {CacheKey}", ex, status, provider, result, messages, cacheKey);

                // see if there is an existing object logged for this exception type
                object existingEx = DataCache.GetCache(cacheKey);
				Logger.LogError("LogModuleProviderExceptionInRequest(ex={Ex},status={Status},provider={Provider},result={Result},messages={Messages}): existingEx → {ExistingEx}", ex, status, provider, result, messages, existingEx);
                if (existingEx == null)
                {
                    // if there was no existing object logged for this exception type, this is a new exception
                    DateTime expire = DateTime.Now.AddHours(1);
					Logger.LogError("LogModuleProviderExceptionInRequest(ex={Ex},status={Status},provider={Provider},result={Result},messages={Messages}): expire → {Expire}", ex, status, provider, result, messages, expire);
                    DataCache.SetCache(cacheKey, cacheKey, expire);

                    // just store the cache key - it doesn't really matter
                    // create a log event
                    string productVer = DotNetNukeContext.Current.Application.Version.ToString();
                    var log = new LogInfo { LogTypeKey = "GENERAL_EXCEPTION" };
					Logger.LogError("LogModuleProviderExceptionInRequest(ex={Ex},status={Status},provider={Provider},result={Result},messages={Messages}): log → {Log}", ex, status, provider, result, messages, log);
                    log.AddProperty(
                        "Url Rewriting Extension Url Provider Exception",
                        "Exception in Url Rewriting Process");
                    log.AddProperty("Provider Name", moduleProviderName);
                    log.AddProperty("Provider Version", moduleProviderVersion);
                    log.AddProperty("Http Status", status);
                    log.AddProperty("Product Version", productVer);
                    if (result != null)
                    {
                        log.AddProperty("Original Path", result.OriginalPath ?? "null");
                        log.AddProperty("Raw Url", result.RawUrl ?? "null");
                        log.AddProperty("Final Url", result.FinalUrl ?? "null");

                        log.AddProperty("Rewrite Result", !string.IsNullOrEmpty(result.RewritePath) ? result.RewritePath : "[no rewrite]");
                        log.AddProperty("Redirect Location", string.IsNullOrEmpty(result.FinalUrl) ? "[no redirect]" : result.FinalUrl);
                        log.AddProperty("Action", result.Action.ToString());
                        log.AddProperty("Reason", result.Reason.ToString());
                        log.AddProperty("Portal Id", result.PortalId.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("Tab Id", result.TabId.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("Http Alias", result.PortalAlias != null ? result.PortalAlias.HTTPAlias : "Null");

                        if (result.DebugMessages != null)
                        {
                            int i = 1;
                            foreach (string debugMessage in result.DebugMessages)
                            {
                                string msg = debugMessage;
								Logger.LogError("LogModuleProviderExceptionInRequest(ex={Ex},status={Status},provider={Provider},result={Result},messages={Messages}): msg → {Msg}", ex, status, provider, result, messages, msg);
                                if (debugMessage == null)
                                {
                                    msg = "[message was null]";
                                }

                                log.AddProperty("Debug Message[result] " + i.ToString(CultureInfo.InvariantCulture), msg);
                                i++;
                            }
                        }
                    }
                    else
                    {
                        log.AddProperty("Result", "Result value null");
                    }

                    if (messages != null)
                    {
                        int i = 1;
                        foreach (string msg in messages)
                        {
                            log.AddProperty("Debug Message[raw] " + i.ToString(CultureInfo.InvariantCulture), msg);
                            i++;
                        }
                    }

                    log.AddProperty("Exception Type", ex.GetType().ToString());
                    log.AddProperty("Message", ex.Message);
                    log.AddProperty("Stack Trace", ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        log.AddProperty("Inner Exception Message", ex.InnerException.Message);
                        log.AddProperty("Inner Exception Stacktrace", ex.InnerException.StackTrace);
                    }

                    log.BypassBuffering = true;
					Logger.LogDebug("LogModuleProviderExceptionInRequest(ex={Ex},status={Status},provider={Provider},result={Result},messages={Messages}): log.BypassBuffering → {Arg5}", ex, status, provider, result, messages, log.BypassBuffering);
                    LogController.Instance.AddLog(log);
                }
            }
			Logger.LogWarning("<<< Exiting LogModuleProviderExceptionInRequest(ex={Ex},status={Status},provider={Provider},result={Result},messages={Messages})", ex, status, provider, result, messages);
        }

        public static void SaveProvider(ExtensionUrlProviderInfo provider)
        {
			Logger.LogError(">>> Entering SaveProvider(provider={Provider})", provider);
            provider.ExtensionUrlProviderId = DataProvider.Instance().AddExtensionUrlProvider(
                                                    provider.ExtensionUrlProviderId,
                                                    provider.DesktopModuleId,
                                                    provider.ProviderName,
                                                    provider.ProviderType,
                                                    provider.SettingsControlSrc,
                                                    provider.IsActive,
                                                    provider.RewriteAllUrls,
                                                    provider.RedirectAllUrls,
                                                    provider.ReplaceAllUrls);
													Logger.LogWarning("<<< Exiting SaveProvider(provider={Provider})", provider);
        }

        public static void SaveSetting(int providerId, int portalId, string settingName, string settingValue)
        {
			Logger.LogError(">>> Entering SaveSetting(providerId=<redacted>,portalId={PortalId},settingName={SettingName},settingValue={SettingValue})", portalId, settingName, settingValue);
            DataProvider.Instance().SaveExtensionUrlProviderSetting(providerId, portalId, settingName, settingValue);
            ClearCache(portalId);
			Logger.LogWarning("<<< Exiting SaveSetting(providerId=<redacted>,portalId={PortalId},settingName={SettingName},settingValue={SettingValue})", portalId, settingName, settingValue);
        }

        /// <summary>Checks to see if any providers are marked as 'always call for rewrites'.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <param name="parentTraceId">The parent trace ID.</param>
        /// <returns><see langword="true"/> if the there are providers that need to always be called, otherwise <see langword="false"/>.</returns>
        internal static bool CheckForAlwaysCallProviders(int portalId, int tabId, FriendlyUrlSettings settings, Guid parentTraceId)
        {
			Logger.LogError(">>> Entering CheckForAlwaysCallProviders(portalId={PortalId},tabId={TabId},settings={Settings},parentTraceId={ParentTraceId})", portalId, tabId, settings, parentTraceId);
            List<int> alwaysCallTabids = CacheController.GetAlwaysCallProviderTabs(portalId);
            bool checkForAlwaysCallResult = false;
            if (alwaysCallTabids == null)
            {
				Logger.LogError("CheckForAlwaysCallProviders(portalId={PortalId},tabId={TabId},settings={Settings},parentTraceId={ParentTraceId}): checkForAlwaysCallResult → {CheckForAlwaysCallResult}", portalId, tabId, settings, parentTraceId, checkForAlwaysCallResult);
                alwaysCallTabids = new List<int>(); // create new list
				Logger.LogError("CheckForAlwaysCallProviders(portalId={PortalId},tabId={TabId},settings={Settings},parentTraceId={ParentTraceId}): alwaysCallTabids → {AlwaysCallTabids}", portalId, tabId, settings, parentTraceId, alwaysCallTabids);

                // nothing in cache, build list
                List<ExtensionUrlProvider> providers = GetModuleProviders(portalId).Where(p => p.ProviderConfig.IsActive).ToList();
				Logger.LogError("CheckForAlwaysCallProviders(portalId={PortalId},tabId={TabId},settings={Settings},parentTraceId={ParentTraceId}): providers → {Providers}", portalId, tabId, settings, parentTraceId, providers);
                foreach (ExtensionUrlProvider provider in providers)
                {
                    // check the always call property
                    if (provider.AlwaysCallForRewrite(portalId))
                    {
                        if (provider.ProviderConfig.AllTabs)
                        {
                            if (alwaysCallTabids.Contains(RewriteController.AllTabsRewrite) == false)
                            {
                                alwaysCallTabids.Add(RewriteController.AllTabsRewrite);
                            }
                        }
                        else
                        {
                            foreach (int providerTabId in provider.ProviderConfig.TabIds)
                            {
                                if (alwaysCallTabids.Contains(tabId) == false)
                                {
                                    alwaysCallTabids.Add(providerTabId);
                                }
                            }
                        }
                    }
                }

                // now store back in cache
                CacheController.StoreAlwaysCallProviderTabs(portalId, alwaysCallTabids, settings);
            }

            if (alwaysCallTabids.Contains(tabId) || alwaysCallTabids.Contains(RewriteController.AllTabsRewrite))
            {
                checkForAlwaysCallResult = true;
            }

			Logger.LogWarning("<<< Exiting CheckForAlwaysCallProviders(portalId={PortalId},tabId={TabId},settings={Settings},parentTraceId={ParentTraceId})", portalId, tabId, settings, parentTraceId);
            return checkForAlwaysCallResult;
        }

        internal static bool CheckForRedirect(
            Uri requestUri,
            UrlAction result,
            NameValueCollection queryStringCol,
            FriendlyUrlSettings settings,
            out string location,
            ref List<string> messages,
            Guid parentTraceId)
        {
			Logger.LogError(">>> Entering CheckForRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},messages={Messages},parentTraceId={ParentTraceId})", requestUri, result, queryStringCol, settings, messages, parentTraceId);
            bool redirected = false;
            location = string.Empty;
			Logger.LogError("CheckForRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): location → {Location}", requestUri, result, queryStringCol, settings, messages, parentTraceId, location);
			Logger.LogDebug("CheckForRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): location → {Location}", requestUri, result, queryStringCol, settings, messages, parentTraceId, location);
            ExtensionUrlProvider activeProvider = null;
            try
            {
				Logger.LogError("CheckForRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): activeProvider → {ActiveProvider}", requestUri, result, queryStringCol, settings, messages, parentTraceId, activeProvider);
				Logger.LogError("CheckForRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): redirected → {Redirected}", requestUri, result, queryStringCol, settings, messages, parentTraceId, redirected);
                List<ExtensionUrlProvider> providersToCall = GetProvidersToCall(result.TabId, result.PortalId, settings, parentTraceId);
                if (providersToCall != null && providersToCall.Count > 0)
                {
					Logger.LogError("CheckForRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): providersToCall → {ProvidersToCall}", requestUri, result, queryStringCol, settings, messages, parentTraceId, providersToCall);
                    FriendlyUrlOptions options = UrlRewriterUtils.GetOptionsFromSettings(settings);
					Logger.LogError("CheckForRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): options → {Options}", requestUri, result, queryStringCol, settings, messages, parentTraceId, options);
                    foreach (ExtensionUrlProvider provider in providersToCall)
                    {
                        activeProvider = provider; // for error handling
                        redirected = provider.CheckForRedirect(
                            result.TabId,
                            result.PortalId,
                            result.HttpAlias,
                            requestUri,
                            queryStringCol,
                            options,
                            out location,
                            ref messages);
                        if (redirected)
                        {
                            result.FinalUrl = location;
							Logger.LogDebug("CheckForRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): result.FinalUrl → {Arg6}", requestUri, result, queryStringCol, settings, messages, parentTraceId, result.FinalUrl);
                            result.Reason = RedirectReason.Module_Provider_Redirect;
							Logger.LogDebug("CheckForRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): result.Reason → {Arg6}", requestUri, result, queryStringCol, settings, messages, parentTraceId, result.Reason);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in CheckForRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): {Arg6}", requestUri, result, queryStringCol, settings, messages, parentTraceId, ex.Message);
                // log module provider exception
                LogModuleProviderExceptionInRequest(ex, "500 Internal Server Error", activeProvider, result, messages);

                // return defaults
                redirected = false;
                location = string.Empty;
                string providerName = "Unknown";
                if (activeProvider != null)
                {
                    providerName = activeProvider.ProviderConfig.ProviderName;
					Logger.LogError("CheckForRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): providerName → {ProviderName}", requestUri, result, queryStringCol, settings, messages, parentTraceId, providerName);
                }

                if (result != null)
                {
                    result.DebugMessages.Add("Exception in provider [" + providerName + "] :" + ex.Message);
                }
            }

			Logger.LogWarning("<<< Exiting CheckForRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},messages={Messages},parentTraceId={ParentTraceId})", requestUri, result, queryStringCol, settings, messages, parentTraceId);
            return redirected;
        }

        /// <summary>Returns boolean value is any loaded providers require checking of rewrite / redirect values from the site root (ie, not dnn tab path).</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <param name="parentTraceId">The parent trace ID.</param>
        /// <returns><see langword="true"/> if there is a provider that requires checks for a site root rewrite/redirect, otherwise <see langword="false"/>.</returns>
        internal static bool CheckForSiteRootRewrite(int portalId, FriendlyUrlSettings settings, Guid parentTraceId)
        {
			Logger.LogError(">>> Entering CheckForSiteRootRewrite(portalId={PortalId},settings={Settings},parentTraceId={ParentTraceId})", portalId, settings, parentTraceId);
            var providers = GetProvidersToCall(RewriteController.SiteRootRewrite, portalId, settings, parentTraceId);

			Logger.LogError("CheckForSiteRootRewrite(portalId={PortalId},settings={Settings},parentTraceId={ParentTraceId}): providers → {Providers}", portalId, settings, parentTraceId, providers);
            // list should have returned all providers with site root rewrite, but double check here in case of faulty third-party logic
			Logger.LogWarning("<<< Exiting CheckForSiteRootRewrite(portalId={PortalId},settings={Settings},parentTraceId={ParentTraceId})", portalId, settings, parentTraceId);
            return providers.Any(provider => provider.AlwaysUsesDnnPagePath(portalId) == false);
        }

        internal static bool GetUrlFromExtensionUrlProviders(
            int portalId,
            TabInfo tab,
            FriendlyUrlSettings settings,
            string friendlyUrlPath,
            string cultureCode,
            ref string endingPageName,
            out string changedPath,
            out bool changeToSiteRoot,
            ref List<string> messages,
            Guid parentTraceId)
        {
			Logger.LogError(">>> Entering GetUrlFromExtensionUrlProviders(portalId={PortalId},tab={Tab},settings={Settings},friendlyUrlPath={FriendlyUrlPath},cultureCode={CultureCode},endingPageName={EndingPageName},messages={Messages},parentTraceId={ParentTraceId})", portalId, tab, settings, friendlyUrlPath, cultureCode, endingPageName, messages, parentTraceId);
            bool wasChanged = false;
            changedPath = friendlyUrlPath;
			Logger.LogError("GetUrlFromExtensionUrlProviders(portalId={PortalId},tab={Tab},settings={Settings},friendlyUrlPath={FriendlyUrlPath},cultureCode={CultureCode},endingPageName={EndingPageName},messages={Messages},parentTraceId={ParentTraceId}): changedPath → {ChangedPath}", portalId, tab, settings, friendlyUrlPath, cultureCode, endingPageName, messages, parentTraceId, changedPath);
			Logger.LogDebug("GetUrlFromExtensionUrlProviders(portalId={PortalId},tab={Tab},settings={Settings},friendlyUrlPath={FriendlyUrlPath},cultureCode={CultureCode},endingPageName={EndingPageName},messages={Messages},parentTraceId={ParentTraceId}): changedPath → {ChangedPath}", portalId, tab, settings, friendlyUrlPath, cultureCode, endingPageName, messages, parentTraceId, changedPath);
            changeToSiteRoot = false;
			Logger.LogError("GetUrlFromExtensionUrlProviders(portalId={PortalId},tab={Tab},settings={Settings},friendlyUrlPath={FriendlyUrlPath},cultureCode={CultureCode},endingPageName={EndingPageName},messages={Messages},parentTraceId={ParentTraceId}): changeToSiteRoot → {ChangeToSiteRoot}", portalId, tab, settings, friendlyUrlPath, cultureCode, endingPageName, messages, parentTraceId, changeToSiteRoot);
            ExtensionUrlProvider activeProvider = null;
            if (messages == null)
            {
				Logger.LogError("GetUrlFromExtensionUrlProviders(portalId={PortalId},tab={Tab},settings={Settings},friendlyUrlPath={FriendlyUrlPath},cultureCode={CultureCode},endingPageName={EndingPageName},messages={Messages},parentTraceId={ParentTraceId}): activeProvider → {ActiveProvider}", portalId, tab, settings, friendlyUrlPath, cultureCode, endingPageName, messages, parentTraceId, activeProvider);
				Logger.LogDebug("GetUrlFromExtensionUrlProviders(portalId={PortalId},tab={Tab},settings={Settings},friendlyUrlPath={FriendlyUrlPath},cultureCode={CultureCode},endingPageName={EndingPageName},messages={Messages},parentTraceId={ParentTraceId}): changeToSiteRoot → {ChangeToSiteRoot}", portalId, tab, settings, friendlyUrlPath, cultureCode, endingPageName, messages, parentTraceId, changeToSiteRoot);
                messages = new List<string>();
            }
			Logger.LogError("GetUrlFromExtensionUrlProviders(portalId={PortalId},tab={Tab},settings={Settings},friendlyUrlPath={FriendlyUrlPath},cultureCode={CultureCode},endingPageName={EndingPageName},messages={Messages},parentTraceId={ParentTraceId}): messages → {Messages1}", portalId, tab, settings, friendlyUrlPath, cultureCode, endingPageName, messages, parentTraceId, messages);
			Logger.LogError("GetUrlFromExtensionUrlProviders(portalId={PortalId},tab={Tab},settings={Settings},friendlyUrlPath={FriendlyUrlPath},cultureCode={CultureCode},endingPageName={EndingPageName},messages={Messages},parentTraceId={ParentTraceId}): wasChanged → {WasChanged}", portalId, tab, settings, friendlyUrlPath, cultureCode, endingPageName, messages, parentTraceId, wasChanged);

            try
            {
                List<ExtensionUrlProvider> providersToCall = GetProvidersToCall(tab.TabID, portalId, settings, parentTraceId);
				Logger.LogError("GetUrlFromExtensionUrlProviders(portalId={PortalId},tab={Tab},settings={Settings},friendlyUrlPath={FriendlyUrlPath},cultureCode={CultureCode},endingPageName={EndingPageName},messages={Messages},parentTraceId={ParentTraceId}): providersToCall → {ProvidersToCall}", portalId, tab, settings, friendlyUrlPath, cultureCode, endingPageName, messages, parentTraceId, providersToCall);
                FriendlyUrlOptions options = UrlRewriterUtils.GetOptionsFromSettings(settings);
				Logger.LogError("GetUrlFromExtensionUrlProviders(portalId={PortalId},tab={Tab},settings={Settings},friendlyUrlPath={FriendlyUrlPath},cultureCode={CultureCode},endingPageName={EndingPageName},messages={Messages},parentTraceId={ParentTraceId}): options → {Options}", portalId, tab, settings, friendlyUrlPath, cultureCode, endingPageName, messages, parentTraceId, options);
                foreach (ExtensionUrlProvider provider in providersToCall)
                {
                    activeProvider = provider; // keep for exception purposes
                    bool useDnnPagePath;

                    // go through and call each provider to generate the friendly urls for the module
                    string customPath = provider.ChangeFriendlyUrl(
                        tab,
                        friendlyUrlPath,
                        options,
                        cultureCode,
                        ref endingPageName,
                        out useDnnPagePath,
                        ref messages);

                    if (string.IsNullOrEmpty(endingPageName))
                    {
                        endingPageName = Globals.glbDefaultPage; // set back to default.aspx if provider cleared it
                    }

                    // now check to see if a change was made or not.  Don't trust the provider.
                    if (!string.IsNullOrEmpty(customPath))
                    {
                        // was customPath any different to friendlyUrlPath?
                        if (!string.Equals(customPath, friendlyUrlPath, StringComparison.Ordinal))
                        {
                            wasChanged = true;
                            changedPath = customPath.Trim();
                            changeToSiteRoot = !useDnnPagePath; // useDNNpagePath means no change to site root.
                            messages.Add($"Path returned from {provider.ProviderConfig.ProviderName} -> path:{customPath}, ending Page:{endingPageName}, use Page Path:{useDnnPagePath}");
                            break; // first module provider to change the Url is the only one used
                        }
                    }
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in GetUrlFromExtensionUrlProviders(portalId={PortalId},tab={Tab},settings={Settings},friendlyUrlPath={FriendlyUrlPath},cultureCode={CultureCode},endingPageName={EndingPageName},messages={Messages},parentTraceId={ParentTraceId}): {Arg8}", portalId, tab, settings, friendlyUrlPath, cultureCode, endingPageName, messages, parentTraceId, ex.Message);
                LogModuleProviderExceptionInRequest(ex, "500 Internal Server Error", activeProvider, null, messages);

                // reset all values to defaults
                wasChanged = false;
                changedPath = friendlyUrlPath;
                changeToSiteRoot = false;
            }

			Logger.LogWarning("<<< Exiting GetUrlFromExtensionUrlProviders(portalId={PortalId},tab={Tab},settings={Settings},friendlyUrlPath={FriendlyUrlPath},cultureCode={CultureCode},endingPageName={EndingPageName},messages={Messages},parentTraceId={ParentTraceId})", portalId, tab, settings, friendlyUrlPath, cultureCode, endingPageName, messages, parentTraceId);
            return wasChanged;
        }

        internal static bool TransformFriendlyUrlPath(
            string newUrl,
            string tabKeyVal,
            string[] urlParms,
            bool isSiteRootMatch,
            ref UrlAction result,
            FriendlyUrlSettings settings,
            out string rewrittenUrl,
            out bool newAction,
            ref List<string> messages,
            Guid parentTraceId)
        {
			Logger.LogError(">>> Entering TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId})", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId);
            bool rewriteDone = false;
            rewrittenUrl = newUrl;
			Logger.LogError("TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): rewrittenUrl → {RewrittenUrl}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, rewrittenUrl);
			Logger.LogDebug("TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): rewrittenUrl → {RewrittenUrl}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, rewrittenUrl);
            newAction = false;
			Logger.LogError("TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): newAction → {NewAction}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, newAction);
            ExtensionUrlProvider activeProvider = null;
            try
            {
				Logger.LogError("TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): activeProvider → {ActiveProvider}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, activeProvider);
				Logger.LogDebug("TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): newAction → {NewAction}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, newAction);
                int tabId = result.TabId;
                if (isSiteRootMatch)
                {
					Logger.LogError("TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): rewriteDone → {RewriteDone}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, rewriteDone);
                    tabId = RewriteController.SiteRootRewrite;
					Logger.LogError("TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): tabId → {TabId}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, tabId);
                }

                List<ExtensionUrlProvider> providersToCall = GetProvidersToCall(
                    tabId,
                    result.PortalId,
                    settings,
                    parentTraceId);
                if (providersToCall != null && providersToCall.Count > 0)
                {
                    // now check for providers by calling the providers
                    int upperBound = urlParms.GetUpperBound(0);
					Logger.LogError("TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): upperBound → {UpperBound}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, upperBound);

                    // clean extension off parameters array
                    var parms = new string[upperBound + 1];
                    Array.ConstrainedCopy(urlParms, 0, parms, 0, upperBound + 1);
                    if (upperBound >= 0)
                    {
                        bool replaced;
                        parms[upperBound] = RewriteController.CleanExtension(parms[upperBound], settings, out replaced);
                    }

                    // get options from current settings
                    FriendlyUrlOptions options = UrlRewriterUtils.GetOptionsFromSettings(settings);
					Logger.LogError("TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): options → {Options}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, options);
                    foreach (ExtensionUrlProvider provider in providersToCall)
                    {
                        // set active provider for exception handling
                        activeProvider = provider;

                        // call down to specific providers and see if we get a rewrite
                        string location;
                        int status;
                        string queryString = provider.TransformFriendlyUrlToQueryString(
                            parms,
                            result.TabId,
                            result.PortalId,
                            options,
                            result.CultureCode,
                            result.PortalAlias,
                            ref messages,
                            out status,
                            out location);
                        if (status == 0 || status == 200)
                        {
                            // either not set, or set to '200 OK'.
                            if (!string.IsNullOrEmpty(queryString) && queryString != newUrl)
                            {
                                rewriteDone = true;

                                // check for duplicate tabIds.
                                string qsRemainder = null;
                                if (Regex.IsMatch(queryString, @"tabid=\d+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                                {
                                    // 930 : look for other querystring information in the rewritten Url, or invalid rewritten urls can be created
                                    // pattern to determine which tab matches
                                    // look for any other querystirng information in the already rewritten Url (ie language parameters)
                                    Match rewrittenUrlMatch = RewrittenUrlRegex.Match(rewrittenUrl);
									Logger.LogError("TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): rewrittenUrlMatch → {RewrittenUrlMatch}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, rewrittenUrlMatch);
                                    if (rewrittenUrlMatch.Groups["qs"].Success)
                                    {
                                        // keep any other querystring remainders
                                        qsRemainder = rewrittenUrlMatch.Groups["qs"].Captures.Cast<Capture>().Aggregate(string.Empty, (current, qsCapture) => current + qsCapture.Value); // initialise
                                    }

                                    // supplied value overwrites existing value, so remove from the rewritten url
                                    rewrittenUrl = RewrittenUrlRegex.Replace(rewrittenUrl, string.Empty);
                                }

                                if (rewrittenUrl.Contains("?") == false)
                                {
                                    // use a leading ?, not a leading &
                                    queryString = FriendlyUrlController.EnsureNotLeadingChar("&", queryString);
                                    queryString = FriendlyUrlController.EnsureLeadingChar("?", queryString);
									Logger.LogError("TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): queryString → {QueryString}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, queryString);
                                }
                                else
                                {
                                    // use a leading &, not a leading ?
                                    queryString = FriendlyUrlController.EnsureNotLeadingChar("?", queryString);
                                    queryString = FriendlyUrlController.EnsureLeadingChar("&", queryString);
                                }

                                // add querystring onto rewritten Url
                                rewrittenUrl += queryString;
                                if (qsRemainder != null)
                                {
                                    rewrittenUrl += qsRemainder;
                                }

                                break;
                            }
                        }
                        else
                        {
                            switch (status)
                            {
                                case 301:
                                    result.Action = ActionType.Redirect301;
									Logger.LogDebug("TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): result.Action → {Arg8}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, result.Action);
									Logger.LogDebug("TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): result.FinalUrl → {Arg8}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, result.FinalUrl);
                                    result.Reason = RedirectReason.Module_Provider_Rewrite_Redirect;
									Logger.LogDebug("TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): result.Reason → {Arg8}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, result.Reason);
                                    result.FinalUrl = location;
                                    break;
                                case 302:
                                    result.Action = ActionType.Redirect302;
                                    result.Reason = RedirectReason.Module_Provider_Rewrite_Redirect;
                                    result.FinalUrl = location;
                                    break;
                                case 404:
                                    result.Action = ActionType.Output404;
                                    break;
                                case 500:
                                    result.Action = ActionType.Output500;
                                    break;
                            }

                            newAction = true; // not doing a 200 status
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): {Arg8}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, ex.Message);
                // log module provider exception
                LogModuleProviderExceptionInRequest(ex, "500 Internal Server Error", activeProvider, result, messages);

                // reset values to initial
                rewriteDone = false;
                rewrittenUrl = newUrl;
                newAction = false;
                string providerName = "Unknown";
                if (activeProvider != null)
                {
                    providerName = activeProvider.ProviderConfig.ProviderName;
					Logger.LogError("TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId}): providerName → {ProviderName}", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId, providerName);
                }

                if (result != null)
                {
                    result.DebugMessages.Add("Exception in provider [" + providerName + "] :" + ex.Message);
                }
            }

			Logger.LogWarning("<<< Exiting TransformFriendlyUrlPath(newUrl={NewUrl},tabKeyVal={TabKeyVal},urlParms={UrlParms},isSiteRootMatch={IsSiteRootMatch},result={Result},settings={Settings},messages={Messages},parentTraceId={ParentTraceId})", newUrl, tabKeyVal, urlParms, isSiteRootMatch, result, settings, messages, parentTraceId);
            return rewriteDone;
        }

        private static void ClearCache()
        {
			Logger.LogError(">>> Entering ClearCache()");
            foreach (IPortalInfo portal in PortalController.Instance.GetPortals())
            {
                ClearCache(portal.PortalId);
            }
			Logger.LogWarning("<<< Exiting ClearCache()");
        }

        private static void ClearCache(int portalId)
        {
			Logger.LogError(">>> Entering ClearCache(portalId={PortalId})", portalId);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, "ExtensionUrlProviders_{0}", portalId);
			Logger.LogWarning("<<< Exiting ClearCache(portalId={PortalId})", portalId);
			Logger.LogError("ClearCache(portalId={PortalId}): cacheKey → {CacheKey}", portalId, cacheKey);
            DataCache.RemoveCache(cacheKey);
        }

        /// <summary>Returns the providers to call. Returns tab ID matches first, and any portal ID matches after that.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <param name="parentTraceId">The parent trace ID.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="ExtensionUrlProvider"/> instances.</returns>
        private static List<ExtensionUrlProvider> GetProvidersToCall(
            int tabId,
            int portalId,
            FriendlyUrlSettings settings,
            Guid parentTraceId)
        {
            List<ExtensionUrlProvider> providers;

            // 887 : introduce lockable code to prevent caching race errors
            lock (ProvidersBuildLock)
            {
                bool definitelyNoProvider;

                // 887 : use cached list of tabs instead of per-tab cache of provider
                // get the list of providers to call based on the tab and the portal
                var providersToCall = CacheController.GetProvidersForTabAndPortal(
                    tabId,
                    portalId,
                    settings,
                    out definitelyNoProvider,
                    parentTraceId);
                if (definitelyNoProvider == false && providersToCall == null)
                {
                    // nothing in the cache, and we don't have a definitive 'no' that there isn't a provider
                    // get all providers for the portal
                    var allProviders = GetModuleProviders(portalId).Where(p => p.ProviderConfig.IsActive).ToList();

                    // store the list of tabs for this portal that have a provider attached
                    CacheController.StoreListOfTabsWithProviders(allProviders, portalId, settings);

                    // stash the provider portals in the cache
                    CacheController.StoreModuleProvidersForPortal(portalId, settings, allProviders);

                    // now check if there is a provider for this tab/portal combination
                    if (allProviders.Count > 0)
                    {
                        // find if a module is specific to a tab
                        providersToCall = new List<ExtensionUrlProvider>();
                        providersToCall.AddRange(allProviders);
                    }
                }

                // always return an instantiated provider collection
                providers = providersToCall ?? new List<ExtensionUrlProvider>();
            }

            // return the collection of module providers
			Logger.LogWarning("<<< Exiting GetProvidersToCall(tabId={TabId},portalId={PortalId},settings={Settings},parentTraceId={ParentTraceId})", tabId, portalId, settings, parentTraceId);
            return providers;
        }
    }
}
