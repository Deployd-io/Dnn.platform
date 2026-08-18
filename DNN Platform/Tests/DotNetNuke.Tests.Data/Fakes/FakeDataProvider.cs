// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data.Fakes
{
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Data;

    using Moq;
	using Microsoft.Extensions.Logging;

    internal class FakeDataProvider : DataProvider
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public FakeDataProvider(Dictionary<string, string> settings)
            : base(Mock.Of<IApplicationStatusInfo>())
        {
			_logger.LogDebug(">>> Entering FakeDataProvider(settings={Settings})", settings);
            this.Settings = settings;
			_logger.LogDebug("FakeDataProvider(settings={Settings}): this.Settings → {Arg1}", settings, this.Settings);
			_logger.LogDebug("<<< Exiting FakeDataProvider(settings={Settings})", settings);
        }

        public override bool IsConnectionValid
        {
            get { throw new System.NotImplementedException(); }
        }

        public override Dictionary<string, string> Settings { get; }

        public override void ExecuteNonQuery(string procedureName, params object[] commandParameters)
        {
			_logger.LogDebug(">>> Entering ExecuteNonQuery(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteNonQuery(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
			_logger.LogDebug("<<< Exiting ExecuteNonQuery(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
            throw new System.NotImplementedException();
        }

        public override void ExecuteNonQuery(int timeoutSec, string procedureName, params object[] commandParameters)
        {
			_logger.LogDebug(">>> Entering ExecuteNonQuery(timeoutSec={TimeoutSec},procedureName={ProcedureName},commandParameters={CommandParameters})", timeoutSec, procedureName, commandParameters);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteNonQuery(timeoutSec={TimeoutSec},procedureName={ProcedureName},commandParameters={CommandParameters})", timeoutSec, procedureName, commandParameters);
			_logger.LogDebug("<<< Exiting ExecuteNonQuery(timeoutSec={TimeoutSec},procedureName={ProcedureName},commandParameters={CommandParameters})", timeoutSec, procedureName, commandParameters);
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable)
        {
			_logger.LogDebug(">>> Entering BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", procedureName, tableParameterName, dataTable);
			_logger.LogWarning("Throwing NotImplementedException from BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", procedureName, tableParameterName, dataTable);
			_logger.LogDebug("<<< Exiting BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", procedureName, tableParameterName, dataTable);
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec)
        {
			_logger.LogDebug(">>> Entering BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},timeoutSec={TimeoutSec})", procedureName, tableParameterName, dataTable, timeoutSec);
			_logger.LogWarning("Throwing NotImplementedException from BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},timeoutSec={TimeoutSec})", procedureName, tableParameterName, dataTable, timeoutSec);
			_logger.LogDebug("<<< Exiting BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},timeoutSec={TimeoutSec})", procedureName, tableParameterName, dataTable, timeoutSec);
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, Dictionary<string, object> commandParameters)
        {
			_logger.LogDebug(">>> Entering BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},commandParameters={CommandParameters})", procedureName, tableParameterName, dataTable, commandParameters);
			_logger.LogWarning("Throwing NotImplementedException from BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},commandParameters={CommandParameters})", procedureName, tableParameterName, dataTable, commandParameters);
			_logger.LogDebug("<<< Exiting BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},commandParameters={CommandParameters})", procedureName, tableParameterName, dataTable, commandParameters);
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec,
            Dictionary<string, object> commandParameters)
        {
			_logger.LogDebug(">>> Entering BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},timeoutSec={TimeoutSec},commandParameters={CommandParameters})", procedureName, tableParameterName, dataTable, timeoutSec, commandParameters);
			_logger.LogWarning("Throwing NotImplementedException from BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},timeoutSec={TimeoutSec},commandParameters={CommandParameters})", procedureName, tableParameterName, dataTable, timeoutSec, commandParameters);
			_logger.LogDebug("<<< Exiting BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},timeoutSec={TimeoutSec},commandParameters={CommandParameters})", procedureName, tableParameterName, dataTable, timeoutSec, commandParameters);
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteReader(string procedureName, params object[] commandParameters)
        {
			_logger.LogDebug(">>> Entering ExecuteReader(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteReader(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
			_logger.LogDebug("<<< Exiting ExecuteReader(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteReader(int timeoutSec, string procedureName, params object[] commandParameters)
        {
			_logger.LogDebug(">>> Entering ExecuteReader(timeoutSec={TimeoutSec},procedureName={ProcedureName},commandParameters={CommandParameters})", timeoutSec, procedureName, commandParameters);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteReader(timeoutSec={TimeoutSec},procedureName={ProcedureName},commandParameters={CommandParameters})", timeoutSec, procedureName, commandParameters);
			_logger.LogDebug("<<< Exiting ExecuteReader(timeoutSec={TimeoutSec},procedureName={ProcedureName},commandParameters={CommandParameters})", timeoutSec, procedureName, commandParameters);
            throw new System.NotImplementedException();
        }

        public override T ExecuteScalar<T>(string procedureName, params object[] commandParameters)
        {
			_logger.LogDebug(">>> Entering ExecuteScalar(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
			_logger.LogDebug("<<< Exiting ExecuteScalar(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteScalar(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
            throw new System.NotImplementedException();
        }

        public override T ExecuteScalar<T>(int timeoutSec, string procedureName, params object[] commandParameters)
        {
			_logger.LogWarning("Throwing NotImplementedException from ExecuteScalar(timeoutSec={TimeoutSec},procedureName={ProcedureName},commandParameters={CommandParameters})", timeoutSec, procedureName, commandParameters);
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQL(string sql)
        {
			_logger.LogDebug(">>> Entering ExecuteSQL(sql={Sql})", sql);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteSQL(sql={Sql})", sql);
			_logger.LogDebug("<<< Exiting ExecuteSQL(sql={Sql})", sql);
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQL(string sql, int timeoutSec)
        {
			_logger.LogDebug(">>> Entering ExecuteSQL(sql={Sql},timeoutSec={TimeoutSec})", sql, timeoutSec);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteSQL(sql={Sql},timeoutSec={TimeoutSec})", sql, timeoutSec);
			_logger.LogDebug("<<< Exiting ExecuteSQL(sql={Sql},timeoutSec={TimeoutSec})", sql, timeoutSec);
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string script)
        {
			_logger.LogDebug(">>> Entering ExecuteScript(script={Script})", script);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteScript(script={Script})", script);
			_logger.LogDebug("<<< Exiting ExecuteScript(script={Script})", script);
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string script, int timeoutSec)
        {
			_logger.LogDebug(">>> Entering ExecuteScript(script={Script},timeoutSec={TimeoutSec})", script, timeoutSec);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteScript(script={Script},timeoutSec={TimeoutSec})", script, timeoutSec);
			_logger.LogDebug("<<< Exiting ExecuteScript(script={Script},timeoutSec={TimeoutSec})", script, timeoutSec);
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string connectionString, string sql)
        {
			_logger.LogDebug(">>> Entering ExecuteScript(connectionString={ConnectionString},sql={Sql})", connectionString, sql);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteScript(connectionString={ConnectionString},sql={Sql})", connectionString, sql);
			_logger.LogDebug("<<< Exiting ExecuteScript(connectionString={ConnectionString},sql={Sql})", connectionString, sql);
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string connectionString, string sql, int timeoutSec)
        {
			_logger.LogDebug(">>> Entering ExecuteScript(connectionString={ConnectionString},sql={Sql},timeoutSec={TimeoutSec})", connectionString, sql, timeoutSec);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteScript(connectionString={ConnectionString},sql={Sql},timeoutSec={TimeoutSec})", connectionString, sql, timeoutSec);
			_logger.LogDebug("<<< Exiting ExecuteScript(connectionString={ConnectionString},sql={Sql},timeoutSec={TimeoutSec})", connectionString, sql, timeoutSec);
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql)
        {
			_logger.LogDebug("<<< Exiting ExecuteSQLTemp(connectionString={ConnectionString},sql={Sql})", connectionString, sql);
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec)
        {
			_logger.LogDebug(">>> Entering ExecuteSQLTemp(connectionString={ConnectionString},sql={Sql})", connectionString, sql);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteSQLTemp(connectionString={ConnectionString},sql={Sql})", connectionString, sql);
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, out string errorMessage)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec, out string errorMessage)
        {
			_logger.LogDebug(">>> Entering ExecuteSQLTemp(connectionString={ConnectionString},sql={Sql},timeoutSec={TimeoutSec})", connectionString, sql, timeoutSec);
			_logger.LogWarning("Throwing NotImplementedException from ExecuteSQLTemp(connectionString={ConnectionString},sql={Sql},timeoutSec={TimeoutSec})", connectionString, sql, timeoutSec);
			_logger.LogDebug("<<< Exiting ExecuteSQLTemp(connectionString={ConnectionString},sql={Sql},timeoutSec={TimeoutSec})", connectionString, sql, timeoutSec);
            throw new System.NotImplementedException();
        }
    }
}
