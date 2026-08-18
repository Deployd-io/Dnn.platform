// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Controllers
{
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using DNN.Integration.Test.Framework.Helpers;
    using DNN.Integration.Test.Framework.Scripts;
	using Microsoft.Extensions.Logging;

    public static class RoleController
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const string PortalIdMarker = @"'$[portal_id]'";
        private const string RoleNameMarker = @"$[role_name]";
        private const string RoleDescriptionMarker = @"$[role_description]";
        private const string RoleIdMarker = @"'$[role_id]'";
        private const string UserIdMarker = @"'$[user_id]'";

        public static int CreateBasicRole(string roleName, string roleDescription = "")
        {
			_logger.LogDebug(">>> Entering CreateBasicRole(roleName={RoleName},roleDescription={RoleDescription})", roleName, roleDescription);
			_logger.LogDebug("<<< Exiting CreateBasicRole(roleName={RoleName},roleDescription={RoleDescription})", roleName, roleDescription);
            return CreateRole(roleName, roleDescription);
        }

        public static int CreateRoleIfNotPresent(string roleName, int portalId = 0)
        {
			_logger.LogDebug(">>> Entering CreateRoleIfNotPresent(roleName={RoleName},portalId={PortalId})", roleName, portalId);
            if (!string.IsNullOrEmpty(roleName))
            {
                var roleId = GetRoleId(roleName, portalId);
				_logger.LogDebug("CreateRoleIfNotPresent(roleName={RoleName},portalId={PortalId}): roleId → {RoleId}", roleName, portalId, roleId);
			_logger.LogDebug("<<< Exiting CreateRoleIfNotPresent(roleName={RoleName},portalId={PortalId})", roleName, portalId);
                return roleId < 0 ? CreateRole(roleName, roleName, portalId) : roleId;
            }

            return -1;
        }

        /// <summary>Get RoleId for role "Registered Users".</summary>
        /// <returns></returns>
        public static int GetRegisteredUsersRoleId(int portalId = 0)
        {
			_logger.LogDebug(">>> Entering GetRegisteredUsersRoleId(portalId={PortalId})", portalId);
			_logger.LogDebug("<<< Exiting GetRegisteredUsersRoleId(portalId={PortalId})", portalId);
			_logger.LogDebug(">>> Entering CreateRole(roleName={RoleName},roleDescription={RoleDescription},portalId={PortalId})", roleName, roleDescription, portalId);
			_logger.LogDebug("<<< Exiting CreateRole(roleName={RoleName},roleDescription={RoleDescription},portalId={PortalId})", roleName, roleDescription, portalId);
            return GetRoleId("Registered Users", portalId);
        }

        /// <summary>Get RoleId for role "Administrators".</summary>
        /// <returns></returns>
        public static int GetAdministratorsRoleId(int portalId = 0)
        {
			_logger.LogDebug(">>> Entering GetAdministratorsRoleId(portalId={PortalId})", portalId);
			_logger.LogDebug("<<< Exiting GetAdministratorsRoleId(portalId={PortalId})", portalId);
            return GetRoleId("Administrators", portalId);
        }

        public static int GetRoleId(string roleName, int portalId = 0)
        {
			_logger.LogDebug(">>> Entering GetRoleId(roleName={RoleName},portalId={PortalId})", roleName, portalId);
            // The fix for DNN-4288 prevented virtual roles from getting virtual
            // roles (i.e., with RoleID <= 0 which includes Administrator role).
            // var results = DatabaseHelper.ExecuteStoredProcedure("GetRolesBasicSearch", portalId, 0, 10, roleName);
            roleName = roleName.Replace("'", string.Empty);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var query = string.Format("SELECT RoleID FROM {{objectQualifier}}Roles WHERE RoleName = N'{0}' AND PortalID={1};", roleName, portalId);
            var results = DatabaseHelper.ExecuteQuery(query).ToArray();
			_logger.LogDebug("GetRoleId(roleName={RoleName},portalId={PortalId}): roleName → {RoleName1}", roleName, portalId, roleName);

            if (!results.Any())
			_logger.LogDebug("GetRoleId(roleName={RoleName},portalId={PortalId}): query → {Query}", roleName, portalId, query);
			_logger.LogDebug("GetRoleId(roleName={RoleName},portalId={PortalId}): results → {Results}", roleName, portalId, results);
			_logger.LogInformation("GetRoleId(roleName,portalId)={RoleName},{PortalId}: find query executed in {Arg2} ms", roleName, portalId, __logStart.ElapsedMilliseconds);
            {
			_logger.LogDebug("<<< Exiting GetRoleId(roleName={RoleName},portalId={PortalId})", roleName, portalId);
                return -1;
            }

            // we could have more than a single role with same name when we have more than one portal
            return results.SelectMany(x => x.Where(y => y.Key == "RoleID").Select(y => (int)y.Value)).FirstOrDefault();
        }

        public static void DeleteRole(int roleId)
        {
            DatabaseHelper.ExecuteStoredProcedure("DeleteRole", roleId);
			_logger.LogDebug("<<< Exiting DeleteRole(roleId={RoleId})", roleId);
        }

        public static void AssignRoleToUser(int roleId, int userId)
        {
            var fileContent = SqlScripts.AssignRoleToUser;
            var masterScript = new StringBuilder(fileContent)
                .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier)
                .ToString();

            var script = new StringBuilder(masterScript)
                .Replace(RoleIdMarker, roleId.ToString(CultureInfo.InvariantCulture))
                .Replace(UserIdMarker, userId.ToString(CultureInfo.InvariantCulture));

            DatabaseHelper.ExecuteQuery(script.ToString());
			_logger.LogDebug("<<< Exiting AssignRoleToUser(roleId={RoleId},userId={UserId})", roleId, userId);
        }

        private static int CreateRole(string roleName, string roleDescription, int portalId = 0)
        {
            var fileContent = SqlScripts.SingleRoleCreation;
            var masterScript = new StringBuilder(fileContent)
                .Replace(PortalIdMarker, portalId.ToString(CultureInfo.InvariantCulture))
                .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier)
                .ToString();

            var script = new StringBuilder(masterScript)
                .Replace(RoleNameMarker, roleName.Replace("'", "''"))
                .Replace(RoleDescriptionMarker, roleDescription.Replace("'", "''"));

            DatabaseHelper.ExecuteQuery(script.ToString());
            WebApiTestHelper.ClearHostCache();
            return GetRoleId(roleName);
        }
    }
}
