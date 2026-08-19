// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.UrlManagement
{
    using System;
    using System.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.UI.Modules;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A control which displays the settings of a URL provider.</summary>
    public partial class ProviderSettings : ModuleUserControlBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UrlProviderSettings.ascx));
        private readonly INavigationManager navigationManager;

        private int providerId;
        private IExtensionUrlProviderSettingsControl providerSettingsControl;

        /// <summary>Initializes a new instance of the <see cref="ProviderSettings"/> class.</summary>
        public ProviderSettings()
        {
			Logger.LogDebug(">>> Entering ProviderSettings()");
            this.navigationManager = Globals.GetCurrentServiceProvider().GetService<INavigationManager>();
			Logger.LogDebug("ProviderSettings(): navigationManager → {NavigationManager}", navigationManager);
			Logger.LogDebug("<<< Exiting ProviderSettings()");
        }

        private string DisplayMode => (this.Request.QueryString["Display"] ?? string.Empty).ToLowerInvariant();

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.cmdUpdate.Click += this.CmdUpdate_Click;
            this.cmdCancel.Click += this.CmdCancel_Click;

            this.providerId = Convert.ToInt32(this.Request.Params["ProviderId"]);
			Logger.LogDebug("OnInit(e={E}): providerId → {ProviderId}", e, providerId);

            var provider = ExtensionUrlProviderController.GetModuleProviders(this.ModuleContext.PortalId)
                                .SingleOrDefault(p => p.ProviderConfig.ExtensionUrlProviderId == this.providerId);

            if (provider != null)
            {
                var settingsControlSrc = provider.ProviderConfig.SettingsControlSrc;

                var settingsControl = this.Page.LoadControl(settingsControlSrc);
				Logger.LogDebug("OnInit(e={E}): settingsControl → {SettingsControl}", e, settingsControl);

                this.providerSettingsPlaceHolder.Controls.Add(settingsControl);

                // ReSharper disable SuspiciousTypeConversion.Global
                this.providerSettingsControl = settingsControl as IExtensionUrlProviderSettingsControl;
				Logger.LogDebug("OnInit(e={E}): providerSettingsControl → {ProviderSettingsControl}", e, providerSettingsControl);
				Logger.LogDebug("OnInit(e={E}): this.providerSettingsControl → {Arg1}", e, this.providerSettingsControl);

                // ReSharper restore SuspiciousTypeConversion.Global
                if (this.providerSettingsControl != null)
                {
                    this.providerSettingsControl.Provider = provider.ProviderConfig;
                }
            }
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            if (this.providerSettingsControl != null)
            {
                this.providerSettingsControl.LoadSettings();
            }

            if (this.DisplayMode == "editor" || this.DisplayMode == "settings")
            {
                this.cmdCancel.Visible = false;
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        private void CmdCancel_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdCancel_Click(sender={Sender},e={E})", sender, e);
            this.Response.Redirect(this.navigationManager.NavigateURL(this.ModuleContext.PortalSettings.ActiveTab.TabID));
			Logger.LogDebug("<<< Exiting CmdCancel_Click(sender={Sender},e={E})", sender, e);
        }

        private void CmdUpdate_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdUpdate_Click(sender={Sender},e={E})", sender, e);
            if (!this.Page.IsValid)
            {
                return;
            }

            if (this.providerSettingsControl != null)
            {
                var settings = this.providerSettingsControl.SaveSettings();
				Logger.LogDebug("CmdUpdate_Click(sender={Sender},e={E}): settings → {Settings}", sender, e, settings);
                foreach (var setting in settings)
                {
                    ExtensionUrlProviderController.SaveSetting(this.providerId, this.ModuleContext.PortalId, setting.Key, setting.Value);
                }
            }

            if (this.DisplayMode != "editor" && this.DisplayMode != "settings")
            {
                this.Response.Redirect(this.navigationManager.NavigateURL(this.ModuleContext.PortalSettings.ActiveTab.TabID));
            }
			Logger.LogDebug("<<< Exiting CmdUpdate_Click(sender={Sender},e={E})", sender, e);
        }
    }
}
