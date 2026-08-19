// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.ControlPanels
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The ControlPanel class defines a custom base class inherited by all ControlPanel controls.</summary>
    public class ControlPanelBase : UserControl
    {
        private readonly IEventLogger eventLogger;
        private readonly IPermissionDefinitionService permissionDefinitionService;
        private string localResourceFile;

        /// <summary>Initializes a new instance of the <see cref="ControlPanelBase"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public ControlPanelBase()
            : this(null, null)
        {
		eventLogger.LogDebug(">>> Entering ControlPanelBase()");
        }

		eventLogger.LogDebug("<<< Exiting ControlPanelBase()");
        /// <summary>Initializes a new instance of the <see cref="ControlPanelBase"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        public ControlPanelBase(IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService)
        {
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
			eventLogger.LogDebug("ControlPanelBase(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService}): permissionDefinitionService → {PermissionDefinitionService1}", eventLogger, permissionDefinitionService, permissionDefinitionService);
        }

        protected enum ViewPermissionType
        {
            /// <summary>Permission to view.</summary>
            View = 0,

            /// <summary>Permission to edit.</summary>
            Edit = 1,
        }

        public virtual bool IncludeInControlHierarchy => true;

        /// <summary>Gets or sets the Local ResourceFile for the Control Panel.</summary>
        /// <value>A String.</value>
        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(this.localResourceFile))
                {
                    fileRoot = $"{this.TemplateSourceDirectory}/{Localization.LocalResourceDirectory}/{this.ID}";
                }
                else
                {
                    fileRoot = this.localResourceFile;
                }

                return fileRoot;
            }

            set
            {
                this.localResourceFile = value;
            }
        }

        public virtual bool IsDockable
        {
            get { return false; }
            set { }
        }

        /// <summary>Gets a value indicating whether the ControlPanel is Visible.</summary>
        protected bool IsVisible => this.PortalSettings.ControlPanelVisible;

        /// <summary>Gets the current Portal Settings.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected PortalSettings PortalSettings => PortalSettings.Current;

        /// <summary>Gets the User mode of the Control Panel.</summary>
        /// <value>A Boolean.</value>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected PortalSettings.Mode UserMode => Personalization.GetUserMode();

        internal static bool IsModuleAdminInternal()
        {
			eventLogger.LogDebug(">>> Entering IsModuleAdminInternal()");
            bool isModuleAdmin = Null.NullBoolean;
            foreach (ModuleInfo objModule in TabController.CurrentPage.Modules)
            {
			eventLogger.LogDebug("IsModuleAdminInternal(): isModuleAdmin → {IsModuleAdmin}", isModuleAdmin);
                if (!objModule.IsDeleted)
                {
                    bool blnHasModuleEditPermissions = ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, Null.NullString, objModule);
                    if (blnHasModuleEditPermissions)
                    {
                        isModuleAdmin = true;
                        break;
                    }
                }
            }

			eventLogger.LogDebug("<<< Exiting IsModuleAdminInternal()");
            return PortalSettings.Current.ControlPanelSecurity == PortalSettings.ControlPanelPermission.ModuleEditor && isModuleAdmin;
        }

        internal static bool IsPageAdminInternal()
        {
			eventLogger.LogDebug(">>> Entering IsPageAdminInternal()");
            bool isPageAdmin = Null.NullBoolean;
            if (TabPermissionController.CanAddContentToPage() || TabPermissionController.CanAddPage() || TabPermissionController.CanAdminPage() || TabPermissionController.CanCopyPage() ||
                TabPermissionController.CanDeletePage() || TabPermissionController.CanExportPage() || TabPermissionController.CanImportPage() || TabPermissionController.CanManagePage())
            {
				eventLogger.LogDebug("IsPageAdminInternal(): isPageAdmin → {IsPageAdmin}", isPageAdmin);
                isPageAdmin = true;
            }

			eventLogger.LogDebug("<<< Exiting IsPageAdminInternal()");
            return isPageAdmin;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected bool IsModuleAdmin() => IsModuleAdminInternal();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected bool IsPageAdmin() => IsPageAdminInternal();

        /// <summary>Adds an Existing Module to a Pane.</summary>
        /// <param name="moduleId">The ID of the existing module.</param>
        /// <param name="tabId">The id of the tab.</param>
        /// <param name="paneName">The pane to add the module to.</param>
        /// <param name="position">The relative position within the pane for the module.</param>
        /// <param name="align">The alignment for the Module.</param>
        protected void AddExistingModule(int moduleId, int tabId, string paneName, int position, string align)
        {
			eventLogger.LogDebug(">>> Entering AddExistingModule(moduleId={ModuleId},tabId={TabId},paneName={PaneName},position={Position},align={Align})", moduleId, tabId, paneName, position, align);
            ModuleInfo objModule;

            int userId = -1;
			eventLogger.LogDebug("AddExistingModule(moduleId={ModuleId},tabId={TabId},paneName={PaneName},position={Position},align={Align}): userId → {UserId}", moduleId, tabId, paneName, position, align, userId);
            if (this.Request.IsAuthenticated)
            {
                UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
				eventLogger.LogDebug("AddExistingModule(moduleId={ModuleId},tabId={TabId},paneName={PaneName},position={Position},align={Align}): objUserInfo → {ObjUserInfo}", moduleId, tabId, paneName, position, align, objUserInfo);
                userId = objUserInfo.UserID;
            }

            objModule = ModuleController.Instance.GetModule(moduleId, tabId, false);
            if (objModule != null)
            {
                // clone the module object ( to avoid creating an object reference to the data cache )
                ModuleInfo objClone = objModule.Clone();
				eventLogger.LogDebug("AddExistingModule(moduleId={ModuleId},tabId={TabId},paneName={PaneName},position={Position},align={Align}): objClone → {ObjClone}", moduleId, tabId, paneName, position, align, objClone);
				eventLogger.LogDebug("AddExistingModule(moduleId={ModuleId},tabId={TabId},paneName={PaneName},position={Position},align={Align}): objClone.TabID → {Arg5}", moduleId, tabId, paneName, position, align, objClone.TabID);
				eventLogger.LogDebug("AddExistingModule(moduleId={ModuleId},tabId={TabId},paneName={PaneName},position={Position},align={Align}): objClone.ModuleOrder → {Arg5}", moduleId, tabId, paneName, position, align, objClone.ModuleOrder);
				eventLogger.LogDebug("AddExistingModule(moduleId={ModuleId},tabId={TabId},paneName={PaneName},position={Position},align={Align}): objClone.PaneName → {Arg5}", moduleId, tabId, paneName, position, align, objClone.PaneName);
				eventLogger.LogDebug("AddExistingModule(moduleId={ModuleId},tabId={TabId},paneName={PaneName},position={Position},align={Align}): objClone.Alignment → {Arg5}", moduleId, tabId, paneName, position, align, objClone.Alignment);
                objClone.TabID = this.PortalSettings.ActiveTab.TabID;
                objClone.ModuleOrder = position;
                objClone.PaneName = paneName;
                objClone.Alignment = align;
                ModuleController.Instance.AddModule(objClone);
                this.eventLogger.AddLog(objClone, this.PortalSettings, userId, string.Empty, EventLogType.MODULE_CREATED);
            }
			eventLogger.LogDebug("<<< Exiting AddExistingModule(moduleId={ModuleId},tabId={TabId},paneName={PaneName},position={Position},align={Align})", moduleId, tabId, paneName, position, align);
        }

        /// <summary>Adds a New Module to a Pane. </summary>
        /// <param name="title">The Title for the resulting module.</param>
        /// <param name="desktopModuleId">The ID of the DesktopModule.</param>
        /// <param name="paneName">The pane to add the module to.</param>
        /// <param name="position">The relative position within the pane for the module.</param>
        /// <param name="permissionType">The View Permission Type for the Module.</param>
        /// <param name="align">The alignment for the Module.</param>
        protected void AddNewModule(string title, int desktopModuleId, string paneName, int position, ViewPermissionType permissionType, string align)
        {
			eventLogger.LogDebug(">>> Entering AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align})", title, desktopModuleId, paneName, position, permissionType, align);
            TabPermissionCollection objTabPermissions = this.PortalSettings.ActiveTab.TabPermissions;
            try
            {
                if (!DesktopModuleController.GetDesktopModules(this.PortalSettings.PortalId).TryGetValue(desktopModuleId, out _))
                {
					eventLogger.LogWarning("Throwing ArgumentException from AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align})", title, desktopModuleId, paneName, position, permissionType, align);
                    throw new ArgumentException($"Could not find desktop module with given ID: {desktopModuleId}", nameof(desktopModuleId));
                }
            }
            catch (Exception ex)
            {
				eventLogger.LogError("Exception in AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): {Arg6}", title, desktopModuleId, paneName, position, permissionType, align, ex.getMessage());
                Exceptions.LogException(ex);
            }

            foreach (var objModuleDefinition in ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId).Values)
            {
                var objModule = new ModuleInfo();
				eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule → {ObjModule}", title, desktopModuleId, paneName, position, permissionType, align, objModule);
                objModule.Initialize(this.PortalSettings.PortalId);
                objModule.PortalID = this.PortalSettings.PortalId;
				eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.PortalID → {Arg6}", title, desktopModuleId, paneName, position, permissionType, align, objModule.PortalID);
				eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.TabID → {Arg6}", title, desktopModuleId, paneName, position, permissionType, align, objModule.TabID);
				eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.ModuleOrder → {Arg6}", title, desktopModuleId, paneName, position, permissionType, align, objModule.ModuleOrder);
                objModule.TabID = this.PortalSettings.ActiveTab.TabID;
                objModule.ModuleOrder = position;
                if (string.IsNullOrEmpty(title))
                {
                    objModule.ModuleTitle = objModuleDefinition.FriendlyName;
					eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.ModuleTitle → {Arg6}", title, desktopModuleId, paneName, position, permissionType, align, objModule.ModuleTitle);
                }
                else
                {
                    objModule.ModuleTitle = title;
                }

                objModule.PaneName = paneName;
				eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.PaneName → {Arg6}", title, desktopModuleId, paneName, position, permissionType, align, objModule.PaneName);
				eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.ModuleDefID → {Arg6}", title, desktopModuleId, paneName, position, permissionType, align, objModule.ModuleDefID);
                objModule.ModuleDefID = objModuleDefinition.ModuleDefID;
                if (objModuleDefinition.DefaultCacheTime > 0)
                {
                    objModule.CacheTime = objModuleDefinition.DefaultCacheTime;
					eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.CacheTime → {Arg6}", title, desktopModuleId, paneName, position, permissionType, align, objModule.CacheTime);
                    if (PortalSettings.Current.DefaultModuleId > Null.NullInteger && PortalSettings.Current.DefaultTabId > Null.NullInteger)
                    {
                        ModuleInfo defaultModule = ModuleController.Instance.GetModule(PortalSettings.Current.DefaultModuleId, PortalSettings.Current.DefaultTabId, true);
						eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): defaultModule → {DefaultModule}", title, desktopModuleId, paneName, position, permissionType, align, defaultModule);
                        if (defaultModule != null)
                        {
                            objModule.CacheTime = defaultModule.CacheTime;
                        }
                    }
                }

                switch (permissionType)
                {
                    case ViewPermissionType.View:
                        objModule.InheritViewPermissions = true;
						eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.InheritViewPermissions → {Arg6}", title, desktopModuleId, paneName, position, permissionType, align, objModule.InheritViewPermissions);
                        break;
                    case ViewPermissionType.Edit:
                        objModule.InheritViewPermissions = false;
                        break;
                }

                // get the default module view permissions
                var systemModuleViewPermission = this.permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW").FirstOrDefault();
				eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): systemModuleViewPermission → {SystemModuleViewPermission}", title, desktopModuleId, paneName, position, permissionType, align, systemModuleViewPermission);

                // get the permissions from the page
                foreach (IPermissionInfo objTabPermission in objTabPermissions)
                {
                    if (objTabPermission.PermissionKey == "VIEW" && permissionType == ViewPermissionType.View)
                    {
                        // Don't need to explicitly add View permissions if "Same As Page"
                        continue;
                    }

                    // get the system module permissions for the permission key
                    var systemModulePermissions = this.permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_MODULE_DEFINITION", objTabPermission.PermissionKey);
					eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): systemModulePermissions → {SystemModulePermissions}", title, desktopModuleId, paneName, position, permissionType, align, systemModulePermissions);

                    // loop through the system module permissions
                    foreach (var objSystemModulePermission in systemModulePermissions)
                    {
                        if (objSystemModulePermission.PermissionKey == "VIEW" && permissionType == ViewPermissionType.Edit && objTabPermission.PermissionKey != "EDIT")
                        {
                            // Only Page Editors get View permissions if "Page Editors Only"
                            continue;
                        }

                        IPermissionInfo objModulePermission = AddModulePermission(
                            objModule,
                            objSystemModulePermission,
                            objTabPermission.RoleId,
                            objTabPermission.UserId,
                            objTabPermission.AllowAccess);

                        // ensure that every EDIT permission which allows access also provides VIEW permission
                        if (objModulePermission.PermissionKey == "EDIT" && objModulePermission.AllowAccess)
                        {
                            AddModulePermission(
                                objModule,
                                systemModuleViewPermission,
                                objModulePermission.RoleId,
                                objModulePermission.UserId,
                                true);
                        }
                    }

                    // Get the custom Module Permissions,  Assume that roles with Edit Tab Permissions
                    // are automatically assigned to the Custom Module Permissions
                    if (objTabPermission.PermissionKey == "EDIT")
                    {
                        // loop through the custom module permissions
                        foreach (var objCustomModulePermission in this.permissionDefinitionService.GetDefinitionsByModuleDefId(objModule.ModuleDefID))
                        {
                            // create the module permission
                            AddModulePermission(objModule, objCustomModulePermission, objTabPermission.RoleId, objTabPermission.UserId, objTabPermission.AllowAccess);
                        }
                    }
                }

                if (PortalSettings.Current.ContentLocalizationEnabled)
                {
                    Locale defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalSettings.Current.PortalId);
					eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): defaultLocale → {DefaultLocale}", title, desktopModuleId, paneName, position, permissionType, align, defaultLocale);

                    // set the culture of the module to that of the tab
                    var tabInfo = TabController.Instance.GetTab(objModule.TabID, PortalSettings.Current.PortalId, false);
					eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): tabInfo → {TabInfo}", title, desktopModuleId, paneName, position, permissionType, align, tabInfo);
                    objModule.CultureCode = tabInfo != null ? tabInfo.CultureCode : defaultLocale.Code;
                }
                else
                {
                    objModule.CultureCode = Null.NullString;
					eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.CultureCode → {Arg6}", title, desktopModuleId, paneName, position, permissionType, align, objModule.CultureCode);
                }

                objModule.AllTabs = false;
				eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.AllTabs → {Arg6}", title, desktopModuleId, paneName, position, permissionType, align, objModule.AllTabs);
				eventLogger.LogDebug("AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.Alignment → {Arg6}", title, desktopModuleId, paneName, position, permissionType, align, objModule.Alignment);
                objModule.Alignment = align;
                ModuleController.Instance.AddModule(objModule);
            }
			eventLogger.LogDebug("<<< Exiting AddNewModule(title={Title},desktopModuleId={DesktopModuleId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align})", title, desktopModuleId, paneName, position, permissionType, align);
        }

        /// <summary>Builds a URL to a page with a module matching the given definition <paramref name="friendlyName"/>.</summary>
        /// <param name="portalID">The ID of the portal.</param>
        /// <param name="friendlyName">The friendly name of the Module.</param>
        /// <returns>A formatted URL.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected string BuildURL(int portalID, string friendlyName)
        {
			eventLogger.LogDebug(">>> Entering BuildURL(portalID={PortalID},friendlyName={FriendlyName})", portalID, friendlyName);
            string strURL = "~/" + Globals.glbDefaultPage;
            ModuleInfo objModule = ModuleController.Instance.GetModuleByDefinition(portalID, friendlyName);
			eventLogger.LogDebug("BuildURL(portalID={PortalID},friendlyName={FriendlyName}): objModule → {ObjModule}", portalID, friendlyName, objModule);
			eventLogger.LogDebug("BuildURL(portalID={PortalID},friendlyName={FriendlyName}): strURL → {StrURL}", portalID, friendlyName, strURL);
            if (objModule != null)
            {
                if (portalID == Null.NullInteger)
                {
                    strURL = TestableGlobals.Instance.NavigateURL(objModule.TabID, true);
                }
                else
                {
                    strURL = TestableGlobals.Instance.NavigateURL(objModule.TabID);
                }
            }

			eventLogger.LogDebug("<<< Exiting BuildURL(portalID={PortalID},friendlyName={FriendlyName})", portalID, friendlyName);
            return strURL;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected bool GetModulePermission(int portalID, string friendlyName)
        {
			eventLogger.LogDebug(">>> Entering GetModulePermission(portalID={PortalID},friendlyName={FriendlyName})", portalID, friendlyName);
            bool allowAccess = Null.NullBoolean;
            ModuleInfo objModule = ModuleController.Instance.GetModuleByDefinition(portalID, friendlyName);
			eventLogger.LogDebug("GetModulePermission(portalID={PortalID},friendlyName={FriendlyName}): objModule → {ObjModule}", portalID, friendlyName, objModule);
			eventLogger.LogDebug("GetModulePermission(portalID={PortalID},friendlyName={FriendlyName}): allowAccess → {AllowAccess}", portalID, friendlyName, allowAccess);
            if (objModule != null)
            {
                allowAccess = ModulePermissionController.CanViewModule(objModule);
            }

			eventLogger.LogDebug("<<< Exiting GetModulePermission(portalID={PortalID},friendlyName={FriendlyName})", portalID, friendlyName);
            return allowAccess;
        }

        /// <summary>Sets the UserMode.</summary>
        /// <param name="userMode">The userMode to set.</param>
        protected void SetUserMode(string userMode)
        {
			eventLogger.LogDebug(">>> Entering SetUserMode(userMode={UserMode})", userMode);
            Personalization.SetProfile("Usability", "UserMode" + this.PortalSettings.PortalId, userMode.ToUpperInvariant());
			eventLogger.LogDebug("<<< Exiting SetUserMode(userMode={UserMode})", userMode);
        }

        /// <summary>Sets the current Visible Mode.</summary>
        /// <param name="isVisible">A flag indicating whether the Control Panel should be visible.</param>
        protected void SetVisibleMode(bool isVisible)
        {
			eventLogger.LogDebug(">>> Entering SetVisibleMode(isVisible={IsVisible})", isVisible);
            Personalization.SetProfile("Usability", "ControlPanelVisible" + this.PortalSettings.PortalId, isVisible.ToString());
			eventLogger.LogDebug("<<< Exiting SetVisibleMode(isVisible={IsVisible})", isVisible);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnInit(e={E})", e);
            if (this.Page.Items.Contains(typeof(ControlPanelBase)) && this.Page.Items[typeof(ControlPanelBase)] is ControlPanelBase)
            {
                this.Parent.Controls.Remove(this);
            }
            else
            {
                this.Page.Items[typeof(ControlPanelBase)] = this;
                base.OnInit(e);
            }
			eventLogger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <summary>Adds a Module Permission.</summary>
        /// <param name="objModule">Module Info.</param>
        /// <param name="permission">The permission to add.</param>
        /// <param name="roleId">The ID of the role to add the permission for.</param>
        /// <param name="userId">Operator.</param>
        /// <param name="allowAccess">Whether allow to access the module.</param>
        private static ModulePermissionInfo AddModulePermission(ModuleInfo objModule, IPermissionDefinitionInfo permission, int roleId, int userId, bool allowAccess)
        {
			eventLogger.LogDebug(">>> Entering AddModulePermission(objModule={ObjModule},permission={Permission},roleId={RoleId},userId={UserId},allowAccess={AllowAccess})", objModule, permission, roleId, userId, allowAccess);
            var objModulePermission = new ModulePermissionInfo
            {
                ModuleID = objModule.ModuleID,
                PermissionKey = permission.PermissionKey,
                AllowAccess = allowAccess,
            };
            ((IPermissionInfo)objModulePermission).PermissionId = permission.PermissionId;
            ((IPermissionInfo)objModulePermission).RoleId = roleId;
            ((IPermissionInfo)objModulePermission).UserId = userId;

            // add the permission to the collection
            if (!objModule.ModulePermissions.Contains(objModulePermission))
            {
                objModule.ModulePermissions.Add(objModulePermission);
            }

			eventLogger.LogDebug("<<< Exiting AddModulePermission(objModule={ObjModule},permission={Permission},roleId={RoleId},userId={UserId},allowAccess={AllowAccess})", objModule, permission, roleId, userId, allowAccess);
            return objModulePermission;
        }
    }
}
