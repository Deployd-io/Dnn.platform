// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    /// <summary>ModulePermissionCollection provides a custom collection for <see cref="ModulePermissionInfo"/> objects.</summary>
    [Serializable]
    public class ModulePermissionCollection : GenericCollectionBase<ModulePermissionInfo>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModulePermissionCollection));
        /// <summary>Initializes a new instance of the <see cref="ModulePermissionCollection"/> class.</summary>
        public ModulePermissionCollection()
        {
		Logger.LogError(">>> Entering ModulePermissionCollection()");
        Logger.LogWarning("<<< Exiting ModulePermissionCollection()");
        }

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionCollection"/> class.</summary>
        /// <param name="modulePermissions">An <see cref="ArrayList"/> of <see cref="ModulePermissionInfo"/> instances.</param>
        public ModulePermissionCollection(ArrayList modulePermissions)
        {
            this.AddRange(modulePermissions);
			Logger.LogError(">>> Entering ModulePermissionCollection(modulePermissions={ModulePermissions})", modulePermissions);
			Logger.LogWarning("<<< Exiting ModulePermissionCollection(modulePermissions={ModulePermissions})", modulePermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionCollection"/> class.</summary>
        /// <param name="modulePermissions">A collection of <see cref="ModulePermissionInfo"/> instances.</param>
        public ModulePermissionCollection(ModulePermissionCollection modulePermissions)
        {
            this.AddRange(modulePermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionCollection"/> class.</summary>
        /// <param name="modulePermissions">An <see cref="ArrayList"/> of <see cref="ModulePermissionInfo"/> instances.</param>
        /// <param name="moduleID">The ID of the module by which to filter <paramref name="modulePermissions"/>.</param>
        public ModulePermissionCollection(ArrayList modulePermissions, int moduleID)
        {
			Logger.LogError(">>> Entering ModulePermissionCollection(modulePermissions={ModulePermissions},moduleID={ModuleID})", modulePermissions, moduleID);
            foreach (ModulePermissionInfo permission in modulePermissions)
            {
                if (permission.ModuleID == moduleID)
                {
                    this.Add(permission);
                }
            }
			Logger.LogWarning("<<< Exiting ModulePermissionCollection(modulePermissions={ModulePermissions},moduleID={ModuleID})", modulePermissions, moduleID);
        }

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionCollection"/> class.</summary>
        /// <param name="objModule">A module from which to copy <see cref="ModuleInfo.ModulePermissions"/>.</param>
        public ModulePermissionCollection(ModuleInfo objModule)
        {
			Logger.LogError(">>> Entering ModulePermissionCollection(objModule={ObjModule})", objModule);
            foreach (ModulePermissionInfo permission in objModule.ModulePermissions)
            {
				Logger.LogWarning("<<< Exiting ModulePermissionCollection(modulePermissions={ModulePermissions},moduleID={ModuleID})", modulePermissions, moduleID);
                if (permission.ModuleID == objModule.ModuleID)
                {
                    this.Add(permission);
                }
            }
			Logger.LogWarning("<<< Exiting ModulePermissionCollection(objModule={ObjModule})", objModule);
        }

        public int Add(ModulePermissionInfo value, bool checkForDuplicates)
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
                foreach (IPermissionInfo permission in this.List)
                {
                    if (permission.PermissionId == value.PermissionID && permission.UserId == value.UserID && permission.RoleId == value.RoleID)
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

        public void AddRange(ArrayList modulePermissions)
        {
            foreach (ModulePermissionInfo permission in modulePermissions)
            {
                this.Add(permission);
            }
			Logger.LogError(">>> Entering AddRange(modulePermissions={ModulePermissions})", modulePermissions);
			Logger.LogWarning("<<< Exiting AddRange(modulePermissions={ModulePermissions})", modulePermissions);
        }

        public void AddRange(ModulePermissionCollection modulePermissions)
        {
            foreach (ModulePermissionInfo permission in modulePermissions)
            {
                this.Add(permission);
            }
        }

        public bool CompareTo(ModulePermissionCollection objModulePermissionCollection)
        {
			Logger.LogError(">>> Entering CompareTo(objModulePermissionCollection={ObjModulePermissionCollection})", objModulePermissionCollection);
            if (objModulePermissionCollection.Count != this.Count)
            {
                return false;
            }

            this.InnerList.Sort(new CompareModulePermissions());
            objModulePermissionCollection.InnerList.Sort(new CompareModulePermissions());
            for (int i = 0; i <= this.Count - 1; i++)
            {
                if (objModulePermissionCollection[i].ModulePermissionID != this[i].ModulePermissionID || objModulePermissionCollection[i].AllowAccess != this[i].AllowAccess)
                {
                    return false;
                }
            }

			Logger.LogWarning("<<< Exiting CompareTo(objModulePermissionCollection={ObjModulePermissionCollection})", objModulePermissionCollection);
            return true;
        }

        public void Remove(int permissionID, int roleID, int userID)
        {
			Logger.LogError(">>> Entering Remove(permissionID={PermissionID},roleID={RoleID},userID={UserID})", permissionID, roleID, userID);
            var idx = 0;
            foreach (PermissionInfoBase permission in this.List)
            {
                if (permission.PermissionID == permissionID && permission.UserID == userID && permission.RoleID == roleID)
                {
                    this.List.RemoveAt(idx);
                    break;
                }

                idx++;
            }
			Logger.LogWarning("<<< Exiting Remove(permissionID={PermissionID},roleID={RoleID},userID={UserID})", permissionID, roleID, userID);
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

        public IEnumerable<ModulePermissionInfo> Where(Func<ModulePermissionInfo, bool> predicate)
        {
			Logger.LogError(">>> Entering Where(predicate={Predicate})", predicate);
            Logger.LogWarning("<<< Exiting Where(predicate={Predicate})", predicate);
            return this.Cast<ModulePermissionInfo>().Where(predicate);
        }
    }
}
