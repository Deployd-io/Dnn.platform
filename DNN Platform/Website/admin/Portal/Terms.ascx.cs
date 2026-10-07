// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Controls
{
    using System;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    /// <summary>A control which displays the terms of the website.</summary>
    public partial class Terms : PortalModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Terms.ascx));
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

        /// <summary>The Page_Load server event handler on this page is used to populate the role information for the page.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            try
            {
                if (!this.Page.IsPostBack)
                {
                    this.lblTerms.Text = Localization.GetSystemMessage(this.PortalSettings, "MESSAGE_PORTAL_TERMS");
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        // This call is required by the Web Form Designer.
        private void InitializeComponent()
        {
		Logger.LogError(">>> Entering InitializeComponent()");
        Logger.LogWarning("<<< Exiting InitializeComponent()");
        }
    }
}
