// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Permissions
{
    public class CorePermissionProvider : PermissionProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public override bool SupportsFullControl()
        {
			Logger.LogDebug(">>> Entering SupportsFullControl()");
			Logger.LogDebug("<<< Exiting SupportsFullControl()");
            return false;
        }
    }
}
