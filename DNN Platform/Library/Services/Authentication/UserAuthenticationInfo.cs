// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;

    /// <summary>
    /// DNN-4016
    /// The UserAuthenticationInfo class provides the Entity Layer for the
    /// user information in the Authentication Systems.
    /// </summary>
    [Serializable]
    public class UserAuthenticationInfo : BaseEntityInfo, IHydratable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="UserAuthenticationInfo"/> class.</summary>
        public UserAuthenticationInfo()
        {
			Logger.LogDebug(">>> Entering UserAuthenticationInfo()");
            this.AuthenticationToken = Null.NullString;
            this.AuthenticationType = Null.NullString;
			Logger.LogDebug("UserAuthenticationInfo(): this.AuthenticationToken → {Arg0}", this.AuthenticationToken);
			Logger.LogDebug("UserAuthenticationInfo(): this.AuthenticationType → {Arg0}", this.AuthenticationType);
            this.UserAuthenticationID = Null.NullInteger;
			Logger.LogDebug("<<< Exiting UserAuthenticationInfo()");
			Logger.LogDebug("UserAuthenticationInfo(): this.UserAuthenticationID → {Arg0}", this.UserAuthenticationID);
        }

        /// <summary>Gets or sets the ID of the User Record in the Authentication System.</summary>
        public int UserAuthenticationID { get; set; }

        /// <summary>Gets or sets the PackageID for the Authentication System.</summary>
        public int UserID { get; set; }

        /// <summary>Gets or sets the type (name) of the Authentication System (eg DNN, OpenID, LiveID).</summary>
        public string AuthenticationType { get; set; }

        /// <summary>Gets or sets the url for the Logoff Control.</summary>
        public string AuthenticationToken { get; set; }

        /// <summary>Gets or sets the Key ID.</summary>
        /// <returns>An Integer.</returns>
        public virtual int KeyID
        {
            get
            {
                return this.UserAuthenticationID;
            }

            set
            {
                this.UserAuthenticationID = value;
            }
        }

        /// <summary>Fills a UserAuthenticationInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        public virtual void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.UserAuthenticationID = Null.SetNullInteger(dr["UserAuthenticationID"]);
            this.UserID = Null.SetNullInteger(dr["UserID"]);
            this.AuthenticationType = Null.SetNullString(dr["AuthenticationType"]);
            this.AuthenticationToken = Null.SetNullString(dr["AuthenticationToken"]);

            // Fill base class fields
            this.FillInternal(dr);
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }
    }
}
