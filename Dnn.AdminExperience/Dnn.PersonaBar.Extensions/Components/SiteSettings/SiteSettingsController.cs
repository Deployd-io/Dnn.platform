// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteSettings.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Installer.Writers;
    using DotNetNuke.Services.Localization;

    using Constants = Dnn.PersonaBar.Library.Constants;

    public class SiteSettingsController
    {
        private Dictionary<string, InstallFile> files;
        private string manifest = Null.NullString;

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected string BasePath => HttpContext.Current.Server.MapPath("~/Install/Language");

        public void SaveLocalizedKeys(int portalId, string propertyName, string propertyCategory, string cultureCode, string propertyNameString, string propertyHelpString, string propertyRequiredString, string propertyValidationString, string categoryNameString)
        {
			eventLogger.LogError(">>> Entering SaveLocalizedKeys(portalId={PortalId},propertyName={PropertyName},propertyCategory={PropertyCategory},cultureCode={CultureCode},propertyNameString={PropertyNameString},propertyHelpString={PropertyHelpString},propertyRequiredString={PropertyRequiredString},propertyValidationString={PropertyValidationString},categoryNameString={CategoryNameString})", portalId, propertyName, propertyCategory, cultureCode, propertyNameString, propertyHelpString, propertyRequiredString, propertyValidationString, categoryNameString);
            var portalResources = new XmlDocument { XmlResolver = null };
            var defaultResources = new XmlDocument { XmlResolver = null };
            XmlNode parent;

            using (var defaultResourcesReader = XmlReader.Create(this.GetResourceFile(string.Empty, Localization.SystemLocale, portalId), new XmlReaderSettings { XmlResolver = null, }))
            {
                defaultResources.Load(defaultResourcesReader);
            }

            string filename = this.GetResourceFile("Portal", cultureCode, portalId);

            if (File.Exists(filename))
            {
                using var portalResourcesReader = XmlReader.Create(filename, new XmlReaderSettings { XmlResolver = null, });
				eventLogger.LogError("SaveLocalizedKeys(portalId={PortalId},propertyName={PropertyName},propertyCategory={PropertyCategory},cultureCode={CultureCode},propertyNameString={PropertyNameString},propertyHelpString={PropertyHelpString},propertyRequiredString={PropertyRequiredString},propertyValidationString={PropertyValidationString},categoryNameString={CategoryNameString}): portalResourcesReader → {PortalResourcesReader}", portalId, propertyName, propertyCategory, cultureCode, propertyNameString, propertyHelpString, propertyRequiredString, propertyValidationString, categoryNameString, portalResourcesReader);
                portalResources.Load(portalResourcesReader);
            }
            else
            {
                using var portalResourcesReader = XmlReader.Create(this.GetResourceFile(string.Empty, Localization.SystemLocale, portalId), new XmlReaderSettings { XmlResolver = null, });
                portalResources.Load(portalResourcesReader);
            }

            UpdateResourceFileNode(portalResources, "ProfileProperties_" + propertyName + ".Text", propertyNameString);
            UpdateResourceFileNode(portalResources, "ProfileProperties_" + propertyName + ".Help", propertyHelpString);
            UpdateResourceFileNode(portalResources, "ProfileProperties_" + propertyName + ".Required", propertyRequiredString);
            UpdateResourceFileNode(portalResources, "ProfileProperties_" + propertyName + ".Validation", propertyValidationString);
            UpdateResourceFileNode(portalResources, "ProfileProperties_" + propertyCategory + ".Header", categoryNameString);

            // remove unmodified keys
            foreach (XmlNode node in portalResources.SelectNodes("//root/data"))
            {
                XmlNode defaultNode = defaultResources.SelectSingleNode("//root/data[@name='" + node.Attributes["name"].Value + "']");
				eventLogger.LogError("SaveLocalizedKeys(portalId={PortalId},propertyName={PropertyName},propertyCategory={PropertyCategory},cultureCode={CultureCode},propertyNameString={PropertyNameString},propertyHelpString={PropertyHelpString},propertyRequiredString={PropertyRequiredString},propertyValidationString={PropertyValidationString},categoryNameString={CategoryNameString}): defaultNode → {DefaultNode}", portalId, propertyName, propertyCategory, cultureCode, propertyNameString, propertyHelpString, propertyRequiredString, propertyValidationString, categoryNameString, defaultNode);
                if (defaultNode != null && defaultNode.InnerXml == node.InnerXml)
                {
                    parent = node.ParentNode;
					eventLogger.LogError("SaveLocalizedKeys(portalId={PortalId},propertyName={PropertyName},propertyCategory={PropertyCategory},cultureCode={CultureCode},propertyNameString={PropertyNameString},propertyHelpString={PropertyHelpString},propertyRequiredString={PropertyRequiredString},propertyValidationString={PropertyValidationString},categoryNameString={CategoryNameString}): parent → {Parent}", portalId, propertyName, propertyCategory, cultureCode, propertyNameString, propertyHelpString, propertyRequiredString, propertyValidationString, categoryNameString, parent);
					eventLogger.LogDebug("SaveLocalizedKeys(portalId={PortalId},propertyName={PropertyName},propertyCategory={PropertyCategory},cultureCode={CultureCode},propertyNameString={PropertyNameString},propertyHelpString={PropertyHelpString},propertyRequiredString={PropertyRequiredString},propertyValidationString={PropertyValidationString},categoryNameString={CategoryNameString}): parent → {Parent}", portalId, propertyName, propertyCategory, cultureCode, propertyNameString, propertyHelpString, propertyRequiredString, propertyValidationString, categoryNameString, parent);
                    parent.RemoveChild(node);
                }
            }

            // remove duplicate keys
            foreach (XmlNode node in portalResources.SelectNodes("//root/data"))
            {
                if (portalResources.SelectNodes("//root/data[@name='" + node.Attributes["name"].Value + "']").Count > 1)
                {
                    parent = node.ParentNode;
                    parent.RemoveChild(node);
                }
            }

            if (portalResources.SelectNodes("//root/data").Count > 0)
            {
                // there's something to save
                portalResources.Save(filename);
            }
            else
            {
                // nothing to be saved, if file exists delete
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
			eventLogger.LogWarning("<<< Exiting SaveLocalizedKeys(portalId={PortalId},propertyName={PropertyName},propertyCategory={PropertyCategory},cultureCode={CultureCode},propertyNameString={PropertyNameString},propertyHelpString={PropertyHelpString},propertyRequiredString={PropertyRequiredString},propertyValidationString={PropertyValidationString},categoryNameString={CategoryNameString})", portalId, propertyName, propertyCategory, cultureCode, propertyNameString, propertyHelpString, propertyRequiredString, propertyValidationString, categoryNameString);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IList<string> GetAvailableAnalyzers()
        {
			eventLogger.LogError(">>> Entering GetAvailableAnalyzers()");
            var analyzers = new List<string>();
			eventLogger.LogError("GetAvailableAnalyzers(): analyzers → {Analyzers}", analyzers);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    analyzers.AddRange(from t in assembly.GetTypes() where IsAnalyzerType(t) && IsAllowType(t) select $"{t.FullName}, {assembly.GetName().Name}");
                }
                catch (Exception)
                {
					eventLogger.LogError("Exception in GetAvailableAnalyzers(): {Arg0}", Exception.Message);
                    // do nothing but just ignore the error.
                }
            }

			eventLogger.LogWarning("<<< Exiting GetAvailableAnalyzers()");
            return analyzers;
        }

        public bool CreateAuthSystemPackage(string cultureCode, PackageInfo authPackage, bool createZip)
        {
			eventLogger.LogError(">>> Entering CreateAuthSystemPackage(cultureCode={CultureCode},authPackage={AuthPackage},createZip={CreateZip})", cultureCode, authPackage, createZip);
            var package = new PackageInfo
            {
                Name = authPackage.Name,
                FriendlyName = authPackage.FriendlyName,
                Version = authPackage.Version,
                License = Util.PACKAGE_NoLicense,
            };
            var fileName = Path.Combine(this.BasePath, "ResourcePack." + package.Name);
			eventLogger.LogError("CreateAuthSystemPackage(cultureCode={CultureCode},authPackage={AuthPackage},createZip={CreateZip}): fileName → {FileName}", cultureCode, authPackage, createZip, fileName);
            var authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(authPackage.PackageID);
			eventLogger.LogError("CreateAuthSystemPackage(cultureCode={CultureCode},authPackage={AuthPackage},createZip={CreateZip}): authSystem → {AuthSystem}", cultureCode, authPackage, createZip, authSystem);
            var authPath = authSystem.LoginControlSrc.Substring(0, authSystem.LoginControlSrc.LastIndexOf("/", StringComparison.Ordinal));
			eventLogger.LogError("CreateAuthSystemPackage(cultureCode={CultureCode},authPackage={AuthPackage},createZip={CreateZip}): authPath → {AuthPath}", cultureCode, authPackage, createZip, authPath);
            return this.CreatePackage(cultureCode, package, authPackage.PackageID, authPath.Replace("/", @"\"), fileName, createZip);
        }

        public bool CreateCorePackage(string cultureCode, string fileName, bool createZip)
        {
			eventLogger.LogError(">>> Entering CreateCorePackage(cultureCode={CultureCode},fileName={FileName},createZip={CreateZip})", cultureCode, fileName, createZip);
            var package = new PackageInfo { Name = Globals.CleanFileName(fileName) };
            package.FriendlyName = package.Name;
			eventLogger.LogDebug("CreateCorePackage(cultureCode={CultureCode},fileName={FileName},createZip={CreateZip}): package.FriendlyName → {Arg3}", cultureCode, fileName, createZip, package.FriendlyName);
			eventLogger.LogDebug("CreateCorePackage(cultureCode={CultureCode},fileName={FileName},createZip={CreateZip}): package.Version → {Arg3}", cultureCode, fileName, createZip, package.Version);
			eventLogger.LogDebug("CreateCorePackage(cultureCode={CultureCode},fileName={FileName},createZip={CreateZip}): package.License → {Arg3}", cultureCode, fileName, createZip, package.License);
			eventLogger.LogError("CreateCorePackage(cultureCode={CultureCode},fileName={FileName},createZip={CreateZip}): package → {Package}", cultureCode, fileName, createZip, package);
			eventLogger.LogError("CreateCorePackage(cultureCode={CultureCode},fileName={FileName},createZip={CreateZip}): fileName → {FileName1}", cultureCode, fileName, createZip, fileName);
            package.Version = DotNetNukeContext.Current.Application.Version;
            package.License = Util.PACKAGE_NoLicense;

            fileName = Path.Combine(this.BasePath, "ResourcePack." + package.Name);

			eventLogger.LogWarning("<<< Exiting CreateCorePackage(cultureCode={CultureCode},fileName={FileName},createZip={CreateZip})", cultureCode, fileName, createZip);
            return this.CreatePackage(cultureCode, package, -2, string.Empty, fileName, createZip);
        }

        public void CreateFullPackage(string cultureCode, string fileName)
        {
			eventLogger.LogError(">>> Entering CreateFullPackage(cultureCode={CultureCode},fileName={FileName})", cultureCode, fileName);
            Locale language = LocaleController.Instance.GetLocale(cultureCode);
			eventLogger.LogError("CreateFullPackage(cultureCode={CultureCode},fileName={FileName}): language → {Language}", cultureCode, fileName, language);
            var package = new PackageInfo
            {
                Name = Globals.CleanFileName(fileName),
                Version = DotNetNukeContext.Current.Application.Version,
                License = Util.PACKAGE_NoLicense,
                PackageType = "CoreLanguagePack",
            };

            this.files = new Dictionary<string, InstallFile>();
			eventLogger.LogDebug("CreateFullPackage(cultureCode={CultureCode},fileName={FileName}): files → {Files}", cultureCode, fileName, files);
            this.CreateCorePackage(cultureCode, fileName, false);
            foreach (var desktopModule in DesktopModuleController.GetDesktopModules(Null.NullInteger).Values.Where(desktopModule => !desktopModule.FolderName.StartsWith("Admin/", StringComparison.OrdinalIgnoreCase)))
            {
                this.CreateModulePackage(cultureCode, desktopModule, false);
            }

            foreach (var provider in PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "Provider"))
            {
                this.CreateProviderPackage(cultureCode, provider, false);
            }

            foreach (var authSystem in PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "Auth_System"))
            {
                this.CreateAuthSystemPackage(cultureCode, authSystem, false);
            }

            foreach (var library in PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "Library" || p.PackageType == "EvoqConnector"))
            {
                // only generate if a folder name is known for the library
                if (library.FolderName != null)
                {
                    this.CreateLibraryPackage(cultureCode, library, false);
                }
            }

            fileName = Path.Combine(this.BasePath, "ResourcePack." + package.Name);
			eventLogger.LogError("CreateFullPackage(cultureCode={CultureCode},fileName={FileName}): fileName → {FileName1}", cultureCode, fileName, fileName);
            fileName = fileName + "." + package.Version.ToString(3) + "." + language.Code + ".zip";

            var packageWriter = PackageWriterFactory.GetWriter(package) as LanguagePackWriter;
			eventLogger.LogError("CreateFullPackage(cultureCode={CultureCode},fileName={FileName}): packageWriter → {PackageWriter}", cultureCode, fileName, packageWriter);
			eventLogger.LogDebug("CreateFullPackage(cultureCode={CultureCode},fileName={FileName}): packageWriter.Language → {Arg2}", cultureCode, fileName, packageWriter.Language);
			eventLogger.LogDebug("CreateFullPackage(cultureCode={CultureCode},fileName={FileName}): packageWriter.BasePath → {Arg2}", cultureCode, fileName, packageWriter.BasePath);
            packageWriter.Language = language;
            packageWriter.BasePath = string.Empty;
            foreach (KeyValuePair<string, InstallFile> kvp in this.files)
            {
                packageWriter.Files.Add(kvp.Key, kvp.Value);
            }

            packageWriter.CreatePackage(fileName, package.Name + " " + language.Text + ".dnn", this.manifest, true);
			eventLogger.LogWarning("<<< Exiting CreateFullPackage(cultureCode={CultureCode},fileName={FileName})", cultureCode, fileName);
        }

        public bool CreateLibraryPackage(string cultureCode, PackageInfo library, bool createZip)
        {
			eventLogger.LogError(">>> Entering CreateLibraryPackage(cultureCode={CultureCode},library={Library},createZip={CreateZip})", cultureCode, library, createZip);
            var package = new PackageInfo
            {
                Name = library.Name,
                FriendlyName = library.FriendlyName,
                Version = library.Version,
                License = Util.PACKAGE_NoLicense,
            };

            var fileName = Path.Combine(this.BasePath, "ResourcePack" + package.Name);
			eventLogger.LogError("CreateLibraryPackage(cultureCode={CultureCode},library={Library},createZip={CreateZip}): fileName → {FileName}", cultureCode, library, createZip, fileName);
			eventLogger.LogWarning("<<< Exiting CreateLibraryPackage(cultureCode={CultureCode},library={Library},createZip={CreateZip})", cultureCode, library, createZip);
            return this.CreatePackage(cultureCode, package, library.PackageID, library.FolderName, fileName, createZip);
        }

        public bool CreateModulePackage(string cultureCode, DesktopModuleInfo desktopModule, bool createZip)
        {
			eventLogger.LogError(">>> Entering CreateModulePackage(cultureCode={CultureCode},desktopModule={DesktopModule},createZip={CreateZip})", cultureCode, desktopModule, createZip);
            var modulePackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == desktopModule.PackageID);

			eventLogger.LogError("CreateModulePackage(cultureCode={CultureCode},desktopModule={DesktopModule},createZip={CreateZip}): modulePackage → {ModulePackage}", cultureCode, desktopModule, createZip, modulePackage);
            var package = new PackageInfo
            {
                Name = modulePackage.Name,
                FriendlyName = modulePackage.FriendlyName,
                Version = modulePackage.Version,
                License = Util.PACKAGE_NoLicense,
            };

            var fileName = Path.Combine(this.BasePath, "ResourcePack." + package.Name);
			eventLogger.LogError("CreateModulePackage(cultureCode={CultureCode},desktopModule={DesktopModule},createZip={CreateZip}): fileName → {FileName}", cultureCode, desktopModule, createZip, fileName);
			eventLogger.LogWarning("<<< Exiting CreateModulePackage(cultureCode={CultureCode},desktopModule={DesktopModule},createZip={CreateZip})", cultureCode, desktopModule, createZip);
            return this.CreatePackage(cultureCode, package, modulePackage.PackageID, Path.Combine("DesktopModules\\", desktopModule.FolderName), fileName, createZip);
        }

        public bool CreateProviderPackage(string cultureCode, PackageInfo providerPackage, bool createZip)
        {
			eventLogger.LogError(">>> Entering CreateProviderPackage(cultureCode={CultureCode},providerPackage={ProviderPackage},createZip={CreateZip})", cultureCode, providerPackage, createZip);
            var package = new PackageInfo
            {
                Name = providerPackage.Name,
                FriendlyName = providerPackage.FriendlyName,
                Version = providerPackage.Version,
                License = Util.PACKAGE_NoLicense,
            };

            var fileName = Path.Combine(this.BasePath, "ResourcePack." + package.Name);
			eventLogger.LogError("CreateProviderPackage(cultureCode={CultureCode},providerPackage={ProviderPackage},createZip={CreateZip}): fileName → {FileName}", cultureCode, providerPackage, createZip, fileName);

            // Get the provider "path"
            XmlDocument configDoc = Config.Load();
			eventLogger.LogError("CreateProviderPackage(cultureCode={CultureCode},providerPackage={ProviderPackage},createZip={CreateZip}): configDoc → {ConfigDoc}", cultureCode, providerPackage, createZip, configDoc);
            string providerName = package.Name;
			eventLogger.LogError("CreateProviderPackage(cultureCode={CultureCode},providerPackage={ProviderPackage},createZip={CreateZip}): providerName → {ProviderName}", cultureCode, providerPackage, createZip, providerName);
            if (providerName.IndexOf(".", StringComparison.Ordinal) > Null.NullInteger)
            {
                providerName = providerName.Substring(providerName.IndexOf(".", StringComparison.Ordinal) + 1);
            }

            switch (providerName)
            {
                case "SchedulingProvider":
                    providerName = "DNNScheduler";
                    break;
                case "SearchIndexProvider":
                    providerName = "ModuleIndexProvider";
                    break;
                case "SearchProvider":
                    providerName = "SearchDataStoreProvider";
                    break;
            }

            var providerNavigator = configDoc.CreateNavigator()?.SelectSingleNode(XmlUtils.CreateXPathExpression("/configuration/dotnetnuke/*/providers/add[@name=$providerName]", new KeyValuePair<string, object>("providerName", providerName))) ??
                                    configDoc.CreateNavigator()?.SelectSingleNode(XmlUtils.CreateXPathExpression("/configuration/dotnetnuke/*/providers/add[@name=$packageName]", new KeyValuePair<string, object>("packageName", package.Name)));

            if (providerNavigator != null)
            {
                string providerPath = providerNavigator.GetAttribute("providerPath", string.Empty);
                return this.CreatePackage(
                    cultureCode,
                    package,
                    providerPackage.PackageID,
                    providerPath.Substring(2).Replace("/", @"\"),
                    fileName,
                    createZip);
            }
            else
            {
				eventLogger.LogWarning("<<< Exiting CreateProviderPackage(cultureCode={CultureCode},providerPackage={ProviderPackage},createZip={CreateZip})", cultureCode, providerPackage, createZip);
                return false;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string GetResourceFile(string type, string language, int portalId)
        {
			eventLogger.LogError(">>> Entering GetResourceFile(type={Type},language={Language},portalId={PortalId})", type, language, portalId);
            string resourceFilename = "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx";
			eventLogger.LogError("GetResourceFile(type={Type},language={Language},portalId={PortalId}): resourceFilename → {ResourceFilename}", type, language, portalId, resourceFilename);
            if (language != Localization.SystemLocale)
            {
                resourceFilename = resourceFilename + "." + language;
            }

            if (type == "Portal")
            {
                resourceFilename = resourceFilename + "." + "Portal-" + portalId;
            }
            else if (type == "Host")
            {
                resourceFilename = resourceFilename + "." + "Host";
            }

			eventLogger.LogWarning("<<< Exiting GetResourceFile(type={Type},language={Language},portalId={PortalId})", type, language, portalId);
            return HttpContext.Current.Server.MapPath(resourceFilename + ".resx");
        }

        private static bool IsAnalyzerType(Type type)
        {
			eventLogger.LogError(">>> Entering IsAnalyzerType(type={Type})", type);
            eventLogger.LogWarning("<<< Exiting IsAnalyzerType(type={Type})", type);
            return type is { FullName: not null } && (type.FullName.Contains("Lucene.Net.Analysis.Analyzer") || IsAnalyzerType(type.BaseType));
        }

        private static bool IsAllowType(Type type)
        {
			eventLogger.LogError(">>> Entering IsAllowType(type={Type})", type);
            eventLogger.LogWarning("<<< Exiting IsAllowType(type={Type})", type);
            return !type.FullName.Contains("Lucene.Net.Analysis.Analyzer") && !type.FullName.Contains("DotNetNuke");
        }

        private static void UpdateResourceFileNode(XmlDocument xmlDoc, string key, string text)
        {
			eventLogger.LogError(">>> Entering UpdateResourceFileNode(xmlDoc={XmlDoc},key={Key},text={Text})", xmlDoc, key, text);
            var node = xmlDoc.CreateNavigator()?.SelectSingleNode(XmlUtils.CreateXPathExpression("//root/data[@name='$key']/value", new KeyValuePair<string, object>("key", key)));
			eventLogger.LogError("UpdateResourceFileNode(xmlDoc={XmlDoc},key={Key},text={Text}): node → {Node}", xmlDoc, key, text, node);
            if (node == null)
            {
                // missing entry
                var nodeData = xmlDoc.CreateElement("data");
				eventLogger.LogError("UpdateResourceFileNode(xmlDoc={XmlDoc},key={Key},text={Text}): nodeData → {NodeData}", xmlDoc, key, text, nodeData);
                var attr = xmlDoc.CreateAttribute("name");
				eventLogger.LogError("UpdateResourceFileNode(xmlDoc={XmlDoc},key={Key},text={Text}): attr → {Attr}", xmlDoc, key, text, attr);
				eventLogger.LogDebug("UpdateResourceFileNode(xmlDoc={XmlDoc},key={Key},text={Text}): attr.Value → {Arg3}", xmlDoc, key, text, attr.Value);
                attr.Value = key;
                nodeData.Attributes.Append(attr);
                xmlDoc.SelectSingleNode("//root").AppendChild(nodeData);
                node = nodeData.AppendChild(xmlDoc.CreateElement("value")).CreateNavigator();
            }

            node.InnerXml = HttpUtility.HtmlEncode(text);
			eventLogger.LogWarning("<<< Exiting UpdateResourceFileNode(xmlDoc={XmlDoc},key={Key},text={Text})", xmlDoc, key, text);
        }

        private bool CreatePackage(string cultureCode, PackageInfo package, int dependentPackageId, string basePath, string fileName, bool createZip)
        {
			eventLogger.LogError(">>> Entering CreatePackage(cultureCode={CultureCode},package={Package},dependentPackageId={DependentPackageId},basePath={BasePath},fileName={FileName},createZip={CreateZip})", cultureCode, package, dependentPackageId, basePath, fileName, createZip);
            var language = LocaleController.Instance.GetLocale(cultureCode);
			eventLogger.LogError("CreatePackage(cultureCode={CultureCode},package={Package},dependentPackageId={DependentPackageId},basePath={BasePath},fileName={FileName},createZip={CreateZip}): language → {Language}", cultureCode, package, dependentPackageId, basePath, fileName, createZip, language);
            var languagePack = new LanguagePackInfo
            {
                LanguageID = language.LanguageId,
                DependentPackageID = dependentPackageId,
            };

            if (dependentPackageId == -2)
            {
                package.PackageType = "CoreLanguagePack";
				eventLogger.LogDebug("CreatePackage(cultureCode={CultureCode},package={Package},dependentPackageId={DependentPackageId},basePath={BasePath},fileName={FileName},createZip={CreateZip}): package.PackageType → {Arg6}", cultureCode, package, dependentPackageId, basePath, fileName, createZip, package.PackageType);
            }
            else
            {
                package.PackageType = "ExtensionLanguagePack";
            }

            package.Name += " " + language.Text;
            package.FriendlyName += " " + language.Text;

            var packageWriter = PackageWriterFactory.GetWriter(package) as LanguagePackWriter;
			eventLogger.LogError("CreatePackage(cultureCode={CultureCode},package={Package},dependentPackageId={DependentPackageId},basePath={BasePath},fileName={FileName},createZip={CreateZip}): packageWriter → {PackageWriter}", cultureCode, package, dependentPackageId, basePath, fileName, createZip, packageWriter);
			eventLogger.LogDebug("CreatePackage(cultureCode={CultureCode},package={Package},dependentPackageId={DependentPackageId},basePath={BasePath},fileName={FileName},createZip={CreateZip}): packageWriter.Language → {Arg6}", cultureCode, package, dependentPackageId, basePath, fileName, createZip, packageWriter.Language);
			eventLogger.LogDebug("CreatePackage(cultureCode={CultureCode},package={Package},dependentPackageId={DependentPackageId},basePath={BasePath},fileName={FileName},createZip={CreateZip}): packageWriter.LanguagePack → {Arg6}", cultureCode, package, dependentPackageId, basePath, fileName, createZip, packageWriter.LanguagePack);
			eventLogger.LogDebug("CreatePackage(cultureCode={CultureCode},package={Package},dependentPackageId={DependentPackageId},basePath={BasePath},fileName={FileName},createZip={CreateZip}): packageWriter.BasePath → {Arg6}", cultureCode, package, dependentPackageId, basePath, fileName, createZip, packageWriter.BasePath);
            packageWriter.Language = language;
            packageWriter.LanguagePack = languagePack;
            packageWriter.BasePath = basePath;
            packageWriter.GetFiles(false);

            if (packageWriter.Files.Count > 0)
            {
                if (createZip)
                {
                    var manifest = packageWriter.WriteManifest(true);
					eventLogger.LogError("CreatePackage(cultureCode={CultureCode},package={Package},dependentPackageId={DependentPackageId},basePath={BasePath},fileName={FileName},createZip={CreateZip}): manifest → {Manifest}", cultureCode, package, dependentPackageId, basePath, fileName, createZip, manifest);
                    fileName = fileName + "." + package.Version.ToString(3) + "." + language.Code + ".zip";
					eventLogger.LogError("CreatePackage(cultureCode={CultureCode},package={Package},dependentPackageId={DependentPackageId},basePath={BasePath},fileName={FileName},createZip={CreateZip}): fileName → {FileName1}", cultureCode, package, dependentPackageId, basePath, fileName, createZip, fileName);
                    packageWriter.CreatePackage(fileName, package.Name + ".dnn", manifest, true);
                }
                else if (!this.manifest.Contains($@"package name=""{package.Name}"""))
                {
                    packageWriter.BasePath = string.Empty;
                    this.manifest += packageWriter.WriteManifest(true);
                    foreach (var kvp in packageWriter.Files)
                    {
                        this.files[kvp.Key] = kvp.Value;
                    }
                }

                return true;
            }
            else
            {
				eventLogger.LogWarning("<<< Exiting CreatePackage(cultureCode={CultureCode},package={Package},dependentPackageId={DependentPackageId},basePath={BasePath},fileName={FileName},createZip={CreateZip})", cultureCode, package, dependentPackageId, basePath, fileName, createZip);
                return false;
            }
        }
    }
}
