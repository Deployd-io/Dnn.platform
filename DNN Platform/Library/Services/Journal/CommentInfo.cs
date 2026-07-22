// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Journal
{
    using System;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Tokens;

    public class CommentInfo : IHydratable, IPropertyAccess
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

        public int CommentId { get; set; }

        public int JournalId { get; set; }

        public string Comment { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }

        public XmlDocument CommentXML { get; set; }

        public int UserId { get; set; }

        public string DisplayName { get; set; }

        /// <inheritdoc />
        public int KeyID
        {
            get
            {
                return this.CommentId;
            }

            set
            {
                this.CommentId = value;
            }
        }

        /// <inheritdoc />
        public void Fill(System.Data.IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.CommentId = Null.SetNullInteger(dr["CommentId"]);
            this.JournalId = Null.SetNullInteger(dr["JournalId"]);
            this.Comment = Null.SetNullString(dr["Comment"]);
            this.DateCreated = Null.SetNullDateTime(dr["DateCreated"]);
            this.DateUpdated = Null.SetNullDateTime(dr["DateUpdated"]);
            if (!string.IsNullOrEmpty(Null.SetNullString(dr["CommentXML"])))
            {
                this.CommentXML = new XmlDocument { XmlResolver = null };
				Logger.LogDebug("Fill(dr={Dr}): this.CommentXML → {Arg1}", dr, this.CommentXML);
                using var commentReader = XmlReader.Create(new StringReader(dr["CommentXML"].ToString()), new XmlReaderSettings { XmlResolver = null, });
                this.CommentXML.Load(commentReader);
				Logger.LogDebug("Fill(dr={Dr}): commentReader → {CommentReader}", dr, commentReader);
            }

            this.UserId = Null.SetNullInteger(dr["UserId"]);
            this.DisplayName = Null.SetNullString(dr["DisplayName"]);
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
			Logger.LogDebug(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            throw new NotImplementedException();
        }
    }
}
