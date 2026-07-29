// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    using System.Globalization;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Users.Components;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    using Constants = Dnn.PersonaBar.Recyclebin.Components.Constants;

    [ConsoleCommand("restore-user", Constants.RecylcleBinCategory, "Prompt_RestoreUser_Description")]

    public class RestoreUser : ConsoleCommandBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [FlagParameter("id", "Prompt_RestoreUser_FlagId", "Integer", true)]
        private const string FlagId = "id";

        private IUserValidator userValidator;
        private IRecyclebinController recyclebinController;

        /// <summary>Initializes a new instance of the <see cref="RestoreUser"/> class.</summary>
        public RestoreUser()
            : this(new UserValidator(), RecyclebinController.Instance)
        {
		Logger.LogDebug(">>> Entering RestoreUser()");
		Logger.LogDebug("<<< Exiting RestoreUser()");
		Logger.LogDebug(">>> Entering RestoreUser(userValidator={UserValidator},instance={Instance})", userValidator, instance);
		Logger.LogDebug("<<< Exiting RestoreUser(userValidator={UserValidator},instance={Instance})", userValidator, instance);
        }

        /// <summary>Initializes a new instance of the <see cref="RestoreUser"/> class.</summary>
        /// <param name="userValidator">The user validator.</param>
        /// <param name="instance">The recycle bin controller.</param>
        public RestoreUser(IUserValidator userValidator, IRecyclebinController instance)
        {
            this.userValidator = userValidator;
            this.recyclebinController = instance;
			Logger.LogDebug("RestoreUser(userValidator={UserValidator},instance={Instance}): this.userValidator → {Arg2}", userValidator, instance, this.userValidator);
			Logger.LogDebug("RestoreUser(userValidator={UserValidator},instance={Instance}): recyclebinController → {RecyclebinController}", userValidator, instance, recyclebinController);
			Logger.LogDebug("RestoreUser(userValidator={UserValidator},instance={Instance}): this.recyclebinController → {Arg2}", userValidator, instance, this.recyclebinController);
			Logger.LogDebug("RestoreUser(userValidator={UserValidator},instance={Instance}): userValidator → {UserValidator1}", userValidator, instance, userValidator);
        }

        /// <inheritdoc />
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private int UserId { get; set; }

        /// <inheritdoc />
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			Logger.LogDebug(">>> Entering Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            this.UserId = this.GetFlagValue(FlagId, "User Id", -1, true, true, true);
			Logger.LogDebug("<<< Exiting Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override ConsoleResultModel Run()
        {
			Logger.LogDebug(">>> Entering Run()");
            this.userValidator.ValidateUser(this.UserId, this.PortalSettings, this.User, out var userInfo);

            if (userInfo == null)
            {
                return new ConsoleErrorResultModel(string.Format(CultureInfo.CurrentCulture, this.LocalizeString("UserNotFound"), this.UserId));
            }

            if (!userInfo.IsDeleted)
            {
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_RestoreNotRequired"));
            }

            var restoredUser = this.recyclebinController.RestoreUser(userInfo, out var message);
			Logger.LogDebug("Run(): restoredUser → {RestoredUser}", restoredUser);
			Logger.LogDebug("<<< Exiting Run()");
            return restoredUser
                ? new ConsoleResultModel(this.LocalizeString("UserRestored")) { Records = 1, }
                : new ConsoleErrorResultModel(message);
        }
    }
}
