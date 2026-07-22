// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Roles
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Web;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Tokens;
    using Newtonsoft.Json;

    /// <summary>The RoleInfo class provides the Entity Layer Role object.</summary>
    [Serializable]
    public class RoleInfo : BaseEntityInfo, IHydratable, IXmlSerializable, IPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private RoleType roleType = RoleType.None;
        private bool roleTypeSet = Null.NullBoolean;
        private Dictionary<string, string> settings;

        /// <summary>Initializes a new instance of the <see cref="RoleInfo"/> class.</summary>
        public RoleInfo()
        {
			Logger.LogDebug(">>> Entering RoleInfo()");
            this.TrialFrequency = "N";
            this.BillingFrequency = "N";
			Logger.LogDebug("RoleInfo(): this.TrialFrequency → {Arg0}", this.TrialFrequency);
			Logger.LogDebug("RoleInfo(): this.BillingFrequency → {Arg0}", this.BillingFrequency);
            this.RoleID = Null.NullInteger;
            this.IsSystemRole = false;
			Logger.LogDebug("<<< Exiting RoleInfo()");
			Logger.LogDebug("RoleInfo(): this.RoleID → {Arg0}", this.RoleID);
			Logger.LogDebug("RoleInfo(): this.IsSystemRole → {Arg0}", this.IsSystemRole);
        }

        /// <summary>Gets the Role Type.</summary>
        /// <value>A enum representing the type of the role.</value>
        public RoleType RoleType
        {
            get
            {
                if (!this.roleTypeSet)
                {
                    this.GetRoleType();
                    this.roleTypeSet = true;
                }

                return this.roleType;
            }
        }

        /// <summary>Gets the role settings.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public Dictionary<string, string> Settings
        {
            get
            {
                return this.settings ?? (this.settings = (this.RoleID == Null.NullInteger)
                                                     ? new Dictionary<string, string>()
                                                     : RoleController.Instance.GetRoleSettings(this.RoleID) as
                                                       Dictionary<string, string>);
            }
        }

        public string PhotoURL
        {
            get
            {
                string photoURL = Globals.ApplicationPath + "/images/sample-group-profile.jpg";

                if (this.IconFile != null)
                {
                    if (!string.IsNullOrEmpty(this.IconFile))
                    {
                        IFileInfo fileInfo =
                            FileManager.Instance.GetFile(int.Parse(this.IconFile.Replace("FileID=", string.Empty), CultureInfo.InvariantCulture));
                        if (fileInfo != null)
                        {
                            photoURL = FileManager.Instance.GetUrl(fileInfo);
                        }
                    }
                }

                return photoURL;
            }
        }

        /// <inheritdoc />
        public CacheLevel Cacheability
        {
            get { return CacheLevel.fullyCacheable; }
        }

        /// <summary>Gets or sets a value indicating whether this role is a system role.</summary>
        /// <value>A boolean representing whether this is a system role such as Administrators, Registered Users etc.</value>
        public bool IsSystemRole { get; set; }

        /// <summary>Gets or sets a value indicating whether users are automatically assigned to the role.</summary>
        public bool AutoAssignment { get; set; }

        /// <summary>Gets or sets the Billing Frequency for the role.</summary>
        /// <value>A String representing the Billing Frequency of the Role.
        /// <list>
        ///     <item>N - None</item>
        ///     <item>O - One time fee</item>
        ///     <item>D - Daily</item>
        ///     <item>W - Weekly</item>
        ///     <item>M - Monthly</item>
        ///     <item>Y - Yearly</item>
        /// </list>
        /// </value>
        public string BillingFrequency { get; set; }

        /// <summary>Gets or sets the length of the billing period.</summary>
        public int BillingPeriod { get; set; }

        /// <summary>Gets or sets the Description of the Role.</summary>
        public string Description { get; set; }

        /// <summary>Gets or sets the Icon File for the role.</summary>
        public string IconFile { get; set; }

        /// <summary>Gets or sets a value indicating whether the role is public.</summary>
        public bool IsPublic { get; set; }

        /// <summary>Gets or sets the Portal Id for the Role.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public int PortalID { get; set; }

        /// <summary>Gets or sets the Role Id.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public int RoleID { get; set; }

        /// <summary>Gets or sets the RoleGroup Id.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public int RoleGroupID { get; set; }

        /// <summary>Gets or sets the Role Name.</summary>
        public string RoleName { get; set; }

        /// <summary>Gets or sets the RSVP Code for the role.</summary>
        public string RSVPCode { get; set; }

        /// <summary>Gets or sets whether the role is a security role and can be used in Permission Grids etc.</summary>
        public SecurityMode SecurityMode { get; set; }

        /// <summary>Gets or sets the fee for the role.</summary>
        public float ServiceFee { get; set; }

        /// <summary>Gets or sets the status for the role.</summary>
        /// <value>An enumerated value Pending, Disabled, Approved.</value>
        public RoleStatus Status { get; set; }

        /// <summary>Gets or sets the trial fee for the role.</summary>
        public float TrialFee { get; set; }

        /// <summary>Gets or sets the Trial Frequency for the role.</summary>
        /// <value>A String representing the Trial Frequency of the Role.
        /// <list type="bullet">
        ///     <item>N - None</item>
        ///     <item>O - One time fee</item>
        ///     <item>D - Daily</item>
        ///     <item>W - Weekly</item>
        ///     <item>M - Monthly</item>
        ///     <item>Y - Yearly</item>
        /// </list>
        /// </value>
        public string TrialFrequency { get; set; }

        /// <summary>Gets or sets the length of the trial period.</summary>
        public int TrialPeriod { get; set; }

        /// <summary>Gets the number of users in the role.</summary>
        public int UserCount { get; private set; }

        /// <inheritdoc />
        public virtual int KeyID
        {
            get { return this.RoleID; }
            set { this.RoleID = value; }
        }

        /// <summary>Fills a RoleInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        public virtual void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.RoleID = Null.SetNullInteger(dr["RoleId"]);
            this.PortalID = Null.SetNullInteger(dr["PortalID"]);
            this.RoleGroupID = Null.SetNullInteger(dr["RoleGroupId"]);
            this.RoleName = Null.SetNullString(dr["RoleName"]);
            this.Description = Null.SetNullString(dr["Description"]);
            this.ServiceFee = Null.SetNullSingle(dr["ServiceFee"]);
            this.BillingPeriod = Null.SetNullInteger(dr["BillingPeriod"]);
            this.BillingFrequency = Null.SetNullString(dr["BillingFrequency"]);
            this.TrialFee = Null.SetNullSingle(dr["TrialFee"]);
            this.TrialPeriod = Null.SetNullInteger(dr["TrialPeriod"]);
            this.TrialFrequency = Null.SetNullString(dr["TrialFrequency"]);
            this.IsPublic = Null.SetNullBoolean(dr["IsPublic"]);
            this.AutoAssignment = Null.SetNullBoolean(dr["AutoAssignment"]);
            this.RSVPCode = Null.SetNullString(dr["RSVPCode"]);
            this.IconFile = Null.SetNullString(dr["IconFile"]);

            // New properties may not be present if called before 6.2 Upgrade has been executed
            try
            {
                int mode = Null.SetNullInteger(dr["SecurityMode"]);
                switch (mode)
				Logger.LogDebug("Fill(dr={Dr}): mode → {Mode}", dr, mode);
                {
                    case 0:
                        this.SecurityMode = SecurityMode.SecurityRole;
						Logger.LogDebug("Fill(dr={Dr}): this.SecurityMode → {Arg1}", dr, this.SecurityMode);
                        break;
                    case 1:
                        this.SecurityMode = SecurityMode.SocialGroup;
                        break;
                    default:
                        this.SecurityMode = SecurityMode.Both;
                        break;
                }

                int status = Null.SetNullInteger(dr["Status"]);
                switch (status)
				Logger.LogDebug("Fill(dr={Dr}): status → {Status}", dr, status);
                {
                    case -1:
                        this.Status = RoleStatus.Pending;
						Logger.LogDebug("Fill(dr={Dr}): this.Status → {Arg1}", dr, this.Status);
                        break;
                    case 0:
                        this.Status = RoleStatus.Disabled;
                        break;
                    default:
                        this.Status = RoleStatus.Approved;
                        break;
                }

                // check for values only relevant to UserRoles
                var schema = dr.GetSchemaTable();
                if (schema != null)
				Logger.LogDebug("Fill(dr={Dr}): schema → {Schema}", dr, schema);
                {
                    if (schema.Select("ColumnName = 'UserCount'").Length > 0)
                    {
                        this.UserCount = Null.SetNullInteger(dr["UserCount"]);
                    }

                    if (schema.Select("ColumnName = 'IsSystemRole'").Length > 0)
                    {
                        this.IsSystemRole = Null.SetNullBoolean(dr["IsSystemRole"]);
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
				Logger.LogError("Exception in Fill(dr={Dr}): {Arg1}", dr, ex.Message);
                // do nothing
            }

            // Fill base class fields
            this.FillInternal(dr);
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
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

            string propName = propertyName.ToLowerInvariant();
            switch (propName)
            {
                case "roleid":
                    return PropertyAccess.FormatString(this.RoleID.ToString(formatProvider), format);
                case "groupid":
                    return PropertyAccess.FormatString(this.RoleID.ToString(formatProvider), format);
                case "status":
                    return PropertyAccess.FormatString(this.Status.ToString(), format);
                case "groupname":
                    return PropertyAccess.FormatString(this.RoleName, format);
                case "rolename":
                    return PropertyAccess.FormatString(this.RoleName, format);
                case "groupdescription":
                    return PropertyAccess.FormatString(this.Description, format);
                case "description":
                    return PropertyAccess.FormatString(this.Description, format);
                case "usercount":
                    return PropertyAccess.FormatString(this.UserCount.ToString(formatProvider), format);
                case "street":
                    return PropertyAccess.FormatString(this.GetString("Street", string.Empty), format);
                case "city":
                    return PropertyAccess.FormatString(this.GetString("City", string.Empty), format);
                case "region":
                    return PropertyAccess.FormatString(this.GetString("Region", string.Empty), format);
                case "country":
                    return PropertyAccess.FormatString(this.GetString("Country", string.Empty), format);
                case "postalcode":
                    return PropertyAccess.FormatString(this.GetString("PostalCode", string.Empty), format);
                case "website":
                    return PropertyAccess.FormatString(this.GetString("Website", string.Empty), format);
                case "datecreated":
                    return PropertyAccess.FormatString(this.CreatedOnDate.ToString(formatProvider), format);
                case "photourl":
                    return PropertyAccess.FormatString(FormatUrl(this.PhotoURL), format);
                case "stat_status":
                    return PropertyAccess.FormatString(this.GetString("stat_status", string.Empty), format);
                case "stat_photo":
                    return PropertyAccess.FormatString(this.GetString("stat_photo", string.Empty), format);
                case "stat_file":
                    return PropertyAccess.FormatString(this.GetString("stat_file", string.Empty), format);
                case "url":
                    return PropertyAccess.FormatString(FormatUrl(this.GetString("URL", string.Empty)), format);
                case "issystemrole":
                    return PropertyAccess.Boolean2LocalizedYesNo(this.IsSystemRole, formatProvider);
                case "grouptype":
                    return this.IsPublic ? "Public.Text" : "Private.Text";
                case "groupcreatorname":
                    return PropertyAccess.FormatString(this.GetString("GroupCreatorName", string.Empty), format);
                default:
                    if (this.Settings.ContainsKey(propertyName))
                    {
                        return PropertyAccess.FormatString(this.GetString(propertyName, string.Empty), format);
                    }

                    propertyNotFound = true;
					Logger.LogDebug("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
                    return string.Empty;
            }
        }

        /// <inheritdoc />
        public XmlSchema GetSchema()
        {
			Logger.LogDebug(">>> Entering GetSchema()");
			Logger.LogDebug("<<< Exiting GetSchema()");
            return null;
        }

        /// <summary>Reads a RoleInfo from an XmlReader.</summary>
        /// <inheritdoc />
        public void ReadXml(XmlReader reader)
        {
			Logger.LogDebug(">>> Entering ReadXml(reader={Reader})", reader);
            // Set status to approved by default
            this.Status = RoleStatus.Approved;
			Logger.LogDebug("ReadXml(reader={Reader}): this.Status → {Arg1}", reader, this.Status);

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name.ToLowerInvariant())
                    {
                        case "role":
                            break;
                        case "rolename":
                            this.RoleName = reader.ReadElementContentAsString();
                            break;
                        case "description":
                            this.Description = reader.ReadElementContentAsString();
                            break;
                        case "billingfrequency":
                            this.BillingFrequency = reader.ReadElementContentAsString();
                            if (string.IsNullOrEmpty(this.BillingFrequency))
                            {
                                this.BillingFrequency = "N";
								Logger.LogDebug("ReadXml(reader={Reader}): this.BillingFrequency → {Arg1}", reader, this.BillingFrequency);
                            }

                            break;
                        case "billingperiod":
                            this.BillingPeriod = reader.ReadElementContentAsInt();
                            break;
                        case "servicefee":
                            this.ServiceFee = reader.ReadElementContentAsFloat();
                            if (this.ServiceFee < 0)
                            {
                                this.ServiceFee = 0;
								Logger.LogDebug("ReadXml(reader={Reader}): this.ServiceFee → {Arg1}", reader, this.ServiceFee);
                            }

                            break;
                        case "trialfrequency":
                            this.TrialFrequency = reader.ReadElementContentAsString();
                            if (string.IsNullOrEmpty(this.TrialFrequency))
                            {
                                this.TrialFrequency = "N";
								Logger.LogDebug("ReadXml(reader={Reader}): this.TrialFrequency → {Arg1}", reader, this.TrialFrequency);
                            }

                            break;
                        case "trialperiod":
                            this.TrialPeriod = reader.ReadElementContentAsInt();
                            break;
                        case "trialfee":
                            this.TrialFee = reader.ReadElementContentAsFloat();
                            if (this.TrialFee < 0)
                            {
                                this.TrialFee = 0;
								Logger.LogDebug("ReadXml(reader={Reader}): this.TrialFee → {Arg1}", reader, this.TrialFee);
                            }

                            break;
                        case "ispublic":
                            this.IsPublic = reader.ReadElementContentAsBoolean();
                            break;
                        case "autoassignment":
                            this.AutoAssignment = reader.ReadElementContentAsBoolean();
                            break;
                        case "rsvpcode":
                            this.RSVPCode = reader.ReadElementContentAsString();
                            break;
                        case "iconfile":
                            this.IconFile = reader.ReadElementContentAsString();
                            break;
                        case "issystemrole":
                            this.IsSystemRole = reader.ReadElementContentAsBoolean();
                            break;
                        case "roletype":
                            switch (reader.ReadElementContentAsString())
                            {
                                case "adminrole":
                                    this.roleType = RoleType.Administrator;
									Logger.LogDebug("ReadXml(reader={Reader}): roleType → {RoleType}", reader, roleType);
									Logger.LogDebug("ReadXml(reader={Reader}): this.roleType → {Arg1}", reader, this.roleType);
                                    break;
                                case "registeredrole":
                                    this.roleType = RoleType.RegisteredUser;
                                    break;
                                case "subscriberrole":
                                    this.roleType = RoleType.Subscriber;
                                    break;
                                case "unverifiedrole":
                                    this.roleType = RoleType.UnverifiedUser;
                                    break;
                                default:
                                    this.roleType = RoleType.None;
                                    break;
                            }

                            this.roleTypeSet = true;
							Logger.LogDebug("ReadXml(reader={Reader}): this.roleTypeSet → {Arg1}", reader, this.roleTypeSet);
                            break;
                        case "securitymode":
                            switch (reader.ReadElementContentAsString())
                            {
                                case "securityrole":
                                    this.SecurityMode = SecurityMode.SecurityRole;
									Logger.LogDebug("ReadXml(reader={Reader}): this.SecurityMode → {Arg1}", reader, this.SecurityMode);
                                    break;
                                case "socialgroup":
                                    this.SecurityMode = SecurityMode.SocialGroup;
                                    break;
                                case "both":
                                    this.SecurityMode = SecurityMode.Both;
                                    break;
                            }

                            break;
                        case "status":
                            switch (reader.ReadElementContentAsString())
                            {
                                case "pending":
                                    this.Status = RoleStatus.Pending;
                                    break;
                                case "disabled":
                                    this.Status = RoleStatus.Disabled;
                                    break;
                                default:
                                    this.Status = RoleStatus.Approved;
                                    break;
                            }

                            break;
                        default:
                            if (reader.NodeType == XmlNodeType.Element && !string.IsNullOrEmpty(reader.Name))
                            {
                                reader.ReadElementContentAsString();
                            }

                            break;
                    }
                }
            }
			Logger.LogDebug("<<< Exiting ReadXml(reader={Reader})", reader);
        }

        /// <summary>Writes a RoleInfo to an XmlWriter.</summary>
        /// <inheritdoc />
        public void WriteXml(XmlWriter writer)
        {
			Logger.LogDebug(">>> Entering WriteXml(writer={Writer})", writer);
            // Write start of main elements
            writer.WriteStartElement("role");

            // write out properties
            writer.WriteElementString("rolename", this.RoleName);
            writer.WriteElementString("description", this.Description);
            writer.WriteElementString("billingfrequency", this.BillingFrequency);
            writer.WriteElementString("billingperiod", this.BillingPeriod.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("servicefee", this.ServiceFee.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("trialfrequency", this.TrialFrequency);
            writer.WriteElementString("trialperiod", this.TrialPeriod.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("trialfee", this.TrialFee.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("ispublic", this.IsPublic.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
            writer.WriteElementString("autoassignment", this.AutoAssignment.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
            writer.WriteElementString("rsvpcode", this.RSVPCode);
            writer.WriteElementString("iconfile", this.IconFile);
            writer.WriteElementString("issystemrole", this.IsSystemRole.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
            switch (this.RoleType)
            {
                case RoleType.Administrator:
                    writer.WriteElementString("roletype", "adminrole");
                    break;
                case RoleType.RegisteredUser:
                    writer.WriteElementString("roletype", "registeredrole");
                    break;
                case RoleType.Subscriber:
                    writer.WriteElementString("roletype", "subscriberrole");
                    break;
                case RoleType.UnverifiedUser:
                    writer.WriteElementString("roletype", "unverifiedrole");
                    break;
                case RoleType.None:
                    writer.WriteElementString("roletype", "none");
                    break;
            }

            switch (this.SecurityMode)
            {
                case SecurityMode.SecurityRole:
                    writer.WriteElementString("securitymode", "securityrole");
                    break;
                case SecurityMode.SocialGroup:
                    writer.WriteElementString("securitymode", "socialgroup");
                    break;
                case SecurityMode.Both:
                    writer.WriteElementString("securitymode", "both");
                    break;
            }

            switch (this.Status)
            {
                case RoleStatus.Pending:
                    writer.WriteElementString("status", "pending");
                    break;
                case RoleStatus.Disabled:
                    writer.WriteElementString("status", "disabled");
                    break;
                case RoleStatus.Approved:
                    writer.WriteElementString("status", "approved");
                    break;
            }

            // Write end of main element
            writer.WriteEndElement();
			Logger.LogDebug("<<< Exiting WriteXml(writer={Writer})", writer);
        }

        private static string FormatUrl(string url)
        {
			Logger.LogDebug(">>> Entering FormatUrl(url={Url})", url);
            if (url.StartsWith("/", StringComparison.Ordinal) && HttpContext.Current != null)
            {
                // server absolute path
                return Globals.AddHTTP(HttpContext.Current.Request.Url.Host) + url;
            }

			Logger.LogDebug("<<< Exiting FormatUrl(url={Url})", url);
            return url;
        }

        private void GetRoleType()
        {
			Logger.LogDebug(">>> Entering GetRoleType()");
            var portal = PortalController.Instance.GetPortal(this.PortalID);
            if (this.RoleID == portal.AdministratorRoleId)
			Logger.LogDebug("GetRoleType(): portal → {Portal}", portal);
            {
                this.roleType = RoleType.Administrator;
				Logger.LogDebug("GetRoleType(): roleType → {RoleType}", roleType);
				Logger.LogDebug("GetRoleType(): this.roleType → {Arg0}", this.roleType);
            }
            else if (this.RoleID == portal.RegisteredRoleId)
            {
                this.roleType = RoleType.RegisteredUser;
            }
            else if (this.RoleName == "Subscribers")
            {
                this.roleType = RoleType.Subscriber;
            }
            else if (this.RoleName == "Unverified Users")
            {
                this.roleType = RoleType.UnverifiedUser;
            }
			Logger.LogDebug("<<< Exiting GetRoleType()");
        }

        private string GetString(string keyName, string defaultValue)
        {
			Logger.LogDebug(">>> Entering GetString(keyName={KeyName},defaultValue={DefaultValue})", keyName, defaultValue);
            if (this.Settings == null)
            {
                return defaultValue;
            }

            if (this.Settings.TryGetValue(keyName, out var settingValue))
            {
                return settingValue;
            }

			Logger.LogDebug("<<< Exiting GetString(keyName={KeyName},defaultValue={DefaultValue})", keyName, defaultValue);
            return defaultValue;
        }
    }
}
