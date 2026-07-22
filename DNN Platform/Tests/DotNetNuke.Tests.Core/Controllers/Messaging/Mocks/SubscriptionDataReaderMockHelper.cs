// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.Services.Social.Subscriptions.Entities;

    internal class SubscriptionDataReaderMockHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        internal static IDataReader CreateEmptySubscriptionReader()
        {
			Logger.LogDebug(">>> Entering CreateEmptySubscriptionReader()");
			Logger.LogDebug("<<< Exiting CreateEmptySubscriptionReader()");
            return CreateSubscriptionTable().CreateDataReader();
        }

        internal static IDataReader CreateSubscriptionReader(IEnumerable<Subscription> subscriptions)
        {
			Logger.LogDebug(">>> Entering CreateSubscriptionReader(subscriptions={Subscriptions})", subscriptions);
            var datatable = CreateSubscriptionTable();
            foreach (var subscription in subscriptions)
            {
			Logger.LogDebug("CreateSubscriptionReader(subscriptions={Subscriptions}): datatable → {Datatable}", subscriptions, datatable);
                AddSubscriptionToTable(datatable, subscription);
            }

			Logger.LogDebug("<<< Exiting CreateSubscriptionReader(subscriptions={Subscriptions})", subscriptions);
            return datatable.CreateDataReader();
        }

        private static DataTable CreateSubscriptionTable()
        {
			Logger.LogDebug(">>> Entering CreateSubscriptionTable()");
            var table = new DataTable();

			Logger.LogDebug("CreateSubscriptionTable(): table → {Table}", table);
            var idColumn = table.Columns.Add("SubscriptionId", typeof(int));
            table.Columns.Add("UserId", typeof(int));
			Logger.LogDebug("CreateSubscriptionTable(): idColumn → {IdColumn}", idColumn);
            table.Columns.Add("PortalId", typeof(int));
            table.Columns.Add("SubscriptionTypeId", typeof(int));
            table.Columns.Add("ObjectKey", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("CreatedOnDate", typeof(DateTime));
            table.Columns.Add("ModuleID", typeof(int));
            table.Columns.Add("TabId", typeof(int));

            table.PrimaryKey = new[] { idColumn };
			Logger.LogDebug("CreateSubscriptionTable(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

			Logger.LogDebug("<<< Exiting CreateSubscriptionTable()");
            return table;
        }

        private static void AddSubscriptionToTable(DataTable table, Subscription subscription)
        {
            table.Rows.Add(new object[]
            {
                subscription.SubscriptionId,
                subscription.UserId,
                subscription.PortalId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.Description,
                subscription.CreatedOnDate,
                subscription.ModuleId,
                subscription.TabId,
            });
        }
    }
}
