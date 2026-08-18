// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using System.Data;

    using DotNetNuke.Data;
	using Microsoft.Extensions.Logging;

    /// <summary>An implementation of <see cref="IDataProvider"/> that relies on the <see cref="DataProvider"/> class.</summary>
    internal sealed class DataProviderShim : IDataProvider
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <inheritdoc />
        public IDataReader ExecuteSQL(string sql)
        {
			_logger.LogDebug(">>> Entering ExecuteSQL(sql={Sql})", sql);
			_logger.LogDebug("<<< Exiting ExecuteSQL(sql={Sql})", sql);
            return DataProvider.Instance().ExecuteSQL(sql);
        }
    }
}
