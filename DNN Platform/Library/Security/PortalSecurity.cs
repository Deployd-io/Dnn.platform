// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Security;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Cookies;
    using DotNetNuke.Services.Cryptography;

    using Microsoft.Extensions.DependencyInjection;

    using ICryptographyProvider = DotNetNuke.Abstractions.Security.ICryptographyProvider;

    /// <summary>A variety of security-related utility functions.</summary>
    public partial class PortalSecurity
    {
        /// <summary>A <see cref="PortalSecurity"/> instance.</summary>
        public static readonly PortalSecurity Instance = ActivatorUtilities.GetServiceOrCreateInstance<PortalSecurity>(Globals.DependencyProvider);

        private const string EncryptedStringAlgorithmSeparator = "___|^Algorithm^|___";
        private const string EncryptedStringInitializationVectorSeparator = "___|^IV^|___";

        private const string RoleFriendPrefix = "FRIEND:";
        private const string RoleFollowerPrefix = "FOLLOWER:";
        private const string RoleOwnerPrefix = "OWNER:";

        private const string BadStatementExpression = ";|--|\bcreate\b|\bdrop\b|\bselect\b|\binsert\b|\bdelete\b|\bupdate\b|\bunion\b|sp_|xp_|\bexec\b|\bexecute\b|/\\*.*\\*/|\bdeclare\b|\bwaitfor\b|%|&";

        private const RegexOptions RxOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;

        private static readonly DateTime OldExpiryTime = new DateTime(1999, 1, 1);

        private static readonly Regex StripTagsRegex = new Regex("<[^<>]*>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex BadStatementRegex = new Regex(BadStatementExpression, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ControlCharactersRegex = new Regex(@"\p{C}+", RegexOptions.Compiled);
        private static readonly Regex ControlCharacterToWhitespaceRegex = new Regex(@"\r\n|\r|\n|\t", RegexOptions.Compiled);

        private static readonly Regex[] RxListStrings = new[]
        {
            new Regex("<script[^>]*>.*?</script[^><]*>", RxOptions),
            new Regex("<script", RxOptions),
            new Regex("<input[^>]*>.*?</input[^><]*>", RxOptions),
            new Regex("<object[^>]*>.*?</object[^><]*>", RxOptions),
            new Regex("<embed[^>]*>.*?</embed[^><]*>", RxOptions),
            new Regex("<applet[^>]*>.*?</applet[^><]*>", RxOptions),
            new Regex("<form[^>]*>.*?</form[^><]*>", RxOptions),
            new Regex("<option[^>]*>.*?</option[^><]*>", RxOptions),
            new Regex("<select[^>]*>.*?</select[^><]*>", RxOptions),
            new Regex("<source[^>]*>.*?</source[^><]*>", RxOptions),
            new Regex("<iframe[^>]*>.*?</iframe[^><]*>", RxOptions),
            new Regex("<iframe.*?<", RxOptions),
            new Regex("<iframe.*?", RxOptions),
            new Regex("<ilayer[^>]*>.*?</ilayer[^><]*>", RxOptions),
            new Regex("<form[^>]*>", RxOptions),
            new Regex("</form[^><]*>", RxOptions),
            new Regex("\bonerror\b", RxOptions),
            new Regex("\bonload\b", RxOptions),
            new Regex("\bonfocus\b", RxOptions),
            new Regex("\bonblur\b", RxOptions),
            new Regex("\bonclick\b", RxOptions),
            new Regex("\bondblclick\b", RxOptions),
            new Regex("\bonchange\b", RxOptions),
            new Regex("\bonselect\b", RxOptions),
            new Regex("\bonsubmit\b", RxOptions),
            new Regex("\bonreset\b", RxOptions),
            new Regex("\bonkeydown\b", RxOptions),
            new Regex("\bonkeyup\b", RxOptions),
            new Regex("\bonkeypress\b", RxOptions),
            new Regex("\bonmousedown\b", RxOptions),
            new Regex("\bonmousemove\b", RxOptions),
            new Regex("\bonmouseout\b", RxOptions),
            new Regex("\bonmouseover\b", RxOptions),
            new Regex("\bonmouseup\b", RxOptions),
            new Regex("\bonreadystatechange\b", RxOptions),
            new Regex("\bonfinish\b", RxOptions),
            new Regex("javascript:", RxOptions),
            new Regex("vbscript:", RxOptions),
            new Regex("unescape", RxOptions),
            new Regex(@"alert[\s(&nbsp;)]*\([\s(&nbsp;)]*'?[\s(&nbsp;)]*[""(&quot;)]?", RxOptions),
            new Regex(@"eval*.\(", RxOptions),
        };

        private static readonly Regex DangerElementsRegex = new Regex(@"(<[^>]*?) on.*?\=(['""]*)[\s\S]*?(\2)( *)([^>]*?>)", RxOptions);
        private static readonly Regex DangerElementContentRegex = new Regex(@"on.*?\=(['""]*)[\s\S]*?(\1)( *)", RxOptions);

        private readonly ICryptographyProvider cryptographyProvider;
        private readonly ListController listController;
        private readonly IPortalController portalController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IPortalGroupController portalGroupController;

        /// <summary>Initializes a new instance of the <see cref="PortalSecurity"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public PortalSecurity()
            : this(null, null, null, null, null)
        {
		Logger.LogDebug(">>> Entering PortalSecurity()");
        }

		Logger.LogDebug("<<< Exiting PortalSecurity()");
        /// <summary>Initializes a new instance of the <see cref="PortalSecurity"/> class.</summary>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public PortalSecurity(ICryptographyProvider cryptographyProvider)
            : this(cryptographyProvider, null, null, null, null)
        {
		Logger.LogDebug(">>> Entering PortalSecurity(cryptographyProvider={CryptographyProvider})", cryptographyProvider);
		Logger.LogDebug("<<< Exiting PortalSecurity(cryptographyProvider={CryptographyProvider})", cryptographyProvider);
        }

        /// <summary>Initializes a new instance of the <see cref="PortalSecurity"/> class.</summary>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        /// <param name="listController">The list controller.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        public PortalSecurity(ICryptographyProvider cryptographyProvider, ListController listController, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController)
        {
            this.cryptographyProvider = cryptographyProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
            this.listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();
			Logger.LogDebug("PortalSecurity(cryptographyProvider={CryptographyProvider},listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): listController → {ListController1}", cryptographyProvider, listController, portalController, appStatus, portalGroupController, listController);
			Logger.LogDebug("PortalSecurity(cryptographyProvider={CryptographyProvider},listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): appStatus → {AppStatus1}", cryptographyProvider, listController, portalController, appStatus, portalGroupController, appStatus);
			Logger.LogDebug("PortalSecurity(cryptographyProvider={CryptographyProvider},listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): portalGroupController → {PortalGroupController1}", cryptographyProvider, listController, portalController, appStatus, portalGroupController, portalGroupController);
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
			Logger.LogDebug("PortalSecurity(cryptographyProvider={CryptographyProvider},listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): portalController → {PortalController1}", cryptographyProvider, listController, portalController, appStatus, portalGroupController, portalController);
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
        }

        /// <summary>
        /// The FilterFlag enum determines which filters are applied by the InputFilter
        /// function.  The Flags attribute allows the user to include multiple
        /// enumerated values in a single variable by OR'ing the individual values
        /// together.
        /// </summary>
        [Flags]
        [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
        public enum FilterFlag
        {
			private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalSecurity));
            /// <summary>Replaces line breaks with <c>&lt;br&gt;</c> tags.</summary>
            MultiLine = 1,

            /// <summary>Removes text which could be interpreted as HTML markup.</summary>
            [Obsolete("Deprecated in DotNetNuke 9.8.1. A direct call to WebUtility.HtmlEncode should be used. Scheduled for removal in v11.0.0.")]
            NoMarkup = 2,

            /// <summary>Removes text which could be interpreted as JavaScript commands.</summary>
            [Obsolete("Deprecated in DotNetNuke 9.8.1. A direct call to WebUtility.HtmlEncode should be used. Scheduled for removal in v11.0.0.")]
            NoScripting = 4,

            /// <summary>Removes text which could be interpreted as SQL commands.</summary>
            [Obsolete("Deprecated in DotNetNuke 9.8.1. Parameterized SQL should be preferred for SQL Injection protection. Scheduled for removal in v11.0.0.")]
            NoSQL = 8,

            /// <summary>Removes angle brackets from the text.</summary>
            [Obsolete("Deprecated in DotNetNuke 9.8.1. Individual string replacement should be completed. Scheduled for removal in v11.0.0.")]
            NoAngleBrackets = 16,

            /// <summary>Removes matches with the site's profanity filter.</summary>
            NoProfanity = 32,

            /// <summary>Removes all Unicode control characters (like \0, \t, \n, \r, etc.) from the string.</summary>
            /// <remarks>The control characters \r\n, \r, \n, and \t are replaced with a single space instead of being removed.</remarks>
            NoControlCharacters = 64,
        }

        /// <summary>Determines the configuration source for the remove and replace functions.</summary>
        public enum ConfigType
        {
            /// <summary>Retrieve configuration via <see cref="DotNetNuke.Common.Lists.ListController"/>.</summary>
            ListController = 0,

            /// <summary>Retrieve configuration from an external file.</summary>
            ExternalFile = 1,
        }

        /// <summary>
        /// determines whether to use system (host) list, portal specific list, or combine both
        /// At present only supported by ConfigType.ListController.
        /// </summary>
        public enum FilterScope
        {
            /// <summary>Use the host-level list.</summary>
            SystemList = 0,

            /// <summary>Use a portal-specific list.</summary>
            PortalList = 1,

            /// <summary>Combine both lists.</summary>
            SystemAndPortalList = 2,
        }

        private enum RoleType
        {
            Security = 0,
            Friend = 1,
            Follower = 2,
            Owner = 3,
        }

        /// <summary>Forces the secure connection.</summary>
        public static void ForceSecureConnection()
        {
			Logger.LogDebug(">>> Entering ForceSecureConnection()");
            // get current url
            var url = HttpContext.Current.Request.Url.ToString();
			Logger.LogDebug("ForceSecureConnection(): url → {Url}", url);

            // if unsecure connection
            if (url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                // switch to secure connection
                url = "https://" + url.Substring("http://".Length);

                // append ssl parameter to querystring to indicate secure connection processing has already occurred
                if (url.IndexOf("?", StringComparison.Ordinal) == -1)
                {
                    url = url + "?ssl=1";
                }
                else
                {
                    url = url + "&ssl=1";
                }

                // redirect to secure connection
                HttpContext.Current.Response.Redirect(url, true);
            }
			Logger.LogDebug("<<< Exiting ForceSecureConnection()");
        }

        /// <summary>Gets the cookie domain for the portal group or from web.config.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>Cookie domain for the portal group or from web.config.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload with IPortalController")]
        public static partial string GetCookieDomain(int portalId)
            => GetCookieDomain(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                portalId);

        /// <summary>Gets the cookie domain for the portal group or from web.config.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>Cookie domain for the portal group or from web.config.</returns>
        public static string GetCookieDomain(IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, int portalId)
        {
			Logger.LogDebug(">>> Entering GetCookieDomain(portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},portalId={PortalId})", portalController, appStatus, portalGroupController, portalId);
            string cookieDomain = string.Empty;
            if (PortalController.IsMemberOfPortalGroup(portalController, portalId))
			Logger.LogDebug("GetCookieDomain(portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},portalId={PortalId}): cookieDomain → {CookieDomain}", portalController, appStatus, portalGroupController, portalId, cookieDomain);
            {
                // set cookie domain for portal group
                var group = portalGroupController.GetPortalGroups().SingleOrDefault(p => p.MasterPortalId == PortalController.GetEffectivePortalId(portalController, appStatus, portalGroupController, portalId));
				Logger.LogDebug("GetCookieDomain(portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},portalId={PortalId}): group → {Group}", portalController, appStatus, portalGroupController, portalId, group);

                if (@group != null
                        && !string.IsNullOrEmpty(@group.AuthenticationDomain)
                        && ((IPortalAliasInfo)PortalSettings.Current.PortalAlias).HttpAlias.Contains(@group.AuthenticationDomain))
                {
                    cookieDomain = @group.AuthenticationDomain;
                }

                if (string.IsNullOrEmpty(cookieDomain))
                {
                    cookieDomain = FormsAuthentication.CookieDomain;
                }
            }
            else
            {
                // set cookie domain to be consistent with domain specification in web.config
                cookieDomain = FormsAuthentication.CookieDomain;
            }

			Logger.LogDebug("<<< Exiting GetCookieDomain(portalId={PortalId})", portalId);
            return cookieDomain;
        }

        /// <summary>Determines whether the current user is denied for the given role(s).</summary>
        /// <param name="roles">The semicolon separated list of roles.</param>
        /// <returns><see langword="true"/> if the current user is denied from the provided specified roles; otherwise, <see langword="false"/>.</returns>
        public static bool IsDenied(string roles)
        {
			Logger.LogDebug(">>> Entering IsDenied(roles={Roles})", roles);
            UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
            var settings = PortalController.Instance.GetCurrentSettings();
			Logger.LogDebug("IsDenied(roles={Roles}): settings → {Settings}", roles, settings);
			Logger.LogDebug("IsDenied(roles={Roles}): objUserInfo → {ObjUserInfo}", roles, objUserInfo);
			Logger.LogDebug("<<< Exiting IsDenied(roles={Roles})", roles);
            return IsDenied(objUserInfo, settings, roles);
        }

        /// <summary>Determines whether the specified user is denied for the given roles.</summary>
        /// <param name="objUserInfo">The user information.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="roles">The semicolon separated list of roles.</param>
        /// <returns><see langword="true"/> if the specified user is denied; otherwise, <see langword="false"/>.</returns>
        public static bool IsDenied(UserInfo objUserInfo, PortalSettings settings, string roles)
            => IsDenied(objUserInfo, (IPortalSettings)settings, roles);

        /// <summary>Determines whether the specified user is denied for the given roles.</summary>
        /// <param name="objUserInfo">The user information.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="roles">The semicolon separated list of roles.</param>
        /// <returns><see langword="true"/> if the specified user is denied; otherwise, <see langword="false"/>.</returns>
        public static bool IsDenied(UserInfo objUserInfo, IPortalSettings settings, string roles)
        {
            // superuser always has full access
            if (objUserInfo.IsSuperUser)
            {
                return false;
            }

            bool isDenied = false;

            if (roles != null)
            {
                // permissions strings are encoded with Deny permissions at the beginning and Grant permissions at the end for optimal performance
                foreach (string role in roles.Split(';'))
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        // Deny permission
                        if (role.StartsWith("!", StringComparison.Ordinal))
                        {
                            // Portal Admin cannot be denied from his/her portal (so ignore deny permissions if user is portal admin)
                            if (settings != null && !(settings.PortalId == objUserInfo.PortalID && objUserInfo.IsInRole(settings.AdministratorRoleName)))
                            {
                                string denyRole = role.Replace("!", string.Empty);
                                if (denyRole == Globals.glbRoleAllUsersName || objUserInfo.IsInRole(denyRole))
                                {
                                    isDenied = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return isDenied;
        }

        /// <summary>Determines whether the current user belongs to the specified role.</summary>
        /// <param name="role">The role name.</param>
        /// <returns><see langword="true"/> if user belongs to the specified role; otherwise, <see langword="false"/>.</returns>
        public static bool IsInRole(string role)
        {
			Logger.LogDebug(">>> Entering IsInRole(role={Role})", role);
            if (!string.IsNullOrEmpty(role) && role == Globals.glbRoleUnauthUserName && !HttpContext.Current.Request.IsAuthenticated)
            {
			Logger.LogDebug("<<< Exiting IsInRole(role={Role})", role);
                return true;
            }

            return IsInRoles(UserController.Instance.GetCurrentUserInfo(), PortalController.Instance.GetCurrentSettings(), role);
        }

        /// <summary>Determines whether the current user belongs to the specified roles.</summary>
        /// <param name="roles">The semicolon separated list of roles.</param>
        /// <returns><see langword="true"/> if user belongs to the specified roles; otherwise, <see langword="false"/>.</returns>
        public static bool IsInRoles(string roles)
        {
			Logger.LogDebug(">>> Entering IsInRoles(roles={Roles})", roles);
            var objUserInfo = UserController.Instance.GetCurrentUserInfo();
            var settings = PortalController.Instance.GetCurrentSettings();
			Logger.LogDebug("IsInRoles(roles={Roles}): settings → {Settings}", roles, settings);
			Logger.LogDebug("IsInRoles(roles={Roles}): objUserInfo → {ObjUserInfo}", roles, objUserInfo);
			Logger.LogDebug("<<< Exiting IsInRoles(roles={Roles})", roles);
            return IsInRoles(objUserInfo, settings, roles);
        }

        /// <summary>Determines whether the provided user belongs to the specified roles.</summary>
        /// <param name="objUserInfo">The user information.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="roles">The semicolon separated list of roles.</param>
        /// <returns><see langword="true"/> if the provided user belongs to the specific roles; otherwise, <see langword="false"/>.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IPortalSettings")]
        public static partial bool IsInRoles(UserInfo objUserInfo, PortalSettings settings, string roles)
            => IsInRoles(objUserInfo, (IPortalSettings)settings, roles);

        /// <summary>Determines whether the provided user belongs to the specified roles.</summary>
        /// <param name="objUserInfo">The user information.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="roles">The semicolon separated list of roles.</param>
        /// <returns><see langword="true"/> if the provided user belongs to the specific roles; otherwise, <see langword="false"/>.</returns>
        public static bool IsInRoles(UserInfo objUserInfo, IPortalSettings settings, string roles)
        {
            if (objUserInfo.IsSuperUser)
            {
                return true;
            }

            if (roles == null)
            {
                return false;
            }

            foreach (var role in roles.Split(';'))
            {
                ProcessRole(objUserInfo, settings, role, out var roleAllowed);
                if (roleAllowed.HasValue)
                {
                    return roleAllowed.Value;
                }
            }

            return false;
        }

        /// <summary>Determines whether the specified user is a friend of the current user.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns><see langword="true"/> if the specified user is a friend of the current user; otherwise, <see langword="false"/>.</returns>
        public static bool IsFriend(int userId)
        {
			Logger.LogDebug(">>> Entering IsFriend(userId={UserId})", userId);
            var objUserInfo = UserController.Instance.GetCurrentUserInfo();
            var settings = PortalController.Instance.GetCurrentSettings();
			Logger.LogDebug("IsFriend(userId={UserId}): settings → {Settings}", userId, settings);
			Logger.LogDebug("IsFriend(userId={UserId}): objUserInfo → {ObjUserInfo}", userId, objUserInfo);
			Logger.LogDebug("<<< Exiting IsFriend(userId={UserId})", userId);
            return IsInRoles(objUserInfo, settings, RoleFriendPrefix + userId);
        }

        /// <summary>Determines whether the specified user is a follower of the current user.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns><see langword="true"/> if the specified user is a follower of the current user; otherwise, <see langword="false"/>.</returns>
        public static bool IsFollower(int userId)
        {
			Logger.LogDebug(">>> Entering IsFollower(userId={UserId})", userId);
            var objUserInfo = UserController.Instance.GetCurrentUserInfo();
            var settings = PortalController.Instance.GetCurrentSettings();
			Logger.LogDebug("IsFollower(userId={UserId}): settings → {Settings}", userId, settings);
			Logger.LogDebug("IsFollower(userId={UserId}): objUserInfo → {ObjUserInfo}", userId, objUserInfo);
			Logger.LogDebug("<<< Exiting IsFollower(userId={UserId})", userId);
            return IsInRoles(objUserInfo, settings, RoleFollowerPrefix + userId);
        }

        /// <summary>Determines whether the specified user is an owner.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns><see langword="true"/> if the specified user is an owner; otherwise, <see langword="false"/>.</returns>
        public static bool IsOwner(int userId)
        {
			Logger.LogDebug(">>> Entering IsOwner(userId={UserId})", userId);
            var objUserInfo = UserController.Instance.GetCurrentUserInfo();
            var settings = PortalController.Instance.GetCurrentSettings();
			Logger.LogDebug("IsOwner(userId={UserId}): settings → {Settings}", userId, settings);
			Logger.LogDebug("IsOwner(userId={UserId}): objUserInfo → {ObjUserInfo}", userId, objUserInfo);
			Logger.LogDebug("<<< Exiting IsOwner(userId={UserId})", userId);
            return IsInRoles(objUserInfo, settings, RoleOwnerPrefix + userId);
        }

        /// <summary>This function creates a random key.</summary>
        /// <param name="numBytes">This is the number of bytes for the key.</param>
        /// <returns>A random string.</returns>
        /// <remarks>This is a public function used for generating SHA1 keys.</remarks>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string CreateKey(int numBytes)
        {
			Logger.LogDebug(">>> Entering CreateKey(numBytes={NumBytes})", numBytes);
            using var rng = new RNGCryptoServiceProvider();
            var buff = new byte[numBytes];
			Logger.LogDebug("CreateKey(numBytes={NumBytes}): rng → {Rng}", numBytes, rng);
            rng.GetBytes(buff);
			Logger.LogDebug("<<< Exiting CreateKey(numBytes={NumBytes})", numBytes);
            return BytesToHexString(buff);
        }

        /// <summary>Decrypts the provided string data using a supplied key.</summary>
        /// <param name="strKey">The encryption key.</param>
        /// <param name="strData">The encrypted data.</param>
        /// <returns>The decrypted string.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        [DnnDeprecated(10, 2, 2, "Use DotNetNuke.Abstractions.Security.ICryptographyProvider")]
        public partial string Decrypt(string strKey, string strData)
        {
			Logger.LogDebug(">>> Entering Decrypt(strKey={StrKey},strData={StrData})", strKey, strData);
			Logger.LogDebug("<<< Exiting Decrypt(strKey={StrKey},strData={StrData})", strKey, strData);
            return this.cryptographyProvider.DecryptParameter(strData, strKey, CryptographyProvider.Instance().EncryptParameterAlgorithmName);
        }

        /// <summary>Decrypts a string using a provided passphrase.</summary>
        /// <param name="message">The encrypted message.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns>The decrypted string.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        [DnnDeprecated(10, 2, 2, "Use DotNetNuke.Abstractions.Security.ICryptographyProvider")]
        public partial string DecryptString(string message, string passphrase)
        {
			Logger.LogDebug(">>> Entering DecryptString(message={Message},passphrase=<redacted>)", message);
            if (!message.Contains(EncryptedStringAlgorithmSeparator, StringComparison.Ordinal))
            {
                return this.cryptographyProvider.DecryptString(
                    message,
                    passphrase,
                    CryptographyProvider.Instance().EncryptStringAlgorithmName,
                    null);
            }

            var endOfMessage = message.IndexOf(EncryptedStringAlgorithmSeparator, StringComparison.Ordinal);
			Logger.LogDebug("DecryptString(message={Message},passphrase=<redacted>): endOfMessage → {EndOfMessage}", message, endOfMessage);
            var encryptedMessage = message.Substring(0, endOfMessage);
			Logger.LogDebug("DecryptString(message={Message},passphrase=<redacted>): encryptedMessage → {EncryptedMessage}", message, encryptedMessage);
            var startOfAlgorithm = endOfMessage + EncryptedStringAlgorithmSeparator.Length;
            var endOfAlgorithm = message.IndexOf(EncryptedStringInitializationVectorSeparator, startOfAlgorithm, StringComparison.Ordinal);
			Logger.LogDebug("DecryptString(message={Message},passphrase=<redacted>): endOfAlgorithm → {EndOfAlgorithm}", message, endOfAlgorithm);
            var algorithmName = message.Substring(startOfAlgorithm, endOfAlgorithm - startOfAlgorithm);
			Logger.LogDebug("DecryptString(message={Message},passphrase=<redacted>): algorithmName → {AlgorithmName}", message, algorithmName);
            var startOfInitializationVector = endOfAlgorithm + EncryptedStringInitializationVectorSeparator.Length;
            var initializationVector = message.Substring(startOfInitializationVector);
			Logger.LogDebug("DecryptString(message={Message},passphrase=<redacted>): initializationVector → {InitializationVector}", message, initializationVector);
			Logger.LogDebug("<<< Exiting DecryptString(message={Message},passphrase=<redacted>)", message);
            return this.cryptographyProvider.DecryptString(encryptedMessage, passphrase, algorithmName, initializationVector);
        }

        /// <summary>Encrypts the specified key.</summary>
        /// <param name="key">The key.</param>
        /// <param name="data">The data.</param>
        /// <returns>The encrypted string.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        [DnnDeprecated(10, 2, 2, "Use DotNetNuke.Abstractions.Security.ICryptographyProvider")]
        public partial string Encrypt(string key, string data)
        {
			Logger.LogDebug(">>> Entering Encrypt(key={Key},data={Data})", key, data);
			Logger.LogDebug("<<< Exiting Encrypt(key={Key},data={Data})", key, data);
            return this.cryptographyProvider.EncryptParameter(data, key).EncryptedMessage;
        }

        /// <summary>Encrypts a string using a provided passphrase.</summary>
        /// <param name="message">The message.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns>The encrypted string.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        [DnnDeprecated(10, 2, 2, "Use DotNetNuke.Abstractions.Security.ICryptographyProvider")]
        public partial string EncryptString(string message, string passphrase)
        {
			Logger.LogDebug(">>> Entering EncryptString(message={Message},passphrase=<redacted>)", message);
            var (encryptedMessage, algorithm, initializationVector) = this.cryptographyProvider.EncryptString(message, passphrase);
			Logger.LogDebug("<<< Exiting EncryptString(message={Message},passphrase=<redacted>)", message);
            return $"{encryptedMessage}{EncryptedStringAlgorithmSeparator}{algorithm}{EncryptedStringInitializationVectorSeparator}{initializationVector}";
        }

        /// <summary>This function applies security filtering to the UserInput string.</summary>
        /// <param name="userInput">This is the string to be filtered.</param>
        /// <param name="filterType">Flags which designate the filters to be applied.</param>
        /// <returns>Filtered UserInput.</returns>
        public string InputFilter(string userInput, FilterFlag filterType)
        {
			Logger.LogDebug(">>> Entering InputFilter(userInput={UserInput},filterType={FilterType})", userInput, filterType);
            if (userInput == null)
            {
                return string.Empty;
            }

            var tempInput = userInput;
			Logger.LogDebug("<<< Exiting InputFilter(userInput={UserInput},filterType={FilterType})", userInput, filterType);
			Logger.LogDebug("InputFilter(userInput={UserInput},filterType={FilterType}): tempInput → {TempInput}", userInput, filterType, tempInput);
#pragma warning disable CS0618 // Type or member is obsolete
            if ((filterType & FilterFlag.NoAngleBrackets) == FilterFlag.NoAngleBrackets)
#pragma warning restore CS0618 // Type or member is obsolete
            {
                var removeAngleBrackets = Config.GetSetting("RemoveAngleBrackets") != null && bool.Parse(Config.GetSetting("RemoveAngleBrackets"));
				Logger.LogDebug("InputFilter(userInput={UserInput},filterType={FilterType}): removeAngleBrackets → {RemoveAngleBrackets}", userInput, filterType, removeAngleBrackets);
                if (removeAngleBrackets)
                {
                    tempInput = FormatAngleBrackets(tempInput);
                }
            }

#pragma warning disable CS0618 // Type or member is obsolete
            if ((filterType & FilterFlag.NoSQL) == FilterFlag.NoSQL)
#pragma warning restore CS0618 // Type or member is obsolete
            {
                tempInput = FormatRemoveSQL(tempInput);
            }

#pragma warning disable CS0618 // Type or member is obsolete
            if ((filterType & FilterFlag.NoMarkup) == FilterFlag.NoMarkup && IncludesMarkup(tempInput))
#pragma warning restore CS0618 // Type or member is obsolete
            {
                tempInput = HttpUtility.HtmlEncode(tempInput);
            }

#pragma warning disable CS0618 // Type or member is obsolete
            if ((filterType & FilterFlag.NoScripting) == FilterFlag.NoScripting)
#pragma warning restore CS0618 // Type or member is obsolete
            {
                tempInput = FilterStrings(tempInput);
            }

            if ((filterType & FilterFlag.MultiLine) == FilterFlag.MultiLine)
            {
                tempInput = FormatMultiLine(tempInput);
            }

            if ((filterType & FilterFlag.NoProfanity) == FilterFlag.NoProfanity)
            {
                tempInput = this.Replace(tempInput, ConfigType.ListController, "ProfanityFilter", FilterScope.SystemAndPortalList);
            }

            if ((filterType & FilterFlag.NoControlCharacters) == FilterFlag.NoControlCharacters)
            {
                tempInput = FormatRemoveControlCharacters(tempInput);
            }

            return tempInput;
        }

        /// <summary>Replaces profanity words with other words in the provided input string.</summary>
        /// <remarks>
        /// The correspondence between the words to search and the words to replace could be specified in two different places:
        /// 1) In an external file. (NOT IMPLEMENTED)
        /// 2) In System/Site lists.
        /// The name of the System List is "ProfanityFilter". The name of the list in each portal is composed using the following rule:
        /// "ProfanityFilter-" + PortalID.
        /// </remarks>
        /// <param name="inputString">The string to search the words in.</param>
        /// <param name="configType">The type of configuration.</param>
        /// <param name="configSource">The external file to search the words. Ignored when configType is ListController.</param>
        /// <param name="filterScope">When using ListController configType, this parameter indicates which list(s) to use.</param>
        /// <returns>The original text with the profanity words replaced.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string Replace(string inputString, ConfigType configType, string configSource, FilterScope filterScope)
        {
			Logger.LogDebug(">>> Entering Replace(inputString={InputString},configType={ConfigType},configSource={ConfigSource},filterScope={FilterScope})", inputString, configType, configSource, filterScope);
            switch (configType)
            {
                case ConfigType.ListController:
                    const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
                    const string listName = "ProfanityFilter";

                    IPortalSettings settings;

                    IEnumerable<ListEntryInfo> listEntryHostInfos;
                    IEnumerable<ListEntryInfo> listEntryPortalInfos;

                    switch (filterScope)
                    {
                        case FilterScope.SystemList:
                            listEntryHostInfos = this.listController.GetListEntryInfoItems(listName, string.Empty, Null.NullInteger);
                            inputString = listEntryHostInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", removeItem.Value, options));
							Logger.LogDebug("Replace(inputString={InputString},configType={ConfigType},configSource={ConfigSource},filterScope={FilterScope}): listEntryHostInfos → {ListEntryHostInfos}", inputString, configType, configSource, filterScope, listEntryHostInfos);
                            break;
                        case FilterScope.SystemAndPortalList:
                            settings = PortalController.Instance.GetCurrentSettings();
                            listEntryHostInfos = this.listController.GetListEntryInfoItems(listName, string.Empty, Null.NullInteger);
                            listEntryPortalInfos = this.listController.GetListEntryInfoItems(listName + "-" + settings.PortalId, string.Empty, settings.PortalId);
                            inputString = listEntryHostInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", removeItem.Value, options));
                            inputString = listEntryPortalInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", removeItem.Value, options));
							Logger.LogDebug("Replace(inputString={InputString},configType={ConfigType},configSource={ConfigSource},filterScope={FilterScope}): settings → {Settings}", inputString, configType, configSource, filterScope, settings);
                            break;
                        case FilterScope.PortalList:
                            settings = PortalController.Instance.GetCurrentSettings();
                            listEntryPortalInfos = this.listController.GetListEntryInfoItems(listName + "-" + settings.PortalId, string.Empty, settings.PortalId);
                            inputString = listEntryPortalInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", removeItem.Value, options));
							Logger.LogDebug("Replace(inputString={InputString},configType={ConfigType},configSource={ConfigSource},filterScope={FilterScope}): listEntryPortalInfos → {ListEntryPortalInfos}", inputString, configType, configSource, filterScope, listEntryPortalInfos);
                            break;
                    }

                    break;
                case ConfigType.ExternalFile:
					Logger.LogWarning("Throwing NotImplementedException from Replace(inputString={InputString},configType={ConfigType},configSource={ConfigSource},filterScope={FilterScope})", inputString, configType, configSource, filterScope);
                    throw new NotImplementedException();
                default:
					Logger.LogWarning("Throwing ArgumentOutOfRangeException from Replace(inputString={InputString},configType={ConfigType},configSource={ConfigSource},filterScope={FilterScope})", inputString, configType, configSource, filterScope);
			Logger.LogDebug("<<< Exiting Replace(inputString={InputString},configType={ConfigType},configSource={ConfigSource},filterScope={FilterScope})", inputString, configType, configSource, filterScope);
                    throw new ArgumentOutOfRangeException(nameof(configType));
            }

            return inputString;
        }

        /// <summary>Removes profanity words in the provided input string.</summary>
        /// <remarks>
        /// The words to search could be defined in two different places:
        /// 1) In an external file. (NOT IMPLEMENTED)
        /// 2) In System/Site lists.
        /// The name of the System List is "ProfanityFilter". The name of the list in each portal is composed using the following rule:
        /// "ProfanityFilter-" + PortalID.
        /// </remarks>
        /// <param name="inputString">The string to search the words in.</param>
        /// <param name="configType">The type of configuration.</param>
        /// <param name="configSource">The external file to search the words. Ignored when configType is ListController.</param>
        /// <param name="filterScope">When using ListController configType, this parameter indicates which list(s) to use.</param>
        /// <returns>The original text with the profanity words removed.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string Remove(string inputString, ConfigType configType, string configSource, FilterScope filterScope)
        {
			Logger.LogDebug(">>> Entering Remove(inputString={InputString},configType={ConfigType},configSource={ConfigSource},filterScope={FilterScope})", inputString, configType, configSource, filterScope);
            switch (configType)
            {
                case ConfigType.ListController:
                    const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
                    const string listName = "ProfanityFilter";

                    IPortalSettings settings;

                    IEnumerable<ListEntryInfo> listEntryHostInfos;
                    IEnumerable<ListEntryInfo> listEntryPortalInfos;

                    switch (filterScope)
                    {
                        case FilterScope.SystemList:
                            listEntryHostInfos = this.listController.GetListEntryInfoItems(listName, string.Empty, Null.NullInteger);
                            inputString = listEntryHostInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", string.Empty, options));
							Logger.LogDebug("Remove(inputString={InputString},configType={ConfigType},configSource={ConfigSource},filterScope={FilterScope}): listEntryHostInfos → {ListEntryHostInfos}", inputString, configType, configSource, filterScope, listEntryHostInfos);
                            break;
                        case FilterScope.SystemAndPortalList:
                            settings = PortalController.Instance.GetCurrentSettings();
                            listEntryHostInfos = this.listController.GetListEntryInfoItems(listName, string.Empty, Null.NullInteger);
                            listEntryPortalInfos = this.listController.GetListEntryInfoItems(listName + "-" + settings.PortalId, string.Empty, settings.PortalId);
                            inputString = listEntryHostInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", string.Empty, options));
                            inputString = listEntryPortalInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", string.Empty, options));
							Logger.LogDebug("Remove(inputString={InputString},configType={ConfigType},configSource={ConfigSource},filterScope={FilterScope}): settings → {Settings}", inputString, configType, configSource, filterScope, settings);
                            break;
                        case FilterScope.PortalList:
                            settings = PortalController.Instance.GetCurrentSettings();
                            listEntryPortalInfos = this.listController.GetListEntryInfoItems(listName + "-" + settings.PortalId, string.Empty, settings.PortalId);
                            inputString = listEntryPortalInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", string.Empty, options));
							Logger.LogDebug("Remove(inputString={InputString},configType={ConfigType},configSource={ConfigSource},filterScope={FilterScope}): listEntryPortalInfos → {ListEntryPortalInfos}", inputString, configType, configSource, filterScope, listEntryPortalInfos);
                            break;
                    }

                    break;
                case ConfigType.ExternalFile:
					Logger.LogWarning("Throwing NotImplementedException from Remove(inputString={InputString},configType={ConfigType},configSource={ConfigSource},filterScope={FilterScope})", inputString, configType, configSource, filterScope);
                    throw new NotImplementedException();
                default:
					Logger.LogWarning("Throwing ArgumentOutOfRangeException from Remove(inputString={InputString},configType={ConfigType},configSource={ConfigSource},filterScope={FilterScope})", inputString, configType, configSource, filterScope);
			Logger.LogDebug("<<< Exiting Remove(inputString={InputString},configType={ConfigType},configSource={ConfigSource},filterScope={FilterScope})", inputString, configType, configSource, filterScope);
                    throw new ArgumentOutOfRangeException(nameof(configType));
            }

            return inputString;
        }

        /// <summary>
        /// Signs the provided user in and sets a persistent login cookie if needed.
        /// </summary>
        /// <param name="user">The user info.</param>
        /// <param name="createPersistentCookie">if set to <see langword="true"/> [create persistent cookie].</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void SignIn(UserInfo user, bool createPersistentCookie)
        {
			Logger.LogDebug(">>> Entering SignIn(user={User},createPersistentCookie=<redacted>)", user);
            if (PortalController.IsMemberOfPortalGroup(this.portalController, user.PortalID) || createPersistentCookie)
            {
                // Create a custom auth cookie

                // first, create the authentication ticket
                var authenticationTicket = createPersistentCookie
                    ? new FormsAuthenticationTicket(user.Username, true, Config.GetPersistentCookieTimeout(this.appStatus))
                    : new FormsAuthenticationTicket(user.Username, false, Config.GetAuthCookieTimeout(this.appStatus));

                // encrypt it
                var encryptedAuthTicket = FormsAuthentication.Encrypt(authenticationTicket);
				Logger.LogDebug("SignIn(user={User},createPersistentCookie=<redacted>): encryptedAuthTicket → {EncryptedAuthTicket}", user, encryptedAuthTicket);

                // Create a new Cookie
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedAuthTicket)
                {
                    Expires = authenticationTicket.Expiration,
                    Domain = GetCookieDomain(this.portalController, this.appStatus, this.portalGroupController, user.PortalID),
                    Path = FormsAuthentication.FormsCookiePath,
                    Secure = FormsAuthentication.RequireSSL,
                };

                if (HttpContext.Current.Response.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    HttpContext.Current.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
                }

                HttpContext.Current.Response.Cookies.Set(authCookie);
                AuthCookieController.Instance.Update(authCookie.Value, authCookie.Expires.ToUniversalTime(), user.UserID);

                if (PortalController.IsMemberOfPortalGroup(this.portalController, user.PortalID))
                {
                    var domain = GetCookieDomain(this.portalController, this.appStatus, this.portalGroupController, user.PortalID);
					Logger.LogDebug("SignIn(user={User},createPersistentCookie=<redacted>): domain → {Domain}", user, domain);
                    var siteGroupCookie = new HttpCookie("SiteGroup", domain)
                    {
                        Expires = authenticationTicket.Expiration,
                        Domain = domain,
                        Path = FormsAuthentication.FormsCookiePath,
                        Secure = FormsAuthentication.RequireSSL,
                    };

                    HttpContext.Current.Response.Cookies.Set(siteGroupCookie);
                }
            }
            else
            {
                if (HttpContext.Current.Response.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    HttpContext.Current.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
                }

                FormsAuthentication.SetAuthCookie(user.Username, false);
                var authCookie = HttpContext.Current.Response.Cookies[FormsAuthentication.FormsCookieName];
                if (!string.IsNullOrEmpty(authCookie?.Value))
                {
                    var t = FormsAuthentication.Decrypt(authCookie.Value);
					Logger.LogDebug("SignIn(user={User},createPersistentCookie=<redacted>): t → {T}", user, t);
                    if (t != null)
                    {
                        AuthCookieController.Instance.Update(authCookie.Value, t.Expiration.ToUniversalTime(), user.UserID);
                    }
                }
            }

            if (user.IsSuperUser)
            {
                // save userinfo object in context to ensure Personalization is saved correctly
                HttpContext.Current.Items["UserInfo"] = user;
            }

            // Identity the Login is processed by system.
            HttpContext.Current.Items["DNN_UserSignIn"] = true;
			Logger.LogDebug("<<< Exiting SignIn(user={User},createPersistentCookie=<redacted>)", user);
        }

        /// <summary>
        /// Signs the current user out.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void SignOut()
        {
			Logger.LogDebug(">>> Entering SignOut()");
            InvalidateAspNetSession(HttpContext.Current);

            var currentAuthCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (currentAuthCookie != null)
            {
                // This will prevent next requests from being authenticated if using smae cookie
                AuthCookieController.Instance.Update(currentAuthCookie.Value, OldExpiryTime, Null.NullInteger);
            }

            // Log User Off from Cookie Authentication System
            var domainCookie = HttpContext.Current.Request.Cookies["SiteGroup"];
			Logger.LogDebug("SignOut(): domainCookie → {DomainCookie}", domainCookie);
            if (domainCookie == null)
            {
                // Forms Authentication's Logout
                FormsAuthentication.SignOut();
            }
            else
            {
                // clear custom domain cookie
                var domain = domainCookie.Value;

                // Create a new Cookie
                var str = string.Empty;
				Logger.LogDebug("SignOut(): str → {Str}", str);
                if (HttpContext.Current.Request.Browser["supportsEmptyStringInCookieValue"] == "false")
                {
                    str = "NoCookie";
                }

                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, str)
                {
                    Expires = OldExpiryTime,
                    Domain = domain,
                    Path = FormsAuthentication.FormsCookiePath,
                    Secure = FormsAuthentication.RequireSSL,
                };

                HttpContext.Current.Response.Cookies.Set(authCookie);

                var siteGroupCookie = new HttpCookie("SiteGroup", str)
                {
                    Expires = OldExpiryTime,
                    Domain = domain,
                    Path = FormsAuthentication.FormsCookiePath,
                    Secure = FormsAuthentication.RequireSSL,
                };

                HttpContext.Current.Response.Cookies.Set(siteGroupCookie);
            }

            // Remove current userinfo from context items
            HttpContext.Current.Items.Remove("UserInfo");

            // remove language cookie
            var httpCookie = HttpContext.Current.Response.Cookies["language"];
			Logger.LogDebug("SignOut(): httpCookie → {HttpCookie}", httpCookie);
            if (httpCookie != null)
            {
                httpCookie.Value = string.Empty;
				Logger.LogDebug("SignOut(): httpCookie.Value → {Arg0}", httpCookie.Value);
            }

            // remove authentication type cookie
            var cookie = HttpContext.Current.Response.Cookies["authentication"];
			Logger.LogDebug("SignOut(): cookie → {Cookie}", cookie);
            if (cookie != null)
            {
                cookie.Value = string.Empty;
				Logger.LogDebug("SignOut(): cookie.Value → {Arg0}", cookie.Value);
            }

            // expire cookies
            cookie = HttpContext.Current.Response.Cookies["portalaliasid"];
            if (cookie != null)
            {
                cookie.Value = null;
                cookie.Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/";
                cookie.Expires = DateTime.Now.AddYears(-30);
            }

            cookie = HttpContext.Current.Response.Cookies["portalroles"];
            if (cookie != null)
            {
                cookie.Value = null;
                cookie.Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/";
                cookie.Expires = DateTime.Now.AddYears(-30);
            }

            // clear any authentication provider tokens that match *UserToken convention e.g. FacebookUserToken ,TwitterUserToken, LiveUserToken and GoogleUserToken
            var authCookies = HttpContext.Current.Request.Cookies.AllKeys;
            foreach (var authCookie in authCookies)
            {
                if (authCookie.EndsWith("UserToken", StringComparison.OrdinalIgnoreCase))
                {
                    var auth = HttpContext.Current.Response.Cookies[authCookie];
                    if (auth != null)
                    {
                        auth.Value = null;
						Logger.LogDebug("SignOut(): auth.Value → {Arg0}", auth.Value);
                        auth.Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/";
                        auth.Expires = DateTime.Now.AddYears(-30);
                    }
                }
            }
			Logger.LogDebug("<<< Exiting SignOut()");
        }

        /// <summary>This function applies security filtering to the UserInput string, and reports whether the input string is valid.</summary>
        /// <param name="userInput">This is the string to be filtered.</param>
        /// <param name="filterType">Flags which designate the filters to be applied.</param>
        /// <returns><see langword="true"/> if the input is valid, otherwise <see langword="false"/>.</returns>
        public bool ValidateInput(string userInput, FilterFlag filterType)
        {
			Logger.LogDebug(">>> Entering ValidateInput(userInput={UserInput},filterType={FilterType})", userInput, filterType);
            string filteredInput = this.InputFilter(userInput, filterType);

			Logger.LogDebug("<<< Exiting ValidateInput(userInput={UserInput},filterType={FilterType})", userInput, filterType);
            return userInput == filteredInput;
        }

        /// <summary>
        /// This function loops through every portal that has set its own AllowedExtensionWhitelist
        /// and checks that there are no extensions there that are restricted by the host
        ///
        /// The only time we should call this is if the host allowed extensions list has changed.
        /// </summary>
        /// <param name="newMasterList">Comma separated list of extensions that govern all users on this installation.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void CheckAllPortalFileExtensionWhitelists(string newMasterList)
        {
			Logger.LogDebug(">>> Entering CheckAllPortalFileExtensionWhitelists(newMasterList={NewMasterList})", newMasterList);
            var masterList = new FileExtensionWhitelist(newMasterList);
            var portalSettings = Data.DataProvider.Instance().GetPortalSettingsBySetting("AllowedExtensionsWhitelist", null);
			Logger.LogDebug("CheckAllPortalFileExtensionWhitelists(newMasterList={NewMasterList}): portalSettings → {PortalSettings}", newMasterList, portalSettings);
			Logger.LogDebug("CheckAllPortalFileExtensionWhitelists(newMasterList={NewMasterList}): masterList → {MasterList}", newMasterList, masterList);
            foreach (var portalId in portalSettings.Keys)
            {
                if (!string.IsNullOrEmpty(portalSettings[portalId]))
                {
                    var portalExts = new FileExtensionWhitelist(portalSettings[portalId]);
					Logger.LogDebug("CheckAllPortalFileExtensionWhitelists(newMasterList={NewMasterList}): portalExts → {PortalExts}", newMasterList, portalExts);
                    var newValue = portalExts.RestrictBy(masterList).ToStorageString();
					Logger.LogDebug("CheckAllPortalFileExtensionWhitelists(newMasterList={NewMasterList}): newValue → {NewValue}", newMasterList, newValue);
                    if (newValue != portalSettings[portalId])
                    {
                        PortalController.UpdatePortalSetting(this.portalController, portalId, "AllowedExtensionsWhitelist", newValue, false);
                    }
                }
            }
			Logger.LogDebug("<<< Exiting CheckAllPortalFileExtensionWhitelists(newMasterList={NewMasterList})", newMasterList);
        }

        private static void ProcessRole(UserInfo user, IPortalSettings settings, string roleName, out bool? roleAllowed)
        {
			Logger.LogDebug(">>> Entering ProcessRole(user={User},settings={Settings},roleName={RoleName})", user, settings, roleName);
            var roleType = GetRoleType(roleName);
            switch (roleType)
			Logger.LogDebug("ProcessRole(user={User},settings={Settings},roleName={RoleName}): roleType → {RoleType}", user, settings, roleName, roleType);
            {
                case RoleType.Friend:
                    ProcessFriendRole(user, roleName, out roleAllowed);
                    break;
                case RoleType.Follower:
                    ProcessFollowerRole(user, roleName, out roleAllowed);
                    break;
                case RoleType.Owner:
                    ProcessOwnerRole(user, roleName, out roleAllowed);
                    break;
                default:
                    ProcessSecurityRole(user, settings, roleName, out roleAllowed);
                    break;
            }
			Logger.LogDebug("<<< Exiting ProcessRole(user={User},settings={Settings},roleName={RoleName})", user, settings, roleName);
        }

        private static void ProcessFriendRole(UserInfo user, string roleName, out bool? roleAllowed)
        {
			Logger.LogDebug(">>> Entering ProcessFriendRole(user={User},roleName={RoleName})", user, roleName);
            roleAllowed = null;
            var targetUser = UserController.Instance.GetUserById(user.PortalID, GetEntityFromRoleName(roleName));
			Logger.LogDebug("ProcessFriendRole(user={User},roleName={RoleName}): targetUser → {TargetUser}", user, roleName, targetUser);
			Logger.LogDebug("ProcessFriendRole(user={User},roleName={RoleName}): roleAllowed → {RoleAllowed}", user, roleName, roleAllowed);
            var relationShip = RelationshipController.Instance.GetFriendRelationship(user, targetUser);
			Logger.LogDebug("ProcessFriendRole(user={User},roleName={RoleName}): relationShip → {RelationShip}", user, roleName, relationShip);
            if (relationShip != null && relationShip.Status == RelationshipStatus.Accepted)
            {
                roleAllowed = true;
            }
			Logger.LogDebug("<<< Exiting ProcessFriendRole(user={User},roleName={RoleName})", user, roleName);
        }

        private static void ProcessFollowerRole(UserInfo user, string roleName, out bool? roleAllowed)
        {
			Logger.LogDebug(">>> Entering ProcessFollowerRole(user={User},roleName={RoleName})", user, roleName);
            roleAllowed = null;
            var targetUser = UserController.Instance.GetUserById(user.PortalID, GetEntityFromRoleName(roleName));
			Logger.LogDebug("ProcessFollowerRole(user={User},roleName={RoleName}): targetUser → {TargetUser}", user, roleName, targetUser);
			Logger.LogDebug("ProcessFollowerRole(user={User},roleName={RoleName}): roleAllowed → {RoleAllowed}", user, roleName, roleAllowed);
            var relationShip = RelationshipController.Instance.GetFollowerRelationship(user, targetUser);
			Logger.LogDebug("ProcessFollowerRole(user={User},roleName={RoleName}): relationShip → {RelationShip}", user, roleName, relationShip);
            if (relationShip != null && relationShip.Status == RelationshipStatus.Accepted)
            {
                roleAllowed = true;
            }
			Logger.LogDebug("<<< Exiting ProcessFollowerRole(user={User},roleName={RoleName})", user, roleName);
        }

        private static void ProcessOwnerRole(UserInfo user, string roleName, out bool? roleAllowed)
        {
			Logger.LogDebug(">>> Entering ProcessOwnerRole(user={User},roleName={RoleName})", user, roleName);
            roleAllowed = null;
            var entityId = GetEntityFromRoleName(roleName);
			Logger.LogDebug("ProcessOwnerRole(user={User},roleName={RoleName}): entityId → {EntityId}", user, roleName, entityId);
			Logger.LogDebug("ProcessOwnerRole(user={User},roleName={RoleName}): roleAllowed → {RoleAllowed}", user, roleName, roleAllowed);
            if (entityId == user.UserID)
            {
                roleAllowed = true;
            }
			Logger.LogDebug("<<< Exiting ProcessOwnerRole(user={User},roleName={RoleName})", user, roleName);
        }

        private static int GetEntityFromRoleName(string roleName)
        {
			Logger.LogDebug(">>> Entering GetEntityFromRoleName(roleName={RoleName})", roleName);
            var roleParts = roleName.Split(':');
            int result;
			Logger.LogDebug("GetEntityFromRoleName(roleName={RoleName}): roleParts → {RoleParts}", roleName, roleParts);
            if (roleParts.Length > 1 && int.TryParse(roleParts[1], out result))
            {
			Logger.LogDebug("<<< Exiting GetEntityFromRoleName(roleName={RoleName})", roleName);
                return result;
            }

            return Null.NullInteger;
        }

        private static void ProcessSecurityRole(UserInfo user, IPortalSettings settings, string roleName, out bool? roleAllowed)
        {
			Logger.LogDebug(">>> Entering ProcessSecurityRole(user={User},settings={Settings},roleName={RoleName})", user, settings, roleName);
            roleAllowed = null;

			Logger.LogDebug("ProcessSecurityRole(user={User},settings={Settings},roleName={RoleName}): roleAllowed → {RoleAllowed}", user, settings, roleName, roleAllowed);
            // permissions strings are encoded with Deny permissions at the beginning and Grant permissions at the end for optimal performance
            if (!string.IsNullOrEmpty(roleName))
            {
                // Deny permission
                if (roleName.StartsWith("!", StringComparison.Ordinal))
                {
                    // Portal Admin cannot be denied from his/her portal (so ignore deny permissions if user is portal admin)
                    if (settings != null && !(settings.PortalId == user.PortalID && user.IsInRole(settings.AdministratorRoleName)))
                    {
                        string denyRole = roleName.Replace("!", string.Empty);
                        if (denyRole == Globals.glbRoleAllUsersName || user.IsInRole(denyRole))
                        {
                            roleAllowed = false;
                        }
                    }
                }

                // Grant permission
                else
                {
                    if (roleName == Globals.glbRoleAllUsersName || user.IsInRole(roleName))
                    {
                        roleAllowed = true;
                    }
                }
            }
			Logger.LogDebug("<<< Exiting ProcessSecurityRole(user={User},settings={Settings},roleName={RoleName})", user, settings, roleName);
        }

        private static RoleType GetRoleType(string roleName)
        {
			Logger.LogDebug(">>> Entering GetRoleType(roleName={RoleName})", roleName);
            if (roleName.StartsWith(RoleFriendPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return RoleType.Friend;
            }

            if (roleName.StartsWith(RoleFollowerPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return RoleType.Follower;
            }

            if (roleName.StartsWith(RoleOwnerPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
			Logger.LogDebug("<<< Exiting GetRoleType(roleName={RoleName})", roleName);
                return RoleType.Owner;
            }

            return RoleType.Security;
        }

        private static string BytesToHexString(IEnumerable<byte> bytes)
        {
			Logger.LogDebug(">>> Entering BytesToHexString(bytes={Bytes})", bytes);
            var hexString = new StringBuilder();
            foreach (var b in bytes)
            {
			Logger.LogDebug("BytesToHexString(bytes={Bytes}): hexString → {HexString}", bytes, hexString);
                hexString.Append(string.Format(CultureInfo.InvariantCulture, "{0:X2}", b));
            }

			Logger.LogDebug("<<< Exiting BytesToHexString(bytes={Bytes})", bytes);
            return hexString.ToString();
        }

        /// <summary>
        /// This function uses Regex search strings to remove HTML tags which are
        /// targeted in Cross-site scripting (XSS) attacks.  This function will evolve
        /// to provide more robust checking as additional holes are found.
        /// </summary>
        /// <param name="strInput">This is the string to be filtered.</param>
        /// <returns>Filtered UserInput.</returns>
        /// <remarks>
        /// This is a private function that is used internally by the <see cref="InputFilter"/> function.
        /// </remarks>
        private static string FilterStrings(string strInput)
        {
			Logger.LogDebug(">>> Entering FilterStrings(strInput={StrInput})", strInput);
            // setup up list of search terms as items may be used twice
            var tempInput = strInput;
			Logger.LogDebug("FilterStrings(strInput={StrInput}): tempInput → {TempInput}", strInput, tempInput);
            if (string.IsNullOrWhiteSpace(tempInput))
            {
                return tempInput;
            }

            const string replacement = " ";

            // remove the js event from html tags
            var tagMatches = DangerElementsRegex.Matches(tempInput);
			Logger.LogDebug("FilterStrings(strInput={StrInput}): tagMatches → {TagMatches}", strInput, tagMatches);
            foreach (Match match in tagMatches)
            {
                var tagContent = match.Value;
                var cleanTagContent = DangerElementContentRegex.Replace(tagContent, string.Empty);
				Logger.LogDebug("FilterStrings(strInput={StrInput}): cleanTagContent → {CleanTagContent}", strInput, cleanTagContent);
                tempInput = tempInput.Replace(tagContent, cleanTagContent);
            }

            // check if text contains encoded angle brackets, if it does it we decode it to check the plain text
            if (tempInput.Contains("&gt;") || tempInput.Contains("&lt;") || tempInput.Contains("&#"))
            {
                // text is encoded, so decode and try again
                tempInput = HttpUtility.HtmlDecode(tempInput);
                tempInput = RxListStrings.Aggregate(tempInput, (current, s) => s.Replace(current, replacement));

                // Re-encode
                tempInput = HttpUtility.HtmlEncode(tempInput);
            }
            else
            {
                tempInput = RxListStrings.Aggregate(tempInput, (current, s) => s.Replace(current, replacement));
            }

			Logger.LogDebug("<<< Exiting FilterStrings(strInput={StrInput})", strInput);
            return tempInput;
        }

        /// <summary>This filter removes angle brackets i.e.</summary>
        /// <param name="strInput">This is the string to be filtered.</param>
        /// <returns>Filtered UserInput.</returns>
        /// <remarks>This is a private function that is used internally by the <see cref="InputFilter"/> function.</remarks>
        private static string FormatAngleBrackets(string strInput)
        {
			Logger.LogDebug(">>> Entering FormatAngleBrackets(strInput={StrInput})", strInput);
            var tempInput = new StringBuilder(strInput).Replace("<", string.Empty).Replace(">", string.Empty);
			Logger.LogDebug("FormatAngleBrackets(strInput={StrInput}): tempInput → {TempInput}", strInput, tempInput);
			Logger.LogDebug("<<< Exiting FormatAngleBrackets(strInput={StrInput})", strInput);
            return tempInput.ToString();
        }

        /// <summary>This filter removes CrLf characters and inserts br.</summary>
        /// <param name="strInput">This is the string to be filtered.</param>
        /// <returns>Filtered UserInput.</returns>
        /// <remarks>This is a private function that is used internally by the <see cref="InputFilter"/> function.</remarks>
        private static string FormatMultiLine(string strInput)
        {
			Logger.LogDebug(">>> Entering FormatMultiLine(strInput={StrInput})", strInput);
            const string lbreak = "<br />";
            var tempInput = new StringBuilder(strInput).Replace("\r\n", lbreak).Replace("\n", lbreak).Replace("\r", lbreak);
			Logger.LogDebug("FormatMultiLine(strInput={StrInput}): tempInput → {TempInput}", strInput, tempInput);
			Logger.LogDebug("<<< Exiting FormatMultiLine(strInput={StrInput})", strInput);
            return tempInput.ToString();
        }

        /// <summary>This function verifies raw SQL statements to prevent SQL injection attacks and replaces a similar function (PreventSQLInjection) from the Common.Globals.vb module.</summary>
        /// <param name="strSQL">This is the string to be filtered.</param>
        /// <returns>Filtered UserInput.</returns>
        /// <remarks>This is a private function that is used internally by the <see cref="InputFilter"/> function.</remarks>
        private static string FormatRemoveSQL(string strSQL)
        {
			Logger.LogDebug(">>> Entering FormatRemoveSQL(strSQL={StrSQL})", strSQL);
            // Check for forbidden T-SQL commands. Use word boundaries to filter only real statements.
			Logger.LogDebug("<<< Exiting FormatRemoveSQL(strSQL={StrSQL})", strSQL);
            return BadStatementRegex.Replace(strSQL, " ").Replace("'", "''");
        }

        /// <summary>This function removes control characters from the string.</summary>
        /// <param name="str">This is the string to be filtered.</param>
        /// <returns>Filtered UserInput.</returns>
        /// <remarks>This is a private function that is used internally by the <see cref="InputFilter"/> function.</remarks>
        private static string FormatRemoveControlCharacters(string str)
        {
			Logger.LogDebug(">>> Entering FormatRemoveControlCharacters(str={Str})", str);
			Logger.LogDebug("<<< Exiting FormatRemoveControlCharacters(str={Str})", str);
            return ControlCharactersRegex.Replace(ControlCharacterToWhitespaceRegex.Replace(str, " "), string.Empty);
        }

        /// <summary>This function determines if the Input string contains any markup.</summary>
        /// <param name="strInput">This is the string to be checked.</param>
        /// <returns>True if string contains Markup tag(s).</returns>
        /// <remarks>This is a private function that is used internally by the <see cref="InputFilter"/> function.</remarks>
        private static bool IncludesMarkup(string strInput)
        {
			Logger.LogDebug(">>> Entering IncludesMarkup(strInput={StrInput})", strInput);
			Logger.LogDebug("<<< Exiting IncludesMarkup(strInput={StrInput})", strInput);
            return StripTagsRegex.IsMatch(strInput);
        }

        private static void InvalidateAspNetSession(HttpContext context)
        {
			Logger.LogDebug(">>> Entering InvalidateAspNetSession(context={Context})", context);
            if (context.Session != null && !context.Session.IsNewSession)
            {
                // invalidate existing session so a new one is created
                context.Session.Clear();
                context.Session.Abandon();
            }
			Logger.LogDebug("<<< Exiting InvalidateAspNetSession(context={Context})", context);
        }
    }
}
