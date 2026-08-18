// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    public class DataAssert
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public static void IsFieldValueEqual<T>(T expectedValue, string databaseName, string tableName, string field, string primaryKeyField, int id)
        {
			_logger.LogDebug(">>> Entering IsFieldValueEqual(expectedValue={ExpectedValue},databaseName={DatabaseName},tableName={TableName},field={Field},primaryKeyField={PrimaryKeyField},id={Id})", expectedValue, databaseName, tableName, field, primaryKeyField, id);
            var value = DataUtil.GetFieldValue<T>(databaseName, tableName, field, primaryKeyField, id.ToString());

			_logger.LogDebug("IsFieldValueEqual(expectedValue={ExpectedValue},databaseName={DatabaseName},tableName={TableName},field={Field},primaryKeyField={PrimaryKeyField},id={Id}): value → {Value}", expectedValue, databaseName, tableName, field, primaryKeyField, id, value);
            Assert.That(value, Is.EqualTo(expectedValue));
			_logger.LogDebug("<<< Exiting IsFieldValueEqual(expectedValue={ExpectedValue},databaseName={DatabaseName},tableName={TableName},field={Field},primaryKeyField={PrimaryKeyField},id={Id})", expectedValue, databaseName, tableName, field, primaryKeyField, id);
        }

        public static void RecordWithIdPresent(string databaseName, string tableName, string primaryKeyField, int id)
        {
			_logger.LogDebug(">>> Entering RecordWithIdPresent(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField},id={Id})", databaseName, tableName, primaryKeyField, id);
            var count = DataUtil.GetRecordCount(databaseName, tableName, primaryKeyField, id.ToString());
            Assert.That(count, Is.EqualTo(1));
			_logger.LogDebug("RecordWithIdPresent(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField},id={Id}): count → {Count}", databaseName, tableName, primaryKeyField, id, count);
			_logger.LogDebug("<<< Exiting RecordWithIdPresent(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField},id={Id})", databaseName, tableName, primaryKeyField, id);
        }

        public static void RecordWithIdNotPresent(string databaseName, string tableName, string primaryKeyField, int id)
        {
			_logger.LogDebug(">>> Entering RecordWithIdNotPresent(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField},id={Id})", databaseName, tableName, primaryKeyField, id);
            var count = DataUtil.GetRecordCount(databaseName, tableName, primaryKeyField, id.ToString());
            Assert.That(count, Is.EqualTo(0));
			_logger.LogDebug("RecordWithIdNotPresent(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField},id={Id}): count → {Count}", databaseName, tableName, primaryKeyField, id, count);
			_logger.LogDebug("<<< Exiting RecordWithIdNotPresent(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField},id={Id})", databaseName, tableName, primaryKeyField, id);
        }
    }
}
