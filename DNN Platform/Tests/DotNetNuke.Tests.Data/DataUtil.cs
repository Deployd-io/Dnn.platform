// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using System;
    using System.Data;
    using System.Data.SqlServerCe;
    using System.IO;
    using System.Text.RegularExpressions;

    using DotNetNuke.Data;
    using DotNetNuke.Tests.Utilities;

    public class DataUtil
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static void CreateDatabase(string databaseName)
        {
			Logger.LogDebug(">>> Entering CreateDatabase(databaseName={DatabaseName})", databaseName);
            if (File.Exists(databaseName))
            {
                File.Delete(databaseName);
            }

            var engine = new SqlCeEngine(GetConnectionString(databaseName));
            engine.CreateDatabase();
            engine.Dispose();
			Logger.LogDebug("<<< Exiting CreateDatabase(databaseName={DatabaseName})", databaseName);
        }

        public static void DeleteDatabase(string databaseName)
        {
			Logger.LogDebug(">>> Entering DeleteDatabase(databaseName={DatabaseName})", databaseName);
            if (File.Exists(databaseName))
            {
                File.Delete(databaseName);
            }
			Logger.LogDebug("<<< Exiting DeleteDatabase(databaseName={DatabaseName})", databaseName);
        }

        public static void ExecuteNonQuery(string databaseName, string sqlScript)
        {
			Logger.LogDebug(">>> Entering ExecuteNonQuery(databaseName={DatabaseName},sqlScript={SqlScript})", databaseName, sqlScript);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            ExecuteNonQuery(databaseName, ReplaceTokens(sqlScript), (cmd) => cmd.ExecuteNonQuery());
			Logger.LogDebug("<<< Exiting ExecuteNonQuery(databaseName={DatabaseName},sqlScript={SqlScript})", databaseName, sqlScript);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("ExecuteNonQuery(databaseName)={DatabaseName}: database query executed in {Arg1} ms", databaseName, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("ExecuteNonQuery(databaseName)={DatabaseName}: database query executed in {Arg1} ms", databaseName, __logStart.ElapsedMilliseconds);
        public static int ExecuteScalar(string databaseName, string sqlScript)
        {
			Logger.LogDebug(">>> Entering ExecuteScalar(databaseName={DatabaseName},sqlScript={SqlScript})", databaseName, sqlScript);
			Logger.LogDebug("<<< Exiting ExecuteScalar(databaseName={DatabaseName},sqlScript={SqlScript})", databaseName, sqlScript);
            return ExecuteQuery(databaseName, ReplaceTokens(sqlScript), (cmd) => (int)cmd.ExecuteScalar());
        }

        public static string GetConnectionString(string databaseName)
        {
			Logger.LogDebug(">>> Entering GetConnectionString(databaseName={DatabaseName})", databaseName);
			Logger.LogDebug("<<< Exiting GetConnectionString(databaseName={DatabaseName})", databaseName);
            return string.Format("Data Source = {0};", databaseName);
        }

        public static T GetFieldValue<T>(string databaseName, string tableName, string field, string primaryKeyField, string primaryKeyValue)
        {
			Logger.LogDebug(">>> Entering GetFieldValue(databaseName={DatabaseName},tableName={TableName},field={Field},primaryKeyField={PrimaryKeyField},primaryKeyValue={PrimaryKeyValue})", databaseName, tableName, field, primaryKeyField, primaryKeyValue);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            return ExecuteQuery(databaseName, string.Format(DataResources.GetFieldValue, field, tableName, primaryKeyField, primaryKeyValue), (cmd) => (T)cmd.ExecuteScalar());
        }

		Logger.LogInformation("GetFieldValue(databaseName,tableName,field,primaryKeyField,primaryKeyValue)={DatabaseName},{TableName},{Field},{PrimaryKeyField},{PrimaryKeyValue}: find query executed in {Arg5} ms", databaseName, tableName, field, primaryKeyField, primaryKeyValue, __logStart.ElapsedMilliseconds);
        public static int GetLastAddedRecordID(string databaseName, string tableName, string primaryKeyField)
        {
			Logger.LogDebug(">>> Entering GetLastAddedRecordID(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField})", databaseName, tableName, primaryKeyField);
			Logger.LogDebug("<<< Exiting GetLastAddedRecordID(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField})", databaseName, tableName, primaryKeyField);
            return ExecuteScalar(databaseName, string.Format(DataResources.GetLastAddedRecordID, tableName, primaryKeyField));
        }

        public static int GetRecordCount(string databaseName, string tableName)
        {
			Logger.LogDebug(">>> Entering GetRecordCount(databaseName={DatabaseName},tableName={TableName})", databaseName, tableName);
			Logger.LogDebug("<<< Exiting GetRecordCount(databaseName={DatabaseName},tableName={TableName})", databaseName, tableName);
            return ExecuteScalar(databaseName, string.Format(DataResources.RecordCountScript, tableName));
        }

        public static int GetRecordCount(string databaseName, string tableName, string field, string value)
        {
			Logger.LogDebug("<<< Exiting GetRecordCount(databaseName={DatabaseName},tableName={TableName},field={Field},value={Value})", databaseName, tableName, field, value);
            return ExecuteScalar(databaseName, string.Format(DataResources.GetRecordCountByField, tableName, field, value));
        }

        public static DataTable GetTable(string databaseName, string tableName)
        {
			Logger.LogDebug(">>> Entering GetTable(databaseName={DatabaseName},tableName={TableName})", databaseName, tableName);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            return ExecuteQuery<DataTable>(databaseName, string.Format(DataResources.GetTable, tableName),
                                            (cmd) =>
                                            {
                                                var reader = cmd.ExecuteReader();
												Logger.LogInformation("GetTable(databaseName,tableName)={DatabaseName},{TableName}: database query executed in {Arg2} ms", databaseName, tableName, __logStart.ElapsedMilliseconds);
                                                var table = new DataTable();
                                                table.Load(reader);
                                                return table;
                                            });
											Logger.LogDebug("<<< Exiting GetTable(databaseName={DatabaseName},tableName={TableName})", databaseName, tableName);
        }

        public static void SetUpDatabase(int count)
        {
			Logger.LogDebug(">>> Entering SetUpDatabase(count={Count})", count);
            string[] _dogAges = Constants.PETAPOCO_DogAges.Split(',');
            string[] _dogNames = Constants.PETAPOCO_DogNames.Split(',');

            CreateDatabase(Constants.PETAPOCO_DatabaseName);
            ExecuteNonQuery(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_CreateDogTableSql);
            var index = 0;
            for (int i = 0; i < count; i++)
            {
                if (index == 5)
                {
                    index = 0;
                }

                ExecuteNonQuery(Constants.PETAPOCO_DatabaseName, string.Format(Constants.PETAPOCO_InsertDogRow, _dogNames[index], _dogAges[index]));
                index++;
            }
			Logger.LogDebug("<<< Exiting SetUpDatabase(count={Count})", count);
        }

        private static TReturn ExecuteQuery<TReturn>(string databaseName, string sqlScript, Func<SqlCeCommand, TReturn> command)
        {
            using (var connection = new SqlCeConnection(GetConnectionString(databaseName)))
            {
                connection.Open();

                using (SqlCeCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sqlScript;
                    return command(cmd);
                }
            }
        }

        private static void ExecuteNonQuery(string databaseName, string sqlScript, Action<SqlCeCommand> command)
        {
			Logger.LogDebug(">>> Entering ExecuteNonQuery(databaseName={DatabaseName},sqlScript={SqlScript},command={Command})", databaseName, sqlScript, command);
            using (var connection = new SqlCeConnection(GetConnectionString(databaseName)))
            {
                connection.Open();

                using (SqlCeCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sqlScript;
                    command(cmd);
                }
            }
			Logger.LogDebug("<<< Exiting ExecuteNonQuery(databaseName={DatabaseName},sqlScript={SqlScript},command={Command})", databaseName, sqlScript, command);
        }

        private static string ReplaceTokens(string sql)
        {
			Logger.LogDebug(">>> Entering ReplaceTokens(sql={Sql})", sql);
			Logger.LogDebug("<<< Exiting ReplaceTokens(sql={Sql})", sql);
            return sql.Replace("{databaseOwner}", DataProvider.Instance().DatabaseOwner)
                        .Replace("{objectQualifier}", DataProvider.Instance().ObjectQualifier);
        }
    }
}
