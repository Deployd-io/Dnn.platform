// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Styles.MenuControllers
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Portals.Extensions;
    using DotNetNuke.Entities.Users;

    /// <summary>
    /// Controls the Styles menu.
    /// </summary>
    public class StylesMenuController : IMenuItemController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IPortalController portalController;
        private readonly IUserController userController;

        /// <summary>
        /// Initializes a new instance of the <see cref="StylesMenuController"/> class.
        /// </summary>
        /// <param name="portalController">Provides access to portals information.</param>
        public StylesMenuController(IPortalController portalController)
            : this(portalController, UserController.Instance)
        {
		Logger.LogDebug(">>> Entering StylesMenuController(portalController={PortalController})", portalController);
        }
		Logger.LogDebug("<<< Exiting StylesMenuController(portalController={PortalController})", portalController);

        /// <summary>
        /// Initializes a new instance of the <see cref="StylesMenuController"/> class.
        /// </summary>
        /// <param name="portalController">Provides information about portals.</param>
        /// <param name="userController">Provides information about users.</param>
        public StylesMenuController(IPortalController portalController, IUserController userController)
        {
            this.portalController = portalController;
            this.userController = userController;
			Logger.LogDebug("StylesMenuController(portalController={PortalController},userController={UserController}): this.portalController → {Arg2}", portalController, userController, this.portalController);
			Logger.LogDebug("StylesMenuController(portalController={PortalController},userController={UserController}): userController → {UserController1}", portalController, userController, userController);
			Logger.LogDebug("StylesMenuController(portalController={PortalController},userController={UserController}): this.userController → {Arg2}", portalController, userController, this.userController);
			Logger.LogDebug("StylesMenuController(portalController={PortalController},userController={UserController}): portalController → {PortalController1}", portalController, userController, portalController);
        }

        /// <inheritdoc />
        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
           return new Dictionary<string, object>();
        }

        /// <inheritdoc />
        public void UpdateParameters(MenuItem menuItem)
        {
			Logger.LogDebug(">>> Entering UpdateParameters(menuItem={MenuItem})", menuItem);
			Logger.LogDebug("UpdateParameters(menuItem={MenuItem}): menuItem.AllowHost → {Arg1}", menuItem, menuItem.AllowHost);
            menuItem.AllowHost = true;
			Logger.LogDebug("<<< Exiting UpdateParameters(menuItem={MenuItem})", menuItem);
        }

        /// <inheritdoc />
        public bool Visible(MenuItem menuItem)
        {
			Logger.LogDebug(">>> Entering Visible(menuItem={MenuItem})", menuItem);
            var allowAdminEdits = this.portalController
                .GetCurrentSettings()
                .GetStyles()
                .AllowAdminEdits;
            var user = this.userController.GetCurrentUserInfo();

			Logger.LogDebug("Visible(menuItem={MenuItem}): user → {User}", menuItem, user);
            if (user.IsSuperUser)
            {
                return true;
            }

            if (user.IsAdmin && allowAdminEdits)
            {
                return true;
            }

			Logger.LogDebug("<<< Exiting Visible(menuItem={MenuItem})", menuItem);
            return false;
        }
    }
}
