// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Utilities
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using DNNConnect.CKEditorProvider.Objects;

    /// <summary>Finds, and extract all Anchors from a Page.</summary>
    public static class AnchorFinder
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Extract the Anchors.</summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <returns>
        /// All Anchors on the Page.
        /// </returns>
        public static List<LinkItem> ListAll(string file)
        {
			Logger.LogDebug(">>> Entering ListAll(file={File})", file);
            var anchorList = new List<LinkItem>();

			Logger.LogDebug("ListAll(file={File}): anchorList → {AnchorList}", file, anchorList);
            MatchCollection m1 = Regex.Matches(file, @"(<a.*?>.*?</a>)", RegexOptions.IgnoreCase);

			Logger.LogDebug("ListAll(file={File}): m1 → {M1}", file, m1);
            foreach (Match m in m1)
            {
                string value = m.Groups[1].Value;
                LinkItem item = new LinkItem();

				Logger.LogDebug("ListAll(file={File}): item → {Item}", file, item);
                Match m2 = Regex.Match(value, @"href=\""(.*?)\""", RegexOptions.Singleline);

				Logger.LogDebug("ListAll(file={File}): m2 → {M2}", file, m2);
                if (m2.Success)
                {
                    item.Href = m2.Groups[1].Value;
					Logger.LogDebug("ListAll(file={File}): item.Href → {Arg1}", file, item.Href);
                }

                Match m3 = Regex.Match(value, @"name=\""(.*?)\""", RegexOptions.Singleline);

				Logger.LogDebug("ListAll(file={File}): m3 → {M3}", file, m3);
                if (m3.Success)
                {
                    item.Anchor = m3.Groups[1].Value;
					Logger.LogDebug("ListAll(file={File}): item.Anchor → {Arg1}", file, item.Anchor);
                }

                item.Text = Regex.Replace(value, @"\s*<.*?>\s*", string.Empty, RegexOptions.Singleline);

                anchorList.Add(item);
            }

			Logger.LogDebug("<<< Exiting ListAll(file={File})", file);
            return anchorList;
        }
    }
}
