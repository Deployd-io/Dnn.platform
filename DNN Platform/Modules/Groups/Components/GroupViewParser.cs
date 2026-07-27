// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Groups.Components
{
    using System.Linq;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    public class GroupViewParser
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="GroupViewParser"/> class.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="roleInfo">The role info.</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="template">The template.</param>
        /// <param name="groupViewTabId">The tab ID of the group view module.</param>
        public GroupViewParser(PortalSettings portalSettings, RoleInfo roleInfo, UserInfo currentUser, string template, int groupViewTabId)
        {
			Logger.LogDebug(">>> Entering GroupViewParser(portalSettings={PortalSettings},roleInfo={RoleInfo},currentUser={CurrentUser},template={Template},groupViewTabId={GroupViewTabId})", portalSettings, roleInfo, currentUser, template, groupViewTabId);
            this.PortalSettings = portalSettings;
            this.RoleInfo = roleInfo;
			Logger.LogDebug("GroupViewParser(portalSettings={PortalSettings},roleInfo={RoleInfo},currentUser={CurrentUser},template={Template},groupViewTabId={GroupViewTabId}): this.PortalSettings → {Arg5}", portalSettings, roleInfo, currentUser, template, groupViewTabId, this.PortalSettings);
			Logger.LogDebug("GroupViewParser(portalSettings={PortalSettings},roleInfo={RoleInfo},currentUser={CurrentUser},template={Template},groupViewTabId={GroupViewTabId}): this.RoleInfo → {Arg5}", portalSettings, roleInfo, currentUser, template, groupViewTabId, this.RoleInfo);
            this.CurrentUser = currentUser;
            this.Template = template;
            this.GroupViewTabId = groupViewTabId;
			Logger.LogDebug("GroupViewParser(portalSettings={PortalSettings},roleInfo={RoleInfo},currentUser={CurrentUser},template={Template},groupViewTabId={GroupViewTabId}): this.CurrentUser → {Arg5}", portalSettings, roleInfo, currentUser, template, groupViewTabId, this.CurrentUser);
			Logger.LogDebug("GroupViewParser(portalSettings={PortalSettings},roleInfo={RoleInfo},currentUser={CurrentUser},template={Template},groupViewTabId={GroupViewTabId}): this.Template → {Arg5}", portalSettings, roleInfo, currentUser, template, groupViewTabId, this.Template);
			Logger.LogDebug("GroupViewParser(portalSettings={PortalSettings},roleInfo={RoleInfo},currentUser={CurrentUser},template={Template},groupViewTabId={GroupViewTabId}): this.GroupViewTabId → {Arg5}", portalSettings, roleInfo, currentUser, template, groupViewTabId, this.GroupViewTabId);
            this.NavigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
			Logger.LogDebug("<<< Exiting GroupViewParser(portalSettings={PortalSettings},roleInfo={RoleInfo},currentUser={CurrentUser},template={Template},groupViewTabId={GroupViewTabId})", portalSettings, roleInfo, currentUser, template, groupViewTabId);
        }

        public string Template { get; set; }

        public int GroupViewTabId { get; set; }

        public string GroupEditUrl { get; set; }

        protected INavigationManager NavigationManager { get; }

        private PortalSettings PortalSettings { get; set; }

        private RoleInfo RoleInfo { get; set; }

        private UserInfo CurrentUser { get; set; }

        public string ParseView()
        {
			Logger.LogDebug(">>> Entering ParseView()");
            var membershipPending = false;
            var isOwner = false;
			Logger.LogDebug("ParseView(): membershipPending → {MembershipPending}", membershipPending);
			Logger.LogDebug("ParseView(): isOwner → {IsOwner}", isOwner);

            if (HttpContext.Current.Request.IsAuthenticated)
            {
                var userRoleInfo = this.CurrentUser.Social.Roles.FirstOrDefault(r => r.RoleID == this.RoleInfo.RoleID);

				Logger.LogDebug("ParseView(): userRoleInfo → {UserRoleInfo}", userRoleInfo);
                if (userRoleInfo != null)
                {
                    isOwner = userRoleInfo.IsOwner;
                    if (userRoleInfo.Status == RoleStatus.Pending)
                    {
                        membershipPending = true;
                    }
                }

                if (this.RoleInfo.CreatedByUserID == this.CurrentUser.UserID || this.CurrentUser.IsSuperUser)
                {
                    isOwner = true;
                }
            }

            var editUrl = Localization.GetString("GroupEditUrl", Constants.SharedResourcesPath);

			Logger.LogDebug("ParseView(): editUrl → {EditUrl}", editUrl);
            if (isOwner)
            {
                this.Template = this.Template.Replace("[GROUPEDITBUTTON]", string.Format(editUrl, this.GroupEditUrl));
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsNotOwner", false);
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsOwner", true);
            }
            else if (this.CurrentUser.IsInRole(this.RoleInfo.RoleName))
            {
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsNotOwner", true);
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsOwner", false);
            }

            this.Template = Utilities.ParseTokenWrapper(this.Template, "IsNotOwner", false);
            this.Template = Utilities.ParseTokenWrapper(this.Template, "IsOwner", false);

            if (this.CurrentUser.IsInRole(this.RoleInfo.RoleName) || !HttpContext.Current.Request.IsAuthenticated || membershipPending)
            {
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsNotMember", false);
            }
            else
            {
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsNotMember", true);
            }

            if (this.CurrentUser.IsInRole(this.RoleInfo.RoleName))
            {
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsMember", true);
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsPendingMember", false);
            }
            else
            {
                this.Template = Utilities.ParseTokenWrapper(this.Template, "IsMember", false);
            }

            this.Template = Utilities.ParseTokenWrapper(this.Template, "AllowJoin", this.RoleInfo.IsPublic);

            this.Template = this.Template.Replace("[GROUPEDITBUTTON]", string.Empty);

            var url = this.NavigationManager.NavigateURL(this.GroupViewTabId, string.Empty, new string[] { "groupid=" + this.RoleInfo.RoleID.ToString() });

			Logger.LogDebug("ParseView(): url → {Url}", url);
            this.Template = Utilities.ParseTokenWrapper(this.Template, "IsPendingMember", membershipPending);
            this.Template = this.Template.Replace("[groupviewurl]", url);
            Components.GroupItemTokenReplace tokenReplace = new Components.GroupItemTokenReplace(this.RoleInfo);
            this.Template = tokenReplace.ReplaceGroupItemTokens(this.Template);
			Logger.LogDebug("ParseView(): tokenReplace → {TokenReplace}", tokenReplace);
			Logger.LogDebug("<<< Exiting ParseView()");
            return this.Template;
        }
    }
}
