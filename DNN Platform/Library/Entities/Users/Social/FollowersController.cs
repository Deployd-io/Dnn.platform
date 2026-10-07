// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users.Social
{
    using System;

    using DotNetNuke.Entities.Users.Social.Internal;
    using DotNetNuke.Framework;

    public class FollowersController : ServiceLocator<IFollowersController, FollowersController>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FollowersController));
        /// <inheritdoc />
        protected override Func<IFollowersController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new FollowersControllerImpl();
        }
    }
}
