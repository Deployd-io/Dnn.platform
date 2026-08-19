// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;

    /// <summary>The TabPathHelper class provides helper methods for working with tab urls.</summary>
    public class TabPathHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabPathHelper));
        public static string BuildTabPathWithReplacement(TabInfo tab, FriendlyUrlOptions options, Guid parentTraceId)
        {
			Logger.LogDebug(">>> Entering BuildTabPathWithReplacement(tab={Tab},options={Options},parentTraceId={ParentTraceId})", tab, options, parentTraceId);
            string path = string.Empty;
            if (tab.ParentId > -1)
            {
                TabInfo parentTab = TabController.Instance.GetTab(tab.ParentId, tab.PortalID, false);
				Logger.LogDebug("BuildTabPathWithReplacement(tab={Tab},options={Options},parentTraceId={ParentTraceId}): parentTab → {ParentTab}", tab, options, parentTraceId, parentTab);

                // 822 : don't assume parent tab is going to exist - database might be corrupted
                // 896 : check to make sure tabid and parentid are different - or stack overflow occurs with terminal loop
                if (parentTab != null && parentTab.TabID != tab.TabID)
                {
                    path = BuildTabPathWithReplacement(parentTab, options, parentTraceId);
                }
            }

            bool modified;
            path = AppendToTabPath(path, tab, options, out modified);

			Logger.LogDebug("<<< Exiting BuildTabPathWithReplacement(tab={Tab},options={Options},parentTraceId={ParentTraceId})", tab, options, parentTraceId);
            return path;
        }

        /// <summary>Replaces the diacritic characters in a path with other values.</summary>
        /// <param name="tabPath">THe tab path.</param>
        /// <param name="replacedDiacritic"><see langword="true"/> if any diacritics were replaced, otherwise <see langword="false"/>.</param>
        /// <returns>The <paramref name="tabPath"/> with diacritics replaced.</returns>
        public static string ReplaceDiacritics(string tabPath, out bool replacedDiacritic)
        {
			Logger.LogDebug(">>> Entering ReplaceDiacritics(tabPath={TabPath})", tabPath);
            string nmTabPath = tabPath.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
			Logger.LogDebug("ReplaceDiacritics(tabPath={TabPath}): sb → {Sb}", tabPath, sb);
            for (int i = 0; i < nmTabPath.Length; i++)
            {
                char c = nmTabPath[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            string result = sb.ToString();
			Logger.LogDebug("ReplaceDiacritics(tabPath={TabPath}): result → {Result}", tabPath, result);
            replacedDiacritic = !string.Equals(tabPath, result, StringComparison.Ordinal);
			Logger.LogDebug("ReplaceDiacritics(tabPath={TabPath}): replacedDiacritic → {ReplacedDiacritic}", tabPath, replacedDiacritic);
			Logger.LogDebug("<<< Exiting ReplaceDiacritics(tabPath={TabPath})", tabPath);
            return sb.ToString();
        }

        /// <summary>Get the tab path for the supplied Tab.</summary>
        /// <param name="tab">The tab info.</param>
        /// <param name="options">The friendly URL options.</param>
        /// <param name="parentTraceId">The parent trace ID.</param>
        /// <returns>The friendly URL path.</returns>
        internal static string GetFriendlyUrlTabPath(TabInfo tab, FriendlyUrlOptions options, Guid parentTraceId)
        {
			Logger.LogDebug(">>> Entering GetFriendlyUrlTabPath(tab={Tab},options={Options},parentTraceId={ParentTraceId})", tab, options, parentTraceId);
            string baseTabPath = tab.TabPath.Replace("//", "/").TrimStart('/');
            if (options.CanGenerateNonStandardPath)
            {
                // build up a non-space version of the tab path
                baseTabPath = BuildTabPathWithReplacement(tab, options, parentTraceId);
                baseTabPath = baseTabPath.Replace("//", "/");

                // automatic diacritic conversion
                if (options.ConvertDiacriticChars)
                {
                    bool diacriticsChanged;
                    baseTabPath = ReplaceDiacritics(baseTabPath, out diacriticsChanged);
                }
            }

			Logger.LogDebug("<<< Exiting GetFriendlyUrlTabPath(tab={Tab},options={Options},parentTraceId={ParentTraceId})", tab, options, parentTraceId);
            return baseTabPath;
        }

        /// <summary>Finds a culture-specific homepage tab ID for a non-default language.</summary>
        /// <param name="defaultCulture">The default culture of the portal.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <param name="defaultHomeTabId">The default home page tab id.</param>
        /// <returns>The valid home page tab ID for the portal ID and culture.</returns>
        /// <remarks>Note if no specific home page found for the culture, will return defaultHomeTabId back.</remarks>
        internal static int GetHomePageTabIdForCulture(string defaultCulture, int portalId, string cultureCode, int defaultHomeTabId)
        {
			Logger.LogDebug(">>> Entering GetHomePageTabIdForCulture(defaultCulture={DefaultCulture},portalId={PortalId},cultureCode={CultureCode},defaultHomeTabId={DefaultHomeTabId})", defaultCulture, portalId, cultureCode, defaultHomeTabId);
            int homeTabId = defaultHomeTabId;
            if (!string.Equals(defaultCulture, cultureCode, StringComparison.OrdinalIgnoreCase))
            {
                // not the default culture, so there could be a different home page for the different culture in 5.5+ builds
                var cultureLocale = new Locale { Code = cultureCode, Fallback = cultureCode, Text = cultureCode };
                TabInfo tab = TabController.Instance.GetTabByCulture(defaultHomeTabId, portalId, cultureLocale);
				Logger.LogDebug("GetHomePageTabIdForCulture(defaultCulture={DefaultCulture},portalId={PortalId},cultureCode={CultureCode},defaultHomeTabId={DefaultHomeTabId}): tab → {Tab}", defaultCulture, portalId, cultureCode, defaultHomeTabId, tab);
                if (tab != null)
                {
                    // this is the culture specific tab of the home page
                    homeTabId = tab.TabID;
                }
            }

			Logger.LogDebug("<<< Exiting GetHomePageTabIdForCulture(defaultCulture={DefaultCulture},portalId={PortalId},cultureCode={CultureCode},defaultHomeTabId={DefaultHomeTabId})", defaultCulture, portalId, cultureCode, defaultHomeTabId);
            return homeTabId;
        }

        internal static string GetTabAliasSkinForTabAndAlias(int portalId, string httpAlias, string culture)
        {
			Logger.LogDebug(">>> Entering GetTabAliasSkinForTabAndAlias(portalId={PortalId},httpAlias={HttpAlias},culture={Culture})", portalId, httpAlias, culture);
            string skin = null;
            Hashtable homePageSkins = CacheController.GetHomePageSkinsFromCache(portalId);
			Logger.LogDebug("GetTabAliasSkinForTabAndAlias(portalId={PortalId},httpAlias={HttpAlias},culture={Culture}): homePageSkins → {HomePageSkins}", portalId, httpAlias, culture, homePageSkins);
			Logger.LogDebug("GetTabAliasSkinForTabAndAlias(portalId={PortalId},httpAlias={HttpAlias},culture={Culture}): skin → {Skin}", portalId, httpAlias, culture, skin);
            if (homePageSkins != null)
            {
                string key = httpAlias + "::" + culture;
                string key2 = httpAlias;
                if (homePageSkins.ContainsKey(key))
                {
                    skin = (string)homePageSkins[key];
                }
                else if (homePageSkins.ContainsKey(key2))
                {
                    skin = (string)homePageSkins[key2];
                }
            }

			Logger.LogDebug("<<< Exiting GetTabAliasSkinForTabAndAlias(portalId={PortalId},httpAlias={HttpAlias},culture={Culture})", portalId, httpAlias, culture);
            return skin;
        }

        /// <summary>For the supplied options, return a tab path for the specified tab.</summary>
        /// <param name="tab">TabInfo object of selected tab.</param>
        /// <param name="settings">FriendlyUrlSettings.</param>
        /// <param name="options">The friendly URL options.</param>
        /// <param name="ignoreCustomRedirects">Whether to add in the customised Tab redirects or not.</param>
        /// <param name="homePageSiteRoot">Whether the home page is the site root.</param>
        /// <param name="isHomeTab">Whether it's the home tab.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <param name="isDefaultCultureCode">Whether it's the default culture code.</param>
        /// <param name="hasPath">Whether there's a path….</param>
        /// <param name="dropLangParms">Whether the language parameters were removed.</param>
        /// <param name="customHttpAlias">The custom HTTP alias that was used, if any.</param>
        /// <param name="isCustomPath">Whether it's a custom path.</param>
        /// <param name="parentTraceId">The parent trace ID.</param>
        /// <remarks>751 : include isDefaultCultureCode flag to determine when using the portal default language
        /// 770 : include custom http alias output for when the Url uses a specific alias due to custom Url rules
        ///  : include new out parameter 'isCustomPath' to return whether the Url was generated from Url-Master custom url.
        /// </remarks>
        /// <returns>The tab path as specified.</returns>
        internal static string GetTabPath(
            TabInfo tab,
            FriendlyUrlSettings settings,
            FriendlyUrlOptions options,
            bool ignoreCustomRedirects,
            bool homePageSiteRoot,
            bool isHomeTab,
            string cultureCode,
            bool isDefaultCultureCode,
            bool hasPath,
            out bool dropLangParms,
            out string customHttpAlias,
            out bool isCustomPath,
            Guid parentTraceId)
        {
			Logger.LogDebug(">>> Entering GetTabPath(tab={Tab},settings={Settings},options={Options},ignoreCustomRedirects={IgnoreCustomRedirects},homePageSiteRoot={HomePageSiteRoot},isHomeTab={IsHomeTab},cultureCode={CultureCode},isDefaultCultureCode={IsDefaultCultureCode},hasPath={HasPath},parentTraceId={ParentTraceId})", tab, settings, options, ignoreCustomRedirects, homePageSiteRoot, isHomeTab, cultureCode, isDefaultCultureCode, hasPath, parentTraceId);
            string newTabPath;
            dropLangParms = false;
            customHttpAlias = null;
            isCustomPath = false;
            if (homePageSiteRoot && isHomeTab && !hasPath)
            {
                ////&& !isDefaultCultureCode - not working for non-language specific custom root urls
                newTabPath = "/"; // site root for home page
            }
            else
            {
                // build the tab path and check for space replacement
                string baseTabPath = TabIndexController.GetTabPath(tab, options, parentTraceId);
				Logger.LogDebug("GetTabPath(tab={Tab},settings={Settings},options={Options},ignoreCustomRedirects={IgnoreCustomRedirects},homePageSiteRoot={HomePageSiteRoot},isHomeTab={IsHomeTab},cultureCode={CultureCode},isDefaultCultureCode={IsDefaultCultureCode},hasPath={HasPath},parentTraceId={ParentTraceId}): baseTabPath → {BaseTabPath}", tab, settings, options, ignoreCustomRedirects, homePageSiteRoot, isHomeTab, cultureCode, isDefaultCultureCode, hasPath, parentTraceId, baseTabPath);

                // this is the new tab path
                newTabPath = baseTabPath;

				Logger.LogDebug("GetTabPath(tab={Tab},settings={Settings},options={Options},ignoreCustomRedirects={IgnoreCustomRedirects},homePageSiteRoot={HomePageSiteRoot},isHomeTab={IsHomeTab},cultureCode={CultureCode},isDefaultCultureCode={IsDefaultCultureCode},hasPath={HasPath},parentTraceId={ParentTraceId}): newTabPath → {NewTabPath}", tab, settings, options, ignoreCustomRedirects, homePageSiteRoot, isHomeTab, cultureCode, isDefaultCultureCode, hasPath, parentTraceId, newTabPath);
                // 871 : case insensitive compare for culture code, all lookups done on lower case
                string cultureCodeKey = string.Empty;
				Logger.LogDebug("GetTabPath(tab={Tab},settings={Settings},options={Options},ignoreCustomRedirects={IgnoreCustomRedirects},homePageSiteRoot={HomePageSiteRoot},isHomeTab={IsHomeTab},cultureCode={CultureCode},isDefaultCultureCode={IsDefaultCultureCode},hasPath={HasPath},parentTraceId={ParentTraceId}): cultureCodeKey → {CultureCodeKey}", tab, settings, options, ignoreCustomRedirects, homePageSiteRoot, isHomeTab, cultureCode, isDefaultCultureCode, hasPath, parentTraceId, cultureCodeKey);
                if (cultureCode != null)
                {
                    cultureCodeKey = cultureCode.ToLowerInvariant();
                }

                bool checkForCustomHttpAlias = false;
				Logger.LogDebug("GetTabPath(tab={Tab},settings={Settings},options={Options},ignoreCustomRedirects={IgnoreCustomRedirects},homePageSiteRoot={HomePageSiteRoot},isHomeTab={IsHomeTab},cultureCode={CultureCode},isDefaultCultureCode={IsDefaultCultureCode},hasPath={HasPath},parentTraceId={ParentTraceId}): checkForCustomHttpAlias → {CheckForCustomHttpAlias}", tab, settings, options, ignoreCustomRedirects, homePageSiteRoot, isHomeTab, cultureCode, isDefaultCultureCode, hasPath, parentTraceId, checkForCustomHttpAlias);

                // get a custom tab name if redirects are being used
                SharedDictionary<string, string> customAliasForTabs = null;
                SharedDictionary<int, SharedDictionary<string, string>> urlDict;

                // 886 : don't fetch custom urls for host tabs (host tabs can't have redirects or custom Urls)
                if (tab.PortalID > -1)
                {
                    urlDict = CustomUrlDictController.FetchCustomUrlDictionary(tab.PortalID, false, false, settings, out customAliasForTabs, parentTraceId);
                }
                else
                {
                    // create empty dictionary for host tabs
                    urlDict = new SharedDictionary<int, SharedDictionary<string, string>>();
                }

                if (!ignoreCustomRedirects)
                {
                    // if not ignoring the custom redirects, look for the Url of the page in this list
                    // this will be used as the page path if there is one.
                    using (urlDict.GetReadLock())
                    {
                        if (urlDict.TryGetValue(tab.TabID, out var tabPaths))
                        {
                            // we want the custom value
                            string customTabPath = null;
							Logger.LogDebug("GetTabPath(tab={Tab},settings={Settings},options={Options},ignoreCustomRedirects={IgnoreCustomRedirects},homePageSiteRoot={HomePageSiteRoot},isHomeTab={IsHomeTab},cultureCode={CultureCode},isDefaultCultureCode={IsDefaultCultureCode},hasPath={HasPath},parentTraceId={ParentTraceId}): customTabPath → {CustomTabPath}", tab, settings, options, ignoreCustomRedirects, homePageSiteRoot, isHomeTab, cultureCode, isDefaultCultureCode, hasPath, parentTraceId, customTabPath);

                            using (tabPaths.GetReadLock())
                            {
                                if (tabPaths.TryGetValue(cultureCodeKey, out var tabPath))
                                {
                                    customTabPath = tabPath;
                                    dropLangParms = true;

                                    // the url is based on a custom value which has embedded language parms, therefore don't need them in the url
                                }
                                else
                                {
                                    if (isDefaultCultureCode && tabPaths.TryGetValue(string.Empty, out tabPath))
                                    {
                                        customTabPath = tabPath;

                                        ////dropLangParms = true; // drop the language parms if they exist, because this is the default language
                                    }
                                }
                            }

                            if (customTabPath != null)
                            {
                                // 770 : pull out custom http alias if in string
                                int aliasSeparator = customTabPath.IndexOf("::", StringComparison.Ordinal);
								Logger.LogDebug("GetTabPath(tab={Tab},settings={Settings},options={Options},ignoreCustomRedirects={IgnoreCustomRedirects},homePageSiteRoot={HomePageSiteRoot},isHomeTab={IsHomeTab},cultureCode={CultureCode},isDefaultCultureCode={IsDefaultCultureCode},hasPath={HasPath},parentTraceId={ParentTraceId}): aliasSeparator → {AliasSeparator}", tab, settings, options, ignoreCustomRedirects, homePageSiteRoot, isHomeTab, cultureCode, isDefaultCultureCode, hasPath, parentTraceId, aliasSeparator);
                                if (aliasSeparator > 0)
                                {
                                    customHttpAlias = customTabPath.Substring(0, aliasSeparator);
                                    newTabPath = customTabPath.Substring(aliasSeparator + 2);
                                }
                                else
                                {
                                    newTabPath = customTabPath;
                                }
                            }

                            if (newTabPath == string.Empty && hasPath)
                            {
                                // can't pass back a custom path which is blank if there are path segments to the requested final Url
                                newTabPath = baseTabPath; // revert back to the standard DNN page path
                            }
                            else
                            {
                                isCustomPath = true; // we are providing a custom Url
                            }
                        }
                        else
                        {
                            checkForCustomHttpAlias = true;
                        }
                    }
                }
                else
                {
                    checkForCustomHttpAlias = true;

                    // always want to check for custom alias, even when we don't want to see any custom redirects
                }

                // 770 : check for custom alias in these tabs
                if (checkForCustomHttpAlias && customAliasForTabs != null)
                {
                    string key = tab.TabID.ToString(CultureInfo.InvariantCulture) + ":" + cultureCodeKey;
                    using (customAliasForTabs.GetReadLock())
                    {
                        if (customAliasForTabs.TryGetValue(key, out var alias))
                        {
                            // this tab uses a custom alias
                            customHttpAlias = alias;
                            isCustomPath = true; // using custom alias
                        }
                    }
                }

                if (!dropLangParms)
                {
                    string tabCultureCode = tab.CultureCode;
                    if (!string.IsNullOrEmpty(tabCultureCode))
                    {
                        dropLangParms = true;

                        // if the tab has a specified culture code, then drop the language parameters from the friendly Url
                    }
                }

                // make lower case if necessary
                newTabPath = AdvancedFriendlyUrlProvider.ForceLowerCaseIfAllowed(tab, newTabPath, settings);
            }

			Logger.LogDebug("<<< Exiting GetTabPath(tab={Tab},settings={Settings},options={Options},ignoreCustomRedirects={IgnoreCustomRedirects},homePageSiteRoot={HomePageSiteRoot},isHomeTab={IsHomeTab},cultureCode={CultureCode},isDefaultCultureCode={IsDefaultCultureCode},hasPath={HasPath},parentTraceId={ParentTraceId})", tab, settings, options, ignoreCustomRedirects, homePageSiteRoot, isHomeTab, cultureCode, isDefaultCultureCode, hasPath, parentTraceId);
            return newTabPath;
        }

        internal static bool IsTabHomePage(TabInfo tab, PortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering IsTabHomePage(tab={Tab},portalSettings={PortalSettings})", tab, portalSettings);
            bool isTabHomePage = tab.TabID == portalSettings.HomeTabId;
            if (!isTabHomePage)
            {
                // 756: check if content localization is switched on before checking for other languages
                if (Host.Host.EnableBrowserLanguage)
                {
                    // check the localised versions of the tab
                    TabInfo defaultLangTab = tab.DefaultLanguageTab;
                    if (defaultLangTab != null)
                    {
                        // is the default language tab the home tab?
                        if (defaultLangTab.TabID == portalSettings.HomeTabId)
                        {
                            isTabHomePage = true;
                        }
                        else
                        {
                            // iterate through the other localised versions of this tab and check if they are the same
                            // as the home tabid
                            Dictionary<string, TabInfo> localizedTabs = tab.LocalizedTabs;
                            if (localizedTabs.Values.Any(localTab => localTab.TabID == portalSettings.HomeTabId))
                            {
                                isTabHomePage = true;
                            }
                        }
                    }
                    else
                    {
                        // check if this tab belongs to the default language, in which case it is already the default language tab
                        string cultureCode = tab.CultureCode;
                        if (string.Equals(cultureCode, portalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                        {
                            // get the localized versions and see if this matches
                            Dictionary<string, TabInfo> localizedTabs = tab.LocalizedTabs;
                            if (localizedTabs.Values.Any(localTab => localTab.TabID == portalSettings.HomeTabId))
                            {
                                isTabHomePage = true;
                            }
                        }
                    }
                }
            }

			Logger.LogDebug("<<< Exiting IsTabHomePage(tab={Tab},portalSettings={PortalSettings})", tab, portalSettings);
            return isTabHomePage;
        }

        private static string AppendToTabPath(string path, TabInfo tab, FriendlyUrlOptions options, out bool modified)
        {
			Logger.LogDebug(">>> Entering AppendToTabPath(path={Path},tab={Tab},options={Options})", path, tab, options);
            string tabName = tab.TabName;
            var result = new StringBuilder(tabName.Length);
			Logger.LogDebug("AppendToTabPath(path={Path},tab={Tab},options={Options}): result → {Result}", path, tab, options, result);
			Logger.LogDebug("AppendToTabPath(path={Path},tab={Tab},options={Options}): tabName → {TabName}", path, tab, options, tabName);

            // 922 : change to harmonise cleaning of tab + other url name items
            tabName = FriendlyUrlController.CleanNameForUrl(tabName, options, out modified);
            if (!modified
                && string.IsNullOrEmpty(options.PunctuationReplacement) == false
                && tab.TabName.Contains(" ")
                && tabName.Contains(" ") == false)
            {
                modified = true;
				Logger.LogDebug("AppendToTabPath(path={Path},tab={Tab},options={Options}): modified → {Modified}", path, tab, options, modified);

                // spaces replaced - the modified parameter is for all other replacements but space replacements
            }

            result.Append(tabName);
            result.Insert(0, "//");
            result.Insert(0, path); // effectively adds result to the end of the path
			Logger.LogDebug("<<< Exiting AppendToTabPath(path={Path},tab={Tab},options={Options})", path, tab, options);
            return result.ToString();
        }
    }
}
