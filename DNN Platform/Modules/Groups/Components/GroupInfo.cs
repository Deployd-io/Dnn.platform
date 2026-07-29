// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Groups
{
    using System;
    using System.Collections;
    using System.Xml;
    using System.Xml.Serialization;

    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Tokens;

    public class GroupInfo : RoleInfo, IPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        ////private RoleInfo roleInfo;

        /// <summary>Initializes a new instance of the <see cref="GroupInfo"/> class.</summary>
        public GroupInfo()
        {
		Logger.LogDebug(">>> Entering GroupInfo()");
        }

        /// <summary>Initializes a new instance of the <see cref="GroupInfo"/> class.</summary>
        /// <param name="roleInfo">The role info.</param>
        public GroupInfo(RoleInfo roleInfo)
        {
			Logger.LogDebug(">>> Entering GroupInfo(roleInfo={RoleInfo})", roleInfo);
            this.RoleID = roleInfo.RoleID;
            this.RoleName = roleInfo.RoleName;
			Logger.LogDebug("GroupInfo(roleInfo={RoleInfo}): this.RoleID → {Arg1}", roleInfo, this.RoleID);
			Logger.LogDebug("GroupInfo(roleInfo={RoleInfo}): this.RoleName → {Arg1}", roleInfo, this.RoleName);
            this.Description = roleInfo.Description;
            this.PortalID = roleInfo.PortalID;
            this.SecurityMode = roleInfo.SecurityMode;
			Logger.LogDebug("GroupInfo(roleInfo={RoleInfo}): this.Description → {Arg1}", roleInfo, this.Description);
			Logger.LogDebug("GroupInfo(roleInfo={RoleInfo}): this.PortalID → {Arg1}", roleInfo, this.PortalID);
			Logger.LogDebug("GroupInfo(roleInfo={RoleInfo}): this.SecurityMode → {Arg1}", roleInfo, this.SecurityMode);
            this.ServiceFee = roleInfo.ServiceFee;
            this.RSVPCode = roleInfo.RSVPCode;
			Logger.LogDebug("<<< Exiting GroupInfo(roleInfo={RoleInfo})", roleInfo);
			Logger.LogDebug("GroupInfo(roleInfo={RoleInfo}): this.ServiceFee → {Arg1}", roleInfo, this.ServiceFee);
			Logger.LogDebug("GroupInfo(roleInfo={RoleInfo}): this.RSVPCode → {Arg1}", roleInfo, this.RSVPCode);
			Logger.LogDebug("<<< Exiting GroupInfo()");
        }

        ////public RoleInfo Role {
        ////   get {
        ////       return roleInfo;
        ////   }
        ////   set {
        ////       roleInfo = value;
        ////   }
        ////}
        ////public int GroupId {
        ////   get {
        ////       return RoleID;
        ////   }
        ////}

        ////public int ModuleId { get; set; }
        public string Street
        {
            get
            {
                return this.GetString("Street", string.Empty);
            }

            set
            {
                this.SetString("Street", value);
            }
        }

        public string City
        {
            get
            {
                return this.GetString("City", string.Empty);
            }

            set
            {
                this.SetString("City", value);
            }
        }

        public string Region
        {
            get
            {
                return this.GetString("Region", string.Empty);
            }

            set
            {
                this.SetString("Region", value);
            }
        }

        public string Country
        {
            get
            {
                return this.GetString("Country", string.Empty);
            }

            set
            {
                this.SetString("Country", value);
            }
        }

        public string PostalCode
        {
            get
            {
                return this.GetString("PostalCode", string.Empty);
            }

            set
            {
                this.SetString("PostalCode", value);
            }
        }

        public string Website
        {
            get
            {
                return this.GetString("Website", string.Empty);
            }

            set
            {
                this.SetString("Website", value);
            }
        }

        public bool Featured
        {
            get
            {
                return Convert.ToBoolean(this.GetString("Featured", "false"));
            }

            set
            {
                this.SetString("Featured", value.ToString());
            }
        }

        private string GetString(string keyName, string defaultValue)
        {
			Logger.LogDebug(">>> Entering GetString(keyName={KeyName},defaultValue={DefaultValue})", keyName, defaultValue);
            if (this.Settings == null)
            {
                return defaultValue;
            }

            if (this.Settings.ContainsKey(keyName))
            {
                return this.Settings[keyName];
            }
            else
            {
				Logger.LogDebug("<<< Exiting GetString(keyName={KeyName},defaultValue={DefaultValue})", keyName, defaultValue);
                return defaultValue;
            }
        }

        private void SetString(string keyName, string value)
        {
			Logger.LogDebug(">>> Entering SetString(keyName={KeyName},value={Value})", keyName, value);
            if (this.Settings.ContainsKey(keyName))
            {
                this.Settings[keyName] = value;
            }
            else
            {
                this.Settings.Add(keyName, value);
            }
			Logger.LogDebug("<<< Exiting SetString(keyName={KeyName},value={Value})", keyName, value);
        }
    }
}
