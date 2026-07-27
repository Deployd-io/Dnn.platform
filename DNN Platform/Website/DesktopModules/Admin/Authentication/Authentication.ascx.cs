// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.UserControls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Manages the Authentication settings.</summary>
    public partial class Authentication : PortalModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IHostSettings hostSettings;
        private readonly List<AuthenticationSettingsBase> settingControls = new List<AuthenticationSettingsBase>();

        /// <summary>Initializes a new instance of the <see cref="Authentication"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public Authentication()
            : this(null)
        {
		Logger.LogDebug(">>> Entering Authentication()");
		Logger.LogDebug("<<< Exiting Authentication()");
		Logger.LogDebug(">>> Entering Authentication(hostSettings={HostSettings})", hostSettings);
		Logger.LogDebug("<<< Exiting Authentication(hostSettings={HostSettings})", hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="Authentication"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public Authentication(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? this.DependencyProvider.GetRequiredService<IHostSettings>();
			Logger.LogDebug("Authentication(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.cmdUpdate.Click += this.OnUpdateClick;

            var authSystems = AuthenticationController.GetEnabledAuthenticationServices(this.hostSettings);

			Logger.LogDebug("OnLoad(e={E}): authSystems → {AuthSystems}", e, authSystems);
            foreach (var authSystem in authSystems)
            {
                // Add a Section Header
                var sectionHeadControl = (SectionHeadControl)this.LoadControl("~/controls/SectionHeadControl.ascx");
                sectionHeadControl.IncludeRule = true;
				Logger.LogDebug("OnLoad(e={E}): sectionHeadControl → {SectionHeadControl}", e, sectionHeadControl);
				Logger.LogDebug("OnLoad(e={E}): sectionHeadControl.IncludeRule → {Arg1}", e, sectionHeadControl.IncludeRule);
				Logger.LogDebug("OnLoad(e={E}): sectionHeadControl.CssClass → {Arg1}", e, sectionHeadControl.CssClass);
                sectionHeadControl.CssClass = "Head";

                // Create a <div> to hold the control
                var container = new HtmlGenericControl();
                container.ID = authSystem.AuthenticationType;
				Logger.LogDebug("OnLoad(e={E}): container → {Container}", e, container);
				Logger.LogDebug("OnLoad(e={E}): container.ID → {Arg1}", e, container.ID);

                var authSettingsControl = (AuthenticationSettingsBase)this.LoadControl("~/" + authSystem.SettingsControlSrc);

				Logger.LogDebug("OnLoad(e={E}): authSettingsControl → {AuthSettingsControl}", e, authSettingsControl);
                // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
                // this is necessary for the Localization in PageBase
                authSettingsControl.ID = Path.GetFileNameWithoutExtension(authSystem.SettingsControlSrc) + "_" + authSystem.AuthenticationType;

                // Add Settings Control to Container
                container.Controls.Add(authSettingsControl);
                this.settingControls.Add(authSettingsControl);

                // Add Section Head Control to Container
                this.pnlSettings.Controls.Add(sectionHeadControl);

                // Add Container to Controls
                this.pnlSettings.Controls.Add(container);

                // Attach Settings Control's container to Section Head Control
                sectionHeadControl.Section = container.ID;
				Logger.LogDebug("OnLoad(e={E}): sectionHeadControl.Section → {Arg1}", e, sectionHeadControl.Section);

                // Get Section Head Text from the setting controls LocalResourceFile
                authSettingsControl.LocalResourceFile = authSettingsControl.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" +
                                                        Path.GetFileNameWithoutExtension(authSystem.SettingsControlSrc);
														Logger.LogDebug("OnLoad(e={E}): authSettingsControl.LocalResourceFile → {Arg1}", e, authSettingsControl.LocalResourceFile);
                sectionHeadControl.Text = Localization.GetString("Title", authSettingsControl.LocalResourceFile);
                this.pnlSettings.Controls.Add(new LiteralControl("<br/>"));
                this.cmdUpdate.Visible = this.IsEditable;
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        protected void OnUpdateClick(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnUpdateClick(sender={Sender},e={E})", sender, e);
            foreach (var settingControl in this.settingControls)
            {
                settingControl.UpdateSettings();
            }

            // Validate Enabled
            var enabled = false;
            var authSystems = AuthenticationController.GetEnabledAuthenticationServices(this.hostSettings);
			Logger.LogDebug("OnUpdateClick(sender={Sender},e={E}): enabled → {Enabled}", sender, e, enabled);
			Logger.LogDebug("OnUpdateClick(sender={Sender},e={E}): authSystems → {AuthSystems}", sender, e, authSystems);
            foreach (var authSystem in authSystems)
            {
                var authLoginControl = (AuthenticationLoginBase)this.LoadControl("~/" + authSystem.LoginControlSrc);

				Logger.LogDebug("OnUpdateClick(sender={Sender},e={E}): authLoginControl → {AuthLoginControl}", sender, e, authLoginControl);
                // Check if AuthSystem is Enabled
                if (authLoginControl.Enabled)
                {
                    enabled = true;
                    break;
                }
            }

            if (!enabled)
            {
                // Display warning
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("NoProvidersEnabled", this.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }
			Logger.LogDebug("<<< Exiting OnUpdateClick(sender={Sender},e={E})", sender, e);
        }
    }
}
