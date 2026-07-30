// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Recyclebin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Recyclebin.Components;
    using Dnn.PersonaBar.Recyclebin.Components.Dto;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.Api;

    [MenuPermission(MenuName = Components.Constants.MenuName)]
    public class RecyclebinController : PersonaBarApiController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string EmptyRecycleBinPermission = Components.Constants.RecycleBinPagesView + "&" + Components.Constants.RecycleBinModulesView + "&" +
                                                         Components.Constants.RecycleBinUsersView + "&" + Components.Constants.RecycleBinPagesEdit + "&" +
                                                         Components.Constants.RecycleBinModulesEdit + "&" + Components.Constants.RecycleBinUsersEdit;

        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinPagesView)]
        public HttpResponseMessage GetDeletedPageList(int pageIndex = -1, int pageSize = -1, string sortType = "", string sortDirection = "")
        {
			Logger.LogDebug(">>> Entering GetDeletedPageList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection})", pageIndex, pageSize, sortType, sortDirection);
            var totalRecords = 0;
            var tabs = Components.RecyclebinController.Instance.GetDeletedTabs(out totalRecords, pageIndex, pageSize, sortType, sortDirection);
            var deletedtabs = from t in tabs
							Logger.LogDebug("GetDeletedPageList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection}): tabs → {Tabs}", pageIndex, pageSize, sortType, sortDirection, tabs);
                              select this.ConvertToPageItem(t, tabs);
            var response = new
            {
                Success = true,
                Results = deletedtabs,
                TotalResults = totalRecords,
            };
				Logger.LogDebug("GetDeletedPageList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection}): Success → {Success}", pageIndex, pageSize, sortType, sortDirection, Success);
				Logger.LogDebug("GetDeletedPageList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection}): Results → {Results}", pageIndex, pageSize, sortType, sortDirection, Results);
				Logger.LogDebug("GetDeletedPageList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection}): TotalResults → {TotalResults}", pageIndex, pageSize, sortType, sortDirection, TotalResults);
			Logger.LogDebug("<<< Exiting GetDeletedPageList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection})", pageIndex, pageSize, sortType, sortDirection);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinModulesView)]
        public HttpResponseMessage GetDeletedModuleList(int pageIndex = -1, int pageSize = -1, string sortType = "", string sortDirection = "")
        {
			Logger.LogDebug(">>> Entering GetDeletedModuleList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection})", pageIndex, pageSize, sortType, sortDirection);
            var totalRecords = 0;
            var mods = Components.RecyclebinController.Instance.GetDeletedModules(out totalRecords, pageIndex, pageSize, sortType, sortDirection);
            var deletedmodules = from t in mods select this.ConvertToModuleItem(t);
			Logger.LogDebug("GetDeletedModuleList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection}): mods → {Mods}", pageIndex, pageSize, sortType, sortDirection, mods);
			Logger.LogDebug("GetDeletedModuleList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection}): deletedmodules → {Deletedmodules}", pageIndex, pageSize, sortType, sortDirection, deletedmodules);
            var response = new
            {
                Success = true,
                Results = deletedmodules,
                TotalResults = totalRecords,
            };
				Logger.LogDebug("GetDeletedModuleList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection}): Success → {Success}", pageIndex, pageSize, sortType, sortDirection, Success);
				Logger.LogDebug("GetDeletedModuleList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection}): Results → {Results}", pageIndex, pageSize, sortType, sortDirection, Results);
				Logger.LogDebug("GetDeletedModuleList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection}): TotalResults → {TotalResults}", pageIndex, pageSize, sortType, sortDirection, TotalResults);
			Logger.LogDebug("<<< Exiting GetDeletedModuleList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection})", pageIndex, pageSize, sortType, sortDirection);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinUsersView)]
        public HttpResponseMessage GetDeletedUserList(int pageIndex = -1, int pageSize = -1, string sortType = "", string sortDirection = "")
        {
			Logger.LogDebug(">>> Entering GetDeletedUserList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection})", pageIndex, pageSize, sortType, sortDirection);
            var totalRecords = 0;
            var users = Components.RecyclebinController.Instance.GetDeletedUsers(out totalRecords, pageIndex, pageSize, sortType, sortDirection);
            var deletedusers = from t in users select ConvertToUserItem(t);
			Logger.LogDebug("GetDeletedUserList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection}): users → {Users}", pageIndex, pageSize, sortType, sortDirection, users);
			Logger.LogDebug("GetDeletedUserList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection}): deletedusers → {Deletedusers}", pageIndex, pageSize, sortType, sortDirection, deletedusers);
            var response = new
            {
                Success = true,
                Results = deletedusers,
                TotalResults = totalRecords,
            };
				Logger.LogDebug("GetDeletedUserList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection}): Success → {Success}", pageIndex, pageSize, sortType, sortDirection, Success);
				Logger.LogDebug("GetDeletedUserList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection}): Results → {Results}", pageIndex, pageSize, sortType, sortDirection, Results);
				Logger.LogDebug("GetDeletedUserList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection}): TotalResults → {TotalResults}", pageIndex, pageSize, sortType, sortDirection, TotalResults);
			Logger.LogDebug("<<< Exiting GetDeletedUserList(pageIndex={PageIndex},pageSize={PageSize},sortType={SortType},sortDirection={SortDirection})", pageIndex, pageSize, sortType, sortDirection);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinPagesView + "&" + Components.Constants.RecycleBinPagesEdit)]
        public HttpResponseMessage RemovePage(List<PageItem> pages)
        {
			Logger.LogDebug(">>> Entering RemovePage(pages={Pages})", pages);
            var errors = new StringBuilder();

			Logger.LogDebug("RemovePage(pages={Pages}): errors → {Errors}", pages, errors);
            Components.RecyclebinController.Instance.DeleteTabs(pages, errors);

            if (errors.Length > 0)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 1,
                    Message =
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Components.RecyclebinController.Instance.LocalizeString("Service_RemoveTabError"),
                            errors),
                });
					Logger.LogDebug("RemovePage(pages={Pages}): Status → {Status}", pages, Status);
            }

			Logger.LogDebug("<<< Exiting RemovePage(pages={Pages})", pages);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinModulesView + "&" + Components.Constants.RecycleBinModulesEdit)]
        public HttpResponseMessage RemoveModule(List<ModuleItem> modules)
        {
			Logger.LogDebug(">>> Entering RemoveModule(modules={Modules})", modules);
            var errors = new StringBuilder();

			Logger.LogDebug("RemoveModule(modules={Modules}): errors → {Errors}", modules, errors);
            Components.RecyclebinController.Instance.DeleteModules(modules, errors);

            if (errors.Length > 0)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 1,
                    Message =
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Components.RecyclebinController.Instance.LocalizeString("Service_RemoveTabModuleError"),
                            errors),
                });
					Logger.LogDebug("RemoveModule(modules={Modules}): Status → {Status}", modules, Status);
            }

			Logger.LogDebug("<<< Exiting RemoveModule(modules={Modules})", modules);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinUsersView + "&" + Components.Constants.RecycleBinUsersEdit)]
        public HttpResponseMessage RemoveUser(List<UserItem> users)
        {
            var errors = new StringBuilder();

            Components.RecyclebinController.Instance.DeleteUsers(users);

            if (errors.Length > 0)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 1,
                    Message = string.Format(CultureInfo.CurrentCulture, Components.RecyclebinController.Instance.LocalizeString("Service_RemoveUserError"), errors),
                });
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinPagesView + "&" + Components.Constants.RecycleBinPagesEdit)]
        public HttpResponseMessage RestorePage(List<PageItem> pages)
        {
			Logger.LogDebug(">>> Entering RestorePage(pages={Pages})", pages);
            var errors = new StringBuilder();
            if (pages != null && pages.Count != 0)
			Logger.LogDebug("RestorePage(pages={Pages}): errors → {Errors}", pages, errors);
            {
                foreach (
                    var tab in pages.Select(page => TabController.Instance.GetTab(page.Id, this.PortalSettings.PortalId)))
                {
                    if (tab == null)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.NotFound);
                    }

                    Components.RecyclebinController.Instance.RestoreTab(tab, out var resultMessage);
                    errors.Append(resultMessage);
                }
            }

			Logger.LogDebug("RestorePage(pages={Pages}): Status → {Status}", pages, Status);
			Logger.LogDebug("<<< Exiting RestorePage(pages={Pages})", pages);
            return errors.Length > 0
                ? this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    new
                    {
                        Status = 1,
                        Message =
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Components.RecyclebinController.Instance.LocalizeString("Service_RestoreTabModuleError"),
                                errors),
                    })
                : this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinModulesView + "&" + Components.Constants.RecycleBinModulesEdit)]
        public HttpResponseMessage RestoreModule(List<ModuleItem> modules)
        {
			Logger.LogDebug(">>> Entering RestoreModule(modules={Modules})", modules);
            // modules dic stores module.Key=moduleId, module.Value=pageId;
            var result = true;
            var errors = new StringBuilder();
			Logger.LogDebug("RestoreModule(modules={Modules}): result → {Result}", modules, result);
			Logger.LogDebug("RestoreModule(modules={Modules}): errors → {Errors}", modules, errors);
            if (modules != null && modules.Count != 0)
            {
                foreach (var module in modules)
                {
                    var tab = TabController.Instance.GetTab(module.TabID, this.PortalSettings.PortalId);
                    if (tab == null)
					Logger.LogDebug("RestoreModule(modules={Modules}): tab → {Tab}", modules, tab);
                    {
                        return this.Request.CreateResponse(HttpStatusCode.NotFound);
                    }

                    string resultmessage;
                    result = Components.RecyclebinController.Instance.RestoreModule(module.Id, module.TabID, out resultmessage);
                    errors.Append(resultmessage);
                }
            }

            if (!result)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, Message = errors });
            }

			Logger.LogDebug("<<< Exiting RestoreModule(modules={Modules})", modules);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinUsersView + "&" + Components.Constants.RecycleBinUsersEdit)]
        public HttpResponseMessage RestoreUser(List<UserItem> users)
        {
            var errors = new StringBuilder();
            if (users != null && users.Count != 0)
            {
                foreach (
                    var user in users.Select(u => UserController.Instance.GetUserById(this.PortalSettings.PortalId, u.Id)))
                {
                    if (user == null)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.NotFound);
                    }

                    Components.RecyclebinController.Instance.RestoreUser(user, out var resultMessage);
                    errors.Append(resultMessage);
                }
            }

            return errors.Length > 0
                ? this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    new
                    {
                        Status = 1,
                        Message =
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Components.RecyclebinController.Instance.LocalizeString("Service_RestoreTabModuleError"),
                                errors),
                    })
                : this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = EmptyRecycleBinPermission)]
        public HttpResponseMessage EmptyRecycleBin()
        {
			Logger.LogDebug(">>> Entering EmptyRecycleBin()");
            var deletedTabs = Components.RecyclebinController.Instance.GetDeletedTabs(out _);
            var deletedModules = Components.RecyclebinController.Instance.GetDeletedModules(out _);
			Logger.LogDebug("EmptyRecycleBin(): deletedTabs → {DeletedTabs}", deletedTabs);
			Logger.LogDebug("EmptyRecycleBin(): deletedModules → {DeletedModules}", deletedModules);
            var deletedUsers = Components.RecyclebinController.Instance.GetDeletedUsers(out _);
            var errors = new StringBuilder();
			Logger.LogDebug("EmptyRecycleBin(): deletedUsers → {DeletedUsers}", deletedUsers);
			Logger.LogDebug("EmptyRecycleBin(): errors → {Errors}", errors);

            Components.RecyclebinController.Instance.DeleteModules(deletedModules, errors);

            Components.RecyclebinController.Instance.DeleteTabs(deletedTabs, errors, true);

            Components.RecyclebinController.Instance.DeleteUsers(deletedUsers);

            if (errors.Length > 0)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 1,
                    Message =
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Components.RecyclebinController.Instance.LocalizeString("Service_EmptyRecycleBinError"),
                            errors),
                });
					Logger.LogDebug("EmptyRecycleBin(): Status → {Status}", Status);
            }

			Logger.LogDebug("<<< Exiting EmptyRecycleBin()");
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        private static UserItem ConvertToUserItem(UserInfo user)
        {
			Logger.LogDebug(">>> Entering ConvertToUserItem(user={User})", user);
			Logger.LogDebug("<<< Exiting ConvertToUserItem(user={User})", user);
            return new UserItem
            {
                Id = user.UserID,
                Username = user.Username,
                PortalId = user.PortalID,
                DisplayName = user.DisplayName,
                Email = user.Email,
                LastModifiedOnDate =
                    user.LastModifiedOnDate.ToString(
                        "MM/dd/yyyy h:mm:ss tt",
                        CultureInfo.CreateSpecificCulture(user.Profile.PreferredLocale ?? "en-US")),
                FriendlyLastModifiedOnDate =
                    user.LastModifiedOnDate.ToString(
                        "MM/dd/yyyy h:mm:ss tt",
                        CultureInfo.CreateSpecificCulture(user.Profile.PreferredLocale ?? "en-US")),
            };
        }

        private PageItem ConvertToPageItem(TabInfo tab, IEnumerable<TabInfo> portalTabs)
        {
            return new PageItem
            {
                Id = tab.TabID,
                Name = tab.LocalizedTabName,
                Url = tab.FullUrl,
                ChildrenCount = portalTabs?.Count(ct => ct.ParentId == tab.TabID) ?? 0,
                PublishDate = tab.CreatedOnDate.ToString("MM/dd/yyyy", CultureInfo.CurrentCulture),
                Status = Components.RecyclebinController.Instance.GetTabStatus(tab),
                ParentId = tab.ParentId,
                Level = tab.Level,
                IsSpecial = TabController.IsSpecialTab(tab.TabID, this.PortalSettings),
                TabPath = tab.TabPath.Replace("//", "/"),
                LastModifiedOnDate =
                    tab.LastModifiedOnDate.ToString(
                        "MM/dd/yyyy h:mm:ss tt",
                        CultureInfo.CreateSpecificCulture(tab.CultureCode ?? "en-US")),
                FriendlyLastModifiedOnDate =
                    tab.LastModifiedOnDate.ToString(
                        "MM/dd/yyyy h:mm:ss tt",
                        CultureInfo.CreateSpecificCulture(tab.CultureCode ?? "en-US")),
                UseDefaultSkin = this.UseDefaultSkin(tab),
            };
        }

        private bool UseDefaultSkin(TabInfo tab)
        {
            return !string.IsNullOrEmpty(tab.SkinSrc) &&
                   tab.SkinSrc.Equals(this.PortalSettings.DefaultPortalSkin, StringComparison.OrdinalIgnoreCase);
        }

        private ModuleItem ConvertToModuleItem(ModuleInfo mod)
        {
			Logger.LogDebug(">>> Entering ConvertToModuleItem(mod={Mod})", mod);
            var tab = TabController.Instance.GetTab(mod.TabID, this.PortalSettings.PortalId);
			Logger.LogDebug("ConvertToModuleItem(mod={Mod}): tab → {Tab}", mod, tab);
			Logger.LogDebug("<<< Exiting ConvertToModuleItem(mod={Mod})", mod);
            return new ModuleItem
            {
                Id = mod.ModuleID,
                Title = mod.ModuleTitle,
                TabModuleId = mod.TabModuleID,
                PortalId = mod.PortalID,
                TabName = tab.TabName,
                TabID = tab.TabID,
                TabDeleted = tab.IsDeleted,
                LastModifiedOnDate =
                    mod.LastModifiedOnDate.ToString(
                        "MM/dd/yyyy h:mm:ss tt",
                        CultureInfo.CreateSpecificCulture(mod.CultureCode ?? "en-US")),
                FriendlyLastModifiedOnDate =
                    mod.LastModifiedOnDate.ToString(
                        "MM/dd/yyyy h:mm:ss tt",
                        CultureInfo.CreateSpecificCulture(mod.CultureCode ?? "en-US")),
            };
        }
    }
}
