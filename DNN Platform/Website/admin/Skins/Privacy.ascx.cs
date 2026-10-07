// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A skin/theme object which displays a link to the site's privacy page.</summary>
    public partial class Privacy : SkinObjectBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Privacy.ascx));
        private const string MyFileName = "Privacy.ascx";
        private readonly INavigationManager navigationManager;

        /// <summary>Initializes a new instance of the <see cref="Privacy"/> class.</summary>
        public Privacy()
        {
			Logger.LogError(">>> Entering Privacy()");
            this.navigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
			Logger.LogDebug("Privacy(): navigationManager → {NavigationManager}", navigationManager);
        Logger.LogWarning("<<< Exiting Privacy()");
        }

        public string Text { get; set; }

        public string CssClass { get; set; }

        public string Rel { get; set; } = "nofollow";

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
                    this.hypPrivacy.CssClass = this.CssClass;
                }

                if (!string.IsNullOrEmpty(this.Text))
                {
                    this.hypPrivacy.Text = this.Text;
                }
                else
                {
                    this.hypPrivacy.Text = Localization.GetString("Privacy", Localization.GetResourceFile(this, MyFileName));
                }

                this.hypPrivacy.NavigateUrl = this.PortalSettings.PrivacyTabId == Null.NullInteger ? this.navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Privacy") : this.navigationManager.NavigateURL(this.PortalSettings.PrivacyTabId);
                if (!string.IsNullOrWhiteSpace(this.Rel))
                {
                    this.hypPrivacy.Attributes["rel"] = this.Rel;
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
