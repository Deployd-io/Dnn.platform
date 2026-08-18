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

    [Serializable]
    public class FolderPermissionCollection : GenericCollectionBase<FolderPermissionInfo>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FolderPermissionCollection));
        /// <summary>Initializes a new instance of the <see cref="FolderPermissionCollection"/> class.</summary>
        public FolderPermissionCollection()
        {
		Logger.LogDebug(">>> Entering FolderPermissionCollection()");
		Logger.LogDebug("<<< Exiting FolderPermissionCollection()");
        }

        /// <summary>Initializes a new instance of the <see cref="FolderPermissionCollection"/> class.</summary>
        /// <param name="folderPermissions">An <see cref="ArrayList"/> of <see cref="FolderPermissionInfo"/> instances.</param>
        public FolderPermissionCollection(ArrayList folderPermissions)
        {
            this.AddRange(folderPermissions);
			Logger.LogDebug(">>> Entering FolderPermissionCollection(folderPermissions={FolderPermissions})", folderPermissions);
			Logger.LogDebug("<<< Exiting FolderPermissionCollection(folderPermissions={FolderPermissions})", folderPermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="FolderPermissionCollection"/> class.</summary>
        /// <param name="folderPermissions">A collection of <see cref="FolderPermissionInfo"/> instances.</param>
        public FolderPermissionCollection(FolderPermissionCollection folderPermissions)
        {
            this.AddRange(folderPermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="FolderPermissionCollection"/> class.</summary>
        /// <param name="folderPermissions">An <see cref="ArrayList"/> of <see cref="FolderPermissionInfo"/> instances.</param>
        /// <param name="folderPath">The path of the folder by which to filter <paramref name="folderPermissions"/>.</param>
        public FolderPermissionCollection(ArrayList folderPermissions, string folderPath)
        {
			Logger.LogDebug(">>> Entering FolderPermissionCollection(folderPermissions={FolderPermissions},folderPath={FolderPath})", folderPermissions, folderPath);
            foreach (FolderPermissionInfo permission in folderPermissions)
            {
                if (permission.FolderPath.Equals(folderPath, StringComparison.OrdinalIgnoreCase))
                {
                    this.Add(permission);
                }
            }
			Logger.LogDebug("<<< Exiting FolderPermissionCollection(folderPermissions={FolderPermissions},folderPath={FolderPath})", folderPermissions, folderPath);
        }

        public int Add(FolderPermissionInfo value, bool checkForDuplicates)
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

        public void AddRange(IEnumerable<FolderPermissionInfo> folderPermissions)
        {
            foreach (var permission in folderPermissions)
            {
                this.List.Add(permission);
            }
			Logger.LogDebug("<<< Exiting AddRange(folderPermissions={FolderPermissions})", folderPermissions);
        }

        public void AddRange(ArrayList folderPermissions)
        {
            foreach (FolderPermissionInfo permission in folderPermissions)
            {
                this.List.Add(permission);
            }
			Logger.LogDebug(">>> Entering AddRange(folderPermissions={FolderPermissions})", folderPermissions);
        }

        public void AddRange(FolderPermissionCollection folderPermissions)
        {
            foreach (FolderPermissionInfo permission in folderPermissions)
            {
                this.List.Add(permission);
            }
        }

        public void Remove(int permissionID, int roleID, int userID)
        {
			Logger.LogDebug(">>> Entering Remove(permissionID={PermissionID},roleID={RoleID},userID={UserID})", permissionID, roleID, userID);
            foreach (IPermissionInfo permission in this.List)
            {
                if (permission.PermissionId == permissionID && permission.UserId == userID && permission.RoleId == roleID)
                {
                    this.List.Remove(permission);
                    break;
                }
            }
			Logger.LogDebug("<<< Exiting Remove(permissionID={PermissionID},roleID={RoleID},userID={UserID})", permissionID, roleID, userID);
        }

        public bool Contains(string key, int folderId, int roleId, int userId)
        {
			Logger.LogDebug(">>> Entering Contains(key={Key},folderId={FolderId},roleId={RoleId},userId={UserId})", key, folderId, roleId, userId);
            bool result = Null.NullBoolean;
            foreach (IFolderPermissionInfo permission in this.List)
            {
			Logger.LogDebug("Contains(key={Key},folderId={FolderId},roleId={RoleId},userId={UserId}): result → {Result}", key, folderId, roleId, userId, result);
                if (permission.PermissionKey == key && permission.FolderId == folderId && permission.RoleId == roleId && permission.UserId == userId)
                {
                    result = true;
                    break;
                }
            }

			Logger.LogDebug("<<< Exiting Contains(key={Key},folderId={FolderId},roleId={RoleId},userId={UserId})", key, folderId, roleId, userId);
            return result;
        }

        public bool CompareTo(FolderPermissionCollection objFolderPermissionCollection)
        {
			Logger.LogDebug(">>> Entering CompareTo(objFolderPermissionCollection={ObjFolderPermissionCollection})", objFolderPermissionCollection);
            if (objFolderPermissionCollection.Count != this.Count)
            {
                return false;
            }

            this.InnerList.Sort(new CompareFolderPermissions());
            objFolderPermissionCollection.InnerList.Sort(new CompareFolderPermissions());
            for (int i = 0; i <= this.Count - 1; i++)
            {
                IFolderPermissionInfo otherPermission = objFolderPermissionCollection[i];
                IFolderPermissionInfo thisPermission = this[i];
                if (otherPermission.FolderPermissionId != thisPermission.FolderPermissionId || otherPermission.AllowAccess != thisPermission.AllowAccess)
                {
                    return false;
                }
            }

			Logger.LogDebug("<<< Exiting CompareTo(objFolderPermissionCollection={ObjFolderPermissionCollection})", objFolderPermissionCollection);
            return true;
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
    }
}
