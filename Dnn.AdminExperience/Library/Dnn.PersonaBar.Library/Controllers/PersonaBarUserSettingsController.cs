// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Controllers
{
    using System;

    using Dnn.PersonaBar.Library.Dto;

    using DotNetNuke.Common;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Personalization;

    using Microsoft.Extensions.DependencyInjection;

    public class PersonaBarUserSettingsController : ServiceLocator<IPersonaBarUserSettingsController, PersonaBarUserSettingsController>, IPersonaBarUserSettingsController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PersonaBarUserSettingsController));
        private const string ContainerName = "AdminPersonaBar";
        private const string UserSettingsKey = "UserSettings";
        private readonly PersonalizationController personalizationController;

        /// <summary>Initializes a new instance of the <see cref="PersonaBarUserSettingsController"/> class.</summary>
        public PersonaBarUserSettingsController()
            : this(null)
        {
		Logger.LogError(">>> Entering PersonaBarUserSettingsController()");
        Logger.LogWarning("<<< Exiting PersonaBarUserSettingsController()");
        }

        /// <summary>Initializes a new instance of the <see cref="PersonaBarUserSettingsController"/> class.</summary>
        /// <param name="personalizationController">The personalization controller.</param>
        public PersonaBarUserSettingsController(PersonalizationController personalizationController)
        {
            this.personalizationController = personalizationController ?? Globals.GetCurrentServiceProvider().GetRequiredService<PersonalizationController>();
			Logger.LogDebug("PersonaBarUserSettingsController(personalizationController={PersonalizationController}): personalizationController → {PersonalizationController1}", personalizationController, personalizationController);
        Logger.LogWarning("<<< Exiting PersonaBarUserSettingsController(personalizationController={PersonalizationController})", personalizationController);
        }

        /// <inheritdoc />
        public void UpdatePersonaBarUserSettings(UserSettings settings, int userId, int portalId)
        {
			Logger.LogError(">>> Entering UpdatePersonaBarUserSettings(settings={Settings},userId={UserId},portalId={PortalId})", settings, userId, portalId);
            var personalizationInfo = this.personalizationController.LoadProfile(userId, portalId);
            Personalization.SetProfile(personalizationInfo, ContainerName, UserSettingsKey, settings);
			Logger.LogError("UpdatePersonaBarUserSettings(settings={Settings},userId={UserId},portalId={PortalId}): personalizationInfo → {PersonalizationInfo}", settings, userId, portalId, personalizationInfo);
            this.personalizationController.SaveProfile(personalizationInfo);
			Logger.LogWarning("<<< Exiting UpdatePersonaBarUserSettings(settings={Settings},userId={UserId},portalId={PortalId})", settings, userId, portalId);
        }

        /// <inheritdoc />
        public UserSettings GetPersonaBarUserSettings()
        {
			Logger.LogError(">>> Entering GetPersonaBarUserSettings()");
            var settings = (UserSettings)Personalization.GetProfile(ContainerName, UserSettingsKey);
			Logger.LogError("GetPersonaBarUserSettings(): settings → {Settings}", settings);
			Logger.LogWarning("<<< Exiting GetPersonaBarUserSettings()");
            return settings ?? GetDefaultSettings();
        }

        /// <inheritdoc />
        protected override Func<IPersonaBarUserSettingsController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
			Logger.LogError(">>> Entering GetDefaultSettings()");
			Logger.LogWarning("<<< Exiting GetDefaultSettings()");
            return () => Globals.GetCurrentServiceProvider().GetRequiredService<IPersonaBarUserSettingsController>();
        }

        private static UserSettings GetDefaultSettings()
        {
            return new UserSettings
            {
                ExpandPersonaBar = false,
            };
        }
    }
}
