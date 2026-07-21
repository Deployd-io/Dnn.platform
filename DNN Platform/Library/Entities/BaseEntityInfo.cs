// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using Newtonsoft.Json;

    /// <summary>BaseEntityInfo provides auditing fields for Core tables.</summary>
    [Serializable]
    public abstract class BaseEntityInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="BaseEntityInfo"/> class.</summary>
        protected BaseEntityInfo()
        {
			Logger.LogDebug(">>> Entering BaseEntityInfo()");
            this.CreatedByUserID = Null.NullInteger;
            this.LastModifiedByUserID = Null.NullInteger;
			Logger.LogDebug("<<< Exiting BaseEntityInfo()");
			Logger.LogDebug("BaseEntityInfo(): this.CreatedByUserID → {Arg0}", this.CreatedByUserID);
			Logger.LogDebug("BaseEntityInfo(): this.LastModifiedByUserID → {Arg0}", this.LastModifiedByUserID);
        }

        /// <summary>Gets the CreatedByUserID.</summary>
        /// <returns>An Integer.</returns>
        [Browsable(false)]
        [XmlIgnore]
        [JsonIgnore]
        public int CreatedByUserID { get; internal set; }

        /// <summary>Gets the CreatedOnDate.</summary>
        /// <returns>A DateTime.</returns>
        [Browsable(false)]
        [XmlIgnore]
        [JsonIgnore]
        public DateTime CreatedOnDate { get; private set; }

        /// <summary>Gets the LastModifiedByUserID.</summary>
        /// <returns>An Integer.</returns>
        [Browsable(false)]
        [XmlIgnore]
        [JsonIgnore]
        public int LastModifiedByUserID { get; internal set; }

        /// <summary>Gets the LastModifiedOnDate.</summary>
        /// <returns>A DateTime.</returns>
        [Browsable(false)]
        [XmlIgnore]
        [JsonIgnore]
        public DateTime LastModifiedOnDate { get; private set; }

        /// <summary>Gets the UserInfo object associated with this user.</summary>
        /// <param name="portalId">The PortalID associated with the desired user.</param>
        /// <returns>A UserInfo object.</returns>
        public UserInfo CreatedByUser(int portalId)
        {
			Logger.LogDebug(">>> Entering CreatedByUser(portalId={PortalId})", portalId);
            if (this.CreatedByUserID > Null.NullInteger)
            {
                UserInfo user = UserController.GetUserById(portalId, this.CreatedByUserID);
                return user;
            }

			Logger.LogDebug("<<< Exiting CreatedByUser(portalId={PortalId})", portalId);
            return null;
        }

        /// <summary>Gets the UserInfo object associated with this user.</summary>
        /// <param name="portalId">The PortalID associated with the desired user.</param>
        /// <returns>A UserInfo object.</returns>
        public UserInfo LastModifiedByUser(int portalId)
        {
			Logger.LogDebug(">>> Entering LastModifiedByUser(portalId={PortalId})", portalId);
            if (this.LastModifiedByUserID > Null.NullInteger)
            {
                UserInfo user = UserController.GetUserById(portalId, this.LastModifiedByUserID);
                return user;
            }

			Logger.LogDebug("<<< Exiting LastModifiedByUser(portalId={PortalId})", portalId);
            return null;
        }

        /// <summary>method used by cbo to fill readonly properties ignored by HydrateObject reflection.</summary>
        /// <param name="dr">the data reader to use.</param>
        internal void FillBaseProperties(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering FillBaseProperties(dr={Dr})", dr);
            this.FillInternal(dr);
			Logger.LogDebug("<<< Exiting FillBaseProperties(dr={Dr})", dr);
        }

        /// <summary>Fills a BaseEntityInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        protected virtual void FillInternal(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering FillInternal(dr={Dr})", dr);
            this.CreatedByUserID = Null.SetNullInteger(dr["CreatedByUserID"]);
            this.CreatedOnDate = Null.SetNullDateTime(dr["CreatedOnDate"]);
            this.LastModifiedByUserID = Null.SetNullInteger(dr["LastModifiedByUserID"]);
            this.LastModifiedOnDate = Null.SetNullDateTime(dr["LastModifiedOnDate"]);
			Logger.LogDebug("<<< Exiting FillInternal(dr={Dr})", dr);
        }

        /// <summary>Clones the base properties of the entity.</summary>
        /// <param name="clonedItem">The cloned entity (cloned to).</param>
        /// <param name="originalItem">The original entity (cloned from).</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected void CloneBaseProperties(BaseEntityInfo clonedItem, BaseEntityInfo originalItem)
        {
			Logger.LogDebug(">>> Entering CloneBaseProperties(clonedItem={ClonedItem},originalItem={OriginalItem})", clonedItem, originalItem);
            clonedItem.CreatedByUserID = originalItem.CreatedByUserID;
            clonedItem.CreatedOnDate = originalItem.CreatedOnDate;
			Logger.LogDebug("CloneBaseProperties(clonedItem={ClonedItem},originalItem={OriginalItem}): clonedItem.CreatedOnDate → {Arg2}", clonedItem, originalItem, clonedItem.CreatedOnDate);
			Logger.LogDebug("CloneBaseProperties(clonedItem={ClonedItem},originalItem={OriginalItem}): clonedItem.LastModifiedByUserID → {Arg2}", clonedItem, originalItem, clonedItem.LastModifiedByUserID);
			Logger.LogDebug("CloneBaseProperties(clonedItem={ClonedItem},originalItem={OriginalItem}): clonedItem.LastModifiedOnDate → {Arg2}", clonedItem, originalItem, clonedItem.LastModifiedOnDate);
			Logger.LogDebug("CloneBaseProperties(clonedItem={ClonedItem},originalItem={OriginalItem}): clonedItem.CreatedByUserID → {Arg2}", clonedItem, originalItem, clonedItem.CreatedByUserID);
            clonedItem.LastModifiedByUserID = originalItem.LastModifiedByUserID;
            clonedItem.LastModifiedOnDate = originalItem.LastModifiedOnDate;
			Logger.LogDebug("<<< Exiting CloneBaseProperties(clonedItem={ClonedItem},originalItem={OriginalItem})", clonedItem, originalItem);
        }
    }
}
