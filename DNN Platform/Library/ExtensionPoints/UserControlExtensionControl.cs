// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Web.UI;

    [DefaultProperty("Text")]
    [ToolboxData("<{0}:UserControlExtensionControl runat=server></{0}:UserControlExtensionControl>")]
    public class UserControlExtensionControl : DefaultExtensionControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserControlExtensionControl));
        public void BindAction(int portalId, int tabId, int moduleId)
        {
			Logger.LogDebug(">>> Entering BindAction(portalId={PortalId},tabId={TabId},moduleId={ModuleId})", portalId, tabId, moduleId);
            foreach (var control in this.Controls)
            {
                var actionsControl = control as IUserControlActions;
                if (actionsControl != null)
                {
                    actionsControl.BindAction(portalId, tabId, moduleId);
                }
            }
			Logger.LogDebug("<<< Exiting BindAction(portalId={PortalId},tabId={TabId},moduleId={ModuleId})", portalId, tabId, moduleId);
        }

        public void SaveAction(int portalId, int tabId, int moduleId)
        {
			Logger.LogDebug(">>> Entering SaveAction(portalId={PortalId},tabId={TabId},moduleId={ModuleId})", portalId, tabId, moduleId);
            foreach (var control in this.Controls)
            {
                var actionsControl = control as IUserControlActions;
                if (actionsControl != null)
                {
                    actionsControl.SaveAction(portalId, tabId, moduleId);
                }
            }
			Logger.LogDebug("<<< Exiting SaveAction(portalId={PortalId},tabId={TabId},moduleId={ModuleId})", portalId, tabId, moduleId);
        }

        public void CancelAction(int portalId, int tabId, int moduleId)
        {
			Logger.LogDebug(">>> Entering CancelAction(portalId={PortalId},tabId={TabId},moduleId={ModuleId})", portalId, tabId, moduleId);
            foreach (var control in this.Controls)
            {
                var actionsControl = control as IUserControlActions;
                if (actionsControl != null)
                {
                    actionsControl.CancelAction(portalId, tabId, moduleId);
                }
            }
			Logger.LogDebug("<<< Exiting CancelAction(portalId={PortalId},tabId={TabId},moduleId={ModuleId})", portalId, tabId, moduleId);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);
            var extensionPointManager = new ExtensionPointManager();
			Logger.LogDebug("OnInit(e={E}): extensionPointManager → {ExtensionPointManager}", e, extensionPointManager);

            if (!string.IsNullOrEmpty(this.Name))
            {
                var extension = extensionPointManager.GetUserControlExtensionPointFirstByPriority(this.Module, this.Name);
				Logger.LogDebug("OnInit(e={E}): extension → {Extension}", e, extension);
                this.LoadControl(extension);
            }
            else
            {
                foreach (var extension in extensionPointManager.GetUserControlExtensionPoints(this.Module, this.Group))
                {
                    this.LoadControl(extension);
                }
            }
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        private void LoadControl(IUserControlExtensionPoint extension)
        {
            var control = this.Page.LoadControl(extension.UserControlSrc);
            control.ID = Path.GetFileNameWithoutExtension(extension.UserControlSrc);
            this.Controls.Add(control);
			Logger.LogDebug("<<< Exiting LoadControl(extension={Extension})", extension);
        }
    }
}
