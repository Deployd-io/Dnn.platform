// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    public class PermissionProvider
    {
        // Folder Permission Keys
        private const string AdminFolderPermissionKey = "WRITE";
        private const string AddFolderPermissionKey = "WRITE";
        private const string BrowseFolderPermissionKey = "BROWSE";
        private const string CopyFolderPermissionKey = "WRITE";
        private const string DeleteFolderPermissionKey = "WRITE";
        private const string ManageFolderPermissionKey = "WRITE";
        private const string ViewFolderPermissionKey = "READ";

        // Module Permission Keys
        private const string AdminModulePermissionKey = "EDIT";
        private const string ContentModulePermissionKey = "EDIT";
        private const string DeleteModulePermissionKey = "EDIT";
        private const string ExportModulePermissionKey = "EDIT";
        private const string ImportModulePermissionKey = "EDIT";
        private const string ManageModulePermissionKey = "EDIT";
        private const string ViewModulePermissionKey = "VIEW";

        // Page Permission Keys
        private const string AddPagePermissionKey = "EDIT";
        private const string AdminPagePermissionKey = "EDIT";
        private const string ContentPagePermissionKey = "EDIT";
        private const string CopyPagePermissionKey = "EDIT";
        private const string DeletePagePermissionKey = "EDIT";
        private const string ExportPagePermissionKey = "EDIT";
        private const string ImportPagePermissionKey = "EDIT";
        private const string ManagePagePermissionKey = "EDIT";
        private const string NavigatePagePermissionKey = "VIEW";
        private const string ViewPagePermissionKey = "VIEW";

        // Portal Permission Keys
        private const string TopPagePermissionKey = "ADDTOPLEVELPAGE";
        private const string PageAdminPermissionKey = "PAGEADMIN";

        private static SharedDictionary<int, DNNCacheDependency> cacheDependencyDict = new SharedDictionary<int, DNNCacheDependency>();

        private readonly DataProvider dataProvider = DataProvider.Instance();
        private readonly IEventLogger eventLogger;
        private readonly IPermissionDefinitionService permissionDefinitionService;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="PermissionProvider"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public PermissionProvider()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PermissionProvider"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="hostSettings">The host settings.</param>
        public PermissionProvider(IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService, IHostSettings hostSettings)
        {
			eventLogger.LogError(">>> Entering PermissionProvider(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings})", eventLogger, permissionDefinitionService, hostSettings);
			eventLogger.LogDebug("PermissionProvider(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings}): permissionDefinitionService → {PermissionDefinitionService1}", eventLogger, permissionDefinitionService, hostSettings, permissionDefinitionService);
			eventLogger.LogDebug("PermissionProvider(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings}): hostSettings → {HostSettings1}", eventLogger, permissionDefinitionService, hostSettings, hostSettings);
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
			eventLogger.LogWarning("<<< Exiting PermissionProvider(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings})", eventLogger, permissionDefinitionService, hostSettings);
            this.permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        }

        // return the provider
        public virtual string LocalResourceFile => Localization.GlobalResourceFile;

        public static PermissionProvider Instance()
        {
			eventLogger.LogError(">>> Entering Instance()");
            eventLogger.LogWarning("<<< Exiting Instance()");
            return ComponentFactory.GetComponent<PermissionProvider>();
        }

        public virtual bool SupportsFullControl()
        {
			eventLogger.LogError(">>> Entering SupportsFullControl()");
			eventLogger.LogWarning("<<< Exiting SupportsFullControl()");
            return true;
        }

        /// <summary>The portal editor can edit whole site's content, it should be only administrators by default.</summary>
        /// <returns><see langword="true"/> if the current user is in the administrator role for the current portal, otherwise <see langword="false"/>.</returns>
        public virtual bool IsPortalEditor()
        {
			eventLogger.LogError(">>> Entering IsPortalEditor()");
            var settings = PortalController.Instance.GetCurrentSettings();
			eventLogger.LogError("IsPortalEditor(): settings → {Settings}", settings);
			eventLogger.LogWarning("<<< Exiting IsPortalEditor()");
            return settings != null && PortalSecurity.IsInRole(settings.AdministratorRoleName);
        }

        /// <summary>Returns a flag indicating whether the current user can add a folder or file.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanAddFolder(FolderInfo folder)
        {
			eventLogger.LogError(">>> Entering CanAddFolder(folder={Folder})", folder);
            eventLogger.LogWarning("<<< Exiting CanAddFolder(folder={Folder})", folder);
            return HasFolderPermission(folder, AddFolderPermissionKey);
        }

        /// <summary>Returns a flag indicating whether the current user can browse the folder.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanBrowseFolder(FolderInfo folder)
        {
			eventLogger.LogError(">>> Entering CanBrowseFolder(folder={Folder})", folder);
            if (folder == null)
            {
                return false;
            }

			eventLogger.LogWarning("<<< Exiting CanBrowseFolder(folder={Folder})", folder);
            return (PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(BrowseFolderPermissionKey))
                    || PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(ViewFolderPermissionKey)))
                   && !PortalSecurity.IsDenied(folder.FolderPermissions.ToString(BrowseFolderPermissionKey));
        }

        /// <summary>Returns a flag indicating whether the current user can administer a folder.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanAdminFolder(FolderInfo folder)
        {
			eventLogger.LogError(">>> Entering CanAdminFolder(folder={Folder})", folder);
            if (folder == null)
            {
                return false;
            }

			eventLogger.LogWarning("<<< Exiting CanAdminFolder(folder={Folder})", folder);
            return PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(AdminFolderPermissionKey));
        }

        /// <summary>Returns a flag indicating whether the current user can copy a folder or file.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanCopyFolder(FolderInfo folder)
        {
			eventLogger.LogError(">>> Entering CanCopyFolder(folder={Folder})", folder);
            eventLogger.LogWarning("<<< Exiting CanCopyFolder(folder={Folder})", folder);
            return HasFolderPermission(folder, CopyFolderPermissionKey);
        }

        /// <summary>Returns a flag indicating whether the current user can delete a folder or file.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanDeleteFolder(FolderInfo folder)
        {
			eventLogger.LogError(">>> Entering CanDeleteFolder(folder={Folder})", folder);
            eventLogger.LogWarning("<<< Exiting CanDeleteFolder(folder={Folder})", folder);
            return HasFolderPermission(folder, DeleteFolderPermissionKey);
        }

        /// <summary>Returns a flag indicating whether the current user can manage a folder's settings.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanManageFolder(FolderInfo folder)
        {
			eventLogger.LogError(">>> Entering CanManageFolder(folder={Folder})", folder);
            eventLogger.LogWarning("<<< Exiting CanManageFolder(folder={Folder})", folder);
            return HasFolderPermission(folder, ManageFolderPermissionKey);
        }

        /// <summary>Returns a flag indicating whether the current user can view a folder or file.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanViewFolder(FolderInfo folder)
        {
			eventLogger.LogError(">>> Entering CanViewFolder(folder={Folder})", folder);
            eventLogger.LogWarning("<<< Exiting CanViewFolder(folder={Folder})", folder);
            return HasFolderPermission(folder, ViewFolderPermissionKey);
        }

        public virtual void DeleteFolderPermissionsByUser(UserInfo objUser)
        {
			eventLogger.LogError(">>> Entering DeleteFolderPermissionsByUser(objUser={ObjUser})", objUser);
            this.dataProvider.DeleteFolderPermissionsByUserID(objUser.PortalID, objUser.UserID);
			eventLogger.LogWarning("<<< Exiting DeleteFolderPermissionsByUser(objUser={ObjUser})", objUser);
        }

        public virtual FolderPermissionCollection GetFolderPermissionsCollectionByFolder(int portalID, string folder)
        {
#if fale
			eventLogger.LogError(">>> Entering GetFolderPermissionsCollectionByFolder(portalID={PortalID},folder={Folder})", portalID, folder);
            string dictionaryKey = Folder;
			eventLogger.LogError("GetFolderPermissionsCollectionByFolder(portalID={PortalID},folder={Folder}): dictionaryKey → {DictionaryKey}", portalID, folder, dictionaryKey);
            if (string.IsNullOrEmpty(dictionaryKey))
            {
                dictionaryKey = "[PortalRoot]";
            }
            //Get the Portal FolderPermission Dictionary
            Dictionary<string, FolderPermissionCollection> dicFolderPermissions = GetFolderPermissions(PortalID);
			eventLogger.LogError("GetFolderPermissionsCollectionByFolder(portalID={PortalID},folder={Folder}): dicFolderPermissions → {DicFolderPermissions}", portalID, folder, dicFolderPermissions);

            //Get the Collection from the Dictionary
            FolderPermissionCollection folderPermissions;
            bool bFound = dicFolderPermissions.TryGetValue(dictionaryKey, out folderPermissions);
			eventLogger.LogError("GetFolderPermissionsCollectionByFolder(portalID={PortalID},folder={Folder}): bFound → {BFound}", portalID, folder, bFound);
            if (!bFound)
            {
                //Return empty collection
                folderPermissions = new FolderPermissionCollection();
				eventLogger.LogError("GetFolderPermissionsCollectionByFolder(portalID={PortalID},folder={Folder}): folderPermissions → {FolderPermissions}", portalID, folder, folderPermissions);
				eventLogger.LogDebug("GetFolderPermissionsCollectionByFolder(portalID={PortalID},folder={Folder}): folderPermissions → {FolderPermissions}", portalID, folder, folderPermissions);
            }
			eventLogger.LogError("Exception in GetFolderPermissionsCollectionByFolder(portalID={PortalID},folder={Folder}): {Arg2}", portalID, folder, exc.Message);
			eventLogger.LogWarning("<<< Exiting GetFolderPermissionsCollectionByFolder(portalID={PortalID},folder={Folder})", portalID, folder);
            return folderPermissions;
#else
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.FolderPathPermissionCacheKey, portalID, folder);
            return CBO.GetCachedObject<FolderPermissionCollection>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.FolderPermissionCacheTimeOut, DataCache.FolderPermissionCachePriority)
                {
                    CacheDependency = GetCacheDependency(portalID),
                },
                _ =>
                {
                    var collection = new FolderPermissionCollection();
                    try
                    {
                        using (var dr = this.dataProvider.GetFolderPermissionsByPortalAndPath(portalID, folder))
                        {
                            while (dr.Read())
                            {
                                var folderPermissionInfo = CBO.FillObject<FolderPermissionInfo>(dr, false);
                                collection.Add(folderPermissionInfo);
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        Exceptions.LogException(exc);
                    }

                    return collection;
                });
#endif
        }

        public virtual bool HasFolderPermission(FolderPermissionCollection objFolderPermissions, string permissionKey)
        {
			eventLogger.LogError(">>> Entering HasFolderPermission(objFolderPermissions={ObjFolderPermissions},permissionKey={PermissionKey})", objFolderPermissions, permissionKey);
            eventLogger.LogWarning("<<< Exiting HasFolderPermission(objFolderPermissions={ObjFolderPermissions},permissionKey={PermissionKey})", objFolderPermissions, permissionKey);
			eventLogger.LogWarning("<<< Exiting HasFolderPermission(folder={Folder},permissionKey={PermissionKey})", folder, permissionKey);
            return PortalSecurity.IsInRoles(objFolderPermissions.ToString(permissionKey));
        }

        /// <summary>SaveFolderPermissions updates a Folder's permissions.</summary>
        /// <param name="folder">The Folder to update.</param>
        public virtual void SaveFolderPermissions(FolderInfo folder)
        {
            this.SaveFolderPermissions((IFolderInfo)folder);
			eventLogger.LogError(">>> Entering SaveFolderPermissions(folder={Folder})", folder);
			eventLogger.LogWarning("<<< Exiting SaveFolderPermissions(folder={Folder})", folder);
        }

        /// <summary>SaveFolderPermissions updates a Folder's permissions.</summary>
        /// <param name="folder">The Folder to update.</param>
        public virtual void SaveFolderPermissions(IFolderInfo folder)
        {
            if (folder.FolderPermissions != null)
            {
                // Ensure that if role/user has been given a permission that is not Read/Browse then they also need Read/Browse
                var readPerm = this.permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_FOLDER", "READ").FirstOrDefault();
				eventLogger.LogError("SaveFolderPermissions(folder={Folder}): readPerm → {ReadPerm}", folder, readPerm);
                var browsePerm = this.permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_FOLDER", "BROWSE").FirstOrDefault();
				eventLogger.LogError("SaveFolderPermissions(folder={Folder}): browsePerm → {BrowsePerm}", folder, browsePerm);

                var additionalPermissions = new FolderPermissionCollection();
				eventLogger.LogError("SaveFolderPermissions(folder={Folder}): additionalPermissions → {AdditionalPermissions}", folder, additionalPermissions);
                foreach (IFolderPermissionInfo folderPermission in folder.FolderPermissions)
                {
                    if (folderPermission.PermissionKey != "BROWSE" && folderPermission.PermissionKey != "READ" && folderPermission.AllowAccess)
                    {
                        // Try to add Read permission
                        var newFolderPerm = new FolderPermissionInfo(readPerm)
                        {
                            AllowAccess = true,
                        };
                        ((IFolderPermissionInfo)newFolderPerm).FolderId = folderPermission.FolderId;
                        ((IFolderPermissionInfo)newFolderPerm).RoleId = folderPermission.RoleId;
                        ((IFolderPermissionInfo)newFolderPerm).UserId = folderPermission.UserId;

                        additionalPermissions.Add(newFolderPerm);

                        // Try to add Browse permission
                        newFolderPerm = new FolderPermissionInfo(browsePerm)
                        {
                            AllowAccess = true,
                        };
                        ((IFolderPermissionInfo)newFolderPerm).FolderId = folderPermission.FolderId;
                        ((IFolderPermissionInfo)newFolderPerm).RoleId = folderPermission.RoleId;
                        ((IFolderPermissionInfo)newFolderPerm).UserId = folderPermission.UserId;

                        additionalPermissions.Add(newFolderPerm);
                    }
                }

                foreach (FolderPermissionInfo folderPermission in additionalPermissions)
                {
                    folder.FolderPermissions.Add(folderPermission, true);
                }

                this.dataProvider.DeleteFolderPermissionsByFolderPath(folder.PortalID, folder.FolderPath);
                foreach (IFolderPermissionInfo folderPermission in folder.FolderPermissions)
                {
                    this.dataProvider.AddFolderPermission(
                        folder.FolderID,
                        folderPermission.PermissionId,
                        folderPermission.RoleId,
                        folderPermission.AllowAccess,
                        folderPermission.UserId,
                        UserController.Instance.GetCurrentUserInfo().UserID);
                }
            }
        }

        /// <summary>Returns a flag indicating whether the current user can administer a module.</summary>
        /// <param name="module">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        public virtual bool CanAdminModule(ModuleInfo module)
        {
			eventLogger.LogError(">>> Entering CanAdminModule(module={Module})", module);
            eventLogger.LogWarning("<<< Exiting CanAdminModule(module={Module})", module);
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(AdminModulePermissionKey));
        }

        /// <summary>Returns a flag indicating whether the current user can delete a module.</summary>
        /// <param name="module">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        public virtual bool CanDeleteModule(ModuleInfo module)
        {
			eventLogger.LogError(">>> Entering CanDeleteModule(module={Module})", module);
            eventLogger.LogWarning("<<< Exiting CanDeleteModule(module={Module})", module);
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(DeleteModulePermissionKey));
        }

        /// <summary>Returns a flag indicating whether the current user can edit module content.</summary>
        /// <param name="module">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        public virtual bool CanEditModuleContent(ModuleInfo module)
        {
			eventLogger.LogError(">>> Entering CanEditModuleContent(module={Module})", module);
            eventLogger.LogWarning("<<< Exiting CanEditModuleContent(module={Module})", module);
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ContentModulePermissionKey));
        }

        /// <summary>Returns a flag indicating whether the current user can export a module.</summary>
        /// <param name="module">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        public virtual bool CanExportModule(ModuleInfo module)
        {
			eventLogger.LogError(">>> Entering CanExportModule(module={Module})", module);
            eventLogger.LogWarning("<<< Exiting CanExportModule(module={Module})", module);
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ExportModulePermissionKey));
        }

        /// <summary>Returns a flag indicating whether the current user can import a module.</summary>
        /// <param name="module">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        public virtual bool CanImportModule(ModuleInfo module)
        {
			eventLogger.LogError(">>> Entering CanImportModule(module={Module})", module);
            eventLogger.LogWarning("<<< Exiting CanImportModule(module={Module})", module);
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ImportModulePermissionKey));
        }

        /// <summary>Returns a flag indicating whether the current user can manage a module's settings.</summary>
        /// <param name="module">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        public virtual bool CanManageModule(ModuleInfo module)
        {
			eventLogger.LogError(">>> Entering CanManageModule(module={Module})", module);
            eventLogger.LogWarning("<<< Exiting CanManageModule(module={Module})", module);
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ManageModulePermissionKey));
        }

        /// <summary>Returns a flag indicating whether the current user can view a module.</summary>
        /// <param name="module">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        public virtual bool CanViewModule(ModuleInfo module)
        {
			eventLogger.LogError(">>> Entering CanViewModule(module={Module})", module);
            bool canView;
            if (module.InheritViewPermissions)
            {
                TabInfo objTab = TabController.Instance.GetTab(module.TabID, module.PortalID, false);
				eventLogger.LogError("CanViewModule(module={Module}): objTab → {ObjTab}", module, objTab);
                canView = TabPermissionController.CanViewPage(objTab);
				eventLogger.LogError("CanViewModule(module={Module}): canView → {CanView}", module, canView);
            }
            else
            {
				eventLogger.LogDebug("CanViewModule(module={Module}): canView → {CanView}", module, canView);
                canView = PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ViewModulePermissionKey));
            }

			eventLogger.LogWarning("<<< Exiting CanViewModule(module={Module})", module);
            return canView;
        }

        /// <summary>DeleteModulePermissionsByUser deletes a user's Module Permission in the Database.</summary>
        /// <param name="user">The user.</param>
        public virtual void DeleteModulePermissionsByUser(UserInfo user)
        {
			eventLogger.LogError(">>> Entering DeleteModulePermissionsByUser(user={User})", user);
            this.dataProvider.DeleteModulePermissionsByUserID(user.PortalID, user.UserID);
            DataCache.ClearModulePermissionsCachesByPortal(user.PortalID);
			eventLogger.LogWarning("<<< Exiting DeleteModulePermissionsByUser(user={User})", user);
        }

        /// <summary>GetModulePermissions gets a ModulePermissionCollection.</summary>
        /// <param name="moduleID">The ID of the module.</param>
        /// <param name="tabID">The ID of the tab.</param>
        /// <returns>A <see cref="ModulePermissionCollection"/> with the module permissions, or an empty <see cref="ModulePermissionCollection"/> if the module wasn't found.</returns>
        public virtual ModulePermissionCollection GetModulePermissions(int moduleID, int tabID)
        {
			eventLogger.LogError(">>> Entering GetModulePermissions(moduleID={ModuleID},tabID={TabID})", moduleID, tabID);
            // Get the Tab ModulePermission Dictionary
            Dictionary<int, ModulePermissionCollection> dictionary = this.GetModulePermissions(tabID);
			eventLogger.LogError("GetModulePermissions(moduleID={ModuleID},tabID={TabID}): dictionary → {Dictionary}", moduleID, tabID, dictionary);

            // Get the Collection from the Dictionary
            ModulePermissionCollection modulePermissions;
            bool found = dictionary.TryGetValue(moduleID, out modulePermissions);
            if (!found)
            {
                // Return empty collection
                modulePermissions = new ModulePermissionCollection();
				eventLogger.LogError("GetModulePermissions(moduleID={ModuleID},tabID={TabID}): modulePermissions → {ModulePermissions}", moduleID, tabID, modulePermissions);
				eventLogger.LogDebug("GetModulePermissions(moduleID={ModuleID},tabID={TabID}): modulePermissions → {ModulePermissions}", moduleID, tabID, modulePermissions);
            }

			eventLogger.LogWarning("<<< Exiting GetModulePermissions(moduleID={ModuleID},tabID={TabID})", moduleID, tabID);
            return modulePermissions;
        }

        /// <summary>Determines if user has the necessary permissions to access an item with the designated AccessLevel.</summary>
        /// <param name="accessLevel">The SecurityAccessLevel required to access a portal module or module action.</param>
        /// <param name="permissionKey">If Security Access is Edit the permissionKey is the actual "edit" permission required.</param>
        /// <param name="moduleConfiguration">The ModuleInfo object for the associated module.</param>
        /// <returns>A boolean value indicating if the user has the necessary permissions.</returns>
        /// <remarks>Every module control and module action has an associated permission level.  This
        /// function determines whether the user represented by UserName has sufficient permissions, as
        /// determined by the PortalSettings and ModuleSettings, to access a resource with the
        /// designated AccessLevel.</remarks>
        public virtual bool HasModuleAccess(SecurityAccessLevel accessLevel, string permissionKey, ModuleInfo moduleConfiguration)
        {
			eventLogger.LogError(">>> Entering HasModuleAccess(accessLevel={AccessLevel},permissionKey={PermissionKey},moduleConfiguration={ModuleConfiguration})", accessLevel, permissionKey, moduleConfiguration);
            bool isAuthorized = false;
            UserInfo userInfo = UserController.Instance.GetCurrentUserInfo();
			eventLogger.LogError("HasModuleAccess(accessLevel={AccessLevel},permissionKey={PermissionKey},moduleConfiguration={ModuleConfiguration}): userInfo → {UserInfo}", accessLevel, permissionKey, moduleConfiguration, userInfo);
			eventLogger.LogError("HasModuleAccess(accessLevel={AccessLevel},permissionKey={PermissionKey},moduleConfiguration={ModuleConfiguration}): isAuthorized → {IsAuthorized}", accessLevel, permissionKey, moduleConfiguration, isAuthorized);
            TabInfo tab = TabController.Instance.GetTab(moduleConfiguration.TabID, moduleConfiguration.PortalID, false);
            if (userInfo != null && userInfo.IsSuperUser)
            {
				eventLogger.LogError("HasModuleAccess(accessLevel={AccessLevel},permissionKey={PermissionKey},moduleConfiguration={ModuleConfiguration}): tab → {Tab}", accessLevel, permissionKey, moduleConfiguration, tab);
                isAuthorized = true;
            }
            else
            {
                switch (accessLevel)
                {
                    case SecurityAccessLevel.Anonymous:
                        isAuthorized = true;
                        break;
                    case SecurityAccessLevel.View:
                        if (ModulePermissionController.CanViewModule(moduleConfiguration))
                        {
                            isAuthorized = true;
                        }

                        break;
                    case SecurityAccessLevel.ViewPermissions:
                        isAuthorized = TabPermissionController.CanAddContentToPage(tab);
                        break;
                    case SecurityAccessLevel.Edit:
                        if (!((moduleConfiguration.IsShared && moduleConfiguration.IsShareableViewOnly) && TabPermissionController.CanAddContentToPage(tab)))
                        {
                            if (string.IsNullOrEmpty(permissionKey))
                            {
                                permissionKey = "CONTENT,DELETE,EXPORT,IMPORT,MANAGE";
								eventLogger.LogError("HasModuleAccess(accessLevel={AccessLevel},permissionKey={PermissionKey},moduleConfiguration={ModuleConfiguration}): permissionKey → {PermissionKey1}", accessLevel, permissionKey, moduleConfiguration, permissionKey);
                            }

                            if (TabPermissionController.CanAddContentToPage(tab))
                            {
                                // Need to check for Deny Edit at the Module Level
                                if (permissionKey == "CONTENT")
                                {
                                    isAuthorized = !this.IsDeniedModulePermission(moduleConfiguration, permissionKey);
                                }
                                else
                                {
                                    isAuthorized = true;
                                }
                            }
                            else
                            {
                                // Need to check if it was denied at Tab level
                                if (!this.IsDeniedTabPermission(tab, "CONTENT,EDIT"))
                                {
                                    isAuthorized = this.HasModulePermission(moduleConfiguration, permissionKey);
                                }
                            }
                        }

                        break;
                    case SecurityAccessLevel.Admin:
                        if (!((moduleConfiguration.IsShared && moduleConfiguration.IsShareableViewOnly) && TabPermissionController.CanAddContentToPage(tab)))
                        {
                            isAuthorized = TabPermissionController.CanAddContentToPage(tab);
                        }

                        break;
                    case SecurityAccessLevel.Host:
                        break;
                }
            }

			eventLogger.LogWarning("<<< Exiting HasModuleAccess(accessLevel={AccessLevel},permissionKey={PermissionKey},moduleConfiguration={ModuleConfiguration})", accessLevel, permissionKey, moduleConfiguration);
            return isAuthorized;
        }

        /// <summary>HasModulePermission checks whether the current user has a specific Module Permission.</summary>
        /// <param name="modulePermissions">The Permissions for the Module.</param>
        /// <param name="permissionKey">The Permission to check.</param>
        /// <returns><see langword="true"/> if the current user has the requested permission, otherwise <see langword="false"/>.</returns>
        public virtual bool HasModulePermission(ModulePermissionCollection modulePermissions, string permissionKey)
        {
			eventLogger.LogError(">>> Entering HasModulePermission(modulePermissions={ModulePermissions},permissionKey={PermissionKey})", modulePermissions, permissionKey);
            bool hasPermission = Null.NullBoolean;
			eventLogger.LogError("HasModulePermission(modulePermissions={ModulePermissions},permissionKey={PermissionKey}): hasPermission → {HasPermission}", modulePermissions, permissionKey, hasPermission);
            if (permissionKey.Contains(","))
            {
                foreach (string permission in permissionKey.Split(','))
                {
                    if (PortalSecurity.IsInRoles(modulePermissions.ToString(permission)))
                    {
                        hasPermission = true;
                        break;
                    }
                }
            }
            else
            {
                hasPermission = PortalSecurity.IsInRoles(modulePermissions.ToString(permissionKey));
            }

			eventLogger.LogWarning("<<< Exiting HasModulePermission(modulePermissions={ModulePermissions},permissionKey={PermissionKey})", modulePermissions, permissionKey);
            return hasPermission;
        }

        /// <summary>SaveModulePermissions updates a Module's permissions.</summary>
        /// <param name="module">The Module to update.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        public virtual void SaveModulePermissions(ModuleInfo module)
        {
			eventLogger.LogError(">>> Entering SaveModulePermissions(module={Module})", module);
            if (module.ModulePermissions != null)
            {
                ModulePermissionCollection modulePermissions = ModulePermissionController.GetModulePermissions(module.ModuleID, module.TabID);
				eventLogger.LogError("SaveModulePermissions(module={Module}): modulePermissions → {ModulePermissions}", module, modulePermissions);
                if (!modulePermissions.CompareTo(module.ModulePermissions))
                {
                    this.dataProvider.DeleteModulePermissionsByModuleID(module.ModuleID, module.PortalID);

                    foreach (ModulePermissionInfo modulePermission in module.ModulePermissions)
                    {
                        if (!module.IsShared && module.InheritViewPermissions && modulePermission.PermissionKey == "VIEW")
                        {
                            this.dataProvider.DeleteModulePermission(modulePermission.ModulePermissionID);
                        }
                        else
                        {
                            this.dataProvider.AddModulePermission(
                                module.ModuleID,
                                module.PortalID,
                                ((IPermissionInfo)modulePermission).PermissionId,
                                ((IPermissionInfo)modulePermission).RoleId,
                                modulePermission.AllowAccess,
                                ((IPermissionInfo)modulePermission).UserId,
                                UserController.Instance.GetCurrentUserInfo().UserID);
                        }
                    }
                }
            }
			eventLogger.LogWarning("<<< Exiting SaveModulePermissions(module={Module})", module);
        }

        /// <summary>Returns a list with all roles with implicit permissions on Tabs.</summary>
        /// <param name="portalId">The Portal Id where the Roles are.</param>
        /// <returns>A List with the implicit roles.</returns>
        public virtual IEnumerable<RoleInfo> ImplicitRolesForPages(int portalId)
        {
			eventLogger.LogError(">>> Entering ImplicitRolesForPages(portalId={PortalId})", portalId);
            eventLogger.LogWarning("<<< Exiting ImplicitRolesForPages(portalId={PortalId})", portalId);
            return DefaultImplicitRoles(portalId);
        }

        /// <summary>Returns a list with all roles with implicit permissions on Folders.</summary>
        /// <param name="portalId">The Portal Id where the permissions are.</param>
        /// <returns>A List with the implicit roles.</returns>
        public virtual IEnumerable<RoleInfo> ImplicitRolesForFolders(int portalId)
        {
			eventLogger.LogError(">>> Entering ImplicitRolesForFolders(portalId={PortalId})", portalId);
            eventLogger.LogWarning("<<< Exiting ImplicitRolesForFolders(portalId={PortalId})", portalId);
            return DefaultImplicitRoles(portalId);
        }

        /// <summary>Returns a flag indicating whether the current user can add content to a page.</summary>
        /// <param name="tab">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanAddContentToPage(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering CanAddContentToPage(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting CanAddContentToPage(tab={Tab})", tab);
            return HasPagePermission(tab, ContentPagePermissionKey) || this.IsPageAdmin(tab.PortalID);
        }

        /// <summary>Returns a flag indicating whether the current user can add a child page to a page.</summary>
        /// <param name="tab">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanAddPage(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering CanAddPage(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting CanAddPage(tab={Tab})", tab);
            return HasPagePermission(tab, AddPagePermissionKey) || (tab.TabID == Null.NullInteger && this.CanAddTopLevel(tab.PortalID)) || this.IsPageAdmin(tab.PortalID);
        }

        /// <summary>Returns a flag indicating whether the current user can administer a page.</summary>
        /// <param name="tab">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanAdminPage(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering CanAdminPage(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting CanAdminPage(tab={Tab})", tab);
            return PortalSecurity.IsInRoles(tab.TabPermissions.ToString(AdminPagePermissionKey)) || this.IsPageAdmin(tab.PortalID);
        }

        /// <summary>Returns a flag indicating whether the current user can copy a page.</summary>
        /// <param name="tab">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanCopyPage(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering CanCopyPage(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting CanCopyPage(tab={Tab})", tab);
            return HasPagePermission(tab, CopyPagePermissionKey) || this.IsPageAdmin(tab.PortalID);
        }

        /// <summary>Returns a flag indicating whether the current user can delete a page.</summary>
        /// <param name="tab">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanDeletePage(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering CanDeletePage(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting CanDeletePage(tab={Tab})", tab);
            return HasPagePermission(tab, DeletePagePermissionKey) || this.IsPageAdmin(tab.PortalID);
        }

        /// <summary>Returns a flag indicating whether the current user can export a page.</summary>
        /// <param name="tab">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanExportPage(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering CanExportPage(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting CanExportPage(tab={Tab})", tab);
            return HasPagePermission(tab, ExportPagePermissionKey) || this.IsPageAdmin(tab.PortalID);
        }

        /// <summary>Returns a flag indicating whether the current user can import a page.</summary>
        /// <param name="tab">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanImportPage(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering CanImportPage(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting CanImportPage(tab={Tab})", tab);
            return HasPagePermission(tab, ImportPagePermissionKey) || this.IsPageAdmin(tab.PortalID);
        }

        /// <summary>Returns a flag indicating whether the current user can manage a page's settings.</summary>
        /// <param name="tab">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanManagePage(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering CanManagePage(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting CanManagePage(tab={Tab})", tab);
            return HasPagePermission(tab, ManagePagePermissionKey) || this.IsPageAdmin(tab.PortalID);
        }

        /// <summary>Returns a flag indicating whether the current user can see a page in a navigation object.</summary>
        /// <param name="tab">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanNavigateToPage(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering CanNavigateToPage(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting CanNavigateToPage(tab={Tab})", tab);
            return HasPagePermission(tab, NavigatePagePermissionKey) || HasPagePermission(tab, ViewPagePermissionKey) || this.IsPageAdmin(tab.PortalID);
        }

        /// <summary>Returns a flag indicating whether the current user can view a page.</summary>
        /// <param name="tab">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanViewPage(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering CanViewPage(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting CanViewPage(tab={Tab})", tab);
            return HasPagePermission(tab, ViewPagePermissionKey) || this.IsPageAdmin(tab.PortalID);
        }

        /// <summary>Returns a flag indicating whether the current user can add top level pages.</summary>
        /// <param name="portalId">The id of the portal.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool CanAddTopLevel(int portalId)
        {
			eventLogger.LogError(">>> Entering CanAddTopLevel(portalId={PortalId})", portalId);
            var portal = PortalController.Instance.GetPortal(portalId);
			eventLogger.LogError("CanAddTopLevel(portalId={PortalId}): portal → {Portal}", portalId, portal);
            if (portal == null)
            {
                return false;
            }

			eventLogger.LogWarning("<<< Exiting CanAddTopLevel(portalId={PortalId})", portalId);
            return this.HasPortalPermission(portal.PortalPermissions, TopPagePermissionKey);
        }

        /// <summary>Returns a flag indicating whether the current user is a page admin.</summary>
        /// <param name="portalId">The id of the portal.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public virtual bool IsPageAdmin(int portalId)
        {
			eventLogger.LogError(">>> Entering IsPageAdmin(portalId={PortalId})", portalId);
            var portal = PortalController.Instance.GetPortal(portalId);
			eventLogger.LogError("IsPageAdmin(portalId={PortalId}): portal → {Portal}", portalId, portal);
            if (portal == null)
            {
                return false;
            }

			eventLogger.LogWarning("<<< Exiting IsPageAdmin(portalId={PortalId})", portalId);
            return this.HasPortalPermission(portal.PortalPermissions, PageAdminPermissionKey);
        }

        /// <summary>DeleteTabPermissionsByUser deletes a user's Tab Permissions in the Database.</summary>
        /// <param name="user">The user.</param>
        public virtual void DeleteTabPermissionsByUser(UserInfo user)
        {
			eventLogger.LogError(">>> Entering DeleteTabPermissionsByUser(user={User})", user);
            this.dataProvider.DeleteTabPermissionsByUserID(user.PortalID, user.UserID);
            DataCache.ClearTabPermissionsCache(user.PortalID);
			eventLogger.LogWarning("<<< Exiting DeleteTabPermissionsByUser(user={User})", user);
        }

        /// <summary>GetTabPermissions gets a TabPermissionCollection.</summary>
        /// <param name="tabId">The ID of the tab.</param>
        /// <param name="portalId">The ID of the portal.</param>
        /// <returns>A <see cref="TabPermissionCollection"/> instance with the tab's permissions, or an empty <see cref="TabPermissionCollection"/> if it can't be found.</returns>
        public virtual TabPermissionCollection GetTabPermissions(int tabId, int portalId)
        {
			eventLogger.LogError(">>> Entering GetTabPermissions(tabId={TabId},portalId={PortalId})", tabId, portalId);
            // Get the Portal TabPermission Dictionary
            Dictionary<int, TabPermissionCollection> dicTabPermissions = this.GetTabPermissions(portalId);
			eventLogger.LogError("GetTabPermissions(tabId={TabId},portalId={PortalId}): dicTabPermissions → {DicTabPermissions}", tabId, portalId, dicTabPermissions);

            // Get the Collection from the Dictionary
            TabPermissionCollection tabPermissions;
            bool bFound = dicTabPermissions.TryGetValue(tabId, out tabPermissions);
			eventLogger.LogError("GetTabPermissions(tabId={TabId},portalId={PortalId}): bFound → {BFound}", tabId, portalId, bFound);
            if (!bFound)
            {
                // Return empty collection
                tabPermissions = new TabPermissionCollection();
				eventLogger.LogError("GetTabPermissions(tabId={TabId},portalId={PortalId}): tabPermissions → {TabPermissions}", tabId, portalId, tabPermissions);
				eventLogger.LogDebug("GetTabPermissions(tabId={TabId},portalId={PortalId}): tabPermissions → {TabPermissions}", tabId, portalId, tabPermissions);
            }

			eventLogger.LogWarning("<<< Exiting GetTabPermissions(tabId={TabId},portalId={PortalId})", tabId, portalId);
            return tabPermissions;
        }

        /// <summary>HasTabPermission checks whether the current user has a specific Tab Permission.</summary>
        /// <param name="tabPermissions">The Permissions for the Tab.</param>
        /// <param name="permissionKey">The Permission to check.</param>
        /// <returns><see langword="true"/> if the current user has the requested permission, otherwise <see langword="false"/>.</returns>
        public virtual bool HasTabPermission(TabPermissionCollection tabPermissions, string permissionKey)
        {
			eventLogger.LogError(">>> Entering HasTabPermission(tabPermissions={TabPermissions},permissionKey={PermissionKey})", tabPermissions, permissionKey);
            bool hasPermission = PortalSecurity.IsInRoles(tabPermissions.ToString("EDIT"));
			eventLogger.LogError("HasTabPermission(tabPermissions={TabPermissions},permissionKey={PermissionKey}): hasPermission → {HasPermission}", tabPermissions, permissionKey, hasPermission);
            if (!hasPermission)
            {
                if (permissionKey.Contains(","))
                {
                    foreach (string permission in permissionKey.Split(','))
                    {
                        if (PortalSecurity.IsInRoles(tabPermissions.ToString(permission)))
                        {
                            hasPermission = true;
                            break;
                        }
                    }
                }
                else
                {
                    hasPermission = PortalSecurity.IsInRoles(tabPermissions.ToString(permissionKey));
                }
            }

			eventLogger.LogWarning("<<< Exiting HasTabPermission(tabPermissions={TabPermissions},permissionKey={PermissionKey})", tabPermissions, permissionKey);
            return hasPermission;
        }

        /// <summary>SaveTabPermissions saves a Tab's permissions.</summary>
        /// <param name="tab">The Tab to update.</param>
        public virtual void SaveTabPermissions(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering SaveTabPermissions(tab={Tab})", tab);
            TabPermissionCollection objCurrentTabPermissions = this.GetTabPermissions(tab.TabID, tab.PortalID);
			eventLogger.LogError("SaveTabPermissions(tab={Tab}): objCurrentTabPermissions → {ObjCurrentTabPermissions}", tab, objCurrentTabPermissions);
            if (!objCurrentTabPermissions.CompareTo(tab.TabPermissions))
            {
                var portalSettings = PortalController.Instance.GetCurrentSettings();
				eventLogger.LogError("SaveTabPermissions(tab={Tab}): portalSettings → {PortalSettings}", tab, portalSettings);
                var userId = UserController.Instance.GetCurrentUserInfo().UserID;
				eventLogger.LogError("SaveTabPermissions(tab={Tab}): userId → {UserId}", tab, userId);

                if (objCurrentTabPermissions.Count > 0)
                {
                    this.dataProvider.DeleteTabPermissionsByTabID(tab.TabID);
                    this.eventLogger.AddLog(tab, portalSettings, userId, string.Empty, EventLogType.TABPERMISSION_DELETED);
                }

                if (tab.TabPermissions is { Count: > 0 })
                {
                    foreach (TabPermissionInfo objTabPermission in tab.TabPermissions)
                    {
                        objTabPermission.TabPermissionID = this.dataProvider.AddTabPermission(
                            tab.TabID,
                            ((IPermissionInfo)objTabPermission).PermissionId,
                            ((IPermissionInfo)objTabPermission).RoleId,
                            objTabPermission.AllowAccess,
                            ((IPermissionInfo)objTabPermission).UserId,
                            userId);
                    }

                    this.eventLogger.AddLog(tab, portalSettings, userId, string.Empty, EventLogType.TABPERMISSION_CREATED);
                }
            }
			eventLogger.LogWarning("<<< Exiting SaveTabPermissions(tab={Tab})", tab);
        }

        /// <summary>GetDesktopModulePermission gets a DesktopModule Permission from the Database.</summary>
        /// <param name="desktopModulePermissionId">The ID of the DesktopModule Permission.</param>
        /// <returns>A <see cref="DesktopModulePermissionInfo"/> or <see langword="null"/>.</returns>
        public virtual DesktopModulePermissionInfo GetDesktopModulePermission(int desktopModulePermissionId)
        {
			eventLogger.LogError(">>> Entering GetDesktopModulePermission(desktopModulePermissionId={DesktopModulePermissionId})", desktopModulePermissionId);
            eventLogger.LogWarning("<<< Exiting GetDesktopModulePermission(desktopModulePermissionId={DesktopModulePermissionId})", desktopModulePermissionId);
            return CBO.FillObject<DesktopModulePermissionInfo>(DataProvider.Instance().GetDesktopModulePermission(desktopModulePermissionId));
        }

        /// <summary>GetDesktopModulePermissions gets a DesktopModulePermissionCollection.</summary>
        /// <param name="portalDesktopModuleId">The ID of the DesktopModule.</param>
        /// <returns>A <see cref="DesktopModulePermissionCollection"/> with the desktop module permissions, or an empty <see cref="DesktopModulePermissionCollection"/> if the desktop module wasn't found.</returns>
        public virtual DesktopModulePermissionCollection GetDesktopModulePermissions(int portalDesktopModuleId)
        {
			eventLogger.LogError(">>> Entering GetDesktopModulePermissions(portalDesktopModuleId={PortalDesktopModuleId})", portalDesktopModuleId);
            // Get the Tab DesktopModulePermission Dictionary
            Dictionary<int, DesktopModulePermissionCollection> dicDesktopModulePermissions = GetDesktopModulePermissions(this.hostSettings);
			eventLogger.LogError("GetDesktopModulePermissions(portalDesktopModuleId={PortalDesktopModuleId}): dicDesktopModulePermissions → {DicDesktopModulePermissions}", portalDesktopModuleId, dicDesktopModulePermissions);

            // Get the Collection from the Dictionary
            DesktopModulePermissionCollection desktopModulePermissions;
            bool bFound = dicDesktopModulePermissions.TryGetValue(portalDesktopModuleId, out desktopModulePermissions);
			eventLogger.LogError("GetDesktopModulePermissions(portalDesktopModuleId={PortalDesktopModuleId}): bFound → {BFound}", portalDesktopModuleId, bFound);
            if (!bFound)
            {
                // Return empty collection
                desktopModulePermissions = new DesktopModulePermissionCollection();
				eventLogger.LogError("GetDesktopModulePermissions(portalDesktopModuleId={PortalDesktopModuleId}): desktopModulePermissions → {DesktopModulePermissions}", portalDesktopModuleId, desktopModulePermissions);
				eventLogger.LogDebug("GetDesktopModulePermissions(portalDesktopModuleId={PortalDesktopModuleId}): desktopModulePermissions → {DesktopModulePermissions}", portalDesktopModuleId, desktopModulePermissions);
            }

			eventLogger.LogWarning("<<< Exiting GetDesktopModulePermissions(portalDesktopModuleId={PortalDesktopModuleId})", portalDesktopModuleId);
            return desktopModulePermissions;
        }

        /// <summary>HasDesktopModulePermission checks whether the current user has a specific DesktopModule Permission.</summary>
        /// <param name="desktopModulePermissions">The Permissions for the DesktopModule.</param>
        /// <param name="permissionKey">The Permission to check.</param>
        /// <returns><see langword="true"/> if the current user has the requested permission, otherwise <see langword="false"/>.</returns>
        public virtual bool HasDesktopModulePermission(DesktopModulePermissionCollection desktopModulePermissions, string permissionKey)
        {
			eventLogger.LogError(">>> Entering HasDesktopModulePermission(desktopModulePermissions={DesktopModulePermissions},permissionKey={PermissionKey})", desktopModulePermissions, permissionKey);
            eventLogger.LogWarning("<<< Exiting HasDesktopModulePermission(desktopModulePermissions={DesktopModulePermissions},permissionKey={PermissionKey})", desktopModulePermissions, permissionKey);
            return PortalSecurity.IsInRoles(desktopModulePermissions.ToString(permissionKey));
        }

        /// <summary>DeletePortalPermissionsByUser deletes a user's Portal Permissions in the Database.</summary>
        /// <param name="user">The user.</param>
        public virtual void DeletePortalPermissionsByUser(UserInfo user)
        {
			eventLogger.LogError(">>> Entering DeletePortalPermissionsByUser(user={User})", user);
            this.dataProvider.DeletePortalPermissionsByUserID(user.PortalID, user.UserID);
            DataCache.ClearPortalPermissionsCache(user.PortalID);
			eventLogger.LogWarning("<<< Exiting DeletePortalPermissionsByUser(user={User})", user);
        }

        /// <summary>GetPortalPermissions gets a PortalPermissionCollection.</summary>
        /// <param name="portalId">The ID of the portal.</param>
        /// <returns>A <see cref="PortalPermissionCollection"/> with the portal permissions, or an empty <see cref="PortalPermissionCollection"/> if the portal wasn't found.</returns>
        public virtual PortalPermissionCollection GetPortalPermissions(int portalId)
        {
			eventLogger.LogError(">>> Entering GetPortalPermissions(portalId={PortalId})", portalId);
            // Get the Portal PortalPermission Dictionary
            var dicPortalPermissions = this.GetPortalPermissionsDic(portalId);
			eventLogger.LogError("GetPortalPermissions(portalId={PortalId}): dicPortalPermissions → {DicPortalPermissions}", portalId, dicPortalPermissions);

            // Get the Collection from the Dictionary
            var bFound = dicPortalPermissions.TryGetValue(portalId, out PortalPermissionCollection portalPermissions);
			eventLogger.LogError("GetPortalPermissions(portalId={PortalId}): bFound → {BFound}", portalId, bFound);
            if (!bFound)
            {
                // Return empty collection
                portalPermissions = new PortalPermissionCollection();
				eventLogger.LogError("GetPortalPermissions(portalId={PortalId}): portalPermissions → {PortalPermissions}", portalId, portalPermissions);
				eventLogger.LogDebug("GetPortalPermissions(portalId={PortalId}): portalPermissions → {PortalPermissions}", portalId, portalPermissions);
            }

			eventLogger.LogWarning("<<< Exiting GetPortalPermissions(portalId={PortalId})", portalId);
            return portalPermissions;
        }

        /// <summary>HasPortalPermission checks whether the current user has a specific Portal Permission.</summary>
        /// <param name="portalPermissions">The Permissions for the Portal.</param>
        /// <param name="permissionKey">The Permission to check.</param>
        /// <returns><see langword="true"/> if the current user has the requested permission, otherwise <see langword="false"/>.</returns>
        public virtual bool HasPortalPermission(PortalPermissionCollection portalPermissions, string permissionKey)
        {
			eventLogger.LogError(">>> Entering HasPortalPermission(portalPermissions={PortalPermissions},permissionKey={PermissionKey})", portalPermissions, permissionKey);
            bool hasPermission = false;
			eventLogger.LogError("HasPortalPermission(portalPermissions={PortalPermissions},permissionKey={PermissionKey}): hasPermission → {HasPermission}", portalPermissions, permissionKey, hasPermission);
            if (permissionKey.Contains(","))
            {
                if (permissionKey.Split(',').Any(permission => PortalSecurity.IsInRoles(portalPermissions.ToString(permission))))
                {
                    hasPermission = true;
                }
            }
            else
            {
                hasPermission = PortalSecurity.IsInRoles(portalPermissions.ToString(permissionKey));
            }

			eventLogger.LogWarning("<<< Exiting HasPortalPermission(portalPermissions={PortalPermissions},permissionKey={PermissionKey})", portalPermissions, permissionKey);
            return hasPermission;
        }

        /// <summary>SavePortalPermissions saves a Portal's permissions.</summary>
        /// <param name="portal">The Portal to update.</param>
        public virtual void SavePortalPermissions(PortalInfo portal)
        {
			eventLogger.LogError(">>> Entering SavePortalPermissions(portal={Portal})", portal);
            var objCurrentPortalPermissions = this.GetPortalPermissions(((IPortalInfo)portal).PortalId);
			eventLogger.LogError("SavePortalPermissions(portal={Portal}): objCurrentPortalPermissions → {ObjCurrentPortalPermissions}", portal, objCurrentPortalPermissions);
            if (!objCurrentPortalPermissions.CompareTo(portal.PortalPermissions))
            {
                var portalSettings = PortalController.Instance.GetCurrentSettings();
				eventLogger.LogError("SavePortalPermissions(portal={Portal}): portalSettings → {PortalSettings}", portal, portalSettings);
                var userId = UserController.Instance.GetCurrentUserInfo().UserID;
				eventLogger.LogError("SavePortalPermissions(portal={Portal}): userId → {UserId}", portal, userId);

                if (objCurrentPortalPermissions.Count > 0)
                {
                    this.dataProvider.DeletePortalPermissionsByPortalID(((IPortalInfo)portal).PortalId);
                    this.eventLogger.AddLog(portal, portalSettings, userId, string.Empty, EventLogType.PORTALPERMISSION_DELETED);
                }

                if (portal.PortalPermissions is { Count: > 0 })
                {
                    foreach (PortalPermissionInfo objPortalPermission in portal.PortalPermissions)
                    {
                        objPortalPermission.PortalPermissionID = this.dataProvider.AddPortalPermission(
                            ((IPortalInfo)portal).PortalId,
                            ((IPermissionInfo)objPortalPermission).PermissionId,
                            ((IPermissionInfo)objPortalPermission).RoleId,
                            objPortalPermission.AllowAccess,
                            ((IPermissionInfo)objPortalPermission).UserId,
                            userId);
                    }

                    this.eventLogger.AddLog(portal, portalSettings, userId, string.Empty, EventLogType.PORTALPERMISSION_CREATED);
                }
            }
			eventLogger.LogWarning("<<< Exiting SavePortalPermissions(portal={Portal})", portal);
        }

        internal static void ResetCacheDependency(int portalId, Action cacheClearAction)
        {
			eventLogger.LogError(">>> Entering ResetCacheDependency(portalId={PortalId},cacheClearAction={CacheClearAction})", portalId, cacheClearAction);
            // first execute the cache clear action then check the dependency change
            cacheClearAction.Invoke();
            DNNCacheDependency dependency;
            using (cacheDependencyDict.GetReadLock())
            {
                cacheDependencyDict.TryGetValue(portalId, out dependency);
            }

            if (dependency != null)
            {
                using (cacheDependencyDict.GetWriteLock())
                {
                    cacheDependencyDict.Remove(portalId);
                }

                dependency.Dispose();
            }
			eventLogger.LogWarning("<<< Exiting ResetCacheDependency(portalId={PortalId},cacheClearAction={CacheClearAction})", portalId, cacheClearAction);
        }

        protected bool HasModulePermission(ModuleInfo moduleConfiguration, string permissionKey)
        {
            return this.CanViewModule(moduleConfiguration) &&
                   (this.HasModulePermission(moduleConfiguration.ModulePermissions, permissionKey)
                    || this.HasModulePermission(moduleConfiguration.ModulePermissions, "EDIT"));
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected bool IsDeniedModulePermission(ModuleInfo moduleConfiguration, string permissionKey)
        {
			eventLogger.LogError(">>> Entering IsDeniedModulePermission(moduleConfiguration={ModuleConfiguration},permissionKey={PermissionKey})", moduleConfiguration, permissionKey);
            eventLogger.LogWarning("<<< Exiting IsDeniedModulePermission(moduleConfiguration={ModuleConfiguration},permissionKey={PermissionKey})", moduleConfiguration, permissionKey);
            return IsDeniedModulePermission(moduleConfiguration.ModulePermissions, "VIEW")
                   || IsDeniedModulePermission(moduleConfiguration.ModulePermissions, permissionKey)
			eventLogger.LogWarning("<<< Exiting IsDeniedModulePermission(modulePermissions={ModulePermissions},permissionKey={PermissionKey})", modulePermissions, permissionKey);
                   || IsDeniedModulePermission(moduleConfiguration.ModulePermissions, "EDIT");
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected bool IsDeniedTabPermission(TabInfo tab, string permissionKey)
        {
			eventLogger.LogError(">>> Entering IsDeniedTabPermission(tab={Tab},permissionKey={PermissionKey})", tab, permissionKey);
            eventLogger.LogWarning("<<< Exiting IsDeniedTabPermission(tab={Tab},permissionKey={PermissionKey})", tab, permissionKey);
            return IsDeniedTabPermission(tab.TabPermissions, "VIEW")
                   || IsDeniedTabPermission(tab.TabPermissions, permissionKey)
			eventLogger.LogWarning("<<< Exiting IsDeniedTabPermission(tabPermissions={TabPermissions},permissionKey={PermissionKey})", tabPermissions, permissionKey);
                   || IsDeniedTabPermission(tab.TabPermissions, "EDIT");
        }

        private static DNNCacheDependency GetCacheDependency(int portalId)
        {
			eventLogger.LogError(">>> Entering GetCacheDependency(portalId={PortalId})", portalId);
            DNNCacheDependency dependency;
            using (cacheDependencyDict.GetReadLock())
            {
                cacheDependencyDict.TryGetValue(portalId, out dependency);
            }

            if (dependency == null)
            {
                var startAt = DateTime.UtcNow;
                var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.FolderPermissionCacheKey, portalId);
				eventLogger.LogError("GetCacheDependency(portalId={PortalId}): cacheKey → {CacheKey}", portalId, cacheKey);
                DataCache.SetCache(cacheKey, portalId); // no expiration set for this
                dependency = new DNNCacheDependency(null, new[] { cacheKey }, startAt);
				eventLogger.LogDebug("GetCacheDependency(portalId={PortalId}): dependency → {Dependency}", portalId, dependency);
                using (cacheDependencyDict.GetWriteLock())
                {
					eventLogger.LogError("GetCacheDependency(portalId={PortalId}): dependency → {Dependency}", portalId, dependency);
                    cacheDependencyDict[portalId] = dependency;
                }
            }

			eventLogger.LogWarning("<<< Exiting GetCacheDependency(portalId={PortalId})", portalId);
            return dependency;
        }

        /// <summary>GetDesktopModulePermissions gets a Dictionary of DesktopModulePermissionCollections by DesktopModule.</summary>
        /// <param name="hostSettings">The host settings.</param>
        private static Dictionary<int, DesktopModulePermissionCollection> GetDesktopModulePermissions(IHostSettings hostSettings)
        {
            return CBO.GetCachedObject<Dictionary<int, DesktopModulePermissionCollection>>(
                hostSettings,
                new CacheItemArgs(DataCache.DesktopModulePermissionCacheKey, DataCache.DesktopModulePermissionCachePriority),
                GetDesktopModulePermissionsCallBack);
        }

        /// <summary>GetDesktopModulePermissionsCallBack gets a Dictionary of DesktopModulePermissionCollections by DesktopModule from the Database.</summary>
        /// <param name="cacheItemArgs">The CacheItemArgs object that contains the parameters needed for the database call.</param>
        private static object GetDesktopModulePermissionsCallBack(CacheItemArgs cacheItemArgs)
        {
			eventLogger.LogError(">>> Entering GetDesktopModulePermissionsCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            eventLogger.LogWarning("<<< Exiting GetDesktopModulePermissionsCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return FillDesktopModulePermissionDictionary(DataProvider.Instance().GetDesktopModulePermissions());
        }

#if false
        private object GetFolderPermissionsCallBack(CacheItemArgs cacheItemArgs)
        {
			eventLogger.LogError(">>> Entering GetFolderPermissionsCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            var PortalID = (int)cacheItemArgs.ParamList[0];
            IDataReader dr = dataProvider.GetFolderPermissionsByPortal(PortalID);
			eventLogger.LogError("GetFolderPermissionsCallBack(cacheItemArgs={CacheItemArgs}): dr → {Dr}", cacheItemArgs, dr);
			eventLogger.LogError("GetFolderPermissionsCallBack(cacheItemArgs={CacheItemArgs}): PortalID → {PortalID}", cacheItemArgs, PortalID);
            var dic = new Dictionary<string, FolderPermissionCollection>();
            try
            {
				eventLogger.LogError("GetFolderPermissionsCallBack(cacheItemArgs={CacheItemArgs}): dic → {Dic}", cacheItemArgs, dic);
                while (dr.Read())
                {
                    //fill business object
                    var folderPermissionInfo = CBO.FillObject<FolderPermissionInfo>(dr, false);
					eventLogger.LogError("GetFolderPermissionsCallBack(cacheItemArgs={CacheItemArgs}): folderPermissionInfo → {FolderPermissionInfo}", cacheItemArgs, folderPermissionInfo);
                    string dictionaryKey = folderPermissionInfo.FolderPath;
					eventLogger.LogError("GetFolderPermissionsCallBack(cacheItemArgs={CacheItemArgs}): dictionaryKey → {DictionaryKey}", cacheItemArgs, dictionaryKey);
                    if (string.IsNullOrEmpty(dictionaryKey))
                    {
                        dictionaryKey = "[PortalRoot]";
                    }

                    //add Folder Permission to dictionary
                    if (dic.ContainsKey(dictionaryKey))
                    {
                        //Add FolderPermission to FolderPermission Collection already in dictionary for FolderPath
                        dic[dictionaryKey].Add(folderPermissionInfo);
                    }
                    else
                    {
                        //Create new FolderPermission Collection for TabId
                        var collection = new FolderPermissionCollection {folderPermissionInfo};

                        //Add Permission to Collection

                        //Add Collection to Dictionary
                        dic.Add(dictionaryKey, collection);
                    }
                }
            }
            catch (Exception exc)
            {
				eventLogger.LogError("Exception in GetFolderPermissionsCallBack(cacheItemArgs={CacheItemArgs}): {Arg1}", cacheItemArgs, exc.Message);
                Exceptions.LogException(exc);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }
			eventLogger.LogWarning("<<< Exiting GetFolderPermissionsCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return dic;
        }

        private Dictionary<string, FolderPermissionCollection> GetFolderPermissions(int PortalID)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.FolderPermissionCacheKey, PortalID);
            return
                CBO.GetCachedObject<Dictionary<string, FolderPermissionCollection>>(
                    new CacheItemArgs(cacheKey, DataCache.FolderPermissionCacheTimeOut, DataCache.FolderPermissionCachePriority, PortalID), GetFolderPermissionsCallBack);
        }
#endif

        /// <summary>FillDesktopModulePermissionDictionary fills a Dictionary of DesktopModulePermissions from a dataReader.</summary>
        /// <param name="dr">The IDataReader.</param>
        private static Dictionary<int, DesktopModulePermissionCollection> FillDesktopModulePermissionDictionary(IDataReader dr)
        {
            var dic = new Dictionary<int, DesktopModulePermissionCollection>();
            try
            {
                while (dr.Read())
                {
                    // fill business object
                    var desktopModulePermissionInfo = CBO.FillObject<DesktopModulePermissionInfo>(dr, false);

                    // add DesktopModule Permission to dictionary
                    if (dic.TryGetValue(desktopModulePermissionInfo.PortalDesktopModuleID, out var permissions))
                    {
                        permissions.Add(desktopModulePermissionInfo);
                    }
                    else
                    {
                        // Create new DesktopModulePermission Collection for DesktopModulePermissionID
                        var collection = new DesktopModulePermissionCollection { desktopModulePermissionInfo };

                        // Add Collection to Dictionary
                        dic.Add(desktopModulePermissionInfo.PortalDesktopModuleID, collection);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                // close datareader
                CBO.CloseDataReader(dr, true);
            }

            return dic;
        }

        private static bool HasFolderPermission(FolderInfo folder, string permissionKey)
        {
            if (folder == null)
            {
                return false;
            }

            return (PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(permissionKey))
                    || PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(AdminFolderPermissionKey)))
                   && !PortalSecurity.IsDenied(folder.FolderPermissions.ToString(permissionKey));

            // Deny on Edit permission on folder shouldn't take away any other explicitly Allowed
            ////&& !PortalSecurity.IsDenied(folder.FolderPermissions.ToString(AdminFolderPermissionKey));
        }

        private static bool HasPagePermission(TabInfo tab, string permissionKey)
        {
			eventLogger.LogError(">>> Entering HasPagePermission(tab={Tab},permissionKey={PermissionKey})", tab, permissionKey);
			eventLogger.LogWarning("<<< Exiting HasPagePermission(tab={Tab},permissionKey={PermissionKey})", tab, permissionKey);
            return (PortalSecurity.IsInRoles(tab.TabPermissions.ToString(permissionKey))
                    || PortalSecurity.IsInRoles(tab.TabPermissions.ToString(AdminPagePermissionKey)))
                   && !PortalSecurity.IsDenied(tab.TabPermissions.ToString(permissionKey));

            // Deny on Edit permission on page shouldn't take away any other explicitly Allowed
            ////&&!PortalSecurity.IsDenied(tab.TabPermissions.ToString(AdminPagePermissionKey));
        }

        private static bool HasSitePermission(PortalInfo portal, string permissionKey)
        {
			eventLogger.LogError(">>> Entering HasSitePermission(portal={Portal},permissionKey={PermissionKey})", portal, permissionKey);
			eventLogger.LogWarning("<<< Exiting HasSitePermission(portal={Portal},permissionKey={PermissionKey})", portal, permissionKey);
            return (PortalSecurity.IsInRoles(portal.PortalPermissions.ToString(permissionKey))
                    || PortalSecurity.IsInRoles(portal.PortalPermissions.ToString(AdminPagePermissionKey)))
                   && !PortalSecurity.IsDenied(portal.PortalPermissions.ToString(permissionKey));

            // Deny on Edit permission on page shouldn't take away any other explicitly Allowed
            ////&&!PortalSecurity.IsDenied(tab.TabPermissions.ToString(AdminPagePermissionKey));
        }

        private static bool IsDeniedModulePermission(ModulePermissionCollection modulePermissions, string permissionKey)
        {
            bool isDenied = Null.NullBoolean;
            if (permissionKey.Contains(","))
            {
                foreach (string permission in permissionKey.Split(','))
                {
                    if (PortalSecurity.IsDenied(modulePermissions.ToString(permission)))
                    {
                        isDenied = true;
                        break;
                    }
                }
            }
            else
            {
                isDenied = PortalSecurity.IsDenied(modulePermissions.ToString(permissionKey));
            }

            return isDenied;
        }

        private static bool IsDeniedTabPermission(TabPermissionCollection tabPermissions, string permissionKey)
        {
            bool isDenied = Null.NullBoolean;
            if (permissionKey.Contains(","))
            {
                foreach (string permission in permissionKey.Split(','))
                {
                    if (PortalSecurity.IsDenied(tabPermissions.ToString(permission)))
                    {
                        isDenied = true;
                        break;
                    }
                }
            }
            else
            {
                isDenied = PortalSecurity.IsDenied(tabPermissions.ToString(permissionKey));
            }

            return isDenied;
        }

        private static List<RoleInfo> DefaultImplicitRoles(int portalId)
        {
			eventLogger.LogError(">>> Entering DefaultImplicitRoles(portalId={PortalId})", portalId);
			eventLogger.LogWarning("<<< Exiting DefaultImplicitRoles(portalId={PortalId})", portalId);
            return
            [
                RoleController.Instance.GetRoleById(
                    portalId,
                    PortalController.Instance.GetPortal(portalId).AdministratorRoleId),
            ];
        }

        /// <summary>GetModulePermissions gets a Dictionary of ModulePermissionCollections by Module.</summary>
        /// <param name="tabId">The ID of the tab.</param>
        private Dictionary<int, ModulePermissionCollection> GetModulePermissions(int tabId)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.ModulePermissionCacheKey, tabId);
            return CBO.GetCachedObject<Dictionary<int, ModulePermissionCollection>>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.ModulePermissionCacheTimeOut, DataCache.ModulePermissionCachePriority, tabId),
                this.GetModulePermissionsCallBack);
        }

        /// <summary>GetModulePermissionsCallBack gets a Dictionary of ModulePermissionCollections by Module from the Database.</summary>
        /// <param name="cacheItemArgs">The CacheItemArgs object that contains the parameters needed for the database call.</param>
        private object GetModulePermissionsCallBack(CacheItemArgs cacheItemArgs)
        {
			eventLogger.LogError(">>> Entering GetModulePermissionsCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            var tabId = (int)cacheItemArgs.ParamList[0];
            IDataReader dr = this.dataProvider.GetModulePermissionsByTabID(tabId);
			eventLogger.LogError("GetModulePermissionsCallBack(cacheItemArgs={CacheItemArgs}): dr → {Dr}", cacheItemArgs, dr);
			eventLogger.LogError("GetModulePermissionsCallBack(cacheItemArgs={CacheItemArgs}): tabId → {TabId}", cacheItemArgs, tabId);
            var dic = new Dictionary<int, ModulePermissionCollection>();
            try
            {
				eventLogger.LogError("GetModulePermissionsCallBack(cacheItemArgs={CacheItemArgs}): dic → {Dic}", cacheItemArgs, dic);
                while (dr.Read())
                {
                    // fill business object
                    var modulePermissionInfo = CBO.FillObject<ModulePermissionInfo>(dr, false);
					eventLogger.LogError("GetModulePermissionsCallBack(cacheItemArgs={CacheItemArgs}): modulePermissionInfo → {ModulePermissionInfo}", cacheItemArgs, modulePermissionInfo);

                    // add Module Permission to dictionary
                    if (dic.TryGetValue(modulePermissionInfo.ModuleID, out var permissions))
                    {
                        permissions.Add(modulePermissionInfo);
                    }
                    else
                    {
                        // Create new ModulePermission Collection for ModuleId
                        var collection = new ModulePermissionCollection { modulePermissionInfo };

                        // Add Permission to Collection

                        // Add Collection to Dictionary
                        dic.Add(modulePermissionInfo.ModuleID, collection);
                    }
                }
            }
            catch (Exception exc)
            {
				eventLogger.LogError("Exception in GetModulePermissionsCallBack(cacheItemArgs={CacheItemArgs}): {Arg1}", cacheItemArgs, exc.Message);
                Exceptions.LogException(exc);
            }
            finally
            {
                // close datareader
                CBO.CloseDataReader(dr, true);
            }

			eventLogger.LogWarning("<<< Exiting GetModulePermissionsCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return dic;
        }

        /// <summary>GetTabPermissions gets a Dictionary of TabPermissionCollections by Tab.</summary>
        /// <param name="portalId">The ID of the portal.</param>
        private Dictionary<int, TabPermissionCollection> GetTabPermissions(int portalId)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.TabPermissionCacheKey, portalId);
            return CBO.GetCachedObject<Dictionary<int, TabPermissionCollection>>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.TabPermissionCacheTimeOut, DataCache.TabPermissionCachePriority, portalId),
                this.GetTabPermissionsCallBack);
        }

        /// <summary>GetTabPermissionsCallBack gets a Dictionary of TabPermissionCollections by Tab from the the Database.</summary>
        /// <param name="cacheItemArgs">The CacheItemArgs object that contains the parameters needed for the database call.</param>
        private object GetTabPermissionsCallBack(CacheItemArgs cacheItemArgs)
        {
			eventLogger.LogError(">>> Entering GetTabPermissionsCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            var portalId = (int)cacheItemArgs.ParamList[0];
            var dic = new Dictionary<int, TabPermissionCollection>();
			eventLogger.LogError("GetTabPermissionsCallBack(cacheItemArgs={CacheItemArgs}): dic → {Dic}", cacheItemArgs, dic);
			eventLogger.LogError("GetTabPermissionsCallBack(cacheItemArgs={CacheItemArgs}): portalId → {PortalId}", cacheItemArgs, portalId);

            if (portalId > -1)
            {
                IDataReader dr = this.dataProvider.GetTabPermissionsByPortal(portalId);
				eventLogger.LogError("GetTabPermissionsCallBack(cacheItemArgs={CacheItemArgs}): dr → {Dr}", cacheItemArgs, dr);
                try
                {
                    while (dr.Read())
                    {
                        // fill business object
                        var tabPermissionInfo = CBO.FillObject<TabPermissionInfo>(dr, false);
						eventLogger.LogError("GetTabPermissionsCallBack(cacheItemArgs={CacheItemArgs}): tabPermissionInfo → {TabPermissionInfo}", cacheItemArgs, tabPermissionInfo);

                        // add Tab Permission to dictionary
                        if (dic.TryGetValue(tabPermissionInfo.TabID, out var permissions))
                        {
                            permissions.Add(tabPermissionInfo);
                        }
                        else
                        {
                            // Create new TabPermission Collection for TabId
                            var collection = new TabPermissionCollection { tabPermissionInfo };

                            // Add Collection to Dictionary
                            dic.Add(tabPermissionInfo.TabID, collection);
                        }
                    }
                }
                catch (Exception exc)
                {
					eventLogger.LogError("Exception in GetTabPermissionsCallBack(cacheItemArgs={CacheItemArgs}): {Arg1}", cacheItemArgs, exc.Message);
                    Exceptions.LogException(exc);
                }
                finally
                {
                    // close datareader
                    CBO.CloseDataReader(dr, true);
                }
            }

			eventLogger.LogWarning("<<< Exiting GetTabPermissionsCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return dic;
        }

        /// <summary>GetPortalPermissions gets a Dictionary of PortalPermissionCollections by PortalId.</summary>
        /// <param name="portalId">The ID of the portal.</param>
        private Dictionary<int, PortalPermissionCollection> GetPortalPermissionsDic(int portalId)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.PortalPermissionCacheKey, portalId);
            return CBO.GetCachedObject<Dictionary<int, PortalPermissionCollection>>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.PortalPermissionCacheTimeOut, DataCache.PortalPermissionCachePriority, portalId),
                this.GetPortalPermissionsCallBack);
        }

        /// <summary>GetPortalPermissionsCallBack gets a Dictionary of PortalPermissionCollections.</summary>
        /// <param name="cacheItemArgs">The CacheItemArgs object that contains the parameters needed for the database call.</param>
        private object GetPortalPermissionsCallBack(CacheItemArgs cacheItemArgs)
        {
			eventLogger.LogError(">>> Entering GetPortalPermissionsCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            var portalId = (int)cacheItemArgs.ParamList[0];
            var dic = new Dictionary<int, PortalPermissionCollection>();
			eventLogger.LogError("GetPortalPermissionsCallBack(cacheItemArgs={CacheItemArgs}): dic → {Dic}", cacheItemArgs, dic);
			eventLogger.LogError("GetPortalPermissionsCallBack(cacheItemArgs={CacheItemArgs}): portalId → {PortalId}", cacheItemArgs, portalId);

            if (portalId > -1)
            {
                var dr = this.dataProvider.GetPortalPermissionsByPortal(portalId);
				eventLogger.LogError("GetPortalPermissionsCallBack(cacheItemArgs={CacheItemArgs}): dr → {Dr}", cacheItemArgs, dr);
                try
                {
                    while (dr.Read())
                    {
                        // fill business object
                        var portalPermissionInfo = CBO.FillObject<PortalPermissionInfo>(dr, false);
						eventLogger.LogError("GetPortalPermissionsCallBack(cacheItemArgs={CacheItemArgs}): portalPermissionInfo → {PortalPermissionInfo}", cacheItemArgs, portalPermissionInfo);

                        // add Portal Permission to dictionary
                        if (dic.TryGetValue(portalPermissionInfo.PortalID, out var permissions))
                        {
                            permissions.Add(portalPermissionInfo);
                        }
                        else
                        {
                            // Create new PortalPermission Collection for PortalId
                            var collection = new PortalPermissionCollection { portalPermissionInfo };

                            // Add Collection to Dictionary
                            dic.Add(portalPermissionInfo.PortalID, collection);
                        }
                    }
                }
                catch (Exception exc)
                {
					eventLogger.LogError("Exception in GetPortalPermissionsCallBack(cacheItemArgs={CacheItemArgs}): {Arg1}", cacheItemArgs, exc.Message);
                    Exceptions.LogException(exc);
                }
                finally
                {
                    // close datareader
                    CBO.CloseDataReader(dr, true);
                }
            }

			eventLogger.LogWarning("<<< Exiting GetPortalPermissionsCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return dic;
        }
    }
}
