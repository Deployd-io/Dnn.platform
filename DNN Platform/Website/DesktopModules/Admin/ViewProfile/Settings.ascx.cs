// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Admin.Users
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    /// <summary>A settings control for the profile module.</summary>
    public partial class ViewProfileSettings : ModuleSettingsBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public override void LoadSettings()
        {
			Logger.LogDebug(">>> Entering LoadSettings()");
            try
            {
                ClientAPI.AddButtonConfirm(this.cmdLoadDefault, Localization.GetString("LoadDefault.Confirm", this.LocalResourceFile));
                this.cmdLoadDefault.ToolTip = Localization.GetString("LoadDefault.Help", this.LocalResourceFile);

                if (!this.Page.IsPostBack)
                {
                    if (!string.IsNullOrEmpty((string)this.TabModuleSettings["ProfileTemplate"]))
                    {
                        this.txtTemplate.Text = (string)this.TabModuleSettings["ProfileTemplate"];
                    }

                    if (this.Settings.ContainsKey("IncludeButton"))
                    {
                        this.IncludeButton.Checked = Convert.ToBoolean(this.Settings["IncludeButton"]);
                    }
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in LoadSettings(): {Arg0}", ex.Message);
                // Module failed to load
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting LoadSettings()");
        }

        /// <inheritdoc />
        public override void UpdateSettings()
        {
			Logger.LogDebug(">>> Entering UpdateSettings()");
            try
            {
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, "ProfileTemplate", this.txtTemplate.Text);
                ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, "IncludeButton", this.IncludeButton.Checked.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in UpdateSettings(): {Arg0}", ex.Message);
                // Module failed to load
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting UpdateSettings()");
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            this.cmdLoadDefault.Click += this.cmdLoadDefault_Click;
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        protected void cmdLoadDefault_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering cmdLoadDefault_Click(sender={Sender},e={E})", sender, e);
            this.txtTemplate.Text = Localization.GetString("DefaultTemplate", this.LocalResourceFile);
			Logger.LogDebug("<<< Exiting cmdLoadDefault_Click(sender={Sender},e={E})", sender, e);
        }
    }
}
