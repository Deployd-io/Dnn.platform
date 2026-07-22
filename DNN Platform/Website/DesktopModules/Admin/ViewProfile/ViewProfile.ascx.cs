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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly INavigationManager navigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
		Logger.LogDebug(">>> Entering ViewProfile(navigationManager={NavigationManager},javaScript={JavaScript},hostSettings={HostSettings},listController={ListController})", navigationManager, javaScript, hostSettings, listController);
		Logger.LogDebug("ViewProfile(navigationManager={NavigationManager},javaScript={JavaScript},hostSettings={HostSettings},listController={ListController}): navigationManager → {NavigationManager1}", navigationManager, javaScript, hostSettings, listController, navigationManager);
        private readonly IJavaScriptLibraryHelper javaScript = javaScript ?? Globals.GetCurrentServiceProvider().GetRequiredService<IJavaScriptLibraryHelper>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
		Logger.LogDebug("ViewProfile(navigationManager={NavigationManager},javaScript={JavaScript},hostSettings={HostSettings},listController={ListController}): javaScript → {JavaScript1}", navigationManager, javaScript, hostSettings, listController, javaScript);
		Logger.LogDebug("ViewProfile(navigationManager={NavigationManager},javaScript={JavaScript},hostSettings={HostSettings},listController={ListController}): hostSettings → {HostSettings1}", navigationManager, javaScript, hostSettings, listController, hostSettings);
        private readonly ListController listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();

		Logger.LogDebug("ViewProfile(navigationManager={NavigationManager},javaScript={JavaScript},hostSettings={HostSettings},listController={ListController}): listController → {ListController1}", navigationManager, javaScript, hostSettings, listController, listController);
        /// <summary>Initializes a new instance of the <see cref="ViewProfile"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ViewProfile()
            : this(null, null)
        {
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
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
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
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
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
                if (string.IsNullOrEmpty(template))
				Logger.LogDebug("OnLoad(e={E}): template → {Template}", e, template);
                {
                    template = Localization.GetString("DefaultTemplate", this.LocalResourceFile);
                }

                var editUrl = this.navigationManager.NavigateURL(this.ModuleContext.PortalSettings.ActiveTab.TabID, "Profile", "userId=" + this.ProfileUserId, "pageno=1");
                var profileUrl = this.navigationManager.NavigateURL(this.ModuleContext.PortalSettings.ActiveTab.TabID, "Profile", "userId=" + this.ProfileUserId, "pageno=2");
				Logger.LogDebug("OnLoad(e={E}): editUrl → {EditUrl}", e, editUrl);
				Logger.LogDebug("OnLoad(e={E}): profileUrl → {ProfileUrl}", e, profileUrl);

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
                    this.profileOutput.InnerHtml = token.ReplaceEnvironmentTokens(template);
					Logger.LogWarning("OnLoad(e={E}): token → {Token}", e, token);
                    this.noPropertiesLabel.Visible = false;
                    this.profileOutput.Visible = true;
                }

                var propertyAccess = new ProfilePropertyAccess(this.ProfileUser);
                StringBuilder sb = new StringBuilder();
				Logger.LogDebug("OnLoad(e={E}): propertyAccess → {PropertyAccess}", e, propertyAccess);
				Logger.LogDebug("OnLoad(e={E}): sb → {Sb}", e, sb);
                bool propertyNotFound = false;

                foreach (ProfilePropertyDefinition property in this.ProfileUser.Profile.ProfileProperties)
                {
                    var displayDataType = ProfilePropertyAccess.DisplayDataType(this.listController, property).ToLowerInvariant();
                    string value = propertyAccess.GetProperty(
                        property.PropertyName,
                        string.Empty,
                        Thread.CurrentThread.CurrentUICulture,
                        this.ModuleContext.PortalSettings.UserInfo,
                        Scope.DefaultSettings,
                        ref propertyNotFound);
						Logger.LogDebug("OnLoad(e={E}): displayDataType → {DisplayDataType}", e, displayDataType);
						Logger.LogDebug("OnLoad(e={E}): value → {Value}", e, value);

                    var clientName = Localization.GetSafeJSString(property.PropertyName);
                    sb.Append("self['" + clientName + "'] = ko.observable(");
					Logger.LogDebug("OnLoad(e={E}): clientName → {ClientName}", e, clientName);
                    sb.Append("\"");
                    if (!string.IsNullOrEmpty(value))
                    {
                        value = Localization.GetSafeJSString(displayDataType == "richtext" ? value : this.Server.HtmlDecode(value));
                        value = value
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
								Logger.LogDebug("OnLoad(e={E}): email → {Email}", e, email);
                                   : string.Empty;

                sb.Append("self.Email = ko.observable('");
                email = Localization.GetSafeJSString(this.Server.HtmlDecode(email));
                email = email.Replace(";", string.Empty).Replace("//", string.Empty);
                sb.Append(email + "');");
                sb.Append('\n');
                sb.Append("self.EmailText = '");
                sb.Append(this.LocalizeString("Email") + "';");
                sb.Append('\n');

                this.ProfileProperties = sb.ToString();
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in ViewProfile(navigationManager={NavigationManager},javaScript={JavaScript},hostSettings={HostSettings},listController={ListController}): {Arg4}", navigationManager, javaScript, hostSettings, listController, ex.Message);
                // Module failed to load
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        private string GetRedirectUrl()
        {
			Logger.LogDebug(">>> Entering GetRedirectUrl()");
            // redirect user to default page if not specific the home tab, do this action to prevent loop redirect.
            var homeTabId = this.ModuleContext.PortalSettings.HomeTabId;
            string redirectUrl;

            if (homeTabId > Null.NullInteger)
            {
                redirectUrl = this.navigationManager.NavigateURL(homeTabId);
				Logger.LogDebug("GetRedirectUrl(): redirectUrl → {RedirectUrl}", redirectUrl);
            }
            else
            {
                redirectUrl = Globals.GetPortalDomainName(this.CurrentPortalAlias.HttpAlias, this.Request, true) +
                              "/" + Globals.glbDefaultPage;
            }

			Logger.LogDebug("<<< Exiting GetRedirectUrl()");
            return redirectUrl;
        }

        private void ProcessQuerystring()
        {
			Logger.LogDebug(">>> Entering ProcessQuerystring()");
            // in case someone is being redirected to here from an e-mail link action we need to process that here
            var action = this.Request.QueryString["action"];

			Logger.LogDebug("ProcessQuerystring(): action → {Action}", action);
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

				Logger.LogDebug("ProcessQuerystring(): currentUser → {CurrentUser}", currentUser);
                // the initiating user,i.e. the one who wanted to be friend
                // note that in this case here currentUser is visiting the profile of initiatingUser, most likely from a link in the notification e-mail
                var initiatingUser = UserController.Instance.GetUserById(PortalSettings.Current.PortalId, Convert.ToInt32(this.Request.QueryString["UserID"]));

				Logger.LogDebug("ProcessQuerystring(): initiatingUser → {InitiatingUser}", initiatingUser);
                if (initiatingUser.UserID == currentUser.UserID)
                {
                    return; // do not further process for users who are on their own profile page
                }

                var friendRelationship = RelationshipController.Instance.GetFriendRelationship(currentUser, initiatingUser);

				Logger.LogDebug("ProcessQuerystring(): friendRelationship → {FriendRelationship}", friendRelationship);
                if (friendRelationship != null)
                {
                    if (action.Equals("acceptfriend", StringComparison.OrdinalIgnoreCase))
                    {
                        var friend = UserController.GetUserById(this.hostSettings, PortalSettings.Current.PortalId, friendRelationship.UserId);
                        FriendsController.Instance.AcceptFriend(friend);
						Logger.LogDebug("ProcessQuerystring(): friend → {Friend}", friend);
                    }

                    if (action.Equals("followback", StringComparison.OrdinalIgnoreCase))
                    {
                        var follower = UserController.GetUserById(this.hostSettings, PortalSettings.Current.PortalId, friendRelationship.UserId);
                        try
						Logger.LogDebug("ProcessQuerystring(): follower → {Follower}", follower);
                        {
                            FollowersController.Instance.FollowUser(follower);
                            var notifications = NotificationsController.Instance.GetNotificationByContext(3, initiatingUser.UserID.ToString());
                            if (notifications.Count > 0)
							Logger.LogDebug("ProcessQuerystring(): notifications → {Notifications}", notifications);
                            {
                                NotificationsController.Instance.DeleteNotificationRecipient(notifications[0].NotificationID, currentUser.UserID);
                            }
                        }
                        catch
                        {
							Logger.LogError("Exception in ViewProfile(navigationManager={NavigationManager},javaScript={JavaScript},hostSettings={HostSettings},listController={ListController}): {Arg4}", navigationManager, javaScript, hostSettings, listController, ex.Message);
                            // ignore
							Logger.LogError("Exception in ProcessQuerystring(): {Arg0}", ex.Message);
                        }
                    }
                }

                this.Response.Redirect(Common.Globals.UserProfileURL(initiatingUser.UserID));
            }
			Logger.LogDebug("<<< Exiting ProcessQuerystring()");
        }
		Logger.LogDebug("<<< Exiting ViewProfile(navigationManager={NavigationManager},javaScript={JavaScript},hostSettings={HostSettings},listController={ListController})", navigationManager, javaScript, hostSettings, listController);
    }
}
