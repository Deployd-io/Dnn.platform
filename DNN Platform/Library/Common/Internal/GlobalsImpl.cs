// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Internal
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A collection of Dnn global methods and properties.</summary>
    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public class GlobalsImpl : IGlobals
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(GlobalsImpl));
        /// <summary>Initializes a new instance of the <see cref="GlobalsImpl"/> class.</summary>
        public GlobalsImpl()
        {
			Logger.LogError(">>> Entering GlobalsImpl()");
            this.NavigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
        }

        /// <inheritdoc />
        public string ApplicationPath
        {
            get { return Globals.ApplicationPath; }
        }

        /// <inheritdoc />
        public string HostMapPath
        {
            get { return Globals.HostMapPath; }
        }

        /// <summary>Gets a navigation manager to provide navigation services.</summary>
        protected INavigationManager NavigationManager { get; }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public string GetSubFolderPath(string strFileNamePath, int portalId)
        {
			Logger.LogError(">>> Entering GetSubFolderPath(strFileNamePath={StrFileNamePath},portalId={PortalId})", strFileNamePath, portalId);
            Logger.LogWarning("<<< Exiting GetSubFolderPath(strFileNamePath={StrFileNamePath},portalId={PortalId})", strFileNamePath, portalId);
            return Globals.GetSubFolderPath(strFileNamePath, portalId);
        }

        /// <inheritdoc />
        public string LinkClick(string link, int tabId, int moduleId)
        {
			Logger.LogError(">>> Entering LinkClick(link={Link},tabId={TabId},moduleId={ModuleId})", link, tabId, moduleId);
            Logger.LogWarning("<<< Exiting LinkClick(link={Link},tabId={TabId},moduleId={ModuleId})", link, tabId, moduleId);
            return Globals.LinkClick(link, tabId, moduleId);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public string LinkClick(string link, int tabID, int moduleID, bool trackClicks, bool forceDownload, int portalId, bool enableUrlLanguage, string portalGuid)
        {
            return Globals.LinkClick(link, tabID, moduleID, trackClicks, forceDownload, portalId, enableUrlLanguage, portalGuid);
        }

        /// <inheritdoc />
        public string ResolveUrl(string url)
        {
			Logger.LogError(">>> Entering ResolveUrl(url={Url})", url);
            Logger.LogWarning("<<< Exiting ResolveUrl(url={Url})", url);
            return Globals.ResolveUrl(url);
        }

        /// <inheritdoc />
        public bool IsHostTab(int tabId)
        {
			Logger.LogError(">>> Entering IsHostTab(tabId={TabId})", tabId);
            Logger.LogWarning("<<< Exiting IsHostTab(tabId={TabId})", tabId);
            return Globals.IsHostTab(tabId);
        }

        /// <inheritdoc />
        public string AddHTTP(string strURL)
        {
			Logger.LogError(">>> Entering AddHTTP(strURL={StrURL})", strURL);
            Logger.LogWarning("<<< Exiting AddHTTP(strURL={StrURL})", strURL);
            return Globals.AddHTTP(strURL);
        }

        /// <inheritdoc />
        public string GetPortalDomainName(string strPortalAlias, HttpRequest request, bool blnAddHTTP)
        {
			Logger.LogError(">>> Entering GetPortalDomainName(strPortalAlias={StrPortalAlias},request={Request},blnAddHTTP={BlnAddHTTP})", strPortalAlias, request, blnAddHTTP);
            Logger.LogWarning("<<< Exiting GetPortalDomainName(strPortalAlias={StrPortalAlias},request={Request},blnAddHTTP={BlnAddHTTP})", strPortalAlias, request, blnAddHTTP);
            return Globals.GetPortalDomainName(strPortalAlias, request, blnAddHTTP);
        }

        /// <inheritdoc />
        public string GetDomainName(Uri requestedUri)
        {
			Logger.LogError(">>> Entering GetDomainName(requestedUri={RequestedUri})", requestedUri);
			Logger.LogWarning("<<< Exiting GetDomainName(requestedUri={RequestedUri})", requestedUri);
			Logger.LogWarning("<<< Exiting GetDomainName(requestedUri={RequestedUri},parsePortNumber={ParsePortNumber})", requestedUri, parsePortNumber);
            return this.GetDomainName(requestedUri, false);
        }

        /// <inheritdoc />
        public string GetDomainName(Uri requestedUri, bool parsePortNumber)
        {
            var domainName = new StringBuilder();

            // split both URL separater, and parameter separator
            // We trim right of '?' so test for filename extensions can occur at END of URL-componenet.
            // Test:   'www.aspxforum.net'  should be returned as a valid domain name.
            // just consider left of '?' in URI
            // Binary, else '?' isn't taken literally; only interested in one (left) string
            string uri = requestedUri.ToString();
            string hostHeader = Config.GetSetting("HostHeader");
            if (!string.IsNullOrEmpty(hostHeader))
            {
                uri = uri.ToLowerInvariant().Replace(hostHeader.ToLowerInvariant(), string.Empty);
            }

            int queryIndex = uri.IndexOf("?", StringComparison.Ordinal);
            if (queryIndex > -1)
            {
                uri = uri.Substring(0, queryIndex);
            }

            string[] url = uri.Split('/');
            for (queryIndex = 2; queryIndex <= url.GetUpperBound(0); queryIndex++)
            {
                bool needExit = false;
                switch (url[queryIndex].ToLowerInvariant())
                {
                    case "":
                        continue;
                    case "admin":
                    case "controls":
                    case "desktopmodules":
                    case "mobilemodules":
                    case "premiummodules":
                    case "providers":
                    case "api":
                        needExit = true;
                        break;
                    default:
                        // exclude filenames ENDing in ".aspx" or ".axd" ---
                        //   we'll use reverse match,
                        //   - but that means we are checking position of left end of the match;
                        //   - and to do that, we need to ensure the string we test against is long enough;
                        if (url[queryIndex].Length >= ".aspx".Length)
                        {
                            if (url[queryIndex].LastIndexOf(".aspx", StringComparison.OrdinalIgnoreCase) == (url[queryIndex].Length - ".aspx".Length) ||
                                url[queryIndex].LastIndexOf(".axd", StringComparison.OrdinalIgnoreCase) == (url[queryIndex].Length - ".axd".Length) ||
                                url[queryIndex].LastIndexOf(".ashx", StringComparison.OrdinalIgnoreCase) == (url[queryIndex].Length - ".ashx".Length))
                            {
                                break;
                            }
                        }

                        // non of the exclusionary names found
                        domainName.Append((!string.IsNullOrEmpty(domainName.ToString()) ? "/" : string.Empty) + url[queryIndex]);
                        break;
                }

                if (needExit)
                {
                    break;
                }
            }

            if (parsePortNumber)
            {
                if (domainName.ToString().IndexOf(":", StringComparison.Ordinal) != -1)
                {
                    if (!Globals.UsePortNumber())
                    {
                        domainName = domainName.Replace(":" + requestedUri.Port, string.Empty);
                    }
                }
            }

            return domainName.ToString();
        }

        /// <inheritdoc />
        public string FormatHelpUrl(string helpUrl, PortalSettings objPortalSettings, string name)
        {
			Logger.LogError(">>> Entering FormatHelpUrl(helpUrl={HelpUrl},objPortalSettings={ObjPortalSettings},name={Name})", helpUrl, objPortalSettings, name);
            Logger.LogWarning("<<< Exiting FormatHelpUrl(helpUrl={HelpUrl},objPortalSettings={ObjPortalSettings},name={Name})", helpUrl, objPortalSettings, name);
            return Globals.FormatHelpUrl(helpUrl, objPortalSettings, name);
        }

        /// <inheritdoc />
        public string FormatHelpUrl(string helpUrl, PortalSettings objPortalSettings, string name, string version)
        {
            return Globals.FormatHelpUrl(helpUrl, objPortalSettings, name, version);
        }

        /// <inheritdoc />
        public string AccessDeniedURL()
        {
			Logger.LogError(">>> Entering AccessDeniedURL()");
            Logger.LogWarning("<<< Exiting AccessDeniedURL()");
            return Globals.AccessDeniedURL();
        }

        /// <inheritdoc />
        public string AccessDeniedURL(string message)
        {
            return Globals.AccessDeniedURL(message);
        }

        /// <inheritdoc />
        public string LoginURL(string returnURL, bool @override)
        {
			Logger.LogError(">>> Entering LoginURL(returnURL={ReturnURL})", returnURL);
            Logger.LogWarning("<<< Exiting LoginURL(returnURL={ReturnURL})", returnURL);
            return Globals.LoginURL(returnURL, @override);
        }

        /// <inheritdoc />
        public string NavigateURL()
        {
			Logger.LogError(">>> Entering NavigateURL()");
            Logger.LogWarning("<<< Exiting NavigateURL()");
            return this.NavigationManager.NavigateURL();
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID)
        {
            return this.NavigationManager.NavigateURL(tabID);
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID, bool isSuperTab)
        {
            return this.NavigationManager.NavigateURL(tabID, isSuperTab);
        }

        /// <inheritdoc />
        public string NavigateURL(string controlKey)
        {
            return this.NavigationManager.NavigateURL(controlKey);
        }

        /// <inheritdoc />
        public string NavigateURL(string controlKey, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(controlKey, additionalParameters);
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID, string controlKey)
        {
            return this.NavigationManager.NavigateURL(tabID, controlKey);
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID, string controlKey, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(tabID, controlKey, additionalParameters);
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(tabID, settings, controlKey, additionalParameters);
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(tabID, isSuperTab, settings, controlKey, additionalParameters);
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(tabID, isSuperTab, settings, controlKey, language, additionalParameters);
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, string pageName, params string[] additionalParameters)
        {
            return this.NavigationManager.NavigateURL(tabID, isSuperTab, settings, controlKey, language, pageName, additionalParameters);
        }

        /// <inheritdoc />
        public string FriendlyUrl(TabInfo tab, string path)
        {
			Logger.LogError(">>> Entering FriendlyUrl(tab={Tab},path={Path})", tab, path);
            Logger.LogWarning("<<< Exiting FriendlyUrl(tab={Tab},path={Path})", tab, path);
            return Globals.FriendlyUrl(tab, path);
        }

        /// <inheritdoc />
        public string FriendlyUrl(TabInfo tab, string path, string pageName)
        {
            return Globals.FriendlyUrl(tab, path, pageName);
        }

        /// <inheritdoc />
        public string FriendlyUrl(TabInfo tab, string path, PortalSettings settings)
        {
            return Globals.FriendlyUrl(tab, path, (IPortalSettings)settings);
        }

        /// <inheritdoc />
        public string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings settings)
        {
            return Globals.FriendlyUrl(tab, path, pageName, (IPortalSettings)settings);
        }

        /// <inheritdoc />
        public string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias)
        {
            return Globals.FriendlyUrl(tab, path, pageName, portalAlias);
        }
    }
}
