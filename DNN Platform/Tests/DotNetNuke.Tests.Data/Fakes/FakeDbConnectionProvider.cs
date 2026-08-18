// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data.Fakes
{
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.Data;
	using Microsoft.Extensions.Logging;

    internal class FakeDbConnectionProvider : DatabaseConnectionProvider
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public override int ExecuteNonQuery(string connectionString, CommandType commandType, int commandTimeout, string query)
        {
			_logger.LogDebug(">>> Entering ExecuteNonQuery(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},query={Query})", connectionString, commandType, commandTimeout);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteNonQuery(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},query={Query})", connectionString, commandType, commandTimeout);
			_logger.LogDebug("<<< Exiting ExecuteNonQuery(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},query={Query})", connectionString, commandType, commandTimeout);
            throw new System.NotImplementedException();
        }

        public override void ExecuteNonQuery(string connectionString, CommandType commandType, int commandTimeout, string procedure, object[] commandParameters)
        {
			_logger.LogDebug(">>> Entering ExecuteNonQuery(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedure={Procedure},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedure, commandParameters);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteNonQuery(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedure={Procedure},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedure, commandParameters);
			_logger.LogDebug("<<< Exiting ExecuteNonQuery(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedure={Procedure},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedure, commandParameters);
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSql(string connectionString, CommandType commandType, int commandTimeout, string query)
        {
			_logger.LogDebug(">>> Entering ExecuteSql(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},query={Query})", connectionString, commandType, commandTimeout);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteSql(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},query={Query})", connectionString, commandType, commandTimeout);
			_logger.LogDebug("<<< Exiting ExecuteSql(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},query={Query})", connectionString, commandType, commandTimeout);
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteReader(string connectionString, CommandType commandType, int commandTimeout, string procedureName,
            params object[] commandParameters)
        {
			_logger.LogDebug(">>> Entering ExecuteReader(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedureName={ProcedureName},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedureName, commandParameters);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteReader(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedureName={ProcedureName},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedureName, commandParameters);
			_logger.LogDebug("<<< Exiting ExecuteReader(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedureName={ProcedureName},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedureName, commandParameters);
            throw new System.NotImplementedException();
        }

        public override T ExecuteScalar<T>(string connectionString, CommandType commandType, int commandTimeout, string procedureName, params object[] commandParameters)
        {
			_logger.LogDebug(">>> Entering ExecuteScalar(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedureName={ProcedureName},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedureName, commandParameters);
			_logger.LogDebug("<<< Exiting ExecuteScalar(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedureName={ProcedureName},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedureName, commandParameters);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteScalar(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedureName={ProcedureName},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedureName, commandParameters);
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string connectionString, int commandTimeout, string procedureName, string tableParameterName, DataTable dataTable)
        {
			_logger.LogDebug(">>> Entering BulkInsert(connectionString={ConnectionString},commandTimeout={CommandTimeout},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", connectionString, commandTimeout, procedureName, tableParameterName, dataTable);
			_logger.LogWarning("Throwing NotImplementedException from BulkInsert(connectionString={ConnectionString},commandTimeout={CommandTimeout},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", connectionString, commandTimeout, procedureName, tableParameterName, dataTable);
			_logger.LogDebug("<<< Exiting BulkInsert(connectionString={ConnectionString},commandTimeout={CommandTimeout},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", connectionString, commandTimeout, procedureName, tableParameterName, dataTable);
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string connectionString, int commandTimeout, string procedureName, string tableParameterName, DataTable dataTable,
            Dictionary<string, object> commandParameters)
        {
			_logger.LogDebug(">>> Entering BulkInsert(connectionString={ConnectionString},commandTimeout={CommandTimeout},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},commandParameters={CommandParameters})", connectionString, commandTimeout, procedureName, tableParameterName, dataTable, commandParameters);
			_logger.LogWarning("Throwing NotImplementedException from BulkInsert(connectionString={ConnectionString},commandTimeout={CommandTimeout},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},commandParameters={CommandParameters})", connectionString, commandTimeout, procedureName, tableParameterName, dataTable, commandParameters);
			_logger.LogDebug("<<< Exiting BulkInsert(connectionString={ConnectionString},commandTimeout={CommandTimeout},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},commandParameters={CommandParameters})", connectionString, commandTimeout, procedureName, tableParameterName, dataTable, commandParameters);
            throw new System.NotImplementedException();
        }
    }
}
