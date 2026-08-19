// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Styles.Services
{
    using System.Globalization;
    using System.IO;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Portals.Extensions;
    using DotNetNuke.Web.Api;

    /// <summary>
    /// REST API Controller for the Styles module.
    /// </summary>
    [MenuPermission(MenuName = Components.Constants.MenuName, Scope = ServiceScope.Admin)]
    public class StylesController : PersonaBarApiController
    {
        /// <summary>
        /// Gets the styles.
        /// </summary>
        /// <returns>The current portal style settings.</returns>
        [HttpGet]
        public IHttpActionResult GetStyles()
        {
			eventLogger.LogDebug(">>> Entering GetStyles()");
            if (!this.CanEdit())
            {
                return this.Unauthorized();
            }

            var repo = new PortalStylesRepository();
			eventLogger.LogDebug("GetStyles(): repo → {Repo}", repo);
            var settings = repo.GetSettings(this.PortalId);
			eventLogger.LogDebug("GetStyles(): settings → {Settings}", settings);
			eventLogger.LogDebug("<<< Exiting GetStyles()");
            return this.Ok(settings);
        }

        /// <summary>
        /// Saves the styles.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>OK.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IHttpActionResult SaveStyles(PortalStyles settings)
        {
			eventLogger.LogDebug(">>> Entering SaveStyles(settings={Settings})", settings);
            if (!this.CanEdit())
            {
                return this.Unauthorized();
            }

            // Save to database
            var repo = new PortalStylesRepository();
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            repo.SaveSettings(this.PortalId, settings);

			eventLogger.LogDebug("SaveStyles(settings={Settings}): repo → {Repo}", settings, repo);
			eventLogger.LogInformation("SaveStyles(settings)={Settings}: save query executed in {Arg1} ms", settings, __logStart.ElapsedMilliseconds);
            // Clear cache
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.PortalStylesCacheKey, this.PortalId);
			eventLogger.LogDebug("SaveStyles(settings={Settings}): cacheKey → {CacheKey}", settings, cacheKey);
            DataCache.RemoveCache(cacheKey);

            // Overwrite the file
            var path = $"{this.PortalSettings.HomeSystemDirectoryMapPath}{settings.FileName}";
			eventLogger.LogDebug("SaveStyles(settings={Settings}): path → {Path}", settings, path);
            var styles = settings.ToString();
			eventLogger.LogDebug("SaveStyles(settings={Settings}): styles → {Styles}", settings, styles);
            File.WriteAllText(path, styles);

			eventLogger.LogDebug("<<< Exiting SaveStyles(settings={Settings})", settings);
            return this.Ok();
        }

        /// <summary>
        /// Restores the styles to their out-of-box defaults.
        /// </summary>
        /// <returns><see cref="PortalStyles"/>.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IHttpActionResult RestoreStyles()
        {
			eventLogger.LogDebug(">>> Entering RestoreStyles()");
            if (!this.CanEdit())
            {
                return this.Unauthorized();
            }

            var repo = new PortalStylesRepository();
			eventLogger.LogDebug("RestoreStyles(): repo → {Repo}", repo);
            var styles = new PortalStyles();
            repo.SaveSettings(this.PortalId, styles);
			eventLogger.LogDebug("RestoreStyles(): styles → {Styles}", styles);
			eventLogger.LogDebug("<<< Exiting RestoreStyles()");
            return this.Ok(styles);
        }

        private bool CanEdit()
        {
			eventLogger.LogDebug(">>> Entering CanEdit()");
            if (this.UserInfo.IsSuperUser)
            {
                return true;
            }

            var allowAdminEdits = PortalController.Instance.GetCurrentSettings().GetStyles().AllowAdminEdits;
			eventLogger.LogDebug("CanEdit(): allowAdminEdits → {AllowAdminEdits}", allowAdminEdits);
            if (this.UserInfo.IsAdmin && allowAdminEdits)
            {
			eventLogger.LogDebug("<<< Exiting CanEdit()");
                return true;
            }

            return false;
        }
    }
}
