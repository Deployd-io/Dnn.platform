// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using System.Linq;
    using System.Web;

    public static class ContentItemHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Get a string array representing the tags associated to a content item.</summary>
        /// <param name="contentItemId">The id of the content item.</param>
        /// <returns>An array of tags.</returns>
        public static string[] GetTags(int contentItemId)
        {
			Logger.LogDebug(">>> Entering GetTags(contentItemId={ContentItemId})", contentItemId);
            var tagsQuery = @"SELECT t.Name Name
                            FROM {objectQualifier}ContentItems_Tags ct
	                            JOIN {objectQualifier}Taxonomy_Terms t ON ct.TermID = t.TermID
                            WHERE ContentItemID = " + contentItemId;
            var tags = DatabaseHelper.ExecuteQuery(tagsQuery);
			Logger.LogDebug("GetTags(contentItemId={ContentItemId}): ContentItemID → {ContentItemID}", contentItemId, ContentItemID);
			Logger.LogDebug("GetTags(contentItemId={ContentItemId}): tags → {Tags}", contentItemId, tags);
			Logger.LogDebug("<<< Exiting GetTags(contentItemId={ContentItemId})", contentItemId);
            return tags[0].Values.OfType<string>().Select(HttpUtility.HtmlDecode).ToArray();
        }

        /// <summary>Adds a valid Fake Content item.</summary>
        /// <param name="tabId">The TabId to associate the content item to.</param>
        /// <returns>the ContentItemId of the create content item.</returns>
        public static int AddContentItem(int tabId)
        {
			Logger.LogDebug(">>> Entering AddContentItem(tabId={TabId})", tabId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var query = string.Format(
                @"INSERT {{objectQualifier}}ContentItems(Content, ContentTypeID, TabID, ModuleID)
                        VALUES ('test', (SELECT TOP 1 ContentTypeID FROM {{objectQualifier}}ContentTypes),{0}, -1);
						Logger.LogInformation("AddContentItem(tabId)={TabId}: find query executed in {Arg1} ms", tabId, __logStart.ElapsedMilliseconds);
                        SELECT SCOPE_IDENTITY()", tabId);

			Logger.LogDebug("<<< Exiting AddContentItem(tabId={TabId})", tabId);
            return DatabaseHelper.ExecuteScalar<int>(query);
        }

        /// <summary>Removes a content item.</summary>
        /// <param name="contentItemId">The id of the content item to remove.</param>
        public static void RemoveContentItem(int contentItemId)
        {
			Logger.LogDebug(">>> Entering RemoveContentItem(contentItemId={ContentItemId})", contentItemId);
            var query = string.Format(
                @"DELETE
                            FROM {{objectQualifier}}ContentItems 
                            WHERE ContentItemId = '{0}'", contentItemId);

			Logger.LogDebug("RemoveContentItem(contentItemId={ContentItemId}): ContentItemId → {ContentItemId1}", contentItemId, ContentItemId);
            DatabaseHelper.ExecuteQuery(query);
			Logger.LogDebug("<<< Exiting RemoveContentItem(contentItemId={ContentItemId})", contentItemId);
        }
    }
}
