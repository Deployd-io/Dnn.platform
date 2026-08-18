// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An error code for a <see cref="DotNetNukeException"/>.</summary>
    public enum DotNetNukeErrorCode
    {
        /// <summary>Not set.</summary>
        NotSet = 0,

        /// <summary>The page already exists.</summary>
        PageExists = 1,

        /// <summary>THe page name is missing.</summary>
        PageNameRequired = 2,

        /// <summary>The page name is invalid.</summary>
        PageNameInvalid = 3,

        /// <summary>Failed deserializing the page's modules.</summary>
        DeserializePanesFailed = 4,

        /// <summary>The page's parent would also be a descendant of the page.</summary>
        PageCircularReference = 5,

        /// <summary>The parent page is invalid.</summary>
        ParentTabInvalid = 6,

        /// <summary>The user does not have permission to edit the page.</summary>
        PageEditorPermissionError = 7,

        /// <summary>Cannot move a tab before or after the host tab.</summary>
        HostBeforeAfterError = 8,

        /// <summary>The page's URL would conflict with a portal alias.</summary>
        DuplicateWithAlias = 9,
    }

    /// <summary>The relative location of a tab.</summary>
    public enum TabRelativeLocation
    {
        /// <summary>Location not set.</summary>
        NOTSET = 0,

        /// <summary>This tab is before the other tab.</summary>
        BEFORE = 1,

        /// <summary>THis tab is after the other tab.</summary>
        AFTER = 2,

        /// <summary>This tab is a child of the other tab.</summary>
        CHILD = 3,
    }

    /// <summary>Manages the old ribbon bar.</summary>
    public partial class RibbonBarManager
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RibbonBarManager));

        /// <summary>Initializes tab info user to add a new tab/page.</summary>
        /// <returns>The new <see cref="TabInfo"/> instance.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload with IPermissionDefinitionService")]
        public static partial TabInfo InitTabInfoObject()
            => InitTabInfoObject(Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>());

        /// <summary>Initializes tab info user to add a new tab/page.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <returns>The new <see cref="TabInfo"/> instance.</returns>
        public static TabInfo InitTabInfoObject(IPermissionDefinitionService permissionDefinitionService)
            => InitTabInfoObject(permissionDefinitionService, null, TabRelativeLocation.AFTER);

        /// <summary>Initializes tab info user to add a new tab/page after the given <paramref name="relativeToTab"/>.</summary>
        /// <param name="relativeToTab">The page/tab to which the new page/tab should be relative.</param>
        /// <returns>The new <see cref="TabInfo"/> instance.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload with IPermissionDefinitionService")]
        public static partial TabInfo InitTabInfoObject(TabInfo relativeToTab)
            => InitTabInfoObject(Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>(), relativeToTab);

        /// <summary>Initializes tab info user to add a new tab/page after the given <paramref name="relativeToTab"/>.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="relativeToTab">The page/tab to which the new page/tab should be relative.</param>
        /// <returns>The new <see cref="TabInfo"/> instance.</returns>
        public static TabInfo InitTabInfoObject(IPermissionDefinitionService permissionDefinitionService, TabInfo relativeToTab)
            => InitTabInfoObject(permissionDefinitionService, relativeToTab, TabRelativeLocation.AFTER);

        /// <summary>Initializes tab info user to add a new tab/page, relative to the given <paramref name="relativeToTab"/>.</summary>
        /// <param name="relativeToTab">The page/tab to which the new page/tab should be relative.</param>
        /// <param name="location">The new page/tab's location relative to <paramref name="relativeToTab"/>.</param>
        /// <returns>The new <see cref="TabInfo"/> instance.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload with IPermissionDefinitionService")]
        public static partial TabInfo InitTabInfoObject(TabInfo relativeToTab, TabRelativeLocation location)
            => InitTabInfoObject(Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>(), relativeToTab, location);

        /// <summary>Initializes tab info user to add a new tab/page, relative to the given <paramref name="relativeToTab"/>.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="relativeToTab">The page/tab to which the new page/tab should be relative.</param>
        /// <param name="location">The new page/tab's location relative to <paramref name="relativeToTab"/>.</param>
        /// <returns>The new <see cref="TabInfo"/> instance.</returns>
        public static TabInfo InitTabInfoObject(IPermissionDefinitionService permissionDefinitionService, TabInfo relativeToTab, TabRelativeLocation location)
        {
			Logger.LogDebug(">>> Entering InitTabInfoObject(permissionDefinitionService={PermissionDefinitionService},relativeToTab={RelativeToTab},location={Location})", permissionDefinitionService, relativeToTab, location);
            if (relativeToTab == null)
            {
                if ((PortalSettings.Current != null) && (PortalSettings.Current.ActiveTab != null))
                {
                    relativeToTab = PortalSettings.Current.ActiveTab;
                }
            }

            var newTab = new TabInfo
            {
                TabID = Null.NullInteger,
                TabName = string.Empty,
                Title = string.Empty,
                IsVisible = false,
                DisableLink = false,
                IsDeleted = false,
                IsSecure = false,
                PermanentRedirect = false,
            };

            TabInfo parentTab = GetParentTab(relativeToTab, location);
			Logger.LogDebug("InitTabInfoObject(permissionDefinitionService={PermissionDefinitionService},relativeToTab={RelativeToTab},location={Location}): parentTab → {ParentTab}", permissionDefinitionService, relativeToTab, location, parentTab);

            if (parentTab != null)
            {
                newTab.PortalID = parentTab.PortalID;
				Logger.LogDebug("InitTabInfoObject(permissionDefinitionService={PermissionDefinitionService},relativeToTab={RelativeToTab},location={Location}): newTab.PortalID → {Arg3}", permissionDefinitionService, relativeToTab, location, newTab.PortalID);
				Logger.LogDebug("InitTabInfoObject(permissionDefinitionService={PermissionDefinitionService},relativeToTab={RelativeToTab},location={Location}): newTab.ParentId → {Arg3}", permissionDefinitionService, relativeToTab, location, newTab.ParentId);
				Logger.LogDebug("InitTabInfoObject(permissionDefinitionService={PermissionDefinitionService},relativeToTab={RelativeToTab},location={Location}): newTab.Level → {Arg3}", permissionDefinitionService, relativeToTab, location, newTab.Level);
                newTab.ParentId = parentTab.TabID;
                newTab.Level = parentTab.Level + 1;
                switch (PortalSettings.Current.SSLSetup)
                {
                    case Abstractions.Security.SiteSslSetup.Off:
                        newTab.IsSecure = false;
						Logger.LogDebug("InitTabInfoObject(permissionDefinitionService={PermissionDefinitionService},relativeToTab={RelativeToTab},location={Location}): newTab.IsSecure → {Arg3}", permissionDefinitionService, relativeToTab, location, newTab.IsSecure);
                        break;
                    case Abstractions.Security.SiteSslSetup.Advanced:
                        newTab.IsSecure = parentTab.IsSecure;
                        break;
                    default:
                        newTab.IsSecure = true;
                        break;
                }
            }
            else
            {
                newTab.PortalID = PortalSettings.Current.PortalId;
                newTab.ParentId = Null.NullInteger;
                newTab.Level = 0;
            }

            // Inherit permissions from parent
            newTab.TabPermissions.Clear();
            if (newTab.PortalID != Null.NullInteger && (parentTab != null))
            {
                newTab.TabPermissions.AddRange(parentTab.TabPermissions);
            }
            else if (newTab.PortalID != Null.NullInteger)
            {
                // Give admin full permission
                foreach (var permission in permissionDefinitionService.GetDefinitionsByTab())
                {
                    IPermissionInfo newTabPermission = new TabPermissionInfo();
					Logger.LogDebug("InitTabInfoObject(permissionDefinitionService={PermissionDefinitionService},relativeToTab={RelativeToTab},location={Location}): newTabPermission → {NewTabPermission}", permissionDefinitionService, relativeToTab, location, newTabPermission);
					Logger.LogDebug("InitTabInfoObject(permissionDefinitionService={PermissionDefinitionService},relativeToTab={RelativeToTab},location={Location}): newTabPermission.PermissionId → {Arg3}", permissionDefinitionService, relativeToTab, location, newTabPermission.PermissionId);
					Logger.LogDebug("InitTabInfoObject(permissionDefinitionService={PermissionDefinitionService},relativeToTab={RelativeToTab},location={Location}): newTabPermission.PermissionKey → {Arg3}", permissionDefinitionService, relativeToTab, location, newTabPermission.PermissionKey);
					Logger.LogDebug("InitTabInfoObject(permissionDefinitionService={PermissionDefinitionService},relativeToTab={RelativeToTab},location={Location}): newTabPermission.PermissionName → {Arg3}", permissionDefinitionService, relativeToTab, location, newTabPermission.PermissionName);
					Logger.LogDebug("InitTabInfoObject(permissionDefinitionService={PermissionDefinitionService},relativeToTab={RelativeToTab},location={Location}): newTabPermission.AllowAccess → {Arg3}", permissionDefinitionService, relativeToTab, location, newTabPermission.AllowAccess);
					Logger.LogDebug("InitTabInfoObject(permissionDefinitionService={PermissionDefinitionService},relativeToTab={RelativeToTab},location={Location}): newTabPermission.RoleId → {Arg3}", permissionDefinitionService, relativeToTab, location, newTabPermission.RoleId);
                    newTabPermission.PermissionId = permission.PermissionId;
                    newTabPermission.PermissionKey = permission.PermissionKey;
                    newTabPermission.PermissionName = permission.PermissionName;
                    newTabPermission.AllowAccess = true;
                    newTabPermission.RoleId = PortalSettings.Current.AdministratorRoleId;
                    newTab.TabPermissions.Add((TabPermissionInfo)newTabPermission);
                }
            }

			Logger.LogDebug("<<< Exiting InitTabInfoObject(permissionDefinitionService={PermissionDefinitionService})", permissionDefinitionService);
            return newTab;
        }

        /// <summary>Gets the parent of the new page/tab.</summary>
        /// <param name="relativeToTab">The page/tab to which the new page/tab should be relative.</param>
        /// <param name="location">The new page/tab's location relative to <paramref name="relativeToTab"/>.</param>
        /// <returns>The <see cref="TabInfo"/> or <see langword="null"/> if there is no parent.</returns>
        public static TabInfo GetParentTab(TabInfo relativeToTab, TabRelativeLocation location)
        {
			Logger.LogDebug(">>> Entering GetParentTab(relativeToTab={RelativeToTab},location={Location})", relativeToTab, location);
            if (relativeToTab == null)
            {
                return null;
            }

            TabInfo parentTab = null;
			Logger.LogDebug("GetParentTab(relativeToTab={RelativeToTab},location={Location}): parentTab → {ParentTab}", relativeToTab, location, parentTab);
            if (location == TabRelativeLocation.CHILD)
            {
                parentTab = relativeToTab;
            }
            else if ((relativeToTab != null) && relativeToTab.ParentId != Null.NullInteger)
            {
                parentTab = TabController.Instance.GetTab(relativeToTab.ParentId, relativeToTab.PortalID, false);
            }

			Logger.LogDebug("<<< Exiting GetParentTab(relativeToTab={RelativeToTab},location={Location})", relativeToTab, location);
            return parentTab;
        }

        /// <summary>Gets the list of pages the current user can edit.</summary>
        /// <returns>A list of <see cref="TabInfo"/> instances.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial IList<TabInfo> GetPagesList()
            => GetPagesList(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>());

        /// <summary>Gets the list of pages the current user can edit.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <returns>A list of <see cref="TabInfo"/> instances.</returns>
        public static IList<TabInfo> GetPagesList(IHostSettings hostSettings, IApplicationStatusInfo appStatus)
        {
			Logger.LogDebug(">>> Entering GetPagesList(hostSettings={HostSettings},appStatus={AppStatus})", hostSettings, appStatus);
            IList<TabInfo> portalTabs = null;
            UserInfo userInfo = UserController.Instance.GetCurrentUserInfo();
			Logger.LogDebug("GetPagesList(hostSettings={HostSettings},appStatus={AppStatus}): userInfo → {UserInfo}", hostSettings, appStatus, userInfo);
			Logger.LogDebug("GetPagesList(hostSettings={HostSettings},appStatus={AppStatus}): portalTabs → {PortalTabs}", hostSettings, appStatus, portalTabs);
            if ((userInfo != null) && userInfo.UserID != Null.NullInteger)
            {
                if (userInfo.IsSuperUser && PortalSettings.Current.ActiveTab.IsSuperTab)
                {
                    portalTabs = TabController.Instance.GetTabsByPortal(Null.NullInteger).AsList();
                }
                else
                {
                    portalTabs = TabController.GetPortalTabs(hostSettings, appStatus, PortalSettings.Current.PortalId, Null.NullInteger, false, Null.NullString, true, false, true, false, true);
                }
            }

            if (portalTabs == null)
            {
                portalTabs = new List<TabInfo>();
            }

			Logger.LogDebug("<<< Exiting GetPagesList()");
            return portalTabs;
        }

        /// <summary>Gets a value indicating whether the current page is the host dashboard page.</summary>
        /// <returns><see langword="true"/> if the current page is the host console, otherwise <see langword="false"/>.</returns>
        public static bool IsHostConsolePage()
        {
			Logger.LogDebug(">>> Entering IsHostConsolePage()");
			Logger.LogDebug("<<< Exiting IsHostConsolePage()");
			Logger.LogDebug("<<< Exiting IsHostConsolePage(tab={Tab})", tab);
            return PortalSettings.Current.ActiveTab.IsSuperTab && PortalSettings.Current.ActiveTab.TabPath == "//Host";
        }

        /// <summary>Gets a value indicating whether the given <paramref name="tab"/> is the host dashboard page.</summary>
        /// <param name="tab">The page.</param>
        /// <returns><see langword="true"/> if the given <paramref name="tab"/> is the host console, otherwise <see langword="false"/>.</returns>
        public static bool IsHostConsolePage(TabInfo tab)
        {
            return tab.IsSuperTab && tab.TabPath == "//Host";
        }

        /// <summary>Gets a value indicating whether the current page can be moved.</summary>
        /// <returns><see langword="true"/> if the current page can be moved, otherwise <see langword="false"/>.</returns>
        public static bool CanMovePage()
        {
			Logger.LogDebug(">>> Entering CanMovePage()");
            // Cannot move the host console page
            if (IsHostConsolePage())
            {
                return false;
            }

            // Page Editors - Can only move children they have 'Manage' permission to, they cannot move the top level page
            if (!PortalSecurity.IsInRole("Administrators"))
            {
                int parentTabID = PortalSettings.Current.ActiveTab.ParentId;
                if (parentTabID == Null.NullInteger)
                {
                    return false;
                }

                TabInfo parentTab = TabController.Instance.GetTab(parentTabID, PortalSettings.Current.ActiveTab.PortalID, false);
				Logger.LogDebug("CanMovePage(): parentTab → {ParentTab}", parentTab);
                string permissionList = "MANAGE";
                if (!TabPermissionController.HasTabPermission(parentTab.TabPermissions, permissionList))
                {
                    return false;
                }
            }

			Logger.LogDebug("<<< Exiting CanMovePage()");
            return true;
        }

        /// <inheritdoc cref="SaveTabInfoObject(DotNetNuke.Abstractions.Modules.IBusinessControllerProvider,DotNetNuke.Entities.Tabs.TabInfo,DotNetNuke.Entities.Tabs.TabInfo,DotNetNuke.Web.UI.TabRelativeLocation,string)"/>
        [DnnDeprecated(10, 0, 0, "Please use overload with IServiceProvider")]
        public static partial int SaveTabInfoObject(
            TabInfo tab,
            TabInfo relativeToTab,
            TabRelativeLocation location,
            string templateFileId)
        {
			Logger.LogDebug(">>> Entering SaveTabInfoObject(tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId})", tab, relativeToTab, location, templateFileId);
            using var scope = Globals.GetOrCreateServiceScope();
			Logger.LogDebug("SaveTabInfoObject(tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): scope → {Scope}", tab, relativeToTab, location, templateFileId, scope);
			Logger.LogDebug("<<< Exiting SaveTabInfoObject(tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId})", tab, relativeToTab, location, templateFileId);
            return SaveTabInfoObject(scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(), tab, relativeToTab, location, templateFileId);
        }

        /// <summary>Creates a new page/tab.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="tab">The page to create.</param>
        /// <param name="relativeToTab">The page/tab to which the new page/tab should be relative.</param>
        /// <param name="location">The new page/tab's location relative to <paramref name="relativeToTab"/>.</param>
        /// <param name="templateFileId">The file ID of the page template (or <see langword="null"/> or <see cref="string.Empty"/> to not use a page template).</param>
        /// <returns>The new tab's ID.</returns>
        /// <exception cref="DotNetNukeException">If the page is invalid.</exception>
        [DnnDeprecated(10, 2, 2, "Please use overload with IPermissionDefinitionService")]
        public static partial int SaveTabInfoObject(IBusinessControllerProvider businessControllerProvider, TabInfo tab, TabInfo relativeToTab, TabRelativeLocation location, string templateFileId)
            => SaveTabInfoObject(businessControllerProvider, Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>(), tab, relativeToTab, location, templateFileId);

        /// <summary>Creates a new page/tab.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="tab">The page to create.</param>
        /// <param name="relativeToTab">The page/tab to which the new page/tab should be relative.</param>
        /// <param name="location">The new page/tab's location relative to <paramref name="relativeToTab"/>.</param>
        /// <param name="templateFileId">The file ID of the page template (or <see langword="null"/> or <see cref="string.Empty"/> to not use a page template).</param>
        /// <returns>The new tab's ID.</returns>
        /// <exception cref="DotNetNukeException">If the page is invalid.</exception>
        public static int SaveTabInfoObject(IBusinessControllerProvider businessControllerProvider, IPermissionDefinitionService permissionDefinitionService, TabInfo tab, TabInfo relativeToTab, TabRelativeLocation location, string templateFileId)
        {
			Logger.LogDebug(">>> Entering SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId})", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId);
            // Validation:
            // Tab name is required
            // Tab name is invalid
            if (!TabController.IsValidTabName(tab.TabName, out var invalidType))
            {
                switch (invalidType)
                {
                    case "EmptyTabName":
						Logger.LogWarning("Throwing DotNetNukeException from SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId})", businessControllerProvider, tab, relativeToTab, location, templateFileId);
						Logger.LogWarning("Throwing DotNetNukeException from SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId})", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId);
                        throw new DotNetNukeException("Page name is required.", DotNetNukeErrorCode.PageNameRequired);
                    case "InvalidTabName":
                        throw new DotNetNukeException("Page name is invalid.", DotNetNukeErrorCode.PageNameInvalid);
                }
            }
            else if (Validate_IsCircularReference(tab.PortalID, tab.TabID))
            {
                throw new DotNetNukeException("Cannot move page to that location.", DotNetNukeErrorCode.PageCircularReference);
            }

            bool usingDefaultLanguage = (tab.CultureCode == PortalSettings.Current.DefaultLanguage) || tab.CultureCode == null;

            if (PortalSettings.Current.ContentLocalizationEnabled)
            {
                if (!usingDefaultLanguage)
                {
                    TabInfo defaultLanguageSelectedTab = tab.DefaultLanguageTab;
					Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): defaultLanguageSelectedTab → {DefaultLanguageSelectedTab}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, defaultLanguageSelectedTab);

                    if (defaultLanguageSelectedTab == null)
                    {
                        // get the siblings from the selected tab and iterate through until you find a sibling with a corresponding default language tab
                        // if none are found get a list of all the tabs from the default language and then select the last one
                        var selectedTabSiblings = TabController.Instance.GetTabsByPortal(tab.PortalID).WithCulture(tab.CultureCode, true).AsList();
						Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): selectedTabSiblings → {SelectedTabSiblings}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, selectedTabSiblings);
                        foreach (TabInfo sibling in selectedTabSiblings)
                        {
                            TabInfo siblingDefaultTab = sibling.DefaultLanguageTab;
                            if (siblingDefaultTab != null)
                            {
                                defaultLanguageSelectedTab = siblingDefaultTab;
                                break;
                            }
                        }

                        // still haven't found it
                        if (defaultLanguageSelectedTab == null)
                        {
                            var defaultLanguageTabs = TabController.Instance.GetTabsByPortal(tab.PortalID).WithCulture(PortalSettings.Current.DefaultLanguage, true).AsList();
							Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): defaultLanguageTabs → {DefaultLanguageTabs}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, defaultLanguageTabs);
                            defaultLanguageSelectedTab = defaultLanguageTabs[defaultLanguageTabs.Count];

                            // get the last tab
                        }
                    }

                    relativeToTab = defaultLanguageSelectedTab;
                }
            }

            if (location != TabRelativeLocation.NOTSET)
            {
                // Check Host tab - don't allow adding before or after
                if (IsHostConsolePage(relativeToTab) && location is TabRelativeLocation.AFTER or TabRelativeLocation.BEFORE)
                {
                    throw new DotNetNukeException("You cannot add or move pages before or after the Host tab.", DotNetNukeErrorCode.HostBeforeAfterError);
                }

                TabInfo parentTab = GetParentTab(relativeToTab, location);
				Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): parentTab → {ParentTab}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, parentTab);
                string permissionList = "ADD,COPY,EDIT,MANAGE";

                // Check permissions for Page Editors when moving or inserting
                if (!PortalSecurity.IsInRole("Administrators"))
                {
                    if ((parentTab == null) || !TabPermissionController.HasTabPermission(parentTab.TabPermissions, permissionList))
                    {
                        throw new DotNetNukeException(
                            "You do not have permissions to add or move pages to this location. You can only add or move pages as children of pages you can edit.",
                            DotNetNukeErrorCode.PageEditorPermissionError);
                    }
                }

                if (parentTab != null)
                {
                    tab.ParentId = parentTab.TabID;
					Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): tab.ParentId → {Arg6}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, tab.ParentId);
					Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): tab.Level → {Arg6}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, tab.Level);
                    tab.Level = parentTab.Level + 1;
                }
                else
                {
                    tab.ParentId = Null.NullInteger;
                    tab.Level = 0;
                }
            }

            if (tab.TabID > Null.NullInteger && tab.TabID == tab.ParentId)
            {
                throw new DotNetNukeException("Parent page is invalid.", DotNetNukeErrorCode.ParentTabInvalid);
            }

            tab.TabPath = Globals.GenerateTabPath(tab.ParentId, tab.TabName);

            // check whether have conflict between tab path and portal alias.
            if (TabController.IsDuplicateWithPortalAlias(PortalSettings.Current.PortalId, tab.TabPath))
            {
                throw new DotNetNukeException("The page path is duplicate with a site alias", DotNetNukeErrorCode.DuplicateWithAlias);
            }

            try
            {
                if (tab.TabID < 0)
                {
                    if (tab.TabPermissions.Count == 0 && tab.PortalID != Null.NullInteger)
                    {
                        // Give admin full permission
                        foreach (IPermissionDefinitionInfo permission in permissionDefinitionService.GetDefinitionsByTab())
                        {
                            IPermissionInfo newTabPermission = new TabPermissionInfo();
							Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): newTabPermission → {NewTabPermission}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, newTabPermission);
							Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): newTabPermission.PermissionId → {Arg6}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, newTabPermission.PermissionId);
							Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): newTabPermission.PermissionKey → {Arg6}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, newTabPermission.PermissionKey);
							Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): newTabPermission.PermissionName → {Arg6}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, newTabPermission.PermissionName);
							Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): newTabPermission.AllowAccess → {Arg6}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, newTabPermission.AllowAccess);
							Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): newTabPermission.RoleId → {Arg6}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, newTabPermission.RoleId);
                            newTabPermission.PermissionId = permission.PermissionId;
                            newTabPermission.PermissionKey = permission.PermissionKey;
                            newTabPermission.PermissionName = permission.PermissionName;
                            newTabPermission.AllowAccess = true;
                            newTabPermission.RoleId = PortalSettings.Current.AdministratorRoleId;
                            tab.TabPermissions.Add((TabPermissionInfo)newTabPermission);
                        }
                    }

                    var portalSettings = PortalController.Instance.GetCurrentSettings();
					Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): portalSettings → {PortalSettings}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, portalSettings);
                    if (portalSettings.ContentLocalizationEnabled)
                    {
                        Locale defaultLocale = LocaleController.Instance.GetDefaultLocale(tab.PortalID);
						Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): defaultLocale → {DefaultLocale}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, defaultLocale);
						Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): tab.CultureCode → {Arg6}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, tab.CultureCode);
                        tab.CultureCode = defaultLocale.Code;
                    }
                    else
                    {
                        tab.CultureCode = Null.NullString;
                    }

                    if (location == TabRelativeLocation.AFTER && (relativeToTab != null))
                    {
                        tab.TabID = TabController.Instance.AddTabAfter(tab, relativeToTab.TabID);
                    }
                    else if (location == TabRelativeLocation.BEFORE && (relativeToTab != null))
                    {
                        tab.TabID = TabController.Instance.AddTabBefore(tab, relativeToTab.TabID);
                    }
                    else
                    {
                        tab.TabID = TabController.Instance.AddTab(tab);
                    }

                    if (portalSettings.ContentLocalizationEnabled)
                    {
                        TabController.Instance.CreateLocalizedCopies(tab);
                    }

                    TabController.Instance.UpdateTabSetting(tab.TabID, "CacheProvider", string.Empty);
                    TabController.Instance.UpdateTabSetting(tab.TabID, "CacheDuration", string.Empty);
                    TabController.Instance.UpdateTabSetting(tab.TabID, "CacheIncludeExclude", "0");
                    TabController.Instance.UpdateTabSetting(tab.TabID, "IncludeVaryBy", string.Empty);
                    TabController.Instance.UpdateTabSetting(tab.TabID, "ExcludeVaryBy", string.Empty);
                    TabController.Instance.UpdateTabSetting(tab.TabID, "MaxVaryByCount", string.Empty);
                }
                else
                {
                    TabController.Instance.UpdateTab(tab);

                    if (location == TabRelativeLocation.AFTER && (relativeToTab != null))
                    {
                        TabController.Instance.MoveTabAfter(tab, relativeToTab.TabID);
                    }
                    else if (location == TabRelativeLocation.BEFORE && (relativeToTab != null))
                    {
                        TabController.Instance.MoveTabBefore(tab, relativeToTab.TabID);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                if (ex.Message.StartsWith("Page Exists", StringComparison.OrdinalIgnoreCase))
                {
                    throw new DotNetNukeException(ex.Message, DotNetNukeErrorCode.PageExists);
                }
            }

            // create the page from a template
            if (!string.IsNullOrEmpty(templateFileId))
            {
                XmlDocument xmlDoc = new XmlDocument { XmlResolver = null, };
                try
                {
                    var templateFile = FileManager.Instance.GetFile(Convert.ToInt32(templateFileId, CultureInfo.InvariantCulture));
					Logger.LogDebug("SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): templateFile → {TemplateFile}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, templateFile);
                    using (var fileContent = FileManager.Instance.GetFileContent(templateFile))
                    using (var xmlReader = XmlReader.Create(fileContent, new XmlReaderSettings { XmlResolver = null, }))
                    {
                        xmlDoc.Load(xmlReader);
                    }

                    TabController.DeserializePanes(businessControllerProvider, permissionDefinitionService, xmlDoc.SelectSingleNode("//portal/tabs/tab/panes"), tab.PortalID, tab.TabID, PortalTemplateModuleAction.Ignore, new Hashtable());

                    // save tab permissions
                    DeserializeTabPermissions(permissionDefinitionService, xmlDoc.SelectNodes("//portal/tabs/tab/tabpermissions/permission"), tab);
                }
                catch (Exception ex)
                {
					Logger.LogError("Exception in SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): {Arg5}", businessControllerProvider, tab, relativeToTab, location, templateFileId, ex.getMessage());
                    Exceptions.LogException(ex);
					Logger.LogError("Exception in SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId}): {Arg6}", businessControllerProvider, permissionDefinitionService, tab, relativeToTab, location, templateFileId, ex.getMessage());
                    throw new DotNetNukeException("Unable to process page template.", ex, DotNetNukeErrorCode.DeserializePanesFailed);
                }
            }

			Logger.LogDebug("<<< Exiting SaveTabInfoObject(businessControllerProvider={BusinessControllerProvider},tab={Tab},relativeToTab={RelativeToTab},location={Location},templateFileId={TemplateFileId})", businessControllerProvider, tab, relativeToTab, location, templateFileId);
            return tab.TabID;
        }

        /// <summary>Validates whether the page's parent contains a circular reference back to this page/tab.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="tabID">The page/tab ID.</param>
        /// <returns><see langword="true"/> if there is a circular reference, otherwise <see langword="false"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        public static bool Validate_IsCircularReference(int portalID, int tabID)
        {
			Logger.LogDebug(">>> Entering Validate_IsCircularReference(portalID={PortalID},tabID={TabID})", portalID, tabID);
            if (tabID != -1)
            {
                TabInfo objtab = TabController.Instance.GetTab(tabID, portalID, false);
				Logger.LogDebug("Validate_IsCircularReference(portalID={PortalID},tabID={TabID}): objtab → {Objtab}", portalID, tabID, objtab);

                if (objtab == null)
                {
                    return false;
                }

                if (objtab.Level == 0)
                {
                    return false;
                }

                if (tabID == objtab.ParentId)
                {
                    return true;
                }

			Logger.LogDebug("<<< Exiting Validate_IsCircularReference(portalID={PortalID},tabID={TabID})", portalID, tabID);
                return Validate_IsCircularReference(portalID, objtab.ParentId);
            }

            return false;
        }

        /// <summary>Updates a <paramref name="tab"/> with permission information.</summary>
        /// <param name="nodeTabPermissions">A list of tab permission elements.</param>
        /// <param name="tab">The tab/page to update.</param>
        [DnnDeprecated(10, 2, 2, "Please use overload with IPermissionDefinitionService")]
        public static partial void DeserializeTabPermissions(XmlNodeList nodeTabPermissions, TabInfo tab)
            => DeserializeTabPermissions(Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>(), nodeTabPermissions, tab);

        /// <summary>Updates a <paramref name="tab"/> with permission information.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="nodeTabPermissions">A list of tab permission elements.</param>
        /// <param name="tab">The tab/page to update.</param>
        public static void DeserializeTabPermissions(IPermissionDefinitionService permissionDefinitionService, XmlNodeList nodeTabPermissions, TabInfo tab)
        {
			Logger.LogDebug(">>> Entering DeserializeTabPermissions(permissionDefinitionService={PermissionDefinitionService},nodeTabPermissions={NodeTabPermissions},tab={Tab})", permissionDefinitionService, nodeTabPermissions, tab);
            foreach (XmlNode xmlTabPermission in nodeTabPermissions)
            {
			Logger.LogDebug("<<< Exiting DeserializeTabPermissions(nodeTabPermissions={NodeTabPermissions},tab={Tab})", nodeTabPermissions, tab);
                var permissionKey = XmlUtils.GetNodeValue(xmlTabPermission.CreateNavigator(), "permissionkey");
				Logger.LogDebug("DeserializeTabPermissions(permissionDefinitionService={PermissionDefinitionService},nodeTabPermissions={NodeTabPermissions},tab={Tab}): permissionKey → {PermissionKey}", permissionDefinitionService, nodeTabPermissions, tab, permissionKey);
                var permissionCode = XmlUtils.GetNodeValue(xmlTabPermission.CreateNavigator(), "permissioncode");
				Logger.LogDebug("DeserializeTabPermissions(permissionDefinitionService={PermissionDefinitionService},nodeTabPermissions={NodeTabPermissions},tab={Tab}): permissionCode → {PermissionCode}", permissionDefinitionService, nodeTabPermissions, tab, permissionCode);
                var roleName = XmlUtils.GetNodeValue(xmlTabPermission.CreateNavigator(), "rolename");
				Logger.LogDebug("DeserializeTabPermissions(permissionDefinitionService={PermissionDefinitionService},nodeTabPermissions={NodeTabPermissions},tab={Tab}): roleName → {RoleName}", permissionDefinitionService, nodeTabPermissions, tab, roleName);
                var allowAccess = XmlUtils.GetNodeValueBoolean(xmlTabPermission, "allowaccess");
				Logger.LogDebug("DeserializeTabPermissions(permissionDefinitionService={PermissionDefinitionService},nodeTabPermissions={NodeTabPermissions},tab={Tab}): allowAccess → {AllowAccess}", permissionDefinitionService, nodeTabPermissions, tab, allowAccess);
                var permissions = permissionDefinitionService.GetDefinitionsByCodeAndKey(permissionCode, permissionKey);
				Logger.LogDebug("DeserializeTabPermissions(permissionDefinitionService={PermissionDefinitionService},nodeTabPermissions={NodeTabPermissions},tab={Tab}): permissions → {Permissions}", permissionDefinitionService, nodeTabPermissions, tab, permissions);
                var permissionId = permissions.Last().PermissionId;
				Logger.LogDebug("DeserializeTabPermissions(permissionDefinitionService={PermissionDefinitionService},nodeTabPermissions={NodeTabPermissions},tab={Tab}): permissionId → {PermissionId}", permissionDefinitionService, nodeTabPermissions, tab, permissionId);

                var roleId = int.MinValue;
				Logger.LogDebug("DeserializeTabPermissions(permissionDefinitionService={PermissionDefinitionService},nodeTabPermissions={NodeTabPermissions},tab={Tab}): roleId → {RoleId}", permissionDefinitionService, nodeTabPermissions, tab, roleId);
                switch (roleName)
                {
                    case Globals.glbRoleAllUsersName:
                        roleId = Convert.ToInt32(Globals.glbRoleAllUsers, CultureInfo.InvariantCulture);
                        break;
                    case Globals.glbRoleUnauthUserName:
                        roleId = Convert.ToInt32(Globals.glbRoleUnauthUser, CultureInfo.InvariantCulture);
                        break;
                    default:
                        IPortalInfo portal = PortalController.Instance.GetPortal(tab.PortalID);
						Logger.LogDebug("DeserializeTabPermissions(permissionDefinitionService={PermissionDefinitionService},nodeTabPermissions={NodeTabPermissions},tab={Tab}): portal → {Portal}", permissionDefinitionService, nodeTabPermissions, tab, portal);
                        var role = RoleController.Instance.GetRole(portal.PortalId, r => r.RoleName == roleName);
						Logger.LogDebug("DeserializeTabPermissions(permissionDefinitionService={PermissionDefinitionService},nodeTabPermissions={NodeTabPermissions},tab={Tab}): role → {Role}", permissionDefinitionService, nodeTabPermissions, tab, role);
                        if (role != null)
                        {
                            roleId = role.RoleID;
                        }

                        break;
                }

                if (roleId != int.MinValue && !tab.TabPermissions.Cast<IPermissionInfo>().Any(p => p.RoleId == roleId && p.PermissionId == permissionId))
                {
                    var tabPermission = new TabPermissionInfo
                    {
                        TabID = tab.TabID,
                        AllowAccess = allowAccess,
                    };
                    ((IPermissionInfo)tabPermission).PermissionId = permissionId;
                    ((IPermissionInfo)tabPermission).RoleId = roleId;

                    tab.TabPermissions.Add(tabPermission);
                }
            }

            TabController.Instance.UpdateTab(tab);
        }
    }
}
