// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Templates
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;

    internal class PortalTemplateExporter(IBusinessControllerProvider businessControllerProvider, ListController listController, IPortalController portalController, IHostSettings hostSettings, RoleProvider roleProvider, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController)
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalTemplateExporter));
        private readonly IBusinessControllerProvider businessControllerProvider = businessControllerProvider;
        private readonly ListController listController = listController;
        private readonly IPortalController portalController = portalController;
        private readonly IHostSettings hostSettings = hostSettings;
        private readonly RoleProvider roleProvider = roleProvider;
        private readonly IApplicationStatusInfo appStatus = appStatus;
        private readonly IPortalGroupController portalGroupController = portalGroupController;

        private static string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Sites/App_LocalResources/Sites.resx");

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal (bool Success, string Message) ExportPortalTemplate(int portalId, string fileName, string description, bool isMultiLanguage, IEnumerable<string> locales, string localizationCulture, IEnumerable<int> exportTabIds, bool includeContent, bool includeFiles, bool includeModules, bool includeProfile, bool includeRoles)
        {
			Logger.LogError(">>> Entering ExportPortalTemplate(portalId={PortalId},fileName={FileName},description={Description},isMultiLanguage={IsMultiLanguage},locales={Locales},localizationCulture={LocalizationCulture},exportTabIds={ExportTabIds},includeContent={IncludeContent},includeFiles={IncludeFiles},includeModules={IncludeModules},includeProfile={IncludeProfile},includeRoles={IncludeRoles})", portalId, fileName, description, isMultiLanguage, locales, localizationCulture, exportTabIds, includeContent, includeFiles, includeModules, includeProfile, includeRoles);
            if (!exportTabIds.Any())
            {
                return (false, Localization.GetString("ErrorPages", LocalResourcesFile));
            }

            var xmlSettings = new XmlWriterSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                OmitXmlDeclaration = true,
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
                WriteEndDocumentOnClose = true,
            };

            var filename = Globals.HostMapPath + fileName.Replace("/", @"\");
            if (!filename.EndsWith(".template", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".template";
				Logger.LogError("ExportPortalTemplate(portalId={PortalId},fileName={FileName},description={Description},isMultiLanguage={IsMultiLanguage},locales={Locales},localizationCulture={LocalizationCulture},exportTabIds={ExportTabIds},includeContent={IncludeContent},includeFiles={IncludeFiles},includeModules={IncludeModules},includeProfile={IncludeProfile},includeRoles={IncludeRoles}): filename → {Filename}", portalId, fileName, description, isMultiLanguage, locales, localizationCulture, exportTabIds, includeContent, includeFiles, includeModules, includeProfile, includeRoles, filename);
            }

            using (var writer = XmlWriter.Create(filename, xmlSettings))
            {
                writer.WriteStartElement("portal");
                writer.WriteAttributeString("version", "5.0");

                // Add template description
                writer.WriteElementString("description", HttpUtility.HtmlEncode(description));

                // Serialize portal settings
                var portal = PortalController.Instance.GetPortal(portalId);
				Logger.LogError("ExportPortalTemplate(portalId={PortalId},fileName={FileName},description={Description},isMultiLanguage={IsMultiLanguage},locales={Locales},localizationCulture={LocalizationCulture},exportTabIds={ExportTabIds},includeContent={IncludeContent},includeFiles={IncludeFiles},includeModules={IncludeModules},includeProfile={IncludeProfile},includeRoles={IncludeRoles}): portal → {Portal}", portalId, fileName, description, isMultiLanguage, locales, localizationCulture, exportTabIds, includeContent, includeFiles, includeModules, includeProfile, includeRoles, portal);

                SerializePortalSettings(writer, portal, isMultiLanguage);
                SerializeEnabledLocales(writer, portal, isMultiLanguage, locales);
                SerializeExtensionUrlProviders(writer, portalId);

                if (includeProfile)
                {
                    // Serialize Profile Definitions
                    this.SerializeProfileDefinitions(writer, portal);
                }

                if (includeModules)
                {
                    // Serialize Portal Desktop Modules
                    DesktopModuleController.SerializePortalDesktopModules(writer, portalId);
                }

                if (includeRoles)
                {
                    // Serialize Roles
                    RoleController.SerializeRoleGroups(this.roleProvider, writer, portalId);
                }

                // Serialize tabs
                this.SerializeTabs(writer, portal, isMultiLanguage, exportTabIds, includeContent, locales, localizationCulture);

                if (includeFiles)
                {
                    // Create Zip File to hold files
                    var resourcesFile = new ZipArchive(File.Create(filename + ".resources"), ZipArchiveMode.Create, true);
					Logger.LogError("ExportPortalTemplate(portalId={PortalId},fileName={FileName},description={Description},isMultiLanguage={IsMultiLanguage},locales={Locales},localizationCulture={LocalizationCulture},exportTabIds={ExportTabIds},includeContent={IncludeContent},includeFiles={IncludeFiles},includeModules={IncludeModules},includeProfile={IncludeProfile},includeRoles={IncludeRoles}): resourcesFile → {ResourcesFile}", portalId, fileName, description, isMultiLanguage, locales, localizationCulture, exportTabIds, includeContent, includeFiles, includeModules, includeProfile, includeRoles, resourcesFile);

                    // Serialize folders (while adding files to zip file)
                    SerializeFolders(writer, portal, ref resourcesFile);

                    // Finish and Close Zip file
                    resourcesFile.Dispose();
                }

                writer.WriteEndElement();
                writer.Close();
            }

            EventManager.Instance.OnPortalTemplateCreated(new PortalTemplateEventArgs()
            {
                PortalId = portalId,
                TemplatePath = filename,
            });

            return (true, string.Format(CultureInfo.CurrentCulture, Localization.GetString("ExportedMessage", LocalResourcesFile), filename));
        }

        private static TabCollection GetExportableTabs(TabCollection tabs)
        {
			Logger.LogError(">>> Entering GetExportableTabs(tabs={Tabs})", tabs);
            var exportableTabs = tabs.Where(kvp => !kvp.Value.IsSystem).Select(kvp => kvp.Value);
			Logger.LogError("GetExportableTabs(tabs={Tabs}): exportableTabs → {ExportableTabs}", tabs, exportableTabs);
			Logger.LogWarning("<<< Exiting GetExportableTabs(tabs={Tabs})", tabs);
            return new TabCollection(exportableTabs);
        }

        private static void SerializePortalSettings(XmlWriter writer, PortalInfo portal, bool isMultilanguage)
        {
			Logger.LogError(">>> Entering SerializePortalSettings(writer={Writer},portal={Portal},isMultilanguage={IsMultilanguage})", writer, portal, isMultilanguage);
            writer.WriteStartElement("settings");

            writer.WriteElementString("logofile", portal.LogoFile);
            writer.WriteElementString("footertext", portal.FooterText);
            writer.WriteElementString("userregistration", portal.UserRegistration.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("banneradvertising", portal.BannerAdvertising.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("defaultlanguage", portal.DefaultLanguage);

            var settingsDictionary = PortalController.Instance.GetPortalSettings(((IPortalInfo)portal).PortalId);
			Logger.LogError("SerializePortalSettings(writer={Writer},portal={Portal},isMultilanguage={IsMultilanguage}): settingsDictionary → {SettingsDictionary}", writer, portal, isMultilanguage, settingsDictionary);

            string setting;
            settingsDictionary.TryGetValue("DefaultPortalSkin", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("skinsrc", setting);
            }

            settingsDictionary.TryGetValue("DefaultAdminSkin", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("skinsrcadmin", setting);
            }

            settingsDictionary.TryGetValue("DefaultPortalContainer", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("containersrc", setting);
            }

            settingsDictionary.TryGetValue("DefaultAdminContainer", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("containersrcadmin", setting);
            }

            settingsDictionary.TryGetValue("EnableSkinWidgets", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("enableskinwidgets", setting);
            }

            settingsDictionary.TryGetValue("portalaliasmapping", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("portalaliasmapping", setting);
            }

            writer.WriteElementString("contentlocalizationenabled", isMultilanguage.ToString());

            settingsDictionary.TryGetValue("TimeZone", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("timezone", setting);
            }

            settingsDictionary.TryGetValue("EnablePopUps", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("enablepopups", setting);
            }

            settingsDictionary.TryGetValue("InlineEditorEnabled", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("inlineeditorenabled", setting);
            }

            settingsDictionary.TryGetValue("HideFoldersEnabled", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("hidefoldersenabled", setting);
            }

            settingsDictionary.TryGetValue("ControlPanelMode", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("controlpanelmode", setting);
            }

            settingsDictionary.TryGetValue("ControlPanelSecurity", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("controlpanelsecurity", setting);
            }

            settingsDictionary.TryGetValue("ControlPanelVisibility", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("controlpanelvisibility", setting);
            }

            writer.WriteElementString("hostspace", portal.HostSpace.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("userquota", portal.UserQuota.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("pagequota", portal.PageQuota.ToString(CultureInfo.InvariantCulture));

            settingsDictionary.TryGetValue("PageHeadText", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("pageheadtext", setting);
            }

            settingsDictionary.TryGetValue("InjectModuleHyperLink", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("injectmodulehyperlink", setting);
            }

            settingsDictionary.TryGetValue("AddCompatibleHttpHeader", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("addcompatiblehttpheader", setting);
            }

            settingsDictionary.TryGetValue("ShowCookieConsent", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("showcookieconsent", setting);
            }

            settingsDictionary.TryGetValue("CookieMoreLink", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("cookiemorelink", setting);
            }

            settingsDictionary.TryGetValue("DataConsentActive", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("dataconsentactive", setting);
            }

            settingsDictionary.TryGetValue("DataConsentTermsLastChange", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("dataconsenttermslastchange", setting);
            }

            settingsDictionary.TryGetValue("DataConsentConsentRedirect", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("dataconsentconsentredirect", setting);
            }

            settingsDictionary.TryGetValue("DataConsentUserDeleteAction", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("dataconsentuserdeleteaction", setting);
            }

            settingsDictionary.TryGetValue("DataConsentDelay", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("dataconsentdelay", setting);
            }

            settingsDictionary.TryGetValue("DataConsentDelayMeasurement", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("dataconsentdelaymeasurement", setting);
            }

            settingsDictionary.TryGetValue("ShowQuickModuleAddMenu", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("showquickmoduleaddmenu", setting);
            }

            settingsDictionary.TryGetValue("AllowJsInModuleHeaders", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("allowjsinmoduleheaders", setting);
            }

            settingsDictionary.TryGetValue("AllowJsInModuleFooters", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("allowjsinmodulefooters", setting);
            }

            settingsDictionary.TryGetValue("AllowUserUICulture", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("allowuseruiculture", setting);
            }

            settingsDictionary.TryGetValue("EnableBrowserLanguage", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("enablebrowserlanguage", setting);
            }

            settingsDictionary.TryGetValue("SitemapCacheDays", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("sitemapcachedays", setting);
            }

            settingsDictionary.TryGetValue("SitemapExcludePriority", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("sitemapexcludepriority", setting);
            }

            settingsDictionary.TryGetValue("SitemmpIncludeHidden", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("sitemapincludehidden", setting);
            }

            settingsDictionary.TryGetValue("SitemapLevelMode", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("sitemaplevelmode", setting);
            }

            settingsDictionary.TryGetValue("SitemapMinPriority", out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("sitemapminpriority", setting);
            }

            settingsDictionary.TryGetValue(PortalTemplateImporter.HtmlTextAutoSaveEnabled, out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("enableautosave", setting);
            }

            settingsDictionary.TryGetValue(PortalTemplateImporter.HtmlTextTimeToAutoSave, out setting);
            if (!string.IsNullOrEmpty(setting))
            {
                writer.WriteElementString("timetoautosave", setting);
            }

            // End Portal Settings
            writer.WriteEndElement();
			Logger.LogWarning("<<< Exiting SerializePortalSettings(writer={Writer},portal={Portal},isMultilanguage={IsMultilanguage})", writer, portal, isMultilanguage);
        }

        private static void SerializeEnabledLocales(XmlWriter writer, IPortalInfo portal, bool isMultilanguage, IEnumerable<string> locales)
        {
			Logger.LogError(">>> Entering SerializeEnabledLocales(writer={Writer},portal={Portal},isMultilanguage={IsMultilanguage},locales={Locales})", writer, portal, isMultilanguage, locales);
            var enabledLocales = LocaleController.Instance.GetLocales(portal.PortalId);
			Logger.LogError("SerializeEnabledLocales(writer={Writer},portal={Portal},isMultilanguage={IsMultilanguage},locales={Locales}): enabledLocales → {EnabledLocales}", writer, portal, isMultilanguage, locales, enabledLocales);
            if (enabledLocales.Count > 1)
            {
                writer.WriteStartElement("locales");
                if (isMultilanguage)
                {
                    foreach (var cultureCode in locales)
                    {
                        writer.WriteElementString("locale", cultureCode);
                    }
                }
                else
                {
                    foreach (var enabledLocale in enabledLocales)
                    {
                        writer.WriteElementString("locale", enabledLocale.Value.Code);
                    }
                }

                writer.WriteEndElement();
            }
			Logger.LogWarning("<<< Exiting SerializeEnabledLocales(writer={Writer},portal={Portal},isMultilanguage={IsMultilanguage},locales={Locales})", writer, portal, isMultilanguage, locales);
        }

        private static void SerializeExtensionUrlProviders(XmlWriter writer, int portalId)
        {
			Logger.LogError(">>> Entering SerializeExtensionUrlProviders(writer={Writer},portalId={PortalId})", writer, portalId);
            var providers = ExtensionUrlProviderController.GetModuleProviders(portalId);
			Logger.LogError("SerializeExtensionUrlProviders(writer={Writer},portalId={PortalId}): providers → {Providers}", writer, portalId, providers);
            if (providers.Count == 0)
            {
                return;
            }

            writer.WriteStartElement("extensionUrlProviders");

            foreach (var provider in providers)
            {
                writer.WriteStartElement("extensionUrlProvider");
                writer.WriteElementString("name", provider.ProviderConfig.ProviderName);
                writer.WriteElementString("active", provider.ProviderConfig.IsActive.ToString());
                var settings = provider.ProviderConfig.Settings;
                if (settings.Count != 0)
                {
                    writer.WriteStartElement("settings");
                    foreach (var setting in settings)
                    {
                        writer.WriteStartElement("setting");
                        writer.WriteAttributeString("name", setting.Key);
                        writer.WriteAttributeString("value", setting.Value);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                ////if (provider.ProviderConfig.TabIds.Any())
                ////{
                ////    writer.WriteStartElement("tabIds");
                ////    foreach (var tabId in provider.ProviderConfig.TabIds)
                ////    {
                ////        writer.WriteElementString("tabId", tabId.ToString(CultureInfo.InvariantCulture));
                ////    }
                ////    writer.WriteEndElement();
                ////}

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
			Logger.LogWarning("<<< Exiting SerializeExtensionUrlProviders(writer={Writer},portalId={PortalId})", writer, portalId);
        }

        private static void SerializeFolders(XmlWriter writer, IPortalInfo portal, ref ZipArchive zipFile)
        {
			Logger.LogError(">>> Entering SerializeFolders(writer={Writer},portal={Portal},zipFile={ZipFile})", writer, portal, zipFile);
            // Sync db and filesystem before exporting so all required files are found
            var folderManager = FolderManager.Instance;
            folderManager.Synchronize(portal.PortalId);
            writer.WriteStartElement("folders");

            foreach (var folder in folderManager.GetFolders(portal.PortalId))
            {
                writer.WriteStartElement("folder");

                writer.WriteElementString("folderpath", folder.FolderPath);
                writer.WriteElementString("storagelocation", folder.StorageLocation.ToString(CultureInfo.InvariantCulture));

                // Serialize Folder Permissions
                SerializeFolderPermissions(writer, portal, folder.FolderPath);

                // Serialize files
                SerializeFiles(writer, portal, folder.FolderPath, ref zipFile);

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
			Logger.LogWarning("<<< Exiting SerializeFolders(writer={Writer},portal={Portal},zipFile={ZipFile})", writer, portal, zipFile);
        }

        private static void SerializeFiles(XmlWriter writer, IPortalInfo portal, string folderPath, ref ZipArchive zipFile)
        {
			Logger.LogError(">>> Entering SerializeFiles(writer={Writer},portal={Portal},folderPath={FolderPath},zipFile={ZipFile})", writer, portal, folderPath, zipFile);
            var folderManager = FolderManager.Instance;
            var objFolder = folderManager.GetFolder(portal.PortalId, folderPath);
			Logger.LogError("SerializeFiles(writer={Writer},portal={Portal},folderPath={FolderPath},zipFile={ZipFile}): objFolder → {ObjFolder}", writer, portal, folderPath, zipFile, objFolder);

            writer.WriteStartElement("files");
            foreach (var fileInfo in folderManager.GetFiles(objFolder))
            {
                var objFile = (Services.FileSystem.FileInfo)fileInfo;
				Logger.LogError("SerializeFiles(writer={Writer},portal={Portal},folderPath={FolderPath},zipFile={ZipFile}): objFile → {ObjFile}", writer, portal, folderPath, zipFile, objFile);

                // verify that the file exists on the file system
                var filePath = portal.HomeDirectoryMapPath + folderPath + GetActualFileName(objFile);
				Logger.LogError("SerializeFiles(writer={Writer},portal={Portal},folderPath={FolderPath},zipFile={ZipFile}): filePath → {FilePath}", writer, portal, folderPath, zipFile, filePath);
                if (File.Exists(filePath))
                {
                    writer.WriteStartElement("file");

                    writer.WriteElementString("contenttype", objFile.ContentType);
                    writer.WriteElementString("extension", objFile.Extension);
                    writer.WriteElementString("filename", objFile.FileName);
                    writer.WriteElementString("height", objFile.Height.ToString(CultureInfo.InvariantCulture));
                    writer.WriteElementString("size", objFile.Size.ToString(CultureInfo.InvariantCulture));
                    writer.WriteElementString("width", objFile.Width.ToString(CultureInfo.InvariantCulture));

                    writer.WriteEndElement();

                    FileSystemUtils.AddToZip(
                        zipFile: ref zipFile,
                        filePath: filePath,
                        fileName: GetActualFileName(objFile),
                        folder: folderPath);
                }
            }

            writer.WriteEndElement();
			Logger.LogWarning("<<< Exiting SerializeFiles(writer={Writer},portal={Portal},folderPath={FolderPath},zipFile={ZipFile})", writer, portal, folderPath, zipFile);
        }

        private static string GetActualFileName(Services.FileSystem.FileInfo objFile)
        {
			Logger.LogError(">>> Entering GetActualFileName(objFile={ObjFile})", objFile);
            Logger.LogWarning("<<< Exiting GetActualFileName(objFile={ObjFile})", objFile);
            return (objFile.StorageLocation == (int)FolderController.StorageLocationTypes.SecureFileSystem)
                ? objFile.FileName + Globals.glbProtectedExtension
                : objFile.FileName;
        }

        private static void SerializeFolderPermissions(XmlWriter writer, IPortalInfo portal, string folderPath)
        {
			Logger.LogError(">>> Entering SerializeFolderPermissions(writer={Writer},portal={Portal},folderPath={FolderPath})", writer, portal, folderPath);
            var permissions = FolderPermissionController.GetFolderPermissionsCollectionByFolder(portal.PortalId, folderPath);

			Logger.LogError("SerializeFolderPermissions(writer={Writer},portal={Portal},folderPath={FolderPath}): permissions → {Permissions}", writer, portal, folderPath, permissions);
            writer.WriteStartElement("folderpermissions");

            foreach (FolderPermissionInfo permission in permissions)
            {
                writer.WriteStartElement("permission");

                writer.WriteElementString("permissioncode", permission.PermissionCode);
                writer.WriteElementString("permissionkey", permission.PermissionKey);
                writer.WriteElementString("rolename", permission.RoleName);
                writer.WriteElementString("allowaccess", permission.AllowAccess.ToString().ToLowerInvariant());

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
			Logger.LogWarning("<<< Exiting SerializeFolderPermissions(writer={Writer},portal={Portal},folderPath={FolderPath})", writer, portal, folderPath);
        }

        private void SerializeProfileDefinitions(XmlWriter writer, IPortalInfo portal)
        {
			Logger.LogError(">>> Entering SerializeProfileDefinitions(writer={Writer},portal={Portal})", writer, portal);
            writer.WriteStartElement("profiledefinitions");
            foreach (ProfilePropertyDefinition objProfileProperty in
                ProfileController.GetPropertyDefinitionsByPortal(this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, portal.PortalId, false, false))
            {
                writer.WriteStartElement("profiledefinition");

                writer.WriteElementString("propertycategory", objProfileProperty.PropertyCategory);
                writer.WriteElementString("propertyname", objProfileProperty.PropertyName);

                var objList = this.listController.GetListEntryInfo("DataType", objProfileProperty.DataType);
				Logger.LogError("SerializeProfileDefinitions(writer={Writer},portal={Portal}): objList → {ObjList}", writer, portal, objList);
                writer.WriteElementString("datatype", objList == null ? "Unknown" : objList.Value);
                writer.WriteElementString("length", objProfileProperty.Length.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("defaultvisibility", ((int)objProfileProperty.DefaultVisibility).ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
			Logger.LogWarning("<<< Exiting SerializeProfileDefinitions(writer={Writer},portal={Portal})", writer, portal);
        }

        private void SerializeTabs(XmlWriter writer, PortalInfo portal, bool isMultilanguage, IEnumerable<int> tabsToExport, bool includeContent, IEnumerable<string> locales, string localizationCulture = "")
        {
			Logger.LogError(">>> Entering SerializeTabs(writer={Writer},portal={Portal},isMultilanguage={IsMultilanguage},tabsToExport={TabsToExport},includeContent={IncludeContent},locales={Locales},localizationCulture={LocalizationCulture})", writer, portal, isMultilanguage, tabsToExport, includeContent, locales, localizationCulture);
            // supporting object to build the tab hierarchy
            var tabs = new Hashtable();
			Logger.LogError("SerializeTabs(writer={Writer},portal={Portal},isMultilanguage={IsMultilanguage},tabsToExport={TabsToExport},includeContent={IncludeContent},locales={Locales},localizationCulture={LocalizationCulture}): tabs → {Tabs}", writer, portal, isMultilanguage, tabsToExport, includeContent, locales, localizationCulture, tabs);

            writer.WriteStartElement("tabs");

            if (isMultilanguage)
            {
                // Process Default Language first
                this.SerializeTabs(
                    writer,
                    portal,
                    tabs,
                    GetExportableTabs(TabController.Instance.GetTabsByPortal(((IPortalInfo)portal).PortalId).WithCulture(portal.DefaultLanguage, true)),
                    tabsToExport,
                    includeContent);

                // Process other locales
                foreach (var cultureCode in locales)
                {
                    if (cultureCode != portal.DefaultLanguage)
                    {
                        this.SerializeTabs(
                            writer,
                            portal,
                            tabs,
                            GetExportableTabs(TabController.Instance.GetTabsByPortal(((IPortalInfo)portal).PortalId).WithCulture(cultureCode, false)),
                            tabsToExport,
                            includeContent);
                    }
                }
            }
            else
            {
                if (this.portalController.GetPortalSettings(((IPortalInfo)portal).PortalId).TryGetValue("ContentLocalizationEnabled", out var contentLocalizable) && Convert.ToBoolean(contentLocalizable))
                {
                    this.SerializeTabs(
                        writer,
                        portal,
                        tabs,
                        GetExportableTabs(TabController.Instance.GetTabsByPortal(((IPortalInfo)portal).PortalId).WithCulture(localizationCulture, true)),
                        tabsToExport,
                        includeContent);
                }
                else
                {
                    this.SerializeTabs(
                        writer,
                        portal,
                        tabs,
                        GetExportableTabs(TabController.Instance.GetTabsByPortal(((IPortalInfo)portal).PortalId)),
                        tabsToExport,
                        includeContent);
                }
            }

            writer.WriteEndElement();
			Logger.LogWarning("<<< Exiting SerializeTabs(writer={Writer},portal={Portal},isMultilanguage={IsMultilanguage},tabsToExport={TabsToExport},includeContent={IncludeContent},locales={Locales},localizationCulture={LocalizationCulture})", writer, portal, isMultilanguage, tabsToExport, includeContent, locales, localizationCulture);
        }

        private void SerializeTabs(XmlWriter writer, PortalInfo portal, Hashtable tabs, TabCollection tabCollection, IEnumerable<int> tabsToExport, bool chkContent)
        {
			Logger.LogError(">>> Entering SerializeTabs(writer={Writer},portal={Portal},tabs={Tabs},tabCollection={TabCollection},tabsToExport={TabsToExport},chkContent={ChkContent})", writer, portal, tabs, tabCollection, tabsToExport, chkContent);
            tabsToExport = tabsToExport.ToList();
			Logger.LogError("SerializeTabs(writer={Writer},portal={Portal},tabs={Tabs},tabCollection={TabCollection},tabsToExport={TabsToExport},chkContent={ChkContent}): tabsToExport → {TabsToExport1}", writer, portal, tabs, tabCollection, tabsToExport, chkContent, tabsToExport);
            foreach (var tab in tabCollection.Values.OrderBy(x => x.Level))
            {
				Logger.LogWarning("<<< Exiting SerializeTabs(writer={Writer},portal={Portal},isMultilanguage={IsMultilanguage},tabsToExport={TabsToExport},includeContent={IncludeContent},locales={Locales},localizationCulture={LocalizationCulture})", writer, portal, isMultilanguage, tabsToExport, includeContent, locales, localizationCulture);
                // if not deleted
                if (!tab.IsDeleted)
                {
                    XmlNode tabNode = null;
					Logger.LogError("SerializeTabs(writer={Writer},portal={Portal},tabs={Tabs},tabCollection={TabCollection},tabsToExport={TabsToExport},chkContent={ChkContent}): tabNode → {TabNode}", writer, portal, tabs, tabCollection, tabsToExport, chkContent, tabNode);
                    if (string.IsNullOrEmpty(tab.CultureCode) || tab.CultureCode == portal.DefaultLanguage)
                    {
                        // page in default culture and checked or page doesn't exist in tree(which should always export).
                        var tabId = tab.TabID;
                        if (tabsToExport.Any(p => p == tabId) ||
                            tabsToExport.All(p => p != tabId))
                        {
                            tabNode = TabController.SerializeTab(this.businessControllerProvider, new XmlDocument { XmlResolver = null }, tabs, tab, portal, chkContent);
                        }
                    }
                    else
                    {
                        // check if default culture page is selected or default page doesn't exist in tree(which should always export).
                        var defaultTab = tab.DefaultLanguageTab;
                        if (defaultTab == null
                            || tabsToExport.All(p => p != defaultTab.TabID)
                            || tabsToExport.Any(p => p == defaultTab.TabID))
                        {
                            tabNode = TabController.SerializeTab(this.businessControllerProvider, new XmlDocument { XmlResolver = null, }, tabs, tab, portal, chkContent);
                        }
                    }

                    tabNode?.WriteTo(writer);
                }
            }
        }
    }
}
