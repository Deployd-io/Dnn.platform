// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.Permissions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library.Model;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security.Permissions;

    [Serializable]
    public class MenuPermissionCollection : GenericCollectionBase<MenuPermissionInfo>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public MenuPermissionCollection()
        {
		Logger.LogDebug(">>> Entering MenuPermissionCollection()");
		Logger.LogDebug("<<< Exiting MenuPermissionCollection()");
        }

        public MenuPermissionCollection(IList<MenuPermissionInfo> menuPermissions)
        {
			Logger.LogDebug(">>> Entering MenuPermissionCollection(menuPermissions={MenuPermissions})", menuPermissions);
            this.AddRange(menuPermissions);
			Logger.LogDebug("<<< Exiting MenuPermissionCollection(menuPermissions={MenuPermissions})", menuPermissions);
        }

        public MenuPermissionCollection(MenuPermissionCollection permissions)
        {
			Logger.LogDebug(">>> Entering MenuPermissionCollection(permissions={Permissions})", permissions);
            this.AddRange(permissions);
			Logger.LogDebug("<<< Exiting MenuPermissionCollection(permissions={Permissions})", permissions);
        }

        public MenuPermissionCollection(ArrayList menuPermissions, int menuId)
        {
			Logger.LogDebug(">>> Entering MenuPermissionCollection(menuPermissions={MenuPermissions},menuId={MenuId})", menuPermissions, menuId);
            foreach (MenuPermissionInfo permission in menuPermissions)
            {
                if (permission.MenuId == menuId)
                {
                    this.Add(permission);
                }
            }
			Logger.LogDebug("<<< Exiting MenuPermissionCollection(menuPermissions={MenuPermissions},menuId={MenuId})", menuPermissions, menuId);
        }

        public int Add(MenuPermissionInfo value, bool checkForDuplicates)
        {
			Logger.LogDebug(">>> Entering Add(value={Value},checkForDuplicates={CheckForDuplicates})", value, checkForDuplicates);
            int id = Null.NullInteger;
            if (!checkForDuplicates)
			Logger.LogDebug("Add(value={Value},checkForDuplicates={CheckForDuplicates}): id → {Id}", value, checkForDuplicates, id);
            {
                id = this.Add(value);
            }
            else
            {
                bool isMatch = false;
                foreach (MenuPermissionInfo permission in this.List)
                {
				Logger.LogDebug("Add(value={Value},checkForDuplicates={CheckForDuplicates}): isMatch → {IsMatch}", value, checkForDuplicates, isMatch);
                    IPermissionInfo existingPermission = permission;
                    IPermissionInfo newPermission = value;
                    if (permission.PortalId == value.PortalId &&
                        permission.MenuId == value.MenuId &&
                        existingPermission.PermissionId == newPermission.PermissionId &&
                        existingPermission.UserId == newPermission.UserId &&
                        existingPermission.RoleId == newPermission.RoleId)
                    {
                        isMatch = true;
                        break;
                    }
                }

                if (!isMatch)
                {
                    id = this.Add(value);
                }
            }

			Logger.LogDebug("<<< Exiting Add(value={Value},checkForDuplicates={CheckForDuplicates})", value, checkForDuplicates);
            return id;
        }

        public void AddRange(ArrayList menuPermissions)
        {
            this.AddRange(menuPermissions.Cast<MenuPermissionInfo>().ToList());
			Logger.LogDebug(">>> Entering AddRange(menuPermissions={MenuPermissions})", menuPermissions);
			Logger.LogDebug("<<< Exiting AddRange(menuPermissions={MenuPermissions})", menuPermissions);
        }

        public void AddRange(IList<MenuPermissionInfo> menuPermissions)
        {
            foreach (MenuPermissionInfo permission in menuPermissions)
            {
                this.Add(permission);
            }
        }

        public void AddRange(MenuPermissionCollection menuPermissions)
        {
            foreach (MenuPermissionInfo permission in menuPermissions)
            {
                this.Add(permission);
            }
        }

        public bool CompareTo(MenuPermissionCollection menuPermissionCollection)
        {
			Logger.LogDebug(">>> Entering CompareTo(menuPermissionCollection={MenuPermissionCollection})", menuPermissionCollection);
            if (menuPermissionCollection.Count != this.Count)
            {
                return false;
            }

            this.InnerList.Sort(new CompareMenuPermissions());
            menuPermissionCollection.InnerList.Sort(new CompareMenuPermissions());
            for (int i = 0; i <= this.Count - 1; i++)
            {
                if (menuPermissionCollection[i].MenuPermissionId != this[i].MenuPermissionId
                    || menuPermissionCollection[i].AllowAccess != this[i].AllowAccess)
                {
                    return false;
                }
            }

			Logger.LogDebug("<<< Exiting CompareTo(menuPermissionCollection={MenuPermissionCollection})", menuPermissionCollection);
            return true;
        }

        public void Remove(int permissionId, int roleId, int userId)
        {
			Logger.LogDebug(">>> Entering Remove(permissionId={PermissionId},roleId={RoleId},userId={UserId})", permissionId, roleId, userId);
            foreach (IPermissionInfo permission in this.List)
            {
                if (permission.PermissionId == permissionId && permission.UserId == userId && permission.RoleId == roleId)
                {
                    this.List.Remove(permission);
                    break;
                }
            }
			Logger.LogDebug("<<< Exiting Remove(permissionId={PermissionId},roleId={RoleId},userId={UserId})", permissionId, roleId, userId);
        }

        public List<PermissionInfoBase> ToList()
        {
			Logger.LogDebug(">>> Entering ToList()");
			Logger.LogDebug("<<< Exiting ToList()");
            return [..this.List.Cast<PermissionInfoBase>()];
        }

        public string ToString(string key)
        {
			Logger.LogDebug(">>> Entering ToString(key={Key})", key);
			Logger.LogDebug("<<< Exiting ToString(key={Key})", key);
            return PermissionController.BuildPermissions(this.List, key);
        }

        public IEnumerable<MenuPermissionInfo> Where(Func<MenuPermissionInfo, bool> predicate)
        {
            return this.Cast<MenuPermissionInfo>().Where(predicate);
        }
    }
}
