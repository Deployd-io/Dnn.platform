// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using NUnit.Framework;

    public class DataAssert
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static void IsFieldValueEqual<T>(T expectedValue, string databaseName, string tableName, string field, string primaryKeyField, int id)
        {
			Logger.LogDebug(">>> Entering IsFieldValueEqual(expectedValue={ExpectedValue},databaseName={DatabaseName},tableName={TableName},field={Field},primaryKeyField={PrimaryKeyField},id={Id})", expectedValue, databaseName, tableName, field, primaryKeyField, id);
            var value = DataUtil.GetFieldValue<T>(databaseName, tableName, field, primaryKeyField, id.ToString());

			Logger.LogDebug("IsFieldValueEqual(expectedValue={ExpectedValue},databaseName={DatabaseName},tableName={TableName},field={Field},primaryKeyField={PrimaryKeyField},id={Id}): value → {Value}", expectedValue, databaseName, tableName, field, primaryKeyField, id, value);
            Assert.That(value, Is.EqualTo(expectedValue));
			Logger.LogDebug("<<< Exiting IsFieldValueEqual(expectedValue={ExpectedValue},databaseName={DatabaseName},tableName={TableName},field={Field},primaryKeyField={PrimaryKeyField},id={Id})", expectedValue, databaseName, tableName, field, primaryKeyField, id);
        }

        public static void RecordWithIdPresent(string databaseName, string tableName, string primaryKeyField, int id)
        {
			Logger.LogDebug(">>> Entering RecordWithIdPresent(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField},id={Id})", databaseName, tableName, primaryKeyField, id);
            var count = DataUtil.GetRecordCount(databaseName, tableName, primaryKeyField, id.ToString());
            Assert.That(count, Is.EqualTo(1));
			Logger.LogDebug("<<< Exiting RecordWithIdPresent(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField},id={Id})", databaseName, tableName, primaryKeyField, id);
			Logger.LogDebug("RecordWithIdPresent(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField},id={Id}): count → {Count}", databaseName, tableName, primaryKeyField, id, count);
        }

        public static void RecordWithIdNotPresent(string databaseName, string tableName, string primaryKeyField, int id)
        {
			Logger.LogDebug(">>> Entering RecordWithIdNotPresent(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField},id={Id})", databaseName, tableName, primaryKeyField, id);
            var count = DataUtil.GetRecordCount(databaseName, tableName, primaryKeyField, id.ToString());
            Assert.That(count, Is.EqualTo(0));
			Logger.LogDebug("<<< Exiting RecordWithIdNotPresent(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField},id={Id})", databaseName, tableName, primaryKeyField, id);
			Logger.LogDebug("RecordWithIdNotPresent(databaseName={DatabaseName},tableName={TableName},primaryKeyField={PrimaryKeyField},id={Id}): count → {Count}", databaseName, tableName, primaryKeyField, id, count);
        }
    }
}
