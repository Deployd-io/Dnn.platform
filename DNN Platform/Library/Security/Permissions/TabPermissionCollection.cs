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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="TabPermissionCollection"/> class.</summary>
        public TabPermissionCollection()
        {
		Logger.LogDebug(">>> Entering TabPermissionCollection()");
		Logger.LogDebug("<<< Exiting TabPermissionCollection()");
        }

        /// <summary>Initializes a new instance of the <see cref="TabPermissionCollection"/> class.</summary>
        /// <param name="tabPermissions">An <see cref="ArrayList"/> of <see cref="TabPermissionInfo"/> instances.</param>
        public TabPermissionCollection(ArrayList tabPermissions)
        {
            this.AddRange(tabPermissions);
			Logger.LogDebug(">>> Entering TabPermissionCollection(tabPermissions={TabPermissions})", tabPermissions);
			Logger.LogDebug("<<< Exiting TabPermissionCollection(tabPermissions={TabPermissions})", tabPermissions);
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
			Logger.LogDebug(">>> Entering TabPermissionCollection(tabPermissions={TabPermissions},tabId={TabId})", tabPermissions, tabId);
            foreach (TabPermissionInfo permission in tabPermissions)
            {
                if (permission.TabID == tabId)
                {
                    this.Add(permission);
                }
            }
			Logger.LogDebug("<<< Exiting TabPermissionCollection(tabPermissions={TabPermissions},tabId={TabId})", tabPermissions, tabId);
        }

        public int Add(TabPermissionInfo value, bool checkForDuplicates)
        {
			Logger.LogDebug(">>> Entering Add(value={Value},checkForDuplicates={CheckForDuplicates})", value, checkForDuplicates);
            int id = Null.NullInteger;

			Logger.LogDebug("Add(value={Value},checkForDuplicates={CheckForDuplicates}): id → {Id}", value, checkForDuplicates, id);
            if (!checkForDuplicates)
            {
                id = this.Add(value);
            }
            else
            {
                bool isMatch = false;
                foreach (IPermissionInfo permission in this.List)
                {
				Logger.LogDebug("Add(value={Value},checkForDuplicates={CheckForDuplicates}): isMatch → {IsMatch}", value, checkForDuplicates, isMatch);
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

        public void AddRange(ArrayList tabPermissions)
        {
            foreach (TabPermissionInfo permission in tabPermissions)
            {
                this.Add(permission);
            }
			Logger.LogDebug(">>> Entering AddRange(tabPermissions={TabPermissions})", tabPermissions);
			Logger.LogDebug("<<< Exiting AddRange(tabPermissions={TabPermissions})", tabPermissions);
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
			Logger.LogDebug(">>> Entering CompareTo(objTabPermissionCollection={ObjTabPermissionCollection})", objTabPermissionCollection);
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

			Logger.LogDebug("<<< Exiting CompareTo(objTabPermissionCollection={ObjTabPermissionCollection})", objTabPermissionCollection);
            return true;
        }

        public void Remove(int permissionID, int roleID, int userID)
        {
			Logger.LogDebug(">>> Entering Remove(permissionID={PermissionID},roleID={RoleID},userID={UserID})", permissionID, roleID, userID);
            foreach (PermissionInfoBase permission in this.List)
            {
                if (permission.PermissionID == permissionID && permission.UserID == userID && permission.RoleID == roleID)
                {
                    this.List.Remove(permission);
                    break;
                }
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

        public IEnumerable<TabPermissionInfo> Where(Func<TabPermissionInfo, bool> predicate)
        {
            return this.Cast<TabPermissionInfo>().Where(predicate);
        }
    }
}
