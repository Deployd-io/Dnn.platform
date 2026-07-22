// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Roles.Components.Prompt.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Roles.Components.Prompt.Models;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("get-role", Constants.RolesCategory, "Prompt_GetRole_Description")]
    public class GetRole : ConsoleCommandBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [FlagParameter("id", "Prompt_GetRole_FlagId", "Integer", true)]
        private const string FlagId = "id";

        /// <inheritdoc />
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        public int RoleId { get; private set; } = Convert.ToInt32(Globals.glbRoleNothing, CultureInfo.InvariantCulture);

        /// <inheritdoc />
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			Logger.LogDebug(">>> Entering Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            this.RoleId = this.GetFlagValue(FlagId, "Role Id", -1, true, true);

            if (this.RoleId < 0)
            {
                this.AddMessage(this.LocalizeString("Prompt_RoleIdNegative"));
            }
			Logger.LogDebug("<<< Exiting Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override ConsoleResultModel Run()
        {
			Logger.LogDebug(">>> Entering Run()");
            var lst = new List<RoleModel>();
            var role = RolesController.Instance.GetRole(this.PortalSettings, this.RoleId);
			Logger.LogDebug("Run(): lst → {Lst}", lst);
            if (role == null)
			Logger.LogDebug("Run(): role → {Role}", role);
            {
                return new ConsoleErrorResultModel(string.Format(CultureInfo.CurrentCulture, this.LocalizeString("Prompt_NoRoleWithId"), this.RoleId));
            }

            lst.Add(new RoleModel(role));
			Logger.LogDebug("<<< Exiting Run()");
            return new ConsoleResultModel
            {
                Data = lst,
                Records = lst.Count,
                Output = string.Format(
                    CultureInfo.CurrentCulture,
                    this.LocalizeString("Prompt_RoleFound"),
                    this.RoleId),
            };
        }
    }
}
