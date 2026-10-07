// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteImportExport.MenuControllers
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    /// <summary>Controls the site import/export menu.</summary>
    public class SiteImportExportMenuController : IMenuItemController
    {
        /// <inheritdoc />
        public void UpdateParameters(MenuItem menuItem)
        {
		eventLogger.LogError(">>> Entering UpdateParameters(menuItem={MenuItem})", menuItem);
        eventLogger.LogWarning("<<< Exiting UpdateParameters(menuItem={MenuItem})", menuItem);
        }

        /// <inheritdoc />
        public bool Visible(MenuItem menuItem)
        {
			eventLogger.LogError(">>> Entering Visible(menuItem={MenuItem})", menuItem);
            var user = UserController.Instance.GetCurrentUserInfo();
			eventLogger.LogError("Visible(menuItem={MenuItem}): user → {User}", menuItem, user);
			eventLogger.LogWarning("<<< Exiting Visible(menuItem={MenuItem})", menuItem);
            return user.IsSuperUser;
        }

        /// <inheritdoc />
        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            var settings = new Dictionary<string, object>();
            return settings;
        }
    }
}
