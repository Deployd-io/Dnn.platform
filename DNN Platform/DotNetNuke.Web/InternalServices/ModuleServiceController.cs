// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A web API controller for module information.</summary>
    [DnnAuthorize]
    public class ModuleServiceController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleServiceController));
        private readonly IHostSettings hostSettings;
        private readonly DataProvider dataProvider;

        /// <summary>Initializes a new instance of the <see cref="ModuleServiceController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ModuleServiceController()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering ModuleServiceController()");
		Logger.LogDebug("<<< Exiting ModuleServiceController()");
		Logger.LogDebug(">>> Entering ModuleServiceController(hostSettings={HostSettings},dataProvider={DataProvider})", hostSettings, dataProvider);
		Logger.LogDebug("ModuleServiceController(hostSettings={HostSettings},dataProvider={DataProvider}): dataProvider → {DataProvider1}", hostSettings, dataProvider, dataProvider);
        }

        /// <summary>Initializes a new instance of the <see cref="ModuleServiceController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="dataProvider">The data provider.</param>
        public ModuleServiceController(IHostSettings hostSettings, DataProvider dataProvider)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.dataProvider = dataProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();
			Logger.LogDebug("<<< Exiting ModuleServiceController(hostSettings={HostSettings},dataProvider={DataProvider})", hostSettings, dataProvider);
        }

        /// <summary>Gets a value determining whether a module is shareable.</summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A response with an object containing <c>Shareable</c> and <c>RequiredWarning</c> fields.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = "Registered Users")]
        public HttpResponseMessage GetModuleShareable(int moduleId, int tabId, int portalId = -1)
        {
			Logger.LogDebug(">>> Entering GetModuleShareable(moduleId={ModuleId},tabId={TabId},portalId={PortalId})", moduleId, tabId, portalId);
            var requiresWarning = false;
            if (portalId <= -1)
			Logger.LogDebug("GetModuleShareable(moduleId={ModuleId},tabId={TabId},portalId={PortalId}): requiresWarning → {RequiresWarning}", moduleId, tabId, portalId, requiresWarning);
            {
                var portalDict = PortalController.GetPortalDictionary(this.hostSettings, this.dataProvider);
				Logger.LogDebug("GetModuleShareable(moduleId={ModuleId},tabId={TabId},portalId={PortalId}): portalDict → {PortalDict}", moduleId, tabId, portalId, portalDict);
                portalId = portalDict[tabId];
				Logger.LogDebug("GetModuleShareable(moduleId={ModuleId},tabId={TabId},portalId={PortalId}): portalId → {PortalId1}", moduleId, tabId, portalId, portalId);
            }
            else
            {
                portalId = this.FixPortalId(portalId);
            }

            DesktopModuleInfo desktopModule;
            if (tabId < 0)
            {
                desktopModule = DesktopModuleController.GetDesktopModule(this.hostSettings, moduleId, portalId);
            }
            else
            {
                var moduleInfo = ModuleController.Instance.GetModule(moduleId, tabId, false);
				Logger.LogDebug("GetModuleShareable(moduleId={ModuleId},tabId={TabId},portalId={PortalId}): moduleInfo → {ModuleInfo}", moduleId, tabId, portalId, moduleInfo);

                desktopModule = moduleInfo.DesktopModule;

                requiresWarning = moduleInfo.PortalID != this.PortalSettings.PortalId && desktopModule.Shareable == ModuleSharing.Unknown;
            }

            if (desktopModule == null)
            {
                var message = $"Cannot find module ID {moduleId} (tab ID {tabId}, portal ID {portalId})";
				Logger.LogDebug("GetModuleShareable(moduleId={ModuleId},tabId={TabId},portalId={PortalId}): message → {Message}", moduleId, tabId, portalId, message);
                Logger.Error(message);
			Logger.LogDebug("<<< Exiting GetModuleShareable(moduleId={ModuleId},tabId={TabId},portalId={PortalId})", moduleId, tabId, portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Shareable = desktopModule.Shareable.ToString(), RequiresWarning = requiresWarning });
        }

        /// <summary>Moves a module.</summary>
        /// <param name="postData">Information about the move request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnPageEditor]
        public HttpResponseMessage MoveModule(MoveModuleDTO postData)
        {
			Logger.LogDebug(">>> Entering MoveModule(postData={PostData})", postData);
            var moduleOrder = postData.ModuleOrder;
            if (moduleOrder > 0)
            {
                // DNN-7099: the deleted modules won't show in page, so when the module index calculated from client, it will lost the
                // index count of deleted modules and will cause order issue.
                var deletedModules = ModuleController.Instance.GetTabModules(postData.TabId).Values.Where(m => m.IsDeleted);
				Logger.LogDebug("MoveModule(postData={PostData}): deletedModules → {DeletedModules}", postData, deletedModules);
                foreach (var module in deletedModules)
                {
                    if (module.ModuleOrder < moduleOrder && module.PaneName == postData.Pane)
                    {
                        moduleOrder += 2;
                    }
                }
            }

            ModuleController.Instance.UpdateModuleOrder(postData.TabId, postData.ModuleId, moduleOrder, postData.Pane);
            ModuleController.Instance.UpdateTabModuleOrder(postData.TabId);

			Logger.LogDebug("<<< Exiting MoveModule(postData={PostData})", postData);
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>Web method that deletes a tab module.</summary>
        /// <remarks>This has been introduced for integration testing purposes.</remarks>
        /// <param name="deleteModuleDto">delete module dto.</param>
        /// <returns>Http response message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage DeleteModule(DeleteModuleDto deleteModuleDto)
        {
			Logger.LogDebug(">>> Entering DeleteModule(deleteModuleDto={DeleteModuleDto})", deleteModuleDto);
            ModuleController.Instance.DeleteTabModule(deleteModuleDto.TabId, deleteModuleDto.ModuleId, deleteModuleDto.SoftDelete);

			Logger.LogDebug("<<< Exiting DeleteModule(deleteModuleDto={DeleteModuleDto})", deleteModuleDto);
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        private int FixPortalId(int portalId)
        {
			Logger.LogDebug(">>> Entering FixPortalId(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting FixPortalId(portalId={PortalId})", portalId);
            return this.UserInfo.IsSuperUser &&
                   this.PortalSettings.PortalId != portalId &&
                   PortalController.Instance.GetPortals().OfType<IPortalInfo>().Any(x => x.PortalId == portalId)
                ? portalId
                : this.PortalSettings.PortalId;
        }

        /// <summary>A data transfer object with information about moving a module.</summary>
        public class MoveModuleDTO
        {
            /// <summary>Gets or sets the module's ID.</summary>
            public int ModuleId { get; set; }

            /// <summary>Gets or sets the module order.</summary>
            public int ModuleOrder { get; set; }

            /// <summary>Gets or sets the pane name.</summary>
            public string Pane { get; set; }

            /// <summary>Gets or sets the tab ID.</summary>
            public int TabId { get; set; }
        }

        /// <summary>A data transfer object with information about a request to delete a module.</summary>
        public class DeleteModuleDto
        {
            /// <summary>Gets or sets the module ID.</summary>
            public int ModuleId { get; set; }

            /// <summary>Gets or sets the tab ID.</summary>
            public int TabId { get; set; }

            /// <summary>Gets or sets a value indicating whether it is a soft or hard delete.</summary>
            public bool SoftDelete { get; set; }
        }
    }
}
