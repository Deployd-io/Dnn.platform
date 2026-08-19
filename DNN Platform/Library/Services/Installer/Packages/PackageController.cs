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
		eventLogger.LogDebug(">>> Entering PackageController()");
		eventLogger.LogDebug("<<< Exiting PackageController()");
		eventLogger.LogDebug(">>> Entering PackageController(hostSettings={HostSettings},eventLogger={EventLogger})", hostSettings, eventLogger);
		eventLogger.LogDebug("PackageController(hostSettings={HostSettings},eventLogger={EventLogger}): eventLogger → {EventLogger1}", hostSettings, eventLogger, eventLogger);
        }

        /// <summary>Initializes a new instance of the <see cref="PackageController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="eventLogger">The event logger.</param>
        public PackageController(IHostSettings hostSettings, IEventLogger eventLogger)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
			eventLogger.LogDebug("<<< Exiting PackageController(hostSettings={HostSettings},eventLogger={EventLogger})", hostSettings, eventLogger);
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
			eventLogger.LogDebug(">>> Entering CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings})", hostSettings, appStatus, package, portalSettings);
            bool bCanDelete = true;

			eventLogger.LogDebug("CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings}): bCanDelete → {BCanDelete}", hostSettings, appStatus, package, portalSettings, bCanDelete);
            var dependencies = Instance.GetPackageDependencies(d => d.PackageName.Equals(package.Name, StringComparison.OrdinalIgnoreCase) && d.Version <= package.Version);
			eventLogger.LogDebug("CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings}): dependencies → {Dependencies}", hostSettings, appStatus, package, portalSettings, dependencies);
            if (dependencies.Count > 0)
            {
                // There is at least one package dependent on this package.
                foreach (var dependency in dependencies)
                {
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
						eventLogger.LogDebug("CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings}): skinPackageInfo → {SkinPackageInfo}", hostSettings, appStatus, package, portalSettings, skinPackageInfo);
                        string strFolderPath = Path.Combine(skinPackageInfo.PortalID == Null.NullInteger ? Path.Combine(Globals.HostMapPath, strRootSkin) : Path.Combine(portalSettings.HomeSystemDirectoryMapPath, strRootSkin), skinPackageInfo.SkinName);
						eventLogger.LogDebug("CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings}): strFolderPath → {StrFolderPath}", hostSettings, appStatus, package, portalSettings, strFolderPath);

                        bCanDelete = SkinController.CanDeleteSkin(hostSettings, strFolderPath, portalSettings.HomeSystemDirectoryMapPath);
                        if (skinPackageInfo.PortalID != Null.NullInteger)
                        {
                            // To be compliant with all versions
                            strFolderPath = Path.Combine(Path.Combine(portalSettings.HomeDirectoryMapPath, strRootSkin), skinPackageInfo.SkinName);
                            bCanDelete = bCanDelete && SkinController.CanDeleteSkin(hostSettings, strFolderPath, portalSettings.HomeDirectoryMapPath);
                        }

                        break;
                    case "Provider":
                        // Check if the provider is the default provider
                        XmlDocument configDoc = Config.Load(appStatus);
						eventLogger.LogDebug("CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings}): configDoc → {ConfigDoc}", hostSettings, appStatus, package, portalSettings, configDoc);
                        string providerName = package.Name;
						eventLogger.LogDebug("CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings}): providerName → {ProviderName}", hostSettings, appStatus, package, portalSettings, providerName);
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

                        XPathNavigator providerNavigator = configDoc.CreateNavigator().SelectSingleNode("/configuration/dotnetnuke/*[@defaultProvider='" + providerName + "']");
						eventLogger.LogDebug("CanDeletePackage(hostSettings={HostSettings},appStatus={AppStatus},package={Package},portalSettings={PortalSettings}): providerNavigator → {ProviderNavigator}", hostSettings, appStatus, package, portalSettings, providerNavigator);
                        bCanDelete = providerNavigator == null;
                        break;
                }
            }

			eventLogger.LogDebug("<<< Exiting CanDeletePackage(package={Package},portalSettings={PortalSettings})", package, portalSettings);
            return bCanDelete;
        }

        public static IDictionary<int, PackageInfo> GetModulePackagesInUse(int portalID, bool forHost)
        {
            return CBO.FillDictionary<int, PackageInfo>("PackageID", Provider.GetModulePackagesInUse(portalID, forHost));
        }

        public static void ParsePackage(string file, string installPath, Dictionary<string, PackageInfo> packages, List<string> invalidPackages)
        {
			eventLogger.LogDebug(">>> Entering ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages})", file, installPath, packages, invalidPackages);
            var unzip = new ZipArchive(new FileStream(file, FileMode.Open, FileAccess.Read));
            try
			eventLogger.LogDebug("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): unzip → {Unzip}", file, installPath, packages, invalidPackages, unzip);
            {
                foreach (var entry in unzip.FileEntries())
                {
                    var fileName = entry.FullName;
                    string extension = Path.GetExtension(fileName);
                    if (extension != null && (extension.Equals(".dnn", StringComparison.OrdinalIgnoreCase) || extension.Equals(".dnn5", StringComparison.OrdinalIgnoreCase)))
                    {
                        // Manifest
                        var manifest = entry.ReadTextFile();
						eventLogger.LogDebug("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): manifest → {Manifest}", file, installPath, packages, invalidPackages, manifest);

                        var package = new PackageInfo { Manifest = manifest };
                        if (!string.IsNullOrEmpty(manifest))
                        {
                            XPathDocument doc;
                            using (var manifestReader = XmlReader.Create(new StringReader(manifest)))
                            {
                                doc = new XPathDocument(manifestReader);
                            }

                            XPathNavigator rootNav = doc.CreateNavigator().SelectSingleNode("dotnetnuke");
							eventLogger.LogDebug("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): rootNav → {RootNav}", file, installPath, packages, invalidPackages, rootNav);
                            string packageType = string.Empty;
							eventLogger.LogDebug("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): packageType → {PackageType}", file, installPath, packages, invalidPackages, packageType);
                            if (rootNav.Name == "dotnetnuke")
                            {
                                packageType = XmlUtils.GetAttributeValue(rootNav, "type");
                            }
                            else if (rootNav.Name.Equals("languagepack", StringComparison.OrdinalIgnoreCase))
                            {
                                packageType = "LanguagePack";
                            }

                            XPathNavigator nav = null;
							eventLogger.LogDebug("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): nav → {Nav}", file, installPath, packages, invalidPackages, nav);
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
										eventLogger.LogDebug("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): folderNameValue → {FolderNameValue}", file, installPath, packages, invalidPackages, folderNameValue);
                                        if (!string.IsNullOrEmpty(folderNameValue))
                                        {
                                            package.FolderName = folderNameValue.Replace('\\', '/');
                                        }

                                        break;
                                    case "Auth_System":
                                        folderNameNav = nav.SelectSingleNode("components/component/files");
                                        if (folderNameNav != null)
										eventLogger.LogDebug("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): folderNameNav → {FolderNameNav}", file, installPath, packages, invalidPackages, folderNameNav);
                                        {
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
									eventLogger.LogDebug("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): iconFileNav → {IconFileNav}", file, installPath, packages, invalidPackages, iconFileNav);
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
									eventLogger.LogDebug("ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages}): dependency → {Dependency}", file, installPath, packages, invalidPackages, dependency);
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
			eventLogger.LogDebug("<<< Exiting ParsePackage(file={File},installPath={InstallPath},packages={Packages},invalidPackages={InvalidPackages})", file, installPath, packages, invalidPackages);
        }

        /// <inheritdoc />
        public void DeleteExtensionPackage(PackageInfo package)
        {
			eventLogger.LogDebug(">>> Entering DeleteExtensionPackage(package={Package})", package);
            switch (package.PackageType)
            {
                case "Auth_System":
                    AuthenticationInfo authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(package.PackageID);
					eventLogger.LogDebug("DeleteExtensionPackage(package={Package}): authSystem → {AuthSystem}", package, authSystem);
                    if (authSystem != null)
                    {
                        AuthenticationController.DeleteAuthentication(authSystem);
                    }

                    break;
                case "CoreLanguagePack":
                    LanguagePackInfo languagePack = LanguagePackController.GetLanguagePackByPackage(package.PackageID);
					eventLogger.LogDebug("DeleteExtensionPackage(package={Package}): languagePack → {LanguagePack}", package, languagePack);
                    if (languagePack != null)
                    {
                        LanguagePackController.DeleteLanguagePack(languagePack);
                    }

                    break;
                case "Module":
                    var controller = new DesktopModuleController();
					eventLogger.LogDebug("DeleteExtensionPackage(package={Package}): controller → {Controller}", package, controller);
                    DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(package.PackageID);
					eventLogger.LogDebug("DeleteExtensionPackage(package={Package}): desktopModule → {DesktopModule}", package, desktopModule);
                    if (desktopModule != null)
                    {
                        controller.DeleteDesktopModule(desktopModule);
                    }

                    break;
                case "SkinObject":
                    SkinControlInfo skinControl = SkinControlController.GetSkinControlByPackageID(package.PackageID);
					eventLogger.LogDebug("DeleteExtensionPackage(package={Package}): skinControl → {SkinControl}", package, skinControl);
                    if (skinControl != null)
                    {
                        SkinControlController.DeleteSkinControl(skinControl);
                    }

                    break;
            }

            DeletePackageInternal(this.eventLogger, package);
			eventLogger.LogDebug("<<< Exiting DeleteExtensionPackage(package={Package})", package);
        }

        /// <inheritdoc />
        public PackageInfo GetExtensionPackage(int portalId, Func<PackageInfo, bool> predicate)
        {
			eventLogger.LogDebug(">>> Entering GetExtensionPackage(portalId={PortalId},predicate={Predicate})", portalId, predicate);
			eventLogger.LogDebug("<<< Exiting GetExtensionPackage(portalId={PortalId},predicate={Predicate})", portalId, predicate);
			eventLogger.LogDebug("<<< Exiting GetExtensionPackage(portalId={PortalId},predicate={Predicate},useCopy={UseCopy})", portalId, predicate, useCopy);
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
			eventLogger.LogDebug(">>> Entering GetExtensionPackages(portalId={PortalId})", portalId);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.PackagesCacheKey, portalId);
            var cacheItemArgs = new CacheItemArgs(cacheKey, DataCache.PackagesCacheTimeout, DataCache.PackagesCachePriority, portalId);
			eventLogger.LogDebug("GetExtensionPackages(portalId={PortalId}): cacheItemArgs → {CacheItemArgs}", portalId, cacheItemArgs);
			eventLogger.LogDebug("GetExtensionPackages(portalId={PortalId}): cacheKey → {CacheKey}", portalId, cacheKey);
			eventLogger.LogDebug("<<< Exiting GetExtensionPackages(portalId={PortalId})", portalId);
            return CBO.GetCachedObject<List<PackageInfo>>(
                this.hostSettings,
                cacheItemArgs,
			eventLogger.LogDebug("<<< Exiting GetExtensionPackages(portalId={PortalId},predicate={Predicate})", portalId, predicate);
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
			eventLogger.LogDebug(">>> Entering SaveExtensionPackage(package={Package})", package);
            if (package.PackageID == Null.NullInteger)
            {
                AddPackageInternal(this.eventLogger, package);
            }
            else
            {
                UpdatePackageInternal(this.eventLogger, package);
            }
			eventLogger.LogDebug("<<< Exiting SaveExtensionPackage(package={Package})", package);
        }

        /// <inheritdoc />
        public PackageType GetExtensionPackageType(Func<PackageType, bool> predicate)
        {
			eventLogger.LogDebug(">>> Entering GetExtensionPackageType(predicate={Predicate})", predicate);
			eventLogger.LogDebug("<<< Exiting GetExtensionPackageType(predicate={Predicate})", predicate);
            return this.GetExtensionPackageTypes().SingleOrDefault(predicate);
        }

        /// <inheritdoc />
        public IList<PackageType> GetExtensionPackageTypes()
        {
			eventLogger.LogDebug(">>> Entering GetExtensionPackageTypes()");
			eventLogger.LogDebug("<<< Exiting GetExtensionPackageTypes()");
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
			eventLogger.LogDebug(">>> Entering GetPackageDependencies(predicate={Predicate})", predicate);
			eventLogger.LogDebug("<<< Exiting GetPackageDependencies(predicate={Predicate})", predicate);
			eventLogger.LogDebug("<<< Exiting GetPackageDependencies(hostSettings={HostSettings})", hostSettings);
            return GetPackageDependencies(this.hostSettings).Where(predicate).ToList();
        }

        internal static string GetSpecificFolderName(XPathNavigator manifestNav, string xpath, string elementName, string startWith)
        {
			eventLogger.LogDebug(">>> Entering GetSpecificFolderName(manifestNav={ManifestNav},xpath={Xpath},elementName={ElementName},startWith={StartWith})", manifestNav, xpath, elementName, startWith);
            string result = string.Empty;
            var folderNameNav = manifestNav.Select(xpath);
			eventLogger.LogDebug("GetSpecificFolderName(manifestNav={ManifestNav},xpath={Xpath},elementName={ElementName},startWith={StartWith}): folderNameNav → {FolderNameNav}", manifestNav, xpath, elementName, startWith, folderNameNav);
			eventLogger.LogDebug("GetSpecificFolderName(manifestNav={ManifestNav},xpath={Xpath},elementName={ElementName},startWith={StartWith}): result → {Result}", manifestNav, xpath, elementName, startWith, result);

            if (folderNameNav != null)
            {
                while (folderNameNav.MoveNext())
                {
                    var elementValue = Util.ReadElement(folderNameNav.Current, elementName);
					eventLogger.LogDebug("GetSpecificFolderName(manifestNav={ManifestNav},xpath={Xpath},elementName={ElementName},startWith={StartWith}): elementValue → {ElementValue}", manifestNav, xpath, elementName, startWith, elementValue);
                    if (!string.IsNullOrEmpty(elementValue) && elementValue.StartsWith(startWith, StringComparison.OrdinalIgnoreCase))
                    {
                        result = elementValue;
                        break;
                    }
                }
            }

			eventLogger.LogDebug("<<< Exiting GetSpecificFolderName(manifestNav={ManifestNav},xpath={Xpath},elementName={ElementName},startWith={StartWith})", manifestNav, xpath, elementName, startWith);
            return result;
        }

        /// <inheritdoc />
        protected override Func<IPackageController> GetFactory()
        {
			eventLogger.LogDebug(">>> Entering GetFactory()");
			eventLogger.LogDebug("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<IPackageController>;
        }

        private static void AddLog(IEventLogger eventLogger, PackageInfo package, EventLogType logType)
        {
			eventLogger.LogDebug(">>> Entering AddLog(eventLogger={EventLogger},package={Package},logType={LogType})", eventLogger, package, logType);
            eventLogger.AddLog(
                package,
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                string.Empty,
                logType);
				eventLogger.LogDebug("<<< Exiting AddLog(eventLogger={EventLogger},package={Package},logType={LogType})", eventLogger, package, logType);
        }

        private static void AddPackageInternal(IEventLogger eventLogger, PackageInfo package)
        {
			eventLogger.LogDebug(">>> Entering AddPackageInternal(eventLogger={EventLogger},package={Package})", eventLogger, package);
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
			eventLogger.LogDebug("<<< Exiting AddPackageInternal(eventLogger={EventLogger},package={Package})", eventLogger, package);
        }

        private static void ClearCache(int portalId)
        {
			eventLogger.LogDebug(">>> Entering ClearCache(portalId={PortalId})", portalId);
            DataCache.ClearPackagesCache(portalId);
			eventLogger.LogDebug("<<< Exiting ClearCache(portalId={PortalId})", portalId);
        }

        private static void ClearDependenciesCache()
        {
			eventLogger.LogDebug(">>> Entering ClearDependenciesCache()");
            DataCache.RemoveCache(DataCache.PackageDependenciesCacheKey);
			eventLogger.LogDebug("<<< Exiting ClearDependenciesCache()");
        }

        private static void DeletePackageInternal(IEventLogger eventLogger, PackageInfo package)
        {
			eventLogger.LogDebug(">>> Entering DeletePackageInternal(eventLogger={EventLogger},package={Package})", eventLogger, package);
            Provider.DeletePackage(package.PackageID);
            AddLog(eventLogger, package, EventLogType.PACKAGE_DELETED);

            if (PortalSettings.Current != null)
            {
                ClearCache(PortalSettings.Current.PortalId);
            }

            ClearCache(Null.NullInteger);
			eventLogger.LogDebug("<<< Exiting DeletePackageInternal(eventLogger={EventLogger},package={Package})", eventLogger, package);
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
			eventLogger.LogDebug(">>> Entering UpdatePackageInternal(eventLogger={EventLogger},package={Package})", eventLogger, package);
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
			eventLogger.LogDebug("<<< Exiting UpdatePackageInternal(eventLogger={EventLogger},package={Package})", eventLogger, package);
        }

        private static void SavePackageDependency(PackageDependencyInfo dependency)
        {
			eventLogger.LogDebug(">>> Entering SavePackageDependency(dependency={Dependency})", dependency);
            dependency.PackageDependencyId = Provider.SavePackageDependency(
                dependency.PackageDependencyId,
                dependency.PackageId,
                dependency.PackageName,
                dependency.Version.ToString());

            ClearDependenciesCache();
			eventLogger.LogDebug("<<< Exiting SavePackageDependency(dependency={Dependency})", dependency);
        }
    }
}
