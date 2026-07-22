// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Roles
{
    using System.Collections;

    /// <summary>
    /// The RoleComparer class provides an implementation of <see cref="IComparer"/> for
    /// <see cref="RoleInfo"/> objects.
    /// </summary>
    public class RoleComparer : IComparer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Compares two <see cref="RoleInfo"/> objects by performing a comparison of their rolenames.</summary>
        /// <param name="x">One of the <see cref="RoleInfo"/> instances to compare.</param>
        /// <param name="y">The other <see cref="RoleInfo"/> instance to compare.</param>
        /// <returns>An Integer that determines whether x is greater, smaller or equal to y. </returns>
        public int Compare(object x, object y)
        {
			Logger.LogDebug(">>> Entering Compare(x={X},y={Y})", x, y);
			Logger.LogDebug("<<< Exiting Compare(x={X},y={Y})", x, y);
            return new CaseInsensitiveComparer().Compare(((RoleInfo)x).RoleName, ((RoleInfo)y).RoleName);
        }
    }
}
