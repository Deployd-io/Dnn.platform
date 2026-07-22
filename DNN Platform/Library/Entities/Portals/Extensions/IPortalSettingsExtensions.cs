// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Extensions
{
    using DotNetNuke.Abstractions.Portals;

    /// <summary>
    /// Extends the <see cref="IPortalSettings"/> interface.
    /// </summary>
    public static class IPortalSettingsExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>
        /// Gets the styles for the portal.
        /// </summary>
        /// <param name="portalSettings">The portal settings to the the styles from.</param>
        /// <returns><see cref="IPortalStyles"/>.</returns>
        public static IPortalStyles GetStyles(this IPortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering GetStyles(portalSettings={PortalSettings})", portalSettings);
            var repo = new PortalStylesRepository();
			Logger.LogDebug("GetStyles(portalSettings={PortalSettings}): repo → {Repo}", portalSettings, repo);
			Logger.LogDebug("<<< Exiting GetStyles(portalSettings={PortalSettings})", portalSettings);
            return repo.GetSettings(portalSettings.PortalId);
        }
    }
}
