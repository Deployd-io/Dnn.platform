// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.ClientCapability;
    using DotNetNuke.Services.Exceptions;

    using NewBrowserTypes = DotNetNuke.Abstractions.Urls.BrowserTypes;
#pragma warning disable CS0618 // Type or member is obsolete
    using OldBrowserTypes = DotNetNuke.Entities.Urls.BrowserTypes;
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>The friendly URL controller.</summary>
    public partial class FriendlyUrlController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FriendlyUrlController));
        private const string DisableMobileRedirectCookieName = "disablemobileredirect"; // dnn cookies
        private const string DisableRedirectPresistCookieName = "disableredirectpresist"; // dnn cookies

        private const string DisableMobileRedirectQueryStringName = "nomo";

        // google uses the same name nomo=1 means do not redirect to mobile
        // set the web.config AppSettings for the mobile view cookie name
        private static readonly string MobileViewSiteCookieName = ConfigurationManager.AppSettings[name: "MobileViewSiteCookieName"] ?? "dnn_IsMobile";
        private static readonly string DisableMobileViewCookieName = ConfigurationManager.AppSettings[name: "DisableMobileViewSiteCookieName"] ?? "dnn_NoMobile";

        // <summary>Gets the Friendly URL Settings for the given portal.</summary>
        public static FriendlyUrlSettings GetCurrentSettings(int portalId)
            => new FriendlyUrlSettings(portalId);

        /// <summary>Gets a dictionary of pages in the portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeStdUrls">This parameter is unused.</param>
        /// <returns>A dictionary mapping from tab ID to <see cref="TabInfo"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload with portal ID and FriendlyUrlSettings (the other argument isn't used).")]
        public static partial Dictionary<int, TabInfo> GetTabs(int portalId, bool includeStdUrls)
            => GetTabs(portalId, includeStdUrls, GetCurrentSettings(portalId));

        /// <summary>Gets a dictionary of pages in the portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeStdUrls">This parameter is unused.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <returns>A dictionary mapping from tab ID to <see cref="TabInfo"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload with portal ID and FriendlyUrlSettings (the other argument isn't used).")]
        public static partial Dictionary<int, TabInfo> GetTabs(int portalId, bool includeStdUrls, FriendlyUrlSettings settings)
            => GetTabs(portalId, settings);

        /// <summary>Gets a dictionary of pages in the portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeStdUrls">This parameter is unused.</param>
        /// <param name="portalSettings">This parameter is also unused.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <returns>A dictionary mapping from tab ID to <see cref="TabInfo"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload with portal ID and FriendlyUrlSettings (the other arguments aren't used).")]
        public static partial Dictionary<int, TabInfo> GetTabs(int portalId, bool includeStdUrls, PortalSettings portalSettings, FriendlyUrlSettings settings)
            => GetTabs(portalId, settings);

        /// <summary>Gets a dictionary of pages in the portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <returns>A dictionary mapping from tab ID to <see cref="TabInfo"/>.</returns>
        public static Dictionary<int, TabInfo> GetTabs(int portalId, FriendlyUrlSettings settings)
        {
            // 811 : friendly urls for admin/host tabs
            var tabs = new Dictionary<int, TabInfo>();
            var portalTabs = TabController.Instance.GetTabsByPortal(portalId);
            var hostTabs = TabController.Instance.GetTabsByPortal(-1);

            foreach (TabInfo tab in portalTabs.Values)
            {
                tabs[tab.TabID] = tab;
            }

            if (settings.FriendlyAdminHostUrls)
            {
                foreach (TabInfo tab in hostTabs.Values)
                {
                    tabs[tab.TabID] = tab;
                }
            }

            return tabs;
        }

        /// <summary>Returns a list of http alias values where that alias is associated with a tab as a custom alias.</summary>
        /// <remarks>Aliases returned are all in lower case only.</remarks>
        /// <returns>A <see cref="List{T}"/> of alias strings.</returns>
        public static List<string> GetCustomAliasesForTabs()
        {
			Logger.LogDebug(">>> Entering GetCustomAliasesForTabs()");
            var aliases = new List<string>();

			Logger.LogDebug("GetCustomAliasesForTabs(): aliases → {Aliases}", aliases);
            IDataReader dr = DataProvider.Instance().GetCustomAliasesForTabs();
			Logger.LogDebug("GetCustomAliasesForTabs(): dr → {Dr}", dr);
            try
            {
                while (dr.Read())
                {
                    aliases.Add(Null.SetNullString(dr["HttpAlias"]));
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in GetCustomAliasesForTabs(): {Arg0}", exc.getMessage());
                Exceptions.LogException(exc);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

			Logger.LogDebug("<<< Exiting GetCustomAliasesForTabs()");
            return aliases;
        }

        public static TabInfo GetTab(int tabId, bool addStdUrls)
        {
			Logger.LogDebug(">>> Entering GetTab(tabId={TabId},addStdUrls={AddStdUrls})", tabId, addStdUrls);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
			Logger.LogDebug("GetTab(tabId={TabId},addStdUrls={AddStdUrls}): portalSettings → {PortalSettings}", tabId, addStdUrls, portalSettings);
			Logger.LogDebug("<<< Exiting GetTab(tabId={TabId},addStdUrls={AddStdUrls})", tabId, addStdUrls);
			Logger.LogDebug("<<< Exiting GetTab(tabId={TabId},addStdUrls={AddStdUrls},portalSettings={PortalSettings},settings={Settings})", tabId, addStdUrls, portalSettings, settings);
            return GetTab(tabId, addStdUrls, portalSettings, GetCurrentSettings(portalSettings.PortalId));
        }

        public static TabInfo GetTab(int tabId, bool addStdUrls, PortalSettings portalSettings, FriendlyUrlSettings settings)
            => GetTab(tabId, addStdUrls, (IPortalSettings)portalSettings, settings);

        public static TabInfo GetTab(int tabId, bool addStdUrls, IPortalSettings portalSettings, FriendlyUrlSettings settings)
        {
            TabInfo tab = TabController.Instance.GetTab(tabId, portalSettings.PortalId, false);
            if (addStdUrls)
            {
                // Add on the standard Urls that exist for a tab, based on settings like
                // replacing spaces, diacritic characters and languages
                ////BuildFriendlyUrls(tab, true, portalSettings, settings);
            }

            return tab;
        }

        public static string CleanNameForUrl(string urlName, FriendlyUrlOptions options, out bool replacedUnwantedChars)
        {
			Logger.LogDebug(">>> Entering CleanNameForUrl(urlName={UrlName},options={Options})", urlName, options);
            replacedUnwantedChars = false;

			Logger.LogDebug("CleanNameForUrl(urlName={UrlName},options={Options}): replacedUnwantedChars → {ReplacedUnwantedChars}", urlName, options, replacedUnwantedChars);
            // get options
            if (options == null)
            {
                options = new FriendlyUrlOptions();
				Logger.LogDebug("CleanNameForUrl(urlName={UrlName},options={Options}): options → {Options1}", urlName, options, options);
            }

            bool convertDiacritics = options.ConvertDiacriticChars;
            Regex regexMatch = options.RegexMatchRegex;
            string replaceWith = options.PunctuationReplacement;
            bool replaceDoubleChars = options.ReplaceDoubleChars;
            Dictionary<string, string> replacementChars = options.ReplaceCharWithChar;

            if (urlName == null)
            {
                urlName = string.Empty;
            }

            var result = new StringBuilder(urlName.Length);
			Logger.LogDebug("CleanNameForUrl(urlName={UrlName},options={Options}): result → {Result}", urlName, options, result);
            int i = 0;
            string normalisedUrl = urlName;
			Logger.LogDebug("CleanNameForUrl(urlName={UrlName},options={Options}): normalisedUrl → {NormalisedUrl}", urlName, options, normalisedUrl);
            if (convertDiacritics)
            {
                normalisedUrl = urlName.Normalize(NormalizationForm.FormD);
                if (!string.Equals(normalisedUrl, urlName, StringComparison.Ordinal))
                {
                    replacedUnwantedChars = true; // replaced an accented character
                }
            }

            int last = normalisedUrl.Length - 1;
            bool doublePeriod = false;
			Logger.LogDebug("CleanNameForUrl(urlName={UrlName},options={Options}): doublePeriod → {DoublePeriod}", urlName, options, doublePeriod);
            foreach (char c in normalisedUrl)
            {
                // look for a double period in the name
                if (!doublePeriod && i > 0 && c == '.' && normalisedUrl[i - 1] == '.')
                {
                    doublePeriod = true;
                }

                // use string for manipulation
                string ch = c.ToString(CultureInfo.InvariantCulture);
				Logger.LogDebug("CleanNameForUrl(urlName={UrlName},options={Options}): ch → {Ch}", urlName, options, ch);

                // do replacement in pre-defined list?
                if (replacementChars != null && replacementChars.TryGetValue(ch, out var replacement))
                {
                    // replace with value
                    ch = replacement;
                    replacedUnwantedChars = true;
                }
                else if (convertDiacritics && CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                {
                    ch = string.Empty;
                    replacedUnwantedChars = true;
                }
                else
                {
                    // Check if ch is in the replace list
                    CheckCharsForReplace(options, ref ch, ref replacedUnwantedChars);

                    // not in replacement list, check if valid char
                    if (regexMatch.IsMatch(ch))
                    {
                        ch = string.Empty; // not a replacement or allowed char, so doesn't go into Url
                        replacedUnwantedChars = true;

                        // if we are here, this character isn't going into the output Url
                    }
                }

                // Check if the final ch is an illegal char
                CheckIllegalChars(options.IllegalChars, ref ch, ref replacedUnwantedChars);
                if (i == last)
                {
                    // 834 : strip off last character if it is a '.'
                    if (!(ch == "-" || ch == replaceWith || ch == "."))
                    {
                        // only append if not the same as the replacement character
                        result.Append(ch);
                    }
                    else
                    {
                        replacedUnwantedChars = true; // last char not added - effectively replaced with nothing.
                    }
                }
                else
                {
                    result.Append(ch);
                }

                i++; // increment counter
            }

            if (doublePeriod)
            {
                result = result.Replace("..", string.Empty);
            }

            // replace any duplicated replacement characters by doing replace twice
            // replaces -- with - or --- with -  //749 : ampersand not completed replaced
            if (replaceDoubleChars && !string.IsNullOrEmpty(replaceWith))
            {
                result = result.Replace(replaceWith + replaceWith, replaceWith);
                result = result.Replace(replaceWith + replaceWith, replaceWith);
            }

			Logger.LogDebug("<<< Exiting CleanNameForUrl(urlName={UrlName},options={Options})", urlName, options);
            return result.ToString();
        }

        /// <summary>Ensures that the path starts with the leading character.</summary>
        /// <param name="leading">The content to ensure is at the beginning of the <paramref name="path"/>.</param>
        /// <param name="path">The path to ensure starts with <paramref name="leading"/>.</param>
        /// <returns>The <paramref name="path"/> with <paramref name="leading"/> at the start.</returns>
        public static string EnsureLeadingChar(string leading, string path)
        {
			Logger.LogDebug(">>> Entering EnsureLeadingChar(leading={Leading},path={Path})", leading, path);
            if (leading != null && path != null
                                && leading.Length <= path.Length && leading != string.Empty)
            {
                string start = path.Substring(0, leading.Length);
                if (!string.Equals(start, leading, StringComparison.OrdinalIgnoreCase))
                {
                    // not leading with this
                    path = leading + path;
                }
            }

			Logger.LogDebug("<<< Exiting EnsureLeadingChar(leading={Leading},path={Path})", leading, path);
            return path;
        }

        public static string EnsureNotLeadingChar(string leading, string path)
        {
			Logger.LogDebug(">>> Entering EnsureNotLeadingChar(leading={Leading},path={Path})", leading, path);
            if (leading != null && path != null
                                && leading.Length <= path.Length && leading != string.Empty)
            {
                string start = path.Substring(0, leading.Length);
                if (string.Equals(start, leading, StringComparison.OrdinalIgnoreCase))
                {
                    // matches start, take leading off
                    path = path.Substring(leading.Length);
					Logger.LogDebug("EnsureNotLeadingChar(leading={Leading},path={Path}): path → {Path1}", leading, path, path);
                }
            }

			Logger.LogDebug("<<< Exiting EnsureNotLeadingChar(leading={Leading},path={Path})", leading, path);
            return path;
        }

        /// <summary>Gets the browser type for the request.</summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="settings">The URL settings.</param>
        /// <returns>The browser type.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking HttpRequestBase")]
        public static partial OldBrowserTypes GetBrowserType(HttpRequest request, HttpResponse response, FriendlyUrlSettings settings)
            => GetBrowserType(new HttpRequestWrapper(request), new HttpResponseWrapper(response), settings).ToDeprecatedBrowserTypes();

        /// <summary>Gets the browser type for the request.</summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="settings">The URL settings.</param>
        /// <returns>The browser type.</returns>
        public static NewBrowserTypes GetBrowserType(HttpRequestBase request, HttpResponseBase response, FriendlyUrlSettings settings)
        {
            var browserType = NewBrowserTypes.Normal;
            if (request != null && settings != null)
            {
                bool isCookieSet = false;
                bool isMobile = false;
                if (CanUseMobileDevice(request, response))
                {
                    HttpCookie viewMobileCookie = response.Cookies[MobileViewSiteCookieName];
                    if (viewMobileCookie != null && bool.TryParse(viewMobileCookie.Value, out isMobile))
                    {
                        isCookieSet = true;
                    }

                    if (isMobile == false)
                    {
                        if (!isCookieSet)
                        {
                            isMobile = IsMobileClient();
                            if (isMobile)
                            {
                                browserType = NewBrowserTypes.Mobile;
                            }

                            // Store the result as a cookie.
                            if (viewMobileCookie == null)
                            {
                                response.Cookies.Add(new HttpCookie(MobileViewSiteCookieName, isMobile.ToString())
                                    { Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/" });
                            }
                            else
                            {
                                viewMobileCookie.Value = isMobile.ToString();
                            }
                        }
                    }
                    else
                    {
                        browserType = NewBrowserTypes.Mobile;
                    }
                }
            }

            return browserType;
        }

        public static string ValidateUrl(string cleanUrl, int validateUrlForTabId, PortalSettings settings, out bool modified)
        {
			Logger.LogDebug(">>> Entering ValidateUrl(cleanUrl={CleanUrl},validateUrlForTabId={ValidateUrlForTabId},settings={Settings})", cleanUrl, validateUrlForTabId, settings);
            modified = false;
            bool isUnique;
			Logger.LogDebug("ValidateUrl(cleanUrl={CleanUrl},validateUrlForTabId={ValidateUrlForTabId},settings={Settings}): modified → {Modified}", cleanUrl, validateUrlForTabId, settings, modified);
            var uniqueUrl = cleanUrl;
			Logger.LogDebug("ValidateUrl(cleanUrl={CleanUrl},validateUrlForTabId={ValidateUrlForTabId},settings={Settings}): uniqueUrl → {UniqueUrl}", cleanUrl, validateUrlForTabId, settings, uniqueUrl);
            int counter = 0;
            do
            {
                if (counter > 0)
                {
                    uniqueUrl = uniqueUrl + counter.ToString(CultureInfo.InvariantCulture);
                    modified = true;
                }

                isUnique = ValidateUrl(uniqueUrl, validateUrlForTabId, settings);
                counter++;
				Logger.LogDebug("ValidateUrl(cleanUrl={CleanUrl},validateUrlForTabId={ValidateUrlForTabId},settings={Settings}): isUnique → {IsUnique}", cleanUrl, validateUrlForTabId, settings, isUnique);
            }
            while (!isUnique);

			Logger.LogDebug("<<< Exiting ValidateUrl(cleanUrl={CleanUrl},validateUrlForTabId={ValidateUrlForTabId},settings={Settings})", cleanUrl, validateUrlForTabId, settings);
			Logger.LogDebug("<<< Exiting ValidateUrl(url={Url},validateUrlForTabId={ValidateUrlForTabId},settings={Settings})", url, validateUrlForTabId, settings);
            return uniqueUrl;
        }

        internal static bool CanUseMobileDevice(HttpRequestBase request, HttpResponseBase response)
        {
			Logger.LogDebug(">>> Entering CanUseMobileDevice(request={Request},response={Response})", request, response);
            var canUseMobileDevice = true;
            int val;
			Logger.LogDebug("CanUseMobileDevice(request={Request},response={Response}): canUseMobileDevice → {CanUseMobileDevice}", request, response, canUseMobileDevice);
            if (int.TryParse(request.QueryString[DisableMobileRedirectQueryStringName], out val))
            {
                // the nomo value is in the querystring
                if (val == 1)
                {
                    // no, can't do it
                    canUseMobileDevice = false;
                    var cookie = new HttpCookie(DisableMobileViewCookieName)
                    {
                        Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                    };
                    response.Cookies.Set(cookie);
                }
                else
                {
                    // check for disable mobile view cookie name
                    var cookie = request.Cookies[DisableMobileViewCookieName];

                    if (cookie != null)
                    {
                        // if exists, expire cookie to allow redirect
                        cookie = new HttpCookie(DisableMobileViewCookieName)
                        {
                            Expires = DateTime.Now.AddMinutes(-1),
                            Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                        };
                        response.Cookies.Set(cookie);
                    }

                    // check the DotNetNuke cookies for allowed
                    // check for cookie
                    if (request.Cookies[DisableMobileRedirectCookieName] != null
                        && request.Cookies[DisableRedirectPresistCookieName] != null)
                    {
                        // cookies exist, can't use mobile device
                        canUseMobileDevice = false;
                    }
                }
            }
            else
            {
                // look for disable mobile view cookie
                var cookie = request.Cookies[DisableMobileViewCookieName];

                if (cookie != null)
                {
                    canUseMobileDevice = false;
                }
            }

			Logger.LogDebug("<<< Exiting CanUseMobileDevice(request={Request},response={Response})", request, response);
            return canUseMobileDevice;
        }

        /// <summary>Replaces the core IsAdminTab call which was decommissioned for DNN 5.0.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="tabPath">The path of the tab, e.g. <c>"//admin//someothername"</c>.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <returns><see langword="true"/> if the page is an admin page, otherwise <see langword="false"/>.</returns>
        internal static bool IsAdminTab(int portalId, string tabPath, FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering IsAdminTab(portalId={PortalId},tabPath={TabPath},settings={Settings})", portalId, tabPath, settings);
			Logger.LogDebug("<<< Exiting IsAdminTab(portalId={PortalId},tabPath={TabPath},settings={Settings})", portalId, tabPath, settings);
            return RewriteController.IsAdminTab(portalId, tabPath, settings);
        }

        private static bool IsMobileClient()
        {
			Logger.LogDebug(">>> Entering IsMobileClient()");
			Logger.LogDebug("<<< Exiting IsMobileClient()");
            return (HttpContext.Current.Request.Browser != null) && (ClientCapabilityProvider.Instance() != null) && ClientCapabilityProvider.CurrentClientCapability.IsMobile;
        }

        private static void CheckIllegalChars(string illegalChars, ref string ch, ref bool replacedUnwantedChars)
        {
			Logger.LogDebug(">>> Entering CheckIllegalChars(illegalChars={IllegalChars},ch={Ch},replacedUnwantedChars={ReplacedUnwantedChars})", illegalChars, ch, replacedUnwantedChars);
            var resultingCh = new StringBuilder(ch.Length);
            foreach (char c in ch)
            {
			Logger.LogDebug("CheckIllegalChars(illegalChars={IllegalChars},ch={Ch},replacedUnwantedChars={ReplacedUnwantedChars}): resultingCh → {ResultingCh}", illegalChars, ch, replacedUnwantedChars, resultingCh);
                // ch could contain several chars from the pre-defined replacement list
                if (illegalChars.ToUpperInvariant().Contains(char.ToUpperInvariant(c)))
                {
                    replacedUnwantedChars = true;
                }
                else
                {
                    resultingCh.Append(c);
                }
            }

            ch = resultingCh.ToString();
			Logger.LogDebug("CheckIllegalChars(illegalChars={IllegalChars},ch={Ch},replacedUnwantedChars={ReplacedUnwantedChars}): ch → {Ch1}", illegalChars, ch, replacedUnwantedChars, ch);
			Logger.LogDebug("<<< Exiting CheckIllegalChars(illegalChars={IllegalChars},ch={Ch},replacedUnwantedChars={ReplacedUnwantedChars})", illegalChars, ch, replacedUnwantedChars);
        }

        private static void CheckCharsForReplace(FriendlyUrlOptions options, ref string ch, ref bool replacedUnwantedChars)
        {
			Logger.LogDebug(">>> Entering CheckCharsForReplace(options={Options},ch={Ch},replacedUnwantedChars={ReplacedUnwantedChars})", options, ch, replacedUnwantedChars);
            if (!options.ReplaceChars.ToUpperInvariant().Contains(ch.ToUpperInvariant()))
            {
                return;
            }

            // if not replacing spaces, which are implied
            if (ch != " ")
            {
                replacedUnwantedChars = true;
            }

            ch = options.PunctuationReplacement; // in list of replacment chars
			Logger.LogDebug("CheckCharsForReplace(options={Options},ch={Ch},replacedUnwantedChars={ReplacedUnwantedChars}): ch → {Ch1}", options, ch, replacedUnwantedChars, ch);

            // If we still have a space ensure it's encoded
            if (ch == " ")
            {
                ch = options.SpaceEncoding;
            }
			Logger.LogDebug("<<< Exiting CheckCharsForReplace(options={Options},ch={Ch},replacedUnwantedChars={ReplacedUnwantedChars})", options, ch, replacedUnwantedChars);
        }

        private static bool ValidateUrl(string url, int validateUrlForTabId, PortalSettings settings)
        {
            // Try and get a user by the url
            var user = UserController.GetUserByVanityUrl(settings.PortalId, url);
            bool isUnique = user == null;

            if (isUnique)
            {
                // Try and get a tab by the url
                int tabId = TabController.GetTabByTabPath(settings.PortalId, "//" + url, settings.CultureCode);
                isUnique = tabId == -1 || tabId == validateUrlForTabId;
            }

            // check whether have a tab which use the url.
            if (isUnique)
            {
                var friendlyUrlSettings = GetCurrentSettings(settings.PortalId);
                var tabs = TabController.Instance.GetTabsByPortal(settings.PortalId).AsList();

                // DNN-6492: if content localize enabled, only check tab names in current culture.
                if (settings.ContentLocalizationEnabled)
                {
                    tabs = tabs.Where(t => t.CultureCode == settings.CultureCode).ToList();
                }

                foreach (TabInfo tab in tabs)
                {
                    if (tab.TabID == validateUrlForTabId)
                    {
                        continue;
                    }

                    if (tab.TabUrls.Count == 0)
                    {
                        IPortalAliasInfo alias = settings.PortalAlias;
                        var baseUrl = Globals.AddHTTP(alias.HttpAlias) + "/Default.aspx?TabId=" + tab.TabID;
                        var path = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(
                            tab,
                            baseUrl,
                            Globals.glbDefaultPage,
                            alias.HttpAlias,
                            false,
                            friendlyUrlSettings,
                            Guid.Empty);

                        var tabUrl = path.Replace(Globals.AddHTTP(alias.HttpAlias), string.Empty);

                        if (tabUrl.Equals("/" + url, StringComparison.OrdinalIgnoreCase))
                        {
                            isUnique = false;
                            break;
                        }
                    }
                    else if (tab.TabUrls.Any(u => u.Url.Equals("/" + url, StringComparison.OrdinalIgnoreCase)))
                    {
                        isUnique = false;
                        break;
                    }
                }
            }

            return isUnique;
        }
    }
}
