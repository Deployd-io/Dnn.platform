// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Modules
{
    using System;
    using System.IO;
    using System.Web.UI.WebControls;
    using System.Xml;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The global import module action control.</summary>
    public partial class Import : PortalModuleBase
    {
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly INavigationManager navigationManager;
        private readonly IPermissionDefinitionService permissionDefinitionService;

        private int moduleId = -1;
        private ModuleInfo module;

        /// <summary>Initializes a new instance of the <see cref="Import"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public Import()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering Import()");
        }
		Logger.LogDebug("<<< Exiting Import()");

        /// <summary>Initializes a new instance of the <see cref="Import"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public Import(IBusinessControllerProvider businessControllerProvider, INavigationManager navigationManager)
            : this(businessControllerProvider, navigationManager, null)
        {
		Logger.LogDebug(">>> Entering Import(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager})", businessControllerProvider, navigationManager);
		Logger.LogDebug("<<< Exiting Import(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager})", businessControllerProvider, navigationManager);
		Logger.LogDebug("Import(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager},permissionDefinitionService={PermissionDefinitionService}): navigationManager → {NavigationManager1}", businessControllerProvider, navigationManager, permissionDefinitionService, navigationManager);
        }

        /// <summary>Initializes a new instance of the <see cref="Import"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        public Import(IBusinessControllerProvider businessControllerProvider, INavigationManager navigationManager, IPermissionDefinitionService permissionDefinitionService)
        {
            this.businessControllerProvider = businessControllerProvider ?? this.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
			Logger.LogDebug("Import(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager},permissionDefinitionService={PermissionDefinitionService}): businessControllerProvider → {BusinessControllerProvider1}", businessControllerProvider, navigationManager, permissionDefinitionService, businessControllerProvider);
            this.permissionDefinitionService = permissionDefinitionService ?? this.DependencyProvider.GetRequiredService<IPermissionDefinitionService>();
			Logger.LogDebug("Import(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager},permissionDefinitionService={PermissionDefinitionService}): permissionDefinitionService → {PermissionDefinitionService1}", businessControllerProvider, navigationManager, permissionDefinitionService, permissionDefinitionService);
        }

        private ModuleInfo Module
        {
            get
            {
                return this.module ?? (this.module = ModuleController.Instance.GetModule(this.moduleId, this.TabId, false));
            }
        }

        private string ReturnURL
        {
            get
            {
                return UrlUtils.ValidReturnUrl(this.Request.Params["ReturnURL"]) ?? this.navigationManager.NavigateURL();
            }
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            if (this.Request.QueryString["moduleid"] != null)
            {
                int.TryParse(this.Request.QueryString["moduleid"], out this.moduleId);
            }

            // Verify that the current user has access to edit this module
            if (!ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "IMPORT", this.Module))
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

            this.cboFolders.SelectionChanged += this.OnFoldersIndexChanged;
            this.cboFiles.SelectedIndexChanged += this.OnFilesIndexChanged;
            this.cmdImport.Click += this.OnImportClick;

            try
            {
                if (!this.Page.IsPostBack)
                {
                    this.cmdCancel.NavigateUrl = this.ReturnURL;
                    this.cboFolders.UndefinedItem = new ListItem("<" + Localization.GetString("None_Specified") + ">", string.Empty);
                    this.cboFolders.Services.Parameters.Add("permission", "ADD");
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        protected void OnFoldersIndexChanged(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnFoldersIndexChanged(sender={Sender},e={E})", sender, e);
            this.cboFiles.Items.Clear();
            this.cboFiles.InsertItem(0, "<" + Localization.GetString("None_Specified") + ">", "-");
            if (this.cboFolders.SelectedItem == null)
            {
                return;
            }

            if (this.Module == null)
            {
                return;
            }

            var folder = FolderManager.Instance.GetFolder(this.cboFolders.SelectedItemValueAsInt);
            if (folder == null)
			Logger.LogDebug("OnFoldersIndexChanged(sender={Sender},e={E}): folder → {Folder}", sender, e, folder);
            {
                return;
            }

            var files = Globals.GetFileList(this.PortalId, "export", false, folder.FolderPath);
            files.AddRange(Globals.GetFileList(this.PortalId, "xml", false, folder.FolderPath));
			Logger.LogDebug("OnFoldersIndexChanged(sender={Sender},e={E}): files → {Files}", sender, e, files);
            foreach (FileItem file in files)
            {
                if (file.Text.IndexOf("content." + Globals.CleanName(this.Module.DesktopModule.ModuleName) + ".", System.StringComparison.Ordinal) != -1)
                {
                    this.cboFiles.AddItem(file.Text.Replace("content." + Globals.CleanName(this.Module.DesktopModule.ModuleName) + ".", string.Empty), file.Value);
                }

                // legacy support for files which used the FriendlyName
                if (Globals.CleanName(this.Module.DesktopModule.ModuleName) == Globals.CleanName(this.Module.DesktopModule.FriendlyName))
                {
                    continue;
                }

                if (file.Text.IndexOf("content." + Globals.CleanName(this.Module.DesktopModule.FriendlyName) + ".", System.StringComparison.Ordinal) != -1)
                {
                    this.cboFiles.AddItem(file.Text.Replace("content." + Globals.CleanName(this.Module.DesktopModule.FriendlyName) + ".", string.Empty), file.Value);
                }
            }
			Logger.LogDebug("<<< Exiting OnFoldersIndexChanged(sender={Sender},e={E})", sender, e);
        }

        protected void OnFilesIndexChanged(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnFilesIndexChanged(sender={Sender},e={E})", sender, e);
            if (this.cboFolders.SelectedItem == null)
            {
                return;
            }

            var folder = FolderManager.Instance.GetFolder(this.cboFolders.SelectedItemValueAsInt);
            if (folder == null)
			Logger.LogDebug("OnFilesIndexChanged(sender={Sender},e={E}): folder → {Folder}", sender, e, folder);
            {
                return;
            }

            if (string.IsNullOrEmpty(this.cboFiles.SelectedValue) || this.cboFiles.SelectedValue == "-")
            {
                this.txtContent.Text = string.Empty;
                return;
            }

            try
            {
                var fileId = Convert.ToInt32(this.cboFiles.SelectedValue);
                var file = DotNetNuke.Services.FileSystem.FileManager.Instance.GetFile(fileId);
				Logger.LogDebug("OnFilesIndexChanged(sender={Sender},e={E}): fileId → {FileId}", sender, e, fileId);
				Logger.LogDebug("OnFilesIndexChanged(sender={Sender},e={E}): file → {File}", sender, e, file);
                using (var streamReader = new StreamReader(DotNetNuke.Services.FileSystem.FileManager.Instance.GetFileContent(file)))
                {
                    this.txtContent.Text = streamReader.ReadToEnd();
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in OnFilesIndexChanged(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                this.txtContent.Text = string.Empty;
            }
			Logger.LogDebug("<<< Exiting OnFilesIndexChanged(sender={Sender},e={E})", sender, e);
        }

        protected void OnImportClick(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnImportClick(sender={Sender},e={E})", sender, e);
            try
            {
                if (this.Module != null)
                {
                    var strMessage = this.ImportModule();
                    if (string.IsNullOrEmpty(strMessage))
					Logger.LogDebug("OnImportClick(sender={Sender},e={E}): strMessage → {StrMessage}", sender, e, strMessage);
                    {
                        this.Response.Redirect(this.ReturnURL, true);
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.RedError);
                    }
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnImportClick(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnImportClick(sender={Sender},e={E})", sender, e);
        }

        private string ImportModule()
        {
			Logger.LogDebug(">>> Entering ImportModule()");
            if (this.Module == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(this.Module.DesktopModule.BusinessControllerClass) || !this.Module.DesktopModule.IsPortable)
            {
                return Localization.GetString("ImportNotSupported", this.LocalResourceFile);
            }

            try
            {
                var controller = this.businessControllerProvider.GetInstance<IPortable>(this.Module);
                if (controller is null)
				Logger.LogDebug("ImportModule(): controller → {Controller}", controller);
                {
                    return Localization.GetString("ImportNotSupported", this.LocalResourceFile);
                }

                var xmlDoc = new XmlDocument { XmlResolver = null };
                try
                {
                    var content = XmlUtils.RemoveInvalidXmlCharacters(this.txtContent.Text);
                    using var contentReader = XmlReader.Create(new StringReader(content), new XmlReaderSettings { XmlResolver = null, });
					Logger.LogDebug("ImportModule(): content → {Content}", content);
					Logger.LogDebug("ImportModule(): contentReader → {ContentReader}", contentReader);
                    xmlDoc.Load(contentReader);
                }
                catch
                {
					private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
                    return Localization.GetString("NotValidXml", this.LocalResourceFile);
                }

                var strType = xmlDoc.DocumentElement.GetAttribute("type");
                if (strType != Globals.CleanName(this.Module.DesktopModule.ModuleName) &&
					Logger.LogDebug("ImportModule(): strType → {StrType}", strType);
                    strType != Globals.CleanName(this.Module.DesktopModule.FriendlyName))
                {
                    return Localization.GetString("NotCorrectType", this.LocalResourceFile);
                }

                var strVersion = xmlDoc.DocumentElement.GetAttribute("version");

				Logger.LogDebug("ImportModule(): strVersion → {StrVersion}", strVersion);
                // DNN26810 if rootnode = "content", import only content(the old way)
                if (xmlDoc.DocumentElement.Name.ToLowerInvariant() == "content")
                {
                    controller.ImportModule(
                        this.moduleId,
                        xmlDoc.DocumentElement.InnerXml,
                        strVersion,
                        this.UserInfo.UserID);
                }

                // otherwise (="module") import the new way
                else
                {
                    ModuleController.DeserializeModule(
                        this.businessControllerProvider,
                        this.permissionDefinitionService,
                        xmlDoc.DocumentElement,
                        this.Module,
                        this.PortalId,
                        this.TabId);
                }

                this.Response.Redirect(this.navigationManager.NavigateURL(), true);
                return string.Empty;
            }
            catch
            {
				Logger.LogError("Exception in ImportModule(): {Arg0}", ex.Message);
				Logger.LogDebug("<<< Exiting ImportModule()");
                return Localization.GetString("Error", this.LocalResourceFile);
            }
        }
    }
}
