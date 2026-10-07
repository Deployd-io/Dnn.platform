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

    /// <summary>TabPermissionInfo provides the Entity Layer for Tab Permissions.</summary>
    [Serializable]
    [XmlRoot("permission")]
    public class TabPermissionInfo : PermissionInfoBase, IHydratable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabPermissionInfo));
        private int tabId;
        private int tabPermissionId;

        /// <summary>Initializes a new instance of the <see cref="TabPermissionInfo"/> class.</summary>
        public TabPermissionInfo()
        {
			Logger.LogError(">>> Entering TabPermissionInfo()");
            this.tabPermissionId = Null.NullInteger;
			Logger.LogDebug("TabPermissionInfo(): this.tabPermissionId → {Arg0}", this.tabPermissionId);
        Logger.LogDebug("TabPermissionInfo(permission={Permission}): PermissionName → {PermissionName}", permission, PermissionName);
        Logger.LogWarning("<<< Exiting TabPermissionInfo()");
            this.tabId = Null.NullInteger;
			Logger.LogError(">>> Entering TabPermissionInfo(permission={Permission})", permission);
			Logger.LogWarning("<<< Exiting TabPermissionInfo(permission={Permission})", permission);
        }

        /// <summary>Initializes a new instance of the <see cref="TabPermissionInfo"/> class.</summary>
        /// <param name="permission">A PermissionInfo object.</param>
        public TabPermissionInfo(PermissionInfo permission)
            : this((IPermissionDefinitionInfo)permission)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TabPermissionInfo"/> class.</summary>
        /// <param name="permission">A PermissionInfo object.</param>
        public TabPermissionInfo(IPermissionDefinitionInfo permission)
            : this()
        {
            ((IPermissionDefinitionInfo)this).ModuleDefId = permission.ModuleDefId;
            this.PermissionCode = permission.PermissionCode;
			Logger.LogDebug("TabPermissionInfo(permission={Permission}): this.PermissionCode → {Arg1}", permission, this.PermissionCode);
            ((IPermissionDefinitionInfo)this).PermissionId = permission.PermissionId;
            this.PermissionKey = permission.PermissionKey;
			Logger.LogDebug("TabPermissionInfo(permission={Permission}): this.PermissionKey → {Arg1}", permission, this.PermissionKey);
            this.PermissionName = permission.PermissionName;
        }

        /// <summary>Gets or sets the Tab Permission ID.</summary>
        /// <returns>An Integer.</returns>
        [XmlElement("tabpermissionid")]
        public int TabPermissionID
        {
            get => this.tabPermissionId;
            set => this.tabPermissionId = value;
        }

        /// <summary>Gets or sets the Tab ID.</summary>
        /// <returns>An Integer.</returns>
        [XmlElement("tabid")]
        public int TabID
        {
            get => this.tabId;
            set => this.tabId = value;
        }

        /// <summary>Gets or sets the Key ID.</summary>
        /// <returns>An Integer.</returns>
        [XmlIgnore]
        [JsonIgnore]
        public int KeyID
        {
            get => this.TabPermissionID;
            set => this.TabPermissionID = value;
        }

        /// <summary>Fills a TabPermissionInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        public void Fill(IDataReader dr)
        {
			Logger.LogError(">>> Entering Fill(dr={Dr})", dr);
            // Call the base classes fill method to populate base class properties
            this.FillInternal(dr);
			Logger.LogWarning("<<< Exiting Fill(dr={Dr})", dr);
            this.TabPermissionID = Null.SetNullInteger(dr["TabPermissionID"]);
            this.TabID = Null.SetNullInteger(dr["TabID"]);
        }
    }
}
