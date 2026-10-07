// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Tabs
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI.WebControls;
    using System.Xml;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The view for the global import page action.</summary>
    public partial class Import : PortalModuleBase
    {
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly INavigationManager navigationManager;
        private readonly IEventLogger eventLogger;
        private readonly IPermissionDefinitionService permissionDefinitionService;
        private readonly IHostSettings hostSettings;

        private TabInfo tab;

        /// <summary>Initializes a new instance of the <see cref="Import"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public Import()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Import"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public Import(IBusinessControllerProvider businessControllerProvider, INavigationManager navigationManager)
            : this(businessControllerProvider, navigationManager, null)
        {
		eventLogger.LogError(">>> Entering Import(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager})", businessControllerProvider, navigationManager);
        eventLogger.LogWarning("<<< Exiting Import(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager})", businessControllerProvider, navigationManager);
        }

        /// <summary>Initializes a new instance of the <see cref="Import"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="eventLogger">The event logger.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public Import(IBusinessControllerProvider businessControllerProvider, INavigationManager navigationManager, IEventLogger eventLogger)
            : this(businessControllerProvider, navigationManager, eventLogger, null)
        {
		eventLogger.LogError(">>> Entering Import(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager},eventLogger={EventLogger})", businessControllerProvider, navigationManager, eventLogger);
        eventLogger.LogWarning("<<< Exiting Import(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager},eventLogger={EventLogger})", businessControllerProvider, navigationManager, eventLogger);
        }

        /// <summary>Initializes a new instance of the <see cref="Import"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public Import(IBusinessControllerProvider businessControllerProvider, INavigationManager navigationManager, IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService)
            : this(businessControllerProvider, navigationManager, eventLogger, permissionDefinitionService, null)
        {
		eventLogger.LogError(">>> Entering Import(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager},eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService})", businessControllerProvider, navigationManager, eventLogger, permissionDefinitionService);
        }

        /// <summary>Initializes a new instance of the <see cref="Import"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="hostSettings">The host settings.</param>
        public Import(IBusinessControllerProvider businessControllerProvider, INavigationManager navigationManager, IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService, IHostSettings hostSettings)
        {
			eventLogger.LogDebug("Import(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager},eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings}): navigationManager → {NavigationManager1}", businessControllerProvider, navigationManager, eventLogger, permissionDefinitionService, hostSettings, navigationManager);
			eventLogger.LogDebug("Import(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager},eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings}): eventLogger → {EventLogger1}", businessControllerProvider, navigationManager, eventLogger, permissionDefinitionService, hostSettings, eventLogger);
			eventLogger.LogDebug("Import(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager},eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings}): permissionDefinitionService → {PermissionDefinitionService1}", businessControllerProvider, navigationManager, eventLogger, permissionDefinitionService, hostSettings, permissionDefinitionService);
			eventLogger.LogDebug("Import(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager},eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings}): hostSettings → {HostSettings1}", businessControllerProvider, navigationManager, eventLogger, permissionDefinitionService, hostSettings, hostSettings);
            this.businessControllerProvider = businessControllerProvider ?? this.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
			eventLogger.LogWarning("<<< Exiting Import(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager},eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings})", businessControllerProvider, navigationManager, eventLogger, permissionDefinitionService, hostSettings);
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.eventLogger = eventLogger ?? this.DependencyProvider.GetRequiredService<IEventLogger>();
            this.permissionDefinitionService = permissionDefinitionService ?? this.DependencyProvider.GetRequiredService<IPermissionDefinitionService>();
            this.hostSettings = hostSettings ?? this.DependencyProvider.GetRequiredService<IHostSettings>();
        }

        public TabInfo Tab => this.tab ??= TabController.Instance.GetTab(this.TabId, this.PortalId, false);

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			eventLogger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            if (!TabPermissionController.CanImportPage())
            {
                this.Response.Redirect(Globals.AccessDeniedURL(), true);
            }
			eventLogger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			eventLogger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.cboFolders.SelectionChanged += this.OnFolderIndexChanged;
            this.cmdImport.Click += this.OnImportClick;
            this.cboParentTab.SelectionChanged += this.OnParentTabIndexChanged;
            this.cboTemplate.SelectedIndexChanged += this.OnTemplateIndexChanged;
            this.optMode.SelectedIndexChanged += this.OptModeSelectedIndexChanged;

            try
            {
                if (!this.Page.IsPostBack)
                {
                    this.cmdCancel.NavigateUrl = this.navigationManager.NavigateURL();
                    this.cboFolders.UndefinedItem = new ListItem("<" + Localization.GetString("None_Specified") + ">", string.Empty);
                    var folders = FolderManager.Instance.GetFolders(this.UserInfo, "BROWSE, ADD");
					eventLogger.LogError("OnLoad(e={E}): folders → {Folders}", e, folders);
                    var templateFolder = folders.SingleOrDefault(f => f.FolderPath == "Templates/");
					eventLogger.LogError("OnLoad(e={E}): templateFolder → {TemplateFolder}", e, templateFolder);
                    if (templateFolder != null)
                    {
                        this.cboFolders.SelectedFolder = templateFolder;
                    }

                    this.BindFiles();
                    this.BindTabControls();
                    this.DisplayNewRows();
                }
            }
            catch (Exception exc)
            {
				eventLogger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			eventLogger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        protected void OnFolderIndexChanged(object sender, EventArgs e)
        {
			eventLogger.LogError(">>> Entering OnFolderIndexChanged(sender={Sender},e={E})", sender, e);
            this.BindFiles();
			eventLogger.LogWarning("<<< Exiting OnFolderIndexChanged(sender={Sender},e={E})", sender, e);
        }

        protected void OnImportClick(object sender, EventArgs e)
        {
			eventLogger.LogError(">>> Entering OnImportClick(sender={Sender},e={E})", sender, e);
            try
            {
                if (this.cboTemplate.SelectedItem == null || this.cboTemplate.SelectedValue == "None_Specified")
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("SpecifyFile", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                if (this.optMode.SelectedIndex == -1)
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("SpecifyMode", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                if (this.cboFolders.SelectedItem == null)
                {
                    return;
                }

                var selectedFolder = FolderManager.Instance.GetFolder(this.cboFolders.SelectedItemValueAsInt);
				eventLogger.LogError("OnImportClick(sender={Sender},e={E}): selectedFolder → {SelectedFolder}", sender, e, selectedFolder);
                if (selectedFolder == null)
                {
                    return;
                }

                var selectedFile = FileManager.Instance.GetFile(Convert.ToInt32(this.cboTemplate.SelectedValue));
				eventLogger.LogError("OnImportClick(sender={Sender},e={E}): selectedFile → {SelectedFile}", sender, e, selectedFile);
                var xmlDoc = new XmlDocument { XmlResolver = null };
                using (var content = FileManager.Instance.GetFileContent(selectedFile))
                using (var xmlReader = XmlReader.Create(content, new XmlReaderSettings { XmlResolver = null, }))
                {
                    xmlDoc.Load(xmlReader);
                }

                var tabNodes = new List<XmlNode>();
				eventLogger.LogError("OnImportClick(sender={Sender},e={E}): tabNodes → {TabNodes}", sender, e, tabNodes);
                var selectSingleNode = xmlDoc.SelectSingleNode("//portal/tabs");
				eventLogger.LogError("OnImportClick(sender={Sender},e={E}): selectSingleNode → {SelectSingleNode}", sender, e, selectSingleNode);
                if (selectSingleNode != null)
                {
                    tabNodes.AddRange(selectSingleNode.ChildNodes.Cast<XmlNode>());
                }

                if (tabNodes.Count == 0)
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("NoTabsInTemplate", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                TabInfo objTab;
                if (this.optMode.SelectedValue == "ADD")
                {
                    // Check for invalid
                    string invalidType;
                    if (!TabController.IsValidTabName(this.txtTabName.Text, out invalidType))
                    {
                        var warningMessage = string.Format(CultureInfo.CurrentCulture, Localization.GetString(invalidType, this.LocalResourceFile), this.txtTabName.Text);
						eventLogger.LogError("OnImportClick(sender={Sender},e={E}): warningMessage → {WarningMessage}", sender, e, warningMessage);
                        UI.Skins.Skin.AddModuleMessage(this, warningMessage, ModuleMessage.ModuleMessageType.RedError);
                        return;
                    }

                    // New Tab
                    objTab = new TabInfo { PortalID = this.PortalId, TabName = this.txtTabName.Text, IsVisible = true };
					eventLogger.LogError("OnImportClick(sender={Sender},e={E}): objTab → {ObjTab}", sender, e, objTab);
					eventLogger.LogDebug("OnImportClick(sender={Sender},e={E}): objTab → {ObjTab}", sender, e, objTab);
                    var parentId = this.cboParentTab.SelectedItemValueAsInt;
                    if (parentId != Null.NullInteger)
                    {
                        objTab.ParentId = parentId;
						eventLogger.LogDebug("OnImportClick(sender={Sender},e={E}): objTab.ParentId → {Arg2}", sender, e, objTab.ParentId);
                    }

                    objTab.TabPath = Globals.GenerateTabPath(objTab.ParentId, objTab.TabName);
                    var tabId = TabController.GetTabByTabPath(this.hostSettings, objTab.PortalID, objTab.TabPath, Null.NullString);
					eventLogger.LogError("OnImportClick(sender={Sender},e={E}): tabId → {TabId}", sender, e, tabId);

                    // Check if tab exists
                    if (tabId != Null.NullInteger)
                    {
                        TabInfo existingTab = TabController.Instance.GetTab(tabId, this.PortalId, false);
						eventLogger.LogError("OnImportClick(sender={Sender},e={E}): existingTab → {ExistingTab}", sender, e, existingTab);
                        if (existingTab != null && existingTab.IsDeleted)
                        {
                            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("TabRecycled", this.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                        }
                        else
                        {
                            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("TabExists", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                        }

                        return;
                    }

                    var positionTabId = int.Parse(this.cboPositionTab.SelectedItem.Value);
					eventLogger.LogError("OnImportClick(sender={Sender},e={E}): positionTabId → {PositionTabId}", sender, e, positionTabId);

                    if (this.rbInsertPosition.SelectedValue == "After" && positionTabId > Null.NullInteger)
                    {
                        objTab.TabID = TabController.Instance.AddTabAfter(objTab, positionTabId);
                    }
                    else if (this.rbInsertPosition.SelectedValue == "Before" && positionTabId > Null.NullInteger)
                    {
                        objTab.TabID = TabController.Instance.AddTabBefore(objTab, positionTabId);
                    }
                    else
                    {
                        objTab.TabID = TabController.Instance.AddTab(objTab);
                    }

                    this.eventLogger.AddLog(objTab, this.PortalSettings, this.UserId, string.Empty, EventLogType.TAB_CREATED);

                    objTab = TabController.DeserializeTab(this.businessControllerProvider, this.permissionDefinitionService, tabNodes[0], objTab, this.PortalId, PortalTemplateModuleAction.Replace);

                    var exceptions = string.Empty;

                    // Create second tabs onwards. For firs tab, we like to use tab details from text box, for rest it'll come from template
                    for (var tab = 1; tab < tabNodes.Count; tab++)
                    {
                        try
                        {
                            TabController.DeserializeTab(this.businessControllerProvider, this.permissionDefinitionService, tabNodes[tab], null, this.PortalId, PortalTemplateModuleAction.Replace);
                        }
                        catch (Exception ex)
                        {
							eventLogger.LogError("Exception in OnImportClick(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                            Exceptions.LogException(ex);
                            exceptions += string.Format("Template Tab # {0}. Error {1}<br/>", tab + 1, ex.Message);
                        }
                    }

                    if (!string.IsNullOrEmpty(exceptions))
                    {
                        UI.Skins.Skin.AddModuleMessage(this, exceptions, ModuleMessage.ModuleMessageType.RedError);
                        return;
                    }
                }
                else
                {
                    // Replace Existing Tab
                    objTab = TabController.DeserializeTab(this.businessControllerProvider, this.permissionDefinitionService, tabNodes[0], this.Tab, this.PortalId, PortalTemplateModuleAction.Replace);
                }

                switch (this.optRedirect.SelectedValue)
                {
                    case "VIEW":
                        this.Response.Redirect(this.navigationManager.NavigateURL(objTab.TabID), true);
                        break;
                    default:
                        this.Response.Redirect(this.navigationManager.NavigateURL(objTab.TabID, "Tab", "action=edit"), true);
                        break;
                }
            }
            catch (Exception exc)
            {
				eventLogger.LogError("Exception in OnImportClick(sender={Sender},e={E}): {Arg2}", sender, e, exc.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			eventLogger.LogWarning("<<< Exiting OnImportClick(sender={Sender},e={E})", sender, e);
        }

        protected void OnParentTabIndexChanged(object sender, EventArgs e)
        {
			eventLogger.LogError(">>> Entering OnParentTabIndexChanged(sender={Sender},e={E})", sender, e);
            this.BindBeforeAfterTabControls();
			eventLogger.LogWarning("<<< Exiting OnParentTabIndexChanged(sender={Sender},e={E})", sender, e);
        }

        protected void OnTemplateIndexChanged(object sender, EventArgs e)
        {
			eventLogger.LogError(">>> Entering OnTemplateIndexChanged(sender={Sender},e={E})", sender, e);
            try
            {
                if (this.cboTemplate.SelectedIndex > 0 && this.cboFolders.SelectedItem != null)
                {
                    var selectedFile = FileManager.Instance.GetFile(Convert.ToInt32(this.cboTemplate.SelectedValue));
					eventLogger.LogError("OnTemplateIndexChanged(sender={Sender},e={E}): selectedFile → {SelectedFile}", sender, e, selectedFile);
                    var xmldoc = new XmlDocument { XmlResolver = null };
                    using (var fileContent = FileManager.Instance.GetFileContent(selectedFile))
                    using (var xmlReader = XmlReader.Create(fileContent, new XmlReaderSettings { XmlResolver = null, }))
                    {
                        xmldoc.Load(xmlReader);
                    }

                    var node = xmldoc.SelectSingleNode("//portal/description");
					eventLogger.LogError("OnTemplateIndexChanged(sender={Sender},e={E}): node → {Node}", sender, e, node);
                    if (node != null && !string.IsNullOrEmpty(node.InnerXml))
                    {
                        this.lblTemplateDescription.Visible = true;
                        this.lblTemplateDescription.Text = this.Server.HtmlDecode(node.InnerXml);
                        this.txtTabName.Text = this.cboTemplate.SelectedItem.Text;
                    }
                    else
                    {
                        this.lblTemplateDescription.Visible = false;
                    }
                }
                else
                {
                    this.lblTemplateDescription.Visible = false;
                }
            }
            catch (Exception exc)
            {
				eventLogger.LogError("Exception in OnTemplateIndexChanged(sender={Sender},e={E}): {Arg2}", sender, e, exc.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			eventLogger.LogWarning("<<< Exiting OnTemplateIndexChanged(sender={Sender},e={E})", sender, e);
        }

        protected void OptModeSelectedIndexChanged(object sender, EventArgs e)
        {
			eventLogger.LogError(">>> Entering OptModeSelectedIndexChanged(sender={Sender},e={E})", sender, e);
            this.DisplayNewRows();
			eventLogger.LogWarning("<<< Exiting OptModeSelectedIndexChanged(sender={Sender},e={E})", sender, e);
        }

        private void BindBeforeAfterTabControls()
        {
			eventLogger.LogError(">>> Entering BindBeforeAfterTabControls()");
            var noneSpecified = "<" + Localization.GetString("None_Specified") + ">";
            this.cboParentTab.UndefinedItem = new ListItem(noneSpecified, string.Empty);
			eventLogger.LogError("BindBeforeAfterTabControls(): noneSpecified → {NoneSpecified}", noneSpecified);
            var parentTab = this.cboParentTab.SelectedPage;

            List<TabInfo> listTabs = parentTab != null ? TabController.Instance.GetTabsByPortal(parentTab.PortalID).WithParentId(parentTab.TabID) : TabController.Instance.GetTabsByPortal(this.PortalId).WithParentId(Null.NullInteger);
            listTabs = TabController.GetPortalTabs(listTabs, Null.NullInteger, true, noneSpecified, false, false, false, false, true);
			eventLogger.LogError("BindBeforeAfterTabControls(): listTabs → {ListTabs}", listTabs);
            this.cboPositionTab.DataSource = listTabs;
            this.cboPositionTab.DataBind();
            this.rbInsertPosition.Items.Clear();
            this.rbInsertPosition.Items.Add(new ListItem(Localization.GetString("InsertBefore", this.LocalResourceFile), "Before"));
            this.rbInsertPosition.Items.Add(new ListItem(Localization.GetString("InsertAfter", this.LocalResourceFile), "After"));
            this.rbInsertPosition.Items.Add(new ListItem(Localization.GetString("InsertAtEnd", this.LocalResourceFile), "AtEnd"));
            this.rbInsertPosition.SelectedValue = "After";
			eventLogger.LogWarning("<<< Exiting BindBeforeAfterTabControls()");
        }

        private void BindFiles()
        {
			eventLogger.LogError(">>> Entering BindFiles()");
            this.cboTemplate.Items.Clear();
            if (this.cboFolders.SelectedItem != null)
            {
                var folder = FolderManager.Instance.GetFolder(this.cboFolders.SelectedItemValueAsInt);
				eventLogger.LogError("BindFiles(): folder → {Folder}", folder);
                if (folder != null)
                {
                    // var files = Directory.GetFiles(PortalSettings.HomeDirectoryMapPath + folder.FolderPath, "*.page.template");
                    var files = Globals.GetFileList(this.PortalId, "page.template", false, folder.FolderPath);
					eventLogger.LogError("BindFiles(): files → {Files}", files);
                    foreach (FileItem file in files)
                    {
                        this.cboTemplate.AddItem(file.Text.Replace(".page.template", string.Empty), file.Value);
                    }

                    this.cboTemplate.InsertItem(0, "<" + Localization.GetString("None_Specified") + ">", "None_Specified");
                    this.cboTemplate.SelectedIndex = 0;
                }
            }
			eventLogger.LogWarning("<<< Exiting BindFiles()");
        }

        private void BindTabControls()
        {
			eventLogger.LogError(">>> Entering BindTabControls()");
            this.BindBeforeAfterTabControls();
            this.divInsertPositionRow.Visible = this.cboPositionTab.Items.Count > 0;
            this.cboParentTab.AutoPostBack = true;
            if (this.cboPositionTab.FindItemByValue(this.TabId.ToString(CultureInfo.InvariantCulture)) != null)
            {
                this.cboPositionTab.ClearSelection();
                this.cboPositionTab.FindItemByValue(this.TabId.ToString(CultureInfo.InvariantCulture)).Selected = true;
            }
			eventLogger.LogWarning("<<< Exiting BindTabControls()");
        }

        private void DisplayNewRows()
        {
			eventLogger.LogError(">>> Entering DisplayNewRows()");
            this.divTabName.Visible = this.optMode.SelectedIndex == 0;
            this.divParentTab.Visible = this.optMode.SelectedIndex == 0;
            this.divInsertPositionRow.Visible = this.optMode.SelectedIndex == 0;
            this.divInsertPositionRow.Visible = this.optMode.SelectedIndex == 0;
			eventLogger.LogWarning("<<< Exiting DisplayNewRows()");
        }
    }
}
