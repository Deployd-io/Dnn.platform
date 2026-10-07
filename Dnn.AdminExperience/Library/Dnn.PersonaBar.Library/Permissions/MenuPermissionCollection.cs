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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MenuPermissionCollection));
        public MenuPermissionCollection()
        {
		Logger.LogError(">>> Entering MenuPermissionCollection()");
        Logger.LogWarning("<<< Exiting MenuPermissionCollection()");
        }

        public MenuPermissionCollection(IList<MenuPermissionInfo> menuPermissions)
        {
			Logger.LogError(">>> Entering MenuPermissionCollection(menuPermissions={MenuPermissions})", menuPermissions);
            this.AddRange(menuPermissions);
			Logger.LogWarning("<<< Exiting MenuPermissionCollection(menuPermissions={MenuPermissions})", menuPermissions);
        }

        public MenuPermissionCollection(MenuPermissionCollection permissions)
        {
			Logger.LogError(">>> Entering MenuPermissionCollection(permissions={Permissions})", permissions);
            this.AddRange(permissions);
			Logger.LogWarning("<<< Exiting MenuPermissionCollection(permissions={Permissions})", permissions);
        }

        public MenuPermissionCollection(ArrayList menuPermissions, int menuId)
        {
			Logger.LogError(">>> Entering MenuPermissionCollection(menuPermissions={MenuPermissions},menuId={MenuId})", menuPermissions, menuId);
            foreach (MenuPermissionInfo permission in menuPermissions)
            {
                if (permission.MenuId == menuId)
                {
                    this.Add(permission);
                }
            }
			Logger.LogWarning("<<< Exiting MenuPermissionCollection(menuPermissions={MenuPermissions},menuId={MenuId})", menuPermissions, menuId);
        }

        public int Add(MenuPermissionInfo value, bool checkForDuplicates)
        {
			Logger.LogError(">>> Entering Add(value={Value},checkForDuplicates={CheckForDuplicates})", value, checkForDuplicates);
            int id = Null.NullInteger;
			Logger.LogError("Add(value={Value},checkForDuplicates={CheckForDuplicates}): id → {Id}", value, checkForDuplicates, id);
            if (!checkForDuplicates)
            {
                id = this.Add(value);
            }
            else
            {
                bool isMatch = false;
				Logger.LogError("Add(value={Value},checkForDuplicates={CheckForDuplicates}): isMatch → {IsMatch}", value, checkForDuplicates, isMatch);
                foreach (MenuPermissionInfo permission in this.List)
                {
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

			Logger.LogWarning("<<< Exiting Add(value={Value},checkForDuplicates={CheckForDuplicates})", value, checkForDuplicates);
            return id;
        }

        public void AddRange(ArrayList menuPermissions)
        {
            this.AddRange(menuPermissions.Cast<MenuPermissionInfo>().ToList());
			Logger.LogError(">>> Entering AddRange(menuPermissions={MenuPermissions})", menuPermissions);
			Logger.LogWarning("<<< Exiting AddRange(menuPermissions={MenuPermissions})", menuPermissions);
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
			Logger.LogError(">>> Entering CompareTo(menuPermissionCollection={MenuPermissionCollection})", menuPermissionCollection);
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

			Logger.LogWarning("<<< Exiting CompareTo(menuPermissionCollection={MenuPermissionCollection})", menuPermissionCollection);
            return true;
        }

        public void Remove(int permissionId, int roleId, int userId)
        {
			Logger.LogError(">>> Entering Remove(permissionId={PermissionId},roleId={RoleId},userId={UserId})", permissionId, roleId, userId);
            foreach (IPermissionInfo permission in this.List)
            {
                if (permission.PermissionId == permissionId && permission.UserId == userId && permission.RoleId == roleId)
                {
                    this.List.Remove(permission);
                    break;
                }
            }
			Logger.LogWarning("<<< Exiting Remove(permissionId={PermissionId},roleId={RoleId},userId={UserId})", permissionId, roleId, userId);
        }

        public List<PermissionInfoBase> ToList()
        {
			Logger.LogError(">>> Entering ToList()");
            Logger.LogWarning("<<< Exiting ToList()");
            return [..this.List.Cast<PermissionInfoBase>()];
        }

        public string ToString(string key)
        {
			Logger.LogError(">>> Entering ToString(key={Key})", key);
            Logger.LogWarning("<<< Exiting ToString(key={Key})", key);
            return PermissionController.BuildPermissions(this.List, key);
        }

        public IEnumerable<MenuPermissionInfo> Where(Func<MenuPermissionInfo, bool> predicate)
        {
			Logger.LogError(">>> Entering Where(predicate={Predicate})", predicate);
            Logger.LogWarning("<<< Exiting Where(predicate={Predicate})", predicate);
            return this.Cast<MenuPermissionInfo>().Where(predicate);
        }
    }
}
