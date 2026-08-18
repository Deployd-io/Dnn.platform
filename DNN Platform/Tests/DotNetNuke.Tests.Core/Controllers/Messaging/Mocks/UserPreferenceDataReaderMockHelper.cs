// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Mocks
{
    using System.Data;

    using DotNetNuke.Services.Social.Messaging;

    internal class UserPreferenceDataReaderMockHelper
    {
        internal static IDataReader CreateEmptyUserPreferenceReader()
        {
			mockEventLogger.LogDebug(">>> Entering CreateEmptyUserPreferenceReader()");
			mockEventLogger.LogDebug("<<< Exiting CreateEmptyUserPreferenceReader()");
            return CreateUserPreferenceTable().CreateDataReader();
        }

        internal static IDataReader CreateUserPreferenceReader(UserPreference userPreference)
        {
			mockEventLogger.LogDebug(">>> Entering CreateUserPreferenceReader(userPreference={UserPreference})", userPreference);
            var table = CreateUserPreferenceTable();
            AddUserPreferenceToTable(table, userPreference);
			mockEventLogger.LogDebug("CreateUserPreferenceReader(userPreference={UserPreference}): table → {Table}", userPreference, table);
			mockEventLogger.LogDebug("<<< Exiting CreateUserPreferenceReader(userPreference={UserPreference})", userPreference);
            return table.CreateDataReader();
        }

        private static DataTable CreateUserPreferenceTable()
        {
			mockEventLogger.LogDebug(">>> Entering CreateUserPreferenceTable()");
            var table = new DataTable();

			mockEventLogger.LogDebug("CreateUserPreferenceTable(): table → {Table}", table);
            var idColumn = table.Columns.Add("UserPreferenceId", typeof(int));
			mockEventLogger.LogDebug("CreateUserPreferenceTable(): idColumn → {IdColumn}", idColumn);
            table.Columns.Add("PortalId", typeof(int));
            table.Columns.Add("UserId", typeof(string));
            table.Columns.Add("MessagesEmailFrequency", typeof(int));
            table.Columns.Add("NotificationsEmailFrequency", typeof(int));

            table.PrimaryKey = new[] { idColumn };
			mockEventLogger.LogDebug("CreateUserPreferenceTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			mockEventLogger.LogDebug("<<< Exiting CreateUserPreferenceTable()");
            return table;
        }

        private static void AddUserPreferenceToTable(DataTable table, UserPreference userPreference)
        {
            table.Rows.Add(new object[]
            {
                1,
                userPreference.PortalId,
                userPreference.UserId,
                userPreference.MessagesEmailFrequency,
                userPreference.NotificationsEmailFrequency,
            });
			mockEventLogger.LogDebug("<<< Exiting AddUserPreferenceToTable(table={Table},userPreference={UserPreference})", table, userPreference);
        }
    }
}
