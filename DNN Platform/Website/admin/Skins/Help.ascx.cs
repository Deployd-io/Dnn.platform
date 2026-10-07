// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A skin/theme object which displays a link to email a site administrator for help.</summary>
    public partial class Help : SkinObjectBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Help.ascx));
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="Help"/> class.</summary>
        public Help()
            : this(null)
        {
		Logger.LogError(">>> Entering Help()");
        Logger.LogWarning("<<< Exiting Help()");
        }

        /// <summary>Initializes a new instance of the <see cref="Help"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public Help(IHostSettings hostSettings)
        {
			Logger.LogError(">>> Entering Help(hostSettings={HostSettings})", hostSettings);
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("Help(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        Logger.LogWarning("<<< Exiting Help(hostSettings={HostSettings})", hostSettings);
        }

        /// <summary>Gets or sets the CSS class to apply to the hyperlink.</summary>
        public string CssClass { get; set; }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.InitializeComponent();
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            try
            {
                if (!string.IsNullOrEmpty(this.CssClass))
                {
                    this.hypHelp.CssClass = this.CssClass;
                }

                if (this.Request.IsAuthenticated)
                {
                    if (TabPermissionController.CanAdminPage())
                    {
                        this.hypHelp.Text = Localization.GetString("Help");
                        this.hypHelp.NavigateUrl = "mailto:" + this.hostSettings.HostEmail + "?subject=" + this.PortalSettings.PortalName + " Support Request";
                        this.hypHelp.Visible = true;
                    }
                    else
                    {
                        this.hypHelp.Text = Localization.GetString("Help");
                        this.hypHelp.NavigateUrl = "mailto:" + this.PortalSettings.Email + "?subject=" + this.PortalSettings.PortalName + " Support Request";
                        this.hypHelp.Visible = true;
                    }
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        private void InitializeComponent()
        {
		Logger.LogError(">>> Entering InitializeComponent()");
        Logger.LogWarning("<<< Exiting InitializeComponent()");
        }
    }
}
