// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Security
{
    using System;
    using System.Security.Permissions;

    using Dnn.PersonaBar.Library.Model;
    using Dnn.PersonaBar.Library.Permissions;
    using Dnn.PersonaBar.Pages.Services.Dto;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem;
    using Newtonsoft.Json.Linq;

    public class SecurityService : ISecurityService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly ITabController tabController;

        /// <summary>Initializes a new instance of the <see cref="SecurityService"/> class.</summary>
        public SecurityService()
        {
			Logger.LogDebug(">>> Entering SecurityService()");
            this.tabController = TabController.Instance;
			Logger.LogDebug("<<< Exiting SecurityService()");
			Logger.LogDebug("SecurityService(): tabController → {TabController}", tabController);
			Logger.LogDebug("SecurityService(): this.tabController → {Arg0}", this.tabController);
        }

        public static ISecurityService Instance
        {
            get
            {
                var controller = ComponentFactory.GetComponent<ISecurityService>("PagesSecurityService");
                if (controller == null)
                {
                    ComponentFactory.RegisterComponent<ISecurityService, SecurityService>("PagesSecurityService");
                }

                return ComponentFactory.GetComponent<ISecurityService>("PagesSecurityService");
            }
        }

        /// <inheritdoc />
        public virtual bool IsPageAdminUser()
        {
			Logger.LogDebug(">>> Entering IsPageAdminUser()");
            var user = UserController.Instance.GetCurrentUserInfo();
			Logger.LogDebug("IsPageAdminUser(): user → {User}", user);
			Logger.LogDebug("<<< Exiting IsPageAdminUser()");
            return user.IsSuperUser || user.IsInRole(PortalSettings.Current?.AdministratorRoleName);
        }

        /// <inheritdoc />
        public virtual bool IsVisible(MenuItem menuItem)
        {
			Logger.LogDebug(">>> Entering IsVisible(menuItem={MenuItem})", menuItem);
			Logger.LogDebug("<<< Exiting IsVisible(menuItem={MenuItem})", menuItem);
            return this.IsPageAdminUser() || this.CanViewPageList(menuItem.MenuId);
        }

        /// <inheritdoc />
        public virtual JObject GetCurrentPagePermissions()
        {
			Logger.LogDebug(">>> Entering GetCurrentPagePermissions()");
            var permissions = new JObject
            {
                { "addContentToPage", TabPermissionController.CanAddContentToPage() },
                { "addPage", TabPermissionController.CanAddPage() },
                { "adminPage", TabPermissionController.CanAdminPage() },
                { "copyPage", TabPermissionController.CanCopyPage() },
                { "deletePage", TabPermissionController.CanDeletePage() },
                { "exportPage", TabPermissionController.CanExportPage() },
                { "importPage", TabPermissionController.CanImportPage() },
                { "managePage", TabPermissionController.CanManagePage() },
            };

			Logger.LogDebug("<<< Exiting GetCurrentPagePermissions()");
            return permissions;
        }

        /// <inheritdoc />
        public virtual JObject GetPagePermissions(TabInfo tab)
        {
            var permissions = new JObject
            {
                { "addContentToPage", TabPermissionController.CanAddContentToPage(tab) },
                { "addPage", TabPermissionController.CanAddPage(tab) },
                { "adminPage", TabPermissionController.CanAdminPage(tab) },
                { "copyPage", TabPermissionController.CanCopyPage(tab) },
                { "deletePage", TabPermissionController.CanDeletePage(tab) },
                { "exportPage", TabPermissionController.CanExportPage(tab) },
                { "importPage", TabPermissionController.CanImportPage(tab) },
                { "managePage", TabPermissionController.CanManagePage(tab) },
            };

            return permissions;
        }

        /// <inheritdoc />
        public virtual bool CanAdminPage(int tabId)
        {
            return this.IsPageAdminUser() || TabPermissionController.CanAdminPage(this.GetTabById(tabId));
        }

        /// <inheritdoc />
        public virtual bool CanManagePage(int tabId)
        {
            return this.CanAdminPage(tabId) || TabPermissionController.CanManagePage(this.GetTabById(tabId));
        }

        /// <inheritdoc />
        public virtual bool CanDeletePage(int tabId)
        {
            return this.CanAdminPage(tabId) || TabPermissionController.CanDeletePage(this.GetTabById(tabId));
        }

        /// <inheritdoc />
        public virtual bool CanAddPage(int tabId)
        {
            return this.CanAdminPage(tabId) || TabPermissionController.CanAddPage(this.GetTabById(tabId));
        }

        /// <inheritdoc />
        public virtual bool CanCopyPage(int tabId)
        {
            return this.CanAdminPage(tabId) || TabPermissionController.CanCopyPage(this.GetTabById(tabId));
        }

        /// <inheritdoc />
        public virtual bool CanExportPage(int tabId)
        {
            return this.CanAdminPage(tabId) || TabPermissionController.CanExportPage(this.GetTabById(tabId));
        }

        /// <inheritdoc />
        public virtual bool CanViewPageList(int menuId)
        {
			Logger.LogDebug(">>> Entering CanViewPageList(menuId={MenuId})", menuId);
            var permissions = MenuPermissionController.GetMenuPermissions(PortalSettings.Current.PortalId, menuId);
			Logger.LogDebug("CanViewPageList(menuId={MenuId}): permissions → {Permissions}", menuId, permissions);
			Logger.LogDebug("<<< Exiting CanViewPageList(menuId={MenuId})", menuId);
            return MenuPermissionController.HasMenuPermission(new MenuPermissionCollection(permissions), "VIEW_PAGE_LIST") || IsPageAdmin();
        }

        /// <inheritdoc />
        public virtual bool CanSavePageDetails(PageSettings pageSettings)
        {
			Logger.LogDebug(">>> Entering CanSavePageDetails(pageSettings={PageSettings})", pageSettings);
            var tabId = pageSettings.TabId;
            var pageType = pageSettings.PageType;
            var parentId = pageSettings.ParentId ?? 0;
            var creatingPage = tabId <= 0;
            var updatingPage = tabId > 0;
            var creatingTemplate = tabId <= 0 && pageSettings.TemplateTabId > 0 && pageType == "template";
            var duplicatingPage = tabId <= 0 && pageSettings.TemplateTabId > 0;
			Logger.LogDebug("CanSavePageDetails(pageSettings={PageSettings}): creatingTemplate → {CreatingTemplate}", pageSettings, creatingTemplate);
            var updatingParentPage = false;
            if (updatingPage)
			Logger.LogDebug("CanSavePageDetails(pageSettings={PageSettings}): updatingParentPage → {UpdatingParentPage}", pageSettings, updatingParentPage);
            {
                var tab = TabController.Instance.GetTab(tabId, PortalSettings.Current.PortalId);
                if (tab != null && tab.ParentId != parentId)
				Logger.LogDebug("CanSavePageDetails(pageSettings={PageSettings}): tab → {Tab}", pageSettings, tab);
                {
                    updatingParentPage = true;
                }
            }

				Logger.LogDebug("<<< Exiting CanSavePageDetails(pageSettings={PageSettings})", pageSettings);
            return
                this.IsPageAdminUser() ||
                (creatingPage && this.CanAddPage(parentId) && !creatingTemplate) ||
                (creatingTemplate && this.CanExportPage(pageSettings.TemplateTabId)) ||
                (updatingPage && this.CanManagePage(tabId) && !updatingParentPage) ||
                (updatingParentPage && this.CanManagePage(tabId) && this.CanAddPage(parentId)) ||
                (duplicatingPage && this.CanCopyPage(pageSettings.TemplateTabId) && this.CanAddPage(parentId));
        }

        /// <inheritdoc />
        public virtual bool IsAdminHostSystemPage()
        {
			Logger.LogDebug(">>> Entering IsAdminHostSystemPage()");
			Logger.LogDebug("<<< Exiting IsAdminHostSystemPage()");
            return PortalSettings.Current.ActiveTab.IsSystem || PortalSettings.Current.ActiveTab.IsSuperTab ||
                PortalSettings.Current.ActiveTab.ParentId == PortalSettings.Current.AdminTabId || PortalSettings.Current.ActiveTab.TabID == PortalSettings.Current.AdminTabId;
        }

        private static bool IsPageAdmin()
        {
			Logger.LogDebug(">>> Entering IsPageAdmin()");
			Logger.LogDebug("<<< Exiting IsPageAdmin()");
            return ////TabPermissionController.CanAddContentToPage() ||
                TabPermissionController.CanAddPage()
                || TabPermissionController.CanAdminPage()
                || TabPermissionController.CanCopyPage()
                || TabPermissionController.CanDeletePage()
                || TabPermissionController.CanExportPage()
                || TabPermissionController.CanImportPage()
                || TabPermissionController.CanManagePage();
        }

        private TabInfo GetTabById(int pageId)
        {
			Logger.LogDebug(">>> Entering GetTabById(pageId={PageId})", pageId);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
			Logger.LogDebug("GetTabById(pageId={PageId}): portalSettings → {PortalSettings}", pageId, portalSettings);
			Logger.LogDebug("<<< Exiting GetTabById(pageId={PageId})", pageId);
            return pageId <= 0 ? new TabInfo { PortalID = portalSettings.PortalId, } : this.tabController.GetTab(pageId, portalSettings.PortalId, false);
        }
    }
}
