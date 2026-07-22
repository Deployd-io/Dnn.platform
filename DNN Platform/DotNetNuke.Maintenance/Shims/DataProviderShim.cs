// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using System.Data;

    using DotNetNuke.Data;

    /// <summary>An implementation of <see cref="IDataProvider"/> that relies on the <see cref="DataProvider"/> class.</summary>
    internal sealed class DataProviderShim : IDataProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public IDataReader ExecuteSQL(string sql)
        {
			Logger.LogDebug(">>> Entering ExecuteSQL(sql={Sql})", sql);
			Logger.LogDebug("<<< Exiting ExecuteSQL(sql={Sql})", sql);
            return DataProvider.Instance().ExecuteSQL(sql);
        }
    }
}
