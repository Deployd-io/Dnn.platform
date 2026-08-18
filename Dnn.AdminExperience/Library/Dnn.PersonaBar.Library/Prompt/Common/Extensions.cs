// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Prompt.Common
{
    using System;
    using System.Globalization;

    public static class Extensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Extensions));
        public static string ToPromptShortDateString(this DateTime input)
        {
			Logger.LogDebug(">>> Entering ToPromptShortDateString(input={Input})", input);
			Logger.LogDebug("<<< Exiting ToPromptShortDateString(input={Input})", input);
            return input.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
        }

        public static string ToPromptShortDateAndTimeString(this DateTime input)
        {
			Logger.LogDebug(">>> Entering ToPromptShortDateAndTimeString(input={Input})", input);
			Logger.LogDebug("<<< Exiting ToPromptShortDateAndTimeString(input={Input})", input);
            return input.ToString("yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture);
        }

        public static string ToPromptLongDateString(this DateTime input)
        {
			Logger.LogDebug(">>> Entering ToPromptLongDateString(input={Input})", input);
			Logger.LogDebug("<<< Exiting ToPromptLongDateString(input={Input})", input);
            return input.ToString("F", CultureInfo.CurrentCulture);
        }
    }
}
