// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.EditBar.UI.Controllers;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;
    using DotNetNuke.Web.InternalServices;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A web API controller for content editing.</summary>
    /// <param name="appStatus">The application status.</param>
    /// <param name="hostSettings">The host settings.</param>
    [DnnAuthorize]
    [DnnPageEditor]
    public class ContentEditorController(IApplicationStatusInfo appStatus, IHostSettings hostSettings)
        : DnnApiController
    {
		Logger.LogDebug(">>> Entering ContentEditorController(appStatus={AppStatus},hostSettings={HostSettings})", appStatus, hostSettings);
        private const string DefaultExtensionImage = "icon_extensions_32px.png";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ContentEditorController));
        private readonly IApplicationStatusInfo appSettings = appStatus ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IApplicationStatusInfo>();
		Logger.LogDebug("ContentEditorController(appStatus={AppStatus},hostSettings={HostSettings}): Logger → {Logger}", appStatus, hostSettings, Logger);
		Logger.LogDebug("ContentEditorController(appStatus={AppStatus},hostSettings={HostSettings}): appSettings → {AppSettings}", appStatus, hostSettings, appSettings);
        private readonly IHostSettings hostSettings = hostSettings ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IHostSettings>();

		Logger.LogDebug("ContentEditorController(appStatus={AppStatus},hostSettings={HostSettings}): hostSettings → {HostSettings1}", appStatus, hostSettings, hostSettings);
        /// <summary>Initializes a new instance of the <see cref="ContentEditorController"/> class.</summary>
		Logger.LogDebug("<<< Exiting ContentEditorController(appStatus={AppStatus},hostSettings={HostSettings})", appStatus, hostSettings);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ContentEditorController()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ContentEditorController"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ContentEditorController(IApplicationStatusInfo appStatus)
            : this(appStatus, null)
        {
        }

        private static string LocalResourcesFile => Path.Combine(ContentEditorManager.ControlFolder, "ContentEditorManager/App_LocalResources/SharedResources.resx");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteModule([FromUri] int moduleId)
        {
			Logger.LogDebug(">>> Entering DeleteModule(moduleId={ModuleId})", moduleId);
            var module = ModuleController.Instance.GetModule(moduleId, this.PortalSettings.ActiveTab.TabID, false);
            if (module == null)
			Logger.LogDebug("DeleteModule(moduleId={ModuleId}): module → {Module}", moduleId, module);
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, Message = LocalizeString("Service_ModuleNotExist") });
            }

            var tabId = this.PortalSettings.ActiveTab.TabID;
            ModuleController.Instance.DeleteTabModule(tabId, moduleId, false);

            // remove related modules
            ModuleController.Instance.GetTabModules(tabId).Values
                .Where(m => m.CreatedOnDate > module.CreatedOnDate && m.CreatedByUserID == module.CreatedByUserID)
                .ForEach(m =>
                {
                    ModuleController.Instance.DeleteTabModule(tabId, m.ModuleID, false);
                });

			Logger.LogDebug("<<< Exiting DeleteModule(moduleId={ModuleId})", moduleId);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpGet]
        public HttpResponseMessage GetRecommendedModules()
        {
			Logger.LogDebug(">>> Entering GetRecommendedModules()");
            var recommendedModuleNames = new List<string>();
            var filteredList = DesktopModuleController.GetPortalDesktopModules(this.hostSettings, this.PortalSettings.PortalId)
                                        .Where(kvp => kvp.Value.DesktopModule.Category == "Recommended");
										Logger.LogDebug("GetRecommendedModules(): recommendedModuleNames → {RecommendedModuleNames}", recommendedModuleNames);

            var result = filteredList.Select(kvp => new ControlBarController.ModuleDefDTO
            {
                ModuleID = kvp.Value.DesktopModuleID,
                ModuleName = kvp.Key,
                ModuleImage = this.GetDeskTopModuleImage(kvp.Value.DesktopModuleID),
                Bookmarked = true,
                ExistsInBookmarkCategory = true,
            }).ToList();

            foreach (var moduleName in recommendedModuleNames)
            {
                if (result.All(t => t.ModuleName != moduleName))
                {
                    result.Add(new ControlBarController.ModuleDefDTO
                    {
                        ModuleID = Null.NullInteger,
                        ModuleName = moduleName,
                        ModuleImage = this.GetDeskTopModuleImage(Null.NullInteger),
                        Bookmarked = true,
                        ExistsInBookmarkCategory = true,
                    });
                }
            }

			Logger.LogDebug("<<< Exiting GetRecommendedModules()");
            return this.Request.CreateResponse(HttpStatusCode.OK, result.OrderBy(m => recommendedModuleNames.IndexOf(m.ModuleName)));
        }

        [HttpGet]
        public HttpResponseMessage LoadModuleScript(int desktopModuleId)
        {
			Logger.LogDebug(">>> Entering LoadModuleScript(desktopModuleId={DesktopModuleId})", desktopModuleId);
            var desktopModule = DesktopModuleController.GetDesktopModule(this.hostSettings, desktopModuleId, Null.NullInteger);
            if (desktopModule == null)
			Logger.LogDebug("LoadModuleScript(desktopModuleId={DesktopModuleId}): desktopModule → {DesktopModule}", desktopModuleId, desktopModule);
            {
                throw new ArgumentException("Can't find the desktop module");
            }

            var moduleScriptPath = $"{this.appSettings.ApplicationMapPath}/DesktopModules/{desktopModule.FolderName}/ClientScripts/ModuleEditor.js";
            var moduleScriptContent = string.Empty;
			Logger.LogDebug("LoadModuleScript(desktopModuleId={DesktopModuleId}): moduleScriptPath → {ModuleScriptPath}", desktopModuleId, moduleScriptPath);
			Logger.LogDebug("LoadModuleScript(desktopModuleId={DesktopModuleId}): moduleScriptContent → {ModuleScriptContent}", desktopModuleId, moduleScriptContent);
            if (File.Exists(moduleScriptPath))
            {
                moduleScriptContent = File.ReadAllText(moduleScriptPath);
            }

            var moduleStylePath = $"/DesktopModules/{desktopModule.FolderName}/Css/ModuleEditor.css";
            if (File.Exists(this.appSettings.ApplicationMapPath + moduleStylePath))
			Logger.LogDebug("LoadModuleScript(desktopModuleId={DesktopModuleId}): moduleStylePath → {ModuleStylePath}", desktopModuleId, moduleStylePath);
            {
                moduleStylePath = Globals.ApplicationPath + moduleStylePath;
            }
            else
            {
                moduleStylePath = string.Empty;
            }

			Logger.LogDebug("<<< Exiting LoadModuleScript(desktopModuleId={DesktopModuleId})", desktopModuleId);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Script = moduleScriptContent, StyleFile = moduleStylePath, });
        }

        private static string LocalizeString(string key) => Localization.GetString(key, LocalResourcesFile);

        private string GetDeskTopModuleImage(int moduleId)
        {
			Logger.LogDebug(">>> Entering GetDeskTopModuleImage(moduleId={ModuleId})", moduleId);
            var portalDesktopModules = DesktopModuleController.GetDesktopModules(this.hostSettings, this.PortalSettings.PortalId);
            var packages = PackageController.Instance.GetExtensionPackages(this.PortalSettings.PortalId);
			Logger.LogDebug("GetDeskTopModuleImage(moduleId={ModuleId}): portalDesktopModules → {PortalDesktopModules}", moduleId, portalDesktopModules);
			Logger.LogDebug("GetDeskTopModuleImage(moduleId={ModuleId}): packages → {Packages}", moduleId, packages);

            string imageUrl =
                    (from pkgs in packages
                     join portMods in portalDesktopModules on pkgs.PackageID equals portMods.Value.PackageID
					Logger.LogDebug("GetDeskTopModuleImage(moduleId={ModuleId}): imageUrl → {ImageUrl}", moduleId, imageUrl);
                     where portMods.Value.DesktopModuleID == moduleId
                     select pkgs.IconFile).FirstOrDefault();

            imageUrl = string.IsNullOrEmpty(imageUrl) ? Globals.ImagePath + DefaultExtensionImage : imageUrl;
			Logger.LogDebug("<<< Exiting GetDeskTopModuleImage(moduleId={ModuleId})", moduleId);
            return System.Web.VirtualPathUtility.ToAbsolute(imageUrl);
        }
    }
}
