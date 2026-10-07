// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Authentication.OAuth
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.UI.WebControls;

    public class OAuthSettingsBase : AuthenticationSettingsBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(OAuthSettingsBase));
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected PropertyEditorControl SettingsEditor;

        protected virtual string AuthSystemApplicationName
        {
            get { return string.Empty; }
        }

        /// <inheritdoc />
        public override void UpdateSettings()
        {
			Logger.LogError(">>> Entering UpdateSettings()");
            if (this.SettingsEditor.IsValid && this.SettingsEditor.IsDirty)
            {
                var config = (OAuthConfigBase)this.SettingsEditor.DataSource;
				Logger.LogError("UpdateSettings(): config → {Config}", config);
                OAuthConfigBase.UpdateConfig(config);
            }
			Logger.LogWarning("<<< Exiting UpdateSettings()");
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            OAuthConfigBase config = OAuthConfigBase.GetConfig(this.AuthSystemApplicationName, this.PortalId);
			Logger.LogError("OnLoad(e={E}): config → {Config}", e, config);
            this.SettingsEditor.DataSource = config;
            this.SettingsEditor.DataBind();
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }
    }
}
