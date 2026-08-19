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
		Logger.LogDebug(">>> Entering ModulePermissionCollection()");
		Logger.LogDebug("<<< Exiting ModulePermissionCollection()");
        }

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionCollection"/> class.</summary>
        /// <param name="modulePermissions">An <see cref="ArrayList"/> of <see cref="ModulePermissionInfo"/> instances.</param>
        public ModulePermissionCollection(ArrayList modulePermissions)
        {
            this.AddRange(modulePermissions);
			Logger.LogDebug(">>> Entering ModulePermissionCollection(modulePermissions={ModulePermissions})", modulePermissions);
			Logger.LogDebug("<<< Exiting ModulePermissionCollection(modulePermissions={ModulePermissions})", modulePermissions);
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
			Logger.LogDebug(">>> Entering ModulePermissionCollection(modulePermissions={ModulePermissions},moduleID={ModuleID})", modulePermissions, moduleID);
            foreach (ModulePermissionInfo permission in modulePermissions)
            {
                if (permission.ModuleID == moduleID)
                {
                    this.Add(permission);
                }
            }
			Logger.LogDebug("<<< Exiting ModulePermissionCollection(modulePermissions={ModulePermissions},moduleID={ModuleID})", modulePermissions, moduleID);
        }

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionCollection"/> class.</summary>
        /// <param name="objModule">A module from which to copy <see cref="ModuleInfo.ModulePermissions"/>.</param>
        public ModulePermissionCollection(ModuleInfo objModule)
        {
			Logger.LogDebug(">>> Entering ModulePermissionCollection(objModule={ObjModule})", objModule);
            foreach (ModulePermissionInfo permission in objModule.ModulePermissions)
            {
                if (permission.ModuleID == objModule.ModuleID)
                {
                    this.Add(permission);
                }
            }
			Logger.LogDebug("<<< Exiting ModulePermissionCollection(objModule={ObjModule})", objModule);
        }

        public int Add(ModulePermissionInfo value, bool checkForDuplicates)
        {
			Logger.LogDebug(">>> Entering Add(value={Value},checkForDuplicates={CheckForDuplicates})", value, checkForDuplicates);
            int id = Null.NullInteger;
            if (!checkForDuplicates)
            {
                id = this.Add(value);
            }
            else
            {
                bool isMatch = false;
				Logger.LogDebug("Add(value={Value},checkForDuplicates={CheckForDuplicates}): isMatch → {IsMatch}", value, checkForDuplicates, isMatch);
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

			Logger.LogDebug("<<< Exiting Add(value={Value},checkForDuplicates={CheckForDuplicates})", value, checkForDuplicates);
            return id;
        }

        public void AddRange(ArrayList modulePermissions)
        {
            foreach (ModulePermissionInfo permission in modulePermissions)
            {
                this.Add(permission);
            }
			Logger.LogDebug(">>> Entering AddRange(modulePermissions={ModulePermissions})", modulePermissions);
			Logger.LogDebug("<<< Exiting AddRange(modulePermissions={ModulePermissions})", modulePermissions);
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
			Logger.LogDebug(">>> Entering CompareTo(objModulePermissionCollection={ObjModulePermissionCollection})", objModulePermissionCollection);
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

			Logger.LogDebug("<<< Exiting CompareTo(objModulePermissionCollection={ObjModulePermissionCollection})", objModulePermissionCollection);
            return true;
        }

        public void Remove(int permissionID, int roleID, int userID)
        {
			Logger.LogDebug(">>> Entering Remove(permissionID={PermissionID},roleID={RoleID},userID={UserID})", permissionID, roleID, userID);
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
			Logger.LogDebug("<<< Exiting Remove(permissionID={PermissionID},roleID={RoleID},userID={UserID})", permissionID, roleID, userID);
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

        public IEnumerable<ModulePermissionInfo> Where(Func<ModulePermissionInfo, bool> predicate)
        {
			Logger.LogDebug(">>> Entering Where(predicate={Predicate})", predicate);
			Logger.LogDebug("<<< Exiting Where(predicate={Predicate})", predicate);
            return this.Cast<ModulePermissionInfo>().Where(predicate);
        }
    }
}
