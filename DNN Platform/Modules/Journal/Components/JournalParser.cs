// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal.Components
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Journal.Internal;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    public class JournalParser
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JournalParser));
        private const string ResxPath = "~/DesktopModules/Journal/App_LocalResources/SharedResources.resx";
        private static readonly Regex CdataRegex = new Regex(@"\<\!\[CDATA\[(?<text>[^\]]*)\]\]\>", RegexOptions.Compiled);
        private static readonly Regex TemplateRegex = new Regex("{CanComment}(.*?){/CanComment}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex BaseUrlRegex = new Regex("\\[BaseUrl\\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly string url = string.Empty;
        private bool isAdmin;
        private bool isUnverifiedUser;

        /// <summary>Initializes a new instance of the <see cref="JournalParser"/> class.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="profileId">The profile ID.</param>
        /// <param name="socialGroupId">The social group ID.</param>
        /// <param name="userInfo">The user info.</param>
        public JournalParser(
            PortalSettings portalSettings,
            int moduleId,
            int profileId,
            int socialGroupId,
            UserInfo userInfo)
            : this(null, portalSettings, moduleId, profileId, socialGroupId, userInfo)
        {
		Logger.LogDebug(">>> Entering JournalParser(portalSettings={PortalSettings},moduleId={ModuleId},profileId={ProfileId},socialGroupId={SocialGroupId},userInfo={UserInfo})", portalSettings, moduleId, profileId, socialGroupId, userInfo);
        }
		Logger.LogDebug("<<< Exiting JournalParser(portalSettings={PortalSettings},moduleId={ModuleId},profileId={ProfileId},socialGroupId={SocialGroupId},userInfo={UserInfo})", portalSettings, moduleId, profileId, socialGroupId, userInfo);

        /// <summary>Initializes a new instance of the <see cref="JournalParser"/> class.</summary>
        /// <param name="navigationManager">A navigation manager.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="profileId">The profile ID.</param>
        /// <param name="socialGroupId">The social group ID.</param>
        /// <param name="userInfo">The user info.</param>
        public JournalParser(INavigationManager navigationManager, PortalSettings portalSettings, int moduleId, int profileId, int socialGroupId, UserInfo userInfo)
        {
            this.NavigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
            this.PortalSettings = portalSettings;
			Logger.LogDebug("JournalParser(navigationManager={NavigationManager},portalSettings={PortalSettings},moduleId={ModuleId},profileId={ProfileId},socialGroupId={SocialGroupId},userInfo={UserInfo}): this.PortalSettings → {Arg6}", navigationManager, portalSettings, moduleId, profileId, socialGroupId, userInfo, this.PortalSettings);
            this.ModuleId = moduleId;
            this.ProfileId = profileId;
			Logger.LogDebug("JournalParser(navigationManager={NavigationManager},portalSettings={PortalSettings},moduleId={ModuleId},profileId={ProfileId},socialGroupId={SocialGroupId},userInfo={UserInfo}): this.ModuleId → {Arg6}", navigationManager, portalSettings, moduleId, profileId, socialGroupId, userInfo, this.ModuleId);
			Logger.LogDebug("JournalParser(navigationManager={NavigationManager},portalSettings={PortalSettings},moduleId={ModuleId},profileId={ProfileId},socialGroupId={SocialGroupId},userInfo={UserInfo}): this.ProfileId → {Arg6}", navigationManager, portalSettings, moduleId, profileId, socialGroupId, userInfo, this.ProfileId);
            this.SocialGroupId = socialGroupId;
            this.CurrentUser = userInfo;
            this.url = this.PortalSettings.DefaultPortalAlias;
            this.OwnerPortalId = portalSettings.PortalId;
			Logger.LogDebug("JournalParser(navigationManager={NavigationManager},portalSettings={PortalSettings},moduleId={ModuleId},profileId={ProfileId},socialGroupId={SocialGroupId},userInfo={UserInfo}): this.SocialGroupId → {Arg6}", navigationManager, portalSettings, moduleId, profileId, socialGroupId, userInfo, this.SocialGroupId);
			Logger.LogDebug("JournalParser(navigationManager={NavigationManager},portalSettings={PortalSettings},moduleId={ModuleId},profileId={ProfileId},socialGroupId={SocialGroupId},userInfo={UserInfo}): this.CurrentUser → {Arg6}", navigationManager, portalSettings, moduleId, profileId, socialGroupId, userInfo, this.CurrentUser);
			Logger.LogDebug("JournalParser(navigationManager={NavigationManager},portalSettings={PortalSettings},moduleId={ModuleId},profileId={ProfileId},socialGroupId={SocialGroupId},userInfo={UserInfo}): this.url → {Arg6}", navigationManager, portalSettings, moduleId, profileId, socialGroupId, userInfo, this.url);
			Logger.LogDebug("JournalParser(navigationManager={NavigationManager},portalSettings={PortalSettings},moduleId={ModuleId},profileId={ProfileId},socialGroupId={SocialGroupId},userInfo={UserInfo}): this.OwnerPortalId → {Arg6}", navigationManager, portalSettings, moduleId, profileId, socialGroupId, userInfo, this.OwnerPortalId);
            ModuleInfo moduleInfo = ModuleController.Instance.GetModule(moduleId, this.PortalSettings.ActiveTab.TabID, false);
			Logger.LogDebug("JournalParser(navigationManager={NavigationManager},portalSettings={PortalSettings},moduleId={ModuleId},profileId={ProfileId},socialGroupId={SocialGroupId},userInfo={UserInfo}): moduleInfo → {ModuleInfo}", navigationManager, portalSettings, moduleId, profileId, socialGroupId, userInfo, moduleInfo);
            if (moduleInfo.OwnerPortalID != portalSettings.PortalId)
            {
                this.OwnerPortalId = moduleInfo.OwnerPortalID;
            }

            if (string.IsNullOrEmpty(this.url))
            {
                this.url = HttpContext.Current.Request.Url.Host;
            }

            this.url = string.Format(
                "{0}://{1}{2}",
                UrlUtils.IsSecureConnectionOrSslOffload(HttpContext.Current.Request) ? "https" : "http",
                this.url,
                !HttpContext.Current.Request.Url.IsDefaultPort && !this.url.Contains(":") ? ":" + HttpContext.Current.Request.Url.Port : string.Empty);
        }

        public int JournalId { get; set; }

        protected INavigationManager NavigationManager { get; }

        private PortalSettings PortalSettings { get; set; }

        private int ProfileId { get; set; }

        private int SocialGroupId { get; set; }

        private int ModuleId { get; set; }

        private UserInfo CurrentUser { get; set; }

        private int OwnerPortalId { get; set; }

        public string GetList(int currentIndex, int rows)
        {
			Logger.LogDebug(">>> Entering GetList(currentIndex={CurrentIndex},rows={Rows})", currentIndex, rows);
            if (this.CurrentUser.UserID > 0)
            {
                this.isAdmin = this.CurrentUser.IsInRole(this.PortalSettings.AdministratorRoleName);
            }

            this.isUnverifiedUser = !this.CurrentUser.IsSuperUser && this.CurrentUser.IsInRole("Unverified Users");

            var journalControllerInternal = InternalJournalController.Instance;
            var sb = new StringBuilder();
			Logger.LogDebug("GetList(currentIndex={CurrentIndex},rows={Rows}): sb → {Sb}", currentIndex, rows, sb);

            string statusTemplate = Localization.GetString("journal_status", ResxPath);
			Logger.LogDebug("GetList(currentIndex={CurrentIndex},rows={Rows}): statusTemplate → {StatusTemplate}", currentIndex, rows, statusTemplate);
            string linkTemplate = Localization.GetString("journal_link", ResxPath);
			Logger.LogDebug("GetList(currentIndex={CurrentIndex},rows={Rows}): linkTemplate → {LinkTemplate}", currentIndex, rows, linkTemplate);
            string photoTemplate = Localization.GetString("journal_photo", ResxPath);
			Logger.LogDebug("GetList(currentIndex={CurrentIndex},rows={Rows}): photoTemplate → {PhotoTemplate}", currentIndex, rows, photoTemplate);
            string fileTemplate = Localization.GetString("journal_file", ResxPath);
			Logger.LogDebug("GetList(currentIndex={CurrentIndex},rows={Rows}): fileTemplate → {FileTemplate}", currentIndex, rows, fileTemplate);

            statusTemplate = BaseUrlRegex.Replace(statusTemplate, this.url);
            linkTemplate = BaseUrlRegex.Replace(linkTemplate, this.url);
            photoTemplate = BaseUrlRegex.Replace(photoTemplate, this.url);
            fileTemplate = BaseUrlRegex.Replace(fileTemplate, this.url);

            string comment = Localization.GetString("comment", ResxPath);
			Logger.LogDebug("GetList(currentIndex={CurrentIndex},rows={Rows}): comment → {Comment}", currentIndex, rows, comment);

            IList<JournalItem> journalList;
            if (this.JournalId > 0)
            {
                var journal = JournalController.Instance.GetJournalItem(
                    this.PortalSettings.PortalId,
                    this.CurrentUser.UserID,
                    this.JournalId,
                    false,
                    false,
                    true);
                journalList = new List<JournalItem>();
                if (journal != null)
                {
                    journalList.Add(journal);
                }
            }
            else if (this.ProfileId > 0)
            {
                journalList = journalControllerInternal.GetJournalItemsByProfile(this.OwnerPortalId, this.ModuleId, this.CurrentUser.UserID, this.ProfileId, currentIndex, rows);
            }
            else if (this.SocialGroupId > 0)
            {
                journalList = journalControllerInternal.GetJournalItemsByGroup(this.OwnerPortalId, this.ModuleId, this.CurrentUser.UserID, this.SocialGroupId, currentIndex, rows);
            }
            else
            {
                journalList = journalControllerInternal.GetJournalItems(this.OwnerPortalId, this.ModuleId, this.CurrentUser.UserID, currentIndex, rows);
            }

            var journalIds = journalList.Select(ji => ji.JournalId).ToList();
			Logger.LogDebug("GetList(currentIndex={CurrentIndex},rows={Rows}): journalIds → {JournalIds}", currentIndex, rows, journalIds);
            IList<CommentInfo> comments = JournalController.Instance.GetCommentsByJournalIds(journalIds);
			Logger.LogDebug("GetList(currentIndex={CurrentIndex},rows={Rows}): comments → {Comments}", currentIndex, rows, comments);

            foreach (JournalItem ji in journalList)
            {
                string replacement = this.GetStringReplacement(ji);
				Logger.LogDebug("GetList(currentIndex={CurrentIndex},rows={Rows}): replacement → {Replacement}", currentIndex, rows, replacement);

                string rowTemplate;
                if (ji.JournalType == "status")
                {
                    rowTemplate = statusTemplate;
                    rowTemplate = TemplateRegex.Replace(rowTemplate, replacement);
                }
                else if (ji.JournalType == "link")
                {
                    rowTemplate = linkTemplate;
                    rowTemplate = TemplateRegex.Replace(rowTemplate, replacement);
                }
                else if (ji.JournalType == "photo")
                {
                    rowTemplate = photoTemplate;
                    rowTemplate = TemplateRegex.Replace(rowTemplate, replacement);
                }
                else if (ji.JournalType == "file")
                {
                    rowTemplate = fileTemplate;
                    rowTemplate = TemplateRegex.Replace(rowTemplate, replacement);
                }
                else
                {
                    rowTemplate = this.GetJournalTemplate(ji.JournalType, ji);
                }

                var ctl = new JournalControl();
				Logger.LogDebug("GetList(currentIndex={CurrentIndex},rows={Rows}): ctl → {Ctl}", currentIndex, rows, ctl);

                bool isLiked = false;
                ctl.LikeList = this.GetLikeListHTML(ji, ref isLiked);
                ctl.LikeLink = string.Empty;
				Logger.LogDebug("GetList(currentIndex={CurrentIndex},rows={Rows}): ctl.LikeLink → {Arg2}", currentIndex, rows, ctl.LikeLink);
				Logger.LogDebug("GetList(currentIndex={CurrentIndex},rows={Rows}): ctl.CommentLink → {Arg2}", currentIndex, rows, ctl.CommentLink);
                ctl.CommentLink = string.Empty;

                var authorLink = this.NavigationManager.NavigateURL(this.PortalSettings.UserTabId, string.Empty, "userId=" + ji.JournalAuthor.Id);
				Logger.LogDebug("GetList(currentIndex={CurrentIndex},rows={Rows}): authorLink → {AuthorLink}", currentIndex, rows, authorLink);
                ctl.AuthorNameLink = $"<a href=\"{WebUtility.HtmlEncode(authorLink)}\">{WebUtility.HtmlEncode(ji.JournalAuthor.Name)}</a>";
                if (this.CurrentUser.UserID > 0 && !this.isUnverifiedUser)
                {
                    if (!ji.CommentsDisabled)
                    {
                        ctl.CommentLink = $"<a href=\"#\" id=\"cmtbtn-{ji.JournalId}\">{WebUtility.HtmlEncode(comment)}</a>";
                    }

                    if (isLiked)
                    {
                        ctl.LikeLink = $"<a href=\"#\" id=\"like-{ji.JournalId}\">{{resx:unlike}}</a>";
                    }
                    else
                    {
                        ctl.LikeLink = $"<a href=\"#\" id=\"like-{ji.JournalId}\">{{resx:like}}</a>";
                    }
                }

                ctl.CommentArea = this.GetCommentAreaHTML(ji, comments);
                ji.TimeFrame = DateUtils.CalculateDateForDisplay(ji.DateCreated);
                ji.DateCreated = this.CurrentUser.LocalTime(ji.DateCreated);

                if (ji.Summary != null)
                {
                    ji.Summary = ji.Summary.Replace("\n", "<br />");
                }

                if (ji.Body != null)
                {
                    ji.Body = ji.Body.Replace(Environment.NewLine, "<br />");
                }

                var tokenReplace = new JournalItemTokenReplace(ji, ctl);
				Logger.LogDebug("GetList(currentIndex={CurrentIndex},rows={Rows}): tokenReplace → {TokenReplace}", currentIndex, rows, tokenReplace);
                string tmp = tokenReplace.ReplaceJournalItemTokens(rowTemplate);
                tmp = tmp.Replace("<br>", "<br />");
                sb.Append($"<div class=\"journalrow\" id=\"jid-{ji.JournalId}\">");
                if (this.isAdmin || this.CurrentUser.UserID == ji.UserId || (this.ProfileId > Null.NullInteger && this.CurrentUser.UserID == this.ProfileId))
                {
                    sb.Append("<div class=\"minidel\" onclick=\"journalDelete(this);\"></div>");
                }

                sb.Append(tmp);
                sb.Append("</div>");
            }

			Logger.LogDebug("<<< Exiting GetList(currentIndex={CurrentIndex},rows={Rows})", currentIndex, rows);
            return Utilities.LocalizeControl(sb.ToString());
        }

        internal string GetJournalTemplate(string journalType, JournalItem ji)
        {
			Logger.LogDebug(">>> Entering GetJournalTemplate(journalType={JournalType},ji={Ji})", journalType, ji);
            string template = Localization.GetString("journal_" + journalType, ResxPath);
            if (string.IsNullOrEmpty(template))
            {
                template = Localization.GetString("journal_generic", ResxPath);
            }

            template = BaseUrlRegex.Replace(template, this.url);
            template = template.Replace("[journalitem:action]", Localization.GetString(journalType + ".Action", ResxPath));

            var replacement = this.GetStringReplacement(ji);
			Logger.LogDebug("GetJournalTemplate(journalType={JournalType},ji={Ji}): replacement → {Replacement}", journalType, ji, replacement);
			Logger.LogDebug("<<< Exiting GetJournalTemplate(journalType={JournalType},ji={Ji})", journalType, ji);
            return TemplateRegex.Replace(template, replacement);
        }

        internal string GetLikeListHTML(JournalItem ji, ref bool isLiked)
        {
			Logger.LogDebug(">>> Entering GetLikeListHTML(ji={Ji},isLiked={IsLiked})", ji, isLiked);
            var sb = new StringBuilder();
            isLiked = false;
			Logger.LogDebug("GetLikeListHTML(ji={Ji},isLiked={IsLiked}): isLiked → {IsLiked1}", ji, isLiked, isLiked);
			Logger.LogDebug("GetLikeListHTML(ji={Ji},isLiked={IsLiked}): sb → {Sb}", ji, isLiked, sb);
            if (ji.JournalXML == null)
            {
                return string.Empty;
            }

            XmlNodeList xLikes = ji.JournalXML.DocumentElement.SelectNodes("//likes/u");
			Logger.LogDebug("GetLikeListHTML(ji={Ji},isLiked={IsLiked}): xLikes → {XLikes}", ji, isLiked, xLikes);
            if (xLikes == null)
            {
                return string.Empty;
            }

            foreach (XmlNode xLike in xLikes)
            {
                if (Convert.ToInt32(xLike.Attributes["uid"].Value) == this.CurrentUser.UserID)
                {
                    ji.CurrentUserLikes = true;
					Logger.LogDebug("GetLikeListHTML(ji={Ji},isLiked={IsLiked}): ji.CurrentUserLikes → {Arg2}", ji, isLiked, ji.CurrentUserLikes);
                    isLiked = true;
                    break;
                }
            }

            int xc = 0;
            sb.Append("<div class=\"likes\">");
            if (xLikes.Count == 1 && ji.CurrentUserLikes)
            {
                sb.Append("{resx:youlikethis}");
            }
            else if (xLikes.Count > 1)
            {
                if (ji.CurrentUserLikes)
                {
                    sb.Append("{resx:you}");
                    xc += 1;
                }

                foreach (XmlNode l in xLikes)
                {
                    int userId = Convert.ToInt32(l.Attributes["uid"].Value);
					Logger.LogDebug("GetLikeListHTML(ji={Ji},isLiked={IsLiked}): userId → {UserId}", ji, isLiked, userId);
                    string name = l.Attributes["un"].Value;
                    if (userId != this.CurrentUser.UserID)
                    {
                        if (xc < xLikes.Count - 1 && xc > 0 && xc < 3)
                        {
                            sb.Append(", ");
                        }
                        else if (xc > 0 & xc < xLikes.Count & xc < 3)
                        {
                            sb.Append(" {resx:and} ");
                        }
                        else if (xc >= 3)
                        {
                            int diff = xLikes.Count - xc;
                            sb.Append(" {resx:and} " + (xLikes.Count - xc).ToString(CultureInfo.InvariantCulture));
                            if (diff > 1)
                            {
                                sb.Append(" {resx:others}");
                            }
                            else
                            {
                                sb.Append(" {resx:other}");
                            }

                            break;
                        }

                        sb.Append($"<span id=\"user-{userId}\" class=\"juser\">{WebUtility.HtmlEncode(name)}</span>");
                        xc += 1;
                    }
                }

                if (xc == 1)
                {
                    sb.Append(" {resx:likesthis}");
                }
                else if (xc > 1)
                {
                    sb.Append(" {resx:likethis}");
                }
            }
            else
            {
                foreach (XmlNode l in xLikes)
                {
                    int userId = Convert.ToInt32(l.Attributes["uid"].Value);
                    string name = l.Attributes["un"].Value;
                    sb.Append($"<span id=\"user-{userId}\" class=\"juser\">{WebUtility.HtmlEncode(name)}</span>");
                    xc += 1;
                    if (xc == xLikes.Count - 1)
                    {
                        sb.Append(" {resx:and} ");
                    }
                    else if (xc < xLikes.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }

                if (xc == 1)
                {
                    sb.Append(" {resx:likesthis}");
                }
                else if (xc > 1)
                {
                    sb.Append(" {resx:likethis}");
                }
            }

            sb.Append("</div>");
			Logger.LogDebug("<<< Exiting GetLikeListHTML(ji={Ji},isLiked={IsLiked})", ji, isLiked);
            return sb.ToString();
        }

        internal string GetCommentAreaHTML(JournalItem journal, IList<CommentInfo> comments)
        {
			Logger.LogDebug(">>> Entering GetCommentAreaHTML(journal={Journal},comments={Comments})", journal, comments);
            if (journal.CommentsHidden)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
			Logger.LogDebug("GetCommentAreaHTML(journal={Journal},comments={Comments}): sb → {Sb}", journal, comments, sb);
            sb.Append($"<ul class=\"jcmt\" id=\"jcmt-{journal.JournalId}\">");
            foreach (CommentInfo ci in comments)
            {
                if (ci.JournalId == journal.JournalId)
                {
                    sb.Append(this.GetCommentRow(journal, ci));
                }
            }

            if (this.CurrentUser.UserID > 0 && !journal.CommentsDisabled)
            {
                sb.Append($"<li id=\"jcmt-{journal.JournalId}-txtrow\" class=\"cmteditarea\">");
                sb.Append($"<textarea id=\"jcmt-{journal.JournalId}-txt\" class=\"cmteditor\"></textarea>");
                sb.Append("<div class=\"editorPlaceholder\">{resx:leavecomment}</div></li>");
                sb.Append("<li class=\"cmtbtn\">");
                sb.Append("<a href=\"#\">{resx:comment}</a></li>");
            }

            sb.Append("</ul>");
			Logger.LogDebug("<<< Exiting GetCommentAreaHTML(journal={Journal},comments={Comments})", journal, comments);
            return sb.ToString();
        }

        internal string GetCommentRow(JournalItem journal, CommentInfo comment)
        {
			Logger.LogDebug(">>> Entering GetCommentRow(journal={Journal},comment={Comment})", journal, comment);
            var sb = new StringBuilder();
            string pic = UserController.Instance.GetUserProfilePictureUrl(comment.UserId, 32, 32);
            sb.Append($"<li id=\"cmt-{comment.CommentId}\">");
			Logger.LogDebug("GetCommentRow(journal={Journal},comment={Comment}): sb → {Sb}", journal, comment, sb);
            if (comment.UserId == this.CurrentUser.UserID || journal.UserId == this.CurrentUser.UserID || this.isAdmin)
            {
                sb.Append("<div class=\"miniclose\"></div>");
            }

            sb.Append($"<img src=\"{WebUtility.HtmlEncode(pic)}\" />");
            sb.Append("<p>");
            string userUrl = this.NavigationManager.NavigateURL(this.PortalSettings.UserTabId, string.Empty, "userId=" + comment.UserId);
            sb.Append($"<a href=\"{WebUtility.HtmlEncode(userUrl)}\">{WebUtility.HtmlEncode(comment.DisplayName)}</a>");

            if (comment.CommentXML?.SelectSingleNode("/root/comment") != null)
            {
                string text;
                if (CdataRegex.IsMatch(comment.CommentXML.SelectSingleNode("/root/comment").InnerText))
                {
                    var match = CdataRegex.Match(comment.CommentXML.SelectSingleNode("/root/comment").InnerText);
					Logger.LogDebug("GetCommentRow(journal={Journal},comment={Comment}): match → {Match}", journal, comment, match);
                    text = match.Groups["text"].Value;
                }
                else
                {
                    text = comment.CommentXML.SelectSingleNode("/root/comment").InnerText;
                }

                sb.Append(text.Replace("\n", "<br />"));
            }
            else
            {
                sb.Append(comment.Comment.Replace("\n", "<br />"));
            }

            var timeFrame = DateUtils.CalculateDateForDisplay(comment.DateCreated);
			Logger.LogDebug("GetCommentRow(journal={Journal},comment={Comment}): timeFrame → {TimeFrame}", journal, comment, timeFrame);
            comment.DateCreated = this.CurrentUser.LocalTime(comment.DateCreated);
            sb.Append($"<abbr title=\"{comment.DateCreated}\">{WebUtility.HtmlEncode(timeFrame)}</abbr>");

            sb.Append("</p>");
            sb.Append("</li>");
			Logger.LogDebug("<<< Exiting GetCommentRow(journal={Journal},comment={Comment})", journal, comment);
            return sb.ToString();
        }

        private string GetStringReplacement(JournalItem journalItem)
        {
			Logger.LogDebug(">>> Entering GetStringReplacement(journalItem={JournalItem})", journalItem);
            string replacement = string.Empty;
            if (this.CurrentUser.UserID > 0 && this.SocialGroupId <= 0 && !this.isUnverifiedUser)
            {
                replacement = "$1";
            }

            if (this.CurrentUser.UserID > 0 && journalItem.SocialGroupId > 0 && !this.isUnverifiedUser)
            {
                replacement = this.CurrentUser.IsInRole(journalItem.JournalOwner.Name) ? "$1" : string.Empty;
            }

			Logger.LogDebug("<<< Exiting GetStringReplacement(journalItem={JournalItem})", journalItem);
            return replacement;
        }
    }
}
