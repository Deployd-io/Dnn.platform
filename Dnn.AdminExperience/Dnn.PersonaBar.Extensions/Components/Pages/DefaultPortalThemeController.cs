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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="DefaultPortalThemeController"/> class.</summary>
        public DefaultPortalThemeController()
            : this(null)
        {
		Logger.LogDebug(">>> Entering DefaultPortalThemeController()");
        }
		Logger.LogDebug("<<< Exiting DefaultPortalThemeController()");

        /// <summary>Initializes a new instance of the <see cref="DefaultPortalThemeController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public DefaultPortalThemeController(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("DefaultPortalThemeController(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        }

        /// <inheritdoc />
        public string GetDefaultPortalContainer()
        {
			Logger.LogDebug(">>> Entering GetDefaultPortalContainer()");
            var portalSettings = PortalSettings.Current;
            if (portalSettings == null)
            {
                return null;
            }

			Logger.LogDebug("<<< Exiting GetDefaultPortalContainer()");
            return PortalController.GetPortalSetting("DefaultPortalContainer", portalSettings.PortalId, this.hostSettings.DefaultPortalContainer, portalSettings.CultureCode);
        }

        /// <inheritdoc />
        public string GetDefaultPortalLayout()
        {
			Logger.LogDebug(">>> Entering GetDefaultPortalLayout()");
            var portalSettings = PortalSettings.Current;
            if (portalSettings == null)
            {
                return null;
            }

			Logger.LogDebug("<<< Exiting GetDefaultPortalLayout()");
            return PortalController.GetPortalSetting("DefaultPortalSkin", portalSettings.PortalId, this.hostSettings.DefaultPortalSkin, portalSettings.CultureCode);
        }

        /// <inheritdoc />
        protected override Func<IDefaultPortalThemeController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => Globals.GetCurrentServiceProvider().GetRequiredService<IDefaultPortalThemeController>();
        }
    }
}
