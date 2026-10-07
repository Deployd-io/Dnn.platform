// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;

    /// <summary>The LanguagePackWriter class.</summary>
    public class LanguagePackWriter : PackageWriterBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LanguagePackWriter));
        private bool isCore = Null.NullBoolean;
        private Locale language;
        private LanguagePackInfo languagePack;

        /// <summary>Initializes a new instance of the <see cref="LanguagePackWriter"/> class.</summary>
        /// <param name="package">The package info.</param>
        public LanguagePackWriter(PackageInfo package)
            : base(package)
        {
			Logger.LogError(">>> Entering LanguagePackWriter(package={Package})", package);
            this.languagePack = LanguagePackController.GetLanguagePackByPackage(package.PackageID);
			Logger.LogDebug("LanguagePackWriter(package={Package}): languagePack → {LanguagePack}", package, languagePack);
            if (this.LanguagePack != null)
            {
                this.language = LocaleController.Instance.GetLocale(this.languagePack.LanguageID);
				Logger.LogDebug("LanguagePackWriter(package={Package}): language → {Language}", package, language);
                if (this.LanguagePack.PackageType == LanguagePackType.Core)
                {
                    this.BasePath = Null.NullString;
                }
                else
                {
					Logger.LogDebug("LanguagePackWriter(package={Package}): this.BasePath → {Arg1}", package, this.BasePath);
                    // Get the BasePath of the Dependent Package
                    PackageInfo dependendentPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == this.LanguagePack.DependentPackageID);
					Logger.LogError("LanguagePackWriter(package={Package}): dependendentPackage → {DependendentPackage}", package, dependendentPackage);
                    PackageWriterBase dependentPackageWriter = PackageWriterFactory.GetWriter(dependendentPackage);
					Logger.LogError("LanguagePackWriter(package={Package}): dependentPackageWriter → {DependentPackageWriter}", package, dependentPackageWriter);
                    this.BasePath = dependentPackageWriter.BasePath;
                }
            }
            else
            {
                this.BasePath = Null.NullString;
				Logger.LogDebug("LanguagePackWriter(manifestNav={ManifestNav},installer={Installer}): this.BasePath → {Arg2}", manifestNav, installer, this.BasePath);
			Logger.LogWarning("<<< Exiting LanguagePackWriter(package={Package})", package);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="LanguagePackWriter"/> class.</summary>
        /// <param name="manifestNav">The XPath navigator for the manifest section.</param>
        /// <param name="installer">The installer info.</param>
        public LanguagePackWriter(XPathNavigator manifestNav, InstallerInfo installer)
        {
            this.language = new Locale();
            XPathNavigator cultureNav = manifestNav.SelectSingleNode("Culture");
			Logger.LogError("LanguagePackWriter(manifestNav={ManifestNav},installer={Installer}): cultureNav → {CultureNav}", manifestNav, installer, cultureNav);
			Logger.LogDebug("LanguagePackWriter(manifestNav={ManifestNav},installer={Installer}): language → {Language}", manifestNav, installer, language);
            this.language.Text = Util.ReadAttribute(cultureNav, "DisplayName");
            this.language.Code = Util.ReadAttribute(cultureNav, "Code");
            this.language.Fallback = Localization.SystemLocale;

            // Create a Package
            this.Package = new PackageInfo(installer);
            this.Package.Name = this.Language.Text;
            this.Package.FriendlyName = this.Language.Text;
            this.Package.Description = Null.NullString;
            this.Package.Version = new Version(1, 0, 0);
            this.Package.License = Util.PACKAGE_NoLicense;

            this.ReadLegacyManifest(manifestNav);

            if (this.isCore)
            {
                this.Package.PackageType = "CoreLanguagePack";
            }
            else
            {
                this.Package.PackageType = "ExtensionLanguagePack";
            }

            this.BasePath = Null.NullString;
        }

        /// <summary>Initializes a new instance of the <see cref="LanguagePackWriter"/> class.</summary>
        /// <param name="language">The locale.</param>
        /// <param name="package">The package info.</param>
        public LanguagePackWriter(Locale language, PackageInfo package)
            : base(package)
        {
            this.language = language;
			Logger.LogDebug("LanguagePackWriter(language={Language},package={Package}): this.language → {Arg2}", language, package, this.language);
            this.BasePath = Null.NullString;
        }

        /// <inheritdoc />
        public override bool IncludeAssemblies
        {
            get
            {
                return false;
            }
        }

        /// <summary>Gets or sets the associated Language.</summary>
        /// <value>An Locale object.</value>
        public Locale Language
        {
            get
            {
                return this.language;
            }

            set
            {
                this.language = value;
            }
        }

        /// <summary>Gets or sets the associated Language Pack.</summary>
        /// <value>An LanguagePackInfo object.</value>
        public LanguagePackInfo LanguagePack
        {
            get
            {
                return this.languagePack;
            }

            set
            {
                this.languagePack = value;
            }
        }

        /// <inheritdoc />
        protected override void GetFiles(bool includeSource, bool includeAppCode)
        {
			Logger.LogError(">>> Entering GetFiles(includeSource={IncludeSource},includeAppCode={IncludeAppCode})", includeSource, includeAppCode);
            // Language file starts at the root
            this.ParseFolder(Path.Combine(Globals.ApplicationMapPath, this.BasePath), Globals.ApplicationMapPath);
			Logger.LogWarning("<<< Exiting GetFiles(includeSource={IncludeSource},includeAppCode={IncludeAppCode})", includeSource, includeAppCode);
        }

        /// <inheritdoc />
        protected override void ParseFiles(DirectoryInfo folder, string rootPath)
        {
			Logger.LogError(">>> Entering ParseFiles(folder={Folder},rootPath=<redacted>)", folder);
            if (this.LanguagePack.PackageType == LanguagePackType.Core)
            {
                if ((folder.FullName.ToLowerInvariant().Contains("desktopmodules") && !folder.FullName.ToLowerInvariant().Contains("admin")) || folder.FullName.ToLowerInvariant().Contains("providers"))
                {
                    return;
                }

                if (folder.FullName.ToLowerInvariant().Contains("install") && folder.FullName.ToLowerInvariant().Contains("temp"))
                {
                    return;
                }
            }

            if (folder.Name.Equals("app_localresources", StringComparison.OrdinalIgnoreCase) || folder.Name.Equals("app_globalresources", StringComparison.OrdinalIgnoreCase) || folder.Name.Equals("_default", StringComparison.OrdinalIgnoreCase))
            {
                // Add the Files in the Folder
                FileInfo[] files = folder.GetFiles();
                foreach (FileInfo file in files)
                {
                    string filePath = folder.FullName.Replace(rootPath, string.Empty);
					Logger.LogError("ParseFiles(folder={Folder},rootPath=<redacted>): filePath → {FilePath}", folder, filePath);
                    if (filePath.StartsWith(@"\", StringComparison.Ordinal))
                    {
                        filePath = filePath.Substring(1);
                    }

                    if (file.Name.Contains(this.Language.Code, StringComparison.OrdinalIgnoreCase) || (this.Language.Code.Equals("en-us", StringComparison.OrdinalIgnoreCase) && !file.Name.Contains("-")))
                    {
                        this.AddFile(Path.Combine(filePath, file.Name));
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void WriteFilesToManifest(XmlWriter writer)
        {
			Logger.LogError(">>> Entering WriteFilesToManifest(writer={Writer})", writer);
            LanguageComponentWriter languageFileWriter;
            if (this.LanguagePack == null)
            {
                languageFileWriter = new LanguageComponentWriter(this.Language, this.BasePath, this.Files, this.Package);
				Logger.LogError("WriteFilesToManifest(writer={Writer}): languageFileWriter → {LanguageFileWriter}", writer, languageFileWriter);
            }
            else
            {
                languageFileWriter = new LanguageComponentWriter(this.LanguagePack, this.BasePath, this.Files, this.Package);
				Logger.LogDebug("WriteFilesToManifest(writer={Writer}): languageFileWriter → {LanguageFileWriter}", writer, languageFileWriter);
            }

            languageFileWriter.WriteManifest(writer);
			Logger.LogWarning("<<< Exiting WriteFilesToManifest(writer={Writer})", writer);
        }

        private void ReadLegacyManifest(XPathNavigator manifestNav)
        {
			Logger.LogError(">>> Entering ReadLegacyManifest(manifestNav={ManifestNav})", manifestNav);
            string fileName = Null.NullString;
            string filePath = Null.NullString;
			Logger.LogError("ReadLegacyManifest(manifestNav={ManifestNav}): filePath → {FilePath}", manifestNav, filePath);
            string sourceFileName = Null.NullString;
			Logger.LogError("ReadLegacyManifest(manifestNav={ManifestNav}): sourceFileName → {SourceFileName}", manifestNav, sourceFileName);
            string resourcetype = Null.NullString;
			Logger.LogError("ReadLegacyManifest(manifestNav={ManifestNav}): resourcetype → {Resourcetype}", manifestNav, resourcetype);
			Logger.LogError("ReadLegacyManifest(manifestNav={ManifestNav}): fileName → {FileName}", manifestNav, fileName);
            string moduleName = Null.NullString;
            foreach (XPathNavigator fileNav in manifestNav.Select("Files/File"))
            {
				Logger.LogError("ReadLegacyManifest(manifestNav={ManifestNav}): moduleName → {ModuleName}", manifestNav, moduleName);
                fileName = Util.ReadAttribute(fileNav, "FileName").ToLowerInvariant();
                resourcetype = Util.ReadAttribute(fileNav, "FileType");
                moduleName = Util.ReadAttribute(fileNav, "ModuleName").ToLowerInvariant();
                sourceFileName = Path.Combine(resourcetype, Path.Combine(moduleName, fileName));
                string extendedExtension = "." + this.Language.Code.ToLowerInvariant() + ".resx";
                switch (resourcetype)
                {
                    case "GlobalResource":
                        filePath = "App_GlobalResources";
                        this.isCore = true;
						Logger.LogDebug("ReadLegacyManifest(manifestNav={ManifestNav}): this.isCore → {Arg1}", manifestNav, this.isCore);
                        break;
                    case "ControlResource":
                        filePath = "Controls\\App_LocalResources";
                        break;
                    case "AdminResource":
                        this.isCore = true;
                        switch (moduleName)
                        {
                            case "authentication":
                                filePath = "DesktopModules\\Admin\\Authentication\\App_LocalResources";
                                break;
                            case "controlpanel":
                                filePath = "Admin\\ControlPanel\\App_LocalResources";
                                break;
                            case "files":
                                filePath = "DesktopModules\\Admin\\FileManager\\App_LocalResources";
                                break;
                            case "host":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "authentication.ascx":
                                        filePath = string.Empty;
                                        break;
                                    case "friendlyurls.ascx":
                                        filePath = "DesktopModules\\Admin\\HostSettings\\App_LocalResources";
                                        break;
                                    case "hostsettings.ascx":
                                        filePath = "DesktopModules\\Admin\\HostSettings\\App_LocalResources";
                                        break;
                                    case "requestfilters.ascx":
                                        filePath = "DesktopModules\\Admin\\HostSettings\\App_LocalResources";
                                        break;
                                }

                                break;
                            case "lists":
                                filePath = "DesktopModules\\Admin\\Lists\\App_LocalResources";
                                break;
                            case "localization":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "languageeditor.ascx":
                                        filePath = "DesktopModules\\Admin\\Extensions\\Editors\\App_LocalResources";
                                        break;
                                    case "languageeditorext.ascx":
                                        filePath = "DesktopModules\\Admin\\Extensions\\Editors\\App_LocalResources";
                                        break;
                                    case "timezoneeditor.ascx":
                                        filePath = "DesktopModules\\Admin\\Extensions\\Editors\\App_LocalResources";
                                        break;
                                    case "resourceverifier.ascx":
                                        filePath = "DesktopModules\\Admin\\Extensions\\Editors\\App_LocalResources";
                                        break;
                                    default:
                                        filePath = string.Empty;
                                        break;
                                }

                                break;
                            case "logging":
                                filePath = "DesktopModules\\Admin\\LogViewer\\App_LocalResources";
                                break;
                            case "moduledefinitions":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "editmodulecontrol.ascx":
                                        filePath = "DesktopModules\\Admin\\Extensions\\Editors\\App_LocalResources";
                                        break;
                                    case "importmoduledefinition.ascx":
                                        filePath = "DesktopModules\\Admin\\Extensions\\Editors\\App_LocalResources";
                                        break;
                                    case "timezoneeditor.ascx":
                                        filePath = "DesktopModules\\Admin\\Extensions\\Editors\\App_LocalResources";
                                        break;
                                    default:
                                        filePath = string.Empty;
                                        break;
                                }

                                break;
                            case "modules":
                                filePath = "Admin\\Modules\\App_LocalResources";
                                break;
                            case "packages":
                                filePath = "DesktopModules\\Admin\\Extensions\\App_LocalResources";
                                break;
                            case "portal":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "editportalalias.ascx":
                                        filePath = "DesktopModules\\Admin\\Portals\\App_LocalResources";
                                        break;
                                    case "portalalias.ascx":
                                        filePath = "DesktopModules\\Admin\\Portals\\App_LocalResources";
                                        break;
                                    case "portals.ascx":
                                        filePath = "DesktopModules\\Admin\\Portals\\App_LocalResources";
                                        break;
                                    case "privacy.ascx":
                                        filePath = "Admin\\Portal\\App_LocalResources";
                                        break;
                                    case "signup.ascx":
                                        filePath = "DesktopModules\\Admin\\Portals\\App_LocalResources";
                                        break;
                                    case "sitesettings.ascx":
                                        filePath = "DesktopModules\\Admin\\Portals\\App_LocalResources";
                                        break;
                                    case "sitewizard.ascx":
                                        filePath = "DesktopModules\\Admin\\SiteWizard\\App_LocalResources";
                                        break;
                                    case "sql.ascx":
                                        filePath = "DesktopModules\\Admin\\SQL\\App_LocalResources";
                                        break;
                                    case "systemmessages.ascx":
                                        filePath = string.Empty;
                                        break;
                                    case "template.ascx":
                                        filePath = "DesktopModules\\Admin\\Portals\\App_LocalResources";
                                        break;
                                    case "terms.ascx":
                                        filePath = "Admin\\Portal\\App_LocalResources";
                                        break;
                                }

                                break;
                            case "scheduling":
                                filePath = "DesktopModules\\Admin\\Scheduler\\App_LocalResources";
                                break;
                            case "search":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "inputsettings.ascx":
                                        filePath = "DesktopModules\\Admin\\SearchInput\\App_LocalResources";
                                        break;
                                    case "resultssettings.ascx":
                                        filePath = "DesktopModules\\Admin\\SearchResults\\App_LocalResources";
                                        break;
                                    case "searchadmin.ascx":
                                        filePath = "DesktopModules\\Admin\\SearchAdmin\\App_LocalResources";
                                        break;
                                    case "searchinput.ascx":
                                        filePath = "DesktopModules\\Admin\\SearchInput\\App_LocalResources";
                                        break;
                                    case "searchresults.ascx":
                                        filePath = "DesktopModules\\Admin\\SearchResults\\App_LocalResources";
                                        break;
                                }

                                break;
                            case "security":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "accessdenied.ascx":
                                        filePath = "Admin\\Security\\App_LocalResources";
                                        break;
                                    case "authenticationsettings.ascx":
                                        filePath = string.Empty;
                                        break;
                                    case "editgroups.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "editroles.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "register.ascx":
                                        filePath = string.Empty;
                                        break;
                                    case "roles.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "securityroles.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "sendpassword.ascx":
                                        filePath = "Admin\\Security\\App_LocalResources";
                                        break;
                                    case "signin.ascx":
                                        filePath = string.Empty;
                                        break;
                                }

                                break;
                            case "skins":
                                filePath = "Admin\\Skins\\App_LocalResources";
                                break;
                            case "syndication":
                                filePath = "DesktopModules\\Admin\\FeedExplorer\\App_LocalResources";
                                break;
                            case "tabs":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "export.ascx":
                                        filePath = "Admin\\Tabs\\App_LocalResources";
                                        break;
                                    case "import.ascx":
                                        filePath = "Admin\\Tabs\\App_LocalResources";
                                        break;
                                    case "managetabs.ascx":
                                        filePath = "DesktopModules\\Admin\\Tabs\\App_LocalResources";
                                        break;
                                    case "recyclebin.ascx":
                                        filePath = "DesktopModules\\Admin\\RecycleBin\\App_LocalResources";
                                        break;
                                    case "tabs.ascx":
                                        filePath = "DesktopModules\\Admin\\Tabs\\App_LocalResources";
                                        break;
                                }

                                break;
                            case "users":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "bulkemail.ascx":
                                        filePath = "DesktopModules\\Admin\\Newsletters\\App_LocalResources";
                                        fileName = "Newsletter.ascx" + extendedExtension;
                                        break;
                                    case "editprofiledefinition.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "manageusers.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "memberservices.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "membership.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "password.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "profile.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "profiledefinitions.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "user.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "users.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "usersettings.ascx":
                                        filePath = "DesktopModules\\Admin\\Security\\App_LocalResources";
                                        break;
                                    case "viewprofile.ascx":
                                        filePath = "Admin\\Users\\App_LocalResources";
                                        break;
                                }

                                break;
                            case "vendors":
                                switch (fileName.Replace(extendedExtension, string.Empty))
                                {
                                    case "adsense.ascx":
                                        filePath = string.Empty;
                                        break;
                                    case "editadsense.ascx":
                                        filePath = string.Empty;
                                        break;
                                    case "affiliates.ascx":
                                        filePath = "DesktopModules\\Admin\\Vendors\\App_LocalResources";
                                        break;
                                    case "banneroptions.ascx":
                                        filePath = "DesktopModules\\Admin\\Banners\\App_LocalResources";
                                        break;
                                    case "banners.ascx":
                                        filePath = "DesktopModules\\Admin\\Vendors\\App_LocalResources";
                                        break;
                                    case "displaybanners.ascx":
                                        filePath = "DesktopModules\\Admin\\Banners\\App_LocalResources";
                                        break;
                                    case "editaffiliate.ascx":
                                        filePath = "DesktopModules\\Admin\\Vendors\\App_LocalResources";
                                        break;
                                    case "editbanner.ascx":
                                        filePath = "DesktopModules\\Admin\\Vendors\\App_LocalResources";
                                        break;
                                    case "editvendors.ascx":
                                        filePath = "DesktopModules\\Admin\\Vendors\\App_LocalResources";
                                        break;
                                    case "vendors.ascx":
                                        filePath = "DesktopModules\\Admin\\Vendors\\App_LocalResources";
                                        break;
                                }

                                break;
                        }

                        break;
                    case "LocalResource":
                        filePath = Path.Combine("DesktopModules", Path.Combine(moduleName, "App_LocalResources"));

                        // Two assumptions are made here
                        // 1. Core files appear in the package before extension files
                        // 2. Module packages only include one module
                        if (!this.isCore && this.languagePack == null)
                        {
                            // Check if language is installed
                            Locale locale = LocaleController.Instance.GetLocale(this.language.Code);
							Logger.LogError("ReadLegacyManifest(manifestNav={ManifestNav}): locale → {Locale}", manifestNav, locale);
                            if (locale == null)
                            {
                                this.LegacyError = "CoreLanguageError";
                            }
                            else
                            {
								Logger.LogDebug("ReadLegacyManifest(manifestNav={ManifestNav}): this.LegacyError → {Arg1}", manifestNav, this.LegacyError);
                                // Attempt to figure out the Extension
                                foreach (var kvp in DesktopModuleController.GetDesktopModules(Null.NullInteger))
                                {
                                    if (kvp.Value.FolderName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Found Module - Get Package
                                        var dependentPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == kvp.Value.PackageID);
										Logger.LogError("ReadLegacyManifest(manifestNav={ManifestNav}): dependentPackage → {DependentPackage}", manifestNav, dependentPackage);
                                        this.Package.Name += "_" + dependentPackage.Name;
                                        this.Package.FriendlyName += " " + dependentPackage.FriendlyName;
										Logger.LogDebug("ReadLegacyManifest(manifestNav={ManifestNav}): languagePack → {LanguagePack}", manifestNav, languagePack);
                                        this.languagePack = new LanguagePackInfo
                                        {
											Logger.LogDebug("ReadLegacyManifest(manifestNav={ManifestNav}): this.languagePack → {Arg1}", manifestNav, this.languagePack);
                                            DependentPackageID = dependentPackage.PackageID,
                                            LanguageID = locale.LanguageId,
                                        };
                                        break;
                                    }
                                }

                                if (this.languagePack == null)
                                {
                                    this.LegacyError = "DependencyError";
                                }
                            }
                        }

                        break;
                    case "ProviderResource":
                        filePath = Path.Combine("Providers", Path.Combine(moduleName, "App_LocalResources"));
                        break;
                    case "InstallResource":
                        filePath = "Install\\App_LocalResources";
                        break;
                }

                if (!string.IsNullOrEmpty(filePath))
                {
                    this.AddFile(Path.Combine(filePath, fileName), sourceFileName);
                }
            }
			Logger.LogWarning("<<< Exiting ReadLegacyManifest(manifestNav={ManifestNav})", manifestNav);
        }
    }
}
