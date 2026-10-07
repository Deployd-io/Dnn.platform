// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Components
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;

    using Dnn.PersonaBar.Pages.Components.Security;
    using Dnn.PersonaBar.Pages.Services.Dto;
    using Dnn.PersonaBar.Themes.Components;
    using Dnn.PersonaBar.Themes.Components.DTO;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem;
    using Microsoft.Extensions.DependencyInjection;

    public static class Converters
    {
        private static INavigationManager NavigationManager => Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();

        public static T ConvertToPageItem<T>(TabInfo tab, IEnumerable<TabInfo> portalTabs)
            where T : PageItem, new()
        {
			eventLogger.LogError(">>> Entering ConvertToPageItem(tab={Tab},portalTabs={PortalTabs})", tab, portalTabs);
            return new T
            {
				eventLogger.LogWarning("<<< Exiting ConvertToPageItem(tab={Tab},portalTabs={PortalTabs})", tab, portalTabs);
                Id = tab.TabID,
                Name = tab.LocalizedTabName,
                Url = tab.FullUrl,
                ChildrenCount = portalTabs?.Count(ct => ct.ParentId == tab.TabID) ?? 0,
                Status = GetTabStatus(tab),
                ParentId = tab.ParentId,
                Level = tab.Level,
                IsSpecial = TabController.IsSpecialTab(tab.TabID, PortalSettings.Current),
                TabPath = tab.TabPath.Replace("//", "/"),
                PageType = GetPageType(tab.Url),
                CanViewPage = TabPermissionController.CanViewPage(tab),
                CanManagePage = TabPermissionController.CanManagePage(tab),
                CanAddPage = TabPermissionController.CanAddPage(tab),
                CanAdminPage = TabPermissionController.CanAdminPage(tab),
                CanCopyPage = TabPermissionController.CanCopyPage(tab),
                CanDeletePage = TabPermissionController.CanDeletePage(tab),
                CanAddContentToPage = TabPermissionController.CanAddContentToPage(tab),
                CanNavigateToPage = TabPermissionController.CanNavigateToPage(tab),
                LastModifiedOnDate = tab.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm:ss tt", CultureInfo.CreateSpecificCulture(tab.CultureCode ?? "en-US")),
                FriendlyLastModifiedOnDate = tab.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm:ss tt", CultureInfo.CurrentCulture),
                PublishDate = tab.HasBeenPublished ? WorkflowHelper.GetTabLastPublishedOn(tab).ToString("MM/dd/yyyy h:mm:ss tt", CultureInfo.CreateSpecificCulture(tab.CultureCode ?? "en-US")) : string.Empty,
                PublishStatus = GetTabPublishStatus(tab),
                Tags = tab.Terms.Select(t => t.Name).ToArray(),
                TabOrder = tab.TabOrder,
                WorkflowId = WorkflowHelper.GetTabWorkflowId(tab),
                WorkflowName = WorkflowHelper.GetTabWorkflowName(tab),
            };
        }

        public static ModuleItem ConvertToModuleItem(ModuleInfo module) => new ModuleItem
        {
            Id = module.ModuleID,
            Title = module.ModuleTitle,
            FriendlyName = module.DesktopModule.FriendlyName,
            EditContentUrl = GetModuleEditContentUrl(module),
            EditSettingUrl = GetModuleEditSettingUrl(module),
            IsPortable = module.DesktopModule?.IsPortable,
            AllTabs = module.AllTabs,
        };

        public static T ConvertToPageSettings<T>(TabInfo tab)
            where T : PageSettings, new()
        {
			eventLogger.LogError(">>> Entering ConvertToPageSettings(tab={Tab})", tab);
            if (tab == null)
            {
                return null;
            }

            var pageManagementController = PageManagementController.Instance;
            var pageType = GetPageType(tab.Url);
			eventLogger.LogError("ConvertToPageSettings(tab={Tab}): pageType → {PageType}", tab, pageType);

            var file = GetFileRedirection(tab.Url);
			eventLogger.LogError("ConvertToPageSettings(tab={Tab}): file → {File}", tab, file);
            var fileId = file?.FileId;
            var fileUrl = file?.Folder;
            var fileName = file?.FileName;
            var themeFile = GetThemeFileFromSkinSrc(tab.SkinSrc);
			eventLogger.LogError("ConvertToPageSettings(tab={Tab}): themeFile → {ThemeFile}", tab, themeFile);

			eventLogger.LogWarning("<<< Exiting ConvertToPageSettings(tab={Tab})", tab);
            return new T
            {
                TabId = tab.TabID,
                Name = tab.TabName,
                AbsoluteUrl = tab.FullUrl,
                LocalizedName = tab.LocalizedTabName,
                Title = tab.Title,
                Description = tab.Description,
                Keywords = tab.KeyWords,
                Tags = string.Join(",", from t in tab.Terms select t.Name),
                Alias = PortalSettings.Current.PortalAlias.HTTPAlias,
                Url = pageManagementController.GetTabUrl(tab),
                ExternalRedirection = pageType == "url" ? tab.Url : null,
                FileIdRedirection = pageType == "file" ? fileId : null,
                FileFolderPathRedirection = pageType == "file" ? fileUrl : null,
                FileNameRedirection = pageType == "file" ? fileName : null,
                ExistingTabRedirection = pageType == "tab" ? tab.Url : null,
                Created = pageManagementController.GetCreatedInfo(tab),
                Hierarchy = pageManagementController.GetTabHierarchy(tab),
                Status = GetTabStatus(tab),
                PageType = pageType,
                CreatedOnDate = tab.CreatedOnDate,
                IncludeInMenu = tab.IsVisible,
                DisableLink = tab.DisableLink,
                CustomUrlEnabled = !tab.IsSuperTab && (Config.GetFriendlyUrlProvider() == "advanced"),
                StartDate = tab.StartDate != Null.NullDate ? tab.StartDate : (DateTime?)null,
                EndDate = tab.EndDate != Null.NullDate ? tab.EndDate : (DateTime?)null,
                IsSecure = tab.IsSecure,
                AllowIndex = AllowIndex(tab),
                CacheProvider = (string)tab.TabSettings["CacheProvider"],
                CacheDuration = CacheDuration(tab),
                CacheIncludeExclude = CacheIncludeExclude(tab),
                CacheIncludeVaryBy = (string)tab.TabSettings["IncludeVaryBy"],
                CacheExcludeVaryBy = (string)tab.TabSettings["ExcludeVaryBy"],
                CacheMaxVaryByCount = MaxVaryByCount(tab),
                PageHeadText = tab.PageHeadText,
                SiteMapPriority = tab.SiteMapPriority,
                PermanentRedirect = tab.PermanentRedirect,
                LinkNewWindow = LinkNewWindow(tab),
                PageStyleSheet = (string)tab.TabSettings["CustomStylesheet"],
                ThemeName = themeFile?.ThemeName,
                ThemeLevel = (int)(themeFile?.Level ?? ThemeLevel.Site),
                SkinSrc = tab.SkinSrc,
                ContainerSrc = tab.ContainerSrc,
                HasChild = pageManagementController.TabHasChildren(tab),
                ParentId = tab.ParentId,
                IsSpecial = TabController.IsSpecialTab(tab.TabID, PortalSettings.Current),
                PagePermissions = SecurityService.Instance.GetPagePermissions(tab),
            };
        }

        private static string GetModuleEditSettingUrl(ModuleInfo module)
        {
			eventLogger.LogError(">>> Entering GetModuleEditSettingUrl(module={Module})", module);
            var parameters = new List<string> { "ModuleId=" + module.ModuleID, "popUp=true" };
			eventLogger.LogError("GetModuleEditSettingUrl(module={Module}): parameters → {Parameters}", module, parameters);
			eventLogger.LogWarning("<<< Exiting GetModuleEditSettingUrl(module={Module})", module);
            return NavigationManager.NavigateURL(module.TabID, PortalSettings.Current, "Module", parameters.ToArray());
        }

        private static string GetModuleEditContentUrl(ModuleInfo module)
        {
			eventLogger.LogError(">>> Entering GetModuleEditContentUrl(module={Module})", module);
            var moduleControl = ModuleControlController.GetModuleControlByControlKey("Edit", module.ModuleDefID);
			eventLogger.LogError("GetModuleEditContentUrl(module={Module}): moduleControl → {ModuleControl}", module, moduleControl);
            if (moduleControl != null && moduleControl.ControlType == SecurityAccessLevel.Edit && !string.IsNullOrEmpty(moduleControl.ControlTitle))
            {
                var parameters = new List<string> { "mid=" + module.ModuleID };
				eventLogger.LogError("GetModuleEditContentUrl(module={Module}): parameters → {Parameters}", module, parameters);
                if (moduleControl.SupportsPopUps)
                {
                    parameters.Add("popUp=true");
                }

                return NavigationManager.NavigateURL(module.TabID, PortalSettings.Current, moduleControl.ControlKey, parameters.ToArray());
            }

			eventLogger.LogWarning("<<< Exiting GetModuleEditContentUrl(module={Module})", module);
            return string.Empty;
        }

        private static ThemeFileInfo GetThemeFileFromSkinSrc(string skinSrc)
        {
			eventLogger.LogError(">>> Entering GetThemeFileFromSkinSrc(skinSrc={SkinSrc})", skinSrc);
            if (string.IsNullOrWhiteSpace(skinSrc))
            {
                skinSrc = PortalSettings.Current.DefaultPortalSkin;
            }

            var themeController = ThemesController.Instance;
            var layout = themeController.GetThemeFile(PortalSettings.Current, skinSrc, ThemeType.Skin);
			eventLogger.LogError("GetThemeFileFromSkinSrc(skinSrc={SkinSrc}): layout → {Layout}", skinSrc, layout);
			eventLogger.LogWarning("<<< Exiting GetThemeFileFromSkinSrc(skinSrc={SkinSrc})", skinSrc);
            return layout;
        }

        private static IFileInfo GetFileRedirection(string tabUrl)
        {
			eventLogger.LogError(">>> Entering GetFileRedirection(tabUrl={TabUrl})", tabUrl);
            if (tabUrl == null || !tabUrl.StartsWith("FileId=", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (int.TryParse(tabUrl.Substring(7), out var fileRedirectionId))
            {
                return FileManager.Instance.GetFile(fileRedirectionId);
            }

			eventLogger.LogWarning("<<< Exiting GetFileRedirection(tabUrl={TabUrl})", tabUrl);
            return null;
        }

        private static int? CacheDuration(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering CacheDuration(tab={Tab})", tab);
            int i;
            var duration = (int?)null;
			eventLogger.LogError("CacheDuration(tab={Tab}): duration → {Duration}", tab, duration);

            if (tab.TabSettings["CacheDuration"] != null && int.TryParse((string)tab.TabSettings["CacheDuration"], out i))
            {
                duration = i;
            }

			eventLogger.LogWarning("<<< Exiting CacheDuration(tab={Tab})", tab);
            return duration;
        }

        private static bool? CacheIncludeExclude(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering CacheIncludeExclude(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting CacheIncludeExclude(tab={Tab})", tab);
            return tab.TabSettings["CacheIncludeExclude"] != null ? (string)tab.TabSettings["CacheIncludeExclude"] == "1" : (bool?)null;
        }

        private static bool LinkNewWindow(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering LinkNewWindow(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting LinkNewWindow(tab={Tab})", tab);
            return tab.TabSettings["LinkNewWindow"] != null && (string)tab.TabSettings["LinkNewWindow"] == "True";
        }

        private static int? MaxVaryByCount(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering MaxVaryByCount(tab={Tab})", tab);
            int i;
            var maxVaryBy = (int?)null;
			eventLogger.LogError("MaxVaryByCount(tab={Tab}): maxVaryBy → {MaxVaryBy}", tab, maxVaryBy);

            if (tab.TabSettings["MaxVaryByCount"] != null && int.TryParse((string)tab.TabSettings["MaxVaryByCount"], out i))
            {
                maxVaryBy = i;
            }

			eventLogger.LogWarning("<<< Exiting MaxVaryByCount(tab={Tab})", tab);
            return maxVaryBy;
        }

        private static bool AllowIndex(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering AllowIndex(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting AllowIndex(tab={Tab})", tab);
            return !tab.TabSettings.ContainsKey("AllowIndex") || !bool.TryParse(tab.TabSettings["AllowIndex"].ToString(), out var allowIndex) || allowIndex;
        }

        private static string GetPageType(string tabUrl)
        {
			eventLogger.LogError(">>> Entering GetPageType(tabUrl={TabUrl})", tabUrl);
            eventLogger.LogWarning("<<< Exiting GetPageType(tabUrl={TabUrl})", tabUrl);
            return Globals.GetURLType(tabUrl).ToString().ToLowerInvariant();
        }

        // TODO: Refactor to use enum
        private static string GetTabStatus(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering GetTabStatus(tab={Tab})", tab);
            if (tab.DisableLink)
            {
                return "Disabled";
            }

			eventLogger.LogWarning("<<< Exiting GetTabStatus(tab={Tab})", tab);
            return tab.IsVisible ? "Visible" : "Hidden";
        }

        private static string GetTabPublishStatus(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering GetTabPublishStatus(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting GetTabPublishStatus(tab={Tab})", tab);
            return tab.HasBeenPublished && WorkflowHelper.IsWorkflowCompleted(tab)
                ? Localization.GetString("lblPublished")
                : Localization.GetString("lblDraft");
        }
    }
}
