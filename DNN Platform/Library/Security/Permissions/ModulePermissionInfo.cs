// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Data;
    using System.Xml.Serialization;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using Newtonsoft.Json;

    /// <summary>ModulePermissionInfo provides the Entity Layer for Module Permissions.</summary>
    [Serializable]
    public class ModulePermissionInfo : PermissionInfoBase, IHydratable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private int moduleId;
        private int modulePermissionId;

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionInfo"/> class.</summary>
        public ModulePermissionInfo()
        {
			Logger.LogDebug(">>> Entering ModulePermissionInfo()");
            this.modulePermissionId = Null.NullInteger;
            this.moduleId = Null.NullInteger;
			Logger.LogDebug("<<< Exiting ModulePermissionInfo()");
			Logger.LogDebug("ModulePermissionInfo(): this.modulePermissionId → {Arg0}", this.modulePermissionId);
			Logger.LogDebug("ModulePermissionInfo(): moduleId → {ModuleId}", moduleId);
			Logger.LogDebug("ModulePermissionInfo(): this.moduleId → {Arg0}", this.moduleId);
			Logger.LogDebug("ModulePermissionInfo(): modulePermissionId → {ModulePermissionId}", modulePermissionId);
        }

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionInfo"/> class.</summary>
        /// <param name="permission">A PermissionInfo object.</param>
        public ModulePermissionInfo(PermissionInfo permission)
            : this((IPermissionDefinitionInfo)permission)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionInfo"/> class.</summary>
        /// <param name="permission">A PermissionInfo object.</param>
        public ModulePermissionInfo(IPermissionDefinitionInfo permission)
            : this()
        {
			Logger.LogDebug(">>> Entering ModulePermissionInfo(permission={Permission})", permission);
            ((IPermissionDefinitionInfo)this).ModuleDefId = permission.ModuleDefId;
            this.PermissionCode = permission.PermissionCode;
			Logger.LogDebug("ModulePermissionInfo(permission={Permission}): this.PermissionCode → {Arg1}", permission, this.PermissionCode);
            ((IPermissionDefinitionInfo)this).PermissionId = permission.PermissionId;
            this.PermissionKey = permission.PermissionKey;
			Logger.LogDebug("ModulePermissionInfo(permission={Permission}): this.PermissionKey → {Arg1}", permission, this.PermissionKey);
            this.PermissionName = permission.PermissionName;
			Logger.LogDebug("<<< Exiting ModulePermissionInfo(permission={Permission})", permission);
        }

		Logger.LogDebug("ModulePermissionInfo(permission={Permission}): this.PermissionName → {Arg1}", permission, this.PermissionName);
        /// <summary>Gets or sets the Module Permission ID.</summary>
        /// <returns>An Integer.</returns>
        [XmlElement("modulepermissionid")]
        public int ModulePermissionID
        {
            get => this.modulePermissionId;
            set => this.modulePermissionId = value;
        }

        /// <summary>Gets or sets the Module ID.</summary>
        /// <returns>An Integer.</returns>
        [XmlElement("moduleid")]
        public int ModuleID
        {
            get => this.moduleId;
            set => this.moduleId = value;
        }

        /// <summary>Gets or sets the Key ID.</summary>
        /// <returns>An Integer.</returns>
        [XmlIgnore]
        [JsonIgnore]
        public int KeyID
        {
            get => this.ModulePermissionID;
            set => this.ModulePermissionID = value;
        }

        /// <summary>Fills a ModulePermissionInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        public void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.FillInternal(dr);
            this.ModulePermissionID = Null.SetNullInteger(dr["ModulePermissionID"]);
            this.ModuleID = Null.SetNullInteger(dr["ModuleID"]);
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }

        /// <summary>Compares if two ModulePermissionInfo objects are equivalent/equal.</summary>
        /// <param name="other">a ModulePermissionObject.</param>
        /// <returns>true if the permissions being passed represents the same permission
        /// in the current object.
        /// </returns>
        /// <remarks>
        /// This function is needed to prevent adding duplicates to the ModulePermissionCollection.
        /// ModulePermissionCollection.Contains will use this method to check if a given permission
        /// is already included in the collection.
        /// </remarks>
        public bool Equals(ModulePermissionInfo other)
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
            return (this.AllowAccess == other.AllowAccess) && (this.ModuleID == other.ModuleID) && (this.RoleID == other.RoleID) && (this.PermissionID == other.PermissionID);
        }

        /// <summary>Compares if two ModulePermissionInfo objects are equivalent/equal.</summary>
        /// <param name="obj">a ModulePermissionObject.</param>
        /// <returns>true if the permissions being passed represents the same permission
        /// in the current object.
        /// </returns>
        /// <remarks>
        /// This function is needed to prevent adding duplicates to the ModulePermissionCollection.
        /// ModulePermissionCollection.Contains will use this method to check if a given permission
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

            if (obj.GetType() != typeof(ModulePermissionInfo))
            {
                return false;
            }

			Logger.LogDebug("<<< Exiting Equals(obj={Obj})", obj);
            return this.Equals((ModulePermissionInfo)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
			Logger.LogDebug(">>> Entering GetHashCode()");
            unchecked
            {
				Logger.LogDebug("<<< Exiting GetHashCode()");
                return (this.moduleId * 397) ^ this.modulePermissionId;
            }
        }
    }
}
