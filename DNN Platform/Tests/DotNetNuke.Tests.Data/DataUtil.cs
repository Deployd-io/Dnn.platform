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
	using Microsoft.Extensions.Logging;

    public class DataUtil
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public static void CreateDatabase(string databaseName)
        {
			_logger.LogDebug(">>> Entering CreateDatabase(databaseName={DatabaseName})", databaseName);
            if (File.Exists(databaseName))
            {
                File.Delete(databaseName);
            }

            var engine = new SqlCeEngine(GetConnectionString(databaseName));
			_logger.LogDebug("CreateDatabase(databaseName={DatabaseName}): engine → {Engine}", databaseName, engine);
            engine.CreateDatabase();
            engine.Dispose();
			_logger.LogDebug("<<< Exiting CreateDatabase(databaseName={DatabaseName})", databaseName);
        }

        public static void DeleteDatabase(string databaseName)
        {
			_logger.LogDebug(">>> Entering DeleteDatabase(databaseName={DatabaseName})", databaseName);
            if (File.Exists(databaseName))
            {
                File.Delete(databaseName);
            }
			_logger.LogDebug("<<< Exiting DeleteDatabase(databaseName={DatabaseName})", databaseName);
        }

        public static void ExecuteNonQuery(string databaseName, string sqlScript)
        {
			_logger.LogDebug(">>> Entering ExecuteNonQuery(databaseName={DatabaseName},sqlScript={SqlScript})", databaseName, sqlScript);
            ExecuteNonQuery(databaseName, ReplaceTokens(sqlScript), (cmd) => cmd.ExecuteNonQuery());
			_logger.LogDebug("<<< Exiting ExecuteNonQuery(databaseName={DatabaseName},sqlScript={SqlScript})", databaseName, sqlScript);
        }

        public static int ExecuteScalar(string databaseName, string sqlScript)
        {
			_logger.LogDebug(">>> Entering ExecuteScalar(databaseName={DatabaseName},sqlScript={SqlScript})", databaseName, sqlScript);
			_logger.LogDebug("<<< Exiting ExecuteScalar(databaseName={DatabaseName},sqlScript={SqlScript})", databaseName, sqlScript);
            return ExecuteQuery(databaseName, ReplaceTokens(sqlScript), (cmd) => (int)cmd.ExecuteScalar());
        }

        public static string GetConnectionString(string databaseName)
        {
			_logger.LogDebug(">>> Entering GetConnectionString(databaseName={DatabaseName})", databaseName);
			_logger.LogDebug("<<< Exiting GetConnectionString(databaseName={DatabaseName})", databaseName);
            return string.Format("Data Source = {0};", databaseName);
        }

        public static T GetFieldValue<T>(string databaseName, string tableName, string field, string primaryKeyField, string primaryKeyValue)
        {
			_logger.LogDebug(">>> Entering GetFieldValue(databaseName={DatabaseName},tableName={TableName},field={Field},primaryKeyField={PrimaryKeyField},primaryKeyValue={PrimaryKeyValue})", databaseName, tableName, field, primaryKeyField, primaryKeyValue);
			_logger.LogDebug("<<< Exiting GetFieldValue(databaseName={DatabaseName},tableName={TableName},field={Field},primaryKeyField={PrimaryKeyField},primaryKeyValue={PrimaryKeyValue})", databaseName, tableName, field, primaryKeyField, primaryKeyValue);
            return ExecuteQuery(databaseName, string.Format(DataResources.GetFieldValue, field, tableName, primaryKeyField, primaryKeyValue), (cmd) => (T)cmd.ExecuteScalar());
        }

        public static int GetLastAddedRecordID(string databaseName, string tableName, string primaryKeyField)
        {
			_logger.LogDebug(">>> Entering GetLastAddedRecordID(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField})", databaseName, tableName, primaryKeyField);
			_logger.LogDebug("<<< Exiting GetLastAddedRecordID(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField})", databaseName, tableName, primaryKeyField);
            return ExecuteScalar(databaseName, string.Format(DataResources.GetLastAddedRecordID, tableName, primaryKeyField));
        }

        public static int GetRecordCount(string databaseName, string tableName)
        {
			_logger.LogDebug(">>> Entering GetRecordCount(databaseName={DatabaseName},tableName={TableName})", databaseName, tableName);
			_logger.LogDebug("<<< Exiting GetRecordCount(databaseName={DatabaseName},tableName={TableName})", databaseName, tableName);
			_logger.LogDebug("<<< Exiting GetRecordCount(databaseName={DatabaseName},tableName={TableName},field={Field},value={Value})", databaseName, tableName, field, value);
            return ExecuteScalar(databaseName, string.Format(DataResources.RecordCountScript, tableName));
        }

        public static int GetRecordCount(string databaseName, string tableName, string field, string value)
        {
            return ExecuteScalar(databaseName, string.Format(DataResources.GetRecordCountByField, tableName, field, value));
        }

        public static DataTable GetTable(string databaseName, string tableName)
        {
			_logger.LogDebug(">>> Entering GetTable(databaseName={DatabaseName},tableName={TableName})", databaseName, tableName);
            return ExecuteQuery<DataTable>(databaseName, string.Format(DataResources.GetTable, tableName),
                                            (cmd) =>
                                            {
												var __logStart = System.Diagnostics.Stopwatch.StartNew();
												_logger.LogInformation("GetTable(databaseName,tableName)={DatabaseName},{TableName}: database query executed in {Arg2} ms", databaseName, tableName, __logStart.ElapsedMilliseconds);
                                                var reader = cmd.ExecuteReader();
                                                var table = new DataTable();
                                                table.Load(reader);
											_logger.LogDebug("<<< Exiting GetTable(databaseName={DatabaseName},tableName={TableName})", databaseName, tableName);
                                                return table;
                                            });
        }

        public static void SetUpDatabase(int count)
        {
			_logger.LogDebug(">>> Entering SetUpDatabase(count={Count})", count);
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
			_logger.LogDebug("<<< Exiting SetUpDatabase(count={Count})", count);
        }

        private static TReturn ExecuteQuery<TReturn>(string databaseName, string sqlScript, Func<SqlCeCommand, TReturn> command)
        {
			_logger.LogDebug(">>> Entering ExecuteQuery(databaseName={DatabaseName},sqlScript={SqlScript},command={Command})", databaseName, sqlScript, command);
            using (var connection = new SqlCeConnection(GetConnectionString(databaseName)))
            {
                connection.Open();

                using (SqlCeCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sqlScript;
                    return command(cmd);
                }
            }
			_logger.LogDebug("<<< Exiting ExecuteQuery(databaseName={DatabaseName},sqlScript={SqlScript},command={Command})", databaseName, sqlScript, command);
        }

        private static void ExecuteNonQuery(string databaseName, string sqlScript, Action<SqlCeCommand> command)
        {
			_logger.LogDebug(">>> Entering ExecuteNonQuery(databaseName={DatabaseName},sqlScript={SqlScript},command={Command})", databaseName, sqlScript, command);
            using (var connection = new SqlCeConnection(GetConnectionString(databaseName)))
            {
                connection.Open();

                using (SqlCeCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sqlScript;
                    command(cmd);
                }
            }
			_logger.LogDebug("<<< Exiting ExecuteNonQuery(databaseName={DatabaseName},sqlScript={SqlScript},command={Command})", databaseName, sqlScript, command);
        }

        private static string ReplaceTokens(string sql)
        {
			_logger.LogDebug(">>> Entering ReplaceTokens(sql={Sql})", sql);
			_logger.LogDebug("<<< Exiting ReplaceTokens(sql={Sql})", sql);
            return sql.Replace("{databaseOwner}", DataProvider.Instance().DatabaseOwner)
                        .Replace("{objectQualifier}", DataProvider.Instance().ObjectQualifier);
        }
    }
}
