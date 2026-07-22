// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components
{
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Membership;

    public class UsersMenuController : IMenuItemController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public void UpdateParameters(MenuItem menuItem)
        {
		Logger.LogDebug(">>> Entering UpdateParameters(menuItem={MenuItem})", menuItem);
		Logger.LogDebug("<<< Exiting UpdateParameters(menuItem={MenuItem})", menuItem);
        }

        /// <inheritdoc />
        public bool Visible(MenuItem menuItem)
        {
			Logger.LogDebug(">>> Entering Visible(menuItem={MenuItem})", menuItem);
			Logger.LogDebug("<<< Exiting Visible(menuItem={MenuItem})", menuItem);
            return true;
        }

        /// <inheritdoc />
        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            var settings = new Dictionary<string, object>();
            settings.Add("userId", UserController.Instance.GetCurrentUserInfo().UserID);
            settings.Add("requiresQuestionAndAnswer", MembershipProviderConfig.RequiresQuestionAndAnswer);
            settings.Add("dataConsentActive", PortalSettings.Current.DataConsentActive);
            settings.Add("userNameMinLength", PortalController.GetPortalSettingAsInteger("Security_UserNameMinLength", PortalSettings.Current.PortalId, Globals.glbUserNameMinLength));
            return settings;
        }
    }
}
