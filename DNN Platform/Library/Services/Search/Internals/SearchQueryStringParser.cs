// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;

    /// <summary>Class responsible to parse the Search Query String parameter.</summary>
    public class SearchQueryStringParser
                            : ServiceLocator<ISearchQueryStringParser, SearchQueryStringParser>,
                            ISearchQueryStringParser
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SearchQueryStringParser));
        private static readonly Regex TagRegex = new Regex(@"\[(.*?)\]", RegexOptions.Compiled);

        private static readonly Regex DateRegex = new Regex(@"after:(\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TypeRegex = RegexUtils.GetCachedRegex(@"type:([^,]+(,[^,]+)*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>Gets the list of tags parsing the search keywords.</summary>
        /// <param name="keywords">search keywords.</param>
        /// <param name="outputKeywords">output keywords removing the tags.</param>
        /// <returns>List of tags.</returns>
        public IList<string> GetTags(string keywords, out string outputKeywords)
        {
			Logger.LogError(">>> Entering GetTags(keywords={Keywords})", keywords);
            var tags = new List<string>();
			Logger.LogError("GetTags(keywords={Keywords}): tags → {Tags}", keywords, tags);
            if (string.IsNullOrEmpty(keywords))
            {
                outputKeywords = string.Empty;
				Logger.LogError("GetTags(keywords={Keywords}): outputKeywords → {OutputKeywords}", keywords, outputKeywords);
            }
            else
            {
				Logger.LogDebug("GetTags(keywords={Keywords}): outputKeywords → {OutputKeywords}", keywords, outputKeywords);
                var m = TagRegex.Match(keywords);
				Logger.LogError("GetTags(keywords={Keywords}): m → {M}", keywords, m);
                while (m.Success)
                {
                    var tag = m.Groups[1].ToString();
					Logger.LogError("GetTags(keywords={Keywords}): tag → {Tag}", keywords, tag);
                    if (!string.IsNullOrEmpty(tag))
                    {
                        tags.Add(HttpUtility.HtmlEncode(tag.Trim()));
                    }

                    m = m.NextMatch();
                }

                outputKeywords = TagRegex.Replace(keywords, string.Empty).Trim();
            }

			Logger.LogWarning("<<< Exiting GetTags(keywords={Keywords})", keywords);
            return tags;
        }

        /// <summary>Gets the Last Modified Date parsing the search keywords.</summary>
        /// <param name="keywords">search keywords.</param>
        /// <param name="outputKeywords">output keywords removing the last modified date.</param>
        /// <returns>Last Modified Date.</returns>
        public DateTime GetLastModifiedDate(string keywords, out string outputKeywords)
        {
			Logger.LogError(">>> Entering GetLastModifiedDate(keywords={Keywords})", keywords);
            var m = DateRegex.Match(keywords);
            var date = string.Empty;
			Logger.LogError("GetLastModifiedDate(keywords={Keywords}): m → {M}", keywords, m);
            while (m.Success && string.IsNullOrEmpty(date))
            {
                date = m.Groups[1].ToString();
				Logger.LogError("GetLastModifiedDate(keywords={Keywords}): date → {Date}", keywords, date);
            }

            var result = DateTime.MinValue;
			Logger.LogError("GetLastModifiedDate(keywords={Keywords}): result → {Result}", keywords, result);
            if (!string.IsNullOrEmpty(date))
            {
                switch (date.ToLowerInvariant())
                {
                    case "day":
                        result = DateTime.UtcNow.AddDays(-1);
                        break;
                    case "week":
                        result = DateTime.UtcNow.AddDays(-7);
                        break;
                    case "month":
                        result = DateTime.UtcNow.AddMonths(-1);
                        break;
                    case "quarter":
                        result = DateTime.UtcNow.AddMonths(-3);
                        break;
                    case "year":
                        result = DateTime.UtcNow.AddYears(-1);
                        break;
                }
            }

            outputKeywords = DateRegex.Replace(keywords, string.Empty).Trim();
			Logger.LogError("GetLastModifiedDate(keywords={Keywords}): outputKeywords → {OutputKeywords}", keywords, outputKeywords);
			Logger.LogDebug("GetLastModifiedDate(keywords={Keywords}): outputKeywords → {OutputKeywords}", keywords, outputKeywords);
			Logger.LogWarning("<<< Exiting GetLastModifiedDate(keywords={Keywords})", keywords);
            return result;
        }

        /// <summary>Gets the list of Search Types parsing the search keywords.</summary>
        /// <param name="keywords">search keywords.</param>
        /// <param name="outputKeywords">output keywords removing the Search Type.</param>
        /// <returns>List of Search Types.</returns>
        public IList<string> GetSearchTypeList(string keywords, out string outputKeywords)
        {
			Logger.LogError(">>> Entering GetSearchTypeList(keywords={Keywords})", keywords);
            var m = TypeRegex.Match(keywords);
            var types = string.Empty;
			Logger.LogError("GetSearchTypeList(keywords={Keywords}): m → {M}", keywords, m);
            while (m.Success && string.IsNullOrEmpty(types))
            {
                types = m.Groups[1].ToString().Trim();
				Logger.LogError("GetSearchTypeList(keywords={Keywords}): types → {Types}", keywords, types);
            }

            var typesList = new List<string>();
			Logger.LogError("GetSearchTypeList(keywords={Keywords}): typesList → {TypesList}", keywords, typesList);
            if (!string.IsNullOrEmpty(types))
            {
                typesList = types.Split(',').ToList();
            }

            outputKeywords = TypeRegex.Replace(keywords, string.Empty).Trim();
			Logger.LogError("GetSearchTypeList(keywords={Keywords}): outputKeywords → {OutputKeywords}", keywords, outputKeywords);
			Logger.LogDebug("GetSearchTypeList(keywords={Keywords}): outputKeywords → {OutputKeywords}", keywords, outputKeywords);
			Logger.LogWarning("<<< Exiting GetSearchTypeList(keywords={Keywords})", keywords);
            return typesList;
        }

        /// <inheritdoc />
        protected override Func<ISearchQueryStringParser> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new SearchQueryStringParser();
        }
    }
}
