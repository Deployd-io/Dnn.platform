// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Data;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    /// <summary>DesktopModulePermissionInfo provides the Entity Layer for DesktopModulePermissionInfo Permissions.</summary>
    [Serializable]
    public class DesktopModulePermissionInfo : PermissionInfoBase, IHydratable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        // local property declarations
        private int desktopModulePermissionID;
        private int portalDesktopModuleID;

        /// <summary>Initializes a new instance of the <see cref="DesktopModulePermissionInfo"/> class.</summary>
        public DesktopModulePermissionInfo()
        {
			Logger.LogDebug(">>> Entering DesktopModulePermissionInfo()");
            this.desktopModulePermissionID = Null.NullInteger;
            this.portalDesktopModuleID = Null.NullInteger;
			Logger.LogDebug("<<< Exiting DesktopModulePermissionInfo()");
			Logger.LogDebug("DesktopModulePermissionInfo(): this.desktopModulePermissionID → {Arg0}", this.desktopModulePermissionID);
			Logger.LogDebug("DesktopModulePermissionInfo(): portalDesktopModuleID → {PortalDesktopModuleID}", portalDesktopModuleID);
			Logger.LogDebug("DesktopModulePermissionInfo(): this.portalDesktopModuleID → {Arg0}", this.portalDesktopModuleID);
			Logger.LogDebug("DesktopModulePermissionInfo(): desktopModulePermissionID → {DesktopModulePermissionID}", desktopModulePermissionID);
        }

        /// <summary>Initializes a new instance of the <see cref="DesktopModulePermissionInfo"/> class.</summary>
        /// <param name="permission">A PermissionInfo object.</param>
        public DesktopModulePermissionInfo(PermissionInfo permission)
            : this((IPermissionDefinitionInfo)permission)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DesktopModulePermissionInfo"/> class.</summary>
        /// <param name="permission">A PermissionInfo object.</param>
        public DesktopModulePermissionInfo(IPermissionDefinitionInfo permission)
            : this()
        {
			Logger.LogDebug(">>> Entering DesktopModulePermissionInfo(permission={Permission})", permission);
            ((IPermissionInfo)this).ModuleDefId = permission.ModuleDefId;
            this.PermissionCode = permission.PermissionCode;
			Logger.LogDebug("DesktopModulePermissionInfo(permission={Permission}): this.PermissionCode → {Arg1}", permission, this.PermissionCode);
            ((IPermissionInfo)this).PermissionId = permission.PermissionId;
            this.PermissionKey = permission.PermissionKey;
			Logger.LogDebug("DesktopModulePermissionInfo(permission={Permission}): this.PermissionKey → {Arg1}", permission, this.PermissionKey);
            this.PermissionName = permission.PermissionName;
			Logger.LogDebug("<<< Exiting DesktopModulePermissionInfo(permission={Permission})", permission);
        }

		Logger.LogDebug("DesktopModulePermissionInfo(permission={Permission}): this.PermissionName → {Arg1}", permission, this.PermissionName);
        /// <summary>Gets or sets the DesktopModule Permission ID.</summary>
        /// <returns>An Integer.</returns>
        public int DesktopModulePermissionID
        {
            get => this.desktopModulePermissionID;
            set => this.desktopModulePermissionID = value;
        }

        /// <summary>Gets or sets the PortalDesktopModule ID.</summary>
        /// <returns>An Integer.</returns>
        public int PortalDesktopModuleID
        {
            get => this.portalDesktopModuleID;
            set => this.portalDesktopModuleID = value;
        }

        /// <summary>Gets or sets the Key ID.</summary>
        /// <returns>An Integer.</returns>
        public int KeyID
        {
            get => this.DesktopModulePermissionID;
            set => this.DesktopModulePermissionID = value;
        }

        /// <summary>Fills a DesktopModulePermissionInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        public void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.FillInternal(dr);
            this.DesktopModulePermissionID = Null.SetNullInteger(dr["DesktopModulePermissionID"]);
            this.PortalDesktopModuleID = Null.SetNullInteger(dr["PortalDesktopModuleID"]);
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }

        /// <summary>Compares if two DesktopModulePermissionInfo objects are equivalent/equal.</summary>
        /// <param name="other">a DesktopModulePermissionObject.</param>
        /// <returns>true if the permissions being passed represents the same permission
        /// in the current object.
        /// </returns>
        /// <remarks>
        /// This function is needed to prevent adding duplicates to the DesktopModulePermissionCollection.
        /// DesktopModulePermissionCollection.Contains will use this method to check if a given permission
        /// is already included in the collection.
        /// </remarks>
        public bool Equals(DesktopModulePermissionInfo other)
        {
			Logger.LogDebug(">>> Entering Equals(other={Other})", other);
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

			Logger.LogDebug("<<< Exiting Equals(other={Other})", other);
            return (this.AllowAccess == other.AllowAccess) && (this.PortalDesktopModuleID == other.PortalDesktopModuleID) && (this.RoleID == other.RoleID) && (this.PermissionID == other.PermissionID);
        }

        /// <summary>Compares if two DesktopModulePermissionInfo objects are equivalent/equal.</summary>
        /// <param name="obj">a DesktopModulePermissionObject.</param>
        /// <returns>true if the permissions being passed represents the same permission
        /// in the current object.
        /// </returns>
        /// <remarks>
        /// This function is needed to prevent adding duplicates to the DesktopModulePermissionCollection.
        /// DesktopModulePermissionCollection.Contains will use this method to check if a given permission
        /// is already included in the collection.
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(DesktopModulePermissionInfo))
            {
                return false;
            }

			Logger.LogDebug("<<< Exiting Equals(obj={Obj})", obj);
            return this.Equals((DesktopModulePermissionInfo)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
			Logger.LogDebug(">>> Entering GetHashCode()");
            unchecked
            {
				Logger.LogDebug("<<< Exiting GetHashCode()");
                return (this.desktopModulePermissionID * 397) ^ this.portalDesktopModuleID;
            }
        }
    }
}
