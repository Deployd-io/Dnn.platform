// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Authentication
{
    using System;

    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Exceptions;

    /// <summary>A settings control for DNN authentication.</summary>
    public partial class Settings : AuthenticationSettingsBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Settings.ascx));
        /// <inheritdoc />
        public override void UpdateSettings()
        {
			Logger.LogDebug(">>> Entering UpdateSettings()");
            if (this.SettingsEditor.IsValid && this.SettingsEditor.IsDirty)
            {
                var config = (AuthenticationConfig)this.SettingsEditor.DataSource;
				Logger.LogDebug("UpdateSettings(): config → {Config}", config);
                AuthenticationConfig.UpdateConfig(config);
            }
			Logger.LogDebug("<<< Exiting UpdateSettings()");
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            try
            {
                AuthenticationConfig config = AuthenticationConfig.GetConfig(this.PortalId);
				Logger.LogDebug("OnLoad(e={E}): config → {Config}", e, config);
                this.SettingsEditor.DataSource = config;
                this.SettingsEditor.DataBind();
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }
    }
}
