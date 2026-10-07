// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.HtmlEditorManager.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.UI;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Modules.HtmlEditorManager.ViewModels;
    using DotNetNuke.Modules.HtmlEditorManager.Views;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.Mvp;
    using DotNetNuke.Web.UI;
    using DotNetNuke.Web.UI.WebControls;
	using Microsoft.Extensions.Logging;

    /// <summary>Presenter for Provider Configuration.</summary>
    [DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
    public partial class ProviderConfigurationPresenter : ModulePresenter<IProviderConfigurationView, ProviderConfigurationViewModel>
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>The HTML editor node.</summary>
        private const string HtmlEditorNode = "/configuration/dotnetnuke/htmlEditor";

        /// <summary>The dot net nuke document.</summary>
        private XmlDocument dotnetNukeDocument;

        /// <summary>Initializes a new instance of the <see cref="ProviderConfigurationPresenter" /> class.</summary>
        /// <param name="view">the interface provider view.</param>
        public ProviderConfigurationPresenter(IProviderConfigurationView view)
            : base(view)
        {
			_logger.LogError(">>> Entering ProviderConfigurationPresenter(view={View})", view);
            this.View.Initialize += this.View_Initialize;
            this.View.SaveEditorChoice += this.View_SaveEditorChoice;
            this.View.EditorChanged += this.View_EditorChanged;
			_logger.LogWarning("<<< Exiting ProviderConfigurationPresenter(view={View})", view);
        }

        /// <summary>Gets the DNN configuration.</summary>
        /// <value>The DNN configuration.</value>
        protected XmlDocument DNNConfiguration
        {
            get
            {
                if (this.dotnetNukeDocument == null)
                {
                    UserInfo currentUser = UserController.Instance.GetCurrentUserInfo();
                    if (currentUser != null && currentUser.IsSuperUser)
                    {
                        this.dotnetNukeDocument = Config.Load();
                    }
                }

                return this.dotnetNukeDocument;
            }
        }

        /// <summary>Handles the Initialize event of the View control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void View_Initialize(object sender, EventArgs e)
        {
			_logger.LogError(">>> Entering View_Initialize(sender={Sender},e={E})", sender, e);
            this.View.Model.Editors = this.GetAvailableEditors();
            this.View.Model.SelectedEditor = this.GetSelectedEditor();
            this.View.Editor.Controls.Add(this.LoadCurrentEditor());

            // hack: force DNN to load the Telerik Combobox skin. Needed for the RadEditor provider
            // TODO: Remove as the included file path isn't distributed with installs
            ClientResourceManager.RegisterStyleSheet(this.View.Editor.Page, "~/Portals/_default/Skins/_default/WebControlSkin/Default/ComboBox.Default.css");
			_logger.LogWarning("<<< Exiting View_Initialize(sender={Sender},e={E})", sender, e);
        }

        /// <summary>Loads the current editor.</summary>
        /// <param name="editorName">Name of the editor.</param>
        /// <returns>The editor based on the current editor settings in the web configuration.</returns>
        private Control LoadCurrentEditor(string editorName)
        {
			_logger.LogError(">>> Entering LoadCurrentEditor(editorName={EditorName})", editorName);
            XmlDocument dnnConfiguration = this.DNNConfiguration;
            XmlNode htmlProviderNode = this.GetHtmlEditorProviderNode(dnnConfiguration);
			_logger.LogError("LoadCurrentEditor(editorName={EditorName}): htmlProviderNode → {HtmlProviderNode}", editorName, htmlProviderNode);
            XmlNode currentProvider = htmlProviderNode.SelectSingleNode("providers/add[@name='" + editorName + "']");
			_logger.LogError("LoadCurrentEditor(editorName={EditorName}): currentProvider → {CurrentProvider}", editorName, currentProvider);

            var settingsAttribute = currentProvider.Attributes["settingsControlPath"];
			_logger.LogError("LoadCurrentEditor(editorName={EditorName}): settingsAttribute → {SettingsAttribute}", editorName, settingsAttribute);
            if (settingsAttribute != null)
            {
                try
                {
                    var controlPath = settingsAttribute.Value;
                    var control = (PortalModuleBase)((TemplateControl)this.View).LoadControl(controlPath);
					_logger.LogError("LoadCurrentEditor(editorName={EditorName}): control → {Control}", editorName, control);
                    control.ModuleConfiguration = this.ModuleContext.Configuration;
					_logger.LogDebug("LoadCurrentEditor(editorName={EditorName}): control.ModuleConfiguration → {Arg1}", editorName, control.ModuleConfiguration);
                    control.ID = Path.GetFileNameWithoutExtension(controlPath);
                    this.View.Model.CanSave = true;
                    return control;
                }
                catch (Exception)
                {
					_logger.LogError("Exception in LoadCurrentEditor(editorName={EditorName}): {Arg1}", editorName, Exception.Message);
                    this.View.Model.CanSave = false;
                }
            }

            // Display a nice message to the user that this provider is not supported.
			_logger.LogWarning("<<< Exiting LoadCurrentEditor(editorName={EditorName})", editorName);
            return ((TemplateControl)this.View).LoadControl("~/DesktopModules/Admin/HtmlEditorManager/Controls/InvalidConfiguration.ascx");
        }

        /// <summary>Loads the current editor.</summary>
        /// <returns>The editor based on the current editor settings in the web configuration.</returns>
        private Control LoadCurrentEditor()
        {
            return this.LoadCurrentEditor(this.GetSelectedEditor());
        }

        /// <summary>Handles the Editor was changed event of the View control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EditorEventArgs"/> instance containing the event data.</param>
        private void View_EditorChanged(object sender, EditorEventArgs e)
        {
			_logger.LogError(">>> Entering View_EditorChanged(sender={Sender},e={E})", sender, e);
            this.View.Editor.Controls.Clear();
            this.View.Model.SelectedEditor = e.Editor;
            this.View.Model.CanSave = true;
			_logger.LogWarning("<<< Exiting View_EditorChanged(sender={Sender},e={E})", sender, e);
        }

        /// <summary>Handles the SaveEditorChoice event of the View control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EditorEventArgs"/> instance containing the event data.</param>
        private void View_SaveEditorChoice(object sender, EditorEventArgs e)
        {
			_logger.LogError(">>> Entering View_SaveEditorChoice(sender={Sender},e={E})", sender, e);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.SaveEditorInConfiguration(e.Editor);
            this.View.Refresh();
			_logger.LogError("View_SaveEditorChoice(sender,e)={Sender},{E}: database query executed in {Arg2} ms", sender, e, __logStart.ElapsedMilliseconds);
            this.View.Editor.Controls.Clear();
            this.View.Editor.Controls.Add(this.LoadCurrentEditor());
            this.View.Model.SelectedEditor = e.Editor;
			_logger.LogWarning("<<< Exiting View_SaveEditorChoice(sender={Sender},e={E})", sender, e);
        }

        /// <summary>Saves the editor in configuration.</summary>
        /// <param name="name">The name.</param>
        private void SaveEditorInConfiguration(string name)
        {
			_logger.LogError(">>> Entering SaveEditorInConfiguration(name={Name})", name);
            XmlDocument dnnConfiguration = this.DNNConfiguration;
            XmlNode providerNode = this.GetHtmlEditorProviderNode(dnnConfiguration);
			_logger.LogError("SaveEditorInConfiguration(name={Name}): providerNode → {ProviderNode}", name, providerNode);

            if (providerNode != null)
            {
                providerNode.Attributes["defaultProvider"].Value = name;
                Config.Save(dnnConfiguration);
            }
			_logger.LogWarning("<<< Exiting SaveEditorInConfiguration(name={Name})", name);
        }

        /// <summary>Gets the HTML provider node.</summary>
        /// <param name="dnnConfiguration">The DNN configuration.</param>
        /// <returns>The XmlNode for the htmlEditor provider.</returns>
        private XmlNode GetHtmlEditorProviderNode(XmlDocument dnnConfiguration)
        {
			_logger.LogError(">>> Entering GetHtmlEditorProviderNode(dnnConfiguration={DnnConfiguration})", dnnConfiguration);
            if (dnnConfiguration != null && dnnConfiguration.DocumentElement != null)
            {
                return dnnConfiguration.DocumentElement.SelectSingleNode(HtmlEditorNode);
            }

			_logger.LogWarning("<<< Exiting GetHtmlEditorProviderNode(dnnConfiguration={DnnConfiguration})", dnnConfiguration);
            return null;
        }

        /// <summary>Gets the provider list.</summary>
        /// <returns>A list of the installed providers.</returns>
        private List<string> GetAvailableEditors()
        {
			_logger.LogError(">>> Entering GetAvailableEditors()");
            var editors = new List<string>();
            XmlDocument xmlConfig = this.DNNConfiguration;
			_logger.LogError("GetAvailableEditors(): editors → {Editors}", editors);
            if (xmlConfig != null && xmlConfig.DocumentElement != null)
            {
                XmlNodeList editorNodes = xmlConfig.DocumentElement.SelectNodes(HtmlEditorNode + "/providers/add");
				_logger.LogError("GetAvailableEditors(): editorNodes → {EditorNodes}", editorNodes);
                if (editorNodes != null)
                {
                    int i = 0;
                    while (i < editorNodes.Count)
                    {
                        XmlNode node = editorNodes[i];
                        if (node.Attributes["name"] != null)
                        {
                            editors.Add(node.Attributes["name"].Value);
                        }

                        i = i + 1;
                    }
                }
            }

			_logger.LogWarning("<<< Exiting GetAvailableEditors()");
            return editors;
        }

        /// <summary>Gets the selected editor.</summary>
        /// <returns>The currently configured editor.</returns>
        private string GetSelectedEditor()
        {
			_logger.LogError(">>> Entering GetSelectedEditor()");
            XmlDocument xmlConfig = this.DNNConfiguration;

            if (xmlConfig != null && xmlConfig.DocumentElement != null)
            {
                XmlNode editorProviderNode = xmlConfig.DocumentElement.SelectSingleNode(HtmlEditorNode);
				_logger.LogError("GetSelectedEditor(): editorProviderNode → {EditorProviderNode}", editorProviderNode);
                return editorProviderNode.Attributes["defaultProvider"].Value;
            }

			_logger.LogWarning("<<< Exiting GetSelectedEditor()");
            return string.Empty;
        }
    }
}
