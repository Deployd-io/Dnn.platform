// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Journal
{
    using System;
    using System.Data;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Tokens;
    using Newtonsoft.Json;

    public class JournalItem : IHydratable, IPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        public int JournalId { get; set; }

        public int JournalTypeId { get; set; }

        public int PortalId { get; set; }

        public int UserId { get; set; }

        public int ProfileId { get; set; }

        public int SocialGroupId { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

        public string Body { get; set; }

        public ItemData ItemData { get; set; }

        public XmlDocument JournalXML { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }

        public string ObjectKey { get; set; }

        public Guid AccessKey { get; set; }

        public string SecuritySet { get; set; }

        public int ContentItemId { get; set; }

        public JournalEntity JournalAuthor { get; set; }

        public JournalEntity JournalOwner { get; set; }

        public string TimeFrame { get; set; }

        public bool CurrentUserLikes { get; set; }

        public string JournalType { get; set; }

        public bool IsDeleted { get; set; }

        public bool CommentsDisabled { get; set; }

        public bool CommentsHidden { get; set; }

        public int SimilarCount { get; set; }

        /// <summary>Gets or sets the key ID.</summary>
        /// <value>
        /// The key ID.
        /// </value>
        /// <remarks>
        /// If you derive class has its own key id, please override this property and set the value to your own key id.
        /// </remarks>
        [XmlIgnore]
        [JsonIgnore]
        public virtual int KeyID
        {
            get
            {
                return this.JournalId;
            }

            set
            {
                this.JournalId = value;
            }
        }

        /// <inheritdoc />
        public void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.JournalId = Null.SetNullInteger(dr["JournalId"]);
            this.JournalTypeId = Null.SetNullInteger(dr["JournalTypeId"]);
            this.PortalId = Null.SetNullInteger(dr["PortalId"]);
            this.UserId = Null.SetNullInteger(dr["UserId"]);
            this.ProfileId = Null.SetNullInteger(dr["ProfileId"]);
            this.SocialGroupId = Null.SetNullInteger(dr["GroupId"]);
            if (!string.IsNullOrEmpty(Null.SetNullString(dr["JournalXML"])))
            {
                this.JournalXML = new XmlDocument { XmlResolver = null };
				Logger.LogDebug("Fill(dr={Dr}): this.JournalXML → {Arg1}", dr, this.JournalXML);
                using (var journalReader = XmlReader.Create(new StringReader(dr["JournalXML"].ToString()), new XmlReaderSettings { XmlResolver = null, }))
                {
                    this.JournalXML.Load(journalReader);
                }

                XmlNode xRoot = this.JournalXML.DocumentElement;
                XmlNode xNode = xRoot.SelectSingleNode("//items/item/body");
                if (xNode != null)
				Logger.LogDebug("Fill(dr={Dr}): xNode → {XNode}", dr, xNode);
                {
                    this.Body = xNode.InnerText;
					Logger.LogDebug("Fill(dr={Dr}): this.Body → {Arg1}", dr, this.Body);
                }
            }

            this.DateCreated = Null.SetNullDateTime(dr["DateCreated"]);
            this.DateUpdated = Null.SetNullDateTime(dr["DateUpdated"]);
            this.ObjectKey = Null.SetNullString(dr["ObjectKey"]);
            this.AccessKey = Null.SetNullGuid(dr["AccessKey"]);
            this.Title = Null.SetNullString(dr["Title"]);
            this.Summary = Null.SetNullString(dr["Summary"]);
            string itemd = Null.SetNullString(dr["ItemData"]);
            this.ItemData = new ItemData();
            if (!string.IsNullOrEmpty(itemd))
            {
                this.ItemData = itemd.FromJson<ItemData>();
            }

            this.ContentItemId = Null.SetNullInteger(dr["ContentItemId"]);
            this.JournalAuthor = new JournalEntity(dr["JournalAuthor"].ToString());
            this.JournalOwner = new JournalEntity(dr["JournalOwner"].ToString());
            this.JournalType = Null.SetNullString(dr["JournalType"]);

            this.IsDeleted = Null.SetNullBoolean(dr["IsDeleted"]);
            this.CommentsDisabled = Null.SetNullBoolean(dr["CommentsDisabled"]);
            this.CommentsHidden = Null.SetNullBoolean(dr["CommentsHidden"]);
            this.SimilarCount = Null.SetNullInteger(dr["SimilarCount"]);
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
			Logger.LogDebug(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            string outputFormat = string.Empty;
            if (format == string.Empty)
			Logger.LogDebug("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound}): outputFormat → {OutputFormat}", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound, outputFormat);
            {
                outputFormat = "g";
            }
            else
            {
                outputFormat = format;
            }

            propertyName = propertyName.ToLowerInvariant();
            switch (propertyName)
			Logger.LogDebug("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound}): propertyName → {PropertyName1}", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound, propertyName);
            {
                case "journalid":
                    return PropertyAccess.FormatString(this.JournalId.ToString(formatProvider), format);
                case "journaltypeid":
                    return PropertyAccess.FormatString(this.JournalTypeId.ToString(formatProvider), format);
                case "profileid":
                    return PropertyAccess.FormatString(this.ProfileId.ToString(formatProvider), format);
                case "socialgroupid":
                    return PropertyAccess.FormatString(this.SocialGroupId.ToString(formatProvider), format);
                case "datecreated":
                    return PropertyAccess.FormatString(this.DateCreated.ToString(formatProvider), format);
                case "title":
                    return PropertyAccess.FormatString(this.Title, format);
                case "summary":
                    return PropertyAccess.FormatString(this.Summary, format);
                case "body":
                    return PropertyAccess.FormatString(this.Body, format);
                case "timeframe":
                    return PropertyAccess.FormatString(this.TimeFrame, format);
                case "isdeleted":
                    return this.IsDeleted.ToString();
            }

            propertyNotFound = true;
			Logger.LogDebug("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            return string.Empty;
        }
    }
}
