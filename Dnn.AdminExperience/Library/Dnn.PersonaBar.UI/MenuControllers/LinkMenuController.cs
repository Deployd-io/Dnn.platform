// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.MenuControllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="IMenuItemController"/> for menu items that link to other pages.</summary>
    /// <param name="navigationManager">The navigation manager.</param>
    /// <param name="hostSettings">The host settings.</param>
    public class LinkMenuController(INavigationManager navigationManager, IHostSettings hostSettings)
        : IMenuItemController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LinkMenuController));
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="LinkMenuController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public LinkMenuController()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering LinkMenuController()");
		Logger.LogDebug("<<< Exiting LinkMenuController()");
        }

        /// <summary>Gets the navigation manager.</summary>
        protected INavigationManager NavigationManager { get; } = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();

        /// <inheritdoc />
        public void UpdateParameters(MenuItem menuItem)
        {
			Logger.LogDebug(">>> Entering UpdateParameters(menuItem={MenuItem})", menuItem);
            if (!this.Visible(menuItem))
            {
                return;
            }

            var query = GetPathQuery(menuItem);
			Logger.LogDebug("UpdateParameters(menuItem={MenuItem}): query → {Query}", menuItem, query);

            int tabId, portalId;
            if (query.TryGetValue("path", out var path))
            {
                portalId = query.TryGetValue("portalId", out var queryPortalId) ? Convert.ToInt32(queryPortalId, CultureInfo.InvariantCulture) : PortalSettings.Current.PortalId;
                tabId = TabController.GetTabByTabPath(this.hostSettings, portalId, path, string.Empty);
            }
            else
			Logger.LogDebug("UpdateParameters(menuItem={MenuItem}): tabId → {TabId}", menuItem, tabId);
            {
                portalId = Convert.ToInt32(query["portalId"], CultureInfo.InvariantCulture);
                tabId = Convert.ToInt32(query["tabId"], CultureInfo.InvariantCulture);
            }

            var tabUrl = this.NavigationManager.NavigateURL(tabId, portalId == Null.NullInteger);
            var alias = Globals.AddHTTP(((IPortalAliasInfo)PortalSettings.Current.PortalAlias).HttpAlias);
			Logger.LogDebug("UpdateParameters(menuItem={MenuItem}): tabUrl → {TabUrl}", menuItem, tabUrl);
			Logger.LogDebug("UpdateParameters(menuItem={MenuItem}): alias → {Alias}", menuItem, alias);
            tabUrl = tabUrl.Replace(alias, string.Empty).TrimStart('/');

            menuItem.Link = tabUrl;
			Logger.LogDebug("UpdateParameters(menuItem={MenuItem}): menuItem.Link → {Arg1}", menuItem, menuItem.Link);
			Logger.LogDebug("<<< Exiting UpdateParameters(menuItem={MenuItem})", menuItem);
        }

        /// <inheritdoc />
        public bool Visible(MenuItem menuItem)
        {
			Logger.LogDebug(">>> Entering Visible(menuItem={MenuItem})", menuItem);
            var query = GetPathQuery(menuItem);
            if (PortalSettings.Current == null || query == null)
			Logger.LogDebug("Visible(menuItem={MenuItem}): query → {Query}", menuItem, query);
            {
                return false;
            }

            if (query.TryGetValue("sku", out var sku) && !string.IsNullOrEmpty(sku))
            {
                if (DotNetNukeContext.Current.Application.SKU != query["sku"])
                {
                    return false;
                }
            }

            int tabId, portalId;
            if (query.TryGetValue("path", out var path) && !string.IsNullOrEmpty(path))
            {
                portalId = query.TryGetValue("portalId", out var queryPortalId) ? Convert.ToInt32(queryPortalId, CultureInfo.InvariantCulture) : PortalSettings.Current.PortalId;
                tabId = TabController.GetTabByTabPath(this.hostSettings, portalId, path, string.Empty);

				Logger.LogDebug("Visible(menuItem={MenuItem}): portalId → {PortalId}", menuItem, portalId);
                if (tabId == Null.NullInteger)
				Logger.LogDebug("Visible(menuItem={MenuItem}): tabId → {TabId}", menuItem, tabId);
                {
                    return false;
                }
            }
            else
            {
                if (!query.TryGetValue("portalId", out var portalIdQuery) || !query.TryGetValue("tabId", out var tabIdQuery))
                {
                    return false;
                }

                portalId = Convert.ToInt32(portalIdQuery, CultureInfo.InvariantCulture);
                tabId = Convert.ToInt32(tabIdQuery, CultureInfo.InvariantCulture);
            }

            var tab = TabController.Instance.GetTab(tabId, portalId);
			Logger.LogDebug("Visible(menuItem={MenuItem}): tab → {Tab}", menuItem, tab);
			Logger.LogDebug("<<< Exiting Visible(menuItem={MenuItem})", menuItem);
            return (portalId == Null.NullInteger || portalId == PortalSettings.Current.PortalId)
                   && tab is { IsDeleted: false, DisableLink: false, IsVisible: true };
        }

        /// <inheritdoc />
        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            return null;
        }

        private static Dictionary<string, string> GetPathQuery(MenuItem menuItem)
        {
            var path = menuItem.Path;
            if (!path.Contains("?"))
            {
                return null;
            }

            return path.Substring(path.IndexOf("?", StringComparison.InvariantCultureIgnoreCase) + 1)
                .Split('&')
                .Select(p => p.Split('='))
                .Where(q => q.Length == 2 && !string.IsNullOrEmpty(q[0]) && !string.IsNullOrEmpty(q[1]))
                .ToDictionary(q => q[0], q => q[1]);
        }
    }
}
