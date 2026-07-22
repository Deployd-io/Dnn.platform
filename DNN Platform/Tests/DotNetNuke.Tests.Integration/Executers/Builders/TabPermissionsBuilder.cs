// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Integration.Executers.Builders
{
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Tests.Integration.Executers.Dto;
    using TestStack.Dossier;

    public class TabPermissionsBuilder : TestDataBuilder<TabPermissions, TabPermissionsBuilder>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public TabPermissionsBuilder()
        {
			Logger.LogDebug(">>> Entering TabPermissionsBuilder()");
            this.WithTabId(Null.NullInteger);
			Logger.LogDebug("<<< Exiting TabPermissionsBuilder()");
        }

        public TabPermissionsBuilder WithTabId(int tabId)
        {
			Logger.LogDebug(">>> Entering WithTabId(tabId={TabId})", tabId);
            this.Set(x => x.TabId, tabId);
			Logger.LogDebug("<<< Exiting WithTabId(tabId={TabId})", tabId);
            return this;
        }

        public TabPermissionsBuilder WithPermissionDefinitions(IList<Permission> permissionDefinitions)
        {
			Logger.LogDebug(">>> Entering WithPermissionDefinitions(permissionDefinitions={PermissionDefinitions})", permissionDefinitions);
            this.Set(x => x.PermissionDefinitions, permissionDefinitions);
			Logger.LogDebug("<<< Exiting WithPermissionDefinitions(permissionDefinitions={PermissionDefinitions})", permissionDefinitions);
            return this;
        }

        public TabPermissionsBuilder WithRolePermissions(IList<RolePermission> rolePermissions)
        {
			Logger.LogDebug(">>> Entering WithRolePermissions(rolePermissions={RolePermissions})", rolePermissions);
            this.Set(x => x.RolePermissions, rolePermissions);
			Logger.LogDebug("<<< Exiting WithRolePermissions(rolePermissions={RolePermissions})", rolePermissions);
            return this;
        }

        public TabPermissionsBuilder WithUserPermissions(IList<UserPermission> userPermissions)
        {
			Logger.LogDebug(">>> Entering WithUserPermissions(userPermissions={UserPermissions})", userPermissions);
            this.Set(x => x.UserPermissions, userPermissions);
			Logger.LogDebug("<<< Exiting WithUserPermissions(userPermissions={UserPermissions})", userPermissions);
            return this;
        }

        protected override TabPermissions BuildObject()
        {
			Logger.LogDebug(">>> Entering BuildObject()");
			Logger.LogDebug("<<< Exiting BuildObject()");
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
