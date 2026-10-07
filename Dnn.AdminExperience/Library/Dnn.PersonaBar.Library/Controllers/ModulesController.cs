// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;

    using Dnn.PersonaBar.Library.Helper;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;

    public class ModulesController : ServiceLocator<IModulesController, ModulesController>, IModulesController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModulesController));
        private IContentVerifier contentVerifier;

        /// <summary>Initializes a new instance of the <see cref="ModulesController"/> class.</summary>
        public ModulesController()
            : this(new ContentVerifier())
        {
		Logger.LogError(">>> Entering ModulesController(contentVerifier={ContentVerifier})", contentVerifier);
		Logger.LogDebug("ModulesController(contentVerifier={ContentVerifier}): contentVerifier → {ContentVerifier1}", contentVerifier, contentVerifier);
        Logger.LogWarning("<<< Exiting ModulesController(contentVerifier={ContentVerifier})", contentVerifier);
        }

        /// <summary>Initializes a new instance of the <see cref="ModulesController"/> class.</summary>
        /// <param name="contentVerifier">The content verifier.</param>
        public ModulesController(IContentVerifier contentVerifier)
        {
			Logger.LogError(">>> Entering ModulesController(contentVerifier={ContentVerifier})", contentVerifier);
            this.contentVerifier = contentVerifier;
        }

        /// <inheritdoc />
        public List<ModuleInfo> AddNewModule(PortalSettings portalSettings, string title, int desktopModuleId, int tabId, string paneName, int position, int permissionType, string align, out KeyValuePair<HttpStatusCode, string> message)
        {
			Logger.LogError(">>> Entering AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align})", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align);
            message = default(KeyValuePair<HttpStatusCode, string>);
            var page = TabController.Instance.GetTab(tabId, portalSettings.PortalId);
			Logger.LogDebug("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): message → {Message}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, message);
            if (page == null)
            {
				Logger.LogError("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): page → {Page}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, page);
                message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Localization.GetString("Prompt_PageNotFound", Constants.LocalResourcesFile), tabId));
				Logger.LogError("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): message → {Message}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, message);
                return null;
            }

            if (!TabPermissionController.CanManagePage(page))
            {
                message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, Localization.GetString("Prompt_InsufficientPermissions", Constants.LocalResourcesFile));
                return null;
            }

            var moduleList = new List<ModuleInfo>();
			Logger.LogError("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): moduleList → {ModuleList}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, moduleList);

            foreach (var objModuleDefinition in ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId).Values)
            {
                var objModule = new ModuleInfo();
				Logger.LogError("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule → {ObjModule}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, objModule);
                objModule.Initialize(portalSettings.PortalId);

                objModule.PortalID = portalSettings.PortalId;
				Logger.LogDebug("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.PortalID → {Arg8}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, objModule.PortalID);
				Logger.LogDebug("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.ModuleOrder → {Arg8}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, objModule.ModuleOrder);
                objModule.TabID = tabId;
				Logger.LogDebug("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.TabID → {Arg8}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, objModule.TabID);
                objModule.ModuleOrder = position;
                objModule.ModuleTitle = string.IsNullOrEmpty(title) ? objModuleDefinition.FriendlyName : title;
                objModule.PaneName = paneName;
				Logger.LogDebug("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.PaneName → {Arg8}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, objModule.PaneName);
                objModule.ModuleDefID = objModuleDefinition.ModuleDefID;
				Logger.LogDebug("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.ModuleDefID → {Arg8}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, objModule.ModuleDefID);
                if (objModuleDefinition.DefaultCacheTime > 0)
                {
                    objModule.CacheTime = objModuleDefinition.DefaultCacheTime;
					Logger.LogDebug("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.CacheTime → {Arg8}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, objModule.CacheTime);
                    if (portalSettings.DefaultModuleId > Null.NullInteger &&
                        portalSettings.DefaultTabId > Null.NullInteger)
                    {
                        var defaultModule = ModuleController.Instance.GetModule(
                            portalSettings.DefaultModuleId,
                            portalSettings.DefaultTabId,
                            true);
                        if (defaultModule != null)
                        {
                            objModule.CacheTime = defaultModule.CacheTime;
                        }
                    }
                }

                ModuleController.Instance.InitialModulePermission(objModule, objModule.TabID, permissionType);

                if (portalSettings.ContentLocalizationEnabled)
                {
                    var defaultLocale = LocaleController.Instance.GetDefaultLocale(portalSettings.PortalId);
					Logger.LogError("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): defaultLocale → {DefaultLocale}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, defaultLocale);

                    // check whether original tab is exists, if true then set culture code to default language,
                    // otherwise set culture code to current.
                    objModule.CultureCode =
                        TabController.Instance.GetTabByCulture(objModule.TabID, portalSettings.PortalId, defaultLocale) !=
                        null
                            ? defaultLocale.Code
                            : portalSettings.CultureCode;
                }
                else
                {
                    objModule.CultureCode = Null.NullString;
					Logger.LogDebug("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.CultureCode → {Arg8}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, objModule.CultureCode);
                }

                objModule.AllTabs = false;
				Logger.LogDebug("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.AllTabs → {Arg8}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, objModule.AllTabs);
                objModule.Alignment = align;
				Logger.LogDebug("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): objModule.Alignment → {Arg8}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, objModule.Alignment);

                ModuleController.Instance.AddModule(objModule);
                moduleList.Add(objModule);

                // Set position so future additions to page can operate correctly
                position = ModuleController.Instance.GetTabModule(objModule.TabModuleID).ModuleOrder + 1;
				Logger.LogError("AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align}): position → {Position1}", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align, position);
            }

			Logger.LogWarning("<<< Exiting AddNewModule(portalSettings={PortalSettings},title={Title},desktopModuleId={DesktopModuleId},tabId={TabId},paneName={PaneName},position={Position},permissionType={PermissionType},align={Align})", portalSettings, title, desktopModuleId, tabId, paneName, position, permissionType, align);
            return moduleList;
        }

        /// <inheritdoc />
        public ModuleInfo CopyModule(PortalSettings portalSettings, int moduleId, int sourcePageId, int targetPageId, string pane, bool includeSettings, out KeyValuePair<HttpStatusCode, string> message, bool moveBahaviour = false)
        {
			Logger.LogError(">>> Entering CopyModule(portalSettings={PortalSettings},moduleId={ModuleId},sourcePageId={SourcePageId},targetPageId={TargetPageId},pane={Pane},includeSettings={IncludeSettings},moveBahaviour={MoveBahaviour})", portalSettings, moduleId, sourcePageId, targetPageId, pane, includeSettings, moveBahaviour);
            var sourceModule = this.GetModule(portalSettings, moduleId, sourcePageId, out message);

			Logger.LogError("CopyModule(portalSettings={PortalSettings},moduleId={ModuleId},sourcePageId={SourcePageId},targetPageId={TargetPageId},pane={Pane},includeSettings={IncludeSettings},moveBahaviour={MoveBahaviour}): sourceModule → {SourceModule}", portalSettings, moduleId, sourcePageId, targetPageId, pane, includeSettings, moveBahaviour, sourceModule);
            if (sourceModule == null)
            {
                return null;
            }

            var targetPage = TabController.Instance.GetTab(targetPageId, portalSettings.PortalId);
			Logger.LogError("CopyModule(portalSettings={PortalSettings},moduleId={ModuleId},sourcePageId={SourcePageId},targetPageId={TargetPageId},pane={Pane},includeSettings={IncludeSettings},moveBahaviour={MoveBahaviour}): targetPage → {TargetPage}", portalSettings, moduleId, sourcePageId, targetPageId, pane, includeSettings, moveBahaviour, targetPage);

            message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Localization.GetString("Prompt_PageNotFound", Constants.LocalResourcesFile), targetPageId));
			Logger.LogError("CopyModule(portalSettings={PortalSettings},moduleId={ModuleId},sourcePageId={SourcePageId},targetPageId={TargetPageId},pane={Pane},includeSettings={IncludeSettings},moveBahaviour={MoveBahaviour}): message → {Message}", portalSettings, moduleId, sourcePageId, targetPageId, pane, includeSettings, moveBahaviour, message);
			Logger.LogDebug("CopyModule(portalSettings={PortalSettings},moduleId={ModuleId},sourcePageId={SourcePageId},targetPageId={TargetPageId},pane={Pane},includeSettings={IncludeSettings},moveBahaviour={MoveBahaviour}): message → {Message}", portalSettings, moduleId, sourcePageId, targetPageId, pane, includeSettings, moveBahaviour, message);

            if (targetPage == null)
            {
                return null;
            }

            if (this.contentVerifier.IsContentExistsForRequestedPortal(targetPage.PortalID, portalSettings))
            {
                try
                {
                    if (moveBahaviour)
                    {
                        ModuleController.Instance.MoveModule(sourceModule.ModuleID, sourceModule.TabID, targetPage.TabID, pane);
                    }
                    else
                    {
                        ModuleController.Instance.CopyModule(sourceModule, targetPage, pane, includeSettings);
                    }

                    ModuleController.Instance.ClearCache(targetPageId);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.InternalServerError, Localization.GetString(moveBahaviour ? "Prompt_ErrorWhileMoving" : "Prompt_ErrorWhileCopying"));
                }

                // get the new module
                return ModuleController.Instance.GetModule(sourceModule.ModuleID, targetPageId, true);
            }
            else
            {
				Logger.LogWarning("<<< Exiting CopyModule(portalSettings={PortalSettings},moduleId={ModuleId},sourcePageId={SourcePageId},targetPageId={TargetPageId},pane={Pane},includeSettings={IncludeSettings},moveBahaviour={MoveBahaviour})", portalSettings, moduleId, sourcePageId, targetPageId, pane, includeSettings, moveBahaviour);
                return null;
            }
        }

        /// <inheritdoc />
        public void DeleteModule(PortalSettings portalSettings, int moduleId, int pageId, out KeyValuePair<HttpStatusCode, string> message)
        {
			Logger.LogError(">>> Entering DeleteModule(portalSettings={PortalSettings},moduleId={ModuleId},pageId={PageId})", portalSettings, moduleId, pageId);
            var module = this.GetModule(portalSettings, moduleId, pageId, out message);

			Logger.LogError("DeleteModule(portalSettings={PortalSettings},moduleId={ModuleId},pageId={PageId}): module → {Module}", portalSettings, moduleId, pageId, module);
            if (module != null)
            {
                try
                {
                    ModuleController.Instance.DeleteTabModule(pageId, moduleId, true);
                    ModuleController.Instance.ClearCache(pageId);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.InternalServerError, string.Format(CultureInfo.CurrentCulture, Localization.GetString("Prompt_FailedtoDeleteModule", Constants.LocalResourcesFile), moduleId));
					Logger.LogError("DeleteModule(portalSettings={PortalSettings},moduleId={ModuleId},pageId={PageId}): message → {Message}", portalSettings, moduleId, pageId, message);
					Logger.LogDebug("DeleteModule(portalSettings={PortalSettings},moduleId={ModuleId},pageId={PageId}): message → {Message}", portalSettings, moduleId, pageId, message);
                }
            }
			Logger.LogWarning("<<< Exiting DeleteModule(portalSettings={PortalSettings},moduleId={ModuleId},pageId={PageId})", portalSettings, moduleId, pageId);
        }

        /// <inheritdoc />
        public ModuleInfo GetModule(PortalSettings portalSettings, int moduleId, int? pageId, out KeyValuePair<HttpStatusCode, string> message)
        {
			Logger.LogError(">>> Entering GetModule(portalSettings={PortalSettings},moduleId={ModuleId},pageId={PageId})", portalSettings, moduleId, pageId);
            message = default(KeyValuePair<HttpStatusCode, string>);
			Logger.LogDebug("GetModule(portalSettings={PortalSettings},moduleId={ModuleId},pageId={PageId}): message → {Message}", portalSettings, moduleId, pageId, message);
            if (pageId.HasValue)
            {
				Logger.LogError("GetModule(portalSettings={PortalSettings},moduleId={ModuleId},pageId={PageId}): message → {Message}", portalSettings, moduleId, pageId, message);
                var module = ModuleController.Instance.GetModule(moduleId, pageId.Value, true);
				Logger.LogError("GetModule(portalSettings={PortalSettings},moduleId={ModuleId},pageId={PageId}): module → {Module}", portalSettings, moduleId, pageId, module);

                if (module != null)
                {
                    if (this.contentVerifier.IsContentExistsForRequestedPortal(module.PortalID, portalSettings, true))
                    {
                        return module;
                    }
                }
                else
                {
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Localization.GetString("Prompt_ModuleNotFound", Constants.LocalResourcesFile), moduleId, pageId));
                    return null;
                }
            }
            else
            {
                var modules = ModuleController.Instance.GetAllTabsModulesByModuleID(moduleId);
				Logger.LogError("GetModule(portalSettings={PortalSettings},moduleId={ModuleId},pageId={PageId}): modules → {Modules}", portalSettings, moduleId, pageId, modules);
                if (modules != null && modules.Count != 0)
                {
                    return modules[0] as ModuleInfo;
                }
            }

            message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Localization.GetString("Prompt_NoModule", Constants.LocalResourcesFile), moduleId));
			Logger.LogWarning("<<< Exiting GetModule(portalSettings={PortalSettings},moduleId={ModuleId},pageId={PageId})", portalSettings, moduleId, pageId);
            return null;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public IEnumerable<ModuleInfo> GetModules(PortalSettings portalSettings, bool? deleted, out int total, string moduleName = null, string moduleTitle = null, int? pageId = null, int pageIndex = 0, int pageSize = 10)
        {
			Logger.LogError(">>> Entering GetModules(portalSettings={PortalSettings},deleted={Deleted},moduleName={ModuleName},moduleTitle={ModuleTitle},pageId={PageId},pageIndex={PageIndex},pageSize={PageSize})", portalSettings, deleted, moduleName, moduleTitle, pageId, pageIndex, pageSize);
            pageIndex = pageIndex < 0 ? 0 : pageIndex;
            pageSize = pageSize > 0 && pageSize <= 100 ? pageSize : 10;
            moduleName = moduleName?.Replace("*", string.Empty);
			Logger.LogError("GetModules(portalSettings={PortalSettings},deleted={Deleted},moduleName={ModuleName},moduleTitle={ModuleTitle},pageId={PageId},pageIndex={PageIndex},pageSize={PageSize}): moduleName → {ModuleName1}", portalSettings, deleted, moduleName, moduleTitle, pageId, pageIndex, pageSize, moduleName);
            moduleTitle = moduleTitle?.Replace("*", string.Empty);
			Logger.LogError("GetModules(portalSettings={PortalSettings},deleted={Deleted},moduleName={ModuleName},moduleTitle={ModuleTitle},pageId={PageId},pageIndex={PageIndex},pageSize={PageSize}): moduleTitle → {ModuleTitle1}", portalSettings, deleted, moduleName, moduleTitle, pageId, pageIndex, pageSize, moduleTitle);
            var modules = ModuleController.Instance.GetModules(portalSettings.PortalId)
                    .Cast<ModuleInfo>().Where(ModulePermissionController.CanViewModule);
            if (!string.IsNullOrEmpty(moduleName))
            {
                modules = modules.Where(module => module.DesktopModule.ModuleName.IndexOf(moduleName, StringComparison.OrdinalIgnoreCase) >= 0);
				Logger.LogError("GetModules(portalSettings={PortalSettings},deleted={Deleted},moduleName={ModuleName},moduleTitle={ModuleTitle},pageId={PageId},pageIndex={PageIndex},pageSize={PageSize}): modules → {Modules}", portalSettings, deleted, moduleName, moduleTitle, pageId, pageIndex, pageSize, modules);
            }

            if (!string.IsNullOrEmpty(moduleTitle))
            {
                modules = modules.Where(module => module.ModuleTitle.IndexOf(moduleTitle, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Return only deleted modules with matching criteria.
            if (pageId.HasValue && pageId.Value > 0)
            {
                modules = modules.Where(x => x.TabID == pageId.Value);
            }

            if (deleted.HasValue)
            {
                modules = modules.Where(module => module.IsDeleted == deleted);
            }

            // Get distincts.
            modules = modules.GroupBy(x => x.ModuleID).Select(group => group.First()).OrderBy(x => x.ModuleID);
            var moduleInfos = modules as IList<ModuleInfo> ?? modules.ToList();
			Logger.LogError("GetModules(portalSettings={PortalSettings},deleted={Deleted},moduleName={ModuleName},moduleTitle={ModuleTitle},pageId={PageId},pageIndex={PageIndex},pageSize={PageSize}): moduleInfos → {ModuleInfos}", portalSettings, deleted, moduleName, moduleTitle, pageId, pageIndex, pageSize, moduleInfos);
            total = moduleInfos.Count;
			Logger.LogError("GetModules(portalSettings={PortalSettings},deleted={Deleted},moduleName={ModuleName},moduleTitle={ModuleTitle},pageId={PageId},pageIndex={PageIndex},pageSize={PageSize}): total → {Total}", portalSettings, deleted, moduleName, moduleTitle, pageId, pageIndex, pageSize, total);
			Logger.LogDebug("GetModules(portalSettings={PortalSettings},deleted={Deleted},moduleName={ModuleName},moduleTitle={ModuleTitle},pageId={PageId},pageIndex={PageIndex},pageSize={PageSize}): total → {Total}", portalSettings, deleted, moduleName, moduleTitle, pageId, pageIndex, pageSize, total);
            return moduleInfos.Skip(pageIndex * pageSize).Take(pageSize);
        }

        /// <inheritdoc />
        protected override Func<IModulesController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new ModulesController();
        }
    }
}
