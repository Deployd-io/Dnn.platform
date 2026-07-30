// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Prompt.Models
{
    using System.Diagnostics.CodeAnalysis;

    using Dnn.PersonaBar.Library.Prompt.Common;
    using DotNetNuke.Entities.Users;

    public class UserModelBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        // provide a default field order for use of callers
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public static readonly string[] FieldOrder =
        [
            "UserId",
            "Username",
            "Email",
            "LastLogin",
            "IsDeleted",
            "IsAuthorized",
            "IsLockedOut"
        ];

        public UserModelBase()
        {
		Logger.LogDebug(">>> Entering UserModelBase()");
		Logger.LogDebug("<<< Exiting UserModelBase()");
        }

        public UserModelBase(UserInfo userInfo)
        {
			Logger.LogDebug(">>> Entering UserModelBase(userInfo={UserInfo})", userInfo);
            this.Email = userInfo.Email;
            this.LastLogin = userInfo.Membership.LastLoginDate.ToPromptShortDateString();
			Logger.LogDebug("UserModelBase(userInfo={UserInfo}): this.Email → {Arg1}", userInfo, this.Email);
            this.UserId = userInfo.UserID;
            this.Username = userInfo.Username;
			Logger.LogDebug("UserModelBase(userInfo={UserInfo}): this.UserId → {Arg1}", userInfo, this.UserId);
			Logger.LogDebug("UserModelBase(userInfo={UserInfo}): this.Username → {Arg1}", userInfo, this.Username);
            this.IsDeleted = userInfo.IsDeleted;
            this.IsAuthorized = userInfo.Membership.Approved;
            this.IsLockedOut = userInfo.Membership.LockedOut;
			Logger.LogDebug("<<< Exiting UserModelBase(userInfo={UserInfo})", userInfo);
			Logger.LogDebug("UserModelBase(userInfo={UserInfo}): this.IsDeleted → {Arg1}", userInfo, this.IsDeleted);
			Logger.LogDebug("UserModelBase(userInfo={UserInfo}): this.IsAuthorized → {Arg1}", userInfo, this.IsAuthorized);
			Logger.LogDebug("UserModelBase(userInfo={UserInfo}): this.IsLockedOut → {Arg1}", userInfo, this.IsLockedOut);
        }

        // ReSharper disable InconsistentNaming
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        public string __Email => $"get-user '{this.Email}'";

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        public string __UserId => $"get-user {this.UserId}";

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        public string __Username => $"get-user '{this.Username}'";

        // ReSharper restore InconsistentNaming
        public int UserId { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string LastLogin { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsAuthorized { get; set; }

        public bool IsLockedOut { get; set; }
    }
}
