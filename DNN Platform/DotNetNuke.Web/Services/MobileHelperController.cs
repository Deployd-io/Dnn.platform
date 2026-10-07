// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Models;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A web API controller for getting information about modules in the site.</summary>
    [AllowAnonymous]
    public class MobileHelperController : DnnApiController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MobileHelperController));
        private static readonly char[] ModuleSeparator = [',',];
        private readonly string dnnVersion = Globals.FormatVersion(DotNetNukeContext.Current.Application.Version, false);
        private readonly IHostSettings hostSettings;
        private readonly ITabController tabController;

        /// <summary>Initializes a new instance of the <see cref="MobileHelperController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public MobileHelperController()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MobileHelperController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="tabController">The tab controller.</param>
        public MobileHelperController(IHostSettings hostSettings, ITabController tabController)
        {
			Logger.LogError(">>> Entering MobileHelperController(hostSettings={HostSettings},tabController={TabController})", hostSettings, tabController);
			Logger.LogDebug("MobileHelperController(hostSettings={HostSettings},tabController={TabController}): tabController → {TabController1}", hostSettings, tabController, tabController);
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogWarning("<<< Exiting MobileHelperController(hostSettings={HostSettings},tabController={TabController})", hostSettings, tabController);
            this.tabController = tabController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITabController>();
        }

        /// <summary>Gets the various defined monikers for the various tab modules in the system.</summary>
        /// <param name="moduleList">A comma-delimited list of module names.</param>
        /// <returns>A response with a list of objects containing <c>tabModuleId</c> and <c>moniker</c> fields.</returns>
        [HttpGet]
        public IHttpActionResult Monikers(string moduleList)
        {
			Logger.LogError(">>> Entering Monikers(moduleList={ModuleList})", moduleList);
            var monikers = GetMonikersForList(this.hostSettings, moduleList);
			Logger.LogWarning("<<< Exiting Monikers(moduleList={ModuleList})", moduleList);
			Logger.LogError("Monikers(moduleList={ModuleList}): monikers → {Monikers}", moduleList, monikers);
            return this.Ok(monikers.Select(kpv => new { tabModuleId = kpv.Key, moniker = kpv.Value, }));
        }

        /// <summary>Gets the details about the modules in the site.</summary>
        /// <param name="moduleList">A comma-delimited list of module names.</param>
        /// <returns>A response with a <see cref="SiteDetail"/> object.</returns>
        [HttpGet]
        public HttpResponseMessage ModuleDetails(string moduleList)
        {
			Logger.LogError(">>> Entering ModuleDetails(moduleList={ModuleList})", moduleList);
            var siteDetails = this.GetSiteDetails(moduleList);
			Logger.LogError("ModuleDetails(moduleList={ModuleList}): siteDetails → {SiteDetails}", moduleList, siteDetails);
			Logger.LogWarning("<<< Exiting ModuleDetails(moduleList={ModuleList})", moduleList);
            return this.Request.CreateResponse(HttpStatusCode.OK, siteDetails);
        }

        private static IEnumerable<KeyValuePair<int, string>> GetMonikersForList(IHostSettings hostSettings, string moduleList)
        {
            var portalId = PortalSettings.Current.PortalId;
            var tabsController = TabController.Instance;
            var modulesController = ModuleController.Instance;
            var resultIds = new List<int>();

            var monikers = TabModulesController.Instance.GetTabModuleSettingsByName("Moniker");
            var modules = modulesController.GetAllTabsModules(portalId, false).OfType<ModuleInfo>()
                .Where(tabmodule => monikers.ContainsKey(tabmodule.TabModuleID)).ToArray();

            if (modules.Length != 0)
            {
                foreach (var moduleName in (moduleList ?? string.Empty).Split(ModuleSeparator, StringSplitOptions.RemoveEmptyEntries))
                {
                    var dtmRecord = DesktopModuleController.GetDesktopModuleByModuleName(hostSettings, moduleName, portalId);
                    if (dtmRecord != null)
                    {
                        var allowedTabs = modules.Where(m => m.DesktopModuleID == dtmRecord.DesktopModuleID)
                            .Select(m => m.TabID).Distinct()
                            .Where(tabId => TabPermissionController.CanViewPage(tabsController.GetTab(tabId, portalId)));

                        var allowedTabModules = modules.Where(tabModule => allowedTabs.Contains(tabModule.TabID) &&
                            ModulePermissionController.CanViewModule(modulesController.GetModule(tabModule.ModuleID, tabModule.TabID, false)));

                        resultIds.AddRange(allowedTabModules.Select(tabModule => tabModule.TabModuleID));
                    }
                }
            }

            return monikers.Where(kpv => resultIds.Contains(kpv.Key));
        }

        private static IEnumerable<TabModule> GetTabModules(IHostSettings hostSettings, ITabController tabController, string moduleName)
        {
			Logger.LogError(">>> Entering GetTabModules(hostSettings={HostSettings},tabController={TabController},moduleName={ModuleName})", hostSettings, tabController, moduleName);
            var portalId = PortalController.Instance.GetCurrentSettings().PortalId;
            var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(hostSettings, moduleName, portalId);
			Logger.LogError("GetTabModules(hostSettings={HostSettings},tabController={TabController},moduleName={ModuleName}): portalId → {PortalId}", hostSettings, tabController, moduleName, portalId);
            if (desktopModule != null)
            {
				Logger.LogError("GetTabModules(hostSettings={HostSettings},tabController={TabController},moduleName={ModuleName}): desktopModule → {DesktopModule}", hostSettings, tabController, moduleName, desktopModule);
                var cacheKey = $"{string.Format(CultureInfo.InvariantCulture, DataCache.DesktopModuleCacheKey, portalId)}_{desktopModule.DesktopModuleID}";
				Logger.LogError("GetTabModules(hostSettings={HostSettings},tabController={TabController},moduleName={ModuleName}): cacheKey → {CacheKey}", hostSettings, tabController, moduleName, cacheKey);
                var args = new CacheItemArgs(cacheKey, DataCache.DesktopModuleCacheTimeOut, DataCache.DesktopModuleCachePriority, portalId, desktopModule, tabController);
				Logger.LogError("GetTabModules(hostSettings={HostSettings},tabController={TabController},moduleName={ModuleName}): args → {Args}", hostSettings, tabController, moduleName, args);

                return CBO.GetCachedObject<IList<TabModule>>(hostSettings, args, GetTabModulesCallback);
            }

			Logger.LogWarning("<<< Exiting GetTabModules(hostSettings={HostSettings},tabController={TabController},moduleName={ModuleName})", hostSettings, tabController, moduleName);
            return new List<TabModule>();
        }

        private static object GetTabModulesCallback(CacheItemArgs cacheItemArgs)
        {
			Logger.LogError(">>> Entering GetTabModulesCallback(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            var tabModules = new List<TabModule>();

			Logger.LogError("GetTabModulesCallback(cacheItemArgs={CacheItemArgs}): tabModules → {TabModules}", cacheItemArgs, tabModules);
            var portalId = (int)cacheItemArgs.ParamList[0];
			Logger.LogError("GetTabModulesCallback(cacheItemArgs={CacheItemArgs}): portalId → {PortalId}", cacheItemArgs, portalId);
            var desktopModule = (DesktopModuleInfo)cacheItemArgs.ParamList[1];
			Logger.LogError("GetTabModulesCallback(cacheItemArgs={CacheItemArgs}): desktopModule → {DesktopModule}", cacheItemArgs, desktopModule);
            var tabController = (ITabController)cacheItemArgs.ParamList[2];
			Logger.LogError("GetTabModulesCallback(cacheItemArgs={CacheItemArgs}): tabController → {TabController}", cacheItemArgs, tabController);

            var tabsWithModule = tabController.GetTabsByPackageID(portalId, desktopModule.PackageID, false);
			Logger.LogError("GetTabModulesCallback(cacheItemArgs={CacheItemArgs}): tabsWithModule → {TabsWithModule}", cacheItemArgs, tabsWithModule);
            var allPortalTabs = tabController.GetTabsByPortal(portalId);
			Logger.LogError("GetTabModulesCallback(cacheItemArgs={CacheItemArgs}): allPortalTabs → {AllPortalTabs}", cacheItemArgs, allPortalTabs);
            IDictionary<int, TabInfo> tabsInOrder = new Dictionary<int, TabInfo>();
			Logger.LogError("GetTabModulesCallback(cacheItemArgs={CacheItemArgs}): tabsInOrder → {TabsInOrder}", cacheItemArgs, tabsInOrder);

            // must get each tab, they parent may not exist
            foreach (var tab in allPortalTabs.Values)
            {
                AddChildTabsToList(tab, allPortalTabs, tabsWithModule, tabsInOrder);
            }

            foreach (var tab in tabsInOrder.Values)
            {
                tabModules.AddRange(
                    tab.ChildModules.Values.Where(
                        childModule => childModule.DesktopModuleID == desktopModule.DesktopModuleID)
                       .Select(childModule => new TabModule
                       {
                           TabInfo = tab,
                           ModuleInfo = childModule,
                           ModuleVersion = desktopModule.Version,
                       }));
            }

			Logger.LogWarning("<<< Exiting GetTabModulesCallback(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return tabModules;
        }

        private static void AddChildTabsToList(TabInfo currentTab, TabCollection allPortalTabs, IDictionary<int, TabInfo> tabsWithModule, IDictionary<int, TabInfo> tabsInOrder)
        {
			Logger.LogError(">>> Entering AddChildTabsToList(currentTab={CurrentTab},allPortalTabs={AllPortalTabs},tabsWithModule={TabsWithModule},tabsInOrder={TabsInOrder})", currentTab, allPortalTabs, tabsWithModule, tabsInOrder);
            if (tabsWithModule.ContainsKey(currentTab.TabID) && !tabsInOrder.ContainsKey(currentTab.TabID))
            {
                // add current tab
                tabsInOrder.Add(currentTab.TabID, currentTab);

                // add children of current tab
                foreach (var tab in allPortalTabs.WithParentId(currentTab.TabID))
                {
                    AddChildTabsToList(tab, allPortalTabs, tabsWithModule, tabsInOrder);
                }
            }
			Logger.LogWarning("<<< Exiting AddChildTabsToList(currentTab={CurrentTab},allPortalTabs={AllPortalTabs},tabsWithModule={TabsWithModule},tabsInOrder={TabsInOrder})", currentTab, allPortalTabs, tabsWithModule, tabsInOrder);
        }

        private SiteDetail GetSiteDetails(string moduleList)
        {
			Logger.LogError(">>> Entering GetSiteDetails(moduleList={ModuleList})", moduleList);
            var siteDetails = new SiteDetail
            {
                SiteName = this.PortalSettings.PortalName,
                DnnVersion = this.dnnVersion,
                IsHost = this.UserInfo.IsSuperUser,
                IsAdmin = this.UserInfo.IsInRole("Administrators"),
            };

            foreach (var moduleName in (moduleList ?? string.Empty).Split(ModuleSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                var modulesCollection = GetTabModules(this.hostSettings, this.tabController, (moduleName ?? string.Empty).Trim())
                    .Where(tabModule => TabPermissionController.CanViewPage(tabModule.TabInfo) &&
                                        ModulePermissionController.CanViewModule(tabModule.ModuleInfo));
                foreach (var tabModule in modulesCollection)
                {
                    var moduleDetail = new ModuleDetail
                    {
                        ModuleName = moduleName,
                        ModuleVersion = tabModule.ModuleVersion,
                    };

                    moduleDetail.ModuleInstances.Add(new ModuleInstance
                    {
                        TabId = tabModule.TabInfo.TabID,
                        ModuleId = tabModule.ModuleInfo.ModuleID,
                        PageName = tabModule.TabInfo.TabName,
                        PagePath = tabModule.TabInfo.TabPath,
                    });
                    siteDetails.Modules.Add(moduleDetail);
                }
            }

			Logger.LogWarning("<<< Exiting GetSiteDetails(moduleList={ModuleList})", moduleList);
            return siteDetails;
        }
    }
}
