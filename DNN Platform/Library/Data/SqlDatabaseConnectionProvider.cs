// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Data
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    using DotNetNuke.Data.PetaPoco;

    public class SqlDatabaseConnectionProvider : DatabaseConnectionProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SqlDatabaseConnectionProvider));
        /// <inheritdoc />
        public override int ExecuteNonQuery(string connectionString, CommandType commandType, int commandTimeout, string query)
        {
			Logger.LogDebug(">>> Entering ExecuteNonQuery(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},query={Query})", connectionString, commandType, commandTimeout);
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                if (commandTimeout > 0)
                {
                    command.CommandTimeout = commandTimeout;
                }

                connection.Open();
                try
                {
					var __logStart = System.Diagnostics.Stopwatch.StartNew();
					Logger.LogInformation("ExecuteNonQuery(connectionString,commandType,commandTimeout)={ConnectionString},{CommandType},{CommandTimeout}: database query executed in {Arg3} ms", connectionString, commandType, commandTimeout, __logStart.ElapsedMilliseconds);
					Logger.LogDebug("<<< Exiting ExecuteNonQuery(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},query={Query})", connectionString, commandType, commandTimeout);
                    return command.ExecuteNonQuery();
                }
                finally
                {
                    connection.Close();
                }
			Logger.LogDebug("<<< Exiting ExecuteNonQuery(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedure={Procedure},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedure, commandParameters);
            }
        }

        /// <inheritdoc />
        public override void ExecuteNonQuery(string connectionString, CommandType commandType, int commandTimeout, string procedure, object[] commandParameters)
        {
            PetaPocoHelper.ExecuteNonQuery(connectionString, commandType, commandTimeout, procedure, commandParameters);
        }

        /// <inheritdoc />
        public override IDataReader ExecuteSql(string connectionString, CommandType commandType, int commandTimeout, string query)
        {
			Logger.LogDebug(">>> Entering ExecuteSql(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},query={Query})", connectionString, commandType, commandTimeout);
            ////return SqlHelper.ExecuteReader(ConnectionString, CommandType, Query);
			Logger.LogDebug("<<< Exiting ExecuteSql(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},query={Query})", connectionString, commandType, commandTimeout);
            return PetaPocoHelper.ExecuteReader(connectionString, commandType, commandTimeout, query);
        }

        /// <inheritdoc />
        public override IDataReader ExecuteReader(string connectionString, CommandType commandType, int commandTimeout, string procedureName, params object[] commandParameters)
        {
			Logger.LogDebug(">>> Entering ExecuteReader(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedureName={ProcedureName},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedureName, commandParameters);
			Logger.LogDebug("<<< Exiting ExecuteReader(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedureName={ProcedureName},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedureName, commandParameters);
            return PetaPocoHelper.ExecuteReader(connectionString, commandType, commandTimeout, procedureName, commandParameters);
        }

        /// <inheritdoc />
        public override T ExecuteScalar<T>(string connectionString, CommandType commandType, int commandTimeout, string procedureName, params object[] commandParameters)
        {
			Logger.LogDebug(">>> Entering ExecuteScalar(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedureName={ProcedureName},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedureName, commandParameters);
			Logger.LogDebug("<<< Exiting ExecuteScalar(connectionString={ConnectionString},commandType={CommandType},commandTimeout={CommandTimeout},procedureName={ProcedureName},commandParameters={CommandParameters})", connectionString, commandType, commandTimeout, procedureName, commandParameters);
            return PetaPocoHelper.ExecuteScalar<T>(connectionString, commandType, commandTimeout, procedureName, commandParameters);
        }

        /// <inheritdoc />
        public override void BulkInsert(string connectionString, int commandTimeout, string procedureName, string tableParameterName, DataTable dataTable)
        {
			Logger.LogDebug(">>> Entering BulkInsert(connectionString={ConnectionString},commandTimeout={CommandTimeout},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", connectionString, commandTimeout, procedureName, tableParameterName, dataTable);
            PetaPocoHelper.BulkInsert(connectionString, commandTimeout, procedureName, tableParameterName, dataTable);
			Logger.LogDebug("<<< Exiting BulkInsert(connectionString={ConnectionString},commandTimeout={CommandTimeout},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", connectionString, commandTimeout, procedureName, tableParameterName, dataTable);
        }

        /// <inheritdoc />
        public override void BulkInsert(string connectionString, int commandTimeout, string procedureName, string tableParameterName, DataTable dataTable, Dictionary<string, object> commandParameters)
        {
			Logger.LogDebug(">>> Entering BulkInsert(connectionString={ConnectionString},commandTimeout={CommandTimeout},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},commandParameters={CommandParameters})", connectionString, commandTimeout, procedureName, tableParameterName, dataTable, commandParameters);
            PetaPocoHelper.BulkInsert(connectionString, procedureName, tableParameterName, dataTable, commandTimeout, commandParameters);
			Logger.LogDebug("<<< Exiting BulkInsert(connectionString={ConnectionString},commandTimeout={CommandTimeout},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},commandParameters={CommandParameters})", connectionString, commandTimeout, procedureName, tableParameterName, dataTable, commandParameters);
        }
    }
}
