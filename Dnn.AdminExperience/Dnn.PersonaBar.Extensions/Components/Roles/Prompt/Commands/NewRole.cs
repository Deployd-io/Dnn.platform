// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Roles.Components.Prompt.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Roles.Components.Prompt.Models;
    using Dnn.PersonaBar.Roles.Services.DTO;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Roles;

    [ConsoleCommand("new-role", Constants.RolesCategory, "Prompt_NewRole_Description")]
    public class NewRole : ConsoleCommandBase
    {
        [FlagParameter("public", "Prompt_NewRole_FlagIsPublic", "Boolean", "false")]
        private const string FlagIsPublic = "public";

        [FlagParameter("autoassign", "Prompt_NewRole_FlagAutoAssign", "Boolean", "false")]
        private const string FlagAutoAssign = "autoassign";

        [FlagParameter("name", "Prompt_NewRole_FlagRoleName", "String", true)]
        private const string FlagRoleName = "name";

        [FlagParameter("description", "Prompt_NewRole_FlagDescription", "String")]
        private const string FlagDescription = "description";

        [FlagParameter("status", "Prompt_NewRole_FlagStatus", "Boolean", "approved")]
        private const string FlagStatus = "status";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(NewRole));

        /// <inheritdoc />
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        public string RoleName { get; set; }

        public string Description { get; set; }

        public bool IsPublic { get; set; }

        public bool AutoAssign { get; set; }

        public RoleStatus Status { get; set; }

        /// <inheritdoc />
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			Logger.LogError(">>> Entering Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            this.RoleName = this.GetFlagValue(FlagRoleName, "Rolename", string.Empty, true, true);
            this.Description = this.GetFlagValue(FlagDescription, "Description", string.Empty);
			Logger.LogDebug("Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): Description → {Description}", args, portalSettings, userInfo, activeTabId, Description);
			Logger.LogDebug("Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): AutoAssign → {AutoAssign}", args, portalSettings, userInfo, activeTabId, AutoAssign);
            this.IsPublic = this.GetFlagValue(FlagIsPublic, "Is Public", false);
			Logger.LogDebug("Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): IsPublic → {IsPublic}", args, portalSettings, userInfo, activeTabId, IsPublic);
            this.AutoAssign = this.GetFlagValue(FlagAutoAssign, "Auto Assign", false, true);
            var status = this.GetFlagValue(FlagStatus, "Status", "approved");
            switch (status)
            {
                case "pending":
					Logger.LogError("Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): status → {Status}", args, portalSettings, userInfo, activeTabId, status);
                    this.Status = RoleStatus.Pending;
					Logger.LogDebug("Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): Status → {Status}", args, portalSettings, userInfo, activeTabId, Status);
					Logger.LogDebug("Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): this.Status → {Arg4}", args, portalSettings, userInfo, activeTabId, this.Status);
                    break;
                case "approved":
                    this.Status = RoleStatus.Approved;
                    break;
                case "disabled":
                    this.Status = RoleStatus.Disabled;
                    break;
                default:
                    this.AddMessage(string.Format(CultureInfo.InvariantCulture, this.LocalizeString("Prompt_InvalidRoleStatus"), FlagStatus));
                    break;
            }
			Logger.LogWarning("<<< Exiting Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override ConsoleResultModel Run()
        {
			Logger.LogError(">>> Entering Run()");
            try
            {
                var lstResults = new List<RoleModel>();
				Logger.LogError("Run(): lstResults → {LstResults}", lstResults);
                var roleDto = new RoleDto
                {
                    Id = Null.NullInteger,
                    Description = this.Description,
                    Status = this.Status,
                    Name = this.RoleName,
                    AutoAssign = this.AutoAssign,
                    IsPublic = this.IsPublic,
                    GroupId = -1,
                    IsSystem = false,
                    SecurityMode = SecurityMode.SecurityRole,
                };
                KeyValuePair<HttpStatusCode, string> message;
                var success = RolesController.Instance.SaveRole(this.PortalSettings, roleDto, false, out message);
				Logger.LogError("Run(): success → {Success}", success);
                if (!success)
                {
                    return new ConsoleErrorResultModel(message.Value);
                }

                lstResults.Add(new RoleModel(RoleController.Instance.GetRoleById(this.PortalId, roleDto.Id)));
                return new ConsoleResultModel(this.LocalizeString("RoleAdded.Message")) { Data = lstResults, Records = lstResults.Count };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting Run()");
                return new ConsoleErrorResultModel(this.LocalizeString("RoleAdded.Error"));
            }
        }
    }
}
