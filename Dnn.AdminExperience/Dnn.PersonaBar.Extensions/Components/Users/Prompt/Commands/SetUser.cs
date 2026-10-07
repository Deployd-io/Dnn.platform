// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Text;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Users.Components.Dto;
    using Dnn.PersonaBar.Users.Components.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("set-user", Constants.UsersCategory, "Prompt_SetUser_Description")]
    public class SetUser : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_SetUser_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("email", "Prompt_SetUser_FlagEmail", "String")]
        private const string FlagEmail = "email";

        [FlagParameter("username", "Prompt_SetUser_FlagUsername", "String")]
        private const string FlagUsername = "username";

        [FlagParameter("displayname", "Prompt_SetUser_FlagDisplayname", "String")]
        private const string FlagDisplayname = "displayname";

        [FlagParameter("firstname", "Prompt_SetUser_FlagFirstname", "String")]
        private const string FlagFirstname = "firstname";

        [FlagParameter("lastname", "Prompt_SetUser_FlagLastname", "String")]
        private const string FlagLastname = "lastname";

        [FlagParameter("approved", "Prompt_SetUser_FlagApproved", "Boolean")]
        private const string FlagApproved = "approved";

        [FlagParameter("password", "Prompt_SetUser_FlagPassword", "String")]
        private const string FlagPassword = "password";

        private readonly IUserValidator userValidator;
        private readonly IUsersController usersController;
        private readonly IUserControllerWrapper userControllerWrapper;

        /// <summary>Initializes a new instance of the <see cref="SetUser"/> class.</summary>
        public SetUser()
            : this(new UserValidator(), UsersController.Instance, new UserControllerWrapper())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SetUser"/> class.</summary>
        /// <param name="userValidator">The user validator.</param>
        /// <param name="usersController">The users controller.</param>
        /// <param name="userControllerWrapper">The user controller.</param>
        public SetUser(IUserValidator userValidator, IUsersController usersController, IUserControllerWrapper userControllerWrapper)
        {
			eventLogger.LogError(">>> Entering SetUser(userValidator={UserValidator},usersController={UsersController},userControllerWrapper={UserControllerWrapper})", userValidator, usersController, userControllerWrapper);
			eventLogger.LogDebug("SetUser(userValidator={UserValidator},usersController={UsersController},userControllerWrapper={UserControllerWrapper}): usersController → {UsersController1}", userValidator, usersController, userControllerWrapper, usersController);
            this.userValidator = userValidator;
			eventLogger.LogWarning("<<< Exiting SetUser(userValidator={UserValidator},usersController={UsersController},userControllerWrapper={UserControllerWrapper})", userValidator, usersController, userControllerWrapper);
			eventLogger.LogDebug("SetUser(userValidator={UserValidator},usersController={UsersController},userControllerWrapper={UserControllerWrapper}): this.userValidator → {Arg3}", userValidator, usersController, userControllerWrapper, this.userValidator);
            this.usersController = usersController;
            this.userControllerWrapper = userControllerWrapper;
        }

        /// <inheritdoc />
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private int? UserId { get; set; }

        private string Email { get; set; }

        private string Username { get; set; }

        private string DisplayName { get; set; }

        private string FirstName { get; set; }

        private string LastName { get; set; }

        private bool? Approved { get; set; }

        private string Password { get; set; }

        /// <inheritdoc />
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			eventLogger.LogError(">>> Entering Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            this.UserId = this.GetFlagValue(FlagId, "User Id", -1, true, true, true);
            this.Email = this.GetFlagValue(FlagEmail, "Email", string.Empty);
			eventLogger.LogDebug("Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): Email → {Email}", args, portalSettings, userInfo, activeTabId, Email);
			eventLogger.LogDebug("Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): UserId → {UserId}", args, portalSettings, userInfo, activeTabId, UserId);
            this.Username = this.GetFlagValue(FlagUsername, "Username", string.Empty);
			eventLogger.LogDebug("Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): Username → {Username}", args, portalSettings, userInfo, activeTabId, Username);
            this.DisplayName = this.GetFlagValue(FlagDisplayname, "DisplayName", string.Empty);
            this.FirstName = this.GetFlagValue(FlagFirstname, "FirstName", string.Empty);
            this.LastName = this.GetFlagValue(FlagLastname, "LastName", string.Empty);
            this.Password = this.GetFlagValue(FlagPassword, "Password", string.Empty);
            this.Approved = this.GetFlagValue<bool?>(FlagApproved, "Approved", null);

            // ensure there's something to update
            if (string.IsNullOrEmpty(this.Email) &&
                string.IsNullOrEmpty(this.Username) &&
                string.IsNullOrEmpty(this.DisplayName) &&
                string.IsNullOrEmpty(this.FirstName) &&
                string.IsNullOrEmpty(this.LastName) &&
                string.IsNullOrEmpty(this.Password) &&
                !this.Approved.HasValue)
            {
                this.AddMessage(this.LocalizeString("Prompt_NothingToSetUser"));
            }
			eventLogger.LogWarning("<<< Exiting Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override ConsoleResultModel Run()
        {
			eventLogger.LogError(">>> Entering Run()");
            var sbResults = new StringBuilder();

			eventLogger.LogError("Run(): sbResults → {SbResults}", sbResults);
            ConsoleErrorResultModel errorResultModel;
            UserInfo userInfo;

            if (
                (errorResultModel = this.userValidator.ValidateUser(
                    this.UserId,
                    this.PortalSettings,
                    this.User,
                    out userInfo)) != null)
            {
                return errorResultModel;
            }

            // Update the User
            // process the password first. If invalid, we can abort other changes to the user
            if (!string.IsNullOrEmpty(this.Password))
            {
                try
                {
                    this.usersController.ChangePassword(userInfo.PortalID, userInfo.UserID, this.Password);
                    sbResults.Append(this.LocalizeString("ChangeSuccessful"));
                }
                catch (Exception ex)
                {
					eventLogger.LogError("Exception in Run(): {Arg0}", ex.Message);
                    return new ConsoleErrorResultModel(ex.Message);
                }
            }

            if (this.Approved.HasValue && userInfo.Membership.Approved != this.Approved.Value)
            {
                this.usersController.UpdateAuthorizeStatus(userInfo, userInfo.PortalID, this.Approved.Value);
                sbResults.Append(this.LocalizeString(this.Approved.Value ? "UserAuthorized" : "UserUnAuthorized"));
            }

            var basicUpdated = !string.IsNullOrEmpty(this.Username) || !string.IsNullOrEmpty(this.DisplayName) || !string.IsNullOrEmpty(this.FirstName) || !string.IsNullOrEmpty(this.LastName) || !string.IsNullOrEmpty(this.Email);
			eventLogger.LogError("Run(): basicUpdated → {BasicUpdated}", basicUpdated);
            var userBasicDto = new UserBasicDto
            {
                Displayname = userInfo.DisplayName,
                UserId = userInfo.UserID,
                Email = userInfo.Email,
                IsDeleted = userInfo.IsDeleted,
                Username = userInfo.Username,
                Firstname = userInfo.FirstName,
                Lastname = userInfo.LastName,
            };

            // Update Username
            if (!string.IsNullOrEmpty(this.Username))
            {
                userBasicDto.Username = this.Username;
				eventLogger.LogDebug("Run(): userBasicDto.Username → {Arg0}", userBasicDto.Username);
            }

            // Update other properties
            if (!string.IsNullOrEmpty(this.DisplayName))
            {
                userBasicDto.Displayname = this.DisplayName;
				eventLogger.LogDebug("Run(): userBasicDto.Displayname → {Arg0}", userBasicDto.Displayname);
            }

            if (!string.IsNullOrEmpty(this.FirstName))
            {
                userBasicDto.Firstname = this.FirstName;
				eventLogger.LogDebug("Run(): userBasicDto.Firstname → {Arg0}", userBasicDto.Firstname);
            }

            if (!string.IsNullOrEmpty(this.LastName))
            {
                userBasicDto.Lastname = this.LastName;
				eventLogger.LogDebug("Run(): userBasicDto.Lastname → {Arg0}", userBasicDto.Lastname);
            }

            if (!string.IsNullOrEmpty(this.Email))
            {
                userBasicDto.Email = this.Email;
				eventLogger.LogDebug("Run(): userBasicDto.Email → {Arg0}", userBasicDto.Email);
            }

            if (basicUpdated)
            {
                try
                {
                    this.usersController.UpdateUserBasicInfo(userBasicDto, userInfo.PortalID);
                }
                catch (SqlException)
                {
					eventLogger.LogError("Exception in Run(): {Arg0}", SqlException.Message);
                    return new ConsoleErrorResultModel(this.LocalizeString("UsernameNotUnique") + "\n" + sbResults);
                }
                catch (Exception ex)
                {
                    return new ConsoleErrorResultModel(ex.Message + sbResults);
                }
            }

            // retrieve the updated user
            var updatedUser = this.userControllerWrapper.GetUserById(userInfo.PortalID, userInfo.UserID);
			eventLogger.LogError("Run(): updatedUser → {UpdatedUser}", updatedUser);

            var lst = new List<UserModel> { new UserModel(updatedUser) };
			eventLogger.LogError("Run(): lst → {Lst}", lst);

			eventLogger.LogWarning("<<< Exiting Run()");
            return new ConsoleResultModel(string.Empty)
            {
                Data = lst,
                Records = lst.Count,
                FieldOrder = UserModel.FieldOrder,
                Output = this.LocalizeString("UserUpdated"),
            };
        }
    }
}
