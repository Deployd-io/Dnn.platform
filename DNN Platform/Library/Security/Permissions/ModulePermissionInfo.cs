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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModulePermissionInfo));
        private int moduleId;
        private int modulePermissionId;

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionInfo"/> class.</summary>
        public ModulePermissionInfo()
        {
			Logger.LogError(">>> Entering ModulePermissionInfo()");
            this.modulePermissionId = Null.NullInteger;
			Logger.LogDebug("ModulePermissionInfo(): this.modulePermissionId → {Arg0}", this.modulePermissionId);
        Logger.LogDebug("ModulePermissionInfo(permission={Permission}): PermissionName → {PermissionName}", permission, PermissionName);
        Logger.LogWarning("<<< Exiting ModulePermissionInfo()");
            this.moduleId = Null.NullInteger;
			Logger.LogError(">>> Entering ModulePermissionInfo(permission={Permission})", permission);
			Logger.LogWarning("<<< Exiting ModulePermissionInfo(permission={Permission})", permission);
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
            ((IPermissionDefinitionInfo)this).ModuleDefId = permission.ModuleDefId;
            this.PermissionCode = permission.PermissionCode;
			Logger.LogDebug("ModulePermissionInfo(permission={Permission}): this.PermissionCode → {Arg1}", permission, this.PermissionCode);
            ((IPermissionDefinitionInfo)this).PermissionId = permission.PermissionId;
            this.PermissionKey = permission.PermissionKey;
			Logger.LogDebug("ModulePermissionInfo(permission={Permission}): this.PermissionKey → {Arg1}", permission, this.PermissionKey);
            this.PermissionName = permission.PermissionName;
        }

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
			Logger.LogError(">>> Entering Fill(dr={Dr})", dr);
            this.FillInternal(dr);
			Logger.LogWarning("<<< Exiting Fill(dr={Dr})", dr);
            this.ModulePermissionID = Null.SetNullInteger(dr["ModulePermissionID"]);
            this.ModuleID = Null.SetNullInteger(dr["ModuleID"]);
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
			Logger.LogError(">>> Entering Equals(other={Other})", other);
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

			Logger.LogWarning("<<< Exiting Equals(other={Other})", other);
			Logger.LogWarning("<<< Exiting Equals(obj={Obj})", obj);
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

            return this.Equals((ModulePermissionInfo)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
			Logger.LogError(">>> Entering GetHashCode()");
            unchecked
            {
				Logger.LogWarning("<<< Exiting GetHashCode()");
                return (this.moduleId * 397) ^ this.modulePermissionId;
            }
        }
    }
}
