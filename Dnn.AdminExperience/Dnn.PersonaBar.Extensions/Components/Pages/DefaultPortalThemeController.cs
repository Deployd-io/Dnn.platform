// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Components
{
    using System;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    public class DefaultPortalThemeController : ServiceLocator<IDefaultPortalThemeController, DefaultPortalThemeController>, IDefaultPortalThemeController
    {
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="DefaultPortalThemeController"/> class.</summary>
        public DefaultPortalThemeController()
            : this(null)
        {
		eventLogger.LogDebug(">>> Entering DefaultPortalThemeController()");
        }
		eventLogger.LogDebug("<<< Exiting DefaultPortalThemeController()");

        /// <summary>Initializes a new instance of the <see cref="DefaultPortalThemeController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public DefaultPortalThemeController(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			eventLogger.LogDebug("DefaultPortalThemeController(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        }

        /// <inheritdoc />
        public string GetDefaultPortalContainer()
        {
			eventLogger.LogDebug(">>> Entering GetDefaultPortalContainer()");
            var portalSettings = PortalSettings.Current;
            if (portalSettings == null)
            {
			eventLogger.LogDebug("<<< Exiting GetDefaultPortalContainer()");
                return null;
            }

            return PortalController.GetPortalSetting("DefaultPortalContainer", portalSettings.PortalId, this.hostSettings.DefaultPortalContainer, portalSettings.CultureCode);
        }

        /// <inheritdoc />
        public string GetDefaultPortalLayout()
        {
			eventLogger.LogDebug(">>> Entering GetDefaultPortalLayout()");
            var portalSettings = PortalSettings.Current;
            if (portalSettings == null)
            {
			eventLogger.LogDebug("<<< Exiting GetDefaultPortalLayout()");
                return null;
            }

            return PortalController.GetPortalSetting("DefaultPortalSkin", portalSettings.PortalId, this.hostSettings.DefaultPortalSkin, portalSettings.CultureCode);
        }

        /// <inheritdoc />
        protected override Func<IDefaultPortalThemeController> GetFactory()
        {
			eventLogger.LogDebug(">>> Entering GetFactory()");
			eventLogger.LogDebug("<<< Exiting GetFactory()");
            return () => Globals.GetCurrentServiceProvider().GetRequiredService<IDefaultPortalThemeController>();
        }
    }
}
