// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Mocks
{
    using System.Data;

    using DotNetNuke.Services.Social.Messaging;

    internal class UserPreferenceDataReaderMockHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        internal static IDataReader CreateEmptyUserPreferenceReader()
        {
			Logger.LogDebug(">>> Entering CreateEmptyUserPreferenceReader()");
			Logger.LogDebug("<<< Exiting CreateEmptyUserPreferenceReader()");
            return CreateUserPreferenceTable().CreateDataReader();
        }

        internal static IDataReader CreateUserPreferenceReader(UserPreference userPreference)
        {
			Logger.LogDebug(">>> Entering CreateUserPreferenceReader(userPreference={UserPreference})", userPreference);
            var table = CreateUserPreferenceTable();
            AddUserPreferenceToTable(table, userPreference);
			Logger.LogDebug("CreateUserPreferenceReader(userPreference={UserPreference}): table → {Table}", userPreference, table);
			Logger.LogDebug("<<< Exiting CreateUserPreferenceReader(userPreference={UserPreference})", userPreference);
            return table.CreateDataReader();
        }

        private static DataTable CreateUserPreferenceTable()
        {
			Logger.LogDebug(">>> Entering CreateUserPreferenceTable()");
            var table = new DataTable();

			Logger.LogDebug("CreateUserPreferenceTable(): table → {Table}", table);
            var idColumn = table.Columns.Add("UserPreferenceId", typeof(int));
            table.Columns.Add("PortalId", typeof(int));
			Logger.LogDebug("CreateUserPreferenceTable(): idColumn → {IdColumn}", idColumn);
            table.Columns.Add("UserId", typeof(string));
            table.Columns.Add("MessagesEmailFrequency", typeof(int));
            table.Columns.Add("NotificationsEmailFrequency", typeof(int));

            table.PrimaryKey = new[] { idColumn };
			Logger.LogDebug("CreateUserPreferenceTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			Logger.LogDebug("<<< Exiting CreateUserPreferenceTable()");
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
        }
    }
}
