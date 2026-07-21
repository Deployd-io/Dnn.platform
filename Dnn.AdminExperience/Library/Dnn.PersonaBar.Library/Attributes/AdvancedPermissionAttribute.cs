// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Attributes
{
    using System;
    using System.Linq;

    using Dnn.PersonaBar.Library.Model;
    using Dnn.PersonaBar.Library.Permissions;
    using Dnn.PersonaBar.Library.Repository;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class AdvancedPermissionAttribute : AuthorizeAttributeBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly char[] PermissionCombiner = ['&',];

        /// <summary>Gets or sets the menu identifier.</summary>
        public string MenuName { get; set; }

        /// <summary>Gets or sets the permission key.</summary>
        public string Permission { get; set; }

        /// <summary>Gets or sets a value indicating whether when true, it will force admin to have explicit Permission. When false, admin is passed without checking the Permission.</summary>
        public bool CheckPermissionForAdmin { get; set; }

        /// <inheritdoc />
        public override bool IsAuthorized(AuthFilterContext context)
        {
			Logger.LogDebug(">>> Entering IsAuthorized(context={Context})", context);
            var menuItem = this.GetMenuByIdentifier();
            var portalSettings = PortalSettings.Current;
			Logger.LogDebug("IsAuthorized(context={Context}): menuItem → {MenuItem}", context, menuItem);

            if (menuItem == null || portalSettings == null)
            {
                return false;
            }

            if (!this.CheckPermissionForAdmin && PortalSecurity.IsInRole(Constants.AdminsRoleName))
            {
                return true;
            }

            // Permissions separated by & should be treated with AND operand.
            // Permissions separated by , are internally treated with OR operand.
            var hostSettings = Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            var portalController = Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
			Logger.LogDebug("IsAuthorized(context={Context}): hostSettings → {HostSettings}", context, hostSettings);
			Logger.LogDebug("IsAuthorized(context={Context}): portalController → {PortalController}", context, portalController);
            var allPermissionGroups = this.Permission.Split(PermissionCombiner, StringSplitOptions.RemoveEmptyEntries);
			Logger.LogDebug("IsAuthorized(context={Context}): allPermissionGroups → {AllPermissionGroups}", context, allPermissionGroups);
			Logger.LogDebug("<<< Exiting IsAuthorized(context={Context})", context);
            return allPermissionGroups.All(allPermissions => MenuPermissionController.HasMenuPermission(hostSettings, portalController, portalSettings.PortalId, menuItem, allPermissions));
        }

        private MenuItem GetMenuByIdentifier()
        {
			Logger.LogDebug(">>> Entering GetMenuByIdentifier()");
            if (string.IsNullOrEmpty(this.MenuName))
            {
                return null;
            }

			Logger.LogDebug("<<< Exiting GetMenuByIdentifier()");
            return PersonaBarRepository.Instance.GetMenuItem(this.MenuName);
        }
    }
}
