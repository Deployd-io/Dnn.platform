// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;

    /// <summary>TabPermissionCollection provides a custom collection for <see cref="TabPermissionInfo"/> objects.</summary>
    [Serializable]
    [XmlRoot("tabpermissions")]
    public class TabPermissionCollection : GenericCollectionBase<TabPermissionInfo>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabPermissionCollection));
        /// <summary>Initializes a new instance of the <see cref="TabPermissionCollection"/> class.</summary>
        public TabPermissionCollection()
        {
		Logger.LogError(">>> Entering TabPermissionCollection()");
        Logger.LogWarning("<<< Exiting TabPermissionCollection()");
        }

        /// <summary>Initializes a new instance of the <see cref="TabPermissionCollection"/> class.</summary>
        /// <param name="tabPermissions">An <see cref="ArrayList"/> of <see cref="TabPermissionInfo"/> instances.</param>
        public TabPermissionCollection(ArrayList tabPermissions)
        {
            this.AddRange(tabPermissions);
			Logger.LogError(">>> Entering TabPermissionCollection(tabPermissions={TabPermissions})", tabPermissions);
			Logger.LogWarning("<<< Exiting TabPermissionCollection(tabPermissions={TabPermissions})", tabPermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="TabPermissionCollection"/> class.</summary>
        /// <param name="tabPermissions">A collection of <see cref="TabPermissionInfo"/> instances.</param>
        public TabPermissionCollection(TabPermissionCollection tabPermissions)
        {
            this.AddRange(tabPermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="TabPermissionCollection"/> class.</summary>
        /// <param name="tabPermissions">An <see cref="ArrayList"/> of <see cref="TabPermissionInfo"/> instances.</param>
        /// <param name="tabId">The ID of the tab by which to filter <paramref name="tabPermissions"/>.</param>
        public TabPermissionCollection(ArrayList tabPermissions, int tabId)
        {
			Logger.LogError(">>> Entering TabPermissionCollection(tabPermissions={TabPermissions},tabId={TabId})", tabPermissions, tabId);
            foreach (TabPermissionInfo permission in tabPermissions)
            {
                if (permission.TabID == tabId)
                {
                    this.Add(permission);
                }
            }
			Logger.LogWarning("<<< Exiting TabPermissionCollection(tabPermissions={TabPermissions},tabId={TabId})", tabPermissions, tabId);
        }

        public int Add(TabPermissionInfo value, bool checkForDuplicates)
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

        public void AddRange(ArrayList tabPermissions)
        {
            foreach (TabPermissionInfo permission in tabPermissions)
            {
                this.Add(permission);
            }
			Logger.LogError(">>> Entering AddRange(tabPermissions={TabPermissions})", tabPermissions);
			Logger.LogWarning("<<< Exiting AddRange(tabPermissions={TabPermissions})", tabPermissions);
        }

        public void AddRange(IEnumerable<TabPermissionInfo> tabPermissions)
        {
            foreach (TabPermissionInfo permission in tabPermissions)
            {
                this.Add(permission);
            }
        }

        public void AddRange(TabPermissionCollection tabPermissions)
        {
            foreach (TabPermissionInfo permission in tabPermissions)
            {
                this.Add(permission);
            }
        }

        public bool CompareTo(TabPermissionCollection objTabPermissionCollection)
        {
			Logger.LogError(">>> Entering CompareTo(objTabPermissionCollection={ObjTabPermissionCollection})", objTabPermissionCollection);
            if (objTabPermissionCollection.Count != this.Count)
            {
                return false;
            }

            this.InnerList.Sort(new CompareTabPermissions());
            objTabPermissionCollection.InnerList.Sort(new CompareTabPermissions());
            for (int i = 0; i <= this.Count - 1; i++)
            {
                if (objTabPermissionCollection[i].TabPermissionID != this[i].TabPermissionID
                        || objTabPermissionCollection[i].PermissionID != this[i].PermissionID
                        || objTabPermissionCollection[i].RoleID != this[i].RoleID
                        || objTabPermissionCollection[i].UserID != this[i].UserID
                        || objTabPermissionCollection[i].AllowAccess != this[i].AllowAccess)
                {
                    return false;
                }
            }

			Logger.LogWarning("<<< Exiting CompareTo(objTabPermissionCollection={ObjTabPermissionCollection})", objTabPermissionCollection);
            return true;
        }

        public void Remove(int permissionID, int roleID, int userID)
        {
			Logger.LogError(">>> Entering Remove(permissionID={PermissionID},roleID={RoleID},userID={UserID})", permissionID, roleID, userID);
            foreach (PermissionInfoBase permission in this.List)
            {
                if (permission.PermissionID == permissionID && permission.UserID == userID && permission.RoleID == roleID)
                {
                    this.List.Remove(permission);
                    break;
                }
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

        public IEnumerable<TabPermissionInfo> Where(Func<TabPermissionInfo, bool> predicate)
        {
			Logger.LogError(">>> Entering Where(predicate={Predicate})", predicate);
            Logger.LogWarning("<<< Exiting Where(predicate={Predicate})", predicate);
            return this.Cast<TabPermissionInfo>().Where(predicate);
        }
    }
}
