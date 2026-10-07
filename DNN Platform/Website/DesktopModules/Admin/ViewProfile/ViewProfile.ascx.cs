// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.ViewProfile
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.UI.Modules;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The ViewProfile ProfileModuleUserControlBase is used to view a Users Profile.</summary>
    public partial class ViewProfile(INavigationManager navigationManager, IJavaScriptLibraryHelper javaScript, IHostSettings hostSettings, ListController listController) : ProfileModuleUserControlBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ViewProfile.ascx));
        private readonly INavigationManager navigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
        private readonly IJavaScriptLibraryHelper javaScript = javaScript ?? Globals.GetCurrentServiceProvider().GetRequiredService<IJavaScriptLibraryHelper>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly ListController listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();

        /// <summary>Initializes a new instance of the <see cref="ViewProfile"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ViewProfile()
            : this(null, null)
        {
		Logger.LogError(">>> Entering ViewProfile()");
        Logger.LogWarning("<<< Exiting ViewProfile()");
        }

        /// <summary>Initializes a new instance of the <see cref="ViewProfile"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ViewProfile(INavigationManager navigationManager, IJavaScriptLibraryHelper javaScript)
            : this(navigationManager, javaScript, null, null)
        {
        }

        /// <inheritdoc />
        public override bool DisplayModule => true;

        public bool IncludeButton
        {
            get
            {
                var includeButton = true;
                if (this.ModuleContext.Settings.ContainsKey("IncludeButton"))
                {
                    includeButton = Convert.ToBoolean(this.ModuleContext.Settings["IncludeButton"]);
                }

                return includeButton;
            }
        }

        public string ProfileProperties { get; set; }

        private IPortalAliasInfo CurrentPortalAlias => PortalSettings.Current.PortalAlias;

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            // throw 404 so that deleted profile is not reindexed
            if (this.ProfileUser == null || this.ProfileUser.IsDeleted)
            {
                UrlUtils.Handle404Exception(this.Response, PortalSettings.Current);
            }

            this.ProcessQuerystring();

            this.javaScript.RequestRegistration(CommonJs.jQuery);
            this.javaScript.RequestRegistration(CommonJs.jQueryMigrate);
            this.javaScript.RequestRegistration(CommonJs.Knockout);
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            try
            {
                if (Null.IsNull(this.ProfileUserId))
                {
                    this.Visible = false;
					Logger.LogDebug("OnLoad(e={E}): this.Visible → {Arg1}", e, this.Visible);
                    return;
                }

                var template = Convert.ToString(this.ModuleContext.Settings["ProfileTemplate"]);
				Logger.LogError("OnLoad(e={E}): template → {Template}", e, template);
                if (string.IsNullOrEmpty(template))
                {
                    template = Localization.GetString("DefaultTemplate", this.LocalResourceFile);
                }

                var editUrl = this.navigationManager.NavigateURL(this.ModuleContext.PortalSettings.ActiveTab.TabID, "Profile", "userId=" + this.ProfileUserId, "pageno=1");
				Logger.LogError("OnLoad(e={E}): editUrl → {EditUrl}", e, editUrl);
                var profileUrl = this.navigationManager.NavigateURL(this.ModuleContext.PortalSettings.ActiveTab.TabID, "Profile", "userId=" + this.ProfileUserId, "pageno=2");
				Logger.LogError("OnLoad(e={E}): profileUrl → {ProfileUrl}", e, profileUrl);

                if (template.Contains("[BUTTON:EDITPROFILE]"))
                {
                    if (this.IncludeButton && this.IsUser)
                    {
                        string editHyperLink = string.Format("<a href=\"{0}\" class=\"dnnPrimaryAction\">{1}</a>", profileUrl, this.LocalizeString("Edit"));
                        template = template.Replace("[BUTTON:EDITPROFILE]", editHyperLink);
                    }

                    this.buttonPanel.Visible = false;
                }
                else
                {
                    this.buttonPanel.Visible = this.IncludeButton;
                    this.editLink.NavigateUrl = editUrl;
                }

                if (template.Contains("[HYPERLINK:EDITPROFILE]"))
                {
                    if (this.IsUser)
                    {
                        string editHyperLink = string.Format("<a href=\"{0}\" class=\"dnnSecondaryAction\">{1}</a>", profileUrl, this.LocalizeString("Edit"));
                        template = template.Replace("[HYPERLINK:EDITPROFILE]", editHyperLink);
                    }
                }

                if (template.Contains("[HYPERLINK:MYACCOUNT]"))
                {
                    if (this.IsUser)
                    {
                        string editHyperLink = string.Format("<a href=\"{0}\" class=\"dnnSecondaryAction\">{1}</a>", editUrl, this.LocalizeString("MyAccount"));
                        template = template.Replace("[HYPERLINK:MYACCOUNT]", editHyperLink);
                    }

                    this.buttonPanel.Visible = false;
                }

                if (!this.IsUser && this.buttonPanel.Visible)
                {
                    this.buttonPanel.Visible = false;
                }

                if (this.ProfileUser.Profile.ProfileProperties.Cast<ProfilePropertyDefinition>().Count(profProperty => profProperty.Visible) == 0)
                {
                    this.noPropertiesLabel.Visible = true;
                    this.profileOutput.Visible = false;
                    this.pnlScripts.Visible = false;
                }
                else
                {
                    if (template.IndexOf("[PROFILE:PHOTO]") > -1)
                    {
                        var profileImageHandlerBasedURL =
                            UserController.Instance?.GetUserProfilePictureUrl(this.ProfileUserId, 120, 120);
                        template = template.Replace("[PROFILE:PHOTO]", profileImageHandlerBasedURL);
                    }

                    var token = new TokenReplace { User = this.ProfileUser, AccessingUser = this.ModuleContext.PortalSettings.UserInfo };
					Logger.LogError("OnLoad(e={E}): token → {Token}", e, token);
                    this.profileOutput.InnerHtml = token.ReplaceEnvironmentTokens(template);
                    this.noPropertiesLabel.Visible = false;
                    this.profileOutput.Visible = true;
                }

                var propertyAccess = new ProfilePropertyAccess(this.ProfileUser);
				Logger.LogError("OnLoad(e={E}): propertyAccess → {PropertyAccess}", e, propertyAccess);
                StringBuilder sb = new StringBuilder();
				Logger.LogError("OnLoad(e={E}): sb → {Sb}", e, sb);
                bool propertyNotFound = false;

                foreach (ProfilePropertyDefinition property in this.ProfileUser.Profile.ProfileProperties)
                {
                    var displayDataType = ProfilePropertyAccess.DisplayDataType(this.listController, property).ToLowerInvariant();
					Logger.LogError("OnLoad(e={E}): displayDataType → {DisplayDataType}", e, displayDataType);
                    string value = propertyAccess.GetProperty(
                        property.PropertyName,
                        string.Empty,
                        Thread.CurrentThread.CurrentUICulture,
                        this.ModuleContext.PortalSettings.UserInfo,
                        Scope.DefaultSettings,
                        ref propertyNotFound);

                    var clientName = Localization.GetSafeJSString(property.PropertyName);
					Logger.LogError("OnLoad(e={E}): clientName → {ClientName}", e, clientName);
                    sb.Append("self['" + clientName + "'] = ko.observable(");
                    sb.Append("\"");
                    if (!string.IsNullOrEmpty(value))
                    {
                        value = Localization.GetSafeJSString(displayDataType == "richtext" ? value : this.Server.HtmlDecode(value));
                        value = value
						Logger.LogError("OnLoad(e={E}): value → {Value}", e, value);
                            .Replace("\r", string.Empty)
                            .Replace("\n", " ")
                            .Replace(";", string.Empty)
                            .Replace("://", ":||") // protect http protocols won't be replaced in next step
                            .Replace("//", string.Empty)
                            .Replace(":||", "://"); // restore http protocols
                    }

                    sb.Append(value + "\"" + ");");
                    sb.Append('\n');
                    sb.Append("self['" + clientName + "Text'] = '");
                    sb.Append(clientName + "';");
                    sb.Append('\n');
                }

                string email = (this.ProfileUserId == this.ModuleContext.PortalSettings.UserId
                                || this.ModuleContext.PortalSettings.UserInfo.IsInRole(this.ModuleContext.PortalSettings.AdministratorRoleName))
                                   ? this.ProfileUser.Email
                                   : string.Empty;

                sb.Append("self.Email = ko.observable('");
                email = Localization.GetSafeJSString(this.Server.HtmlDecode(email));
                email = email.Replace(";", string.Empty).Replace("//", string.Empty);
				Logger.LogError("OnLoad(e={E}): email → {Email}", e, email);
                sb.Append(email + "');");
                sb.Append('\n');
                sb.Append("self.EmailText = '");
                sb.Append(this.LocalizeString("Email") + "';");
                sb.Append('\n');

                this.ProfileProperties = sb.ToString();
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.Message);
                // Module failed to load
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        private string GetRedirectUrl()
        {
			Logger.LogError(">>> Entering GetRedirectUrl()");
            // redirect user to default page if not specific the home tab, do this action to prevent loop redirect.
            var homeTabId = this.ModuleContext.PortalSettings.HomeTabId;
            string redirectUrl;

            if (homeTabId > Null.NullInteger)
            {
                redirectUrl = this.navigationManager.NavigateURL(homeTabId);
				Logger.LogError("GetRedirectUrl(): redirectUrl → {RedirectUrl}", redirectUrl);
            }
            else
            {
                redirectUrl = Globals.GetPortalDomainName(this.CurrentPortalAlias.HttpAlias, this.Request, true) +
				Logger.LogDebug("GetRedirectUrl(): redirectUrl → {RedirectUrl}", redirectUrl);
                              "/" + Globals.glbDefaultPage;
            }

			Logger.LogWarning("<<< Exiting GetRedirectUrl()");
            return redirectUrl;
        }

        private void ProcessQuerystring()
        {
			Logger.LogError(">>> Entering ProcessQuerystring()");
            // in case someone is being redirected to here from an e-mail link action we need to process that here
            var action = this.Request.QueryString["action"];
			Logger.LogError("ProcessQuerystring(): action → {Action}", action);

            if (!this.Request.IsAuthenticated && !string.IsNullOrEmpty(action))
            {
                // action requested but not logged in.
                string loginUrl = Common.Globals.LoginURL(this.Request.RawUrl, false);
                this.Response.Redirect(loginUrl);
            }

            // only process this for authenticated requests
            if (this.Request.IsAuthenticated && !string.IsNullOrEmpty(action))
            {
                // current user, i.e. the one that the request was for
                var currentUser = UserController.Instance.GetCurrentUserInfo();
				Logger.LogError("ProcessQuerystring(): currentUser → {CurrentUser}", currentUser);

                // the initiating user,i.e. the one who wanted to be friend
                // note that in this case here currentUser is visiting the profile of initiatingUser, most likely from a link in the notification e-mail
                var initiatingUser = UserController.Instance.GetUserById(PortalSettings.Current.PortalId, Convert.ToInt32(this.Request.QueryString["UserID"]));
				Logger.LogError("ProcessQuerystring(): initiatingUser → {InitiatingUser}", initiatingUser);

                if (initiatingUser.UserID == currentUser.UserID)
                {
                    return; // do not further process for users who are on their own profile page
                }

                var friendRelationship = RelationshipController.Instance.GetFriendRelationship(currentUser, initiatingUser);
				Logger.LogError("ProcessQuerystring(): friendRelationship → {FriendRelationship}", friendRelationship);

                if (friendRelationship != null)
                {
                    if (action.Equals("acceptfriend", StringComparison.OrdinalIgnoreCase))
                    {
                        var friend = UserController.GetUserById(this.hostSettings, PortalSettings.Current.PortalId, friendRelationship.UserId);
						Logger.LogError("ProcessQuerystring(): friend → {Friend}", friend);
                        FriendsController.Instance.AcceptFriend(friend);
                    }

                    if (action.Equals("followback", StringComparison.OrdinalIgnoreCase))
                    {
                        var follower = UserController.GetUserById(this.hostSettings, PortalSettings.Current.PortalId, friendRelationship.UserId);
						Logger.LogError("ProcessQuerystring(): follower → {Follower}", follower);
                        try
                        {
                            FollowersController.Instance.FollowUser(follower);
                            var notifications = NotificationsController.Instance.GetNotificationByContext(3, initiatingUser.UserID.ToString());
							Logger.LogError("ProcessQuerystring(): notifications → {Notifications}", notifications);
                            if (notifications.Count > 0)
                            {
                                NotificationsController.Instance.DeleteNotificationRecipient(notifications[0].NotificationID, currentUser.UserID);
                            }
                        }
                        catch
                        {
							Logger.LogError("Exception in ProcessQuerystring(): {Arg0}", exc.Message);
                            // ignore
                        }
                    }
                }

                this.Response.Redirect(Common.Globals.UserProfileURL(initiatingUser.UserID));
            }
			Logger.LogWarning("<<< Exiting ProcessQuerystring()");
        }
    }
}
