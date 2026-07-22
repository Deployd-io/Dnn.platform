// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System.Collections;

    /// <summary>
    /// The ScheduleHistorySortStartDate Class is a custom IComparer Implementation
    /// used to sort the Schedule Items.
    /// </summary>
    public class ScheduleHistorySortStartDate : IComparer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public int Compare(object x, object y)
        {
			Logger.LogDebug(">>> Entering Compare(x={X},y={Y})", x, y);
			Logger.LogDebug("<<< Exiting Compare(x={X},y={Y})", x, y);
            return ((ScheduleHistoryItem)y).StartDate.CompareTo(((ScheduleHistoryItem)x).StartDate);
        }
    }
}
