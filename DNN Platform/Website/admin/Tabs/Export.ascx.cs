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
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly INavigationManager navigationManager;
        private TabInfo tab;

        /// <summary>Initializes a new instance of the <see cref="Export"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public Export()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering Export()");
		Logger.LogDebug("<<< Exiting Export()");
		Logger.LogDebug(">>> Entering Export(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager})", businessControllerProvider, navigationManager);
		Logger.LogDebug("Export(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager}): navigationManager → {NavigationManager1}", businessControllerProvider, navigationManager, navigationManager);
        }

        /// <summary>Initializes a new instance of the <see cref="Export"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        public Export(IBusinessControllerProvider businessControllerProvider, INavigationManager navigationManager)
        {
            this.businessControllerProvider = businessControllerProvider ?? this.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
			Logger.LogDebug("<<< Exiting Export(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager})", businessControllerProvider, navigationManager);
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
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            if (!TabPermissionController.CanExportPage())
            {
                this.Response.Redirect(Globals.AccessDeniedURL(), true);
            }
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
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
				Logger.LogDebug("OnLoad(e={E}): folderPath → {FolderPath}", e, folderPath);
                var templateFolder = FolderManager.Instance.GetFolder(this.UserInfo.PortalID, folderPath);
				Logger.LogDebug("OnLoad(e={E}): templateFolder → {TemplateFolder}", e, templateFolder);
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
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        protected void OnExportClick(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnExportClick(sender={Sender},e={E})", sender, e);
            try
            {
                if (!this.Page.IsValid)
                {
                    return;
                }

                if (this.cboFolders.SelectedItem != null)
                {
                    var folder = FolderManager.Instance.GetFolder(this.cboFolders.SelectedItemValueAsInt);
					Logger.LogDebug("OnExportClick(sender={Sender},e={E}): folder → {Folder}", sender, e, folder);
                    if (folder != null)
                    {
                        var filename = folder.FolderPath + this.txtFile.Text + ".page.template";
						Logger.LogDebug("OnExportClick(sender={Sender},e={E}): filename → {Filename}", sender, e, filename);
                        filename = filename.Replace("/", @"\");

                        var xmlTemplate = new XmlDocument { XmlResolver = null };
                        XmlNode nodePortal = xmlTemplate.AppendChild(xmlTemplate.CreateElement("portal"));
                        if (nodePortal.Attributes != null)
                        {
							Logger.LogDebug("OnExportClick(sender={Sender},e={E}): nodePortal → {NodePortal}", sender, e, nodePortal);
                            nodePortal.Attributes.Append(XmlUtils.CreateAttribute(xmlTemplate, "version", "3.0"));
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
							Logger.LogDebug("OnExportClick(sender={Sender},e={E}): node → {Node}", sender, e, node);
							Logger.LogDebug("OnExportClick(sender={Sender},e={E}): nodeTabs → {NodeTabs}", sender, e, nodeTabs);
                            Services.FileSystem.FileManager.Instance.AddFile(folder, this.txtFile.Text + ".page.template", fileContent, true, true, "application/octet-stream");
                        }
                    }
                }
            }
            catch (Exception exc)
            {
				private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Export.ascx));
                Exceptions.ProcessModuleLoadException(this, exc);
				Logger.LogError("Exception in OnExportClick(sender={Sender},e={E}): {Arg2}", sender, e, exc.getMessage());
            }
        }

        /// <summary>Serializes the Tab.</summary>
        /// <param name="xmlTemplate">Reference to XmlDocument context.</param>
        /// <param name="nodeTabs">Node to add the serialized objects.</param>
        private void SerializeTab(XmlDocument xmlTemplate, XmlNode nodeTabs)
        {
			Logger.LogDebug(">>> Entering SerializeTab(xmlTemplate={XmlTemplate},nodeTabs={NodeTabs})", xmlTemplate, nodeTabs);
            var xmlTab = new XmlDocument { XmlResolver = null };
            var nodeTab = TabController.SerializeTab(this.businessControllerProvider, xmlTab, this.Tab, this.chkContent.Checked);
			Logger.LogDebug("SerializeTab(xmlTemplate={XmlTemplate},nodeTabs={NodeTabs}): nodeTab → {NodeTab}", xmlTemplate, nodeTabs, nodeTab);
            nodeTabs.AppendChild(xmlTemplate.ImportNode(nodeTab, true));
			Logger.LogDebug("<<< Exiting SerializeTab(xmlTemplate={XmlTemplate},nodeTabs={NodeTabs})", xmlTemplate, nodeTabs);
        }

        private bool IsAccessibleByUser(IFolderInfo folder)
        {
			Logger.LogDebug(">>> Entering IsAccessibleByUser(folder={Folder})", folder);
			Logger.LogDebug("<<< Exiting IsAccessibleByUser(folder={Folder})", folder);
            return FolderPermissionController.Instance.CanAddFolder(folder);
        }
    }
}
