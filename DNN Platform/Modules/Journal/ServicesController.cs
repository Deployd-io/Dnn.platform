// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal;

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
	Logger.LogDebug(">>> Entering ServicesController()");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnAuthorize(DenyRoles = "Unverified Users")]
    public HttpResponseMessage Create(CreateDTO postData)
    {
		Logger.LogDebug(">>> Entering Create(postData={PostData})", postData);
        try
        {
            var userId = this.UserInfo.UserID;
			Logger.LogDebug("Create(postData={PostData}): userId → {UserId}", postData, userId);
            IDictionary<string, UserInfo> mentionedUsers = new Dictionary<string, UserInfo>();
			Logger.LogDebug("Create(postData={PostData}): mentionedUsers → {MentionedUsers}", postData, mentionedUsers);

            if (postData.ProfileId == -1)
            {
                postData.ProfileId = userId;
				Logger.LogDebug("Create(postData={PostData}): postData.ProfileId → {Arg1}", postData, postData.ProfileId);
            }

            this.CheckProfileAccess(postData.ProfileId, this.UserInfo);

            this.CheckGroupAccess(postData);

            var journalItem = this.PrepareJournalItem(postData, mentionedUsers);
			Logger.LogDebug("Create(postData={PostData}): journalItem → {JournalItem}", postData, journalItem);

            JournalController.Instance.SaveJournalItem(journalItem, this.ActiveModule);

            var originalSummary = journalItem.Summary;
            this.SendMentionNotifications(mentionedUsers, journalItem, originalSummary);

            return this.Request.CreateResponse(HttpStatusCode.OK, journalItem);
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
			Logger.LogDebug("<<< Exiting Create(postData={PostData})", postData);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnAuthorize(DenyRoles = "Unverified Users")]
    public HttpResponseMessage Delete(JournalIdDTO postData)
    {
		Logger.LogDebug(">>> Entering Delete(postData={PostData})", postData);
        try
        {
            var journalController = JournalController.Instance;
            var journalItem = journalController.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
			Logger.LogDebug("Delete(postData={PostData}): journalItem → {JournalItem}", postData, journalItem);

            if (journalItem == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "invalid request");
            }

            if (journalItem.UserId != this.UserInfo.UserID &&
                journalItem.ProfileId != this.UserInfo.UserID &&
                !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }

            journalController.DeleteJournalItem(this.PortalSettings.PortalId, this.UserInfo.UserID, postData.JournalId);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
			Logger.LogDebug("<<< Exiting Delete(postData={PostData})", postData);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnAuthorize(DenyRoles = "Unverified Users")]
    public HttpResponseMessage SoftDelete(JournalIdDTO postData)
    {
		Logger.LogDebug(">>> Entering SoftDelete(postData={PostData})", postData);
        try
        {
            var journalController = JournalController.Instance;
            var journalItem = journalController.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
			Logger.LogDebug("SoftDelete(postData={PostData}): journalItem → {JournalItem}", postData, journalItem);
            if (journalItem is null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "invalid request");
            }

            if (journalItem.UserId != this.UserInfo.UserID &&
                journalItem.ProfileId != this.UserInfo.UserID &&
                !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }

            journalController.SoftDeleteJournalItem(this.PortalSettings.PortalId, this.UserInfo.UserID, postData.JournalId);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
			Logger.LogDebug("<<< Exiting SoftDelete(postData={PostData})", postData);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnAuthorize]
    public HttpResponseMessage PreviewUrl(PreviewDTO postData)
    {
		Logger.LogDebug(">>> Entering PreviewUrl(postData={PostData})", postData);
        try
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, Utilities.GetLinkData(postData.Url));
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
			Logger.LogDebug("<<< Exiting PreviewUrl(postData={PostData})", postData);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public HttpResponseMessage GetListForProfile(GetListForProfileDTO postData)
    {
		Logger.LogDebug(">>> Entering GetListForProfile(postData={PostData})", postData);
        try
        {
            var journalParser = new JournalParser(this.PortalSettings, this.ActiveModule.ModuleID, postData.ProfileId, postData.GroupId, this.UserInfo);
			Logger.LogDebug("GetListForProfile(postData={PostData}): journalParser → {JournalParser}", postData, journalParser);
			Logger.LogDebug("<<< Exiting GetListForProfile(postData={PostData})", postData);
            return this.Request.CreateResponse(HttpStatusCode.OK, journalParser.GetList(postData.RowIndex, postData.MaxRows), "text/html");
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
		Logger.LogDebug(">>> Entering Like(postData={PostData})", postData);
        try
        {
            var journalItem = JournalController.Instance.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
			Logger.LogDebug("Like(postData={PostData}): journalItem → {JournalItem}", postData, journalItem);
            if (journalItem is null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }

            JournalController.Instance.LikeJournalItem(postData.JournalId, this.UserInfo.UserID, this.UserInfo.DisplayName);

            journalItem = JournalController.Instance.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
            var journalParser = new JournalParser(this.PortalSettings, this.ActiveModule.ModuleID, journalItem.ProfileId, -1, this.UserInfo);
			Logger.LogDebug("Like(postData={PostData}): journalParser → {JournalParser}", postData, journalParser);
            var isLiked = false;
            var likeList = journalParser.GetLikeListHTML(journalItem, ref isLiked);
			Logger.LogDebug("Like(postData={PostData}): likeList → {LikeList}", postData, likeList);
            likeList = Utilities.LocalizeControl(likeList);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { LikeList = likeList, Liked = isLiked, });
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
			Logger.LogDebug("<<< Exiting Like(postData={PostData})", postData);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    /// <summary>Posts a comment to a journal item.</summary>
    /// <param name="postData">The information about the comment to post.</param>
    /// <returns>A <see cref="HttpResponseMessage"/> indicating the result of the operation.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnAuthorize(DenyRoles = "Unverified Users")]
    public HttpResponseMessage CommentSave(CommentSaveDTO postData)
    {
		Logger.LogDebug(">>> Entering CommentSave(postData={PostData})", postData);
        try
        {
            var journalItem = JournalController.Instance.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId, false, false, true);
			Logger.LogDebug("CommentSave(postData={PostData}): journalItem → {JournalItem}", postData, journalItem);
            if (journalItem == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }

            var comment = Utilities.RemoveHTML(HttpUtility.UrlDecode(postData.Comment));
			Logger.LogDebug("CommentSave(postData={PostData}): comment → {Comment}", postData, comment);

            IDictionary<string, UserInfo> mentionedUsers = new Dictionary<string, UserInfo>();
			Logger.LogDebug("CommentSave(postData={PostData}): mentionedUsers → {MentionedUsers}", postData, mentionedUsers);
            var originalComment = comment;
            comment = this.ParseMentions(comment, postData.Mentions, ref mentionedUsers);
            var commentInfo = new CommentInfo
            {
                JournalId = postData.JournalId,
                Comment = comment,
                UserId = this.UserInfo.UserID,
                DisplayName = this.UserInfo.DisplayName,
            };
            JournalController.Instance.SaveComment(commentInfo);

            var jp = new JournalParser(this.PortalSettings, this.ActiveModule.ModuleID, journalItem.ProfileId, -1, this.UserInfo);
			Logger.LogDebug("CommentSave(postData={PostData}): jp → {Jp}", postData, jp);

            this.SendMentionNotifications(mentionedUsers, journalItem, originalComment, "Comment");

            return this.Request.CreateResponse(HttpStatusCode.OK, jp.GetCommentRow(journalItem, commentInfo), "text/html");
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
			Logger.LogDebug("<<< Exiting CommentSave(postData={PostData})", postData);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnAuthorize(DenyRoles = "Unverified Users")]
    public HttpResponseMessage CommentDelete(CommentDeleteDTO postData)
    {
		Logger.LogDebug(">>> Entering CommentDelete(postData={PostData})", postData);
        try
        {
            var commentInfo = JournalController.Instance.GetComment(postData.CommentId);
			Logger.LogDebug("CommentDelete(postData={PostData}): commentInfo → {CommentInfo}", postData, commentInfo);
            if (commentInfo is null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "delete failed");
            }

            var journalItem = JournalController.Instance.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
			Logger.LogDebug("CommentDelete(postData={PostData}): journalItem → {JournalItem}", postData, journalItem);
            if (journalItem is null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "invalid request");
            }

            if (commentInfo.UserId != this.UserInfo.UserID &&
                journalItem.UserId != this.UserInfo.UserID &&
                !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }

            JournalController.Instance.DeleteComment(postData.JournalId, postData.CommentId);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
			Logger.LogDebug("<<< Exiting CommentDelete(postData={PostData})", postData);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    [HttpGet]
    [DnnAuthorize(DenyRoles = "Unverified Users")]
    public HttpResponseMessage GetSuggestions(string keyword)
    {
		Logger.LogDebug(">>> Entering GetSuggestions(keyword={Keyword})", keyword);
        try
        {
            var foundUsers = new List<SuggestDTO>();
			Logger.LogDebug("GetSuggestions(keyword={Keyword}): foundUsers → {FoundUsers}", keyword, foundUsers);
            var relations = RelationshipController.Instance.GetUserRelationships(this.UserInfo);
			Logger.LogDebug("GetSuggestions(keyword={Keyword}): relations → {Relations}", keyword, relations);
            foreach (var ur in relations)
            {
                var targetUserId = ur.UserId == this.UserInfo.UserID ? ur.RelatedUserId : ur.UserId;
                var targetUser = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, targetUserId);
				Logger.LogDebug("GetSuggestions(keyword={Keyword}): targetUser → {TargetUser}", keyword, targetUser);
                var relationship = RelationshipController.Instance.GetRelationship(ur.RelationshipId);
				Logger.LogDebug("GetSuggestions(keyword={Keyword}): relationship → {Relationship}", keyword, relationship);
                if (ur.Status == RelationshipStatus.Accepted && targetUser != null
                                                             && ((relationship.RelationshipTypeId == (int)DefaultRelationshipTypes.Followers && ur.RelatedUserId == this.UserInfo.UserID)
                                                                 || relationship.RelationshipTypeId == (int)DefaultRelationshipTypes.Friends)
                                                             && (targetUser.DisplayName.ToLowerInvariant().Contains(keyword.ToLowerInvariant())
                                                                 || targetUser.DisplayName.ToLowerInvariant().Contains(keyword.Replace("-", " ").ToLowerInvariant()))
                                                             && foundUsers.All(s => s.userId != targetUser.UserID))
                {
                    foundUsers.Add(new SuggestDTO
                    {
                        displayName = targetUser.DisplayName.Replace(" ", "-"),
                        userId = targetUser.UserID,
                        avatar = targetUser.Profile.PhotoURL,
                        key = keyword,
                    });
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, foundUsers.Cast<object>().Take(5));
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
			Logger.LogDebug("<<< Exiting GetSuggestions(keyword={Keyword})", keyword);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    private static bool IsImageFile(string relativePath)
    {
		Logger.LogDebug(">>> Entering IsImageFile(relativePath={RelativePath})", relativePath);
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

        var lastDotIndex = relativePath.LastIndexOf(".", StringComparison.Ordinal);
		Logger.LogDebug("IsImageFile(relativePath={RelativePath}): lastDotIndex → {LastDotIndex}", relativePath, lastDotIndex);
        var extension = relativePath.Substring(lastDotIndex + 1).ToLowerInvariant();
		Logger.LogDebug("IsImageFile(relativePath={RelativePath}): extension → {Extension}", relativePath, extension);
		Logger.LogDebug("<<< Exiting IsImageFile(relativePath={RelativePath})", relativePath);
        return AcceptedFileExtensions.Contains(extension);
    }

    private static bool IsAllowedLink(string url)
    {
		Logger.LogDebug(">>> Entering IsAllowedLink(url={Url})", url);
		Logger.LogDebug("<<< Exiting IsAllowedLink(url={Url})", url);
        return !string.IsNullOrEmpty(url) && !url.Contains("//");
    }

    // Check if a user can post content on a specific profile's page
    private void CheckProfileAccess(int profileId, UserInfo currentUser)
    {
		Logger.LogDebug(">>> Entering CheckProfileAccess(profileId={ProfileId},currentUser={CurrentUser})", profileId, currentUser);
        if (profileId == currentUser.UserID)
        {
            return;
        }

        var profileUser = UserController.Instance.GetUser(this.PortalSettings.PortalId, profileId);
		Logger.LogDebug("CheckProfileAccess(profileId={ProfileId},currentUser={CurrentUser}): profileUser → {ProfileUser}", profileId, currentUser, profileUser);
        if (profileUser == null || (!this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName) && !Utilities.AreFriends(profileUser, currentUser)))
        {
			Logger.LogWarning("Throwing ArgumentException from CheckProfileAccess(profileId={ProfileId},currentUser={CurrentUser})", profileId, currentUser);
		Logger.LogDebug("<<< Exiting CheckProfileAccess(profileId={ProfileId},currentUser={CurrentUser})", profileId, currentUser);
            throw new ArgumentException("you have no permission to post journal on current profile page.");
        }
    }

    private void CheckGroupAccess(CreateDTO postData)
    {
		Logger.LogDebug(">>> Entering CheckGroupAccess(postData={PostData})", postData);
        if (postData.GroupId <= 0)
        {
            return;
        }

        postData.ProfileId = -1;
		Logger.LogDebug("CheckGroupAccess(postData={PostData}): postData.ProfileId → {Arg1}", postData, postData.ProfileId);

        var roleInfo = RoleController.Instance.GetRoleById(this.ActiveModule.OwnerPortalID, postData.GroupId);
		Logger.LogDebug("CheckGroupAccess(postData={PostData}): roleInfo → {RoleInfo}", postData, roleInfo);
        if (roleInfo == null)
        {
            return;
        }

        if (!this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName) && !this.UserInfo.IsInRole(roleInfo.RoleName))
        {
			Logger.LogWarning("Throwing ArgumentException from CheckGroupAccess(postData={PostData})", postData);
            throw new ArgumentException("you have no permission to post journal on current group.");
        }

        if (!roleInfo.IsPublic)
        {
            postData.SecuritySet = "R";
			Logger.LogDebug("CheckGroupAccess(postData={PostData}): postData.SecuritySet → {Arg1}", postData, postData.SecuritySet);
        }
		Logger.LogDebug("<<< Exiting CheckGroupAccess(postData={PostData})", postData);
    }

    private JournalItem PrepareJournalItem(CreateDTO postData, IDictionary<string, UserInfo> mentionedUsers)
    {
		Logger.LogDebug(">>> Entering PrepareJournalItem(postData={PostData},mentionedUsers={MentionedUsers})", postData, mentionedUsers);
        var journalItem = new JournalItem
        {
            JournalId = -1,
            JournalTypeId = postData.JournalType switch
            {
                "link" => 2,
                "photo" => 3,
                "file" => 4,
                _ => 1,
            },
            PortalId = this.ActiveModule.OwnerPortalID,
            UserId = this.UserInfo.UserID,
            SocialGroupId = postData.GroupId,
            ProfileId = postData.ProfileId,
            Summary = postData.Text ?? string.Empty,
            SecuritySet = postData.SecuritySet,
        };
        journalItem.Title = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(journalItem.Title));
        journalItem.Summary = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(journalItem.Summary));

		Logger.LogDebug("<<< Exiting PrepareJournalItem(postData={PostData},mentionedUsers={MentionedUsers})", postData, mentionedUsers);
        var ps = PortalSecurity.Instance;

#pragma warning disable CS0618 // Type or member is obsolete
        journalItem.Title = ps.InputFilter(journalItem.Title, PortalSecurity.FilterFlag.NoScripting);
        journalItem.Title = Utilities.RemoveHTML(journalItem.Title);
        journalItem.Title = ps.InputFilter(journalItem.Title, PortalSecurity.FilterFlag.NoMarkup);

        journalItem.Summary = ps.InputFilter(journalItem.Summary, PortalSecurity.FilterFlag.NoScripting);
        journalItem.Summary = Utilities.RemoveHTML(journalItem.Summary);
        journalItem.Summary = ps.InputFilter(journalItem.Summary, PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete

        // parse the mentions context in post data
        journalItem.Summary = this.ParseMentions(journalItem.Summary, postData.Mentions, ref mentionedUsers);

        if (journalItem.Summary.Length > 2000)
        {
            journalItem.Body = journalItem.Summary;
			Logger.LogDebug("PrepareJournalItem(postData={PostData},mentionedUsers={MentionedUsers}): journalItem.Body → {Arg2}", postData, mentionedUsers, journalItem.Body);
			Logger.LogDebug("PrepareJournalItem(postData={PostData},mentionedUsers={MentionedUsers}): journalItem.Summary → {Arg2}", postData, mentionedUsers, journalItem.Summary);
            journalItem.Summary = null;
        }

        if (string.IsNullOrEmpty(postData.ItemData))
        {
            return journalItem;
        }

        journalItem.ItemData = postData.ItemData.FromJson<ItemData>();
        var originalImageUrl = journalItem.ItemData.ImageUrl;
        if (!IsImageFile(journalItem.ItemData.ImageUrl))
        {
            journalItem.ItemData.ImageUrl = string.Empty;
        }

        journalItem.ItemData.Description = HttpUtility.UrlDecode(journalItem.ItemData.Description);

        if (!IsAllowedLink(journalItem.ItemData.Url))
        {
            journalItem.ItemData.Url = string.Empty;
        }

        if (string.IsNullOrEmpty(journalItem.ItemData.Url) || !journalItem.ItemData.Url.StartsWith("fileid="))
        {
            return journalItem;
        }

        var fileId = Convert.ToInt32(journalItem.ItemData.Url.Replace("fileid=", string.Empty).Trim());
		Logger.LogDebug("PrepareJournalItem(postData={PostData},mentionedUsers={MentionedUsers}): fileId → {FileId}", postData, mentionedUsers, fileId);
        var file = FileManager.Instance.GetFile(fileId);
		Logger.LogDebug("PrepareJournalItem(postData={PostData},mentionedUsers={MentionedUsers}): file → {File}", postData, mentionedUsers, file);

        if (!this.IsCurrentUserFile(file))
        {
			Logger.LogWarning("Throwing ArgumentException from PrepareJournalItem(postData={PostData},mentionedUsers={MentionedUsers})", postData, mentionedUsers);
            throw new ArgumentException("you have no permission to attach files not belongs to you.");
        }

        journalItem.ItemData.Title = file.FileName;
        journalItem.ItemData.Url = Globals.LinkClick(journalItem.ItemData.Url, Null.NullInteger, Null.NullInteger);

        if (string.IsNullOrEmpty(journalItem.ItemData.ImageUrl) &&
            originalImageUrl.ToLowerInvariant().StartsWith("/linkclick.aspx?") &&
            AcceptedFileExtensions.Contains(file.Extension.ToLowerInvariant()))
        {
            journalItem.ItemData.ImageUrl = originalImageUrl;
        }

        return journalItem;
    }

    private string ParseMentions(string content, IList<MentionDTO> mentions, ref IDictionary<string, UserInfo> mentionedUsers)
    {
		Logger.LogDebug(">>> Entering ParseMentions(content={Content},mentions={Mentions},mentionedUsers={MentionedUsers})", content, mentions, mentionedUsers);
        if (mentions == null || mentions.Count == 0)
        {
            return content;
        }

        foreach (var mention in mentions)
        {
            var user = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, mention.UserId);
			Logger.LogDebug("ParseMentions(content={Content},mentions={Mentions},mentionedUsers={MentionedUsers}): user → {User}", content, mentions, mentionedUsers, user);

            if (user == null)
            {
                continue;
            }

            var relationship = RelationshipController.Instance.GetFollowingRelationship(this.UserInfo, user) ??
                               RelationshipController.Instance.GetFriendRelationship(this.UserInfo, user);
            if (relationship is not { Status: RelationshipStatus.Accepted, })
            {
                continue;
            }

            var userLink =
                $"""<a href="{Globals.UserProfileURL(user.UserID)}" class="userLink" target="_blank">{MentionIdentityChar}{user.DisplayName}</a>""";
            content = content.Replace(MentionIdentityChar + mention.DisplayName, userLink);
			Logger.LogDebug("ParseMentions(content={Content},mentions={Mentions},mentionedUsers={MentionedUsers}): content → {Content1}", content, mentions, mentionedUsers, content);

            mentionedUsers.Add(mention.DisplayName, user);
        }

		Logger.LogDebug("<<< Exiting ParseMentions(content={Content},mentions={Mentions},mentionedUsers={MentionedUsers})", content, mentions, mentionedUsers);
        return content;
    }

    private void SendMentionNotifications(IDictionary<string, UserInfo> mentionedUsers, JournalItem item, string originalSummary, string type = "Post")
    {
		Logger.LogDebug(">>> Entering SendMentionNotifications(mentionedUsers={MentionedUsers},item={Item},originalSummary={OriginalSummary},type={Type})", mentionedUsers, item, originalSummary, type);
        // send notification to the mention users
        var subjectTemplate = Utilities.GetSharedResource("Notification_Mention.Subject");
		Logger.LogDebug("SendMentionNotifications(mentionedUsers={MentionedUsers},item={Item},originalSummary={OriginalSummary},type={Type}): subjectTemplate → {SubjectTemplate}", mentionedUsers, item, originalSummary, type, subjectTemplate);
        var bodyTemplate = Utilities.GetSharedResource("Notification_Mention.Body");
		Logger.LogDebug("SendMentionNotifications(mentionedUsers={MentionedUsers},item={Item},originalSummary={OriginalSummary},type={Type}): bodyTemplate → {BodyTemplate}", mentionedUsers, item, originalSummary, type, bodyTemplate);
        var mentionType = Utilities.GetSharedResource("Notification_MentionType_" + type);
		Logger.LogDebug("SendMentionNotifications(mentionedUsers={MentionedUsers},item={Item},originalSummary={OriginalSummary},type={Type}): mentionType → {MentionType}", mentionedUsers, item, originalSummary, type, mentionType);
        var notificationType = NotificationsController.Instance.GetNotificationType("JournalMention");
		Logger.LogDebug("SendMentionNotifications(mentionedUsers={MentionedUsers},item={Item},originalSummary={OriginalSummary},type={Type}): notificationType → {NotificationType}", mentionedUsers, item, originalSummary, type, notificationType);

        foreach (var key in mentionedUsers.Keys)
        {
            var mentionUser = mentionedUsers[key];
            var mentionText = originalSummary.Substring(originalSummary.IndexOf(MentionIdentityChar + key, StringComparison.InvariantCultureIgnoreCase));
			Logger.LogDebug("SendMentionNotifications(mentionedUsers={MentionedUsers},item={Item},originalSummary={OriginalSummary},type={Type}): mentionText → {MentionText}", mentionedUsers, item, originalSummary, type, mentionText);
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
                Context = $"{this.UserInfo.UserID}_{item.JournalId}",
            };

            NotificationsController.Instance.SendNotification(notification, this.PortalSettings.PortalId, null, [mentionUser,]);
        }
		Logger.LogDebug("<<< Exiting SendMentionNotifications(mentionedUsers={MentionedUsers},item={Item},originalSummary={OriginalSummary},type={Type})", mentionedUsers, item, originalSummary, type);
    }

    private bool IsCurrentUserFile(IFileInfo file)
    {
		Logger.LogDebug(">>> Entering IsCurrentUserFile(file={File})", file);
        if (file == null)
        {
            return false;
        }

        var userFolders = this.GetUserFolders();
		Logger.LogDebug("IsCurrentUserFile(file={File}): userFolders → {UserFolders}", file, userFolders);

		Logger.LogDebug("<<< Exiting IsCurrentUserFile(file={File})", file);
        return userFolders.Any(f => file.FolderId == f.FolderID);
    }

    private IList<IFolderInfo> GetUserFolders()
    {
		Logger.LogDebug(">>> Entering GetUserFolders()");
        var folders = new List<IFolderInfo>();

		Logger.LogDebug("GetUserFolders(): folders → {Folders}", folders);
        var userFolder = FolderManager.Instance.GetUserFolder(this.UserInfo);
		Logger.LogDebug("GetUserFolders(): userFolder → {UserFolder}", userFolder);
        folders.Add(userFolder);
        folders.AddRange(this.GetSubFolders(userFolder));

		Logger.LogDebug("<<< Exiting GetUserFolders()");
        return folders;
    }

    private IList<IFolderInfo> GetSubFolders(IFolderInfo parentFolder)
    {
		Logger.LogDebug(">>> Entering GetSubFolders(parentFolder={ParentFolder})", parentFolder);
        var folders = new List<IFolderInfo>();
        foreach (var folder in FolderManager.Instance.GetFolders(parentFolder))
        {
		Logger.LogDebug("GetSubFolders(parentFolder={ParentFolder}): folders → {Folders}", parentFolder, folders);
            folders.Add(folder);
            folders.AddRange(this.GetSubFolders(folder));
        }

		Logger.LogDebug("<<< Exiting GetSubFolders(parentFolder={ParentFolder})", parentFolder);
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
