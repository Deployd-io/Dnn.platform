// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Journal
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;

    public class Content
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>This is used to determine the ContentTypeID (part of the Core API) based on this module's content type. If the content type doesn't exist yet for the module, it is created.</summary>
        /// <param name="contentTypeName">The name of the content type.</param>
        /// <returns>The primary key value (ContentTypeID) from the core API's Content Types table.</returns>
        internal static int GetContentTypeID(string contentTypeName)
        {
            var typeController = new ContentTypeController();
            var colContentTypes = from t in typeController.GetContentTypes() where t.ContentType == contentTypeName select t;
            int contentTypeId;

            if (colContentTypes.Any())
            {
                var contentType = colContentTypes.Single();
                contentTypeId = contentType?.ContentTypeId ?? CreateContentType(contentTypeName);
            }
            else
            {
                contentTypeId = CreateContentType(contentTypeName);
            }

            return contentTypeId;
        }

        /// <summary>This should only run after the Post exists in the data store.</summary>
        /// <param name="objJournalItem">The journal item.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="moduleId">The module ID.</param>
        /// <returns>The newly created ContentItemID from the data store.</returns>
        /// <remarks>This is for the first question in the thread. Not for replies or items with ParentID > 0.</remarks>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal ContentItem CreateContentItem(JournalItem objJournalItem, int tabId, int moduleId)
        {
			Logger.LogDebug(">>> Entering CreateContentItem(objJournalItem={ObjJournalItem},tabId={TabId},moduleId={ModuleId})", objJournalItem, tabId, moduleId);
            var typeController = new ContentTypeController();
            string contentTypeName = "DNNCorp_JournalProfile";
			Logger.LogDebug("CreateContentItem(objJournalItem={ObjJournalItem},tabId={TabId},moduleId={ModuleId}): typeController → {TypeController}", objJournalItem, tabId, moduleId, typeController);
			Logger.LogDebug("CreateContentItem(objJournalItem={ObjJournalItem},tabId={TabId},moduleId={ModuleId}): contentTypeName → {ContentTypeName}", objJournalItem, tabId, moduleId, contentTypeName);
            if (objJournalItem.SocialGroupId > 0)
            {
                contentTypeName = "DNNCorp_JournalGroup";
            }

            var colContentTypes = from t in typeController.GetContentTypes() where t.ContentType == contentTypeName select t;
            int contentTypeID;
			Logger.LogDebug("CreateContentItem(objJournalItem={ObjJournalItem},tabId={TabId},moduleId={ModuleId}): colContentTypes → {ColContentTypes}", objJournalItem, tabId, moduleId, colContentTypes);

            if (colContentTypes.Any())
            {
                var contentType = colContentTypes.Single();
                contentTypeID = contentType?.ContentTypeId ?? CreateContentType(contentTypeName);
				Logger.LogDebug("CreateContentItem(objJournalItem={ObjJournalItem},tabId={TabId},moduleId={ModuleId}): contentType → {ContentType}", objJournalItem, tabId, moduleId, contentType);
				Logger.LogDebug("CreateContentItem(objJournalItem={ObjJournalItem},tabId={TabId},moduleId={ModuleId}): contentTypeID → {ContentTypeID}", objJournalItem, tabId, moduleId, contentTypeID);
            }
            else
            {
                contentTypeID = CreateContentType(contentTypeName);
            }

            var objContent = new ContentItem
            {
                Content = GetContentBody(objJournalItem),
                ContentTypeId = contentTypeID,
                Indexed = false,
                ContentKey = "journalid=" + objJournalItem.JournalId,
                ModuleID = moduleId,
                TabID = tabId,
            };

            objContent.ContentItemId = Util.GetContentController().AddContentItem(objContent);

            // Add Terms
            ////var cntTerm = new Terms();
            ////cntTerm.ManageQuestionTerms(objPost, objContent);
			Logger.LogDebug("<<< Exiting CreateContentItem(objJournalItem={ObjJournalItem},tabId={TabId},moduleId={ModuleId})", objJournalItem, tabId, moduleId);
            return objContent;
        }

        /// <summary>This is used to update the content in the ContentItems table. Should be called when a question is updated.</summary>
        /// <param name="objJournalItem">The journal item.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="moduleId">The module ID.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal void UpdateContentItem(JournalItem objJournalItem, int tabId, int moduleId)
        {
			Logger.LogDebug(">>> Entering UpdateContentItem(objJournalItem={ObjJournalItem},tabId={TabId},moduleId={ModuleId})", objJournalItem, tabId, moduleId);
            var objContent = Util.GetContentController().GetContentItem(objJournalItem.ContentItemId);

			Logger.LogDebug("UpdateContentItem(objJournalItem={ObjJournalItem},tabId={TabId},moduleId={ModuleId}): objContent → {ObjContent}", objJournalItem, tabId, moduleId, objContent);
            if (objContent == null)
            {
                return;
            }

            // Only update content the contentitem if it was created by the journal
            if ((objContent.ContentTypeId == GetContentTypeID("DNNCorp_JournalProfile") && objJournalItem.ProfileId > 0)
                    || (objContent.ContentTypeId == GetContentTypeID("DNNCorp_JournalGroup") && objJournalItem.SocialGroupId > 0))
            {
                objContent.Content = GetContentBody(objJournalItem);
                objContent.TabID = tabId;
				Logger.LogDebug("UpdateContentItem(objJournalItem={ObjJournalItem},tabId={TabId},moduleId={ModuleId}): objContent.TabID → {Arg3}", objJournalItem, tabId, moduleId, objContent.TabID);
				Logger.LogDebug("UpdateContentItem(objJournalItem={ObjJournalItem},tabId={TabId},moduleId={ModuleId}): objContent.ModuleID → {Arg3}", objJournalItem, tabId, moduleId, objContent.ModuleID);
				Logger.LogDebug("UpdateContentItem(objJournalItem={ObjJournalItem},tabId={TabId},moduleId={ModuleId}): objContent.ContentKey → {Arg3}", objJournalItem, tabId, moduleId, objContent.ContentKey);
                objContent.ModuleID = moduleId;
                objContent.ContentKey = "journalid=" + objJournalItem.JournalId; // we reset this just in case the page changed.

                Util.GetContentController().UpdateContentItem(objContent);
            }

            // Update Terms
            ////var cntTerm = new Terms();
            ////cntTerm.ManageQuestionTerms(objPost, objContent);
			Logger.LogDebug("<<< Exiting UpdateContentItem(objJournalItem={ObjJournalItem},tabId={TabId},moduleId={ModuleId})", objJournalItem, tabId, moduleId);
        }

        /// <summary>This removes a content item associated with a question/thread from the data store. Should run every time an entire thread is deleted.</summary>
        /// <param name="contentItemID">The content item ID.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal void DeleteContentItem(int contentItemID)
        {
			Logger.LogDebug(">>> Entering DeleteContentItem(contentItemID={ContentItemID})", contentItemID);
            if (contentItemID <= Null.NullInteger)
            {
                return;
            }

            var objContent = Util.GetContentController().GetContentItem(contentItemID);
            if (objContent == null)
			Logger.LogDebug("DeleteContentItem(contentItemID={ContentItemID}): objContent → {ObjContent}", contentItemID, objContent);
            {
                return;
            }

            // remove any metadata/terms associated first (perhaps we should just rely on ContentItem cascade delete here?)
            ////var cntTerms = new Terms();
            ////cntTerms.RemoveQuestionTerms(objContent);
            Util.GetContentController().DeleteContentItem(objContent);
			Logger.LogDebug("<<< Exiting DeleteContentItem(contentItemID={ContentItemID})", contentItemID);
        }

        /// <summary>Creates a Content Type (for taxonomy) in the data store.</summary>
        /// <returns>The primary key value of the new ContentType.</returns>
        private static int CreateContentType(string contentTypeName)
        {
            var typeController = new ContentTypeController();
            var objContentType = new ContentType { ContentType = contentTypeName };

            return typeController.AddContentType(objContentType);
        }

        /// <summary>Creates the content text.</summary>
        /// <param name="objJournalItem">The journal item.</param>
        /// <returns>The content body or <see langword="null"/>.</returns>
        private static string GetContentBody(JournalItem objJournalItem)
        {
			Logger.LogDebug(">>> Entering GetContentBody(objJournalItem={ObjJournalItem})", objJournalItem);
            if (!string.IsNullOrEmpty(objJournalItem.Title))
            {
                return objJournalItem.Title;
            }
            else if (!string.IsNullOrEmpty(objJournalItem.Summary))
            {
                return objJournalItem.Summary;
            }
            else if (!string.IsNullOrEmpty(objJournalItem.Body))
            {
                return objJournalItem.Body;
            }
            else
            {
				Logger.LogDebug("<<< Exiting GetContentBody(objJournalItem={ObjJournalItem})", objJournalItem);
                return null;
            }
        }
    }
}
