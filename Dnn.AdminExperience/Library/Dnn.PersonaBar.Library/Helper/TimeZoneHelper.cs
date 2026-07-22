// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Helper
{
    using System;
    using System.Globalization;

    using Dnn.PersonaBar.Library.Dto;

    public class TimeZoneHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static TimeZoneDto GetPreferredTimeZone(TimeZoneInfo userTimeZone)
        {
			Logger.LogDebug(">>> Entering GetPreferredTimeZone(userTimeZone={UserTimeZone})", userTimeZone);
			Logger.LogDebug("<<< Exiting GetPreferredTimeZone(userTimeZone={UserTimeZone})", userTimeZone);
            return new TimeZoneDto()
            {
                Id = userTimeZone.Id,
                DisplayName = userTimeZone.DisplayName,
                BaseUtcOffset = FormatOffset(userTimeZone.BaseUtcOffset),
                CurrentUtcOffset = FormatOffset(userTimeZone.GetUtcOffset(DateTime.UtcNow)),
            };
        }

        private static string FormatOffset(TimeSpan time)
        {
			Logger.LogDebug(">>> Entering FormatOffset(time={Time})", time);
			Logger.LogDebug("<<< Exiting FormatOffset(time={Time})", time);
            return ((time < TimeSpan.Zero) ? "-" : "+") + time.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
        }
    }
}
