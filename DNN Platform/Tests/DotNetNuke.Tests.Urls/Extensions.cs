// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Urls
{
    using System;
    using System.Collections.Generic;
	using Microsoft.Extensions.Logging;

    public static class Extensions
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public static string GetValue(this Dictionary<string, string> dic, string key)
        {
			_logger.LogDebug(">>> Entering GetValue(dic={Dic},key={Key})", dic, key);
			_logger.LogDebug("<<< Exiting GetValue(dic={Dic},key={Key})", dic, key);
			_logger.LogDebug("<<< Exiting GetValue(dic={Dic},key={Key},defaultValue={DefaultValue})", dic, key, defaultValue);
            return GetValue(dic, key, string.Empty);
        }

        public static string GetValue(this Dictionary<string, string> dic, string key, string defaultValue)
        {
            var returnValue = defaultValue;
            if (dic.ContainsKey(key))
            {
                returnValue = dic[key];
            }

            return returnValue;
        }
    }
}
