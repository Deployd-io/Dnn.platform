// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.EventQueue;
    using DotNetNuke.Services.Installer.Dependencies;
    using DotNetNuke.Services.Installer.Packages;

    using SchwabenCode.QuickIO;

    /// <summary>The PackageInstaller class is an Installer for Packages.</summary>
    public class PackageInstaller : ComponentInstallerBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PackageInstaller));
        private readonly SortedList<int, ComponentInstallerBase> componentInstallers = new SortedList<int, ComponentInstallerBase>();
        private PackageInfo installedPackage;
        private EventMessage eventMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstaller"/> class.
        /// This Constructor creates a new PackageInstaller instance.
        /// </summary>
        /// <param name="package">A PackageInfo instance.</param>
        public PackageInstaller(PackageInfo package)
        {
			Logger.LogDebug(">>> Entering PackageInstaller(package={Package})", package);
            this.IsValid = true;
            this.DeleteFiles = Null.NullBoolean;
			Logger.LogDebug("PackageInstaller(package={Package}): this.IsValid → {Arg1}", package, this.IsValid);
			Logger.LogDebug("PackageInstaller(package={Package}): this.DeleteFiles → {Arg1}", package, this.DeleteFiles);
            this.Package = package;
            if (!string.IsNullOrEmpty(package.Manifest))
            {
				Logger.LogDebug("PackageInstaller(package={Package}): this.Package → {Arg1}", package, this.Package);
                // Create an XPathDocument from the XML
                XPathDocument doc;
                using (var manifestReader = XmlReader.Create(new StringReader(package.Manifest)))
                {
                    doc = new XPathDocument(manifestReader);
                }

                XPathNavigator nav = doc.CreateNavigator().SelectSingleNode("package");
				Logger.LogDebug("PackageInstaller(package={Package}): nav → {Nav}", package, nav);
                this.ReadComponents(nav);
            }
            else
            {
                ComponentInstallerBase installer = InstallerFactory.GetInstaller(package.PackageType);
				Logger.LogDebug("PackageInstaller(package={Package}): installer → {Installer}", package, installer);
                if (installer != null)
                {
                    // Set package
                    installer.Package = package;
					Logger.LogDebug("PackageInstaller(package={Package}): installer.Package → {Arg1}", package, installer.Package);

                    // Set type
                    installer.Type = package.PackageType;
					Logger.LogDebug("PackageInstaller(package={Package}): installer.Type → {Arg1}", package, installer.Type);
                    this.componentInstallers.Add(0, installer);
                }
            }
			Logger.LogDebug("<<< Exiting PackageInstaller(package={Package})", package);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstaller"/> class.
        /// This Constructor creates a new PackageInstaller instance.
        /// </summary>
        /// <param name="info">An InstallerInfo instance.</param>
        /// <param name="packageManifest">The manifest as a string.</param>
        public PackageInstaller(string packageManifest, InstallerInfo info)
        {
			Logger.LogDebug(">>> Entering PackageInstaller(packageManifest={PackageManifest},info={Info})", packageManifest, info);
            this.IsValid = true;
            this.DeleteFiles = Null.NullBoolean;
			Logger.LogDebug("PackageInstaller(packageManifest={PackageManifest},info={Info}): this.IsValid → {Arg2}", packageManifest, info, this.IsValid);
			Logger.LogDebug("PackageInstaller(packageManifest={PackageManifest},info={Info}): this.DeleteFiles → {Arg2}", packageManifest, info, this.DeleteFiles);
            this.Package = new PackageInfo(info) { Manifest = packageManifest, };

            if (!string.IsNullOrEmpty(packageManifest))
            {
                // Create an XPathDocument from the XML
				Logger.LogDebug("<<< Exiting PackageInstaller(package={Package})", package);
                XPathDocument doc;
                using (var xmlReader = XmlReader.Create(new StringReader(packageManifest)))
                {
                    doc = new XPathDocument(xmlReader);
                }

                XPathNavigator nav = doc.CreateNavigator().SelectSingleNode("package");
				Logger.LogDebug("PackageInstaller(packageManifest={PackageManifest},info={Info}): nav → {Nav}", packageManifest, info, nav);
                this.ReadManifest(nav);
            }
        }

        /// <summary>Gets or sets a value indicating whether the Packages files are deleted when uninstalling the package.</summary>
        /// <value>A Boolean value.</value>
        public bool DeleteFiles { get; set; }

        /// <summary>Gets a value indicating whether the Package is Valid.</summary>
        /// <value>A Boolean value.</value>
        public bool IsValid { get; private set; }

        /// <summary>The Commit method commits the package installation.</summary>
        public override void Commit()
        {
			Logger.LogDebug(">>> Entering Commit()");
            for (int index = 0; index <= this.componentInstallers.Count - 1; index++)
            {
                ComponentInstallerBase compInstaller = this.componentInstallers.Values[index];
                if (compInstaller.Version >= this.Package.InstalledVersion && compInstaller.Completed)
                {
                    compInstaller.Commit();
                }
            }

            // Add Event Message
            if (this.eventMessage != null && !string.IsNullOrEmpty(this.eventMessage.Attributes["UpgradeVersionsList"]))
            {
                this.eventMessage.Attributes.Set("desktopModuleID", Null.NullInteger.ToString(CultureInfo.InvariantCulture));
                EventQueueController.SendMessage(this.eventMessage, "Application_Start");
            }

            if (this.Log.Valid)
            {
                this.Log.AddInfo(Util.INSTALL_Committed);
            }
            else
            {
                this.Log.AddFailure(Util.INSTALL_Aborted);
            }

            this.Package.InstallerInfo.PackageID = this.Package.PackageID;
			Logger.LogDebug("<<< Exiting Commit()");
        }

        /// <summary>The Install method installs the components of the package.</summary>
        public override void Install()
        {
			Logger.LogDebug(">>> Entering Install()");
            bool isCompleted = true;
            try
			Logger.LogDebug("Install(): isCompleted → {IsCompleted}", isCompleted);
            {
                // Save the Package Information
                if (this.installedPackage != null)
                {
                    this.Package.PackageID = this.installedPackage.PackageID;
                }

                // Save Package
                PackageController.Instance.SaveExtensionPackage(this.Package);

                // Iterate through all the Components
                for (int index = 0; index <= this.componentInstallers.Count - 1; index++)
                {
                    ComponentInstallerBase compInstaller = this.componentInstallers.Values[index];
                    if ((this.installedPackage == null) || (compInstaller.Version > this.Package.InstalledVersion) || this.Package.InstallerInfo.RepairInstall)
                    {
                        this.Log.AddInfo(Util.INSTALL_Start + " - " + compInstaller.Type);
                        compInstaller.Install();
                        if (compInstaller.Completed)
                        {
                            if (compInstaller.Skipped)
                            {
                                this.Log.AddInfo(Util.COMPONENT_Skipped + " - " + compInstaller.Type);
                            }
                            else
                            {
                                this.Log.AddInfo(Util.COMPONENT_Installed + " - " + compInstaller.Type);
                            }
                        }
                        else
                        {
                            this.Log.AddFailure(Util.INSTALL_Failed + " - " + compInstaller.Type);
                            isCompleted = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(Util.INSTALL_Aborted + " - " + this.Package.Name + ":" + ex.Message);
            }

            if (isCompleted)
            {
                // All components successfully installed so Commit any pending changes
                this.Commit();
            }
            else
            {
                // There has been a failure so Rollback
                this.Rollback();
            }
			Logger.LogDebug("<<< Exiting Install()");
        }

        /// <summary>The ReadManifest method reads the manifest file and parses it into components.</summary>
        /// <param name="manifestNav">The XPath navigator for the package section of the manifest.</param>
        public override void ReadManifest(XPathNavigator manifestNav)
        {
			Logger.LogDebug(">>> Entering ReadManifest(manifestNav={ManifestNav})", manifestNav);
            // Get Name Property
            this.Package.Name = Util.ReadAttribute(manifestNav, "name", this.Log, Util.EXCEPTION_NameMissing);

            // Get Type
            this.Package.PackageType = Util.ReadAttribute(manifestNav, "type", this.Log, Util.EXCEPTION_TypeMissing);

            // If Skin or Container then set PortalID
            if (this.Package.PackageType.Equals("Skin", StringComparison.OrdinalIgnoreCase) || this.Package.PackageType.Equals("Container", StringComparison.OrdinalIgnoreCase))
            {
                this.Package.PortalID = this.Package.InstallerInfo.PortalID;
            }

            this.CheckSecurity();
            if (!this.IsValid)
            {
                return;
            }

            // Get IsSystem
            this.Package.IsSystemPackage = bool.Parse(Util.ReadAttribute(manifestNav, "isSystem", false, this.Log, string.Empty, bool.FalseString));

            // Get Version
            string strVersion = Util.ReadAttribute(manifestNav, "version", this.Log, Util.EXCEPTION_VersionMissing);
            if (string.IsNullOrEmpty(strVersion))
            {
                this.IsValid = false;
				Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): this.IsValid → {Arg1}", manifestNav, this.IsValid);
            }

            if (this.IsValid)
            {
                this.Package.Version = new Version(strVersion);
            }
            else
            {
                return;
            }

            // Attempt to get the Package from the Data Store (see if its installed)
            var packageType = PackageController.Instance.GetExtensionPackageType(t => t.PackageType.Equals(this.Package.PackageType, StringComparison.OrdinalIgnoreCase));
			Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): packageType → {PackageType}", manifestNav, packageType);

            if (packageType.SupportsSideBySideInstallation)
            {
                this.installedPackage = PackageController.Instance.GetExtensionPackage(this.Package.PortalID, p => p.Name.Equals(this.Package.Name, StringComparison.OrdinalIgnoreCase)
                                                                                                            && p.PackageType.Equals(this.Package.PackageType, StringComparison.OrdinalIgnoreCase)
                                                                                                            && p.Version == this.Package.Version);
																											Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): installedPackage → {InstalledPackage}", manifestNav, installedPackage);
            }
            else
            {
                this.installedPackage = PackageController.Instance.GetExtensionPackage(this.Package.PortalID, p => p.Name.Equals(this.Package.Name, StringComparison.OrdinalIgnoreCase)
                                                                                                            && p.PackageType.Equals(this.Package.PackageType, StringComparison.OrdinalIgnoreCase));
            }

            if (this.installedPackage != null)
            {
                this.Package.InstalledVersion = this.installedPackage.Version;
                this.Package.InstallerInfo.PackageID = this.installedPackage.PackageID;

                if (this.Package.InstalledVersion > this.Package.Version)
                {
                    this.Log.AddFailure(Util.INSTALL_Version + " - " + this.Package.InstalledVersion.ToString(3));
                    this.IsValid = false;
                }
                else if (this.Package.InstalledVersion == this.Package.Version)
                {
                    this.Package.InstallerInfo.Installed = true;
                    this.Package.InstallerInfo.PortalID = this.installedPackage.PortalID;
                }
            }

            this.Log.AddInfo(Util.DNN_ReadingPackage + " - " + this.Package.PackageType + " - " + this.Package.Name);
            this.Package.FriendlyName = Util.ReadElement(manifestNav, "friendlyName", this.Package.Name);
            this.Package.Description = Util.ReadElement(manifestNav, "description");

            XPathNavigator folderNameNav = null;
			Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): folderNameNav → {FolderNameNav}", manifestNav, folderNameNav);
            this.Package.FolderName = string.Empty;
            switch (this.Package.PackageType)
            {
                case "Module":
                    // In Dynamics modules, a component:type=File can have a basePath pointing to the App_Conde folder. This is not a correct FolderName
                    // To ensure that FolderName is DesktopModules...
                    var folderNameValue = PackageController.GetSpecificFolderName(manifestNav, "components/component/files|components/component/resourceFiles", "basePath", "DesktopModules");
					Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): folderNameValue → {FolderNameValue}", manifestNav, folderNameValue);
                    if (!string.IsNullOrEmpty(folderNameValue))
                    {
                        this.Package.FolderName = folderNameValue.Replace('\\', '/');
                    }

                    break;
                case "Auth_System":
                    folderNameNav = manifestNav.SelectSingleNode("components/component/files");
                    if (folderNameNav != null)
                    {
                        this.Package.FolderName = Util.ReadElement(folderNameNav, "basePath").Replace('\\', '/');
                    }

                    break;
                case "Container":
                    folderNameNav = manifestNav.SelectSingleNode("components/component/containerFiles");
                    if (folderNameNav != null)
                    {
                        this.Package.FolderName = Globals.glbContainersPath + Util.ReadElement(folderNameNav, "containerName").Replace('\\', '/');
                    }

                    break;
                case "Skin":
                    folderNameNav = manifestNav.SelectSingleNode("components/component/skinFiles");
                    if (folderNameNav != null)
                    {
                        this.Package.FolderName = Globals.glbSkinsPath + Util.ReadElement(folderNameNav, "skinName").Replace('\\', '/');
                    }

                    break;
                default:
                    // copied from "Module" without the extra OR condition
                    folderNameValue = PackageController.GetSpecificFolderName(manifestNav, "components/component/resourceFiles", "basePath", "DesktopModules");
                    if (!string.IsNullOrEmpty(folderNameValue))
                    {
                        this.Package.FolderName = folderNameValue.Replace('\\', '/');
                    }

                    break;
            }

            this.eventMessage = this.ReadEventMessageNode(manifestNav);
			Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): eventMessage → {EventMessage}", manifestNav, eventMessage);

            // Get Icon
            XPathNavigator iconFileNav = manifestNav.SelectSingleNode("iconFile");
			Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): iconFileNav → {IconFileNav}", manifestNav, iconFileNav);
            if (iconFileNav != null)
            {
                if (iconFileNav.Value != string.Empty)
                {
                    if (iconFileNav.Value.StartsWith("~/", StringComparison.Ordinal))
                    {
                        this.Package.IconFile = iconFileNav.Value;
                    }
                    else if (iconFileNav.Value.StartsWith("DesktopModules", StringComparison.OrdinalIgnoreCase))
                    {
                        this.Package.IconFile = $"~/{iconFileNav.Value}";
                    }
                    else
                    {
                        this.Package.IconFile = (string.IsNullOrEmpty(this.Package.FolderName) ? string.Empty : this.Package.FolderName + "/") + iconFileNav.Value;
                        this.Package.IconFile = (!this.Package.IconFile.StartsWith("~/", StringComparison.Ordinal)) ? "~/" + this.Package.IconFile : this.Package.IconFile;
                    }
                }
            }

            // Get Author
            XPathNavigator authorNav = manifestNav.SelectSingleNode("owner");
			Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): authorNav → {AuthorNav}", manifestNav, authorNav);
            if (authorNav != null)
            {
                this.Package.Owner = Util.ReadElement(authorNav, "name");
                this.Package.Organization = Util.ReadElement(authorNav, "organization");
                this.Package.Url = Util.ReadElement(authorNav, "url");
                this.Package.Email = Util.ReadElement(authorNav, "email");
            }

            // Get License
            XPathNavigator licenseNav = manifestNav.SelectSingleNode("license");
			Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): licenseNav → {LicenseNav}", manifestNav, licenseNav);
            if (licenseNav != null)
            {
                string licenseSrc = Util.ReadAttribute(licenseNav, "src");
                if (string.IsNullOrEmpty(licenseSrc))
                {
                    // Load from element
                    this.Package.License = licenseNav.Value;
                }
                else
                {
                    this.Package.License = this.ReadTextFromFile(licenseSrc);
                    if (this.Package.License == null)
                    {
                        // failure reading file
                        return;
                    }
                }
            }

            if (string.IsNullOrEmpty(this.Package.License))
            {
                // Legacy Packages have no license
                this.Package.License = Util.PACKAGE_NoLicense;
            }

            // Get Release Notes
            XPathNavigator relNotesNav = manifestNav.SelectSingleNode("releaseNotes");
			Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): relNotesNav → {RelNotesNav}", manifestNav, relNotesNav);
            if (relNotesNav != null)
            {
                string relNotesSrc = Util.ReadAttribute(relNotesNav, "src");
                if (string.IsNullOrEmpty(relNotesSrc))
                {
                    // Load from element
                    this.Package.ReleaseNotes = relNotesNav.Value;
                }
                else
                {
                    this.Package.ReleaseNotes = this.ReadTextFromFile(relNotesSrc);
                    if (this.Package.ReleaseNotes == null)
                    {
                        // failure reading file
                        return;
                    }
                }
            }

            if (string.IsNullOrEmpty(this.Package.ReleaseNotes))
            {
                // Legacy Packages have no Release Notes
                this.Package.ReleaseNotes = Util.PACKAGE_NoReleaseNotes;
            }

            // Parse the Dependencies
            var packageDependencies = this.Package.Dependencies;
            foreach (XPathNavigator dependencyNav in manifestNav.CreateNavigator().Select("dependencies/dependency"))
            {
                var dependency = DependencyFactory.GetDependency(dependencyNav);
				Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): dependency → {Dependency}", manifestNav, dependency);
                if (dependency is IManagedPackageDependency packageDependency)
                {
                    packageDependencies.Add(packageDependency.PackageDependency);
                }

                if (!dependency.IsValid)
                {
                    this.Log.AddFailure(dependency.ErrorMessage);
                    return;
                }
            }

            // Read Components
            this.ReadComponents(manifestNav);
        }

        /// <summary>The Rollback method rolls back the package installation.</summary>
        public override void Rollback()
        {
			Logger.LogDebug(">>> Entering Rollback()");
            for (int index = 0; index <= this.componentInstallers.Count - 1; index++)
            {
                ComponentInstallerBase compInstaller = this.componentInstallers.Values[index];
                if (compInstaller.Version > this.Package.InstalledVersion && compInstaller.Completed)
                {
                    this.Log.AddInfo(Util.COMPONENT_RollingBack + " - " + compInstaller.Type);
                    compInstaller.Rollback();
                    this.Log.AddInfo(Util.COMPONENT_RolledBack + " - " + compInstaller.Type);
                }
            }

            // If Previously Installed Package exists then we need to update the DataStore with this
            if (this.installedPackage == null)
            {
                // No Previously Installed Package - Delete newly added Package
                PackageController.Instance.DeleteExtensionPackage(this.Package);
            }
            else
            {
                // Previously Installed Package - Rollback to Previously Installed
                PackageController.Instance.SaveExtensionPackage(this.installedPackage);
            }
			Logger.LogDebug("<<< Exiting Rollback()");
        }

        /// <summary>The Uninstall method uninstalls the components of the package.</summary>
        public override void UnInstall()
        {
			Logger.LogDebug(">>> Entering UnInstall()");
            // Iterate through all the Components
            for (int index = 0; index <= this.componentInstallers.Count - 1; index++)
            {
                ComponentInstallerBase compInstaller = this.componentInstallers.Values[index];
                if (compInstaller is FileInstaller fileInstaller)
                {
                    fileInstaller.DeleteFiles = this.DeleteFiles;
					Logger.LogDebug("UnInstall(): fileInstaller.DeleteFiles → {Arg0}", fileInstaller.DeleteFiles);
                }

                this.Log.ResetFlags();
                this.Log.AddInfo(Util.UNINSTALL_StartComp + " - " + compInstaller.Type);
                compInstaller.UnInstall();
                this.Log.AddInfo(Util.COMPONENT_UnInstalled + " - " + compInstaller.Type);
                if (this.Log.Valid)
                {
                    this.Log.AddInfo(Util.UNINSTALL_SuccessComp + " - " + compInstaller.Type);
                }
                else
                {
                    this.Log.AddWarning(Util.UNINSTALL_WarningsComp + " - " + compInstaller.Type);
                }
            }

            // Remove the Package information from the Data Store
            PackageController.Instance.DeleteExtensionPackage(this.Package);
			Logger.LogDebug("<<< Exiting UnInstall()");
        }

        /// <summary>The CheckSecurity method checks whether the user has the appropriate security.</summary>
        private void CheckSecurity()
        {
			Logger.LogDebug(">>> Entering CheckSecurity()");
            PackageType type = PackageController.Instance.GetExtensionPackageType(t => t.PackageType.Equals(this.Package.PackageType, StringComparison.OrdinalIgnoreCase));
            if (type == null)
			Logger.LogDebug("CheckSecurity(): type → {Type}", type);
            {
                // This package type not registered
                this.Log.Logs.Clear();
                this.Log.AddFailure(Util.SECURITY_NotRegistered + " - " + this.Package.PackageType);
                this.IsValid = false;
				Logger.LogDebug("CheckSecurity(): this.IsValid → {Arg0}", this.IsValid);
            }
            else
            {
                if (type.SecurityAccessLevel > this.Package.InstallerInfo.SecurityAccessLevel)
                {
                    this.Log.Logs.Clear();
                    this.Log.AddFailure(Util.SECURITY_Installer);
                    this.IsValid = false;
                }
            }
			Logger.LogDebug("<<< Exiting CheckSecurity()");
        }

        /// <summary>The ReadComponents method reads the components node of the manifest file.</summary>
        private void ReadComponents(XPathNavigator manifestNav)
        {
			Logger.LogDebug(">>> Entering ReadComponents(manifestNav={ManifestNav})", manifestNav);
            foreach (XPathNavigator componentNav in manifestNav.CreateNavigator().Select("components/component"))
            {
                // Set default order to next value (ie the same as the size of the collection)
                int order = this.componentInstallers.Count;
				Logger.LogDebug("ReadComponents(manifestNav={ManifestNav}): order → {Order}", manifestNav, order);

                string type = componentNav.GetAttribute("type", string.Empty);
				Logger.LogDebug("ReadComponents(manifestNav={ManifestNav}): type → {Type}", manifestNav, type);
                if (this.InstallMode == InstallMode.Install)
                {
                    string installOrder = componentNav.GetAttribute("installOrder", string.Empty);
                    if (!string.IsNullOrEmpty(installOrder))
                    {
                        order = int.Parse(installOrder, CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    string unInstallOrder = componentNav.GetAttribute("unInstallOrder", string.Empty);
                    if (!string.IsNullOrEmpty(unInstallOrder))
                    {
                        order = int.Parse(unInstallOrder, CultureInfo.InvariantCulture);
                    }
                }

                if (this.Package.InstallerInfo != null)
                {
                    this.Log.AddInfo(Util.DNN_ReadingComponent + " - " + type);
                }

                ComponentInstallerBase installer = InstallerFactory.GetInstaller(componentNav, this.Package);
				Logger.LogDebug("ReadComponents(manifestNav={ManifestNav}): installer → {Installer}", manifestNav, installer);
                if (installer == null)
                {
                    this.Log.AddFailure(Util.EXCEPTION_InstallerCreate);
                }
                else
                {
                    this.componentInstallers.Add(order, installer);
                    this.Package.InstallerInfo.AllowableFiles += ", " + installer.AllowableFiles;
                }
            }
			Logger.LogDebug("<<< Exiting ReadComponents(manifestNav={ManifestNav})", manifestNav);
        }

        private string ReadTextFromFile(string source)
        {
			Logger.LogDebug(">>> Entering ReadTextFromFile(source={Source})", source);
            if (this.Package.InstallerInfo.InstallMode == InstallMode.ManifestOnly)
            {
                return Null.NullString;
            }

            try
            {
                return FileSystemUtils.ReadFile(this.Package.InstallerInfo.TempInstallFolder + @"\" + source);
            }
            catch (PathNotFoundException)
            {
                this.Log.AddFailure($"Unable to read {source}, file not found");
				Logger.LogDebug("<<< Exiting ReadTextFromFile(source={Source})", source);
                return null;
            }
        }
    }
}
