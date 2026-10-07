// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using DotNetNuke.Common.Utilities;
	using Microsoft.Extensions.Logging;

    /// <summary>Implementation of <see cref="IDataCache"/> that relies on the <see cref="DataCache"/> class.</summary>
    internal sealed class DataCacheShim : IDataCache
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <inheritdoc />
        public void ClearCache()
        {
			_logger.LogError(">>> Entering ClearCache()");
            DataCache.ClearCache();
			_logger.LogWarning("<<< Exiting ClearCache()");
        }
    }
}
