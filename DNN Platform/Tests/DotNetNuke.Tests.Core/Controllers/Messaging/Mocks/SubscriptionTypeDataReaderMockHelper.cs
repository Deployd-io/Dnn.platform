// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Mocks
{
    using System.Data;

    internal class SubscriptionTypeDataReaderMockHelper
    {
        internal static IDataReader CreateEmptySubscriptionTypeReader()
        {
			mockEventLogger.LogDebug(">>> Entering CreateEmptySubscriptionTypeReader()");
			mockEventLogger.LogDebug("<<< Exiting CreateEmptySubscriptionTypeReader()");
            return CreateSubscriptionTypeTable().CreateDataReader();
        }

        private static DataTable CreateSubscriptionTypeTable()
        {
			mockEventLogger.LogDebug(">>> Entering CreateSubscriptionTypeTable()");
            var table = new DataTable();

			mockEventLogger.LogDebug("CreateSubscriptionTypeTable(): table → {Table}", table);
            var idColumn = table.Columns.Add("SubscriptionTypeId", typeof(int));
			mockEventLogger.LogDebug("CreateSubscriptionTypeTable(): idColumn → {IdColumn}", idColumn);
            table.Columns.Add("SubscriptionName", typeof(string));
            table.Columns.Add("FriendlyName", typeof(string));
            table.Columns.Add("DesktopModuleId", typeof(int));

            table.PrimaryKey = new[] { idColumn };
			mockEventLogger.LogDebug("CreateSubscriptionTypeTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			mockEventLogger.LogDebug("<<< Exiting CreateSubscriptionTypeTable()");
            return table;
        }
    }
}
