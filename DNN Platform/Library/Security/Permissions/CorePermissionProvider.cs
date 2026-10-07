// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Permissions
{
    public class CorePermissionProvider : PermissionProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CorePermissionProvider));
        /// <inheritdoc />
        public override bool SupportsFullControl()
        {
			Logger.LogError(">>> Entering SupportsFullControl()");
			Logger.LogWarning("<<< Exiting SupportsFullControl()");
            return false;
        }
    }
}
