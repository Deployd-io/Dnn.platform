// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Dto;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A Persona Bar API controller for user settings.</summary>
    [MenuPermission(Scope = ServiceScope.Regular)]
    public class UserSettingsController : PersonaBarApiController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserSettingsController));
        private readonly IPortalController portalController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IPortalGroupController portalGroupController;

        /// <summary>Initializes a new instance of the <see cref="UserSettingsController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public UserSettingsController()
            : this(null, null, null)
        {
		Logger.LogDebug(">>> Entering UserSettingsController()");
		Logger.LogDebug("<<< Exiting UserSettingsController()");
		Logger.LogDebug(">>> Entering UserSettingsController(portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController})", portalController, appStatus, portalGroupController);
		Logger.LogDebug("UserSettingsController(portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): appStatus → {AppStatus1}", portalController, appStatus, portalGroupController, appStatus);
		Logger.LogDebug("UserSettingsController(portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): portalGroupController → {PortalGroupController1}", portalController, appStatus, portalGroupController, portalGroupController);
        }

        /// <summary>Initializes a new instance of the <see cref="UserSettingsController"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        public UserSettingsController(IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController)
        {
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
			Logger.LogDebug("<<< Exiting UserSettingsController(portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController})", portalController, appStatus, portalGroupController);
        }

        /// <summary>Update Persona Bar's User Settings.</summary>
        /// <param name="settings">The user settings.</param>
        /// <returns>A response with an empty object.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateUserSettings(IDictionary<string, object> settings)
        {
			Logger.LogDebug(">>> Entering UpdateUserSettings(settings={Settings})", settings);
            try
            {
                var controller = PersonaBarUserSettingsController.Instance;
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, this.PortalSettings.PortalId);
				Logger.LogDebug("UpdateUserSettings(settings={Settings}): portalId → {PortalId}", settings, portalId);

                var userSettings = new UserSettings();
				Logger.LogDebug("UpdateUserSettings(settings={Settings}): userSettings → {UserSettings}", settings, userSettings);
                settings.ForEach(kvp =>
                {
                    userSettings.Add(kvp.Key, kvp.Value);
                });

                controller.UpdatePersonaBarUserSettings(userSettings, this.UserInfo.UserID, portalId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in UpdateUserSettings(settings={Settings}): {Arg1}", settings, ex.getMessage());
                Exceptions.LogException(ex);
				Logger.LogDebug("<<< Exiting UpdateUserSettings(settings={Settings})", settings);
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }
    }
}
