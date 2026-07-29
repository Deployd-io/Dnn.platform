// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DesktopModules.Admin.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    using MembershipProvider = DotNetNuke.Security.Membership.MembershipProvider;

    /// <summary>The Profile UserModuleBase is used to register Users.</summary>
    public partial class DNNProfile(ListController listController, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, IEventLogger eventLogger) : ProfileUserControlBase
    {
		eventLogger.LogDebug(">>> Entering DNNProfile(listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},eventLogger={EventLogger})", listController, portalController, appStatus, portalGroupController, eventLogger);
        private readonly ListController listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
		eventLogger.LogDebug("DNNProfile(listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},eventLogger={EventLogger}): listController → {ListController1}", listController, portalController, appStatus, portalGroupController, eventLogger, listController);
		eventLogger.LogDebug("DNNProfile(listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},eventLogger={EventLogger}): portalController → {PortalController1}", listController, portalController, appStatus, portalGroupController, eventLogger, portalController);
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        private readonly IPortalGroupController portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
		eventLogger.LogDebug("DNNProfile(listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},eventLogger={EventLogger}): appStatus → {AppStatus1}", listController, portalController, appStatus, portalGroupController, eventLogger, appStatus);
		eventLogger.LogDebug("DNNProfile(listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},eventLogger={EventLogger}): portalGroupController → {PortalGroupController1}", listController, portalController, appStatus, portalGroupController, eventLogger, portalGroupController);
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();

		eventLogger.LogDebug("DNNProfile(listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},eventLogger={EventLogger}): eventLogger → {EventLogger1}", listController, portalController, appStatus, portalGroupController, eventLogger, eventLogger);
        /// <summary>Initializes a new instance of the <see cref="DNNProfile"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public DNNProfile()
            : this(null, null, null, null, null)
        {
        }

        /// <summary>Gets a value indicating whether the User is valid.</summary>
        public bool IsValid => this.ProfileProperties.IsValid || this.IsAdmin;

        /// <summary>Gets the UserProfile associated with this control.</summary>
        public UserProfile UserProfile
        {
            get
            {
                UserProfile profile = null;
                if (this.User != null)
                {
                    profile = this.User.Profile;
                }

                return profile;
            }
        }

        /// <summary>Gets or sets the EditorMode.</summary>
        public PropertyEditorMode EditorMode
        {
            get => this.ProfileProperties.EditMode;
            set => this.ProfileProperties.EditMode = value;
        }

        /// <summary>Gets or sets a value indicating whether the Update button.</summary>
        public bool ShowUpdate
        {
            get => this.actionsRow.Visible;
            set => this.actionsRow.Visible = value;
        }

        /// <summary>Gets a value indicating whether to display the Visibility controls.</summary>
        protected bool ShowVisibility
        {
            get
            {
                object setting = GetSetting(this.PortalId, "Profile_DisplayVisibility");
                return Convert.ToBoolean(setting) && this.IsUser;
            }
        }

        /// <summary>DataBind binds the data to the controls.</summary>
        public override void DataBind()
        {
			eventLogger.LogDebug(">>> Entering DataBind()");
            // Before we bind the Profile to the editor we need to "update" the visible data
            var properties = new ProfilePropertyDefinitionCollection();
            var imageType = this.listController.GetListEntryInfo("DataType", "Image");
			eventLogger.LogDebug("DataBind(): properties → {Properties}", properties);
			eventLogger.LogDebug("DataBind(): imageType → {ImageType}", imageType);
            foreach (ProfilePropertyDefinition profProperty in this.UserProfile.ProfileProperties)
            {
                if (this.IsAdmin && !this.IsProfile)
                {
                    profProperty.Visible = true;
					eventLogger.LogDebug("DataBind(): profProperty.Visible → {Arg0}", profProperty.Visible);
                }

                if (!profProperty.Deleted && (this.Request.IsAuthenticated || profProperty.DataType != imageType.EntryID))
                {
                    properties.Add(profProperty);
                }
            }

            this.ProfileProperties.User = this.User;
            this.ProfileProperties.ShowVisibility = this.ShowVisibility;
            this.ProfileProperties.DataSource = properties;
            this.ProfileProperties.DataBind();
			eventLogger.LogDebug("<<< Exiting DataBind()");
        }

        /// <summary>Page_Init runs when the control is initialized.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInit(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);
            this.ID = "Profile.ascx";
			eventLogger.LogDebug("OnInit(e={E}): this.ID → {Arg1}", e, this.ID);

            // Get the base Page
            if (this.Page is PageBase basePage)
            {
                // Check if culture is RTL
                this.ProfileProperties.LabelMode = basePage.PageCulture.TextInfo.IsRightToLeft ? LabelMode.Right : LabelMode.Left;
            }

            this.ProfileProperties.LocalResourceFile = this.LocalResourceFile;
			eventLogger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <summary>Page_Load runs when the control is loaded.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            this.cmdUpdate.Click += this.CmdUpdate_Click;
			eventLogger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <summary>cmdUpdate_Click runs when the Update Button is clicked.</summary>
        private void CmdUpdate_Click(object sender, EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering CmdUpdate_Click(sender={Sender},e={E})", sender, e);
            if (this.IsUserOrAdmin == false && this.UserId == Null.NullInteger)
            {
                return;
            }

            if (this.IsValid)
            {
                if (this.User.UserID == this.PortalSettings.AdministratorId)
                {
                    // Clear the Portal Cache
                    DataCache.ClearPortalCache(this.UserPortalID, true);
                }

                // Update DisplayName to conform to Format
                this.UpdateDisplayName();

                if (this.PortalSettings.Registration.RequireUniqueDisplayName)
                {
                    var usersWithSameDisplayName = (List<UserInfo>)MembershipProvider.Instance().GetUsersBasicSearch(this.PortalId, 0, 2, "DisplayName", true, "DisplayName", this.User.DisplayName);
                    if (usersWithSameDisplayName.Any(user => user.UserID != this.User.UserID))
					eventLogger.LogDebug("CmdUpdate_Click(sender={Sender},e={E}): usersWithSameDisplayName → {UsersWithSameDisplayName}", sender, e, usersWithSameDisplayName);
                    {
                        this.AddModuleMessage("DisplayNameNotUnique", ModuleMessage.ModuleMessageType.RedError, true);
                        return;
                    }
                }

                var properties = (ProfilePropertyDefinitionCollection)this.ProfileProperties.DataSource;

				eventLogger.LogDebug("CmdUpdate_Click(sender={Sender},e={E}): properties → {Properties}", sender, e, properties);
                // Update User's profile
                this.User = ProfileController.UpdateUserProfile(this.portalController, this.appStatus, this.portalGroupController, this.eventLogger, this.User, properties);

                this.OnProfileUpdated(EventArgs.Empty);
                this.OnProfileUpdateCompleted(EventArgs.Empty);
            }
			eventLogger.LogDebug("<<< Exiting CmdUpdate_Click(sender={Sender},e={E})", sender, e);
        }

        private void UpdateDisplayName()
        {
			eventLogger.LogDebug(">>> Entering UpdateDisplayName()");
            if (!string.IsNullOrEmpty(this.PortalSettings.Registration.DisplayNameFormat))
            {
                this.User.UpdateDisplayName(this.PortalSettings.Registration.DisplayNameFormat);
            }
			eventLogger.LogDebug("<<< Exiting UpdateDisplayName()");
        }
		eventLogger.LogDebug("<<< Exiting DNNProfile(listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},eventLogger={EventLogger})", listController, portalController, appStatus, portalGroupController, eventLogger);
    }
}
