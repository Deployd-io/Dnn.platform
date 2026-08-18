// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Common
{
    using System.Collections.Generic;

    internal class ContentItemEqualityComparer : IEqualityComparer<ContentItem>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ContentItemEqualityComparer));
        /// <inheritdoc />
        public bool Equals(ContentItem x, ContentItem y)
        {
			Logger.LogDebug(">>> Entering Equals(x={X},y={Y})", x, y);
			Logger.LogDebug("<<< Exiting Equals(x={X},y={Y})", x, y);
            return x.ContentItemId == y.ContentItemId;
        }

        /// <inheritdoc />
        public int GetHashCode(ContentItem obj)
        {
			Logger.LogDebug(">>> Entering GetHashCode(obj={Obj})", obj);
			Logger.LogDebug("<<< Exiting GetHashCode(obj={Obj})", obj);
            return obj.ContentItemId.GetHashCode();
        }
    }
}
