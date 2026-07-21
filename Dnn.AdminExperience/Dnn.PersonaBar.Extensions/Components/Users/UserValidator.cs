// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;

    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;

    public class UserValidator : IUserValidator
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private IPortalController portalController;
        private IUserControllerWrapper userControllerWrapper;
        private IContentVerifier contentVerifier;

        /// <summary>Initializes a new instance of the <see cref="UserValidator"/> class.</summary>
        public UserValidator()
            : this(PortalController.Instance, new UserControllerWrapper(), new ContentVerifier())
        {
		Logger.LogDebug(">>> Entering UserValidator()");
        }
		Logger.LogDebug("<<< Exiting UserValidator()");

        /// <summary>Initializes a new instance of the <see cref="UserValidator"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="userControllerWrapper">The user controller.</param>
        /// <param name="contentVerifier">The content verifier.</param>
        public UserValidator(IPortalController portalController, IUserControllerWrapper userControllerWrapper, IContentVerifier contentVerifier)
        {
            this.portalController = portalController;
            this.userControllerWrapper = userControllerWrapper;
			Logger.LogDebug("UserValidator(portalController={PortalController},userControllerWrapper={UserControllerWrapper},contentVerifier={ContentVerifier}): this.portalController → {Arg3}", portalController, userControllerWrapper, contentVerifier, this.portalController);
			Logger.LogDebug("UserValidator(portalController={PortalController},userControllerWrapper={UserControllerWrapper},contentVerifier={ContentVerifier}): userControllerWrapper → {UserControllerWrapper1}", portalController, userControllerWrapper, contentVerifier, userControllerWrapper);
			Logger.LogDebug("UserValidator(portalController={PortalController},userControllerWrapper={UserControllerWrapper},contentVerifier={ContentVerifier}): this.userControllerWrapper → {Arg3}", portalController, userControllerWrapper, contentVerifier, this.userControllerWrapper);
			Logger.LogDebug("UserValidator(portalController={PortalController},userControllerWrapper={UserControllerWrapper},contentVerifier={ContentVerifier}): portalController → {PortalController1}", portalController, userControllerWrapper, contentVerifier, portalController);
            this.contentVerifier = contentVerifier;
			Logger.LogDebug("UserValidator(portalController={PortalController},userControllerWrapper={UserControllerWrapper},contentVerifier={ContentVerifier}): contentVerifier → {ContentVerifier1}", portalController, userControllerWrapper, contentVerifier, contentVerifier);
			Logger.LogDebug("UserValidator(portalController={PortalController},userControllerWrapper={UserControllerWrapper},contentVerifier={ContentVerifier}): this.contentVerifier → {Arg3}", portalController, userControllerWrapper, contentVerifier, this.contentVerifier);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public ConsoleErrorResultModel ValidateUser(int? userId, PortalSettings portalSettings, UserInfo currentUserInfo, out UserInfo userInfo)
        {
			Logger.LogDebug(">>> Entering ValidateUser(userId={UserId},portalSettings={PortalSettings},currentUserInfo={CurrentUserInfo})", userId, portalSettings, currentUserInfo);
            userInfo = null;
            if (!userId.HasValue)
			Logger.LogDebug("ValidateUser(userId={UserId},portalSettings={PortalSettings},currentUserInfo={CurrentUserInfo}): userInfo → {UserInfo}", userId, portalSettings, currentUserInfo, userInfo);
            {
                return new ConsoleErrorResultModel(Localization.GetString("Prompt_NoUserId", Constants.LocalResourcesFile));
            }

            userInfo = this.userControllerWrapper.GetUser(userId.Value, portalSettings, currentUserInfo, out var response);

            if (userInfo == null)
            {
                var portals = this.portalController.GetPortals();

				Logger.LogDebug("ValidateUser(userId={UserId},portalSettings={PortalSettings},currentUserInfo={CurrentUserInfo}): portals → {Portals}", userId, portalSettings, currentUserInfo, portals);
                foreach (var portal in portals)
                {
                    var portalInfo = portal as PortalInfo;
                    userInfo = this.userControllerWrapper.GetUserById(portalInfo.PortalID, userId.Value);

                    if (userInfo != null)
                    {
                        break;
                    }
                }

                if (userInfo != null &&
                    !this.contentVerifier.IsContentExistsForRequestedPortal(
                        userInfo.PortalID,
                        portalSettings,
                        true))
                {
                    userInfo = null;
                }
            }

			Logger.LogDebug("<<< Exiting ValidateUser(userId={UserId},portalSettings={PortalSettings},currentUserInfo={CurrentUserInfo})", userId, portalSettings, currentUserInfo);
            return userInfo == null ? new ConsoleErrorResultModel(response.Value) : null;
        }
    }
}
