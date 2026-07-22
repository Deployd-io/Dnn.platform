// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Controllers
{
    using System;
    using System.Linq;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Search.Entities;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>Search Result Controller for Tab Indexer.</summary>
    [Serializable]
    public class TabResultController : BaseResultController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string LocalizedResxFile = "~/DesktopModules/Admin/SearchResults/App_LocalResources/SearchableModules.resx";

        /// <inheritdoc />
        public override string LocalizedSearchTypeName => Localization.GetString("Crawler_tab", LocalizedResxFile);

        /// <inheritdoc />
        public override bool HasViewPermission(SearchResult searchResult)
        {
			Logger.LogDebug(">>> Entering HasViewPermission(searchResult={SearchResult})", searchResult);
            var viewable = true;

			Logger.LogDebug("HasViewPermission(searchResult={SearchResult}): viewable → {Viewable}", searchResult, viewable);
            if (searchResult.TabId > 0)
            {
                var tab = TabController.Instance.GetTab(searchResult.TabId, searchResult.PortalId, false);
                viewable = tab != null && !tab.IsDeleted && !tab.DisableLink && TabPermissionController.CanViewPage(tab);
				Logger.LogDebug("HasViewPermission(searchResult={SearchResult}): tab → {Tab}", searchResult, tab);
            }

			Logger.LogDebug("<<< Exiting HasViewPermission(searchResult={SearchResult})", searchResult);
            return viewable;
        }

        /// <inheritdoc />
        public override string GetDocUrl(SearchResult searchResult)
        {
			Logger.LogDebug(">>> Entering GetDocUrl(searchResult={SearchResult})", searchResult);
            var url = Localization.GetString("SEARCH_NoLink");

			Logger.LogDebug("GetDocUrl(searchResult={SearchResult}): url → {Url}", searchResult, url);
            var tab = TabController.Instance.GetTab(searchResult.TabId, searchResult.PortalId, false);
            if (TabPermissionController.CanViewPage(tab))
			Logger.LogDebug("GetDocUrl(searchResult={SearchResult}): tab → {Tab}", searchResult, tab);
            {
                if (searchResult.PortalId != PortalSettings.Current.PortalId)
                {
                    var alias = PortalAliasController.Instance.GetPortalAliasesByPortalId(searchResult.PortalId)
                                    .OrderByDescending(a => a.IsPrimary)
                                    .FirstOrDefault();

                    if (alias != null)
                    {
                        var portalSettings = new PortalSettings(searchResult.PortalId, alias);
                        url = TestableGlobals.Instance.NavigateURL(searchResult.TabId, portalSettings, string.Empty, searchResult.QueryString);
						Logger.LogDebug("GetDocUrl(searchResult={SearchResult}): portalSettings → {PortalSettings}", searchResult, portalSettings);
                    }
                }
                else
                {
                    url = TestableGlobals.Instance.NavigateURL(searchResult.TabId, string.Empty, searchResult.QueryString);
                }
            }

			Logger.LogDebug("<<< Exiting GetDocUrl(searchResult={SearchResult})", searchResult);
            return url;
        }
    }
}
