// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System.Collections;

    /// <summary>Provides a custom <see cref="IComparer"/> implementation for <see cref="FolderPermissionInfo"/> objects.</summary>
    internal class CompareFolderPermissions : IComparer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CompareFolderPermissions));
        /// <inheritdoc />
        public int Compare(object x, object y)
        {
			Logger.LogDebug(">>> Entering Compare(x={X},y={Y})", x, y);
			Logger.LogDebug("<<< Exiting Compare(x={X},y={Y})", x, y);
            return ((FolderPermissionInfo)x).FolderPermissionID.CompareTo(((FolderPermissionInfo)y).FolderPermissionID);
        }
    }
}
