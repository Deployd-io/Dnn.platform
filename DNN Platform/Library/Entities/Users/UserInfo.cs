// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Abstractions.Users;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The UserInfo class provides Business Layer model for Users.</summary>
    [Serializable]
    public class UserInfo : BaseEntityInfo, IPropertyAccess, IUserInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserInfo));
        private readonly ConcurrentDictionary<int, UserSocial> social = new ConcurrentDictionary<int, UserSocial>();
        private readonly ConcurrentDictionary<int, string[]> roles = new ConcurrentDictionary<int, string[]>();
        private string administratorRoleName;
        private UserMembership membership;
        private UserProfile profile;

        /// <summary>Initializes a new instance of the <see cref="UserInfo"/> class.</summary>
        public UserInfo()
        {
			Logger.LogError(">>> Entering UserInfo()");
            this.IsDeleted = Null.NullBoolean;
			Logger.LogDebug("UserInfo(): this.IsDeleted → {Arg0}", this.IsDeleted);
            this.UserID = Null.NullInteger;
            this.PortalID = Null.NullInteger;
			Logger.LogDebug("UserInfo(): this.UserID → {Arg0}", this.UserID);
            this.IsSuperUser = Null.NullBoolean;
            this.AffiliateID = Null.NullInteger;
        }

        /// <summary>Gets a value indicating whether the user is in the portal's administrators role.</summary>
        public bool IsAdmin
        {
            get
            {
                if (this.IsSuperUser)
                {
                    return true;
                }

                PortalInfo ps = PortalController.Instance.GetPortal(this.PortalID);
                return ps != null && this.IsInRole(ps.AdministratorRoleName);
            }
        }

        /// <summary>Gets and sets the Social property.</summary>
        [Browsable(false)]
        public UserSocial Social
        {
            get
            {
                return this.social.GetOrAdd(this.PortalID, i => new UserSocial(this));
            }
        }

        /// <inheritdoc />
        [Browsable(false)]
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        /// <summary>Gets or sets the AffiliateId for this user.</summary>
        [Browsable(false)]
        public int AffiliateID { get; set; }

        /// <summary>Gets or sets the Display Name.</summary>
        [SortOrder(3)]
        [Required(true)]
        [MaxLength(128)]
        public string DisplayName { get; set; }

        /// <summary>Gets or sets the Email Address.</summary>
        [SortOrder(4)]
        [MaxLength(256)]
        [Required(true)]
        [RegularExpressionValidator(Globals.glbEmailRegEx)]
        public string Email { get; set; }

        /// <summary>Gets or sets the First Name.</summary>
        [SortOrder(1)]
        [MaxLength(50)]
        public string FirstName
        {
            get { return this.Profile.FirstName; }
            set { this.Profile.FirstName = value; }
        }

        /// <summary>Gets or sets a value indicating whether the User is deleted.</summary>
        [Browsable(false)]
        public bool IsDeleted { get; set; }

        /// <summary>Gets or sets a value indicating whether the User is a SuperUser.</summary>
        [Browsable(false)]
        public bool IsSuperUser { get; set; }

        /// <summary>Gets or sets the Last IP address used by user.</summary>
        [Browsable(false)]
        public string LastIPAddress { get; set; }

        /// <summary>Gets or sets the Last Name.</summary>
        [SortOrder(2)]
        [MaxLength(50)]
        public string LastName
        {
            get { return this.Profile.LastName; }
            set { this.Profile.LastName = value; }
        }

        /// <summary>Gets or sets the Membership object.</summary>
        [Browsable(false)]
        public UserMembership Membership
        {
            get
            {
                if (this.membership == null)
                {
                    this.membership = new UserMembership(this);
                    if ((this.Username != null) && (!string.IsNullOrEmpty(this.Username)))
                    {
                        UserController.GetUserMembership(this);
                    }
                }

                return this.membership;
            }

            set
            {
                this.membership = value;
            }
        }

        /// <summary>Gets or sets the token created for resetting passwords.</summary>
        [Browsable(false)]
        public Guid PasswordResetToken { get; set; }

        /// <summary>Gets or sets the datetime that the PasswordResetToken is valid.</summary>
        [Browsable(false)]
        public DateTime PasswordResetExpiration { get; set; }

        /// <summary>Gets or sets the PortalId.</summary>
        [Browsable(false)]
        public int PortalID { get; set; }

        /// <summary>Gets or sets a value indicating whether the user has agreed to the terms and conditions.</summary>
        [Browsable(false)]
        public bool HasAgreedToTerms { get; set; }

        /// <summary>Gets or sets when the user last agreed to the terms and conditions.</summary>
        [Browsable(false)]
        public DateTime HasAgreedToTermsOn { get; set; }

        /// <summary>Gets or sets a value indicating whether the user has requested they be removed from the site.</summary>
        [Browsable(false)]
        public bool RequestsRemoval { get; set; }

        /// <summary>Gets or sets the Profile Object.</summary>
        [Browsable(false)]
        public UserProfile Profile
        {
            get
            {
                if (this.profile == null)
                {
                    this.profile = new UserProfile(this);
                    UserInfo userInfo = this;
                    ProfileController.GetUserProfile(ref userInfo);
                }

                return this.profile;
            }

            set
            {
                this.profile = value;
            }
        }

        /// <inheritdoc />
        [Browsable(false)]
        public string[] Roles
        {
            get
            {
                return this.roles.GetOrAdd(this.PortalID, i =>
                {
                    var socialRoles = this.Social.Roles;
                    if (socialRoles.Count == 0)
                    {
                        return [];
                    }
                    else
                    {
                        return (from r in this.Social.Roles
                                      where
                                          r.Status == RoleStatus.Approved &&
                                          (r.EffectiveDate < DateTime.Now || Null.IsNull(r.EffectiveDate)) &&
                                          (r.ExpiryDate > DateTime.Now || Null.IsNull(r.ExpiryDate))
                                      select r.RoleName)
                            .ToArray();
                    }
                });
            }

            set
            {
            }
        }

        /// <summary>Gets or sets the User Id.</summary>
        [Browsable(false)]
        public int UserID { get; set; }

        /// <summary>Gets or sets the User Name.</summary>
        [SortOrder(0)]
        [MaxLength(100)]
        [IsReadOnly(true)]
        [Required(true)]
        public string Username { get; set; }

        /// <inheritdoc />
        public string VanityUrl { get; set; }

        /// <summary>Property access, initially provided for TokenReplace.</summary>
        /// <param name="propertyName">Name of the Property.</param>
        /// <param name="format">format string.</param>
        /// <param name="formatProvider">format provider for numbers, dates, currencies.</param>
        /// <param name="accessingUser">userinfo of the user, who queries the data (used to determine permissions).</param>
        /// <param name="currentScope">requested maximum access level, might be restricted due to user level.</param>
        /// <param name="propertyNotFound">out: flag, if property could be retrieved.</param>
        /// <returns>current value of the property for this userinfo object.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {
			Logger.LogError(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound);
            Scope internScope;
            if (this.UserID == -1 && currentScope > Scope.Configuration)
            {
                internScope = Scope.Configuration; // anonymous users only get access to display name
				Logger.LogError("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound}): internScope → {InternScope}", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound, internScope);
            }
            else if (this.UserID != accessingUser.UserID && !this.IsAdminUser(ref accessingUser) && currentScope > Scope.DefaultSettings)
            {
                internScope = Scope.DefaultSettings; // registered users can access username and userID as well
            }
            else
            {
                internScope = currentScope; // admins and user himself can access all data
            }

            string outputFormat = format == string.Empty ? "g" : format;
            switch (propertyName.ToLowerInvariant())
            {
                case "verificationcode":
                    if (internScope < Scope.SystemMessages)
                    {
                        propertyNotFound = true;
						Logger.LogError("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound}): propertyNotFound → {PropertyNotFound1}", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound, propertyNotFound);
                        return PropertyAccess.ContentLocked;
                    }

                    var cryptographyProvider = Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
					Logger.LogError("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound}): cryptographyProvider → {CryptographyProvider}", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound, cryptographyProvider);
                    var code = cryptographyProvider.EncryptParameter($"{this.PortalID}-{this.GetMembershipUserId()}", Config.GetDecryptionkey()).EncryptedMessage;
					Logger.LogError("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound}): code → {Code}", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound, code);
                    return code.Replace("+", ".").Replace("/", "-").Replace("=", "_");
                case "affiliateid":
                    if (internScope < Scope.SystemMessages)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return this.AffiliateID.ToString(outputFormat, formatProvider);
                case "displayname":
                    if (internScope < Scope.Configuration)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return PropertyAccess.FormatString(this.DisplayName, format);
                case "email":
                    if (internScope < Scope.DefaultSettings)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return PropertyAccess.FormatString(this.Email, format);
                case "firstname": // using profile property is recommended!
                    if (internScope < Scope.DefaultSettings)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return PropertyAccess.FormatString(this.FirstName, format);
                case "issuperuser":
                    if (internScope < Scope.Debug)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return this.IsSuperUser.ToString(formatProvider);
                case "lastname": // using profile property is recommended!
                    if (internScope < Scope.DefaultSettings)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return PropertyAccess.FormatString(this.LastName, format);
                case "portalid":
                    if (internScope < Scope.Configuration)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return this.PortalID.ToString(outputFormat, formatProvider);
                case "userid":
                    if (internScope < Scope.DefaultSettings)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return this.UserID.ToString(outputFormat, formatProvider);
                case "username":
                    if (internScope < Scope.DefaultSettings)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    var settings = PortalController.Instance.GetPortalSettings(this.PortalID);
					Logger.LogError("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound}): settings → {Settings}", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound, settings);
                    var regSettings = new RegistrationSettings(settings);
					Logger.LogError("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound}): regSettings → {RegSettings}", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound, regSettings);

                    return regSettings.UseEmailAsUserName ?
                        PropertyAccess.FormatString(this.Email, format) :
                        PropertyAccess.FormatString(this.Username, format);
                case "fullname": // fullname is obsolete, it will return DisplayName
                    if (internScope < Scope.Configuration)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return PropertyAccess.FormatString(this.DisplayName, format);
                case "roles":
                    if (currentScope < Scope.SystemMessages)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return PropertyAccess.FormatString(string.Join(", ", this.Roles), format);
            }

            propertyNotFound = true;
			Logger.LogWarning("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound);
            return string.Empty;
        }

        /// <summary>IsInRole determines whether the user is in the role passed.</summary>
        /// <param name="role">The role to check.</param>
        /// <returns>A Boolean indicating success or failure.</returns>
        public bool IsInRole(string role)
        {
			Logger.LogError(">>> Entering IsInRole(role={Role})", role);
            // super users should always be verified.
            if (this.IsSuperUser)
            {
                return role != "Unverified Users";
            }

            if (role == Globals.glbRoleAllUsersName)
            {
                return true;
            }

            if (this.UserID == Null.NullInteger && role == Globals.glbRoleUnauthUserName)
            {
                return true;
            }

            if ("[" + this.UserID + "]" == role)
            {
                return true;
            }

            var roles = this.Roles;
            if (roles != null)
            {
                return roles.Any(s => s == role);
            }

			Logger.LogWarning("<<< Exiting IsInRole(role={Role})", role);
            return false;
        }

        /// <summary>Gets current time in User's timezone.</summary>
        /// <returns>The time in the user's (or portal's) preferred time zone.</returns>
        public DateTime LocalTime()
        {
			Logger.LogError(">>> Entering LocalTime()");
			Logger.LogWarning("<<< Exiting LocalTime()");
			Logger.LogWarning("<<< Exiting LocalTime(utcTime={UtcTime})", utcTime);
            return this.LocalTime(DateUtils.GetDatabaseUtcTime());
        }

        /// <summary>Convert utc time in User's timezone.</summary>
        /// <param name="utcTime">Utc time to convert.</param>
        /// <returns>The time in the user's (or portal's) preferred time zone.</returns>
        public DateTime LocalTime(DateTime utcTime)
        {
            if (this.UserID > Null.NullInteger)
            {
                return TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, this.Profile.PreferredTimeZone);
            }

            return TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, PortalController.Instance.GetCurrentSettings().TimeZone);
        }

        /// <summary>UpdateDisplayName updates the displayname to the format provided.</summary>
        /// <param name="format">The format to use.</param>
        public void UpdateDisplayName(string format)
        {
			Logger.LogError(">>> Entering UpdateDisplayName(format={Format})", format);
			Logger.LogError("UpdateDisplayName(format={Format}): format → {Format1}", format, format);
            // Replace Tokens
            format = format.Replace("[USERID]", this.UserID.ToString(CultureInfo.InvariantCulture));
            format = format.Replace("[FIRSTNAME]", this.FirstName);
            format = format.Replace("[LASTNAME]", this.LastName);
            format = format.Replace("[USERNAME]", this.Username);
            this.DisplayName = format;
        }

        /// <summary>Determine, if accessing user is Administrator.</summary>
        /// <param name="accessingUser">userinfo of the user to query.</param>
        /// <returns>true, if user is portal administrator or superuser.</returns>
        private bool IsAdminUser(ref UserInfo accessingUser)
        {
			Logger.LogError(">>> Entering IsAdminUser(accessingUser={AccessingUser})", accessingUser);
            if (accessingUser == null || accessingUser.UserID == -1)
            {
                return false;
            }

            if (string.IsNullOrEmpty(this.administratorRoleName))
            {
                PortalInfo ps = PortalController.Instance.GetPortal(accessingUser.PortalID);
				Logger.LogError("IsAdminUser(accessingUser={AccessingUser}): ps → {Ps}", accessingUser, ps);
                this.administratorRoleName = ps.AdministratorRoleName;
				Logger.LogDebug("IsAdminUser(accessingUser={AccessingUser}): this.administratorRoleName → {Arg1}", accessingUser, this.administratorRoleName);
            }

			Logger.LogWarning("<<< Exiting IsAdminUser(accessingUser={AccessingUser})", accessingUser);
            return accessingUser.IsInRole(this.administratorRoleName) || accessingUser.IsSuperUser;
        }

        private string GetMembershipUserId()
        {
			Logger.LogError(">>> Entering GetMembershipUserId()");
            Logger.LogWarning("<<< Exiting GetMembershipUserId()");
            return MembershipProvider.Instance().GetProviderUserKey(this)?.Replace("-", string.Empty) ?? string.Empty;
        }
    }
}
