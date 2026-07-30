// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Lists
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;

    /// <summary>Represents the information relative to a list.</summary>
    [Serializable]
    public class ListInfo : BaseEntityInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="ListInfo"/> class.</summary>
        public ListInfo()
            : this(string.Empty)
        {
		Logger.LogDebug(">>> Entering ListInfo()");
		Logger.LogDebug(">>> Entering ListInfo(name={Name})", name);
		Logger.LogDebug("<<< Exiting ListInfo(name={Name})", name);
        }

        /// <summary>Initializes a new instance of the <see cref="ListInfo"/> class.</summary>
        /// <param name="name">The name of the list.</param>
        public ListInfo(string name)
        {
            this.SystemList = Null.NullBoolean;
            this.EnableSortOrder = Null.NullBoolean;
			Logger.LogDebug("ListInfo(name={Name}): this.SystemList → {Arg1}", name, this.SystemList);
			Logger.LogDebug("ListInfo(name={Name}): this.EnableSortOrder → {Arg1}", name, this.EnableSortOrder);
            this.IsPopulated = Null.NullBoolean;
            this.ParentList = Null.NullString;
            this.Parent = Null.NullString;
			Logger.LogDebug("ListInfo(name={Name}): this.IsPopulated → {Arg1}", name, this.IsPopulated);
			Logger.LogDebug("ListInfo(name={Name}): this.ParentList → {Arg1}", name, this.ParentList);
			Logger.LogDebug("ListInfo(name={Name}): this.Parent → {Arg1}", name, this.Parent);
            this.ParentKey = Null.NullString;
            this.PortalID = Null.NullInteger;
            this.DefinitionID = Null.NullInteger;
            this.Name = name;
			Logger.LogDebug("<<< Exiting ListInfo(name={Name})", name);
			Logger.LogDebug("ListInfo(name={Name}): this.ParentKey → {Arg1}", name, this.ParentKey);
			Logger.LogDebug("ListInfo(name={Name}): this.PortalID → {Arg1}", name, this.PortalID);
			Logger.LogDebug("ListInfo(name={Name}): this.DefinitionID → {Arg1}", name, this.DefinitionID);
			Logger.LogDebug("ListInfo(name={Name}): this.Name → {Arg1}", name, this.Name);
        }

        /// <summary>Gets the display name, which includes the <see cref="Parent"/> list and this list's <see cref="Name"/>.</summary>
        public string DisplayName
        {
            get
            {
                string displayName = this.Parent;
                if (!string.IsNullOrEmpty(displayName))
                {
                    displayName += ":";
                }

                return displayName + this.Name;
            }
        }

        /// <summary>Gets a unique key to identify this list, which includes the <see cref="ParentKey"/> and this list's <see cref="Name"/>.</summary>
        public string Key
        {
            get
            {
                string key = this.ParentKey;
                if (!string.IsNullOrEmpty(key))
                {
                    key += ":";
                }

                return key + this.Name;
            }
        }

        /// <summary>Gets or sets the id of the definition.</summary>
        public int DefinitionID { get; set; }

        /// <summary>Gets or sets a value indicating whether to enable the sort order.</summary>
        public bool EnableSortOrder { get; set; }

        /// <summary>Gets or sets the total number of entries.</summary>
        public int EntryCount { get; set; }

        /// <summary>Gets or sets a value indicating whether this instance is populated.</summary>
        public bool IsPopulated { get; set; }

        /// <summary>Gets or sets a value indicating how deep this list in nested into the hierarchy.</summary>
        public int Level { get; set; }

        /// <summary>Gets or sets the list name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the parent.</summary>
        public string Parent { get; set; }

        /// <summary>Gets or sets the id of the parent list.</summary>
        public int ParentID { get; set; }

        /// <summary>Gets or sets the parent key.</summary>
        public string ParentKey { get; set; }

        /// <summary>Gets or sets the parent list.</summary>
        public string ParentList { get; set; }

        /// <summary>Gets or sets the id of the site (portal).</summary>
        public int PortalID { get; set; }

        /// <summary>Gets or sets a value indicating whether this list is a system list.</summary>
        public bool SystemList { get; set; }
    }
}
