// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Globalization;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>DesktopModulePermissionController provides the Business Layer for DesktopModule Permissions.</summary>
    [Serializable]
    public partial class DesktopModulePermissionController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly PermissionProvider Provider = PermissionProvider.Instance();

        /// <summary>AddDesktopModulePermission adds a DesktopModule Permission to the Database.</summary>
        /// <param name="objDesktopModulePermission">The DesktopModule Permission to add.</param>
        /// <returns>The new desktop module permission ID.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial int AddDesktopModulePermission(DesktopModulePermissionInfo objDesktopModulePermission)
            => AddDesktopModulePermission(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), objDesktopModulePermission);
			Logger.LogDebug(">>> Entering AddDesktopModulePermission(objDesktopModulePermission={ObjDesktopModulePermission})", objDesktopModulePermission);

        /// <summary>AddDesktopModulePermission adds a DesktopModule Permission to the Database.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="desktopModulePermission">The DesktopModule Permission to add.</param>
        /// <returns>The new desktop module permission ID.</returns>
        public static int AddDesktopModulePermission(IEventLogger eventLogger, DesktopModulePermissionInfo desktopModulePermission)
        {
            IPermissionInfo permission = desktopModulePermission;
            int id = DataProvider.Instance().AddDesktopModulePermission(
                desktopModulePermission.PortalDesktopModuleID,
                permission.PermissionId,
                permission.RoleId,
                permission.AllowAccess,
                permission.UserId,
                UserController.Instance.GetCurrentUserInfo().UserID);
            eventLogger.AddLog(
                desktopModulePermission,
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                string.Empty,
                EventLogType.DESKTOPMODULEPERMISSION_CREATED);
            ClearPermissionCache();
			Logger.LogDebug("<<< Exiting AddDesktopModulePermission(objDesktopModulePermission={ObjDesktopModulePermission})", objDesktopModulePermission);
            return id;
        }

        /// <summary>DeleteDesktopModulePermission deletes a DesktopModule Permission in the Database.</summary>
        /// <param name="desktopModulePermissionID">The ID of the DesktopModule Permission to delete.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void DeleteDesktopModulePermission(int desktopModulePermissionID)
            => DeleteDesktopModulePermission(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), desktopModulePermissionID);
			Logger.LogDebug(">>> Entering DeleteDesktopModulePermission(desktopModulePermissionID={DesktopModulePermissionID})", desktopModulePermissionID);

        /// <summary>DeleteDesktopModulePermission deletes a DesktopModule Permission in the Database.</summary>
        /// <param name="eventLogger">The event logger.</param>
		Logger.LogDebug("<<< Exiting DeleteDesktopModulePermission(desktopModulePermissionID={DesktopModulePermissionID})", desktopModulePermissionID);
        /// <param name="desktopModulePermissionId">The ID of the DesktopModule Permission to delete.</param>
        public static void DeleteDesktopModulePermission(IEventLogger eventLogger, int desktopModulePermissionId)
        {
            DataProvider.Instance().DeleteDesktopModulePermission(desktopModulePermissionId);
            eventLogger.AddLog(
                "DesktopModulePermissionID",
                desktopModulePermissionId.ToString(CultureInfo.InvariantCulture),
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogType.DESKTOPMODULEPERMISSION_DELETED);
            ClearPermissionCache();
        }

        /// <summary>DeleteDesktopModulePermissionsByPortalDesktopModuleID deletes a DesktopModule's DesktopModule Permission in the Database.</summary>
        /// <param name="portalDesktopModuleID">The ID of the DesktopModule to delete.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void DeleteDesktopModulePermissionsByPortalDesktopModuleID(int portalDesktopModuleID)
            => DeleteDesktopModulePermissionsByPortalDesktopModuleID(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), portalDesktopModuleID);
			Logger.LogDebug(">>> Entering DeleteDesktopModulePermissionsByPortalDesktopModuleID(portalDesktopModuleID={PortalDesktopModuleID})", portalDesktopModuleID);

        /// <summary>DeleteDesktopModulePermissionsByPortalDesktopModuleID deletes a DesktopModule's DesktopModule Permission in the Database.</summary>
        /// <param name="eventLogger">The event logger.</param>
		Logger.LogDebug("<<< Exiting DeleteDesktopModulePermissionsByPortalDesktopModuleID(portalDesktopModuleID={PortalDesktopModuleID})", portalDesktopModuleID);
        /// <param name="portalDesktopModuleId">The ID of the DesktopModule to delete.</param>
        public static void DeleteDesktopModulePermissionsByPortalDesktopModuleID(IEventLogger eventLogger, int portalDesktopModuleId)
        {
            DataProvider.Instance().DeleteDesktopModulePermissionsByPortalDesktopModuleID(portalDesktopModuleId);
            eventLogger.AddLog(
                "PortalDesktopModuleID",
                portalDesktopModuleId.ToString(CultureInfo.InvariantCulture),
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogType.DESKTOPMODULE_DELETED);
            ClearPermissionCache();
        }

        /// <summary>DeleteDesktopModulePermissionsByUserID deletes a user's DesktopModule Permission in the Database.</summary>
        /// <param name="objUser">The user.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void DeleteDesktopModulePermissionsByUserID(UserInfo objUser)
            => DeleteDesktopModulePermissionsByUserID(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), objUser);
			Logger.LogDebug(">>> Entering DeleteDesktopModulePermissionsByUserID(objUser={ObjUser})", objUser);

        /// <summary>DeleteDesktopModulePermissionsByUserID deletes a user's DesktopModule Permission in the Database.</summary>
        /// <param name="eventLogger">The event logger.</param>
		Logger.LogDebug("<<< Exiting DeleteDesktopModulePermissionsByUserID(objUser={ObjUser})", objUser);
        /// <param name="user">The user.</param>
        public static void DeleteDesktopModulePermissionsByUserID(IEventLogger eventLogger, UserInfo user)
        {
            DataProvider.Instance().DeleteDesktopModulePermissionsByUserID(user.UserID, user.PortalID);
            eventLogger.AddLog(
                "UserID",
                user.UserID.ToString(CultureInfo.InvariantCulture),
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogType.DESKTOPMODULE_DELETED);
            ClearPermissionCache();
        }

        /// <summary>GetDesktopModulePermission gets a DesktopModule Permission from the Database.</summary>
        /// <param name="desktopModulePermissionID">The ID of the DesktopModule Permission.</param>
        /// <returns>A <see cref="DesktopModulePermissionInfo"/> instance or <see langword="null"/>.</returns>
        public static DesktopModulePermissionInfo GetDesktopModulePermission(int desktopModulePermissionID)
        {
			Logger.LogDebug(">>> Entering GetDesktopModulePermission(desktopModulePermissionID={DesktopModulePermissionID})", desktopModulePermissionID);
			Logger.LogDebug("<<< Exiting GetDesktopModulePermission(desktopModulePermissionID={DesktopModulePermissionID})", desktopModulePermissionID);
            return Provider.GetDesktopModulePermission(desktopModulePermissionID);
        }

        /// <summary>GetDesktopModulePermissions gets a DesktopModulePermissionCollection.</summary>
        /// <param name="portalDesktopModuleID">The ID of the DesktopModule.</param>
        /// <returns>A <see cref="DesktopModulePermissionCollection"/> with the desktop module permissions, or an empty <see cref="DesktopModulePermissionCollection"/> if the desktop module wasn't found.</returns>
        public static DesktopModulePermissionCollection GetDesktopModulePermissions(int portalDesktopModuleID)
        {
			Logger.LogDebug(">>> Entering GetDesktopModulePermissions(portalDesktopModuleID={PortalDesktopModuleID})", portalDesktopModuleID);
			Logger.LogDebug("<<< Exiting GetDesktopModulePermissions(portalDesktopModuleID={PortalDesktopModuleID})", portalDesktopModuleID);
            return Provider.GetDesktopModulePermissions(portalDesktopModuleID);
        }

        /// <summary>HasDesktopModulePermission checks whether the current user has a specific DesktopModule Permission.</summary>
        /// <param name="objDesktopModulePermissions">The Permissions for the DesktopModule.</param>
        /// <param name="permissionKey">The Permission to check.</param>
        /// <returns><see langword="true"/> if the current user has the requested permission, otherwise <see langword="false"/>.</returns>
        public static bool HasDesktopModulePermission(DesktopModulePermissionCollection objDesktopModulePermissions, string permissionKey)
        {
			Logger.LogDebug(">>> Entering HasDesktopModulePermission(objDesktopModulePermissions={ObjDesktopModulePermissions},permissionKey={PermissionKey})", objDesktopModulePermissions, permissionKey);
			Logger.LogDebug("<<< Exiting HasDesktopModulePermission(objDesktopModulePermissions={ObjDesktopModulePermissions},permissionKey={PermissionKey})", objDesktopModulePermissions, permissionKey);
            return Provider.HasDesktopModulePermission(objDesktopModulePermissions, permissionKey);
        }

        /// <summary>UpdateDesktopModulePermission updates a DesktopModule Permission in the Database.</summary>
        /// <param name="objDesktopModulePermission">The DesktopModule Permission to update.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void UpdateDesktopModulePermission(DesktopModulePermissionInfo objDesktopModulePermission)
            => UpdateDesktopModulePermission(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), objDesktopModulePermission);
			Logger.LogDebug(">>> Entering UpdateDesktopModulePermission(objDesktopModulePermission={ObjDesktopModulePermission})", objDesktopModulePermission);

        /// <summary>UpdateDesktopModulePermission updates a DesktopModule Permission in the Database.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="desktopModulePermission">The DesktopModule Permission to update.</param>
        public static void UpdateDesktopModulePermission(IEventLogger eventLogger, DesktopModulePermissionInfo desktopModulePermission)
        {
            IPermissionInfo permission = desktopModulePermission;
			Logger.LogDebug("<<< Exiting UpdateDesktopModulePermission(objDesktopModulePermission={ObjDesktopModulePermission})", objDesktopModulePermission);
            DataProvider.Instance().UpdateDesktopModulePermission(
                desktopModulePermission.DesktopModulePermissionID,
                desktopModulePermission.PortalDesktopModuleID,
                permission.PermissionId,
                permission.RoleId,
                permission.AllowAccess,
                permission.UserId,
                UserController.Instance.GetCurrentUserInfo().UserID);
            eventLogger.AddLog(
                desktopModulePermission,
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                string.Empty,
                EventLogType.DESKTOPMODULEPERMISSION_UPDATED);
            ClearPermissionCache();
        }

        /// <summary>ClearPermissionCache clears the DesktopModule Permission Cache.</summary>
        private static void ClearPermissionCache()
        {
			Logger.LogDebug(">>> Entering ClearPermissionCache()");
            DataCache.ClearDesktopModulePermissionsCache();
			Logger.LogDebug("<<< Exiting ClearPermissionCache()");
        }
    }
}
