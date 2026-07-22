// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Modules
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.UI.WebControls;
    using System.Xml;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The view for the global export module action.</summary>
    public partial class Export : PortalModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly INavigationManager navigationManager;
        private readonly IHostSettings hostSettings;

        private int moduleId = -1;
        private ModuleInfo module;

        /// <summary>Initializes a new instance of the <see cref="Export"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public Export()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering Export()");
        }
		Logger.LogDebug("<<< Exiting Export()");

        /// <summary>Initializes a new instance of the <see cref="Export"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public Export(IBusinessControllerProvider businessControllerProvider, INavigationManager navigationManager)
            : this(businessControllerProvider, navigationManager, null)
        {
		Logger.LogDebug(">>> Entering Export(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager})", businessControllerProvider, navigationManager);
		Logger.LogDebug("<<< Exiting Export(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager})", businessControllerProvider, navigationManager);
		Logger.LogDebug("Export(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager},hostSettings={HostSettings}): navigationManager → {NavigationManager1}", businessControllerProvider, navigationManager, hostSettings, navigationManager);
        }

        /// <summary>Initializes a new instance of the <see cref="Export"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="hostSettings">The host settings.</param>
        public Export(IBusinessControllerProvider businessControllerProvider, INavigationManager navigationManager, IHostSettings hostSettings)
        {
            this.businessControllerProvider = businessControllerProvider ?? this.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
			Logger.LogDebug("Export(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager},hostSettings={HostSettings}): businessControllerProvider → {BusinessControllerProvider1}", businessControllerProvider, navigationManager, hostSettings, businessControllerProvider);
            this.hostSettings = hostSettings ?? this.DependencyProvider.GetRequiredService<IHostSettings>();
			Logger.LogDebug("Export(businessControllerProvider={BusinessControllerProvider},navigationManager={NavigationManager},hostSettings={HostSettings}): hostSettings → {HostSettings1}", businessControllerProvider, navigationManager, hostSettings, hostSettings);
        }

        private ModuleInfo Module => this.module ??= ModuleController.Instance.GetModule(this.moduleId, this.TabId, false);

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

            if (!ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "EXPORT", this.Module))
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
                if (this.Request.QueryString["moduleid"] != null)
                {
                    int.TryParse(this.Request.QueryString["moduleid"], out this.moduleId);
                }

                if (!this.Page.IsPostBack)
                {
                    this.cmdCancel.NavigateUrl = this.ReturnURL;

                    this.cboFolders.UndefinedItem = new ListItem("<" + Localization.GetString("None_Specified") + ">", string.Empty);
                    this.cboFolders.Services.Parameters.Add("permission", "ADD");
                    if (this.Module != null)
                    {
                        this.txtFile.Text = CleanName(this.Module.ModuleTitle);
                    }
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        protected void OnExportClick(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnExportClick(sender={Sender},e={E})", sender, e);
            try
            {
                IFolderInfo folder = null;
                if (this.cboFolders.SelectedItem != null && !string.IsNullOrEmpty(this.txtFile.Text))
				Logger.LogDebug("OnExportClick(sender={Sender},e={E}): folder → {Folder}", sender, e, folder);
                {
                    folder = FolderManager.Instance.GetFolder(this.cboFolders.SelectedItemValueAsInt);
                }

                if (folder != null)
                {
                    var strFile = $"content.{CleanName(this.Module.DesktopModule.ModuleName)}.{CleanName(this.txtFile.Text)}.export";
                    var strMessage = this.ExportModule(this.moduleId, strFile, folder);
					Logger.LogDebug("OnExportClick(sender={Sender},e={E}): strFile → {StrFile}", sender, e, strFile);
					Logger.LogDebug("OnExportClick(sender={Sender},e={E}): strMessage → {StrMessage}", sender, e, strMessage);
                    if (string.IsNullOrEmpty(strMessage))
                    {
                        this.Response.Redirect(this.ReturnURL, true);
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.RedError);
                    }
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("Validation", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnExportClick(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnExportClick(sender={Sender},e={E})", sender, e);
        }

        private static string CleanName(string name)
        {
			Logger.LogDebug(">>> Entering CleanName(name={Name})", name);
            var strName = name;
            const string strBadChars = ". ~`!@#$%^&*()-_+={[}]|\\:;<,>?/\"'";
			Logger.LogDebug("CleanName(name={Name}): strName → {StrName}", name, strName);

            int intCounter;
            for (intCounter = 0; intCounter <= strBadChars.Length - 1; intCounter++)
            {
                strName = strName.Replace(strBadChars.Substring(intCounter, 1), string.Empty);
            }

			Logger.LogDebug("<<< Exiting CleanName(name={Name})", name);
            return strName;
        }

        private string ExportModule(int moduleID, string fileName, IFolderInfo folder)
        {
            var strMessage = string.Empty;
            if (this.Module != null)
            {
                if (!string.IsNullOrEmpty(this.Module.DesktopModule.BusinessControllerClass) && this.Module.DesktopModule.IsPortable)
                {
                    try
                    {
                        var businessControllerType = Reflection.CreateType(this.Module.DesktopModule.BusinessControllerClass);

                        // Double-check
                        if (typeof(IPortable).IsAssignableFrom(businessControllerType))
                        {
                            XmlDocument moduleXml = new XmlDocument { XmlResolver = null };
                            XmlNode moduleNode = ModuleController.SerializeModule(this.businessControllerProvider, this.hostSettings, moduleXml, this.Module, true);

                            // add attributes to XML document
                            XmlAttribute typeAttribute = moduleXml.CreateAttribute("type");
                            typeAttribute.Value = CleanName(this.Module.DesktopModule.ModuleName);
                            moduleNode.Attributes.Append(typeAttribute);

                            XmlAttribute versionAttribute = moduleXml.CreateAttribute("version");
                            versionAttribute.Value = this.Module.DesktopModule.Version;
                            moduleNode.Attributes.Append(versionAttribute);

                            // Create content from XmlNode
                            StringWriter sw = new StringWriter();
                            XmlTextWriter xw = new XmlTextWriter(sw);
                            moduleNode.WriteTo(xw);
                            var content = sw.ToString();
                            if (!string.IsNullOrEmpty(content))
                            {
                                // remove invalid chars in content -> DNN 26810: Handled by ModuleController.SerializeModule
                                // content = Regex.Replace(content, _invalidCharsRegex, string.Empty);
                                // add attributes to XML document
                                // content = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "<content type=\"" + CleanName(Module.DesktopModule.ModuleName) + "\" version=\"" +
                                //          Module.DesktopModule.Version + "\">" + content + "</content>";

                                // First check the Portal limits will not be exceeded (this is approximate)
                                if (PortalController.Instance.HasSpaceAvailable(this.PortalId, content.Length))
                                {
                                    // add file to Files table
                                    using var fileContent = new MemoryStream(Encoding.UTF8.GetBytes(content));
                                    Services.FileSystem.FileManager.Instance.AddFile(folder, fileName, fileContent, true, true, "application/octet-stream");
                                }
                                else
                                {
                                    strMessage += "<br>" + string.Format(CultureInfo.CurrentCulture, Localization.GetString("DiskSpaceExceeded"), fileName);
                                }
                            }
                            else
                            {
                                strMessage = Localization.GetString("NoContent", this.LocalResourceFile);
                            }
                        }
                        else
                        {
                            strMessage = Localization.GetString("ExportNotSupported", this.LocalResourceFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is InvalidFileExtensionException || ex is PermissionsNotMetException || ex is InvalidFilenameException)
                        {
                            strMessage = ex.Message;
                        }
                        else
                        {
                            strMessage = Localization.GetString("Error", this.LocalResourceFile);
                        }
                    }
                }
                else
                {
                    strMessage = Localization.GetString("ExportNotSupported", this.LocalResourceFile);
                }
            }

            return strMessage;
        }
    }
}
