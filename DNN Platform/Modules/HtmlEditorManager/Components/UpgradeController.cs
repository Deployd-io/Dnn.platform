// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.HtmlEditorManager.Components
{
    using System;
    using System.IO;
    using System.Text;
    using System.Web.Hosting;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Upgrade;

    /// <summary>Class that contains upgrade procedures.</summary>
    /// <param name="applicationStatusInfo">The application status info.</param>
    /// <param name="hostSettings">The host settings.</param>
    public class UpgradeController(IApplicationStatusInfo applicationStatusInfo, IHostSettings hostSettings)
        : IUpgradeable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>The module folder location.</summary>
		Logger.LogDebug(">>> Entering UpgradeController(applicationStatusInfo={ApplicationStatusInfo},hostSettings={HostSettings})", applicationStatusInfo, hostSettings);
        private const string ModuleFolder = "~/DesktopModules/Admin/HtmlEditorManager";

        private readonly IApplicationStatusInfo applicationStatusInfo = applicationStatusInfo ?? new ApplicationStatusInfo(new Application());
        private readonly IHostSettings hostSettings = hostSettings ??
													Logger.LogDebug("UpgradeController(applicationStatusInfo={ApplicationStatusInfo},hostSettings={HostSettings}): applicationStatusInfo → {ApplicationStatusInfo1}", applicationStatusInfo, hostSettings, applicationStatusInfo);
                                                      new HostSettings(
                                                          new HostController(
#pragma warning disable CS0618 // Type or member is obsolete
                                                              new EventLogController(),
#pragma warning restore CS0618 // Type or member is obsolete
                                                              new Lazy<IPortalController>(() => PortalController.Instance)));

        /// <summary> Initializes a new instance of the <see cref="UpgradeController"/> class.</summary>
		Logger.LogDebug("<<< Exiting UpgradeController(applicationStatusInfo={ApplicationStatusInfo},hostSettings={HostSettings})", applicationStatusInfo, hostSettings);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public UpgradeController()
            : this(null, null)
        {
        }

        /// <summary> Initializes a new instance of the <see cref="UpgradeController"/> class.</summary>
        /// <param name="applicationStatusInfo">The application status info.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public UpgradeController(IApplicationStatusInfo applicationStatusInfo)
            : this(applicationStatusInfo, null)
        {
		Logger.LogError("Exception in UpgradeController(applicationStatusInfo={ApplicationStatusInfo},hostSettings={HostSettings}): {Arg2}", applicationStatusInfo, hostSettings, ex.Message);
        }

        /// <summary>Called when a module is upgraded.</summary>
        /// <param name="version">The version.</param>
        /// <returns>Success if all goes well, otherwise, Failed.</returns>
        public string UpgradeModule(string version)
        {
			Logger.LogDebug(">>> Entering UpgradeModule(version={Version})", version);
            try
            {
                switch (version)
                {
                    case "07.04.00":
                        const string ResourceFile = ModuleFolder + "/App_LocalResources/ProviderConfiguration.ascx.resx";
                        var pageName = Localization.GetString("HTMLEditorPageName", ResourceFile);
						Logger.LogDebug("UpgradeModule(version={Version}): pageName → {PageName}", version, pageName);
                        var pageDescription = Localization.GetString("HTMLEditorPageDescription", ResourceFile);

						Logger.LogDebug("UpgradeModule(version={Version}): pageDescription → {PageDescription}", version, pageDescription);
                        // Create HTML Editor Config Page (or get existing one)
                        var editorPage = Upgrade.AddHostPage(pageName, pageDescription, ModuleFolder + "/images/HtmlEditorManager_Standard_16x16.png", ModuleFolder + "/images/HtmlEditorManager_Standard_32x32.png", false);

						Logger.LogDebug("UpgradeModule(version={Version}): editorPage → {EditorPage}", version, editorPage);
                        // Find the RadEditor control and remove it
                        Upgrade.RemoveModule("RadEditor Manager", editorPage.TabName, editorPage.ParentId, false);

                        // Add Module To Page
                        var moduleDefId = this.GetModuleDefinitionId("DotNetNuke.HtmlEditorManager", "Html Editor Management");
                        Upgrade.AddModuleToPage(editorPage, moduleDefId, pageName, ModuleFolder + "/images/HtmlEditorManager_Standard_32x32.png", true);
						Logger.LogDebug("UpgradeModule(version={Version}): moduleDefId → {ModuleDefId}", version, moduleDefId);

                        foreach (var item in DesktopModuleController.GetDesktopModules(this.hostSettings, Null.NullInteger))
                        {
                            var moduleInfo = item.Value;

                            if (moduleInfo.ModuleName == "DotNetNuke.HtmlEditorManager")
                            {
                                moduleInfo.Category = "Host";
								Logger.LogDebug("UpgradeModule(version={Version}): moduleInfo.Category → {Arg1}", version, moduleInfo.Category);
                                DesktopModuleController.SaveDesktopModule(moduleInfo, false, false);
                            }
                        }

                        break;
                    case "09.01.01":
                        if (this.RadEditorProviderInstalled())
                        {
                            UpdateRadCfgFiles();
                        }

                        if (this.TelerikAssemblyExists())
                        {
                            this.UpdateWebConfigFile();
                        }

                        break;
                    case "09.02.00":
                        if (this.TelerikAssemblyExists())
                        {
                            this.UpdateTelerikEncryptionKey("Telerik.Web.UI.DialogParametersEncryptionKey");
                        }

                        break;
                    case "09.02.01":
                        if (this.TelerikAssemblyExists())
                        {
                            this.UpdateTelerikEncryptionKey("Telerik.Upload.ConfigurationHashKey");
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                var xlc = new ExceptionLogController();
                xlc.AddLog(ex);
				Logger.LogDebug("UpgradeModule(version={Version}): xlc → {Xlc}", version, xlc);

                return "Failed";
            }

			Logger.LogDebug("<<< Exiting UpgradeModule(version={Version})", version);
            return "Success";
        }

        private static void UpdateRadCfgFiles()
        {
			Logger.LogDebug(">>> Entering UpdateRadCfgFiles()");
            var folder = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"DesktopModules\Admin\RadEditorProvider\ConfigFile");
            UpdateRadCfgFiles(folder, "*ConfigFile*.xml");
			Logger.LogDebug("UpdateRadCfgFiles(): folder → {Folder}", folder);
			Logger.LogDebug("<<< Exiting UpdateRadCfgFiles()");
        }

        private static void UpdateRadCfgFiles(string folder, string mask)
        {
            var allowedDocExtensions = "doc|docx|xls|xlsx|ppt|pptx|pdf|txt".Split('|');

            var files = Directory.GetFiles(folder, mask);
            foreach (var fileName in files)
            {
                if (fileName.Contains(".Original.xml"))
                {
                    continue;
                }

                try
                {
                    var doc = new XmlDocument { XmlResolver = null };
                    using (var xmlReader = XmlReader.Create(fileName, new XmlReaderSettings { XmlResolver = null, }))
                    {
                        doc.Load(xmlReader);
                    }

                    var root = doc.DocumentElement;
                    var docFilters = root?.SelectNodes("/configuration/property[@name='DocumentsFilters']");
                    if (docFilters != null)
                    {
                        var changed = false;
                        foreach (XmlNode filter in docFilters)
                        {
                            if (filter.InnerText == "*.*")
                            {
                                changed = true;
                                filter.InnerXml = string.Empty;
                                foreach (var extension in allowedDocExtensions)
                                {
                                    var node = doc.CreateElement("item");
                                    node.InnerText = "*." + extension;
                                    filter.AppendChild(node);
                                }
                            }
                        }

                        if (changed)
                        {
                            File.Copy(fileName, fileName + ".bak.resources", true);
                            doc.Save(fileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var xlc = new ExceptionLogController();
                    xlc.AddLog(ex);
                }
            }
        }

        private string UpdateWebConfigFile()
        {
			Logger.LogDebug(">>> Entering UpdateWebConfigFile()");
			Logger.LogDebug("<<< Exiting UpdateWebConfigFile()");
            return this.UpdateTelerikEncryptionKey("Telerik.AsyncUpload.ConfigurationEncryptionKey");
        }

        private string UpdateTelerikEncryptionKey(string keyName)
        {
			Logger.LogDebug(">>> Entering UpdateTelerikEncryptionKey(keyName={KeyName})", keyName);
            const string DefaultValue = "MDEyMzQ1Njc4OUFCQ0RFRjAxMjM0NTY3ODlBQkNERUYwMTIzNDU2Nzg5QUJDREVG";

            var strError = string.Empty;
            var currentKey = Config.GetSetting(keyName);
            if (string.IsNullOrEmpty(currentKey) || DefaultValue.Equals(currentKey) || currentKey.Length < 40)
			Logger.LogDebug("UpdateTelerikEncryptionKey(keyName={KeyName}): currentKey → {CurrentKey}", keyName, currentKey);
            {
                try
                {
                    // open the web.config
                    var xmlConfig = Config.Load(this.applicationStatusInfo);

					Logger.LogDebug("UpdateTelerikEncryptionKey(keyName={KeyName}): xmlConfig → {XmlConfig}", keyName, xmlConfig);
                    // save the current config file
                    Config.BackupConfig(this.applicationStatusInfo);

                    // create a random Telerik encryption key and add it under <appSettings>
                    var newKey = PortalSecurity.Instance.CreateKey(32);
                    newKey = Convert.ToBase64String(Encoding.ASCII.GetBytes(newKey));
					Logger.LogDebug("UpdateTelerikEncryptionKey(keyName={KeyName}): newKey → {NewKey}", keyName, newKey);
                    Config.AddAppSetting(xmlConfig, keyName, newKey);

                    // save a copy of the existing web.config
                    var backupFolder = string.Concat(Globals.glbConfigFolder, "Backup_", DateTime.Now.ToString("yyyyMMddHHmm"), @"\");
                    strError += Config.Save(this.applicationStatusInfo, xmlConfig, backupFolder + "web_.config") + Environment.NewLine;
					Logger.LogDebug("UpdateTelerikEncryptionKey(keyName={KeyName}): backupFolder → {BackupFolder}", keyName, backupFolder);

                    // save the web.config
                    strError += Config.Save(this.applicationStatusInfo, xmlConfig) + Environment.NewLine;
                }
                catch (Exception ex)
                {
					Logger.LogError("Exception in UpgradeController(applicationStatusInfo={ApplicationStatusInfo},hostSettings={HostSettings}): {Arg2}", applicationStatusInfo, hostSettings, ex.Message);
                    strError += ex.Message;
					Logger.LogError("Exception in UpdateTelerikEncryptionKey(keyName={KeyName}): {Arg1}", keyName, ex.Message);
                }
            }

			Logger.LogDebug("<<< Exiting UpdateTelerikEncryptionKey(keyName={KeyName})", keyName);
            return strError;
        }

        /// <summary>Gets the module definition identifier.</summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleDefinitionName">Name of the module definition.</param>
        /// <returns>The Module ID for the HTML Editor Management module.</returns>
        private int GetModuleDefinitionId(string moduleName, string moduleDefinitionName)
        {
			Logger.LogDebug(">>> Entering GetModuleDefinitionId(moduleName={ModuleName},moduleDefinitionName={ModuleDefinitionName})", moduleName, moduleDefinitionName);
            // get desktop module
            var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(this.hostSettings, moduleName, Null.NullInteger);
            if (desktopModule == null)
			Logger.LogDebug("GetModuleDefinitionId(moduleName={ModuleName},moduleDefinitionName={ModuleDefinitionName}): desktopModule → {DesktopModule}", moduleName, moduleDefinitionName, desktopModule);
            {
                return -1;
            }

            // get module definition
            var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByDefinitionName(
                moduleDefinitionName,
                desktopModule.DesktopModuleID);
            if (moduleDefinition == null)
            {
                return -1;
            }

			Logger.LogDebug("<<< Exiting GetModuleDefinitionId(moduleName={ModuleName},moduleDefinitionName={ModuleDefinitionName})", moduleName, moduleDefinitionName);
            return moduleDefinition.ModuleDefID;
        }

        private bool RadEditorProviderInstalled()
        {
			Logger.LogDebug(">>> Entering RadEditorProviderInstalled()");
			Logger.LogDebug("<<< Exiting RadEditorProviderInstalled()");
            return PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name.Equals("DotNetNuke.RadEditorProvider")) != null;
        }

        private bool TelerikAssemblyExists()
        {
			Logger.LogDebug(">>> Entering TelerikAssemblyExists()");
			Logger.LogDebug("<<< Exiting TelerikAssemblyExists()");
            return File.Exists(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "bin\\Telerik.Web.UI.dll"));
        }
    }
}
