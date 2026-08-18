// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Integration.Executers.Builders
{
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Tests.Integration.Executers.Dto;
    using TestStack.Dossier;
	using Microsoft.Extensions.Logging;

    public class TabPermissionsBuilder : TestDataBuilder<TabPermissions, TabPermissionsBuilder>
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public TabPermissionsBuilder()
        {
			_logger.LogDebug(">>> Entering TabPermissionsBuilder()");
            this.WithTabId(Null.NullInteger);
			_logger.LogDebug("<<< Exiting TabPermissionsBuilder()");
        }

        public TabPermissionsBuilder WithTabId(int tabId)
        {
			_logger.LogDebug(">>> Entering WithTabId(tabId={TabId})", tabId);
            this.Set(x => x.TabId, tabId);
			_logger.LogDebug("<<< Exiting WithTabId(tabId={TabId})", tabId);
            return this;
        }

        public TabPermissionsBuilder WithPermissionDefinitions(IList<Permission> permissionDefinitions)
        {
			_logger.LogDebug(">>> Entering WithPermissionDefinitions(permissionDefinitions={PermissionDefinitions})", permissionDefinitions);
            this.Set(x => x.PermissionDefinitions, permissionDefinitions);
			_logger.LogDebug("<<< Exiting WithPermissionDefinitions(permissionDefinitions={PermissionDefinitions})", permissionDefinitions);
            return this;
        }

        public TabPermissionsBuilder WithRolePermissions(IList<RolePermission> rolePermissions)
        {
			_logger.LogDebug(">>> Entering WithRolePermissions(rolePermissions={RolePermissions})", rolePermissions);
            this.Set(x => x.RolePermissions, rolePermissions);
			_logger.LogDebug("<<< Exiting WithRolePermissions(rolePermissions={RolePermissions})", rolePermissions);
            return this;
        }

        public TabPermissionsBuilder WithUserPermissions(IList<UserPermission> userPermissions)
        {
			_logger.LogDebug(">>> Entering WithUserPermissions(userPermissions={UserPermissions})", userPermissions);
            this.Set(x => x.UserPermissions, userPermissions);
			_logger.LogDebug("<<< Exiting WithUserPermissions(userPermissions={UserPermissions})", userPermissions);
            return this;
        }

        protected override TabPermissions BuildObject()
        {
			_logger.LogDebug(">>> Entering BuildObject()");
			_logger.LogDebug("<<< Exiting BuildObject()");
            return new TabPermissions
            {
                TabId = this.GetOrDefault(x => x.TabId),
                PermissionDefinitions = this.GetOrDefault(x => x.PermissionDefinitions),
                RolePermissions = this.GetOrDefault(x => x.RolePermissions),
                UserPermissions = this.GetOrDefault(x => x.UserPermissions),
            };
        }
    }
}
