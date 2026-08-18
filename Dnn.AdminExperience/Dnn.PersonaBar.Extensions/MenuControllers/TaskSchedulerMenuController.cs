// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.TaskScheduler.MenuControllers
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Entities.Users;

    /// <summary>Controls the task scheduler menu.</summary>
    public class TaskSchedulerMenuController : IMenuItemController
    {
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
            return user.IsSuperUser;
        }

        /// <inheritdoc />
        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            return null;
        }
    }
}
