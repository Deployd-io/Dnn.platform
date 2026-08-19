// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Users.Components.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("delete-user", Constants.UsersCategory, "Prompt_DeleteUser_Description")]
    public class DeleteUser : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_DeleteUser_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("notify", "Prompt_DeleteUser_FlagNotify", "Boolean", "false")]
        private const string FlagNotify = "notify";

        private readonly IUserValidator userValidator;
        private readonly IUserControllerWrapper userControllerWrapper;

        /// <summary>Initializes a new instance of the <see cref="DeleteUser"/> class.</summary>
        public DeleteUser()
            : this(new UserValidator(), new UserControllerWrapper())
        {
		eventLogger.LogDebug(">>> Entering DeleteUser()");
		eventLogger.LogDebug("<<< Exiting DeleteUser()");
		eventLogger.LogDebug(">>> Entering DeleteUser(userValidator={UserValidator},userControllerWrapper={UserControllerWrapper})", userValidator, userControllerWrapper);
        }

        /// <summary>Initializes a new instance of the <see cref="DeleteUser"/> class.</summary>
        /// <param name="userValidator">The user validator.</param>
        /// <param name="userControllerWrapper">The user controller.</param>
        public DeleteUser(IUserValidator userValidator, IUserControllerWrapper userControllerWrapper)
        {
            this.userValidator = userValidator;
            this.userControllerWrapper = userControllerWrapper;
			eventLogger.LogDebug("DeleteUser(userValidator={UserValidator},userControllerWrapper={UserControllerWrapper}): this.userValidator → {Arg2}", userValidator, userControllerWrapper, this.userValidator);
			eventLogger.LogDebug("<<< Exiting DeleteUser(userValidator={UserValidator},userControllerWrapper={UserControllerWrapper})", userValidator, userControllerWrapper);
        }

        /// <inheritdoc />
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private int UserId { get; set; }

        private bool Notify { get; set; }

        /// <inheritdoc />
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			eventLogger.LogDebug(">>> Entering Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            this.UserId = this.GetFlagValue(FlagId, "User Id", -1, true, true, true);
            this.Notify = this.GetFlagValue(FlagNotify, "Notify", false);
			eventLogger.LogDebug("<<< Exiting Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override ConsoleResultModel Run()
        {
			eventLogger.LogDebug(">>> Entering Run()");
            ConsoleErrorResultModel errorResultModel;
            UserInfo userInfo;

            if ((errorResultModel = this.userValidator.ValidateUser(this.UserId, this.PortalSettings, this.User, out userInfo)) != null)
            {
                return errorResultModel;
            }

            var userModels = new List<UserModel> { new UserModel(userInfo) };
			eventLogger.LogDebug("Run(): userModels → {UserModels}", userModels);

            if (userInfo.IsDeleted)
            {
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_UserAlreadyDeleted"));
            }

            var validPortalId = userInfo.PortalID;

            if (!this.userControllerWrapper.DeleteUserAndClearCache(ref userInfo, this.Notify, false))
            {
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_UserDeletionFailed"))
                {
                    Data = userModels,
                };
            }

            // attempt to retrieve the user from the dB
            userInfo = this.userControllerWrapper.GetUserById(validPortalId, userInfo.UserID);
            userModels = new List<UserModel> { new UserModel(userInfo) };
			eventLogger.LogDebug("Run(): userInfo → {UserInfo}", userInfo);
			eventLogger.LogDebug("<<< Exiting Run()");
            return new ConsoleResultModel(this.LocalizeString("UserDeleted")) { Data = userModels, Records = userModels.Count };
        }
    }
}
