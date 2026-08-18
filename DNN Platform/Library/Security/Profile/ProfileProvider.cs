// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Profile
{
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Users;

    public abstract class ProfileProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ProfileProvider));
        public abstract bool CanEditProviderProperties { get; }

        // return the provider
        public static ProfileProvider Instance()
        {
			Logger.LogDebug(">>> Entering Instance()");
			Logger.LogDebug("<<< Exiting Instance()");
            return ComponentFactory.GetComponent<ProfileProvider>();
        }

        public abstract void GetUserProfile(ref UserInfo user);

        public abstract void UpdateUserProfile(UserInfo user);
    }
}
