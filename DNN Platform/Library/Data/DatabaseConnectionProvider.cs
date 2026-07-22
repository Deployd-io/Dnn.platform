// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Data
{
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.ComponentModel;

    public abstract class DatabaseConnectionProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static DatabaseConnectionProvider Instance()
        {
			Logger.LogDebug(">>> Entering Instance()");
			Logger.LogDebug("<<< Exiting Instance()");
            return ComponentFactory.GetComponent<DatabaseConnectionProvider>();
        }

        public abstract int ExecuteNonQuery(string connectionString, CommandType commandType, int commandTimeout, string query);

        public abstract void ExecuteNonQuery(string connectionString, CommandType commandType, int commandTimeout, string procedure, object[] commandParameters);

        public abstract IDataReader ExecuteSql(string connectionString, CommandType commandType, int commandTimeout, string query);

        public abstract IDataReader ExecuteReader(string connectionString, CommandType commandType, int commandTimeout, string procedureName, params object[] commandParameters);

        public abstract T ExecuteScalar<T>(string connectionString, CommandType commandType, int commandTimeout, string procedureName, params object[] commandParameters);

        public abstract void BulkInsert(string connectionString, int commandTimeout, string procedureName, string tableParameterName, DataTable dataTable);

        public abstract void BulkInsert(string connectionString, int commandTimeout, string procedureName, string tableParameterName, DataTable dataTable, Dictionary<string, object> commandParameters);
    }
}
