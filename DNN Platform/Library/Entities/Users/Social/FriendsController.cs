// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users.Social
{
    using System;

    using DotNetNuke.Entities.Users.Social.Internal;
    using DotNetNuke.Framework;

    public class FriendsController : ServiceLocator<IFriendsController, FriendsController>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        protected override Func<IFriendsController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new FriendsControllerImpl();
        }
    }
}
