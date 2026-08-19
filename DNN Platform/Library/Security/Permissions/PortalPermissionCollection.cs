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

    /// <summary>PortalPermissionCollection provides a custom collection for <see cref="PortalPermissionInfo"/> objects.</summary>
    [Serializable]
    [XmlRoot("portalpermissions")]
    public class PortalPermissionCollection : GenericCollectionBase<PortalPermissionInfo>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalPermissionCollection));
        /// <summary>Initializes a new instance of the <see cref="PortalPermissionCollection"/> class.</summary>
        public PortalPermissionCollection()
        {
		Logger.LogDebug(">>> Entering PortalPermissionCollection()");
		Logger.LogDebug("<<< Exiting PortalPermissionCollection()");
        }

        /// <summary>Initializes a new instance of the <see cref="PortalPermissionCollection"/> class.</summary>
        /// <param name="portalPermissions">An <see cref="ArrayList"/> of <see cref="PortalPermissionInfo"/> instances.</param>
        public PortalPermissionCollection(ArrayList portalPermissions)
        {
            this.AddRange(portalPermissions);
			Logger.LogDebug(">>> Entering PortalPermissionCollection(portalPermissions={PortalPermissions})", portalPermissions);
			Logger.LogDebug("<<< Exiting PortalPermissionCollection(portalPermissions={PortalPermissions})", portalPermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="PortalPermissionCollection"/> class.</summary>
        /// <param name="portalPermissions">A collection of <see cref="PortalPermissionInfo"/> instances.</param>
        public PortalPermissionCollection(PortalPermissionCollection portalPermissions)
        {
            this.AddRange(portalPermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="PortalPermissionCollection"/> class.</summary>
        /// <param name="portalPermissions">An <see cref="ArrayList"/> of <see cref="PortalPermissionInfo"/> instances.</param>
        /// <param name="portalId">The ID of the portal by which to filter <paramref name="portalPermissions"/>.</param>
        public PortalPermissionCollection(ArrayList portalPermissions, int portalId)
        {
			Logger.LogDebug(">>> Entering PortalPermissionCollection(portalPermissions={PortalPermissions},portalId={PortalId})", portalPermissions, portalId);
            foreach (PortalPermissionInfo permission in portalPermissions)
            {
                if (permission.PortalID == portalId)
                {
                    this.Add(permission);
                }
            }
			Logger.LogDebug("<<< Exiting PortalPermissionCollection(portalPermissions={PortalPermissions},portalId={PortalId})", portalPermissions, portalId);
        }

        public int Add(PortalPermissionInfo value, bool checkForDuplicates)
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

        public void AddRange(ArrayList portalPermissions)
        {
            foreach (PortalPermissionInfo permission in portalPermissions)
            {
                this.Add(permission);
            }
			Logger.LogDebug(">>> Entering AddRange(portalPermissions={PortalPermissions})", portalPermissions);
			Logger.LogDebug("<<< Exiting AddRange(portalPermissions={PortalPermissions})", portalPermissions);
        }

        public void AddRange(IEnumerable<PortalPermissionInfo> portalPermissions)
        {
            foreach (PortalPermissionInfo permission in portalPermissions)
            {
                this.Add(permission);
            }
        }

        public void AddRange(PortalPermissionCollection portalPermissions)
        {
            foreach (PortalPermissionInfo permission in portalPermissions)
            {
                this.Add(permission);
            }
        }

        public bool CompareTo(PortalPermissionCollection objPortalPermissionCollection)
        {
			Logger.LogDebug(">>> Entering CompareTo(objPortalPermissionCollection={ObjPortalPermissionCollection})", objPortalPermissionCollection);
            if (objPortalPermissionCollection.Count != this.Count)
            {
                return false;
            }

            this.InnerList.Sort(new ComparePortalPermissions());
            objPortalPermissionCollection.InnerList.Sort(new ComparePortalPermissions());
            for (int i = 0; i <= this.Count - 1; i++)
            {
                if (objPortalPermissionCollection[i].PortalPermissionID != this[i].PortalPermissionID
                        || objPortalPermissionCollection[i].PermissionID != this[i].PermissionID
                        || objPortalPermissionCollection[i].RoleID != this[i].RoleID
                        || objPortalPermissionCollection[i].UserID != this[i].UserID
                        || objPortalPermissionCollection[i].AllowAccess != this[i].AllowAccess)
                {
                    return false;
                }
            }

			Logger.LogDebug("<<< Exiting CompareTo(objPortalPermissionCollection={ObjPortalPermissionCollection})", objPortalPermissionCollection);
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

        public IEnumerable<PortalPermissionInfo> Where(Func<PortalPermissionInfo, bool> predicate)
        {
			Logger.LogDebug(">>> Entering Where(predicate={Predicate})", predicate);
			Logger.LogDebug("<<< Exiting Where(predicate={Predicate})", predicate);
            return this.Cast<PortalPermissionInfo>().Where(predicate);
        }
    }
}
