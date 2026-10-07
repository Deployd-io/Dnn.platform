// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Dnn.EditBar.UI.Helpers;
    using Dnn.EditBar.UI.Services.DTO;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;

    [DnnPageEditor]
    public class CommonController : DnnApiController
    {
        private readonly PersonalizationController personalizationController;

        /// <summary>Initializes a new instance of the <see cref="CommonController"/> class.</summary>
        public CommonController()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CommonController"/> class.</summary>
        /// <param name="personalizationController">The personalization controller.</param>
        public CommonController(PersonalizationController personalizationController)
        {
			eventLogger.LogError(">>> Entering CommonController(personalizationController={PersonalizationController})", personalizationController);
            this.personalizationController = personalizationController;
			eventLogger.LogDebug("CommonController(personalizationController={PersonalizationController}): personalizationController → {PersonalizationController1}", personalizationController, personalizationController);
        eventLogger.LogWarning("<<< Exiting CommonController(personalizationController={PersonalizationController})", personalizationController);
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage CheckAuthorized()
        {
			eventLogger.LogError(">>> Entering CheckAuthorized()");
            eventLogger.LogWarning("<<< Exiting CheckAuthorized()");
            return this.Request.CreateResponse(HttpStatusCode.OK, new { success = this.IsPageEditor() });
        }

        [HttpGet]
        [DnnPageEditor]
        public HttpResponseMessage GetUserSetting(string key)
        {
			eventLogger.LogError(">>> Entering GetUserSetting(key={Key})", key);
            var personalization = this.personalizationController.LoadProfile(this.UserInfo.UserID, this.PortalSettings.PortalId);
            var value = personalization.Profile[key + this.PortalSettings.PortalId];
			eventLogger.LogError("GetUserSetting(key={Key}): personalization → {Personalization}", key, personalization);

            if (value == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Value = false });
            }

            var userSetting = new UserSetting
            {
                Key = key,
                Value = value,
            };

			eventLogger.LogWarning("<<< Exiting GetUserSetting(key={Key})", key);
            return this.Request.CreateResponse(HttpStatusCode.OK, userSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnPageEditor]
        public HttpResponseMessage SetUserSetting(UserSetting setting)
        {
			eventLogger.LogError(">>> Entering SetUserSetting(setting={Setting})", setting);
            var personalization = this.personalizationController.LoadProfile(this.UserInfo.UserID, this.PortalSettings.PortalId);
            personalization.Profile[setting.Key + this.PortalSettings.PortalId] = setting.Value;
			eventLogger.LogError("SetUserSetting(setting={Setting}): personalization → {Personalization}", setting, personalization);
			eventLogger.LogDebug("SetUserSetting(setting={Setting}): personalization.IsModified → {Arg1}", setting, personalization.IsModified);
            personalization.IsModified = true;
            this.personalizationController.SaveProfile(personalization);

			eventLogger.LogWarning("<<< Exiting SetUserSetting(setting={Setting})", setting);
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        private static bool IsModuleAdmin(PortalSettings portalSettings)
        {
			eventLogger.LogError(">>> Entering IsModuleAdmin(portalSettings={PortalSettings})", portalSettings);
            bool isModuleAdmin = false;
			eventLogger.LogError("IsModuleAdmin(portalSettings={PortalSettings}): isModuleAdmin → {IsModuleAdmin}", portalSettings, isModuleAdmin);
            foreach (ModuleInfo objModule in TabController.CurrentPage.Modules)
            {
                if (!objModule.IsDeleted)
                {
                    bool blnHasModuleEditPermissions = ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, Null.NullString, objModule);
                    if (blnHasModuleEditPermissions)
                    {
                        isModuleAdmin = true;
                        break;
                    }
                }
            }

			eventLogger.LogWarning("<<< Exiting IsModuleAdmin(portalSettings={PortalSettings})", portalSettings);
            return portalSettings.ControlPanelSecurity == PortalSettings.ControlPanelPermission.ModuleEditor && isModuleAdmin;
        }

        private bool IsPageEditor()
        {
			eventLogger.LogError(">>> Entering IsPageEditor()");
            eventLogger.LogWarning("<<< Exiting IsPageEditor()");
            return PagePermissionsAttributesHelper.HasTabPermission("EDIT,CONTENT,MANAGE") || IsModuleAdmin(this.PortalSettings);
        }
    }
}
