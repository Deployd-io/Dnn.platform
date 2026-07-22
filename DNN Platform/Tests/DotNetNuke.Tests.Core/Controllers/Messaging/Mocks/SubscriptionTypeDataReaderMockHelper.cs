// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Mocks
{
    using System.Data;

    internal class SubscriptionTypeDataReaderMockHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        internal static IDataReader CreateEmptySubscriptionTypeReader()
        {
			Logger.LogDebug(">>> Entering CreateEmptySubscriptionTypeReader()");
			Logger.LogDebug("<<< Exiting CreateEmptySubscriptionTypeReader()");
            return CreateSubscriptionTypeTable().CreateDataReader();
        }

        private static DataTable CreateSubscriptionTypeTable()
        {
			Logger.LogDebug(">>> Entering CreateSubscriptionTypeTable()");
            var table = new DataTable();

			Logger.LogDebug("CreateSubscriptionTypeTable(): table → {Table}", table);
            var idColumn = table.Columns.Add("SubscriptionTypeId", typeof(int));
            table.Columns.Add("SubscriptionName", typeof(string));
			Logger.LogDebug("CreateSubscriptionTypeTable(): idColumn → {IdColumn}", idColumn);
            table.Columns.Add("FriendlyName", typeof(string));
            table.Columns.Add("DesktopModuleId", typeof(int));

            table.PrimaryKey = new[] { idColumn };
			Logger.LogDebug("CreateSubscriptionTypeTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			Logger.LogDebug("<<< Exiting CreateSubscriptionTypeTable()");
            return table;
        }
    }
}
