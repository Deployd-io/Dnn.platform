// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal.Components
{
    using System.Globalization;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Tokens;

    public class ProfilePicPropertyAccess : IPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly int userId;

        /// <summary>Initializes a new instance of the <see cref="ProfilePicPropertyAccess"/> class.</summary>
        /// <param name="userId">The user ID.</param>
        public ProfilePicPropertyAccess(int userId)
        {
			Logger.LogDebug(">>> Entering ProfilePicPropertyAccess(userId={UserId})", userId);
            this.userId = userId;
			Logger.LogDebug("<<< Exiting ProfilePicPropertyAccess(userId={UserId})", userId);
			Logger.LogDebug("ProfilePicPropertyAccess(userId={UserId}): userId → {UserId1}", userId, userId);
			Logger.LogDebug("ProfilePicPropertyAccess(userId={UserId}): this.userId → {Arg1}", userId, this.userId);
        }

        /// <inheritdoc />
        public CacheLevel Cacheability => CacheLevel.notCacheable;

        public int Size { get; set; } = 32;

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {
			Logger.LogDebug(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound);
            if (propertyName.ToLowerInvariant() == "relativeurl")
            {
                int size;
                if (int.TryParse(format, out size))
                {
                    this.Size = size;
					Logger.LogDebug("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound}): this.Size → {Arg6}", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound, this.Size);
                }

                return UserController.Instance.GetUserProfilePictureUrl(this.userId, this.Size, this.Size);
            }

            propertyNotFound = true;
			Logger.LogDebug("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound);
            return string.Empty;
        }
    }
}
