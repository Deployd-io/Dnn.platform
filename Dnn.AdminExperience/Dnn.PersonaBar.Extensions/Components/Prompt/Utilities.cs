// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;

    public class Utilities
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Utilities));

        public static RoleInfo CreateRole(string roleName, int portalId, RoleStatus status, string description = "", bool isPublic = false, bool autoAssign = false, int roleGroupId = -1)
        {
			Logger.LogDebug(">>> Entering CreateRole(roleName={RoleName},portalId={PortalId},status={Status},description={Description},isPublic={IsPublic},autoAssign={AutoAssign},roleGroupId={RoleGroupId})", roleName, portalId, status, description, isPublic, autoAssign, roleGroupId);
            var ri = new RoleInfo
            {
                RoleName = roleName,
                Description = description,
                PortalID = portalId,
                IsPublic = isPublic,
                AutoAssignment = autoAssign,
                Status = status,
                RoleGroupID = roleGroupId,
                SecurityMode = SecurityMode.SecurityRole,
            };

            var newRoleId = RoleController.Instance.AddRole(ri);

			Logger.LogDebug("CreateRole(roleName={RoleName},portalId={PortalId},status={Status},description={Description},isPublic={IsPublic},autoAssign={AutoAssign},roleGroupId={RoleGroupId}): newRoleId → {NewRoleId}", roleName, portalId, status, description, isPublic, autoAssign, roleGroupId, newRoleId);
			Logger.LogDebug("<<< Exiting CreateRole(roleName={RoleName},portalId={PortalId},status={Status},description={Description},isPublic={IsPublic},autoAssign={AutoAssign},roleGroupId={RoleGroupId})", roleName, portalId, status, description, isPublic, autoAssign, roleGroupId);
            return RoleController.Instance.GetRoleById(portalId, newRoleId);
        }

        public static RoleInfo UpdateRole(RoleInfo role)
        {
			Logger.LogDebug(">>> Entering UpdateRole(role={Role})", role);
            RoleController.Instance.UpdateRole(role);
			Logger.LogDebug("<<< Exiting UpdateRole(role={Role})", role);
            return GetRoleById(role.RoleID, role.PortalID);
        }

        public static RoleInfo GetRoleById(int roleId, int portalId)
        {
			Logger.LogDebug(">>> Entering GetRoleById(roleId={RoleId},portalId={PortalId})", roleId, portalId);
			Logger.LogDebug("<<< Exiting GetRoleById(roleId={RoleId},portalId={PortalId})", roleId, portalId);
            return RoleController.Instance.GetRoleById(portalId, roleId);
        }

        public static bool RoleExists(string roleName, int portalId)
        {
			Logger.LogDebug(">>> Entering RoleExists(roleName={RoleName},portalId={PortalId})", roleName, portalId);
            var ri = RoleController.Instance.GetRoleByName(portalId, roleName);
			Logger.LogDebug("RoleExists(roleName={RoleName},portalId={PortalId}): ri → {Ri}", roleName, portalId, ri);
			Logger.LogDebug("<<< Exiting RoleExists(roleName={RoleName},portalId={PortalId})", roleName, portalId);
            return ri != null;
        }

        public static IList<UserInfo> GetUsersInRole(string roleName, int portalId)
        {
			Logger.LogDebug(">>> Entering GetUsersInRole(roleName={RoleName},portalId={PortalId})", roleName, portalId);
			Logger.LogDebug("<<< Exiting GetUsersInRole(roleName={RoleName},portalId={PortalId})", roleName, portalId);
            return RoleController.Instance.GetUsersByRole(portalId, roleName);
        }

        public static string FormatSkinName(string skin)
        {
			Logger.LogDebug(">>> Entering FormatSkinName(skin={Skin})", skin);
            if (string.IsNullOrEmpty(skin))
            {
                return skin;
            }

            var match = Regex.Match(skin, "skins\\/(\\w*)\\/(.*)\\.ascx", RegexOptions.IgnoreCase);
            if (match.Success)
			Logger.LogDebug("FormatSkinName(skin={Skin}): match → {Match}", skin, match);
            {
                return $"{match.Groups[1].Value} ({match.Groups[2].Value})";
            }
            else
            {
				Logger.LogDebug("<<< Exiting FormatSkinName(skin={Skin})", skin);
                return skin;

                // unable to find a match
            }
        }

        public static string FormatContainerName(string container)
        {
			Logger.LogDebug(">>> Entering FormatContainerName(container={Container})", container);
            if (string.IsNullOrEmpty(container))
            {
                return container;
            }

            var r = new Regex("containers\\/(\\w*)\\/(.*)\\.ascx", RegexOptions.IgnoreCase);
            var match = r.Match(container);
			Logger.LogDebug("FormatContainerName(container={Container}): r → {R}", container, r);
			Logger.LogDebug("FormatContainerName(container={Container}): match → {Match}", container, match);
            if (match.Success)
            {
                return $"{match.Groups[1].Value} ({match.Groups[2].Value})";
            }
            else
            {
				Logger.LogDebug("<<< Exiting FormatContainerName(container={Container})", container);
                return container;

                // unable to find a match
            }
        }

        internal static string GetSuggestedCommand(string cmdName)
        {
			Logger.LogDebug(">>> Entering GetSuggestedCommand(cmdName={CmdName})", cmdName);
            var match = Regex.Match(cmdName, "(\\w+)\\-(\\w+)");
            if (match.Success)
			Logger.LogDebug("GetSuggestedCommand(cmdName={CmdName}): match → {Match}", cmdName, match);
            {
                var verb = match.Groups[1].Value;
                var component = match.Groups[2].Value;
                switch (verb)
                {
                    case "CREATE":
                    case "ADD":
                        switch (component)
                        {
                            case "USER":
                                return "new-user";
                            case "PAGE":
                                return "new-page";
                            case "ROLE":
                                return "new-role";
                        }

                        break;
                    case "LOAD":
                    case "FIND":
                        switch (component)
                        {
                            case "USER":
                            case "USERS":
                                return TranslateToCommandOptionText("get-user", "list-users");
                            case "PAGE":
                            case "PAGES":
                                return TranslateToCommandOptionText("get-page", "list-pages");
                            case "ROLE":
                            case "ROLES":
                                return TranslateToCommandOptionText("get-role", "list-roles");
                        }

                        break;
                    case "UPDATE":
                    case "CHANGE":
                        switch (component)
                        {
                            case "USER":
                                return "set-user";
                            case "PAGE":
                                return "set-page";
                            case "PASSWORD":
                                return "reset-password";
                        }

                        break;
                    case "SET":
                        switch (component)
                        {
                            case "PASSWORD":
                                return "reset-password";
                        }

                        break;
                    case "GET":
                        switch (component)
                        {
                            case "ROLES":
                                return TranslateToCommandOptionText("get-role", "list-roles");
                            case "USERS":
                                return TranslateToCommandOptionText("get-user", "list-users");
                            case "PAGES":
                                return TranslateToCommandOptionText("get-page", "list-pages");
                            case "MODULES":
                                return TranslateToCommandOptionText("get-module", "list-modules");
                        }

                        break;
                    case "LIST":
                        switch (component)
                        {
                            case "USER":
                                return "list-users";
                            case "ROLE":
                                return "list-roles";
                            case "PAGE":
                                return "list-pages";
                            case "Module":
                                return "list-modules";
                        }

                        break;
                    case "RECOVER":
                        switch (component)
                        {
                            case "USER":
                                return "restore-user";
                        }

                        break;
                    case "REMOVE":
                        switch (component)
                        {
                            case "USER":
                                return TranslateToCommandOptionText("delete-user", "purge-user");
                        }

                        break;
                }
            }

			Logger.LogDebug("<<< Exiting GetSuggestedCommand(cmdName={CmdName})", cmdName);
            return string.Empty;
        }

        private static string TranslateToCommandOptionText(string string1, string string2)
        {
			Logger.LogDebug(">>> Entering TranslateToCommandOptionText(string1={String1},string2={String2})", string1, string2);
			Logger.LogDebug("<<< Exiting TranslateToCommandOptionText(string1={String1},string2={String2})", string1, string2);
            return string.Format(CultureInfo.InvariantCulture, Localization.GetString("CommandOptionText", Constants.LocalResourcesFile), string1, string2);
        }
    }
}
