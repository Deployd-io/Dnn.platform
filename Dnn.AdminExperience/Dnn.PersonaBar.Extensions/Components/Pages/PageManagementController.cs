// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Framework;

    public class PageManagementController : ServiceLocator<IPageManagementController, PageManagementController>, IPageManagementController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public static readonly string PageDateTimeFormat = "yyyy-MM-dd hh:mm tt";
        private readonly ITabController tabController;

        public PageManagementController()
        {
			Logger.LogDebug(">>> Entering PageManagementController()");
            this.tabController = TabController.Instance;
			Logger.LogDebug("<<< Exiting PageManagementController()");
			Logger.LogDebug("PageManagementController(): tabController → {TabController}", tabController);
			Logger.LogDebug("PageManagementController(): this.tabController → {Arg0}", this.tabController);
        }

        /// <inheritdoc />
        public string GetCreatedInfo(TabInfo tab)
        {
			Logger.LogDebug(">>> Entering GetCreatedInfo(tab={Tab})", tab);
            var createdBy = tab.CreatedByUser(PortalSettings.Current.PortalId);
            var displayName = Localization.GetString("System");
			Logger.LogDebug("GetCreatedInfo(tab={Tab}): createdBy → {CreatedBy}", tab, createdBy);
			Logger.LogDebug("GetCreatedInfo(tab={Tab}): displayName → {DisplayName}", tab, displayName);
            if (createdBy != null)
            {
                displayName = createdBy.DisplayName;
            }

			Logger.LogDebug("<<< Exiting GetCreatedInfo(tab={Tab})", tab);
            return displayName;
        }

        /// <inheritdoc />
        public bool TabHasChildren(TabInfo tabInfo)
        {
			Logger.LogDebug(">>> Entering TabHasChildren(tabInfo={TabInfo})", tabInfo);
            var children = TabController.GetTabsByParent(tabInfo.TabID, tabInfo.PortalID);
			Logger.LogDebug("TabHasChildren(tabInfo={TabInfo}): children → {Children}", tabInfo, children);
			Logger.LogDebug("<<< Exiting TabHasChildren(tabInfo={TabInfo})", tabInfo);
            return children != null && children.Count >= 1;
        }

        /// <inheritdoc />
        public string GetTabHierarchy(TabInfo tab)
        {
			Logger.LogDebug(">>> Entering GetTabHierarchy(tab={Tab})", tab);
            this.tabController.PopulateBreadCrumbs(ref tab);
			Logger.LogDebug("<<< Exiting GetTabHierarchy(tab={Tab})", tab);
            return tab.BreadCrumbs.Count == 1 ? string.Empty : string.Join(" > ", from t in tab.BreadCrumbs.Cast<TabInfo>().Take(tab.BreadCrumbs.Count - 1) select t.LocalizedTabName);
        }

        /// <inheritdoc />
        public string GetTabUrl(TabInfo tab)
        {
			Logger.LogDebug(">>> Entering GetTabUrl(tab={Tab})", tab);
            var url = string.Empty;

			Logger.LogDebug("GetTabUrl(tab={Tab}): url → {Url}", tab, url);
            if (tab.IsSuperTab || (Config.GetFriendlyUrlProvider() != "advanced"))
            {
                return url;
            }

            if (tab.TabUrls.Count > 0)
            {
                var tabUrl = tab.TabUrls.SingleOrDefault(t => t.IsSystem && t.HttpStatus == "200" && t.SeqNum == 0);

				Logger.LogDebug("GetTabUrl(tab={Tab}): tabUrl → {TabUrl}", tab, tabUrl);
                if (tabUrl != null)
                {
                    url = tabUrl.Url;
                }
            }

            if (string.IsNullOrEmpty(url) && tab.TabID > -1 && !tab.IsSuperTab)
            {
                var friendlyUrlSettings = new FriendlyUrlSettings(PortalSettings.Current.PortalId);
                var baseUrl = Globals.AddHTTP(PortalSettings.Current.PortalAlias.HTTPAlias) + "/Default.aspx?TabId=" + tab.TabID;
				Logger.LogDebug("GetTabUrl(tab={Tab}): friendlyUrlSettings → {FriendlyUrlSettings}", tab, friendlyUrlSettings);
				Logger.LogDebug("GetTabUrl(tab={Tab}): baseUrl → {BaseUrl}", tab, baseUrl);
                var path = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(
                    tab,
                    baseUrl,
                    Globals.glbDefaultPage,
                    PortalSettings.Current.PortalAlias.HTTPAlias,
                    false, // dnndev-27493 :we want any custom Urls that apply
                    friendlyUrlSettings,
                    Guid.Empty);

                url = path.Replace(Globals.AddHTTP(PortalSettings.Current.PortalAlias.HTTPAlias), string.Empty);
            }

			Logger.LogDebug("<<< Exiting GetTabUrl(tab={Tab})", tab);
            return url;
        }

        /// <inheritdoc />
        protected override Func<IPageManagementController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new PageManagementController();
        }
    }
}
