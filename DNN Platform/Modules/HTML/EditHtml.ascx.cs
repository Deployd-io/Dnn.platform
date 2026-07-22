// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html
{
    using System;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The EditHtml PortalModuleBase is used to manage HTML.</summary>
    public partial class EditHtml : HtmlModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly HtmlTextLogController htmlTextLogController = new HtmlTextLogController();
        private readonly IWorkflowManager workflowManager = WorkflowManager.Instance;
        private readonly INavigationManager navigationManager;
        private readonly HtmlTextController htmlTextController;
        private readonly IPortalAliasService portalAliasService;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="EditHtml"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public EditHtml()
            : this(null, null, null)
        {
		Logger.LogDebug(">>> Entering EditHtml()");
        }
		Logger.LogDebug("<<< Exiting EditHtml()");

        /// <summary>Initializes a new instance of the <see cref="EditHtml"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="htmlTextController">The HTML/Text controller.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public EditHtml(INavigationManager navigationManager, HtmlTextController htmlTextController, IPortalAliasService portalAliasService)
            : this(null, navigationManager, htmlTextController, portalAliasService, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EditHtml"/> class.</summary>
        /// <param name="settingsRepository">The settings repository.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="htmlTextController">The HTML/Text controller.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="hostSettings">The host settings.</param>
        public EditHtml(HtmlModuleSettingsRepository settingsRepository, INavigationManager navigationManager, HtmlTextController htmlTextController, IPortalAliasService portalAliasService, IHostSettings hostSettings)
            : base(settingsRepository)
        {
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.htmlTextController = htmlTextController ?? this.DependencyProvider.GetRequiredService<HtmlTextController>();
			Logger.LogDebug("EditHtml(settingsRepository={SettingsRepository},navigationManager={NavigationManager},htmlTextController={HtmlTextController},portalAliasService={PortalAliasService},hostSettings={HostSettings}): htmlTextController → {HtmlTextController1}", settingsRepository, navigationManager, htmlTextController, portalAliasService, hostSettings, htmlTextController);
			Logger.LogDebug("EditHtml(settingsRepository={SettingsRepository},navigationManager={NavigationManager},htmlTextController={HtmlTextController},portalAliasService={PortalAliasService},hostSettings={HostSettings}): hostSettings → {HostSettings1}", settingsRepository, navigationManager, htmlTextController, portalAliasService, hostSettings, hostSettings);
			Logger.LogDebug("EditHtml(settingsRepository={SettingsRepository},navigationManager={NavigationManager},htmlTextController={HtmlTextController},portalAliasService={PortalAliasService},hostSettings={HostSettings}): navigationManager → {NavigationManager1}", settingsRepository, navigationManager, htmlTextController, portalAliasService, hostSettings, navigationManager);
            this.portalAliasService = portalAliasService ?? this.DependencyProvider.GetRequiredService<IPortalAliasService>();
			Logger.LogDebug("EditHtml(settingsRepository={SettingsRepository},navigationManager={NavigationManager},htmlTextController={HtmlTextController},portalAliasService={PortalAliasService},hostSettings={HostSettings}): portalAliasService → {PortalAliasService1}", settingsRepository, navigationManager, htmlTextController, portalAliasService, hostSettings, portalAliasService);
            this.hostSettings = hostSettings ?? this.DependencyProvider.GetRequiredService<IHostSettings>();
        }

        private enum WorkflowType
        {
            DirectPublish = 1,
            SaveDraft = 2,
            ContentApproval = 3,
            CustomWorkflow = 4,
        }

        protected string CurrentView
        {
            get
            {
                if (this.phEdit.Visible)
                {
                    return "EditView";
                }
                else if (this.phPreview.Visible)
                {
                    return "PreviewView";
                }

                if (this.phHistory.Visible)
                {
                    return "HistoryView";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private int WorkflowID
        {
            get
            {
                int workflowID;

                if (this.ViewState["WorkflowID"] == null)
                {
                    workflowID = this.htmlTextController.GetWorkflow(this.ModuleId, this.TabId, this.PortalId).Value;
                    this.ViewState.Add("WorkflowID", workflowID);
                }
                else
                {
                    workflowID = int.Parse(this.ViewState["WorkflowID"].ToString());
                }

                return workflowID;
            }
        }

        private string TempContent
        {
            get
            {
                var content = string.Empty;
                if (this.ViewState["TempContent"] != null)
                {
                    content = this.ViewState["TempContent"].ToString();
                }

                return content;
            }

            set
            {
                this.ViewState["TempContent"] = value;
            }
        }

        private WorkflowType CurrentWorkflowType
        {
            get
            {
                var currentWorkflowType = default(WorkflowType);
                if (this.ViewState["_currentWorkflowType"] != null)
                {
                    currentWorkflowType = (WorkflowType)Enum.Parse(typeof(WorkflowType), this.ViewState["_currentWorkflowType"].ToString());
                }

                return currentWorkflowType;
            }

            set
            {
                this.ViewState["_currentWorkflowType"] = value;
            }
        }

        private bool IsVersioningEnabled => TabVersionSettings.Instance.IsVersioningEnabled(PortalSettings.Current.PortalId, TabController.CurrentPage.TabID);

        private bool IsWorkflowEnabled => this.IsVersioningEnabled && TabWorkflowSettings.Instance.IsWorkflowEnabled(PortalSettings.Current.PortalId, TabController.CurrentPage.TabID);

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.hlCancel.NavigateUrl = this.navigationManager.NavigateURL();

            this.cmdEdit.Click += this.OnEditClick;
            this.cmdPreview.Click += this.OnPreviewClick;
            this.cmdHistory.Click += this.OnHistoryClick;
            this.cmdMasterContent.Click += this.OnMasterContentClick;
            this.ddlRender.SelectedIndexChanged += this.OnRenderSelectedIndexChanged;
            this.cmdSave.Click += this.OnSaveClick;
            this.dgHistory.RowDataBound += this.OnHistoryGridItemDataBound;
            this.dgVersions.RowCommand += this.OnVersionsGridItemCommand;
            this.dgVersions.RowDataBound += this.OnVersionsGridItemDataBound;
            this.dgVersions.PageIndexChanged += this.OnVersionsGridPageIndexChanged;
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            try
            {
                var htmlContentItemID = Null.NullInteger;
                var htmlContent = this.htmlTextController.GetTopHtmlText(this.ModuleId, false, this.WorkflowID);
				Logger.LogDebug("OnLoad(e={E}): htmlContentItemID → {HtmlContentItemID}", e, htmlContentItemID);
				Logger.LogDebug("OnLoad(e={E}): htmlContent → {HtmlContent}", e, htmlContent);

                if (htmlContent != null)
                {
                    htmlContentItemID = htmlContent.ItemID;
                }

                if (!this.Page.IsPostBack)
                {
                    var workflow = this.workflowManager.GetWorkflow(this.WorkflowID);
                    var workflowStates = workflow.States.ToList();
					Logger.LogDebug("OnLoad(e={E}): workflow → {Workflow}", e, workflow);
					Logger.LogDebug("OnLoad(e={E}): workflowStates → {WorkflowStates}", e, workflowStates);
                    var maxVersions = this.htmlTextController.GetMaximumVersionHistory(this.PortalId);
                    var userCanEdit = this.UserInfo.IsSuperUser || PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);
					Logger.LogDebug("OnLoad(e={E}): maxVersions → {MaxVersions}", e, maxVersions);
					Logger.LogDebug("OnLoad(e={E}): userCanEdit → {UserCanEdit}", e, userCanEdit);

                    this.lblMaxVersions.Text = maxVersions.ToString();
                    this.dgVersions.PageSize = Math.Min(Math.Max(maxVersions, 5), 10); // min 5, max 10

                    switch (workflow.WorkflowKey)
                    {
                        case SystemWorkflowManager.DirectPublishWorkflowKey:
                            this.CurrentWorkflowType = WorkflowType.DirectPublish;
							Logger.LogDebug("OnLoad(e={E}): this.CurrentWorkflowType → {Arg1}", e, this.CurrentWorkflowType);
                            break;
                        case SystemWorkflowManager.SaveDraftWorkflowKey:
                            this.CurrentWorkflowType = WorkflowType.SaveDraft;
                            break;
                        case SystemWorkflowManager.ContentAprovalWorkflowKey:
                            this.CurrentWorkflowType = WorkflowType.ContentApproval;
                            break;
                        default:
                            this.CurrentWorkflowType = WorkflowType.CustomWorkflow;
                            break;
                    }

                    if (htmlContentItemID != -1)
                    {
                        this.DisplayContent(htmlContent);

                        ////DisplayPreview(htmlContent);
                        this.DisplayHistory(htmlContent);
                    }
                    else
                    {
                        this.DisplayInitialContent(workflowStates[0]);
                    }

                    this.phCurrentVersion.Visible = this.CurrentWorkflowType != WorkflowType.DirectPublish;
                    this.phPreviewVersion.Visible = this.CurrentWorkflowType != WorkflowType.DirectPublish;

                    ////DisplayVersions();
                    this.BindRenderItems();
                    this.ddlRender.SelectedValue = this.txtContent.Mode;
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        protected void OnSaveClick(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnSaveClick(sender={Sender},e={E})", sender, e);
            const bool redirect = true;

            try
            {
                // get content
                var htmlContent = this.GetLatestHTMLContent();

				Logger.LogDebug("OnSaveClick(sender={Sender},e={E}): htmlContent → {HtmlContent}", sender, e, htmlContent);
                var aliases = this.portalAliasService.GetPortalAliasesByPortalId(this.PortalSettings.PortalId).Select(pa => pa.HttpAlias);
                var content = this.phEdit.Visible ? this.txtContent.Text : this.hfEditor.Value;
				Logger.LogDebug("OnSaveClick(sender={Sender},e={E}): aliases → {Aliases}", sender, e, aliases);
				Logger.LogDebug("OnSaveClick(sender={Sender},e={E}): content → {Content}", sender, e, content);

                if (this.Request.QueryString["nuru"] == null)
                {
                    content = HtmlUtils.AbsoluteToRelativeUrls(content, aliases);
                }

                htmlContent.Content = content;
				Logger.LogDebug("OnSaveClick(sender={Sender},e={E}): htmlContent.Content → {Arg2}", sender, e, htmlContent.Content);
                var workflow = this.workflowManager.GetWorkflow(this.WorkflowID);
                var draftStateID = workflow.FirstState.StateID;
				Logger.LogDebug("OnSaveClick(sender={Sender},e={E}): workflow → {Workflow}", sender, e, workflow);
                var publishedStateID = workflow.LastState.StateID;

                switch (this.CurrentWorkflowType)
                {
                    case WorkflowType.DirectPublish:
                        this.htmlTextController.UpdateHtmlText(htmlContent, this.htmlTextController.GetMaximumVersionHistory(this.PortalId));

                        break;
                    default:
                        // if it's already published set it back to draft
                        if (htmlContent.StateID == publishedStateID)
                        {
                            htmlContent.StateID = draftStateID;
							Logger.LogDebug("OnSaveClick(sender={Sender},e={E}): htmlContent.StateID → {Arg2}", sender, e, htmlContent.StateID);
                        }

                        this.htmlTextController.UpdateHtmlText(htmlContent, this.htmlTextController.GetMaximumVersionHistory(this.PortalId));
                        break;
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnSaveClick(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.LogException(exc);
                UI.Skins.Skin.AddModuleMessage(this.Page, "Error occurred: ", exc.Message, ModuleMessage.ModuleMessageType.RedError);
                return;
            }

            // redirect back to portal
            if (redirect)
            {
                this.Response.Redirect(this.navigationManager.NavigateURL(), true);
            }
			Logger.LogDebug("<<< Exiting OnSaveClick(sender={Sender},e={E})", sender, e);
        }

        protected void OnEditClick(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnEditClick(sender={Sender},e={E})", sender, e);
            try
            {
                this.DisplayEdit(this.hfEditor.Value);

                if (this.phMasterContent.Visible)
                {
                    this.DisplayMasterLanguageContent();
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnEditClick(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnEditClick(sender={Sender},e={E})", sender, e);
        }

        protected void OnPreviewClick(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreviewClick(sender={Sender},e={E})", sender, e);
            try
            {
                if (this.phEdit.Visible)
                {
                    this.hfEditor.Value = this.txtContent.Text;
                }

                this.DisplayPreview(this.phEdit.Visible ? this.txtContent.Text : this.hfEditor.Value);
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnPreviewClick(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnPreviewClick(sender={Sender},e={E})", sender, e);
        }

        protected void OnHistoryGridItemDataBound(object sender, GridViewRowEventArgs e)
        {
			Logger.LogDebug(">>> Entering OnHistoryGridItemDataBound(sender={Sender},e={E})", sender, e);
            var item = e.Row;

            if (item.RowType == DataControlRowType.DataRow)
            {
                // Localize columns
                item.Cells[2].Text = Localization.GetString(item.Cells[2].Text, this.LocalResourceFile);
                item.Cells[3].Text = Localization.GetString(item.Cells[3].Text, this.LocalResourceFile);
            }
			Logger.LogDebug("<<< Exiting OnHistoryGridItemDataBound(sender={Sender},e={E})", sender, e);
        }

        protected void OnVersionsGridItemCommand(object source, GridViewCommandEventArgs e)
        {
			Logger.LogDebug(">>> Entering OnVersionsGridItemCommand(source={Source},e={E})", source, e);
            try
            {
                HtmlTextInfo htmlContent;

                // disable delete button if user doesn't have delete rights???
                switch (e.CommandName.ToLowerInvariant())
                {
                    case "remove":
                        htmlContent = this.GetHTMLContent(e);
						Logger.LogDebug("OnVersionsGridItemCommand(source={Source},e={E}): htmlContent → {HtmlContent}", source, e, htmlContent);
                        this.htmlTextController.DeleteHtmlText(this.ModuleId, htmlContent.ItemID);
                        break;
                    case "rollback":
                        htmlContent = this.GetHTMLContent(e);
						Logger.LogDebug("OnVersionsGridItemCommand(source={Source},e={E}): htmlContent.ItemID → {Arg2}", source, e, htmlContent.ItemID);
						Logger.LogDebug("OnVersionsGridItemCommand(source={Source},e={E}): htmlContent.ModuleID → {Arg2}", source, e, htmlContent.ModuleID);
						Logger.LogDebug("OnVersionsGridItemCommand(source={Source},e={E}): htmlContent.WorkflowID → {Arg2}", source, e, htmlContent.WorkflowID);
                        htmlContent.ItemID = -1;
                        htmlContent.ModuleID = this.ModuleId;
                        htmlContent.WorkflowID = this.WorkflowID;
                        htmlContent.StateID = this.workflowManager.GetWorkflow(this.WorkflowID).FirstState.StateID;
                        this.htmlTextController.UpdateHtmlText(htmlContent, this.htmlTextController.GetMaximumVersionHistory(this.PortalId));
                        break;
                    case "preview":
                        htmlContent = this.GetHTMLContent(e);
                        this.DisplayPreview(htmlContent);
                        break;
                }

                if (e.CommandName.ToLowerInvariant() != "preview")
                {
                    var latestContent = this.htmlTextController.GetTopHtmlText(this.ModuleId, false, this.WorkflowID);
                    if (latestContent == null)
					Logger.LogDebug("OnVersionsGridItemCommand(source={Source},e={E}): latestContent → {LatestContent}", source, e, latestContent);
                    {
                        this.DisplayInitialContent(this.workflowManager.GetWorkflow(this.WorkflowID).FirstState);
                    }
                    else
                    {
                        this.DisplayContent(latestContent);

                        ////DisplayPreview(latestContent);
                        ////DisplayVersions();
                    }
                }

                // Module failed to load
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnVersionsGridItemCommand(source={Source},e={E}): {Arg2}", source, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnVersionsGridItemCommand(source={Source},e={E})", source, e);
        }

        protected void OnVersionsGridItemDataBound(object sender, GridViewRowEventArgs e)
        {
			Logger.LogDebug(">>> Entering OnVersionsGridItemDataBound(sender={Sender},e={E})", sender, e);
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var htmlContent = e.Row.DataItem as HtmlTextInfo;
                var createdBy = "Default";

				Logger.LogDebug("OnVersionsGridItemDataBound(sender={Sender},e={E}): createdBy → {CreatedBy}", sender, e, createdBy);
                if (htmlContent.CreatedByUserID != -1)
                {
                    var createdByByUser = UserController.GetUserById(this.hostSettings, this.PortalId, htmlContent.CreatedByUserID);
                    if (createdByByUser != null)
					Logger.LogDebug("OnVersionsGridItemDataBound(sender={Sender},e={E}): createdByByUser → {CreatedByByUser}", sender, e, createdByByUser);
                    {
                        createdBy = createdByByUser.DisplayName;
                    }
                }

                foreach (TableCell cell in e.Row.Cells)
                {
                    foreach (Control cellControl in cell.Controls)
                    {
                        if (cellControl is ImageButton)
                        {
                            var imageButton = cellControl as ImageButton;
                            imageButton.CommandArgument = htmlContent.ItemID.ToString();
                            switch (imageButton.CommandName.ToLowerInvariant())
                            {
                                case "rollback":
                                    // hide rollback for the first item
                                    if (this.dgVersions.CurrentPageIndex == 0)
                                    {
                                        if (e.Row.RowIndex == 0)
                                        {
                                            imageButton.Visible = false;
											Logger.LogDebug("OnVersionsGridItemDataBound(sender={Sender},e={E}): imageButton.Visible → {Arg2}", sender, e, imageButton.Visible);
                                            break;
                                        }
                                    }

                                    imageButton.Visible = true;

                                    break;
                                case "remove":
                                    var msg = this.GetLocalizedString("DeleteVersion.Confirm");
                                    msg =
                                        msg.Replace("[VERSION]", htmlContent.Version.ToString()).Replace("[STATE]", htmlContent.StateName).Replace("[DATECREATED]", htmlContent.CreatedOnDate.ToString())
                                            .Replace("[USERNAME]", createdBy);
										Logger.LogDebug("OnVersionsGridItemDataBound(sender={Sender},e={E}): msg → {Msg}", sender, e, msg);
                                    imageButton.OnClientClick = "return confirm(\"" + msg + "\");";

                                    // hide the delete button
                                    var showDelete = this.UserInfo.IsSuperUser || PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);

									Logger.LogDebug("OnVersionsGridItemDataBound(sender={Sender},e={E}): showDelete → {ShowDelete}", sender, e, showDelete);
                                    if (!showDelete)
                                    {
                                        showDelete = htmlContent.IsPublished == false;
                                    }

                                    imageButton.Visible = showDelete;
                                    break;
                            }
                        }
                    }
                }
            }
			Logger.LogDebug("<<< Exiting OnVersionsGridItemDataBound(sender={Sender},e={E})", sender, e);
        }

        protected void OnVersionsGridPageIndexChanged(object source, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnVersionsGridPageIndexChanged(source={Source},e={E})", source, e);
            this.DisplayVersions();
			Logger.LogDebug("<<< Exiting OnVersionsGridPageIndexChanged(source={Source},e={E})", source, e);
        }

        /// <summary>Displays the history of an html content item in a grid in the preview section.</summary>
        /// <param name="htmlContent">Content of the HTML.</param>
        private void DisplayHistory(HtmlTextInfo htmlContent)
        {
            this.dnnSitePanelEditHTMLHistory.Visible = this.CurrentWorkflowType != WorkflowType.DirectPublish;
            this.fsEditHtmlHistory.Visible = this.CurrentWorkflowType != WorkflowType.DirectPublish;

            if (this.CurrentWorkflowType == WorkflowType.DirectPublish)
            {
                return;
            }

            var htmlLogging = this.htmlTextLogController.GetHtmlTextLog(htmlContent.ItemID);
            this.dgHistory.DataSource = htmlLogging;
            this.dgHistory.DataBind();

            this.dnnSitePanelEditHTMLHistory.Visible = htmlLogging.Count != 0;
            this.fsEditHtmlHistory.Visible = htmlLogging.Count != 0;
			Logger.LogDebug("<<< Exiting DisplayHistory(htmlContent={HtmlContent})", htmlContent);
        }

        /// <summary>Displays the versions of the html content in the versions section.</summary>
        private void DisplayVersions()
        {
			Logger.LogDebug(">>> Entering DisplayVersions()");
            var versions = this.htmlTextController.GetAllHtmlText(this.ModuleId);

			Logger.LogDebug("DisplayVersions(): versions → {Versions}", versions);
            foreach (var item in versions)
            {
                item.StateName = this.GetLocalizedString(item.StateName);
            }

            this.dgVersions.DataSource = versions;
            this.dgVersions.DataBind();

            this.phEdit.Visible = false;
            this.phPreview.Visible = false;
            this.phHistory.Visible = true;
            this.cmdEdit.Enabled = true;
            this.cmdPreview.Enabled = true;
            this.cmdHistory.Enabled = false;
            this.cmdMasterContent.Visible = false;
            this.ddlRender.Visible = false;
			Logger.LogDebug("<<< Exiting DisplayVersions()");
        }

        /// <summary>Displays the content of the master language if localized content is enabled.</summary>
        private void DisplayMasterLanguageContent()
        {
			Logger.LogDebug(">>> Entering DisplayMasterLanguageContent()");
            // Get master language
            var objModule = ModuleController.Instance.GetModule(this.ModuleId, this.TabId, false);
            if (objModule.DefaultLanguageModule != null)
			Logger.LogDebug("DisplayMasterLanguageContent(): objModule → {ObjModule}", objModule);
            {
                var masterContent = this.htmlTextController.GetTopHtmlText(objModule.DefaultLanguageModule.ModuleID, false, this.WorkflowID);
                if (masterContent != null)
				Logger.LogDebug("DisplayMasterLanguageContent(): masterContent → {MasterContent}", masterContent);
                {
                    this.placeMasterContent.Controls.Add(new LiteralControl(HtmlTextController.FormatHtmlText(objModule.DefaultLanguageModule.ModuleID, this.FormatContent(masterContent.Content), this.Settings, this.PortalSettings, this.Page)));
                }
            }
			Logger.LogDebug("<<< Exiting DisplayMasterLanguageContent()");
        }

        /// <summary>Displays the html content in the preview section.</summary>
        /// <param name="htmlContent">Content of the HTML.</param>
        private void DisplayContent(HtmlTextInfo htmlContent)
        {
            this.lblCurrentWorkflowInUse.Text = this.GetLocalizedString(htmlContent.WorkflowName);
            this.lblCurrentWorkflowState.Text = this.GetLocalizedString(htmlContent.StateName);
            this.lblCurrentVersion.Text = htmlContent.Version.ToString();
            this.txtContent.Text = this.FormatContent(htmlContent.Content);
            this.phEdit.Visible = true;
            this.phPreview.Visible = false;
            this.phHistory.Visible = false;
            this.cmdEdit.Enabled = false;
            this.cmdPreview.Enabled = true;
            this.cmdHistory.Enabled = true;

            ////DisplayMasterLanguageContent();
            this.DisplayMasterContentButton();
            this.ddlRender.Visible = true;
        }

        private void DisplayMasterContentButton()
        {
			Logger.LogDebug(">>> Entering DisplayMasterContentButton()");
            var objModule = ModuleController.Instance.GetModule(this.ModuleId, this.TabId, false);
            if (objModule.DefaultLanguageModule != null)
			Logger.LogDebug("DisplayMasterContentButton(): objModule → {ObjModule}", objModule);
            {
                this.cmdMasterContent.Visible = true;
                this.cmdMasterContent.Text = Localization.GetString("cmdShowMasterContent", this.LocalResourceFile);

                this.cmdMasterContent.Text = this.phMasterContent.Visible ?
                    Localization.GetString("cmdHideMasterContent", this.LocalResourceFile) :
                    Localization.GetString("cmdShowMasterContent", this.LocalResourceFile);
            }
			Logger.LogDebug("<<< Exiting DisplayMasterContentButton()");
        }

        /// <summary>Displays the content preview in the preview section.</summary>
        /// <param name="htmlContent">Content of the HTML.</param>
        private void DisplayPreview(HtmlTextInfo htmlContent)
        {
            this.lblPreviewVersion.Text = htmlContent.Version.ToString();
            this.lblPreviewWorkflowInUse.Text = this.GetLocalizedString(htmlContent.WorkflowName);
            this.lblPreviewWorkflowState.Text = this.GetLocalizedString(htmlContent.StateName);
            this.litPreview.Text = HtmlTextController.FormatHtmlText(this.ModuleId, htmlContent.Content, this.Settings, this.PortalSettings, this.Page);
            this.phEdit.Visible = false;
            this.phPreview.Visible = true;
            this.phHistory.Visible = false;
            this.cmdEdit.Enabled = true;
            this.cmdPreview.Enabled = false;
            this.cmdHistory.Enabled = true;
            this.DisplayHistory(htmlContent);
            this.cmdMasterContent.Visible = false;
            this.ddlRender.Visible = false;
        }

        /// <summary>Displays the preview in the preview section.</summary>
        /// <param name="htmlContent">Content of the HTML.</param>
        private void DisplayPreview(string htmlContent)
        {
            this.litPreview.Text = HtmlTextController.FormatHtmlText(this.ModuleId, htmlContent, this.Settings, this.PortalSettings, this.Page);
            this.divPreviewVersion.Visible = false;
            this.divPreviewWorlflow.Visible = false;

            this.divPreviewWorkflowState.Visible = true;
            this.lblPreviewWorkflowState.Text = this.GetLocalizedString("EditPreviewState");

            this.phEdit.Visible = false;
            this.phPreview.Visible = true;
            this.phHistory.Visible = false;
            this.cmdEdit.Enabled = true;
            this.cmdPreview.Enabled = false;
            this.cmdHistory.Enabled = true;
            this.cmdMasterContent.Visible = false;
            this.ddlRender.Visible = false;
        }

        private void DisplayEdit(string htmlContent)
        {
            this.txtContent.Text = htmlContent;
            this.phEdit.Visible = true;
            this.phPreview.Visible = false;
            this.phHistory.Visible = false;
            this.cmdEdit.Enabled = false;
            this.cmdPreview.Enabled = true;
            this.cmdHistory.Enabled = true;
            this.DisplayMasterContentButton();
            this.ddlRender.Visible = true;
        }

        /// <summary>Displays the content but hide the editor if editing is locked from the current user.</summary>
        /// <param name="htmlContent">Content of the HTML.</param>
        /// <param name="lastPublishedContent">Last content of the published.</param>
        private void DisplayLockedContent(HtmlTextInfo htmlContent, HtmlTextInfo lastPublishedContent)
        {
            this.txtContent.Visible = false;
            this.cmdSave.Visible = false;

            ////cmdPreview.Enabled = false;
            this.divSubmittedContent.Visible = true;

            this.lblCurrentWorkflowInUse.Text = this.GetLocalizedString(htmlContent.WorkflowName);
            this.lblCurrentWorkflowState.Text = this.GetLocalizedString(htmlContent.StateName);

            this.litCurrentContentPreview.Text = HtmlTextController.FormatHtmlText(this.ModuleId, htmlContent.Content, this.Settings, this.PortalSettings, this.Page);
            this.lblCurrentVersion.Text = htmlContent.Version.ToString();
            this.DisplayVersions();

            if (lastPublishedContent != null)
            {
                this.DisplayPreview(lastPublishedContent);

                ////DisplayHistory(lastPublishedContent);
            }
            else
            {
                this.dnnSitePanelEditHTMLHistory.Visible = false;
                this.fsEditHtmlHistory.Visible = false;
                this.DisplayPreview(htmlContent.Content);
            }
        }

        /// <summary>Displays the initial content when a module is first added to the page.</summary>
        /// <param name="firstState">The first state.</param>
        private void DisplayInitialContent(WorkflowState firstState)
        {
			Logger.LogDebug(">>> Entering DisplayInitialContent(firstState={FirstState})", firstState);
            this.cmdHistory.Enabled = false;

            this.txtContent.Text = this.GetLocalizedString("AddContent");
            this.litPreview.Text = this.GetLocalizedString("AddContent");
            this.lblCurrentWorkflowInUse.Text = this.workflowManager.GetWorkflow(firstState.WorkflowID).WorkflowName;
            this.lblPreviewWorkflowInUse.Text = this.workflowManager.GetWorkflow(firstState.WorkflowID).WorkflowName;
            this.divPreviewVersion.Visible = false;

            this.dnnSitePanelEditHTMLHistory.Visible = false;
            this.fsEditHtmlHistory.Visible = false;

            this.divCurrentWorkflowState.Visible = false;
            this.phCurrentVersion.Visible = false;
            this.divPreviewWorkflowState.Visible = false;

            this.lblPreviewWorkflowState.Text = firstState.StateName;
			Logger.LogDebug("<<< Exiting DisplayInitialContent(firstState={FirstState})", firstState);
        }

        /// <summary>Formats the content to make it HTML safe.</summary>
        /// <param name="htmlContent">Content of the HTML.</param>
        /// <returns>The <paramref name="htmlContent"/> with relative paths updated and encoding issues resolved.</returns>
        private string FormatContent(string htmlContent)
        {
            var strContent = HttpUtility.HtmlDecode(htmlContent);
            strContent = HtmlTextController.ManageRelativePaths(strContent, this.PortalSettings.HomeDirectory, "src");
            strContent = HtmlTextController.ManageRelativePaths(strContent, this.PortalSettings.HomeDirectory, "background");
            return HttpUtility.HtmlEncode(strContent);
        }

        /// <summary>Gets the localized string from a resource file if it exists.</summary>
        /// <param name="str">The resource key.</param>
        /// <returns>The localized text or <paramref name="str"/>.</returns>
        private string GetLocalizedString(string str)
        {
			Logger.LogDebug(">>> Entering GetLocalizedString(str={Str})", str);
            var localizedString = Localization.GetString(str, this.LocalResourceFile);
			Logger.LogDebug("GetLocalizedString(str={Str}): localizedString → {LocalizedString}", str, localizedString);
			Logger.LogDebug("<<< Exiting GetLocalizedString(str={Str})", str);
            return string.IsNullOrEmpty(localizedString) ? str : localizedString;
        }

        /// <summary>Gets the latest html content of the module.</summary>
        /// <returns>An <see cref="HtmlTextInfo"/> instance (with <see cref="HtmlTextInfo.ItemID"/> set to <c>-1</c> if there's no existing content).</returns>
        private HtmlTextInfo GetLatestHTMLContent()
        {
			Logger.LogDebug(">>> Entering GetLatestHTMLContent()");
            var htmlContent = this.htmlTextController.GetTopHtmlText(this.ModuleId, false, this.WorkflowID);
            if (htmlContent == null)
			Logger.LogDebug("GetLatestHTMLContent(): htmlContent → {HtmlContent}", htmlContent);
            {
                htmlContent = new HtmlTextInfo();
                htmlContent.ItemID = -1;
				Logger.LogDebug("GetLatestHTMLContent(): htmlContent.ItemID → {Arg0}", htmlContent.ItemID);
				Logger.LogDebug("GetLatestHTMLContent(): htmlContent.WorkflowID → {Arg0}", htmlContent.WorkflowID);
				Logger.LogDebug("GetLatestHTMLContent(): htmlContent.ModuleID → {Arg0}", htmlContent.ModuleID);
                htmlContent.StateID = this.workflowManager.GetWorkflow(this.WorkflowID).FirstState.StateID;
                htmlContent.WorkflowID = this.WorkflowID;
                htmlContent.ModuleID = this.ModuleId;
            }

			Logger.LogDebug("<<< Exiting GetLatestHTMLContent()");
            return htmlContent;
        }

        /// <summary>Gets the last published version of this module.</summary>
        /// <param name="publishedStateID">The published state ID.</param>
        /// <returns>An <see cref="HtmlTextInfo"/> instance.</returns>
        private HtmlTextInfo GetLastPublishedVersion(int publishedStateID)
        {
            return (from version in this.htmlTextController.GetAllHtmlText(this.ModuleId) where version.StateID == publishedStateID orderby version.Version descending select version).ToList()[0];
        }

        private void OnRenderSelectedIndexChanged(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnRenderSelectedIndexChanged(sender={Sender},e={E})", sender, e);
            this.txtContent.ChangeMode(this.ddlRender.SelectedValue);
			Logger.LogDebug("<<< Exiting OnRenderSelectedIndexChanged(sender={Sender},e={E})", sender, e);
        }

        private void OnHistoryClick(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnHistoryClick(sender={Sender},e={E})", sender, e);
            try
            {
                if (this.phEdit.Visible)
                {
                    this.hfEditor.Value = this.txtContent.Text;
                }

                this.DisplayVersions();
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnHistoryClick(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnHistoryClick(sender={Sender},e={E})", sender, e);
        }

        private void OnMasterContentClick(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnMasterContentClick(sender={Sender},e={E})", sender, e);
            try
            {
                this.phMasterContent.Visible = !this.phMasterContent.Visible;
                this.cmdMasterContent.Text = this.phMasterContent.Visible ?
                    Localization.GetString("cmdHideMasterContent", this.LocalResourceFile) :
                    Localization.GetString("cmdShowMasterContent", this.LocalResourceFile);

                if (this.phMasterContent.Visible)
                {
                    this.DisplayMasterLanguageContent();
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnMasterContentClick(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnMasterContentClick(sender={Sender},e={E})", sender, e);
        }

        private HtmlTextInfo GetHTMLContent(GridViewCommandEventArgs e)
        {
			Logger.LogDebug(">>> Entering GetHTMLContent(e={E})", e);
			Logger.LogDebug("<<< Exiting GetHTMLContent(e={E})", e);
            return this.htmlTextController.GetHtmlText(this.ModuleId, int.Parse(e.CommandArgument.ToString()));
        }

        private void BindRenderItems()
        {
			Logger.LogDebug(">>> Entering BindRenderItems()");
            if (this.txtContent.IsRichEditorAvailable)
            {
                this.ddlRender.Items.Add(new ListItem(this.LocalizeText("liRichText"), "RICH"));
            }

            this.ddlRender.Items.Add(new ListItem(this.LocalizeText("liBasicText"), "BASIC"));
			Logger.LogDebug("<<< Exiting BindRenderItems()");
        }
    }
}
