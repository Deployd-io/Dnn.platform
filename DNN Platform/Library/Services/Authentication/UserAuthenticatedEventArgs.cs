// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication
{
    using System;
    using System.Collections.Specialized;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Membership;

    /// <summary>
    /// The UserAuthenticatedEventArgs class provides a custom EventArgs object for the
    /// UserAuthenticated event.
    /// </summary>
    public class UserAuthenticatedEventArgs : EventArgs
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserAuthenticatedEventArgs));
        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuthenticatedEventArgs"/> class.
        /// All properties Constructor.
        /// </summary>
        /// <param name="user">The user being authenticated.</param>
        /// <param name="token">The user token.</param>
        /// <param name="status">The login status.</param>
        /// <param name="type">The type of Authentication.</param>
        public UserAuthenticatedEventArgs(UserInfo user, string token, UserLoginStatus status, string type)
        {
			Logger.LogError(">>> Entering UserAuthenticatedEventArgs(user={User},token=<redacted>,status={Status},type={Type})", user, status, type);
            this.Profile = new NameValueCollection();
            this.Message = string.Empty;
			Logger.LogDebug("UserAuthenticatedEventArgs(user={User},token=<redacted>,status={Status},type={Type}): this.Message → {Arg4}", user, status, type, this.Message);
            this.AutoRegister = false;
            this.Authenticated = true;
			Logger.LogDebug("UserAuthenticatedEventArgs(user={User},token=<redacted>,status={Status},type={Type}): this.AutoRegister → {Arg4}", user, status, type, this.AutoRegister);
			Logger.LogDebug("UserAuthenticatedEventArgs(user={User},token=<redacted>,status={Status},type={Type}): this.Authenticated → {Arg4}", user, status, type, this.Authenticated);
            this.User = user;
            this.LoginStatus = status;
            this.UserToken = token;
            this.AuthenticationType = type;
			Logger.LogDebug("UserAuthenticatedEventArgs(user={User},token=<redacted>,status={Status},type={Type}): this.User → {Arg4}", user, status, type, this.User);
        Logger.LogWarning("<<< Exiting UserAuthenticatedEventArgs(user={User},token=<redacted>,status={Status},type={Type})", user, status, type);
            this.RememberMe = false;
        }

        /// <summary>Gets or sets a value indicating whether the User was authenticated.</summary>
        public bool Authenticated { get; set; }

        /// <summary>Gets or sets the Authentication Type.</summary>
        public string AuthenticationType { get; set; }

        /// <summary>Gets or sets a value indicating whether the user should be automatically registered.</summary>
        public bool AutoRegister { get; set; }

        /// <summary>Gets or sets the Login Status.</summary>
        public UserLoginStatus LoginStatus { get; set; }

        /// <summary>Gets or sets the Message.</summary>
        public string Message { get; set; }

        /// <summary>Gets or sets the Profile.</summary>
        public NameValueCollection Profile { get; set; }

        /// <summary>Gets or sets a value indicating whether to remember the user.</summary>
        public bool RememberMe { get; set; }

        /// <summary>Gets or sets the User.</summary>
        public UserInfo User { get; set; }

        /// <summary>Gets or sets the UserToken (the userid or authenticated id).</summary>
        public string UserToken { get; set; }

        /// <summary>Gets or sets the Username.</summary>
        public string UserName { get; set; }
    }
}
