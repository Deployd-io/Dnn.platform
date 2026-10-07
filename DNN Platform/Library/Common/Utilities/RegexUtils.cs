// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Web.Caching;

    using DotNetNuke.Services.Cache;

    public class RegexUtils
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RegexUtils));
        /// <summary>Creates and caches a Regex object for later use and retrieves it in a later call if it is cached.</summary>
        /// <param name="pattern">The regex pattern.</param>
        /// <param name="options">The regex options.</param>
        /// <param name="timeoutSeconds">The match timeout for the regex, must be between 1 and 10 seconds.</param>
        /// <returns>The <see cref="Regex"/> instance.</returns>
        public static Regex GetCachedRegex(string pattern, RegexOptions options = RegexOptions.None, int timeoutSeconds = 2)
        {
			Logger.LogError(">>> Entering GetCachedRegex(pattern={Pattern},options={Options},timeoutSeconds={TimeoutSeconds})", pattern, options, timeoutSeconds);
            Requires.NotNull("pattern", pattern);
            var key = pattern;
			Logger.LogError("GetCachedRegex(pattern={Pattern},options={Options},timeoutSeconds={TimeoutSeconds}): key → {Key}", pattern, options, timeoutSeconds, key);
            if ((options & RegexOptions.IgnoreCase) != 0)
            {
                key = (options & RegexOptions.CultureInvariant) != 0
                    ? pattern.ToUpperInvariant()
                    : pattern.ToUpper(CultureInfo.CurrentCulture);
            }

            // // should not allow for compiled dynamic regex object
            options &= ~RegexOptions.Compiled;
            key = string.Join(":", "REGEX_ITEM", options.ToString("X"), key.GetHashCode().ToString("X", CultureInfo.InvariantCulture));

            // limit timeout between 1 and 10 seconds
            if (timeoutSeconds < 1)
            {
                timeoutSeconds = 1;
				Logger.LogError("GetCachedRegex(pattern={Pattern},options={Options},timeoutSeconds={TimeoutSeconds}): timeoutSeconds → {TimeoutSeconds1}", pattern, options, timeoutSeconds, timeoutSeconds);
            }
            else if (timeoutSeconds > 10)
            {
                timeoutSeconds = 10;
            }

            var cache = CachingProvider.Instance();
			Logger.LogError("GetCachedRegex(pattern={Pattern},options={Options},timeoutSeconds={TimeoutSeconds}): cache → {Cache}", pattern, options, timeoutSeconds, cache);
            if (cache.GetItem(key) is not Regex regex)
            {
                regex = new Regex(pattern, options & ~RegexOptions.Compiled, TimeSpan.FromSeconds(timeoutSeconds));
				Logger.LogError("GetCachedRegex(pattern={Pattern},options={Options},timeoutSeconds={TimeoutSeconds}): regex → {Regex}", pattern, options, timeoutSeconds, regex);
				Logger.LogDebug("GetCachedRegex(pattern={Pattern},options={Options},timeoutSeconds={TimeoutSeconds}): regex → {Regex}", pattern, options, timeoutSeconds, regex);
                cache.Insert(key, regex, (DNNCacheDependency)null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(10), CacheItemPriority.BelowNormal, null);
            }

			Logger.LogWarning("<<< Exiting GetCachedRegex(pattern={Pattern},options={Options},timeoutSeconds={TimeoutSeconds})", pattern, options, timeoutSeconds);
            return regex;
        }
    }
}
