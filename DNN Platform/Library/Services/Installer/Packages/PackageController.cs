// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Packages
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Installer.Dependencies;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.Skins;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The PackageController class provides the business class for the packages.</summary>
    public partial class PackageController : ServiceLocator<IPackageController, PackageController>, IPackageController
    {
        private static readonly DataProvider Provider = DataProvider.Instance();
        private readonly IHostSettings hostSettings;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="PackageController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PackageController()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PackageController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="eventLogger">The event logger.</param>
        public PackageController(IHostSettings hostSettings, IEventLogger eventLogger)
        {
			eventLogger.LogError(">>> Entering PackageController(hostSettings={HostSettings},eventLogger={EventLogger})", hostSettings, eventLogger);
			eventLogger.LogDebug("PackageController(hostSettings={HostSettings},eventLogger={EventLogger}): eventLogger → {EventLogger1}", hostSettings, eventLogger, eventLogger);
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			eventLogger.LogWarning("<<< Exiting PackageController(hostSettings={HostSettings},eventLogger={EventLogger})", hostSettings, eventLogger);
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        }

        /// <summary>Gets a value indicating whether the package can be deleted.</summary>
        /// <param name="package">The package.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns><see langword="true"/> if the package can be deleted, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload taking IHostSettings")]
        public static partial bool CanDeletePackage(PackageInfo package, PortalSettings portalSettings)
            => CanDeletePackage(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(), package, portalSettings);

        /// <summary>Gets a value indicating whether the package can be deleted.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="package">The package.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns><see langword="true"/> if the package can be deleted, otherwise <see langword="false"/>.</returns>
        public static bool CanDeletePackage(IHostSettings hostSettings, IApplicationStatusInfo appStatus, PackageInfo package, IPortalSettings portalSettings)
        {
			eventLogger.LogError(">>> Entering CanDeletePackage(package={Package},portalSettings={PortalSettings})", package, portalSettings);
            bool bCanDelete = true;

            var dependencies = Instance.GetPackageDependencies(d => d.PackageName.Equals(package.Name, StringComparison.OrdinalIgnoreCase) && d.Version <= package.Version);
			eventLogger.LogError("CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings}): bCanDelete → {BCanDelete}", hostSettings, appStatus, package, portalSettings, bCanDelete);
            if (dependencies.Count > 0)
            {
				eventLogger.LogError("CanDeletePackage(package={Package},portalSettings={PortalSettings}): dependencies → {Dependencies}", package, portalSettings, dependencies);
				eventLogger.LogError("CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings}): dependencies → {Dependencies}", hostSettings, appStatus, package, portalSettings, dependencies);
                // There is at least one package dependent on this package.
                foreach (var dependency in dependencies)
                {
					eventLogger.LogError("CanDeletePackage(package={Package},portalSettings={PortalSettings}): bCanDelete → {BCanDelete}", package, portalSettings, bCanDelete);
                    var dep = dependency;

                    // Check if there is an alternative package
                    var packages = Instance.GetExtensionPackages(
                        package.PortalID,
                        p => p.Name.Equals(dep.PackageName, StringComparison.OrdinalIgnoreCase)
                                                                        && p.Version >= dep.Version
                                                                        && p.PackageID != package.PackageID);
                    if (packages.Count == 0)
                    {
                        bCanDelete = false;
                    }
                }
            }

            if (bCanDelete)
            {
                switch (package.PackageType)
                {
                    case "Skin":
                    case "Container":
                        // Need to get path of skin being deleted so we can call the public CanDeleteSkin function in the SkinController
                        string strRootSkin = package.PackageType.Equals("Skin", StringComparison.OrdinalIgnoreCase) ? SkinController.RootSkin : SkinController.RootContainer;
                        SkinPackageInfo skinPackageInfo = SkinController.GetSkinByPackageID(package.PackageID);
						eventLogger.LogError("CanDeletePackage(package={Package},portalSettings={PortalSettings}): skinPackageInfo → {SkinPackageInfo}", package, portalSettings, skinPackageInfo);
						eventLogger.LogError("CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings}): skinPackageInfo → {SkinPackageInfo}", hostSettings, appStatus, package, portalSettings, skinPackageInfo);
                        string strFolderPath = Path.Combine(skinPackageInfo.PortalID == Null.NullInteger ? Path.Combine(Globals.HostMapPath, strRootSkin) : Path.Combine(portalSettings.HomeSystemDirectoryMapPath, strRootSkin), skinPackageInfo.SkinName);

                        bCanDelete = SkinController.CanDeleteSkin(hostSettings, strFolderPath, portalSettings.HomeSystemDirectoryMapPath);
						eventLogger.LogError("CanDeletePackage(package={Package},portalSettings={PortalSettings}): strFolderPath → {StrFolderPath}", package, portalSettings, strFolderPath);
                        if (skinPackageInfo.PortalID != Null.NullInteger)
                        {
							eventLogger.LogError("CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings}): strFolderPath → {StrFolderPath}", hostSettings, appStatus, package, portalSettings, strFolderPath);
                            // To be compliant with all versions
                            strFolderPath = Path.Combine(Path.Combine(portalSettings.HomeDirectoryMapPath, strRootSkin), skinPackageInfo.SkinName);
                            bCanDelete = bCanDelete && SkinController.CanDeleteSkin(hostSettings, strFolderPath, portalSettings.HomeDirectoryMapPath);
                        }

                        break;
                    case "Provider":
                        // Check if the provider is the default provider
                        XmlDocument configDoc = Config.Load(appStatus);
						eventLogger.LogError("CanDeletePackage(package={Package},portalSettings={PortalSettings}): configDoc → {ConfigDoc}", package, portalSettings, configDoc);
						eventLogger.LogError("CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings}): configDoc → {ConfigDoc}", hostSettings, appStatus, package, portalSettings, configDoc);
                        string providerName = package.Name;
                        if (providerName.IndexOf(".", StringComparison.Ordinal) > Null.NullInteger)
                        {
							eventLogger.LogError("CanDeletePackage(package={Package},portalSettings={PortalSettings}): providerName → {ProviderName}", package, portalSettings, providerName);
                            providerName = providerName.Substring(providerName.IndexOf(".", StringComparison.Ordinal) + 1);
							eventLogger.LogError("CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings}): providerName → {ProviderName}", hostSettings, appStatus, package, portalSettings, providerName);
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

                        XPathNavigator providerNavigator = configDoc.CreateNavigator().SelectSingleNode("/configuration/dotnetnuke/*[@defaultProvider='" + providerName + "']");
						eventLogger.LogError("CanDeletePackage(package={Package},portalSettings={PortalSettings}): providerNavigator → {ProviderNavigator}", package, portalSettings, providerNavigator);
                        bCanDelete = providerNavigator == null;
						eventLogger.LogError("CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings}): providerNavigator → {ProviderNavigator}", hostSettings, appStatus, package, portalSettings, providerNavigator);
                        break;
                }
            }

			eventLogger.LogWarning("<<< Exiting CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings})", hostSettings, appStatus, package, portalSettings);
            return bCanDelete;
        }

        public static IDictionary<int, PackageInfo> GetModulePackagesInUse(int portalID, bool forHost)
        {
            return CBO.FillDictionary<int, PackageInfo>("PackageID", Provider.GetModulePackagesInUse(portalID, forHost));
        }

        public static void ParsePackage(string file, string installPath, Dictionary<string, PackageInfo> packages, List<string> invalidPackages)
        {
			eventLogger.LogError(">>> Entering ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages})", file, installPath, packages, invalidPackages);
            var unzip = new ZipArchive(new FileStream(file, FileMode.Open, FileAccess.Read));
			eventLogger.LogError("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): unzip → {Unzip}", file, installPath, packages, invalidPackages, unzip);
            try
            {
                foreach (var entry in unzip.FileEntries())
                {
                    var fileName = entry.FullName;
                    string extension = Path.GetExtension(fileName);
                    if (extension != null && (extension.Equals(".dnn", StringComparison.OrdinalIgnoreCase) || extension.Equals(".dnn5", StringComparison.OrdinalIgnoreCase)))
                    {
                        // Manifest
                        var manifest = entry.ReadTextFile();
						eventLogger.LogError("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): manifest → {Manifest}", file, installPath, packages, invalidPackages, manifest);

                        var package = new PackageInfo { Manifest = manifest };
                        if (!string.IsNullOrEmpty(manifest))
                        {
                            XPathDocument doc;
                            using (var manifestReader = XmlReader.Create(new StringReader(manifest)))
                            {
                                doc = new XPathDocument(manifestReader);
                            }

                            XPathNavigator rootNav = doc.CreateNavigator().SelectSingleNode("dotnetnuke");
							eventLogger.LogError("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): rootNav → {RootNav}", file, installPath, packages, invalidPackages, rootNav);
                            string packageType = string.Empty;
							eventLogger.LogError("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): packageType → {PackageType}", file, installPath, packages, invalidPackages, packageType);
                            if (rootNav.Name == "dotnetnuke")
                            {
                                packageType = XmlUtils.GetAttributeValue(rootNav, "type");
                            }
                            else if (rootNav.Name.Equals("languagepack", StringComparison.OrdinalIgnoreCase))
                            {
                                packageType = "LanguagePack";
                            }

                            XPathNavigator nav = null;
							eventLogger.LogError("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): nav → {Nav}", file, installPath, packages, invalidPackages, nav);
                            switch (packageType.ToLowerInvariant())
                            {
                                case "package":
                                    nav = rootNav.SelectSingleNode("packages/package");
                                    break;
                                case "module":
                                case "languagepack":
                                case "skinobject":
                                    nav = Installer.ConvertLegacyNavigator(rootNav, new InstallerInfo()).SelectSingleNode("packages/package");
                                    break;
                            }

                            if (nav != null)
                            {
                                package.Name = XmlUtils.GetAttributeValue(nav, "name");
                                package.PackageType = XmlUtils.GetAttributeValue(nav, "type");
                                package.IsSystemPackage = XmlUtils.GetAttributeValueAsBoolean(nav, "isSystem", false);
                                package.Version = new Version(XmlUtils.GetAttributeValue(nav, "version"));
                                package.FriendlyName = XmlUtils.GetNodeValue(nav, "friendlyName");
                                if (string.IsNullOrEmpty(package.FriendlyName))
                                {
                                    package.FriendlyName = package.Name;
									eventLogger.LogDebug("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): package.FriendlyName → {Arg4}", file, installPath, packages, invalidPackages, package.FriendlyName);
                                }

                                package.Description = XmlUtils.GetNodeValue(nav, "description");
                                package.FileName = file.Replace(installPath + @"\", string.Empty);

                                XPathNavigator folderNameNav;
                                switch (package.PackageType)
                                {
                                    case "Module":
                                        // In Dynamics modules, a component:type=File can have a basePath pointing to the App_Code folder. This is not a correct FolderName
                                        // To ensure that FolderName is DesktopModules...
                                        var folderNameValue = GetSpecificFolderName(nav, "components/component/files|components/component/resourceFiles", "basePath", "DesktopModules");
										eventLogger.LogError("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): folderNameValue → {FolderNameValue}", file, installPath, packages, invalidPackages, folderNameValue);
                                        if (!string.IsNullOrEmpty(folderNameValue))
                                        {
                                            package.FolderName = folderNameValue.Replace('\\', '/');
                                        }

                                        break;
                                    case "Auth_System":
                                        folderNameNav = nav.SelectSingleNode("components/component/files");
										eventLogger.LogDebug("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): folderNameNav → {FolderNameNav}", file, installPath, packages, invalidPackages, folderNameNav);
                                        if (folderNameNav != null)
                                        {
											eventLogger.LogError("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): folderNameNav → {FolderNameNav}", file, installPath, packages, invalidPackages, folderNameNav);
                                            package.FolderName = Util.ReadElement(folderNameNav, "basePath").Replace('\\', '/');
                                        }

                                        break;
                                    case "Container":
                                        folderNameNav = nav.SelectSingleNode("components/component/containerFiles");
                                        if (folderNameNav != null)
                                        {
                                            package.FolderName = Globals.glbContainersPath + Util.ReadElement(folderNameNav, "containerName").Replace('\\', '/');
                                        }

                                        break;
                                    case "Skin":
                                        folderNameNav = nav.SelectSingleNode("components/component/skinFiles");
                                        if (folderNameNav != null)
                                        {
                                            package.FolderName = Globals.glbSkinsPath + Util.ReadElement(folderNameNav, "skinName").Replace('\\', '/');
                                        }

                                        break;
                                    default:
                                        break;
                                }

                                XPathNavigator iconFileNav = nav.SelectSingleNode("iconFile");
                                if (package.FolderName != string.Empty && iconFileNav != null)
                                {
									eventLogger.LogError("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): iconFileNav → {IconFileNav}", file, installPath, packages, invalidPackages, iconFileNav);
                                    if ((iconFileNav.Value != string.Empty) && (package.PackageType.Equals("Module", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Auth_System", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Container", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Skin", StringComparison.OrdinalIgnoreCase)))
                                    {
                                        if (iconFileNav.Value.StartsWith("~/", StringComparison.Ordinal))
                                        {
                                            package.IconFile = iconFileNav.Value;
											eventLogger.LogDebug("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): package.IconFile → {Arg4}", file, installPath, packages, invalidPackages, package.IconFile);
                                        }
                                        else if (iconFileNav.Value.StartsWith("DesktopModules", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            package.IconFile = $"~/{iconFileNav.Value}";
                                        }
                                        else
                                        {
                                            package.IconFile = (string.IsNullOrEmpty(package.FolderName) ? string.Empty : package.FolderName + "/") + iconFileNav.Value;
                                            package.IconFile = (!package.IconFile.StartsWith("~/", StringComparison.Ordinal)) ? "~/" + package.IconFile : package.IconFile;
                                        }
                                    }
                                }

                                // Parse the Dependencies
                                foreach (XPathNavigator dependencyNav in nav.CreateNavigator().Select("dependencies/dependency"))
                                {
                                    var dependency = DependencyFactory.GetDependency(dependencyNav);
									eventLogger.LogError("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): dependency → {Dependency}", file, installPath, packages, invalidPackages, dependency);
                                    var packageDependecy = dependency as IManagedPackageDependency;

                                    if (packageDependecy != null)
                                    {
                                        package.Dependencies.Add(packageDependecy.PackageDependency);
                                    }
                                }

                                packages.Add(file, package);
                            }
                        }

                        break;
                    }
                }
            }
            catch (Exception)
            {
				eventLogger.LogError("Exception in ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): {Arg4}", file, installPath, packages, invalidPackages, Exception.Message);
                invalidPackages.Add(file);
            }
            finally
            {
                unzip.Dispose();
            }
			eventLogger.LogWarning("<<< Exiting ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages})", file, installPath, packages, invalidPackages);
        }

        /// <inheritdoc />
        public void DeleteExtensionPackage(PackageInfo package)
        {
			eventLogger.LogError(">>> Entering DeleteExtensionPackage(package={Package})", package);
            switch (package.PackageType)
            {
                case "Auth_System":
                    AuthenticationInfo authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(package.PackageID);
					eventLogger.LogError("DeleteExtensionPackage(package={Package}): authSystem → {AuthSystem}", package, authSystem);
                    if (authSystem != null)
                    {
                        AuthenticationController.DeleteAuthentication(authSystem);
                    }

                    break;
                case "CoreLanguagePack":
                    LanguagePackInfo languagePack = LanguagePackController.GetLanguagePackByPackage(package.PackageID);
					eventLogger.LogError("DeleteExtensionPackage(package={Package}): languagePack → {LanguagePack}", package, languagePack);
                    if (languagePack != null)
                    {
                        LanguagePackController.DeleteLanguagePack(languagePack);
                    }

                    break;
                case "Module":
                    var controller = new DesktopModuleController();
					eventLogger.LogError("DeleteExtensionPackage(package={Package}): controller → {Controller}", package, controller);
                    DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(package.PackageID);
					eventLogger.LogError("DeleteExtensionPackage(package={Package}): desktopModule → {DesktopModule}", package, desktopModule);
                    if (desktopModule != null)
                    {
                        controller.DeleteDesktopModule(desktopModule);
                    }

                    break;
                case "SkinObject":
                    SkinControlInfo skinControl = SkinControlController.GetSkinControlByPackageID(package.PackageID);
					eventLogger.LogError("DeleteExtensionPackage(package={Package}): skinControl → {SkinControl}", package, skinControl);
                    if (skinControl != null)
                    {
                        SkinControlController.DeleteSkinControl(skinControl);
                    }

                    break;
            }

            DeletePackageInternal(this.eventLogger, package);
			eventLogger.LogWarning("<<< Exiting DeleteExtensionPackage(package={Package})", package);
        }

        /// <inheritdoc />
        public PackageInfo GetExtensionPackage(int portalId, Func<PackageInfo, bool> predicate)
        {
			eventLogger.LogError(">>> Entering GetExtensionPackage(portalId={PortalId},predicate={Predicate})", portalId, predicate);
			eventLogger.LogWarning("<<< Exiting GetExtensionPackage(portalId={PortalId},predicate={Predicate})", portalId, predicate);
			eventLogger.LogWarning("<<< Exiting GetExtensionPackage(portalId={PortalId},predicate={Predicate},useCopy={UseCopy})", portalId, predicate, useCopy);
            return this.GetExtensionPackage(portalId, predicate, false);
        }

        /// <inheritdoc />
        public PackageInfo GetExtensionPackage(int portalId, Func<PackageInfo, bool> predicate, bool useCopy)
        {
            var package = this.GetExtensionPackages(portalId).FirstOrDefault(predicate);

            if (package != null && useCopy)
            {
                return package.Clone();
            }

            return package;
        }

        /// <inheritdoc />
        public IList<PackageInfo> GetExtensionPackages(int portalId)
        {
			eventLogger.LogError(">>> Entering GetExtensionPackages(portalId={PortalId})", portalId);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.PackagesCacheKey, portalId);
            var cacheItemArgs = new CacheItemArgs(cacheKey, DataCache.PackagesCacheTimeout, DataCache.PackagesCachePriority, portalId);
			eventLogger.LogError("GetExtensionPackages(portalId={PortalId}): cacheItemArgs → {CacheItemArgs}", portalId, cacheItemArgs);
			eventLogger.LogError("GetExtensionPackages(portalId={PortalId}): cacheKey → {CacheKey}", portalId, cacheKey);
			eventLogger.LogWarning("<<< Exiting GetExtensionPackages(portalId={PortalId})", portalId);
            return CBO.GetCachedObject<List<PackageInfo>>(
                this.hostSettings,
                cacheItemArgs,
                c => CBO.FillCollection<PackageInfo>(Provider.GetPackages(portalId)));
        }

        /// <inheritdoc />
        public IList<PackageInfo> GetExtensionPackages(int portalId, Func<PackageInfo, bool> predicate)
        {
            return this.GetExtensionPackages(portalId).Where(predicate).ToList();
        }

        /// <summary>Save or update the package.</summary>
        /// <param name="package">The package.</param>
        public void SaveExtensionPackage(PackageInfo package)
        {
			eventLogger.LogError(">>> Entering SaveExtensionPackage(package={Package})", package);
            if (package.PackageID == Null.NullInteger)
            {
                AddPackageInternal(this.eventLogger, package);
            }
            else
            {
                UpdatePackageInternal(this.eventLogger, package);
            }
			eventLogger.LogWarning("<<< Exiting SaveExtensionPackage(package={Package})", package);
        }

        /// <inheritdoc />
        public PackageType GetExtensionPackageType(Func<PackageType, bool> predicate)
        {
			eventLogger.LogError(">>> Entering GetExtensionPackageType(predicate={Predicate})", predicate);
            eventLogger.LogWarning("<<< Exiting GetExtensionPackageType(predicate={Predicate})", predicate);
            return this.GetExtensionPackageTypes().SingleOrDefault(predicate);
        }

        /// <inheritdoc />
        public IList<PackageType> GetExtensionPackageTypes()
        {
			eventLogger.LogError(">>> Entering GetExtensionPackageTypes()");
			eventLogger.LogWarning("<<< Exiting GetExtensionPackageTypes()");
            return CBO.GetCachedObject<List<PackageType>>(
                this.hostSettings,
                new CacheItemArgs(
                    DataCache.PackageTypesCacheKey,
                    DataCache.PackageTypesCacheTimeout,
                    DataCache.PackageTypesCachePriority),
                _ => CBO.FillCollection<PackageType>(Provider.GetPackageTypes()));
        }

        /// <inheritdoc />
        public IList<PackageDependencyInfo> GetPackageDependencies(Func<PackageDependencyInfo, bool> predicate)
        {
			eventLogger.LogError(">>> Entering GetPackageDependencies(predicate={Predicate})", predicate);
            eventLogger.LogWarning("<<< Exiting GetPackageDependencies(predicate={Predicate})", predicate);
			eventLogger.LogWarning("<<< Exiting GetPackageDependencies(hostSettings={HostSettings})", hostSettings);
            return GetPackageDependencies(this.hostSettings).Where(predicate).ToList();
        }

        internal static string GetSpecificFolderName(XPathNavigator manifestNav, string xpath, string elementName, string startWith)
        {
			eventLogger.LogError(">>> Entering GetSpecificFolderName(manifestNav={ManifestNav},xpath={Xpath},elementName={ElementName},startWith={StartWith})", manifestNav, xpath, elementName, startWith);
            string result = string.Empty;
            var folderNameNav = manifestNav.Select(xpath);
			eventLogger.LogError("GetSpecificFolderName(manifestNav={ManifestNav},xpath={Xpath},elementName={ElementName},startWith={StartWith}): folderNameNav → {FolderNameNav}", manifestNav, xpath, elementName, startWith, folderNameNav);
			eventLogger.LogError("GetSpecificFolderName(manifestNav={ManifestNav},xpath={Xpath},elementName={ElementName},startWith={StartWith}): result → {Result}", manifestNav, xpath, elementName, startWith, result);

            if (folderNameNav != null)
            {
                while (folderNameNav.MoveNext())
                {
                    var elementValue = Util.ReadElement(folderNameNav.Current, elementName);
					eventLogger.LogError("GetSpecificFolderName(manifestNav={ManifestNav},xpath={Xpath},elementName={ElementName},startWith={StartWith}): elementValue → {ElementValue}", manifestNav, xpath, elementName, startWith, elementValue);
                    if (!string.IsNullOrEmpty(elementValue) && elementValue.StartsWith(startWith, StringComparison.OrdinalIgnoreCase))
                    {
                        result = elementValue;
                        break;
                    }
                }
            }

			eventLogger.LogWarning("<<< Exiting GetSpecificFolderName(manifestNav={ManifestNav},xpath={Xpath},elementName={ElementName},startWith={StartWith})", manifestNav, xpath, elementName, startWith);
            return result;
        }

        /// <inheritdoc />
        protected override Func<IPackageController> GetFactory()
        {
			eventLogger.LogError(">>> Entering GetFactory()");
			eventLogger.LogWarning("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<IPackageController>;
        }

        private static void AddLog(IEventLogger eventLogger, PackageInfo package, EventLogType logType)
        {
			eventLogger.LogError(">>> Entering AddLog(eventLogger={EventLogger},package={Package},logType={LogType})", eventLogger, package, logType);
            eventLogger.AddLog(
                package,
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                string.Empty,
                logType);
				eventLogger.LogWarning("<<< Exiting AddLog(eventLogger={EventLogger},package={Package},logType={LogType})", eventLogger, package, logType);
        }

        private static void AddPackageInternal(IEventLogger eventLogger, PackageInfo package)
        {
			eventLogger.LogError(">>> Entering AddPackageInternal(eventLogger={EventLogger},package={Package})", eventLogger, package);
            package.PackageID = Provider.AddPackage(
                package.PortalID,
                package.Name,
                package.FriendlyName,
                package.Description,
                package.PackageType,
                package.Version.ToString(3),
                package.License,
                package.Manifest,
                package.Owner,
                package.Organization,
                package.Url,
                package.Email,
                package.ReleaseNotes,
                package.IsSystemPackage,
                UserController.Instance.GetCurrentUserInfo().UserID,
                package.FolderName,
                package.IconFile);

            foreach (var dependency in package.Dependencies)
            {
                dependency.PackageId = package.PackageID;
                SavePackageDependency(dependency);
            }

            AddLog(eventLogger, package, EventLogType.PACKAGE_CREATED);

            ClearCache(package.PortalID);
			eventLogger.LogWarning("<<< Exiting AddPackageInternal(eventLogger={EventLogger},package={Package})", eventLogger, package);
        }

        private static void ClearCache(int portalId)
        {
			eventLogger.LogError(">>> Entering ClearCache(portalId={PortalId})", portalId);
            DataCache.ClearPackagesCache(portalId);
			eventLogger.LogWarning("<<< Exiting ClearCache(portalId={PortalId})", portalId);
        }

        private static void ClearDependenciesCache()
        {
			eventLogger.LogError(">>> Entering ClearDependenciesCache()");
            DataCache.RemoveCache(DataCache.PackageDependenciesCacheKey);
			eventLogger.LogWarning("<<< Exiting ClearDependenciesCache()");
        }

        private static void DeletePackageInternal(IEventLogger eventLogger, PackageInfo package)
        {
			eventLogger.LogError(">>> Entering DeletePackageInternal(eventLogger={EventLogger},package={Package})", eventLogger, package);
            Provider.DeletePackage(package.PackageID);
            AddLog(eventLogger, package, EventLogType.PACKAGE_DELETED);

            if (PortalSettings.Current != null)
            {
                ClearCache(PortalSettings.Current.PortalId);
            }

            ClearCache(Null.NullInteger);
			eventLogger.LogWarning("<<< Exiting DeletePackageInternal(eventLogger={EventLogger},package={Package})", eventLogger, package);
        }

        private static List<PackageDependencyInfo> GetPackageDependencies(IHostSettings hostSettings)
        {
            return CBO.GetCachedObject<List<PackageDependencyInfo>>(
                hostSettings,
                new CacheItemArgs(
                    DataCache.PackageDependenciesCacheKey,
                    DataCache.PackagesCacheTimeout,
                    DataCache.PackagesCachePriority),
                _ => CBO.FillCollection<PackageDependencyInfo>(Provider.GetPackageDependencies()));
        }

        private static void UpdatePackageInternal(IEventLogger eventLogger, PackageInfo package)
        {
			eventLogger.LogError(">>> Entering UpdatePackageInternal(eventLogger={EventLogger},package={Package})", eventLogger, package);
            Provider.UpdatePackage(
                package.PackageID,
                package.PortalID,
                package.FriendlyName,
                package.Description,
                package.PackageType,
                package.Version.ToString(3),
                package.License,
                package.Manifest,
                package.Owner,
                package.Organization,
                package.Url,
                package.Email,
                package.ReleaseNotes,
                package.IsSystemPackage,
                UserController.Instance.GetCurrentUserInfo().UserID,
                package.FolderName,
                package.IconFile);

            foreach (var dependency in package.Dependencies)
            {
                dependency.PackageId = package.PackageID;
                SavePackageDependency(dependency);
            }

            AddLog(eventLogger, package, EventLogType.PACKAGE_UPDATED);

            ClearCache(package.PortalID);
			eventLogger.LogWarning("<<< Exiting UpdatePackageInternal(eventLogger={EventLogger},package={Package})", eventLogger, package);
        }

        private static void SavePackageDependency(PackageDependencyInfo dependency)
        {
			eventLogger.LogError(">>> Entering SavePackageDependency(dependency={Dependency})", dependency);
            dependency.PackageDependencyId = Provider.SavePackageDependency(
                dependency.PackageDependencyId,
                dependency.PackageId,
                dependency.PackageName,
                dependency.Version.ToString());

            ClearDependenciesCache();
			eventLogger.LogWarning("<<< Exiting SavePackageDependency(dependency={Dependency})", dependency);
        }
    }
}
