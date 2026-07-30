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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="GlobalsImpl"/> class.</summary>
        public GlobalsImpl()
        {
			Logger.LogDebug(">>> Entering GlobalsImpl()");
            this.NavigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
			Logger.LogDebug("<<< Exiting GlobalsImpl()");
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
			Logger.LogDebug(">>> Entering GetSubFolderPath(strFileNamePath={StrFileNamePath},portalId={PortalId})", strFileNamePath, portalId);
			Logger.LogDebug("<<< Exiting GetSubFolderPath(strFileNamePath={StrFileNamePath},portalId={PortalId})", strFileNamePath, portalId);
            return Globals.GetSubFolderPath(strFileNamePath, portalId);
        }

        /// <inheritdoc />
        public string LinkClick(string link, int tabId, int moduleId)
        {
			Logger.LogDebug(">>> Entering LinkClick(link={Link},tabId={TabId},moduleId={ModuleId})", link, tabId, moduleId);
			Logger.LogDebug("<<< Exiting LinkClick(link={Link},tabId={TabId},moduleId={ModuleId})", link, tabId, moduleId);
            return Globals.LinkClick(link, tabId, moduleId);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public string LinkClick(string link, int tabID, int moduleID, bool trackClicks, bool forceDownload, int portalId, bool enableUrlLanguage, string portalGuid)
        {
			Logger.LogDebug("<<< Exiting LinkClick(link={Link},tabID={TabID},moduleID={ModuleID},trackClicks={TrackClicks},forceDownload={ForceDownload},portalId={PortalId},enableUrlLanguage={EnableUrlLanguage},portalGuid={PortalGuid})", link, tabID, moduleID, trackClicks, forceDownload, portalId, enableUrlLanguage, portalGuid);
            return Globals.LinkClick(link, tabID, moduleID, trackClicks, forceDownload, portalId, enableUrlLanguage, portalGuid);
        }

        /// <inheritdoc />
        public string ResolveUrl(string url)
        {
			Logger.LogDebug(">>> Entering ResolveUrl(url={Url})", url);
			Logger.LogDebug("<<< Exiting ResolveUrl(url={Url})", url);
            return Globals.ResolveUrl(url);
        }

        /// <inheritdoc />
        public bool IsHostTab(int tabId)
        {
			Logger.LogDebug(">>> Entering IsHostTab(tabId={TabId})", tabId);
			Logger.LogDebug("<<< Exiting IsHostTab(tabId={TabId})", tabId);
            return Globals.IsHostTab(tabId);
        }

        /// <inheritdoc />
        public string AddHTTP(string strURL)
        {
			Logger.LogDebug(">>> Entering AddHTTP(strURL={StrURL})", strURL);
			Logger.LogDebug("<<< Exiting AddHTTP(strURL={StrURL})", strURL);
            return Globals.AddHTTP(strURL);
        }

        /// <inheritdoc />
        public string GetPortalDomainName(string strPortalAlias, HttpRequest request, bool blnAddHTTP)
        {
			Logger.LogDebug(">>> Entering GetPortalDomainName(strPortalAlias={StrPortalAlias},request={Request},blnAddHTTP={BlnAddHTTP})", strPortalAlias, request, blnAddHTTP);
			Logger.LogDebug("<<< Exiting GetPortalDomainName(strPortalAlias={StrPortalAlias},request={Request},blnAddHTTP={BlnAddHTTP})", strPortalAlias, request, blnAddHTTP);
            return Globals.GetPortalDomainName(strPortalAlias, request, blnAddHTTP);
        }

        /// <inheritdoc />
        public string GetDomainName(Uri requestedUri)
        {
			Logger.LogDebug(">>> Entering GetDomainName(requestedUri={RequestedUri})", requestedUri);
			Logger.LogDebug("<<< Exiting GetDomainName(requestedUri={RequestedUri})", requestedUri);
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

			Logger.LogDebug("<<< Exiting GetDomainName(requestedUri={RequestedUri},parsePortNumber={ParsePortNumber})", requestedUri, parsePortNumber);
            return domainName.ToString();
        }

        /// <inheritdoc />
        public string FormatHelpUrl(string helpUrl, PortalSettings objPortalSettings, string name)
        {
			Logger.LogDebug(">>> Entering FormatHelpUrl(helpUrl={HelpUrl},objPortalSettings={ObjPortalSettings},name={Name})", helpUrl, objPortalSettings, name);
			Logger.LogDebug("<<< Exiting FormatHelpUrl(helpUrl={HelpUrl},objPortalSettings={ObjPortalSettings},name={Name})", helpUrl, objPortalSettings, name);
            return Globals.FormatHelpUrl(helpUrl, objPortalSettings, name);
        }

        /// <inheritdoc />
        public string FormatHelpUrl(string helpUrl, PortalSettings objPortalSettings, string name, string version)
        {
			Logger.LogDebug("<<< Exiting FormatHelpUrl(helpUrl={HelpUrl},objPortalSettings={ObjPortalSettings},name={Name},version={Version})", helpUrl, objPortalSettings, name, version);
            return Globals.FormatHelpUrl(helpUrl, objPortalSettings, name, version);
        }

        /// <inheritdoc />
        public string AccessDeniedURL()
        {
			Logger.LogDebug(">>> Entering AccessDeniedURL()");
			Logger.LogDebug("<<< Exiting AccessDeniedURL()");
            return Globals.AccessDeniedURL();
        }

        /// <inheritdoc />
        public string AccessDeniedURL(string message)
        {
			Logger.LogDebug("<<< Exiting AccessDeniedURL(message={Message})", message);
            return Globals.AccessDeniedURL(message);
        }

        /// <inheritdoc />
        public string LoginURL(string returnURL, bool @override)
        {
			Logger.LogDebug(">>> Entering LoginURL(returnURL={ReturnURL})", returnURL);
			Logger.LogDebug("<<< Exiting LoginURL(returnURL={ReturnURL})", returnURL);
            return Globals.LoginURL(returnURL, @override);
        }

        /// <inheritdoc />
        public string NavigateURL()
        {
			Logger.LogDebug(">>> Entering NavigateURL()");
			Logger.LogDebug("<<< Exiting NavigateURL()");
            return this.NavigationManager.NavigateURL();
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID)
        {
			Logger.LogDebug("<<< Exiting NavigateURL(tabID={TabID})", tabID);
            return this.NavigationManager.NavigateURL(tabID);
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID, bool isSuperTab)
        {
			Logger.LogDebug("<<< Exiting NavigateURL(tabID={TabID},isSuperTab={IsSuperTab})", tabID, isSuperTab);
            return this.NavigationManager.NavigateURL(tabID, isSuperTab);
        }

        /// <inheritdoc />
        public string NavigateURL(string controlKey)
        {
			Logger.LogDebug("<<< Exiting NavigateURL(controlKey={ControlKey})", controlKey);
            return this.NavigationManager.NavigateURL(controlKey);
        }

        /// <inheritdoc />
        public string NavigateURL(string controlKey, params string[] additionalParameters)
        {
			Logger.LogDebug("<<< Exiting NavigateURL(controlKey={ControlKey},additionalParameters={AdditionalParameters})", controlKey, additionalParameters);
            return this.NavigationManager.NavigateURL(controlKey, additionalParameters);
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID, string controlKey)
        {
			Logger.LogDebug("<<< Exiting NavigateURL(tabID={TabID},controlKey={ControlKey})", tabID, controlKey);
            return this.NavigationManager.NavigateURL(tabID, controlKey);
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID, string controlKey, params string[] additionalParameters)
        {
			Logger.LogDebug("<<< Exiting NavigateURL(tabID={TabID},controlKey={ControlKey},additionalParameters={AdditionalParameters})", tabID, controlKey, additionalParameters);
            return this.NavigationManager.NavigateURL(tabID, controlKey, additionalParameters);
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
			Logger.LogDebug("<<< Exiting NavigateURL(tabID={TabID},settings={Settings},controlKey={ControlKey},additionalParameters={AdditionalParameters})", tabID, settings, controlKey, additionalParameters);
            return this.NavigationManager.NavigateURL(tabID, settings, controlKey, additionalParameters);
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
			Logger.LogDebug("<<< Exiting NavigateURL(tabID={TabID},isSuperTab={IsSuperTab},settings={Settings},controlKey={ControlKey},additionalParameters={AdditionalParameters})", tabID, isSuperTab, settings, controlKey, additionalParameters);
            return this.NavigationManager.NavigateURL(tabID, isSuperTab, settings, controlKey, additionalParameters);
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, params string[] additionalParameters)
        {
			Logger.LogDebug("<<< Exiting NavigateURL(tabID={TabID},isSuperTab={IsSuperTab},settings={Settings},controlKey={ControlKey},language={Language},additionalParameters={AdditionalParameters})", tabID, isSuperTab, settings, controlKey, language, additionalParameters);
            return this.NavigationManager.NavigateURL(tabID, isSuperTab, settings, controlKey, language, additionalParameters);
        }

        /// <inheritdoc />
        public string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, string pageName, params string[] additionalParameters)
        {
			Logger.LogDebug("<<< Exiting NavigateURL(tabID={TabID},isSuperTab={IsSuperTab},settings={Settings},controlKey={ControlKey},language={Language},pageName={PageName},additionalParameters={AdditionalParameters})", tabID, isSuperTab, settings, controlKey, language, pageName, additionalParameters);
            return this.NavigationManager.NavigateURL(tabID, isSuperTab, settings, controlKey, language, pageName, additionalParameters);
        }

        /// <inheritdoc />
        public string FriendlyUrl(TabInfo tab, string path)
        {
			Logger.LogDebug(">>> Entering FriendlyUrl(tab={Tab},path={Path})", tab, path);
			Logger.LogDebug("<<< Exiting FriendlyUrl(tab={Tab},path={Path})", tab, path);
            return Globals.FriendlyUrl(tab, path);
        }

        /// <inheritdoc />
        public string FriendlyUrl(TabInfo tab, string path, string pageName)
        {
			Logger.LogDebug("<<< Exiting FriendlyUrl(tab={Tab},path={Path},pageName={PageName})", tab, path, pageName);
            return Globals.FriendlyUrl(tab, path, pageName);
        }

        /// <inheritdoc />
        public string FriendlyUrl(TabInfo tab, string path, PortalSettings settings)
        {
			Logger.LogDebug("<<< Exiting FriendlyUrl(tab={Tab},path={Path},settings={Settings})", tab, path, settings);
            return Globals.FriendlyUrl(tab, path, (IPortalSettings)settings);
        }

        /// <inheritdoc />
        public string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings settings)
        {
			Logger.LogDebug("<<< Exiting FriendlyUrl(tab={Tab},path={Path},pageName={PageName},settings={Settings})", tab, path, pageName, settings);
            return Globals.FriendlyUrl(tab, path, pageName, (IPortalSettings)settings);
        }

        /// <inheritdoc />
        public string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias)
        {
			Logger.LogDebug("<<< Exiting FriendlyUrl(tab={Tab},path={Path},pageName={PageName},portalAlias={PortalAlias})", tab, path, pageName, portalAlias);
            return Globals.FriendlyUrl(tab, path, pageName, portalAlias);
        }
    }
}
