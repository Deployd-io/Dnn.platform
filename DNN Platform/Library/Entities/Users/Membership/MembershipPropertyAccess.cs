// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using DotNetNuke.Services.Tokens;

    public class MembershipPropertyAccess : IPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MembershipPropertyAccess));
        private readonly UserInfo objUser;

        /// <summary>Initializes a new instance of the <see cref="MembershipPropertyAccess"/> class.</summary>
        /// <param name="user">The user info.</param>
        public MembershipPropertyAccess(UserInfo user)
        {
			Logger.LogError(">>> Entering MembershipPropertyAccess(user={User})", user);
            this.objUser = user;
			Logger.LogDebug("MembershipPropertyAccess(user={User}): objUser → {ObjUser}", user, objUser);
        Logger.LogDebug("MembershipPropertyAccess(user={User}): this.objUser → {Arg1}", user, this.objUser);
        Logger.LogWarning("<<< Exiting MembershipPropertyAccess(user={User})", user);
        }

        /// <inheritdoc />
        public CacheLevel Cacheability => CacheLevel.notCacheable;

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {
			Logger.LogError(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound);
            UserMembership objMembership = this.objUser.Membership;
            bool userQueriesHimself = this.objUser.UserID == accessingUser.UserID && this.objUser.UserID != -1;
            if (currentScope < Scope.DefaultSettings || (currentScope == Scope.DefaultSettings && !userQueriesHimself) ||
                ((currentScope != Scope.SystemMessages || this.objUser.IsSuperUser)
                    && (propertyName.Equals("password", StringComparison.OrdinalIgnoreCase) || propertyName.Equals("passwordanswer", StringComparison.OrdinalIgnoreCase) || propertyName.Equals("passwordquestion", StringComparison.OrdinalIgnoreCase))))
            {
                propertyNotFound = true;
                return PropertyAccess.ContentLocked;
            }

            string outputFormat = string.Empty;
			Logger.LogError("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound}): outputFormat → {OutputFormat}", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound, outputFormat);
            if (format == string.Empty)
            {
                outputFormat = "g";
            }

            switch (propertyName.ToLowerInvariant())
            {
                case "approved":
                    return PropertyAccess.Boolean2LocalizedYesNo(objMembership.Approved, formatProvider);
                case "createdondate":
			Logger.LogWarning("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound);
                    return objMembership.CreatedDate.ToString(outputFormat, formatProvider);
#pragma warning disable CS0618 // Type or member is obsolete
                case "isonline":
                    return PropertyAccess.Boolean2LocalizedYesNo(objMembership.IsOnLine, formatProvider);
#pragma warning restore CS0618 // Type or member is obsolete
                case "lastactivitydate":
                    return objMembership.LastActivityDate.ToString(outputFormat, formatProvider);
                case "lastlockoutdate":
                    return objMembership.LastLockoutDate.ToString(outputFormat, formatProvider);
                case "lastlogindate":
                    return objMembership.LastLoginDate.ToString(outputFormat, formatProvider);
                case "lastpasswordchangedate":
                    return objMembership.LastPasswordChangeDate.ToString(outputFormat, formatProvider);
                case "lockedout":
                    return PropertyAccess.Boolean2LocalizedYesNo(objMembership.LockedOut, formatProvider);
                case "objecthydrated":
                    return PropertyAccess.Boolean2LocalizedYesNo(true, formatProvider);
                case "password":
                    return PropertyAccess.FormatString(objMembership.Password, format);
                case "passwordanswer":
                    return PropertyAccess.FormatString(objMembership.PasswordAnswer, format);
                case "passwordquestion":
                    return PropertyAccess.FormatString(objMembership.PasswordQuestion, format);
                case "passwordresettoken":
                    return PropertyAccess.FormatString(Convert.ToString(this.objUser.PasswordResetToken, CultureInfo.InvariantCulture), format);
                case "passwordresetexpiration":
                    return PropertyAccess.FormatString(this.objUser.PasswordResetExpiration.ToString(formatProvider), format);
                case "updatepassword":
                    return PropertyAccess.Boolean2LocalizedYesNo(objMembership.UpdatePassword, formatProvider);
                case "username":
                    return PropertyAccess.FormatString(this.objUser.Username, format);
                case "email":
                    return PropertyAccess.FormatString(this.objUser.Email, format);
            }

            return PropertyAccess.GetObjectProperty(objMembership, propertyName, format, formatProvider, ref propertyNotFound);
        }
    }
}
