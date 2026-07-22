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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly PersonalizationController personalizationController;

        /// <summary>Initializes a new instance of the <see cref="CommonController"/> class.</summary>
        public CommonController()
            : this(null)
        {
		Logger.LogDebug(">>> Entering CommonController()");
		Logger.LogDebug("<<< Exiting CommonController()");
		Logger.LogDebug(">>> Entering CommonController(personalizationController={PersonalizationController})", personalizationController);
		Logger.LogDebug("<<< Exiting CommonController(personalizationController={PersonalizationController})", personalizationController);
        }

        /// <summary>Initializes a new instance of the <see cref="CommonController"/> class.</summary>
        /// <param name="personalizationController">The personalization controller.</param>
        public CommonController(PersonalizationController personalizationController)
        {
            this.personalizationController = personalizationController;
			Logger.LogDebug("CommonController(personalizationController={PersonalizationController}): personalizationController → {PersonalizationController1}", personalizationController, personalizationController);
			Logger.LogDebug("CommonController(personalizationController={PersonalizationController}): this.personalizationController → {Arg1}", personalizationController, this.personalizationController);
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage CheckAuthorized()
        {
			Logger.LogDebug(">>> Entering CheckAuthorized()");
			Logger.LogDebug("<<< Exiting CheckAuthorized()");
            return this.Request.CreateResponse(HttpStatusCode.OK, new { success = this.IsPageEditor() });
        }

        [HttpGet]
        [DnnPageEditor]
        public HttpResponseMessage GetUserSetting(string key)
        {
			Logger.LogDebug(">>> Entering GetUserSetting(key={Key})", key);
            var personalization = this.personalizationController.LoadProfile(this.UserInfo.UserID, this.PortalSettings.PortalId);
            var value = personalization.Profile[key + this.PortalSettings.PortalId];
			Logger.LogDebug("GetUserSetting(key={Key}): personalization → {Personalization}", key, personalization);

            if (value == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Value = false });
            }

            var userSetting = new UserSetting
            {
                Key = key,
                Value = value,
            };

			Logger.LogDebug("<<< Exiting GetUserSetting(key={Key})", key);
            return this.Request.CreateResponse(HttpStatusCode.OK, userSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnPageEditor]
        public HttpResponseMessage SetUserSetting(UserSetting setting)
        {
			Logger.LogDebug(">>> Entering SetUserSetting(setting={Setting})", setting);
            var personalization = this.personalizationController.LoadProfile(this.UserInfo.UserID, this.PortalSettings.PortalId);
            personalization.Profile[setting.Key + this.PortalSettings.PortalId] = setting.Value;
			Logger.LogDebug("SetUserSetting(setting={Setting}): personalization → {Personalization}", setting, personalization);
			Logger.LogDebug("SetUserSetting(setting={Setting}): personalization.IsModified → {Arg1}", setting, personalization.IsModified);
            personalization.IsModified = true;
            this.personalizationController.SaveProfile(personalization);

			Logger.LogDebug("<<< Exiting SetUserSetting(setting={Setting})", setting);
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        private static bool IsModuleAdmin(PortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering IsModuleAdmin(portalSettings={PortalSettings})", portalSettings);
            bool isModuleAdmin = false;
            foreach (ModuleInfo objModule in TabController.CurrentPage.Modules)
            {
			Logger.LogDebug("IsModuleAdmin(portalSettings={PortalSettings}): isModuleAdmin → {IsModuleAdmin}", portalSettings, isModuleAdmin);
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

			Logger.LogDebug("<<< Exiting IsModuleAdmin(portalSettings={PortalSettings})", portalSettings);
            return portalSettings.ControlPanelSecurity == PortalSettings.ControlPanelPermission.ModuleEditor && isModuleAdmin;
        }

        private bool IsPageEditor()
        {
			Logger.LogDebug(">>> Entering IsPageEditor()");
			Logger.LogDebug("<<< Exiting IsPageEditor()");
            return PagePermissionsAttributesHelper.HasTabPermission("EDIT,CONTENT,MANAGE") || IsModuleAdmin(this.PortalSettings);
        }
    }
}
