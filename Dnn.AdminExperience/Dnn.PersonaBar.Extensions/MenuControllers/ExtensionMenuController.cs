// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.MenuControllers
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Controls the extensions menu.</summary>
    public class ExtensionMenuController : IMenuItemController
    {
        /// <summary>Initializes a new instance of the <see cref="ExtensionMenuController"/> class.</summary>
        public ExtensionMenuController()
        {
			eventLogger.LogDebug(">>> Entering ExtensionMenuController()");
            this.NavigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
			eventLogger.LogDebug("<<< Exiting ExtensionMenuController()");
        }

        /// <summary>Gets an <see cref="INavigationManager"/> instance to provide navigation services.</summary>
        protected INavigationManager NavigationManager { get; }

        /// <inheritdoc />
        public void UpdateParameters(MenuItem menuItem)
        {
		eventLogger.LogDebug(">>> Entering UpdateParameters(menuItem={MenuItem})", menuItem);
		eventLogger.LogDebug("<<< Exiting UpdateParameters(menuItem={MenuItem})", menuItem);
        }

        /// <inheritdoc />
        public bool Visible(MenuItem menuItem)
        {
			eventLogger.LogDebug(">>> Entering Visible(menuItem={MenuItem})", menuItem);
            var user = UserController.Instance.GetCurrentUserInfo();
			eventLogger.LogDebug("Visible(menuItem={MenuItem}): user → {User}", menuItem, user);
			eventLogger.LogDebug("<<< Exiting Visible(menuItem={MenuItem})", menuItem);
            return user.IsSuperUser || user.IsInRole(PortalSettings.Current?.AdministratorRoleName);
        }

        /// <inheritdoc />
        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            var settings = new Dictionary<string, object>();
            settings.Add("portalId", PortalSettings.Current.PortalId);
            settings.Add("installUrl", this.NavigationManager.NavigateURL(PortalSettings.Current.ActiveTab.TabID, PortalSettings.Current, "Install", "popUp=true"));
            return settings;
        }
    }
}
