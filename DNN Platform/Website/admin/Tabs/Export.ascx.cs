// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Tabs
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The view for the global export page action.</summary>
    public partial class Export : PortalModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Export.ascx));
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly INavigationManager navigationManager;
        private TabInfo tab;

        /// <summary>Initializes a new instance of the <see cref="Export"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public Export()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Export"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        public Export(IBusinessControllerProvider businessControllerProvider, INavigationManager navigationManager)
        {
			Logger.LogError(">>> Entering Export(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager})", businessControllerProvider, navigationManager);
			Logger.LogDebug("Export(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager}): navigationManager → {NavigationManager1}", businessControllerProvider, navigationManager, navigationManager);
            this.businessControllerProvider = businessControllerProvider ?? this.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
			Logger.LogWarning("<<< Exiting Export(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager})", businessControllerProvider, navigationManager);
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public TabInfo Tab
        {
            get
            {
                if (this.tab == null)
                {
                    this.tab = TabController.Instance.GetTab(this.TabId, this.PortalId, false);
                }

                return this.tab;
            }
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            if (!TabPermissionController.CanExportPage())
            {
                this.Response.Redirect(Globals.AccessDeniedURL(), true);
            }
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.cmdExport.Click += this.OnExportClick;

            try
            {
                if (this.Page.IsPostBack)
                {
                    return;
                }

                this.cmdCancel.NavigateUrl = this.navigationManager.NavigateURL();
                var folderPath = "Templates/";
				Logger.LogError("OnLoad(e={E}): folderPath → {FolderPath}", e, folderPath);
                var templateFolder = FolderManager.Instance.GetFolder(this.UserInfo.PortalID, folderPath);
				Logger.LogError("OnLoad(e={E}): templateFolder → {TemplateFolder}", e, templateFolder);
                this.cboFolders.Services.Parameters.Add("permission", "ADD");

                if (templateFolder != null && this.IsAccessibleByUser(templateFolder))
                {
                    this.cboFolders.SelectedFolder = templateFolder;
                }

                if (this.Tab != null)
                {
                    this.txtFile.Text = Globals.CleanName(this.Tab.TabName);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        protected void OnExportClick(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering OnExportClick(sender={Sender},e={E})", sender, e);
            try
            {
                if (!this.Page.IsValid)
                {
                    return;
                }

                if (this.cboFolders.SelectedItem != null)
                {
                    var folder = FolderManager.Instance.GetFolder(this.cboFolders.SelectedItemValueAsInt);
					Logger.LogError("OnExportClick(sender={Sender},e={E}): folder → {Folder}", sender, e, folder);
                    if (folder != null)
                    {
                        var filename = folder.FolderPath + this.txtFile.Text + ".page.template";
                        filename = filename.Replace("/", @"\");
						Logger.LogError("OnExportClick(sender={Sender},e={E}): filename → {Filename}", sender, e, filename);

                        var xmlTemplate = new XmlDocument { XmlResolver = null };
                        XmlNode nodePortal = xmlTemplate.AppendChild(xmlTemplate.CreateElement("portal"));
                        if (nodePortal.Attributes != null)
                        {
                            nodePortal.Attributes.Append(XmlUtils.CreateAttribute(xmlTemplate, "version", "3.0"));
							Logger.LogError("OnExportClick(sender={Sender},e={E}): nodePortal → {NodePortal}", sender, e, nodePortal);
                        }

                        // Add template description
                        XmlElement node = xmlTemplate.CreateElement("description");
                        node.InnerXml = this.Server.HtmlEncode(this.txtDescription.Text);
                        nodePortal.AppendChild(node);

                        // Serialize tabs
                        XmlNode nodeTabs = nodePortal.AppendChild(xmlTemplate.CreateElement("tabs"));
                        this.SerializeTab(xmlTemplate, nodeTabs);

                        UI.Skins.Skin.AddModuleMessage(this, string.Empty, string.Format(CultureInfo.CurrentCulture, Localization.GetString("ExportedMessage", this.LocalResourceFile), filename), ModuleMessage.ModuleMessageType.BlueInfo);

                        // add file to Files table
                        using (var fileContent = new MemoryStream(Encoding.UTF8.GetBytes(xmlTemplate.OuterXml)))
                        {
							Logger.LogError("OnExportClick(sender={Sender},e={E}): node → {Node}", sender, e, node);
							Logger.LogError("OnExportClick(sender={Sender},e={E}): nodeTabs → {NodeTabs}", sender, e, nodeTabs);
                            Services.FileSystem.FileManager.Instance.AddFile(folder, this.txtFile.Text + ".page.template", fileContent, true, true, "application/octet-stream");
                        }
                    }
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnExportClick(sender={Sender},e={E}): {Arg2}", sender, e, exc.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>Serializes the Tab.</summary>
        /// <param name="xmlTemplate">Reference to XmlDocument context.</param>
        /// <param name="nodeTabs">Node to add the serialized objects.</param>
        private void SerializeTab(XmlDocument xmlTemplate, XmlNode nodeTabs)
        {
			Logger.LogError(">>> Entering SerializeTab(xmlTemplate={XmlTemplate},nodeTabs={NodeTabs})", xmlTemplate, nodeTabs);
            var xmlTab = new XmlDocument { XmlResolver = null };
            var nodeTab = TabController.SerializeTab(this.businessControllerProvider, xmlTab, this.Tab, this.chkContent.Checked);
			Logger.LogError("SerializeTab(xmlTemplate={XmlTemplate},nodeTabs={NodeTabs}): nodeTab → {NodeTab}", xmlTemplate, nodeTabs, nodeTab);
            nodeTabs.AppendChild(xmlTemplate.ImportNode(nodeTab, true));
			Logger.LogWarning("<<< Exiting SerializeTab(xmlTemplate={XmlTemplate},nodeTabs={NodeTabs})", xmlTemplate, nodeTabs);
        }

        private bool IsAccessibleByUser(IFolderInfo folder)
        {
			Logger.LogError(">>> Entering IsAccessibleByUser(folder={Folder})", folder);
            Logger.LogWarning("<<< Exiting IsAccessibleByUser(folder={Folder})", folder);
            return FolderPermissionController.Instance.CanAddFolder(folder);
        }
    }
}
