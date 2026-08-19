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
		eventLogger.LogDebug(">>> Entering UserModelBase()");
		eventLogger.LogDebug("<<< Exiting UserModelBase()");
        }

        public UserModelBase(UserInfo userInfo)
        {
			eventLogger.LogDebug(">>> Entering UserModelBase(userInfo={UserInfo})", userInfo);
            this.Email = userInfo.Email;
            this.LastLogin = userInfo.Membership.LastLoginDate.ToPromptShortDateString();
			eventLogger.LogDebug("UserModelBase(userInfo={UserInfo}): this.Email → {Arg1}", userInfo, this.Email);
            this.UserId = userInfo.UserID;
			eventLogger.LogDebug("UserModelBase(userInfo={UserInfo}): this.UserId → {Arg1}", userInfo, this.UserId);
            this.Username = userInfo.Username;
            this.IsDeleted = userInfo.IsDeleted;
            this.IsAuthorized = userInfo.Membership.Approved;
			eventLogger.LogDebug("UserModelBase(userInfo={UserInfo}): this.Username → {Arg1}", userInfo, this.Username);
            this.IsLockedOut = userInfo.Membership.LockedOut;
			eventLogger.LogDebug("<<< Exiting UserModelBase(userInfo={UserInfo})", userInfo);
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
