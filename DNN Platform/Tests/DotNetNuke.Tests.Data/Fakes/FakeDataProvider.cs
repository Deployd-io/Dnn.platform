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

    internal class FakeDataProvider : DataProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public FakeDataProvider(Dictionary<string, string> settings)
            : base(Mock.Of<IApplicationStatusInfo>())
        {
            this.Settings = settings;
        }

        public override bool IsConnectionValid
        {
            get { throw new System.NotImplementedException(); }
        }

        public override Dictionary<string, string> Settings { get; }

        public override void ExecuteNonQuery(string procedureName, params object[] commandParameters)
        {
			Logger.LogDebug(">>> Entering ExecuteNonQuery(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
            throw new System.NotImplementedException();
        }

        public override void ExecuteNonQuery(int timeoutSec, string procedureName, params object[] commandParameters)
        {
			Logger.LogDebug(">>> Entering ExecuteNonQuery(timeoutSec={TimeoutSec},procedureName={ProcedureName},commandParameters={CommandParameters})", timeoutSec, procedureName, commandParameters);
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable)
        {
			Logger.LogDebug(">>> Entering BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", procedureName, tableParameterName, dataTable);
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec)
        {
			Logger.LogDebug(">>> Entering BulkInsert(procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},timeoutSec={TimeoutSec})", procedureName, tableParameterName, dataTable, timeoutSec);
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, Dictionary<string, object> commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec,
            Dictionary<string, object> commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteReader(string procedureName, params object[] commandParameters)
        {
			Logger.LogDebug(">>> Entering ExecuteReader(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteReader(int timeoutSec, string procedureName, params object[] commandParameters)
        {
			Logger.LogDebug(">>> Entering ExecuteReader(timeoutSec={TimeoutSec},procedureName={ProcedureName},commandParameters={CommandParameters})", timeoutSec, procedureName, commandParameters);
            throw new System.NotImplementedException();
        }

        public override T ExecuteScalar<T>(string procedureName, params object[] commandParameters)
        {
			Logger.LogDebug(">>> Entering ExecuteScalar(procedureName={ProcedureName},commandParameters={CommandParameters})", procedureName, commandParameters);
            throw new System.NotImplementedException();
        }

        public override T ExecuteScalar<T>(int timeoutSec, string procedureName, params object[] commandParameters)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQL(string sql)
        {
			Logger.LogDebug(">>> Entering ExecuteSQL(sql={Sql})", sql);
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQL(string sql, int timeoutSec)
        {
			Logger.LogDebug(">>> Entering ExecuteSQL(sql={Sql},timeoutSec={TimeoutSec})", sql, timeoutSec);
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string script)
        {
			Logger.LogDebug(">>> Entering ExecuteScript(script={Script})", script);
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string script, int timeoutSec)
        {
			Logger.LogDebug(">>> Entering ExecuteScript(script={Script},timeoutSec={TimeoutSec})", script, timeoutSec);
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string connectionString, string sql)
        {
			Logger.LogDebug(">>> Entering ExecuteScript(connectionString={ConnectionString},sql={Sql})", connectionString, sql);
            throw new System.NotImplementedException();
        }

        public override string ExecuteScript(string connectionString, string sql, int timeoutSec)
        {
			Logger.LogDebug(">>> Entering ExecuteScript(connectionString={ConnectionString},sql={Sql},timeoutSec={TimeoutSec})", connectionString, sql, timeoutSec);
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec)
        {
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, out string errorMessage)
        {
			Logger.LogDebug(">>> Entering ExecuteSQLTemp(connectionString={ConnectionString},sql={Sql})", connectionString, sql);
            throw new System.NotImplementedException();
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec, out string errorMessage)
        {
			Logger.LogDebug(">>> Entering ExecuteSQLTemp(connectionString={ConnectionString},sql={Sql},timeoutSec={TimeoutSec})", connectionString, sql, timeoutSec);
            throw new System.NotImplementedException();
        }
    }
}
