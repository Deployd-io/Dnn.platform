// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Entities.Portals;
	using Microsoft.Extensions.Logging;

    public class PortalAliasHelper
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public static PortalAliasInfo GetPrimaryPortalAlias(int portalId = 0)
        {
			_logger.LogDebug(">>> Entering GetPrimaryPortalAlias(portalId={PortalId})", portalId);
			_logger.LogDebug("<<< Exiting GetPrimaryPortalAlias(portalId={PortalId})", portalId);
            return DatabaseHelper.ExecuteStoredProcedure<PortalAliasInfo>("GetPortalAliases").First(p => p.PortalID == portalId && p.IsPrimary);
        }

        public static IEnumerable<PortalAliasInfo> GetPortalAliaes(int portalId = 0)
        {
			_logger.LogDebug(">>> Entering GetPortalAliaes(portalId={PortalId})", portalId);
			_logger.LogDebug("<<< Exiting GetPortalAliaes(portalId={PortalId})", portalId);
            return DatabaseHelper.ExecuteStoredProcedure<PortalAliasInfo>("GetPortalAliases").Where(p => p.PortalID == portalId);
        }

        public static IEnumerable<PortalAliasInfo> GetAllPortalAliaes()
        {
			_logger.LogDebug(">>> Entering GetAllPortalAliaes()");
			_logger.LogDebug("<<< Exiting GetAllPortalAliaes()");
            return DatabaseHelper.ExecuteStoredProcedure<PortalAliasInfo>("GetPortalAliases");
        }

        public static void AddPortalAlias(PortalAliasInfo portal)
        {
			_logger.LogDebug(">>> Entering AddPortalAlias(portal={Portal})", portal);
            DatabaseHelper.ExecuteStoredProcedure(
                "AddPortalAlias",
                portal.PortalID,
                portal.HTTPAlias,
                portal.CultureCode,
                portal.Skin,
                portal.BrowserType,
                portal.IsPrimary,
                -1);
				_logger.LogDebug("<<< Exiting AddPortalAlias(portal={Portal})", portal);
        }
    }
}
