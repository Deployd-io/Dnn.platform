// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html
{
    using System;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.WebControls;
    using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;

    /// <summary>The HtmlModule Class provides the UI for displaying the HTML.</summary>
    public partial class HtmlModule : HtmlModuleBase, IActionable
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly IWorkflowManager workflowManager = WorkflowManager.Instance;
        private readonly INavigationManager navigationManager;
        private readonly HtmlTextController htmlTextController;
        private bool editorEnabled;
        private int workflowId;

        /// <summary>Initializes a new instance of the <see cref="HtmlModule"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public HtmlModule()
            : this(null, null)
        {
		_logger.LogDebug(">>> Entering HtmlModule()");
        }

		_logger.LogDebug("<<< Exiting HtmlModule()");
        /// <summary>Initializes a new instance of the <see cref="HtmlModule"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="htmlTextController">The HTML/Text controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with HtmlModuleSettingsRepository. Scheduled removal in v12.0.0.")]
        public HtmlModule(INavigationManager navigationManager, HtmlTextController htmlTextController)
            : this(null, navigationManager, htmlTextController)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="HtmlModule"/> class.</summary>
        /// <param name="settingsRepository">The settings repository.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="htmlTextController">The HTML/Text controller.</param>
        public HtmlModule(HtmlModuleSettingsRepository settingsRepository, INavigationManager navigationManager, HtmlTextController htmlTextController)
            : base(settingsRepository)
        {
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.htmlTextController = htmlTextController ?? this.DependencyProvider.GetRequiredService<HtmlTextController>();
			_logger.LogDebug("HtmlModule(settingsRepository={SettingsRepository},navigationManager={NavigationManager},htmlTextController={HtmlTextController}): htmlTextController → {HtmlTextController1}", settingsRepository, navigationManager, htmlTextController, htmlTextController);
        }

        /// <summary>Gets moduleActions is an interface property that returns the module actions collection for the module.</summary>
        public ModuleActionCollection ModuleActions =>
            new()
            {
                {
                    this.GetNextActionID(),
                    Localization.GetString(ModuleActionType.AddContent, this.LocalResourceFile),
                    ModuleActionType.AddContent,
                    string.Empty,
                    string.Empty,
                    this.EditUrl(),
                    false,
                    SecurityAccessLevel.Edit,
                    true,
                    false
                },
                {
                    this.GetNextActionID(),
                    Localization.GetString("MyWork.Action", this.LocalResourceFile),
                    "MyWork.Action",
                    string.Empty,
                    "view.gif",
                    this.EditUrl("MyWork"),
                    false,
                    SecurityAccessLevel.Edit,
                    true,
                    false
                },
            };

        /// <summary>Page_Init runs when the control is initialized.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInit(EventArgs e)
        {
			_logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);
            this.lblContent.UpdateLabel += this.LblContent_UpdateLabel;
            this.editorEnabled = this.PortalSettings.InlineEditorEnabled;
			_logger.LogDebug("OnInit(e={E}): this.editorEnabled → {Arg1}", e, this.editorEnabled);
            try
            {
                this.workflowId = this.htmlTextController.GetWorkflow(this.ModuleId, this.TabId, this.PortalId).Value;
				_logger.LogDebug("OnInit(e={E}): workflowId → {WorkflowId}", e, workflowId);

                // Add an Action Event Handler to the Skin
                this.AddActionHandler(this.ModuleAction_Click);
            }
            catch (Exception exc)
            {
				_logger.LogError("Exception in OnInit(e={E}): {Arg1}", e, exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			_logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <summary>Page_Load runs when the control is loaded.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
			_logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            try
            {
                // edit in place
                if (this.editorEnabled && this.IsEditable && Personalization.GetUserMode() == PortalSettings.Mode.Edit)
                {
                    this.editorEnabled = true;
					_logger.LogDebug("OnLoad(e={E}): this.editorEnabled → {Arg1}", e, this.editorEnabled);
                }
                else
                {
                    this.editorEnabled = false;
                }

                // get content
                HtmlTextInfo htmlTextInfo = null;
				_logger.LogDebug("OnLoad(e={E}): htmlTextInfo → {HtmlTextInfo}", e, htmlTextInfo);
                string contentString = string.Empty;
				_logger.LogDebug("OnLoad(e={E}): contentString → {ContentString}", e, contentString);

                htmlTextInfo = this.htmlTextController.GetTopHtmlText(this.ModuleId, !this.IsEditable, this.workflowId);

                if (htmlTextInfo != null)
                {
                    // don't decode yet (this is done in FormatHtmlText)
                    contentString = htmlTextInfo.Content;
                }
                else
                {
                    // get default content from resource file
                    if (Personalization.GetUserMode() == PortalSettings.Mode.Edit)
                    {
                        if (this.editorEnabled)
                        {
                            contentString = Localization.GetString("AddContentFromToolBar.Text", this.LocalResourceFile);
                        }
                        else
                        {
                            contentString = Localization.GetString("AddContentFromActionMenu.Text", this.LocalResourceFile);
                        }
                    }
                    else
                    {
                        // hide the module if no content and in view mode
                        this.ContainerControl.Visible = false;
                    }
                }

                // token replace
                this.editorEnabled = this.editorEnabled && !this.Settings.ReplaceTokens;

                // localize toolbar
                if (this.editorEnabled)
                {
                    foreach (DNNToolBarButton button in this.editorDnnToobar.Buttons)
                    {
                        button.ToolTip = Localization.GetString(button.ToolTip + ".ToolTip", this.LocalResourceFile);
                    }
                }
                else
                {
                    this.editorDnnToobar.Visible = false;
                }

                this.lblContent.EditEnabled = this.editorEnabled;

                // add content to module
                this.lblContent.Controls.Add(new LiteralControl(HtmlTextController.FormatHtmlText(this.ModuleId, contentString, this.Settings, this.PortalSettings, this.Page)));

                // set normalCheckBox on the content wrapper to prevent form decoration if its disabled.
                if (!this.Settings.UseDecorate)
                {
                    this.lblContent.CssClass = string.Format("{0} normalCheckBox", this.lblContent.CssClass);
                }

                if (this.IsPostBack && AJAX.IsEnabled() && AJAX.GetScriptManager(this.Page).IsInAsyncPostBack)
                {
			_logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.getMessage());
			_logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
                    var resetScript = $@"
if(typeof dnn !== 'undefined' && typeof dnn.controls !== 'undefined' && typeof dnn.controls.controls !== 'undefined'){{
    var control = dnn.controls.controls['{HttpUtility.JavaScriptStringEncode(this.lblContent.ClientID)}'];
    if(control && control.container !== $get('{HttpUtility.JavaScriptStringEncode(this.lblContent.ClientID)}')){{
        dnn.controls.controls['{HttpUtility.JavaScriptStringEncode(this.lblContent.ClientID)}'] = null;
    }}
}};";
					_logger.LogDebug("OnLoad(e={E}): control → {Control}", e, control);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), $"ResetHtmlModule{this.ClientID}", resetScript, true);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>  lblContent_UpdateLabel allows for inline editing of content.</summary>
        private void LblContent_UpdateLabel(object source, DNNLabelEditEventArgs e)
        {
			_logger.LogDebug(">>> Entering LblContent_UpdateLabel(source={Source},e={E})", source, e);
            try
            {
                // verify security
                if (!this.PortalSettings.AllowJsInHtmlModule && HtmlUtils.ContainsJavaScript(e.Text))
                {
					_logger.LogWarning("Throwing SecurityException from LblContent_UpdateLabel(source={Source},e={E})", source, e);
                    throw new SecurityException();
                }
                else if (this.editorEnabled && this.IsEditable && Personalization.GetUserMode() == PortalSettings.Mode.Edit)
                {
                    // get content
                    HtmlTextInfo objContent = this.htmlTextController.GetTopHtmlText(this.ModuleId, false, this.workflowId);
					_logger.LogDebug("LblContent_UpdateLabel(source={Source},e={E}): objContent → {ObjContent}", source, e, objContent);
                    if (objContent == null)
                    {
                        objContent = new HtmlTextInfo();
                        objContent.ItemID = -1;
						_logger.LogDebug("LblContent_UpdateLabel(source={Source},e={E}): objContent.ItemID → {Arg2}", source, e, objContent.ItemID);
                    }

                    // set content attributes
                    objContent.ModuleID = this.ModuleId;
					_logger.LogDebug("LblContent_UpdateLabel(source={Source},e={E}): objContent.ModuleID → {Arg2}", source, e, objContent.ModuleID);
                    objContent.Content = this.Server.HtmlEncode(e.Text);
                    objContent.WorkflowID = this.workflowId;
					_logger.LogDebug("LblContent_UpdateLabel(source={Source},e={E}): objContent.WorkflowID → {Arg2}", source, e, objContent.WorkflowID);
                    objContent.StateID = this.workflowManager.GetWorkflow(this.workflowId).FirstState.StateID;

                    // save the content
                    this.htmlTextController.UpdateHtmlText(objContent, this.htmlTextController.GetMaximumVersionHistory(this.PortalId));
                }
                else
                {
                    throw new SecurityException();
                }
            }
            catch (Exception exc)
            {
				_logger.LogError("Exception in LblContent_UpdateLabel(source={Source},e={E}): {Arg2}", source, e, exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			_logger.LogDebug("<<< Exiting LblContent_UpdateLabel(source={Source},e={E})", source, e);
        }

        /// <summary>  ModuleAction_Click handles all ModuleAction events raised from the action menu.</summary>
        private void ModuleAction_Click(object sender, ActionEventArgs e)
        {
			_logger.LogDebug(">>> Entering ModuleAction_Click(sender={Sender},e={E})", sender, e);
            try
            {
                if (e.Action.CommandArgument == "publish")
                {
                    // verify security
                    if (this.IsEditable && Personalization.GetUserMode() == PortalSettings.Mode.Edit)
                    {
                        // get content
                        HtmlTextInfo objContent = this.htmlTextController.GetTopHtmlText(this.ModuleId, false, this.workflowId);
						_logger.LogDebug("ModuleAction_Click(sender={Sender},e={E}): objContent → {ObjContent}", sender, e, objContent);
                        var workflow = this.workflowManager.GetWorkflow(this.workflowId);
						_logger.LogDebug("ModuleAction_Click(sender={Sender},e={E}): workflow → {Workflow}", sender, e, workflow);
                        if (objContent.StateID == workflow.FirstState.StateID)
                        {
                            // publish content
                            objContent.StateID = workflow.LastState.StateID;
							_logger.LogDebug("ModuleAction_Click(sender={Sender},e={E}): objContent.StateID → {Arg2}", sender, e, objContent.StateID);

                            // save the content
                            this.htmlTextController.UpdateHtmlText(objContent, this.htmlTextController.GetMaximumVersionHistory(this.PortalId));

                            // refresh page
                            this.Response.Redirect(this.navigationManager.NavigateURL(), true);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
				_logger.LogError("Exception in ModuleAction_Click(sender={Sender},e={E}): {Arg2}", sender, e, exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			_logger.LogDebug("<<< Exiting ModuleAction_Click(sender={Sender},e={E})", sender, e);
        }
    }
}
