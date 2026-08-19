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

    /// <summary>A skin/theme object which displays a link to the site's terms page.</summary>
    public partial class Terms : SkinObjectBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Terms.ascx));
        private const string MyFileName = "Terms.ascx";
        private readonly INavigationManager navigationManager;

        /// <summary>Initializes a new instance of the <see cref="Terms"/> class.</summary>
        public Terms()
        {
			Logger.LogDebug(">>> Entering Terms()");
            this.navigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
			Logger.LogDebug("Terms(): navigationManager → {NavigationManager}", navigationManager);
			Logger.LogDebug("<<< Exiting Terms()");
        }

        public string Text { get; set; }

        public string CssClass { get; set; }

        public string Rel { get; set; } = "nofollow";

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.InitializeComponent();
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            try
            {
                if (!string.IsNullOrEmpty(this.CssClass))
                {
                    this.hypTerms.CssClass = this.CssClass;
                }

                if (!string.IsNullOrEmpty(this.Text))
                {
                    this.hypTerms.Text = this.Text;
                }
                else
                {
                    this.hypTerms.Text = Localization.GetString("Terms", Localization.GetResourceFile(this, MyFileName));
                }

                this.hypTerms.NavigateUrl = this.PortalSettings.TermsTabId == Null.NullInteger ? this.navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Terms") : this.navigationManager.NavigateURL(this.PortalSettings.TermsTabId);
                if (!string.IsNullOrWhiteSpace(this.Rel))
                {
                    this.hypTerms.Attributes["rel"] = this.Rel;
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        private void InitializeComponent()
        {
		Logger.LogDebug(">>> Entering InitializeComponent()");
		Logger.LogDebug("<<< Exiting InitializeComponent()");
        }
    }
}
