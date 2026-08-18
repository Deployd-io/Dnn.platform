// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using System.Linq;
    using System.Web;
	using Microsoft.Extensions.Logging;

    public static class ContentItemHelper
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>Get a string array representing the tags associated to a content item.</summary>
        /// <param name="contentItemId">The id of the content item.</param>
        /// <returns>An array of tags.</returns>
        public static string[] GetTags(int contentItemId)
        {
			_logger.LogDebug(">>> Entering GetTags(contentItemId={ContentItemId})", contentItemId);
            var tagsQuery = @"SELECT t.Name Name
                            FROM {objectQualifier}ContentItems_Tags ct
	                            JOIN {objectQualifier}Taxonomy_Terms t ON ct.TermID = t.TermID
                            WHERE ContentItemID = " + contentItemId;
							_logger.LogDebug("GetTags(contentItemId={ContentItemId}): ContentItemID → {ContentItemID}", contentItemId, ContentItemID);
            var tags = DatabaseHelper.ExecuteQuery(tagsQuery);
			_logger.LogDebug("GetTags(contentItemId={ContentItemId}): tags → {Tags}", contentItemId, tags);
			_logger.LogDebug("<<< Exiting GetTags(contentItemId={ContentItemId})", contentItemId);
            return tags[0].Values.OfType<string>().Select(HttpUtility.HtmlDecode).ToArray();
        }

        /// <summary>Adds a valid Fake Content item.</summary>
        /// <param name="tabId">The TabId to associate the content item to.</param>
        /// <returns>the ContentItemId of the create content item.</returns>
        public static int AddContentItem(int tabId)
        {
            var query = string.Format(
                @"INSERT {{objectQualifier}}ContentItems(Content, ContentTypeID, TabID, ModuleID)
                        VALUES ('test', (SELECT TOP 1 ContentTypeID FROM {{objectQualifier}}ContentTypes),{0}, -1);
                        SELECT SCOPE_IDENTITY()", tabId);

			_logger.LogDebug("<<< Exiting AddContentItem(tabId={TabId})", tabId);
            return DatabaseHelper.ExecuteScalar<int>(query);
        }

        /// <summary>Removes a content item.</summary>
        /// <param name="contentItemId">The id of the content item to remove.</param>
        public static void RemoveContentItem(int contentItemId)
        {
			_logger.LogDebug(">>> Entering RemoveContentItem(contentItemId={ContentItemId})", contentItemId);
            var query = string.Format(
                @"DELETE
                            FROM {{objectQualifier}}ContentItems 
                            WHERE ContentItemId = '{0}'", contentItemId);

            DatabaseHelper.ExecuteQuery(query);
			_logger.LogDebug("RemoveContentItem(contentItemId={ContentItemId}): ContentItemId → {ContentItemId1}", contentItemId, ContentItemId);
			_logger.LogDebug("<<< Exiting RemoveContentItem(contentItemId={ContentItemId})", contentItemId);
        }
    }
}
