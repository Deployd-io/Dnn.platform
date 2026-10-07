// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.Groups.Components;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.Skins;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Display the group setup view.</summary>
    public partial class Setup : GroupsModuleBase
    {
        private readonly RoleProvider roleProvider;
        private readonly IEventLogger eventLogger;
        private readonly IUserController userController;
        private readonly IPermissionDefinitionService permissionDefinitionService;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="Setup"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with RoleProvider. Scheduled removal in v12.0.0.")]
        public Setup()
            : this(null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Setup"/> class.</summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public Setup(RoleProvider roleProvider, IEventLogger eventLogger, IUserController userController, IPermissionDefinitionService permissionDefinitionService)
            : this(roleProvider, eventLogger, userController, permissionDefinitionService, null)
        {
		eventLogger.LogError(">>> Entering Setup(roleProvider={RoleProvider},eventLogger={EventLogger},userController={UserController},permissionDefinitionService={PermissionDefinitionService})", roleProvider, eventLogger, userController, permissionDefinitionService);
		eventLogger.LogWarning("<<< Exiting Setup(roleProvider={RoleProvider},eventLogger={EventLogger},userController={UserController},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings})", roleProvider, eventLogger, userController, permissionDefinitionService, hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="Setup"/> class.</summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="hostSettings">The host settings.</param>
        public Setup(RoleProvider roleProvider, IEventLogger eventLogger, IUserController userController, IPermissionDefinitionService permissionDefinitionService, IHostSettings hostSettings)
        {
			eventLogger.LogError(">>> Entering Setup(roleProvider={RoleProvider},eventLogger={EventLogger},userController={UserController},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings})", roleProvider, eventLogger, userController, permissionDefinitionService, hostSettings);
			eventLogger.LogDebug("Setup(roleProvider={RoleProvider},eventLogger={EventLogger},userController={UserController},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings}): eventLogger → {EventLogger1}", roleProvider, eventLogger, userController, permissionDefinitionService, hostSettings, eventLogger);
			eventLogger.LogDebug("Setup(roleProvider={RoleProvider},eventLogger={EventLogger},userController={UserController},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings}): userController → {UserController1}", roleProvider, eventLogger, userController, permissionDefinitionService, hostSettings, userController);
			eventLogger.LogDebug("Setup(roleProvider={RoleProvider},eventLogger={EventLogger},userController={UserController},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings}): permissionDefinitionService → {PermissionDefinitionService1}", roleProvider, eventLogger, userController, permissionDefinitionService, hostSettings, permissionDefinitionService);
			eventLogger.LogDebug("Setup(roleProvider={RoleProvider},eventLogger={EventLogger},userController={UserController},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings}): hostSettings → {HostSettings1}", roleProvider, eventLogger, userController, permissionDefinitionService, hostSettings, hostSettings);
            this.roleProvider = roleProvider ?? this.DependencyProvider.GetRequiredService<RoleProvider>();
			eventLogger.LogWarning("<<< Exiting Setup(roleProvider={RoleProvider},eventLogger={EventLogger},userController={UserController},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings})", roleProvider, eventLogger, userController, permissionDefinitionService, hostSettings);
            this.eventLogger = eventLogger ?? this.DependencyProvider.GetRequiredService<IEventLogger>();
            this.userController = userController ?? this.DependencyProvider.GetRequiredService<IUserController>();
            this.permissionDefinitionService = permissionDefinitionService ?? this.DependencyProvider.GetRequiredService<IPermissionDefinitionService>();
            this.hostSettings = hostSettings ?? this.DependencyProvider.GetRequiredService<IHostSettings>();
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        public void btGo_Click(object sender, EventArgs e)
        {
			eventLogger.LogError(">>> Entering btGo_Click(sender={Sender},e={E})", sender, e);
            // Setup Child Page - Main View/Activity
            TabInfo tab = this.CreatePage(this.PortalSettings.ActiveTab, this.PortalId, this.TabId, "Group Activity", false);
			eventLogger.LogError("btGo_Click(sender={Sender},e={E}): tab → {Tab}", sender, e, tab);

            // Add Module to Child Page
            int groupViewModuleId = this.AddModule(tab, this.PortalId, "Social Groups", "ContentPane");
			eventLogger.LogError("btGo_Click(sender={Sender},e={E}): groupViewModuleId → {GroupViewModuleId}", sender, e, groupViewModuleId);
            int journalModuleId = this.AddModule(tab, this.PortalId, "Journal", "ContentPane");
			eventLogger.LogError("btGo_Click(sender={Sender},e={E}): journalModuleId → {JournalModuleId}", sender, e, journalModuleId);
            int consoleId = this.AddModule(tab, this.PortalId, "Console", "RightPane");
			eventLogger.LogError("btGo_Click(sender={Sender},e={E}): consoleId → {ConsoleId}", sender, e, consoleId);

            ModuleInfo groupConsoleModule = ModuleController.Instance.GetModule(consoleId, tab.TabID, false);
			eventLogger.LogError("btGo_Click(sender={Sender},e={E}): groupConsoleModule → {GroupConsoleModule}", sender, e, groupConsoleModule);
            TabInfo memberTab = this.CreatePage(this.PortalSettings.ActiveTab, this.PortalId, tab.TabID, "Members", true);
			eventLogger.LogError("btGo_Click(sender={Sender},e={E}): memberTab → {MemberTab}", sender, e, memberTab);
            ModuleController.Instance.CopyModule(groupConsoleModule, memberTab, "RightPane", true);

            ModuleInfo groupViewModule = ModuleController.Instance.GetModule(groupViewModuleId, tab.TabID, false);
			eventLogger.LogError("btGo_Click(sender={Sender},e={E}): groupViewModule → {GroupViewModule}", sender, e, groupViewModule);
            ModuleController.Instance.CopyModule(groupViewModule, memberTab, "ContentPane", true);
            this.AddModule(memberTab, this.PortalId, "DotNetNuke.Modules.MemberDirectory", "ContentPane");

            // List Settings
            ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupLoadView, nameof(GroupMode.List));
            ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupViewPage, tab.TabID.ToString(CultureInfo.InvariantCulture));

            // Default Social Groups
            var defaultGroup = RoleController.GetRoleGroupByName(this.roleProvider, this.PortalId, Constants.DefaultGroupName);
			eventLogger.LogError("btGo_Click(sender={Sender},e={E}): defaultGroup → {DefaultGroup}", sender, e, defaultGroup);
            var groupId = -2;
			eventLogger.LogError("btGo_Click(sender={Sender},e={E}): groupId → {GroupId}", sender, e, groupId);
            if (defaultGroup != null)
            {
                groupId = defaultGroup.RoleGroupID;
            }
            else
            {
                var groupInfo = new RoleGroupInfo();
				eventLogger.LogError("btGo_Click(sender={Sender},e={E}): groupInfo → {GroupInfo}", sender, e, groupInfo);
				eventLogger.LogDebug("btGo_Click(sender={Sender},e={E}): groupInfo.PortalID → {Arg2}", sender, e, groupInfo.PortalID);
				eventLogger.LogDebug("btGo_Click(sender={Sender},e={E}): groupInfo.RoleGroupName → {Arg2}", sender, e, groupInfo.RoleGroupName);
				eventLogger.LogDebug("btGo_Click(sender={Sender},e={E}): groupInfo.Description → {Arg2}", sender, e, groupInfo.Description);
                groupInfo.PortalID = this.PortalId;
                groupInfo.RoleGroupName = Constants.DefaultGroupName;
                groupInfo.Description = Constants.DefaultGroupName;
                groupId = RoleController.AddRoleGroup(this.roleProvider, this.eventLogger, this.userController, this.PortalSettings, groupInfo);
            }

            ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.DefaultRoleGroupSetting, groupId.ToString());

            this.Response.Redirect(this.Request.RawUrl);
			eventLogger.LogWarning("<<< Exiting btGo_Click(sender={Sender},e={E})", sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
			eventLogger.LogError(">>> Entering Page_Load(sender={Sender},e={E})", sender, e);
            this.btnGo.Visible = this.Request.IsAuthenticated;
            this.btnGo.Enabled = this.Request.IsAuthenticated;
            this.btnGo.Click += this.btGo_Click;
			eventLogger.LogWarning("<<< Exiting Page_Load(sender={Sender},e={E})", sender, e);
        }

        private static int GetDesktopModuleId(IHostSettings hostSettings, int portalId, string moduleName)
        {
			eventLogger.LogError(">>> Entering GetDesktopModuleId(hostSettings={HostSettings},portalId={PortalId},moduleName={ModuleName})", hostSettings, portalId, moduleName);
            var info = DesktopModuleController.GetDesktopModuleByModuleName(hostSettings, moduleName, portalId);
			eventLogger.LogError("GetDesktopModuleId(hostSettings={HostSettings},portalId={PortalId},moduleName={ModuleName}): info → {Info}", hostSettings, portalId, moduleName, info);
			eventLogger.LogWarning("<<< Exiting GetDesktopModuleId(hostSettings={HostSettings},portalId={PortalId},moduleName={ModuleName})", hostSettings, portalId, moduleName);
            return info?.DesktopModuleID ?? -1;
        }

        private static IPermissionInfo AddModulePermission(ModuleInfo objModule, IPermissionDefinitionInfo permission, int roleId, int userId, bool allowAccess)
        {
			eventLogger.LogError(">>> Entering AddModulePermission(objModule={ObjModule},permission={Permission},roleId={RoleId},userId={UserId},allowAccess={AllowAccess})", objModule, permission, roleId, userId, allowAccess);
            IPermissionInfo objModulePermission = new ModulePermissionInfo
            {
                ModuleID = objModule.ModuleID,
                PermissionKey = permission.PermissionKey,
                AllowAccess = allowAccess,
            };

            objModulePermission.PermissionId = permission.PermissionId;
			eventLogger.LogDebug("AddModulePermission(objModule={ObjModule},permission={Permission},roleId={RoleId},userId={UserId},allowAccess={AllowAccess}): objModulePermission.PermissionId → {Arg5}", objModule, permission, roleId, userId, allowAccess, objModulePermission.PermissionId);
			eventLogger.LogDebug("AddModulePermission(objModule={ObjModule},permission={Permission},roleId={RoleId},userId={UserId},allowAccess={AllowAccess}): objModulePermission.RoleId → {Arg5}", objModule, permission, roleId, userId, allowAccess, objModulePermission.RoleId);
			eventLogger.LogDebug("AddModulePermission(objModule={ObjModule},permission={Permission},roleId={RoleId},userId={UserId},allowAccess={AllowAccess}): objModulePermission.UserId → {Arg5}", objModule, permission, roleId, userId, allowAccess, objModulePermission.UserId);
            objModulePermission.RoleId = roleId;
            objModulePermission.UserId = userId;

            // add the permission to the collection
            objModule.ModulePermissions ??= [];
            if (!objModule.ModulePermissions.Contains(objModulePermission))
            {
                objModule.ModulePermissions.Add((ModulePermissionInfo)objModulePermission);
            }

			eventLogger.LogWarning("<<< Exiting AddModulePermission(objModule={ObjModule},permission={Permission},roleId={RoleId},userId={UserId},allowAccess={AllowAccess})", objModule, permission, roleId, userId, allowAccess);
            return objModulePermission;
        }

        private TabInfo CreatePage(TabInfo tab, int portalId, int parentTabId, string tabName, bool includeInMenu)
        {
			eventLogger.LogError(">>> Entering CreatePage(tab={Tab},portalId={PortalId},parentTabId={ParentTabId},tabName={TabName},includeInMenu={IncludeInMenu})", tab, portalId, parentTabId, tabName, includeInMenu);
            var newTab = TabController.Instance.GetTabsByPortal(portalId).WithTabNameAndParentId(tabName, parentTabId);
			eventLogger.LogError("CreatePage(tab={Tab},portalId={PortalId},parentTabId={ParentTabId},tabName={TabName},includeInMenu={IncludeInMenu}): newTab → {NewTab}", tab, portalId, parentTabId, tabName, includeInMenu, newTab);
            if (newTab == null)
            {
                int id = -1;
				eventLogger.LogError("CreatePage(tab={Tab},portalId={PortalId},parentTabId={ParentTabId},tabName={TabName},includeInMenu={IncludeInMenu}): id → {Id}", tab, portalId, parentTabId, tabName, includeInMenu, id);
                newTab = new TabInfo();

                if (tab != null)
                {
                    foreach (IPermissionInfo t in tab.TabPermissions)
                    {
                        IPermissionInfo tNew = new TabPermissionInfo
                        {
                            AllowAccess = t.AllowAccess,
                            DisplayName = t.DisplayName,
                            PermissionCode = t.PermissionCode,
                            PermissionKey = t.PermissionKey,
                            PermissionName = t.PermissionName,
                            RoleName = t.RoleName,
                            TabID = -1,
                            TabPermissionID = -1,
                            Username = t.Username,
                        };

                        tNew.UserId = t.UserId;
						eventLogger.LogDebug("CreatePage(tab={Tab},portalId={PortalId},parentTabId={ParentTabId},tabName={TabName},includeInMenu={IncludeInMenu}): tNew.UserId → {Arg5}", tab, portalId, parentTabId, tabName, includeInMenu, tNew.UserId);
						eventLogger.LogDebug("CreatePage(tab={Tab},portalId={PortalId},parentTabId={ParentTabId},tabName={TabName},includeInMenu={IncludeInMenu}): tNew.ModuleDefId → {Arg5}", tab, portalId, parentTabId, tabName, includeInMenu, tNew.ModuleDefId);
						eventLogger.LogDebug("CreatePage(tab={Tab},portalId={PortalId},parentTabId={ParentTabId},tabName={TabName},includeInMenu={IncludeInMenu}): tNew.PermissionId → {Arg5}", tab, portalId, parentTabId, tabName, includeInMenu, tNew.PermissionId);
						eventLogger.LogDebug("CreatePage(tab={Tab},portalId={PortalId},parentTabId={ParentTabId},tabName={TabName},includeInMenu={IncludeInMenu}): tNew.RoleId → {Arg5}", tab, portalId, parentTabId, tabName, includeInMenu, tNew.RoleId);
                        tNew.ModuleDefId = t.ModuleDefId;
                        tNew.PermissionId = t.PermissionId;
                        tNew.RoleId = t.RoleId;

                        newTab.TabPermissions.Add((TabPermissionInfo)tNew);
                    }
                }

                newTab.ParentId = parentTabId;
				eventLogger.LogDebug("CreatePage(tab={Tab},portalId={PortalId},parentTabId={ParentTabId},tabName={TabName},includeInMenu={IncludeInMenu}): newTab.ParentId → {Arg5}", tab, portalId, parentTabId, tabName, includeInMenu, newTab.ParentId);
				eventLogger.LogDebug("CreatePage(tab={Tab},portalId={PortalId},parentTabId={ParentTabId},tabName={TabName},includeInMenu={IncludeInMenu}): newTab.PortalID → {Arg5}", tab, portalId, parentTabId, tabName, includeInMenu, newTab.PortalID);
				eventLogger.LogDebug("CreatePage(tab={Tab},portalId={PortalId},parentTabId={ParentTabId},tabName={TabName},includeInMenu={IncludeInMenu}): newTab.TabName → {Arg5}", tab, portalId, parentTabId, tabName, includeInMenu, newTab.TabName);
				eventLogger.LogDebug("CreatePage(tab={Tab},portalId={PortalId},parentTabId={ParentTabId},tabName={TabName},includeInMenu={IncludeInMenu}): newTab.Title → {Arg5}", tab, portalId, parentTabId, tabName, includeInMenu, newTab.Title);
				eventLogger.LogDebug("CreatePage(tab={Tab},portalId={PortalId},parentTabId={ParentTabId},tabName={TabName},includeInMenu={IncludeInMenu}): newTab.IsVisible → {Arg5}", tab, portalId, parentTabId, tabName, includeInMenu, newTab.IsVisible);
                newTab.PortalID = portalId;
                newTab.TabName = tabName;
                newTab.Title = tabName;
                newTab.IsVisible = includeInMenu;
                newTab.SkinSrc = this.GetSkin();

                id = TabController.Instance.AddTab(newTab);
                newTab = TabController.Instance.GetTab(id, portalId, true);
            }

			eventLogger.LogWarning("<<< Exiting CreatePage(tab={Tab},portalId={PortalId},parentTabId={ParentTabId},tabName={TabName},includeInMenu={IncludeInMenu})", tab, portalId, parentTabId, tabName, includeInMenu);
            return newTab;
        }

        private string GetSkin()
        {
			eventLogger.LogError(">>> Entering GetSkin()");
            // attempt to find and load a  skin from the assigned skinned source
            var skinSource = this.PortalSettings.DefaultPortalSkin;
			eventLogger.LogError("GetSkin(): skinSource → {SkinSource}", skinSource);

            var tab = TabController.Instance.GetTab(this.TabId, this.PortalId, false);
			eventLogger.LogError("GetSkin(): tab → {Tab}", tab);

            if (!string.IsNullOrEmpty(tab.SkinSrc))
            {
                skinSource = tab.SkinSrc;
            }
            else
            {
                skinSource = SkinController.FormatSkinPath(skinSource) + "groups.ascx";
                var physicalSkinFile = SkinController.FormatSkinSrc(skinSource, this.PortalSettings);
				eventLogger.LogError("GetSkin(): physicalSkinFile → {PhysicalSkinFile}", physicalSkinFile);

                if (!File.Exists(HttpContext.Current.Server.MapPath(physicalSkinFile)))
                {
                    skinSource = string.Empty; // this will load the default skin
                }
            }

			eventLogger.LogWarning("<<< Exiting GetSkin()");
            return skinSource;
        }

        private int AddModule(TabInfo tab, int portalId, string moduleName, string pane)
        {
			eventLogger.LogError(">>> Entering AddModule(tab={Tab},portalId={PortalId},moduleName={ModuleName},pane={Pane})", tab, portalId, moduleName, pane);
            var module = ModuleController.Instance.GetTabModules(tab.TabID).Values.SingleOrDefault(m => m.DesktopModule.ModuleName == moduleName);
            int id = -1;
			eventLogger.LogError("AddModule(tab={Tab},portalId={PortalId},moduleName={ModuleName},pane={Pane}): module → {Module}", tab, portalId, moduleName, pane, module);
            if (module == null)
            {
				eventLogger.LogError("AddModule(tab={Tab},portalId={PortalId},moduleName={ModuleName},pane={Pane}): id → {Id}", tab, portalId, moduleName, pane, id);
                int desktopModuleId = GetDesktopModuleId(this.hostSettings, portalId, moduleName);
				eventLogger.LogError("AddModule(tab={Tab},portalId={PortalId},moduleName={ModuleName},pane={Pane}): desktopModuleId → {DesktopModuleId}", tab, portalId, moduleName, pane, desktopModuleId);
                int moduleId = -1;
				eventLogger.LogError("AddModule(tab={Tab},portalId={PortalId},moduleName={ModuleName},pane={Pane}): moduleId → {ModuleId}", tab, portalId, moduleName, pane, moduleId);
                if (desktopModuleId > -1)
                {
                    if (moduleId <= 0)
                    {
                        moduleId = this.AddNewModule(tab, string.Empty, desktopModuleId, pane, 0, string.Empty);
                    }

                    id = moduleId;
                    ModuleInfo mi = ModuleController.Instance.GetModule(moduleId, tab.TabID, false);
					eventLogger.LogError("AddModule(tab={Tab},portalId={PortalId},moduleName={ModuleName},pane={Pane}): mi → {Mi}", tab, portalId, moduleName, pane, mi);
                    if (moduleName == "Social Groups")
                    {
                        ModuleController.Instance.UpdateTabModuleSetting(mi.TabModuleID, Constants.GroupLoadView, nameof(GroupMode.View));
                        ModuleController.Instance.UpdateTabModuleSetting(mi.TabModuleID, Constants.GroupListPage, tab.TabID.ToString(CultureInfo.InvariantCulture));
                    }

                    if (moduleName == "Console")
                    {
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "AllowSizeChange", "False");
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "AllowViewChange", "False");
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "IncludeParent", "True");
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "Mode", "Group");
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "DefaultSize", "IconNone");
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "ParentTabID", tab.TabID.ToString(CultureInfo.InvariantCulture));
                    }

                    if (moduleName == "DotNetNuke.Modules.MemberDirectory")
                    {
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "FilterBy", "Group");
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "FilterPropertyValue", string.Empty);
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "FilterValue", "-1");
                        ModuleController.Instance.UpdateTabModuleSetting(mi.TabModuleID, "DisplaySearch", "False");
                    }
                }
            }
            else
            {
                id = module.ModuleID;
            }

			eventLogger.LogWarning("<<< Exiting AddModule(tab={Tab},portalId={PortalId},moduleName={ModuleName},pane={Pane})", tab, portalId, moduleName, pane);
            return id;
        }

        private int AddNewModule(TabInfo tab, string title, int desktopModuleId, string paneName, int permissionType, string align)
        {
			eventLogger.LogError(">>> Entering AddNewModule(tab={Tab},title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},permissionType={PermissionType},align={Align})", tab, title, desktopModuleId, paneName, permissionType, align);
            foreach (var objModuleDefinition in ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId).Values)
            {
                var objModule = new ModuleInfo();
				eventLogger.LogError("AddNewModule(tab={Tab},title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},permissionType={PermissionType},align={Align}): objModule → {ObjModule}", tab, title, desktopModuleId, paneName, permissionType, align, objModule);
                objModule.Initialize(tab.PortalID);

                objModule.PortalID = tab.PortalID;
				eventLogger.LogDebug("AddNewModule(tab={Tab},title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},permissionType={PermissionType},align={Align}): objModule.PortalID → {Arg6}", tab, title, desktopModuleId, paneName, permissionType, align, objModule.PortalID);
				eventLogger.LogDebug("AddNewModule(tab={Tab},title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},permissionType={PermissionType},align={Align}): objModule.TabID → {Arg6}", tab, title, desktopModuleId, paneName, permissionType, align, objModule.TabID);
                objModule.TabID = tab.TabID;
                if (string.IsNullOrEmpty(title))
                {
                    objModule.ModuleTitle = objModuleDefinition.FriendlyName;
					eventLogger.LogDebug("AddNewModule(tab={Tab},title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},permissionType={PermissionType},align={Align}): objModule.ModuleTitle → {Arg6}", tab, title, desktopModuleId, paneName, permissionType, align, objModule.ModuleTitle);
                }
                else
                {
                    objModule.ModuleTitle = title;
                }

                objModule.PaneName = paneName;
				eventLogger.LogDebug("AddNewModule(tab={Tab},title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},permissionType={PermissionType},align={Align}): objModule.PaneName → {Arg6}", tab, title, desktopModuleId, paneName, permissionType, align, objModule.PaneName);
				eventLogger.LogDebug("AddNewModule(tab={Tab},title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},permissionType={PermissionType},align={Align}): objModule.ModuleDefID → {Arg6}", tab, title, desktopModuleId, paneName, permissionType, align, objModule.ModuleDefID);
				eventLogger.LogDebug("AddNewModule(tab={Tab},title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},permissionType={PermissionType},align={Align}): objModule.CacheTime → {Arg6}", tab, title, desktopModuleId, paneName, permissionType, align, objModule.CacheTime);
				eventLogger.LogDebug("AddNewModule(tab={Tab},title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},permissionType={PermissionType},align={Align}): objModule.InheritViewPermissions → {Arg6}", tab, title, desktopModuleId, paneName, permissionType, align, objModule.InheritViewPermissions);
				eventLogger.LogDebug("AddNewModule(tab={Tab},title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},permissionType={PermissionType},align={Align}): objModule.DisplayTitle → {Arg6}", tab, title, desktopModuleId, paneName, permissionType, align, objModule.DisplayTitle);
                objModule.ModuleDefID = objModuleDefinition.ModuleDefID;
                objModule.CacheTime = 0;
                objModule.InheritViewPermissions = true;
                objModule.DisplayTitle = false;

                // get the default module view permissions
                var moduleViewPermission = this.permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW").FirstOrDefault();
				eventLogger.LogError("AddNewModule(tab={Tab},title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},permissionType={PermissionType},align={Align}): moduleViewPermission → {ModuleViewPermission}", tab, title, desktopModuleId, paneName, permissionType, align, moduleViewPermission);

                // get the permissions from the page
                foreach (IPermissionInfo objTabPermission in tab.TabPermissions)
                {
                    if (objTabPermission.PermissionKey == "VIEW" && permissionType == 0)
                    {
                        // Don't need to explicitly add View permissions if "Same As Page"
                        continue;
                    }

                    // loop through the system module permissions
                    foreach (var objSystemModulePermission in this.permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_MODULE_DEFINITION", objTabPermission.PermissionKey))
                    {
                        // create the module permission
                        if (objSystemModulePermission.PermissionKey == "VIEW" && permissionType == 1 && objTabPermission.PermissionKey != "EDIT")
                        {
                            // Only Page Editors get View permissions if "Page Editors Only"
                            continue;
                        }

                        var objModulePermission = AddModulePermission(
                            objModule,
                            objSystemModulePermission,
                            objTabPermission.RoleId,
                            objTabPermission.UserId,
                            objTabPermission.AllowAccess);

                        // ensure that every EDIT permission which allows access also provides VIEW permission
                        if (objModulePermission.PermissionKey == "EDIT" & objModulePermission.AllowAccess)
                        {
                            AddModulePermission(
                                objModule,
                                moduleViewPermission,
                                objModulePermission.RoleId,
                                objModulePermission.UserId,
                                true);
                        }
                    }
                }

                objModule.AllTabs = false;
				eventLogger.LogDebug("AddNewModule(tab={Tab},title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},permissionType={PermissionType},align={Align}): objModule.AllTabs → {Arg6}", tab, title, desktopModuleId, paneName, permissionType, align, objModule.AllTabs);
				eventLogger.LogDebug("AddNewModule(tab={Tab},title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},permissionType={PermissionType},align={Align}): objModule.Alignment → {Arg6}", tab, title, desktopModuleId, paneName, permissionType, align, objModule.Alignment);
                objModule.Alignment = align;

                return ModuleController.Instance.AddModule(objModule);
            }

			eventLogger.LogWarning("<<< Exiting AddNewModule(tab={Tab},title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},permissionType={PermissionType},align={Align})", tab, title, desktopModuleId, paneName, permissionType, align);
            return -1;
        }
    }
}
