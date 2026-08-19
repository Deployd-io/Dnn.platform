// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml;

    using Dnn.PersonaBar.Extensions.Components.Dto;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Upgrade;
    using DotNetNuke.Services.Upgrade.Internals;
    using Microsoft.Extensions.DependencyInjection;

    public class ExtensionsController
    {
        private const string OwnerUpdateService = "DotNetNuke Update Service";
        private readonly IHostSettings hostSettings;
        private readonly IHostSettingsService hostSettingsService;
        private readonly IPortalController portalController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IPortalAliasController portalAliasController;
        private readonly IPackageController packageController;
        private readonly ITabController tabController;

        /// <summary>Initializes a new instance of the <see cref="ExtensionsController"/> class.</summary>
        public ExtensionsController()
            : this(null, null, null, null, null, null, null, null)
        {
		eventLogger.LogDebug(">>> Entering ExtensionsController()");
        }
		eventLogger.LogDebug("<<< Exiting ExtensionsController()");

        /// <summary>Initializes a new instance of the <see cref="ExtensionsController"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalAliasController">The portal alias controller.</param>
        /// <param name="packageController">The package controller.</param>
        /// <param name="tabController">The tab controller.</param>
        public ExtensionsController(
            INavigationManager navigationManager,
            IApplicationStatusInfo appStatus,
            IHostSettings hostSettings,
            IHostSettingsService hostSettingsService,
            IPortalController portalController,
            IPortalAliasController portalAliasController,
            IPackageController packageController,
            ITabController tabController)
        {
            this.NavigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
			eventLogger.LogDebug("ExtensionsController(navigationManager={NavigationManager},appStatus={AppStatus},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalAliasController={PortalAliasController},packageController={PackageController},tabController={TabController}): appStatus → {AppStatus1}", navigationManager, appStatus, hostSettings, hostSettingsService, portalController, portalAliasController, packageController, tabController, appStatus);
			eventLogger.LogDebug("ExtensionsController(navigationManager={NavigationManager},appStatus={AppStatus},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalAliasController={PortalAliasController},packageController={PackageController},tabController={TabController}): hostSettingsService → {HostSettingsService1}", navigationManager, appStatus, hostSettings, hostSettingsService, portalController, portalAliasController, packageController, tabController, hostSettingsService);
			eventLogger.LogDebug("ExtensionsController(navigationManager={NavigationManager},appStatus={AppStatus},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalAliasController={PortalAliasController},packageController={PackageController},tabController={TabController}): portalController → {PortalController1}", navigationManager, appStatus, hostSettings, hostSettingsService, portalController, portalAliasController, packageController, tabController, portalController);
			eventLogger.LogDebug("ExtensionsController(navigationManager={NavigationManager},appStatus={AppStatus},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalAliasController={PortalAliasController},packageController={PackageController},tabController={TabController}): portalAliasController → {PortalAliasController1}", navigationManager, appStatus, hostSettings, hostSettingsService, portalController, portalAliasController, packageController, tabController, portalAliasController);
			eventLogger.LogDebug("ExtensionsController(navigationManager={NavigationManager},appStatus={AppStatus},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalAliasController={PortalAliasController},packageController={PackageController},tabController={TabController}): packageController → {PackageController1}", navigationManager, appStatus, hostSettings, hostSettingsService, portalController, portalAliasController, packageController, tabController, packageController);
			eventLogger.LogDebug("ExtensionsController(navigationManager={NavigationManager},appStatus={AppStatus},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalAliasController={PortalAliasController},packageController={PackageController},tabController={TabController}): tabController → {TabController1}", navigationManager, appStatus, hostSettings, hostSettingsService, portalController, portalAliasController, packageController, tabController, tabController);
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			eventLogger.LogDebug("ExtensionsController(navigationManager={NavigationManager},appStatus={AppStatus},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalAliasController={PortalAliasController},packageController={PackageController},tabController={TabController}): hostSettings → {HostSettings1}", navigationManager, appStatus, hostSettings, hostSettingsService, portalController, portalAliasController, packageController, tabController, hostSettings);
            this.hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
            this.portalAliasController = portalAliasController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasController>();
            this.packageController = packageController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPackageController>();
            this.tabController = tabController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITabController>();
        }

        protected INavigationManager NavigationManager { get; }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IDictionary<string, PackageType> GetPackageTypes()
        {
            var installedPackageTypes = new Dictionary<string, PackageType>();
            foreach (var packageType in PackageController.Instance.GetExtensionPackageTypes())
            {
                installedPackageTypes[packageType.PackageType] = packageType;
            }

            return installedPackageTypes;
        }

        public bool HasAvailablePackage(string packageType, out string rootPath)
        {
			eventLogger.LogDebug(">>> Entering HasAvailablePackage(packageType={PackageType})", packageType);
            var type = packageType;
            switch (packageType.ToLowerInvariant())
            {
                case "authsystem":
                case "auth_system":
                    type = nameof(PackageTypes.AuthSystem);
                    rootPath = this.appStatus.ApplicationMapPath + "\\Install\\AuthSystem";
					eventLogger.LogDebug("HasAvailablePackage(packageType={PackageType}): rootPath → {RootPath}", packageType, rootPath);
                    break;
                case "javascriptlibrary":
                case "javascript_library":
                    rootPath = this.appStatus.ApplicationMapPath + "\\Install\\JavaScriptLibrary";
                    break;
                case "extensionlanguagepack":
                    type = nameof(PackageTypes.Language);
                    rootPath = this.appStatus.ApplicationMapPath + "\\Install\\Language";
                    break;
                case "corelanguagepack":
                    rootPath = this.appStatus.ApplicationMapPath + "\\Install\\Language";
                    return true; // core languages should always marked as have available packages.
                case "module":
                case "skin":
                case "container":
                case "provider":
                case "library":
                    rootPath = this.appStatus.ApplicationMapPath + "\\Install\\" + packageType;
                    break;
                default:
                    type = string.Empty;
                    rootPath = string.Empty;
                    break;
            }

            if (!string.IsNullOrEmpty(type) && Directory.Exists(rootPath) &&
                (Directory.GetFiles(rootPath, "*.zip").Length > 0 ||
                    Directory.GetFiles(rootPath, "*.resources").Length > 0))
            {
			eventLogger.LogDebug("<<< Exiting HasAvailablePackage(packageType={PackageType})", packageType);
                return true;
            }

            return false;
        }

        public IList<PackageInfoSlimDto> GetInstalledPackages(int portalId, string packageType)
        {
			eventLogger.LogDebug(">>> Entering GetInstalledPackages(portalId={PortalId},packageType={PackageType})", portalId, packageType);
            var typePackages = new List<PackageInfo>();
            switch (packageType.ToLowerInvariant())
            {
                case "module":
                    if (portalId == Null.NullInteger)
                    {
                        typePackages = this.packageController.GetExtensionPackages(
                            Null.NullInteger, p => "Module".Equals(p.PackageType, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    else
                    {
                        AddModulesToList(portalId, typePackages);
                    }

                    break;
                case "skin":
                case "container":
                    typePackages = this.packageController.GetExtensionPackages(portalId, p => p.PackageType == packageType).ToList();
                    break;
                default:
                    typePackages = this.packageController.GetExtensionPackages(Null.NullInteger, p => p.PackageType == packageType).ToList();
                    break;
            }

			eventLogger.LogDebug("<<< Exiting GetInstalledPackages(portalId={PortalId},packageType={PackageType})", portalId, packageType);
            return typePackages
                .Select(p => new PackageInfoSlimDto(this.hostSettings, this.hostSettingsService, this.portalController, portalId, p))
                .ToList();
        }

        public List<AvailablePackagesDto> GetAvailablePackages(string packageType)
        {
			eventLogger.LogDebug(">>> Entering GetAvailablePackages(packageType={PackageType})", packageType);
            var packages = new List<AvailablePackagesDto>();
            if (this.HasAvailablePackage(packageType, out var packagePath))
            {
                var validPackages = new Dictionary<string, PackageInfo>();
				eventLogger.LogDebug("GetAvailablePackages(packageType={PackageType}): validPackages → {ValidPackages}", packageType, validPackages);
                var invalidPackages = new List<string>();
				eventLogger.LogDebug("GetAvailablePackages(packageType={PackageType}): invalidPackages → {InvalidPackages}", packageType, invalidPackages);

                foreach (string file in Directory.GetFiles(packagePath))
                {
                    if (file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
                    {
                        PackageController.ParsePackage(file, packagePath, validPackages, invalidPackages);
                    }
                }

                if (packageType.Equals("corelanguagepack", StringComparison.OrdinalIgnoreCase))
                {
                    GetAvailableLanguagePacks(validPackages);
                }

                packages.Add(new AvailablePackagesDto
                {
                    PackageType = packageType,
                    ValidPackages = validPackages.Values.Select(p => new PackageInfoSlimDto(this.hostSettings, this.hostSettingsService, this.portalController, Null.NullInteger, p)).ToList(),
                    InvalidPackages = invalidPackages,
                });
            }

			eventLogger.LogDebug("<<< Exiting GetAvailablePackages(packageType={PackageType})", packageType);
            return packages;
        }

        public List<TabInfo> GetPackageUsage(int portalId, int packageId)
        {
			eventLogger.LogDebug(">>> Entering GetPackageUsage(portalId={PortalId},packageId={PackageId})", portalId, packageId);
            var tabs = BuildData(this.tabController, portalId, packageId);
            if (tabs != null && tabs.Count > 0)
            {
			eventLogger.LogDebug("<<< Exiting GetPackageUsage(portalId={PortalId},packageId={PackageId})", portalId, packageId);
                return tabs.Values.ToList();
            }

            return null;
        }

        public string GetFormattedTabLink(int portalId, TabInfo tab)
        {
			eventLogger.LogDebug(">>> Entering GetFormattedTabLink(portalId={PortalId},tab={Tab})", portalId, tab);
            var returnValue = new StringBuilder();

			eventLogger.LogDebug("GetFormattedTabLink(portalId={PortalId},tab={Tab}): returnValue → {ReturnValue}", portalId, tab, returnValue);
            int index = 0;
            this.tabController.PopulateBreadCrumbs(ref tab);
            foreach (TabInfo t in tab.BreadCrumbs)
            {
                if (index > 0)
                {
                    returnValue.Append(" &gt; ");
                }

                if (index < tab.BreadCrumbs.Count - 1)
                {
                    returnValue.Append(t.LocalizedTabName);
                }
                else
                {
                    // use the current portal alias for host tabs
                    var alias = t.PortalID == Null.NullInteger || t.PortalID == portalId
                                    ? PortalSettings.Current.PortalAlias
                                    : this.portalAliasController.GetPortalAliasesByPortalId(t.PortalID)
                                                            .OrderBy(pa => pa.IsPrimary ? 0 : 1)
                                                            .First();
                    var url = this.NavigationManager.NavigateURL(t.TabID, new PortalSettings(t.PortalID, alias), string.Empty);
					eventLogger.LogDebug("GetFormattedTabLink(portalId={PortalId},tab={Tab}): url → {Url}", portalId, tab, url);
                    returnValue.AppendFormat(CultureInfo.InvariantCulture, "<a href=\"{0}\">{1}</a>", url, t.LocalizedTabName);
                }

                index = index + 1;
            }

			eventLogger.LogDebug("<<< Exiting GetFormattedTabLink(portalId={PortalId},tab={Tab})", portalId, tab);
            return returnValue.ToString();
        }

        internal static string IsPackageInUse(PackageInfo packageInfo, int portalId)
        {
			eventLogger.LogDebug(">>> Entering IsPackageInUse(packageInfo={PackageInfo},portalId={PortalId})", packageInfo, portalId);
            if (packageInfo.PackageID == Null.NullInteger)
            {
                return string.Empty;
            }

            if (packageInfo.PackageType.Equals("MODULE", StringComparison.OrdinalIgnoreCase))
            {
                if (portalId == Null.NullInteger)
                {
                    return GetPackagesInUse(true).ContainsKey(packageInfo.PackageID) ? "Yes" : "No";
                }
                else
                {
                    return GetPackagesInUse(false).ContainsKey(packageInfo.PackageID) ? "Yes" : "No";
                }
            }

			eventLogger.LogDebug("<<< Exiting IsPackageInUse(packageInfo={PackageInfo},portalId={PortalId})", packageInfo, portalId);
            return string.Empty;
        }

        internal static string UpgradeRedirect(Version version, string packageType, string packageName)
        {
			eventLogger.LogDebug(">>> Entering UpgradeRedirect(version={Version},packageType={PackageType},packageName={PackageName})", version, packageType, packageName);
			eventLogger.LogDebug("<<< Exiting UpgradeRedirect(version={Version},packageType={PackageType},packageName={PackageName})", version, packageType, packageName);
            return Upgrade.UpgradeRedirect(version, packageType, packageName, string.Empty);
        }

        internal static string UpgradeIndicator(IHostSettings hostSettings, IHostSettingsService hostSettingsService, IPortalController portalController, Version version, string packageType, string packageName)
        {
			eventLogger.LogDebug(">>> Entering UpgradeIndicator(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},version={Version},packageType={PackageType},packageName={PackageName})", hostSettings, hostSettingsService, portalController, version, packageType, packageName);
            var url = Upgrade.UpgradeIndicator(hostSettings, hostSettingsService, portalController, version, packageType, packageName, string.Empty, false, false); // last 2 params are unused
            if (string.IsNullOrEmpty(url))
            {
                url = Globals.ApplicationPath + "/images/spacer.gif";
            }

			eventLogger.LogDebug("<<< Exiting UpgradeIndicator(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},version={Version},packageType={PackageType},packageName={PackageName})", hostSettings, hostSettingsService, portalController, version, packageType, packageName);
            return url;
        }

        internal static string GetPackageIcon(PackageInfo package)
        {
			eventLogger.LogDebug(">>> Entering GetPackageIcon(package={Package})", package);
            switch (package.PackageType.ToLowerInvariant())
            {
                case "module":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultExtensionImage;
                case "container":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultContainerImage;
                case "skin":
                case "skinobject":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultSkinImage;
                case "authenticationsystem":
                case "auth_system":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultAuthenicationImage;
                case "corelanguagepack":
                case "extensionlanguagepack":
                    return package.IconFile != "N\\A" && IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultLanguageImage;
                case "provider":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultProviderImage;
                case "widget":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultWidgetImage;
                case "library":
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultLibraryImage;
                default:
					eventLogger.LogDebug("<<< Exiting GetPackageIcon(package={Package})", package);
                    return IconExists(package.IconFile) ? FixIconUrl(package.IconFile) : Globals.ImagePath + Constants.DefaultExtensionImage;
            }
        }

        internal static IDictionary<int, PackageInfo> GetPackagesInUse(bool forHost)
        {
            return PackageController.GetModulePackagesInUse(PortalController.Instance.GetCurrentSettings().PortalId, forHost);
        }

        private static void AddModulesToList(int portalId, List<PackageInfo> packages)
        {
			eventLogger.LogDebug(">>> Entering AddModulesToList(portalId={PortalId},packages={Packages})", portalId, packages);
            packages.AddRange(from modulePackage in PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "Module")
                              let desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(modulePackage.PackageID)
                              from portalModule in DesktopModuleController.GetPortalDesktopModulesByPortalID(portalId).Values
                              where desktopModule != null && portalModule.DesktopModuleID == desktopModule.DesktopModuleID
                              select modulePackage);
							eventLogger.LogDebug("<<< Exiting AddModulesToList(portalId={PortalId},packages={Packages})", portalId, packages);
        }

        private static string FixIconUrl(string url)
        {
			eventLogger.LogDebug(">>> Entering FixIconUrl(url={Url})", url);
			eventLogger.LogDebug("<<< Exiting FixIconUrl(url={Url})", url);
            return !string.IsNullOrEmpty(Globals.ApplicationPath)
                ? $"{Globals.ApplicationPath}/{url.TrimStart('~').TrimStart('/')}"
                : url;
        }

        private static IDictionary<int, TabInfo> BuildData(ITabController tabController, int portalId, int packageId)
        {
            var tabsWithModule = tabController.GetTabsByPackageID(portalId, packageId, false);
            var allPortalTabs = tabController.GetTabsByPortal(portalId);
            IDictionary<int, TabInfo> tabsInOrder = new Dictionary<int, TabInfo>();

            // must get each tab, they parent may not exist
            foreach (var tab in allPortalTabs.Values)
            {
                AddChildTabsToList(tab, ref allPortalTabs, ref tabsWithModule, ref tabsInOrder);
            }

            return tabsInOrder;
        }

        private static void AddChildTabsToList(TabInfo currentTab, ref TabCollection allPortalTabs, ref IDictionary<int, TabInfo> tabsWithModule, ref IDictionary<int, TabInfo> tabsInOrder)
        {
			eventLogger.LogDebug(">>> Entering AddChildTabsToList(currentTab={CurrentTab},allPortalTabs={AllPortalTabs},tabsWithModule={TabsWithModule},tabsInOrder={TabsInOrder})", currentTab, allPortalTabs, tabsWithModule, tabsInOrder);
            if (tabsWithModule.ContainsKey(currentTab.TabID) && !tabsInOrder.ContainsKey(currentTab.TabID))
            {
                // add current tab
                tabsInOrder.Add(currentTab.TabID, currentTab);

                // add children of current tab
                foreach (var tab in allPortalTabs.WithParentId(currentTab.TabID))
                {
                    AddChildTabsToList(tab, ref allPortalTabs, ref tabsWithModule, ref tabsInOrder);
                }
            }
			eventLogger.LogDebug("<<< Exiting AddChildTabsToList(currentTab={CurrentTab},allPortalTabs={AllPortalTabs},tabsWithModule={TabsWithModule},tabsInOrder={TabsInOrder})", currentTab, allPortalTabs, tabsWithModule, tabsInOrder);
        }

        private static bool IconExists(string imagePath)
        {
			eventLogger.LogDebug(">>> Entering IconExists(imagePath={ImagePath})", imagePath);
            if (string.IsNullOrWhiteSpace(imagePath) || HttpContext.Current == null)
            {
                return false;
            }

            try
            {
                var path = HttpContextSource.Current.Server.MapPath(imagePath);
				eventLogger.LogDebug("IconExists(imagePath={ImagePath}): path → {Path}", imagePath, path);
                return File.Exists(path);
            }
            catch (HttpException)
            {
				eventLogger.LogError("Exception in IconExists(imagePath={ImagePath}): {Arg1}", imagePath, HttpException.getMessage());
				eventLogger.LogDebug("<<< Exiting IconExists(imagePath={ImagePath})", imagePath);
                return false;
            }
        }

        private static void GetAvailableLanguagePacks(IDictionary<string, PackageInfo> validPackages)
        {
			eventLogger.LogDebug(">>> Entering GetAvailableLanguagePacks(validPackages={ValidPackages})", validPackages);
            try
            {
                var myResponseReader = UpdateService.GetLanguageList();
				eventLogger.LogDebug("GetAvailableLanguagePacks(validPackages={ValidPackages}): myResponseReader → {MyResponseReader}", validPackages, myResponseReader);
                var xmlDoc = new XmlDocument { XmlResolver = null };
                using (var xmlReader = XmlReader.Create(myResponseReader, new XmlReaderSettings { XmlResolver = null, }))
                {
                    xmlDoc.Load(xmlReader);
                }

                var languages = xmlDoc.SelectNodes("available/language");
				eventLogger.LogDebug("GetAvailableLanguagePacks(validPackages={ValidPackages}): languages → {Languages}", validPackages, languages);

                if (languages != null)
                {
                    var installedPackages = PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "CoreLanguagePack");
					eventLogger.LogDebug("GetAvailableLanguagePacks(validPackages={ValidPackages}): installedPackages → {InstalledPackages}", validPackages, installedPackages);
                    var installedLanguages = installedPackages.Select(package => LanguagePackController.GetLanguagePackByPackage(package.PackageID)).ToList();
					eventLogger.LogDebug("GetAvailableLanguagePacks(validPackages={ValidPackages}): installedLanguages → {InstalledLanguages}", validPackages, installedLanguages);
					eventLogger.LogDebug("GetAvailableLanguagePacks(validPackages={ValidPackages}): package → {Package}", validPackages, package);
                    foreach (XmlNode language in languages)
                    {
                        string cultureCode = string.Empty;
						eventLogger.LogDebug("GetAvailableLanguagePacks(validPackages={ValidPackages}): cultureCode → {CultureCode}", validPackages, cultureCode);
                        string version = string.Empty;
						eventLogger.LogDebug("GetAvailableLanguagePacks(validPackages={ValidPackages}): version → {Version}", validPackages, version);
                        foreach (XmlNode child in language.ChildNodes)
                        {
                            if (child.Name == "culturecode")
                            {
                                cultureCode = child.InnerText;
                            }

                            if (child.Name == "version")
                            {
                                version = child.InnerText;
                            }
                        }

                        if (!string.IsNullOrEmpty(cultureCode) && !string.IsNullOrEmpty(version) && version.Length == 6)
                        {
                            var myCIintl = new CultureInfo(cultureCode, true);
							eventLogger.LogDebug("GetAvailableLanguagePacks(validPackages={ValidPackages}): myCIintl → {MyCIintl}", validPackages, myCIintl);
                            version = version.Insert(4, ".").Insert(2, ".");
                            var package = new PackageInfo { Owner = OwnerUpdateService, Name = "LanguagePack-" + myCIintl.Name, FriendlyName = myCIintl.NativeName };
                            package.Name = myCIintl.NativeName;
							eventLogger.LogDebug("GetAvailableLanguagePacks(validPackages={ValidPackages}): package.Name → {Arg1}", validPackages, package.Name);
							eventLogger.LogDebug("GetAvailableLanguagePacks(validPackages={ValidPackages}): package.PackageType → {Arg1}", validPackages, package.PackageType);
							eventLogger.LogDebug("GetAvailableLanguagePacks(validPackages={ValidPackages}): package.Description → {Arg1}", validPackages, package.Description);
                            package.PackageType = "CoreLanguagePack";
                            package.Description = cultureCode;
                            if (!Version.TryParse(version, out var ver))
                            {
                                ver = null;
								eventLogger.LogDebug("GetAvailableLanguagePacks(validPackages={ValidPackages}): ver → {Ver}", validPackages, ver);
                            }

                            package.Version = ver;
							eventLogger.LogDebug("GetAvailableLanguagePacks(validPackages={ValidPackages}): package.Version → {Arg1}", validPackages, package.Version);

                            if (
                                installedLanguages.Any(
                                    l =>
                                        LocaleController.Instance.GetLocale(l.LanguageID).Code.ToLowerInvariant().Equals(cultureCode.ToLowerInvariant(), StringComparison.Ordinal)
                                        && installedPackages.First(p => p.PackageID == l.PackageID).Version >= ver))
                            {
                                continue;
                            }

                            if (validPackages.Values.Any(p => p.Name == package.Name))
                            {
                                var existPackage = validPackages.Values.First(p => p.Name == package.Name);
								eventLogger.LogDebug("GetAvailableLanguagePacks(validPackages={ValidPackages}): existPackage → {ExistPackage}", validPackages, existPackage);
                                if (package.Version > existPackage.Version)
                                {
                                    existPackage.Version = package.Version;
									eventLogger.LogDebug("GetAvailableLanguagePacks(validPackages={ValidPackages}): existPackage.Version → {Arg1}", validPackages, existPackage.Version);
                                }
                            }
                            else
                            {
                                validPackages.Add(cultureCode, package);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
				eventLogger.LogError("Exception in GetAvailableLanguagePacks(validPackages={ValidPackages}): {Arg1}", validPackages, Exception.Message);
                // suppress for now - need to decide what to do when webservice is unreachable
                ////throw;
                // same problem happens in InstallWizard.aspx.cs in BindLanguageList method
            }
			eventLogger.LogDebug("<<< Exiting GetAvailableLanguagePacks(validPackages={ValidPackages})", validPackages);
        }
    }
}
