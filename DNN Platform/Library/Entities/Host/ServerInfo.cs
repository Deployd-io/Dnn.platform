// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Host
{
    using System;
    using System.Data;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    [Serializable]
    public class ServerInfo : IHydratable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="ServerInfo"/> class.</summary>
        public ServerInfo()
            : this(DateTime.Now, DateTime.Now)
        {
		Logger.LogDebug(">>> Entering ServerInfo()");
		Logger.LogDebug(">>> Entering ServerInfo(created={Created},lastactivity={Lastactivity})", created, lastactivity);
		Logger.LogDebug("<<< Exiting ServerInfo(created={Created},lastactivity={Lastactivity})", created, lastactivity);
        }

        /// <summary>Initializes a new instance of the <see cref="ServerInfo"/> class.</summary>
        /// <param name="created">The created date for the server.</param>
        /// <param name="lastactivity">The last activity date for the server.</param>
        public ServerInfo(DateTime created, DateTime lastactivity)
        {
            this.IISAppName = Globals.IISAppName;
            this.ServerName = Globals.ServerName;
			Logger.LogDebug("ServerInfo(created={Created},lastactivity={Lastactivity}): this.IISAppName → {Arg2}", created, lastactivity, this.IISAppName);
			Logger.LogDebug("ServerInfo(created={Created},lastactivity={Lastactivity}): this.ServerName → {Arg2}", created, lastactivity, this.ServerName);
            this.ServerGroup = string.Empty;
            this.CreatedDate = created;
            this.LastActivityDate = lastactivity;
			Logger.LogDebug("ServerInfo(created={Created},lastactivity={Lastactivity}): this.ServerGroup → {Arg2}", created, lastactivity, this.ServerGroup);
			Logger.LogDebug("ServerInfo(created={Created},lastactivity={Lastactivity}): this.CreatedDate → {Arg2}", created, lastactivity, this.CreatedDate);
			Logger.LogDebug("ServerInfo(created={Created},lastactivity={Lastactivity}): this.LastActivityDate → {Arg2}", created, lastactivity, this.LastActivityDate);
            this.Enabled = true;
			Logger.LogDebug("<<< Exiting ServerInfo(created={Created},lastactivity={Lastactivity})", created, lastactivity);
			Logger.LogDebug("ServerInfo(created={Created},lastactivity={Lastactivity}): this.Enabled → {Arg2}", created, lastactivity, this.Enabled);
        }

        public int ServerID { get; set; }

        public string IISAppName { get; set; }

        public string ServerGroup { get; set; }

        public string ServerName { get; set; }

        public string Url { get; set; }

        public bool Enabled { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastActivityDate { get; set; }

        public int PingFailureCount { get; set; }

        public string UniqueId { get; set; }

        /// <summary>Gets or sets the Key ID.</summary>
        /// <returns>An Integer.</returns>
        public int KeyID
        {
            get
            {
                return this.ServerID;
            }

            set
            {
                this.ServerID = value;
            }
        }

        /// <summary>Fills a ServerInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        public void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.ServerID = Null.SetNullInteger(dr["ServerID"]);
            this.IISAppName = Null.SetNullString(dr["IISAppName"]);
            this.ServerName = Null.SetNullString(dr["ServerName"]);
            this.Url = Null.SetNullString(dr["URL"]);
            this.Enabled = Null.SetNullBoolean(dr["Enabled"]);
            this.CreatedDate = Null.SetNullDateTime(dr["CreatedDate"]);
            this.LastActivityDate = Null.SetNullDateTime(dr["LastActivityDate"]);

            var schema = dr.GetSchemaTable();
            if (schema != null)
			Logger.LogDebug("Fill(dr={Dr}): schema → {Schema}", dr, schema);
            {
                if (schema.Select("ColumnName = 'PingFailureCount'").Length > 0)
                {
                    this.PingFailureCount = Null.SetNullInteger(dr["PingFailureCount"]);
                }

                if (schema.Select("ColumnName = 'ServerGroup'").Length > 0)
                {
                    this.ServerGroup = Null.SetNullString(dr["ServerGroup"]);
                }

                if (schema.Select("ColumnName = 'UniqueId'").Length > 0)
                {
                    this.UniqueId = Null.SetNullString(dr["UniqueId"]);
                }
            }
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }
    }
}
