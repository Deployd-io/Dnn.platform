// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Controls
{
    using System;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    /// <summary>The Privacy PortalModuleBase displays the Privacy text.</summary>
    public partial class Privacy : PortalModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Privacy.ascx));
        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            // CODEGEN: This method call is required by the Web Form Designer
            // Do not modify it using the code editor.
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
                if (!this.Page.IsPostBack)
                {
                    this.lblPrivacy.Text = Localization.GetSystemMessage(this.PortalSettings, "MESSAGE_PORTAL_PRIVACY");
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
