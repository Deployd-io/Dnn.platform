// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.HtmlEditorManager.Views
{
    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Modules.HtmlEditorManager.ViewModels;
    using DotNetNuke.Web.Mvp;
	using Microsoft.Extensions.Logging;

    /// <summary>View control for selecting an HTML provider.</summary>
    [DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
    public partial class ProviderConfiguration : ModuleView<ProviderConfigurationViewModel>, IProviderConfigurationView
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>Occurs when the save button is clicked.</summary>
        public event EventHandler<EditorEventArgs> SaveEditorChoice;

        /// <summary>Occurs when the editor is changed.</summary>
        public event EventHandler<EditorEventArgs> EditorChanged;

        /// <summary>Gets or sets the editor panel.</summary>
        /// <value>The editor panel.</value>
        public PlaceHolder Editor
        {
            get
            {
                return this.EditorPanel;
            }

            set
            {
                this.EditorPanel = value;
            }
        }

        /// <inheritdoc />
        public void Refresh()
        {
			_logger.LogDebug(">>> Entering Refresh()");
            this.Response.Redirect(this.Request.RawUrl, true);
			_logger.LogDebug("<<< Exiting Refresh()");
        }

        /// <summary>Handles the Click event of the SaveButton control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void SaveButton_Click(object sender, EventArgs e)
        {
			_logger.LogDebug(">>> Entering SaveButton_Click(sender={Sender},e={E})", sender, e);
            this.SaveEditorChoice(this, new EditorEventArgs(this.ProvidersDropDownList.SelectedValue));
			_logger.LogDebug("<<< Exiting SaveButton_Click(sender={Sender},e={E})", sender, e);
        }

        /// <summary>Handles the SelectedIndexChanged event of the ProvidersDropDownList control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ProvidersDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
			_logger.LogDebug(">>> Entering ProvidersDropDownList_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
            this.EditorChanged(this, new EditorEventArgs(this.ProvidersDropDownList.SelectedValue));
			_logger.LogDebug("<<< Exiting ProvidersDropDownList_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			_logger.LogDebug(">>> Entering OnInit(e={E})", e);
            var currentUser = UserController.Instance.GetCurrentUserInfo();
            if (currentUser == null || !currentUser.IsSuperUser)
            {
                this.LocalResourceFile = "/DesktopModules/Admin/HtmlEditorManager/App_LocalResources/ProviderConfiguration.ascx.resx";
				_logger.LogDebug("OnInit(e={E}): this.LocalResourceFile → {Arg1}", e, this.LocalResourceFile);
                Globals.Redirect(Globals.AccessDeniedURL(this.LocalizeString("CannotManageHTMLEditorProviders")), true);
            }

            base.OnInit(e);
			_logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }
    }
}
