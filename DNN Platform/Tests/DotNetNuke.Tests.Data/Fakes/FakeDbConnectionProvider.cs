// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data.Fakes
{
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.Data;

    internal class FakeDbConnectionProvider : DatabaseConnectionProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public override int ExecuteNonQuery(string connectionString, CommandType commandType, int commandTimeout, string query)
        {
			Logger.LogDebug(">>> Entering ExecuteNonQuery(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},query={Query})", connectionString, commandType, commandTimeout);
            throw new System.NotImplementedException();
        }

        public override void ExecuteNonQuery(string connectionString, CommandType commandType, int commandTimeout, string procedure, object[] commandParameters)
        {
			Logger.LogDebug(">>> Entering ExecuteNonQuery(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedure={Procedure},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedure, commandParameters);
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSql(string connectionString, CommandType commandType, int commandTimeout, string query)
        {
			Logger.LogDebug(">>> Entering ExecuteSql(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},query={Query})", connectionString, commandType, commandTimeout);
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteReader(string connectionString, CommandType commandType, int commandTimeout, string procedureName,
            params object[] commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override T ExecuteScalar<T>(string connectionString, CommandType commandType, int commandTimeout, string procedureName, params object[] commandParameters)
        {
			Logger.LogDebug(">>> Entering ExecuteScalar(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedureName={ProcedureName},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedureName, commandParameters);
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string connectionString, int commandTimeout, string procedureName, string tableParameterName, DataTable dataTable)
        {
			Logger.LogDebug(">>> Entering BulkInsert(connectionString={ConnectionString},commandTimeout={CommandTimeout},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", connectionString, commandTimeout, procedureName, tableParameterName, dataTable);
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string connectionString, int commandTimeout, string procedureName, string tableParameterName, DataTable dataTable,
            Dictionary<string, object> commandParameters)
        {
            throw new System.NotImplementedException();
        }
    }
}
