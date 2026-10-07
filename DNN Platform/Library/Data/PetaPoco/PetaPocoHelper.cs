// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data.PetaPoco
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using global::PetaPoco;

    public static class PetaPocoHelper
    {
        private const string SqlProviderName = "System.Data.SqlClient";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PetaPocoHelper));

        public static void ExecuteNonQuery(string connectionString, CommandType type, string sql, params object[] args)
        {
			Logger.LogError(">>> Entering ExecuteNonQuery(connectionString={ConnectionString},type={Type},sql={Sql},args={Args})", connectionString, type, sql, args);
            ExecuteNonQuery(connectionString, type, Null.NullInteger, sql, args);
			Logger.LogWarning("<<< Exiting ExecuteNonQuery(connectionString={ConnectionString},type={Type},sql={Sql},args={Args})", connectionString, type, sql, args);
        }

        public static void ExecuteNonQuery(string connectionString, CommandType type, int timeoutSec, string sql, params object[] args)
        {
			Logger.LogError(">>> Entering ExecuteNonQuery(connectionString={ConnectionString},type={Type},timeoutSec={TimeoutSec},sql={Sql},args={Args})", connectionString, type, timeoutSec, sql, args);
            using (var database = new Database(connectionString, SqlProviderName) { EnableAutoSelect = false })
            {
				Logger.LogWarning("<<< Exiting ExecuteNonQuery(connectionString={ConnectionString},type={Type},sql={Sql},args={Args})", connectionString, type, sql, args);
                if (type == CommandType.StoredProcedure)
                {
                    sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
                }

                if (timeoutSec > 0)
                {
                    database.CommandTimeout = timeoutSec;
                }

                try
                {
                    database.Execute(sql, args);
                }
                catch (Exception ex)
                {
                    Logger.Error("[1] Error executing SQL: " + sql + Environment.NewLine + ex.Message);
                    throw;
                }
            }
        }

        public static void BulkInsert(string connectionString, string procedureName, string tableParameterName, DataTable dataTable)
        {
			Logger.LogError(">>> Entering BulkInsert(connectionString={ConnectionString},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", connectionString, procedureName, tableParameterName, dataTable);
            BulkInsert(connectionString, Null.NullInteger, procedureName, tableParameterName, dataTable);
			Logger.LogWarning("<<< Exiting BulkInsert(connectionString={ConnectionString},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", connectionString, procedureName, tableParameterName, dataTable);
        }

        public static void BulkInsert(string connectionString, int timeoutSec, string procedureName, string tableParameterName, DataTable dataTable)
        {
			Logger.LogError(">>> Entering BulkInsert(connectionString={ConnectionString},timeoutSec={TimeoutSec},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", connectionString, timeoutSec, procedureName, tableParameterName, dataTable);
            if (dataTable.Rows.Count > 0)
            {
				Logger.LogWarning("<<< Exiting BulkInsert(connectionString={ConnectionString},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", connectionString, procedureName, tableParameterName, dataTable);
                using var con = new SqlConnection(connectionString);
				Logger.LogError("BulkInsert(connectionString={ConnectionString},timeoutSec={TimeoutSec},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable}): con → {Con}", connectionString, timeoutSec, procedureName, tableParameterName, dataTable, con);
                using var cmd = new SqlCommand(procedureName, con);
				Logger.LogError("BulkInsert(connectionString={ConnectionString},timeoutSec={TimeoutSec},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable}): cmd → {Cmd}", connectionString, timeoutSec, procedureName, tableParameterName, dataTable, cmd);
                if (!tableParameterName.StartsWith("@", StringComparison.Ordinal))
                {
                    tableParameterName = "@" + tableParameterName;
					Logger.LogError("BulkInsert(connectionString={ConnectionString},timeoutSec={TimeoutSec},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable}): tableParameterName → {TableParameterName1}", connectionString, timeoutSec, procedureName, tableParameterName, dataTable, tableParameterName);
                }

                if (timeoutSec > 0)
                {
                    cmd.CommandTimeout = timeoutSec;
					Logger.LogDebug("BulkInsert(connectionString={ConnectionString},timeoutSec={TimeoutSec},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable}): cmd.CommandTimeout → {Arg5}", connectionString, timeoutSec, procedureName, tableParameterName, dataTable, cmd.CommandTimeout);
                }

                cmd.CommandType = CommandType.StoredProcedure;
				Logger.LogDebug("BulkInsert(connectionString={ConnectionString},timeoutSec={TimeoutSec},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable}): cmd.CommandType → {Arg5}", connectionString, timeoutSec, procedureName, tableParameterName, dataTable, cmd.CommandType);
                cmd.Parameters.AddWithValue(tableParameterName, dataTable);
                con.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.Error("[2] Error executing SQL: " + cmd.CommandText + Environment.NewLine + ex.Message);
                    throw;
                }

                con.Close();
            }
			Logger.LogWarning("<<< Exiting BulkInsert(connectionString={ConnectionString},timeoutSec={TimeoutSec},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable})", connectionString, timeoutSec, procedureName, tableParameterName, dataTable);
        }

        public static void BulkInsert(string connectionString, string procedureName, string tableParameterName, DataTable dataTable, Dictionary<string, object> args)
        {
			Logger.LogError(">>> Entering BulkInsert(connectionString={ConnectionString},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},args={Args})", connectionString, procedureName, tableParameterName, dataTable, args);
            BulkInsert(connectionString, procedureName, tableParameterName, dataTable, Null.NullInteger, args);
			Logger.LogWarning("<<< Exiting BulkInsert(connectionString={ConnectionString},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},args={Args})", connectionString, procedureName, tableParameterName, dataTable, args);
        }

        public static void BulkInsert(string connectionString, string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec, Dictionary<string, object> args)
        {
			Logger.LogError(">>> Entering BulkInsert(connectionString={ConnectionString},procedureName={ProcedureName},tableParameterName={TableParameterName},dataTable={DataTable},timeoutSec={TimeoutSec},args={Args})", connectionString, procedureName, tableParameterName, dataTable, timeoutSec, args);
            if (dataTable.Rows.Count > 0)
            {
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(procedureName, con))
                {
                    if (!tableParameterName.StartsWith("@", StringComparison.Ordinal))
                    {
                        tableParameterName = "@" + tableParameterName;
                    }

                    if (timeoutSec > 0)
                    {
                        cmd.CommandTimeout = timeoutSec;
                    }

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue(tableParameterName, dataTable);
                    foreach (var arg in args)
                    {
                        cmd.Parameters.AddWithValue(arg.Key, arg.Value);
                    }

                    con.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                        Logger.Error("[2] Error executing SQL: " + cmd.CommandText);
                        throw;
                    }

                    con.Close();
                }
            }
        }

        public static IDataReader ExecuteReader(string connectionString, CommandType type, string sql, params object[] args)
        {
			Logger.LogError(">>> Entering ExecuteReader(connectionString={ConnectionString},type={Type},sql={Sql},args={Args})", connectionString, type, sql, args);
			Logger.LogWarning("<<< Exiting ExecuteReader(connectionString={ConnectionString},type={Type},sql={Sql},args={Args})", connectionString, type, sql, args);
            return ExecuteReader(connectionString, type, Null.NullInteger, sql, args);
        }

        public static IDataReader ExecuteReader(string connectionString, CommandType type, int timeoutSec, string sql, params object[] args)
        {
            var database = new Database(connectionString, SqlProviderName) { EnableAutoSelect = false, };

            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
            }

            if (timeoutSec > 0)
            {
                database.CommandTimeout = timeoutSec;
            }

            try
            {
                return database.ExecuteReader(sql, args);
            }
            catch (Exception ex)
            {
                // very special case for installation
                if (!sql.EndsWith("GetDatabaseVersion", StringComparison.Ordinal))
                {
                    Logger.Error("[3] Error executing SQL: " + sql + Environment.NewLine + ex.Message);
                }

                throw;
            }
        }

        public static T ExecuteScalar<T>(string connectionString, CommandType type, string sql, params object[] args)
        {
			Logger.LogError(">>> Entering ExecuteScalar(connectionString={ConnectionString},type={Type},sql={Sql},args={Args})", connectionString, type, sql, args);
			Logger.LogWarning("<<< Exiting ExecuteScalar(connectionString={ConnectionString},type={Type},sql={Sql},args={Args})", connectionString, type, sql, args);
            return ExecuteScalar<T>(connectionString, type, Null.NullInteger, sql, args);
        }

        public static T ExecuteScalar<T>(string connectionString, CommandType type, int timeoutSec, string sql, params object[] args)
        {
            using (var database = new Database(connectionString, SqlProviderName) { EnableAutoSelect = false })
            {
                if (type == CommandType.StoredProcedure)
                {
                    sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
                }

                if (timeoutSec > 0)
                {
                    database.CommandTimeout = timeoutSec;
                }

                try
                {
                    return database.ExecuteScalar<T>(sql, args);
                }
                catch (Exception ex)
                {
                    Logger.Error("[4] Error executing SQL: " + sql + Environment.NewLine + ex.Message);
                    throw;
                }
            }
        }

        // ReSharper disable once InconsistentNaming
        public static void ExecuteSQL(string connectionString, string sql)
        {
			Logger.LogError(">>> Entering ExecuteSQL(connectionString={ConnectionString},sql={Sql})", connectionString, sql);
            ExecuteSQL(connectionString, sql, Null.NullInteger);
			Logger.LogWarning("<<< Exiting ExecuteSQL(connectionString={ConnectionString},sql={Sql})", connectionString, sql);
        }

        // ReSharper disable once InconsistentNaming
        public static void ExecuteSQL(string connectionString, string sql, int timeoutSec)
        {
			Logger.LogError(">>> Entering ExecuteSQL(connectionString={ConnectionString},sql={Sql},timeoutSec={TimeoutSec})", connectionString, sql, timeoutSec);
            using (var database = new Database(connectionString, SqlProviderName) { EnableAutoSelect = false })
            {
				Logger.LogWarning("<<< Exiting ExecuteSQL(connectionString={ConnectionString},sql={Sql})", connectionString, sql);
                if (timeoutSec > 0)
                {
                    database.CommandTimeout = timeoutSec;
                }

                try
                {
                    database.Execute(sql);
                }
                catch (Exception ex)
                {
                    Logger.Error("[5] Error executing SQL: " + sql + Environment.NewLine + ex.Message);
                    throw;
                }
            }
        }
    }
}
