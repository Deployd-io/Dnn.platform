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
        private readonly ITabController tabController;

        /// <summary>Initializes a new instance of the <see cref="SecurityService"/> class.</summary>
        public SecurityService()
        {
			eventLogger.LogDebug(">>> Entering SecurityService()");
            this.tabController = TabController.Instance;
			eventLogger.LogDebug("SecurityService(): tabController → {TabController}", tabController);
			eventLogger.LogDebug("SecurityService(): this.tabController → {Arg0}", this.tabController);
			eventLogger.LogDebug("<<< Exiting SecurityService()");
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
			eventLogger.LogDebug(">>> Entering IsPageAdminUser()");
            var user = UserController.Instance.GetCurrentUserInfo();
			eventLogger.LogDebug("IsPageAdminUser(): user → {User}", user);
			eventLogger.LogDebug("<<< Exiting IsPageAdminUser()");
            return user.IsSuperUser || user.IsInRole(PortalSettings.Current?.AdministratorRoleName);
        }

        /// <inheritdoc />
        public virtual bool IsVisible(MenuItem menuItem)
        {
			eventLogger.LogDebug(">>> Entering IsVisible(menuItem={MenuItem})", menuItem);
			eventLogger.LogDebug("<<< Exiting IsVisible(menuItem={MenuItem})", menuItem);
            return this.IsPageAdminUser() || this.CanViewPageList(menuItem.MenuId);
        }

        /// <inheritdoc />
        public virtual JObject GetCurrentPagePermissions()
        {
			eventLogger.LogDebug(">>> Entering GetCurrentPagePermissions()");
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

			eventLogger.LogDebug("<<< Exiting GetCurrentPagePermissions()");
            return permissions;
        }

        /// <inheritdoc />
        public virtual JObject GetPagePermissions(TabInfo tab)
        {
			eventLogger.LogDebug(">>> Entering GetPagePermissions(tab={Tab})", tab);
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

			eventLogger.LogDebug("<<< Exiting GetPagePermissions(tab={Tab})", tab);
            return permissions;
        }

        /// <inheritdoc />
        public virtual bool CanAdminPage(int tabId)
        {
			eventLogger.LogDebug(">>> Entering CanAdminPage(tabId={TabId})", tabId);
			eventLogger.LogDebug("<<< Exiting CanAdminPage(tabId={TabId})", tabId);
            return this.IsPageAdminUser() || TabPermissionController.CanAdminPage(this.GetTabById(tabId));
        }

        /// <inheritdoc />
        public virtual bool CanManagePage(int tabId)
        {
			eventLogger.LogDebug(">>> Entering CanManagePage(tabId={TabId})", tabId);
			eventLogger.LogDebug("<<< Exiting CanManagePage(tabId={TabId})", tabId);
            return this.CanAdminPage(tabId) || TabPermissionController.CanManagePage(this.GetTabById(tabId));
        }

        /// <inheritdoc />
        public virtual bool CanDeletePage(int tabId)
        {
			eventLogger.LogDebug(">>> Entering CanDeletePage(tabId={TabId})", tabId);
			eventLogger.LogDebug("<<< Exiting CanDeletePage(tabId={TabId})", tabId);
            return this.CanAdminPage(tabId) || TabPermissionController.CanDeletePage(this.GetTabById(tabId));
        }

        /// <inheritdoc />
        public virtual bool CanAddPage(int tabId)
        {
			eventLogger.LogDebug(">>> Entering CanAddPage(tabId={TabId})", tabId);
			eventLogger.LogDebug("<<< Exiting CanAddPage(tabId={TabId})", tabId);
            return this.CanAdminPage(tabId) || TabPermissionController.CanAddPage(this.GetTabById(tabId));
        }

        /// <inheritdoc />
        public virtual bool CanCopyPage(int tabId)
        {
			eventLogger.LogDebug(">>> Entering CanCopyPage(tabId={TabId})", tabId);
			eventLogger.LogDebug("<<< Exiting CanCopyPage(tabId={TabId})", tabId);
            return this.CanAdminPage(tabId) || TabPermissionController.CanCopyPage(this.GetTabById(tabId));
        }

        /// <inheritdoc />
        public virtual bool CanExportPage(int tabId)
        {
			eventLogger.LogDebug(">>> Entering CanExportPage(tabId={TabId})", tabId);
			eventLogger.LogDebug("<<< Exiting CanExportPage(tabId={TabId})", tabId);
            return this.CanAdminPage(tabId) || TabPermissionController.CanExportPage(this.GetTabById(tabId));
        }

        /// <inheritdoc />
        public virtual bool CanViewPageList(int menuId)
        {
			eventLogger.LogDebug(">>> Entering CanViewPageList(menuId={MenuId})", menuId);
            var permissions = MenuPermissionController.GetMenuPermissions(PortalSettings.Current.PortalId, menuId);
			eventLogger.LogDebug("CanViewPageList(menuId={MenuId}): permissions → {Permissions}", menuId, permissions);
			eventLogger.LogDebug("<<< Exiting CanViewPageList(menuId={MenuId})", menuId);
            return MenuPermissionController.HasMenuPermission(new MenuPermissionCollection(permissions), "VIEW_PAGE_LIST") || IsPageAdmin();
        }

        /// <inheritdoc />
        public virtual bool CanSavePageDetails(PageSettings pageSettings)
        {
			eventLogger.LogDebug(">>> Entering CanSavePageDetails(pageSettings={PageSettings})", pageSettings);
            var tabId = pageSettings.TabId;
            var pageType = pageSettings.PageType;
            var parentId = pageSettings.ParentId ?? 0;
            var creatingPage = tabId <= 0;
            var updatingPage = tabId > 0;
            var creatingTemplate = tabId <= 0 && pageSettings.TemplateTabId > 0 && pageType == "template";
			eventLogger.LogDebug("CanSavePageDetails(pageSettings={PageSettings}): creatingTemplate → {CreatingTemplate}", pageSettings, creatingTemplate);
            var duplicatingPage = tabId <= 0 && pageSettings.TemplateTabId > 0;
            var updatingParentPage = false;
			eventLogger.LogDebug("CanSavePageDetails(pageSettings={PageSettings}): updatingParentPage → {UpdatingParentPage}", pageSettings, updatingParentPage);
            if (updatingPage)
            {
                var tab = TabController.Instance.GetTab(tabId, PortalSettings.Current.PortalId);
				eventLogger.LogDebug("CanSavePageDetails(pageSettings={PageSettings}): tab → {Tab}", pageSettings, tab);
                if (tab != null && tab.ParentId != parentId)
                {
                    updatingParentPage = true;
                }
            }

				eventLogger.LogDebug("<<< Exiting CanSavePageDetails(pageSettings={PageSettings})", pageSettings);
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
			eventLogger.LogDebug(">>> Entering IsAdminHostSystemPage()");
			eventLogger.LogDebug("<<< Exiting IsAdminHostSystemPage()");
            return PortalSettings.Current.ActiveTab.IsSystem || PortalSettings.Current.ActiveTab.IsSuperTab ||
                PortalSettings.Current.ActiveTab.ParentId == PortalSettings.Current.AdminTabId || PortalSettings.Current.ActiveTab.TabID == PortalSettings.Current.AdminTabId;
        }

        private static bool IsPageAdmin()
        {
			eventLogger.LogDebug(">>> Entering IsPageAdmin()");
			eventLogger.LogDebug("<<< Exiting IsPageAdmin()");
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
			eventLogger.LogDebug(">>> Entering GetTabById(pageId={PageId})", pageId);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
			eventLogger.LogDebug("GetTabById(pageId={PageId}): portalSettings → {PortalSettings}", pageId, portalSettings);
			eventLogger.LogDebug("<<< Exiting GetTabById(pageId={PageId})", pageId);
            return pageId <= 0 ? new TabInfo { PortalID = portalSettings.PortalId, } : this.tabController.GetTab(pageId, portalSettings.PortalId, false);
        }
    }
}
