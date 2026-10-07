// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.Journal.Components;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A web API controller for the Journal module.</summary>
    /// <param name="hostSettings">The host settings.</param>
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    [SupportedModules("Journal")]
    public class ServicesController(IHostSettings hostSettings)
        : DnnApiController
    {
        private const string MentionNotificationSuffix = "...";
        private const string MentionIdentityChar = "@";
        private const int MentionNotificationLength = 100;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServicesController));
        private static readonly string[] AcceptedFileExtensions = ["jpg", "png", "gif", "jpe", "jpeg", "tiff", "bmp",];

        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="ServicesController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ServicesController()
            : this(null)
        {
		Logger.LogError(">>> Entering ServicesController()");
        Logger.LogWarning("<<< Exiting ServicesController()");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(DenyRoles = "Unverified Users")]
        public HttpResponseMessage Create(CreateDTO postData)
        {
			Logger.LogError(">>> Entering Create(postData={PostData})", postData);
            try
            {
                int userId = this.UserInfo.UserID;
				Logger.LogError("Create(postData={PostData}): userId → {UserId}", postData, userId);
                IDictionary<string, UserInfo> mentionedUsers = new Dictionary<string, UserInfo>();
				Logger.LogError("Create(postData={PostData}): mentionedUsers → {MentionedUsers}", postData, mentionedUsers);

                if (postData.ProfileId == -1)
                {
                    postData.ProfileId = userId;
					Logger.LogDebug("Create(postData={PostData}): postData.ProfileId → {Arg1}", postData, postData.ProfileId);
                }

                this.CheckProfileAccess(postData.ProfileId, this.UserInfo);

                this.CheckGroupAccess(postData);

                var journalItem = this.PrepareJournalItem(postData, mentionedUsers);
				Logger.LogError("Create(postData={PostData}): journalItem → {JournalItem}", postData, journalItem);

                JournalController.Instance.SaveJournalItem(journalItem, this.ActiveModule);

                var originalSummary = journalItem.Summary;
                this.SendMentionNotifications(mentionedUsers, journalItem, originalSummary);

                return this.Request.CreateResponse(HttpStatusCode.OK, journalItem);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting Create(postData={PostData})", postData);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(DenyRoles = "Unverified Users")]
        public HttpResponseMessage Delete(JournalIdDTO postData)
        {
			Logger.LogError(">>> Entering Delete(postData={PostData})", postData);
            try
            {
                var jc = JournalController.Instance;
                var ji = jc.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
				Logger.LogError("Delete(postData={PostData}): ji → {Ji}", postData, ji);

                if (ji == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "invalid request");
                }

                if (ji.UserId == this.UserInfo.UserID || ji.ProfileId == this.UserInfo.UserID || this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
                {
                    jc.DeleteJournalItem(this.PortalSettings.PortalId, this.UserInfo.UserID, postData.JournalId);
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                }

                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting Delete(postData={PostData})", postData);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(DenyRoles = "Unverified Users")]
        public HttpResponseMessage SoftDelete(JournalIdDTO postData)
        {
			Logger.LogError(">>> Entering SoftDelete(postData={PostData})", postData);
            try
            {
                var jc = JournalController.Instance;
                var ji = jc.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
				Logger.LogError("SoftDelete(postData={PostData}): ji → {Ji}", postData, ji);

                if (ji == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "invalid request");
                }

                if (ji.UserId == this.UserInfo.UserID || ji.ProfileId == this.UserInfo.UserID || this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
                {
                    jc.SoftDeleteJournalItem(this.PortalSettings.PortalId, this.UserInfo.UserID, postData.JournalId);
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                }

                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting SoftDelete(postData={PostData})", postData);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize]
        public HttpResponseMessage PreviewUrl(PreviewDTO postData)
        {
			Logger.LogError(">>> Entering PreviewUrl(postData={PostData})", postData);
            try
            {
                var link = Utilities.GetLinkData(postData.Url);
				Logger.LogError("PreviewUrl(postData={PostData}): link → {Link}", postData, link);
                return this.Request.CreateResponse(HttpStatusCode.OK, link);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting PreviewUrl(postData={PostData})", postData);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage GetListForProfile(GetListForProfileDTO postData)
        {
			Logger.LogError(">>> Entering GetListForProfile(postData={PostData})", postData);
            try
            {
                var jp = new JournalParser(this.PortalSettings, this.ActiveModule.ModuleID, postData.ProfileId, postData.GroupId, this.UserInfo);
				Logger.LogError("GetListForProfile(postData={PostData}): jp → {Jp}", postData, jp);
				Logger.LogWarning("<<< Exiting GetListForProfile(postData={PostData})", postData);
                return this.Request.CreateResponse(HttpStatusCode.OK, jp.GetList(postData.RowIndex, postData.MaxRows), "text/html");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                throw new HttpException(500, exc.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(DenyRoles = "Unverified Users")]
        public HttpResponseMessage Like(JournalIdDTO postData)
        {
			Logger.LogError(">>> Entering Like(postData={PostData})", postData);
            try
            {
                JournalController.Instance.LikeJournalItem(postData.JournalId, this.UserInfo.UserID, this.UserInfo.DisplayName);
                var ji = JournalController.Instance.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
				Logger.LogError("Like(postData={PostData}): ji → {Ji}", postData, ji);
                var jp = new JournalParser(this.PortalSettings, this.ActiveModule.ModuleID, ji.ProfileId, -1, this.UserInfo);
				Logger.LogError("Like(postData={PostData}): jp → {Jp}", postData, jp);
                var isLiked = false;
                var likeList = jp.GetLikeListHTML(ji, ref isLiked);
                likeList = Utilities.LocalizeControl(likeList);
				Logger.LogError("Like(postData={PostData}): likeList → {LikeList}", postData, likeList);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { LikeList = likeList, Liked = isLiked });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting Like(postData={PostData})", postData);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(DenyRoles = "Unverified Users")]
        public HttpResponseMessage CommentSave(CommentSaveDTO postData)
        {
			Logger.LogError(">>> Entering CommentSave(postData={PostData})", postData);
            try
            {
                var comment = Utilities.RemoveHTML(HttpUtility.UrlDecode(postData.Comment));
				Logger.LogError("CommentSave(postData={PostData}): comment → {Comment}", postData, comment);

                IDictionary<string, UserInfo> mentionedUsers = new Dictionary<string, UserInfo>();
				Logger.LogError("CommentSave(postData={PostData}): mentionedUsers → {MentionedUsers}", postData, mentionedUsers);
                var originalComment = comment;
                comment = this.ParseMentions(comment, postData.Mentions, ref mentionedUsers);
                var ci = new CommentInfo { JournalId = postData.JournalId, Comment = comment };
                ci.UserId = this.UserInfo.UserID;
				Logger.LogDebug("CommentSave(postData={PostData}): ci.UserId → {Arg1}", postData, ci.UserId);
                ci.DisplayName = this.UserInfo.DisplayName;
				Logger.LogDebug("CommentSave(postData={PostData}): ci.DisplayName → {Arg1}", postData, ci.DisplayName);
                JournalController.Instance.SaveComment(ci);

                var ji = JournalController.Instance.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
				Logger.LogError("CommentSave(postData={PostData}): ji → {Ji}", postData, ji);
                var jp = new JournalParser(this.PortalSettings, this.ActiveModule.ModuleID, ji.ProfileId, -1, this.UserInfo);
				Logger.LogError("CommentSave(postData={PostData}): jp → {Jp}", postData, jp);

                this.SendMentionNotifications(mentionedUsers, ji, originalComment, "Comment");

                return this.Request.CreateResponse(HttpStatusCode.OK, jp.GetCommentRow(ji, ci), "text/html");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting CommentSave(postData={PostData})", postData);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(DenyRoles = "Unverified Users")]
        public HttpResponseMessage CommentDelete(CommentDeleteDTO postData)
        {
			Logger.LogError(">>> Entering CommentDelete(postData={PostData})", postData);
            try
            {
                var ci = JournalController.Instance.GetComment(postData.CommentId);
				Logger.LogError("CommentDelete(postData={PostData}): ci → {Ci}", postData, ci);
                if (ci == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "delete failed");
                }

                var ji = JournalController.Instance.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
				Logger.LogError("CommentDelete(postData={PostData}): ji → {Ji}", postData, ji);

                if (ji == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "invalid request");
                }

                if (ci.UserId == this.UserInfo.UserID || ji.UserId == this.UserInfo.UserID || this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
                {
                    JournalController.Instance.DeleteComment(postData.JournalId, postData.CommentId);
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                }

                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting CommentDelete(postData={PostData})", postData);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        [DnnAuthorize(DenyRoles = "Unverified Users")]
        public HttpResponseMessage GetSuggestions(string keyword)
        {
			Logger.LogError(">>> Entering GetSuggestions(keyword={Keyword})", keyword);
            try
            {
                var findedUsers = new List<SuggestDTO>();
				Logger.LogError("GetSuggestions(keyword={Keyword}): findedUsers → {FindedUsers}", keyword, findedUsers);
                var relations = RelationshipController.Instance.GetUserRelationships(this.UserInfo);
				Logger.LogError("GetSuggestions(keyword={Keyword}): relations → {Relations}", keyword, relations);
                foreach (var ur in relations)
                {
                    var targetUserId = ur.UserId == this.UserInfo.UserID ? ur.RelatedUserId : ur.UserId;
                    var targetUser = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, targetUserId);
					Logger.LogError("GetSuggestions(keyword={Keyword}): targetUser → {TargetUser}", keyword, targetUser);
                    var relationship = RelationshipController.Instance.GetRelationship(ur.RelationshipId);
					Logger.LogError("GetSuggestions(keyword={Keyword}): relationship → {Relationship}", keyword, relationship);
                    if (ur.Status == RelationshipStatus.Accepted && targetUser != null
                        && ((relationship.RelationshipTypeId == (int)DefaultRelationshipTypes.Followers && ur.RelatedUserId == this.UserInfo.UserID)
                                || relationship.RelationshipTypeId == (int)DefaultRelationshipTypes.Friends)
                        && (targetUser.DisplayName.ToLowerInvariant().Contains(keyword.ToLowerInvariant())
                                || targetUser.DisplayName.ToLowerInvariant().Contains(keyword.Replace("-", " ").ToLowerInvariant()))
                        && findedUsers.All(s => s.userId != targetUser.UserID))
                    {
                        findedUsers.Add(new SuggestDTO
                        {
                            displayName = targetUser.DisplayName.Replace(" ", "-"),
                            userId = targetUser.UserID,
                            avatar = targetUser.Profile.PhotoURL,
                            key = keyword,
                        });
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, findedUsers.Cast<object>().Take(5));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetSuggestions(keyword={Keyword})", keyword);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static bool IsImageFile(string relativePath)
        {
			Logger.LogError(">>> Entering IsImageFile(relativePath={RelativePath})", relativePath);
            if (relativePath == null)
            {
                return false;
            }

            if (relativePath.Contains("?"))
            {
                relativePath = relativePath.Substring(
                    0,
                    relativePath.IndexOf("?", StringComparison.InvariantCultureIgnoreCase));
            }

            var extension = relativePath.Substring(relativePath.LastIndexOf(
                ".",
                StringComparison.Ordinal) + 1).ToLowerInvariant();
			Logger.LogWarning("<<< Exiting IsImageFile(relativePath={RelativePath})", relativePath);
            return AcceptedFileExtensions.Contains(extension);
        }

        private static bool IsAllowedLink(string url)
        {
			Logger.LogError(">>> Entering IsAllowedLink(url={Url})", url);
            Logger.LogWarning("<<< Exiting IsAllowedLink(url={Url})", url);
            return !string.IsNullOrEmpty(url) && !url.Contains("//");
        }

        // Check if a user can post content on a specific profile's page
        private void CheckProfileAccess(int profileId, UserInfo currentUser)
        {
			Logger.LogError(">>> Entering CheckProfileAccess(profileId={ProfileId},currentUser={CurrentUser})", profileId, currentUser);
            if (profileId != currentUser.UserID)
            {
                var profileUser = UserController.Instance.GetUser(this.PortalSettings.PortalId, profileId);
				Logger.LogError("CheckProfileAccess(profileId={ProfileId},currentUser={CurrentUser}): profileUser → {ProfileUser}", profileId, currentUser, profileUser);
                if (profileUser == null || (!this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName) && !Utilities.AreFriends(profileUser, currentUser)))
                {
					Logger.LogError("Throwing ArgumentException from CheckProfileAccess(profileId={ProfileId},currentUser={CurrentUser})", profileId, currentUser);
                    throw new ArgumentException("you have no permission to post journal on current profile page.");
                }
            }
			Logger.LogWarning("<<< Exiting CheckProfileAccess(profileId={ProfileId},currentUser={CurrentUser})", profileId, currentUser);
        }

        private void CheckGroupAccess(CreateDTO postData)
        {
			Logger.LogError(">>> Entering CheckGroupAccess(postData={PostData})", postData);
            if (postData.GroupId > 0)
            {
                postData.ProfileId = -1;
				Logger.LogDebug("CheckGroupAccess(postData={PostData}): postData.ProfileId → {Arg1}", postData, postData.ProfileId);

                RoleInfo roleInfo = RoleController.Instance.GetRoleById(this.ActiveModule.OwnerPortalID, postData.GroupId);
				Logger.LogError("CheckGroupAccess(postData={PostData}): roleInfo → {RoleInfo}", postData, roleInfo);
                if (roleInfo != null)
                {
                    if (!this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName) && !this.UserInfo.IsInRole(roleInfo.RoleName))
                    {
						Logger.LogError("Throwing ArgumentException from CheckGroupAccess(postData={PostData})", postData);
                        throw new ArgumentException("you have no permission to post journal on current group.");
                    }

                    if (!roleInfo.IsPublic)
                    {
                        postData.SecuritySet = "R";
						Logger.LogDebug("CheckGroupAccess(postData={PostData}): postData.SecuritySet → {Arg1}", postData, postData.SecuritySet);
                    }
                }
            }
			Logger.LogWarning("<<< Exiting CheckGroupAccess(postData={PostData})", postData);
        }

        private JournalItem PrepareJournalItem(CreateDTO postData, IDictionary<string, UserInfo> mentionedUsers)
        {
			Logger.LogError(">>> Entering PrepareJournalItem(postData={PostData},mentionedUsers={MentionedUsers})", postData, mentionedUsers);
            var journalTypeId = 1;
			Logger.LogError("PrepareJournalItem(postData={PostData},mentionedUsers={MentionedUsers}): journalTypeId → {JournalTypeId}", postData, mentionedUsers, journalTypeId);
            switch (postData.JournalType)
            {
                case "link":
                    journalTypeId = 2;
                    break;
                case "photo":
                    journalTypeId = 3;
                    break;
                case "file":
                    journalTypeId = 4;
                    break;
            }

            var ji = new JournalItem
            {
                JournalId = -1,
                JournalTypeId = journalTypeId,
                PortalId = this.ActiveModule.OwnerPortalID,
                UserId = this.UserInfo.UserID,
                SocialGroupId = postData.GroupId,
                ProfileId = postData.ProfileId,
                Summary = postData.Text ?? string.Empty,
                SecuritySet = postData.SecuritySet,
            };
            ji.Title = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(ji.Title));
            ji.Summary = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(ji.Summary));

			Logger.LogWarning("<<< Exiting PrepareJournalItem(postData={PostData},mentionedUsers={MentionedUsers})", postData, mentionedUsers);
            var ps = PortalSecurity.Instance;

#pragma warning disable CS0618 // Type or member is obsolete
            ji.Title = ps.InputFilter(ji.Title, PortalSecurity.FilterFlag.NoScripting);
            ji.Title = Utilities.RemoveHTML(ji.Title);
            ji.Title = ps.InputFilter(ji.Title, PortalSecurity.FilterFlag.NoMarkup);

            ji.Summary = ps.InputFilter(ji.Summary, PortalSecurity.FilterFlag.NoScripting);
            ji.Summary = Utilities.RemoveHTML(ji.Summary);
            ji.Summary = ps.InputFilter(ji.Summary, PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete

            // parse the mentions context in post data
            var originalSummary = ji.Summary;

            ji.Summary = this.ParseMentions(ji.Summary, postData.Mentions, ref mentionedUsers);

            if (ji.Summary.Length > 2000)
            {
                ji.Body = ji.Summary;
				Logger.LogDebug("PrepareJournalItem(postData={PostData},mentionedUsers={MentionedUsers}): ji.Body → {Arg2}", postData, mentionedUsers, ji.Body);
                ji.Summary = null;
				Logger.LogDebug("PrepareJournalItem(postData={PostData},mentionedUsers={MentionedUsers}): ji.Summary → {Arg2}", postData, mentionedUsers, ji.Summary);
            }

            if (!string.IsNullOrEmpty(postData.ItemData))
            {
                ji.ItemData = postData.ItemData.FromJson<ItemData>();
                var originalImageUrl = ji.ItemData.ImageUrl;
                if (!IsImageFile(ji.ItemData.ImageUrl))
                {
                    ji.ItemData.ImageUrl = string.Empty;
                }

                ji.ItemData.Description = HttpUtility.UrlDecode(ji.ItemData.Description);

                if (!IsAllowedLink(ji.ItemData.Url))
                {
                    ji.ItemData.Url = string.Empty;
                }

                if (!string.IsNullOrEmpty(ji.ItemData.Url) && ji.ItemData.Url.StartsWith("fileid="))
                {
                    var fileId = Convert.ToInt32(ji.ItemData.Url.Replace("fileid=", string.Empty).Trim());
					Logger.LogError("PrepareJournalItem(postData={PostData},mentionedUsers={MentionedUsers}): fileId → {FileId}", postData, mentionedUsers, fileId);
                    var file = FileManager.Instance.GetFile(fileId);
					Logger.LogError("PrepareJournalItem(postData={PostData},mentionedUsers={MentionedUsers}): file → {File}", postData, mentionedUsers, file);

                    if (!this.IsCurrentUserFile(file))
                    {
						Logger.LogError("Throwing ArgumentException from PrepareJournalItem(postData={PostData},mentionedUsers={MentionedUsers})", postData, mentionedUsers);
                        throw new ArgumentException("you have no permission to attach files not belongs to you.");
                    }

                    ji.ItemData.Title = file.FileName;
                    ji.ItemData.Url = Globals.LinkClick(ji.ItemData.Url, Null.NullInteger, Null.NullInteger);

                    if (string.IsNullOrEmpty(ji.ItemData.ImageUrl) &&
                        originalImageUrl.ToLowerInvariant().StartsWith("/linkclick.aspx?") &&
                        AcceptedFileExtensions.Contains(file.Extension.ToLowerInvariant()))
                    {
                        ji.ItemData.ImageUrl = originalImageUrl;
                    }
                }
            }

            return ji;
        }

        private string ParseMentions(string content, IList<MentionDTO> mentions, ref IDictionary<string, UserInfo> mentionedUsers)
        {
			Logger.LogError(">>> Entering ParseMentions(content={Content},mentions={Mentions},mentionedUsers={MentionedUsers})", content, mentions, mentionedUsers);
            if (mentions == null || mentions.Count == 0)
            {
                return content;
            }

            foreach (var mention in mentions)
            {
                var user = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, mention.UserId);
				Logger.LogError("ParseMentions(content={Content},mentions={Mentions},mentionedUsers={MentionedUsers}): user → {User}", content, mentions, mentionedUsers, user);

                if (user != null)
                {
                    var relationship = RelationshipController.Instance.GetFollowingRelationship(this.UserInfo, user) ??
                                       RelationshipController.Instance.GetFriendRelationship(this.UserInfo, user);
                    if (relationship is { Status: RelationshipStatus.Accepted })
                    {
                        var userLink =
                            $"""<a href="{Globals.UserProfileURL(user.UserID)}" class="userLink" target="_blank">{MentionIdentityChar + user.DisplayName}</a>""";
                        content = content.Replace(MentionIdentityChar + mention.DisplayName, userLink);

                        mentionedUsers.Add(mention.DisplayName, user);
                    }
                }
            }

			Logger.LogWarning("<<< Exiting ParseMentions(content={Content},mentions={Mentions},mentionedUsers={MentionedUsers})", content, mentions, mentionedUsers);
            return content;
        }

        private void SendMentionNotifications(IDictionary<string, UserInfo> mentionedUsers, JournalItem item, string originalSummary, string type = "Post")
        {
			Logger.LogError(">>> Entering SendMentionNotifications(mentionedUsers={MentionedUsers},item={Item},originalSummary={OriginalSummary},type={Type})", mentionedUsers, item, originalSummary, type);
            // send notification to the mention users
            var subjectTemplate = Utilities.GetSharedResource("Notification_Mention.Subject");
			Logger.LogError("SendMentionNotifications(mentionedUsers={MentionedUsers},item={Item},originalSummary={OriginalSummary},type={Type}): subjectTemplate → {SubjectTemplate}", mentionedUsers, item, originalSummary, type, subjectTemplate);
            var bodyTemplate = Utilities.GetSharedResource("Notification_Mention.Body");
			Logger.LogError("SendMentionNotifications(mentionedUsers={MentionedUsers},item={Item},originalSummary={OriginalSummary},type={Type}): bodyTemplate → {BodyTemplate}", mentionedUsers, item, originalSummary, type, bodyTemplate);
            var mentionType = Utilities.GetSharedResource("Notification_MentionType_" + type);
			Logger.LogError("SendMentionNotifications(mentionedUsers={MentionedUsers},item={Item},originalSummary={OriginalSummary},type={Type}): mentionType → {MentionType}", mentionedUsers, item, originalSummary, type, mentionType);
            var notificationType = DotNetNuke.Services.Social.Notifications.NotificationsController.Instance.GetNotificationType("JournalMention");
			Logger.LogError("SendMentionNotifications(mentionedUsers={MentionedUsers},item={Item},originalSummary={OriginalSummary},type={Type}): notificationType → {NotificationType}", mentionedUsers, item, originalSummary, type, notificationType);

            foreach (var key in mentionedUsers.Keys)
            {
                var mentionUser = mentionedUsers[key];
                var mentionText = originalSummary.Substring(originalSummary.IndexOf(MentionIdentityChar + key, StringComparison.InvariantCultureIgnoreCase));
				Logger.LogError("SendMentionNotifications(mentionedUsers={MentionedUsers},item={Item},originalSummary={OriginalSummary},type={Type}): mentionText → {MentionText}", mentionedUsers, item, originalSummary, type, mentionText);
                if (mentionText.Length > MentionNotificationLength)
                {
                    mentionText = mentionText.Substring(0, MentionNotificationLength) + MentionNotificationSuffix;
                }

                var notification = new Notification
                {
                    Subject = string.Format(subjectTemplate, this.UserInfo.DisplayName, mentionType),
                    Body = string.Format(bodyTemplate, mentionText),
                    NotificationTypeID = notificationType.NotificationTypeId,
                    SenderUserID = this.UserInfo.UserID,
                    IncludeDismissAction = true,
                    Context = string.Format("{0}_{1}", this.UserInfo.UserID, item.JournalId),
                };

                Services.Social.Notifications.NotificationsController.Instance.SendNotification(notification, this.PortalSettings.PortalId, null, new List<UserInfo> { mentionUser });
            }
			Logger.LogWarning("<<< Exiting SendMentionNotifications(mentionedUsers={MentionedUsers},item={Item},originalSummary={OriginalSummary},type={Type})", mentionedUsers, item, originalSummary, type);
        }

        private bool IsCurrentUserFile(IFileInfo file)
        {
			Logger.LogError(">>> Entering IsCurrentUserFile(file={File})", file);
            if (file == null)
            {
                return false;
            }

            var userFolders = this.GetUserFolders();
			Logger.LogError("IsCurrentUserFile(file={File}): userFolders → {UserFolders}", file, userFolders);

			Logger.LogWarning("<<< Exiting IsCurrentUserFile(file={File})", file);
            return userFolders.Any(f => file.FolderId == f.FolderID);
        }

        private IList<IFolderInfo> GetUserFolders()
        {
			Logger.LogError(">>> Entering GetUserFolders()");
            var folders = new List<IFolderInfo>();

			Logger.LogError("GetUserFolders(): folders → {Folders}", folders);
            var userFolder = FolderManager.Instance.GetUserFolder(this.UserInfo);
			Logger.LogError("GetUserFolders(): userFolder → {UserFolder}", userFolder);
            folders.Add(userFolder);
            folders.AddRange(this.GetSubFolders(userFolder));

			Logger.LogWarning("<<< Exiting GetUserFolders()");
            return folders;
        }

        private IList<IFolderInfo> GetSubFolders(IFolderInfo parentFolder)
        {
			Logger.LogError(">>> Entering GetSubFolders(parentFolder={ParentFolder})", parentFolder);
            var folders = new List<IFolderInfo>();
			Logger.LogError("GetSubFolders(parentFolder={ParentFolder}): folders → {Folders}", parentFolder, folders);
            foreach (var folder in FolderManager.Instance.GetFolders(parentFolder))
            {
                folders.Add(folder);
                folders.AddRange(this.GetSubFolders(folder));
            }

			Logger.LogWarning("<<< Exiting GetSubFolders(parentFolder={ParentFolder})", parentFolder);
            return folders;
        }

        public class CreateDTO
        {
            public string Text { get; set; }

            public int ProfileId { get; set; }

            public string JournalType { get; set; }

            public string ItemData { get; set; }

            public string SecuritySet { get; set; }

            public int GroupId { get; set; }

            public IList<MentionDTO> Mentions { get; set; }
        }

        public class MentionDTO
        {
            public string DisplayName { get; set; }

            public int UserId { get; set; }
        }

        public class JournalIdDTO
        {
            public int JournalId { get; set; }
        }

        public class PreviewDTO
        {
            public string Url { get; set; }
        }

        public class GetListForProfileDTO
        {
            public int ProfileId { get; set; }

            public int GroupId { get; set; }

            public int RowIndex { get; set; }

            public int MaxRows { get; set; }
        }

        public class CommentSaveDTO
        {
            public int JournalId { get; set; }

            public string Comment { get; set; }

            public IList<MentionDTO> Mentions { get; set; }
        }

        public class CommentDeleteDTO
        {
            public int JournalId { get; set; }

            public int CommentId { get; set; }
        }

        public class SuggestDTO
        {
            // ReSharper disable InconsistentNaming
            [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
            public string displayName { get; set; }

            [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
            public int userId { get; set; }

            [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
            public string avatar { get; set; }

            [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
            public string key { get; set; }

            // ReSharper restore InconsistentNaming
        }
    }
}
