// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Notifications
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;
    using Newtonsoft.Json;

    /// <summary>The Notification class describes the a notification received by a user as a consecuence of an action.</summary>
    [Serializable]
    public class Notification : BaseEntityInfo, IHydratable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private int notificationID = -1;
        private string displayDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="Notification"/> class.
        /// Default constructor.
        /// </summary>
        public Notification()
        {
			Logger.LogDebug(">>> Entering Notification()");
            this.SendToast = true;
			Logger.LogDebug("<<< Exiting Notification()");
			Logger.LogDebug("Notification(): this.SendToast → {Arg0}", this.SendToast);
        }

        /// <summary>Gets a pretty printed string with the time since the message was created.</summary>
        [XmlAttribute]
        public string DisplayDate
        {
            get
            {
                if (string.IsNullOrEmpty(this.displayDate))
                {
                    this.displayDate = DateUtils.CalculateDateForDisplay(this.CreatedOnDate);
                }

                return this.displayDate;
            }
        }

        /// <summary>Gets or sets notificationID - The primary key.</summary>
        [XmlAttribute]
        public int NotificationID
        {
            get
            {
                return this.notificationID;
            }

            set
            {
                this.notificationID = value;
            }
        }

        /// <summary>Gets or sets the message type identifier.</summary>
        [XmlAttribute]
        public int NotificationTypeID { get; set; }

        /// <summary>Gets or sets to list for the message. This information is saved for faster display of To list in the message.</summary>
        [XmlAttribute]
        public string To { get; set; }

        /// <summary>Gets or sets message From.</summary>
        [XmlAttribute]
        public string From { get; set; }

        /// <summary>Gets or sets message Subject.</summary>
        [XmlAttribute]
        public string Subject { get; set; }

        /// <summary>Gets or sets message body.</summary>
        [XmlAttribute]
        public string Body { get; set; }

        /// <summary>Gets or sets context set by creator of the notification.</summary>
        [XmlAttribute]
        public string Context { get; set; }

        /// <summary>Gets or sets the UserID of the sender of the message.</summary>
        [XmlAttribute]
        public int SenderUserID { get; set; }

        /// <summary>Gets or sets for notifications, this field indicates when it has to be removed (or not displayed).</summary>
        [XmlAttribute]
        public DateTime ExpirationDate { get; set; }

        /// <summary>Gets or sets iHydratable.KeyID.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public int KeyID
        {
            get
            {
                return this.NotificationID;
            }

            set
            {
                this.NotificationID = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether should this notification support a dismiss action.</summary>
        [XmlAttribute]
        public bool IncludeDismissAction { get; set; }

        /// <summary>Gets or sets a value indicating whether the notification is displayed on the UI as a toast notification.</summary>
        [XmlAttribute]
        public bool SendToast { get; set; }

        /// <summary>Fill the object with data from database.</summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.NotificationID = Convert.ToInt32(dr["MessageID"], CultureInfo.InvariantCulture);
            this.NotificationTypeID = Convert.ToInt32(dr["NotificationTypeID"], CultureInfo.InvariantCulture);
            this.To = Null.SetNullString(dr["To"]);
            this.From = Null.SetNullString(dr["From"]);
            this.Subject = Null.SetNullString(dr["Subject"]);
            this.Body = Null.SetNullString(dr["Body"]);
            this.Context = Null.SetNullString(dr["Context"]);
            this.SenderUserID = Convert.ToInt32(dr["SenderUserID"], CultureInfo.InvariantCulture);
            this.ExpirationDate = Null.SetNullDateTime(dr["ExpirationDate"]);
            this.IncludeDismissAction = Null.SetNullBoolean(dr["IncludeDismissAction"]);

            var schema = dr.GetSchemaTable();
            if (schema != null)
			Logger.LogDebug("Fill(dr={Dr}): schema → {Schema}", dr, schema);
            {
                if (schema.Select("ColumnName = 'SendToast'").Length > 0)
                {
                    this.SendToast = Null.SetNullBoolean(dr["SendToast"]);
                }
                else
                {
                    this.SendToast = false;
					Logger.LogDebug("Fill(dr={Dr}): this.SendToast → {Arg1}", dr, this.SendToast);
                }
            }

            // add audit column data
            this.FillInternal(dr);
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }
    }
}
