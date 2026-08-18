// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ExtensionPoints
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    [DefaultProperty("Module")]
    [ToolboxData("<{0}:EditPageTabExtensionControl runat=server></{0}:EditPageTabExtensionControl>")]
    public class EditPageTabExtensionControl : DefaultExtensionControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EditPageTabExtensionControl));
        [Bindable(true)]
        [DefaultValue("")]
        public string TabControlId
        {
            get
            {
                var s = (string)this.ViewState["TabControlId"];
                return s ?? string.Empty;
            }

            set
            {
                this.ViewState["TabControlId"] = value;
            }
        }

        [Bindable(true)]
        [DefaultValue("")]
        public string PanelControlId
        {
            get
            {
                var s = (string)this.ViewState["PanelControlId"];
                return s ?? string.Empty;
            }

            set
            {
                this.ViewState["PanelControlId"] = value;
            }
        }

        public void BindAction(int portalId, int tabId, int moduleId)
        {
			Logger.LogDebug(">>> Entering BindAction(portalId={PortalId},tabId={TabId},moduleId={ModuleId})", portalId, tabId, moduleId);
            var panel = this.Parent.FindControl(this.PanelControlId);

			Logger.LogDebug("BindAction(portalId={PortalId},tabId={TabId},moduleId={ModuleId}): panel → {Panel}", portalId, tabId, moduleId, panel);
            foreach (var control in panel.Controls)
            {
                var panelcontrol = control as PanelTabExtensionControl;
                if (panelcontrol != null)
                {
                    foreach (var extensionControl in panelcontrol.Controls)
                    {
                        var actionsControl = extensionControl as IEditPageTabControlActions;
                        if (actionsControl != null)
                        {
                            actionsControl.BindAction(portalId, tabId, moduleId);
                        }
                    }
                }
            }
			Logger.LogDebug("<<< Exiting BindAction(portalId={PortalId},tabId={TabId},moduleId={ModuleId})", portalId, tabId, moduleId);
        }

        public void SaveAction(int portalId, int tabId, int moduleId)
        {
			Logger.LogDebug(">>> Entering SaveAction(portalId={PortalId},tabId={TabId},moduleId={ModuleId})", portalId, tabId, moduleId);
            var panel = this.Parent.FindControl(this.PanelControlId);

			Logger.LogDebug("SaveAction(portalId={PortalId},tabId={TabId},moduleId={ModuleId}): panel → {Panel}", portalId, tabId, moduleId, panel);
            foreach (var control in panel.Controls)
            {
                var panelcontrol = control as PanelTabExtensionControl;
                if (panelcontrol != null)
                {
                    foreach (var extensionControl in panelcontrol.Controls)
                    {
                        var actionsControl = extensionControl as IEditPageTabControlActions;
                        if (actionsControl != null)
                        {
                            actionsControl.SaveAction(portalId, tabId, moduleId);
                        }
                    }
                }
            }
			Logger.LogDebug("<<< Exiting SaveAction(portalId={PortalId},tabId={TabId},moduleId={ModuleId})", portalId, tabId, moduleId);
        }

        public void CancelAction(int portalId, int tabId, int moduleId)
        {
			Logger.LogDebug(">>> Entering CancelAction(portalId={PortalId},tabId={TabId},moduleId={ModuleId})", portalId, tabId, moduleId);
            var panel = this.Parent.FindControl(this.PanelControlId);

			Logger.LogDebug("CancelAction(portalId={PortalId},tabId={TabId},moduleId={ModuleId}): panel → {Panel}", portalId, tabId, moduleId, panel);
            foreach (var control in panel.Controls)
            {
                var panelcontrol = control as PanelTabExtensionControl;
                if (panelcontrol != null)
                {
                    foreach (var extensionControl in panelcontrol.Controls)
                    {
                        var actionsControl = extensionControl as IEditPageTabControlActions;
                        if (actionsControl != null)
                        {
                            actionsControl.CancelAction(portalId, tabId, moduleId);
                        }
                    }
                }
            }
			Logger.LogDebug("<<< Exiting CancelAction(portalId={PortalId},tabId={TabId},moduleId={ModuleId})", portalId, tabId, moduleId);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            var extensionPointManager = new ExtensionPointManager();

			Logger.LogDebug("OnInit(e={E}): extensionPointManager → {ExtensionPointManager}", e, extensionPointManager);
            var tabs = (HtmlGenericControl)this.Parent.FindControl(this.TabControlId);
			Logger.LogDebug("OnInit(e={E}): tabs → {Tabs}", e, tabs);
            var panel = this.Parent.FindControl(this.PanelControlId);
			Logger.LogDebug("OnInit(e={E}): panel → {Panel}", e, panel);

            foreach (var extension in extensionPointManager.GetEditPageTabExtensionPoints(this.Module, this.Group))
            {
                if (extension.Visible)
                {
                    var liElement = new HtmlGenericControl("li")
                    {
                        InnerHtml = "<a href=\"#" + extension.EditPageTabId + "\">" + extension.Text + "</a>",
                    };
                    liElement.Attributes.Add("class", extension.CssClass);
                    tabs.Controls.Add(liElement);

                    var container = new PanelTabExtensionControl { PanelId = extension.EditPageTabId };
                    var control = this.Page.LoadControl(extension.UserControlSrc);
					Logger.LogDebug("OnInit(e={E}): control → {Control}", e, control);
                    control.ID = Path.GetFileNameWithoutExtension(extension.UserControlSrc);
                    container.Controls.Add(control);
                    panel.Controls.Add(container);
                }
            }
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }
    }
}
