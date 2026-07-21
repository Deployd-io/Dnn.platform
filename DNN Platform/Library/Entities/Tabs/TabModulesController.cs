// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Framework;
    using DotNetNuke.UI.Skins;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="ITabModulesController"/> implementation.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public class TabModulesController(IHostSettings hostSettings)
        : ServiceLocator<ITabModulesController, TabModulesController>, ITabModulesController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
		Logger.LogDebug(">>> Entering TabModulesController(hostSettings={HostSettings})", hostSettings);
		Logger.LogDebug("TabModulesController(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);

        /// <summary>Initializes a new instance of the <see cref="TabModulesController"/> class.</summary>
		Logger.LogDebug("<<< Exiting TabModulesController(hostSettings={HostSettings})", hostSettings);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public TabModulesController()
            : this(null)
        {
        }

        /// <inheritdoc />
        public ArrayList GetTabModules(TabInfo tab)
        {
			Logger.LogDebug(">>> Entering GetTabModules(tab={Tab})", tab);
            var objPaneModules = new Dictionary<string, int>();

			Logger.LogDebug("GetTabModules(tab={Tab}): objPaneModules → {ObjPaneModules}", tab, objPaneModules);
            var modules = GetModules(tab);
            var configuredModules = new ArrayList();
			Logger.LogDebug("GetTabModules(tab={Tab}): modules → {Modules}", tab, modules);
			Logger.LogDebug("GetTabModules(tab={Tab}): configuredModules → {ConfiguredModules}", tab, configuredModules);
            foreach (var configuringModule in modules)
            {
                ConfigureModule(configuringModule, tab);

                if (objPaneModules.ContainsKey(configuringModule.PaneName) == false)
                {
                    objPaneModules.Add(configuringModule.PaneName, 0);
                }

                configuringModule.PaneModuleCount = 0;
				Logger.LogDebug("GetTabModules(tab={Tab}): configuringModule.PaneModuleCount → {Arg1}", tab, configuringModule.PaneModuleCount);
                if (!configuringModule.IsDeleted)
                {
                    objPaneModules[configuringModule.PaneName] = objPaneModules[configuringModule.PaneName] + 1;
                    configuringModule.PaneModuleIndex = objPaneModules[configuringModule.PaneName] - 1;
					Logger.LogDebug("GetTabModules(tab={Tab}): configuringModule.PaneModuleIndex → {Arg1}", tab, configuringModule.PaneModuleIndex);
                }

                configuredModules.Add(configuringModule);
            }

            foreach (ModuleInfo module in configuredModules)
            {
                if (PortalSettings.Current != null)
                {
                    module.Header = HtmlUtils.SanitizeHtmlIfNeeded(module.Header, PortalSettings.Current.AllowJsInModuleHeaders);
                    module.Footer = HtmlUtils.SanitizeHtmlIfNeeded(module.Footer, PortalSettings.Current.AllowJsInModuleFooters);
                }

                module.PaneModuleCount = objPaneModules[module.PaneName];
				Logger.LogDebug("GetTabModules(tab={Tab}): module.PaneModuleCount → {Arg1}", tab, module.PaneModuleCount);
            }

			Logger.LogDebug("<<< Exiting GetTabModules(tab={Tab})", tab);
            return configuredModules;
        }

        /// <inheritdoc />
        public Dictionary<int, string> GetTabModuleSettingsByName(string settingName)
        {
            var portalId = PortalSettings.Current.PortalId;
            var dataProvider = DataProvider.Instance();
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.TabModuleSettingsNameCacheKey, portalId, settingName);
            var cachedItems = CBO.GetCachedObject<Dictionary<int, string>>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.TabModuleCacheTimeOut, DataCache.TabModuleCachePriority),
                _ =>
                {
                    using var dr = dataProvider.GetTabModuleSettingsByName(portalId, settingName);
                    var result = new Dictionary<int, string>();
                    while (dr.Read())
                    {
                        result[dr.GetInt32(0)] = dr.GetString(1);
                    }

                    return result;
                });

            return cachedItems;
        }

        /// <inheritdoc />
        public IList<int> GetTabModuleIdsBySetting(string settingName, string expectedValue)
        {
			Logger.LogDebug(">>> Entering GetTabModuleIdsBySetting(settingName={SettingName},expectedValue={ExpectedValue})", settingName, expectedValue);
            var items = this.GetTabModuleSettingsByName(settingName);
            var matches = items.Where(e => e.Value.Equals(expectedValue, StringComparison.OrdinalIgnoreCase));
			Logger.LogDebug("GetTabModuleIdsBySetting(settingName={SettingName},expectedValue={ExpectedValue}): items → {Items}", settingName, expectedValue, items);
			Logger.LogDebug("GetTabModuleIdsBySetting(settingName={SettingName},expectedValue={ExpectedValue}): matches → {Matches}", settingName, expectedValue, matches);
            var keyValuePairs = matches as KeyValuePair<int, string>[] ?? matches.ToArray();
            if (keyValuePairs.Length != 0)
			Logger.LogDebug("GetTabModuleIdsBySetting(settingName={SettingName},expectedValue={ExpectedValue}): keyValuePairs → {KeyValuePairs}", settingName, expectedValue, keyValuePairs);
            {
                return keyValuePairs.Select(kpv => kpv.Key).ToList();
            }

            // this is fallback in case a new value was added but not in the cache yet
            var dataProvider = DataProvider.Instance();
            using (var dr = dataProvider.GetTabModuleIdsBySettingNameAndValue(PortalSettings.Current.PortalId, settingName, expectedValue))
            {
			Logger.LogDebug("GetTabModuleIdsBySetting(settingName={SettingName},expectedValue={ExpectedValue}): dataProvider → {DataProvider}", settingName, expectedValue, dataProvider);
                var result = new List<int>();
                while (dr.Read())
				Logger.LogDebug("GetTabModuleIdsBySetting(settingName={SettingName},expectedValue={ExpectedValue}): result → {Result}", settingName, expectedValue, result);
                {
                    result.Add(dr.GetInt32(0));
                }

				Logger.LogDebug("<<< Exiting GetTabModuleIdsBySetting(settingName={SettingName},expectedValue={ExpectedValue})", settingName, expectedValue);
                return result;
            }
        }

        /// <inheritdoc />
        protected override Func<ITabModulesController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => Globals.DependencyProvider.GetRequiredService<ITabModulesController>();
        }

        private static void ConfigureModule(ModuleInfo cloneModule, TabInfo tab)
        {
			Logger.LogDebug(">>> Entering ConfigureModule(cloneModule={CloneModule},tab={Tab})", cloneModule, tab);
            if (Null.IsNull(cloneModule.StartDate))
            {
                cloneModule.StartDate = DateTime.MinValue;
				Logger.LogDebug("ConfigureModule(cloneModule={CloneModule},tab={Tab}): cloneModule.StartDate → {Arg2}", cloneModule, tab, cloneModule.StartDate);
            }

            if (Null.IsNull(cloneModule.EndDate))
            {
                cloneModule.EndDate = DateTime.MaxValue;
				Logger.LogDebug("ConfigureModule(cloneModule={CloneModule},tab={Tab}): cloneModule.EndDate → {Arg2}", cloneModule, tab, cloneModule.EndDate);
            }

            if (string.IsNullOrEmpty(cloneModule.ContainerSrc))
            {
                cloneModule.ContainerSrc = tab.ContainerSrc;
				Logger.LogDebug("ConfigureModule(cloneModule={CloneModule},tab={Tab}): cloneModule.ContainerSrc → {Arg2}", cloneModule, tab, cloneModule.ContainerSrc);
            }

            cloneModule.ContainerSrc = SkinController.FormatSkinSrc(cloneModule.ContainerSrc, PortalSettings.Current);
            cloneModule.ContainerPath = SkinController.FormatSkinPath(cloneModule.ContainerSrc);
			Logger.LogDebug("<<< Exiting ConfigureModule(cloneModule={CloneModule},tab={Tab})", cloneModule, tab);
        }

        private static IEnumerable<ModuleInfo> GetModules(TabInfo tab)
        {
			Logger.LogDebug(">>> Entering GetModules(tab={Tab})", tab);
            int urlVersion;
            if (TabVersionUtils.TryGetUrlVersion(out urlVersion))
            {
                return TabVersionBuilder.Instance.GetVersionModules(tab.TabID, urlVersion);
            }

            if (Globals.IsEditMode())
            {
                return TabVersionBuilder.Instance.GetUnPublishedVersionModules(tab.TabID);
            }

			Logger.LogDebug("<<< Exiting GetModules(tab={Tab})", tab);
            return TabVersionBuilder.Instance.GetCurrentModules(tab.TabID);
        }
    }
}
