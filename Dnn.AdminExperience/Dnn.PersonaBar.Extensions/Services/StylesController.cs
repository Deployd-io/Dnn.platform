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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>
        /// Gets the styles.
        /// </summary>
        /// <returns>The current portal style settings.</returns>
        [HttpGet]
        public IHttpActionResult GetStyles()
        {
			Logger.LogDebug(">>> Entering GetStyles()");
            if (!this.CanEdit())
            {
                return this.Unauthorized();
            }

            var repo = new PortalStylesRepository();
            var settings = repo.GetSettings(this.PortalId);
			Logger.LogDebug("GetStyles(): repo → {Repo}", repo);
			Logger.LogDebug("GetStyles(): settings → {Settings}", settings);
			Logger.LogDebug("<<< Exiting GetStyles()");
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
            if (!this.CanEdit())
            {
                return this.Unauthorized();
            }

            // Save to database
            var repo = new PortalStylesRepository();
            repo.SaveSettings(this.PortalId, settings);

            // Clear cache
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.PortalStylesCacheKey, this.PortalId);
            DataCache.RemoveCache(cacheKey);

            // Overwrite the file
            var path = $"{this.PortalSettings.HomeSystemDirectoryMapPath}{settings.FileName}";
            var styles = settings.ToString();
            File.WriteAllText(path, styles);

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
			Logger.LogDebug(">>> Entering RestoreStyles()");
            if (!this.CanEdit())
            {
                return this.Unauthorized();
            }

            var repo = new PortalStylesRepository();
            var styles = new PortalStyles();
			Logger.LogDebug("RestoreStyles(): repo → {Repo}", repo);
			Logger.LogDebug("RestoreStyles(): styles → {Styles}", styles);
            repo.SaveSettings(this.PortalId, styles);
			Logger.LogDebug("<<< Exiting RestoreStyles()");
            return this.Ok(styles);
        }

        private bool CanEdit()
        {
			Logger.LogDebug(">>> Entering CanEdit()");
            if (this.UserInfo.IsSuperUser)
            {
                return true;
            }

            var allowAdminEdits = PortalController.Instance.GetCurrentSettings().GetStyles().AllowAdminEdits;
            if (this.UserInfo.IsAdmin && allowAdminEdits)
			Logger.LogDebug("CanEdit(): allowAdminEdits → {AllowAdminEdits}", allowAdminEdits);
            {
                return true;
            }

			Logger.LogDebug("<<< Exiting CanEdit()");
            return false;
        }
    }
}
