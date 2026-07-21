// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints.Filters
{
    public class NoFilter : IExtensionPointFilter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public bool Condition(IExtensionPointData m)
        {
			Logger.LogDebug(">>> Entering Condition(m={M})", m);
			Logger.LogDebug("<<< Exiting Condition(m={M})", m);
            return true;
        }
    }
}
