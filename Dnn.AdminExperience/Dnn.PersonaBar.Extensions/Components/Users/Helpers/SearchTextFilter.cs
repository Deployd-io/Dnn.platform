// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Helpers
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    public class SearchTextFilter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static string CleanWildcards(string searchText)
        {
			Logger.LogDebug(">>> Entering CleanWildcards(searchText={SearchText})", searchText);
            if (string.IsNullOrEmpty(searchText) || searchText.Equals("*", StringComparison.Ordinal) || searchText.Equals("%", StringComparison.Ordinal))
            {
                return null;
            }

            var pattern = string.Empty;

			Logger.LogDebug("CleanWildcards(searchText={SearchText}): pattern → {Pattern}", searchText, pattern);
            var prefixWildcard = searchText.StartsWith("%", StringComparison.Ordinal) || searchText.StartsWith("*", StringComparison.Ordinal);
            var suffixWildcard = searchText.EndsWith("%", StringComparison.Ordinal) || searchText.EndsWith("*", StringComparison.Ordinal);
			Logger.LogDebug("CleanWildcards(searchText={SearchText}): prefixWildcard → {PrefixWildcard}", searchText, prefixWildcard);
			Logger.LogDebug("CleanWildcards(searchText={SearchText}): suffixWildcard → {SuffixWildcard}", searchText, suffixWildcard);

            bool inString = prefixWildcard && suffixWildcard;
            bool prefix = prefixWildcard && !suffixWildcard;
            bool suffix = suffixWildcard && !prefixWildcard;
            bool exact = !suffixWildcard && !prefixWildcard;

            if (exact)
            {
                pattern = searchText.Replace("*", string.Empty).Replace("%", string.Empty);
            }
            else if (inString)
            {
                pattern = GetInStringSearchPattern(searchText);
            }
            else if (prefix)
            {
                pattern = GetPrefixSearchPattern(searchText);
            }
            else
            {
                pattern = GetSuffixSearchPattern(searchText);
            }

			Logger.LogDebug("<<< Exiting CleanWildcards(searchText={SearchText})", searchText);
            return pattern;
        }

        private static string GetInStringSearchPattern(string searchText)
        {
			Logger.LogDebug(">>> Entering GetInStringSearchPattern(searchText={SearchText})", searchText);
            var pattern = new StringBuilder();
            var regex = new Regex(@"^(\*|%)?([\w\-_\s\*\%\.\@]+)(\*|%)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			Logger.LogDebug("GetInStringSearchPattern(searchText={SearchText}): pattern → {Pattern}", searchText, pattern);
			Logger.LogDebug("GetInStringSearchPattern(searchText={SearchText}): regex → {Regex}", searchText, regex);
            var matches = regex.Matches(searchText);

			Logger.LogDebug("GetInStringSearchPattern(searchText={SearchText}): matches → {Matches}", searchText, matches);
            if (matches.Count > 0)
            {
                var matchText = matches[0].Groups[2].Value;
                if (matchText != null && !string.IsNullOrEmpty(matchText))
                {
                    pattern.Append('%');
                    pattern.Append(matchText.Replace("*", string.Empty).Replace("%", string.Empty));
                    pattern.Append('%');
                }
            }

			Logger.LogDebug("<<< Exiting GetInStringSearchPattern(searchText={SearchText})", searchText);
            return pattern.ToString();
        }

        private static string GetPrefixSearchPattern(string searchText)
        {
			Logger.LogDebug(">>> Entering GetPrefixSearchPattern(searchText={SearchText})", searchText);
            var pattern = new StringBuilder();
            var regex = new Regex(@"^(\*|%)?([\w\-_\s\*\%\.\@]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			Logger.LogDebug("GetPrefixSearchPattern(searchText={SearchText}): pattern → {Pattern}", searchText, pattern);
			Logger.LogDebug("GetPrefixSearchPattern(searchText={SearchText}): regex → {Regex}", searchText, regex);
            var matches = regex.Matches(searchText);

			Logger.LogDebug("GetPrefixSearchPattern(searchText={SearchText}): matches → {Matches}", searchText, matches);
            if (matches.Count > 0)
            {
                var matchText = matches[0].Groups[2].Value;
                if (matchText != null && !string.IsNullOrEmpty(matchText))
                {
                    pattern.Append('%');
                    pattern.Append(matchText.Replace("*", string.Empty).Replace("%", string.Empty));
                }
            }

			Logger.LogDebug("<<< Exiting GetPrefixSearchPattern(searchText={SearchText})", searchText);
            return pattern.ToString();
        }

        private static string GetSuffixSearchPattern(string searchText)
        {
			Logger.LogDebug(">>> Entering GetSuffixSearchPattern(searchText={SearchText})", searchText);
            var pattern = new StringBuilder();
            var regex = new Regex(@"([\w\-_\*\s\%\.\@]+)(\*|%)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			Logger.LogDebug("GetSuffixSearchPattern(searchText={SearchText}): pattern → {Pattern}", searchText, pattern);
			Logger.LogDebug("GetSuffixSearchPattern(searchText={SearchText}): regex → {Regex}", searchText, regex);
            var matches = regex.Matches(searchText);

			Logger.LogDebug("GetSuffixSearchPattern(searchText={SearchText}): matches → {Matches}", searchText, matches);
            if (matches.Count > 0)
            {
                var matchText = matches[0].Groups[1].Value;
                if (matchText != null && !string.IsNullOrEmpty(matchText))
                {
                    pattern.Append(matchText.Replace("*", string.Empty).Replace("%", string.Empty));
                    pattern.Append('%');
                }
            }

			Logger.LogDebug("<<< Exiting GetSuffixSearchPattern(searchText={SearchText})", searchText);
            return pattern.ToString();
        }
    }
}
